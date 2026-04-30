using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace Kyrsach2WINFORM
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        //Проверка соединения
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string HostName = textBox1.Text;
                string UserName = textBox2.Text;
                string Password = textBox3.Text;

                String Connec = $"host={HostName};uid={UserName};pwd={Password};";

                using (MySqlConnection Con = new MySqlConnection(Connec))
                {
                    Con.Open();
                }
                MessageBox.Show($"Соединение установлено!", "Успех подключения", MessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Невозможно подключиться с указанными настройками", "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //Сохранить
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                config.AppSettings.Settings["HostName"].Value = textBox1.Text;
                config.AppSettings.Settings["UserName"].Value = textBox2.Text;
                config.AppSettings.Settings["Password"].Value = textBox3.Text;

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");

                ConnectAndData.Update();

                MessageBox.Show($"Данные сохранены", "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            try
            {
                textBox1.Text = ConfigurationManager.AppSettings["HostName"];
                textBox2.Text = ConfigurationManager.AppSettings["UserName"];
                textBox3.Text = ConfigurationManager.AppSettings["Password"];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
