using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//
using MySql.Data.MySqlClient;


namespace Kyrsach2WINFORM
{
    public partial class Yslygy : Form
    {
        public Yslygy()
        {
            InitializeComponent();

            // Включаем двойную буферизацию для DataGridView
            Optimize.SetDoubleBuffered(dataGridView2);
            dataGridView2.CellBorderStyle = DataGridViewCellBorderStyle.None;

            //Убираем для админа некоторый функционал
            if (ConnectAndData.Role == "1")
            {
                button1.Visible = false;
                button6.Visible = false;
                button2.Visible = false;
                dataGridView2.Size = new Size(1201, 477);
            }

            //Фильтрация, заполняем из базы
            PullData();

            //Сортировка
            comboBox1.Items.Add("По умолчанию");
            comboBox1.Items.Add("Возр. стоимости");
            comboBox1.Items.Add("Убыв. стоимости");
            comboBox1.SelectedItem = "По умолчанию";
        }

        //Заполнение комбобокса 2
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

                    Dt.Columns.Add("IdCategory");
                    Dt.Columns.Add("Name");
                    Dt.Rows.Add("0", "Все диапазоны");

                    Ad.Fill(Dt);
                    
                    comboBox2.ValueMember = "IdCategory";
                    comboBox2.DisplayMember = "Name";

                    comboBox2.DataSource = Dt;
                    //устанавливаем выбранный элемент
                    comboBox2.SelectedValue = "0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        // При загрузке формы убираем выделение
        private void Yslygy_Load(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection();
        }


        int CurrentRowIndex; // Индекс выбранной строки
        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            CurrentRowIndex = e.RowIndex;

            if (CurrentRowIndex == -1)
            {
                button1.Enabled = false;
                button6.Enabled = false;
                dataGridView2.ClearSelection();
                return;
            }

            button1.Enabled = true;
            button6.Enabled = true;
        }


