using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using player = DiscordBotApp.Modules.Player;
using team = DiscordBotApp.Modules.Team;
using matches = DiscordBotApp.Modules.Matches;
using commands = DiscordBotApp.Modules.Commands.UserCommands;
using System.Configuration;
namespace DiscordBotApp
{
    class MongoDB
    {
        //gets info on certain player
        public async Task<string> playerIfNoDiscord(string ign)
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
            var Filter = Builders<BsonDocument>.Filter.Eq("Ign", ign);
            
            var document = collection.Find(Filter).FirstOrDefault();
            if(document != null)
            {
                var update = Builders<BsonDocument>.Update.Set("CurrentTeam", "");
                await collection.UpdateOneAsync(Filter, update);
                return document["Ign"].AsString;

            }
            else
            {
                return null;
            }
            
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
            if (checkTeamExists != null)
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
        public async Task<bool> inputPlayer(object player, bool checkName)
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
                if (checkName)
                {
                    var updateDocument = Builders<BsonDocument>.Update.Set("Ign", mongoPlayer.Ign).
                    Set("Rank", mongoPlayer.Rank);






                    await collection.UpdateOneAsync(playerFilter, updateDocument);
                    return true;
                }
                else
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
                }
                else
                {
                    update = Builders<BsonDocument>.Update.Set(keyName, propertyForKey);
                }
                var updateTime = Builders<BsonDocument>.Update.Set("TeamLastCheckedForPlayers", DateTime.Now);
                await collection.UpdateOneAsync(filter, update);
                await collection.UpdateOneAsync(filter, updateTime);
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
                {"Logo", mongoTeam.Logo },
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

            var filterChallengerSwap = Builders<BsonDocument>.Filter.Eq("ChallengerId", challenge.ChallengeeId);
            var filterChallengeeSwap = Builders<BsonDocument>.Filter.Eq("ChallengeeId", challenge.ChallengerId);
            //checks if challenge exists either way
            FilterDefinition<BsonDocument> combineFilters = Builders<BsonDocument>.Filter.And(filterChallenger, filterChallengee);
            var checkChallengeExists = await collectionChallenges.Find(combineFilters).FirstOrDefaultAsync();

            FilterDefinition<BsonDocument> combineFiltersSwap = Builders<BsonDocument>.Filter.And(filterChallengerSwap, filterChallengeeSwap);

            var checkChallengeExistsSwap = await collectionChallenges.Find(combineFiltersSwap).FirstOrDefaultAsync();

            if (checkChallengeExists != null || checkChallengeExistsSwap != null)
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



