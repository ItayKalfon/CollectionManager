using Collection_Manager.CustomElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for DeckViewer.xaml
    /// </summary>
    public partial class DeckViewer : UserControl
    {
        public SelectionChangedEventHandler onCardSelection;
        IEnumerable<CardStack> stacks
        {
            get
            {
                return Deck.Children.OfType<CardStack>();
            }
            set
            {
                Deck.Children.Clear();
                foreach (var stack in value)
                {
                    Deck.Children.Add(stack);
                }
            }
        }

        public DeckViewer()
        {
            InitializeComponent();
        }

        private void OnFocus(object sender, RoutedEventArgs e)
        {
            onCardSelection(sender, new SelectionChangedEventArgs(e.RoutedEvent, new List<object>(), new List<object>() { ((CardStack)sender).SelectedItem }));
        }

        public void Set(List<Card> cards)
        {
            Deck.Children.Clear();

            List<double> manaCosts = new List<double>();
            foreach(Card card in cards)
            {
                if (!manaCosts.Contains(card.details.cmc))
                {
                    manaCosts.Add(card.details.cmc);
                }
            }

            manaCosts.Sort();

            foreach(double cmc in manaCosts)
            {
                CardStack stack = new CardStack();
                stack.Tag = cmc;
                stack.Width = 205;
                stack.SelectionChanged += onCardSelection;
                stack.GotFocus += OnFocus;
                foreach (Card card in cards.Where(card => card.details.cmc == cmc).ToList())
                {
                    for (int i = 0; i < card.amount; i++)
                    {
                        stack.Add(card);
                    }
                }
                Deck.Children.Add(stack);
            }
        }

        public void Add(Card card)
        {
            var matchingStacks = stacks.Where(stack => (double)stack.Tag == card.details.cmc).ToList();
            if (matchingStacks.Count == 0)
            {
                CardStack stack = new CardStack();
                stack.Tag = card.details.cmc;
                stack.Width = 205;
                stack.SelectionChanged += onCardSelection;
                stack.GotFocus += OnFocus;
                stack.Add(card);
                Deck.Children.Add(stack);
            }
            else
            {
                matchingStacks[0].Add(card);
            }

            Sort();
        }

        public void Remove(Card card)
        {
            var matchingStacks = stacks.Where(stack => (double)stack.Tag == card.details.cmc).ToList();
            if (matchingStacks.Count != 0)
            {
                if(!matchingStacks[0].Remove(card))
                {
                    Deck.Children.Remove(matchingStacks[0]);
                    Sort();
                }
            }
        }

        public void Clear()
        {
            Deck.Children.Clear();
        }

        private void Sort()
        {
            var stacks = Deck.Children.OfType<CardStack>().ToList();
            stacks = stacks.OrderBy(stack => (double)stack.Tag).ToList();
            Deck.Children.Clear();
            stacks.ForEach(stack => Deck.Children.Add(stack));
        }
    }
}
