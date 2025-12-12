using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MySql.Data.MySqlClient;
using System.Collections;
namespace Kyrsach2WINFORM
{
    public partial class AdminAdminForm : Form
    {
        public AdminAdminForm()
        {
            InitializeComponent();
        }

        //Восстановление стурктуры
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                string File;
                using (StreamReader Reader = new StreamReader(@"Resources\Structure.txt"))
                {
                    File = Reader.ReadToEnd();
                }

                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.TryConnect))
                {
                    Con.Open();

                    MySqlCommand cmd = new MySqlCommand($"{File}", Con);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Структура Базы Данных восстановлена", "Успех операции", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка Чтение или Записи", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
