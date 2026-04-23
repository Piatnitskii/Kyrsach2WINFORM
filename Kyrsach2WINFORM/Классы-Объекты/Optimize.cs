using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kyrsach2WINFORM
{
    //Класс одержит общие функции для оптимизации работы/сокращения кода
    public static class Optimize
    {
        // Метод для включения двойной буферизации DataGridView
        public static void SetDoubleBuffered(DataGridView dgv)
        {
            Type dgvType = dgv.GetType();
            System.Reflection.PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            pi.SetValue(dgv, true, null);
        }


        //Функция маскирует перснональные данные в поле ФИО в формате Имя Отчество Ф.
        public static void HideMyFio(ConvertEventArgs e)
        {
            if (e.Value != null)
            {
                var Val = e.Value.ToString().Split(' ');


                //Формируем новый внешний вид данных в колонке данной строки [Имя] [Фамилия] [Отчество]
                string Result;
                if (Val.Length == 3 && Val[2].Trim() != "")
                    Result = Val[0] + " " + Val[2] + " " + (Val[1])[0] + ".";
                else
                    Result = Val[0] + " " + (Val[1])[0] + ".";

                e.Value = Result;
            }
        }
        //Функция маскирует перснональные данные в поле Номер телефона в формате 7901****882
        public static void HideMyPhone(ConvertEventArgs e)
        {
            if (e.Value != null)
            {
                var Val = e.Value.ToString();
                string Result = Val[0].ToString() + Val[1].ToString() + Val[2].ToString() + Val[3].ToString() + "****" + Val[8].ToString() + Val[9].ToString() + Val[10].ToString();

                e.Value = Result;
            }
        }
    }
}
