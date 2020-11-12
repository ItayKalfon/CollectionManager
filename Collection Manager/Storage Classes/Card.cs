using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;
using MongoDB.Bson.Serialization.Attributes;
using System.Net.NetworkInformation;
using System.Windows.Controls.Primitives;

namespace Collection_Manager
{
    public class Card
    {
        [BsonId]
        public MongoDB.Bson.ObjectId _id { get; set; }
        public string name { get; set; }
        public CardDetails details { get; set; }
        public int used { get; set; }
        public int amount { get; set; }

        public Card(string cardJson)
        { 
            // Handle different card layouts
            details = JsonSerializer.Deserialize<CardDetails>(cardJson);
            if (details.layout == "transform" || details.layout == "modal_dfc")
            {
                details.mana_cost = "";
                details.oracle_text = "";
                details.colors = new List<string>();
                details.power = "";
                details.toughness = "";
                foreach (CardFace face in details.card_faces)
                {
                    details.mana_cost += " // " + face.mana_cost;
                    details.oracle_text += " // " + face.oracle_text;
                    face.colors.ForEach(color => {
                        if (!details.colors.Contains(color))
                        {
                            details.colors.Add(color);
                        }
                    });
                    details.power += " // " + face.power;
                    details.toughness += " // " + face.toughness;
                }
                details.artist = details.card_faces[0].artist;
                details.image_uris = details.card_faces[0].image_uris;

                details.mana_cost = details.mana_cost.Remove(0, 4);
                details.oracle_text = details.oracle_text.Remove(0, 4);
                details.power = details.power.Remove(0, 4);
                details.toughness = details.toughness.Remove(0, 4);
            }
            else if (details.layout == "split" || details.layout == "adventure")
            {
                details.oracle_text = "";
                foreach (CardFace face in details.card_faces)
                {
                    details.oracle_text += " // " + face.oracle_text;
                }
                details.oracle_text = details.oracle_text.Remove(0, 4);
            }
            name = details.name;
        }

        public Card(string cardJson, int amount, int used) : this(cardJson)
        {
            this.used = used;
            this.amount = amount;
        }
    }

    public class ImageUris
    {
        public string small { get; set; }
        public string normal { get; set; }
        public string large { get; set; }
        public string png { get; set; }
        public string art_crop { get; set; }
        public string border_crop { get; set; }
    }

    public class Legalities
    {
        public string standard { get; set; }
        public string future { get; set; }
        public string historic { get; set; }
        public string pioneer { get; set; }
        public string modern { get; set; }
        public string legacy { get; set; }
        public string pauper { get; set; }
        public string vintage { get; set; }
        public string penny { get; set; }
        public string commander { get; set; }
        public string brawl { get; set; }
        public string duel { get; set; }
        public string oldschool { get; set; }
    }

    public class Preview
    {
        public string source { get; set; }
        public string source_uri { get; set; }
        public string previewed_at { get; set; }
    }

    public class Prices
    {
        public string usd { get; set; }
        public string usd_foil { get; set; }
        public string eur { get; set; }
        public string tix { get; set; }
    }

    public class RelatedUris
    {
        public string gatherer { get; set; }
        public string tcgplayer_decks { get; set; }
        public string edhrec { get; set; }
        public string mtgtop8 { get; set; }
    }

    public class PurchaseUris
    {
        public string tcgplayer { get; set; }
        public string cardmarket { get; set; }
        public string cardhoarder { get; set; }
    }

    public class CardFace
    {
        [BsonIgnore]
        public string name { get; set; }
        [BsonIgnore]
        public string mana_cost { get; set; }
        [BsonIgnore]
        public string type_line { get; set; }
        [BsonIgnore]
        public string oracle_text { get; set; }
        [BsonIgnore]
        public List<string> colors { get; set; }
        [BsonIgnore]
        public string power { get; set; }
        [BsonIgnore]
        public string toughness { get; set; }
        [BsonIgnore] [JsonIgnore]
        public string artist { get; set; }
        [BsonIgnore] [JsonIgnore]
        public string artist_id { get; set; }
        [BsonIgnore] [JsonIgnore]
        public string illustration_id { get; set; }
        [BsonIgnore]
        public ImageUris image_uris { get; set; }
        [BsonIgnore] [JsonIgnore]
        public List<string> color_indicator { get; set; }
    }

    public class CardDetails
    {
        [JsonPropertyName("object")]
        public string _object { get; set; }
        public string id { get; set; }
        public string oracle_id { get; set; }
        public List<int> multiverse_ids { get; set; }
        public int mtgo_id { get; set; }
        public int tcgplayer_id { get; set; }
        public string name { get; set; }
        public string lang { get; set; }
        public string released_at { get; set; }
        public string uri { get; set; }
        public string scryfall_uri { get; set; }
        public string layout { get; set; }
        public bool highres_image { get; set; }
        public ImageUris image_uris { get; set; }
        public string mana_cost { get; set; }
        public double cmc { get; set; }
        public string type_line { get; set; }
        public string oracle_text { get; set; }
        public List<string> colors { get; set; }
        public List<string> color_identity { get; set; }
        public List<string> keywords { get; set; }
        public List<string> produced_mana { get; set; }
        public Legalities legalities { get; set; }
        public List<string> games { get; set; }
        public bool reserved { get; set; }
        public bool foil { get; set; }
        public bool nonfoil { get; set; }
        public bool oversized { get; set; }
        public bool promo { get; set; }
        public bool reprint { get; set; }
        public bool variation { get; set; }
        public string set { get; set; }
        [JsonPropertyName("set_name")]
        public string _set_name { get; set; }
        public string set_type { get; set; }
        [JsonPropertyName("set_uri")]
        public string _set_uri { get; set; }
        public string set_search_uri { get; set; }
        public string scryfall_set_uri { get; set; }
        public string rulings_uri { get; set; }
        public string prints_search_uri { get; set; }
        public string collector_number { get; set; }
        public bool digital { get; set; }
        public string rarity { get; set; }
        public string card_back_id { get; set; }
        public string artist { get; set; }
        public List<string> artist_ids { get; set; }
        public string illustration_id { get; set; }
        public string border_color { get; set; }
        public string frame { get; set; }
        public bool full_art { get; set; }
        public bool textless { get; set; }
        public bool booster { get; set; }
        public bool story_spotlight { get; set; }
        public int edhrec_rank { get; set; }
        public Preview preview { get; set; }
        public Prices prices { get; set; }
        public RelatedUris related_uris { get; set; }
        public PurchaseUris purchase_uris { get; set; }
        public string power { get; set; }
        public string toughness { get; set; }
        [BsonIgnore]
        public List<CardFace> card_faces { get; set; }
        public string loyalty { get; set; }
    }
}
