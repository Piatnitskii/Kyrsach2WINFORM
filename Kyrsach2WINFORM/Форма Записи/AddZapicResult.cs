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
    public partial class AddZapicResult : Form
    {
        //Fields
        Client client;      // Выбранный клиент
        Emploey emploey;    // Выбранный мастер

        int Time = 0;       // Длительность
        double Price = 0;   // Стоимость


        public AddZapicResult(Client client, Emploey emploey)
        {
            InitializeComponent();

            // Включаем двойную буферизацию для DataGridView
            Optimize.SetDoubleBuffered(dataGridView1);

            this.client = client;
            this.emploey = emploey;

            //Настраиваем Дата грид
            dataGridView1.Columns.Add("ID", "ID");
            dataGridView1.Columns["ID"].Visible = false;
            dataGridView1.Columns.Add("Название", "Название");
            dataGridView1.Columns.Add("Цена", "Цена");
            dataGridView1.Columns.Add("Продолжительность", "Продолжительность");

            dataGridView1.Columns["Название"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns["Цена"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns["Продолжительность"].SortMode = DataGridViewColumnSortMode.NotSortable;

            dataGridView1.Columns["Название"].DefaultCellStyle.Padding = new Padding(0, 10, 0, 10);
            dataGridView1.Columns["Цена"].DefaultCellStyle.Padding = new Padding(0, 10, 0, 10);
            dataGridView1.Columns["Продолжительность"].DefaultCellStyle.Padding = new Padding(0, 10, 0, 10);

            foreach (DataGridViewColumn column in dataGridView1.Columns)
                column.MinimumWidth = 50;

            //Заполняем данными ДатаГрид
            FillDataGrid1();  
            //Высчитываем время
            CalcTime(Time);

            //Отображаем данныые
            label2.Text = client.Phone;
            label9.Text = client.FIO;
            label4.Text = emploey.Phone;
            label10.Text = emploey.FIO;
            label7.Text = AddZapicWiz.CurrentFiveForm[0].ToString() + " " + AddZapicWiz.CurrentFiveForm[1].ToString();

            //Отображаем скидку

            double TotalPrice = Math.Floor(Price);

            if (Math.Floor(Price) >= 2000)
            {
                label6.Text = TotalPrice.ToString("F2") + " Р.";
                label6.Font = new Font(label6.Font, FontStyle.Strikeout);

                double A = Math.Floor((TotalPrice * 0.15));
                double MinusDiscount = Convert.ToDouble(TotalPrice) - A;

                label11.Text = $"-> {MinusDiscount.ToString("F2")} Р.";
                label11.Visible = true;
            }
            else
                label6.Text = TotalPrice.ToString("F2") + " Р.";
        }
        

        // Вычисляет общее время, занимаемое услугами
        void CalcTime(int Time)
        {
            int Hours = Time / 60;
            int Minutes = Time % 60;

            if (Minutes == 0)
                label8.Text = $"{Hours} ч.";
            else if(Hours == 0)
                label8.Text = $"{Minutes} м.";
            else
                label8.Text = $"{Hours} ч. {Minutes} м.";
        }

        //Заполняем данными ДатаГрид
        void FillDataGrid1()
        {
            try
            {   // Все данные берем из собранного ранее Списка, на этой стадии он должен быть заполнен хотя бы 1 записью
                foreach (Service service in AddZapicWiz.CurrentFourForm)
                {
                    //Все складываем для итога
                    Price += Convert.ToDouble(service.Cost);
                    Time += Convert.ToInt32(service.Duration);
                    dataGridView1.Rows.Add($"{service.IdService}", $"{service.Name}", $"{service.Cost}", $"{service.Duration}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Добавить запись
        private void createOrder_Click(object sender, EventArgs e)
        {
            
            string Id_Client = client.IdClient;
            string Id_Employe = emploey.IdEmploey;
            string Id_Status = "1";
            string Date_Record = AddZapicWiz.CurrentFiveForm[0].ToString();
            string Time_Record = AddZapicWiz.CurrentFiveForm[1].ToString() + ":00";
            string Totla_Price = (Math.Floor(Price)).ToString();
            string Total_Time = Time.ToString();
            string Id_Service;

            //2 запроса -  Одна транзакция
            MySqlTransaction transaction = null;
            string CMD = $"INSERT INTO Record (Id_Client, Id_Employe, Id_Status, Date_Record, Time_Record, Totla_Price, Total_Time) VALUES ('{Id_Client}', '{Id_Employe}', '{Id_Status}',  '{Date_Record}', '{Time_Record}', '{Totla_Price}', '{Total_Time}');";
            string CMDInServiceInRecord = $"INSERT INTO Service_In_Record (Id_Record, Id_Service) VALUES ";
            
           
            DialogResult dialogResult = MessageBox.Show("Добавить запись?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    try
                    {
                        Con.Open();
                        // Начало транзакции
                        transaction = Con.BeginTransaction();

                        MySqlCommand cmd = new MySqlCommand(CMD + "SELECT last_insert_id();", Con);
                        cmd.Transaction = transaction;

                        // 1. Добавляем данные в таблицу "Record" и получаем ID
                        string OrderID = cmd.ExecuteScalar().ToString();

                        // За счет полученного айдишника формируем  2 запрос
                        foreach (DataGridViewRow item in dataGridView1.Rows)
                        {
                            Id_Service = item.Cells[0].Value.ToString();
                            CMDInServiceInRecord += $"('{OrderID}', '{Id_Service}'),";
                        }
                        cmd.CommandText = CMDInServiceInRecord.Substring(0, CMDInServiceInRecord.Length - 1);

                        // 2. Добавляем данные в таблицу "Service_In_Record"
                        cmd.ExecuteNonQuery();

                        //Подтверждаем транзакцию
                        transaction.Commit();
                        MessageBox.Show("Запись была успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        //Закрываем главную форму и нашу 
                        this.Close();
                        FormManager.CloseForm("AddZapicWiz");
                    }
                    catch (Exception ex)
                    {
                        // Откат транзакции при ошибке
                        transaction?.Rollback();
                        MessageBox.Show(ex.Message, "Ошибка транзакции добавления записи", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
                return;
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
