using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using player = DiscordBotApp.Modules.Player;
using team = DiscordBotApp.Modules.Team;
using System.Configuration;
namespace DiscordBotApp
{
    class MongoDB
    {
        //gets info on certain player
        public string player()
        {

            string mongoKey;
            mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");

            var connectionString = mongoKey;

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

        //change status to paid
        public async Task<bool> paid(string teamName)
        {
            string mongoKey;
            mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");

            var connectionString = mongoKey;
            var client = new MongoClient(connectionString);
            //applies player object to new object in this task

            var database = client.GetDatabase("StyleData");
            var collection = database.GetCollection<BsonDocument>("Team");


            var teamFilter = Builders<BsonDocument>.Filter.Eq("TeamName", teamName);

            var checkTeamExists = await collection.Find(teamFilter).FirstOrDefaultAsync();
            if(checkTeamExists !=null)
            {
                var updateDocument = Builders<BsonDocument>.Update.Set("Paid", true);

                await collection.UpdateOneAsync(teamFilter, updateDocument);
                return true;
            }
            else
            {
                return false;
            }
        }

        //checks if team paid
        public async Task<bool> checkPaid(string teamName)
        {
            string mongoKey;
            mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");

            var connectionString = mongoKey;
            var client = new MongoClient(connectionString);
            //applies player object to new object in this task

            var database = client.GetDatabase("StyleData");
            var collection = database.GetCollection<BsonDocument>("Team");

           
            var teamFilter = Builders<BsonDocument>.Filter.Eq("TeamName", teamName);

            var checkTeamExists = await collection.Find(teamFilter).FirstOrDefaultAsync();
            if (checkTeamExists != null)
            {
                BsonElement paid;
                checkTeamExists.TryGetElement("Paid", out paid);
                bool hasPaid = paid.Value.AsBoolean;

                return hasPaid;
            }
            else
            {
                return false;
            }
        }



        //inputs player
        public async Task<bool> inputPlayer(object player)
        {
            //connects to localhost
            string mongoKey;
            mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");

            var connectionString = mongoKey;
            var client = new MongoClient(connectionString);
            //applies player object to new object in this task

            var database = client.GetDatabase("StyleData");
            //starts a collection
            var collection = database.GetCollection<BsonDocument>("Player");

           

            player mongoPlayer = (player)player;

            var playerFilter = Builders<BsonDocument>.Filter.Eq("SummonerId", mongoPlayer.SummonerId);

            var checkPlayerExists = collection.Find(playerFilter).FirstOrDefault();
            if (checkPlayerExists != null)
            {
                var updateDocument = Builders<BsonDocument>.Update.Set("Ign", mongoPlayer.Ign).
                    Set("Rank", mongoPlayer.Rank).
                    Set("CurrentTeam", mongoPlayer.CurrentTeam).
                    Set("DiscordName", mongoPlayer.DiscordName);
                
                

               
                await collection.UpdateOneAsync(playerFilter, updateDocument);
                return true; 
            }

            //creates a new player in player collection
            var document = new BsonDocument { { "Ign", mongoPlayer.Ign },
                { "Rank", mongoPlayer.Rank },
                {"SummonerId", mongoPlayer.SummonerId },
                {"CurrentTeam", mongoPlayer.CurrentTeam },
                {"LossesThisSeason", mongoPlayer.LossesThisSeason },
                {"TotalLosses",mongoPlayer.TotalLosses },
                {"TotalWins",mongoPlayer.TotalWins },
                {"WinsThisSeason", mongoPlayer.WinsThisSeason },
                {"DiscordName" , mongoPlayer.DiscordName }, 
                {"AltNames", new BsonArray{ } },
                {"PreviousTeams", new BsonArray{} }

                
                
                
                
        };
            await collection.InsertOneAsync(document);
            return false;
        }

        public async Task<bool> updateTeam(string[] whatToUpdate)
        {
            try
            {
                string mongoKey;
                mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");

                string teamName = whatToUpdate[0];
                string keyName = whatToUpdate[1];
                string propertyForKey = whatToUpdate[2];

                var connectionString = mongoKey;
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase("StyleData");
                //starts a collection
                var collection = database.GetCollection<BsonDocument>("Team");
                var filter = Builders<BsonDocument>.Filter.Eq("TeamName", teamName);

                var update = Builders<BsonDocument>.Update.Set(keyName, propertyForKey);
                if (whatToUpdate[1] == "Subs")
                {
                     update = Builders<BsonDocument>.Update.Push(keyName, propertyForKey);
                }else
                {
                     update = Builders<BsonDocument>.Update.Set(keyName, propertyForKey);
                }
                await collection.UpdateOneAsync(filter, update);
                return true;
            }
            catch
            {
                return false;
            }


        }
        //inputsTeam
        public async Task inputTeam(object team)
        {
            //connects to localhost
            string mongoKey;
            mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");

            var connectionString = mongoKey;
            var client = new MongoClient(connectionString);
            //applies player object to new object in this task

            var database = client.GetDatabase("StyleData");
            //starts a collection
            var collection = database.GetCollection<BsonDocument>("Team");
            team mongoTeam = (team)team;
            
            //making sure to get each array into bson array
            BsonArray subs = new BsonArray();
            foreach (var player in mongoTeam.Subs)
            {
                subs.Add(player);
            }

            BsonArray captain = new BsonArray();
            foreach (var player in mongoTeam.Captain)
            {
                captain.Add(player);
            }
            BsonArray Esubs = new BsonArray();
            foreach (var player in mongoTeam.ESubs)
            {
                Esubs.Add(player);
            }
            BsonArray Staff = new BsonArray();
            foreach (var player in mongoTeam.Staff)
            {
                Staff.Add(player);
            }
            Console.WriteLine("Made it before document initialization");
            //creates a new player in player collection

            var document = new BsonDocument {
                { "Top", mongoTeam.Top },
                { "Jg", mongoTeam.Jg },
                { "Mid", mongoTeam.Mid },
                { "Adc", mongoTeam.Adc },
                { "Sup", mongoTeam.Sup },
                {"TeamName", mongoTeam.TeamName },
                {"Elo", mongoTeam.Elo },
                {"LossesThisSeason", mongoTeam.LossesThisSeason },
                {"TotalLosses", mongoTeam.TotalLosses },
                {"TotalWins", mongoTeam.TotalWins },
                {"WinsThisSeason", mongoTeam.WinsThisSeason },
                {"Subs",  subs  },
                {"Captain", captain },
                {"Esubs", Esubs },
                {"Staff", Staff },
                {"Paid" ,  mongoTeam.Paid },
                {"RequiredGames" , mongoTeam.RequiredGames },
                {"Challenges" ,  mongoTeam.Challenges },
                {"TeamLastCheckedForPlayers" ,  mongoTeam.TeamLastCheckedForPlayers },
                {"Org", mongoTeam.Org },
                {"TeamTextChannel", mongoTeam.TeamTextChannel },
                {"TeamVoiceChannel", mongoTeam.TeamVoiceChannel }

            
                




        };

            Console.WriteLine(document);
            await collection.InsertOneAsync(document);

        }
        //inserts challenge into challenge database
        public async Task<bool> insertChallenge(Modules.Challenges challenge)
        {
            //connects to localhost
            string mongoKey;
            mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");

            var connectionString = mongoKey;
            var client = new MongoClient(connectionString);
            //applies player object to new object in this task

            var database = client.GetDatabase("StyleData");


            var collectionChallenges = database.GetCollection<BsonDocument>("Challenges");
            var collectionTeam = database.GetCollection<BsonDocument>("Team");
            //creates filter to check if theres already a challenge
            var filterChallenger = Builders<BsonDocument>.Filter.Eq("ChallengerId", challenge.ChallengerId);
            var filterChallengee = Builders<BsonDocument>.Filter.Eq("ChallengeeId", challenge.ChallengeeId);

            var filterChallengerTeamId = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(challenge.ChallengerId));
            var filterChallengeeTeamId = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(challenge.ChallengeeId));

