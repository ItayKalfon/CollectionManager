using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.RegularExpressions;
using System.Windows;
using System.CodeDom;
using System.ComponentModel;

namespace Collection_Manager
{
    class Collection
    {
        private static IMongoCollection<Card> collection;
        static Collection()
        {
            Refresh();
        }

        public static void Refresh()
        {
            collection = DatabaseManager.client.GetDatabase(LoginManager.loggedUser).GetCollection<Card>("Collection");
        }

        public static bool Add(string name, int amount)
        {
            // Check if card doesnt exist
            if (!CardExists(name))
            {
                // Get card details
                Card card = CardFinder.DownloadCard(name);
                if (card == null)
                {
                    // If there is no such card
                    return false;
                }
                else
                {
                    // Update to fixed name
                    name = card.name;
                    // If the fixed name doesnt exist
                    if (!CardExists(name))
                    {
                        // Set the amount and use
                        card.amount = amount;
                        card.used = 0;
                        collection.InsertOne(card); // Insert the new card
                        return true;
                    }
                }
            }
            // If the card exists after or before name fix then update it
            collection.FindOneAndUpdate(GetNameFilter(name), Builders<Card>.Update.Inc("amount", amount));
            return true;
        }

        public static bool Remove(string name, int amount)
        {
            Card card = GetCard(name);
            if (card == null || card.amount - amount < card.used)
            {
                // If all of the cards are used or not enough are in the collection
                return false;
            }
            else
            {
                if (card.amount - amount <= 0)
                {
                    // If all of them are removed delete it
                    collection.DeleteOne(GetNameFilter(name));
                }
                else
                {
                    // Remove the given amount
                    collection.FindOneAndUpdate(GetNameFilter(name), Builders<Card>.Update.Inc("amount", -amount));
                }
                return true;
            }
        }

        public static bool Use(string name, int amount)
        {
            Card card = GetCard(name);
            if (card == null || card.used + amount > card.amount)
            {
                // If the card is not in the collection or there are not enough cards
                return false;
            }
            else
            {
                collection.FindOneAndUpdate(GetNameFilter(name), Builders<Card>.Update.Inc("used", amount)); // Mark the cards as used
                return true;
            }
        }

        public static List<Card> Search(string name = "", CardColor cardColor = null, CardColor colorIdentity = null, string rarity = "", string type = "", string cmc = "", string oracleText = "", bool onlyAvailable = false, string manaCost = "", CardColor producedMana = null, string power = "", string toughness = "", string loyalty = "", string quantity = "")
        {
            // Generate a filter for each field
            var filters = new List<FilterDefinition<Card>>();
            if (name != "")
            {
                filters.Add(generateFilter<string>("details.name", name));
            }
            if (cardColor != null)
            {
                filters.Add(generateFilter<CardColor>("details.colors", cardColor.AsString()));
            }
            if (colorIdentity != null)
            {
                filters.Add(generateFilter<CardColor>("details.color_identity", colorIdentity.AsString()));
            }
            if (rarity != "")
            {
                filters.Add(generateFilter<string>("details.rarity", rarity));
            }
            if (type != "")
            {
                filters.Add(generateFilter<string>("details.type_line", type));
            }
            if (cmc != "")
            {
                filters.Add(generateFilter<int>("details.cmc", cmc));
            }
            if (oracleText != "")
            {
                filters.Add(generateFilter<string>("details.oracle_text", oracleText));
            }
            if (manaCost != "")
            {
                filters.Add(generateFilter<string>("details.mana_cost", manaCost));
            }
            if (producedMana != null)
            {
                filters.Add(generateFilter<CardColor>("details.produced_mana", producedMana.AsString()));
            }
            if (power != "")
            {
                filters.Add(generateFilter<string>("details.power", power));
            }
            if (toughness != "")
            {
                filters.Add(generateFilter<string>("details.toughness", toughness));
            }
            if (loyalty != "")
            {
                filters.Add(generateFilter<string>("details.loyalty", loyalty));
            }
            if (quantity != "")
            {
                filters.Add(generateFilter<int>("amount", quantity));
            }
            if (onlyAvailable)
            {
                return collection.Find(Builders<Card>.Filter.And(filters)).ToEnumerable().Where(card => card.used < card.amount).ToList(); // Filter out the cards that are not available
            }
            return collection.Find(Builders<Card>.Filter.And(filters)).ToList(); // Get the cards with the filters
        }

