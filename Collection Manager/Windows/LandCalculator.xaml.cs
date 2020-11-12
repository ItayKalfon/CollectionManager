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
using System.Windows.Shapes;

namespace Collection_Manager.Windows
{
    /// <summary>
    /// Interaction logic for LandCalculator.xaml
    /// </summary>
    public partial class LandCalculator : Window
    {
        public LandCalculator()
        {
            InitializeComponent();

            Clear();

            KeyDown += OnKey;
        }

        private void OnKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Calculate();
            }
            else if (e.Key == Key.Escape)
            {
                Clear();
            }
            else
            {
                Tips.Visibility = Visibility.Hidden;
            }
        }

        public void Clear()
        {
            WhiteInput.Clear();
            BlueInput.Clear();
            BlackInput.Clear();
            RedInput.Clear();
            GreenInput.Clear();
            AmountInput.Clear();

            Type.SelectedIndex = 0;

            Tips.Visibility = Visibility.Visible;

            Plains.Content = "0";
            Island.Content = "0";
            Swamp.Content = "0";
            Mountain.Content = "0";
            Forest.Content = "0";

            MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        public void Calculate()
        {
            int white = int.TryParse(WhiteInput.Text, out white) ? white : 0;
            int blue = int.TryParse(BlueInput.Text, out blue) ? blue : 0;
            int black = int.TryParse(BlackInput.Text, out black) ? black : 0;
            int red = int.TryParse(RedInput.Text, out red) ? red : 0;
            int green = int.TryParse(GreenInput.Text, out green) ? green : 0;

            int lands;
            if (!int.TryParse(AmountInput.Text, out lands))
            {
                return;
            }

            lands = Type.SelectedIndex == 0 ? lands : (lands / 3) * 2;

            float ratio = (white + blue + black + red + green) / (float)lands;

            Plains.Content = Math.Round(white / ratio).ToString();
            Island.Content = Math.Round(blue / ratio).ToString();
            Swamp.Content = Math.Round(black / ratio).ToString();
            Mountain.Content = Math.Round(red / ratio).ToString();
            Forest.Content = Math.Round(green / ratio).ToString();
        }
    }
}
