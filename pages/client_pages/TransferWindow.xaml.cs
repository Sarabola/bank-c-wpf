using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Windows;

namespace BankWPFApp.pages.client_pages
{
    public partial class TransferWindow : Window
    {
        private long currentUserId;
        private string connStr = ConfigurationManager.ConnectionStrings["BankDB"].ConnectionString;

        public TransferWindow(long userId)
        {
            InitializeComponent();
            currentUserId = userId;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ClientWindow clientWindow = new ClientWindow(currentUserId);
            clientWindow.Show();
            this.Close();
        }

        private void TransferByPhone_Click(object sender, RoutedEventArgs e)
        {
            string phone = PhoneTextBox.Text.Trim();
            if (!decimal.TryParse(AmountTextBox.Text.Trim(), out decimal amount) || amount <= 0)
            {
                InfoTextBlock.Text = "Введите корректную сумму.";
                InfoTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            long recipientId = GetUserIdByPhone(phone);
            string recipientName = GetUsernameById(recipientId);

            if (recipientId == 0)
            {
                InfoTextBlock.Text = "Пользователь с таким телефоном не найден.";
                InfoTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            var result = MessageBox.Show(
                $"Вы хотите перевести {amount} пользователю {recipientName}?",
                "Подтвердите перевод",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (PerformTransfer(currentUserId, recipientId, amount))
                {
                    InfoTextBlock.Text = $"Успешный перевод {amount} пользователю {recipientName}.";
                    InfoTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                }
            }
        }

        private void TransferByCard_Click(object sender, RoutedEventArgs e)
        {
            string card = CardTextBox.Text.Trim();
            if (!decimal.TryParse(AmountTextBox.Text.Trim(), out decimal amount) || amount <= 0)
            {
                InfoTextBlock.Text = "Введите корректную сумму.";
                InfoTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            long recipientId = GetUserIdByCard(card);
            if (recipientId == 0)
            {
                InfoTextBlock.Text = "Пользователь с такой картой не найден.";
                InfoTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            if (PerformTransfer(currentUserId, recipientId, amount))
            {
                InfoTextBlock.Text = $"Успешный перевод {amount} пользователю с картой {card}.";
                InfoTextBlock.Foreground = System.Windows.Media.Brushes.Green;
            }
        }

        private string GetUsernameById(long userId)
        {
            if (userId == 0) return "Неизвестный";

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT Username FROM Users WHERE UserID=@id", conn);
                cmd.Parameters.AddWithValue("@id", userId);
                var result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : "Неизвестный";
            }
        }


        private long GetUserIdByPhone(string phone)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT OwnerID FROM PaymentDetails WHERE Number_of_phone=@phone", conn);
                cmd.Parameters.AddWithValue("@phone", phone);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt64(result) : 0;
            }
        }

        private long GetUserIdByCard(string card)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT OwnerID FROM PaymentDetails WHERE Number_of_cart=@card", conn);
                cmd.Parameters.AddWithValue("@card", card);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt64(result) : 0;
            }
        }


        private bool PerformTransfer(long fromUserId, long toUserId, decimal amount)
        {
            if (fromUserId == toUserId)
            {
                MessageBox.Show("Нельзя перевести самому себе");
                return false;
            }

            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        var checkCmd = new MySqlCommand("SELECT Amount FROM Wallets WHERE OwnerID=@userId", conn, transaction);
                        checkCmd.Parameters.AddWithValue("@userId", fromUserId);
                        var result = checkCmd.ExecuteScalar();
                        if (result == null)
                        {
                            MessageBox.Show("Кошелек отправителя не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }

                        decimal currentAmount = Convert.ToDecimal(result);
                        if (currentAmount < amount)
                        {
                            MessageBox.Show("Недостаточно средств для перевода.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return false;
                        }


                        var cmdDebit = new MySqlCommand("UPDATE Wallets SET Amount = Amount - @amount WHERE OwnerID=@userId", conn, transaction);
                        cmdDebit.Parameters.AddWithValue("@amount", amount);
                        cmdDebit.Parameters.AddWithValue("@userId", fromUserId);
                        cmdDebit.ExecuteNonQuery();

                        var cmdCredit = new MySqlCommand("UPDATE Wallets SET Amount = Amount + @amount WHERE OwnerID=@userId", conn, transaction);
                        cmdCredit.Parameters.AddWithValue("@amount", amount);
                        cmdCredit.Parameters.AddWithValue("@userId", toUserId);
                        cmdCredit.ExecuteNonQuery();

                        var cmdTrans = new MySqlCommand(
                            "INSERT INTO Transactions (TransactionType, FromID, ToID, Transaction_time, Amount) " +
                            "VALUES ('Transfer', @from, @to, NOW(), @amount)", conn, transaction);
                        cmdTrans.Parameters.AddWithValue("@from", fromUserId);
                        cmdTrans.Parameters.AddWithValue("@to", toUserId);
                        cmdTrans.Parameters.AddWithValue("@amount", amount);
                        cmdTrans.ExecuteNonQuery();

                        transaction.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при переводе: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
