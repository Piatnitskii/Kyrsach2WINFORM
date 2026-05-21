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

            // Включаем двойную буферизацию для DataGridView
            Optimize.SetDoubleBuffered(dataGridView2);

            numericUpDown2.Increment = 0.01m;     // Шаг изменения
            numericUpDown2.ThousandsSeparator = true; // Разделитель тысяч

            //Заного проверяем все поля
            CheckData();
        }

        DataTable DtCategory = new DataTable();
        //Заполнение комбобокса категориями
        void PullData()
        {
            try
            {
                string CMD = "SELECT IdCategory as 'ID', Name as 'Название' FROM Category;";
                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    Con.Open();

                    MySqlCommand cmd = new MySqlCommand(CMD, Con);
                    cmd.ExecuteNonQuery();

                    MySqlDataAdapter Ad = new MySqlDataAdapter(cmd);

                    Ad.Fill(DtCategory);

                    dataGridView2.DataSource = DtCategory.DefaultView;
                    dataGridView2.Columns["ID"].Visible = false;
                    dataGridView2.Columns["Название"].DefaultCellStyle.Padding = new Padding(0, 5, 0, 5);

                    dataGridView2.ClearSelection(); //Очистка выделения
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
            if (Id_ServiceCategory != "-1" && textBox1.Text.Trim() != "" && numericUpDown2.Value != 0  && numericUpDown1.Value != 0)
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

                string CMD = $"INSERT INTO Service (Name, Description, Id_Category, Cost, Duration) VALUES ('{Name}', '{Description}', '{Id_ServiceCategory}',  '{Cost}', '{Duration}');";

                DialogResult dialogResult = MessageBox.Show("Добавить услугу?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    if (Description == "")
                    {
                        DialogResult dialogResultTwo = MessageBox.Show("Оставить поле 'Описание' пустым?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dialogResultTwo == DialogResult.Yes)
                            CMD = $"INSERT INTO Service (Name, Id_Category, Cost, Duration) VALUES ('{Name}', '{Id_ServiceCategory}',  '{Cost}', '{Duration}');"; // тут меняем строку запроса
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

        private void textBox3_TextChanged_1(object sender, EventArgs e)
        {
            DataView dv = DtCategory.DefaultView;
            string search = textBox3.Text.Trim();

            if (string.IsNullOrEmpty(search))
            {
                dv.RowFilter = "";  // Показать все
            }
            else
            {
                // Поиск по колонкам
                dv.RowFilter = "[Название] LIKE '%" + search + "%'";
            }

            dataGridView2.Refresh();  // Обновить вид
            SelectRow();
        }

        int CurrentRowIndex = -1; // Индекс выбранной строки
        string Id_ServiceCategory = "-1";
        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            CurrentRowIndex = e.RowIndex;

            if (CurrentRowIndex == -1)
            {
                Id_ServiceCategory = "-1";
                dataGridView2.ClearSelection();
                CheckData();
                return;
            }

            Id_ServiceCategory = dataGridView2.Rows[CurrentRowIndex].Cells["ID"].Value.ToString();
            CheckData();
        }

        //Отображаем ранее выбранную категорию услуг
        void SelectRow()
        {
            if (Id_ServiceCategory != "-1") // Если выбран ранее, отображаем
            {
                bool rowFound = false; // Для отслеживания, нашли ли мы строку

                foreach (DataGridViewRow row in dataGridView2.Rows)
                {
                    // Проверяем, совпадает ли ID категории с ID в строке
                    if (row.Cells["ID"].Value.ToString() == Id_ServiceCategory)
                    {
                        dataGridView2.CurrentCell = row.Cells[1]; // Устанавливаем текущую ячейку
                        rowFound = true; // Отмечаем, что строка найдена
                        row.Selected = true; // Подсвечиваем строку
                        break;
                    }
                }

                if (!rowFound)
                    dataGridView2.ClearSelection(); // Если строка не найдена, очищаем выделение
            }
            else
            {
                dataGridView2.ClearSelection(); // Если не выбран клиент, очищаем выделение
            }
        }

        //Поиск - русские, пробелы, управляющие символы
        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar) || (e.KeyChar >= 'a' && e.KeyChar <= 'z') || (e.KeyChar >= 'A' && e.KeyChar <= 'Z'))
                e.Handled = true;

            else
                e.Handled = false;
        }

        private void AddYslyga_Load(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection();
        }

        //Подсветка строки на которую направлен указатель мыши
        private void dataGridView2_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
                dataGridView2.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGray;
        }
        //Возвращаем состояние строки на исходную, когда указатель "Покидает" строку
        private void dataGridView2_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
                dataGridView2.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
        }
    }
}
