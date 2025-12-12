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
    public partial class AddYslyga : Form
    {
        public AddYslyga()
        {
            InitializeComponent();

            //Отображаем данные
            PullData();

            numericUpDown2.Increment = 0.01m;     // Шаг изменения
            numericUpDown2.ThousandsSeparator = true; // Разделитель тысяч

            //Заного проверяем все поля
            CheckData();
        }

        //Заполнение комбобокса категориями
        void PullData()
        {
            try
            {
                string CMD = "SELECT * FROM Category;";
                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    Con.Open();

                    MySqlCommand cmd = new MySqlCommand(CMD, Con);
                    cmd.ExecuteNonQuery();

                    DataTable Dt = new DataTable();
                    MySqlDataAdapter Ad = new MySqlDataAdapter(cmd);

                    Ad.Fill(Dt);
                    
                    comboBox1.ValueMember = "IdCategory";
                    comboBox1.DisplayMember = "Name";
                    
                    comboBox1.DataSource = Dt;
                    comboBox1.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        // Проверка на дубликат True - если нет дубликата
        bool CheckService(string Name)
        {
            string CMD = $"Select * FROM Service WHERE Name = '{Name}'";

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
            if (textBox1.Text.Trim() != "" && numericUpDown2.Value != 0  && numericUpDown1.Value != 0)
                button1.Enabled = true;
            else
                button1.Enabled = false;
        }

        

        //Добавить
        private void createService_Click(object sender, EventArgs e)
        {
            try
            {
                string Name = textBox1.Text.ToString().Trim();
                string Description = textBox2.Text.ToString().Trim();
                string Cost = numericUpDown2.Value.ToString().Replace(",", ".");
                string Duration = numericUpDown1.Value.ToString();
                string Category = comboBox1.SelectedValue.ToString();

                string CMD = $"INSERT INTO Service (Name, Description, Id_Category, Cost, Duration) VALUES ('{Name}', '{Description}', '{Category}',  '{Cost}', '{Duration}');";

                DialogResult dialogResult = MessageBox.Show("Добавить услугу?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    if (Description == "")
                    {
                        DialogResult dialogResultTwo = MessageBox.Show("Оставить поле 'Описание' пустым?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dialogResultTwo == DialogResult.Yes)
                            CMD = $"INSERT INTO Service (Name, Id_Category, Cost, Duration) VALUES ('{Name}', '{Category}',  '{Cost}', '{Duration}');"; // тут меняем строку запроса
                        else
                            return;
                    }

                    //Проверяем на дубликат
                    if (!CheckService(Name))
                    {
                        MessageBox.Show("Данная услуга уже существует в базе", "Ошибка операции", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                    MessageBox.Show("Услуга была успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            numericUpDown1.Value = 0;
            numericUpDown2.Value = 0;
            comboBox1.SelectedIndex = 0;
        }

        #region Настройка полей


        //Название - Описание
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar))
                e.Handled = true;

            else
                e.Handled = false;
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            CheckData();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            CheckData();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            CheckData();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckData();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            CheckData();
        }


        //Настройка валидации поля Cost
        private void numericUpDown2_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(numericUpDown2.Text))
            {
                numericUpDown2.Value = 0; // или другое значение по умолчанию
                numericUpDown2.Text = "0";
            }
        }
        //Настройка валидации поля Durations
        private void numericUpDown1_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(numericUpDown1.Text))
            {
                numericUpDown1.Value = 0; // или другое значение по умолчанию
                numericUpDown1.Text = "0";
            }
        }

        #endregion



        //Закрыть
        private void button8_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
