using BankWPFApp.pages.admin_pages;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
using System.Windows;
using static BankSystem.RegisterWindow;

namespace BankWPFApp.pages.client_pages
{
    public partial class TransactionsHistoryWindow : Window
    {
        private long currentUserId;
        private bool IsAdmin;
        private string connStr = ConfigurationManager.ConnectionStrings["BankDB"].ConnectionString;

        public TransactionsHistoryWindow(long userId, bool isAdmin)
        {
            InitializeComponent();
            currentUserId = userId;
            IsAdmin = isAdmin;
            LoadTransactions();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsAdmin == false)
            {
                ClientWindow client = new ClientWindow(currentUserId);
                client.Show();
            }

            this.Close();
        }

        private void LoadTransactions()
        {
            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    string sql = @"
                                SELECT 
                                    t.TransactionID,
                                    CASE 
                                        WHEN t.ToID = @userId THEN '+' 
                                        WHEN t.FromID = @userId THEN '-' 
                                        ELSE '' 
                                    END AS OperationType,
                                    CONCAT(
                                        CASE 
                                            WHEN t.ToID = @userId THEN '+' 
                                            WHEN t.FromID = @userId THEN '-' 
                                            ELSE '' 
                                        END,
                                        t.Amount
                                    ) AS AmountDisplay,
                                    COALESCE(uFrom.Username, 'Банкомат') AS FromUser,
                                    COALESCE(uTo.Username, 'Банкомат') AS ToUser,
                                    t.Transaction_time
                                FROM Transactions t
                                LEFT JOIN Users uFrom ON t.FromID = uFrom.UserID
                                LEFT JOIN Users uTo ON t.ToID = uTo.UserID
                                WHERE t.FromID = @userId OR t.ToID = @userId
                                ORDER BY t.Transaction_time DESC";


                    var cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@userId", currentUserId);

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
