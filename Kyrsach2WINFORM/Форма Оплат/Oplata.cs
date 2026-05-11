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

            //Настройка полей
            dataGridView2.Columns.Add("ID", "ID");
            dataGridView2.Columns.Add("Номер записи", "Номер записи");
            dataGridView2.Columns.Add("Клиент-Мастер", "Клиент-Мастер");
            dataGridView2.Columns.Add("Время записи", "Время записи");
            dataGridView2.Columns.Add("Время оплаты", "Время оплаты");
            dataGridView2.Columns.Add("Сумма", "Сумма");
            dataGridView2.Columns.Add("Скидка", "Скидка");

            foreach (DataGridViewColumn column in dataGridView2.Columns)
                column.MinimumWidth = 100;

            dataGridView2.Columns["Номер записи"].MinimumWidth = 20;

            dataGridView2.Columns["ID"].Visible = false;
            dataGridView2.Columns["Номер записи"].Width = 20;
            dataGridView2.Columns["Клиент-Мастер"].Width = 500;
            dataGridView2.Columns["Время записи"].Width = 250;
            dataGridView2.Columns["Время оплаты"].Width = 230;


  

            // Заполняем Дата грид
            FillDataGrid();
        }

        string CMD = @"SELECT IdPayment as ID, IdRecord as 'Номер записи', 
                        CONCAT('К: ',CONCAT_WS(' ', Client.Name, Client.Surname, Client.Patronymic), '\nМ: ', CONCAT_WS(' ', Employe.Name, Employe.Surname, Employe.Patronymic)) as 'Клиент-Мастер', 
                        CONCAT(DATE_FORMAT(Date_Record, '%d.%m.%Y'), '\n', DATE_FORMAT(Time_Record, '%H:%i')) as 'Время',
                        Payment_Time 'Время оплаты', Amount as 'Сумма', Discount as 'Скидка' 
                        
                        FROM Record 
                        
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

                    MySqlDataReader RDR = cmd.ExecuteReader();

                    //Заполняем данными
                    while (RDR.Read())
                    {
                        dataGridView2.Rows.Add(RDR[0].ToString(), RDR[1].ToString(), RDR[2].ToString(), RDR[3].ToString(), RDR[4].ToString(), RDR[5].ToString(), RDR[6].ToString());
                    }


                    dataGridView2.ClearSelection(); //Очистка выделения
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        //ВЫДЕЛЕНИЕ СТРОКИ
        //Подсветка строки на которую направлен указатель мыши
        private void dataGridView2_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                dataGridView2.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGray;
            }
        }
        //Возвращаем состояние строки на исходную, когда указатель "Покидает" строку
        private void dataGridView2_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            
            if (e.RowIndex > -1)
            {
                dataGridView2.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
            }
        }

        
    }
}
