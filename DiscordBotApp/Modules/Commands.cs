using Discord;
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


/*surround every command
 *  var user = Context.User as SocketGuildUser;
            var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
            if (user.Roles.Contains(rolePermissionAdmin)){
*/

namespace DiscordBotApp.Modules
{
    using googleSheet = DiscordBotApp.GoogleSheets.Sheets;
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("msg")]
        public async Task Msg(IRole role, [Remainder]string message)
        {
            var admin = Context.User as SocketGuildUser;
            var roleName = role.ToString();
            //var listOfUsers = (role as IChannel).GetUsersAsync(default, default);
            var listOfUsers = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName).Members;

            

            var rolePermissionAdmin = (admin as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
            if (admin.Roles.Contains(rolePermissionAdmin))
            {


                foreach (var user in listOfUsers)
                {
                    try
                    {
                        var channel = await user.GetOrCreateDMChannelAsync();
                        
                        await channel.SendMessageAsync($"Hello, this is a test for mass dms, {user.Username}. " + message);
                    }

                    catch (Discord.Net.HttpException ex) when (ex.HttpCode == HttpStatusCode.Forbidden)
                    {
                        Console.WriteLine($"Boo, I cannot message {user}.");
                    }
                }
            }
        }
        [Command("teamroster")]

        public async Task Ping(IRole teamName)
        {
            Console.WriteLine("made it in command" + Context.User);

            var user = Context.User as SocketGuildUser;
            var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
            if (user.Roles.Contains(rolePermissionAdmin))
            {
                  string[,] teamArray = new string[6, 3];
                  var teamNameWithoutNumber = teamName.Name;
                  var google = new googleSheet();
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
                        await ReplyAsync($"{row[0]}, {row[1]}, {row[2]}");
                    }
                }
            }
        }

        [Command("teamrostercheck")]

        public async Task teamCheck()
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
                var run = google.CreateTeam();
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

        [Command("createteam")]
        //can be used by admin/owner to create new team on google sheet api
        [Summary
      ("Create team with no org")]
        public async Task CreateTeam(string teamNameBeingCreated, IGuildUser calledUser)
        {
           
            var user = Context.User as SocketGuildUser;
            var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
            if (user.Roles.Contains(rolePermissionAdmin)){
               
                var rolePermissionOwner = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Owner");
                
                //captainroleid
                ulong roleId = 705651654082560003;
                var captainRole = Context.Guild.GetRole(roleId);
                //botid


                //checks if role admin or owner
                if ((user.Roles.Contains(rolePermissionAdmin)) || (user.Roles.Contains(rolePermissionOwner)))
                {
                    try
                    {


                        await ReplyAsync($"Team {teamNameBeingCreated} created, added {teamNameBeingCreated} and Team Captain role to {calledUser}");
                        var permissions = new GuildPermissions(104324673);

                        var addPermissions = new OverwritePermissions(104324673, 0);

                        ulong assignChannelIdTeamText = 705651659736350732;

                        ulong assignChannelIdTeamVoice = 705651662261321783;
                        Console.WriteLine("before text creation");
                        var createTextChannel = await Context.Guild.CreateTextChannelAsync(teamNameBeingCreated, channel => channel.CategoryId = assignChannelIdTeamText, default);
                        var createVoiceChannel = await Context.Guild.CreateVoiceChannelAsync(teamNameBeingCreated, channel => channel.CategoryId = assignChannelIdTeamVoice, default);
                        Console.WriteLine("beforeassigning role");
                        var createdRole = await Context.Guild.CreateRoleAsync(teamNameBeingCreated, permissions, default, false, default);
                        //var everyoneRole = Context.Guild.GetRole(705651653323522121);
                        //var everyoneRestRole = everyoneRole as RestRole;
                        Console.WriteLine("before addpermissions text");
                        

                        await createTextChannel.AddPermissionOverwriteAsync(createdRole, addPermissions);
                        Console.WriteLine("before addpermissions voice");
                        await createVoiceChannel.AddPermissionOverwriteAsync(createdRole, addPermissions);
                        Console.WriteLine("before apply permissions to everyone");
                        
                        await calledUser.AddRoleAsync(createdRole);
                        await calledUser.AddRoleAsync(captainRole);
                        Console.WriteLine("after addpermissions");

                     //   var google = new googleSheet();
                       // IList<IList<Object>> values = google.Google();


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
