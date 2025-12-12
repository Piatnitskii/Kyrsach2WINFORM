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

        public MenuAdmin()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.BackColor = SystemColors.InactiveCaption;

            LeftBorderPBTN = new Panel();
            LeftBorderPBTN.Size = new Size(7, 60);
            MainPanel.Controls.Add(LeftBorderPBTN);

            
            if(ConnectAndData.Role == "2")
            {
                OpenEmploey.Visible = false;
                OpenUser.Visible = false;
                label1.Text = "Меню менеджера";
                this.Text = "BARBERSHOP | Терминал менеджера";
            }
            else   
                OpenBooks.Visible = false;  //У админа скрываем справочники

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
    }
}
