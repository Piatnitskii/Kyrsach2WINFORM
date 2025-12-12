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

            // Настройка диалога
            openFileDialog1.Filter = "CSV - файлы(*.csv) | *.csv";
            openFileDialog1.FileName = "";

            openFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

            openFileDialog1.CheckPathExists = true;
            openFileDialog1.CheckFileExists = true;
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

        

        // Импортирование данных
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                    return;

                string filename = openFileDialog1.FileName;

                string[] FullText = File.ReadAllLines(filename, Encoding.UTF8);

                switch (comboBox1.SelectedItem.ToString())
                {
                    case "Роли":

                        using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                        {
                            Con.Open();
                            MySqlCommand cmd;
                            int Count = 0;

                            // Проверка количества колонок в импортируемом файле
                            for (int i = 1; i < FullText.Length; i++)
                            {
                                string[] ItemInline = FullText[i].Split(';');
                                if (ItemInline.Length > 2 || ItemInline.Length < 2)
                                {
                                    MessageBox.Show("Ошибка соответствия необходимого количества колонок с действительным!", "Ошибка импорта", MessageBoxButtons.OK);
                                    return;
                                }
                            }
                            

                            for (int i = 1; i < FullText.Length; i++)
                            {
                                string[] ItemInline = FullText[i].Split(';');
                                cmd = new MySqlCommand($"INSERT INTO Role (IdRole,  Name) VALUES ('{ItemInline[0]}','{ItemInline[1]}');", Con);

                                cmd.ExecuteNonQuery();
                                Count++;
                            }

                            MessageBox.Show($"Импорт успешен! Импортированно записей: {Count}", "Выполнено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        break;
                    case "Пользователи":
                        for (int i = 1; i < FullText.Length; i++)
                        {
                            string[] ItemInline = FullText[i].Split(';');
                            if (ItemInline.Length > 6 || ItemInline.Length < 5)
                            {
                                MessageBox.Show("Ошибка соответствия необходимого количества колонок с действительным!", "Ошибка импорта", MessageBoxButtons.OK);
                                return;
                            }
                        }

                        using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                        {
                            Con.Open();
                            MySqlCommand cmd;
                            int Count = 0;
                            for (int i = 1; i < FullText.Length; i++)
                            {
                                string[] ItemInline = FullText[i].Split(';');
                                cmd = new MySqlCommand($"INSERT INTO User (Name, Surname, Patronymic, Password, Login, Id_Role) VALUES ('{ItemInline[0]}','{ItemInline[1]}','{ItemInline[2]}','{ItemInline[3]}','{ItemInline[4]}', '{ItemInline[5]}');", Con);

                                cmd.ExecuteNonQuery();
                                Count++;
                            }
                            MessageBox.Show($"Импорт успешен! Импортированно записей: {Count}", "Выполнено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Unknown database"))
                    MessageBox.Show("Отсутствует необходимая База данных!", "Ошибка импорта", MessageBoxButtons.OK);
                else if (ex.Message.Contains("Cannot add or update a child row"))
                    MessageBox.Show("Отсутствуют необходимые данные во второстепенной таблице!", "Ошибка импорта", MessageBoxButtons.OK);
                else
                    MessageBox.Show(ex.Message, "Ошибка Импорта", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Добавляем таблицы
        private void AdminAdminForm_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Add("Пользователи");
            comboBox1.Items.Add("Роли");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button3.Enabled = true;
        }
    }
}
