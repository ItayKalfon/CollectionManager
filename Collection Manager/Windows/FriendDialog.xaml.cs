using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Collection_Manager.Windows
{
    /// <summary>
    /// Interaction logic for FriendDialog.xaml
    /// </summary>
    public partial class FriendDialog : Window
    {
        IMongoCollection<LoginManager.User> users
        {
            get
            {
                return LoginManager.users;
            }
            set
            {
                users = value;
            }
        }
        public FriendDialog()
        {
            InitializeComponent();

            NameInput.Clear();

            CancelButton.Click += Cancel;
            AddButton.Click += Add;
            RemoveButton.Click += Remove;

            KeyDown += OnKey;
        }

        private void OnKey(object sender, KeyEventArgs e)
        {
            Error.Visibility = Visibility.Hidden;
        }

        private void Remove(object sender, RoutedEventArgs e)
        {
            if (NameInput.Text == "")
            {
                Error.Content = "Please enter a username.";
                Error.Visibility = Visibility.Visible;
                return;
            }
            var friend = users.Find(Builders<LoginManager.User>.Filter.Regex("username", new Regex('^' + NameInput.Text + '$', RegexOptions.IgnoreCase))).FirstOrDefault();
            if (friend == null)
            {
                Error.Content = "User not found.";
                Error.Visibility = Visibility.Visible;
            }
            else if (!friend.has_access.Contains(LoginManager.originalLoggedUser))
            {
                Error.Content = "You are already not friended with this user.";
                Error.Visibility = Visibility.Visible;
            }
            else
            {
                users.UpdateOne(Builders<LoginManager.User>.Filter.Regex("username", new Regex('^' + NameInput.Text + '$', RegexOptions.IgnoreCase)), Builders<LoginManager.User>.Update.Pull("has_access", LoginManager.originalLoggedUser));
                MainWindow.Main.UpdateViews();
                Close();
            }
        }

        private void Add(object sender, RoutedEventArgs e)
        {
            if (NameInput.Text == "")
            {
                Error.Content = "Please enter a username.";
                Error.Visibility = Visibility.Visible;
                return;
            }
            var friend = users.Find(Builders<LoginManager.User>.Filter.Regex("username", new Regex('^' + NameInput.Text + '$', RegexOptions.IgnoreCase))).FirstOrDefault();
            if (friend == null)
            {
                Error.Content = "User not found.";
                Error.Visibility = Visibility.Visible;
            }
            else if (friend.has_access.Contains(LoginManager.originalLoggedUser))
            {
                Error.Content = "You are already friends.";
                Error.Visibility = Visibility.Visible;
            }
            else
            {
                users.UpdateOne(Builders<LoginManager.User>.Filter.Regex("username", new Regex('^' + NameInput.Text + '$', RegexOptions.IgnoreCase)), Builders<LoginManager.User>.Update.Push("has_access", LoginManager.originalLoggedUser));
                MainWindow.Main.UpdateViews();
                Close();
            }
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
