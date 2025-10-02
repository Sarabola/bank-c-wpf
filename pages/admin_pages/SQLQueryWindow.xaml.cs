using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
using System.Windows;

namespace BankWPFApp.pages.admin_pages
{
    public partial class SQLQueryWindow : Window
    {
        private string connStr = ConfigurationManager.ConnectionStrings["BankDB"].ConnectionString;

        public SQLQueryWindow()
        {
            InitializeComponent();
        }

        private void ExecuteSqlButton_Click(object sender, RoutedEventArgs e)
        {
            string sql = SqlQueryTextBox.Text.Trim();
            if (string.IsNullOrEmpty(sql))
            {
                MessageBox.Show("Введите SQL-запрос.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    if (sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                    {
                        var adapter = new MySqlDataAdapter(sql, conn);
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        SqlResultDataGrid.ItemsSource = dt.DefaultView;
                    }
                    else
                    {
                        var cmd = new MySqlCommand(sql, conn);
                        int affectedRows = cmd.ExecuteNonQuery();
                        MessageBox.Show($"Запрос выполнен. Затронуто строк: {affectedRows}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении запроса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearSqlButton_Click(object sender, RoutedEventArgs e)
        {
            SqlQueryTextBox.Clear();
            SqlResultDataGrid.ItemsSource = null;
        }
    }
}
