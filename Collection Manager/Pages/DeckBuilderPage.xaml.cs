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
using System.Threading;
using System.Windows.Threading;
using Collection_Manager.UserControls;
using System.IO;
using Microsoft.Win32;

namespace Collection_Manager.Pages
{
    /// <summary>
    /// Interaction logic for DeckBuilderPage.xaml
    /// </summary>
    public partial class DeckBuilderPage : Page
    {
        Deck selectedDeck;
        public DeckBuilderPage()
        {
            InitializeComponent();

            Decks.SelectionChanged += UpdateDeck;
            AddCard.Click += AddToDeck;
            RemoveCard.Click += RemoveFromDeck;
            AddDeck.Click += CreateDeck;
            RemoveDeck.Click += DeleteDeck;

            MainWindow.Main.LoadDeckButton.Click += LoadDeck;
            MainWindow.Main.ExportDeckButton.Click += ExportDeck;

            CardNameInput.Clear();
            DeckNameInput.Clear();
            AmountInput.Clear();

            KeyDown += OnKey;

            DeckView.onCardSelection = OnCardSelect;

            UpdateDecks();
        }

        private void ExportDeck(object sender, RoutedEventArgs e)
        {
            if (LoginManager.loggedUser != "" && selectedDeck != null) // If the user is logged and there is a deck selected
            {
                // Open the file manager dialog
                var dialog = new SaveFileDialog();
                dialog.FileName = selectedDeck.name;
                dialog.DefaultExt = ".txt";
                dialog.Filter = "Text documents (.txt)|*.txt";

                if (dialog.ShowDialog() == true)
                {
                    // Save the cards in the file that was chosen
                    File.WriteAllText(dialog.FileName, selectedDeck.AsString());
                }
            }
        }

        private void LoadDeck(object sender, RoutedEventArgs e)
        {
            if (LoginManager.loggedUser != "" && selectedDeck != null) // If the user is logged and there is a deck selected
            {
                // Open the file manager dialog
                var dialog = new OpenFileDialog();
                dialog.FileName = "Deck";
                dialog.DefaultExt = ".txt";
                dialog.Filter = "Text documents (.txt)|*.txt";

                if (dialog.ShowDialog() == true)
                {
                    // Get the text from the file
                    List<string> lines = File.ReadAllLines(dialog.FileName).Where(line => line != "").ToList(); // Ignore empty lines
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
                    ((MenuItem)sender).IsEnabled = false; // Set the item to disabled while loading

                    // Start a seperate thread because addition takes time
                    new Thread(() =>
                    {
                        // for each line, add that card to the deck
                        foreach (string line in lines)
                        {
                            string[] splitLine = line.Split(new char[] { seperator }, 2);
                            selectedDeck.Add(splitLine[1], int.Parse(splitLine[0]));
                        }
                        Dispatcher.Invoke(() => { ((MenuItem)sender).IsEnabled = true; }); // In the end, make the menu item enabled again
                    }).Start();
                }
            }
        }

        private void OnKey(object sender, KeyEventArgs e)
        {
            // Quick actions
            if (e.Key == Key.Enter)
            {
                AddToDeck(null, null);
            }
            else
            {
                Error.Visibility = Visibility.Hidden;
            }
        }

        private void DeleteDeck(object sender, RoutedEventArgs e)
        {
            // Clear the name input
            string deck = DeckNameInput.Text;
            DeckNameInput.Clear();
            
            // Start a new thread becuase removal takes time
            new Thread(() => {
                Deck.DeleteDeck(deck); // Delete the deck
                Dispatcher.Invoke(() => { UpdateDecks(); }); // Update the deck list
            }).Start();
        }

        private void CreateDeck(object sender, RoutedEventArgs e)
        {
            Deck.CreateDeck(DeckNameInput.Text); // Create a deck
            DeckNameInput.Clear(); // Clear the name input
            UpdateDecks(); // Update the deck list
        }

        private void OnCardSelect(object sender, SelectionChangedEventArgs e)
        {
            // If only one card was selected
            if (e.AddedItems.Count == 1 && e.AddedItems[0] != null)
            {
                // Set the details in the add / remove section
                string cardName = (string)((Image)e.AddedItems[0]).Tag;
                CardNameInput.Insert(cardName);
                AmountInput.Insert(selectedDeck.GetCard(cardName).amount.ToString());
            }
        }

