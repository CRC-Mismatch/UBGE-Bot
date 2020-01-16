using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UBGE.Services.Google
{
    public sealed class GoogleSheetsService
    {
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string ApplicationName = "UBGE-Bot";

        static UserCredential credential = null;

        public async Task<IList<IList<object>>> ReadSheet(string spreadsheetID, string range)
        {
            using (var stream = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\client_secret.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".credentials/sheets.googleapis.com-dotnet-quickstart.json"), true));
            }

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var values = (await service.Spreadsheets.Values.Get(spreadsheetID, range).ExecuteAsync()).Values;

            return values != null && values.Count != 0 ? values : null;
        }

        public async Task WriteOnSheetCensus(string spreadsheet, string range, double timestamp, string nomeDiscord, int idade, string estado, string email, string idiomas, string comoChegouAUBGE, string jogosMaisJogados)
        {
            using (var stream = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\client_secret.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".credentials/sheets.googleapis.com-dotnet-quickstart.json"), true));
            }

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var obj = new List<object>()
            {
                timestamp,
                nomeDiscord,
                idade,
                estado,
                email,
                idiomas,
                comoChegouAUBGE,
                jogosMaisJogados
            };

            var request = service.Spreadsheets.Values.Append(new ValueRange() { Values = new List<IList<object>>() { obj } }, spreadsheet, range);
            request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;

            await request.ExecuteAsync();
        }
    }
}