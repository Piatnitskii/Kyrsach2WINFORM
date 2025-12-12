using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kyrsach2WINFORM
{
    public partial class Employee : Form
    {
        public Employee()
        {
            InitializeComponent();
            this.DoubleBuffered = true; //двойная буферизация

            // Включаем двойную буферизацию для DataGridView
            Optimize.SetDoubleBuffered(dataGridView2);

            //Заполняем дата грид
            FillDataGrid();

            dataGridView2.CellBorderStyle = DataGridViewCellBorderStyle.None;
        }


        // При загрузке формы убираем выделение
        private void Employee_Load(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection();
        }
        // Чистим 
        private void dataGridView2_Sorted(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection();
        }

        int CurrentRowIndex; // Индекс выбранной строки
        // Получаем инфу по выбранной строке ДатаГрида
        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            CurrentRowIndex = e.RowIndex;

            if (CurrentRowIndex == -1)
            {
                button1.Enabled = false;
                button6.Enabled = false;
                dataGridView2.ClearSelection();
                return;
            }

            button1.Enabled = true;
            button6.Enabled = true;
        }

        // Employe.Name as 'Имя', Surname as 'Фамилия', Patronymic as 'Отчество',
        string CMD = "Select IdEmploye as ID, CONCAT_WS(' ', Employe.Name, Surname, Patronymic) AS 'ФИО', Phone as 'Телефон', Birthday as 'Дата рождения', Post.Name as 'Должность', Photo as 'фото', Id_Post as 'Id_Post' FROM Employe INNER JOIN Post ON Id_Post = IdPost";
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

                    dataGridView2.DataSource = Dt;

                    //Настройка полей 
                    dataGridView2.Columns["ID"].Visible = false;
                    dataGridView2.Columns["фото"].Visible = false;
                    dataGridView2.Columns["Id_Post"].Visible = false;


                    foreach (DataGridViewColumn column in dataGridView2.Columns)
                        column.MinimumWidth = 100;

                    dataGridView2.ClearSelection(); //Убираем выделение после обновления грида

                    //Очищаем
                    button1.Enabled = false;
                    button6.Enabled = false;
                    dataGridView2.ClearSelection();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Обновить сотрудника
        private void button1_Click(object sender, EventArgs e)
        {
            string ID = dataGridView2.Rows[CurrentRowIndex].Cells["ID"].Value.ToString();
            string Name = dataGridView2.Rows[CurrentRowIndex].Cells["ФИО"].Value.ToString().Split(' ')[0];
            string Surname = dataGridView2.Rows[CurrentRowIndex].Cells["ФИО"].Value.ToString().Split(' ')[1];
            string Patronymic = dataGridView2.Rows[CurrentRowIndex].Cells["ФИО"].Value.ToString().Split(' ')[2];
            string Phone = dataGridView2.Rows[CurrentRowIndex].Cells["Телефон"].Value.ToString();
            string Birthday = dataGridView2.Rows[CurrentRowIndex].Cells["Дата рождения"].Value.ToString();
            string Post = dataGridView2.Rows[CurrentRowIndex].Cells["Должность"].Value.ToString();
            string Photo  = dataGridView2.Rows[CurrentRowIndex].Cells["фото"].Value.ToString();
            string Id_Post = dataGridView2.Rows[CurrentRowIndex].Cells["Id_Post"].Value.ToString();

            Emploey emploey = new Emploey(ID, Name, Surname, Patronymic, Phone, Birthday, Post, Photo, null, null, Id_Post); // Наш сотрудник в коде

            RedactEmp FormA = new RedactEmp(emploey);
            FormA.ShowDialog();
            FillDataGrid();
        }

        //  Добавить сотрудника
        private void button2_Click(object sender, EventArgs e)
        {
            
            AddEmp FormA = new AddEmp();
            FormA.ShowDialog();
            FillDataGrid();
        }

        //Удаление сотрудника
        private void deleteEmploye_Click(object sender, EventArgs e)
        {
            string ID = dataGridView2.Rows[CurrentRowIndex].Cells["ID"].Value.ToString();
            string Name = dataGridView2.Rows[CurrentRowIndex].Cells["ФИО"].Value.ToString().Split(' ')[0];
            string Surname = dataGridView2.Rows[CurrentRowIndex].Cells["ФИО"].Value.ToString().Split(' ')[1];
            string Patronymic = dataGridView2.Rows[CurrentRowIndex].Cells["ФИО"].Value.ToString().Split(' ')[2];

            string CMD = $"DELETE FROM Employe WHERE IdEmploye = {ID};";
            try
            {
                DialogResult dialogResult = MessageBox.Show($"Удалить сотрудника {Name + " " + Surname + " " + Patronymic}?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    //Удаляем строку
                    using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                    {
                        Con.Open();
                        MySqlCommand cmd = new MySqlCommand(CMD, Con);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Сотрудник был успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    FillDataGrid();
                }
                else
                    return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Скрываем некоторые данные
        private void dataGridView2_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView2.Columns[e.ColumnIndex].Name == "ФИО")
            {
                if (e.Value != null)
                {
                    var Val = e.Value.ToString().Split(' ');
                    string Result;
                    if (Val.Length == 3 && Val[2].Trim() != "")
                        Result = Val[0] + " " + (Val[1])[0] + "." + " " + (Val[2])[0] + ".";
                    else
                        Result = Val[0] + " " + (Val[1])[0] + ".";

                    e.Value = Result;
                }
            }
        }

        //Закрываем форму
        private void button3_Click(object sender, EventArgs e)
        {
            MenuAdmin.DisableButton();
            this.Dispose();
            this.Close();
        }

        //Подсветка строки на которую направлен указатель мыши
        private void dataGridView2_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if(e.RowIndex > -1)
                dataGridView2.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGray;
        }
        //Возвращаем состояние строки на исходную, когда указатель "Покидает" строку
        private void dataGridView2_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
                dataGridView2.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
        }
    }
}