        private void RemoveFromDeck(object sender, RoutedEventArgs e)
        {
            int amount = 0;
            Card card = selectedDeck.GetCard(CardNameInput.Text);
            if (selectedDeck == null)
            {
                // If no decks are selected
                Error.Content = "No deck is selected.";
                Error.Visibility = Visibility.Visible;
            }
            else if (!int.TryParse(AmountInput.Text, out amount))
            {
                // If the card amount is invalid
                Error.Content = "Invalid card amount.";
                Error.Visibility = Visibility.Visible;
            }
            else if (card == null)
            {
                // If the card is already not in the deck
                Error.Content = "Card not in deck.";
                Error.Visibility = Visibility.Visible;
            }
            else if (!selectedDeck.Remove(CardNameInput.Text, amount, MainWindow.Main.ModifyCollection.IsChecked))
            {
                // Remove failed
                Error.Content = "Error while removing card.";
                Error.Visibility = Visibility.Visible;
            }
            else
            {
                // Update the deck view
                for (int i = 0; i < amount; i++)
                {
                    DeckView.Remove(card);
                }
                UpdateCards();
            }
        }

        private void AddToDeck(object sender, RoutedEventArgs e)
        {
            int amount = 0;
            if (selectedDeck == null)
            {
                // If there is deck selected
                Error.Content = "No deck is selected.";
                Error.Visibility = Visibility.Visible;
            }
            else if (!int.TryParse(AmountInput.Text, out amount))
            {
                // If theres a problem with the card amount
                Error.Content = "Invalid card amount.";
                Error.Visibility = Visibility.Visible;
            }
            else if (!selectedDeck.Add(CardNameInput.Text, amount, MainWindow.Main.ModifyCollection.IsChecked))
            {
                // If the addition failed
                Error.Content = "Invalid card name or not enough cards in collection.";
                Error.Visibility = Visibility.Visible;
            }
            else
            {
                // Add the card to the deck view
                for (int i = 0; i < amount; i++)
                {
                    DeckView.Add(selectedDeck.GetCard(CardNameInput.Text));
                }
                UpdateCards();
            }
        }

        private void UpdateDeck(object sender, SelectionChangedEventArgs e)
        {
            // If only one deck was selected and it is a valid deck
            if (e.AddedItems.Count == 1 && Deck.decks.Contains((string)((ComboBoxItem)e.AddedItems[0]).Content))
            {
                selectedDeck = new Deck((string)((ComboBoxItem)e.AddedItems[0]).Content); // Set it as selected
                DeckView.Set(selectedDeck.GetCards()); // Set the deck viewer to view this deck
                UpdateCards(); // Update the cards
            }
            else
            {
                selectedDeck = null; // Set that there is no deck selected
                DeckView.Clear(); // Clear all of the cards
                // Delete the deck info
                Cards.Text = "Cards: N/A";
                Symbols.Text = "W(), U(), B(), R(), G()";
                Types.Text = "Lands: N/A, Non-Lands: N/A";
                Colors.Text = "Colors: N/A";
            }
        }

        public void UpdateDecks()
        {
            Decks.Items.Clear(); // Remove all items

            var empty = new ComboBoxItem();
            empty.Height = 30;
            Decks.Items.Add(empty); // Empty Option

            // Append all decks that the user has
            List<string> decks = Deck.decks;
            foreach (string deck in decks)
            {
                var item = new ComboBoxItem();
                item.Content = deck;
                item.FontSize = 15;
                item.Height = 30;
                item.VerticalContentAlignment = VerticalAlignment.Center;
                Decks.Items.Add(item);
            }
        }

        public void UpdateCards()
        {
            // Start a new thread because getting the data takes time
            new Thread(() => {
                // Get card data
                string cardAmount = selectedDeck.CardAmount.ToString();
                string symbols = "W(" + selectedDeck.Symbols[Collection_Manager.Colors.WHITE] + ")," +
                        " U(" + selectedDeck.Symbols[Collection_Manager.Colors.BLUE] + ")," +
                        " B(" + selectedDeck.Symbols[Collection_Manager.Colors.BLACK] + ")," +
                        " G(" + selectedDeck.Symbols[Collection_Manager.Colors.GREEN] + ")," +
                        " R(" + selectedDeck.Symbols[Collection_Manager.Colors.RED];
                string types = "Lands: " + selectedDeck.Lands.ToString() + ", Non-Lands: " + selectedDeck.NonLands.ToString();
                string colors = "Colors: " + selectedDeck.DeckColors.AsString();
                // Set card data
                Dispatcher.Invoke(() =>
                {
                    Cards.Text = "Cards: " + cardAmount;
                    Symbols.Text = symbols;
                    Types.Text = types;
                    Colors.Text = colors;
                }, DispatcherPriority.Background);
            }).Start();
        }

        public void SetCard(Card card)
        {
            // Set the card name and default card quantity
            CardNameInput.Insert(card.name);
            AmountInput.Insert("1");
        }
    }
}
