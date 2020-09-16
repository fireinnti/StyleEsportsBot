using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DiscordBotApp.Modules
{
    using googleSheet = DiscordBotApp.GoogleSheets.Sheets;
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("teamroster")]

        public async Task Ping(string teamnamess)
        {
            string[,] teamArray = new string[5, 2];
            
            var google = new googleSheet();
            IList<IList<Object>> values = google.Google();
            //Console.WriteLine("am I in here?" + google.Google());
            foreach(var row in values)
            {
               await ReplyAsync($"{row[0]}, {row[1]}");
            }
            
           // await ReplyAsync("recieved"); 
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
        [Command("match")]
        //creates a match, can be used by team captain
        public async Task CheckMatchDetails(string team1, string team2, string date, string time)
        {
            var user = Context.User as SocketGuildUser;
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

       /* [Command("updateroster")]
        // updates roster, can be used by team captain
        public async Task UpdateRoster(IUser user)
        {
            var user = Context.User as SocketGuildUser;
            var roleOfUser = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Team Captain");

        }*/

        [Command("createteam")]
        //can be used by admin/owner to create new team on google sheet api
        public async Task CreateTeam(string sampleteam, IGuildUser calledUser)
        {
           
            var user = Context.User as SocketGuildUser;
            var rolePermissionOwner = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Owner");
            var rolePermissionAdmin = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "Admin");
            ulong roleId = 705651654082560003;
            var captainRole = Context.Guild.GetRole(roleId);

            //checks if role admin or owner
            if ((user.Roles.Contains(rolePermissionAdmin)) || (user.Roles.Contains(rolePermissionOwner)))
            {
                try
                {
                    await ReplyAsync($"Team {sampleteam} created, added {sampleteam} and Team Captain role to {calledUser}");
                    var createdRole = await Context.Guild.CreateRoleAsync(sampleteam, default, default, false, default);
                    await calledUser.AddRoleAsync(createdRole);
                    await calledUser.AddRoleAsync(captainRole);
                }
                catch
                {
                    await ReplyAsync("error, please use format !createteam nameofteam @teamcaptainofteam");
                }

            }
            else
            {
                await ReplyAsync("You are not owner/admin");
            }


        }
        
    }
}
