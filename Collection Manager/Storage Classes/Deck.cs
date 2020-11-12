using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using System.Windows;

namespace Collection_Manager
{
    class Deck
    {
        public static List<string> decks;

        public string name;
        public IMongoCollection<Card> deck;

        public int CardAmount
        {
            get
            {
                int amount = 0;
                foreach (Card card in GetCards())
                {
                    amount += card.amount;
                }
                return amount;
            }
            set
            {
                CardAmount = value;
            }
        }

        public Dictionary<Colors, int> Symbols
        {
            get
            {
                var result = new Dictionary<Colors, int>();
                result[Colors.WHITE] = 0;
                result[Colors.BLUE] = 0;
                result[Colors.BLACK] = 0;
                result[Colors.RED] = 0;
                result[Colors.GREEN] = 0;
                foreach (Card card in GetCards())
                {
                    result[Colors.WHITE] += card.details.mana_cost.Count(symbol => symbol == 'W') * card.amount;
                    result[Colors.BLUE] += card.details.mana_cost.Count(symbol => symbol == 'U') * card.amount;
                    result[Colors.BLACK] += card.details.mana_cost.Count(symbol => symbol == 'B') * card.amount;
                    result[Colors.RED] += card.details.mana_cost.Count(symbol => symbol == 'R') * card.amount;
                    result[Colors.GREEN] += card.details.mana_cost.Count(symbol => symbol == 'G') * card.amount;
                }
                return result;
            }
            set
            {
                Symbols = value;
            }
        }

        public int Lands
        {
            get
            {
                int amount = 0;
                foreach (Card card in deck.Find(Builders<Card>.Filter.Regex("details.type_line", new Regex("Land", RegexOptions.IgnoreCase))).ToList())
                {
                    amount += card.amount;
                }
                return amount;
            }
            set
            {
                Lands = value;
            }
        }

        public int NonLands
        {
            get
            {
                return CardAmount - Lands;
            }
            set
            {
                NonLands = value;
            }
        }

        public CardColor DeckColors
        {
            get
            {
                var colors = new CardColor();
                colors.mode = ColorMode.ALL;
                foreach (var symbol in Symbols)
                {
                    if (symbol.Value != 0)
                    {
                        colors.Add(symbol.Key);
                    }
                }
                return colors;
            }
            set
            {
                DeckColors = value;
            }
        }

        public static void UpdateDecks()
        {
            decks = DatabaseManager.getUserCollections(LoginManager.loggedUser);
            decks.Remove("Collection");
        }

        public static void CreateDeck(string name)
        {
            DatabaseManager.client.GetDatabase(LoginManager.loggedUser).CreateCollection(name);
            decks.Add(name);
        }

        public static void DeleteDeck(string name)
        {
            if (decks.Where(deck => deck.ToLower() == name.ToLower()).ToList().Count != 0)
            {
                Deck currDeck = new Deck(name);
                foreach (Card card in currDeck.GetCards())
                {
                    currDeck.Remove(card.name, card.amount);
                }
                DatabaseManager.client.GetDatabase(LoginManager.loggedUser).DropCollection(name);
                decks.Remove(name);
            }
        }

        public Deck(string deckName)
        {
            var matches = decks.Where(name => name.ToLower() == deckName.ToLower()).ToList();
            if (matches.Count == 0)
            {
                return;
            }
            else
            {
                name = matches[0];
                deck = DatabaseManager.client.GetDatabase(LoginManager.loggedUser).GetCollection<Card>(name);
            }
        }

        public bool Add(string name, int amount, bool toCollection = false)
        {
            if (toCollection)
            {
                Collection.Add(name, amount);
                return Add(name, amount);
            }
            else
            {
                Card collectionCard = Collection.GetCard(name);
                if (!Collection.Use(name, amount))
                {
                    return false;
                }
                else
                {
                    Card deckCard = GetCard(name);
                    if (deckCard != null)
                    {
                        deck.FindOneAndUpdate(Builders<Card>.Filter.Regex("name", new Regex(name, RegexOptions.IgnoreCase)), Builders<Card>.Update.Inc("amount", amount));
                    }
                    else
                    {
                        collectionCard.used = 0;
                        collectionCard.amount = amount;
                        deck.InsertOne(collectionCard);
                    }
                    return true;
                }
            }
        }

        public bool Remove(string name, int amount, bool fromCollection = false)
        {
            if (fromCollection)
            {
                if (Remove(name, amount))
                {
                    Collection.Remove(name, amount);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                Card card = GetCard(name);
                if (card == null || card.amount - amount < 0)
                {
                    return false;
                }
                else
                {
                    Collection.Use(name, -amount);
                    if (card.amount - amount == 0)
                    {
                        deck.DeleteOne(Builders<Card>.Filter.Regex("name", new Regex('^' + name + '$', RegexOptions.IgnoreCase)));
                    }
                    else
                    {
                        deck.FindOneAndUpdate(Builders<Card>.Filter.Regex("name", new Regex(name, RegexOptions.IgnoreCase)), Builders<Card>.Update.Inc("amount", -amount));
                    }
                    return true;
                }
            }
        }

        public string AsString()
        {
            string result = "";
            foreach (Card card in GetCards())
            {
                result += card.amount + " " + card.name + "\n";
            }
            return result;
        }

        public List<Card> GetCards()
        {
            return deck.Find(Builders<Card>.Filter.Empty).ToList();
        }

        public Card GetCard(string name)
        {
            return deck.Find(Builders<Card>.Filter.Regex("name", new Regex('^' + name + '$', RegexOptions.IgnoreCase))).FirstOrDefault();
        }
    }
}
