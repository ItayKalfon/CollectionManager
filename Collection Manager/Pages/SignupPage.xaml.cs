using System;
using System.Collections.Generic;
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
using System.IO;

namespace Collection_Manager.Pages
{
    /// <summary>
    /// Interaction logic for SignupPage.xaml
    /// </summary>
    public partial class SignupPage : Page
    {
        public SignupPage()
        {
            InitializeComponent();

            Signup.Click += Signup_Click;
            Cancel.Click += Cancel_Click;
            Error.Visibility = Visibility.Hidden;
            KeyDown += OnKey;
        }

        private void Signup_Click(object sender, RoutedEventArgs e)
        {
            signup();
        }

        private void OnKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                // Signup on enter
                signup();
            }
            else if (e.Key == Key.Escape)
            {
                // Go bakc on escape
                NavigationService.Navigate(new WelcomePage());
            }
            else
            {
                // Overwise clean the message because the user retypes
                Error.Visibility = Visibility.Hidden;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Go back
            NavigationService.Navigate(new WelcomePage());
        }

        private void signup()
        {
            if (LoginManager.Signup(UsernameInput.Text, PasswordInput.Password) == LoginManager.LoginState.USERNAME_TAKEN)
            {
                // If the user name is taken warn the user
                Error.Content = "Username already taken";
                Error.Visibility = Visibility.Visible;
            }
            else
            {
                // If the user checked remeber me save him
                if ((bool)RememberMe.IsChecked)
                {
                    LoginManager.User user = new LoginManager.User(UsernameInput.Text, PasswordInput.Password);
                    File.WriteAllText(Environment.CurrentDirectory + @"\Assets\config.json", JsonSerializer.Serialize(user));
                }
                Error.Content = "Logged In";
                Error.Visibility = Visibility.Visible;
                Error.Foreground = Brushes.Green;

                MainWindow.Main.SetupPages();
                MainWindow.Main.MoveToEditCollection(null, null);
            }
        }
    }
}
