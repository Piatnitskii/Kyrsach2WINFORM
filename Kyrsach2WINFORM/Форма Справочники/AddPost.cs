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
    public partial class AddRol : Form
    {
        public AddRol()
        {
            InitializeComponent();
        }


        

        // Проверка на дубликат True - если нет дубликата
        bool CheckPost(string NamePost)
        {
            string CMD = $"Select * FROM Post WHERE Name = '{NamePost}';";

            using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
            {
                Con.Open();
                MySqlCommand cmd = new MySqlCommand(CMD, Con);
                bool Result = cmd.ExecuteScalar() == null;

                return Result;
            }
        }

        //Добавить
        private void addPost_Click(object sender, EventArgs e)
        {
            string NamePost = textBox1.Text.Trim();
            string CMD = $"INSERT INTO Post (Name) VALUES ('{NamePost}');";

            try
            {
                DialogResult dialogResult = MessageBox.Show("Добавить должность?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    //Проверяем на дубликат
                    if (!CheckPost(NamePost))
                    {
                        MessageBox.Show("Эта должность уже существует в базе", "Ошибка операции", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        textBox1.Text = "";    // Очистка
                        return;
                    }


                    //Добавляем строку
                    using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                    {
                        Con.Open();
                        MySqlCommand cmd = new MySqlCommand(CMD, Con);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("должность была успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox1.Text = "";    // Очистка
                }
                else
                    return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Должность - НАСТРОЙКА ПОЛЯ - только русские буквы и пробелы
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsWhiteSpace(e.KeyChar) && !char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar) || (e.KeyChar >= 'a' && e.KeyChar <= 'z') || (e.KeyChar >= 'A' && e.KeyChar <= 'Z'))
                e.Handled = true;

            else
                e.Handled = false;
        }

        //Должность
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim() != "")
                button2.Enabled = true;
            else
                button2.Enabled = false;
        }

        //Закрыть
        private void button8_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
