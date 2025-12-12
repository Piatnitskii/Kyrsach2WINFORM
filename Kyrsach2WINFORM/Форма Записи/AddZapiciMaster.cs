using MySql.Data.MySqlClient;
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

namespace Kyrsach2WINFORM
{
    public partial class AddZapiciMaster : Form
    {

        //Fields
        Emploey emploey;
        Button button3;
        Button button4;
        Button button5;

        //Работа с фото
        string ProjectFolderPath;
        string MainImagePath;

        public AddZapiciMaster(Control control, Control control4, Control control5, Emploey emploey)
        {
            InitializeComponent();

            this.emploey = emploey;
            this.button3 = (Button)control;
            this.button4 = (Button)control4;
            this.button5 = (Button)control5;

            // Путь до корня проекта
            ProjectFolderPath = Directory.GetCurrentDirectory();

            if (ProjectFolderPath.Contains("bin\\Debug") || ProjectFolderPath.Contains("bin\\Release"))
                ProjectFolderPath = string.Join("\\", ProjectFolderPath.Split('\\').TakeWhile(el => el != "bin"));

            //Заполняем ДатаГрид данными
            FillDataGrid();
        }

        //Отображаем выбранного ранее клиента, если таковой имеется
        void SelectRow()
        {
            if (emploey.CurrentRowIndex != null && emploey.IdEmploey != null)
            {
                dataGridView1.CurrentCell = dataGridView1.Rows[Convert.ToInt32(emploey.CurrentRowIndex)].Cells[1];
                CurrentRowIndex = Convert.ToInt32(emploey.CurrentRowIndex);
               
                // Подставляем соответсвующую фотку
                ConfigurePath();

                label1.Text = emploey.FIO;
                label2.Text = emploey.Phone;
                pictureBox1.ImageLocation = MainImagePath;

                label1.Visible = true;
                label2.Visible = true;
                pictureBox1.Visible = true;
            }
            else
                dataGridView1.ClearSelection();
        }

        void ConfigurePath()
        {
            // Подставляем соответсвующую фотку
            if (emploey.Photo.Trim() == "" || emploey.Photo == "picture.png")
                MainImagePath = $@"{ProjectFolderPath}\photo\picture.png";
            else
                MainImagePath = $@"{ProjectFolderPath}\photo\" + emploey.Photo;
        }

        //Отображает выбранного пользователя на странице, обновляет переменную, блокирует кнопки
        void Update()
        {
            //Сохраняем
            emploey.IdEmploey = dataGridView1.Rows[CurrentRowIndex].Cells[0].Value.ToString();
            emploey.FIO = dataGridView1.Rows[CurrentRowIndex].Cells[1].Value.ToString();
            emploey.Phone = dataGridView1.Rows[CurrentRowIndex].Cells[2].Value.ToString();
            emploey.Photo = dataGridView1.Rows[CurrentRowIndex].Cells[3].Value.ToString();
            emploey.CurrentRowIndex = CurrentRowIndex.ToString();

            // Подставляем соответсвующую фотку
            ConfigurePath();

            // Отображаем
            label1.Text = emploey.FIO;
            label2.Text = emploey.Phone;
            pictureBox1.ImageLocation = MainImagePath;

            label1.Visible = true;
            label2.Visible = true;
            pictureBox1.Visible = true;

            // ЛОГИКА
            AddZapicWiz.CurrentFiveForm = new System.Collections.ArrayList();

            button5.Enabled = false;
            button5.BackColor = Color.FromArgb(150, 116, 102);
            button5.ForeColor = Color.White;

            button4.BackColor = Color.FromArgb(150, 116, 102);
            button4.ForeColor = Color.White;

            button3.Enabled = true;
        }

        int CurrentRowIndex; // Индекс выбранной строки
        // Получаем инфу по выбранной строке ДатаГрида
        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            CurrentRowIndex = e.RowIndex;

            if (CurrentRowIndex == -1)
            {
                return;
            }

            Update();
        }

        //только БАРБЕРЫ
        string CMD = "Select IdEmploye as ID, CONCAT_WS(' ', Employe.Name, Surname, Patronymic) AS 'ФИО', Phone as 'Телефон', Photo  FROM Employe WHERE Id_Post = 2";
        //Заполняет ДатаГрид данными
        void FillDataGrid()
        {
            try
            {
                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    Con.Open();

                    MySqlCommand cmd = new MySqlCommand(CMD, Con);
                    cmd.ExecuteNonQuery();

                    DataTable Dt = new DataTable();
                    MySqlDataAdapter Ad = new MySqlDataAdapter(cmd);

                    Ad.Fill(Dt);

                    dataGridView1.DataSource = Dt;

                    //Настройка полей
                    dataGridView1.Columns["ФИО"].SortMode = DataGridViewColumnSortMode.NotSortable;
                    dataGridView1.Columns["ФИО"].DefaultCellStyle.Padding = new Padding(0, 3, 0, 3);
                    dataGridView1.Columns["ID"].Visible = false;
                    dataGridView1.Columns["Телефон"].Visible = false;
                    dataGridView1.Columns["Photo"].Visible = false;

                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                        column.MinimumWidth = 10;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Проверяем выбранного ранее Барбера
        private void AddZapiciMaster_Load(object sender, EventArgs e)
        {
            SelectRow();
        }
    }
}
