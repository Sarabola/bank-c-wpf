using System;
using System.Configuration;
using System.Windows;
using MySql.Data.MySqlClient;

namespace BankWPFApp.pages.client_pages
{
    public partial class DepositWindow : Window
    {
        private string connStr = ConfigurationManager.ConnectionStrings["BankDB"].ConnectionString;
        private long userId;

        public DepositWindow(long userId)
        {
            InitializeComponent();
            this.userId = userId;
        }
        
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            ClientWindow client = new ClientWindow(userId);
            client.Show();
            this.Close();
        }
        private void DepositButton_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(AmountTextBox.Text.Trim(), out decimal amount) || amount <= 0)
            {
                InfoTextBlock.Text = "Введите корректную сумму.";
                InfoTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            if (AddAmountToWallet(userId, amount))
            {
                InfoTextBlock.Text = $"Счет успешно пополнен на {amount}!";
                InfoTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                AmountTextBox.Clear();
            }
            else
            {
                InfoTextBlock.Text = "Ошибка при пополнении счета.";
                InfoTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private bool AddAmountToWallet(long userId, decimal amount)
        {
            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                    
                        var cmdUpdate = new MySqlCommand(
                            "UPDATE Wallets SET Amount = Amount + @amount WHERE OwnerID = @userId", conn, transaction);
                        cmdUpdate.Parameters.AddWithValue("@amount", amount);
                        cmdUpdate.Parameters.AddWithValue("@userId", userId);
                        int rowsAffected = cmdUpdate.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Кошелек не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }

                        var cmdTrans = new MySqlCommand(
                            "INSERT INTO Transactions (TransactionType, FromID, ToID, Transaction_time, Amount) " +
                            "VALUES ('Deposit', NULL, @userId, NOW(), @amount)", conn, transaction);
                        cmdTrans.Parameters.AddWithValue("@userId", userId);
                        cmdTrans.Parameters.AddWithValue("@amount", amount);
                        cmdTrans.ExecuteNonQuery();

                        transaction.Commit();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при пополнении счета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

    }
}
