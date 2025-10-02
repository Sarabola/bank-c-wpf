using BankSystem;
using BankWPFApp.pages.client_pages;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
using System.Windows;

namespace BankWPFApp.pages.admin_pages
{
    public partial class AdminWindow : Window
    {
        private string connStr = ConfigurationManager.ConnectionStrings["BankDB"].ConnectionString;
        private bool IsAdmin;
        public AdminWindow()
        {
            InitializeComponent();
        }


        private void ShowAllUsers_Click(object sender, RoutedEventArgs e)
        {
            new UsersWindow().Show();
        }

        private void SQLQuery_Click(object sender, RoutedEventArgs e)
        {
            new SQLQueryWindow().Show();
        }

        private void ShowAllTransactions_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT t.TransactionID, t.TransactionType, t.Amount,
                               uFrom.Username AS FromUser,
                               uTo.Username AS ToUser,
                               t.Transaction_time
                        FROM Transactions t
                        LEFT JOIN Users uFrom ON t.FromID = uFrom.UserID
                        LEFT JOIN Users uTo ON t.ToID = uTo.UserID
                        ORDER BY t.Transaction_time DESC";

                    var adapter = new MySqlDataAdapter(sql, conn);
                    var dt = new DataTable();
                    adapter.Fill(dt);

                    TransactionsWindow adminTransWindow = new TransactionsWindow(dt);
                    adminTransWindow.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении транзакций: {ex.Message}");
            }
        }

        private void ShowTransactionsById_Click(object sender, RoutedEventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox("Введите ID клиента:", "Транзакции клиента");
            if (long.TryParse(input, out long userId))
            {
                TransactionsHistoryWindow transactionsWindow = new TransactionsHistoryWindow(userId, true);
                transactionsWindow.Show();
            }
            else
            {
                MessageBox.Show("Некорректный ID пользователя.");
            }
        }

        private void BanUser_Click(object sender, RoutedEventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox("Введите ID клиента для блокировки:", "Блокировка клиента");
            if (long.TryParse(input, out long userId))
            {
                try
                {
                    MessageBox.Show("Пользователь заблокирован =) (заглушка)");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при блокировке: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Некорректный ID пользователя.");
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}
