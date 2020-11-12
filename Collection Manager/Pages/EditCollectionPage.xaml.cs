using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace Collection_Manager.Pages
{
    /// <summary>
    /// Interaction logic for EditCollectionPage.xaml
    /// </summary>
    public partial class EditCollectionPage : Page
    {
        public EditCollectionPage()
        {
            InitializeComponent();

            NameInput.Clear();
            AmountInput.Clear();

            Error.Visibility = Visibility.Hidden;

            KeyDown += OnKey;

            Add.Click += Add_Click;
            Remove.Click += Remove_Click;
        }

        private void OnKey(object sender, KeyEventArgs e)
        {
            // Quick actions
            if (e.Key == Key.Enter)
            {
                add();
            }
            else if (e.Key == Key.Delete)
            {
                remove();
            }
            else
            {
                Error.Visibility = Visibility.Hidden;
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            remove();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            add();
        }

        private void add()
        {
            try
            {
                if (!Collection.Add(NameInput.Text, int.Parse(AmountInput.Text)))
                {
                    // If addition failed
                    Error.Text = "Error, card not found";
                    Error.Visibility = Visibility.Visible;
                    // Set card to unknown
                    CardPreview.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/Assets/unknown-card.png"));
                    OracleText.Text = "Card not found.";
                    Amount.Text = "Amount: N/A";
                    Used.Text = "Used: N/A";
                }
                else
                {
                    // Search for the card name to get the card
                    Card card = Collection.Search('\"' + NameInput.Text + '\"')[0];
                    // Set card details
                    CardPreview.Source = new CardImage(card).ToBitmapImage();
                    OracleText.Text = card.details.oracle_text;
                    Amount.Text = "Amount: " + card.amount;
                    Used.Text = "Used: " + card.used;
                }
            }
            catch
            {
                // If there was error while adding
                Error.Text = "Problem with card name, quantity or internal error";
                Error.Visibility = Visibility.Visible;
                // Set the card to unknown
                CardPreview.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/Assets/unknown-card.png"));
                OracleText.Text = "Card not found.";
                Amount.Text = "Amount: N/A";
                Used.Text = "Used: N/A";
            }
        }

        private void remove()
        {
            try
            {
                if (!Collection.Remove(NameInput.Text, int.Parse(AmountInput.Text)))
                {
                    // If there was a problem with the removal
                    Error.Text = "Error, card not found";
                    Error.Visibility = Visibility.Visible;
                    // Set the card to unknown
                    CardPreview.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/Assets/unknown-card.png"));
                    OracleText.Text = "Card not found.";
                    Amount.Text = "Amount: N/A";
                    Used.Text = "Used: N/A";
                }
                else
                {
                    // Get the card
                    Card card = Collection.GetCard(NameInput.Text);
                    if (card != null) // If the card was found
                    {
                        // Set card details
                        CardPreview.Source = new CardImage(card).ToBitmapImage();
                        OracleText.Text = card.details.oracle_text;
                        Amount.Text = "Amount: " + card.amount;
                        Used.Text = "Used: " + card.used;
                    }
                    else
                    {
                        // Set the card to unknown
                        CardPreview.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/Assets/unknown-card.png"));
                        OracleText.Text = "Card not found.";
                        Amount.Text = "Amount: N/A";
                        Used.Text = "Used: N/A";
                    }
                }
            }
            catch
            {
                // If there was a problem during the removal
                Error.Text = "Problem with card name, quantity or internal error";
                Error.Visibility = Visibility.Visible;
                // Set the card to unknown
                CardPreview.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/Assets/unknown-card.png"));
                OracleText.Text = "Card not found.";
                Amount.Text = "Amount: N/A";
                Used.Text = "Used: N/A";
            }
        }
    }
}
