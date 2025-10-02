    using System;
    using System.Configuration;
    using System.Windows;
    using MySql.Data.MySqlClient;



    namespace BankSystem
    {
        public partial class RegisterWindow : Window
        {
            int START_BALANCE = 0;
            string connStr = ConfigurationManager.ConnectionStrings["BankDB"].ConnectionString;

            public RegisterWindow()
            {
                InitializeComponent();
            }

            private void Register_Click(object sender, RoutedEventArgs e)
            {
                string login = LoginTextBox.Text.Trim();
                string password = PasswordBox.Password.Trim();
                string phoneNumber = PhoneNumberBox.Text.Trim();
                string cartNumber = GenerateCardNumber();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(phoneNumber))
                {
                    MessageBox.Show("Введите логин и пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var user = new User
                {
                    Username = login,
                    Password = password,
                    Role = 2 
                };

                if (AddUserToDB(user, phoneNumber, cartNumber))
                {
                    MessageBox.Show($"Пользователь {login} успешно зарегистрирован!",
                                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    new LoginWindow().Show();

                    this.Close();
                
                }
            }

        private bool AddUserToDB(User user, string phoneNumber, string cartNumber)
        {
            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        var checkCmd = new MySqlCommand(
                            "SELECT COUNT(*) FROM Users WHERE Username=@username", conn, transaction);
                        checkCmd.Parameters.AddWithValue("@username", user.Username);
                        long count = (long)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show("Такой логин уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }

                        var insertCmd = new MySqlCommand(
                            "INSERT INTO Users (Username, Password, RoleID) VALUES (@username, @password, @roleId);",
                            conn, transaction);
                        insertCmd.Parameters.AddWithValue("@username", user.Username);
                        insertCmd.Parameters.AddWithValue("@password", user.Password);
                        insertCmd.Parameters.AddWithValue("@roleId", user.Role);
                        insertCmd.ExecuteNonQuery();
                        long userId = insertCmd.LastInsertedId;

                        var personalData = new MySqlCommand(
                            "INSERT INTO PaymentDetails (Number_of_phone, Number_of_cart, OwnerID) VALUES (@phoneNumber, @cartNumber, @ownerId)",
                            conn, transaction);
                        personalData.Parameters.AddWithValue("@phoneNumber", phoneNumber);
                        personalData.Parameters.AddWithValue("@cartNumber", cartNumber);
                        personalData.Parameters.AddWithValue("@ownerId", userId);
                        personalData.ExecuteNonQuery();

                        var walletCmd = new MySqlCommand(
                            "INSERT INTO Wallets (OwnerID, Amount) VALUES (@ownerId, @amount)",
                            conn, transaction);
                        walletCmd.Parameters.AddWithValue("@ownerId", userId);
                        walletCmd.Parameters.AddWithValue("@amount", START_BALANCE);
                        walletCmd.ExecuteNonQuery();

                        transaction.Commit();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }



        private string GenerateCardNumber()
        {
            Random rnd = new Random();
            char[] digits = new char[16];
            for (int i = 0; i < 16; i++)
            {
                digits[i] = (char)('0' + rnd.Next(0, 10));
            }
            return new string(digits);
        }


        public class User
            {
                public string Username { get; set; }
                public string Password { get; set; }
                public int Role { get; set; }
            }

            public class Wallet
            {
                public long OwnerID { get; set; }
                public int Amount { get; set; }
            }
        }
    }
