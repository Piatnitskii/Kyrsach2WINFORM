using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


//
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Drawing.Drawing2D;

namespace Kyrsach2WINFORM
{
    public partial class Auth : Form
    {

        //Fields
        bool Status = false;


        public Auth()
        {
            InitializeComponent();

            //Скругляем рамки окна
            GraphicsPath graphicsPath = new GraphicsPath();

            int cornerRadius = 10;

            graphicsPath.AddArc(0, 0, cornerRadius, cornerRadius, 180, 90);
            graphicsPath.AddArc(this.Width - cornerRadius, 0, cornerRadius, cornerRadius, 270, 90);
            graphicsPath.AddArc(this.Width - cornerRadius, this.Height -  cornerRadius, cornerRadius, cornerRadius, 0, 90);
            graphicsPath.AddArc(0, this.Height - cornerRadius,  cornerRadius, cornerRadius, 90, 90);

            Region roundedRegion = new Region(graphicsPath);
            this.Region = roundedRegion;
        }

        //Возвращает ХЕШ переданной строки
        string GetHash(string password)
        {
            var sha256 = SHA256.Create();
            var sha256byte = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

            string hash = BitConverter.ToString(sha256byte).Replace("-", "");
            return hash;
        }

        //Очищает Поля
        void Clear()
        {
            textBox1.Text = "";
            textBox2.Text = "";

            if (Status)
            {
                label1.Text = "Введен неверный Логин или Пароль";
                label1.Visible = true;
            }
        }

        // Кнопка ВОЙТИ
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //Проверяем заполненость полей
                if(textBox1.Text.Trim() == "" || textBox2.Text.Trim() == "")
                {
                    label1.Text = "Не заполнено одно из полей!";
                    label1.Visible = true;
                    return;
                }

                string Login = textBox1.Text;
                string Password = textBox2.Text;

                //Забираем данные из базы в виртуальную таблицу
                DataTable dt = null;
                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    Con.Open();

                    MySqlCommand cmd = new MySqlCommand( $"Select IdUser, Name, Surname, Patronymic, Password, Login, Id_Role from User WHERE Login='{Login}'", Con);
                    cmd.ExecuteNonQuery();

                    MySqlDataAdapter ad = new MySqlDataAdapter(cmd);
                    dt = new DataTable();

                    ad.Fill(dt);
                }

                // Нет такого пользователя/ПРОВЕРКА логина
                if (dt.Rows.Count == 0)
                {
                    //Активируем капчу
                    //MessageBox.Show("Введен неверный Логин или Пароль", "Ошибка доступа, пользователь отсутствует", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Status = true;
                    Clear();
                    return;
                }

                string user = dt.Rows[0].ItemArray.GetValue(6).ToString();

                //ПРОВЕРКА пароля
                if (GetHash(textBox2.Text) == dt.Rows[0].ItemArray.GetValue(4).ToString())
                {
                    this.Visible = false;

                    //СОБИРАЕМ нужные данные
                    ConnectAndData.ID = dt.Rows[0].ItemArray.GetValue(0).ToString();
                    ConnectAndData.NameUser = dt.Rows[0].ItemArray.GetValue(1).ToString();
                    ConnectAndData.SurnameUser = dt.Rows[0].ItemArray.GetValue(2).ToString();
                    ConnectAndData.PatronymicUser = dt.Rows[0].ItemArray.GetValue(3).ToString();
                    ConnectAndData.Role = dt.Rows[0].ItemArray.GetValue(6).ToString();

                    string FIO = ConnectAndData.NameUser + " " + ConnectAndData.SurnameUser + " " + ConnectAndData.PatronymicUser;

                    MenuAdmin FormB = new MenuAdmin();
                    FormB.ShowDialog();
                    

                    Clear();
                    Status = false;
                    label1.Visible = false;
                    this.Visible = true;
                }
                else //НЕВЕРНЫЙ пароль
                {
                    //Активируем капчу
                    //MessageBox.Show("Введен неверный Логин или Пароль", "Ошибка доступа, пользователь отсутствует", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Status = true;
                    Clear();
                    return;
                }
            }
            catch (Exception ex)
            {
                Clear();                                                                                                   // Неверный хост, просим изменить настройку подключения
                if (ex.Message.Contains("Unable to connect to any of the specified MySQL hosts"))
                {
                    MessageBox.Show("Невозможно установить соединение с указанным хостом!", "Ошибка доступа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }                                                                                                           // Нету базы
                else if (ex.Message.Contains("Unknown database"))
                {
                    MessageBox.Show("Отсутствует необходимая База данных!", "Ошибка доступа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                } 
                else if (ex.Message.Contains("Table"))                                                                      // Нету таблицы
                {
                    MessageBox.Show("Одна из таблиц базы сломана!", "Ошибка доступа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else                                                                                                        // что то нето
                {
                    MessageBox.Show(ex.Message, "Ошибка доступа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }
        }

        // Кнопка ВЫЙТИ
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }



        ///////////////////// Логика перемещения за панельку
        private bool issDragging = false;
        private int mouseX, mouseY;

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if( e.Button == MouseButtons.Left)
            {
                issDragging = true;
                mouseX = e.X;
                mouseY = e.Y;
            }
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (issDragging)
            {
                this.Left += e.X - mouseX;
                this.Top += e.Y - mouseY;
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                issDragging = false;
            }
        }


        //Закрыть
        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Вы уверены что хотите выйти из программы?", "Выход из приложения", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                this.Close();
            }
            
        }
    }
}
