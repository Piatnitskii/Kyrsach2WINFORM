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

            //Заполнение комбобокса и дата грида
            PullData();

            // Включаем двойную буферизацию для DataGridView
            Optimize.SetDoubleBuffered(dataGridView2);
            dataGridView2.CellBorderStyle = DataGridViewCellBorderStyle.None;
        }

        DataTable DtEmploey = new DataTable();

        //Заполнение комбобокса и грида
        void PullData()
        {
            try
            {
                string CMD = "SELECT * FROM Role;";
                string CMD2 = "SELECT IdEmploye, CONCAT_WS(' ', Employe.Name, Employe.Surname, Employe.Patronymic) AS 'ФИО сотрудника', Phone FROM Employe";

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

                    //Добавляем данные в дата грид (Сотрудники)
                    cmd = new MySqlCommand(CMD2, Con);
                    cmd.ExecuteNonQuery();
                    
                    Ad = new MySqlDataAdapter(cmd);
                    Ad.Fill(DtEmploey);

                    dataGridView2.DataSource = DtEmploey.DefaultView;
                    dataGridView2.Columns["IdEmploye"].Visible = false;
                    dataGridView2.Columns["Phone"].Visible = false;
                    dataGridView2.Columns["ФИО сотрудника"].DefaultCellStyle.Padding = new Padding(0, 5, 0, 5);

                    dataGridView2.ClearSelection(); //Очистка выделения
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось заполнить таблицу ролей или сотрудников: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        int CurrentRowIndex = -1; // Индекс выбранной строки
        string Id_Employe = "-1";
        // Получаем инфу по выбранной строке ДатаГрида
        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            CurrentRowIndex = e.RowIndex;

            if (CurrentRowIndex == -1)
            {
                Id_Employe = "-1";
                dataGridView2.ClearSelection();
                return;
            }
            Id_Employe = dataGridView2.Rows[CurrentRowIndex].Cells["IdEmploye"].Value.ToString();
            label2.Text = $"Выбранный сотрудник: {dataGridView2.Rows[CurrentRowIndex].Cells["ФИО сотрудника"].Value.ToString()}, +{dataGridView2.Rows[CurrentRowIndex].Cells["Phone"].Value.ToString()}";
            CheckData();
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

            if (CurrentRowIndex != -1 && Id_Employe != "-1" && textBox5.Text.Trim() != "" && textBox4.Text.Trim() != "" && textBox4.Text.Trim().Length == 8)
                button2.Enabled = true;
            else
                button2.Enabled = false;
        }

        //Добавить
        private void addUser_Click(object sender, EventArgs e)
        {

            //Хешируем пароль
            string Password = GetHash(textBox4.Text.ToString());

            string Login = textBox5.Text.ToString();
            string Id_Role = comboBox1.SelectedValue.ToString();

            string CMD = $"INSERT INTO User (Id_Employe, Password, Login, Id_Role) VALUES ({Id_Employe}, '{Password}', '{Login}', '{Id_Role}');";
            try
            {
                DialogResult dialogResult = MessageBox.Show("Добавить пользователя?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    //Проверяем на дубликат
                    if (!CheckUser(Login))
                    {
                        MessageBox.Show("Пользователь с данным логином уже существует в базе", "Ошибка операции", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            textBox4.Text = "";
            textBox5.Text = "";
        }
        //Чистим выбор
        private void AddUser_Load(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection();
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


        //Поиск
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar) || (e.KeyChar >= 'a' && e.KeyChar <= 'z') || (e.KeyChar >= 'A' && e.KeyChar <= 'Z'))
                e.Handled = true;

            else
                e.Handled = false;
        }

        //Поиск
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

            DataView dv = DtEmploey.DefaultView;
            string search = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(search))
            {
                dv.RowFilter = "";  // Показать все
            }
            else
            {
                // Поиск по колонкам
                dv.RowFilter = "[ФИО сотрудника] LIKE '%" + search + "%'";
            }

            dataGridView2.Refresh();  // Обновить вид
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