        public static FilterDefinition<Card> generateFilter<T>(string field, string input)
        {
            string[] phraseSplit = input.Split(new string[] { "\"" }, StringSplitOptions.RemoveEmptyEntries);
            List<string> wordSplit = new List<string>();
            for (int i = 0; i < phraseSplit.Length; i++)
            {
                if (i % 2 == 0)
                {
                    wordSplit.AddRange(phraseSplit[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    wordSplit.Add(phraseSplit[i]);
                }
            }

            List<FilterDefinition<Card>> filters = new List<FilterDefinition<Card>>();

            if (wordSplit.Contains("or"))
            {
                List<FilterDefinition<Card>> orFilters = new List<FilterDefinition<Card>>();
                while (wordSplit.Contains("or"))
                {
                    int orIndex = wordSplit.IndexOf("or");
                    if (wordSplit.Count == 1)
                    {
                        wordSplit.RemoveAt(orIndex);
                        break;
                    }
                    if (orIndex == 0)
                    {
                        orFilters.Add(getFieldFilter<T>(field, wordSplit[orIndex + 1]));
                        wordSplit.RemoveRange(orIndex, 2);
                    }
                    else if (orIndex == wordSplit.Count - 1)
                    {
                        orFilters.Add(getFieldFilter<T>(field, wordSplit[orIndex - 1]));
                        wordSplit.RemoveRange(orIndex - 1, 2);
                    }
                    else
                    {
                        orFilters.Add(getFieldFilter<T>(field, wordSplit[orIndex - 1]));
                        orFilters.Add(getFieldFilter<T>(field, wordSplit[orIndex + 1]));
                        wordSplit.RemoveRange(orIndex - 1, 3);
                    }
                }
                filters.Add(Builders<Card>.Filter.Or(orFilters));
            }

            if (wordSplit.Contains(":"))
            {
                List<FilterDefinition<Card>> rangeFilters = new List<FilterDefinition<Card>>();
                while (wordSplit.Contains(":"))
                {
                    int rangeIndex = wordSplit.IndexOf(":");
                    if (rangeIndex == 0 || rangeIndex == wordSplit.Count - 1)
                    {
                        wordSplit.RemoveAt(rangeIndex);
                    }
                    else
                    {
                        int leftOperand;
                        int rightOperand;
                        if (int.TryParse(wordSplit[rangeIndex - 1], out leftOperand) && int.TryParse(wordSplit[rangeIndex + 1], out rightOperand))
                        {
                            for (int i = leftOperand; i <= rightOperand; i++)
                            {
                                rangeFilters.Add(getFieldFilter<T>(field, i.ToString()));
                            }
                        }

                        wordSplit.RemoveRange(rangeIndex - 1, 3);
                    }
                }
                filters.Add(Builders<Card>.Filter.Or(rangeFilters));
            }

            wordSplit.ForEach(word => filters.Add(getFieldFilter<T>(field, word)));

            return Builders<Card>.Filter.And(filters);
        }

        private static FilterDefinition<Card> getFieldFilter<T>(string field, string value)
        {
            if (typeof(T) == typeof(string))
            {
                if (value[0] == '-')
                {
                    return Builders<Card>.Filter.Not(Builders<Card>.Filter.Regex(field, new Regex(Regex.Escape(new string(value.Skip(1).ToArray())), RegexOptions.IgnoreCase)));
                }
                else
                {
                    return Builders<Card>.Filter.Regex(field, new Regex(Regex.Escape(value), RegexOptions.IgnoreCase));
                }
            }
            else if (typeof(T) == typeof(int))
            {
                return Builders<Card>.Filter.Eq(field, int.Parse(value));
            }
            else if (typeof(T) == typeof(CardColor))
            {
                if (value[0] == '-')
                {
                    return Builders<Card>.Filter.Not(Builders<Card>.Filter.AnyEq(field, value[1].ToString()));
                }
                else
                {
                    return Builders<Card>.Filter.AnyEq(field, value);
                }
            }
            else
            {
                return null;
            }
        }

        private static bool CardExists(string name)
        {
            return GetCard(name) != null;
        }

        private static FilterDefinition<Card> GetNameFilter(string name)
        {
            var regular = Builders<Card>.Filter.Regex("name", new Regex('^' + name + '$', RegexOptions.IgnoreCase));
            var before = Builders<Card>.Filter.Regex("name", new Regex("// " + name + '$', RegexOptions.IgnoreCase));
            var after = Builders<Card>.Filter.Regex("name", new Regex('^' + name + " //", RegexOptions.IgnoreCase));
            return Builders<Card>.Filter.Or(regular, before, after);
        }

        public static Card GetCard(string name)
        {
            return collection.Find(GetNameFilter(name)).FirstOrDefault();
        }

        public static string AsString()
        {
            string result = "";
            foreach (Card card in collection.Find(Builders<Card>.Filter.Where(_ => true)).ToList())
            {
                result += card.amount + " " + card.name + "\n";
            }
            return result;
        }
    }
}
