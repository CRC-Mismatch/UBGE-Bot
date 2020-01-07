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
    public sealed class GoogleSheets
    {
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string ApplicationName = "UBGE-Bot";

        static UserCredential credential = null;

        public class Read
        {
            public async Task<IList<IList<object>>> LerAPlanilha(string spreadsheetID, string range)
            {
                using (FileStream stream = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\client_secret.json", FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".credentials/sheets.googleapis.com-dotnet-quickstart.json"), true));
                }

                SheetsService service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(spreadsheetID, range);
                IList<IList<object>> values = (await request.ExecuteAsync()).Values;

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
                using (FileStream stream = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\client_secret.json", FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".credentials/sheets.googleapis.com-dotnet-quickstart.json"), true));
                }

                SheetsService service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

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