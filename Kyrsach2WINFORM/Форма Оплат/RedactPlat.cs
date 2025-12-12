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
    public partial class RedactPlat : Form
    {
        public RedactPlat()
        {
            InitializeComponent();

            dateTimePicker1.MaxDate = DateTime.Now.AddYears(1);
            dateTimePicker1.MinDate = DateTime.Now.AddDays(-1);

            dateTimePicker2.Format = DateTimePickerFormat.Custom;
            dateTimePicker2.CustomFormat = "HH:mm";
            dateTimePicker2.ShowUpDown = true;
            

        }

        private void button8_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Если что то меняется - можно редактировать
        void CheckData()
        {

            if (textBox1.Text.Trim() != "")
                button1.Enabled = true;
            else
                button1.Enabled = false;
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            CheckData();
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            CheckData();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            CheckData();
        }

        //Итоговая сумма
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
            else { e.Handled = false; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Изменить данные оплаты?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                
            }
            else
                return;
        }
    }
}
