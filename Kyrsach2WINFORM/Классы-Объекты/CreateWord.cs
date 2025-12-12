using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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


namespace Kyrsach2WINFORM.Классы_Объекты
{
    // Класс для создания чека
    public class CreateWord
    {

        static public  void CheckCreator(string OrderID)
        {
            Word.Application Ap = null;
            Word.Document Doc = null;

            try
            {
                // 1. Получаем дату,стоимость и скидку
                string[] DataAmount = GetDataPayment(OrderID).Split(';') ;

                string PaymentTime = DataAmount[0];
                
                string Discount = Convert.ToDouble(DataAmount[2]).ToString("F2");
                string ItogSumma = (Convert.ToDouble(DataAmount[1].Replace(',','.')) - Convert.ToDouble(DataAmount[2].Replace(',', '.'))).ToString("F2");

                // 2. Получаем все услуги по записи
                List<ServiceList> ListServiceName = GetServices(OrderID);


                string fileName = Directory.GetCurrentDirectory() + @"\Талон.docx"; // шаблон

                

                Ap = new Word.Application();
                Ap.Visible = false;

                Doc = Ap.Documents.Open(fileName, ReadOnly: true);

                SetCheckPageSize(Doc);

                ReplaceWord("{НомерЗаписи}", OrderID, Doc);
                ReplaceWord("{Стоимость}", ItogSumma, Doc);
                ReplaceWord("{Скидка}", Discount, Doc);
                ReplaceWord("{Дата}", PaymentTime, Doc);

                Table table = Doc.Tables[1]; // Выбираем 1 таблицу

                foreach (var service in ListServiceName)
                {
                    Row newRow = table.Rows.Add();

                    // Заполняем ячейки
                    newRow.Cells[1].Range.Text = service.ServiceName;
                }

                AdjustPageHeightToContent(Doc);
                Ap.Visible = true;
            }
            catch (Exception ex)
            {
                Doc.Close(SaveChanges: false);
                Ap.Quit(SaveChanges: false);
                MessageBox.Show(ex.Message, "Внимание");
            }
        }

        // Устанавливаем ВЫСОТУ страницы для чека
        static private void SetCheckPageSize(Document doc)
        {
                PageSetup pageSetup = doc.PageSetup;
                pageSetup.PageHeight = doc.Application.CentimetersToPoints(30f);
        }


        // Реализует "динамический" чек
        static private void AdjustPageHeightToContent(Document doc)
        {
            // Вычисляем высоту содержимого
            float contentHeightPoints = doc.Content.ComputeStatistics(WdStatistic.wdStatisticLines) * doc.Content.Font.Size * 1.2f;

            // Конвертируем в сантиметры (1 см = 28.35 точек)
            float contentHeightCm = contentHeightPoints / 28.35f;

            // Добавляем еще
            float totalHeightCm = contentHeightCm + 4f;

            // Устанавливаем высоту страницы
            doc.PageSetup.PageHeight = doc.Application.CentimetersToPoints(totalHeightCm);
        }

        // Получаем дату и стоимость (скидку)
        static private string GetDataPayment(string OrderID)
        {
            
            string CMDPayment = $"SELECT * FROM Payment WHERE Id_Record = {OrderID}";   //Ищем по записи, все равно он только 1 в базе
            string Result = "";

            using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
            {
                    Con.Open();
                        
                    MySqlCommand cmd = new MySqlCommand(CMDPayment, Con);
                    MySqlDataReader RDR = cmd.ExecuteReader();

                    //Заполняем данными
                    while (RDR.Read())
                            Result = RDR[2].ToString() + ";" + Convert.ToDouble(RDR[3]) + ";" + Convert.ToDouble(RDR[4].ToString());   // дата;стоимость;скидка
            }

            return Result;
            
        }

        // Получаем все услуги по записи
        static private List<ServiceList> GetServices(string OrderID)
        {
           
            string CMD = $"Select  Service.Name as 'Название_Услуги' FROM Service_In_Record INNER JOIN Service ON Id_Service = IdService WHERE Id_Record = {OrderID};";
            List<ServiceList> Services = new List<ServiceList>();
            using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
            {
                Con.Open();

                MySqlCommand cmd = new MySqlCommand(CMD, Con);
                MySqlDataReader RDR = cmd.ExecuteReader();

                //Заполняем данными
                while (RDR.Read())
                    Services.Add(new ServiceList
                    {
                        ServiceName = RDR["Название_Услуги"].ToString(),
                    });

                return Services;
            }
        }


        // ....
        static private void ReplaceWord(string src, string dest, Word.Document doc)
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


        // Представляет объект - Услугу
        public class ServiceList
        {
            public string ServiceName { get; set; }
        }
    }
}
