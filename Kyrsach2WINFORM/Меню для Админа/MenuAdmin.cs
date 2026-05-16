using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Kyrsach2WINFORM
{
    public partial class MenuAdmin : Form
    {
        #region Title bar
        //Color title bar
        [DllImport("DwmApi")] //System.Runtime.InteropServices
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

        protected override void OnHandleCreated(EventArgs e)
        {
            if (DwmSetWindowAttribute(Handle, 19, new[] { 1 }, 4) != 0)
                DwmSetWindowAttribute(Handle, 20, new[] { 1 }, 4);
        }
        #endregion

        //Fields
        static private Panel LeftBorderPBTN;
        static public  Button CurrentBTN;


        // Открытая форма
        private Form ActiveForm = null;

        //Таймер
        static Point lastMousePos;      // Последняя позиция
       

        private const double MOVEMENT_THRESHOLD = 0.1;  // Пикселей для "движения"

        // ЛОК
        static readonly object timerLock = new object(); 
        private readonly object mouseLock = new object();

        public void MenuAdmin_Load(object sender, EventArgs e)
        {
            Optimize.remainingTime = 180; // Устанавливаем оставшееся время в секундах (3 минуты)
            Optimize.inactivityTimer.Start(); // Стартуем таймеры
            Optimize.movementDetectTimer.Start();

            lastMousePos = PointToClient(Cursor.Position);
        }
        

        //При закрытии формы останавливаем таймеры
        private void MenuAdmin_FormClosing(object sender, FormClosingEventArgs e)
        {
            Optimize.StopTimerSafely();  
        }

        //Тикет таймера на движение мыши
        private void MovementDetectTimer_Tick(object sender, EventArgs e)
        {

                if (Optimize.isFormClosing) return;

                lock (mouseLock)  // ← ЗАЩИТА
                {
                    Point currentScreenPos = Cursor.Position;
                    Point currentFormPos = PointToClient(currentScreenPos);  // ← КЛЮЧ!

                    // Проверяем только внутри формы
                    if (ClientRectangle.Contains(currentFormPos))
                    {
                        int deltaX = Math.Abs(currentFormPos.X - lastMousePos.X);
                        int deltaY = Math.Abs(currentFormPos.Y - lastMousePos.Y);

                        if (deltaX > MOVEMENT_THRESHOLD || deltaY > MOVEMENT_THRESHOLD)
                        {
                            label5.Text = $"🖱️ ДВИЖЕТСЯ: ΔX={deltaX}, ΔY={deltaY}";

                            if (Optimize.isFormClosing) return;
                            UserActivityDetected(null, null);
                        }
                    }

                    lastMousePos = currentFormPos;
                }
        }


        //Сбрасываем таймер
        static public void UserActivityDetected(object sender, EventArgs e)
        {

            if (Optimize.isFormClosing) return;

            lock (timerLock)  // ← БЛОКИРУЕМ МНОЖЕСТВЕННЫЕ ВЫЗОВЫ
            {
                if (Optimize.isUpdatingTimer) return;  // Пропускаем дубли

                Optimize.isUpdatingTimer = true;

                Optimize.inactivityTimer.Stop();
                Optimize.remainingTime = 10;
                Optimize.UpdateRemainingTimeLabel(MenuAdmin.Instance?.label4);
                Optimize.inactivityTimer.Start();

                Optimize.isUpdatingTimer = false;
            }
        }



        //Тикет таймера для отсчета времени для закрытия формы
        public void InactivityTimer_Tick(object sender, EventArgs e)
        {

                if (Optimize.isFormClosing) return;

                lock (timerLock)  // ← БЛОКИРУЕМ Tick
                {
                    if (Optimize.isFormClosing) return;

                    if (Optimize.remainingTime > 0)
                    {
                        Optimize.remainingTime--;
                    Optimize.UpdateRemainingTimeLabel(this.label4);
                    }
                    else
                    {
                        Optimize.inactivityTimer.Stop();
                        if (Optimize.isFormClosing) return;
                        Application.OpenForms["MenuAdmin"]?.Close();
                    }
                }

        }

        // СТАТИЧЕСКАЯ ссылка на единственный инстанс
        public static MenuAdmin Instance { get; private set; }

        public MenuAdmin()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.BackColor = SystemColors.InactiveCaption;

            // устанавливаем Instance при создании формы
            Instance = this;

            this.OpenEmploey.Click += UserActivityDetected;
            this.OpenClient.Click += UserActivityDetected;
            this.OpenBooks.Click += UserActivityDetected;
            this.OpenOplata.Click += UserActivityDetected;
            this.OpenOrder.Click += UserActivityDetected;
            this.OpenYslygi.Click += UserActivityDetected;
            this.openSchedule.Click += UserActivityDetected;
            this.OpenUser.Click += UserActivityDetected;
            this.openAdmin.Click += UserActivityDetected;

            LeftBorderPBTN = new Panel();
            LeftBorderPBTN.Size = new Size(7, 60);
            MainPanel.Controls.Add(LeftBorderPBTN);

            // Настройка таймера 
            Optimize.inactivityTimer = new Timer();
            Optimize.inactivityTimer.Interval = 1000;
            Optimize.inactivityTimer.Tick += InactivityTimer_Tick;

            // Таймер: если 1 сек без движения → "застыл"
            Optimize.movementDetectTimer = new Timer { Interval = 1000 };
            Optimize.movementDetectTimer.Tick += MovementDetectTimer_Tick;
            


            if (ConnectAndData.Role == "2")
            {
                //У менеджера скрываем
                OpenUser.Visible = false;   // пользователи
                OpenBooks.Visible = false;   // справочники
                openAdmin.Visible = false; //администрирование
                openSchedule.Visible = false; //расписание
                label1.Text = "Меню менеджера";
                this.Text = "BARBERSHOP | Терминал менеджера";
            }
            else if(ConnectAndData.Role == "3")
            {
                label1.Text = "Меню мастера";
                this.Text = "BARBERSHOP | Терминал мастера";
                //У мастера скрываем
                OpenUser.Visible = false;   // пользователи
                OpenBooks.Visible = false;   // справочники
                OpenEmploey.Visible = false;  //сотрудники
                OpenClient.Visible = false;  //клиенты
                OpenYslygi.Visible = false;  //услуги
                OpenOrder.Visible = false;  //записи
                OpenOplata.Visible = false;  //оплата
                openAdmin.Visible = false; //администрирование
            }
            else
            {
                //У админа скрываем
                OpenEmploey.Visible = false;  //сотрудники
                OpenClient.Visible = false;  //клиенты
                OpenYslygi.Visible = false;  //услуги
                OpenOrder.Visible = false;  //записи
                openSchedule.Visible = false; //расписание
                OpenOplata.Visible = false;  //оплата
            }
            label3.Text = ConnectAndData.SurnameUser;
        }


        //Struct
        private struct RGBColors
        {
            public static Color color1 = Color.FromArgb(64, 64, 64);
        }


        //Metods для кнопок
        private void ActiveButton(object SenderBTN, Color color)
        {
            if(SenderBTN != null)
            {
                DisableButton();
                //Button
                CurrentBTN = (Button)SenderBTN;
                CurrentBTN.BackColor = Color.FromArgb(255, 228, 196);
                CurrentBTN.ForeColor = color;
                CurrentBTN.TextAlign = ContentAlignment.MiddleCenter;
                CurrentBTN.TextImageRelation = TextImageRelation.TextBeforeImage;
                CurrentBTN.ImageAlign = ContentAlignment.MiddleRight;
                //Left border button
                LeftBorderPBTN.BackColor = color;
                LeftBorderPBTN.Location = new Point(0, CurrentBTN.Location.Y);
                LeftBorderPBTN.Visible = true;
                LeftBorderPBTN.BringToFront();
            }
        }

        static public void DisableButton()
        {
            if (CurrentBTN != null)
            {
                LeftBorderPBTN.Visible = false;
                CurrentBTN.Enabled = true;
                CurrentBTN.BackColor = Color.FromArgb(150, 116, 102);
                CurrentBTN.ForeColor = Color.White;
                CurrentBTN.TextAlign = ContentAlignment.MiddleLeft;
                CurrentBTN.TextImageRelation = TextImageRelation.ImageBeforeText;
                CurrentBTN.ImageAlign = ContentAlignment.MiddleLeft;
                CurrentBTN = null;
            }
        }


       

        //Метод открытия дочерних форм
        private void OpenChildForm(Form ChildForm)
        {

            if (ActiveForm != null)
            {
                ActiveForm.Dispose();
                ActiveForm.Close();
            }
               

            ActiveForm = ChildForm;

            //Настройка
            ChildForm.TopLevel = false;
            ChildForm.FormBorderStyle = FormBorderStyle.None;
            ChildForm.Dock = DockStyle.Fill;

            PanelChild.Controls.Add(ChildForm);
            PanelChild.Tag = ChildForm;
            ChildForm.BringToFront();
            

            ChildForm.Show();
        }


       



        private void button8_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }


        private void OpenEmploey_Click(object sender, EventArgs e)
        {

            if (!(CurrentBTN == sender))
            {
                ActiveButton(sender, RGBColors.color1);
                OpenChildForm(new Employee());
            }
        }

        private void OpenClient_Click(object sender, EventArgs e)
        {
            if( !(CurrentBTN == sender))
            {
                ActiveButton(sender, RGBColors.color1);
                OpenChildForm(new Clients());
            }
        }

        private void OpenOrder_Click(object sender, EventArgs e)
        {
            if (!(CurrentBTN == sender))
            {
                ActiveButton(sender, RGBColors.color1);
                OpenChildForm(new Zapici());
            }
        }

        private void OpenOplata_Click(object sender, EventArgs e)
        {
            if (!(CurrentBTN == sender))
            {
                ActiveButton(sender, RGBColors.color1);
                OpenChildForm(new Oplata());
            }
        }

        private void OpenYslygi_Click(object sender, EventArgs e)
        {
            if (!(CurrentBTN == sender))
            {
                ActiveButton(sender, RGBColors.color1);
                OpenChildForm(new Yslygy());
            }
        }

        private void OpenBooks_Click(object sender, EventArgs e)
        {
            if (!(CurrentBTN == sender))
            {
                ActiveButton(sender, RGBColors.color1);
                OpenChildForm(new books());
            }
        }

        private void OpenUser_Click(object sender, EventArgs e)
        {
            if (!(CurrentBTN == sender))
            {
                ActiveButton(sender, RGBColors.color1);
                OpenChildForm(new User());
            }
        }

        private void openAdmin_Click(object sender, EventArgs e)
        {
            if (!(CurrentBTN == sender))
            {
                ActiveButton(sender, RGBColors.color1);
                OpenChildForm(new AdminAdminForm());
            }
        }

        private void openSchedule_Click(object sender, EventArgs e)
        {
            if (!(CurrentBTN == sender))
            {
                ActiveButton(sender, RGBColors.color1);
                OpenChildForm(new Schedule());
            }
        }


    }
}
