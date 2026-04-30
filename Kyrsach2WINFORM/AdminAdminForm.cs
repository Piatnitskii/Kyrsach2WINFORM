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


using System.Collections;

using MySql.Data.MySqlClient;

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


        // Проверка csv
        private bool checkData(string[] text, int oneValue)
        {
            // Проверка количества колонок в импортируемом файле
            for (int i = 1; i < text.Length; i++)
            {
                string[] ItemInline = text[i].Split(';');

                if (ItemInline.Length != oneValue)
                    return false;
            }
            return true;
        }
        

        // Импортирование данных
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                    return;

                MySqlTransaction transaction = null;
                string filename = openFileDialog1.FileName;                     //Имя файла
                string[] FullText = File.ReadAllLines(filename, Encoding.UTF8);

                switch (comboBox1.SelectedItem.ToString())
                {
                    case "Категории услуг":

                        //Проверка csv
                        if (checkData(FullText, 2) == false)
                        {
                            MessageBox.Show("Ошибка соответствия необходимого количества колонок с действительным!", "Ошибка импорта", MessageBoxButtons.OK);
                            return;
                        }

                        using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                        {
                            try
                            {
                                Con.Open();
                                // Начало транзакции
                                transaction = Con.BeginTransaction();

                                MySqlCommand cmd;
                                int Count = 0;

                                for (int i = 1; i < FullText.Length; i++)
                                {
                                    string[] ItemInline = FullText[i].Split(';');
                                    cmd = new MySqlCommand($"INSERT INTO Category (Name) VALUES (@name);", Con);
                                    cmd.Transaction = transaction;
                                    cmd.Parameters.AddWithValue("@name", ItemInline[0].Trim());
                                    cmd.ExecuteNonQuery();
                                    Count++;
                                }

                                //Подтверждаем транзакцию
                                transaction.Commit();
                                MessageBox.Show($"Импортированно записей: {Count}", "Импорт завершён", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch(Exception ex)
                            {
                                // Откат транзакции при ошибке
                                transaction?.Rollback();
                                MessageBox.Show(ex.Message, "Ошибка транзакции добавления записей", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }

                        break;


                    case "Услуги":
                        if (checkData(FullText, 5) == false)
                        {
                            MessageBox.Show("Ошибка соответствия необходимого количества колонок с действительным!", "Ошибка импорта", MessageBoxButtons.OK);
                            return;
                        }

                        using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                        {
                            try
                            {
                                Con.Open();
                                // Начало транзакции
                                transaction = Con.BeginTransaction();
                                MySqlCommand cmd;
                                int Count = 0;
                                for (int i = 1; i < FullText.Length; i++)
                                {
                                    string[] ItemInline = FullText[i].Split(';');
                                    cmd = new MySqlCommand($"INSERT INTO Service (Name, Description, Id_Category, Cost, Duration) VALUES (@name, @desc, @id_cat, @cost, @duration);", Con);
                                    cmd.Transaction = transaction;
                                    cmd.Parameters.AddWithValue("@name", ItemInline[0].Trim());
                                    cmd.Parameters.AddWithValue("@desc", ItemInline[1].Trim());
                                    cmd.Parameters.AddWithValue("@id_cat", ItemInline[2].Trim());
                                    cmd.Parameters.AddWithValue("@cost", ItemInline[3].Trim());
                                    cmd.Parameters.AddWithValue("@duration", ItemInline[4].Trim());
                                    cmd.ExecuteNonQuery();
                                    Count++;
                                }

                                //Подтверждаем транзакцию
                                transaction.Commit();
                                MessageBox.Show($"Импортированно записей: {Count}", "Импорт завершён", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch(Exception ex)
                            {
                                // Откат транзакции при ошибке
                                transaction?.Rollback();
                                MessageBox.Show(ex.Message, "Ошибка транзакции добавления записей", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Unknown database"))
                    MessageBox.Show("Отсутствует необходимая База данных!", "Ошибка импорта", MessageBoxButtons.OK);
                else if (ex.Message.Contains("Cannot add or update a child row"))
                    MessageBox.Show($"Отсутствуют необходимые данные во второстепенной таблице: {ex.Message}", "Ошибка импорта", MessageBoxButtons.OK);
                else
                    MessageBox.Show(ex.Message, "Ошибка Импорта", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Добавляем таблицы
        private void AdminAdminForm_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Add("Услуги");
            comboBox1.Items.Add("Категории услуг");
            comboBox2.Items.Add("Услуги");
            comboBox2.Items.Add("Категории услуг");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = true;
        }

        //ЭКСПОРТИРОВАНИЕ
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                switch (comboBox2.SelectedItem.ToString())
                {
                    case "Услуги":
                        // 1. Открываем диалог выбора пути сохранения
                        using (var saveFileDialog = new SaveFileDialog())
                        {
                            saveFileDialog.Title = "Сохранить CSV";
                            saveFileDialog.Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*";
                            saveFileDialog.DefaultExt = "csv";
                            saveFileDialog.AddExtension = true;

                            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                                return;

                            // 2. Загружаем данные в DataTable
                            var dataTable = new DataTable();
                            using (var da = new MySqlDataAdapter("SELECT * FROM Service", ConnectAndData.Сonnect))
                            {
                                da.Fill(dataTable);
                            }

                            // 3. Пишем в выбранный файл через StreamWriter
                            WriteMyData(saveFileDialog, dataTable);

                            MessageBox.Show($"CSV успешно сохранён в:\n{saveFileDialog.FileName}",
                                            "Экспорт завершён", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        break;

                    case "Категории услуг":
                        // 1. Открываем диалог выбора пути сохранения
                        using (var saveFileDialog = new SaveFileDialog())
                        {
                            saveFileDialog.Title = "Сохранить CSV";
                            saveFileDialog.Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*";
                            saveFileDialog.DefaultExt = "csv";
                            saveFileDialog.AddExtension = true;

                            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                                return;

                            // 2. Загружаем данные в DataTable
                            var dataTable = new DataTable();
                            using (var da = new MySqlDataAdapter("SELECT * FROM Category", ConnectAndData.Сonnect))
                            {
                                da.Fill(dataTable);
                            }

                            // 3. Пишем в выбранный файл через StreamWriter
                            WriteMyData(saveFileDialog, dataTable);

                            MessageBox.Show($"CSV успешно сохранён в:\n{saveFileDialog.FileName}",
                                            "Экспорт завершён", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        break;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}",
                       "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Записывает данные в файл csv
        private void WriteMyData(SaveFileDialog saveFileDialog, DataTable dataTable)
        {
            using (var writer = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
            {
                // Заголовок (по именам колонок из DataTable)
                for (int i = 1; i < dataTable.Columns.Count; i++)
                {
                    writer.Write(dataTable.Columns[i].ColumnName);
                    if (i < dataTable.Columns.Count - 1)
                        writer.Write(";");
                }
                writer.WriteLine();

                // Данные
                foreach (DataRow row in dataTable.Rows)
                {
                    for (int i = 1; i < dataTable.Columns.Count; i++)
                    {
                        object val = row[i];
                        string s = val == DBNull.Value ? "" : val.ToString();

                        // Экранирование ';' и кавычек при необходимости
                        s = s.Replace(";", " ");
                        writer.Write(s);

                        if (i < dataTable.Columns.Count - 1)
                            writer.Write(";");
                    }
                    writer.WriteLine();
                }
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            button2.Enabled = true;
        }
    }
}
