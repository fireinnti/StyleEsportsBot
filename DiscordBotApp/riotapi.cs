using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBotApp
{
    namespace riotApi
    {
        public class riot
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int ProfileIconId { get; set; }
            public int SummonerLevel { get; set; }
            public long RevisionDate { get; set; }
        }
    }
}
