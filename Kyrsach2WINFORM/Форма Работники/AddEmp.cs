using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kyrsach2WINFORM
{
    public partial class AddEmp : Form
    {
        //Fields
        string ProjectFolderPath;       // путь до папки проекта

        string MainImagePath;           //путь до фото
        bool PhotoAdd = false;          //было ли изменение фото

        string EmploeyId;

        public AddEmp()
        {
            InitializeComponent();

            //Заполнение комбобокса Должностей
            PullData();

            // Минимальный возраст сотрудника 18 лет
            dateTimePicker1.MaxDate = DateTime.Now.AddYears(-18);


            // Получаем путь до корня проекта
            ProjectFolderPath = Directory.GetCurrentDirectory();

            if (ProjectFolderPath.Contains("bin\\Debug") || ProjectFolderPath.Contains("bin\\Release"))
                ProjectFolderPath = string.Join("\\", ProjectFolderPath.Split('\\').TakeWhile(el => el != "bin"));

            //Настройка Диалога выбора файла
            openFileDialog1.Filter = "JPG - файлы(*.jpg) | *.jpg|PNG - файлы(*.png) | *.png";
            openFileDialog1.FileName = "picture.png";
            openFileDialog1.InitialDirectory = ProjectFolderPath;

            openFileDialog1.CheckPathExists = true;
            openFileDialog1.CheckFileExists = true;


            //Фото по умолчанию - заглушка
            pictureBox1.ImageLocation = $@"{ProjectFolderPath}/photo/picture.png";

            //Устанавливаем значения по дефолту
            MainImagePath = $@"{ProjectFolderPath}/photo/picture.png";

            //Контрольная проверка
            CheckData();
        }

        //Заполнение комбобокса Должностями
        void PullData()
        {
            try
            {
                string CMD = "SELECT * FROM Post;";
                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    Con.Open();

                    MySqlCommand cmd = new MySqlCommand(CMD, Con);
                    cmd.ExecuteNonQuery();

                    DataTable Dt = new DataTable();
                    MySqlDataAdapter Ad = new MySqlDataAdapter(cmd);

                    Ad.Fill(Dt);

                    comboBox1.ValueMember = "IdPost";
                    comboBox1.DisplayMember = "Name";
                    comboBox1.DataSource = Dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Проверка на дубликат True - если нет дубликата
        bool CheckEmploey(string NumberPhone)
        {
            string CMD = $"Select * FROM Employe WHERE Phone = '{NumberPhone}'";

                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    Con.Open();
                    MySqlCommand cmd = new MySqlCommand(CMD, Con);
                    bool Result = cmd.ExecuteScalar() == null;

                    return Result;
                }
        }

        //Проверка заполнености обязательных полей
        void CheckData()
        {
            if (textBox1.Text.Trim() != "" && textBox2.Text.Trim() != "" && maskedTextBox1.Text.Length == 16 )
                button1.Enabled = true;
            else
                button1.Enabled = false;
        }
       

        // Тыкаем по фото
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                    return;

                // ограничение 2 мб
                if (new System.IO.FileInfo(openFileDialog1.FileName).Length > 2097152)
                {
                    MessageBox.Show("Размер выбранного изображения не должен превышать 2 мб", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MainImagePath = openFileDialog1.FileName;   //путь

                // отображаем текущую картинку
                pictureBox1.ImageLocation = MainImagePath;

                PhotoAdd = true; // Произошло изменение фото
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        //Формируем новое имя фотки 
        string SayMyFileName(string EmploeyId, string Name, string Surname, string Patronymic)
        {
            string NewFileName = "picture.png";     // По дефолту заглушка
                            
            //Проверяем  была ли замена и не явлвяется ли выбранная фотка нашей заглушкой
            if (PhotoAdd && MainImagePath != $@"{ProjectFolderPath}\photo\picture.png")
            {
                 NewFileName = EmploeyId + "_" + Name + Surname[0] + '.';

                if (Patronymic != null || Patronymic.Trim() != "")
                    NewFileName += Patronymic[0];

                string FormatFile = (new FileInfo(MainImagePath)).Extension;

                NewFileName += FormatFile;

                // Если произошла непредвиденная ошибка, и файл каким то чудом есть уже в папке
                if (File.Exists($@"{ProjectFolderPath}/photo/" + NewFileName))
                    File.Delete($@"{ProjectFolderPath}/photo/" + NewFileName);
                    
                File.Copy(MainImagePath, $@"{ProjectFolderPath}/photo/" + NewFileName.Trim());   // 54_СергейП.Ю.png
            }

            return NewFileName; // 54_СергейП.Ю.png || picture.png
        }


        //Добавить
        private void addEmploey_Click(object sender, EventArgs e)
        {
            string Name = textBox2.Text.ToString();
            string Surname = textBox1.Text.ToString();
            string Patronymic = textBox3.Text.ToString();
            string NumberPhone = maskedTextBox1.Text.Replace("+", "").Replace("(", "").Replace(")", "").Replace("-", "");
            string Date = dateTimePicker1.Value.ToString("yyyy-MM-dd");
            string Id_Post = comboBox1.SelectedValue.ToString();

            string NewFileName = "";

            string CMD = $"INSERT INTO Employe (Name, Surname, Patronymic, Phone, Birthday, Id_Post, Photo) VALUES ('{Name}', '{Surname}', '{Patronymic}',  '{NumberPhone}', '{Date}', '{Id_Post}',";
            try
            {
                DialogResult dialogResult = MessageBox.Show("Добавить работника?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    if (textBox3.Text.Trim() == "")
                    {
                        DialogResult dialogResultTwo = MessageBox.Show("Оставить поле 'Отчество' пустым?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dialogResultTwo == DialogResult.Yes)
                            CMD = $"INSERT INTO Employe (Name, Surname, Phone, Birthday, Id_Post, Photo) VALUES ('{Name}', '{Surname}', '{NumberPhone}', '{Date}', '{Id_Post}', "; 
                        else
                            return;
                    }

                    //Проверяем на дубликат
                    if (!CheckEmploey(NumberPhone))
                    {
                        MessageBox.Show("Данный работник уже существует в базе", "Ошибка операции", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Clear();    // Очистка
                        return;
                    }

                    //Добавляем строку
                    using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                    {
                        Con.Open();
                        
                        // 1. Получаем максимальный ID  ///////////////////////////////////!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        MySqlCommand cmd = new MySqlCommand(@"SELECT `AUTO_INCREMENT` FROM  INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BarBer' AND TABLE_NAME = 'Employe'; ", Con);
                        EmploeyId = cmd.ExecuteScalar().ToString();

                        // 1.5 Сохраняем фото
                        NewFileName = SayMyFileName(EmploeyId, Name, Surname, Patronymic);

                        // 2. Добавляем в таблицу нашего сотрудника
                        cmd.CommandText = CMD +  $" '{NewFileName}'); ";
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Работник был успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Clear();    // Очистка
                }
                else
                    return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Удаляем фотку
                if (PhotoAdd && MainImagePath != $@"{ProjectFolderPath}\photo\picture.png" && File.Exists($@"{ProjectFolderPath}/photo/" + NewFileName.Trim()))
                {
                    File.Delete($@"{ProjectFolderPath}/photo/" + NewFileName.Trim());
                    MessageBox.Show($@"Выбранные фотографии были удалены из конечной папки {ProjectFolderPath}\photo\", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }
        //Чистим все элементы
        void Clear()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            maskedTextBox1.Text = "";
            pictureBox1.ImageLocation = $@"{ProjectFolderPath}/photo/picture.png";
        }


        #region Настройка полей

        //Фамилия //Отчество  //Имя
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar) || (e.KeyChar >= 'a' && e.KeyChar <= 'z') || (e.KeyChar >= 'A' && e.KeyChar <= 'Z'))
            {
                e.Handled = true;
            }
            else { e.Handled = false; }
        }

        //Фамилия
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Chars(textBox1.Text, textBox1);
            CheckData();
        }
        //Имя
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Chars(textBox2.Text, textBox2);
            CheckData();
        }
        //Отчество
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            Chars(textBox3.Text, textBox3);
            CheckData();
        }
        // Настройка Телефона
        private void maskedTextBox1_Click(object sender, EventArgs e)
        {

            int position = maskedTextBox1.SelectionStart;

            var Array = maskedTextBox1.Text.ToCharArray();
            var Count = 3;

            for (int i = 3; i < Array.Length; i++)
            {
                if (Array[i] == ' ')
                    break;
                Count++;
            }

            // Проверяем символ в этом месте
            if (position < maskedTextBox1.Text.Length)
            {
                char ch = maskedTextBox1.Text[position];

                if (char.IsDigit(maskedTextBox1.Text[position]) && position > 3)
                {
                    maskedTextBox1.SelectionStart = position;
                }
                else if (ch == ' ')
                {
                    maskedTextBox1.Select(Count, 0);
                }
                else if (ch == ')' && char.IsDigit(maskedTextBox1.Text[position - 1]))
                {
                    maskedTextBox1.SelectionStart = position;
                }
                else if (ch == '-' && char.IsDigit(maskedTextBox1.Text[position - 1]))
                {
                    maskedTextBox1.SelectionStart = position;
                }
                else
                {
                    maskedTextBox1.Select(Count, 0);
                }
            }
            else
                maskedTextBox1.Select(Count, 0);
        }

        //Телефон
        private void maskedTextBox1_TextChanged(object sender, EventArgs e)
        {
            CheckData();
        }


        //Логика - Первая буква ЗАГЛАВНАЯ
        void Chars(string Text, TextBox Element)
        {
            int selectionStart = Element.SelectionStart;
            int selectionLength = Element.SelectionLength;

            ArrayList chars = new ArrayList();
            string result = "";

            if (!string.IsNullOrEmpty(Text))
            {

                for (int i = 0; i < Text.Length; i++)
                {
                    if (i == 0)
                        chars.Add(Char.ToUpper(Text[0]));               // Добавляем первую букву в верхнем регистре
                    else
                        chars.Add(Char.ToLower(Text[i]));               // Добавляем остальные символы без изменений
                }

                foreach (var item in chars)
                {
                    result += item.ToString();
                }
            }

            Element.Text = result;
            Element.SelectionStart = selectionStart;
            Element.SelectionLength = selectionLength;
        }


        #endregion


        //закрыть
        private void button8_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
