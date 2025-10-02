using BankWPFApp.pages.client_pages;
using BankWPFApp.pages.admin_pages;
using BankWPFApp.pages;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BankSystem
{
    public partial class LoginWindow : Window
    {
        string connStr = ConfigurationManager.ConnectionStrings["BankDB"].ConnectionString;

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void OpenGuest_Click(object sender, RoutedEventArgs e)
        {
            new GuestWindow().Show();
            this.Close();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb.Text == "Введите логин")
            {
                tb.Text = "";
                tb.Foreground = Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = "Введите логин";
                tb.Foreground = Brushes.Gray;
            }
        }


        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password.Trim();

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = "SELECT u.UserID, r.RoleName FROM Users u " +
                             "JOIN Roles r ON u.RoleID = r.RoleID " +
                             "WHERE u.Username=@username AND u.Password=@password";

                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string role = reader["RoleName"].ToString();
                    long currentUserID = Convert.ToInt64(reader["UserID"]);

                    MessageBox.Show($"Добро пожаловать, {username}! Ваша роль: {role}");

                    if (role == "Клиент")
                        new ClientWindow(currentUserID).Show();
                    else if (role == "Администратор")
                        new AdminWindow().Show();

                    this.Close();
                }
                else
                {
                    lblStatus.Text = "Неверный логин или пароль!";
                }
            }
        }
    }
}
