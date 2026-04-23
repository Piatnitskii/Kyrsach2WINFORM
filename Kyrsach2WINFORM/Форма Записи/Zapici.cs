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
using VPaged.WF;

namespace Kyrsach2WINFORM
{
    public partial class Zapici : Form
    {
        private VPagination _pag;

        public Zapici()
        {
            InitializeComponent();

            _pag = new VPagination(this.groupBox1, pageSize: 10);
            _pag.SelectDataMaster = FillDataGrid;
            _pag.SelectCountMaster = GetCount;


            // Включаем двойную буферизацию для DataGridView
            Optimize.SetDoubleBuffered(dataGridView2);
            dataGridView2.CellBorderStyle = DataGridViewCellBorderStyle.None;

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
            else if (dataGridView2.Rows[CurrentRowIndex].Cells["Статус"].Value.ToString() == "Завершен")
            {
                button1.Enabled = false;
                button6.Enabled = false;
                return;
            }

            button1.Enabled = true;
            button6.Enabled = true;
        }

        //Для подсчета количества записей
        long countRow = 0;
        private long GetCount()
        {
            try
            {
                // Общий запрос без LIMIT
                string countSql = $"SELECT COUNT(*) FROM ({CMD} {having}) AS t";

                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    Con.Open();
                    using (MySqlCommand cmd = new MySqlCommand(countSql, Con))
                    {
                        object result = cmd.ExecuteScalar();
                        return countRow = Convert.ToInt64(result ?? 0);
                    }
                }
            }
            catch
            {
                return 0;
            }
        }

        // Заполняет данными таблицу
        void FillDataGrid()
        {
            try
            {
                dataGridView2.DataSource = null;
                dataGridView2.Columns.Clear();

                // Формируем пагинированный SQL
                int skip = (_pag.PageIndex - 1) * _pag.PageSize;
                string fullSql = CMD + having + orderBy +
                    $" LIMIT {skip}, {_pag.PageSize}";

                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    Con.Open();

                    MySqlCommand cmd = new MySqlCommand(fullSql, Con);
                    MySqlDataAdapter ad = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();

                    
                    ad.Fill(dt);
                   
                    dataGridView2.DataSource = dt;
                    label4.Text = $"Кол-во записей: {countRow}";

                    dataGridView2.Columns["Продолжительность, мин."].DefaultCellStyle.Padding = new Padding(0, 5, 0, 5);

                    dataGridView2.Columns["Телефон"].Visible = false;
                    dataGridView2.Columns["IDClient"].Visible = false;
                    dataGridView2.Columns["IDMaser"].Visible = false;
                    dataGridView2.Columns["ФИО мастера"].Visible = false;
                    dataGridView2.Columns["PhoneMaster"].Visible = false;

                    foreach (DataGridViewColumn column in dataGridView2.Columns)
                        column.MinimumWidth = 100;


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

            RedactZapici FormA = new RedactZapici(Zapici);
            FormA.ShowDialog();
            textBox2_TextChanged(textBox2, EventArgs.Empty);
        }

        //Удаление
        private void deleteOrder_Click(object sender, EventArgs e)
        {
            string ID = dataGridView2.Rows[CurrentRowIndex].Cells["Номер"].Value.ToString();

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
        string CMD = @"SELECT IdRecord as 'Номер', Client.IdClient as 'IDClient', Employe.IdEmploye 'IDMaser',
                CONCAT_WS(' ', Client.Name, Client.Surname, Client.Patronymic) AS 'ФИО клиента', 
                Client.Phone as 'Телефон',
                CONCAT_WS(' ', Employe.Name, Employe.Surname, Employe.Patronymic) AS 'ФИО мастера',  
                Status.Name as 'Статус', 

                DATE_FORMAT(Date_Record, '%d.%m.%Y') AS 'Дата записи',  -- Формат dd.MM.yyyy
                DATE_FORMAT(Time_Record, '%H:%i') AS 'Время записи',    -- Формат HH:MM
                
                Totla_Price as 'Сумма записи', Total_Time as 'Продолжительность, мин.', Employe.Phone as 'PhoneMaster' FROM Record 

                    INNER JOIN `Client` ON IdClient = Id_Client 
                    INNER JOIN `Employe` ON IdEmploye = Id_Employe  
                    INNER JOIN `Status` ON IdStatus = Id_Status";

        string having = "";
        string orderBy = "";



        //СОРТИРОВКА  (по дате)
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
                orderBy = $" ORDER BY IdRecord ASC";
            else if(comboBox1.SelectedIndex == 1)
                orderBy = $" ORDER BY Date_Record";
            else
                orderBy = $" ORDER BY Date_Record DESC";
            FillDataGrid();
            _pag.VPagRunOrRefresh();
        }

        // "Живой" поиск (по номеру)
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text.Length == 0)
                having = "";
            else
                having = $" HAVING IdRecord LIKE '%{textBox2.Text}%'";
            FillDataGrid();
            _pag.VPagRunOrRefresh();
        }


        //Прячем некоторые данные
        private void dataGridView2_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (ShowText)
            {// делаем так, чтобы раскрывалась только та строка, на которую указали мышкой
                if (dataGridView2.Columns[e.ColumnIndex].Name == "ФИО клиента" && e.RowIndex != ThisRow)
                {
                    Optimize.HideMyFio(e);
                }
            }
            else
            {// прячем все
                if (dataGridView2.Columns[e.ColumnIndex].Name == "ФИО клиента")
                {
                    Optimize.HideMyFio(e);
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



        //ВЫДЕЛЕНИЕ СТРОКИ

        bool ShowText = false;
        int ThisRow;

        //Подсветка строки на которую направлен указатель мыши
        private void dataGridView2_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                dataGridView2.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGray;
                ShowText = true;
                ThisRow = e.RowIndex;
                dataGridView2.Rows[e.RowIndex].Cells["ФИО клиента"].Value = dataGridView2.Rows[e.RowIndex].Cells["ФИО клиента"].Value;
            }
               
        }
        //Возвращаем состояние строки на исходную, когда указатель "Покидает" строку
        private void dataGridView2_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                dataGridView2.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                ShowText = false;
                dataGridView2.Rows[e.RowIndex].Cells["ФИО клиента"].Value = dataGridView2.Rows[e.RowIndex].Cells["ФИО клиента"].Value;
            }
                
        }

        //Установил Nuget пакет для пагинации, VPaged.WF + System.Windows.Forms


        // Загрузка формы - очищяем выделение
        private void Zapici_Load(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection();
            _pag.VPagRunOrRefresh();
        }

        //Сортировка - очищяем выделение
        private void dataGridView2_Sorted(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection();
        }

        //Закрыть
        private void button8_Click(object sender, EventArgs e)
        {
            MenuAdmin.DisableButton();
            this.Close();
        }
    }
}
