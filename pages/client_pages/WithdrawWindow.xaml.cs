using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Configuration;
using System.Windows;

namespace BankWPFApp.pages.client_pages
{
    public partial class WithdrawWindow : Window
    {
        private string connStr = ConfigurationManager.ConnectionStrings["BankDB"].ConnectionString;
        private long userId;

        public WithdrawWindow(long userId)
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
        private void WithdrawalButton_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(AmountTextBox.Text.Trim(), out decimal amount) || amount <= 0)
            {
                InfoTextBlock.Text = "Введите корректную сумму.";
                InfoTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            if (DiffAmountFromWallet(userId, amount))
            {
                InfoTextBlock.Text = $"Денежные средства в размере {amount} успешно сняты со счёта!";
                InfoTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                AmountTextBox.Clear();
            }
            else
            {
                InfoTextBlock.Text = "Ошибка при снятии средств со счета.";
                InfoTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private bool DiffAmountFromWallet(long userId, decimal amount)
        {
            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {

                        var checkCmd = new MySqlCommand(
                            "SELECT Amount FROM Wallets WHERE OwnerID = @userId", conn, transaction);
                        checkCmd.Parameters.AddWithValue("@userId", userId);
                        var result = checkCmd.ExecuteScalar();

                        if (result == null)
                        {
                            MessageBox.Show("Кошелек не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;

                            }

                        decimal currentAmount = Convert.ToDecimal(result);
                        if (currentAmount < amount)
                        {
                            MessageBox.Show("Недостаточно средств для снятия.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return false;
                        }


                            var cmdUpdate = new MySqlCommand(
                                "UPDATE Wallets SET Amount = Amount - @amount WHERE OwnerID = @userId", conn, transaction);
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
                                "VALUES ('Withdrawal', @userId, NULL, NOW(), @amount)", conn, transaction);
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
                MessageBox.Show($"Ошибка при снятии средств со счета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

    }
}
