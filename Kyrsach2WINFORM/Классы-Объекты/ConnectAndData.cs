using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// В ссылки
using System.Configuration;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Kyrsach2WINFORM
{
    static public class ConnectAndData
    {
        // Блок ДАННЫХ
        public static string ID = "";
        public static string NameUser = "";
        public static string SurnameUser = "";
        public static string PatronymicUser = "";
        public static string Role = "";



        // Блок ПОДКЛЮЧЕНИЯ
        static string host = ConfigurationManager.AppSettings["HostName"];
        static string User = ConfigurationManager.AppSettings["UserName"];
        static string Password = ConfigurationManager.AppSettings["Password"];

        //Соединяемся с базой
        public static string Сonnect = $"host={host};uid={User};pwd={Password};database={ConfigurationManager.AppSettings["DbName"]}";
        //Попытка подключения
        public static string TryConnect = $"host={host};uid={User};pwd={Password};";

        //При смене настроек обновляем переменные
        public static void Update()
        {
            host = ConfigurationManager.AppSettings["HostName"];
            User = ConfigurationManager.AppSettings["UserName"];
            Password = ConfigurationManager.AppSettings["Password"];

            Сonnect = $"host={host};uid={User};pwd={Password};database={ConfigurationManager.AppSettings["DbName"]};";
            TryConnect = $"host={host};uid={User};pwd={Password};";
        }




        // Создание папки (автоматический режим)
        public static string CreateFolder()
        {
            string NameDirectory = Directory.GetCurrentDirectory() + @"\Dump" + DateTime.Now.ToString().Replace(":", "-");
            NameDirectory = NameDirectory.Replace(" ", "-");
            Directory.CreateDirectory(NameDirectory);

            return NameDirectory;
        }

        // Функция создание бэкапа
        public static void BackUpCopy(string Mode)
        {
            try
            {
                string FolderToFile = "";

                //Формируем Путь + Имя SQL файлика
                if (Mode == "Hand")
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "SQL Files (*.sql)|*.sql|All Files (*.*)|*.*";
                    saveFileDialog.Title = "Выберите место для сохранения резервной копии";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        FolderToFile = saveFileDialog.FileName; // выбранный путь для сохранения
                    }
                    else
                        return;
                }
                else
                    FolderToFile = CreateFolder() + "\\backup.sql";

                // Настройки подключения
                string mysqlDumpPath = Directory.GetCurrentDirectory() + @"\MysqlDump\mysqldump.exe";   // путь к mysqldump
                string databaseName = ConfigurationManager.AppSettings["DbName"];
                string user = User;
                string password = Password;
                string backupPath = FolderToFile;                                                       // путь до файла

                string args = $"--user={user} --host={host} --password={password} {databaseName} -r \"{backupPath}\"";

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = mysqlDumpPath,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true, //
                };

                using (Process process = Process.Start(psi))
                {
                    //string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (process.ExitCode == 0)
                        MessageBox.Show("Резервная копия успешно создана!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show($"Ошибка в операции создания Бэкапа {error}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
