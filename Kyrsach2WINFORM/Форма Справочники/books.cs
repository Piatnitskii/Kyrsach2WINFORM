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
    public partial class books : Form
    {
        public books()
        {
            InitializeComponent();

            // Включаем двойную буферизацию для DataGridView1
            Optimize.SetDoubleBuffered(dataGridView1);
            // Включаем двойную буферизацию для DataGridView2
            Optimize.SetDoubleBuffered(dataGridView2);

            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridView2.CellBorderStyle = DataGridViewCellBorderStyle.None;
            //Заролняем дата грид
            FillDataGrid();
        }

        // При загрузке формы убираем выделение
        private void books_Load(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            dataGridView2.ClearSelection();
        }

        //Заполняет ДатаГрид категорий данными
        void FillDataGrid()
        {
            string CMDCategory = "Select IdCategory as 'ID', Name as 'Название' FROM Category;";
            string  CMDPost = "Select IdPost as 'ID', Name as 'Название' FROM Post;";

            try
            {
                using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                {
                    Con.Open();

                    MySqlCommand cmd = new MySqlCommand(CMDCategory, Con);
                    DataTable DtCategory = new DataTable();
                    DataTable DtPost = new DataTable();

                    // 1. Получаем данные из таблицы категорий
                    cmd.ExecuteNonQuery();
                    MySqlDataAdapter Ad = new MySqlDataAdapter(cmd);
                    Ad.Fill(DtCategory);
                    
                    // 2. Получаем данные из таблицы Должностей
                    cmd.CommandText = CMDPost;
                    cmd.ExecuteNonQuery();
                    Ad = new MySqlDataAdapter(cmd);
                    Ad.Fill(DtPost);

                    //Категория
                    dataGridView1.DataSource = DtCategory;
                    dataGridView1.Columns["ID"].Visible = false;
                    dataGridView1.Columns["Название"].SortMode = DataGridViewColumnSortMode.NotSortable;
                    //Должность
                    dataGridView2.DataSource = DtPost;
                    dataGridView2.Columns["ID"].Visible = false;
                    dataGridView2.Columns["Название"].SortMode = DataGridViewColumnSortMode.NotSortable;

                    //Все блокируем
                    dataGridView1.ClearSelection();
                    dataGridView2.ClearSelection();
                    DsableButton();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        // ДОЛЖНОСТИ
        int CurrentRowIndex; // Индекс выбранной строки
        // Получаем инфу по выбранной строке ДатаГрида
        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            CurrentRowIndex = e.RowIndex;

            if (CurrentRowIndex == -1)
            {
                DsableButton();         // Все блокируем
                dataGridView1.ClearSelection();
                dataGridView2.ClearSelection();
                return;
            }

            //Выбрали в 1 дата гриде - убрали выбор в другом
            dataGridView1.ClearSelection();
            button6.Enabled = false;
            button1.Enabled = false;
            button7.Enabled = true;
            button3.Enabled = true;
        }


        void DsableButton()
        {
            // КАТЕГОРИИ
            button6.Enabled = false;
            button1.Enabled = false;

            // ДОЛЖНОСТИ
            button7.Enabled = false;
            button3.Enabled = false;
        }

        // КАТЕГОРИИ
        int CurrentRowIndex2; // Индекс выбранной строки
        // Получаем инфу по выбранной строке ДатаГрида
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            CurrentRowIndex2 = e.RowIndex;

            if (CurrentRowIndex2 == -1)
            {
                DsableButton();     // Все блокируем
                dataGridView1.ClearSelection();
                dataGridView2.ClearSelection();
                return;
            }

            //Выбрали в 1 дата гриде - убрали выбор в другом
            dataGridView2.ClearSelection();
            button7.Enabled = false;
            button3.Enabled = false;
            button6.Enabled = true;
            button1.Enabled = true;
        }

        // При Сортировке чистим везде выделенные строки
        private void dataGridView1_Sorted(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            dataGridView2.ClearSelection();
        }

        // Редактировать категории
        private void button1_Click(object sender, EventArgs e)
        {
            string ID = dataGridView1.Rows[CurrentRowIndex2].Cells["ID"].Value.ToString();
            string Name = dataGridView1.Rows[CurrentRowIndex2].Cells["Название"].Value.ToString();
            RedactCategory FormA = new RedactCategory(ID, Name);
            FormA.ShowDialog();
            FillDataGrid();
        }

        // Редактировать Должность
        private void button3_Click(object sender, EventArgs e)
        {
            string ID = dataGridView2.Rows[CurrentRowIndex].Cells["ID"].Value.ToString();
            string Name = dataGridView2.Rows[CurrentRowIndex].Cells["Название"].Value.ToString();
            RedactRoly FormA = new RedactRoly(ID, Name);
            FormA.ShowDialog();
            FillDataGrid();
        }

        // Добавить категория
        private void button2_Click(object sender, EventArgs e)
        {
            AddCategory FormA = new AddCategory();
            FormA.ShowDialog(); 
            FillDataGrid();
        }

        // Добавить должность
        private void button4_Click(object sender, EventArgs e)
        {
            AddRol FormA = new AddRol();
            FormA.ShowDialog();
            FillDataGrid();
        }

        //Удаление категории
        private void deleteCategory_Click(object sender, EventArgs e)
        {
            string ID = dataGridView1.Rows[CurrentRowIndex2].Cells["ID"].Value.ToString();
            string Name = dataGridView1.Rows[CurrentRowIndex2].Cells["Название"].Value.ToString();

            string CMD = $"DELETE FROM Category WHERE IdCategory = {ID};";
            try
            {
                DialogResult dialogResult = MessageBox.Show($"Удалить категорию {Name}?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    //Удаляем строку
                    using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                    {
                        Con.Open();
                        MySqlCommand cmd = new MySqlCommand(CMD, Con);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Категория была успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        //Удаление должности
        private void deletePost_Click(object sender, EventArgs e)
        {
            string ID = dataGridView2.Rows[CurrentRowIndex].Cells["ID"].Value.ToString();
            string Name = dataGridView2.Rows[CurrentRowIndex].Cells["Название"].Value.ToString();

            string CMD = $"DELETE FROM Post WHERE IdPost = {ID};";
            try
            {
                DialogResult dialogResult = MessageBox.Show($"Удалить должность {Name}?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    //Удаляем строку
                    using (MySqlConnection Con = new MySqlConnection(ConnectAndData.Сonnect))
                    {
                        Con.Open();
                        MySqlCommand cmd = new MySqlCommand(CMD, Con);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Должность была успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
