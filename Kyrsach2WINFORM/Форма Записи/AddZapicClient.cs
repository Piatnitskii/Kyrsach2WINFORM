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
    public partial class AddZapicClient : Form
    {

        //Fields 
        Client client;      // Наш клиент
        Button button2;

        public AddZapicClient(Control control, Client client)
        {
            InitializeComponent();

            // Включаем двойную буферизацию для DataGridView
            Optimize.SetDoubleBuffered(dataGridView1);

            this.client = client;
            this.button2 = (Button)control;

            //Заполняем ДатаГрид данными
            FillDataGrid();
        }

        
        string CMD = "Select IdClient as ID, CONCAT_WS(' ', Name, Surname, Patronymic) AS 'ФИО', Phone as 'Телефон'  FROM Client";

        //Заполняет ДатаГрид данными
        void FillDataGrid()
        {
            try
            {
                DataTable DtForClient = new DataTable();

                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    Con.Open();

                    MySqlCommand cmd = new MySqlCommand(CMD, Con);
                    cmd.ExecuteNonQuery();

                    MySqlDataAdapter Ad = new MySqlDataAdapter(cmd);

                    Ad.Fill(DtForClient);
                    dataGridView1.DataSource = DtForClient;

                    //Настройка полей
                    dataGridView1.Columns["ФИО"].SortMode = DataGridViewColumnSortMode.NotSortable;
                    dataGridView1.Columns["ФИО"].DefaultCellStyle.Padding = new Padding(0, 3, 0, 3);
                    dataGridView1.Columns["ID"].Visible = false;
                    dataGridView1.Columns["Телефон"].Visible = false;
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
                return;

            Update();
        }

        //Отображает выбранного пользователя на странице и обновляет переменную
        void Update()
        {
            //Сохраняем
            client.IdClient = dataGridView1.Rows[CurrentRowIndex].Cells[0].Value.ToString();
            client.FIO = dataGridView1.Rows[CurrentRowIndex].Cells[1].Value.ToString();
            client.Phone = dataGridView1.Rows[CurrentRowIndex].Cells[2].Value.ToString();
            client.CurrentRowIndex = CurrentRowIndex.ToString();

            //Отображаем
            label1.Text = client.FIO;
            label2.Text = client.Phone;

            label1.Visible = true;
            label2.Visible = true;

            button2.Enabled = true;
        }


        //Отображаем выбранного ранее клиента, если таковой имеется
        void SelectRow()
        {
            if (client.CurrentRowIndex != null && client.IdClient != null) // Если выбран ранее, отображаем
            {
                dataGridView1.CurrentCell = dataGridView1.Rows[Convert.ToInt32(client.CurrentRowIndex)].Cells[1];
                CurrentRowIndex = Convert.ToInt32(client.CurrentRowIndex);

                // отображаем выбранного ранее клиента
                label1.Text = client.FIO;
                label2.Text = client.Phone;

                label1.Visible = true;
                label2.Visible = true;
            }
            else
                dataGridView1.ClearSelection();
        }

        
        //Проверяем выбранного ранее клиента
        private void AddZapicClient_Load(object sender, EventArgs e)
        {
            SelectRow();
        }

        //Подсветка строки на которую направлен указатель мыши
        private void dataGridView1_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex > -1)
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGray;
        }
        //Возвращаем состояние строки на исходную, когда указатель "Покидает" строку
        private void dataGridView1_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
        }
    }
}
