using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;

namespace Collection_Manager
{
    class CardImage
    {
        private static WebClient client;
        private static Dictionary<string, CardImage> cache; // All of the downloaded images, to save time
        private Card card;
        public Bitmap image { get; }


        static CardImage()
        {
            // Init client
            client = new WebClient();
            cache = new Dictionary<string, CardImage>();
        }

        public CardImage(Card source)
        {
            var matches = cache.Keys.Where(key => key.ToLower() == source.name.ToLower()).ToList(); // See if that image was downloaded already
            if (matches.Count == 0)
            {
                // If the image was not found, download the image
                card = source;
                Stream imageStream = client.OpenRead(card.details.image_uris.normal);
                image = new Bitmap(imageStream);
                cache[card.name] = this;
            }
            else
            {
                // If it was found, use that image
                card = cache[matches[0]].card;
                image = cache[matches[0]].image;
            }
        }

        public CardImage(string cardName) : this(Collection.GetCard(cardName))
        {
        }

        public BitmapImage ToBitmapImage()
        {
            // Convert image to bitmapimage by saving it on a memory stream and opening it
            using (var memory = new MemoryStream())
            {
                image.Save(memory, ImageFormat.Jpeg);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}
