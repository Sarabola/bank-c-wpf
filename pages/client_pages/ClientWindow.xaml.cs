using BankSystem;
using System.Windows;

namespace BankWPFApp.pages.client_pages
{
    public partial class ClientWindow : Window
    {
        private long currentUserId;

        public ClientWindow(long userId)
        {
            InitializeComponent();
            currentUserId = userId;
        }


        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }

        private void UserProfile_Click(object sender, RoutedEventArgs e)
        {
            new UserProfileWindow(currentUserId).Show();
            this.Close();
        }

        private void Transfer_Click(object sender, RoutedEventArgs e)
        {
            new TransferWindow(currentUserId).Show();
            this.Close();
        }

        private void TransactionsHistory_Click(object sender, RoutedEventArgs e)
        {
            new TransactionsHistoryWindow(currentUserId, false).Show();
            this.Close();
        }

        private void Withdrawal_Click(object sender, RoutedEventArgs e)
        {
            WithdrawWindow withdraw = new WithdrawWindow(currentUserId);
            withdraw.Show();
            this.Close();
        }

        private void Deposit_Click(object sender, RoutedEventArgs e)
        {
            DepositWindow depositWindow = new DepositWindow(currentUserId);
            depositWindow.Show();
            this.Close();
        }

        private void Balance_Click(object sender, RoutedEventArgs e)
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["BankDB"].ConnectionString;

            try
            {
                using (var conn = new MySql.Data.MySqlClient.MySqlConnection(connStr))
                {
                    conn.Open();

                    var cmd = new MySql.Data.MySqlClient.MySqlCommand(
                        "SELECT Amount FROM Wallets WHERE OwnerID = @userId", conn);
                    cmd.Parameters.AddWithValue("@userId", currentUserId);

                    var result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        decimal balance = Convert.ToDecimal(result);
                        MessageBox.Show($"Текущий баланс: {balance} руб.", "Баланс", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Кошелек не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении баланса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

}
}
