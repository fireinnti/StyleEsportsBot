using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Configuration;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Discord.Rest;
using System.Web;
using System.Runtime.CompilerServices;
using googleSheet = DiscordBotApp.GoogleSheets.Sheets;
using googleCalendar = CalendarStyle.GoogleCalendar;
using RiotNet;
using RiotNet.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using mongo = DiscordBotApp.MongoDB;
using opggScraper = DiscordBotApp.Opggsraper;
using System.Threading;
using System.Timers;
using NUnit.Framework;



/*surround every command
 *  var user = Context.User as SocketGuildUser;
            var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
            if (user.Roles.Contains(rolePermissionAdmin)){
*/
/* how to log
 * var channel = Context.Guild as SocketGuild;
            //var channelIdLogs = channel.GetTextChannel(767455535800385616).SendMessageAsync(Context.User.Username + " has used the msg command for " + role);
*/

namespace DiscordBotApp.Modules

{
    //class that mirrors challenges in the mongodb
    public class Challenges
    {
        public string Challenger = "";
        public string ChallengerId = "";
        public string Challengee = "";
        public string ChallengeeId = "";
        public bool Optional = true;
        public string teamsVs = "";


    }
    //class that mirrors Team in the mongodb
    public class Team
    {


        public string Top = "";
        public string Jg = "";
        public string Mid = "";
        public string Adc = "";
        public string Sup = "";
        public string TeamName = "";
        public decimal Elo = 0;
        public Int32 LossesThisSeason = 0;
        public Int32 TotalLosses = 0;
        public Int32 TotalWins = 0;
        public Int32 WinsThisSeason = 0;
        public List<string> Subs = new List<string>();
        public List<string> Captain = new List<string>();
        public List<string> ESubs = new List<string>();
        public List<string> Staff = new List<string>();
        public bool Paid = false;
        public Int32 RequiredGames = 3;
        public Int32 Challenges = 0;
        public DateTime TeamLastCheckedForPlayers;
        public string Logo = "";

        public string Org = "";
        public string TeamTextChannel = "";
        public string TeamVoiceChannel = "";


    }
    //class that mirrors the player in mongodb
    public class Player
    {
        public string Rank = "";
        public string Ign = "";
        public string SummonerId = "";
        public string CurrentTeam = "";
        public Int32 LossesThisSeason = 0;
        public Int32 TotalLosses = 0;
        public Int32 TotalWins = 0;
        public Int32 WinsThisSeason = 0;
        public List<string> AltNames = new List<string>();
        public List<string> PreviousTeams = new List<string>();
        public string DiscordName = "";
    }
    //class that mirrors the matches in mongodb
    public class Matches
    {
        public string Challenger = "";
        public string ChallengerId = "";
        public string Challengee = "";
        public string ChallengeeId = "";
        public string MatchStatus = "";
        public double ChallengerEloChanged = 0;
        public double ChallengeeEloChanged = 0;
        public string Scheduled;
        public string CalendarId = "";
        public string Result = "";
        public List<string> PicsOfGames = new List<string>();
        public string WhichTeamSchedule = "";

    }


    public class Commands
    {


        public class UserCommands : InteractiveBase
        {

            public CommandService _commands;
            //gets all commands for usercommands
            public UserCommands(CommandService service)
            {
                _commands = service;
            }
            //creates bool if a command should work while in debug mode
#if DEBUG
            public bool work = false;
#else
        public bool work = true;
#endif


            /*[Command("scrape")]
            public async Task Scrape()
            {
               // opggScraper scraper = new opggScraper();

                await scraper.ScrapeMe();

            }*/

            //spits out captain based off of given team
            [Command("captain")]
            [Summary("!captain @team (this will spit out captain of that team)")]
            public async Task Captain(IRole team)
            {
                //checks if in debug mode
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }

                var roleName = team.Name;
                Console.WriteLine(roleName);
                //grabs list of users by rolename
                var listOfUsers = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName).Members;
                string listOfCaptains = "";

                foreach (var user in listOfUsers)
                {
                    //checks each user in certain role if they also have team captain role
                    var isCaptainOfEnemy = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Team Captain");
                    if (user.Roles.Contains(isCaptainOfEnemy))
                    {
                        listOfCaptains = listOfCaptains + user.Mention + " ";



                    }
                }
                await ReplyAsync(listOfCaptains);
            }

            /* [Command("player")]
             public async Task Player()
             {
                 var mongo = new mongo();

                 await ReplyAsync(mongo.player());

             }*/
            //migrates sheet data to database

            [Command("team")]
            [Alias("teamroster")]
            [Summary("!team @teamname(this will create a team card that has stats and multi.opgg")]
            public async Task Team(IRole team)
            {
                //checks if in debug mode
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }

