using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.Json;
using System.IO.Ports;
using System.IO;

namespace Collection_Manager.Server_Extension
{
    class Server
    {
        private static TcpListener socket;
        private const int BUFFER_SIZE = 1024;
        private static bool isActive;
        static Server()
        {
            // Init the listening socket
            socket = new TcpListener(IPAddress.Any, 2212);
            isActive = true;
        }
        ~Server()
        {
            socket.Stop();
        }

        public static void Stop()
        {
            // Stop the socket
            isActive = false;
            socket.Stop();
        }

        public static void AcceptClients()
        {
            socket.Start(); // Start listening dor clients
            new Thread(() => 
            {
                while (isActive) // While the server is running
                {
                    try
                    {
                        TcpClient client = socket.AcceptTcpClient(); // Wait for a client to connect
                        new Thread(() =>
                        {
                            // Start handling the new client in a seperate thread
                            HandleClient(client);
                        }).Start();
                    }
                    catch
                    {
                        // Stop the loop if an error occurs
                        break;
                    }      
                }
            }).Start(); // On a new thread to accept multiple clients
        }

        public static void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream(); // Get the message stream
            while (client.Connected && isActive) // Run as long as the server is active and the client is connected
            {
                while (!stream.DataAvailable) // Wait for data to be available
                {
                    if (!isActive)
                    {
                        // If the server was deactivated while waiting close the thread
                        break;
                    }
                }

                // Get the request
                byte[] requestBytes = new byte[BUFFER_SIZE];
                int bytesRead = stream.Read(requestBytes, 0, BUFFER_SIZE);

                Request request = JsonSerializer.Deserialize<Request>(Encoding.ASCII.GetString(requestBytes, 0, bytesRead));

                // Log the request
                File.AppendAllText(Environment.CurrentDirectory + @"\Assets\log.txt", "Recieved: " + JsonSerializer.Serialize(request) + ".\n");

                // Generate a new empty response
                Response response = new Response() { type = request.type };

                if (LoginManager.loggedUser != "" && LoginManager.loggedUser == LoginManager.originalLoggedUser.Replace(" ", "")) // If the view is on the logged user
                {
                    if (request.type == "Decks")
                    {
                        // If the request is to get the decks, return the list of decks
                        response.result = string.Join(",", Deck.decks);
                    }
                    else if (request.type == "Add")
                    {
                        // If the request is to add a card
                        if (request.location.Length > 1) // Test if its for the collection or a deck
                        {
                            if (request.location[request.location.Length - 1] == '+')
                            {
                                // If for a deck and collection add it to both
                                Deck deck = new Deck(new string(request.location.Take(request.location.Length - 1).ToArray()));
                                response.result = deck.Add(request.name, 1, true).ToString();
                            }
                            else
                            {
                                // If only for a deck add it to the deck
                                Deck deck = new Deck(request.location);
                                response.result = deck.Add(request.name, 1, false).ToString();
                            }
                        }
                        else
                        {
                            if (request.location == "+")
                            {
                                // If only for collection
                                response.result = Collection.Add(request.name, 1).ToString();
                            }
                            else
                            {
                                // If data matches nothing
                                response.result = "False";
                            }
                        }
                    }
                    else if (request.type == "Remove")
                    {
                        // If the request is to remove
                        if (request.location.Length > 1)
                        {
                            if (request.location[request.location.Length - 1] == '+')
                            {
                                // If the request is for collection and deck
                                Deck deck = new Deck(new string(request.location.Take(request.location.Length - 1).ToArray()));
                                response.result = deck.Remove(request.name, 1, true).ToString();
                            }
                            else
                            {
                                // If only for deck
                                Deck deck = new Deck(request.location);
                                response.result = deck.Remove(request.name, 1, false).ToString();
                            }
                        }
                        else
                        {
                            if (request.location == "+")
                            {
                                // If only for collection
                                response.result = Collection.Remove(request.name, 1).ToString();
                            }
                            else
                            {
                                // If nothing matches
                                response.result = "False";
                            }
                        }
                    }
                }
                else
                {
                    // If request type matches nothing
                    response.result = "False";
                }

                // Return the response
                byte[] responseArray = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(response));
                stream.Write(responseArray, 0, responseArray.Length);

                // Log the response
                File.AppendAllText(Environment.CurrentDirectory + @"\Assets\log.txt", "Sent: " + JsonSerializer.Serialize(response) + ".\n");
            }
        }

        public static string GetLocalIPAddress()
        {
            // Get the current computer's local ip address by using a DNS query
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
