using System.Configuration;
using Newtonsoft.Json;
using System.Collections.Specialized;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using RiotNet;
using RiotNet.Models;

using Data = Google.Apis.Sheets.v4.Data;

namespace DiscordBotApp
{
    namespace GoogleSheets
    {
        public class Sheets
        {

            // If modifying these scopes, delete your previously saved credentials
            // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
            static string[] Scopes = { SheetsService.Scope.Spreadsheets };
            static string ApplicationName = "Style Esports Bot";

            public void InputElo(string teamName, int elo)
            {
                Console.WriteLine("made it");
                UserCredential credential;

                using (var stream =
                    new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    // The file token.json stores the user's access and refresh tokens, and is created
                    // automatically when the authorization flow completes for the first time.
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                    Console.WriteLine("Credential file saved to: " + credPath);
                }

                // Create Google Sheets API service.
                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                //get token of sheet url
                string googleSheetUrl;
                googleSheetUrl = ConfigurationManager.AppSettings.Get("googleSheetUrl");


                SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum valueInputOption = (SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum)2;  // TODO: Update placeholder value.
                // Define request parameters.
                var elostring = new List<object>() { elo.ToString() };
                Console.WriteLine(elo);
                String range = $"{teamName}!D1";
                Data.ValueRange requestBody = new Data.ValueRange();
                requestBody.Values = new List<IList<object>>{ elostring };
               

                // Prints the names and igns in spreadsheet:
                SpreadsheetsResource.ValuesResource.UpdateRequest request = service.Spreadsheets.Values.Update(requestBody, googleSheetUrl, range);
                request.ValueInputOption = valueInputOption;
                

                // To execute asynchronously in an async method, replace `request.Execute()` as shown:
                Data.UpdateValuesResponse response = request.Execute();
                // Data.UpdateValuesResponse response = await request.ExecuteAsync();

                // TODO: Change code below to process the `response` object:
                Console.WriteLine(JsonConvert.SerializeObject(response));
               

            }
        
