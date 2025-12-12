using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Word;
//
//
using MySql.Data.MySqlClient;
using Word = Microsoft.Office.Interop.Word;

namespace Kyrsach2WINFORM
{
    public partial class Otchet : Form
    {
        public Otchet()
        {
            InitializeComponent();

            dateTimePicker1.MaxDate = DateTime.Now.AddDays(-7);
            dateTimePicker2.MaxDate = DateTime.Now;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //Формируем отчет
        private void createOtchet_Click(object sender, EventArgs e)
        {
            //Пишем логику, делам функцию проверки правильности выбора даты
            DialogResult dialogResult = MessageBox.Show("Сформировать отчет?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    string Data_Start = dateTimePicker1.Value.ToString("yyyy-MM-dd");
                    string Data_End = dateTimePicker2.Value.ToString("yyyy-MM-dd");

                    // 1. Получаем данные для 1 таблицы - Общая выручка (с учетом скидок)
                    string[]  One_Table = GetTotalRevenue(Data_Start, Data_End).Split(';');

                    // 2. Получаем данные для 2 таблицы - Выручка по категориям
                    //var Two_Table = GetCategoriesRevenueData(Data_Start, Data_End);

                    // 3. Получаем данные для 3 таблицы - Выручка по сотрудникам
                    var Free_Table = GetEmployeRevenue(Data_Start, Data_End);

                    OtchetCreator(Data_Start, Data_End, One_Table[0], One_Table[1], One_Table[2], Free_Table);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Внимание");
                }
            }
            else
                return;
        }

        // Получаем общую выручку за период
        private string GetTotalRevenue(string Data_Start, string Data_End)
        {
            string CMDPayment = $@" SELECT 
                                        COUNT(*) as Количество_Записей,
                                        SUM(Amount - Discount) as Общая_Выручка,
                                        ROUND(AVG(Amount - Discount), 2) as Средний_Чек
                                    FROM Payment 
                                    WHERE Payment_Time BETWEEN '{Data_Start}' AND '{Data_End}'";   

            string Result = "";

            using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
            {
                Con.Open();

                MySqlCommand cmd = new MySqlCommand(CMDPayment, Con);
                MySqlDataReader RDR = cmd.ExecuteReader();

                //Заполняем данными
                while (RDR.Read())
                    Result = RDR[0].ToString() + ";" + RDR[1].ToString() + ";" + RDR[2].ToString();   // Количество_Записей;Общая_Выручка;Средний_Чек
            }

            return Result;
        }

        // Получаем выручку по категориям
        public List<CategoryRevenue> GetCategoriesRevenueData(string Data_Start, string Data_End)
        {
            var categories = new List<CategoryRevenue>();

            string query = $@"
                    SELECT 
                    Category.Name as Название_Категории,
                    COUNT(*) as Количество_Оплат,
                    SUM(Cost) as Выручка

                    FROM Payment

                    JOIN Record ON Payment.Id_Record = Record.IdRecord
                    JOIN Service_In_Record  ON Record.IdRecord = Service_In_Record.Id_Record
                    JOIN Service ON Service_In_Record.Id_Service = Service.IdService
                    JOIN Category ON Service.Id_Category = Category.IdCategory

                    WHERE Payment.Payment_Time BETWEEN '{Data_Start}' AND '{Data_End}' AND Record.Id_Status = 2
                    GROUP BY Category.IdCategory, Category.Name;
            ";

            using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
            {
                MySqlCommand cmd = new MySqlCommand(query, Con);
                Con.Open();
                MySqlDataReader RDR = cmd.ExecuteReader();

                // Заполняем список строками
                while (RDR.Read())
                {
                    categories.Add(new CategoryRevenue
                    {
                        CategoryName = RDR["Название_Категории"].ToString(),
                        PaymentsCount = Convert.ToInt32(RDR["Количество_Оплат"]),
                        Revenue = Convert.ToDouble(RDR["Выручка"]),
                    });
                }
            }

            return categories;
        }

        // Получаем выручку по сотрудникам
        public List<EmploeyRevenue> GetEmployeRevenue(string Data_Start, string Data_End)
        {
            var Emploey = new List<EmploeyRevenue>();

            string query = $@"
                SELECT 

	            CONCAT(Employe.Name, ' ', Employe.Surname, ' ', Employe.Patronymic) as ФИО_Работника,
                Employe.Phone as Номер_Телефона,
                COUNT(*) as Количество_Записей,
                SUM( Payment.Amount - Payment.Discount) as Выручка,
                ROUND(AVG(Payment.Amount - Payment.Discount ), 2) as Средний_Доход

                FROM Payment 

                JOIN Record  ON Payment.Id_Record = Record.IdRecord
                JOIN Employe  ON Record.Id_Employe = Employe.IdEmploye

                WHERE Payment.Payment_Time BETWEEN '{Data_Start}' AND '{Data_End} 23:59:59' AND Record.Id_Status = 2

                GROUP BY Employe.IdEmploye, Employe.Name, Employe.Surname
            ";

            using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
            {
                MySqlCommand cmd = new MySqlCommand(query, Con);
                Con.Open();
                MySqlDataReader RDR = cmd.ExecuteReader();

                // Заполняем список строками
                while (RDR.Read())
                {
                    Emploey.Add(new EmploeyRevenue
                    {
                        FIOEmploey = RDR["ФИО_Работника"].ToString(),
                        Phone = RDR["Номер_Телефона"].ToString(),
                        NumberRecords = RDR["Количество_Записей"].ToString(),
                        Revenue = RDR["Выручка"].ToString(),
                        RevenueAVG = RDR["Средний_Доход"].ToString(),
                    });
                }
            }

            return Emploey;
        }

