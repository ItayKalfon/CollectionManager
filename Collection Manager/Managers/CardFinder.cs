using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Net.Http;
using System.Net;
using System.Text.Json;

namespace Collection_Manager
{

    class CardFinder
    {
        static HttpClient cardSocket;
        static WebClient imageSocket;

        static CardFinder()
        {
            // Init both sockets
            cardSocket = new HttpClient();
            imageSocket = new WebClient();

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            cardSocket.BaseAddress = new Uri("https://api.scryfall.com");
        }

        ~CardFinder()
        {
            // Close sockets
            cardSocket.CancelPendingRequests();
            imageSocket.CancelAsync();
        }

        public static Card DownloadCard(string cardName)
        { 
            Task<HttpResponseMessage> cardRequest = cardSocket.GetAsync("cards/named?exact=" + cardName); // Get the card from ScryFall's API
            if (cardRequest.Result.StatusCode == HttpStatusCode.OK) // If a card was found
            {
                return new Card(cardRequest.Result.Content.ReadAsStringAsync().Result); // Return the card
            }
            return null; // Return null if nothing was found
        }
    }
}
