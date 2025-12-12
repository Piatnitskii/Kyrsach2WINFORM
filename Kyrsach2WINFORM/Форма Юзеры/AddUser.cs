using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kyrsach2WINFORM
{
    public partial class AddUser : Form
    {
        public AddUser()
        {
            InitializeComponent();

            //Заполнение комбобокса
            PullData();
        }

        //Заполнение комбобокса
        void PullData()
        {
            try
            {
                string CMD = "SELECT * FROM Role;";
                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    Con.Open();

                    MySqlCommand cmd = new MySqlCommand(CMD, Con);
                    cmd.ExecuteNonQuery();

                    DataTable Dt = new DataTable();
                    MySqlDataAdapter Ad = new MySqlDataAdapter(cmd);

                    Ad.Fill(Dt);

                    comboBox1.ValueMember = "IdRole";
                    comboBox1.DisplayMember = "Name";
                    comboBox1.DataSource = Dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        //Возвращает ХЕШ переданной строки
        string GetHash(string password)
        {
            var sha256 = SHA256.Create();
            var sha256byte = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

            string hash = BitConverter.ToString(sha256byte).Replace("-", "");
            return hash;
        }

        // Проверка на дубликат True - если нет дубликата
        bool CheckUser(string Login)
        {
            string CMD = $"Select * FROM User WHERE Login = '{Login}'";

            using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
            {
                Con.Open();
                MySqlCommand cmd = new MySqlCommand(CMD, Con);
                bool Result = cmd.ExecuteScalar() == null;

                return Result;
            }
        }

        //Проверка заполнености обязательных полей
        void CheckData()
        {

            if (textBox1.Text.Trim() != "" && textBox2.Text.Trim() != "" && textBox5.Text.Trim() != "" && textBox4.Text.Trim() != "" && textBox4.Text.Trim().Length == 8)
                button2.Enabled = true;
            else
                button2.Enabled = false;
        }

        //Добавить
        private void addUser_Click(object sender, EventArgs e)
        {

            string Name = textBox2.Text.ToString();
            string Surname = textBox1.Text.ToString();
            string Patronymic = textBox3.Text.ToString();

            //Хешируем пароль
            string Password = GetHash(textBox4.Text.ToString());

            string Login = textBox5.Text.ToString();
            string Id_Role = comboBox1.SelectedValue.ToString();

            string CMD = $"INSERT INTO User (Name, Surname, Patronymic, Password, Login, Id_Role) VALUES ('{Name}', '{Surname}', '{Patronymic}', '{Password}', '{Login}', '{Id_Role}');";
            try
            {
                DialogResult dialogResult = MessageBox.Show("Добавить пользователя?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    if (textBox3.Text.Trim() == "")
                    {
                        DialogResult dialogResultTwo = MessageBox.Show("Оставить поле 'Отчество' пустым?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        //В случае надобности удаляем отчество
                        if (dialogResultTwo == DialogResult.Yes)
                            CMD = $"INSERT INTO User (Name, Surname, Password, Login, Id_Role) VALUES ('{Name}', '{Surname}', '{Password}', '{Login}', '{Id_Role}');";
                        else
                            return;
                    }

                    //Проверяем на дубликат
                    if (!CheckUser(Login))
                    {
                        MessageBox.Show("Пользователь с данным логином уже существует в базе", "Ошибка операции", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Clear();    // Очистка
                        return;
                    }

                    //Добавляем строку
                    using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                    {
                        Con.Open();
                        MySqlCommand cmd = new MySqlCommand(CMD, Con);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Пользователь был успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Clear();    // Очистка
                }
                else
                    return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        //Чистим все элементы
        void Clear()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
        }



        // Первая буква ЗАГЛАВНАЯ
        void Chars(string Text, TextBox Element)
        {
            int selectionStart = Element.SelectionStart;
            int selectionLength = Element.SelectionLength;

            ArrayList chars = new ArrayList();
            string result = "";

            if (!string.IsNullOrEmpty(Text))
            {

                for (int i = 0; i < Text.Length; i++)
                {
                    if (i == 0)
                        chars.Add(Char.ToUpper(Text[0]));               // Добавляем первую букву в верхнем регистре
                    else
                        chars.Add(Char.ToLower(Text[i]));               // Добавляем остальные символы без изменений
                }

                foreach (var item in chars)
                {
                    result += item.ToString();
                }
            }

            Element.Text = result;
            Element.SelectionStart = selectionStart;
            Element.SelectionLength = selectionLength;
        }

        #region Настройка полей


        //Фамилия //Имя //Отчество
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar) || (e.KeyChar >= 'a' && e.KeyChar <= 'z') || (e.KeyChar >= 'A' && e.KeyChar <= 'Z'))
                e.Handled = true;

            else
                e.Handled = false;
        }

        //Фамилия
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Chars(textBox1.Text, textBox1);
            CheckData();
        }

        //Имя
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Chars(textBox2.Text, textBox2);
            CheckData();
        }
        
        //Отчество
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            Chars(textBox3.Text, textBox3);
            CheckData();
        }

        //Логин
        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            CheckData();
        }

        //Пароль
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            CheckData();
        }

        //Логин  //Пароль
        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем control символы (например, Backspace)
            if (char.IsControl(e.KeyChar))
            {
                e.Handled = false;
                return;
            }

            // Разрешаем английские буквы (верхний и нижний регистр)
            if (char.IsLetter(e.KeyChar))
            {
                e.Handled = false;
                return;
            }

            // Разрешаем цифры
            if (char.IsDigit(e.KeyChar))
            {
                e.Handled = false;
                return;
            }
            // Разрешаем определённые спецсимволы
            char[] allowedSpecialChars = { '_', '-', '@', '.', '!', '#', '$', '%', '&', '*', '(', ')' };
            if (allowedSpecialChars.Contains(e.KeyChar))
            {
                e.Handled = false;
                return;
            }

            e.Handled = true;
        }
        #endregion

        //Закрыть
        private void button8_Click(object sender, EventArgs e)
        {
            this.Close();
        }

       
    }
    


}
