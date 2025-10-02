using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
using System.Windows;

namespace BankWPFApp.pages.admin_pages
{
    public partial class TransactionsWindow : Window
    {
        private string connStr = ConfigurationManager.ConnectionStrings["BankDB"].ConnectionString;

        // Конструктор для админа, передаем DataTable
        public TransactionsWindow(DataTable dt)
        {
            InitializeComponent();
            TransactionsDataGrid.ItemsSource = dt.DefaultView;
        }

        // Конструктор для конкретного пользователя по userId
        public TransactionsWindow(long userId)
        {
            InitializeComponent();
            LoadTransactions(userId);
        }

        private void LoadTransactions(long userId)
        {
            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    string sql = @"
                        SELECT 
                            t.TransactionID,
                            t.TransactionType,
                            uFrom.Username AS FromUser,
                            uTo.Username AS ToUser,
                            t.Amount,
                            t.Transaction_time
                        FROM Transactions t
                        LEFT JOIN Users uFrom ON t.FromID = uFrom.UserID
                        LEFT JOIN Users uTo ON t.ToID = uTo.UserID
                        WHERE t.FromID = @userId OR t.ToID = @userId
                        ORDER BY t.Transaction_time DESC";

                    var cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@userId", userId);

                    var adapter = new MySqlDataAdapter(cmd);
                    var dt = new DataTable();
                    adapter.Fill(dt);

                    TransactionsDataGrid.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке транзакций: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
