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
using mongo = DiscordBotApp.MongoDB;



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



    public class MsgModule : ModuleBase<SocketCommandContext>
    {

        [Command("player")]
        public async Task Player()
        {
            var mongo = new mongo();
            
            await ReplyAsync(mongo.player());
           
        }


        [Command("team")]
        public async Task Team(IRole teamName)
        {
            var mongo = new mongo();

            BsonDocument teamData = mongo.team(teamName.ToString());

            int numCount = 5;

            string[] numberOfPlayers = new string[teamData["Subs"].AsBsonArray.Count() + 5];

            numberOfPlayers[0] = teamData["Top"].ToString();
            numberOfPlayers[1] = teamData["Jg"].ToString();
            numberOfPlayers[2] = teamData["Mid"].ToString();
            numberOfPlayers[3] = teamData["Adc"].ToString();
            numberOfPlayers[4] = teamData["Sup"].ToString();
            foreach(var sub in teamData["Subs"].AsBsonArray)
            {
                numberOfPlayers[numCount] = sub.ToString();
                numCount++;
            }

            

            string[] teamPlayers = mongo.playerLookup(numberOfPlayers);
            Console.WriteLine(teamPlayers[0].ToString());
            string fullData = ("Name: " + teamData["TeamName"].ToString() + "\n" + "Elo: "+ teamData["Elo"].ToString() + "\n" + "Season Record: "+ teamData["WinsThisSeason"].ToString() + "/" + teamData["LossesThisSeason"] + "\n" +"Top: " + teamPlayers[0] + "\n" + "Jungle: " + teamPlayers[1] + "\n" + "Mid: " + teamPlayers[2] + "\n" + "Adc: " + teamPlayers[3] + "\n" + "Sup: " + teamPlayers[4] + "\n");

            if (teamPlayers.Length > 5)
            {
                numCount = 1;
                for (int i = 5; i < teamData["Subs"].AsBsonArray.Count() + 5; i++)
                {
                    fullData = fullData + "Sub #" + numCount + " " + teamPlayers[i] + "\n";
                    numCount++;
                }
            }
            await ReplyAsync(fullData);

            

        }


        [Command("validateteam")]
        public async Task Validate(IRole teamName)
        {
            var user = Context.User as SocketGuildUser;
            var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
            if (user.Roles.Contains(rolePermissionAdmin))
            {
                var google = new googleSheet();
                google.TeamName(teamName.ToString());
                google.Validate(teamName.ToString());
                await ReplyAsync(teamName.ToString() + " added to ranking");
            }

        }
        [Command("challenge")]
        public async Task Challenge(IRole yourteam, IRole opposingTeam)
        {
            try
            {
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


                    var scheduleSheet = google.Schedule(yourteam.ToString(), opposingTeam.ToString(), convertedDate, locationOfInput);
                    if (scheduleSheet == null)
                    {
                        //checks if match is opitional
                        var optional = google.Optional(yourteam.ToString(), opposingTeam.ToString());
                        foreach (var user in listOfUsers)
                        {
                            
                            var isCaptainOfEnemy = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Team Captain");
                            if (user.Roles.Contains(isCaptainOfEnemy))
                            {
                                listOfCaptains = listOfCaptains + user.Mention+ " ";
                                
                                Console.WriteLine(optional);
                                var channelb = await user.GetOrCreateDMChannelAsync();
                                if (optional)
                                {

                                    
                                    await channelb.SendMessageAsync($"Hello, {user.Username}! " + Context.User.ToString() + " of " + yourteam.ToString() + " has challenged you, this request is nonoptional. Please talk to " + userUsingCommand.Username + " to select a time for the match. After picking out a date for the match" +
                                     " please use the !schedule command with the format of !schedule @" + opposingTeam.ToString() + " @" + yourteam.ToString() + " mm / dd / yy HH: MM AM / PM timezone(est / edt, cst / cdt, pst / pdt, mst / mdt)" );
                                }
                                else
                                {
                                    await channelb.SendMessageAsync($"Hello, {user.Username}! " + Context.User.ToString() + " of " + yourteam.ToString() + " has challenged you, this challenge is optional. If you would like you can !denychallenge @" + opposingTeam.ToString() + " @" + yourteam.ToString() + " Please talk to " + userUsingCommand.Username + "to select a time for the match if you chooose to accept. After picking out a date for the match" +
                                     " please use the !schedule command with the format of !schedule @" + opposingTeam.ToString() + " @" + yourteam.ToString() + " mm / dd / yy HH: MM AM / PM timezone(est / edt, cst / cdt, pst / pdt, mst / mdt)" );


                                }




                            }


                        }
                        if (optional)
                        {

                            await ReplyAsync("A NONOPTIONAL challenge has been issued to " + opposingTeam.ToString() + " and was sent to " + listOfCaptains + "After picking out a date for the match with the other captain" +
                                     " please use the !schedule command with the format of !schedule @" + yourteam.ToString() + " @" + opposingTeam.ToString() + " mm / dd / yy HH: MM AM / PM timezone(est / edt, cst / cdt, pst / pdt, mst / mdt)");
                        }
                        else
                        {
                            await ReplyAsync("An OPTIONAL challenge has been issued to " + opposingTeam.ToString() + " and was sent to " + listOfCaptains + "After picking out a date for the match with the other captain" +
                                     " please use the !schedule command with the format of !schedule @" + yourteam.ToString() + " @" + opposingTeam.ToString() + " mm / dd / yy HH: MM AM / PM timezone(est / edt, cst / cdt, pst / pdt, mst / mdt)");
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
        public async Task DenyChallenge(IRole yourteam, IRole opposingTeam)
        {
            
            string removeChallenge = "challenge";
            string typeOfRemove = "challenge";

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



                var optional = google.OptionalCheck(yourteam.ToString(),positionOfYourTeam, opposingTeam.ToString(), positionOfOpposingTeam);
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
        public async Task ConfirmSchedule(IRole yourteam, IRole opposingTeam)
        {

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
                        //gets converted matchdate of opponents
                        DateTime convertedDate = google.GetMatchDate(yourteam.ToString(), false, positionOfYourTeam);
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

                                    await channelb.SendMessageAsync($"Hello, {user.Username}! " + Context.User.Username + " of " + yourteam.ToString() + " confirmed your match on " + convertedDate + " est/edt.");
                                }

                            }
                            await ReplyAsync("Match has been confirmed for " + convertedDate + " est/edt");
                           var calendar = new googleCalendar();
                            var timeZone = "America/New_York";
                            calendar.AddMatch(yourteam.ToString(), opposingTeam.ToString(), convertedDate, timeZone);
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
                await ReplyAsync("You are not the team captain of the first team in the command " + yourteam.ToString() + " or an admin");
            }
        }


        [Command("schedule")]
        public async Task Schedule(IRole team1, IRole team2, params string[] time)
        {

            string locationOfInput = "pendingschedule";
            string removeChallenge = "challenge";
            string typeOfRemove = "challenge";
            DateTime scheduledDate = DateTime.Parse("12/30/2020");
            try
            {
                scheduledDate = DateTime.Parse(time[0] + " " + time[1] + " " + time[2]);
            }
            catch
            {
                await ReplyAsync("Format needs to be mm/ dd / yy HH:MM AM / PM timezone(est / edt, cst / cdt, pst / pdt, mst / mdt)");
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
                    //makes sure that there is a pending scheduled match
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
                    else {
                        await ReplyAsync("Team not in challenge list.");
                        return;
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


                var scheduleSheet = google.Schedule(team1.ToString(), team2.ToString(), convertedDate, locationOfInput);
                if (scheduleSheet == null)
                {
                    google.RemoveSchedule(team1.ToString(), positionOfYourTeam, howManyInListYourTeam, team2.ToString(), positionOfOpposingTeam, howManyInListOpposingTeam, typeOfRemove);
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
                await ReplyAsync("You are not the captain of the first team " + team1.ToString() + " or and admin.");
            }
        }
                

            
        
    
 


        [Command("sheet")]

        public async Task createSheet(IRole team)
        {

            var user = Context.User as SocketGuildUser;
            var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
            if (user.Roles.Contains(rolePermissionAdmin))
            {
                var google = new googleSheet();
                var run = google.CreateTeam(team.ToString());
                google.TeamName(team.ToString());
                await ReplyAsync("Google sheet created.");
            }
            
        }
        [Command("name")]

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
                    }*/
                }
            }

        }

        [Command("msg")]
        public async Task Msg(IRole role, [Remainder] string message)
        {

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
        public async Task MsgUser(IUser user, [Remainder] string message)
        {
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
    }

    public class Commands : InteractiveBase
    {
     


        [Command("teamroster")]

        public async Task Ping(IRole teamName)
        {

            //channel id of logs 767455535800385616
            //sending info to log
            var channel = Context.Guild as SocketGuild;
            var channelIdLogs = channel.GetTextChannel(767455535800385616).SendMessageAsync(Context.User.Username + " has used the teamroster command for " + teamName);


            var user = Context.User as SocketGuildUser;
            //var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
            //if (user.Roles.Contains(rolePermissionAdmin))
            {
                string[,] teamArray = new string[6, 3];
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
                        else if(num == 2)
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


                    await ReplyAsync(oneMessage + "\nhttps://na.op.gg/multi/query=" + HttpUtility.UrlEncode(stringOfIgns) + "\nIf an op.gg doesn't show up, please contact Admins.");
                    // await ReplyAsync("If there are not 5 IGNs, please contact Admins.");


                }
            }
            /*else
            {
                await ReplyAsync("You do not have permission to use this command");
            }*/
        }

        


        [Command("purge")]
        [Alias("clean")]
        [Summary("Downloads and removes X messages from the current channel.")]

        public async Task PurgeAsync(int amount)
        {
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


        [Command("userinfo")]
        /* [Summary
      ("Returns info about the current user, or the user parameter, if one passed.")]
         [Alias("user", "whois")]
         public async Task UserInfoAsync(
          [Summary("The (optional) user to get info from")]
         SocketUser user = null)
         {
             var userInfo = user ?? Context.Client.CurrentUser;
             await ReplyAsync($"@{userInfo.Username}{userInfo.Discriminator}");
         }
     
        public async Task GetAvatarAsync(IUser user, ITextChannel textChannel)
        {
            var userAvatarUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();
            await textChannel.SendMessageAsync(userAvatarUrl);
        }*/
        public async Task MessageUserAsync(IUser user)
        {
            var admin = Context.User as SocketGuildUser;
            var rolePermissionAdmin = (admin as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
            if (admin.Roles.Contains(rolePermissionAdmin))
            {
                var channel = await user.GetOrCreateDMChannelAsync();
                try
                {
                    if (user.Username == "Innti Health")
                    {
                        await channel.SendMessageAsync("fuck you bro");
                    }
                    await channel.SendMessageAsync($"Awesome stuff! {user.Username}");
                }
                catch (Discord.Net.HttpException ex) when (ex.HttpCode == HttpStatusCode.Forbidden)
                {
                    Console.WriteLine($"Boo, I cannot message {user}.");
                }
            }
        }
        [Command("match")]
        //creates a match, can be used by team captain
        public async Task CheckMatchDetails(string team1, string team2, string date, string time)
        {
            var user = Context.User as SocketGuildUser;
            var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
            if (user.Roles.Contains(rolePermissionAdmin))
            {

                var roleOfUser = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Team Captain");
                if (user.Roles.Contains(roleOfUser))
                {

                    await ReplyAsync($"Logged {team1} vs {team2} on {date} {time}");

                }
                else
                {
                    await ReplyAsync("You are not a captain");
                }


            }
        }
        [Command("imposter")]
        public async Task imposter(IGuildUser user)
        {
            await ReplyAsync(user.Username + " is acting kinda sus...");

        }
        /* [Command("updateroster")]
         // updates roster, can be used by team captain
         public async Task UpdateRoster(IUser user)
         {
             var user = Context.User as SocketGuildUser;
             var roleOfUser = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Team Captain");

         }*/

        [Command("result", RunMode = RunMode.Async)]
        public async Task Result(IRole team, IRole opponentTeam, string result, params string[]links)
        {

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
            if(response.ToString().ToLower() == "yes")
            {
                Console.WriteLine("yes if");
            }
            else
            {
                Console.WriteLine("no if");
                return;
            }

            //add blank string if less than 3 links
            if(links.Length < 3)
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
                    double finishedEloFirstTeam = Math.Round((firstElo + firstEloCalculatedIfWin * timesToRunWin + firstEloCalculatedIfLose * timesToRunLose),1);
                    //calculatedfinished match for second called team
                    double finishedEloSecondTeam = Math.Round((secondElo + secondEloCalculatedIfWin * timesToRunLose + secondEloCalculatedIfLose * timesToRunWin),1);

                    double firstTeamChanged = Math.Round((firstEloCalculatedIfWin * timesToRunWin + firstEloCalculatedIfLose * timesToRunLose),1);
                    double secondTeamChanged = Math.Round((secondEloCalculatedIfWin * timesToRunLose + secondEloCalculatedIfLose * timesToRunWin ),1);



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



                        google.RemoveSchedule(team.ToString(), positionOfYourTeam, howManyInListYourTeam, opponentTeam.ToString(), positionOfOpposingTeam, howManyInListOpposingTeam, locationOfInput);
                        await ReplyAsync(team.ToString() +"elo changes by " + firstTeamChanged + ". " + opponentTeam.ToString() + " elo changed by " + secondTeamChanged);
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
      ("Create team with no org")]
        public async Task CreateTeam(IGuildUser calledUser, [Remainder] string teamNameBeingCreated)
        {
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
                                                    var google = new googleSheet();
                                                    var run = google.CreateTeam(teamNameBeingCreated);
                                                    await ReplyAsync("Google sheet created.");
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
                                                var google = new googleSheet();
                                                var run = google.CreateTeam(teamNameBeingCreated);
                                                await ReplyAsync("Google sheet created.");
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
        [Command("checkign")]
        public async Task CheckIgn([Remainder]string ign)
        {
            string riotkey;
            riotkey = ConfigurationManager.AppSettings.Get("riotkey");

            //string of combine summoner rank
            string rankOfIgn = null;

            IRiotClient client = new RiotClient(new RiotClientSettings
            {
                ApiKey = riotkey
            });
            try
            {
                Summoner summoner = await client.GetSummonerBySummonerNameAsync(ign, PlatformId.NA1).ConfigureAwait(true);
                Console.WriteLine(summoner.Name);
                Console.WriteLine(summoner);
                if (summoner == null)
                {
                    await ReplyAsync("Unfortnately, there isn't a player by that name. Please try again");
                    return;
                }
                else if (summoner != null)
                {
                    Console.WriteLine("made it here fam");
                    Console.WriteLine(summoner.Id.ToString());
                    List<LeagueEntry> lists = await client.GetLeagueEntriesBySummonerIdAsync(summoner.Id.ToString(), PlatformId.NA1).ConfigureAwait(true);
                    var loopThruElements = 0;
                    var rank = lists[loopThruElements];

                    //making sure to pull the right rank
                    while (rank.QueueType != "RANKED_SOLO_5x5")
                    {
                        Console.WriteLine("made it into while " + loopThruElements + 1);
                        loopThruElements++;
                        rank = lists[loopThruElements];
                    }
                    await ReplyAsync("this is the player I found, " + "Name: " + summoner.Name + " Rank: " + rank.Tier + " " + rank.Rank);
                    rankOfIgn = rank.Tier + " " + rank.Rank;
                }
            }
            catch
            {
                await ReplyAsync("Unfortnately, there isn't a player by that name. Please try again");
                return;
            }
        }
        [Command("elo")]
        public async Task CalculateElo(IRole team)
        {
            var google = new googleSheet();
            var user = Context.User as SocketGuildUser;
            var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
            if (user.Roles.Contains(rolePermissionAdmin))
            {
                List<int> topfive = new List<int>();
                string[] values = google.CalculateElo(team.ToString());
                foreach(var row in values)
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
                            Console.WriteLine("made it here");
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
                Console.WriteLine("made it here"+ topfive.ToString());
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

            }

            }

        [Command("add", RunMode = RunMode.Async)]
        //can be used by admin/owner to create new team on google sheet api
        [Summary
       ("add to team")]
        public async Task addToTeam(IRole team, string role, IGuildUser calledUser)
        {

            //holds int for number of subs
            int numberOfSubs = 0;
            //initializes googlesheet
            var google = new googleSheet(); 

            var channel = Context.Guild as SocketGuild;

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
                    Console.WriteLine("adding top");

                }
                else if (role == "jg")
                {
                    Console.WriteLine("adding jg");
                }
                else if (role == "mid")
                {
                    Console.WriteLine("adding mid");
                }
                else if (role == "adc")
                {
                    Console.WriteLine("adding adc");
                }
                else if (role == "sup")
                {
                    Console.WriteLine("adding sup");
                }
                else if(role == "sub")
                {
                    
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
                        if(response == null)
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
                            Summoner summoner = await client.GetSummonerBySummonerNameAsync(response.ToString(), PlatformId.NA1).ConfigureAwait(false);                            if (summoner == null)
                            {
                                await ReplyAsync("Unfortnately, there isn't a player by that name. Please try again");
                                return;
                            }
                            else if(summoner != null)
                            {
                                List<LeagueEntry> lists = await client.GetLeagueEntriesBySummonerIdAsync(summoner.Id.ToString(), PlatformId.NA1).ConfigureAwait(false);
                                
                                var loopThruElements = 0;
                                var rank = lists[loopThruElements];
                                while (rank.QueueType != "RANKED_SOLO_5x5")
                                {
                                    loopThruElements++;
                                    rank = lists[loopThruElements];
                                }
                                await ReplyAsync("This is the player I found, " + "Name: " + summoner.Name + " Rank: " + rank.Tier + " " + rank.Rank + " is this the correct? Reply with 'yes' or 'no'");
                                rankOfIgn = rank.Tier + " " + rank.Rank;
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
                        if(confirmation == null)
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

                            try
                            {
                                if (role == "sub")
                                {
                                    if (numberOfSubs < 6)
                                    {
                                        var sendToGoogle = google.addToTeam(team.ToString(), role, confirm, rankOfIgn, numberOfSubs);
                                        await ReplyAsync(confirm + " successfully added to google sheet!");
                                        await calledUser.AddRoleAsync(team);
                                    }
                                    else
                                    {
                                        await ReplyAsync("You have six subs, do you want to overwrite? 'yes' or 'no' ");
                                        var shouldWeOverwriteASub = await NextMessageAsync();
                                        if(shouldWeOverwriteASub == null)
                                        {
                                            await ReplyAsync("No response");
                                            return;
                                        }
                                        if (shouldWeOverwriteASub.ToString().ToLower() == "yes")
                                        {
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
                                            if(numberOfAxedSub == null)
                                            {
                                                await ReplyAsync("No answer");
                                                return;
                                            }
                                            if (numberOfAxedSub.ToString() == "1" || numberOfAxedSub.ToString() == "2" || numberOfAxedSub.ToString() == "3" || numberOfAxedSub.ToString() == "4"
                                                || numberOfAxedSub.ToString() == "5" || numberOfAxedSub.ToString() == "6")
                                            {
                                                int intNumberOfAxedSub = Int32.Parse(numberOfAxedSub.ToString());
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
                        else if(confirmation == null)
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

        [Command("remove")]
        public async Task RemoveFromTeam(IRole role, IGuildUser removedUser)
        {
            var channel = Context.Guild as SocketGuild;


            var channelIdLogs = channel.GetTextChannel(767455535800385616).SendMessageAsync(Context.User.Username + " has used the remove command to remove role  " + role + " from " + removedUser);


            var user = Context.User as SocketGuildUser;
            var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");

            Console.WriteLine("made it past admin");
            var rolePermissionOwner = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Owner");


            var captainRole = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Team Captain");

            //checks if role admin or owner
            if ((user.Roles.Contains(rolePermissionAdmin)) || (user.Roles.Contains(rolePermissionOwner)) || ((user.Roles.Contains(captainRole) && user.Roles.Contains(role))))
            {
                await removedUser.RemoveRoleAsync(role);
                await ReplyAndDeleteAsync("Role removed");
            }
            else
            {
                await ReplyAndDeleteAsync("You are not owner/admin/captain of called team");
            }
        }

        [Command("checkperms")]
        public async Task checkPerms(IRole roleName)
        {
            var user = Context.User as SocketGuildUser;
            var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
            if (user.Roles.Contains(rolePermissionAdmin))
            {


                var roles = roleName.Permissions.ToString();



                await ReplyAsync(roles);

            }

        }

        [Command("checkcatid")]
        public async Task checkcatid(IGuildChannel id)
        {

            var user = Context.User as SocketGuildUser;
            var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
            if (user.Roles.Contains(rolePermissionAdmin))
            {

                var guildchannelID = id.GuildId.ToString();


                await ReplyAsync(guildchannelID);


            }
        }

        /* [Command("checkValue")]
         public async Task checkDeny(IGuildChannel id)
         {



            // var denyValue = new OverwritePermissions.

             var allowValue = id.AllowValue.ToString();


             await ReplyAsync("this is denyvalue" + denyValue);

             await ReplyAsync("this is approvevalue" + allowValue);



         }*/
    }
}

    
