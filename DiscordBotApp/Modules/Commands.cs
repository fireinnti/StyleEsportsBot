using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Discord.Rest;
using System.Web;
using System.Runtime.CompilerServices;



/*surround every command
 *  var user = Context.User as SocketGuildUser;
            var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
            if (user.Roles.Contains(rolePermissionAdmin)){
*/

namespace DiscordBotApp.Modules
{
    using googleSheet = DiscordBotApp.GoogleSheets.Sheets;
    public class Commands : InteractiveBase
    {
        [Command("msg")]
        public async Task Msg(IRole role, [Remainder]string message)
        {
            //logging purposes
            var channel = Context.Guild as SocketGuild;
            var channelIdLogs = channel.GetTextChannel(767455535800385616).SendMessageAsync(Context.User.Username + " has used the msg command for " + role);

            var admin = Context.User as SocketGuildUser;
            var roleName = role.ToString();
            //var listOfUsers = (role as IChannel).GetUsersAsync(default, default);
            var listOfUsers = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName).Members;
            Console.Write("before checking admin");
            Console.WriteLine("list of users " + listOfUsers);

            var rolePermissionAdmin = (admin as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
            if (admin.Roles.Contains(rolePermissionAdmin))
            {
                Console.Write("is admin");

                foreach (var user in listOfUsers)
                {
                    Console.WriteLine("in for each" + user);
                    try
                    {
                        var channelb = await user.GetOrCreateDMChannelAsync();
                        
                        await channelb.SendMessageAsync($"Hello, {user.Username}! " + message);
                        await Task.Delay(1000);
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
                  IList<IList<Object>> values = google.Google(teamNameWithoutNumber);
                //Console.WriteLine("am I in here?" + google.Google());
                if (values == null)
                {
                    await ReplyAsync("role does not match a team in google sheets roster");
                }
                else
                {
                    foreach (var row in values)
                    {
                        //await ReplyAsync($"{row[0]}, {row[1]}, {row[2]}");
                        oneMessage = oneMessage + ($"{row[0]}, {row[1]}, {row[2]}\n");
                        stringOfIgns = stringOfIgns + row[1] + ",";
                        
                    }


                    await ReplyAsync(oneMessage + "\nhttps://na.op.gg/multi/query=" + HttpUtility.UrlEncode(stringOfIgns) + "\nIf there are not 5 IGNs, please contact Admins.");
                   // await ReplyAsync("If there are not 5 IGNs, please contact Admins.");


                }
            }
            /*else
            {
                await ReplyAsync("You do not have permission to use this command");
            }*/
        }

        [Command("creategooglesheet")]

        public async Task createGoogleSheet()
        {
            Console.WriteLine("made it in command" + Context.User);

            var user = Context.User as SocketGuildUser;
            var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
            if (user.Roles.Contains(rolePermissionAdmin))
            {
                /*  string[,] teamArray = new string[5, 2];

                  var google = new googleSheet();
                  IList<IList<Object>> values = google.Google();
                  //Console.WriteLine("am I in here?" + google.Google());
                  foreach (var row in values)
                  {
                      await ReplyAsync($"{row[0]}, {row[1]}");
                  }
                */
                Console.WriteLine("made it");
                var google = new googleSheet();
               // var run = google.CreateTeam();
                await ReplyAsync("recieved");
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

        /* [Command("updateroster")]
         // updates roster, can be used by team captain
         public async Task UpdateRoster(IUser user)
         {
             var user = Context.User as SocketGuildUser;
             var roleOfUser = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Team Captain");

         }*/

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
            if (user.Roles.Contains(rolePermissionAdmin)){
               
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

                            if (askIfOrg.ToString() == "yes" || askIfOrg.ToString() == "no")
                            {
                                await ReplyAsync("Okay, what is the orgs name?");
                                var nameOfOrg = await NextMessageAsync();
                                if (nameOfOrg != null)
                                {
                                    await ReplyAsync("Do you want to create a googlesheet for this team? Enter yes or no.");
                                    var response = await NextMessageAsync();
                                    if (response != null)
                                    {
                                        if (response.ToString() == "yes" || response.ToString() == "no")
                                        {
                                            await ReplyAsync($"Team {teamNameBeingCreated} created, added {teamNameBeingCreated} and Team Captain role to {calledUser}");

                                            var permissions = new GuildPermissions(104324673);

                                            var addPermissions = new OverwritePermissions(70770240, 0);
                                            //org category id
                                            ulong assignChannelIdOrgText = 774102389341814794;

                                            //full channel team text
                                            // ulong assignChannelIdTeamText = 622919737365495849;
                                            ulong assignChannelIdTeamText = 774104289919500298;


                                            //full voice channel
                                            //ulong assignChannelIdTeamVoice = 622919840444841984;
                                            ulong assignChannelIdTeamVoice = 774104754938052609;

                                            //create new channels and assign them under categories
                                            var createTextChannel = await Context.Guild.CreateTextChannelAsync(teamNameBeingCreated, channel => channel.CategoryId = assignChannelIdTeamText, default);
                                            var createVoiceChannel = await Context.Guild.CreateVoiceChannelAsync(teamNameBeingCreated, channel => channel.CategoryId = assignChannelIdTeamVoice, default);

                                            var createdRole = await Context.Guild.CreateRoleAsync(teamNameBeingCreated, permissions, default, false, default);
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



                                            if (askIfOrg.ToString() == "yes")
                                            {

                                                var createOrgTextChannel = await Context.Guild.CreateTextChannelAsync(nameOfOrg.ToString(), channel => channel.CategoryId = assignChannelIdOrgText, default);
                                                await createOrgTextChannel.AddPermissionOverwriteAsync(createdRole, addPermissions);
                                            }
                                            else if (askIfOrg.ToString() == "no")
                                            {
                                                await ReplyAsync("Okay, this is not an org.");
                                            }





                                            await calledUser.AddRoleAsync(createdRole);
                                            await calledUser.AddRoleAsync(captainRole);
                                            Console.WriteLine("after addpermissions");



                                            if (response.ToString() == "yes")
                                            {
                                                var google = new googleSheet();
                                                var run = google.CreateTeam(teamNameBeingCreated);
                                                await ReplyAsync("Google sheet created.");
                                            }
                                            else if (response.ToString() == "no")
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
                        await ReplyAsync("error, please use format !createteam nameofteam (optional name of org) @teamcaptainofteam");
                    }

                }
                else
                {
                    await ReplyAsync("You are not owner/admin");
                }


            }
           
        }

        [Command("createteam")]
        //can be used by admin/owner to create new team on google sheet api
        [Summary
      ("Create team with org")]
        public async Task CreateTeam(string teamNameBeingCreated, string orgNameBeingCreated, IGuildUser calledUser)
        {
            var user = Context.User as SocketGuildUser;
            var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
            if (user.Roles.Contains(rolePermissionAdmin))
            {
               
                var rolePermissionOwner = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Owner");
                
                ulong roleId = 705651654082560003;
                var captainRole = Context.Guild.GetRole(roleId);

                //checks if role admin or owner
                if ((user.Roles.Contains(rolePermissionAdmin)) || (user.Roles.Contains(rolePermissionOwner)))
                {
                    try
                    {
                        await ReplyAsync($"Team {teamNameBeingCreated} created, Org {orgNameBeingCreated} created, added {teamNameBeingCreated} and Team Captain role to {calledUser}");
                        var createdRole = await Context.Guild.CreateRoleAsync(teamNameBeingCreated, default, default, false, default);
                        await calledUser.AddRoleAsync(createdRole);
                        await calledUser.AddRoleAsync(captainRole);
                    }
                    catch
                    {
                        await ReplyAsync("error, please use format !createteam nameofteam (optional name of org) @teamcaptainofteam");
                    }

                }
                else
                {
                    await ReplyAsync("You are not owner/admin");
                }


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
