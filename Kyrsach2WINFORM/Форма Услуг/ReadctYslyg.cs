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
    public partial class ReadctYslyg : Form
    {

        //Fields

        //Выбранная нами услуга
        Service service;

        public ReadctYslyg(Service service)
        {
            InitializeComponent();

            this.service = service;

            numericUpDown2.Increment = 0.01m;     // Шаг изменения
            numericUpDown2.ThousandsSeparator = true; // Разделитель тысяч


            //Отображаем данные
            PullData();

            numericUpDown2.Value = Convert.ToDecimal(service.Cost);
            numericUpDown1.Value = Convert.ToDecimal(service.Duration);

            textBox1.Text = service.Name;
            textBox2.Text = service.Description;

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

                    //устанавливаем выбранный элемент
                    comboBox1.SelectedValue = service.Id_Category;
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

            if ( (textBox1.Text.Trim() != service.Name || textBox2.Text.Trim() != service.Description || numericUpDown1.Value != Convert.ToDecimal(service.Duration) || numericUpDown2.Value != Convert.ToDecimal(service.Cost) || comboBox1.SelectedValue.ToString() != service.Id_Category) && (numericUpDown2.Text != ""  && numericUpDown1.Text != "" && textBox1.Text.Trim() != "" && numericUpDown1.Value > 0 && numericUpDown2.Value > 0))
                button1.Enabled = true;
            else
                button1.Enabled = false;
        }

        // Проверка на дубликат. True - если нет дубликата
        bool CheckService(string Name)
        { 
            string CMD = $"Select * FROM Service WHERE Name = '{Name}' AND IdService != {service.IdService}";

            using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
            {
                Con.Open();
                MySqlCommand cmd = new MySqlCommand(CMD, Con);
                bool Result = cmd.ExecuteScalar() == null;

                return Result;
            }
        }
        

        //Редактировать
        private void redactService_Click(object sender, EventArgs e)
        {
            try
            {

                string Name = textBox1.Text.ToString().Trim();
                string Description = textBox2.Text.ToString().Trim();
                string Cost = numericUpDown2.Value.ToString().Replace(",", ".");
                string Duration = numericUpDown1.Value.ToString();
                string Category = comboBox1.SelectedValue.ToString();

                string CMD = $"UPDATE Service SET Name = '{Name}', Description = '{Description}', Id_Category = '{Category}', Cost = {Cost}, Duration = '{Duration}' WHERE IdService = {service.IdService};";

                DialogResult dialogResult = MessageBox.Show("Изменить услугу?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    if (Description == "")
                    {
                        DialogResult dialogResultTwo = MessageBox.Show("Оставить поле 'Описание' пустым?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (!(dialogResultTwo == DialogResult.Yes))
                            return;
                    }

                    //Проверяем на дубликат
                    if (!CheckService(Name))
                    {
                        MessageBox.Show("Данная услуга уже существует в базе", "Ошибка операции", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    //Редактируем строку
                    using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                    {
                        Con.Open();
                        MySqlCommand cmd = new MySqlCommand(CMD, Con);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Услуга была успешно обновлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            CheckData();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
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

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            CheckData();
        }

        //Название - Описание
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar))
                e.Handled = true;

            else
                e.Handled = false;
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
        //Настройка валидации поля Duration
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
