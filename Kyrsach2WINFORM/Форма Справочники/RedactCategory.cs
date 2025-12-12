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
    public partial class RedactCategory : Form
    {
        public RedactCategory(string ID, string Name)
        {
            InitializeComponent();
            this.ID = ID;
            this.Name = Name;
            textBox1.Text = Name;
        }

        // Выбранная услуга
        string ID, Name;   

        // Проверка на дубликат True - если нет дубликата
        bool CheckUser(string Name)
        {
            string CMD = $"Select * FROM Category WHERE Name = '{Name}' AND IdCategory != '{ID}'";

            using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
            {
                Con.Open();
                MySqlCommand cmd = new MySqlCommand(CMD, Con);
                bool Result = cmd.ExecuteScalar() == null;

                return Result;
            }
        }

        // Редактируем выбранную категорию
        private void redactCategory_Click(object sender, EventArgs e)
        {
            try
            {
                string CMD = $"UPDATE Category SET Name = '{textBox1.Text.ToString().Trim()}' WHERE IdCategory = '{ID}';";

                DialogResult dialogResult = MessageBox.Show("Изменить категорию?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    // Проверка на дубликат
                    if (!CheckUser(textBox1.Text.Trim()))
                    {
                        MessageBox.Show("Категория с данным названием уже существует в базе", "Ошибка операции", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        textBox1.Text = "";
                        return;
                    }

                    //Редактируем строку
                    using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                    {
                        Con.Open();
                        MySqlCommand cmd = new MySqlCommand(CMD, Con);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Категория обновлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                    return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            if (textBox1.Text.Trim() != Name && textBox1.Text.Trim().Length > 2)
                button1.Enabled = true;
            else
                button1.Enabled = false;
        }
        

        //Закрыть
        private void button8_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
