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
            DisableCaptcha();
            Size_Min();
        }

        //Скругляем рамки окна
        void RoundShape()
        {
            GraphicsPath graphicsPath = new GraphicsPath();

            int cornerRadius = 10;

            graphicsPath.AddArc(0, 0, cornerRadius, cornerRadius, 180, 90);
            graphicsPath.AddArc(this.Width - cornerRadius, 0, cornerRadius, cornerRadius, 270, 90);
            graphicsPath.AddArc(this.Width - cornerRadius, this.Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            graphicsPath.AddArc(0, this.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);

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
            textBox3.Text = "";
            label1.Visible = false;

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

                // Проверяем капчу
                if (ResultAvtorize)
                {
                    //капча пустая - выходим
                    if (textBox3.Text == "")
                    {
                        label1.Text = "Не заполнено одно из полей!";
                        return;
                    }
                    else if (!(textBox3.Text == Text)) // капча неверная - выводим сообщение о блокировке
                    {
                        MessageBox.Show($"Введена неверная Captcha, форма будет заблокирована на 10 секунд!", "Ошибка авторизации", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        Status = false;
                        Clear();
                        Effects();
                        return;
                    }
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
                    Status = false;
                    Clear();
                    //Активируем капчу
                    if (ResultAvtorize == false)
                    {
                        ResultAvtorize = true;
                        MessageBox.Show($"Введен не правильный логин или пароль, для продолжения вам необходимо пройти CAPTCHA", "Ошибка авторизации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ActiveCaptcha();
                    }
                    else
                    {
                        Effects();
                    }
                    
                    
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
                    DisableCaptcha();
                    Status = false;
                    label1.Visible = false;
                    this.Visible = true;
                }
                else //НЕВЕРНЫЙ пароль
                {
                    //Активируем капчу
                    Status = false;
                    Clear();

                    //Активируем капчу
                    if (ResultAvtorize == false)
                    {
                        ResultAvtorize = true;
                        MessageBox.Show($"Введен не правильный логин или пароль, для продолжения вам необходимо пройти CAPTCHA", "Ошибка авторизации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ActiveCaptcha();
                    }
                    else
                    {
                        Effects();
                    }
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

        // БЛОК РАБОТЫ С CAPTCHA

        // Для Капчи
        bool Result = true;
        string Text;
        // не прошли авторизацию - true
        bool ResultAvtorize = false;


        // Функцция для формирования изображения
        //Делает картинку
        Bitmap CreateImage(int W, int H)
        {
            // ОЧИЩАЕМ СТРОКУ
            Text = string.Empty;

            //ФОРМИРУЕМ ИЗОБРАЖЕНИЕ
            Random random = new Random();

            Bitmap ImageActual = new Bitmap(W, H);

            Brush[] Colors = { Brushes.Black, Brushes.Red, Brushes.Green, Brushes.Purple };

            Graphics G = Graphics.FromImage((Image)ImageActual);

            G.Clear(Color.Gray);

            string Words = "1234567890QWERTYUIOPASDFGHJKLZXCVBNM";

            for (int i = 0; i < 4; i++)
                Text += Words[random.Next(Words.Length)];

            int X = random.Next(0, W - 90);
            int Y = random.Next(0, H - 90);

            G.DrawString(Text[0].ToString(), new Font("Atial", 15), Colors[random.Next(Colors.Length)], new PointF(X, Y));
            G.DrawString(Text[1].ToString(), new Font("Atial", 15), Colors[random.Next(Colors.Length)], new PointF(X + 17, random.Next(Y - 5, Y + 5)));
            G.DrawString(Text[2].ToString(), new Font("Atial", 15), Colors[random.Next(Colors.Length)], new PointF(X + 33, random.Next(Y - 5, Y + 5)));
            G.DrawString(Text[3].ToString(), new Font("Atial", 15), Colors[random.Next(Colors.Length)], new PointF(X + 50, random.Next(Y - 5, Y + 5)));

            //ПОМЕХИ
            G.DrawLine(Pens.Black, new Point(0, 0), new Point(W - 1, H - 1));
            G.DrawLine(Pens.Black, new Point(0, H - 1), new Point(W - 1, 0));

            G.DrawLine(Pens.Black, new Point(X - 20, Y + 15), new Point(X + 70, Y + 15));

            return ImageActual;
        }

        // Кнопка для изменения изображеня Captcha
        private void button4_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = CreateImage(pictureBox1.Width, pictureBox1.Height);
        }


        // БЛОКИРОВКА
        int ForTimer = 10;

        // Тикет создается таймером 1 раз в опред период
        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = $"Авторизация доступна через {ForTimer--}";

            if (ForTimer == -1)
            {
                timer1.Stop();
                label1.Visible = false;
                ForTimer = 10;

                UnFreez();
            }
        }


        // Блокировка экрана
        private void Effects()
        {
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            textBox3.Enabled = false;

            button1.Enabled = false;
            button4.Enabled = false;

            textBox3.Text = "";
            label1.Text = $"Авторизация доступна через ";
            label1.Visible = true;
            timer1.Start();
        }

        // Показываем капчу
        private void ActiveCaptcha()
        {
            pictureBox1.Image = CreateImage(pictureBox1.Width, pictureBox1.Height);

            pictureBox1.Visible = true;
            textBox3.Enabled = true;
            label5.Visible = true;
            button4.Enabled = true;
            
            Size_Max();
        }
        // Разморозка формы
        private void UnFreez()
        {
            textBox1.Enabled = true;
            textBox2.Enabled = true;
            textBox3.Enabled = true;

            button1.Enabled = true;
            button4.Enabled = true;
        }

        // Убераем капчу
        private void DisableCaptcha()
        {
            textBox1.Enabled = true;
            textBox2.Enabled = true;
            button1.Enabled = true;

            textBox3.Text = "";
            ResultAvtorize = false;
            Size_Min();

        }


        // Делаем форму меньше
        void Size_Min()
        {   // 770; 346
            button3.Location = new Point(392, 4);
            this.Size = new Size(346, this.Size.Height);
            this.Size = new Size(425, this.Size.Width);
            RoundShape();
        }

        // Делаем форму больше
        void Size_Max()
        {   // 770; 346
            button3.Location = new Point(735, 4);
            this.Size = new Size(346, this.Size.Height);
            this.Size = new Size(770, this.Size.Width);
            RoundShape();
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
