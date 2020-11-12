using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;
using System.IO;

namespace Collection_Manager
{
    class LoginManager
    {
        public static IMongoCollection<User> users;
        public static string loggedUser; // Current view
        public static string originalLoggedUser; // The logged user

        // User class to store in the database
        public class User
        {
            [BsonId] [JsonIgnore]
            public ObjectId _id { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            [JsonIgnore]
            public List<string> has_access { get; set; } // The user's friends
            public User()
            {
                has_access = new List<string>();
            }
            public User(string username, string password) : this()
            {
                this.username = username;
                this.password = password;
            }
            public UserCredentials GetCredentials()
            {
                return new UserCredentials { username = username, password = password };
            }
        }

        // Class to store credentials in the "remember me" file
        public struct UserCredentials
        {
            public string username { get; set; }
            public string password { get; set; }
        }

        public enum LoginState
        {
            SUCESS = 0,
            INCORRECT_PASSWORD,
            INCORRECT_USERNAME,
            USERNAME_TAKEN
        }

        static LoginManager()
        {
            users = DatabaseManager.client.GetDatabase("Users").GetCollection<User>("Users"); // Get users
            loggedUser = "";
            originalLoggedUser = "";
        }

        public static LoginState Signup(string username, string password)
        {
            if (users.Find(Builders<User>.Filter.Regex("username", new Regex('^' + username + '$', RegexOptions.IgnoreCase))).FirstOrDefault() != null)
            {
                // If a user already has this name
                return LoginState.USERNAME_TAKEN;
            }
            // Sign up the user
            users.InsertOne(new User(username, password));
            originalLoggedUser = username;
            loggedUser = username.Replace(" ", "");
            Deck.UpdateDecks();
            return LoginState.SUCESS;
        }

        public static LoginState Login(string username, string password)
        {
            var currUser = users.Find(Builders<User>.Filter.Regex("username", new Regex('^' + username + '$', RegexOptions.IgnoreCase))).FirstOrDefault();
            if (currUser == null)
            {
                // If no user by that name was found
                return LoginState.INCORRECT_USERNAME;
            }
            if (currUser.password != password)
            {
                // If the password is incorrect
                return LoginState.INCORRECT_PASSWORD;
            }
            // Log the user in
            originalLoggedUser = currUser.username;
            loggedUser = currUser.username.Replace(" ", "");
            Deck.UpdateDecks();
            return LoginState.SUCESS;
        }

        public static bool Logout()
        {
            if (loggedUser == "")
            {
                // Check if a user is logged in
                return false;
            }
            else
            {
                // Log out and delete the remember me file
                loggedUser = "";
                originalLoggedUser = "";
                File.Delete(Environment.CurrentDirectory + @"\Assets\config.json");
                Deck.decks.Clear();
                return true;
            }
        }
    }
}
