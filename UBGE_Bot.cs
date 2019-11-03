using Autofac;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using UBGEBot.Main;
using UBGEBot.LogExceptions;
using UBGEBot.UBGEBotConfig;
using UBGEBot.Utilidades;
using System;
using System.IO;
using System.Reflection;
using MongoDB.Driver;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace UBGEBot.Carregamento
{
    public sealed class UBGEBot_
    {
        public UBGEBotConfig_ ubgeBotConfig { get; private set; }
        public LogExceptionsToDiscord logExceptionsToDiscord { get; private set; }

        public DiscordClient discordClient { get; private set; }
        public CommandsNextExtension commandsNext { get; private set; }
        public InteractivityExtension interactivityExtension { get; private set; }
        
        public GoogleSheetsAPIConfig googleSheetsAPIConfig { get; private set; }
        public ServidoresConfig servidoresConfig { get; private set; }
        public DBConnectionConfig dbConnection { get; private set; }

        public MongoClient mongoClient { get; private set; }
        public MySqlConnection mySqlConnection { get; private set; }

        public IContainer servicesIContainer { get; private set; }

        public UtilidadesGerais utilidadesGerais { get; private set; }

        private string PrefixoMensagens = "[Config]";

        public UBGEBot_()
        {
            try
            {
                ubgeBotConfig = new UBGEBotConfig_();
                logExceptionsToDiscord = new LogExceptionsToDiscord();

                dbConnection = JsonConvert.DeserializeObject<DBConnectionConfig>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\DBConnection.json"));
                servidoresConfig = JsonConvert.DeserializeObject<ServidoresConfig>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\ServidoresConfig.json"));
                googleSheetsAPIConfig = JsonConvert.DeserializeObject<GoogleSheetsAPIConfig>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\GoogleSheetsConfig.json"));
            
                mongoClient = new MongoClient($"mongodb://{dbConnection.MongoDB_IP}:{dbConnection.MongoDB_Port}");
                mySqlConnection = new MySqlConnection($"Server={dbConnection.MySQL_IP};Database={dbConnection.MySQL_Database};Uid={dbConnection.MySQL_Usuario};Pwd={dbConnection.MySQL_Senha}");

                try
                {
                    mySqlConnection.Open();
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{Valores.prefixoBot} {PrefixoMensagens} Não foi possível conectar ao MySQL para pegar dados do rank dos jogadores de Counter-Strike 1.6.");
                    Console.ResetColor();
                }
            
                utilidadesGerais = new UtilidadesGerais();

                discordClient = new DiscordClient(new DiscordConfiguration(ubgeBotConfig.ubgeBotDiscordConfig.Build()));

                commandsNext = discordClient.UseCommandsNext(new CommandsNextConfiguration(ubgeBotConfig.ubgeBotCommandsNextConfig.Build(
                    new ServiceCollection().AddSingleton(this)
                    .AddSingleton(discordClient)
                    .BuildServiceProvider())));
                interactivityExtension = discordClient.UseInteractivity(new InteractivityConfiguration(ubgeBotConfig.ubgeBotInteractivityConfig.Build()));

                commandsNext.RegisterCommands(Assembly.GetEntryAssembly());

                Console.Title = $"UBGE-Bot online! v{Valores.versao_Bot}";
            }
            catch (Exception exception)
            {
                logExceptionsToDiscord.ExceptionToTxt(exception);
                Program.ShutdownBot();
            }
        }
    }
}