            //updates number of challenges on team documents
            var result = await collectionTeam.FindOneAndUpdateAsync(filterChallengeeTeamId, update, options);
            Console.WriteLine(result);
            await collectionTeam.FindOneAndUpdateAsync(filterChallengerTeamId, update, options);
            await collectionChallenges.InsertOneAsync(documentChallenge);
            return true;
        }

        public async Task setLogo(string team, string url)
        {
            string mongoKey;
            mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");

            var connectionString = mongoKey;
            var client = new MongoClient(connectionString);
            //applies player object to new object in this task

            var database = client.GetDatabase("StyleData");
            //starts a collection
            var collection = database.GetCollection<BsonDocument>("Team");


            var update = Builders<BsonDocument>.Update.Set("Logo", url);

            var filterSub = Builders<BsonDocument>.Filter.Eq("TeamName", team);
            await collection.UpdateOneAsync(filterSub, update);
            return;

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
        public async Task<Object[]> challenge(Modules.Challenges challenge)
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

        public async Task<bool> DeleteChallenge(string yourTeam, string opposingTeam, bool sentFromGetChallengeInfo = false)
        {
            //connects to localhost
            string mongoKey;
            mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");

            var connectionString = mongoKey;
            var client = new MongoClient(connectionString);


            var database = client.GetDatabase("StyleData");

            //makes sure getting collection of two teams
            var collectionChallenges = database.GetCollection<BsonDocument>("Challenges");
            var collectionTeam = database.GetCollection<BsonDocument>("Team");

            var filterChallenger = Builders<BsonDocument>.Filter.Eq("Challenger", yourTeam);
            var filterChallengee = Builders<BsonDocument>.Filter.Eq("Challengee", opposingTeam);

            FilterDefinition<BsonDocument> combineFilters = Builders<BsonDocument>.Filter.And(filterChallenger, filterChallengee);
            var checkChallengeExists = await collectionChallenges.Find(combineFilters).FirstOrDefaultAsync();


            var filterChallengerSwap = Builders<BsonDocument>.Filter.Eq("Challenger", opposingTeam);
            var filterChallengeeSwap = Builders<BsonDocument>.Filter.Eq("Challengee", yourTeam);
            FilterDefinition<BsonDocument> combineFiltersSwap = Builders<BsonDocument>.Filter.And(filterChallengerSwap, filterChallengeeSwap);


            string challengerId;
            string challengeeId;
            bool optional;

            //checks if challenge exists either way





            var checkChallengeExistsSwap = await collectionChallenges.Find(combineFiltersSwap).FirstOrDefaultAsync();

            if (checkChallengeExists == null && checkChallengeExistsSwap == null)
            {
                return false;
            }
            else if (checkChallengeExists != null)
            {
                BsonElement id;
                checkChallengeExists.TryGetElement("ChallengerId", out id);
                challengerId = id.Value.ToString();


                checkChallengeExists.TryGetElement("ChallengeeId", out id);
                challengeeId = id.Value.ToString();


                var update = new UpdateDefinitionBuilder<BsonDocument>().Inc("Challenges", -1);
                var options = new FindOneAndUpdateOptions<BsonDocument>();
                options.ReturnDocument = ReturnDocument.After;



                var objectChallengFilter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(challengerId));

                var objectChallengeeFilter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(challengeeId));

                //updates number of challenges on team documents
                var result = await collectionTeam.FindOneAndUpdateAsync(objectChallengFilter, update, options);
                Console.WriteLine(result);
                await collectionTeam.FindOneAndUpdateAsync(objectChallengeeFilter, update, options);

                await collectionChallenges.DeleteOneAsync(checkChallengeExists);

                return true;
            }
            else if (checkChallengeExistsSwap != null)
            {
                BsonElement id;
                BsonElement option;
                checkChallengeExistsSwap.TryGetElement("ChallengerId", out id);
                challengerId = id.Value.ToString();
                ObjectId.Parse(challengerId);

                checkChallengeExistsSwap.TryGetElement("ChallengeeId", out id);
                challengeeId = id.Value.ToString();

                checkChallengeExistsSwap.TryGetElement("Optional", out option);
                optional = option.Value.ToBoolean();

                if (optional || sentFromGetChallengeInfo)
                {
                    await collectionChallenges.DeleteOneAsync(checkChallengeExistsSwap);


                    var update = new UpdateDefinitionBuilder<BsonDocument>().Inc("Challenges", -1);
                    var options = new FindOneAndUpdateOptions<BsonDocument>();
                    options.ReturnDocument = ReturnDocument.After;



                    var objectChallengFilter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(challengerId));

                    var objectChallengeeFilter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(challengeeId));

                    //updates number of challenges on team documents
                    var result = await collectionTeam.FindOneAndUpdateAsync(objectChallengFilter, update, options);
                    Console.WriteLine(result);
                    await collectionTeam.FindOneAndUpdateAsync(objectChallengeeFilter, update, options);
                    return true;
                }
                else
                {
                    return false;
                }

            }
            return false;
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

                if (challengerChallenges >= 3)
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
        public async Task<string[]> playerLookup(string[] players, bool recheckPlayer, string team)
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
            var projection = Builders<BsonDocument>.Projection.Exclude("_id").Exclude("Team").Exclude("LossesThisSeason").Exclude("TotalLosses").Exclude("TotalWins").Exclude("WinsThisSeason");
            //creates useable doc of players
            BsonDocument collectionOfPlayers;


            FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;
            FilterDefinition<BsonDocument> filter;

            List<string> justInCase = new List<string>();
            commands command = new commands(null);
            string[] whatToChange = new string[3];
            whatToChange[0] = team;
            string[] newPlayers = new string[players.Length];
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
            if (recheckPlayer)
            {


                var collectionForSubReset = database.GetCollection<BsonDocument>("Team");
                var filterSub = Builders<BsonDocument>.Filter.Eq("TeamName", team);

                var update = Builders<BsonDocument>.Update.Set("Subs", justInCase);

                await collectionForSubReset.UpdateOneAsync(filterSub, update);
                foreach (var check in players)
                {
                    filter = builder.Eq("Ign", players[numCount]);
                    collectionOfPlayers = await collection.Find(filter).Project(projection).FirstOrDefaultAsync();

                    string[] ign = await command.CheckIgn(collectionOfPlayers["SummonerId"].ToString());

                    if (ign != null)
                    {
                        switch (numCount)
                        {
                            case 0:
                                whatToChange[1] = "Top";
                                whatToChange[2] = ign[0];
                                break;
                            case 1:
                                whatToChange[1] = "Jg";
                                whatToChange[2] = ign[0];
                                break;
                            case 2:
                                whatToChange[1] = "Mid";
                                whatToChange[2] = ign[0];
                                break;
                            case 3:
                                whatToChange[1] = "Adc";
                                whatToChange[2] = ign[0];
                                break;
                            case 4:
                                whatToChange[1] = "Sup";
                                whatToChange[2] = ign[0];
                                break;
                            default:
                                whatToChange[1] = "Subs";
                                whatToChange[2] = ign[0];
                                break;
                        }
                        listOfPlayers[numCount] = ign[0] + " " + ign[1];
                        await updateTeam(whatToChange);
                    }
                    else
                    {
                        listOfPlayers[numCount] = collectionOfPlayers["Ign"].ToString() + " " + collectionOfPlayers["Rank"].ToString();
                    }


                    numCount++;

                }

                return listOfPlayers;
            }


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

        public async Task<matches> GetChallengeDetails(string team, string opposingTeam)
        {
            string mongoKey;
            mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");

            var connectionString = mongoKey;
            var client = new MongoClient(connectionString);

            //creates matches object
            matches match = new matches();

            var database = client.GetDatabase("StyleData");

            //makes sure getting collection of two teams
            var collectionChallenges = database.GetCollection<BsonDocument>("Challenges");



            var filterChallenger = Builders<BsonDocument>.Filter.Eq("Challenger", team);
            var filterChallengee = Builders<BsonDocument>.Filter.Eq("Challengee", opposingTeam);

            FilterDefinition<BsonDocument> combineFilters = Builders<BsonDocument>.Filter.And(filterChallenger, filterChallengee);
            var checkChallengeExists = await collectionChallenges.Find(combineFilters).FirstOrDefaultAsync();


            var filterChallengerSwap = Builders<BsonDocument>.Filter.Eq("Challenger", opposingTeam);
            var filterChallengeeSwap = Builders<BsonDocument>.Filter.Eq("Challengee", team);
            FilterDefinition<BsonDocument> combineFiltersSwap = Builders<BsonDocument>.Filter.And(filterChallengerSwap, filterChallengeeSwap);




            //checks if challenge exists either way





            var checkChallengeExistsSwap = await collectionChallenges.Find(combineFiltersSwap).FirstOrDefaultAsync();

            if (checkChallengeExists != null)
            {
                match.Challenger = checkChallengeExists["Challenger"].ToString();
                match.Challengee = checkChallengeExists["Challengee"].ToString();
                match.ChallengerId = checkChallengeExists["ChallengerId"].ToString();
                match.ChallengeeId = checkChallengeExists["ChallengeeId"].ToString();

                await DeleteChallenge(team, opposingTeam, true);
                return match;
            }
            else if (checkChallengeExistsSwap != null)
            {
                match.Challenger = checkChallengeExistsSwap["Challengee"].ToString();
                match.Challengee = checkChallengeExistsSwap["Challenger"].ToString();
                match.ChallengerId = checkChallengeExistsSwap["ChallengeeId"].ToString();
                match.ChallengeeId = checkChallengeExistsSwap["ChallengerId"].ToString();

                await DeleteChallenge(team, opposingTeam, true);
                return match;
            }
            return null;
        }

        public async Task<bool> Schedule(matches match)
        {
            string mongoKey;
            mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");

            var connectionString = mongoKey;
            var client = new MongoClient(connectionString);
            Console.WriteLine(client);
            var database = client.GetDatabase("StyleData");
            //starts a collection
            var collection = database.GetCollection<BsonDocument>("Matches");

            BsonArray pics = new BsonArray();
            foreach (var pic in match.PicsOfGames)
            {
                pics.Add(pic);
            }


            var document = new BsonDocument {
                { "Challenger", match.Challenger },
                { "ChallengerId", match.ChallengerId },
                { "Challengee", match.Challengee },
                { "ChallengeeId", match.ChallengeeId },
                { "MatchStatus", match.MatchStatus },
                {"ChallengerEloChanged", match.ChallengerEloChanged},
                {"ChallengeeEloChanged", match.ChallengeeEloChanged },
                {"Scheduled", match.Scheduled },
                {"CalendarId", match.CalendarId },
                {"Result", match.Result },
                {"PicsOfGames", pics},
                {"WhichTeamScheduled",  match.WhichTeamSchedule  },








        };

            await collection.InsertOneAsync(document);

            return true;
        }


        public async Task<bool> confirmSchedule(string team, string opposingTeam, string status, string calendarId)
        {
            string mongoKey;
            mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");

            var connectionString = mongoKey;
            var client = new MongoClient(connectionString);

            //creates matches object
            matches match = new matches();

            var database = client.GetDatabase("StyleData");

            //makes sure getting collection of two teams
            var collectionMatches = database.GetCollection<BsonDocument>("Matches");



            var filterChallenger = Builders<BsonDocument>.Filter.Eq("Challenger", team);
            var filterChallengee = Builders<BsonDocument>.Filter.Eq("Challengee", opposingTeam);

            FilterDefinition<BsonDocument> combineFilters = Builders<BsonDocument>.Filter.And(filterChallenger, filterChallengee);
            var checkingMatches = await collectionMatches.Find(combineFilters).FirstOrDefaultAsync();


            var filterChallengerSwap = Builders<BsonDocument>.Filter.Eq("Challenger", opposingTeam);
            var filterChallengeeSwap = Builders<BsonDocument>.Filter.Eq("Challengee", team);
            FilterDefinition<BsonDocument> combineFiltersSwap = Builders<BsonDocument>.Filter.And(filterChallengerSwap, filterChallengeeSwap);
            var checkingMatchesSwap = await collectionMatches.Find(combineFiltersSwap).FirstOrDefaultAsync();


            Console.WriteLine("what");
            if (checkingMatches != null)
            {


                BsonElement allowConfirm;
                checkingMatches.TryGetElement("WhichTeamScheduled", out allowConfirm);

                if (team != allowConfirm.Value.ToString())
                {
                    var updateDocument = Builders<BsonDocument>.Update.Set("MatchStatus", status).Set("CalendarId", calendarId);

                    await collectionMatches.UpdateOneAsync(combineFilters, updateDocument);
                    return true;
                }


                return false;
            }
            else if (checkingMatchesSwap != null)
            {
                BsonElement allowConfirm;
                checkingMatchesSwap.TryGetElement("WhichTeamScheduled", out allowConfirm);

                if (team != allowConfirm.Value.ToString())
                {
                    var updateDocument = Builders<BsonDocument>.Update.Set("MatchStatus", status).Set("CalendarId", calendarId);

                    await collectionMatches.UpdateOneAsync(combineFiltersSwap, updateDocument);
                    return true;
                }
                return false;
            }
            Console.WriteLine("what");
            return false;


        }

        //gets info on certain team
        public async Task<BsonDocument> infoTeam(string team)
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


            //used to add to all collection
            /*
            var teamAll = Builders<BsonDocument>.Filter.Empty;
            var updateDocument = Builders<BsonDocument>.Update.Set("Logo", "");

            await teamCollection.UpdateManyAsync(teamAll, updateDocument);*/
            //FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;
            //FilterDefinition<BsonDocument> filter = builder.Eq("Ign", document["Top"]) & builder.Eq("Ign", document["Jg"]) & builder.Eq("Ign", document["Mid"]) & builder.Eq("Ign", document["Adc"]) & builder.Eq("Ign", document["Sup"]);


            // var listOfPlayers = playerCollection.Find(filter).FirstOrDefault();



            // Console.WriteLine(listOfPlayers);

            return document;
        }

        public async Task<bool> InputElo(string teamName, double finishedElo)
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
                var updateDocument = Builders<BsonDocument>.Update.Set("Elo", finishedElo);

                await collection.UpdateOneAsync(teamFilter, updateDocument);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> AddResults(string teamName, int timesToRunWin, int timesToRunLose)
        {
            //connects to localhost
            string mongoKey;
            mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");

            var connectionString = mongoKey;
            var client = new MongoClient(connectionString);
            //applies player object to new object in this task

            var database = client.GetDatabase("StyleData");



            var collectionTeam = database.GetCollection<BsonDocument>("Team");
            //results
            var filter = Builders<BsonDocument>.Filter.Eq("TeamName", teamName);


            var eloUpdate = await collectionTeam.Find(filter).FirstOrDefaultAsync();


            var update = new UpdateDefinitionBuilder<BsonDocument>().Inc("WinsThisSeason", timesToRunWin).Inc("LossesThisSeason", timesToRunLose).Inc("TotalWins", timesToRunWin).Inc("TotalLosses", timesToRunLose);
            var options = new FindOneAndUpdateOptions<BsonDocument>();
            options.ReturnDocument = ReturnDocument.After;



            //updates number of challenges on team documents
            var result = await collectionTeam.FindOneAndUpdateAsync(eloUpdate, update, options);
            Console.WriteLine(result);

            return true;
        }

        public async Task<List<string>> GetRanks()
        {
            List<string> nameAndElo = new List<string>();


            int elementPlaced = 0;
            int numUp = 0;
            int numUpRank = 1;
            string mongoKey;
            mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");

            var connectionString = mongoKey;
            var client = new MongoClient(connectionString);
            //applies player object to new object in this task

            var database = client.GetDatabase("StyleData");
            var collection = database.GetCollection<BsonDocument>("Team");


            var teamFilter = Builders<BsonDocument>.Filter.Eq("Paid", true);

            var checkTeamExists = await collection.Find(teamFilter).SortByDescending(x => x["Elo"]).ToListAsync();

            string[] holdNameAndElo = new string[checkTeamExists.Count / 25 + 1];
            for (int i = 0; i < holdNameAndElo.Length; i++)
            {
                holdNameAndElo[i] = "";
            }
            foreach (var row in checkTeamExists)
            {
                if (numUp == 25)
                {
                    nameAndElo.Add(holdNameAndElo[elementPlaced]);
                    elementPlaced++;
                    numUp = 0;
                }
                holdNameAndElo[elementPlaced] = holdNameAndElo[elementPlaced].ToString() + $"\n{numUpRank}. " + row["TeamName"].ToString() + ": " + Math.Round(row["Elo"].ToDouble(), 1).ToString();
                numUp++;
                numUpRank++;

            }
            if (numUp < 25)
            {
                nameAndElo.Add(holdNameAndElo[elementPlaced]);
            }
            return nameAndElo;
        }
        //gets elo range for team
        public async Task<string> teamEloRange(string name)
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
            string listOfTeams = "";

            var challengerFilter = Builders<BsonDocument>.Filter.Eq("TeamName", name);
            var paidChallenge = Builders<BsonDocument>.Filter.Eq("Paid", true);
            var documentCalledTeam = await collection.Find(challengerFilter).FirstOrDefaultAsync();

            challengerElo = Math.Round(documentCalledTeam["Elo"].ToDouble(), 1);

            var document = await collection.Find(paidChallenge).SortByDescending(x => x["Elo"]).ToListAsync();


            foreach (var item in document)
            {
                challengeeElo = Math.Round(item["Elo"].ToDouble(), 1);


                if ((challengerElo >= (challengeeElo - 200) && challengerElo <= (challengeeElo + 100)) && documentCalledTeam["TeamName"].ToString() != item["TeamName"].ToString())
                {
                    listOfTeams = listOfTeams + "\n" + item["TeamName"].ToString() + " " + Math.Round(item["Elo"].ToDouble(), 1).ToString();
                }
                else
                {
                    Console.WriteLine("not in elo range");
                }
            }

            return listOfTeams;
        }

        public async Task<string[]> getChannelIds(string team)
        {
            string[] channels = new string[2];
            string mongoKey;
            mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");

            var connectionString = mongoKey;
            var client = new MongoClient(connectionString);
            //applies player object to new object in this task

            var database = client.GetDatabase("StyleData");
            //starts a collection
            var collection = database.GetCollection<BsonDocument>("Team");
            var challengerFilter = Builders<BsonDocument>.Filter.Eq("TeamName", team);
            var documentCalledTeam = await collection.Find(challengerFilter).FirstOrDefaultAsync();

            channels[0] = documentCalledTeam["TeamTextChannel"].ToString();
            channels[1] = documentCalledTeam["TeamVoiceChannel"].ToString();

            if (channels[0] == null || channels[1] == null)
            {
                return null;
            }
            else
            {
                var updateDocument = Builders<BsonDocument>.Update.Set("Paid", false);
                var updateTextChannel = Builders<BsonDocument>.Update.Set("TeamTextChannel", "");
                var updateVoiceChannel = Builders<BsonDocument>.Update.Set("TeamVoiceChannel", "");
                await collection.UpdateOneAsync(challengerFilter, updateDocument);
                await collection.UpdateOneAsync(challengerFilter, updateTextChannel);
                await collection.UpdateOneAsync(challengerFilter, updateVoiceChannel);
                return channels;
            }

        }

        public async Task<List<int>> RemovePlayer(string name, string ign)
        {
            try
            {
                string mongoKey;
                mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");




                var connectionString = mongoKey;
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase("StyleData");
                //starts a collection
                var collection = database.GetCollection<BsonDocument>("Team");
                var filter = Builders<BsonDocument>.Filter.Eq("TeamName", name);
                var checkTeamExists = await collection.Find(filter).FirstOrDefaultAsync();
                var update = Builders<BsonDocument>.Update.Set("PlaceHolder","");

                List<int> subPosition = new List<int>();
                

                if (checkTeamExists["Top"].AsString == ign) 
                {
                    subPosition.Add(-1);
                    update = Builders<BsonDocument>.Update.Set("Top", "none");
                }
                else if(checkTeamExists["Jg"].AsString == ign)
                {
                    subPosition.Add(-1);
                    update = Builders<BsonDocument>.Update.Set("Jg", "none");
                }
                else if (checkTeamExists["Mid"].AsString == ign)
                {
                    subPosition.Add(-1);
                    update = Builders<BsonDocument>.Update.Set("Mid", "none");
                }
                else if (checkTeamExists["Adc"].AsString == ign)
                {
                    subPosition.Add(-1);
                    update = Builders<BsonDocument>.Update.Set("Adc", "none");
                }
                else if (checkTeamExists["Sup"].AsString == ign)
                {
                    subPosition.Add(-1);
                    update = Builders<BsonDocument>.Update.Set("Sup", "none");
                }
                else if (checkTeamExists["Subs"].AsBsonArray.Contains(ign))
                {
                    subPosition.Add(checkTeamExists["Subs"].AsBsonArray.IndexOf(ign));
                    subPosition.Add(checkTeamExists["Subs"].AsBsonArray.Count);
                    update = Builders<BsonDocument>.Update.Pull("Subs", ign);
                }
                else
                {
                    subPosition.Add(-1);
                    return subPosition;
                }


                
                
                await collection.UpdateOneAsync(filter, update);
                return subPosition;

            }
            catch
            {
                Console.WriteLine("something went wrong");
                return null;
            }
        }
        public async Task RemovePlayer(string name, string role, IList<object> lists)
        {
            try
            {
                string mongoKey;
                mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");




                var connectionString = mongoKey;
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase("StyleData");
                //starts a collection
                var collection = database.GetCollection<BsonDocument>("Team");
                var filter = Builders<BsonDocument>.Filter.Eq("TeamName", name);


                var update = Builders<BsonDocument>.Update.Set(role, lists[0]);
                if (role == "Subs")
                {
                    update = Builders<BsonDocument>.Update.Pull(role, lists[0]);
                }
                else
                {
                    update = Builders<BsonDocument>.Update.Set(role, lists[0]);
                }
                var updateTime = Builders<BsonDocument>.Update.Set("TeamLastCheckedForPlayers", DateTime.Now);
                await collection.UpdateOneAsync(filter, update);

            }
            catch
            {
                Console.WriteLine("something went wrong");
            }
        }

        public async Task<string> getIgnFromDiscord(string teamName, string discord)
        {
            try
            {
                string mongoKey;
                mongoKey = ConfigurationManager.AppSettings.Get("mongoIP");




                var connectionString = mongoKey;
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase("StyleData");
                //starts a collection
                var collection = database.GetCollection<BsonDocument>("Player");
                var filter = Builders<BsonDocument>.Filter.Eq("DiscordName", discord);

                var checkTeamExists = await collection.Find(filter).FirstOrDefaultAsync();

                if (checkTeamExists != null)
                {
                    var update = Builders<BsonDocument>.Update.Set("CurrentTeam", "");
                    await collection.UpdateOneAsync(filter, update);
                    return checkTeamExists["Ign"].ToString();
                }
                else
                {
                    return null;
                }

                

            }
            catch
            {
                Console.WriteLine("something went wrong");
                return null;
            }
        }
    }
}
