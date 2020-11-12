using Collection_Manager.UserControls;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collection_Manager
{
    public enum ColorMode
    {
        EXACT = 0,
        ANY,
        ALL
    }

    public enum Colors
    {
        WHITE = 0,
        BLUE,
        BLACK,
        RED,
        GREEN,
        NONE
    }

    public static class ColorsExtension
    {
        public static string AsString(this Colors color)
        {
            // Helper method to convert enum to string
            switch (color)
            {
                case Colors.WHITE:
                    return "W";
                case Colors.BLUE:
                    return "U";
                case Colors.BLACK:
                    return "B";
                case Colors.RED:
                    return "R";
                case Colors.GREEN:
                    return "G";
                default:
                    return "";
            }
        }
    }

    public class CardColor
    {
        public ColorMode mode;

        private Dictionary<Colors, bool> colors;

        public CardColor()
        {
            // Init the dictionary that holds whether a color is used or not
            colors = new Dictionary<Colors, bool>() { { Colors.WHITE, false }, { Colors.BLUE, false }, { Colors.BLACK, false }, { Colors.RED, false }, { Colors.GREEN, false } };
            mode = ColorMode.ALL;
        }

        public CardColor(Colors color) : this()
        {
            Add(color);
        }

        public void Add(Colors color)
        {
            // Set the color
            if (color != Colors.NONE)
            {
                colors[color] = true;
            }
        }
        
        public string AsString()
        {
            string result = "";
            if (mode == ColorMode.ALL)
            {
                // If the mode is all chain them spaces
                foreach (var color in colors)
                {
                    if (color.Value)
                    {
                        result += color.Key.AsString() + " ";
                    }
                }
            }
            else if (mode == ColorMode.ANY)
            {
                // If the mode is any chain the color with or between them and use - on the colors that aren't selected
                foreach (var color in colors)
                {
                    if (color.Value)
                    {
                        result += color.Key.AsString() + " or ";
                    }
                }

                foreach (var color in colors)
                {
                    if (!color.Value)
                    {
                        result += " -" + color.Key.AsString();
                    }
                }
            }
            else
            {
                // If the mode is exact chain the colors with spaces and - before those which aren't used
                foreach (var color in colors)
                {
                    if (!color.Value)
                    {
                        result += "-";
                    }
                    result += color.Key.AsString() + " ";
                }
            }
            return result;
        }

        public void Clear()
        {
            // Set all of the colors to not used
            foreach (var isColor in colors.Keys.ToList())
            {
                colors[isColor] = false;
            }
            mode = ColorMode.ALL;
        }
    }
}
