using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kyrsach2WINFORM
{
    public partial class RedactEmp : Form
    {
        //Fields

        Emploey emploey;                    // Выбранный работник

        string ProjectFolderPath;           // Путь до папки проекта

        string MainImagePath;              // Исходный путь до фото (Это нетрогаем)
        string FullPathToNewPhoto;         // Путь до фото
        bool PhotoRedact = false;          // Было ли изменение фото

        public RedactEmp(Emploey emploey)
        {
            InitializeComponent();
            this.emploey = emploey;

            //Заполнение комбобокса Должностей
            PullData();

            // Минимальный возраст сотрудника 18 лет
            dateTimePicker1.MaxDate = DateTime.Now.AddYears(-18);

            
            // Путь до корня проекта
            ProjectFolderPath = Directory.GetCurrentDirectory();

            if (ProjectFolderPath.Contains("bin\\Debug") || ProjectFolderPath.Contains("bin\\Release"))
                ProjectFolderPath = string.Join("\\", ProjectFolderPath.Split('\\').TakeWhile(el => el != "bin"));

            //Настройка Диалога выбора файла
            openFileDialog1.Filter = "JPG - файлы(*.jpg) | *.jpg|PNG - файлы(*.png) | *.png";
            openFileDialog1.FileName = "";
            openFileDialog1.InitialDirectory = ProjectFolderPath;

            openFileDialog1.CheckPathExists = true;
            openFileDialog1.CheckFileExists = true;

            // Подставляем соответсвующую фотку
            if (emploey.Photo.Trim() == "" || emploey.Photo == "picture.png")
            {
                MainImagePath = $@"{ProjectFolderPath}\photo\picture.png";
                emploey.Photo = "picture.png";
            }
            else
                MainImagePath = $@"{ProjectFolderPath}\photo\" + emploey.Photo;


            //Отображаем фотографию
            pictureBox1.ImageLocation = MainImagePath;     // 54_СергейП.Ю.png

            //Подставляем остальные данные
            textBox1.Text = emploey.Surname;
            textBox2.Text = emploey.Name;
            textBox3.Text = emploey.Patronymic;
            comboBox1.SelectedValue = emploey.Id_Post;
            dateTimePicker1.Value = DateTime.Parse(emploey.Birthday);
            maskedTextBox1.Text = emploey.Phone.Remove(0,1);

            //Проверяем данные
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

        //Проверка заполнености обязательных полей
        void CheckData()
        {

            if ((comboBox1.SelectedValue.ToString() != emploey.Id_Post || PhotoRedact == true || textBox1.Text != emploey.Surname || textBox2.Text != emploey.Name || textBox3.Text != emploey.Patronymic || maskedTextBox1.Text.Replace("+", "").Replace("(", "").Replace(")", "").Replace("-", "") != emploey.Phone || dateTimePicker1.Value != DateTime.Parse(emploey.Birthday)) && (maskedTextBox1.Text.Length == 16 && textBox1.Text.Trim() != "" && textBox2.Text.Trim() != ""))
                button1.Enabled = true;
            else
                button1.Enabled = false;
        }

        // Кликаем по фото
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

                FullPathToNewPhoto = openFileDialog1.FileName;   // путь

                // отображаем текущую картинку
                pictureBox1.ImageLocation = FullPathToNewPhoto;
                PhotoRedact = true;
                CheckData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Сохраняем новое фото
        void SafePhoto(string NewFileName)
        {
            string PhotoName = DateTime.Now.ToString("G").Replace(" ", "").Replace(":", "") + "_" + emploey.Photo;    //Составляем новое имя -> 15062006132533_54_СергейП.Ю.png
            string newOldPhotoPath = Path.Combine($@"{ProjectFolderPath}/photo/", PhotoName);

            //Проверяем, вдруг выбрали файл из нашей файловой базы данных, который установлен в данный момент, и если это не заглушка
            if (MainImagePath == FullPathToNewPhoto && PhotoRedact && MainImagePath != $@"{ProjectFolderPath}\photo\picture.png")
                return; // никаких манипуляций

            //переименовываем старый файл, разумеется если была замена и если это не заглушка
            if (MainImagePath != $@"{ProjectFolderPath}\photo\picture.png" && PhotoRedact)
                File.Move(MainImagePath, newOldPhotoPath);
            
            //Копируем, если это не заглушка из нашего хранилища и картинка была выбрана
            if (PhotoRedact && FullPathToNewPhoto != $@"{ProjectFolderPath}\photo\picture.png")
            {
                // Если произошла непредвиденная ошибка, и файл каким то чудом есть уже в папке
                if (File.Exists($@"{ProjectFolderPath}/photo/" + NewFileName))
                    File.Delete($@"{ProjectFolderPath}/photo/" + NewFileName);

                File.Copy(FullPathToNewPhoto, $@"{ProjectFolderPath}/photo/" + NewFileName);     // 54_СергейП.Ю.png
            }
        }

        // Проверка на дубликат True - если нет дубликата
        bool CheckEmploey(string NumberPhone)
        {
            string CMD = $"Select * FROM Employe WHERE Phone = '{NumberPhone}' AND IdEmploye != '{emploey.IdEmploey}'";

            using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
            {
                Con.Open();
                MySqlCommand cmd = new MySqlCommand(CMD, Con);
                bool Result = cmd.ExecuteScalar() == null;

                return Result;
            }
        }

        //Формируем новое имя фотки
        string SayMyFileName( string Name, string Surname, string Patronymic)
        {
            string NewFileName = "picture.png";

            //Проверяем  была ли замена и не явлвяется ли выбранная фотка нашей заглушкой
            if (PhotoRedact && FullPathToNewPhoto != $@"{ProjectFolderPath}\photo\picture.png")
            {
                NewFileName = emploey.IdEmploey + "_" + Name + Surname[0] + '.';

                if (Patronymic != null || Patronymic.Trim() != "")
                    NewFileName += Patronymic[0];

                string FormatFile = (new FileInfo(MainImagePath)).Extension;

                NewFileName += FormatFile;
            }

            return NewFileName; // 54_СергейП.Ю.png
        }


        //Изменить
        private void redactEmploey_Click(object sender, EventArgs e)
        {
            try
            {
                //Заполняем поля
                string Name = textBox2.Text.ToString();
                string Surname = textBox1.Text.ToString();
                string Patronymic = textBox3.Text.ToString();
                string NumberPhone = maskedTextBox1.Text.Replace("+", "").Replace("(", "").Replace(")", "").Replace("-", "");
                string Date = dateTimePicker1.Value.ToString("yyyy-MM-dd");
                string Id_Post = comboBox1.SelectedValue.ToString();

                //Формируем новое имя фотки, если выбрали
                string NewFileName = SayMyFileName(Name, Surname, Patronymic);
                string CMD = $"UPDATE Employe SET Name ='{Name}', Surname = '{Surname}', Patronymic = '{Patronymic}', Phone = '{NumberPhone}', Birthday = '{Date}', Id_Post = '{Id_Post}', Photo = '{NewFileName}' WHERE IdEmploye = '{emploey.IdEmploey}';";

                DialogResult dialogResult = MessageBox.Show("Изменить данные сотрудника?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    if (textBox3.Text.Trim() == "")
                    {
                        DialogResult dialogResultTwo = MessageBox.Show("Оставить поле 'Отчество' пустым?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (!(dialogResultTwo == DialogResult.Yes))
                            return;
                    }

                    //Проверяем на дубликат
                    if (!CheckEmploey(NumberPhone))
                    {
                        MessageBox.Show("Работник, с этим номером телефона уже существует в базе", "Ошибка операции", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Clear();    // Очистка
                        return;
                    }

                    //Сохраняем фото
                    SafePhoto(NewFileName);

                    //Редактируем строку
                    using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                    {
                        Con.Open();
                        MySqlCommand cmd = new MySqlCommand(CMD, Con);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Сотрудник был успешно обновлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                    return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            CheckData();
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckData();
        }

        //Фамилия //Имя //Отчество
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
        

        //Настройка телефона
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


        //Закрыть
        private void button8_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
