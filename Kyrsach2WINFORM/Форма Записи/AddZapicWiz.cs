using System;
using System.Collections;
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
    public partial class AddZapicWiz : Form
    {

        //Fields
        public Client client = new Client(null, null, null, null, null);
        public Emploey emploey = new Emploey(null, null, null, null, null, null, null);

        static public ArrayList CurrentFourForm = new ArrayList();  // Структура Услуг
        static public ArrayList CurrentFiveForm = new ArrayList();  // Структура для выбора даты и времени: дата / время / кнопка Active

        static public Button CurrentBTN;

        public AddZapicWiz()
        {
            InitializeComponent();

            this.DoubleBuffered = true;

            //Очищаем списки, они же у нас статик
            CurrentFourForm = new ArrayList();
            CurrentFiveForm = new ArrayList();

            // Закрытие формы по завершению создания записи
            FormManager.RegisterForm("AddZapicWiz", this);
        }

        //Metods для кнопок
        // Накидываем на кнопку цвета, "Активируем" ее
        private void ActiveButton(object SenderBTN, Color color)
        {
            if (SenderBTN != null)
            {
                CurrentBTN = (Button)SenderBTN;
                CurrentBTN.BackColor = Color.FromArgb(255, 228, 196);
                CurrentBTN.ForeColor = color;
            }
        }

        // Открытая форма
        private Form ActiveForm = null;


        //Метод открытия дочерних форм, "открыл и забыл"
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

            panel2.Controls.Add(ChildForm);
            panel2.Tag = ChildForm;
            ChildForm.BringToFront();


            ChildForm.Show();
        }

        //Обработчики события нажатия на кнопку
        public void button1_Click(object sender, EventArgs e)
        {
            if (!(CurrentBTN == sender))
            {
                ActiveButton(sender, Color.FromArgb(64, 64, 64));
                OpenChildForm(new AddZapicClient(button2, client));
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!(CurrentBTN == sender))
            {
                ActiveButton(sender, Color.FromArgb(64, 64, 64));
                OpenChildForm(new AddZapiciMaster(button3, button4, button5, emploey));
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!(CurrentBTN == sender))
            {
                ActiveButton(sender, Color.FromArgb(64, 64, 64));
                OpenChildForm(new AddZapicYslyga(button4, button5));
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!(CurrentBTN == sender))
            {
                ActiveButton(sender, Color.FromArgb(64, 64, 64));
                OpenChildForm(new AddZapiciData(button5, emploey));
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!(CurrentBTN == sender))
            {
                ActiveButton(sender, Color.FromArgb(64, 64, 64));
                OpenChildForm( new AddZapicResult(client, emploey));
            }
        }

        // При загрузке формы сразу же открываем форму выбора клиента
        private void AddZapicWiz_Load(object sender, EventArgs e)
        {
            button1.PerformClick();
        }
    }
}
