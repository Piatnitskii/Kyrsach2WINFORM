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

                WHERE Id_Emploey = {ConnectAndData.ID}";

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
    }
}
