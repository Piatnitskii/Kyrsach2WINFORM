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
    public partial class AddZapicYslyga : Form
    {
        //Fields 
        Button button4; Button button5;

        public AddZapicYslyga(Control control, Control control5)
        {
            InitializeComponent();

            // Включаем двойную буферизацию для DataGridView1
            Optimize.SetDoubleBuffered(dataGridView1);

            // Включаем двойную буферизацию для DataGridView2
            Optimize.SetDoubleBuffered(dataGridView2);

            //Заполняем ДатаГрид данными
            FillDataGrid();

            // Если услуги уже были выбраны
            if (AddZapicWiz.CurrentFourForm.Count != 0)
                FillDataGrid2();

            button4 = (Button)control;
            button5 = (Button)control5;
        }

        //Заполняет ДатаГрид 2 данными, если услуги были выбраны раньше
        void FillDataGrid2()
        {
            try
            {
                foreach (Service service in AddZapicWiz.CurrentFourForm)
                {
                   dataGridView2.Rows.Add($"{service.IdService}", $"{service.Name}", $"{service.Cost}", $"{service.Duration}");    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        string CMD = "SELECT IdService as ID, Service.Name as 'Название', Cost as 'Цена', Duration as 'Время' FROM Service";

        //Заполняет ДатаГрид 1 данными и настраивает 2 ДатаГрид
        void FillDataGrid()
        {
            try
            {
                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    Con.Open();

                    MySqlCommand cmd = new MySqlCommand(CMD, Con);
                    cmd.ExecuteNonQuery();

                    DataTable Dt = new DataTable();
                    MySqlDataAdapter Ad = new MySqlDataAdapter(cmd);

                    Ad.Fill(Dt);

                    dataGridView1.DataSource = Dt;

                    //Настройка полей
                    dataGridView1.Columns["Название"].SortMode = DataGridViewColumnSortMode.NotSortable;
                    dataGridView1.Columns["Цена"].SortMode = DataGridViewColumnSortMode.NotSortable;
                    dataGridView1.Columns["Время"].SortMode = DataGridViewColumnSortMode.NotSortable;

                    dataGridView1.Columns["Название"].DefaultCellStyle.Padding = new Padding(0, 10, 0, 10);
                    dataGridView1.Columns["Цена"].DefaultCellStyle.Padding = new Padding(0, 10, 0, 10);
                    dataGridView1.Columns["Время"].DefaultCellStyle.Padding = new Padding(0, 10, 0, 10);

                    dataGridView2.Columns.Add("ID", "ID");
                    dataGridView2.Columns["ID"].Visible = false;
                    dataGridView2.Columns.Add("Название", "Название");
                    dataGridView2.Columns.Add("Цена", "Цена");
                    dataGridView2.Columns.Add("Время", "Время");

                    dataGridView2.Columns["Название"].DefaultCellStyle.Padding = new Padding(0, 10, 0, 10);
                    dataGridView2.Columns["Цена"].DefaultCellStyle.Padding = new Padding(0, 10, 0, 10);
                    dataGridView2.Columns["Время"].DefaultCellStyle.Padding = new Padding(0, 10, 0, 10);

                    dataGridView2.Columns["Название"].SortMode = DataGridViewColumnSortMode.NotSortable;
                    dataGridView2.Columns["Цена"].SortMode = DataGridViewColumnSortMode.NotSortable;
                    dataGridView2.Columns["Время"].SortMode = DataGridViewColumnSortMode.NotSortable;

                    dataGridView1.Columns["ID"].Visible = false;


                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                        column.MinimumWidth = 40;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //СПИСОК УСЛУГ
        int CurrentRowIndexOne; // Индекс выбранной строки
        // Получаем инфу по выбранной строке ДатаГрида
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            CurrentRowIndexOne = e.RowIndex;

            // Если тыкнули на шапку
            if (CurrentRowIndexOne == -1 )
            {
                dataGridView1.ClearSelection();
                button1.Enabled = false;
                return;
            }
            if (!CheckRow())    // Если такая запись уже есть во 2 гриде
            {
                button1.Enabled = false;
                return;
            }

            button1.Enabled = true;
        }

        //ДАТА ГРИД ВЫБРАННЫХ УСЛУГ
        int CurrentRowIndexTwo; // Индекс выбранной строки
        // Получаем инфу по выбранной строке ДатаГрида
        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            CurrentRowIndexTwo = e.RowIndex;

            // Если тыкнули на шапку
            if (CurrentRowIndexTwo == -1)
            {
                dataGridView2.ClearSelection();
                button2.Enabled = false;
                return;
            }

            button2.Enabled = true;
        }

        //Очищаем селекты
        private void AddZapicYslyga_Load(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            dataGridView2.ClearSelection();
        }

        // Проверяет, есть ли во 2 гриде текущая запись, true - НЕТУ
        bool CheckRow()
        {
            foreach (DataGridViewRow targetRow in dataGridView2.Rows)
            {
                if (targetRow.IsNewRow) continue;

                if (targetRow.Cells[0].Value.ToString() == dataGridView1.Rows[CurrentRowIndexOne].Cells[0].Value.ToString())
                    return false;
            }
            return true;
        }

        // Кнопка >, добавление
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (CheckRow())
                {
                    // Добавляем во 2 грид
                    string ID = dataGridView1.Rows[CurrentRowIndexOne].Cells[0].Value.ToString();
                    string Name = dataGridView1.Rows[CurrentRowIndexOne].Cells[1].Value.ToString();
                    string Cost = dataGridView1.Rows[CurrentRowIndexOne].Cells[2].Value.ToString();
                    string Duration = dataGridView1.Rows[CurrentRowIndexOne].Cells[3].Value.ToString();

                    Service service = new Service(ID, Name, Cost, Duration);

                    dataGridView2.Rows.Add($"{service.IdService}", $"{service.Name}", $"{service.Cost}", $"{service.Duration}");

                    //Сохраняем
                    AddZapicWiz.CurrentFourForm.Add(service);

                    //CurrentRow остается, поэтому блокируем и очищаем селекты
                    button1.Enabled = false;
                    button2.Enabled = false;
                    dataGridView1.ClearSelection();
                    dataGridView2.ClearSelection();


                    button4.Enabled = true;

                    //ЛОГИКА
                    AddZapicWiz.CurrentFiveForm = new System.Collections.ArrayList();   // ЭТО FIVE 5
                    button5.Enabled = false;
                    button5.BackColor = Color.FromArgb(150, 116, 102);
                    button5.ForeColor = Color.White;
                    button4.BackColor = Color.FromArgb(150, 116, 102);
                    button4.ForeColor = Color.White;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Кнопка <, удаление
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                //Удаляем из структуры и из грида
                var ID = dataGridView2.Rows[CurrentRowIndexTwo].Cells[0].Value.ToString();
                dataGridView2.Rows.Remove(dataGridView2.Rows[CurrentRowIndexTwo]);

                foreach (Service service in AddZapicWiz.CurrentFourForm)
                {
                    if (service.IdService == ID)
                    {
                        AddZapicWiz.CurrentFourForm.Remove(service);
                        break;
                    }
                }

                //CurrentRow остается, поэтому блокируем и очищаем селекты
                button1.Enabled = false;
                button2.Enabled = false;
                dataGridView1.ClearSelection();
                dataGridView2.ClearSelection();

                //ЛОГИКА
                AddZapicWiz.CurrentFiveForm = new System.Collections.ArrayList();
                button5.Enabled = false;
                button5.BackColor = Color.FromArgb(150, 116, 102);
                button5.ForeColor = Color.White;
                button4.BackColor = Color.FromArgb(150, 116, 102);
                button4.ForeColor = Color.White;

                if (dataGridView2.Rows.Count < 1)
                {
                    AddZapicWiz.CurrentFiveForm = new System.Collections.ArrayList();

                    button4.Enabled = false;    //тут разница 1 кнопка
                    button5.Enabled = false;

                    button4.BackColor = Color.FromArgb(150, 116, 102);
                    button4.ForeColor = Color.White;

                    button5.BackColor = Color.FromArgb(150, 116, 102);
                    button5.ForeColor = Color.White;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        //Подсветка строки на которую направлен указатель мыши
        private void dataGridView1_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex > -1)
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGray;
        }
        //Возвращаем состояние строки на исходную, когда указатель "Покидает" строку
        private void dataGridView1_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
        }
    }
}
