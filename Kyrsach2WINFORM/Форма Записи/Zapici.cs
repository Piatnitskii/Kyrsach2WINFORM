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
    public partial class Zapici : Form
    {
        public Zapici()
        {
            InitializeComponent();

            // Включаем двойную буферизацию для DataGridView
            Optimize.SetDoubleBuffered(dataGridView2);
            dataGridView2.CellBorderStyle = DataGridViewCellBorderStyle.None;

            //Настраиваем ДатаГрид
            dataGridView2.Columns.Add("Number", "Номер");
            dataGridView2.Columns.Add("IDClient", "ID");
            dataGridView2.Columns.Add("IDMaser", "ID2");
            dataGridView2.Columns["IDClient"].Visible = false;
            dataGridView2.Columns["IDMaser"].Visible = false;

            dataGridView2.Columns.Add("FIO", "ФИО клиента");
            dataGridView2.Columns.Add("Phone", "Телефон");
            dataGridView2.Columns["Phone"].Visible = false;
            dataGridView2.Columns.Add("FIO2", "ФИО мастера");
            dataGridView2.Columns["FIO2"].Visible = false;
            dataGridView2.Columns.Add("Status", "Статус");
            dataGridView2.Columns.Add("Date_Record", "Дата записи");
            dataGridView2.Columns.Add("Time_Record", "Время записи");
            dataGridView2.Columns.Add("Totla_Price", "Сумма записи");
            dataGridView2.Columns.Add("Total_Time", "Продолжительность, мин.");
            dataGridView2.Columns.Add("PhoneMaster", "PhoneMaster.");
            dataGridView2.Columns["PhoneMaster"].Visible = false;
            dataGridView2.Columns["Total_Time"].DefaultCellStyle.Padding = new Padding(0, 5, 0, 5);

            foreach (DataGridViewColumn column in dataGridView2.Columns)
                column.MinimumWidth = 100;

            //Сортировка по дате
            comboBox1.Items.Add("По умолчанию");
            comboBox1.Items.Add("Возр. даты");
            comboBox1.Items.Add("Убыв. даты");
            comboBox1.SelectedItem = "По умолчанию";

            // Скрываем некоторый функционал от админа
            if (ConnectAndData.Role == "1")
            {
                button1.Visible = false;
                button6.Visible = false;
                button2.Visible = false;
                dataGridView2.Size = new Size(1201, 477);
            }
        }

        int CurrentRowIndex; // Индекс выбранной строки
        // Получаем инфу по выбранной строке ДатаГрида
        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            CurrentRowIndex = e.RowIndex;

            if (CurrentRowIndex == -1)
            {
                button1.Enabled = false;
                button6.Enabled = false;
                dataGridView2.ClearSelection();
                return;
            }       // Если строка в статусе "Завершен" запрещяем редактирование и удаление
            else if (dataGridView2.Rows[CurrentRowIndex].Cells["Status"].Value.ToString() == "Завершен")
            {
                button1.Enabled = false;
                button6.Enabled = false;
                return;
            }

            button1.Enabled = true;
            button6.Enabled = true;
        }

        // Загрузка формы - очищяем выделение
        private void Zapici_Load(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection();
        }

        //Сортировка - очищяем выделение
        private void dataGridView2_Sorted(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection();
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


                    //dataGridView2.Columns.Clear();

                    dataGridView2.Rows.Clear();
                    //Заполняем данными
                    while (RDR.Read())
                    {
                        // Форматируем данные связанный со временем
                        string Date = (DateTime.Parse(RDR[7].ToString())).ToString("dd.MM.yyyy");
                        string Time = (DateTime.Parse(RDR[8].ToString())).ToString("HH:mm");

                        dataGridView2.Rows.Add(RDR[0].ToString(), RDR[1].ToString(), RDR[2].ToString(), RDR[3].ToString(), RDR[4].ToString(), RDR[5].ToString(), RDR[6].ToString(), Date, Time, RDR[9].ToString(), RDR[10].ToString(), RDR[11].ToString());
                    }

                    dataGridView2.ClearSelection(); //Очистка выделения
                    button1.Enabled = false;
                    button6.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Редактировать
        private void button1_Click(object sender, EventArgs e)
        {
            //Формируем нашу запись в объект
            string ID = dataGridView2.Rows[CurrentRowIndex].Cells["Number"].Value.ToString();
            string IDClient = dataGridView2.Rows[CurrentRowIndex].Cells["IDClient"].Value.ToString();
            string IdMaster = dataGridView2.Rows[CurrentRowIndex].Cells["IDMaser"].Value.ToString();

            string FIOClient = dataGridView2.Rows[CurrentRowIndex].Cells["FIO"].Value.ToString();
            string FIOMaster = dataGridView2.Rows[CurrentRowIndex].Cells["FIO2"].Value.ToString();
            string Phone = dataGridView2.Rows[CurrentRowIndex].Cells["Phone"].Value.ToString();
            string Status = dataGridView2.Rows[CurrentRowIndex].Cells["Status"].Value.ToString();
            string Time = dataGridView2.Rows[CurrentRowIndex].Cells["Time_Record"].Value.ToString();
            string Dration = dataGridView2.Rows[CurrentRowIndex].Cells["Total_Time"].Value.ToString(); 
            string Price = dataGridView2.Rows[CurrentRowIndex].Cells["Totla_Price"].Value.ToString();
            string Date = dataGridView2.Rows[CurrentRowIndex].Cells["Date_Record"].Value.ToString();

            string PhoneMaster = dataGridView2.Rows[CurrentRowIndex].Cells["PhoneMaster"].Value.ToString();

            zapici Zapici = new zapici(ID, IDClient, IdMaster, FIOClient, FIOMaster, Phone, Status, Price, Time, Date, Dration, PhoneMaster);

            RedactZapici FormA = new RedactZapici(Zapici);
            FormA.ShowDialog();
            textBox2_TextChanged(textBox2, EventArgs.Empty);
        }

        //Удаление
        private void deleteOrder_Click(object sender, EventArgs e)
        {
            string ID = dataGridView2.Rows[CurrentRowIndex].Cells["Number"].Value.ToString();

            //2 запроса - Одна транзакция
            MySqlTransaction transaction = null;
            string CMDService_In_Record = $"DELETE FROM Service_In_Record WHERE Id_Record = {ID};";
            string CMDRecord = $"DELETE FROM Record WHERE IdRecord = {ID};";

            DialogResult dialogResult = MessageBox.Show($"Удалить запись № {ID}?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    //Удаляем строку
                    using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                    {
                        Con.Open();

                        // Начало транзакции
                        transaction = Con.BeginTransaction();

                        // 1. Удаляем запись в таблице Service_In_Record
                        MySqlCommand cmd = new MySqlCommand(CMDService_In_Record, Con);
                        cmd.Transaction = transaction;
                        cmd.ExecuteNonQuery();

                        // 2. Удаляем данные из таблицы Record
                        cmd.CommandText = CMDRecord;
                        cmd.ExecuteNonQuery();

                        //Подтверждаем транзакцию
                        transaction.Commit();
                    }

                    MessageBox.Show("Запись была успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox2_TextChanged(textBox2, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    // Откат транзакции при ошибке
                    transaction?.Rollback();
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
                return;
        }

        //Добавить
        private void button2_Click(object sender, EventArgs e)
        {
            AddZapicWiz FormA = new AddZapicWiz();
            FormA.ShowDialog();
            textBox2_TextChanged(textBox2, EventArgs.Empty);
        }

        // Строка запроса default
        string CMD = @"SELECT IdRecord, Client.IdClient, Employe.IdEmploye,
                CONCAT_WS(' ', Client.Name, Client.Surname, Client.Patronymic) AS 'ФИОк', 
                Client.Phone,
                CONCAT_WS(' ', Employe.Name, Employe.Surname, Employe.Patronymic) AS 'ФИОм',  
                Status.Name, Date_Record, Time_Record, Totla_Price, Total_Time, Employe.Phone FROM Record 

                    INNER JOIN `Client` ON IdClient = Id_Client 
                    INNER JOIN `Employe` ON IdEmploye = Id_Employe  
                    INNER JOIN `Status` ON IdStatus = Id_Status";


        void SendRequst()
        {
            int I = comboBox1.SelectedIndex;

            switch (I)
            {
                case 0:
                    FillDataGrid(CMD + $" WHERE IdRecord LIKE '%{textBox2.Text}%';");
                    break;
                case 1:
                    FillDataGrid(CMD + $" WHERE IdRecord LIKE '%{textBox2.Text}%' ORDER BY Date_Record {SortMod};");
                    break;
                case 2:
                    FillDataGrid(CMD + $" WHERE IdRecord LIKE '%{textBox2.Text}%' ORDER BY Date_Record {SortMod};");
                    break;
            }
        }

        //СОРТИРОВКА
        //СОРТИРОВКА
        //СОРТИРОВКА  (по дате)
        string SortMod;
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 1:
                    SortMod = "ASC";         // по возрастанию
                    break;

                case 2:
                    SortMod = "DESC";        // по убыванию
                    break;
            }

            SendRequst();
        }

        // "Живой" поиск (по номеру)
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            SendRequst();
        }


        //Прячем некоторые данные
        private void dataGridView2_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView2.Columns[e.ColumnIndex].Name == "FIO")
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
        }

        //Поиск - Только цифры
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
            else { e.Handled = false; }
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
