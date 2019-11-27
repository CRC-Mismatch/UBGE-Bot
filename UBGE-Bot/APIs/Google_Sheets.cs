using DSharpPlus.Entities;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace UBGE_Bot.APIs
{
    public sealed class Google_Sheets
    {
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "UBGE-Bot";

        static UserCredential credential;

        public class Read
        {
            public async Task<IList<IList<object>>> LerAPlanilha(string spreadsheetID, string range)
            {
                using (var stream = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\client_secret.json", FileMode.Open, FileAccess.Read))
                {
                    string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-quickstart.json");

                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true));
                }

                SheetsService service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(spreadsheetID, range);
                var values = (await request.ExecuteAsync()).Values;

                if (values != null && values.Count > 0)
                    return values;
                else
                    return null;
            }
        }

        public class Write
        {
            public async Task EscrevePlanilhaDoCenso(string spreadsheet, string range, double timestamp, string nomeDiscord, int idade, string estado, string email, string idiomas, string comoChegouAUBGE, string jogosMaisJogados)
            {
                using (var stream = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\client_secret.json", FileMode.Open, FileAccess.Read))
                {
                    string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-quickstart.json");

                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true));
                }

                SheetsService service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(spreadsheet, range);
                var values = (await request.ExecuteAsync()).Values;

                IList<object> obj = new List<object>();
                obj.Add(timestamp);
                obj.Add(nomeDiscord);
                obj.Add(idade);
                obj.Add(estado);
                obj.Add(email);
                obj.Add(idiomas);
                obj.Add(comoChegouAUBGE);
                obj.Add(jogosMaisJogados);

                IList<IList<object>> ListaFinal = new List<IList<object>>();
                ListaFinal.Add(obj);

                SpreadsheetsResource.ValuesResource.AppendRequest request2 = service.Spreadsheets.Values.Append(new ValueRange() { Values = ListaFinal }, spreadsheet, range);
                request2.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
                request2.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;

                await request2.ExecuteAsync();
            }
        }
    }
}
