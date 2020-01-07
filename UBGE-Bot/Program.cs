using System;
using System.Threading.Tasks;
using System.Net.Http;
using UBGE_Bot.Carregamento;

namespace UBGE_Bot.Main
{
    public sealed class Program
    {
        public static Program instanciaMain { get; private set; }
        public static UBGEBot_ ubgeBot { get; private set; } = new UBGEBot_();

        public static HttpClient httpClient { get; set; } = new HttpClient();
        public static bool checkDosCanaisFoiIniciado { get; set; }

        private static async Task Main()
        {
            instanciaMain = new Program();

            await instanciaMain.ConectarAoDiscordAsync(ubgeBot);
        }

        private async Task ConectarAoDiscordAsync(UBGEBot_ ubgeBotClient)
        {
            await ubgeBotClient.discordClient.ConnectAsync();
            await Task.Delay(-1);
        }

        public static void DesligarBot()
            => Environment.Exit(1);
    }
}