            FilterDefinition<BsonDocument> combineFilters = Builders<BsonDocument>.Filter.And(filterChallenger, filterChallengee);
            var checkChallengeExists = await collectionChallenges.Find(combineFilters).FirstOrDefaultAsync();
            if(checkChallengeExists != null)
            {
                return false;
            }
            //creates new challenge
            var documentChallenge = new BsonDocument {
                {"Challenger", challenge.Challenger },
                {"ChallengerId", challenge.ChallengerId },
                {"Challengee", challenge.Challengee },
                {"ChallengeeId", challenge.ChallengeeId },
                {"Optional", challenge.Optional },
                {"DateCreated", DateTime.Today }







        };

            var update = new UpdateDefinitionBuilder<BsonDocument>().Inc("Challenges", 1);
            var options = new FindOneAndUpdateOptions<BsonDocument>();
            options.ReturnDocument = ReturnDocument.After;
            

            //var updateDocument = Builders<BsonDocument>.Update.Set("Challenges", +1);

            var result = await collectionTeam.FindOneAndUpdateAsync(filterChallengeeTeamId, update, options);
            Console.WriteLine(result);
            await collectionTeam.FindOneAndUpdateAsync(filterChallengerTeamId, update, options);
            await collectionChallenges.InsertOneAsync(documentChallenge);
            return true;
        }

