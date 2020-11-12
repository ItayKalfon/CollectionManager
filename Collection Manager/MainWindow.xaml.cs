using Collection_Manager.Pages;
using Collection_Manager.Server_Extension;
using Collection_Manager.Windows;
using Microsoft.Win32;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Collection_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Main;
        public Dictionary<string, Page> pages;
        public bool isMyCollection;
        public MainWindow()
        {
            InitializeComponent();

            pages = new Dictionary<string, Page>();

            EditCollectionButton.Click += MoveToEditCollection;
            BrowseCollectionButton.Click += MoveToBrowseCollection;
            DeckBuilderButton.Click += MoveToDeckBuilder;
            LogoutButton.Click += Logout;
            EditFriendsButton.Click += EditFriends;
            ExportCollectionButton.Click += ExportCollection;
            LoadCollectionButton.Click += LoadCollection;
            LandCalculatorButton.Click += LaunchLandCalculator;

            EditCollectionButton.IsEnabled = false;
            BrowseCollectionButton.IsEnabled = false;
            DeckBuilderButton.IsEnabled = false;
            LogoutButton.IsEnabled = false;
            EditFriendsButton.IsEnabled = false;
            ChangeViewButton.IsEnabled = false;
            ExportCollectionButton.IsEnabled = false;
            LoadCollectionButton.IsEnabled = false;
            DeckButton.Visibility = Visibility.Hidden;

            Closed += OnExit;

            Main = this;
            isMyCollection = true;

            Server.AcceptClients();
            ComputerIp.Header = "IP: " + Server.GetLocalIPAddress();

            File.Delete(Environment.CurrentDirectory + @"\Assets\log.txt");

            try
            {
                LoginManager.UserCredentials user = JsonSerializer.Deserialize<LoginManager.UserCredentials>(File.ReadAllText(Environment.CurrentDirectory + @"\Assets\config.json"));
                if (LoginManager.Login(user.username, user.password) == LoginManager.LoginState.SUCESS)
                {
                    // Skip to main menu
                    SetupPages();
                    MoveToBrowseCollection(null, null);
                }
                else
                {
                    MainFrame.Content = new WelcomePage();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                MainFrame.Content = new WelcomePage();
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            Server.Stop();
        }

        private void LaunchLandCalculator(object sender, RoutedEventArgs e)
        {
            var calculator = new LandCalculator();
            calculator.Owner = this;
            calculator.Show();
        }

        private void LoadCollection(object sender, RoutedEventArgs e)
        {
            if (isMyCollection && LoginManager.loggedUser != "")
            {
                var dialog = new OpenFileDialog();
                dialog.FileName = "Collection";
                dialog.DefaultExt = ".txt";
                dialog.Filter = "Text documents (.txt)|*.txt";

                if (dialog.ShowDialog() == true)
                {
                    List<string> lines = File.ReadAllLines(dialog.FileName).Where(line => line != "").ToList();
                    char seperator;
                    if (lines[0].Split(' ')[0].All(char.IsDigit)) // If ' ' is the seperator
                    {
                        seperator = ' ';
                    }
                    else if (lines[0].Split('x')[0].All(char.IsDigit)) // If 'x' is the seperator
                    {
                        seperator = 'x';
                    }
                    else
                    {
                        return;
                    }
                    ((MenuItem)sender).IsEnabled = false;
                    new Thread(() =>
                    {
                        foreach (string line in lines)
                        {
                            string[] splitLine = line.Split(new char[] { seperator }, 2);
                            Collection.Add(splitLine[1], int.Parse(splitLine[0]));
                        }
                    }).Start();
                    Dispatcher.Invoke(() => { ((MenuItem)sender).IsEnabled = true; });
                }
            }
        }

        private void ExportCollection(object sender, RoutedEventArgs e)
        {
            if (LoginManager.loggedUser != "")
            {
                var dialog = new SaveFileDialog();
                dialog.FileName = "Collection";
                dialog.DefaultExt = ".txt";
                dialog.Filter = "Text documents (.txt)|*.txt";

                if (dialog.ShowDialog() == true)
                {
                    File.WriteAllText(dialog.FileName, Collection.AsString());
                }
            }
        }

        private void EditFriends(object sender, RoutedEventArgs e)
        {
            var dialog = new FriendDialog();
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        public void UpdateViews()
        {
            var user = LoginManager.users.Find(Builders<LoginManager.User>.Filter.Regex("username", new Regex('^' + LoginManager.originalLoggedUser + '$', RegexOptions.IgnoreCase))).FirstOrDefault();
            if (user != null)
            {
                ChangeViewButton.Items.Clear();
                MenuItem myCollection = new MenuItem();
                myCollection.Header = "My Collection";
                myCollection.Tag = LoginManager.originalLoggedUser.Replace(" ", "");
                myCollection.Icon = new Image() { Source = new BitmapImage(new Uri(Environment.CurrentDirectory + @"\Assets\user.png")) };
                myCollection.Click += ChangeView;
                ChangeViewButton.Items.Add(myCollection);

                ChangeViewButton.Items.Add(new Separator());

                foreach (string username in user.has_access)
                {
                    MenuItem item = new MenuItem();
                    item.Header = username;
                    item.Tag = username.Replace(" ", "");
                    item.Icon = new Image() { Source = new BitmapImage(new Uri(Environment.CurrentDirectory + @"\Assets\user.png")) };
                    item.Click += ChangeView;
                    ChangeViewButton.Items.Add(item);
                }
            }
        }

        private void ChangeView(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            LoginManager.loggedUser = (string)item.Tag;
            Collection.Refresh();
            if ((string)item.Header != "My Collection")
            {
                BrowseCollectionButton.IsEnabled = true;
                ChangeViewButton.IsEnabled = true;
                LogoutButton.IsEnabled = true;
                ExportCollectionButton.IsEnabled = true;
                DeckBuilderButton.IsEnabled = false;
                EditFriendsButton.IsEnabled = false;
                EditCollectionButton.IsEnabled = false;
                LoadCollectionButton.IsEnabled = false;
                DeckButton.Visibility = Visibility.Hidden;
                isMyCollection = false;
                MoveToBrowseCollection(null, null);
            }
            else
            {
                isMyCollection = true;
                MoveToEditCollection(null, null);
            }
        }

        public void SetupPages()
        {
            pages["EditCollectionPage"] = new EditCollectionPage();
            pages["BrowseCollectionPage"] = new BrowseCollectionPage();
            pages["DeckBuilderPage"] = new DeckBuilderPage();

            UpdateViews();
        }

        public void MoveToDeckBuilder(object sender, RoutedEventArgs e)
        {
            if (isMyCollection && LoginManager.loggedUser != "")
            {
                EditCollectionButton.IsEnabled = true;
                BrowseCollectionButton.IsEnabled = true;
                DeckBuilderButton.IsEnabled = true;
                LogoutButton.IsEnabled = true;
                EditFriendsButton.IsEnabled = true;
                ChangeViewButton.IsEnabled = true;
                ExportCollectionButton.IsEnabled = true;
                LoadCollectionButton.IsEnabled = true;
                DeckButton.Visibility = Visibility.Visible;
                if (((BrowseCollectionPage)pages["BrowseCollectionPage"]).chosenCard != null)
                {
                    ((DeckBuilderPage)pages["DeckBuilderPage"]).SetCard(((BrowseCollectionPage)pages["BrowseCollectionPage"]).chosenCard);
                }
                MainFrame.Content = pages["DeckBuilderPage"];
            }
        }

        public void Logout(object sender, RoutedEventArgs e)
        {
            EditCollectionButton.IsEnabled = false;
            BrowseCollectionButton.IsEnabled = false;
            DeckBuilderButton.IsEnabled = false;
            LogoutButton.IsEnabled = false;
            EditFriendsButton.IsEnabled = false;
            ChangeViewButton.IsEnabled = false;
            ExportCollectionButton.IsEnabled = false;
            LoadCollectionButton.IsEnabled = false;
            DeckButton.Visibility = Visibility.Hidden;
            isMyCollection = true;
            if (LoginManager.Logout())
            {
                MainFrame.Content = new WelcomePage();
            }
        }

        public void MoveToBrowseCollection(object sender, RoutedEventArgs e)
        {
            if (isMyCollection)
            {
                EditCollectionButton.IsEnabled = true;
                BrowseCollectionButton.IsEnabled = true;
                DeckBuilderButton.IsEnabled = true;
                LogoutButton.IsEnabled = true;
                EditFriendsButton.IsEnabled = true;
                ChangeViewButton.IsEnabled = true;
                ExportCollectionButton.IsEnabled = true;
                LoadCollectionButton.IsEnabled = true;
                DeckButton.Visibility = Visibility.Hidden;
            }
            if (LoginManager.loggedUser != "")
            MainFrame.Content = pages["BrowseCollectionPage"];
        }

        public void MoveToEditCollection(object sender, RoutedEventArgs e)
        {
            if (isMyCollection && LoginManager.loggedUser != "")
            {
                EditCollectionButton.IsEnabled = true;
                BrowseCollectionButton.IsEnabled = true;
                DeckBuilderButton.IsEnabled = true;
                LogoutButton.IsEnabled = true;
                EditFriendsButton.IsEnabled = true;
                ChangeViewButton.IsEnabled = true;
                ExportCollectionButton.IsEnabled = true;
                LoadCollectionButton.IsEnabled = true;
                DeckButton.Visibility = Visibility.Hidden;
                MainFrame.Content = pages["EditCollectionPage"];
            }
        }
    }
}