        // Заполняет данными таблицу
        void FillDataGrid(string CMDString)
       {
            try
            {
                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    Con.Open();

                    MySqlCommand cmd = new MySqlCommand(CMDString, Con);
                    MySqlDataReader RDR = cmd.ExecuteReader();

                    dataGridView2.Rows.Clear();
                    dataGridView2.Columns.Clear();

                    dataGridView2.Columns.Add("Number", "Номер");       // Прячем
                    dataGridView2.Columns["Number"].Visible = false;    //

                    dataGridView2.Columns.Add("Name", "Название");
                    dataGridView2.Columns.Add("Description", "Описание");
                    dataGridView2.Columns.Add("NameCategory", "Категория");
                    dataGridView2.Columns.Add("Cost", "Стоимость (руб.)");
                    dataGridView2.Columns.Add("Duration", "Продолжительность (мин.)");
                    dataGridView2.Columns.Add("Id_Category", "");
                    dataGridView2.Columns["Id_Category"].Visible = false;

                    foreach (DataGridViewColumn column in dataGridView2.Columns)
                        column.MinimumWidth = 100;

                    //Заполняем данными
                    while (RDR.Read())
                    {
                        dataGridView2.Rows.Add( RDR[0].ToString(), RDR[1].ToString(), RDR[2].ToString(), RDR[3].ToString(), RDR[4].ToString(), RDR[5].ToString(), RDR[6].ToString());
                    }

                    LastCMD = CMDString;

                    button1.Enabled = false;
                    button6.Enabled = false;

                    dataGridView2.ClearSelection();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        #region "Живой поиск" - Фильтрация - Сортировка
        // Строка запроса default
        string CMD = "SELECT IdService, Service.Name, Service.Description, Category.Name, Cost, Duration, Category.IdCategory, Id_Category FROM Service INNER JOIN `Category` ON IdCategory = Id_Category";

        //СОРТИРОВКА
        //СОРТИРОВКА
        //СОРТИРОВКА  (по стоимости)
        string SortMod;
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 1:
                    // по возрастанию
                    SortMod = "ASC";
                    break;

                case 2:
                    // по убыванию
                    SortMod = "DESC";
                    break;
            }

            comboBox2_SelectedIndexChanged(comboBox2.SelectedValue.ToString(), EventArgs.Empty);
        }
        //ФИЛЬТРАЦИЯ
        //ФИЛЬТРАЦИЯ
        //ФИЛЬТРАЦИЯ (по категории)
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string I = comboBox2.SelectedValue.ToString();
            switch (I)
            {
                case "0": // ТУТ ЗНАК !
                    if (!(comboBox1.SelectedIndex == 0))
                        FillDataGrid(CMD + $" WHERE Service.Name LIKE '%{textBox2.Text}%' ORDER BY Cost {SortMod};");
                    else
                        FillDataGrid(CMD + $" WHERE Service.Name LIKE '%{textBox2.Text}%';");
                    break;
                default:
                    if (!(comboBox1.SelectedIndex == 0))
                        FillDataGrid(CMD + $" WHERE Service.Name LIKE '%{textBox2.Text}%' AND Id_Category = {I} ORDER BY Cost {SortMod};");
                    else
                        FillDataGrid(CMD + $" WHERE Service.Name LIKE '%{textBox2.Text}%' AND Id_Category = {I};");
                    break;
            }
            
        }

        // "Живой" поиск (по названию)
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            comboBox2_SelectedIndexChanged(comboBox2.SelectedValue.ToString(), EventArgs.Empty);
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar))
                e.Handled = true;

            else
                e.Handled = false;
        }
        #endregion



        string LastCMD = ""; //Последний выполненый запрос

        //Редактировать
        private void button1_Click(object sender, EventArgs e)
        {
            string ID = dataGridView2.Rows[CurrentRowIndex].Cells["Number"].Value.ToString();
            string Name = dataGridView2.Rows[CurrentRowIndex].Cells["Name"].Value.ToString();
            string Cost = dataGridView2.Rows[CurrentRowIndex].Cells["Cost"].Value.ToString();
            string Duration = dataGridView2.Rows[CurrentRowIndex].Cells["Duration"].Value.ToString();
            string Description = dataGridView2.Rows[CurrentRowIndex].Cells["Description"].Value.ToString();
            string NameCategory = dataGridView2.Rows[CurrentRowIndex].Cells["NameCategory"].Value.ToString();
            string Id_Category = dataGridView2.Rows[CurrentRowIndex].Cells["Id_Category"].Value.ToString();

            Service service = new Service(ID, Name, Cost, Duration, Description, Id_Category, NameCategory);

            ReadctYslyg FormA = new ReadctYslyg(service);
            FormA.ShowDialog();
            FillDataGrid(LastCMD);
        }

        //Добавить
        private void button2_Click(object sender, EventArgs e)
        {
            AddYslyga FormA = new AddYslyga();
            FormA.ShowDialog();
            FillDataGrid(LastCMD);
        }

        //Удаление
        private void deleteService_Click(object sender, EventArgs e)
        {
            string ID = dataGridView2.Rows[CurrentRowIndex].Cells["Number"].Value.ToString();
            string Name = dataGridView2.Rows[CurrentRowIndex].Cells["Name"].Value.ToString();

            string CMD = $"DELETE FROM Service WHERE IdService = {ID};";
            try
            {
                DialogResult dialogResult = MessageBox.Show($"Удалить услугу {Name}?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    //Удаляем строку
                    using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                    {
                        Con.Open();
                        MySqlCommand cmd = new MySqlCommand(CMD, Con);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Услуга была успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    FillDataGrid(LastCMD);
                }
                else
                    return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Чистим 
        private void dataGridView2_Sorted(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection();
        }


        //Закрыть
        private void button8_Click(object sender, EventArgs e)
        {
            MenuAdmin.DisableButton();
            this.Dispose();
            this.Close();
        }


        //Подсветка строки на которую направлен указатель мыши
        private void dataGridView2_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
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
