using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kyrsach2WINFORM
{
    public partial class AddCategory : Form
    {
        public AddCategory()
        {
            InitializeComponent();
        }

        // Проверка на дубликат True - если нет дубликата
        bool CheckCategoty(string NameCategoty)
        {
            string CMD = $"Select * FROM Category WHERE Name = '{NameCategoty}';";

            using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
            {
                Con.Open();
                MySqlCommand cmd = new MySqlCommand(CMD, Con);
                bool Result = cmd.ExecuteScalar() == null;

                return Result;
            }
        }

        //добавить категорию
        private void addCategory_Click(object sender, EventArgs e)
        {
            string NameCategoty = textBox1.Text.Trim();
            string CMD = $"INSERT INTO Category (Name) VALUES ('{NameCategoty}');";

            DialogResult dialogResult = MessageBox.Show("Добавить категорию?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                //Проверяем на дубликат
                if (!CheckCategoty(NameCategoty))
                {
                    MessageBox.Show("Эта категория уже существует в базе", "Ошибка операции", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox1.Text = Name;    // Очистка
                    return;
                }

                //Добавляем строку
                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    Con.Open();
                    MySqlCommand cmd = new MySqlCommand(CMD, Con);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Категория была успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBox1.Text = "";    // Очистка
            }
            else
                return;
        }



        //Категория - НАСТРОЙКА ПОЛЯ - только русские буквы и пробелы
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsWhiteSpace(e.KeyChar) && !char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar) || (e.KeyChar >= 'a' && e.KeyChar <= 'z') || (e.KeyChar >= 'A' && e.KeyChar <= 'Z'))
                e.Handled = true;

            else
                e.Handled = false;
        }
        //Категория
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim() != "" )
                button2.Enabled = true;
            else
                button2.Enabled = false;
        }


        // Закрыть
        private void button8_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
