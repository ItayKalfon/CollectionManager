using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.Json;

namespace Collection_Manager.Pages
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();

            Login.Click += Login_Click;
            Cancel.Click += Cancel_Click;
            Error.Visibility = Visibility.Hidden;
            KeyDown += OnKey;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Go back
            NavigationService.Navigate(new WelcomePage());
        }

        private void OnKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Login on enter click
                LoginUser();
            }
            else if (e.Key == Key.Escape)
            {
                // Go back on escape
                NavigationService.Navigate(new WelcomePage());
            }
            else
            {
                // Hide the error if the user retypes
                Error.Visibility = Visibility.Hidden;
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            LoginUser();
        }

        private void LoginUser()
        {
            switch (LoginManager.Login(UsernameInput.Text, PasswordInput.Password))
            {
                case LoginManager.LoginState.SUCESS:
                    // If the user check remember me save him
                    if ((bool)RememberMe.IsChecked)
                    {
                        LoginManager.User user = new LoginManager.User(UsernameInput.Text, PasswordInput.Password); // Saves the login details
                        File.WriteAllText(Environment.CurrentDirectory + @"\Assets\config.json", JsonSerializer.Serialize(user)); // Writes them into a file
                    }
                    Error.Content = "Logged In";
                    Error.Visibility = Visibility.Visible;
                    Error.Foreground = Brushes.Green;

                    MainWindow.Main.SetupPages();
                    MainWindow.Main.MoveToEditCollection(null, null);
                    break;
                case LoginManager.LoginState.INCORRECT_USERNAME:
                    // Warn the user about the username
                    Error.Content = "Incorrect username, user not found";
                    Error.Visibility = Visibility.Visible;
                    break;
                case LoginManager.LoginState.INCORRECT_PASSWORD:
                    // Warn the user about the password
                    Error.Content = "Incorrect password, check your password or username";
                    Error.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
