using BankSystem;
using System.Windows;

namespace BankWPFApp.pages
{
    public partial class GuestWindow : Window
    {
        public GuestWindow()
        {
            InitializeComponent();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }

        private void OpenCurrency_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Открываем окно с курсами валют");
        }

        private void OpenServices_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Открываем услуги банка");
        }

        private void OpenLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }

        private void OpenRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow register = new RegisterWindow();
            register.Show();
            this.Close();
        }
    }
}
