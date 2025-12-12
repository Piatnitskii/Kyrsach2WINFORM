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
using Kyrsach2WINFORM.Классы_Объекты;

namespace Kyrsach2WINFORM
{
    public partial class Oplata : Form
    {
        public Oplata()
        {
            InitializeComponent();

            // Включаем двойную буферизацию для DataGridView
            Optimize.SetDoubleBuffered(dataGridView2);
            dataGridView2.CellBorderStyle = DataGridViewCellBorderStyle.None;

            //Если администратор, то убираем некоторый функционал
            if (ConnectAndData.Role == "1")
            {
                button3.Visible = false;
                button4.Location = new Point(960, 536);
            }

            // Заполняем Дата грид
            FillDataGrid();
        }

        // CONCAT_WS(' ', User.Name, Surname, Patronymic) AS 'ФИО'
        string CMD = @"SELECT IdPayment as ID, IdRecord as 'Номер записи', CONCAT_WS(' ', Client.Name, Client.Surname, Client.Patronymic ) AS 'ФИО клиента', Date_Record as 'Дата записи', Time_Record as 'Время записи',  Payment_Time 'Время оплаты', Amount as 'Сумма руб.', Discount as 'Скидка руб.' FROM Record 
                        INNER JOIN `Client` ON IdClient = Id_Client
                        INNER JOIN `Employe` ON IdEmploye = Id_Employe  
                        INNER JOIN `Status` ON IdStatus = Id_Status 
                        INNER JOIN Payment ON IdRecord = Id_Record;";

        private void Oplata_Load(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection(); //Очистка выделения
        }

        private void dataGridView2_Sorted(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection(); //Очистка выделения
        }

        //Заполняет ДатаГрид данными
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

                    dataGridView2.DataSource = Dt;

                    //Настройка полей
                    dataGridView2.Columns["ID"].Visible = false;

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

        //Скрываем некоторые данные
        private void dataGridView2_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView2.Columns[e.ColumnIndex].Name == "ФИО клиента")
            {
                if (e.Value != null)
                {
                    var Val = e.Value.ToString().Split(' ');
                    string Result;
                    if (Val.Length == 3 && Val[2].Trim() != "")
                        Result = Val[0] + " " + (Val[1])[0] + "." + " " + (Val[2])[0] + ".";
                    else
                        Result = Val[0] + " " + (Val[1])[0] + ".";

                    e.Value = Result;
                }
            }

            if (dataGridView2.Columns[e.ColumnIndex].Name == "Время записи")
            {
                string Time = (DateTime.Parse(e.Value.ToString())).ToString("HH:mm");
                e.Value = Time;
            }
        }

        int CurrentRowIndex; // Индекс выбранной строки
        // Получаем инфу по выбранной строке ДатаГрида
        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            CurrentRowIndex = e.RowIndex;

            if (CurrentRowIndex == -1)
            {
                button3.Enabled = false;
                dataGridView2.ClearSelection();
                return;
            }       
            
            button3.Enabled = true;
        }

        //// Редактировать отчет
        //private void button1_Click(object sender, EventArgs e)
        //{
        //    RedactPlat FormA = new RedactPlat();
        //    FormA.ShowDialog();
        //}

        // Сформировать отчет
        private void button4_Click(object sender, EventArgs e)
        {
            Otchet FormA = new Otchet();
            FormA.ShowDialog();
        }


        // Сформировать чек
        private void createCheck_Click(object sender, EventArgs e)
        {
            //Получаем ID записи в оплате
            string RecordID = dataGridView2.Rows[CurrentRowIndex].Cells["Номер записи"].Value.ToString();
            CreateWord.CheckCreator(RecordID);
            dataGridView2.ClearSelection(); //Очистка выделения
            button3.Enabled = false;
        }

        //Закрыть
        private void button8_Click(object sender, EventArgs e)
        {
            MenuAdmin.DisableButton();
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
