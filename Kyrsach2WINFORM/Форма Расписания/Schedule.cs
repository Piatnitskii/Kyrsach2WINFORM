using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MySql.Data.MySqlClient;

namespace Kyrsach2WINFORM
{
    public partial class Schedule : Form
    {
        public Schedule()
        {
            InitializeComponent();

            // Включаем двойную буферизацию для DataGridView
            Optimize.SetDoubleBuffered(dataGridView2);
            dataGridView2.CellBorderStyle = DataGridViewCellBorderStyle.None;

            FillDataGrid();
        }

        // Строка запроса default
        string CMD = $@"SELECT IdRecord as 'Номер', Client.IdClient as 'IDClient', Employe.IdEmploye 'IDMaser',
                CONCAT_WS(' ', Client.Name, Client.Surname, Client.Patronymic) AS 'ФИО клиента', 
                Client.Phone as 'Телефон',
                CONCAT_WS(' ', Employe.Name, Employe.Surname, Employe.Patronymic) AS 'ФИО мастера',  
                Status.Name as 'Статус', 

                DATE_FORMAT(Date_Record, '%d.%m.%Y') AS 'Дата записи',  -- Формат dd.MM.yyyy
                DATE_FORMAT(Time_Record, '%H:%i') AS 'Время записи',    -- Формат HH:MM
                
                Totla_Price as 'Сумма записи', Total_Time as 'Продолжительность, мин.', Employe.Phone as 'PhoneMaster' FROM Record 

                    INNER JOIN `Client` ON IdClient = Id_Client 
                    INNER JOIN `Employe` ON IdEmploye = Id_Employe  
                    INNER JOIN `Status` ON IdStatus = Id_Status

                WHERE Employe.IdEmploye = {ConnectAndData.Id_Employe} AND Status.Name = 'Ожидается'";

        // Заполняет данными таблицу
        void FillDataGrid()
        {
            try
            {
                dataGridView2.DataSource = null;
                dataGridView2.Columns.Clear();

                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    Con.Open();

                    MySqlCommand cmd = new MySqlCommand(CMD, Con);
                    MySqlDataAdapter ad = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();


                    ad.Fill(dt);

                    dataGridView2.DataSource = dt;
 

                    dataGridView2.Columns["Продолжительность, мин."].DefaultCellStyle.Padding = new Padding(0, 5, 0, 5);

                    dataGridView2.Columns["Телефон"].Visible = false;
                    dataGridView2.Columns["IDClient"].Visible = false;
                    dataGridView2.Columns["Номер"].Visible = false;
                    dataGridView2.Columns["Статус"].Visible = false;
                    dataGridView2.Columns["IDMaser"].Visible = false;
                    dataGridView2.Columns["ФИО мастера"].Visible = false;
                    dataGridView2.Columns["PhoneMaster"].Visible = false;
                    dataGridView2.Columns["Продолжительность, мин."].SortMode = DataGridViewColumnSortMode.NotSortable;

                    foreach (DataGridViewColumn column in dataGridView2.Columns)
                        column.MinimumWidth = 100;


                    dataGridView2.ClearSelection(); //Очистка выделения

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //закрыть
        private void button3_Click(object sender, EventArgs e)
        {
            MenuAdmin.DisableButton();
            this.Close();
        }

        int CurrentRowIndex; // Индекс выбранной строки
        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            CurrentRowIndex = e.RowIndex;

            if (CurrentRowIndex == -1)
            {
                dataGridView2.ClearSelection();
                return;
            }

            //Формируем нашу запись в объект
            string ID = dataGridView2.Rows[CurrentRowIndex].Cells["Номер"].Value.ToString();
            string IDClient = dataGridView2.Rows[CurrentRowIndex].Cells["IDClient"].Value.ToString();
            string IdMaster = dataGridView2.Rows[CurrentRowIndex].Cells["IDMaser"].Value.ToString();

            string FIOClient = dataGridView2.Rows[CurrentRowIndex].Cells["ФИО клиента"].Value.ToString();
            string FIOMaster = dataGridView2.Rows[CurrentRowIndex].Cells["ФИО мастера"].Value.ToString();
            string Phone = dataGridView2.Rows[CurrentRowIndex].Cells["Телефон"].Value.ToString();
            string Status = dataGridView2.Rows[CurrentRowIndex].Cells["Статус"].Value.ToString();
            string Time = dataGridView2.Rows[CurrentRowIndex].Cells["Время записи"].Value.ToString();
            string Dration = dataGridView2.Rows[CurrentRowIndex].Cells["Продолжительность, мин."].Value.ToString();
            string Price = dataGridView2.Rows[CurrentRowIndex].Cells["Сумма записи"].Value.ToString();
            string Date = dataGridView2.Rows[CurrentRowIndex].Cells["Дата записи"].Value.ToString();

            string PhoneMaster = dataGridView2.Rows[CurrentRowIndex].Cells["PhoneMaster"].Value.ToString();

            zapici Zapici = new zapici(ID, IDClient, IdMaster, FIOClient, FIOMaster, Phone, Status, Price, Time, Date, Dration, PhoneMaster);

            RedactZapici FormA = new RedactZapici(Zapici, false);
            Optimize.daughterForm = FormA;
            FormA.ShowDialog();
            Optimize.daughterForm = null;
            FillDataGrid();
        }

        private void dataGridView2_Sorted(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection();
        }

        private void Schedule_Load(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FillDataGrid();
        }
    }
}
