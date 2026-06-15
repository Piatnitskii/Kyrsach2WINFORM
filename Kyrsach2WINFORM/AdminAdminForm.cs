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
        public AdminAdminForm(bool isAdmin)
        {
            InitializeComponent();

            this.isAdmin = isAdmin;

            // Настройка диалога
            openFileDialog1.Filter = "CSV - файлы(*.csv) | *.csv";
            openFileDialog1.Title = "Выберите файл для импорта";
            openFileDialog1.FileName = "";

            openFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

            openFileDialog1.CheckPathExists = true;
            openFileDialog1.CheckFileExists = true;

        }

        bool isAdmin = false;

        // Импортирование данных
        private void importData_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                    return;


                string filename = openFileDialog1.FileName;                     //Имя файла


                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    Con.Open();

                    var bulkLoader = new MySqlBulkLoader(Con)
                    {
                        Local = true,
                        TableName = comboBox1.SelectedItem.ToString(),
                        FieldTerminator = ";",
                        FileName = filename,
                        LineTerminator = "\n",
                        NumberOfLinesToSkip = 1, // Заголовок уже пропущен
                    };

                    int count = bulkLoader.Load();

                    MessageBox.Show($"Импортированно записей: {count}", "Импорт завершён", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Unknown database"))
                    MessageBox.Show("Отсутствует необходимая База данных!", "Ошибка импорта", MessageBoxButtons.OK);
                else if (ex.Message.Contains("Cannot add or update a child row"))
                    MessageBox.Show($"Отсутствуют необходимые данные во второстепенной таблице: {ex.Message}", "Ошибка импорта", MessageBoxButtons.OK);
                else
                    MessageBox.Show($"Не удалось импортировать данные: {ex.Message}", "Ошибка Импорта", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Добавляем таблицы
        private void AdminAdminForm_Load(object sender, EventArgs e)
        {
            try
            {
                List<string> tables = new List<string>();

                tables.Add("не выбрано");

                using (MySqlConnection conn = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("SHOW TABLES;", conn);
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            tables.Add(dr.GetValue(0).ToString());
                        }
                    }
                }
                
                comboBox1.DataSource = tables;
                comboBox1.SelectedItem = "не выбрано";
            }
            catch (Exception exc)
            {
                MessageBox.Show($"Не удалось загрузить таблицы\nОшибка: {exc.Message}", "Предупреждение-ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            switch (comboBox1.SelectedItem)
            {
                case "не выбрано":
                    dataGridView1.DataSource = null;
                    button1.Enabled = false;
                    button2.Enabled = false;
                    break;
                default:
                    FillData(comboBox1.SelectedItem.ToString());
                    button1.Enabled = true;
                    button2.Enabled = true;
                    break;
            }
        }

        private void FillData(string table)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    conn.Open();
                    
                    MySqlCommand cmd = new MySqlCommand($"show columns from `{table}`;", conn);
                    DataTable dt = new DataTable();

                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            dt.Columns.Add(dr.GetValue(0).ToString());
                        }
                    }
                    dataGridView1.DataSource = dt.AsDataView();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show($"Не удалось загрузить таблицы: {exc.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //ЭКСПОРТИРОВАНИЕ
        private void exportData_Click(object sender, EventArgs e)
        {
            try
            {
              
                        // 1. Открываем диалог выбора пути сохранения
                        using (var saveFileDialog = new SaveFileDialog())
                        {
                            saveFileDialog.Title = "Выберите файл для экспорта";
                            saveFileDialog.Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*";
                            saveFileDialog.DefaultExt = "csv";
                            saveFileDialog.AddExtension = true;

                            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                                return;

                            // 2. Загружаем данные в DataTable
                            var dataTable = new DataTable();
                            using (var da = new MySqlDataAdapter($"SELECT * FROM `{comboBox1.SelectedItem.ToString()}`", ConnectAndData.Сonnect))
                            {
                                da.Fill(dataTable);
                            }

                            // 3. Пишем в выбранный файл через StreamWriter
                            WriteMyData(saveFileDialog, dataTable);

                            MessageBox.Show($"Файл с данными успешно сохранён в:\n{saveFileDialog.FileName}",
                                            "Экспорт завершён", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    writer.Write(dataTable.Columns[i].ColumnName);
                    if (i < dataTable.Columns.Count - 1)
                        writer.Write(";");
                }
                writer.WriteLine();

                // Данные
                foreach (DataRow row in dataTable.Rows)
                {
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        object val = row[i];
                        string s;

                        // Обработка NULL
                        if (val == DBNull.Value)
                        {
                            s = ""; 
                        }
                        else if (val is DateTime date)
                        {
                            s = date.ToString("yyyy-MM-dd HH:mm");  // формат даты
                        }
                        else if (val is byte[])
                        {
                            s = "";  // бинарные данные (например, картинки) → пропускаем
                            continue;  // не записываем в CSV
                        }
                        else
                        {
                            s = val.ToString();  // остальные типы → строка
                        }

                        // Удаление лишнего `\r`
                        s = s.Replace("\r", "");

                        // экранирование через кавычки 
                        s = s.Replace("\"", "\"\"");  // экранирование кавычек
                        if (s.Contains(";") || s.Contains("\n"))
                            s = $"\"{s}\"";  // обёртка в кавычки

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

        //Закрыть
        private void button5_Click(object sender, EventArgs e)
        {
            MenuAdmin.DisableButton();
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string File;
                OpenFileDialog openFileDialog2 = new OpenFileDialog();
                openFileDialog2.Filter = "SQL - файлы(*.sql) | *.sql";
                openFileDialog2.Title = "Выберите файл для восстановления базы";
                openFileDialog2.FileName = "";

                openFileDialog2.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

                openFileDialog2.CheckPathExists = true;
                openFileDialog2.CheckFileExists = true;
                if (openFileDialog2.ShowDialog() == DialogResult.Cancel)
                    return;

                string filename = openFileDialog2.FileName;

                using (StreamReader Reader = new StreamReader(filename))
                {
                    File = Reader.ReadToEnd();
                }

                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.TryConnect))
                {
                    Con.Open();

                    MySqlCommand cmd = new MySqlCommand($"{File}", Con);
                    cmd.ExecuteNonQuery();
                }
                if (isAdmin)
                {
                    MessageBox.Show("База данных успешно восстановлена, сейчас будет выполен переход на форму авторизации", "Операция восстановления базы данных", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                    Application.OpenForms["MenuAdmin"]?.Close();
                }
                else
                    MessageBox.Show("База данных успешно восстановлена", "Операция восстановления базы данных", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось восстановить базу данных: {ex.Message}", "Ошибка Чтение или Записи", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> tables = new List<string>();

                tables.Add("не выбрано");

                using (MySqlConnection conn = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("SHOW TABLES;", conn);
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            tables.Add(dr.GetValue(0).ToString());
                        }
                    }
                }

                comboBox1.DataSource = tables;
                comboBox1.SelectedItem = "не выбрано";
            }
            catch (Exception exc)
            {
                MessageBox.Show($"Не удалось загрузить таблицы\nОшибка: {exc.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