        // Создает отчет
        private void OtchetCreator(string Date_Start, string Date_End, string Total_Payments, string Total_revenue, string Average_Receipt, List<EmploeyRevenue> ListEmploeyRevenue)
        {
            Word.Application Ap = null;
            Word.Document Doc = null;

            try
            {
                string fileName = Directory.GetCurrentDirectory() + @"\Отчет.docx"; // шаблон

                Ap = new Word.Application();
                Ap.Visible = false;

                Doc = Ap.Documents.Open(fileName, ReadOnly: true);

                // 1. Вставляем выбранную дату и заполням 1 таблицу
                ReplaceWord("{Начало_Дата}", Date_Start, Doc);
                ReplaceWord("{Конец_Дата}", Date_End, Doc);
                ReplaceWord("{Общее_количество_платежей}", Total_Payments, Doc);
                ReplaceWord("{Общая_выручка}", Convert.ToDouble(Total_revenue).ToString("F2"), Doc);
                ReplaceWord("{Средний_Чек}", Convert.ToDouble(Average_Receipt).ToString("F2"), Doc);
                Table table = Doc.Tables[1]; // Выбираем 2 таблицу

                while (table.Rows.Count > 2) // оставляем заголовок и шапку
                {
                    table.Rows[1].Delete();
                }

                
                /*// 2. Добавляем строки и заполняем данными 2 таблицу
                foreach (var category in ListCategoryRevenue)
                {
                    Row newRow = table.Rows.Add();

                    // Заполняем ячейки
                    newRow.Cells[1].Range.Text = category.CategoryName;
                    newRow.Cells[2].Range.Text = category.PaymentsCount.ToString();
                    newRow.Cells[3].Range.Text = category.Revenue.ToString("N2");
                }*/

                // 3. Добавляем строки и заполняем данными 2 таблицу

                table = Doc.Tables[2]; // Выбираем 2 таблицу

                while (table.Rows.Count > 2) // оставляем заголовок и шапку
                {
                    table.Rows[2].Delete();
                }

                foreach (var emploey in ListEmploeyRevenue)
                {
                    Row newRow = table.Rows.Add();

                    // Заполняем ячейки
                    newRow.Cells[1].Range.Text = emploey.FIOEmploey;
                    newRow.Cells[2].Range.Text = emploey.Phone;
                    newRow.Cells[3].Range.Text = emploey.NumberRecords;
                    newRow.Cells[4].Range.Text = Convert.ToDouble(emploey.Revenue).ToString("F2");
                    newRow.Cells[5].Range.Text = Convert.ToDouble(emploey.RevenueAVG).ToString("F2");
                }


                Ap.Visible = true;
            }
            catch (Exception ex)
            {
                Doc.Close(SaveChanges: false);
                Ap.Quit(SaveChanges: false);
                MessageBox.Show(ex.Message, "Внимание");
            }
        }

        // ....
        private void ReplaceWord(string src, string dest, Word.Document doc)
        {
            try
            {
                Word.Range rg = doc.Content;

                rg.Find.ClearFormatting();
                rg.Find.Execute(FindText: src, ReplaceWith: dest);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Внимание");
            }

        }

        // Регулируем выбор даты
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            // Если выдержан диапазон в 7 дней
            if (dateTimePicker2.Value.Date - dateTimePicker1.Value.Date >= TimeSpan.FromDays(7))
            {
                button1.Enabled = true;
                dateTimePicker2.Enabled = true;
                dateTimePicker2.MinDate = dateTimePicker1.Value;
            }
            else
            {
                button1.Enabled = false;
                dateTimePicker2.MinDate = dateTimePicker1.Value;
            }
        }
    }

    // Представляет объект - категорию
    public class CategoryRevenue
    {
        public string CategoryName { get; set; }    
        public int PaymentsCount { get; set; }      // Кол-во оплат по этой категории
        public double Revenue { get; set; }         // Выручка по категории
    }

    // Представляет объект - работника
    public class EmploeyRevenue
    {
        public string FIOEmploey { get; set; }    
        public string Phone { get; set; }     
        public string NumberRecords { get; set; }
        public string Revenue { get; set; }         // Выручка по работнику
        public string RevenueAVG { get; set; }         // Средняя Выручка по работнику
    }

}
