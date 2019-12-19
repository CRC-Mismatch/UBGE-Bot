using Autofac;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using UBGE_Bot.APIs;
using UBGE_Bot.Carregamento;

namespace UBGE_Bot.Main
{
    public sealed class Program
    {
        public static Program instanciaMain { get; private set; }
        public static UBGEBot_ ubgeBot { get; private set; } = new UBGEBot_();

        public static HttpClient httpClient { get; set; } = new HttpClient();

        public static bool checkDosCanaisFoiIniciado { get; set; }

        public static List<DiscordEmoji> emojisCache = new List<DiscordEmoji>();

        private static async Task Main(string[] args)
        {
            try 
            {
                ContainerBuilder containerBuilder = new ContainerBuilder();
                {
                    containerBuilder.RegisterType<Google_Sheets.Read>().SingleInstance();
                    containerBuilder.RegisterType<Google_Sheets.Write>().SingleInstance();
                }
                ubgeBot.servicesIContainer = containerBuilder.Build();

                instanciaMain = new Program();
                await instanciaMain.ConectarAoDiscordAsync(ubgeBot);
            }
            catch (Exception exception)
            {
                ubgeBot.logExceptionsToDiscord.ExceptionToTxt(exception);
                ShutdownBot();
            }
        }

        private async Task ConectarAoDiscordAsync(UBGEBot_ ubgeBotClient)
        {
            try
            {
                await ubgeBotClient.discordClient.ConnectAsync();
                await Task.Delay(-1);
            }
            catch (Exception exception)
            {
                ubgeBotClient.logExceptionsToDiscord.ExceptionToTxt(exception);
                ShutdownBot();
            }
        }

        public static void ShutdownBot()
            => Environment.Exit(1);
    }
}