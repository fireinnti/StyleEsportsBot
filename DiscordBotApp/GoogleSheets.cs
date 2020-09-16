using System.Configuration;
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

namespace DiscordBotApp
{
    namespace GoogleSheets
    {
        public class Sheets
        {

            // If modifying these scopes, delete your previously saved credentials
            // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
            static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
            static string ApplicationName = "Style Esports Bot";

            public IList<IList<Object>> Google()
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

                String range = "The Syndicate!A2:B";
                SpreadsheetsResource.ValuesResource.GetRequest request =
                        service.Spreadsheets.Values.Get(googleSheetUrl, range);

                // Prints the names and igns in spreadsheet:

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
                else
                {
                    Console.WriteLine("No data found.");
                    return null;
                }
                Console.Read();
            }
        }
    }
}