        public async Task<bool> optional(string challenger, string challengee)
        {

            string mongoKey;
            mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");

            var connectionString = mongoKey;
            var client = new MongoClient(connectionString);
            //applies player object to new object in this task

            var database = client.GetDatabase("StyleData");
            //starts a collection
            var collection = database.GetCollection<BsonDocument>("Team");

            Double challengerElo = 0;
            Double challengeeElo = 800;


            var challengerFilter = Builders<BsonDocument>.Filter.Eq("TeamName", challenger);
            var challengeeFilter = Builders<BsonDocument>.Filter.Eq("TeamName", challengee);

            var checkChallengerExists = await collection.Find(challengerFilter).FirstOrDefaultAsync();
            if (checkChallengerExists != null)
            {
                BsonElement elo;
                checkChallengerExists.TryGetElement("Elo", out elo);
                challengerElo = elo.Value.ToDouble();

                Console.WriteLine(challenger + " elo is " + challengerElo);




            }
            var checkChallengeeExists = await collection.Find(challengeeFilter).FirstOrDefaultAsync();
            if (checkChallengeeExists != null)
            {
                BsonElement elo;
                checkChallengeeExists.TryGetElement("Elo", out elo);
                challengeeElo = elo.Value.ToDouble();

                Console.WriteLine(challengee + " elo is " + challengeeElo);

            }


            if (challengerElo >= (challengeeElo - 200) && challengerElo <= (challengeeElo + 100))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        //add challenge
        public async Task<Object []> challenge(Modules.Challenges challenge)
        {
            string mongoKey;
            mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");

            //variable to hold string and bool
            Object[] reasonAndOption = new Object[4];

            
            var connectionString = mongoKey;

            var client = new MongoClient(connectionString);

            var database = client.GetDatabase("StyleData");
            var collectionTeams = database.GetCollection<BsonDocument>("Team");
            

            int challengerChallenges;
            int challengeeChallenges;

            

            //checks if this is an optional challenge
            bool optionalCheck = await optional(challenge.Challenger, challenge.Challengee);

            var challengerFilter = Builders<BsonDocument>.Filter.Eq("TeamName", challenge.Challenger);
            var challengeeFilter = Builders<BsonDocument>.Filter.Eq("TeamName", challenge.Challengee);

            var checkChallengerExists = await collectionTeams.Find(challengerFilter).FirstOrDefaultAsync();
            if (checkChallengerExists != null)
            {
                BsonElement num;
                //checks number of challenges
                checkChallengerExists.TryGetElement("Challenges", out num);
                challengerChallenges = num.Value.ToInt32();

                //outs id 
                checkChallengerExists.TryGetElement("_id", out num);

                reasonAndOption[2] = num.Value.ToString();

                Console.WriteLine(challenge.Challenger + " has " + challengerChallenges + "number of challenges");

                if (challengerChallenges >= 3)
                {
                    reasonAndOption[0] = (challenge.Challenger + " has 3 or more challenges, they need to schedule games before anymore challenges can happen.");
                    return reasonAndOption;
                }
            }
            else
            {
                
                reasonAndOption[0] = (challenge.Challenger + " is not in our database");
                return reasonAndOption;
            }



            var checkChallengeeExists = await collectionTeams.Find(challengeeFilter).FirstOrDefaultAsync();
            if (checkChallengeeExists != null)
            {
                BsonElement num;
                //checks number of challenges
                checkChallengeeExists.TryGetElement("Challenges", out num);
                
                challengeeChallenges = num.Value.ToInt32();

                //outs id
                checkChallengeeExists.TryGetElement("_id", out num);

                reasonAndOption[3] = num.Value.ToString();

                Console.WriteLine(challenge.Challengee + " has " + challengeeChallenges + "number of challenges");

                if (challengeeChallenges >= 3)
                {
                    reasonAndOption[0] = (challenge.Challengee + " has 3 or more challenges, they need to schedule games before anymore challenges can happen.");
                    return reasonAndOption;
                }
            }
            else
            {
                reasonAndOption[0] = (challenge.Challengee + " is not in our database");
                return reasonAndOption;
            }


            reasonAndOption[0] = "";
            reasonAndOption[1] = optionalCheck;
            return reasonAndOption;


        }

        //checking challenges
        public async Task<string> checkChallengesBetweenTeams(string challenger, string challengee)
        {
            string mongoKey;
            mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");

            var connectionString = mongoKey;

            var client = new MongoClient(connectionString);

            var database = client.GetDatabase("StyleData");
            //starts a collection

            // number of challenges
            int challengerChallenges = 0;
            int challengeeChallenges = 0;

            var collection = database.GetCollection<BsonDocument>("Team");

            //filters for certain names
            var challengerFilter = Builders<BsonDocument>.Filter.Eq("TeamName", challenger);
            var challengeeFilter = Builders<BsonDocument>.Filter.Eq("TeamName", challengee);

            var checkChallengerExists = await collection.Find(challengerFilter).FirstOrDefaultAsync();
            if (checkChallengerExists != null)
            {
                BsonElement num;
                checkChallengerExists.TryGetElement("Challenges", out num);
                challengerChallenges = num.Value.ToInt32();

                Console.WriteLine(challenger + " has " + challengerChallenges + "number of challenges");

                if(challengerChallenges >= 3)
                {
                    return (challenger + " has 3 or more challenges, they need to schedule games before anymore challenges can happen.");
                }
            }
            else
            {
                return (challenger + " is not in our database");
            }



            var checkChallengeeExists = await collection.Find(challengeeFilter).FirstOrDefaultAsync();
            if (checkChallengeeExists != null)
            {
                BsonElement num;
                checkChallengeeExists.TryGetElement("Challenges", out num);
                challengeeChallenges = num.Value.ToInt32();

                Console.WriteLine(challengee + " has " + challengeeChallenges + "number of challenges");

                if (challengeeChallenges >= 3)
                {
                    return (challengee + " has 3 or more challenges, they need to schedule games before anymore challenges can happen.");
                }
            }
            else
            {
                return (challengee + " is not in our database");
            }
            return null;
        }


        //looks up player from database
        public async Task<string[]> playerLookup(string [] players)
        {

            string mongoKey;
            mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");

            var connectionString = mongoKey;
            var client = new MongoClient(connectionString);

            string[] listOfPlayers = new string[players.Length];
            int numCount = 0;

            
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
            /*
            try
            {
                foreach (var player in players)
                {
                    filter = builder.Eq("SummonerId", players[numCount]);
                    collectionOfPlayers = await collection.Find(filter).Project(projection).FirstOrDefaultAsync();

                    listOfPlayers[numCount] = collectionOfPlayers["Ign"].ToString() + " " + collectionOfPlayers["Rank"].ToString();
                    numCount++;
                }
            }
            catch
            {*/
                foreach (var player in players)
                {
                    filter = builder.Eq("Ign", players[numCount]);
                    collectionOfPlayers = await collection.Find(filter).Project(projection).FirstOrDefaultAsync();

                    listOfPlayers[numCount] = collectionOfPlayers["Ign"].ToString() + " " + collectionOfPlayers["Rank"].ToString();
                    numCount++;
                }
           // }
            Console.WriteLine(listOfPlayers);

            
            return listOfPlayers;

        }


        //gets info on certain team
        public  BsonDocument infoTeam(string team)
        {

            string mongoKey;
            mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");

            var connectionString = mongoKey;
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
