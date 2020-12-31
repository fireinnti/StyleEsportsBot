using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using player = DiscordBotApp.Modules.Player;
namespace DiscordBotApp
{
    class MongoDB
    {
        //gets info on certain player
        public string player()
        {

            var connectionString = "mongodb://localhost:27017";

            var client = new MongoClient(connectionString);
            Console.WriteLine(client);
          var database = client.GetDatabase("StyleData");
            //starts a collection
            var collection = database.GetCollection<BsonDocument>("Player");
            //filters unneeded stored info
            var projection = Builders<BsonDocument>.Projection.Exclude("_id").Exclude("SummonerId");
            var document = collection.Find(new BsonDocument()).Project(projection).FirstOrDefault();
            Console.WriteLine(document.ToString());
            return document.ToString();
        }

        public async Task inputPlayer(object player)
        {
            //connects to localhost
            var connectionString = "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);
            //applies player object to new object in this task

            var database = client.GetDatabase("StyleData");
            //starts a collection
            var collection = database.GetCollection<BsonDocument>("Player");
            player mongoPlayer = (player)player;


            var document = new BsonDocument { { "Ign", mongoPlayer.Ign },
                { "Rank", mongoPlayer.Rank },
                {"SummonerId", mongoPlayer.SummonerId },
                {"CurrentTeam", mongoPlayer.CurrentTeam }
                
                
                
        };
            await collection.InsertOneAsync(document);

        }

        //looks up player from database
        public string[] playerLookup(string [] players)
        {
            
            var connectionString = "mongodb://localhost:27017";

            string[] listOfPlayers = new string[players.Length];
            int numCount = 0;

            var client = new MongoClient(connectionString);
            Console.WriteLine(client);
            var database = client.GetDatabase("StyleData");
            //starts a collection
            var collection = database.GetCollection<BsonDocument>("Player");
            //filters unneeded stored info
            var projection = Builders<BsonDocument>.Projection.Exclude("_id").Exclude("SummonerId").Exclude("Team").Exclude("LossesThisSeason").Exclude("TotalLosses").Exclude("TotalWins").Exclude("WinsThisSeason");
            //creates useable doc of players
            BsonDocument collectionOfPlayers;


            FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;
            FilterDefinition<BsonDocument> filter;
            

            foreach(var player in players)
            {
                filter = builder.Eq("Ign", players[numCount]);
                collectionOfPlayers = collection.Find(filter).Project(projection).FirstOrDefault();

                listOfPlayers[numCount] = collectionOfPlayers["Ign"].ToString() + " " + collectionOfPlayers["Rank"].ToString();
                numCount++;
            }

            Console.WriteLine(listOfPlayers);

            
            return listOfPlayers;

        }


        //gets info on certain team
        public  BsonDocument team(string team)
        {

            var connectionString = "mongodb://localhost:27017";

            var client = new MongoClient(connectionString);
            Console.WriteLine(client);
            var database = client.GetDatabase("StyleData");
            //starts a collection
            var teamCollection = database.GetCollection<BsonDocument>("Team");
            var playerCollection = database.GetCollection<BsonDocument>("Player");
            //filters unneeded stored info
            var teamProjection = Builders<BsonDocument>.Projection.Exclude("_id");
            //gets specific team
            var teamFilter = Builders<BsonDocument>.Filter.Eq("TeamName", team);

            // var document = collection.Find(new BsonDocument()).Project(projection).FirstOrDefault();
            BsonDocument document = teamCollection.Find(teamFilter).Project(teamProjection).FirstOrDefault();


           //FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;
           //FilterDefinition<BsonDocument> filter = builder.Eq("Ign", document["Top"]) & builder.Eq("Ign", document["Jg"]) & builder.Eq("Ign", document["Mid"]) & builder.Eq("Ign", document["Adc"]) & builder.Eq("Ign", document["Sup"]);


           // var listOfPlayers = playerCollection.Find(filter).FirstOrDefault();

            
            
          // Console.WriteLine(listOfPlayers);
            
            return document;
        }
    }
}
