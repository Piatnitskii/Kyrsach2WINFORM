using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// В ссылки
using System.Configuration;
using System.Windows.Forms;

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
    }
}
