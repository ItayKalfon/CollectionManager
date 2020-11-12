using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Collection_Manager.CustomElements
{
    class CardStack : ListView
    {
        public CardStack()
        {
            // Set default style properties
            Style style = new Style(typeof(ListViewItem));
            style.Setters.Add(new Setter(ListViewItem.MarginProperty, new Thickness(0, 0, 0, -240)));
            style.Setters.Add(new Setter(ListViewItem.WidthProperty, (double)200));
            style.Setters.Add(new Setter(ListViewItem.HorizontalAlignmentProperty, HorizontalAlignment.Center));

            Width = 205;
            Background = System.Windows.Media.Brushes.Transparent;
            BorderBrush = System.Windows.Media.Brushes.Transparent;

            ItemContainerStyle = style;
        }

        public void Insert(List<Card> cards)
        {
            // For each card in the list
            foreach (Card card in cards)
            {
                // Add the card
                Add(card);
            }
        }

        public void Add(Card card)
        {
            // Create a new image that represents the card
            Image cardImage = new Image();
            cardImage.Source = new CardImage(card).ToBitmapImage();
            cardImage.Tag = card.name;
            Items.Add(cardImage);
        }

        public bool Remove(Card card)
        {
            // Find the image that corresponds to the card
            Image cardImage = Items.OfType<Image>().Where(image => (string)image.Tag == card.name).ToList()[0];
            if (cardImage != null)
            {
                // If the card was found remove it
                Items.Remove(cardImage);
            }
            return !Items.IsEmpty; // Return if there are cards left
        }
    }
}
