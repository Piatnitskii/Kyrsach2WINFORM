using MySql.Data.MySqlClient;
using System;
using System.Collections;
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
    public partial class RedactClient : Form
    {
        //Fields
        Client client;      // Наш выбранный клиент

        public RedactClient(Client client)
        {
            InitializeComponent();
            this.client = client;

            // Отображаем данные
            textBox1.Text = client.Surname;
            textBox2.Text = client.Name;
            textBox3.Text = client.Patronymic;

            maskedTextBox1.Text = client.Phone.Remove(0, 1);
        }

        // Проверка на дубликат True - если нет дубликата
        bool CheckClient(string NumberPhone, string Name, string Surname, string Patronymic) // ТУТ самое главное чтобы не было точного совпадения по всем столбцам
        {
            string CMD = $"Select * FROM Client WHERE Phone = '{NumberPhone}' AND Name = '{Name}' AND Surname = '{Surname}' AND Patronymic = '{Patronymic}' AND IdClient != '{client.IdClient}';";

            using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
            {
                Con.Open();
                MySqlCommand cmd = new MySqlCommand(CMD, Con);
                bool Result = cmd.ExecuteScalar() == null;

                return Result;
            }
        }

        //Редактировать
        private void redactClient_Click(object sender, EventArgs e)
        {

            string Name = textBox2.Text.ToString();
            string Surname = textBox1.Text.ToString();
            string Patronymic = textBox3.Text.ToString();
            string NumberPhone = maskedTextBox1.Text.Replace("+", "").Replace("(", "").Replace(")", "").Replace("-", "");

            string CMD = $"UPDATE Client SET Name = '{Name}', Surname = '{Surname}', Patronymic = '{Patronymic}', Phone = '{NumberPhone}' WHERE IdClient = '{client.IdClient}';";

            DialogResult dialogResult = MessageBox.Show("Изменить запись клиента?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                if (textBox3.Text.Trim() == "")
                {
                    DialogResult dialogResultTwo = MessageBox.Show("Оставить поле 'Отчество' пустым?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (!(dialogResultTwo == DialogResult.Yes))
                        return;
                }

                //Проверяем на дубликат
                if (!CheckClient(NumberPhone, Name, Surname, Patronymic))
                {
                    MessageBox.Show("Данынй клиент уже существует в базе!", "Ошибка операции", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Clear();    // Очистка
                    return;
                }

                //Редактируем строку
                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    Con.Open();
                    MySqlCommand cmd = new MySqlCommand(CMD, Con);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Клиент был успешно обновлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            else
                return;
        }
        //Чистим все элементы
        void Clear()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            maskedTextBox1.Text = "";
        }

        //Проверка заполнености обязательных полей
        void CheckData()
        {
            if ( (textBox1.Text != client.Surname || textBox2.Text != client.Name || textBox3.Text != client.Patronymic || maskedTextBox1.Text.Replace("+", "").Replace("(", "").Replace(")", "").Replace("-", "") != client.Phone) && (textBox1.Text.Trim() != "" && textBox2.Text.Trim() != ""  && maskedTextBox1.Text.Length == 16))
                button1.Enabled = true;
            else
                button1.Enabled = false;
        }

        #region Настройка полей
        
        
        //Фамилия
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Chars(textBox1.Text, textBox1);
            CheckData();
        }
        //имя
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

        //Фамилия-имя-отчество
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar) || (e.KeyChar >= 'a' && e.KeyChar <= 'z') || (e.KeyChar >= 'A' && e.KeyChar <= 'Z'))
            {
                e.Handled = true;
            }
            else { e.Handled = false; }
        }

        //телефон
        private void maskedTextBox1_TextChanged(object sender, EventArgs e)
        {
            CheckData();
        }

        //телефон
        private void maskedTextBox1_Click(object sender, EventArgs e)
        {
            int position = maskedTextBox1.SelectionStart;

            var Array = maskedTextBox1.Text.ToCharArray();
            var Count = 3;

            for (int i = 3; i < Array.Length; i++)
            {
                if (Array[i] == ' ')
                    break;
                Count++;
            }

            // Проверяем символ в этом месте
            if (position < maskedTextBox1.Text.Length)
            {
                char ch = maskedTextBox1.Text[position];

                if (char.IsDigit(maskedTextBox1.Text[position]) && position > 3)
                {
                    maskedTextBox1.SelectionStart = position;
                }
                else if (ch == ' ')
                {
                    maskedTextBox1.Select(Count, 0);
                }
                else if (ch == ')' && char.IsDigit(maskedTextBox1.Text[position - 1]))
                {
                    maskedTextBox1.SelectionStart = position;
                }
                else if (ch == '-' && char.IsDigit(maskedTextBox1.Text[position - 1]))
                {
                    maskedTextBox1.SelectionStart = position;
                }
                else
                {
                    maskedTextBox1.Select(Count, 0);
                }
            }
            else
                maskedTextBox1.Select(Count, 0);
        }
        

        //Логика - Первая буква ЗАГЛАВНАЯ
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
        #endregion


        //Закрыть
        private void button8_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
