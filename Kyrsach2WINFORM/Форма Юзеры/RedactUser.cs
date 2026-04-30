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
    public partial class RedactUser : Form
    {
        //Fields

        // Объект пользователя
        UserSystem userSystem;

        public RedactUser(UserSystem userSystem)
        {
            this.userSystem = userSystem;
            InitializeComponent();

            if (userSystem.IdUser == ConnectAndData.ID)
                comboBox1.Enabled = false;

            //Заполняем комбобокс
            PullData();

            // Заполняем поля выбранным объектом
            textBox1.Text = userSystem.Surname;
            textBox2.Text = userSystem.Name;
            textBox3.Text = userSystem.Patronymic;
            textBox5.Text = userSystem.Login;

            //Контрольная проверка
            CheckData();
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

                    //Dt.Columns.Add("IdRole");
                    //Dt.Columns.Add("Name");
                    Ad.Fill(Dt);

                    comboBox1.ValueMember = "IdRole";
                    comboBox1.DisplayMember = "Name";
                    comboBox1.DataSource = Dt;

                    //устанавливаем выбранный элемент
                    comboBox1.SelectedValue = userSystem.Id_Role;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        //Проверка заполнености обязательных полей
        void CheckData()
        {
            //var A = textBox1.Text.Trim() != userSystem.Surname;
            //var v = textBox2.Text.Trim() != userSystem.Name;
            //var c = textBox3.Text.Trim() != userSystem.Patronymic;
            //var b = comboBox1.SelectedValue.ToString() != userSystem.Id_Role;


            //Если что то поменялось и при этом не равно пустоте
            if ( (textBox5.Text.Trim() != userSystem.Login || textBox4.Text.Trim().Length != 0 || textBox1.Text.Trim() != userSystem.Surname || textBox2.Text.Trim() != userSystem.Name || textBox3.Text.Trim() != userSystem.Patronymic || comboBox1.SelectedValue.ToString() != userSystem.Id_Role) && (textBox1.Text.Trim() != "" && textBox2.Text.Trim() != ""  && textBox5.Text.Trim() != ""))
                button1.Enabled = true;
            else
                button1.Enabled = false;
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

        //Возвращает ХЕШ переданной строки
        string GetHash(string password)
        {
            var sha256 = SHA256.Create();
            var sha256byte = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

            string hash = BitConverter.ToString(sha256byte).Replace("-", "");
            return hash;
        }

        //Изменить
        private void redactUser_Click(object sender, EventArgs e)
        {
            string Name = textBox2.Text.ToString();
            string Surname = textBox1.Text.ToString();
            string Patronymic = textBox3.Text.ToString();

            string Password = "";
            //Хешируем пароль
            if (textBox4.Text.ToString().Trim().Length != 0)
                Password = GetHash(textBox4.Text.ToString());

            string Role = comboBox1.SelectedValue.ToString();
            string Login = textBox5.Text.ToString();

            string CMD;
            if (Password != "") //Если что то внесли в строку с паролем, меняем пароль
                CMD = $"UPDATE User SET Name='{Name}', Surname='{Surname}', Patronymic='{Patronymic}', Id_Role='{Role}', Login = '{textBox5.Text.ToString().Trim()}', Password = '{Password}' WHERE IdUser = '{userSystem.IdUser}';";
            else
                CMD = $"UPDATE User SET Name='{Name}', Surname='{Surname}', Patronymic='{Patronymic}', Id_Role='{Role}', Login = '{textBox5.Text.ToString().Trim()}' WHERE IdUser = '{userSystem.IdUser}';";

            try
            {
                DialogResult dialogResult = MessageBox.Show("Изменить данные пользователя?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    if (textBox3.Text.Trim() == "")
                    {
                        DialogResult dialogResultTwo = MessageBox.Show("Оставить поле 'Отчество' пустым?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if ( !(dialogResultTwo == DialogResult.Yes))
                            return;
                    }

                    //Проверяем на дубликат
                    if (!CheckUser(Login) && Login != userSystem.Login)
                    {
                        MessageBox.Show("Пользователь с данным логином уже существует в базе", "Ошибка операции", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    //Изменяем строку
                    using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                    {
                        Con.Open();
                        MySqlCommand cmd = new MySqlCommand(CMD, Con);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Пользователь был успешно обновлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        // Переводит 1 букву в верхний регистр
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
                        chars.Add(Char.ToLower(Text[i]));                 // Добавляем остальные символы без изменений
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

        // Фамилия
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar) || (e.KeyChar >= 'a' && e.KeyChar <= 'z') || (e.KeyChar >= 'A' && e.KeyChar <= 'Z'))
            {
                e.Handled = true;
            }
            else { e.Handled = false; }
        }
        // Фамилия
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

        //Роль
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
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
