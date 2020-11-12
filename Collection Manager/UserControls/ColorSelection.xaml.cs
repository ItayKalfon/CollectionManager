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

namespace Collection_Manager.UserControls
{
    /// <summary>
    /// Interaction logic for ColorSelection.xaml
    /// </summary>
    public partial class ColorSelection : UserControl
    {
        public CardColor colors;

        public ColorSelection()
        {
            InitializeComponent();

            colors = new CardColor();

            IsWhite.Click += Update;
            IsBlue.Click += Update;
            IsBlack.Click += Update;
            IsRed.Click += Update;
            IsGreen.Click += Update;

            ModeSelection.SelectionChanged += Update;
        }

        private void Update(object sender, RoutedEventArgs e)
        {
            colors.Clear();

            colors.Add((bool)IsWhite.IsChecked ? Collection_Manager.Colors.WHITE : Collection_Manager.Colors.NONE);
            colors.Add((bool)IsBlue.IsChecked ? Collection_Manager.Colors.BLUE : Collection_Manager.Colors.NONE);
            colors.Add((bool)IsBlack.IsChecked ? Collection_Manager.Colors.BLACK : Collection_Manager.Colors.NONE);
            colors.Add((bool)IsRed.IsChecked ? Collection_Manager.Colors.RED : Collection_Manager.Colors.NONE);
            colors.Add((bool)IsGreen.IsChecked ? Collection_Manager.Colors.GREEN : Collection_Manager.Colors.NONE); ;

            colors.mode = (ColorMode)ModeSelection.SelectedIndex;
        }

        public void Clear()
        {
            ModeSelection.SelectedIndex = 2;

            IsWhite.IsChecked = false;
            IsBlue.IsChecked = false;
            IsBlack.IsChecked = false;
            IsRed.IsChecked = false;
            IsGreen.IsChecked = false;

            colors.Clear();
        }
    }
}