                var channel = Context.Guild as SocketGuild;
                //gets specific text channel, channel log
                var channelIdLogs = channel.GetTextChannel(767455535800385616).SendMessageAsync(Context.User.Username + " has used the team command for " + team);
                try
                {
                    //access to mongodb.cs methods
                    var mongo = new mongo();
                    //bsondocument is similar to object
                    BsonDocument teamData = await mongo.infoTeam(team.ToString());

                    int numCount = 5;

                    DateTime lastChecked = teamData["TeamLastCheckedForPlayers"].ToUniversalTime();
                    //gets last checked for dates
                    var timeBetweenDays = DateTime.Now - lastChecked;

                    bool updateSheet = false;

                    if (timeBetweenDays.Days > 1)
                    {
                        updateSheet = true;


                    }

                    string[] numberOfPlayers = new string[teamData["Subs"].AsBsonArray.Count() + 5];
                    
                    numberOfPlayers[0] = teamData["Top"].ToString();
                    numberOfPlayers[1] = teamData["Jg"].ToString();
                    numberOfPlayers[2] = teamData["Mid"].ToString();
                    numberOfPlayers[3] = teamData["Adc"].ToString();
                    numberOfPlayers[4] = teamData["Sup"].ToString();
                    foreach (var sub in teamData["Subs"].AsBsonArray)
                    {
                        numberOfPlayers[numCount] = sub.ToString();
                        numCount++;
                    }


                    //looks up players by sending the numberOfPlayers array with each player into mongo.playerlookup. If The updatesheet is false, it skips updating the roster to save needing to check for roster changes every single call. Finally sends what team to lookup
                    string[] teamPlayers = await mongo.playerLookup(numberOfPlayers, updateSheet, team.Name);
                    Console.WriteLine(teamPlayers[0].ToString());

                    string fullData = ("Name: " + teamData["TeamName"].ToString() + "\n" + "Elo: " + Math.Round(teamData["Elo"].ToDouble(), 1).ToString() + "\n" + "Season Record: " + teamData["WinsThisSeason"].ToString() + "/" + teamData["LossesThisSeason"] + "\n" + "Top: " + teamPlayers[0] + "\n" + "Jungle: " + teamPlayers[1] + "\n" + "Mid: " + teamPlayers[2] + "\n" + "Adc: " + teamPlayers[3] + "\n" + "Sup: " + teamPlayers[4] + "\n");

                    string playerData = "Top: " + teamPlayers[0] + "\n" + "Jungle: " + teamPlayers[1] + "\n" + "Mid: " + teamPlayers[2] + "\n" + "Adc: " + teamPlayers[3] + "\n" + "Sup: " + teamPlayers[4] + "\n";


                    //checks if there are subs on the team
                    if (teamPlayers.Length > 5)
                    {
                        numCount = 1;
                        for (int i = 5; i < teamData["Subs"].AsBsonArray.Count() + 5; i++)
                        {
                            fullData = fullData + "Sub #" + numCount + " " + teamPlayers[i] + "\n";
                            playerData = playerData + "Sub #" + numCount + " " + teamPlayers[i] + "\n";
                            numCount++;
                        }
                    }


                    //add all igns into single string
                    string stringOfIgns = null;

                    foreach (var row in numberOfPlayers)
                    {

                        //await ReplyAsync($"{row[0]}, {row[1]}, {row[2]}");



                        stringOfIgns = stringOfIgns + row + ",";



                    }

                    var hyperLink = "https://na.op.gg/multi/query=" + HttpUtility.UrlEncode(stringOfIgns);
                    string logo = "";
                    if (teamData["Logo"].ToString() != null)
                    {
                        logo = teamData["Logo"].ToString();
                    }
                    //creates embeded with team information
                    var exampleAuthor = new EmbedAuthorBuilder()
                .WithName("TeamCard")

                .WithIconUrl("https://cdn.discordapp.com/attachments/643609428012040202/798093546178740224/GG_transparent.png");



                    var exampleField = new EmbedFieldBuilder()
                            .WithName("Name")
                            .WithValue(teamData["TeamName"].ToString())
                            .WithIsInline(true);
                    var otherField = new EmbedFieldBuilder()
                            .WithName("Elo")
                            .WithValue(Math.Round(teamData["Elo"].ToDouble(), 1).ToString())
                            .WithIsInline(false);
                    var winLossField = new EmbedFieldBuilder()
                            .WithName("Season Record")
                            .WithValue(teamData["WinsThisSeason"].ToString() + "/" + teamData["LossesThisSeason"])
                            .WithIsInline(false);
                    var playerField = new EmbedFieldBuilder()
                            .WithName("Players")
                            .WithValue(playerData)
                            .WithIsInline(false);
                    var multiField = new EmbedFieldBuilder()
                        .WithName("Multi.OPGG")
                            .WithValue($"[Link]({hyperLink})")
                            .WithIsInline(false);
                    //var exampleFooter = new EmbedFooterBuilder()
                    // .WithText($"sdf[Multi.opgg link]({hyperLink})")
                    //  .WithIconUrl("https://discord.com/assets/28174a34e77bb5e5310ced9f95cb480b.png");
                    var embed = new EmbedBuilder()
                            .AddField(exampleField)
                            .WithThumbnailUrl(logo)
                            .AddField(otherField)
                            .AddField(winLossField)
                           .AddField(playerField)
                           .AddField(multiField)
                            .WithAuthor(exampleAuthor);




                    await ReplyAsync("", false, embed.Build());


                    // await ReplyAsync(fullData + "\n" + hyperLink + "\nIf an op.gg doesn't show up, please contact Admins.");
                }
                catch
                {
                    await ReplyAsync("Not in database, using depreciated method until this team fixes it. This means their rank won't be accurate and we can't verify if all players exist.");

                    string oldTeamRosterMethod = await teamRosterOld(team);
                    await ReplyAsync(oldTeamRosterMethod);
                }
            }

            [Command("setlogo", RunMode = RunMode.Async)]
            [Summary("!setlogo @yourteam linktologo sets logo for your team. If it's offensive you'll get talked to by admins.")]
            public async Task SetLogo(IRole team, params string[] url)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }
                try
                {
                    var mongo = new MongoDB();






                    var listOfUsers = Context.Guild.Roles.FirstOrDefault(x => x.Name == team.Name).Members;
                    var google = new googleSheet();


                    //makes sure that there is a pending scheduled match


                    var channel = Context.Guild as SocketGuild;

                    //make sure user has correct roles
                    var userUsingCommand = Context.User as SocketGuildUser;
                    var isCaptain = (userUsingCommand as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Team Captain");
                    var rolePermissionAdmin = (userUsingCommand as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
                    var rolePermissionCaster = (userUsingCommand as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Shoutcaster");
                    if ((userUsingCommand.Roles.Contains(team) && userUsingCommand.Roles.Contains(isCaptain)) || userUsingCommand.Roles.Contains(rolePermissionAdmin) || userUsingCommand.Roles.Contains(rolePermissionCaster))
                    {
                        await ReplyAsync("Are you sure the url you provided is your teams logo? 'yes' or 'no'" + url[0].ToString());

                        var confirm = await NextMessageAsync();

                        if (confirm.ToString().ToLower() == "yes")
                        {

                            await mongo.setLogo(team.Name, url[0].ToString());
                            await ReplyAsync("Logo set");
                        }
                        else
                        {
                            await ReplyAsync("Please try again");
                        }
                    }
                    else
                    {
                        await ReplyAsync("You are neither the captain of the called team, or an admin");
                    }
                }
                catch
                {
                    await ReplyAsync("Something went wrong, please contact admins");
                }
            }

            [Command("challenge")]
            [Summary("!challenge @yourteam @otherteam (this will set a challenge between the teams)")]
            public async Task Challenge(IRole yourteam, IRole opposingTeam)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }
                try
                {
                    var mongo = new MongoDB();

                    var roleName = opposingTeam.ToString();
                    Console.WriteLine(roleName);
                    //var listOfUsers = (role as IChannel).GetUsersAsync(default, default);

                    string locationOfInput = "challenge";
                    DateTime convertedDate = default;
                    //gets list of users of opposing team
                    var listOfUsers = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName).Members;
                    var google = new googleSheet();

                    string listOfCaptains = "";
                    //makes sure that there is a pending scheduled match


                    var channel = Context.Guild as SocketGuild;

                    //make sure user has correct roles
                    var userUsingCommand = Context.User as SocketGuildUser;
                    var isCaptain = (userUsingCommand as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Team Captain");
                    var rolePermissionAdmin = (userUsingCommand as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
                    if ((userUsingCommand.Roles.Contains(yourteam) && userUsingCommand.Roles.Contains(isCaptain)) || userUsingCommand.Roles.Contains(rolePermissionAdmin))
                    {



                        //sets challenge object
                        Challenges challenge = new Challenges();
                        challenge.Challenger = yourteam.Name;
                        challenge.Challengee = opposingTeam.Name;

                        Object[] reasonAndOption = new Object[4];
                        //checks to make sure team exists, gets optional tag.
                        reasonAndOption = await mongo.challenge(challenge);
                        if (reasonAndOption[0].ToString() != "")
                        {
                            await ReplyAsync(reasonAndOption[0].ToString());
                            return;
                        }

                        challenge.Optional = (bool)reasonAndOption[1];

                        //gets unique id from team 
                        challenge.ChallengerId = (string)reasonAndOption[2];
                        challenge.ChallengeeId = (string)reasonAndOption[3];
                        bool insertSuccess = await mongo.insertChallenge(challenge);

                        if (!insertSuccess)
                        {
                            await ReplyAsync("Challenge already exists.");
                            return;
                        }

                        Console.WriteLine("woot");
                        var scheduleSheet = google.Schedule(yourteam.ToString(), opposingTeam.ToString(), convertedDate, locationOfInput);
                        if (scheduleSheet == null)
                        {
                            //checks if match is optional
                            var optional = google.Optional(yourteam.ToString(), opposingTeam.ToString());
                            foreach (var user in listOfUsers)
                            {

                                var isCaptainOfEnemy = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Team Captain");
                                if (user.Roles.Contains(isCaptainOfEnemy))
                                {
                                    listOfCaptains = listOfCaptains + user.Mention + " ";

                                    Console.WriteLine(optional);
                                    var channelb = await user.GetOrCreateDMChannelAsync();
                                    if (!challenge.Optional)
                                    {


                                        await channelb.SendMessageAsync($"Hello, {user.Username} from " + opposingTeam.ToString() + "! " + Context.User.ToString() + " of " + yourteam.ToString() + " has challenged you, this request is nonoptional. Please talk to " + userUsingCommand.Username + " to select a time for the match. After picking out a date for the match" +
                                         " please use the !schedule command with the format of !schedule @" + opposingTeam.ToString() + " @" + yourteam.ToString() + " mm/dd/yy HH:MM AM/PM timezone(est / edt, cst / cdt, pst / pdt, mst / mdt) EXAMPLE:'!schedule @yourteam @enemyteam 02/28/21 8:00 pm est'");
                                    }
                                    else
                                    {
                                        await channelb.SendMessageAsync($"Hello, {user.Username} from " + opposingTeam.ToString() + "!! " + Context.User.ToString() + " of " + yourteam.ToString() + " has challenged you, this challenge is optional. If you would like you can !denychallenge @" + opposingTeam.ToString() + " @" + yourteam.ToString() + " Please talk to " + userUsingCommand.Username + "to select a time for the match if you chooose to accept. After picking out a date for the match" +
                                         " please use the !schedule command with the format of !schedule @" + opposingTeam.ToString() + " @" + yourteam.ToString() + " mm/dd/yy HH:MM AM/PM timezone(est / edt, cst / cdt, pst / pdt, mst / mdt) EXAMPLE:'!schedule @yourteam @enemyteam 02/28/21 8:00 pm est'");


                                    }




                                }


                            }
                            if (!challenge.Optional)
                            {

                                await ReplyAsync("A NONOPTIONAL challenge has been issued to " + opposingTeam.ToString() + " and was sent to " + listOfCaptains + "After picking out a date for the match with the other captain" +
                                         " please use the !schedule command with the format of !schedule @" + yourteam.ToString() + " @" + opposingTeam.ToString() + " mm/dd/yy HH:MM AM/PM timezone(est / edt, cst / cdt, pst / pdt, mst / mdt)");
                            }
                            else
                            {
                                await ReplyAsync("An OPTIONAL challenge has been issued to " + opposingTeam.ToString() + " and was sent to " + listOfCaptains + "After picking out a date for the match with the other captain" +
                                         " please use the !schedule command with the format of !schedule @" + yourteam.ToString() + " @" + opposingTeam.ToString() + " mm/dd/yy HH:MM AM/PM timezone(est / edt, cst / cdt, pst / pdt, mst / mdt)");
                            }
                            var channelIdLogs = channel.GetTextChannel(767455535800385616).SendMessageAsync(Context.User.Username + " has used the challenge command to challenge " + opposingTeam.ToString() + " with their team " + yourteam.ToString()); ;
                            return;
                        }
                        else
                        {
                            await ReplyAsync(scheduleSheet);
                            return;
                        }
                    }
                    else
                    {
                        await ReplyAsync("You are not the team captain of the first team in the command " + yourteam.ToString() + " or an admin");
                        return;
                    }


                }
                catch
                {
                    await ReplyAsync("something went wrong, please ask for admin help");
                    return;
                }
            }


            [Command("denychallenge")]
            [Alias("deny")]
            [Summary("!deny @yourteam @otherteam (this will either delete an optional challenge or delete the challenge you created)")]
            public async Task DenyChallenge(IRole yourteam, IRole opposingTeam)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }

                string removeChallenge = "challenge";
                string typeOfRemove = "challenge";
                MongoDB mongo = new MongoDB();

                DateTime convertedDate = DateTime.Parse("12/30/2020");


                // time[0] = time[0] + "/20";


                var channel = Context.Guild as SocketGuild;
                //var channelIdLogs = channel.GetTextChannel(767455535800385616).SendMessageAsync(Context.User.Username + " has used the msg command for " + role);

                var userThatCalledSchedule = Context.User as SocketGuildUser;
                var roleName = opposingTeam.ToString();
                Console.WriteLine(roleName);
                //var listOfUsers = (role as IChannel).GetUsersAsync(default, default);
                var listOfUsers = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName).Members;
                var google = new googleSheet();


                var rolePermissionAdmin = (userThatCalledSchedule as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
                var captainPermission = (userThatCalledSchedule as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Team Captain");
                if (userThatCalledSchedule.Roles.Contains(rolePermissionAdmin) || (userThatCalledSchedule.Roles.Contains(captainPermission) && userThatCalledSchedule.Roles.Contains(yourteam)))
                {
                    int positionOfYourTeam = 0;
                    int positionOfOpposingTeam = 0;
                    int howManyInListYourTeam = 0;
                    int howManyInListOpposingTeam = 0;

                    try
                    {
                        //makes sure that there is a pending scheduled match
                        string[] confirmYourTeamChallenge = google.Confirm(yourteam.ToString(), opposingTeam.ToString(), removeChallenge, false);
                        string[] confirmOpposingTeamChallenge = google.Confirm(yourteam.ToString(), opposingTeam.ToString(), removeChallenge, true);
                        if (confirmYourTeamChallenge[0] == opposingTeam.ToString())
                        {
                            Console.WriteLine("opposing team and confirm[0] are the same");
                            positionOfYourTeam = Int32.Parse(confirmYourTeamChallenge[1]);
                            positionOfOpposingTeam = Int32.Parse(confirmOpposingTeamChallenge[1]);
                            howManyInListYourTeam = Int32.Parse(confirmYourTeamChallenge[2]);
                            howManyInListOpposingTeam = Int32.Parse(confirmOpposingTeamChallenge[2]);

                        }
                    }
                    catch
                    {
                        await ReplyAsync("Something went wrong, please contatct admins");
                        return;
                    }



                    bool optional = await mongo.DeleteChallenge(yourteam.Name, opposingTeam.Name);
                    Console.WriteLine(optional);
                    if (optional)
                    {

                        google.RemoveSchedule(yourteam.ToString(), positionOfYourTeam, howManyInListYourTeam, opposingTeam.ToString(), positionOfOpposingTeam, howManyInListOpposingTeam, typeOfRemove);
                        await ReplyAsync("Denied challenge of " + opposingTeam.ToString());
                        foreach (var user in listOfUsers)
                        {

                            var isCaptainOfEnemy = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Team Captain");
                            if (user.Roles.Contains(isCaptainOfEnemy))
                            {


                                Console.WriteLine(user.Username + " is a captain");

                                var channelb = await user.GetOrCreateDMChannelAsync();

                                await channelb.SendMessageAsync($"Hello, {user.Username}! " + Context.User.Username + " of " + yourteam.ToString() + " has denied your challenge. ");
                                await ReplyAsync("Challenge deleted");

                                var channelIdLogs = channel.GetTextChannel(767455535800385616).SendMessageAsync(Context.User.Username + " has used the denychallenge command for " + opposingTeam.ToString()); ;
                            }

                        }
                    }
                    else
                    {
                        await ReplyAsync("Challenge is not optional.");
                    }

                }
            }




            [Command("confirmschedule")]
            [Summary("!confirmschedule @yourteam @otherteam confirms the schedule that the two teams picked out.")]
            public async Task ConfirmSchedule(IRole yourteam, IRole opposingTeam)
            {
                //if (work == false)
                //{
                //    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                //    return;
                //}

                MongoDB mongo = new MongoDB();


                string typeOfRemove = "preschedule";
                string locationOfInput = "confirmedschedule";
                var roleName = opposingTeam.ToString();
                Console.WriteLine(roleName);
                //var listOfUsers = (role as IChannel).GetUsersAsync(default, default);

                var channel = Context.Guild as SocketGuild;

                //gets list of users of opposing team
                var listOfUsers = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName).Members;
                var google = new googleSheet();
                //makes sure that there is a pending scheduled match
                string[] confirmYourTeam = google.Confirm(yourteam.ToString(), opposingTeam.ToString(), locationOfInput, false);
                string[] confirmOpposingTeam = google.Confirm(yourteam.ToString(), opposingTeam.ToString(), locationOfInput, true);

                //make sure user has correct roles
                var userUsingCommand = Context.User as SocketGuildUser;
                var isCaptain = (userUsingCommand as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Team Captain");
                var rolePermissionAdmin = (userUsingCommand as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
                if ((userUsingCommand.Roles.Contains(yourteam) && userUsingCommand.Roles.Contains(isCaptain)) || userUsingCommand.Roles.Contains(rolePermissionAdmin))
                {
                    var confirmMong = await mongo.confirmSchedule(yourteam.Name, opposingTeam.Name, locationOfInput, "");
                    if (confirmMong)
                    {
                        if (confirmYourTeam[0] == opposingTeam.ToString())
                        {
                            Console.WriteLine("opposing team and confirm[0] are the same");
                            int positionOfYourTeam = Int32.Parse(confirmYourTeam[1]);
                            int positionOfOpposingTeam = Int32.Parse(confirmOpposingTeam[1]);
                            int howManyInListYourTeam = Int32.Parse(confirmYourTeam[2]);
                            int howManyInListOpposingTeam = Int32.Parse(confirmOpposingTeam[2]);

                            try
                            {
                                Console.WriteLine("made it into trying to schedule");
                                //gets converted matchdate of opponents, from google sheet depreciating to database
                                // DateTime convertedDate = google.GetMatchDate(yourteam.ToString(), false, positionOfYourTeam);

                                //gets from mongo

                                DateTime convertedDate = await mongo.GetMatchDate("pendingschedule", yourteam.Name, opposingTeam.Name);

                                //if (convertedDate == DateTime.Parse("0")){
                                //    await ReplyAsync("Issue with db scheduled date, please let innti know");
                                //    return;
                                //}
                                //DateTime stopper = DateTime.Parse("01/11/2021");
                                /* int resultDate = DateTime.Compare(convertedDate, stopper);
                                 if(resultDate > 0)
                                 {
                                     await ReplyAsync("Won't schedule past the tenth, using that night to migrate matches and challenges to database.");
                                     return;
                                 }*/



                                var scheduleSheet = google.Schedule(yourteam.ToString(), opposingTeam.ToString(), convertedDate, locationOfInput);
                                if (scheduleSheet == null)
                                {
                                    google.RemoveSchedule(yourteam.ToString(), positionOfYourTeam, howManyInListYourTeam, opposingTeam.ToString(), positionOfOpposingTeam, howManyInListOpposingTeam, typeOfRemove);
                                    //gets enemy captain and dms them
                                    foreach (var user in listOfUsers)
                                    {

                                        var isCaptainOfEnemy = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Team Captain");
                                        if (user.Roles.Contains(isCaptainOfEnemy))
                                        {


                                            Console.WriteLine(user.Username + " is a captain");

                                            var channelb = await user.GetOrCreateDMChannelAsync();

                                            await channelb.SendMessageAsync($"Hello, {user.Username} from " + opposingTeam.ToString() + "! " + Context.User.Username + " of " + yourteam.ToString() + " confirmed your match on " + convertedDate + " est/edt.");
                                        }

                                    }




                                    await ReplyAsync("Match has been confirmed for " + convertedDate + " est/edt");
                                    var calendar = new googleCalendar();
                                    var timeZone = "America/New_York";
                                    string id = calendar.AddMatch(yourteam.ToString(), opposingTeam.ToString(), convertedDate, timeZone);

                                    confirmMong = await mongo.confirmSchedule(yourteam.Name, opposingTeam.Name, locationOfInput, id);



                                    var channelIdLogs = channel.GetTextChannel(767455535800385616).SendMessageAsync(Context.User.Username + " has used the confirm schedule command to confirm the match between " + yourteam.ToString() + " and " + opposingTeam.ToString() + " at " + convertedDate);
                                }

                                else
                                {
                                    await ReplyAsync(confirmYourTeam[0]);
                                    return;
                                }
                            }
                            catch
                            {
                                await ReplyAsync("Something went wrong, please contact admins.");
                            }
                        }
                        else
                        {
                            await ReplyAsync("Something went wrong, please contact admins.");
                        }
                    }
                    else
                    {
                        await ReplyAsync("You are the team that scheduled, please have other team confirm");
                    }


                }
                else
                {
                    await ReplyAsync("You are not the team captain of the first team in the command " + yourteam.ToString() + " or an admin");
                }
            }


            [Command("schedule", RunMode = RunMode.Async)]
            [Summary("!schedule @yourteam @otherteam mm/dd/yy HH:MM AM/PM timezone(est / edt, cst / cdt, pst / pdt, mst / mdt) EXAMPLE:'!schedule @yourteam @enemyteam 02/28/21 8:00 pm est' creates schedule")]
            public async Task Schedule(IRole team1, IRole team2, params string[] time)
            {
                //  if (work == false)
                // {
                //  await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                // return;
                //}

                MongoDB mongo = new MongoDB();

                Matches match = new Matches();

                string checkIfInSchedule = "confirmedschedule";
                string locationOfInput = "pendingschedule";
                string removeChallenge = "challenge";
                string typeOfRemove = "challenge";
                DateTime scheduledDate = DateTime.Parse("12/30/2020");
                try
                {
                    CultureInfo current = CultureInfo.CurrentCulture;
                    string combinedParams = time[0] + " " + time[1] + " " + time[2];
                    scheduledDate = DateTime.ParseExact(combinedParams, "MM/dd/yy h:mm tt", current);
                }
                catch (FormatException)
                {
                    await ReplyAsync("Format needs to be mm/dd/yy HH:MM AM/PM timezone(est / edt, cst / cdt, pst / pdt, mst / mdt) EXAMPLE: '!schedule @yourteam @enemyteam 02/28/21 8:00 pm est'");
                    return;
                }

                DateTime convertedDate = DateTime.Parse("12/30/2020");

                int numAddToDate = 0;
                // time[0] = time[0] + "/20";


                var channel = Context.Guild as SocketGuild;


                var userThatCalledSchedule = Context.User as SocketGuildUser;
                var roleName = team2.ToString();
                Console.WriteLine(roleName);
                //var listOfUsers = (role as IChannel).GetUsersAsync(default, default);
                var listOfUsers = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName).Members;
                var google = new googleSheet();

                bool reschedule = false;


                var rolePermissionAdmin = (userThatCalledSchedule as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
                var captainPermission = (userThatCalledSchedule as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Team Captain");
                if (userThatCalledSchedule.Roles.Contains(rolePermissionAdmin) || (userThatCalledSchedule.Roles.Contains(captainPermission) && userThatCalledSchedule.Roles.Contains(team1)))
                {
                    int positionOfYourTeam = 0;
                    int positionOfOpposingTeam = 0;
                    int howManyInListYourTeam = 0;
                    int howManyInListOpposingTeam = 0;
                    Console.Write("is admin");
                    try
                    {
                        //makes sure that there is a pending scheduled match with mongo and 
                        match = await mongo.GetChallengeDetails(team1.Name, team2.Name);
                        if (match == null)
                        {
                            match = await mongo.GetScheduleDetails(team1.Name, team2.Name, reschedule, "blank");

                            match.WhichTeamSchedule = team1.ToString();
                            match.MatchStatus = locationOfInput;
                            string[] confirmYourTeam = google.Confirm(team1.ToString(), team2.ToString(), checkIfInSchedule, false);
                            string[] confirmOpposingTeam = google.Confirm(team1.ToString(), team2.ToString(), checkIfInSchedule, true);


                            if (match.MatchStatus == checkIfInSchedule || match.MatchStatus == "pendingschedule")
                            {

                                if (confirmYourTeam[0] == team2.Name)
                                {
                                    positionOfYourTeam = Int32.Parse(confirmYourTeam[1]);
                                    positionOfOpposingTeam = Int32.Parse(confirmOpposingTeam[1]);
                                    //have to add one since its erasing and adding at the same time.
                                    howManyInListYourTeam = Int32.Parse(confirmYourTeam[2]) + 1;
                                    howManyInListOpposingTeam = Int32.Parse(confirmOpposingTeam[2]) + 1;
                                    string convertedDateOfScheduledMatch = google.GetMatchDate(team1.ToString(), false, positionOfYourTeam).ToString();
                                    await ReplyAsync("You already have a game scheduled with this team for " + convertedDateOfScheduledMatch + " do you want to reschedule? 'yes' or ' no'");
                                    Console.WriteLine("Previously scheduled match");
                                    var nextReply = await NextMessageAsync();
                                    if (nextReply.ToString().ToLower() == "yes")
                                    {

                                        reschedule = true;


                                    }
                                    else if (nextReply.ToString().ToLower() == "no")
                                    {
                                        await ReplyAsync("Chose no, nothing done");
                                        return;
                                    }
                                    else
                                    {
                                        await ReplyAsync("No valid answer given, please try again");
                                        return;
                                    }
                                }
                                else
                                {
                                    await ReplyAsync("Team not in schedule or challenge list");
                                }

                            }
                            else
                            {
                                await ReplyAsync("Team not in schedule or challenge list");
                                return;
                            }
                        }
                        else
                        {
                            match.WhichTeamSchedule = team1.ToString();
                            match.MatchStatus = locationOfInput;


                            string[] confirmYourTeamChallenge = google.Confirm(team1.ToString(), team2.ToString(), removeChallenge, false);
                            string[] confirmOpposingTeamChallenge = google.Confirm(team1.ToString(), team2.ToString(), removeChallenge, true);
                            if (confirmYourTeamChallenge[0] == team2.ToString())
                            {
                                Console.WriteLine("opposing team and confirm[0] are the same");
                                positionOfYourTeam = Int32.Parse(confirmYourTeamChallenge[1]);
                                positionOfOpposingTeam = Int32.Parse(confirmOpposingTeamChallenge[1]);
                                howManyInListYourTeam = Int32.Parse(confirmYourTeamChallenge[2]);
                                howManyInListOpposingTeam = Int32.Parse(confirmOpposingTeamChallenge[2]);
                            }

                            else
                            {
                                await ReplyAsync("Team not in challenge list.");
                                return;
                            }
                        }
                    }

                    catch
                    {
                        await ReplyAsync("Something went wrong, please contact admins");
                        return;
                    }







                    Console.WriteLine("list of users " + listOfUsers);

                    if (time[3].ToLower() == "pst" || time[3].ToLower() == "pdt")
                    {
                        convertedDate = scheduledDate.AddHours(3);
                        Console.WriteLine("back 3 hours");
                        numAddToDate = 3;
                    }
                    else if (time[3].ToLower() == "mst" || time[3].ToLower() == "mdt")
                    {
                        convertedDate = scheduledDate.AddHours(2);
                        Console.WriteLine("Back two hours");
                        numAddToDate = 2;
                    }
                    else if (time[3].ToLower() == "cst" || time[3].ToLower() == "cdt")
                    {
                        convertedDate = scheduledDate.AddHours(1);
                        Console.WriteLine("Back one hour");
                        numAddToDate = 1;
                    }
                    else if (time[3].ToLower() == "est" || time[3].ToLower() == "edt")
                    {
                        Console.WriteLine("current Time");
                        convertedDate = scheduledDate;
                        numAddToDate = 0;
                    }
                    else
                    {
                        await ReplyAsync("Did not specify timezone using 'pst/pdt','mst,mdt','cst,cdt','est,edt'");
                        return;
                    }

                    Console.WriteLine("converted date" + convertedDate);
                    Console.WriteLine("scheduled date " + scheduledDate);



                    match.Scheduled = convertedDate.ToString();
                    bool mongoSchedule = false;
                    if (!reschedule)
                    {
                        mongoSchedule = await mongo.Schedule(match);
                        if (!mongoSchedule)
                        {
                            await ReplyAsync("Failed to insert schedule into database.");
                            return;
                        }
                    }
                    else
                    {
                        await mongo.GetScheduleDetails(team1.Name, team2.Name, reschedule, convertedDate.ToString());
                    }



                    var scheduleSheet = google.Schedule(team1.ToString(), team2.ToString(), convertedDate, locationOfInput);
                    if (scheduleSheet == null)
                    {
                        if (!reschedule)
                        {
                            google.RemoveSchedule(team1.ToString(), positionOfYourTeam, howManyInListYourTeam, team2.ToString(), positionOfOpposingTeam, howManyInListOpposingTeam, typeOfRemove);
                        }
                        else
                        {
                            google.RemoveSchedule(team1.ToString(), positionOfYourTeam, howManyInListYourTeam, team2.ToString(), positionOfOpposingTeam, howManyInListOpposingTeam, "preschedule");
                        }
                        foreach (var user in listOfUsers)
                        {

                            var isCaptain = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Team Captain");
                            if (user.Roles.Contains(isCaptain))
                            {

                                await ReplyAsync("Sent dm for confirmation of schedule to " + user.Username);
                                Console.WriteLine(user.Username + " is a captain");

                                var channelb = await user.GetOrCreateDMChannelAsync();

                                await channelb.SendMessageAsync($"Hello, {user.Username}! " + Context.User.Username + " from " + team1.ToString() + " has attempted to schedule a match with your team on " + convertedDate + " est/edt. \n Please use !confirmschedule @" + team2.ToString() + " @" + team1 + " in botcommands or your teamchannel to confirm the schedule.");
                            }
                            Console.WriteLine("in for each" + user);

                        }
                        await ReplyAsync("Sending dm to captain of " + team2 + " to confirm time");
                        var channelIdLogs = channel.GetTextChannel(767455535800385616).SendMessageAsync(Context.User.Username + " has used the schedule command for " + team1.ToString() + " versus " + team2.ToString() + " at " + convertedDate);
                    }
                    else
                    {
                        await ReplyAsync(scheduleSheet);
                        return;
                    }
                }
                else
                {
                    await ReplyAsync("You are not the captain of the first team " + team1.ToString() + " or an admin.");
                }
            }

            /*[Command("name")]


             public async Task nameSheet(IRole team)
             {

                 var channel = Context.Guild as SocketGuild;
                 //var channelIdLogs = channel.GetTextChannel(767455535800385616).SendMessageAsync(Context.User.Username + " has used the msg command for " + role);

                 var admin = Context.User as SocketGuildUser;
                 var roleName = team.ToString();
                 Console.WriteLine(roleName);
                 //var listOfUsers = (role as IChannel).GetUsersAsync(default, default);
                 var listOfUsers = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName).Members;


                 Console.WriteLine("list of users " + listOfUsers);

                 var rolePermissionAdmin = (admin as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
                 if (admin.Roles.Contains(rolePermissionAdmin))
                 {
                     Console.Write("is admin");

                     foreach (var user in listOfUsers)
                     {

                         var isCaptain = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Team Captain");
                         if (user.Roles.Contains(isCaptain))
                         {

                             Console.WriteLine(user.Username + " is a captain");
                             await ReplyAsync("captain found");
                             var channelb = await user.GetOrCreateDMChannelAsync();

                             await channelb.SendMessageAsync($"Hello, {user.Username}! " + Context.User.Username + " has attempted to schedule a match with your team on ");
                         }
                         Console.WriteLine("in for each" + user);

                         /*try
                         {
                             var channelb = await user.GetOrCreateDMChannelAsync();

                             await channelb.SendMessageAsync($"Hello, {user.Username}! " + message);

                         }

                         catch (Discord.Net.HttpException ex) when (ex.HttpCode == HttpStatusCode.Forbidden)
                         {
                             Console.WriteLine($"Boo, I cannot message {user}.");
                             await ReplyAsync($"I cannot message {user}");
                             continue;
                         }
                     }
                 }

             }*/

            public async Task<string> teamRosterOld(IRole teamName)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return null;
                }

                //var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
                //if (user.Roles.Contains(rolePermissionAdmin))


                var teamNameWithoutNumber = teamName.Name;
                var google = new googleSheet();
                string stringOfIgns = null;
                string oneMessage = null;
                string[,] values = google.TeamRoster(teamNameWithoutNumber);
                //Console.WriteLine("am I in here?" + google.Google());
                if (values == null)
                {
                    await ReplyAsync("Role does not match a team in google sheets roster");
                }
                else
                {
                    int num = 0;
                    foreach (var row in values)
                    {

                        //await ReplyAsync($"{row[0]}, {row[1]}, {row[2]}");

                        if (num == 1)
                        {
                            oneMessage = oneMessage + " " + (row);
                            stringOfIgns = stringOfIgns + row + ",";
                        }
                        else if (num == 2)
                        {
                            oneMessage = oneMessage + " " + (row) + "\n";
                            num = -1;
                        }
                        else
                        {
                            oneMessage = oneMessage + (row);
                        }
                        num++;

                    }


                    return (oneMessage + "\nhttps://na.op.gg/multi/query=" + HttpUtility.UrlEncode(stringOfIgns) + "\nIf an op.gg doesn't show up, please contact Admins.");
                    // await ReplyAsync("If there are not 5 IGNs, please contact Admins.");


                }

                return "Nothing found on either google sheets or in database, contact an Admin if you think this is in error";
                /*else
                {
                    await ReplyAsync("You do not have permission to use this command");
                }*/
            }

            [Command("imposter")]
            [Summary("!imposter @person")]
            public async Task imposter(IGuildUser user)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }
                await ReplyAsync(user.Username + " is acting kinda sus...");

            }
            /* [Command("updateroster")]
             // updates roster, can be used by team captain
             public async Task UpdateRoster(IUser user)
             {
                 var user = Context.User as SocketGuildUser;
                 var roleOfUser = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Team Captain");

             }*/


            //test how much you would win
            [Command("testresult", RunMode = RunMode.Async)]
            [Summary("!testresult @yourteam @otherteam 2-0 (lets you check elo gain)")]
            public async Task TestResult(IRole team, IRole opponentTeam, string result)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }
                var user = Context.User as SocketGuildUser;
                var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");



                var channel = Context.Guild as SocketGuild;
                var channelIdLogs = channel.GetTextChannel(767455535800385616).SendMessageAsync(Context.User.Username + " has used testresult command ");



                //add blank string if less than 3 links


                //these are ran opposite for both teams, i.e. times to run win is added to loss for second team and times to run lose is added to the second team's win.
                int timesToRunWin = 0;
                int timesToRunLose = 0;
                var google = new googleSheet();
                double[] teams = google.GetElo(team.ToString(), opponentTeam.ToString());
                Console.WriteLine(teams);
                switch (result)
                {
                    case "2-0":
                        timesToRunWin = 2;

                        break;
                    case "2-1":
                        timesToRunWin = 2;
                        timesToRunLose = 1;
                        break;
                    case "1-2":
                        timesToRunWin = 1;
                        timesToRunLose = 2;
                        break;
                    case "0-2":
                        timesToRunLose = 2;
                        break;
                    default:
                        await ReplyAsync("Incorrect format or amount of games, can be 2-1, 2-0, 1-2, 0-2");
                        return;

                }


                if (teams != null)
                {
                    Console.WriteLine("Made it into teams not null");
                    double firstElo = teams[0];
                    double secondElo = teams[1];




                    double transformedFirstElo = Math.Pow(10, (firstElo / 400));
                    double transformedSecondElo = Math.Pow(10, (secondElo / 400));

                    double formulaFirstElo = transformedFirstElo / (transformedFirstElo + transformedSecondElo);
                    double formulaSecondElo = transformedSecondElo / (transformedFirstElo + transformedSecondElo);


                    int howBigOfEloSwing = 40;




                    double firstEloCalculatedIfWin = firstElo + howBigOfEloSwing * (1 - formulaFirstElo);
                    double firstEloCalculatedIfLose = firstElo + howBigOfEloSwing * (0 - formulaFirstElo);
                    double secondEloCalculatedIfWin = secondElo + howBigOfEloSwing * (1 - formulaSecondElo);
                    double secondEloCalculatedIfLose = secondElo + howBigOfEloSwing * (0 - formulaSecondElo);

                    //get information of elo going up and down for both teams
                    firstEloCalculatedIfWin = Math.Round(firstEloCalculatedIfWin - firstElo, 1);
                    firstEloCalculatedIfLose = Math.Round(firstEloCalculatedIfLose - firstElo, 1);
                    secondEloCalculatedIfWin = Math.Round(secondEloCalculatedIfWin - secondElo, 1);
                    secondEloCalculatedIfLose = Math.Round(secondEloCalculatedIfLose - secondElo, 1);
                    //calculatefinished match for first called team
                    double finishedEloFirstTeam = Math.Round((firstElo + firstEloCalculatedIfWin * timesToRunWin + firstEloCalculatedIfLose * timesToRunLose), 1);
                    //calculatedfinished match for second called team
                    double finishedEloSecondTeam = Math.Round((secondElo + secondEloCalculatedIfWin * timesToRunLose + secondEloCalculatedIfLose * timesToRunWin), 1);

                    double firstTeamChanged = Math.Round((firstEloCalculatedIfWin * timesToRunWin + firstEloCalculatedIfLose * timesToRunLose), 1);
                    double secondTeamChanged = Math.Round((secondEloCalculatedIfWin * timesToRunLose + secondEloCalculatedIfLose * timesToRunWin), 1);



                    var resultOfFirstTeam = timesToRunWin + "-" + timesToRunLose;
                    var resultOfSecondTeam = timesToRunLose + "-" + timesToRunWin;


                    Console.WriteLine("Run win command " + timesToRunWin + " and run lose command " + timesToRunLose);


                    await ReplyAsync(team.ToString() + " elo changes by " + firstTeamChanged + ". " + opponentTeam.ToString() + " elo changes by " + secondTeamChanged);

                }


            }


            //[Command("checkign")]
            public async Task<string[]> CheckIgn(string accountId)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return null;
                }
                string riotkey;
                riotkey = ConfigurationManager.AppSettings.Get("riotkey");
                IRiotClient client = new RiotClient(new RiotClientSettings
                {
                    ApiKey = riotkey
                });
                string[] playerArray = new string[2];
                Player mongoPlayer = new Player();
                var mongo = new MongoDB();
                try
                {
                    Summoner summoner = await client.GetSummonerBySummonerIdAsync(accountId, PlatformId.NA1).ConfigureAwait(false);
                    if (summoner == null)
                    {
                        await ReplyAsync("Unfortnately, there isn't a player by that name. Please try again");
                        return null;
                    }
                    else if (summoner != null)
                    {
                        mongoPlayer.SummonerId = accountId;


                        bool foundSoloRank = false;
                        List<LeagueEntry> lists = await client.GetLeagueEntriesBySummonerIdAsync(summoner.Id.ToString(), PlatformId.NA1).ConfigureAwait(false);
                        foreach (var item in lists)
                        {
                            if (item.QueueType == "RANKED_SOLO_5x5")
                            {
                                foundSoloRank = true;
                            }

                        }
                        if (foundSoloRank)
                        {
                            var loopThruElements = 0;
                            var rank = lists[loopThruElements];
                            while (rank.QueueType != "RANKED_SOLO_5x5")
                            {
                                loopThruElements++;
                                rank = lists[loopThruElements];
                            }



                            mongoPlayer.Rank = rank.Tier + " " + rank.Rank;
                            mongoPlayer.Ign = summoner.Name;

                        }
                        else
                        {
                            opggScraper rank = new opggScraper();
                            var whatRank = await rank.ScrapeMe(summoner.Name);
                            string isRealRank = await checkifRealRank(whatRank.ToString().ToUpper());

                            if (isRealRank != null)
                            {
                                mongoPlayer.Rank = isRealRank;
                                mongoPlayer.Ign = summoner.Name;

                            }

                        }



                        var insert = await mongo.inputPlayer(mongoPlayer, true);
                        playerArray[0] = mongoPlayer.Ign;
                        playerArray[1] = mongoPlayer.Rank;
                        return playerArray;
                    }
                }
                catch
                {
                    //await ReplyAsync("Unfortnately, there isn't a player by that name, or they haven't played ranked this season. Please try again");
                    return null;
                }







                return null;

            }


            //gets elo range around team
            [Command("teamrange")]
            [Summary("!teamrange @team (gives list of teams that are in that elo range)")]
            public async Task teamEloRange(IRole team)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }
                Console.WriteLine("made it in");
                MongoDB mongo = new MongoDB();
                try
                {
                    await ReplyAsync(await mongo.teamEloRange(team.Name));
                }
                catch
                {
                    await ReplyAsync("team has either not been rated for elo, or does not exist in the database.");
                }
            }


            public async Task<string> checkifRealRank(string rank)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return null;
                }

                switch (rank)
                {
                    case "CHALLENGER":
                        return "CHALLENGER";


                    case "GRANDMASTER":
                        return "GRANDMASTER";
                    case "MASTER":
                        return "MASTER";
                    case "DIAMOND 1":
                        return "DIAMOND I";
                    case "DIAMOND 2":
                        return "DIAMOND II";
                    case "DIAMOND 3":
                        return "DIAMOND III";
                    case "DIAMOND 4":
                        return "DIAMOND IV";
                    case "PLATINUM 1":
                        return "PLATINUM I";
                    case "PLATINUM 2":
                        return "PLATINUM II";
                    case "PLATINUM 3":
                        return "PLATINUM III";
                    case "PLATINUM 4":
                        return "PLATINUM IV";
                    case "GOLD 1":
                        return "GOLD I";
                    case "GOLD 2":
                        return "GOLD II";
                    case "GOLD 3":
                        return "GOLD III";
                    case "GOLD 4":
                        return "GOLD IV";
                    case "SILVER 1":
                        return "SILVER I";
                    case "SILVER 2":
                        return "SILVER II";
                    case "SILVER 3":
                        return "SILVER III";
                    case "SILVER 4":
                        return "SILVER IV";
                    case "BRONZE 1":
                        return "BRONZE I";
                    case "BRONZE 2":
                        return "BRONZE II";
                    case "BRONZE 3":
                        return "BRONZE III";
                    case "BRONZE 4":
                        return "BRONZE IV";
                    case "IRON 1":
                        return "IRON I";
                    case "IRON 2":
                        return "IRON II";
                    case "IRON 3":
                        return "IRON III";
                    case "IRON 4":
                        return "IRON IV";
                    default:
                        Console.WriteLine("Nothing");
                        return null;

                }
            }

            [Command("add", RunMode = RunMode.Async)]
            //can be used by admin/owner to create new team on google sheet api
            [Summary
           ("!add @team role @player (add to team)")]
            public async Task addToTeam(IRole team, string role = "nope", IGuildUser calledUser = null)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }
                // commented out until start pushing database
                //holds int for number of subs
                int numberOfSubs = 0;
                //initializes googlesheet
                var google = new googleSheet();

                var channel = Context.Guild as SocketGuild;
                var mongo = new MongoDB();
                var mongoPlayer = new Player();
                var mongoTeam = new Team();

                string[] whatToChange = new string[3];
                whatToChange[0] = team.ToString();

                string[] holdPlayerSummonerId = new string[1];

                //check if while loop should run
                bool run = false;



                //ulong userId = ulong.Parse(Id[0].ToString());


                Console.WriteLine(calledUser);
                var user = Context.User as SocketGuildUser;
                var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");

                Console.WriteLine("made it past admin");
                var rolePermissionOwner = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Owner");


                var captainRole = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Team Captain");

                //checks if role admin or owner or teamcaptain of team
                if ((user.Roles.Contains(rolePermissionAdmin)) || (user.Roles.Contains(rolePermissionOwner)) || ((user.Roles.Contains(captainRole) && user.Roles.Contains(team))))
                {
                    if (role == "top")
                    {
                        whatToChange[1] = "Top";
                        Console.WriteLine("adding top");

                    }
                    else if (role == "jg")
                    {
                        whatToChange[1] = "Jg";
                        Console.WriteLine("adding jg");
                    }
                    else if (role == "mid")
                    {
                        whatToChange[1] = "Mid";
                        Console.WriteLine("adding mid");
                    }
                    else if (role == "adc")
                    {
                        whatToChange[1] = "Adc";
                        Console.WriteLine("adding adc");
                    }
                    else if (role == "sup")
                    {
                        whatToChange[1] = "Sup";
                        Console.WriteLine("adding sup");
                    }
                    else if (role == "sub")
                    {
                        whatToChange[1] = "Subs";
                        numberOfSubs = google.CheckSubCount(team.ToString());
                        Console.WriteLine("adding sub");


                    }
                    else
                    {
                        await ReplyAsync("You did not enter a correct role format, please use 'top','jg','mid','adc','sup','sub' for example !add @teamrole mid @midsdiscord");
                        return;
                    }
                    try
                    {
                        //keep asking until false
                        while (run == false)
                        {
                            await ReplyAsync("What is the ign of the player?");
                            var response = await NextMessageAsync();
                            string riotkey;
                            riotkey = ConfigurationManager.AppSettings.Get("riotkey");

                            //string of combine summoner rank
                            string rankOfIgn = null;
                            //makes sure to not use riot api if no response
                            if (response == null)
                            {
                                await ReplyAsync("no ign given");
                                return;
                            }
                            IRiotClient client = new RiotClient(new RiotClientSettings
                            {
                                ApiKey = riotkey
                            });
                            try
                            {
                                Summoner summoner = await client.GetSummonerBySummonerNameAsync(response.ToString(), PlatformId.NA1).ConfigureAwait(false); if (summoner == null)
                                {
                                    await ReplyAsync("Unfortnately, there isn't a player by that name. Please try again");
                                    return;
                                }
                                else if (summoner != null)
                                {
                                    bool foundSoloRank = false;
                                    List<LeagueEntry> lists = await client.GetLeagueEntriesBySummonerIdAsync(summoner.Id.ToString(), PlatformId.NA1).ConfigureAwait(false);
                                    foreach (var item in lists)
                                    {
                                        if (item.QueueType == "RANKED_SOLO_5x5")
                                        {
                                            foundSoloRank = true;
                                        }

                                    }
                                    if (foundSoloRank)
                                    {
                                        var loopThruElements = 0;
                                        var rank = lists[loopThruElements];
                                        while (rank.QueueType != "RANKED_SOLO_5x5")
                                        {
                                            loopThruElements++;
                                            rank = lists[loopThruElements];
                                        }


                                        await ReplyAsync("This is the player I found, " + "Name: " + summoner.Name + " Rank: " + rank.Tier + " " + rank.Rank + " is this the correct? Reply with 'yes' or 'no'");
                                        mongoPlayer.Rank = rank.Tier + " " + rank.Rank;
                                        mongoPlayer.Ign = summoner.Name;
                                        mongoPlayer.SummonerId = summoner.Id;
                                        mongoPlayer.CurrentTeam = team.Name;
                                        mongoPlayer.DiscordName = calledUser.ToString();
                                        whatToChange[2] = summoner.Name;
                                        rankOfIgn = rank.Tier + " " + rank.Rank;
                                        holdPlayerSummonerId[0] = summoner.Id.ToString();
                                    }
                                    else
                                    {
                                        opggScraper rank = new opggScraper();
                                        var whatRank = await rank.ScrapeMe(summoner.Name);
                                        string isRealRank = await checkifRealRank(whatRank.ToString().ToUpper());

                                        if (isRealRank != null)
                                        {
                                            mongoPlayer.Rank = isRealRank;
                                            mongoPlayer.Ign = summoner.Name;
                                            mongoPlayer.SummonerId = summoner.Id;
                                            mongoPlayer.CurrentTeam = team.Name;
                                            mongoPlayer.DiscordName = calledUser.ToString();
                                            whatToChange[2] = summoner.Name;
                                            rankOfIgn = isRealRank;
                                            holdPlayerSummonerId[0] = summoner.Id.ToString();
                                            await ReplyAsync("This is the player I found, " + "Name: " + summoner.Name + " Rank: " + isRealRank + "(from last season, they haven't finished promos this season) is this the correct? Reply with 'yes' or 'no'");
                                        }
                                        else
                                        {
                                            await ReplyAsync("I'm sorry, the player either doesn't exist or hasn't played ranked last season or this season");
                                            return;
                                        }
                                    }

                                }
                            }
                            catch
                            {
                                await ReplyAsync("Unfortnately, there isn't a player by that name, or they haven't played ranked this season. Please try again");
                                return;
                            }


                            //await ReplyAsync("Name: " + summoner.Name + " Rank: " + rank.Tier + " " + rank.Rank);


                            //Console.WriteLine(data);





                            var confirmation = await NextMessageAsync();
                            if (confirmation == null)
                            {
                                await ReplyAsync("No response");
                                return;
                            }
                            if (confirmation.ToString().ToLower() == "yes")
                            {




                                Console.WriteLine("made it to yes answer");
                                run = true;
                                Console.WriteLine("made it to true");
                                string confirm = response.ToString();


                                Console.WriteLine("made it to cast");
                                await ReplyAsync("Cool, you've confirmed the ign");

                                //array to test if player is in our system


                                try
                                {
                                    if (role == "sub")
                                    {
                                        if (numberOfSubs < 6)
                                        {
                                            bool playerInDataBase = await mongo.inputPlayer(mongoPlayer, false);
                                            bool teamInDataBase = await mongo.updateTeam(whatToChange);

                                            if (playerInDataBase && teamInDataBase)
                                            {
                                                await ReplyAsync("Player has been inputted into the database and added to the team");
                                            }

                                            var sendToGoogle = google.addToTeam(team.ToString(), role, confirm, rankOfIgn, numberOfSubs);
                                            await ReplyAsync(confirm + " successfully added to google sheet!");
                                            await calledUser.AddRoleAsync(team);
                                        }
                                        else
                                        {
                                            await ReplyAsync("You have six subs, do you want to overwrite? 'yes' or 'no' ");
                                            var shouldWeOverwriteASub = await NextMessageAsync();
                                            if (shouldWeOverwriteASub == null)
                                            {
                                                await ReplyAsync("No response");
                                                return;
                                            }
                                            if (shouldWeOverwriteASub.ToString().ToLower() == "yes")
                                            {
                                                bool playerInDataBase = await mongo.inputPlayer(mongoPlayer, false);


                                                if (playerInDataBase)
                                                {
                                                    await ReplyAsync("Player has been inputted into the database and added to the team");
                                                }

                                                //what to do if they say yes :(

                                                string oneMessage = null;
                                                IList<IList<Object>> values = google.SubDestroyer(team.ToString());
                                                //Console.WriteLine("am I in here?" + google.Google());
                                                if (values == null)
                                                {
                                                    await ReplyAsync("Role does not match a team in google sheets roster");
                                                }
                                                else
                                                {
                                                    int num = 1;
                                                    foreach (var row in values)
                                                    {

                                                        //await ReplyAsync($"{row[0]}, {row[1]}, {row[2]}");
                                                        oneMessage = oneMessage + $"{num}. {row[0]}\n";
                                                        num++;


                                                    }


                                                }
                                                await ReplyAsync(oneMessage + "\n Enter the number that corresponds with the sub you want to overwrite");
                                                var numberOfAxedSub = await NextMessageAsync();
                                                if (numberOfAxedSub == null)
                                                {
                                                    await ReplyAsync("No answer");
                                                    return;
                                                }
                                                if (numberOfAxedSub.ToString() == "1" || numberOfAxedSub.ToString() == "2" || numberOfAxedSub.ToString() == "3" || numberOfAxedSub.ToString() == "4"
                                                    || numberOfAxedSub.ToString() == "5" || numberOfAxedSub.ToString() == "6")
                                                {
                                                    int intNumberOfAxedSub = Int32.Parse(numberOfAxedSub.ToString());
                                                    await mongo.RemovePlayer(team.Name, whatToChange[1].ToString(), values[intNumberOfAxedSub - 1]);
                                                    bool teamInDataBase = await mongo.updateTeam(whatToChange);
                                                    var sendToGoogle = google.addToTeam(team.ToString(), role, confirm, rankOfIgn, intNumberOfAxedSub - 1);
                                                    await ReplyAsync(confirm + " successfully added to google sheet!");
                                                    await calledUser.AddRoleAsync(team);
                                                }
                                                else if (numberOfAxedSub == null)
                                                {
                                                    await ReplyAsync("No answer");
                                                    return;
                                                }
                                                else
                                                {
                                                    await ReplyAsync("Use numbers 1-6 to indicate answer");
                                                    return;
                                                }

                                            }
                                            else if (shouldWeOverwriteASub == null)
                                            {
                                                await ReplyAsync("No sub was replaced");
                                                return;
                                            }
                                            else
                                            {
                                                await ReplyAsync("No sub was replaced");
                                                return;
                                            }

                                        }

                                    }
                                    else
                                    {
                                        bool playerInDataBase = await mongo.inputPlayer(mongoPlayer, false);
                                        bool teamInDataBase = await mongo.updateTeam(whatToChange);

                                        if (playerInDataBase && teamInDataBase)
                                        {
                                            await ReplyAsync("Player has been inputted into the database and added to the team");
                                        }


                                        var sendToGoogle = google.addToTeam(team.ToString(), role, confirm, rankOfIgn);
                                        await ReplyAsync(confirm + " successfully added to google sheet!");
                                        await calledUser.AddRoleAsync(team);
                                    }

                                    var channelIdLogs = channel.GetTextChannel(767455535800385616).SendMessageAsync(Context.User.Username + " has used the addtoteam command to add " + calledUser + " with ign of " + confirm + "for the role  " + role);
                                }
                                catch
                                {
                                    await ReplyAsync("No sheet with that role, please contact admins");
                                }
                            }
                            else if (confirmation == null)
                            {
                                await ReplyAsync("Please try again");
                            }

                        }



                    }
                    catch
                    {

                    }

                }
                else
                {
                    await ReplyAsync("You are not owner/admin/captain of called team");
                }





            }

            //removes player from team
            [Command("remove", RunMode = RunMode.Async)]
            [Summary("!remove @team @player (removes role from player so they can't access the team channel)")]
            public async Task RemoveFromTeam(IRole role, IGuildUser removedUser)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }
                var channel = Context.Guild as SocketGuild;
                var mongo = new MongoDB();
                var google = new googleSheet();

                var channelIdLogs = channel.GetTextChannel(767455535800385616).SendMessageAsync(Context.User.Username + " has used the remove command to remove role  " + role + " from " + removedUser);


                var user = Context.User as SocketGuildUser;
                var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");

                Console.WriteLine("made it past admin");
                var rolePermissionOwner = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Owner");


                var captainRole = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Team Captain");



                //checks if role admin or owner
                if ((user.Roles.Contains(rolePermissionAdmin)) || (user.Roles.Contains(rolePermissionOwner)) || ((user.Roles.Contains(captainRole) && user.Roles.Contains(role))))
                {
                    string exists = await mongo.getIgnFromDiscord(role.Name, removedUser.ToString());

                    if (exists != null)
                    {

                        List<int> potentialSubPosition = await mongo.RemovePlayer(role.ToString(), exists);

                        if (potentialSubPosition[0] == -1)
                        {
                            Console.WriteLine("not a sub");
                        }
                        else
                        {

                            await google.RemoveSub(role.ToString(), potentialSubPosition);
                        }
                        await ReplyAsync("Deleted player from team");
                    }
                    else
                    {
                        await ReplyAsync("We do not have that discord name on file, please type the ign of the player you want to remove.");
                        var ignFromInput = await NextMessageAsync();
                        if (ignFromInput == null)
                        {
                            await ReplyAsync("Ign not entered, please try again");
                        }
                        else
                        {
                            exists = await mongo.playerIfNoDiscord(ignFromInput.ToString());
                            if (exists != null)
                            {
                                List<int> potentialSubPosition = await mongo.RemovePlayer(role.ToString(), exists);

                                if (potentialSubPosition[0] == -1)
                                {
                                    Console.WriteLine("not a sub");
                                }
                                else
                                {

                                    await google.RemoveSub(role.ToString(), potentialSubPosition);
                                }
                                await ReplyAsync("Deleted player from team");

                            }
                            else
                            {
                                await ReplyAsync("Ign not in database, make sure to have correct one or contact admins if you're sure its correct. Please try again");
                                return;
                            }

                        }
                    }
                    await removedUser.RemoveRoleAsync(role);
                    await ReplyAndDeleteAsync("Role removed");
                }
                else
                {
                    await ReplyAndDeleteAsync("You are not owner/admin/captain of called team");
                }
            }
            [Command("Help")]
            public async Task Help()
            {

                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }

                //List<CommandInfo> commands = _commands.Commands.

                EmbedBuilder embedBuilder = new EmbedBuilder();

                string description = null;
                foreach (var cmd in _commands.Modules)
                {
                    if (cmd.Name == "UserCommands")

                        foreach (var incmd in cmd.Commands)
                        {


                            {
                                var result = await incmd.CheckPreconditionsAsync(Context);
                                if (result.IsSuccess)
                                    description = incmd.Summary + "\n" ?? "No description available\n";
                                //$"{cmd.Aliases.First()}\n";


                                if (!string.IsNullOrWhiteSpace(description))
                                {
                                    embedBuilder.AddField(x =>
                                    {
                                        x.Name = incmd.Name;
                                        x.Value = description;
                                        x.IsInline = false;
                                    });
                                }
                            }
                        }
                }

                /* [Command("checkperms")]
                 public async Task checkPerms(IRole roleName)
                 {
                     var user = Context.User as SocketGuildUser;
                     var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
                     if (user.Roles.Contains(rolePermissionAdmin))
                     {


                         var roles = roleName.Permissions.ToString();



                         await ReplyAsync(roles);

                     }

                 }*/

                /*[Command("checkcatid")]
                public async Task checkcatid(IGuildChannel id)
                {

                    var user = Context.User as SocketGuildUser;
                    var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
                    if (user.Roles.Contains(rolePermissionAdmin))
                    {

                        var guildchannelID = id.GuildId.ToString();


                        await ReplyAsync(guildchannelID);


                    }
                }*/
                //moving to new database
                await ReplyAsync("", false, embedBuilder.Build());
            }
        }

        public class adminCommands : InteractiveBase
        {
#if DEBUG
            public bool work = false;
#else
        public bool work = true;
#endif
            public CommandService _adminCommands;
            public adminCommands(CommandService service)
            {
                _adminCommands = service;
            }
            [Command("Migrate", RunMode = RunMode.Async)]
            [Summary("not needed anymore, migrated old google sheet teams to new database")]

            public async Task Migrate(IRole teamName)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }
                var user = Context.User as SocketGuildUser;
                var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
                if (user.Roles.Contains(rolePermissionAdmin))
                {
                    try
                    {
                        await ReplyAsync("How many wins has this team had?");
                        var responseToWins = await NextMessageAsync();

                        Console.WriteLine(responseToWins);
                        int wins = Int32.Parse(responseToWins.ToString());
                        await ReplyAsync("How many losses has this team had?");
                        var responseToLosses = await NextMessageAsync();
                        int losses = Int32.Parse(responseToLosses.ToString());

                        //gets riotkey from config
                        string riotkey;
                        riotkey = ConfigurationManager.AppSettings.Get("riotkey");

                        var teamNameWithoutNumber = teamName.Name;
                        var google = new googleSheet();
                        int numCount = 0;
                        string[,] values = google.TeamRoster(teamNameWithoutNumber);
                        decimal elo = google.GetElo(teamNameWithoutNumber);
                        var mongo = new mongo();

                        Console.WriteLine(values.Length);
                        //assigning values to object properties for team migration, must go through player creation first to verify players and grab riot id
                        // Player migratePlayer = new Player();

                        //for loop to check each player
                        for (int i = 0; i < 10; i++)
                        {
                            Player migratePlayer = new Player();
                            string rankOfIgn;
                            //initiales new riot lib
                            IRiotClient client = new RiotClient(new RiotClientSettings
                            {
                                ApiKey = riotkey
                            });
                            try
                            {
                                Summoner summoner = await client.GetSummonerBySummonerNameAsync(values[i, 1].ToString(), PlatformId.NA1).ConfigureAwait(false); if (summoner == null)
                                {
                                    await ReplyAsync("Unfortnately, there isn't a player by that name. Please try again " + values[i, 1].ToString());
                                    return;
                                }
                                else if (summoner != null)
                                {



                                    List<LeagueEntry> lists = await client.GetLeagueEntriesBySummonerIdAsync(summoner.Id.ToString(), PlatformId.NA1).ConfigureAwait(false);
                                    Console.WriteLine(lists.ToString());

                                    if (lists.Count() != 0 && lists[0].QueueType.Contains("RANKED_SOLO_5x5"))
                                    {
                                        var loopThruElements = 0;
                                        var rank = lists[loopThruElements];
                                        while (rank.QueueType != "RANKED_SOLO_5x5")
                                        {
                                            loopThruElements++;
                                            rank = lists[loopThruElements];
                                        }


                                        await ReplyAsync("This is the player I found, " + "Name: " + summoner.Name + " Rank: " + rank.Tier + " " + rank.Rank + " is this the correct? Reply with 'yes' or 'no'");
                                        migratePlayer.Rank = rank.Tier + " " + rank.Rank;
                                        migratePlayer.Ign = summoner.Name;
                                        migratePlayer.SummonerId = summoner.Id;
                                        migratePlayer.CurrentTeam = teamName.Name;

                                        await mongo.inputPlayer(migratePlayer, false);
                                        rankOfIgn = rank.Tier + " " + rank.Rank;

                                    }
                                    else
                                    {
                                        opggScraper rank = new opggScraper();
                                        var whatRank = await rank.ScrapeMe(summoner.Name);
                                        UserCommands userCmd = new UserCommands(null);
                                        string isRealRank = await userCmd.checkifRealRank(whatRank.ToString().ToUpper());

                                        if (isRealRank != null)
                                        {
                                            migratePlayer.Rank = isRealRank;
                                            migratePlayer.Ign = summoner.Name;
                                            migratePlayer.SummonerId = summoner.Id;
                                            migratePlayer.CurrentTeam = teamName.Name;


                                            rankOfIgn = isRealRank;

                                            await mongo.inputPlayer(migratePlayer, false);

                                            await ReplyAsync("This is the player I found, " + "Name: " + summoner.Name + " Rank: " + isRealRank + "(from last season, they haven't finished promos this season) is this the correct? Reply with 'yes' or 'no'");
                                        }

                                    }
                                }
                            }
                            catch
                            {
                                await ReplyAsync("Done with creating player cards.");
                                break;
                            }

                        }


                        await ReplyAsync("Done with creating player cards.");

                        var channelText = Context.Guild as SocketGuild;
                        var channelVoice = Context.Guild as SocketGuild;
                        string changeStringToChannelParams = teamNameWithoutNumber.ToString().Replace(" ", "-").ToLower();
                        string channelTextId = "";
                        string channelVoiceId = "";
                        try
                        {
                            channelTextId = channelText.TextChannels.FirstOrDefault(x => x.Name == changeStringToChannelParams).Id.ToString();
                            channelVoiceId = channelText.VoiceChannels.FirstOrDefault(x => x.Name == teamNameWithoutNumber.ToString()).Id.ToString();
                        }
                        catch
                        {
                            await ReplyAsync("Channel id not properly captured.");
                        }
                        Team migrateTeam = new Team();
                        migrateTeam.Top = values[0, 1];
                        migrateTeam.Jg = values[1, 1];
                        migrateTeam.Mid = values[2, 1];
                        migrateTeam.Adc = values[3, 1];
                        migrateTeam.Sup = values[4, 1];
                        migrateTeam.TeamName = teamNameWithoutNumber.ToString();
                        migrateTeam.Elo = elo;
                        migrateTeam.LossesThisSeason = losses;
                        migrateTeam.TotalLosses = losses;
                        migrateTeam.TotalWins = wins;
                        migrateTeam.WinsThisSeason = wins;
                        migrateTeam.TeamTextChannel = channelTextId;
                        migrateTeam.TeamVoiceChannel = channelVoiceId;
                        //runs for the subs
                        for (int i = 5; i < 10; i++)
                        {
                            try
                            {
                                if (values[i, 1].ToString() != null)
                                {
                                    migrateTeam.Subs.Add(values[i, 1].ToString());
                                    numCount++;
                                }
                            }
                            catch
                            {
                                Console.WriteLine("done with array");
                            }

                        }
                        Console.WriteLine(migrateTeam);

                        await mongo.inputTeam(migrateTeam);

                        await ReplyAsync("Team successfully migrated.");

                    }
                    catch
                    {
                        await ReplyAsync("something went wrong");
                    }
                }
                else
                {
                    await ReplyAsync("You aren't an admin");
                }



            }



            [Command("AdminHelp")]
            public async Task Help()
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }



                EmbedBuilder embedBuilder = new EmbedBuilder();

                string description = null;
                foreach (var cmd in _adminCommands.Modules)
                {
                    if (cmd.Name == "adminCommands")
                    {
                        foreach (var incmd in cmd.Commands)
                        {


                            {
                                var result = await incmd.CheckPreconditionsAsync(Context);
                                if (result.IsSuccess)
                                    description = incmd.Summary + "\n" ?? "No description available\n";
                                //$"{cmd.Aliases.First()}\n";


                                if (!string.IsNullOrWhiteSpace(description))
                                {
                                    embedBuilder.AddField(x =>
                                    {
                                        x.Name = incmd.Name;
                                        x.Value = description;
                                        x.IsInline = false;
                                    });
                                }
                            }
                        }
                    }
                }

                /* foreach (CommandInfo command in commands)
                 {
                     // Get the command Summary attribute information
                     string embedFieldText = command.Summary ?? "No description available\n";
                     commandList = commandList + "\n" + command.Name + " " + embedFieldText;


                 }
                 embedBuilder.AddField("list of commands", commandList);*/
                await ReplyAsync("", false, embedBuilder.Build());
            }

            /* [Command("checkValue")]
             public async Task checkDeny(IGuildChannel id)
             {



                // var denyValue = new OverwritePermissions.

                 var allowValue = id.AllowValue.ToString();


                 await ReplyAsync("this is denyvalue" + denyValue);

                 await ReplyAsync("this is approvevalue" + allowValue);



             }*/
            private RiotClientSettings GetTournamentSettings()
            {

                string riotkey;
                riotkey = ConfigurationManager.AppSettings.Get("riotkey");
                return new RiotClientSettings

                {
                    ApiKey = riotkey,
                    UseTournamentStub = true

                };

            }
            [Command("key")]
            public async Task Key()
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }
                IRiotClient client = new RiotClient(GetTournamentSettings());
                long tournamentProviderId = await client.CreateTournamentProviderAsync("http://207.148.13.35.vultr.com:80/", PlatformId.NA1, CancellationToken.None);

                Assert.That(tournamentProviderId, Is.GreaterThan(0));
                Console.WriteLine(tournamentProviderId);

                long tournamentId = await client.CreateTournamentAsync(tournamentProviderId, "test", CancellationToken.None);

                Assert.That(tournamentId, Is.GreaterThan(0));

                Console.WriteLine(tournamentId);
            }
            [Command("validateteam")]
            [Summary("!validateteam @teamname (adds them ongoing list on googlesheet for ranks)")]
            public async Task Validate(IRole teamName)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }
                var user = Context.User as SocketGuildUser;
                var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
                if (user.Roles.Contains(rolePermissionAdmin))
                {
                    var google = new googleSheet();
                    await google.TeamName(teamName.ToString());
                    google.Validate(teamName.ToString());
                    await ReplyAsync(teamName.ToString() + " added to ranking");
                }

            }
            [Command("sheet")]
            [Summary("Creates googlesheet for role")]
            public async Task createSheet(IRole team)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }
                var user = Context.User as SocketGuildUser;
                var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
                if (user.Roles.Contains(rolePermissionAdmin))
                {
                    var google = new googleSheet();
                    var run = google.CreateTeam(team.ToString());
                    await google.TeamName(team.ToString());
                    await ReplyAsync("Google sheet created.");
                }

            }

            //updates rank of team every hour
            [Command("ranklist")]
            [Summary("updates ranklist in team rankings channel")]


            public async Task updateRanks()
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }
                int numUp = 0;

                MongoDB mongo = new MongoDB();

                List<string> ranks = await mongo.GetRanks();


                var channel = Context.Guild;

                var embedBuilder = new EmbedBuilder();
                var messageSender = channel.GetTextChannel(798058859536056391);


                try
                {
                    var messages = await channel.GetTextChannel(798058859536056391).GetMessagesAsync(ranks.Count() + 1).FlattenAsync();
                    var filteredMessages = messages.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14);
                    await (messageSender as ITextChannel).DeleteMessagesAsync(filteredMessages);
                }
                catch
                {
                    Console.WriteLine("nothing to delete");
                }


                /*  

                  // Download X messages starting from Context.Message, which means
                  // that it won't delete the message used to invoke this command.
                  var messagess = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, 1).FlattenAsync();

                  // Note:
                  // FlattenAsync() might show up as a compiler error, because it's
                  // named differently on stable and nightly versions of Discord.Net.
                  // - Discord.Net 1.x: Flatten()
                  // - Discord.Net 2.x: FlattenAsync()

                  // Ensure that the messages aren't older than 14 days,
                  // because trying to bulk delete messages older than that
                  // will result in a bad request.
                  var filteredMessages = messagess.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14);

                  // Get the total amount of messages.
                  var count = filteredMessages.Count();

                  // Check if there are any messages to delete.
                  if (count == 0)
                      await ReplyAsync("Nothing to delete.");

                  else
                  {
                      // The cast here isn't needed if you're using Discord.Net 1.x,
                      // but I'd recommend leaving it as it's what's required on 2.x, so
                      // if you decide to update you won't have to change this line.
                      await (Context.Channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
                      await ReplyAndDeleteAsync($"Removed {count} {(count > 1 ? "messages" : "message")}.");


          */
                foreach (var twentyFive in ranks)
                {


                    var exampleAuthor = new EmbedAuthorBuilder()
                .WithName("Live Style Ranks")

                .WithIconUrl("https://cdn.discordapp.com/attachments/643609428012040202/798093546178740224/GG_transparent.png");
                    var exampleFooter = new EmbedFooterBuilder()
                            .WithText("")
                            .WithIconUrl("https://discord.com/assets/28174a34e77bb5e5310ced9f95cb480b.png");
                    var exampleField = new EmbedFieldBuilder()
                            .WithName("Name Of Teams: Elo")
                            .WithValue(ranks[numUp].ToString())
                            .WithIsInline(true);
                    /*var otherField = new EmbedFieldBuilder()
                            .WithName("Elo")
                            .WithValue(ranks[numUp].ToString())
                            .WithIsInline(true);*/
                    var embed = new EmbedBuilder()
                            .AddField(exampleField)
                            // .AddField(otherField)
                            .WithAuthor(exampleAuthor)
                            .WithFooter(exampleFooter);

                    await messageSender.SendMessageAsync("", false, embed.Build());
                    numUp++;
                }
                //it (messages as SocketUserMessage).ModifyAsync(x => { x.Content = ranks.ToString(); x.Embed = embedBuilder.Build(); });




            }
            [Command("purge")]
            [Alias("clean")]
            [Summary("Downloads and removes X messages from the current channel.")]

            public async Task PurgeAsync(int amount)
            {

                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }
                var channel = Context.Guild as SocketGuild;
                var channelIdLogs = channel.GetTextChannel(767455535800385616).SendMessageAsync(Context.User.Username + " has used the clean command");

                var admin = Context.User as SocketGuildUser;



                var rolePermissionAdmin = (admin as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
                var currentChannel = Context.Channel.Id;
                ulong botChannel = 774501395566297138;
                ulong adminBotChannel = 757377449016295527;

                if (currentChannel == botChannel || currentChannel == adminBotChannel)
                {
                    if (admin.Roles.Contains(rolePermissionAdmin))
                    {
                        // Check if the amount provided by the user is positive.
                        if (amount <= 0)
                        {
                            await ReplyAsync("The amount of messages to remove must be positive.");
                            return;
                        }

                        // Download X messages starting from Context.Message, which means
                        // that it won't delete the message used to invoke this command.
                        var messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, amount).FlattenAsync();

                        // Note:
                        // FlattenAsync() might show up as a compiler error, because it's
                        // named differently on stable and nightly versions of Discord.Net.
                        // - Discord.Net 1.x: Flatten()
                        // - Discord.Net 2.x: FlattenAsync()

                        // Ensure that the messages aren't older than 14 days,
                        // because trying to bulk delete messages older than that
                        // will result in a bad request.
                        var filteredMessages = messages.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14);

                        // Get the total amount of messages.
                        var count = filteredMessages.Count();

                        // Check if there are any messages to delete.
                        if (count == 0)
                            await ReplyAsync("Nothing to delete.");

                        else
                        {
                            // The cast here isn't needed if you're using Discord.Net 1.x,
                            // but I'd recommend leaving it as it's what's required on 2.x, so
                            // if you decide to update you won't have to change this line.
                            await (Context.Channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
                            await ReplyAndDeleteAsync($"Removed {count} {(count > 1 ? "messages" : "message")}.");
                        }
                    }
                    else
                    {
                        await ReplyAndDeleteAsync("You are not an admin");
                    }
                }
                else
                {
                    await ReplyAndDeleteAsync("This is not admin bot commands or bot commands channel");
                }
            }
            [Command("msg")]
            [Summary("!msg @role messagecontents (lets you message that role. Only use on roles with not a lot of people)")]
            public async Task Msg(IRole role, [Remainder] string message)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }
                //logging purposes
                var channel = Context.Guild as SocketGuild;
                var channelIdLogs = channel.GetTextChannel(767455535800385616).SendMessageAsync(Context.User.Username + " has used the msg command for " + role);

                var admin = Context.User as SocketGuildUser;
                var roleName = role.ToString();
                Console.WriteLine(roleName);
                //var listOfUsers = (role as IChannel).GetUsersAsync(default, default);
                var listOfUsers = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName).Members;


                Console.WriteLine("list of users " + listOfUsers);

                var rolePermissionAdmin = (admin as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
                if (admin.Roles.Contains(rolePermissionAdmin))
                {
                    Console.Write("is admin");

                    foreach (var user in listOfUsers)
                    {

                        await Task.Delay(3000);
                        Console.WriteLine("in for each" + user);
                        try
                        {
                            var channelb = await user.GetOrCreateDMChannelAsync();

                            await channelb.SendMessageAsync($"Hello, {user.Username}! " + message);

                        }

                        catch (Discord.Net.HttpException ex) when (ex.HttpCode == HttpStatusCode.Forbidden)
                        {
                            Console.WriteLine($"Boo, I cannot message {user}.");
                            await ReplyAsync($"I cannot message {user}");
                            continue;
                        }
                    }
                }
            }

            [Command("msguser")]
            [Summary("!msguser @user (Lets you msg user)")]
            public async Task MsgUser(IUser user, [Remainder] string message)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }
                //logging purposes
                var channel = Context.Guild as SocketGuild;
                var channelIdLogs = channel.GetTextChannel(767455535800385616).SendMessageAsync(Context.User.Username + " has used the msg command to " + user);

                var admin = Context.User as SocketGuildUser;

                Console.Write("before checking admin");


                var rolePermissionAdmin = (admin as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
                if (admin.Roles.Contains(rolePermissionAdmin))
                {
                    Console.Write("is admin");


                    try
                    {
                        var channelb = await user.GetOrCreateDMChannelAsync();

                        await channelb.SendMessageAsync($"Hello, {user.Username}! " + message);
                        await Task.Delay(3000);
                    }

                    catch (Discord.Net.HttpException ex) when (ex.HttpCode == HttpStatusCode.Forbidden)
                    {
                        Console.WriteLine($"Boo, I cannot message {user}.");
                        await ReplyAsync($"I cannot message {user}");

                    }


                }
            }
            [Command("result", RunMode = RunMode.Async)]
            [Summary("!result @winningteam @losingteam 2-0 pic pic pic (lets you input results)")]
            public async Task Result(IRole team, IRole opponentTeam, string result, params string[] links)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }
                MongoDB mongo = new MongoDB();
                Team mongoTeam = new Team();
                Matches mongoMatch = new Matches();


                var user = Context.User as SocketGuildUser;
                var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");

                string[] linksFull = new string[3];
                linksFull[0] = links[0];
                linksFull[1] = links[1];

                var channel = Context.Guild as SocketGuild;
                var channelIdLogs = channel.GetTextChannel(767455535800385616).SendMessageAsync(Context.User.Username + " has used result command ");

                await ReplyAsync("Just to verify " + team + " beat " + opponentTeam + " by " + result + "? Please reply with 'yes' or 'no'");
                var response = await NextMessageAsync();
                Console.WriteLine(response);
                if (response.ToString().ToLower() == "yes")
                {
                    Console.WriteLine("yes if");
                }
                else
                {
                    Console.WriteLine("no if");
                    return;
                }

                //add blank string if less than 3 links
                if (links.Length < 3)
                {
                    linksFull[2] = "";
                }
                else
                {
                    linksFull[2] = links[2];
                }
                if (user.Roles.Contains(rolePermissionAdmin))
                {
                    //these are ran opposite for both teams, i.e. times to run win is added to loss for second team and times to run lose is added to the second team's win.
                    int timesToRunWin = 0;
                    int timesToRunLose = 0;
                    var google = new googleSheet();
                    double[] teams = google.GetElo(team.ToString(), opponentTeam.ToString());
                    Console.WriteLine(teams);
                    switch (result)
                    {
                        case "2-0":
                            timesToRunWin = 2;

                            break;
                        case "2-1":
                            timesToRunWin = 2;
                            timesToRunLose = 1;
                            break;
                        case "1-2":
                            timesToRunWin = 1;
                            timesToRunLose = 2;
                            break;
                        case "0-2":
                            timesToRunLose = 2;
                            break;
                        default:
                            await ReplyAsync("Incorrect format or amount of games, can be 2-1, 2-0, 1-2, 0-2");
                            return;

                    }


                    if (teams != null)
                    {
                        Console.WriteLine("Made it into teams not null");
                        double firstElo = teams[0];
                        double secondElo = teams[1];




                        double transformedFirstElo = Math.Pow(10, (firstElo / 400));
                        double transformedSecondElo = Math.Pow(10, (secondElo / 400));

                        double formulaFirstElo = transformedFirstElo / (transformedFirstElo + transformedSecondElo);
                        double formulaSecondElo = transformedSecondElo / (transformedFirstElo + transformedSecondElo);


                        int howBigOfEloSwing = 40;




                        double firstEloCalculatedIfWin = firstElo + howBigOfEloSwing * (1 - formulaFirstElo);
                        double firstEloCalculatedIfLose = firstElo + howBigOfEloSwing * (0 - formulaFirstElo);
                        double secondEloCalculatedIfWin = secondElo + howBigOfEloSwing * (1 - formulaSecondElo);
                        double secondEloCalculatedIfLose = secondElo + howBigOfEloSwing * (0 - formulaSecondElo);

                        //get information of elo going up and down for both teams
                        firstEloCalculatedIfWin = Math.Round(firstEloCalculatedIfWin - firstElo, 1);
                        firstEloCalculatedIfLose = Math.Round(firstEloCalculatedIfLose - firstElo, 1);
                        secondEloCalculatedIfWin = Math.Round(secondEloCalculatedIfWin - secondElo, 1);
                        secondEloCalculatedIfLose = Math.Round(secondEloCalculatedIfLose - secondElo, 1);
                        //calculatefinished match for first called team
                        double finishedEloFirstTeam = Math.Round((firstElo + firstEloCalculatedIfWin * timesToRunWin + firstEloCalculatedIfLose * timesToRunLose), 1);
                        //calculatedfinished match for second called team
                        double finishedEloSecondTeam = Math.Round((secondElo + secondEloCalculatedIfWin * timesToRunLose + secondEloCalculatedIfLose * timesToRunWin), 1);

                        double firstTeamChanged = Math.Round((firstEloCalculatedIfWin * timesToRunWin + firstEloCalculatedIfLose * timesToRunLose), 1);
                        double secondTeamChanged = Math.Round((secondEloCalculatedIfWin * timesToRunLose + secondEloCalculatedIfLose * timesToRunWin), 1);



                        var resultOfFirstTeam = timesToRunWin + "-" + timesToRunLose;
                        var resultOfSecondTeam = timesToRunLose + "-" + timesToRunWin;


                        Console.WriteLine("Run win command " + timesToRunWin + " and run lose command " + timesToRunLose);

                        int positionOfYourTeam = 0;
                        int positionOfOpposingTeam = 0;
                        int howManyInListYourTeam = 0;
                        int howManyInListOpposingTeam = 0;
                        Console.Write("is admin");
                        try
                        {
                            string locationOfInput = "confirmedmatch";
                            //makes sure that there is a pending scheduled match
                            string[] confirmYourTeamChallenge = google.Confirm(team.ToString(), opponentTeam.ToString(), locationOfInput, false);
                            string[] confirmOpposingTeamChallenge = google.Confirm(team.ToString(), opponentTeam.ToString(), locationOfInput, true);
                            if (confirmYourTeamChallenge[0] == opponentTeam.ToString())
                            {
                                Console.WriteLine("opposing team and confirm[0] are the same");
                                positionOfYourTeam = Int32.Parse(confirmYourTeamChallenge[1]);
                                positionOfOpposingTeam = Int32.Parse(confirmOpposingTeamChallenge[1]);
                                howManyInListYourTeam = Int32.Parse(confirmYourTeamChallenge[2]);
                                howManyInListOpposingTeam = Int32.Parse(confirmOpposingTeamChallenge[2]);
                            }
                            string convertedDate = google.GetMatchDate(team.ToString(), true, positionOfYourTeam).ToString();



                            //for first team

                            google.MatchResult(team.ToString(), opponentTeam.ToString(), resultOfFirstTeam.ToString(), convertedDate.ToString(), linksFull, firstTeamChanged);
                            google.InputElo(team.ToString(), finishedEloFirstTeam);
                            //for second team
                            google.MatchResult(opponentTeam.ToString(), team.ToString(), resultOfSecondTeam.ToString(), convertedDate.ToString(), linksFull, secondTeamChanged);
                            google.InputElo(opponentTeam.ToString(), finishedEloSecondTeam);

                            //adding match details
                            await mongo.InputElo(team.ToString(), finishedEloFirstTeam);
                            await mongo.InputElo(opponentTeam.ToString(), finishedEloSecondTeam);


                            await mongo.AddResults(team.ToString(), timesToRunWin, timesToRunLose);
                            await mongo.AddResults(opponentTeam.ToString(), timesToRunLose, timesToRunWin);

                            await mongo.changeMatchToResult(team.ToString(), opponentTeam.ToString());

                            google.RemoveSchedule(team.ToString(), positionOfYourTeam, howManyInListYourTeam, opponentTeam.ToString(), positionOfOpposingTeam, howManyInListOpposingTeam, locationOfInput);
                            await ReplyAsync(team.ToString() + " elo changes by " + firstTeamChanged + ". " + opponentTeam.ToString() + " elo changed by " + secondTeamChanged);
                        }
                        catch
                        {
                            await ReplyAsync("Something went wrong, please contact the admins");
                        }
                    }

                }
                else
                {
                    await ReplyAsync("You are not an admin.");
                }
            }



            [Command("createteam", RunMode = RunMode.Async)]
            //can be used by admin/owner to create new team on google sheet api
            [Summary
          ("!createteam @teamcaptain teamname (creates a team)")]
            public async Task CreateTeam(IGuildUser calledUser, [Remainder] string teamNameBeingCreated)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }
                var channel = Context.Guild as SocketGuild;
                var channelIdLogs = channel.GetTextChannel(767455535800385616).SendMessageAsync(Context.User.Username + " has used the createteam command for " + teamNameBeingCreated);

                var user = Context.User as SocketGuildUser;
                var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
                if (user.Roles.Contains(rolePermissionAdmin))
                {

                    var rolePermissionOwner = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Owner");

                    //captainroleid
                    // style server 609893180019834880
                    //copy of style server 705651654082560003
                    ulong roleId = 609893180019834880;
                    var captainRole = Context.Guild.GetRole(roleId);
                    //botid


                    //checks if role admin or owner
                    if ((user.Roles.Contains(rolePermissionAdmin)) || (user.Roles.Contains(rolePermissionOwner)))
                    {
                        try
                        {
                            await ReplyAsync(" Is this an org? Enter yes or no.");
                            var askIfOrg = await NextMessageAsync();
                            if (askIfOrg != null)
                            {

                                if (askIfOrg.ToString().ToLower() == "yes" || askIfOrg.ToString().ToLower() == "no")
                                {
                                    if (askIfOrg.ToString().ToLower() == "yes")
                                    {
                                        await ReplyAsync("Okay, what is the orgs name?");
                                        var nameOfOrg = await NextMessageAsync();
                                        if (nameOfOrg != null)
                                        {
                                            await ReplyAsync("Do you want to create a googlesheet for this team? Enter yes or no.");
                                            var response = await NextMessageAsync();
                                            if (response != null)
                                            {
                                                if (response.ToString().ToLower() == "yes" || response.ToString().ToLower() == "no")
                                                {
                                                    await ReplyAsync($"Team {teamNameBeingCreated} created, added {teamNameBeingCreated} and Team Captain role to {calledUser}");

                                                    var permissions = new GuildPermissions(104324673);

                                                    var addPermissions = new OverwritePermissions(70770240, 0);
                                                    //org category id
                                                    ulong assignChannelIdOrgText = 774102389341814794;

                                                    //full channel team text
                                                    // ulong assignChannelIdTeamText = 622919737365495849;
                                                    //old id 774104289919500298

                                                    ulong assignChannelIdTeamText = 776610278204506162;


                                                    //full voice channel
                                                    //ulong assignChannelIdTeamVoice = 622919840444841984;

                                                    //old id 774104754938052609
                                                    ulong assignChannelIdTeamVoice = 776611013751078954;

                                                    //create new channels and assign them under categories
                                                    Console.Write("before channel create");
                                                    var createTextChannel = await Context.Guild.CreateTextChannelAsync(teamNameBeingCreated, channel => channel.CategoryId = assignChannelIdTeamText, default);
                                                    Console.WriteLine("between text and voice");
                                                    var createVoiceChannel = await Context.Guild.CreateVoiceChannelAsync(teamNameBeingCreated, channel => channel.CategoryId = assignChannelIdTeamVoice, default);
                                                    Console.WriteLine("before role create");
                                                    var createdRole = await Context.Guild.CreateRoleAsync(teamNameBeingCreated, permissions, default, true, true, default);

                                                    //var everyoneRole = Context.Guild.GetRole(705651653323522121);
                                                    //var everyoneRestRole = everyoneRole as RestRole;
                                                    Console.WriteLine("before addpermissions text");


                                                    //everyone role
                                                    var everyone = Context.Guild.GetRole(601677722577797120);

                                                    await createTextChannel.AddPermissionOverwriteAsync(createdRole, addPermissions);
                                                    // await createTextChannel.AddPermissionOverwriteAsync(everyone, zeroPerms);
                                                    Console.WriteLine("before addpermissions voice");
                                                    await createVoiceChannel.AddPermissionOverwriteAsync(createdRole, addPermissions);
                                                    // await createVoiceChannel.AddPermissionOverwriteAsync(everyone, zeroPermsVoice);
                                                    Console.WriteLine("before apply permissions to everyone");


                                                    //asks if org and creates seperate text channel



                                                    if (askIfOrg.ToString().ToLower() == "yes")
                                                    {

                                                        var createOrgTextChannel = await Context.Guild.CreateTextChannelAsync(nameOfOrg.ToString(), channel => channel.CategoryId = assignChannelIdOrgText, default);
                                                        await createOrgTextChannel.AddPermissionOverwriteAsync(createdRole, addPermissions);
                                                    }
                                                    else if (askIfOrg.ToString().ToLower() == "no")
                                                    {
                                                        await ReplyAsync("Okay, this is not an org.");
                                                    }






                                                    await calledUser.AddRoleAsync(createdRole);
                                                    await calledUser.AddRoleAsync(captainRole);
                                                    Console.WriteLine("after addpermissions");



                                                    if (response.ToString().ToLower() == "yes")
                                                    {
                                                        List<string> captain = new List<string>();
                                                        captain.Add(calledUser.ToString());
                                                        var google = new googleSheet();
                                                        var run = google.CreateTeam(teamNameBeingCreated);
                                                        //creates team in database as well
                                                        MongoDB mongo = new MongoDB();
                                                        Team team = new Team();
                                                        team.TeamName = teamNameBeingCreated;
                                                        team.TeamTextChannel = createTextChannel.Id.ToString();
                                                        team.TeamVoiceChannel = createVoiceChannel.Id.ToString();
                                                        team.Captain = captain;

                                                        await mongo.inputTeam(team);

                                                        await ReplyAsync("Google sheet created and team inserted into database.");

                                                        var adminHelpChannel = channel.GetTextChannel(777718412470255626);
                                                        await createTextChannel.SendMessageAsync("Hello " + calledUser.Mention + " this is your new text channel for your team " + createdRole.Mention + ". \n To start adding players to your team, use the !add " +
                                                            createdRole.Mention + " top/jg/mid/adc/sup/sub @userofthat role" + "\n For example !add " + createdRole.Mention + " sub " + calledUser.Mention + ". After you have filled out your team please let an admin know so we can get you added to the ranking, also if you haven't paid please reach out to sky dancer to submit your payment. If you have any questions feel free to message " + adminHelpChannel.Mention);

                                                    }
                                                    else if (response.ToString().ToLower() == "no")
                                                    {
                                                        await ReplyAsync("No google sheet created.");
                                                    }

                                                    //   var google = new googleSheet();
                                                    // IList<IList<Object>> values = google.Google();
                                                }
                                                else
                                                {
                                                    await ReplyAsync("Please respond with yes or no, retry command"); return;
                                                }

                                            }
                                            else
                                            {
                                                await ReplyAsync("Please respond with yes or no in 10 seconds, retry command");
                                                return;


                                            }
                                        }

                                        else
                                        {
                                            await ReplyAsync("Please name the org, retry command"); return;
                                        }
                                    }
                                    else
                                    {
                                        await ReplyAsync("Do you want to create a googlesheet for this team? Enter yes or no.");
                                        var response = await NextMessageAsync();
                                        if (response != null)
                                        {
                                            if (response.ToString().ToLower() == "yes" || response.ToString().ToLower() == "no")
                                            {
                                                await ReplyAsync($"Team {teamNameBeingCreated} created, added {teamNameBeingCreated} and Team Captain role to {calledUser}");

                                                var permissions = new GuildPermissions(104324673);

                                                var addPermissions = new OverwritePermissions(70770240, 0);


                                                //full channel team text
                                                // ulong assignChannelIdTeamText = 622919737365495849;
                                                //old id 774104289919500298

                                                ulong assignChannelIdTeamText = 776610278204506162;


                                                //full voice channel
                                                //ulong assignChannelIdTeamVoice = 622919840444841984;

                                                //old id 774104754938052609
                                                ulong assignChannelIdTeamVoice = 776611013751078954;

                                                //create new channels and assign them under categories
                                                var createTextChannel = await Context.Guild.CreateTextChannelAsync(teamNameBeingCreated, channel => channel.CategoryId = assignChannelIdTeamText, default);
                                                var createVoiceChannel = await Context.Guild.CreateVoiceChannelAsync(teamNameBeingCreated, channel => channel.CategoryId = assignChannelIdTeamVoice, default);

                                                var createdRole = await Context.Guild.CreateRoleAsync(teamNameBeingCreated, permissions, default, true, true, default);
                                                //var everyoneRole = Context.Guild.GetRole(705651653323522121);
                                                //var everyoneRestRole = everyoneRole as RestRole;
                                                Console.WriteLine("before addpermissions text");


                                                //everyone role
                                                var everyone = Context.Guild.GetRole(601677722577797120);

                                                await createTextChannel.AddPermissionOverwriteAsync(createdRole, addPermissions);
                                                // await createTextChannel.AddPermissionOverwriteAsync(everyone, zeroPerms);
                                                Console.WriteLine("before addpermissions voice");
                                                await createVoiceChannel.AddPermissionOverwriteAsync(createdRole, addPermissions);
                                                // await createVoiceChannel.AddPermissionOverwriteAsync(everyone, zeroPermsVoice);
                                                Console.WriteLine("before apply permissions to everyone");



                                                //need this outside of method for some reason








                                                await calledUser.AddRoleAsync(createdRole);
                                                await calledUser.AddRoleAsync(captainRole);
                                                Console.WriteLine("after addpermissions");



                                                if (response.ToString().ToLower() == "yes")
                                                {
                                                    List<string> captain = new List<string>();
                                                    captain.Add(calledUser.ToString());
                                                    var google = new googleSheet();
                                                    var run = google.CreateTeam(teamNameBeingCreated);
                                                    //creates team in database as well
                                                    MongoDB mongo = new MongoDB();
                                                    Team team = new Team();
                                                    team.TeamName = teamNameBeingCreated;
                                                    team.TeamTextChannel = createTextChannel.Id.ToString();
                                                    team.TeamVoiceChannel = createVoiceChannel.Id.ToString();
                                                    team.Captain = captain;


                                                    await mongo.inputTeam(team);
                                                    await ReplyAsync("Google sheet created and team inserted into database.");

                                                    var adminHelpChannel = channel.GetTextChannel(777718412470255626);
                                                    await createTextChannel.SendMessageAsync("Hello " + calledUser.Mention + " this is your new text channel for your team " + createdRole.Mention + ". \n To start adding players to your team, use the !add " +
                                                            createdRole.Mention + " top/jg/mid/adc/sup/sub @userofthat role" + "\n For example !add " + createdRole.Mention + " sub " + calledUser.Mention + ". After you have filled out your team please let an admin know so we can get you added to the ranking, also if you haven't paid please reach out to sky dancer to submit your payment. If you have any questions feel free to message " + adminHelpChannel.Mention);
                                                }
                                                else if (response.ToString().ToLower() == "no")
                                                {
                                                    await ReplyAsync("No google sheet created.");
                                                }

                                                //   var google = new googleSheet();
                                                // IList<IList<Object>> values = google.Google();
                                            }
                                            else
                                            {
                                                await ReplyAsync("Please respond with yes or no, retry command"); return;
                                            }

                                        }
                                        else
                                        {
                                            await ReplyAsync("Please respond with yes or no in 10 seconds, retry command");
                                            return;


                                        }
                                    }
                                }
                                else
                                {
                                    await ReplyAsync("Please respond within 10 seconds, retry command"); return;
                                }

                            }
                            else
                            {
                                await ReplyAsync("Please respond within 10 seconds, retry command"); return;
                            }
                        }
                        catch
                        {
                            await ReplyAsync("Error, please use format !createteam @teamcaptainofteam nameofteam ");
                        }

                    }
                    else
                    {
                        await ReplyAsync("You are not owner/admin");
                    }


                }

            }
            [Command("deleteteam", RunMode = RunMode.Async)]
            [Summary("!deleteteam @teamname (deletes team from google sheet, removes them from ranklist, deletes text and voice channels)")]
            public async Task DeleteTeam(IRole team)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }
                var google = new googleSheet();
                var mongo = new mongo();
                var user = Context.User as SocketGuildUser;
                var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
                if (user.Roles.Contains(rolePermissionAdmin))
                {
                    await ReplyAsync("Are you sure you want to delete " + team.Name + "? 'yes' or 'no'");

                    var confirmDelete = await NextMessageAsync();

                    if (confirmDelete != null)
                    {
                        if (confirmDelete.ToString().ToLower() == "yes")
                        {
                            try
                            {
                                await google.deleteTeam(team.Name);

                                string[] channelIds = await mongo.getChannelIds(team.Name);

                                var channelText = Context.Guild.GetTextChannel(ulong.Parse(channelIds[0]));
                                var channelVoice = Context.Guild.GetVoiceChannel(ulong.Parse(channelIds[1]));

                                await channelText.DeleteAsync();
                                await channelVoice.DeleteAsync();
                                await team.DeleteAsync();
                                await ReplyAsync("Deleted team");

                            }
                            catch { await ReplyAsync("Something went wrong, potential text or voice channel not found"); }
                        }
                        else
                        {
                            await ReplyAndDeleteAsync("Did not delete role, as you didn't say 'yes'");
                        }
                    }
                    else
                    {
                        await ReplyAndDeleteAsync("Did not delete role, as you didn't answer");
                        return;
                    }
                }
                else
                {
                    await ReplyAndDeleteAsync("You do not have the admin role.");
                    return;
                }
            }

            [Command("elo")]
            [Summary("!elo @teamname (resets elo and bases it off of top 5 players)")]
            public async Task CalculateElo(IRole team)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }
                var google = new googleSheet();
                var user = Context.User as SocketGuildUser;
                var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
                if (user.Roles.Contains(rolePermissionAdmin))
                {
                    List<int> topfive = new List<int>();
                    string[] values = google.CalculateElo(team.ToString());
                    foreach (var row in values)
                    {
                        switch (row.ToString())
                        {
                            case "CHALLENGER I":
                                topfive.Add(1);
                                break;
                            case "GRANDMASTER I":
                                topfive.Add(2);
                                break;
                            case "MASTER I":
                                topfive.Add(3);
                                break;
                            case "DIAMOND I":
                                topfive.Add(4);
                                break;
                            case "DIAMOND II":
                                topfive.Add(5);
                                break;
                            case "DIAMOND III":
                                topfive.Add(6);
                                break;
                            case "DIAMOND IV":
                                topfive.Add(7);
                                break;
                            case "PLATINUM I":
                                topfive.Add(8);
                                break;
                            case "PLATINUM II":
                                topfive.Add(9);
                                break;
                            case "PLATINUM III":
                                topfive.Add(10);
                                break;
                            case "PLATINUM IV":
                                topfive.Add(11);
                                break;
                            case "GOLD I":
                                topfive.Add(12);
                                break;
                            case "GOLD II":
                                topfive.Add(13);
                                break;
                            case "GOLD III":
                                topfive.Add(14);
                                break;
                            case "GOLD IV":
                                topfive.Add(15);
                                break;
                            case "SILVER I":
                                topfive.Add(16);
                                break;
                            case "SILVER II":
                                topfive.Add(17);
                                break;
                            case "SILVER III":
                                topfive.Add(18);
                                break;
                            case "SILVER IV":
                                topfive.Add(19);
                                break;
                            case "BRONZE I":
                                topfive.Add(20);
                                break;
                            case "BRONZE II":
                                topfive.Add(21);
                                break;
                            case "BRONZE III":
                                topfive.Add(22);
                                break;
                            case "BRONZE IV":
                                topfive.Add(23);
                                break;
                            case "IRON I":
                                topfive.Add(24);
                                break;
                            case "IRON II":
                                topfive.Add(25);
                                break;
                            case "IRON III":
                                topfive.Add(26);
                                break;
                            case "IRON IV":
                                topfive.Add(27);
                                break;
                            default:
                                Console.WriteLine("Nothing");
                                break;
                        }
                    }
                    Console.WriteLine("made it here" + topfive.ToString());
                    var sortedList = topfive.OrderBy(x => x).ToList();
                    Console.WriteLine(sortedList.ToString());
                    var totalElo = 0;
                    for (int i = 0; i < 5; i++)
                    {

                        var checkMyElo = sortedList[i];
                        switch (checkMyElo)
                        {
                            case 1:
                                totalElo = totalElo + 600;
                                break;
                            case 2:
                                totalElo = totalElo + 550;
                                break;
                            case 3:
                                totalElo = totalElo + 500;
                                break;
                            case 4:
                                totalElo = totalElo + 475;
                                break;
                            case 5:
                                totalElo = totalElo + 450;
                                Console.WriteLine("made it here");
                                break;
                            case 6:
                                totalElo = totalElo + 425;
                                break;
                            case 7:
                                totalElo = totalElo + 390;
                                break;
                            case 8:
                                totalElo = totalElo + 380;
                                break;
                            case 9:
                                totalElo = totalElo + 360;
                                break;
                            case 10:
                                totalElo = totalElo + 340;
                                break;
                            case 11:
                                totalElo = totalElo + 320;
                                break;
                            case 12:
                                totalElo = totalElo + 310;
                                break;
                            case 13:
                                totalElo = totalElo + 290;
                                break;
                            case 14:
                                totalElo = totalElo + 280;
                                break;
                            case 15:
                                totalElo = totalElo + 270;
                                break;
                            case 16:
                                totalElo = totalElo + 260;
                                break;
                            case 17:
                                totalElo = totalElo + 250;
                                break;
                            case 18:
                                totalElo = totalElo + 240;
                                break;
                            case 19:
                                totalElo = totalElo + 230;
                                break;
                            case 20:
                                totalElo = totalElo + 220;
                                break;
                            case 21:
                                totalElo = totalElo + 210;
                                break;
                            case 22:
                                totalElo = totalElo + 200;
                                break;
                            case 23:
                                totalElo = totalElo + 190;
                                break;
                            case 24:
                                totalElo = totalElo + 180;
                                break;
                            case 25:
                                totalElo = totalElo + 170;
                                break;
                            case 26:
                                totalElo = totalElo + 160;
                                break;
                            case 27:
                                totalElo = totalElo + 150;
                                break;
                            default:
                                Console.WriteLine("Nothing");
                                break;
                        }
                    }

                    google.InputElo(team.ToString(), totalElo);
                    await ReplyAsync(team + " has the starting elo of " + totalElo + "\n Added to googlesheet");
                    MongoDB mongo = new MongoDB();
                    bool working = await mongo.InputElo(team.ToString(), totalElo);
                    if (working)
                    {
                        Console.WriteLine("added elo");

                    }
                    else
                    {
                        await ReplyAsync("did not add to database correctly, make sure this team is in database.");
                    }

                }

            }

            [Command("checkpay", RunMode = RunMode.Async)]
            [Summary("!checkpay @team (checks if team has paid)")]
            //checks if team paid
            public async Task checkPaid(IRole role)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }
                var mongo = new MongoDB();
                var user = Context.User as SocketGuildUser;
                var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
                if (user.Roles.Contains(rolePermissionAdmin))
                {


                    bool paidSuccess = await mongo.checkPaid(role.Name.ToString());

                    if (paidSuccess)
                    {
                        await ReplyAsync("This team has paid.");
                    }
                    else
                    {
                        await ReplyAsync("Team hasn't paid, or doesn't exist.");
                    }
                }

            }
            [Command("paid", RunMode = RunMode.Async)]
            [Summary("!paid @team (marks team as paid in database)")]
            //changes team status to paid
            public async Task paid(IRole role)
            {
                if (work == false)
                {
                    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                    return;
                }
                var mongo = new MongoDB();
                var user = Context.User as SocketGuildUser;
                var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
                if (user.Roles.Contains(rolePermissionAdmin))
                {
                    await ReplyAsync("Has " + role.Name + " team paid? 'yes' or 'no' ");
                    var yesThisTeam = await NextMessageAsync();
                    if (yesThisTeam.ToString().ToLower() == "yes")
                    {
                        bool paidSuccess = await mongo.paid(role.Name.ToString());

                        if (paidSuccess)
                        {
                            await ReplyAsync("Successfully changed team status to paid");
                        }
                        else
                        {
                            await ReplyAsync("No team found with that name.");
                        }
                    }
                }
            }
            [Command("rename", RunMode = RunMode.Async)]
            [Summary("!rename @team newname (lets you rename a team")]
            public async Task rename(IRole role, [Remainder] string newName)
            {
                //if (work == false)
                //{
                //    await ReplyAsync("Innti is working and doesn't want to have something mess up if this bot is on while he's developing. If you need something please reach out to him so he can turn normal bot functionality on.");
                //    return;
                //}
                if (newName == null)
                {
                    await ReplyAsync("new name not typed");
                    return;
                }
                var mongo = new MongoDB();
                var google = new googleSheet();
                var user = Context.User as SocketGuildUser;
                var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
                if (user.Roles.Contains(rolePermissionAdmin))
                {
                    await ReplyAsync("Are you sure you want to rename " + role.Name + " to " + newName + "? 'yes' or 'no'");
                    var response = await NextMessageAsync();

                    if (response == null)
                    {
                        await ReplyAsync("No answer");
                        return;
                    }
                    if (response.ToString().ToLower() == "yes")
                    {
                        await google.renameTeam(role.Name, newName);

                        await mongo.renameTeam(role.Name, newName);
                        await google.TeamName(newName);
                        try
                        {
                            string[] channelIds = await mongo.getChannelIds(role.Name);
                            var channelText = Context.Guild.GetTextChannel(ulong.Parse(channelIds[0])) as IGuildChannel;
                            var channelVoice = Context.Guild.GetVoiceChannel(ulong.Parse(channelIds[1])) as IGuildChannel;
                            await channelText.ModifyAsync(x => { x.Name = newName; });
                            await channelVoice.ModifyAsync(x => { x.Name = newName; });
                        }
                        catch
                        {
                            Console.WriteLine("No channels to modify");
                        }
                        await role.ModifyAsync(x => { x.Name = newName; });
                        await ReplyAsync("Changed teamname ");
                    }
                    else if (response.ToString().ToLower() == "no")
                    {
                        await ReplyAsync("Okay, nothing changed");
                        return;
                    }


                }
            }
            ////[Command("temp", RunMode = RunMode.Async)]

            //////changes team status to paid
            ////public async Task temp()
            ////{

            ////    var mongo = new MongoDB();
            ////    var user = Context.User as SocketGuildUser;
            ////    var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
            ////    if (user.Roles.Contains(rolePermissionAdmin))
            ////    {
            ////        await mongo.temp();
            ////    }
            ////}
        }

    }
}




