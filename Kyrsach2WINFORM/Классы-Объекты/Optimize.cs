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

        //Работа с временем
        public static Timer inactivityTimer;    // Таймер для отсчета 3 минут
        public static Timer movementDetectTimer; // Таймер для движения мыши
        public static int remainingTime; // оставшееся время в секундах
        static public bool isFormClosing = false;  // ← ГЛАВНЫЙ ФЛАГ
        static public bool isUpdatingTimer = false;  // ← ФЛАГ ЗАЩИТЫ

        //Остановка таймера
        public static void Offtimer()
        {
            inactivityTimer.Stop();
            movementDetectTimer.Stop();
        }

        //Перезапуск таймера
        public static void Ontimer()
        {
            remainingTime = 180;
            Optimize.isFormClosing = false;

            Optimize.inactivityTimer.Enabled = true; 
            Optimize.movementDetectTimer.Enabled = true;

            inactivityTimer.Start();
            movementDetectTimer.Start();
        }

        //Безопасная остановка таймеров
        static public void StopTimerSafely()
        {
                Optimize.isFormClosing = true;  // ← БЛОКИРУЕМ Tick

                Optimize.inactivityTimer.Enabled = false;  // БЫСТРОЕ ОСТАНОВЛЕНИЕ
                Optimize.movementDetectTimer.Enabled = false;

                Optimize.inactivityTimer.Stop();
                Optimize.movementDetectTimer.Stop();

                Optimize.inactivityTimer.Dispose();
                Optimize.movementDetectTimer.Dispose();
        }


        // Преобразуем оставшееся время в минуты и секунды
        static public void UpdateRemainingTimeLabel(Label label)
        {
            int minutes = Optimize.remainingTime / 60;
            int seconds = Optimize.remainingTime % 60;
            if (label != null)
                label.Text = $"{minutes:D2}:{seconds:D2}";
        }



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
