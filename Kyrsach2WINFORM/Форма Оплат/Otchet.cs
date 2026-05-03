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

//
//
using MySql.Data.MySqlClient;

using ClosedXML.Excel;
using PdfFont = iTextSharp.text.Font;
using iTextSharp.text;
using iTextSharp.text.pdf;

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

                    if (radioButton1.Checked)
                    {
                        // 1. Получаем данные для 1 таблицы - Общая выручка (с учетом скидок)
                        string[] One_Table = GetTotalRevenue(Data_Start, Data_End).Split(';');

                        // 2. Получаем данные для 2 таблицы - Выручка по сотрудникам
                        var Free_Table = GetEmployeRevenue(Data_Start, Data_End);

                        OtchetCreatorPDF(Data_Start, Data_End, One_Table[0], One_Table[1], One_Table[2], Free_Table);
                    }
                    else
                    {
                        List<ServiceData> servicesData = GetServiceData(Data_Start, Data_End);
                        GenerateReportExcell(servicesData, Data_Start, Data_End);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка создания отчета", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
                return;
        }

        // Получаем общую выручку за период (PDF)
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

        // Получаем выручку по сотрудникам (PDF)
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

        // Создает отчет PDF
        private void OtchetCreatorPDF(
             string dateStart,
             string dateEnd,
             string totalPayments,
             string totalRevenue,
             string averageReceipt,
             List<EmploeyRevenue> listEmploeyRevenue)
        {
            totalRevenue = string.IsNullOrWhiteSpace(totalRevenue) ? "0" : totalRevenue;
            averageReceipt = string.IsNullOrWhiteSpace(averageReceipt) ? "0" : averageReceipt;

            try
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + $@"\Documents");
                string fileNamePdf = Directory.GetCurrentDirectory() + $@"\Documents\Отчет за период {dateStart}-{dateEnd}.pdf";

                BaseFont baseFont = BaseFont.CreateFont(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts", "tahoma.ttf"),
                    BaseFont.IDENTITY_H,
                    BaseFont.EMBEDDED);

                PdfFont fontTitle = new PdfFont(baseFont, 20, PdfFont.BOLD, BaseColor.BLACK);
                PdfFont fontHeader = new PdfFont(baseFont, 14, PdfFont.BOLD, BaseColor.BLACK);
                PdfFont fontTable = new PdfFont(baseFont, 11, PdfFont.NORMAL, BaseColor.BLACK);

                using (var stream = new FileStream(fileNamePdf, FileMode.Create, FileAccess.Write))
                using (var document = new Document(PageSize.A4, 40, 40, 40, 40))
                using (var writer = PdfWriter.GetInstance(document, stream))
                {
                    document.Open();

                    document.Add(new Paragraph("Отчет по выручке барбершопа \"БарБер от БАРБЕРА\"", fontTitle)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingAfter = 10f
                    });

                    document.Add(new Paragraph($"Период с {dateStart} по {dateEnd}", fontHeader)
                    {
                        Alignment = Element.ALIGN_LEFT,
                        SpacingAfter = 15f
                    });

                    document.Add(new Paragraph($"Общая выручка за указанный период:", fontHeader)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingAfter = 15f
                    });
                    PdfPTable table1 = new PdfPTable(3);
                    table1.WidthPercentage = 100;
                    table1.SetWidths(new float[] { 33f, 33f, 34f });
                    table1.SpacingBefore = 5f;
                    table1.SpacingAfter = 15f;
                    table1.SplitLate = false;
                    table1.KeepTogether = true;

                    AddHeaderCell(table1, "Общее количество записей на услуги", fontHeader);
                    AddHeaderCell(table1, "Общая выручка", fontHeader);
                    AddHeaderCell(table1, "Средний чек", fontHeader);

                    AddValueCell(table1, totalPayments, fontTable);
                    AddValueCell(table1, Convert.ToDouble(totalRevenue).ToString("F2"), fontTable);
                    AddValueCell(table1, Convert.ToDouble(averageReceipt).ToString("F2"), fontTable);

                    document.Add(table1);

                    document.Add(new Paragraph("Выручка по сотрудникам", fontHeader)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingAfter = 15f
                    });

                    PdfPTable table2 = new PdfPTable(5);
                    table2.WidthPercentage = 100;
                    table2.SetWidths(new float[] { 32f, 18f, 15f, 17f, 18f });
                    table2.SpacingBefore = 5f;
                    table2.SpacingAfter = 10f;
                    table2.SplitLate = false;
                    table2.SplitRows = true;
                    table2.HeaderRows = 1;

                    AddHeaderCell(table2, "ФИО Работника", fontHeader);
                    AddHeaderCell(table2, "Номер телефона", fontHeader);
                    AddHeaderCell(table2, "Кол-во записей", fontHeader);
                    AddHeaderCell(table2, "Выручка", fontHeader);
                    AddHeaderCell(table2, "Средний доход", fontHeader);

                    foreach (var item in listEmploeyRevenue)
                    {
                        AddValueCell(table2, item.FIOEmploey, fontTable);
                        AddValueCell(table2, item.Phone, fontTable);
                        AddValueCell(table2, item.NumberRecords, fontTable);
                        AddValueCell(table2, Convert.ToDouble(item.Revenue).ToString("F2"), fontTable);
                        AddValueCell(table2, Convert.ToDouble(item.RevenueAVG).ToString("F2"), fontTable);
                    }

                    document.Add(table2);
                    document.Close();
                }

                MessageBox.Show($"Отчет сохранён в PDF: {fileNamePdf}", "Успешно",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании отчёта: {ex.Message}", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddHeaderCell(PdfPTable table, string text, PdfFont font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Padding = 8f;
            cell.BackgroundColor = new BaseColor(230, 230, 230);
            table.AddCell(cell);
        }

        private void AddValueCell(PdfPTable table, string text, PdfFont font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Padding = 8f;
            table.AddCell(cell);
        }

        // ....


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

        //Получаем данные по категориям за период
        private List<ServiceData> GetServiceData(string startDate, string endDate)
        {
            List<ServiceData> servicesData = new List<ServiceData>();

            string query = $@"
            SELECT 
                s.IdService,
                s.Name AS Услуга,
                s.Cost AS Стоимость_руб,
                s.Duration AS Время_мин,
                COUNT(sir.Id_Service) AS Общее_количество_оказаний,
                SUM(CASE 
                        WHEN p.Discount > 0 THEN s.Cost * 0.85
                        ELSE s.Cost 
                    END) AS Итоговая_стоимость_с_учетом_скидки
            FROM Payment p
            INNER JOIN Service_In_Record sir ON sir.Id_Record = p.Id_Record
            INNER JOIN Service s ON s.IdService = sir.Id_Service  
            WHERE p.Payment_Time BETWEEN '{startDate}' AND '{endDate}'
            GROUP BY s.IdService, s.Name, s.Cost, s.Duration
            ORDER BY s.Name;";

            using (MySqlConnection connection = new MySqlConnection(ConnectAndData.Сonnect))
            {
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {

                    connection.Open();

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var serviceData = new ServiceData
                            {
                                ServiceName = reader["Услуга"].ToString(),
                                ServiceCost = Convert.ToDecimal(reader["Стоимость_руб"]),
                                ServiceDuration = Convert.ToInt32(reader["Время_мин"]),
                                ServiceCount = Convert.ToInt32(reader["Общее_количество_оказаний"]),
                                TotalIncome = Convert.ToDecimal(reader["Итоговая_стоимость_с_учетом_скидки"])
                            };

                            servicesData.Add(serviceData);
                        }
                    }
                }
            }

            return servicesData;
        }

        //Печатает отчет в Excell
        public void GenerateReportExcell(List<ServiceData> servicesData, string startDate, string endDate)
        {
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + $@"\Documents");
            var templatePath = Directory.GetCurrentDirectory() + @"\Template.xlsx";  // Путь к шаблону
            var filePath = Directory.GetCurrentDirectory() + $@"\Documents\Отчет по услугам {startDate}-{endDate}.xlsx";

            using (var workbook = new XLWorkbook(templatePath))
            {
                var workSheet = workbook.Worksheet(1);

                // Заголовок отчета
                workSheet.Cell(1, 1).Value = $"ОТЧЕТ ЗА ПЕРИОД с {startDate} по {endDate}";

                // Расчет общих сумм
                var totalRevenue = servicesData.Sum(s => s.TotalIncome);
                var totalRequests = servicesData.Sum(s => s.ServiceCount);

                // Заполнение данными
                int row = 4;
                foreach (var service in servicesData)
                {
                    workSheet.Cell(row, 1).Value = service.ServiceName;
                    workSheet.Cell(row, 2).Value = service.ServiceCost;
                    workSheet.Cell(row, 3).Value = service.ServiceDuration;
                    workSheet.Cell(row, 4).Value = service.ServiceCount;
                    workSheet.Cell(row, 5).Value = totalRequests > 0 ? Math.Round((service.ServiceCount / (double)totalRequests) * 100,2) : 0;
                    workSheet.Cell(row, 6).Value = service.TotalIncome;
                    workSheet.Cell(row, 7).Value = totalRevenue > 0 ? Math.Round((service.TotalIncome / totalRevenue) * 100,2) : 0;
                    row++;
                }

                // Итоговая строка
                workSheet.Cell(row, 1).Value = "ИТОГО";
                workSheet.Cell(row, 6).FormulaA1 = $"SUM(F3:F{row - 1})";   //Сумма всех услуг
                workSheet.Cell(row, 4).FormulaA1 = $"SUM(D3:D{row - 1})";   //Количество оказаных услуг
                workSheet.Range(row, 1, row, 7).Style.Font.Bold = true;



                // Форматирование
                var dataRange = workSheet.Range(4, 2, row - 1, 2);
                dataRange.Style.NumberFormat.Format = "#,##0.00";
                var dataRange2 = workSheet.Range(4, 6, row - 1, 6);
                dataRange2.Style.NumberFormat.Format = "#,##0.00";

                workSheet.Range(3, 1, row, 7).Style
                        .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                        .Border.SetInsideBorder(XLBorderStyleValues.Thin);

                workSheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00 руб.";

                // ДАННЫЕ ДЛЯ ГРАФИКА (заменяем в шаблоне)
                var sortedServices = servicesData
                    .Select(s => new { s.ServiceName, DemandPercent = (s.ServiceCount / (double)totalRequests) * 100 })
                    .OrderByDescending(x => x.DemandPercent)
                    .Take(10)
                    .ToList();

                // Заполняем данные графика (столбцы K:L шаблона)
                for (int i = 0; i < sortedServices.Count; i++)
                {
                    workSheet.Cell(30 + i, 11).Value = sortedServices[i].ServiceName;  // K30:K39
                    workSheet.Cell(30 + i, 12).Value = Math.Round(sortedServices[i].DemandPercent, 1);  // L30:L39
                }



                workSheet.Columns().AdjustToContents();

                // Сохранение
                workbook.SaveAs(filePath);
            }

            MessageBox.Show($"Отчет сохранён в Excel: {filePath}", "Успешно",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    

    // Класс для хранения данных услуг
    public class ServiceData
    {
        public string ServiceName { get; set; } //Название
        public decimal ServiceCost { get; set; } //Стоимость
        public int ServiceDuration { get; set; }    //Время
        public int ServiceCount { get; set; }   //Количество оказанных услуг
        public decimal TotalIncome { get; set; }    //Стоимость со скидкой
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
