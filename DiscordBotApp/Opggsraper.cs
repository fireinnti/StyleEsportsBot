using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Parser;
using System.Web;
using System.Linq;
using AngleSharp.Dom;

namespace DiscordBotApp
{
    class Opggsraper
    {
        public async Task<string> ScrapeMe(string ign)
        {
            //Use the default configuration for AngleSharp
            var config = Configuration.Default.WithDefaultLoader();

            
            var address = "https://na.op.gg/summoner/userName=" + ign;
            //Create a new context for evaluating webpages with the given config
            var context = BrowsingContext.New(config);

            

            

            

            //Source to be parsed
            

            //Create a virtual request to specify the document to load (here from our fixed string)
            var document = await context.OpenAsync(address);

            //Do something with document like the following
            Console.WriteLine("Serializing the (original) document:");



            var cells = document.QuerySelectorAll("li > b");
            string lastSeasonRank = "";
            var checkForS2020 = "S2020";
            foreach (var item in cells)
            {
                Console.WriteLine(item);
                if (checkForS2020 == item.InnerHtml.ToString())
                {
                    lastSeasonRank = item.ParentElement.GetAttribute("title").ToString();
                }
            }

            
           // var cells = document.QuerySelectorAll("b").;

           /// html / body / div[2] / div[3] / div / div / div[1] / div[1] / ul / li[4]

            //Console.WriteLine("Serializing the document again:");
            Console.WriteLine(lastSeasonRank);

            string[] rank = lastSeasonRank.Split(' ');
            string onlyRank = rank[0] + " " + rank[1];



            return onlyRank;
        }

    }
}


