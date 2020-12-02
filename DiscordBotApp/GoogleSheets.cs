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


            public double[] GetElo(string teamName, string opponentName) {
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




                // Define request parameters.

                string rangeForEloOfTeam = $"{teamName}!B3";
                string rangeForEloOfOpponent = $"{opponentName}!B3";



                List<string> ranges = new List<string>();
                ranges.Add(rangeForEloOfTeam);
                ranges.Add(rangeForEloOfOpponent);


                SpreadsheetsResource.ValuesResource.BatchGetRequest.ValueRenderOptionEnum valueRenderOption = (SpreadsheetsResource.ValuesResource.BatchGetRequest.ValueRenderOptionEnum)0;


                SpreadsheetsResource.ValuesResource.BatchGetRequest request = service.Spreadsheets.Values.BatchGet(googleSheetUrl);
                request.Ranges = ranges;
                request.ValueRenderOption = valueRenderOption;

                // Data.BatchGetValuesResponse response = await request.ExecuteAsync();

                // TODO: Change code below to process the `response` object:

                double[] teams = new double[2];
                // Prints the names and igns in spreadsheet:
                try
                {
                    Data.BatchGetValuesResponse response = request.Execute();
                    //IList<IList<Object>> values = response.

                    List<Data.ValueRange> data = (List<ValueRange>)response.ValueRanges;

                    var rangeOfYourTeam = data[0].Values[0];
                    var rangeOfEnemyTeam = data[1].Values[0];
                    Console.WriteLine(rangeOfYourTeam[0].ToString() + rangeOfEnemyTeam[0].ToString());

                    double challenger = Double.Parse(rangeOfYourTeam[0].ToString());
                    double challenged = Double.Parse(rangeOfEnemyTeam[0].ToString());

                    teams[0] = challenger;
                    teams[1] = challenged;
                    return teams;
                
                }catch{
                    return null;
                }
            }
            //adds match result to googlesheet
            public void MatchResult(string teamName, string opponentName, string result, string schedule, string[] links, double plusMinus)
            {

                Console.WriteLine("made it into matchresult");
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
                string versusCell = $"Vs {opponentName}";
                string resultCell = $"{result}";
                string gameDateCell = $"{schedule}";
                string []gameLinks = links;
                
                   
                
                var dataBeingInserted = new List<object>() { versusCell, resultCell, gameDateCell, gameLinks[0], gameLinks[1], gameLinks[2], plusMinus.ToString() };
                



                string range = $"{teamName}!A58:G58";

               
                






                // Data.UpdateValuesResponse response = await request.ExecuteAsync();

                SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum valueInputOption = (SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum)1;  // TODO: Update placeholder value.

                // How the input data should be inserted.
                SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum insertDataOption = (SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum)1;  // TODO: Update placeholder value.

                // TODO: Assign values to desired properties of `requestBody`:
                Data.ValueRange requestBody = new Data.ValueRange();
                requestBody.Values = new List<IList<object>> { dataBeingInserted };
                


                SpreadsheetsResource.ValuesResource.AppendRequest request = service.Spreadsheets.Values.Append(requestBody, googleSheetUrl, range);
                request.ValueInputOption = valueInputOption;
                request.InsertDataOption = insertDataOption;

                // To execute asynchronously in an async method, replace `request.Execute()` as shown:
                Data.AppendValuesResponse response = request.Execute();

                // TODO: Change code below to process the `response` object:
                Console.WriteLine(JsonConvert.SerializeObject(response.ToString()));

            }

            //inputs elo into ladder
            public void Validate(string teamName)
            {

                Console.WriteLine("made it into inputelo");
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
                string formula = $"='{teamName}'!B2";
                string formulaRank = $"='{teamName}'!B3";

                var formularString = new List<object>() { formula, formulaRank };
                
                string range = $"Live Ratings!H2:I2";






                // Data.UpdateValuesResponse response = await request.ExecuteAsync();

                SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum valueInputOption = (SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum)2;  // TODO: Update placeholder value.

                // How the input data should be inserted.
                SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum insertDataOption = (SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum)1;  // TODO: Update placeholder value.

                // TODO: Assign values to desired properties of `requestBody`:
                Data.ValueRange requestBody = new Data.ValueRange();
                requestBody.Values = new List<IList<object>> { formularString };


                SpreadsheetsResource.ValuesResource.AppendRequest request = service.Spreadsheets.Values.Append(requestBody, googleSheetUrl, range);
                request.ValueInputOption = valueInputOption;
                request.InsertDataOption = insertDataOption;

                // To execute asynchronously in an async method, replace `request.Execute()` as shown:
                Data.AppendValuesResponse response = request.Execute();

                // TODO: Change code below to process the `response` object:
                Console.WriteLine(JsonConvert.SerializeObject(response));

            }
            public string[,] MoveUpList(string teamName, int howManyOnList, int start , string locationOfMove)
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




                // Define request parameters.

                string moveInformationRange = null;
                if(locationOfMove == "challenge")
                {
                    moveInformationRange = $"{teamName}!H{start + 9}:I{howManyOnList + 9}";
                }
                else if(locationOfMove == "preschedule")
                {
                    moveInformationRange = $"{teamName}!H{start + 15}:I{howManyOnList + 15}";

                }
                else if(locationOfMove == "confirmedmatch")
                {
                    moveInformationRange = $"{teamName}!E{start + 15}:F{howManyOnList + 15}";
                }

                
                


                List<string> ranges = new List<string>();
                
                ranges.Add(moveInformationRange);
                

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
                    
                    
                    
                    if (valuesOfMain != null && valuesOfMain.Count > 0)
                    {

                        
                        string[,] teamArray = new string[howManyOnList,2];
                        


                            int num = 0;
                            foreach (var row in valuesOfMain)
                            {

                                // Print columns A and E, which correspond to indices 0 and 4.
                                Console.WriteLine("Main roster{0}, {1}", row[0], row[1]);
                                teamArray[num, 0] = row[0].ToString();
                                teamArray[num, 1] = row[1].ToString();
                                
                                num++;


                            }
                            Console.WriteLine("sending" + valuesOfMain);
                            Console.WriteLine(teamArray[0, 0]);

                            return teamArray;
                        }
                    
                }
                catch
                {
                    Console.WriteLine("No data found.");
                    return null;
                }
                return null;

            }

            public void RemoveSchedule(string teamName, int teamPosition, int lengthOfTeamList, string opponentName, int opponentPosition, int lengthOfOpponentList, string typeOfRemove)
            {
                
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

                //ranges of the teams that will need removal.
                string yourTeamRange = null;
                string opponentTeamRange = null;
               // string yourTeamListLengthName = null;
                string yourTeamListLengthBehindName = null;
                //string yourOpponentTeamListLengthName = null;
                string yourOpponentTeamListLengthBehindName = null;
                //checks type of removal
                if (typeOfRemove == "preschedule")
                {
                    yourTeamRange = $"{teamName}!H{teamPosition + 15}:I{teamPosition + 15}";
                    opponentTeamRange = $"{opponentName}!H{opponentPosition + 15}:I{opponentPosition + 15}";
                    //yourTeamListLengthName = $"{teamName}!H16:I16";
                    yourTeamListLengthBehindName = $"{teamName}!H{lengthOfTeamList + 15}:I{lengthOfTeamList + 15}";
                    //yourOpponentTeamListLengthName = $"{opponentName}!H{:I16";
                    yourOpponentTeamListLengthBehindName = $"{opponentName}!H{lengthOfOpponentList + 15}:I{lengthOfOpponentList + 15}";
                }
                else if(typeOfRemove == "challenge")
                {
                    yourTeamRange = $"{teamName}!H{teamPosition + 9}:I{teamPosition + 9}";
                    opponentTeamRange = $"{opponentName}!H{opponentPosition + 9}:I{opponentPosition + 9}";
                    //yourTeamListLengthName = $"{teamName}!H10:I10";
                    yourTeamListLengthBehindName = $"{teamName}!H{lengthOfTeamList + 9}:I{lengthOfTeamList + 9}";
                   // yourOpponentTeamListLengthName = $"{opponentName}!H10:I10";
                    yourOpponentTeamListLengthBehindName = $"{opponentName}!H{lengthOfOpponentList + 9}:I{lengthOfOpponentList  + 9}";
                }
                else if (typeOfRemove == "confirmedmatch")
                {
                    yourTeamRange = $"{teamName}!E{teamPosition + 15}:F{teamPosition + 15}";
                    opponentTeamRange = $"{opponentName}!E{opponentPosition + 15}:F{opponentPosition + 15}";
                    //yourTeamListLengthName = $"{teamName}!H10:I10";
                    yourTeamListLengthBehindName = $"{teamName}!E{lengthOfTeamList + 15}:F{lengthOfTeamList + 15}";
                    // yourOpponentTeamListLengthName = $"{opponentName}!H10:I10";
                    yourOpponentTeamListLengthBehindName = $"{opponentName}!E{lengthOfOpponentList + 15}:F{lengthOfOpponentList + 15}";
                }
                string[,] infoYourTeam = null;
                string[,] infoOpponentTeam = null;
                if (lengthOfTeamList > 1 && lengthOfTeamList != teamPosition)
                {
                    infoYourTeam = MoveUpList(teamName, lengthOfTeamList, teamPosition, typeOfRemove);
                }
                if(lengthOfOpponentList > 1 && lengthOfOpponentList != opponentPosition)
                {
                    infoOpponentTeam = MoveUpList(opponentName, lengthOfOpponentList, opponentPosition, typeOfRemove);
                }

             
                string valueInputOption = "RAW";




                


                var yourTeamList = new string[] { "", "" };
                var opponentTeamList = new string[] { "", "" };
                var moveUpList = new string[] { "", "" };
                var moveUpListOpponent = new string[] { "", "" };



                List<Data.ValueRange> data = new List<Data.ValueRange>();
                data.Add(new Data.ValueRange() { Range = yourTeamRange, Values = new List<IList<object>> { yourTeamList } });
                data.Add(new Data.ValueRange() { Range = opponentTeamRange, Values = new List<IList<object>> { opponentTeamList} });

                //checks if it needs to move up the list
                if (lengthOfTeamList > 1 && lengthOfTeamList != teamPosition)
                {
                    moveUpList[0] = infoYourTeam[lengthOfTeamList - teamPosition, 0].ToString();
                    moveUpList[1] = infoYourTeam[lengthOfTeamList - teamPosition, 1].ToString();
                    
                        data.Add(new Data.ValueRange() { Range = yourTeamRange, Values = new List<IList<object>> { moveUpList } });

                    
                    data.Add(new Data.ValueRange() { Range = yourTeamListLengthBehindName, Values = new List<IList<object>> { yourTeamList } });
                }
                if(lengthOfOpponentList > 1 && lengthOfOpponentList != opponentPosition)
                {
                    moveUpListOpponent[0] = infoOpponentTeam[lengthOfOpponentList-opponentPosition, 0].ToString();
                    moveUpListOpponent[1] = infoOpponentTeam[lengthOfOpponentList-opponentPosition, 1].ToString();
                    data.Add(new Data.ValueRange() { Range = opponentTeamRange, Values = new List<IList<object>> { moveUpListOpponent } });

                    
                    data.Add(new Data.ValueRange() { Range = yourOpponentTeamListLengthBehindName, Values = new List<IList<object>> { opponentTeamList } });
                }
                



                Data.BatchUpdateValuesRequest requestBody = new Data.BatchUpdateValuesRequest();
                requestBody.ValueInputOption = valueInputOption;
                requestBody.Data = data;

                SpreadsheetsResource.ValuesResource.BatchUpdateRequest request = service.Spreadsheets.Values.BatchUpdate(requestBody, googleSheetUrl);

                // To execute asynchronously in an async method, replace `request.Execute()` as shown:
                Data.BatchUpdateValuesResponse response = request.Execute();
                // Data.BatchUpdateValuesResponse response = await request.ExecuteAsync();

                // TODO: Change code below to process the `response` object:
                Console.WriteLine(JsonConvert.SerializeObject(response));

                
                /* else
                 {
                     Console.WriteLine("No data found.");

                 }*/
            }
            public string[] Confirm(string teamName, string opponentName, string locationOfInput, bool switchTeam)
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

                // changes which team to remove from.
                string currentTeam = null ;
                string opposingTeam = null ;
                if (!switchTeam)
                {
                    currentTeam = teamName;
                    opposingTeam = opponentName;
                }else
                {
                    currentTeam = opponentName;
                    opposingTeam = teamName;
                }
                string range = null;
                if (locationOfInput == "confirmedschedule")
                {
                    range = $"{currentTeam}!H16:H25";
                }
                else if (locationOfInput == "challenge")
                {
                    range = $"{currentTeam}!H10:H12";
                }
                else if (locationOfInput == "confirmedmatch")
                {
                    range = $"{currentTeam}!E16:E25";
                }
                SpreadsheetsResource.ValuesResource.GetRequest request =
                        service.Spreadsheets.Values.Get(googleSheetUrl, range);

                //for sending back information
                string[] teamArray = new string[3];
                int numberOfTeams = 0;
                // Prints the names and igns in spreadsheet:
                try
                {
                    
                    ValueRange response = request.Execute();
                    IList<IList<Object>> values = response.Values;

                    if (values != null && values.Count > 0)
                    {

                        foreach ( var team in values) {
                            numberOfTeams++;
                            
                            Console.WriteLine("sending number of matches from confirm" + values.Count);
                            if (team[0].ToString() == opposingTeam)
                            {
                                Console.WriteLine("made it into if");
                                teamArray[0] = team[0].ToString();
                                teamArray[1] = numberOfTeams.ToString();
                                Console.WriteLine(teamArray[0] + " " + teamArray[1]);
                                
                            }
                           
                        }
                        teamArray[2] = numberOfTeams.ToString();
                        return teamArray;
                    }
                    else
                    {
                        teamArray[0] = "Given team is not listed on your teams, challenge, pending schedule, or upcoming matchs";
                        return teamArray;
                    }
                }
                catch
                {
                    Console.WriteLine("No data found.");
                    teamArray[0] = "Given team is not listed on your teams, challenge, pending schedule, or upcoming matchs";
                    return teamArray;
                }
                
            }
            //getting matchdate for pending schedule or result
            public DateTime GetMatchDate(string teamName, bool isResult, int position)
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

                string range = $"{teamName}!I{15 + position}";
                if (isResult)
                {
                    range = $"{teamName}!F{15 + position}";
                }
                SpreadsheetsResource.ValuesResource.GetRequest request =
                        service.Spreadsheets.Values.Get(googleSheetUrl, range);

                // gets time of match
                
                    ValueRange response = request.Execute();
                    IList<IList<Object>> values = response.Values;

                foreach (var row in values)
                {
                    Console.WriteLine(row[0]);
                    return DateTime.Parse(row[0].ToString());

                }
                return DateTime.Parse("");
            }

            //checks if game is optional by the two given teams based on elo
            public bool Optional(string teamName, string opponentName)
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

                


                // Define request parameters.
                
                string rangeForEloOfTeam = $"{teamName}!B3";
                string rangeForEloOfOpponent = $"{opponentName}!B3";
                


                List<string> ranges = new List<string>();
                ranges.Add(rangeForEloOfTeam);
                ranges.Add(rangeForEloOfOpponent);
                

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

                    var rangeOfYourTeam = data[0].Values[0];
                    var rangeOfEnemyTeam = data[1].Values[0];
                    Console.WriteLine(rangeOfYourTeam[0].ToString() + rangeOfEnemyTeam[0].ToString());

                    int challenger = Int32.Parse(rangeOfYourTeam[0].ToString());
                    int challenged = Int32.Parse(rangeOfEnemyTeam[0].ToString());
                    
                    if(challenger >= (challenged-200) && challenger <= (challenged+100))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    
                    
                   

                }
                catch
                {

                }
                return false;
            }
            //checks if yes or no for oppitional when trying to deny challenge
            public bool OptionalCheck(string teamName, int positionOfTeam, string opponentName, int positionOfOpponent)
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




                // Define request parameters.

                string rangeOfYourOption = $"{teamName}!I{positionOfTeam + 9}";
                string rangeOfOpponentOption = $"{opponentName}!I{positionOfOpponent + 9}";



                List<string> ranges = new List<string>();
                ranges.Add(rangeOfYourOption);
                ranges.Add(rangeOfOpponentOption);


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

                    var opitionOfYourTeam = data[0].Values[0];
                    var opitionOfEnemyTeam = data[1].Values[0];
                    

                    string opYourTeam = opitionOfYourTeam[0].ToString();
                    string opOpponentTeam = opitionOfEnemyTeam[0].ToString();

                    if (opYourTeam == "Y" && opOpponentTeam == "Y")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    

                   

                }
                catch
                {

                }
                return false;
            }


            //method used to issue challenge, create pending schedule, or move to scheduled match
            public string Schedule(string teamName, string opponentName, DateTime pendingSchedule, string locationOfInput)
            {
                Console.WriteLine("made it into PendingSchedule");
                
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

                //names of teams
                string rangeForTeamName1 = $"{teamName}!H16";
                string rangeForTeamName2 = $"{opponentName}!H16";
                int numberOfScheduledGamesTeam1 = CheckScheduleCount(teamName, locationOfInput);
                int numberOfScheduledGamesTeam2 = CheckScheduleCount(opponentName, locationOfInput);
                //checks if over 10 matches in pending or scheduled and stops from scheduling
                if (numberOfScheduledGamesTeam1 >= 10)
                {
                    if (locationOfInput == "confirmedschedule") {
                        return teamName + " has over 10 games in their scheduled matches, please wait to schedule until one has finished.";
                     }
                    else if(locationOfInput == "pendingschedule")
                    {
                        return teamName + " has over 10 games in their pending schedule, please wait to schedule until they have confirmed matches.";
                    }
                }
                if ( numberOfScheduledGamesTeam2 >= 10)
                {
                    if (locationOfInput == "confirmedschedule")
                    {
                        return opponentName + " has over 10 games in their scheduled matches, please wait to schedule until one has finished.";
                    }
                    else if (locationOfInput == "pendingschedule")
                    {
                        return opponentName + " has over 10 games in their pending schedule, please wait to schedule until they have confirmed matches.";
                    }
                }
                if(locationOfInput == "challenge")
                {
                    if(numberOfScheduledGamesTeam1 >= 3)
                    {
                        return teamName + " has 3 pending challenges, please confirm or deny a challenge before challenging more";
                    }
                    else if(numberOfScheduledGamesTeam2 >= 3)
                    {
                        return opponentName + " has 3 pending challenges, they need to confirm or deny a challenge before challenging more";
                    }
                }
                // Defines if pending or scheduled matches

                if (locationOfInput == "confirmedschedule")
                {
                    rangeForTeamName1 = $"{teamName}!E{numberOfScheduledGamesTeam1 + 16}";
                    rangeForTeamName2 = $"{opponentName}!E{numberOfScheduledGamesTeam2 + 16}";
                }
                else if( locationOfInput == "pendingschedule")
                {
                    rangeForTeamName1 = $"{teamName}!H{numberOfScheduledGamesTeam1 + 16}";
                    rangeForTeamName2 = $"{opponentName}!H{numberOfScheduledGamesTeam2 + 16}";
                }
                else if (locationOfInput == "challenge")
                {
                    rangeForTeamName1 =  $"{teamName}!H{numberOfScheduledGamesTeam1 + 10}";
                    rangeForTeamName2 =  $"{opponentName}!H{numberOfScheduledGamesTeam2 + 10}";
                }

                //getting schedule
                string rangeForScheduleDateTeam1 = $"{teamName}!I16";
                string rangeForScheduleDateTeam2 = $"{opponentName}!I16";

                //changes where the schedule is going
                if (locationOfInput == "confirmedschedule")
                {
                    rangeForScheduleDateTeam1 = $"{teamName}!F{numberOfScheduledGamesTeam1 + 16}";
                    rangeForScheduleDateTeam2 = $"{opponentName}!F{numberOfScheduledGamesTeam2 + 16}";
                }
                else if(locationOfInput == "pendingschedule")
                {
                    rangeForScheduleDateTeam1 = $"{teamName}!I{numberOfScheduledGamesTeam1 + 16}";
                    rangeForScheduleDateTeam2 = $"{opponentName}!I{numberOfScheduledGamesTeam2 + 16}";
                }
                else if (locationOfInput == "challenge")
                {
                    rangeForScheduleDateTeam1 = $"{teamName}!I{numberOfScheduledGamesTeam1 + 10}";
                    rangeForScheduleDateTeam2 = $"{opponentName}!I{numberOfScheduledGamesTeam2 + 10}";
                }
               
                // Define request parameters.

                string valueInputOption = "RAW";

                

                var teamList = new string[] { teamName };
                var opponentList = new string[] { opponentName };
                var scheduledDate = new string[] { pendingSchedule.ToString() };


                // checks if challenge is optional or not optional
                bool optional = Optional(teamName, opponentName);
                string[] optionalList = null;
                if (optional)
                {
                    optionalList = new string[] { "N" };
                }
                else
                {
                    optionalList = new string[] { "Y" };
                }

                

                List<Data.ValueRange> data = new List<Data.ValueRange>();
                data.Add(new Data.ValueRange() { Range = rangeForTeamName1, Values = new List<IList<object>> { opponentList } });
                data.Add(new Data.ValueRange() { Range = rangeForTeamName2, Values = new List<IList<object>> { teamList } });
                if (locationOfInput == "challenge")
                {
                    data.Add(new Data.ValueRange() { Range = rangeForScheduleDateTeam1, Values = new List<IList<object>> { optionalList } });
                    data.Add(new Data.ValueRange() { Range = rangeForScheduleDateTeam2, Values = new List<IList<object>> { optionalList } });
                }
                else if( locationOfInput == "confirmedschedule" || locationOfInput == "pendingschedule")
                {
                    data.Add(new Data.ValueRange() { Range = rangeForScheduleDateTeam1, Values = new List<IList<object>> { scheduledDate } });
                    data.Add(new Data.ValueRange() { Range = rangeForScheduleDateTeam2, Values = new List<IList<object>> { scheduledDate } });
                }



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

            public int CheckScheduleCount(string teamName, string locationOfInput)
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

                string range = null;
                if (locationOfInput == "confirmedschedule")
                {
                    range = $"{teamName}!E16:E25";
                }
                else if (locationOfInput == "pendingschedule")
                {
                    range = $"{teamName}!H16:H25";
                }
                else if(locationOfInput == "challenge")
                {
                    range = $"{teamName}!H10:H12";
                }
                SpreadsheetsResource.ValuesResource.GetRequest request =
                        service.Spreadsheets.Values.Get(googleSheetUrl, range);

                // Prints the names and igns in spreadsheet:
                try
                {
                    ValueRange response = request.Execute();
                    IList<IList<Object>> values = response.Values;

                    if (values != null && values.Count > 0)
                    {



                        Console.WriteLine("sending number of matches" + values.Count);

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

            public void InputElo(string teamName, double elo)
            {
                Console.WriteLine("made it into inputelo");
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


                SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum valueInputOption = (SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum)1;  // TODO: Update placeholder value.
                // Define request parameters.
                var elostring = new List<object>() { elo.ToString() };
                Console.WriteLine(elo);
                String range = $"{teamName}!B3";
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
                
                string eloRangeForMainTeam = $"{teamName}!C11:C15";
                string eloRangeForSubTeam = null;
                if (numberOfSubs == 0)
                {
                    eloRangeForSubTeam = $"{teamName}!C19";
                    Console.WriteLine("no subs");
                }
                else if (numberOfSubs == 1)
                {
                    eloRangeForSubTeam = $"{teamName}!C19";
                    Console.WriteLine("one sub");
                }
                else
                {
                    eloRangeForSubTeam = $"{teamName}!C19:C{numberOfSubs + 19}";
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
                    
                    IList<IList<object>> valuesOfSub = null;
                    try
                    {
                        valuesOfSub = data[1].Values;
                    }
                    catch
                    {
                        Console.WriteLine("no subs are written");
                    }
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
                string lastChecked = $"{teamName}!B6";
                string rangeForMainTeam = $"{teamName}!A11:C15";
                string rangeForSubTeam = null;
                if(numberOfSubs == 0)
                {
                    Console.WriteLine("no subs");
                }else
                {
                    rangeForSubTeam = $"{teamName}!A19:C{numberOfSubs + 18}";
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
                    IList<IList<object>> valuesOfSub = null;
                    try
                    {
                        valuesOfSub = data[2].Values;
                    }
                    catch
                    {
                        Console.WriteLine("no subs are written");
                    }
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

                String range = $"{teamName}!B19:B24";
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

                String range = $"{teamName}!B19:B24";
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


            //creates team name
            public void TeamName(string teamName)
            {
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


                SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum valueInputOption = (SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum)1;  // TODO: Update placeholder value.
                // Define request parameters.
                var elostring = new List<object>() { teamName };
                Console.WriteLine("team name added to new sheet");
                String range = $"{teamName}!B2";
                Data.ValueRange requestBody = new Data.ValueRange();
                requestBody.Values = new List<IList<object>> { elostring };
                


                // Prints the names and igns in spreadsheet:
                SpreadsheetsResource.ValuesResource.UpdateRequest request = service.Spreadsheets.Values.Update(requestBody, googleSheetUrl, range);
                request.ValueInputOption = valueInputOption;


                // To execute asynchronously in an async method, replace `request.Execute()` as shown:
                Data.UpdateValuesResponse response = request.Execute();
                // Data.UpdateValuesResponse response = await request.ExecuteAsync();

                // TODO: Change code below to process the `response` object:
                Console.WriteLine(JsonConvert.SerializeObject(response));
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
                        InsertSheetIndex = 8,
                        
                        
                       

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
                Console.WriteLine("after execute");
                

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

                String rangeForIgn = $"{teamName}!B11";
                // Define request parameters.
                if (role == "top")
                {

                    rangeForIgn = $"{teamName}!B11";

                }
                else if (role == "jg")
                {
                    rangeForIgn = $"{teamName}!B12";

                }
                else if (role == "mid")
                {
                    rangeForIgn = $"{teamName}!B13";
                }
                else if (role == "adc")
                {
                    rangeForIgn = $"{teamName}!B14";
                }
                else if (role == "sup")
                {
                    rangeForIgn = $"{teamName}!B15";
                }

                String rangeForRank = $"{teamName}!C11";
                // Define request parameters.
                if (role == "top")
                {

                    rangeForRank = $"{teamName}!C11";

                }
                else if (role == "jg")
                {
                    rangeForRank = $"{teamName}!C12";

                }
                else if (role == "mid")
                {
                    rangeForRank = $"{teamName}!C13";
                }
                else if (role == "adc")
                {
                    rangeForRank = $"{teamName}!C14";
                }
                else if (role == "sup")
                {
                    rangeForRank = $"{teamName}!C15";
                }
                string valueInputOption = "RAW";

                //get date and time to mark last change
                DateTime today = DateTime.UtcNow;
                //range for time
                var rangeForTime = $"{teamName}!B6";


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

                String rangeForIgn = $"{teamName}!B11";
                // Define request parameters.
                if (role == "top")
                {

                    rangeForIgn = $"{teamName}!B11";

                }
                else if (role == "jg")
                {
                    rangeForIgn = $"{teamName}!B12";

                }
                else if (role == "mid")
                {
                    rangeForIgn = $"{teamName}!B13";
                }
                else if (role == "adc")
                {
                    rangeForIgn = $"{teamName}!B14";
                }
                else if (role == "sup")
                {
                    rangeForIgn = $"{teamName}!B15";
                }
                else if (role == "sub")
                {
                    rangeForIgn = $"{teamName}!B{numberOfSubs + 19}";
                }

                
                String rangeForRank = $"{teamName}!C11";
                // Define request parameters.
                if (role == "top")
                {

                    rangeForRank = $"{teamName}!C11";

                }
                else if (role == "jg")
                {
                    rangeForRank = $"{teamName}!C12";

                }
                else if (role == "mid")
                {
                    rangeForRank = $"{teamName}!C13";
                }
                else if (role == "adc")
                {
                    rangeForRank = $"{teamName}!C14";
                }
                else if (role == "sup")
                {
                    rangeForRank = $"{teamName}!C15";
                }
                else if (role == "sub")
                {
                    rangeForRank = $"{teamName}!C{numberOfSubs + 19}";
                    
                }
                string valueInputOption = "RAW";

                //get date and time to mark last change
                DateTime today = DateTime.UtcNow;

                var ignList = new string[] { ign };
                var ignListRank = new string[] { rankOfIgn };
                var dateAndTime = new string[] { today.ToString("f") + " " + today.Kind};
                
                
                //range for time
               var rangeForTime = $"{teamName}!B6";


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

