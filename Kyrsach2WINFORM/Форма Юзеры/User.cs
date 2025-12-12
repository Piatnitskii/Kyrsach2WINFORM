using MySql.Data.MySqlClient;
using System;
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
    public partial class User : Form
    {
        public User()
        {
            this.DoubleBuffered = true; // Двойной буфер
            InitializeComponent();

            // Включаем двойную буферизацию для DataGridView
            Optimize.SetDoubleBuffered(dataGridView2);
            dataGridView2.CellBorderStyle = DataGridViewCellBorderStyle.None;

            //Заполняем дата грид
            FillDataGrid();
        }


        int CurrentRowIndex; // Индекс выбранной строки
        // Получаем инфу по выбранной строке ДатаГрида
        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            CurrentRowIndex = e.RowIndex;

            //Если мы ткнули на ту учетку, в которой сидим
            if (ConnectAndData.ID == dataGridView2.Rows[CurrentRowIndex].Cells["ID"].Value.ToString())
            {
                button6.Enabled = false;
                button1.Enabled = true;
                return;
            }

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

        // При загрузке формы убираем выделение
        private void User_Load(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection();
        }

        // IdUser, Name, Surname, Patronymic, Password, Login, Id_Role
        string CMD = "Select IdUser as ID, CONCAT_WS(' ', User.Name, Surname, Patronymic) AS 'ФИО', Password as 'Пароль', Login as 'Логин', Role.Name as 'Роль', Id_Role as 'idrole' FROM User INNER JOIN Role ON Id_Role = IdRole";
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

                    // Настройка полей
                    dataGridView2.Columns["ID"].Visible = false;
                    dataGridView2.Columns["idrole"].Visible = false;

                    foreach (DataGridViewColumn column in dataGridView2.Columns)
                        column.MinimumWidth = 100;

                    //Очищаем, полезно после редактирования, удаления и добавления
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

        //Скрываем некоторые данные
        private void dataGridView2_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (ShowText)
            {
                if (dataGridView2.Columns[e.ColumnIndex].Name == "ФИО" && e.RowIndex != ThisRow)
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
            else
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
        }

        //Чистим
        private void dataGridView2_Sorted(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection();
        }

       

        // Открыть форму редактирования
        private void button1_Click(object sender, EventArgs e)
        {
            string ID = dataGridView2.Rows[CurrentRowIndex].Cells["ID"].Value.ToString();
            string Name = dataGridView2.Rows[CurrentRowIndex].Cells["ФИО"].Value.ToString().Split(' ')[0];
            string Surname = dataGridView2.Rows[CurrentRowIndex].Cells["ФИО"].Value.ToString().Split(' ')[1];
            string Patronymic = dataGridView2.Rows[CurrentRowIndex].Cells["ФИО"].Value.ToString().Split(' ')[2];
            string Login = dataGridView2.Rows[CurrentRowIndex].Cells["Логин"].Value.ToString();
            string RoleName = dataGridView2.Rows[CurrentRowIndex].Cells["Роль"].Value.ToString();
            string Id_Role = dataGridView2.Rows[CurrentRowIndex].Cells["idrole"].Value.ToString();

            UserSystem user = new UserSystem(ID, Name, Surname, Patronymic, Login, Id_Role, RoleName);

            RedactUser FormA = new RedactUser(user);
            FormA.ShowDialog();
            FillDataGrid();
        }

        // Открыть форму добавления
        private void button2_Click(object sender, EventArgs e)
        {
            AddUser FormA = new AddUser();
            FormA.ShowDialog();
            FillDataGrid();
        }

        //Удаление
        private void deleteUser_Click(object sender, EventArgs e)
        {
            string ID = dataGridView2.Rows[CurrentRowIndex].Cells["ID"].Value.ToString();
            string Name = dataGridView2.Rows[CurrentRowIndex].Cells["ФИО"].Value.ToString().Split(' ')[0];
            string Surname = dataGridView2.Rows[CurrentRowIndex].Cells["ФИО"].Value.ToString().Split(' ')[1];
            string Patronymic = dataGridView2.Rows[CurrentRowIndex].Cells["ФИО"].Value.ToString().Split(' ')[2];

            string CMD = $"DELETE FROM User WHERE IdUser = {ID};";
            try
            {
                DialogResult dialogResult = MessageBox.Show($"Удалить пользователя {Name + " " + Surname + " " + Patronymic}?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    //Удаляем строку
                    using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                    {
                        Con.Open();
                        MySqlCommand cmd = new MySqlCommand(CMD, Con);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Пользователь был успешно удален!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        //Закрыть
        private void button8_Click(object sender, EventArgs e)
        {
            MenuAdmin.DisableButton();
            this.Close();
        }

        bool ShowText = false;
        int ThisRow;
        //Подсветка строки на которую направлен указатель мыши
        private void dataGridView2_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                
                dataGridView2.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGray;

                ShowText = true;
                ThisRow = e.RowIndex;
                dataGridView2.Rows[e.RowIndex].Cells["ФИО"].Value = dataGridView2.Rows[e.RowIndex].Cells["ФИО"].Value;
            }
        }
        //Возвращаем состояние строки на исходную, когда указатель "Покидает" строку
        private void dataGridView2_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                dataGridView2.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                ShowText = false;
                dataGridView2.Rows[e.RowIndex].Cells["ФИО"].Value = dataGridView2.Rows[e.RowIndex].Cells["ФИО"].Value;
            }
                
        }

    }
}
