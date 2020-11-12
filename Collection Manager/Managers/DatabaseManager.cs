using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Collection_Manager
{
    class DatabaseManager
    {
        public static MongoClient client;

        static DatabaseManager()
        {
            try
            {
                client = new MongoClient(File.ReadAllLines(Environment.CurrentDirectory + @"\Assets\mongo_config.txt")[0]); // Get connection string from the config file
            }
            catch
            {
                File.AppendAllText(Environment.CurrentDirectory + @"\Assets\log.txt", "The given connection string is invalid."); // If there was a config error log it.
                Environment.Exit(1); // Shut down if there was a config error
            }
        }

        public static List<string> getUserCollections(string username)
        {
            return client.GetDatabase(username).ListCollectionNames().ToList(); // Get all users from the database
        }
    }
}
