using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Windows;

namespace BankWPFApp.pages.client_pages
{
    public partial class UserProfileWindow : Window
    {
        private long currentUserId;
        private string connStr = ConfigurationManager.ConnectionStrings["BankDB"].ConnectionString;

        public UserProfileWindow(long userId)
        {
            InitializeComponent();
            currentUserId = userId;
            LoadUserData();
        }


        private void PersonalData_Click(object sender, RoutedEventArgs e)
        {
            new PersonalDataWindow(currentUserId).Show();
            this.Close();
        }
        private void BackButton_Click(object sender, EventArgs e)
        {
            new ClientWindow(currentUserId).Show();
            this.Close();
        }

        private void SavePhoneButton_Click(object sender, RoutedEventArgs e)
        {
            string newPhone = PhoneTextBox.Text.Trim();

            // Проверка формата номера
            if (!System.Text.RegularExpressions.Regex.IsMatch(newPhone, @"^\+[0-9]{10,15}$"))
            {
                MessageBox.Show("Номер телефона должен быть в формате +79991234567", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    var cmd = new MySqlCommand(
                        "UPDATE PaymentDetails SET Number_of_phone = @phone WHERE OwnerID = @userId", conn);
                    cmd.Parameters.AddWithValue("@phone", newPhone);
                    cmd.Parameters.AddWithValue("@userId", currentUserId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Номер телефона успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Ошибка: пользователь не найден или телефон не изменился.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении номера телефона: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadUserData()
        {
            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    string sql = @"
                        SELECT u.Username, r.RoleName, p.Number_of_phone, p.Number_of_cart
                        FROM Users u
                        LEFT JOIN Roles r ON u.RoleID = r.RoleID
                        LEFT JOIN PaymentDetails p ON u.UserID = p.OwnerID
                        WHERE u.UserID = @userId";

                    var cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@userId", currentUserId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            UsernameTextBlock.Text = reader["Username"].ToString();
                            RoleTextBlock.Text = reader["RoleName"].ToString();
                            PhoneTextBox.Text = reader["Number_of_phone"] != DBNull.Value ? reader["Number_of_phone"].ToString() : "";
                            CardTextBlock.Text = reader["Number_of_cart"] != DBNull.Value ? reader["Number_of_cart"].ToString() : "Не указан";
                        }
                        else
                        {
                            MessageBox.Show("Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            this.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
