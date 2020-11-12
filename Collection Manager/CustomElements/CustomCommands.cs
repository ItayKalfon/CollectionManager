using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Collection_Manager.CustomElements
{
    static public class CustomCommands
    {
        // A class that holds all of the shortcut commands
        public static RoutedCommand EditCollection = new RoutedCommand();
        public static RoutedCommand BrowseCollection = new RoutedCommand();
        public static RoutedCommand DeckBuilder = new RoutedCommand();
        public static RoutedCommand LoadFile = new RoutedCommand();
        public static RoutedCommand ExportFile = new RoutedCommand();
        public static RoutedCommand LandCalculator = new RoutedCommand();
    }
}
