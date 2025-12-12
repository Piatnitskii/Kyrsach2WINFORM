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
    public partial class Menu : Form
    {
        public Menu()
        {
            InitializeComponent();
        }


        // Открытая форма
        private Form ActiveForm = null;

        //Метод открытия дочерних форм
        private void OpenChildForm(Form ChildForm)
        {
            if (ActiveForm != null)
                ActiveForm.Close();

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

        //Кнопка ВЫХОД
        private void button8_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //Клиенты
        private void OpenClient_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Clients());
        }
        //Заказы
        private void OpenOrder_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Zapici());
        }
        //Оплата
        private void OpenPay_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Oplata());
        }
        //Услуги
        private void OpenService_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Yslygy());
        }

        //Справочники
        private void OpenBook_Click(object sender, EventArgs e)
        {
            OpenChildForm(new books());
        }
    }
}
