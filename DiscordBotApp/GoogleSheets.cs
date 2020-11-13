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

            public IList<IList<Object>> Google( string teamName)
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

                String range = $"{teamName}!A2:B6";
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
                        string[,] teamArray = new string[5, 2];
                        foreach (var row in values)
                        {
                            int num = 0;
                            // Print columns A and E, which correspond to indices 0 and 4.
                            Console.WriteLine("{0}, {1}", row[0], row[1]);
                            teamArray[num, 0] = row[0].ToString();
                            num++;
                            

                        }
                        Console.WriteLine("sending" + values);
                        Console.WriteLine(teamArray[0, 0]);
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
                string valueInputOption = "USER_ENTERED";

                var ignList = new string[] { ign };
                var ignListRank = new string[]  { rankOfIgn } ;


                List<Data.ValueRange> data = new List<Data.ValueRange>();
                data.Add(new Data.ValueRange() { Range = rangeForIgn, Values = new List<IList<object>>{ ignList } });
                data.Add(new Data.ValueRange() { Range = rangeForRank, Values = new List<IList<object>> { ignListRank } });



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

