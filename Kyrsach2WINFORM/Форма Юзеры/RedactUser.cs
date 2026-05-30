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

            label1.Text = $"Выбранный сотрудник: {userSystem.Fio}, +{userSystem.Phone}, {userSystem.Post}";

            if (userSystem.IdUser == ConnectAndData.ID)
                comboBox1.Enabled = false;

            //Заполнение комбобокса и дата грида
            PullData();

            // Заполняем поля выбранным объектом
            textBox5.Text = userSystem.Login;

            //Контрольная проверка
            CheckData();

        }

        DataTable DtEmploey = new DataTable();
        //Заполнение комбобокса
        void PullData()
        {
            try
            {
                string CMD = "SELECT * FROM Role;";
                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    Con.Open();

                    //Заполняем комбобокс ролями
                    MySqlCommand cmd = new MySqlCommand(CMD, Con);
                    cmd.ExecuteNonQuery();

                    DataTable Dt = new DataTable();
                    MySqlDataAdapter Ad = new MySqlDataAdapter(cmd);

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
            //Если что то поменялось и при этом не равно пустоте
            if ( (textBox5.Text.Trim() != userSystem.Login || textBox4.Text.Trim().Length != 0 || comboBox1.SelectedValue.ToString() != userSystem.Id_Role) && (textBox5.Text.Trim() != ""))
            {
                //если поменяли все таки пароль, то он должен быть 8 цифр
                if( textBox4.Text.Trim().Length > 0 && textBox4.Text.Trim().Length != 8)
                    button1.Enabled = false;
                else
                    button1.Enabled = true;
            }
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
            string Password = "";
            //Хешируем пароль
            if (textBox4.Text.ToString().Trim().Length != 0)
                Password = GetHash(textBox4.Text.ToString());

            string Role = comboBox1.SelectedValue.ToString();
            string Login = textBox5.Text.ToString();

            string CMD;
            if (Password != "") //Если что то внесли в строку с паролем, меняем пароль
                CMD = $"UPDATE User SET Id_Role='{Role}', Login = '{textBox5.Text.ToString().Trim()}', Password = '{Password}' WHERE IdUser = '{userSystem.IdUser}';";
            else
                CMD = $"UPDATE User SET Id_Role='{Role}', Login = '{textBox5.Text.ToString().Trim()}' WHERE IdUser = '{userSystem.IdUser}';";

            try
            {
                DialogResult dialogResult = MessageBox.Show("Изменить данные пользователя?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
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

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }

}
