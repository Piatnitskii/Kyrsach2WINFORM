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
    public partial class RedactZapici : Form
    {

        //Fields 
        zapici zapici;  // Выбранная запись
        double Discount = 0;
        public RedactZapici(zapici zapici)
        {
            InitializeComponent();

            // Включаем двойную буферизацию для DataGridView
            Optimize.SetDoubleBuffered(dataGridView1);

            this.zapici = zapici;

            //Настраиваем Дата грид
            dataGridView1.Columns.Add("ID", "ID");
            dataGridView1.Columns.Add("Название", "Название");
            dataGridView1.Columns.Add("Цена", "Цена");
            dataGridView1.Columns.Add("Продолжительность", "Время");

            dataGridView1.Columns["Название"].DefaultCellStyle.Padding = new Padding(0, 10, 0, 10);
            dataGridView1.Columns["Цена"].DefaultCellStyle.Padding = new Padding(0, 10, 0, 10);
            dataGridView1.Columns["Продолжительность"].DefaultCellStyle.Padding = new Padding(0, 10, 0, 10);

            dataGridView1.Columns["ID"].Visible = false;    //скрываем

            dataGridView1.Columns["Название"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns["Цена"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns["Продолжительность"].SortMode = DataGridViewColumnSortMode.NotSortable;

            

            foreach (DataGridViewColumn column in dataGridView1.Columns)
                column.MinimumWidth = 50;

            dataGridView1.Columns["Название"].MinimumWidth = 250;

            //Заполняем данными ДатаГрид
            FillDataGrid1();

            //Высчитываем время
            CalcTime(Convert.ToInt32(zapici.Dration));
            label10.Text = zapici.Date + " " + zapici.Time;

            //Считаем стоимость 
            if (Convert.ToDouble(zapici.Price) >= 2000)
            {
                label9.Text = zapici.Price + " Р.";
                label9.Font = new Font(label9.Font, FontStyle.Strikeout);

                Discount = Math.Floor((Convert.ToDouble(zapici.Price) * 0.15));
                double TotalAmount = Math.Floor(Convert.ToDouble(zapici.Price) - Discount);
                label11.Text = $"-> {TotalAmount.ToString("F2")} Р.";
                label11.Visible = true;
            }
            else
                label9.Text = zapici.Price + " Р.";

            label2.Text = zapici.FIOClient;
            label3.Text = zapici.Phone;
            label5.Text = zapici.FIOMaster;
            label7.Text = zapici.PhoneMaster;
        }

        //Заполняем услугами ДатаГрид
        void FillDataGrid1()
        {
            try
            {
                var Str = zapici.ID;
                //CONCAT_WS(' ', Employe.Name, Surname, Patronymic)
                string CMD = $"Select  Service.IdService, Service.Name, Service.Cost, Service.Duration FROM Service_In_Record INNER JOIN Service ON Id_Service = IdService  WHERE Id_Record = {zapici.ID};";

                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    Con.Open();

                    MySqlCommand cmd = new MySqlCommand(CMD, Con);
                    MySqlDataReader RDR = cmd.ExecuteReader();

                    //Заполняем данными
                    while (RDR.Read())
                        dataGridView1.Rows.Add(RDR[0].ToString(), RDR[1].ToString(), RDR[2].ToString(), RDR[3].ToString());
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Вычисляет общее время, занимаемое услугами
        void CalcTime(int Time)
        {
            int Hours = Time / 60;
            int Minutes = Time % 60;

            if (Minutes == 0)
                label8.Text = $"{Hours} ч.";
            else if (Hours == 0)
                label8.Text = $"{Minutes} м.";
            else
                label8.Text = $"{Hours} ч. {Minutes} м.";
        }

        //Изменить - по сути "Оплатить", завершить заказ
        private void endOrder_Click(object sender, EventArgs e)
        {
            //2 запроса - Одна транзакция
            MySqlTransaction transaction = null;
            string TimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string CMDRecord = $"UPDATE Record SET Id_Status = 2 WHERE IdRecord = '{zapici.ID}'";
            string CMDPayment = $"INSERT INTO Payment (Id_Record, Payment_Time, Amount, Discount) VALUES ('{zapici.ID}', '{TimeNow}', '{Convert.ToDouble(zapici.Price)}', '{Discount}')";

            DialogResult dialogResult = MessageBox.Show("Желаете завершить запись и подтвердить оплату? \n Данное действие невозможно отменить", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    try
                    {
                        Con.Open();
                        // Начало транзакции
                        transaction = Con.BeginTransaction();

                        // 1. Обновляем запись в таблице "Record"
                        MySqlCommand cmd = new MySqlCommand(CMDRecord, Con);
                        cmd.Transaction = transaction;
                        cmd.ExecuteNonQuery();

                        // 2. Добавляем данные в таблицу "Payment"
                        cmd.CommandText = CMDPayment;
                        cmd.ExecuteNonQuery();

                        //Подтверждаем транзакцию
                        transaction.Commit();
                        MessageBox.Show("Запись завершена, оплата проведена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        // Откат транзакции при ошибке
                        transaction?.Rollback();
                        MessageBox.Show(ex.Message, "Ошибка транзакции добавления записи", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                DialogResult dialogResult2 = MessageBox.Show("Сформировать чек?", "Операция формирования чека", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult2 == DialogResult.Yes)
                {
                   CreateWord.CheckCreator(zapici.ID);
                }
            }
            else
                return;
        }

        //Закрыть
        private void button8_Click(object sender, EventArgs e)
        {
            this.Close();
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
