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
    public partial class AddZapic : Form
    {
        public AddZapic()
        {
            InitializeComponent();

            monthCalendar1.MaxDate = DateTime.Now.AddYears(1);
            monthCalendar1.MinDate = DateTime.Now.AddDays(-1);

            comboBox1.Items.Add("Не выбрано");
            comboBox2.Items.Add("Не выбрано");
            comboBox3.Items.Add("Не выбрано");
            comboBox4.Items.Add("Не выбрано");
            comboBox5.Items.Add("Не выбрано");

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
            comboBox5.SelectedIndex = 0;
        }

        //Проверка заполнености обязательных полей
        void CheckData()
        {

            if (comboBox1.SelectedIndex != 0 && comboBox2.SelectedIndex != 0 && comboBox3.SelectedIndex != 0 && comboBox4.SelectedIndex != 0 && comboBox5.SelectedIndex != 0)
                button2.Enabled = true;
            else
                button2.Enabled = false;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckData();
        }

        //Добавить
        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Добавить запись?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                //Логика формирования записей
            }
            else
                return;

        }
    }
}
