using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Windows;

namespace BankWPFApp.pages.client_pages
{
    public partial class PersonalDataWindow : Window
    {
        private long userId;
        private string connStr = ConfigurationManager.ConnectionStrings["BankDB"].ConnectionString;

        public PersonalDataWindow(long currentUserId)
        {
            InitializeComponent();
            userId = currentUserId;
            LoadPersonalData();
        }

        private void LoadPersonalData()
        {
            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    string sql = "SELECT FirstName, LastName, Patronymic FROM PersonalDatas WHERE OwnerID = @userId";
                    var cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@userId", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            FirstNameTextBox.Text = reader["FirstName"].ToString();
                            LastNameTextBox.Text = reader["LastName"].ToString();
                            PatronymicTextBox.Text = reader["Patronymic"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string patronymic = PatronymicTextBox.Text.Trim();

            if (string.IsNullOrEmpty(firstName))
            {
                MessageBox.Show("Имя обязательно для заполнения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    string sql = @"
                        INSERT INTO PersonalDatas (FirstName, LastName, Patronymic, OwnerID)
                        VALUES (@firstName, @lastName, @patronymic, @userId)
                        ON DUPLICATE KEY UPDATE 
                            FirstName = VALUES(FirstName), 
                            LastName = VALUES(LastName),
                            Patronymic = VALUES(Patronymic);";

                    var cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@firstName", firstName);
                    cmd.Parameters.AddWithValue("@lastName", string.IsNullOrEmpty(lastName) ? null : lastName);
                    cmd.Parameters.AddWithValue("@patronymic", string.IsNullOrEmpty(patronymic) ? null : patronymic);
                    cmd.Parameters.AddWithValue("@userId", userId);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            new UserProfileWindow(userId).Show();
            this.Close();
        }
    }
}