            public string[] CalculateElo(string teamName)
            {
                Console.WriteLine("made it");
                UserCredential credential;
                string riotkey;
                riotkey = ConfigurationManager.AppSettings.Get("riotkey");

                using (var stream =
                    new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    // The file token.json stores the user's access and refresh tokens, and is created
                    // automatically when the authorization flow completes for the first time.
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                    Console.WriteLine("Credential file saved to: " + credPath);
                }

                // Create Google Sheets API service.
                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                //get token of sheet url
                string googleSheetUrl;
                googleSheetUrl = ConfigurationManager.AppSettings.Get("googleSheetUrl");

                var numberOfSubs = CheckSubCount(teamName);


                // Define request parameters.
                
                string eloRangeForMainTeam = $"{teamName}!C2:C6";
                string eloRangeForSubTeam = $"{teamName}!C9";
                if (numberOfSubs == 0)
                {
                    Console.WriteLine("no subs");
                }
                else if (numberOfSubs == 1)
                {
                    Console.WriteLine("one sub");
                }
                else
                {
                    eloRangeForSubTeam = $"{teamName}!C9:C{numberOfSubs + 9}";
                }


                List<string> ranges = new List<string>();
                
                
                ranges.Add(eloRangeForMainTeam);
                if (numberOfSubs > 0)
                {
                    ranges.Add(eloRangeForSubTeam);
                }

                SpreadsheetsResource.ValuesResource.BatchGetRequest.ValueRenderOptionEnum valueRenderOption = (SpreadsheetsResource.ValuesResource.BatchGetRequest.ValueRenderOptionEnum)0;


                SpreadsheetsResource.ValuesResource.BatchGetRequest request = service.Spreadsheets.Values.BatchGet(googleSheetUrl);
                request.Ranges = ranges;
                request.ValueRenderOption = valueRenderOption;

                // Data.BatchGetValuesResponse response = await request.ExecuteAsync();

                // TODO: Change code below to process the `response` object:


                // Prints the names and igns in spreadsheet:
                try
                {
                    Data.BatchGetValuesResponse response = request.Execute();
                    //IList<IList<Object>> values = response.

                    List<Data.ValueRange> data = (List<ValueRange>)response.ValueRanges;

                    


                    var valuesOfMain = data[0].Values;
                    var valuesOfSub = data[1].Values;
                    if (valuesOfMain != null && valuesOfMain.Count > 0)
                    {

                        Console.WriteLine("Role, In Game Name");
                        string[] teamArray = new string[numberOfSubs + 5];
                        if (valuesOfSub != null && valuesOfSub.Count > 0)
                        {
                            int num = 0;
                            Console.WriteLine("Role, In Game Name, made in subs");



                            foreach (var row in valuesOfMain)
                            {

                                // Print columns A and E, which correspond to indices 0 and 4.
                                Console.WriteLine("Main roster{0}", row[0]);
                                teamArray[num] = row[0].ToString();
                                
                                num++;


                            }
                            foreach (var row in valuesOfSub)
                            {

                                // Print columns A and E, which correspond to indices 0 and 4.
                                Console.WriteLine("Sub roster {0}", row[0]);
                                teamArray[num] = row[0].ToString();
                                
                                num++;


                            }

                            Console.WriteLine("sending main" + valuesOfMain);
                            Console.WriteLine("sending subs" + valuesOfSub);
                            Console.WriteLine(teamArray[0]);
                            return teamArray;

                        }
                        else
                        {
                            int num = 0;
                            foreach (var row in valuesOfMain)
                            {

                                // Print columns A and E, which correspond to indices 0 and 4.
                                Console.WriteLine("Main roster{0}", row[0]);
                                teamArray[num] = row[0].ToString();
                                
                                num++;


                            }
                            Console.WriteLine("sending" + valuesOfMain);
                            Console.WriteLine(teamArray[0]);

                            return teamArray;
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("No data found.");
                    return null;
                }
                return null;

            }

            public string[,] TeamRoster( string teamName)
            {
                Console.WriteLine("made it");
                UserCredential credential;
                string riotkey;
                riotkey = ConfigurationManager.AppSettings.Get("riotkey");

                using (var stream =
                    new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    // The file token.json stores the user's access and refresh tokens, and is created
                    // automatically when the authorization flow completes for the first time.
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                    Console.WriteLine("Credential file saved to: " + credPath);
                }

                // Create Google Sheets API service.
                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                //get token of sheet url
                string googleSheetUrl;
                googleSheetUrl = ConfigurationManager.AppSettings.Get("googleSheetUrl");

                var numberOfSubs = CheckSubCount(teamName);
                

                // Define request parameters.
                string lastChecked = $"{teamName}!B1";
                string rangeForMainTeam = $"{teamName}!A2:C6";
                string rangeForSubTeam = $"{teamName}!B9";
                if(numberOfSubs == 0)
                {
                    Console.WriteLine("no subs");
                }else
                {
                    rangeForSubTeam = $"{teamName}!A9:C{numberOfSubs + 9}";
                }


                List<string> ranges = new List<string>();
                ranges.Add(lastChecked);
                ranges.Add( rangeForMainTeam );
                if (numberOfSubs > 0)
                {
                    ranges.Add(rangeForSubTeam);
                }

                SpreadsheetsResource.ValuesResource.BatchGetRequest.ValueRenderOptionEnum valueRenderOption = (SpreadsheetsResource.ValuesResource.BatchGetRequest.ValueRenderOptionEnum)0;


                SpreadsheetsResource.ValuesResource.BatchGetRequest request = service.Spreadsheets.Values.BatchGet(googleSheetUrl);
                request.Ranges = ranges;
                request.ValueRenderOption = valueRenderOption;
                
                // Data.BatchGetValuesResponse response = await request.ExecuteAsync();

                // TODO: Change code below to process the `response` object:
                
                
                // Prints the names and igns in spreadsheet:
                try
                {
                    Data.BatchGetValuesResponse response = request.Execute();
                    //IList<IList<Object>> values = response.

                    List<Data.ValueRange> data = (List<ValueRange>)response.ValueRanges;

                    /*if(data[0].Values == null)
                    {
                        IRiotClient client = new RiotClient(new RiotClientSettings
                        {
                            ApiKey = riotkey
                        });
                        try
                        {
                            Summoner summoner = client.GetSummonerBySummonerNameAsync(ign, PlatformId.NA1).ConfigureAwait(true);
                            Console.WriteLine(summoner.Name);
                            Console.WriteLine(summoner);
                            
                            else if (summoner != null)
                            {
                                Console.WriteLine("made it here fam");
                                Console.WriteLine(summoner.Id.ToString());
                                List<LeagueEntry> lists = client.GetLeagueEntriesBySummonerIdAsync(summoner.Id.ToString(), PlatformId.NA1).ConfigureAwait(true);
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
                            await ReplyAsync("unfortnately, there isn't a player by that name. Please try again");
                            return;
                        }
                    }*/
                

                    var valuesOfMain = data[1].Values;
                    var valuesOfSub = data[2].Values;
                    if (valuesOfMain != null && valuesOfMain.Count > 0)
                    {
                        
                        Console.WriteLine("Role, In Game Name");
                        string[,] teamArray = new string[numberOfSubs + 5, 3];
                        if (valuesOfSub != null && valuesOfSub.Count > 0)
                        {
                            int num = 0;
                            Console.WriteLine("Role, In Game Name, made in subs");
                            
                            
                            
                            foreach (var row in valuesOfMain)
                            {
                                
                                // Print columns A and E, which correspond to indices 0 and 4.
                                Console.WriteLine("Main roster{0}, {1} , {2}", row[0], row[1], row[2]);
                                teamArray[num, 0] = row[0].ToString();
                                teamArray[num, 1] = row[1].ToString();
                                teamArray[num, 2] = row[2].ToString();
                                num++;


                            }
                            foreach (var row in valuesOfSub)
                            {

                                // Print columns A and E, which correspond to indices 0 and 4.
                                Console.WriteLine("Sub roster {0}, {1}, {2}", row[0], row[1], row[2]);
                                teamArray[num, 0] = row[0].ToString();
                                teamArray[num, 1] = row[1].ToString();
                                teamArray[num, 2] = row[2].ToString();
                                num++;


                            }

                            Console.WriteLine("sending main" + valuesOfMain);
                            Console.WriteLine("sending subs" + valuesOfSub);
                            Console.WriteLine(teamArray[0, 0]);
                            return teamArray;

                        }
                        else
                        {
                            int num = 0;
                            foreach (var row in valuesOfMain)
                            {
                                
                                // Print columns A and E, which correspond to indices 0 and 4.
                                Console.WriteLine("Main roster{0}, {1} , {2}", row[0], row[1], row[2]);
                                teamArray[num, 0] = row[0].ToString();
                                teamArray[num, 1] = row[1].ToString();
                                teamArray[num, 2] = row[2].ToString();
                                num++;


                            }
                            Console.WriteLine("sending" + valuesOfMain);
                            Console.WriteLine(teamArray[0, 0]);
                            
                            return teamArray;
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("No data found.");
                    return null;
                }
                return null;
                
            }

            //gives list of subs to determine which to overwrite
            public IList<IList<Object>> SubDestroyer(string teamName)
            {
                Console.WriteLine("made it");
                UserCredential credential;

                using (var stream =
                    new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    // The file token.json stores the user's access and refresh tokens, and is created
                    // automatically when the authorization flow completes for the first time.
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                    Console.WriteLine("Credential file saved to: " + credPath);
                }

                // Create Google Sheets API service.
                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                //get token of sheet url
                string googleSheetUrl;
                googleSheetUrl = ConfigurationManager.AppSettings.Get("googleSheetUrl");

                // Define request parameters.

                String range = $"{teamName}!B9:B14";
                SpreadsheetsResource.ValuesResource.GetRequest request =
                        service.Spreadsheets.Values.Get(googleSheetUrl, range);

                // Prints the names and igns in spreadsheet:
                try
                {
                    ValueRange response = request.Execute();
                    IList<IList<Object>> values = response.Values;

                    if (values != null && values.Count > 0)
                    {
                        Console.WriteLine("Role, In Game Name");
                        string[] teamArray = new string[6];
                        foreach (var row in values)
                        {
                            int num = 0;
                            // Print columns A and E, which correspond to indices 0 and 4.
                            Console.WriteLine("{0}", row[0]);
                            teamArray[num] = row[0].ToString();
                            num++;


                        }
                        Console.WriteLine("sending" + values);
                        
                        return response.Values;
                    }
                }
                catch
                {
                    Console.WriteLine("No data found.");
                    return null;
                }
                return null;

            }
            //checks sub count
            public int CheckSubCount(string teamName)
            {
                Console.WriteLine("made it");
                UserCredential credential;

                using (var stream =
                    new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    // The file token.json stores the user's access and refresh tokens, and is created
                    // automatically when the authorization flow completes for the first time.
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                    Console.WriteLine("Credential file saved to: " + credPath);
                }

                // Create Google Sheets API service.
                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                //get token of sheet url
                string googleSheetUrl;
                googleSheetUrl = ConfigurationManager.AppSettings.Get("googleSheetUrl");

                // Define request parameters.

                String range = $"{teamName}!B9:B14";
                SpreadsheetsResource.ValuesResource.GetRequest request =
                        service.Spreadsheets.Values.Get(googleSheetUrl, range);

                // Prints the names and igns in spreadsheet:
                try
                {
                    ValueRange response = request.Execute();
                    IList<IList<Object>> values = response.Values;

                    if (values != null && values.Count > 0)
                    {
                        
                        
                        
                        Console.WriteLine("sending number of subs" + values.Count);
                        
                        return values.Count;
                    }
                }
                catch
                {
                    Console.WriteLine("No data found.");
                    return 0;
                }
                return 0;

            }


            //creates team
            public string CreateTeam(string teamName)
            {
                Console.WriteLine("made it into createteam");
                UserCredential credential;

                using (var stream =
                    new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    // The file token.json stores the user's access and refresh tokens, and is created
                    // automatically when the authorization flow completes for the first time.
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                    Console.WriteLine("Credential file saved to: " + credPath);
                }

                // Create Google Sheets API service.
                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                //get token of sheet url
                string googleSheetUrl;
                googleSheetUrl = ConfigurationManager.AppSettings.Get("googleSheetUrl");

                // Define request parameters.

                //String range = teamName;
                var responseOfTeam = service.Spreadsheets.Get(googleSheetUrl);
                //Spreadsheet responsePlz =  service.Spreadsheets().Get(googleSheetUrl);


                // Prints the names and igns in spreadsheet:

                // The ID of the spreadsheet containing the sheet to copy.
                 // TODO: Update placeholder value.


                // The ID of the sheet to copy.
                //guid in url
               // int sheetId = 1048358764;  

                // The ID of the spreadsheet to copy the sheet to.
                string destinationSpreadsheetId = googleSheetUrl;  // TODO: Update placeholder value.

                // TODO: Assign values to desired properties of `requestBody`:



                //CopySheetToAnotherSpreadsheetRequest requestBody = new CopySheetToAnotherSpreadsheetRequest();
                //requestBody.DestinationSpreadsheetId = destinationSpreadsheetId;

                //SpreadsheetsResource.SheetsResource.CopyToRequest request = service.Spreadsheets.Sheets.CopyTo(requestBody, googleSheetUrl, sheetId);

                

                // To execute asynchronously in an async method, replace `request.Execute()` as shown:
              // request.
                
                //SheetProperties response = request.Execute();
                var masterSheet = service.Spreadsheets.Get(googleSheetUrl).Execute().Sheets.First(x => x.Properties.Title.Equals("Master"));
                var newSheet = new Request
                {
                    DuplicateSheet = new DuplicateSheetRequest
                    {
                        SourceSheetId = masterSheet.Properties.SheetId,
                        NewSheetName = teamName,
                        InsertSheetIndex = 8
                       

                    }
                };
                var updater = new BatchUpdateSpreadsheetRequest { Requests = new List<Request> { newSheet } };
                service.Spreadsheets.BatchUpdate(updater, googleSheetUrl).Execute();
                
                //Console.WriteLine(JsonConvert.SerializeObject(response));


                // var requestNewSheetUpdate = new UpdateSheetPropertiesRequest();


                //SpreadsheetsResource.ValuesResource.UpdateRequest requestUpdate = service.Spreadsheets.Values.Update(requestBody, googleSheetUrl, response.Title);
                // response.Title.Equals("SuperTesty");

                // Data.SheetProperties response = await request.ExecuteAsync();

                // TODO: Change code below to process the `response` object:
                // Console.WriteLine(JsonConvert.SerializeObject(response));

                return null;
               /* else
                {
                    Console.WriteLine("No data found.");
                    
                }*/
               // Console.Read();
            }

            public string addToTeam(string teamName, string role, string ign, string rankOfIgn)
            {
                Console.WriteLine("made it into addteam in google");
                Console.WriteLine("team name is " + teamName);
                Console.WriteLine("role is " + role);
                Console.WriteLine("i list object is " + ign);
                UserCredential credential;

                using (var stream =
                    new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    // The file token.json stores the user's access and refresh tokens, and is created
                    // automatically when the authorization flow completes for the first time.
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                    Console.WriteLine("Credential file saved to: " + credPath);
                }

                // Create Google Sheets API service.
                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
                Console.WriteLine("made it past sheets");
                //get token of sheet url
                string googleSheetUrl;
                googleSheetUrl = ConfigurationManager.AppSettings.Get("googleSheetUrl");

                String rangeForIgn = $"{teamName}!B2";
                // Define request parameters.
                if (role == "top")
                {

                    rangeForIgn = $"{teamName}!B2";

                }
                else if (role == "jg")
                {
                    rangeForIgn = $"{teamName}!B3";

                }
                else if (role == "mid")
                {
                    rangeForIgn = $"{teamName}!B4";
                }
                else if (role == "adc")
                {
                    rangeForIgn = $"{teamName}!B5";
                }
                else if (role == "sup")
                {
                    rangeForIgn = $"{teamName}!B6";
                }

                String rangeForRank = $"{teamName}!C2";
                // Define request parameters.
                if (role == "top")
                {

                    rangeForRank = $"{teamName}!C2";

                }
                else if (role == "jg")
                {
                    rangeForRank = $"{teamName}!C3";

                }
                else if (role == "mid")
                {
                    rangeForRank = $"{teamName}!C4";
                }
                else if (role == "adc")
                {
                    rangeForRank = $"{teamName}!C5";
                }
                else if (role == "sup")
                {
                    rangeForRank = $"{teamName}!C6";
                }
                string valueInputOption = "RAW";

                //get date and time to mark last change
                DateTime today = DateTime.UtcNow;
                //range for time
                var rangeForTime = $"{teamName}!B7";


                var ignList = new string[] { ign };
                var ignListRank = new string[] { rankOfIgn };
                var dateAndTime = new string[] { today.ToString("f") + " " + today.Kind };


                List<Data.ValueRange> data = new List<Data.ValueRange>();
                data.Add(new Data.ValueRange() { Range = rangeForIgn, Values = new List<IList<object>>{ ignList } });
                data.Add(new Data.ValueRange() { Range = rangeForRank, Values = new List<IList<object>> { ignListRank } });
                data.Add(new Data.ValueRange() { Range = rangeForTime, Values = new List<IList<object>> { dateAndTime } });



                Data.BatchUpdateValuesRequest requestBody = new Data.BatchUpdateValuesRequest();
                requestBody.ValueInputOption = valueInputOption;
                requestBody.Data = data;

                SpreadsheetsResource.ValuesResource.BatchUpdateRequest request = service.Spreadsheets.Values.BatchUpdate(requestBody, googleSheetUrl);

                // To execute asynchronously in an async method, replace `request.Execute()` as shown:
                Data.BatchUpdateValuesResponse response = request.Execute();
                // Data.BatchUpdateValuesResponse response = await request.ExecuteAsync();

                // TODO: Change code below to process the `response` object:
                Console.WriteLine(JsonConvert.SerializeObject(response));

                return null;
                /* else
                 {
                     Console.WriteLine("No data found.");

                 }*/
                // Console.Read();
            }

            //overload method for subs
            public string addToTeam(string teamName, string role, string ign, string rankOfIgn, int numberOfSubs)
            {
                Console.WriteLine("made it into addteam in google");
                Console.WriteLine("team name is " + teamName);
                Console.WriteLine("role is " + role);
                Console.WriteLine("i list object is " + ign);
                UserCredential credential;

                using (var stream =
                    new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    // The file token.json stores the user's access and refresh tokens, and is created
                    // automatically when the authorization flow completes for the first time.
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                    Console.WriteLine("Credential file saved to: " + credPath);
                }

                // Create Google Sheets API service.
                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
                Console.WriteLine("made it past sheets");
                //get token of sheet url
                string googleSheetUrl;
                googleSheetUrl = ConfigurationManager.AppSettings.Get("googleSheetUrl");

                String rangeForIgn = $"{teamName}!B2";
                // Define request parameters.
                if (role == "top")
                {

                    rangeForIgn = $"{teamName}!B2";

                }
                else if (role == "jg")
                {
                    rangeForIgn = $"{teamName}!B3";

                }
                else if (role == "mid")
                {
                    rangeForIgn = $"{teamName}!B4";
                }
                else if (role == "adc")
                {
                    rangeForIgn = $"{teamName}!B5";
                }
                else if (role == "sup")
                {
                    rangeForIgn = $"{teamName}!B6";
                }
                else if (role == "sub")
                {
                    rangeForIgn = $"{teamName}!B{numberOfSubs + 9}";
                }

                String rangeForRank = $"{teamName}!C2";
                // Define request parameters.
                if (role == "top")
                {

                    rangeForRank = $"{teamName}!C2";

                }
                else if (role == "jg")
                {
                    rangeForRank = $"{teamName}!C3";

                }
                else if (role == "mid")
                {
                    rangeForRank = $"{teamName}!C4";
                }
                else if (role == "adc")
                {
                    rangeForRank = $"{teamName}!C5";
                }
                else if (role == "sup")
                {
                    rangeForRank = $"{teamName}!C6";
                }
                else if (role == "sub")
                {
                    rangeForRank = $"{teamName}!C{numberOfSubs + 9}";
                }
                string valueInputOption = "RAW";

                //get date and time to mark last change
                DateTime today = DateTime.UtcNow;

                var ignList = new string[] { ign };
                var ignListRank = new string[] { rankOfIgn };
                var dateAndTime = new string[] { today.ToString("f") + " " + today.Kind};
                
                //range for time
               var rangeForTime = $"{teamName}!B7";


                List<Data.ValueRange> data = new List<Data.ValueRange>();
                data.Add(new Data.ValueRange() { Range = rangeForIgn, Values = new List<IList<object>> { ignList } });
                data.Add(new Data.ValueRange() { Range = rangeForRank, Values = new List<IList<object>> { ignListRank } });
                data.Add(new Data.ValueRange() { Range = rangeForTime, Values = new List<IList<object>> { dateAndTime } });


                Data.BatchUpdateValuesRequest requestBody = new Data.BatchUpdateValuesRequest();
                requestBody.ValueInputOption = valueInputOption;
                requestBody.Data = data;

                SpreadsheetsResource.ValuesResource.BatchUpdateRequest request = service.Spreadsheets.Values.BatchUpdate(requestBody, googleSheetUrl);

                // To execute asynchronously in an async method, replace `request.Execute()` as shown:
                Data.BatchUpdateValuesResponse response = request.Execute();
                // Data.BatchUpdateValuesResponse response = await request.ExecuteAsync();

                // TODO: Change code below to process the `response` object:
                Console.WriteLine(JsonConvert.SerializeObject(response));

                return null;
                /* else
                 {
                     Console.WriteLine("No data found.");

                 }*/
                // Console.Read();
            }


        }
    }
}

