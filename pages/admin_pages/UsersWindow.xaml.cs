using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
using System.Windows;

namespace BankWPFApp.pages.admin_pages
{
    public partial class UsersWindow : Window
    {
        private string connStr = ConfigurationManager.ConnectionStrings["BankDB"].ConnectionString;

        public UsersWindow()
        {
            InitializeComponent();
            LoadUsers();
        }

        public UsersWindow(DataTable dt)
        {
            InitializeComponent();
            UsersDataGrid.ItemsSource = dt.DefaultView;
        }

        private void LoadUsers()
        {
            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    string sql = @"
                        SELECT 
                    u.UserID, 
                    u.Username, 
                    r.RoleName,
                    pd.LastName,
                    pd.FirstName,
                    pd.Patronymic,
                    pay.Number_of_phone, 
                    pay.Number_of_cart,
                    w.Amount
                        FROM Users u
                        JOIN Roles r ON u.RoleID = r.RoleID
                        JOIN Wallets w ON u.UserID = w.OwnerID
                        LEFT JOIN PaymentDetails pay ON u.UserID = pay.OwnerID
                        LEFT JOIN PersonalDatas pd ON u.UserID = pd.OwnerID
                        WHERE u.RoleID = 2
                        ORDER BY u.UserID";

                    var adapter = new MySqlDataAdapter(sql, conn);
                    var dt = new DataTable();
                    adapter.Fill(dt);

                    UsersDataGrid.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
