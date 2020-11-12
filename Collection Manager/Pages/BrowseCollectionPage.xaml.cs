using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
namespace Collection_Manager.Pages
{
    /// <summary>
    /// Interaction logic for BrowseCollectionPage.xaml
    /// </summary>
    public partial class BrowseCollectionPage : Page
    {
        private List<Card> results;
        private int chosenIndex;
        private Random random = new Random();
        public Card chosenCard
        {
            get
            {
                // Only if the chosen index is in range return the chosen card from the results
                return chosenIndex >= results.Count ? null : results[chosenIndex];
            }
            set
            {
                // Set the value
                chosenCard = value;
            }
        }
        public BrowseCollectionPage()
        {
            InitializeComponent();

            results = new List<Card>();
            chosenIndex = 0;

            clear();

            this.Clear.Click += Clear_Click;
            this.Search.Click += Search_Click;
            this.Forward.Click += Forward_Click;
            this.Backward.Click += Backward_Click;
            this.Random.Click += Random_Click;

            this.KeyDown += OnKey;

            Update();
        }

        private void Random_Click(object sender, RoutedEventArgs e)
        {
            // If there are any results
            if (results.Count > 0)
            {
                // Choose a random index in that range and go to it
                chosenIndex = random.Next(0, results.Count - 1);
                Update();
            }
        }

        private void OnKey(object sender, KeyEventArgs e)
        {
            // Quick actions
            if (e.Key == Key.Enter)
            {
                search();
            }
            else if (e.Key == Key.RightCtrl)
            {
                clear();
            }
            else
            {
                Error.Visibility = Visibility.Hidden;
            }
        }

        private void Backward_Click(object sender, RoutedEventArgs e)
        {
            backward();
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            forward();
        }

        private void Update()
        {
            if (results.Count > 0 && chosenIndex > -1 && chosenIndex < results.Count)
            {
                // If the index is in range
                CardPreview.Source = new CardImage(results[chosenIndex]).ToBitmapImage(); // Set the image to the card image
                OracleText.Text = results[chosenIndex].details.oracle_text; // Set the text to the card text
                Amount.Text = "Amount: " + results[chosenIndex].amount.ToString(); // Set the amount to the card amount
                Used.Text = "Used: " + results[chosenIndex].used.ToString(); // Set the used amount to the number of uses
                ResultNumber.Content = "Result " + (chosenIndex + 1).ToString() + " of " + results.Count.ToString(); // Show the index
            }
            else
            {
                OracleText.Text = "Card not found."; // Tell the user that the card is unavailable
                CardPreview.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + @"\Assets\unknown-card.png")); // Set the image to the unavailable card image
                Amount.Text = "Amount: N/A"; // Show that the amount is not available
                Used.Text = "Used: N/A"; // Show that the amount of uses is not available
                ResultNumber.Content = "Result N/A of " + results.Count.ToString(); // Show that the index is not available
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            search();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            clear();
        }

        private void clear()
        {
            // Clear all fields
            NameInput.Clear();
            CardColors.Clear();
            ColorIdentity.Clear();
            TypeInput.Clear();
            RarityInput.Clear();
            CmcInput.Clear();
            OracleTextInput.Clear();
            OnlyAvailable.IsChecked = false;
            ManaCostInput.Clear();
            ManaProduction.Clear();
            PowerInput.Clear();
            ToughnessInput.Clear();
            LoyaltyInput.Clear();
            QuantityInput.Clear();

            Error.Visibility = Visibility.Hidden;
        }

        private void search()
        {
            try
            {
                // Try to search the collection
                results = Collection.Search(NameInput.Text, CardColors.colors, ColorIdentity.colors, RarityInput.Text, TypeInput.Text, CmcInput.Text, OracleTextInput.Text, (bool)OnlyAvailable.IsChecked, ManaCostInput.Text, ManaProduction.colors, PowerInput.Text, ToughnessInput.Text, LoyaltyInput.Text, QuantityInput.Text);
                chosenIndex = 0; // Reset the index to the first card
                Update(); // Show the card
            }
            catch
            {
                // If there was an error during the search notify the user
                Error.Text = "Error: could not parse your given filters";
                Error.Visibility = Visibility.Visible;
            }
        }

        private void forward()
        {
            // If the index is still in range
            if (chosenIndex < results.Count)
            {
                chosenIndex++; // Increase the index
                Update(); // Show the card
            }
        }

        private void backward()
        {
            // If the index is still in range
            if (chosenIndex > -1)
            {
                chosenIndex--; // Decrease the index
                Update(); // Show the card
            }
        }
    }
}
