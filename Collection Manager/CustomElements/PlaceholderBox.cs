using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Collection_Manager.CustomElements
{
    public class PlaceholderBox : TextBox
    {
        public string Placeholder 
        { 
            get
            {
                // Return the placeholder that was set
                return (string)GetValue(PlaceholderProperty); 
            } 
            set 
            { 
                // Set the placeholder to the given value
                SetValue(PlaceholderProperty, value); 
            } 
        }
        new public string Text
        { 
            get 
            {
                // Return the text if its different from the placeholder
                return (string)GetValue(TextProperty) == Placeholder ? "" : (string)GetValue(TextProperty); 
            }
            set
            {
                // Insert the text to the textbox
                SetValue(TextProperty, value);
            } 
        }

        // Property for the placeholder field
        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register("Placeholder", typeof(string), typeof(PlaceholderBox));

        public PlaceholderBox()
        {
            // Set the focus handlers
            GotFocus += RemovePlaceholder;
            LostFocus += AddPlaceholder;
        }

        new public void Clear()
        {
            // Set the placeholder
            Text = Placeholder;
            Foreground = Brushes.Gray;
            FontStyle = FontStyles.Italic;
        }

        private void AddPlaceholder(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                // If there is no user input set the placeholder
                Clear();
            }
        }

        private void RemovePlaceholder(object sender, RoutedEventArgs e)
        {
            if (Text == "") // If the text is empty (is the same as the placeholder)
            {
                // Remove the placeholder
                Text = "";
                Foreground = Brushes.Black;
                FontStyle = FontStyles.Normal;
            }
        }

        public void Insert(string text)
        {
            // Remove the placeholder and insert the text
            RemovePlaceholder(null, null);
            Text = text;
        }
    }
}
