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
    public partial class Clients : Form
    {
        public Clients()
        {
            InitializeComponent();
            this.DoubleBuffered = true; //двойная буферизация
            // Включаем двойную буферизацию для DataGridView
            Optimize.SetDoubleBuffered(dataGridView2);
            dataGridView2.CellBorderStyle = DataGridViewCellBorderStyle.None;

            //Прячем от админа некоторый функционал
            if (ConnectAndData.Role == "1")
            {
                button1.Visible = false;
                button6.Visible = false;
                button2.Visible = false;
                dataGridView2.Size = new Size(1201, 518);
            }

            
            //Заполняем датагрид
            FillDataGrid();
        }

        // При загрузке формы убираем выделение
        private void Clients_Load(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection();
        }

        // IdClient, Name, Surname, Phone
        string CMD = "Select IdClient as ID, CONCAT_WS(' ', Name, Surname, Patronymic) AS 'ФИО', Phone as 'Телефон'  FROM Client";

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
                    foreach (DataGridViewColumn column in dataGridView2.Columns)
                        column.MinimumWidth = 100;

                    //Очищаем
                    dataGridView2.ClearSelection();
                    button1.Enabled = false;
                    button6.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
        // Чистим 
        private void dataGridView2_Sorted(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection();
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

        //Добавить
        private void button2_Click(object sender, EventArgs e)
        {
            AddClient FormA = new AddClient();
            FormA.ShowDialog();
            FillDataGrid();
        }

        //Редактировать
        private void button1_Click(object sender, EventArgs e)
        {
            // Создаем клиента для передачи
            var ID = dataGridView2.Rows[CurrentRowIndex].Cells["ID"].Value.ToString();
            string Name = dataGridView2.Rows[CurrentRowIndex].Cells["ФИО"].Value.ToString().Split(' ')[0];
            string Surname = dataGridView2.Rows[CurrentRowIndex].Cells["ФИО"].Value.ToString().Split(' ')[1];
            string Patronymic = dataGridView2.Rows[CurrentRowIndex].Cells["ФИО"].Value.ToString().Split(' ')[2];
            string Phone = dataGridView2.Rows[CurrentRowIndex].Cells["Телефон"].Value.ToString();

            RedactClient FormA = new RedactClient(new Client(ID, Name, Surname, Patronymic, Phone));
            FormA.ShowDialog();
            FillDataGrid();
        }

        //Удаление
        private void button6_Click(object sender, EventArgs e)
        {
            var ID = dataGridView2.Rows[CurrentRowIndex].Cells["ID"].Value.ToString();
            string Name = dataGridView2.Rows[CurrentRowIndex].Cells["ФИО"].Value.ToString().Split(' ')[0];
            string Surname = dataGridView2.Rows[CurrentRowIndex].Cells["ФИО"].Value.ToString().Split(' ')[1];
            string Patronymic = dataGridView2.Rows[CurrentRowIndex].Cells["ФИО"].Value.ToString().Split(' ')[2];

            string CMD = $"DELETE FROM Client WHERE IdClient = {ID};";
            try
            {
                DialogResult dialogResult = MessageBox.Show($"Удалить клиента {Name + " " + Surname + " " + Patronymic}?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    //Удаляем строку
                    using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                    {
                        Con.Open();
                        MySqlCommand cmd = new MySqlCommand(CMD, Con);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Клиент был успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        // Закрыть
        private void button8_Click(object sender, EventArgs e)
        {
            MenuAdmin.DisableButton();
            this.Close();
        }

        //Подсветка строки на которую направлен указатель мыши
        private void dataGridView2_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex > -1)
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
