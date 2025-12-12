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
    public partial class AddZapiciData : Form
    {
        //Fields
        Emploey emploey;                                //Наш мастер
        Dictionary<string, Button> timeButtonMap;       // Тут все кнопки и  их времена
        Button MainButton5;

        static private Panel LeftBorderPBTN;
        static public Button CurrentBTN;

        public AddZapiciData(Control control, Emploey emploey)
        {
            InitializeComponent();

            this.emploey = emploey;

            // Установка минимальной даты - сегодня
            dateTimePicker1.MinDate = DateTime.Today;
            // Установка максимальной даты - сегодня + 1 месяц
            dateTimePicker1.MaxDate = DateTime.Today.AddMonths(1);

            this.MainButton5 = (Button)control;

           // Красивости
            LeftBorderPBTN = new Panel();
            LeftBorderPBTN.Size = new Size(5, 32);
            this.Controls.Add(LeftBorderPBTN);

            // Время - кнопка
            this.timeButtonMap = new Dictionary<string, Button>
            {
                {"09:00", button1}, {"09:30", button2}, {"10:00", button3},
                {"10:30", button4},  {"11:00", button6},
                {"11:30", button7}, {"12:00", button8}, {"12:30", button9},
                {"13:00", button10}, {"13:30", button11}, {"14:00", button12},
                {"14:30", button13}, {"15:00", button14}, {"15:30", button15},
                {"16:00", button16}, {"16:30", button17}, {"17:00", button18},
                {"17:30", button19}, {"18:00", button20}, {"18:30", button21},
                {"19:00", button22}, {"19:30", button23}, {"20:00", button24}, 
                {"20:30", button25}
            };

            // Проверяем, выбирали ли ранее уже дату
            if (AddZapicWiz.CurrentFiveForm.Count != 0)
                True_Start();
            else
                dateTimePicker1_ValueChanged(dateTimePicker1, EventArgs.Empty);
        }

        // Приводи страницу к виду ранее (тоесть если уже была выбрана дата и время)
        void True_Start()
        {
            CheckTime($"Select IdRecord, Date_Record, Time_Record, Total_Time FROM Record Where Id_Employe = '{emploey.IdEmploey}' AND Date_Record = '{AddZapicWiz.CurrentFiveForm[0]}';");
            ((Button)AddZapicWiz.CurrentFiveForm[2]).PerformClick();
        }

        // Вычисляет общее время всех услуг
        int GetTotalTimeInt()
        {
            int TotalTimeInt = 0;
            foreach (Service service in AddZapicWiz.CurrentFourForm)
            {
                TotalTimeInt += Convert.ToInt32(service.Duration);
            }
            return TotalTimeInt;
        }

        //ОСНОВНАЯ функция, закрашиваем квадратики на основе свободного, занятого и почти занятого времени(зеленые)
        void CheckTime(string CMD)
        {
            try
            {
                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    Con.Open();

                    MySqlCommand cmd = new MySqlCommand(CMD, Con);
                    MySqlDataReader RDR = cmd.ExecuteReader();


                    //Читаем - Закрашиваем квадратики в СЕРЫЙ
                    while (RDR.Read())  //время  записи   и    общее время всех услуг
                        PaintButtonGray(RDR[2].ToString(), RDR[3].ToString());
                    
                    //Закрашиваем в ЗЕЛЕНЫЙ
                    PaintButtonGreen(TimeSpan.FromMinutes(GetTotalTimeInt()));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Вычисляет конец периода, которая занимает рассматриваемая запись на выбранный день
        // TimeOnly + Total_Time вобщем
        string GetTimeForRecord(string timeOnly, int TotalTimeInt)
        {
            // Парсим время в TimeSpan
            TimeSpan time = TimeSpan.Parse(timeOnly);

            // Добавляем минуты
            TimeSpan result = time.Add(TimeSpan.FromMinutes(TotalTimeInt));

            // Форматируем результат
            string ResultTime = $"{(int)result.TotalHours}:{result.Minutes:00}"; // "10:00"

            return ResultTime;
        }

        //Разрисовываем квадратики в серый
        void PaintButtonGray(string Time_Record, string Total_Time)
        {
            string TimeOnly = (DateTime.Parse(Time_Record)).ToString("HH:mm");                  // Время записи, в формате HH:mm
            int TotalTimeInt = Convert.ToInt32(Total_Time);                                      // Суммарное время услуг

            TimeSpan OccupiedTime = TimeSpan.Parse(GetTimeForRecord(TimeOnly, TotalTimeInt));   // Время записи + время всех услуг

            // Получаем все времена в правильном порядке
            var times = timeButtonMap.Keys.OrderBy(t => TimeSpan.Parse(t)).ToList();

            // Отключаем кнопки начиная с TimeOnly и до OccupiedTime
            bool startDisabling = false;
            foreach (var time in times)
            {
                if (time == TimeOnly || startDisabling)
                {
                    timeButtonMap[time].Enabled = false;
                    timeButtonMap[time].BackColor = Color.Silver;
                    startDisabling = true;

                    // Проверяем, нужно ли продолжать отключать следующие кнопки
                    if (TimeSpan.Parse(time).Add(TimeSpan.FromMinutes(30)) > OccupiedTime)
                        break;
                }
            }
        }

        //Разрисовываем квадратики в зеленый
        void PaintButtonGreen(TimeSpan TimeService)
        {
            // Получаем все времена в правильном порядке
            var times = timeButtonMap.Keys.OrderBy(t => TimeSpan.Parse(t)).ToList();
            TimeSpan endOfDay = TimeSpan.Parse("21:00");

            foreach (var time in times)
            {
                // Работаем только с активными кнопками (не занятыми другими клиентами)
                if (timeButtonMap[time].Enabled)
                {
                    TimeSpan slotStart = TimeSpan.Parse(time);
                    TimeSpan slotEnd = slotStart.Add(TimeService);

                    bool isUnavailable = false;

                    // Проверка 1: выходим за пределы рабочего дня
                    if (slotEnd > endOfDay)
                    {
                        isUnavailable = true;
                    }
                    // Проверка 2: пересекаемся с занятыми слотами
                    else
                    {
                        // Проверяем все слоты
                        foreach (var checkTime in times)
                        {
                            TimeSpan checkSlot = TimeSpan.Parse(checkTime);

                            // Если этот слот внутри нашего интервала и он занят
                            if (checkSlot > slotStart && checkSlot < slotEnd)
                            {
                                if (!timeButtonMap[checkTime].Enabled)
                                {
                                    isUnavailable = true;
                                    break;
                                }
                            }

                            // Если вышли за пределы нашего интервала, прекращаем проверку
                            if (checkSlot >= slotEnd)
                                break;
                        }
                    }

                    // Если слот недоступен - делаем его зеленым и неактивным
                    if (isUnavailable)
                    {
                        timeButtonMap[time].BackColor = Color.YellowGreen;
                        timeButtonMap[time].Enabled = false;
                    }
                }
            }
        }

        // Ресетаем все кнопки
        void TrueButton()
        {
            // Получаем все времена в правильном порядке
            var times = timeButtonMap.Keys.OrderBy(t => TimeSpan.Parse(t)).ToList();
            
            foreach (var time in times)
            {
                timeButtonMap[time].Enabled = true;
                timeButtonMap[time].BackColor = Color.Bisque;
            }

            CurrentBTN = null;
            LeftBorderPBTN.Visible = false;
        }

        // Выбираем дату записи
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            //Блокируем кнопку и очищаем ArrayList
            MainButton5.Enabled = false;
            MainButton5.BackColor = Color.FromArgb(150, 116, 102);
            MainButton5.ForeColor = Color.White;

            AddZapicWiz.CurrentFiveForm = new System.Collections.ArrayList();

            //Ресетаем все и откидываем запрос
            TrueButton();
            CheckTime($"Select IdRecord, Date_Record, Time_Record, Total_Time FROM Record Where Id_Employe = '{emploey.IdEmploey}' AND Date_Record = '{dateTimePicker1.Value.ToString("yyyy-MM-dd")}';");
        }

        
        //Metods для кнопок
        private void ActiveButton(object SenderBTN, Color color)
        {
            if (SenderBTN != null)
            {
                DisableButton();
                //Button
                CurrentBTN = (Button)SenderBTN;
                CurrentBTN.BackColor = Color.FromArgb(255, 228, 196);
                CurrentBTN.ForeColor = Color.Black;
                
                //Left border button
                LeftBorderPBTN.BackColor = color;
                LeftBorderPBTN.Location = new Point(CurrentBTN.Location.X, CurrentBTN.Location.Y);
                LeftBorderPBTN.Visible = true;
                LeftBorderPBTN.BringToFront();
            }
        }

        static public void DisableButton()
        {
            if (CurrentBTN != null)
            {
                LeftBorderPBTN.Visible = false;
                CurrentBTN.BackColor = Color.Bisque;
                CurrentBTN.ForeColor = Color.Black;
                CurrentBTN.FlatAppearance.BorderColor = Color.Black;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MainButton5.Enabled = true;
            ActiveButton(sender, Color.FromArgb(64, 64, 64));

            AddZapicWiz.CurrentFiveForm = new System.Collections.ArrayList();
            AddZapicWiz.CurrentFiveForm.Add(dateTimePicker1.Value.ToString("yyyy-MM-dd"));
            AddZapicWiz.CurrentFiveForm.Add(((Button)sender).Text);
            AddZapicWiz.CurrentFiveForm.Add(((Button)sender));
        }
    }
}
