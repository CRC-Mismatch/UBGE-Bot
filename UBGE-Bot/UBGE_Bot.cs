using Autofac;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using System;
using System.Reflection;
using UBGE_Bot.Main;
using UBGE_Bot.LogExceptions;
using UBGE_Bot.UBGEBotConfig;
using UBGE_Bot.Utilidades;

namespace UBGE_Bot.Carregamento
{
    public sealed class UBGEBot_
    {
        public UBGEBotConfig_ ubgeBotConfig { get; private set; }
        public LogExceptionsToDiscord logExceptionsToDiscord { get; private set; }

        public DiscordClient discordClient { get; private set; }
        public CommandsNextExtension commandsNext { get; private set; }
        public InteractivityExtension interactivityExtension { get; private set; }

        public MongoClient mongoClient { get; private set; }
        public IMongoDatabase localDB { get; private set; }
        public MySqlConnection mySqlConnection { get; private set; }

        public IContainer servicesIContainer { get; set; }

        public UtilidadesGerais utilidadesGerais { get; private set; }
        
        public bool botConectadoAoMongo { get; set; } = true;

        public string prefixoMensagens { get; private set; } = "[Config]";
        public string prefixoBotConsole { get; private set; } = "[UBGE-Bot]";
        public string versaoBot { get; private set; } = $"v{Assembly.GetEntryAssembly().GetName().Version.ToString()}-beta4";

        public UBGEBot_()
        {
            try
            {
                ubgeBotConfig = new UBGEBotConfig_();
                logExceptionsToDiscord = new LogExceptionsToDiscord();

                try
                {
                    mongoClient = new MongoClient(new MongoClientSettings 
                    { 
                        Server = new MongoServerAddress(ubgeBotConfig.ubgeBotDatabasesConfig.mongoDBIP, int.Parse(ubgeBotConfig.ubgeBotDatabasesConfig.mongoDBPorta)),
                        ConnectTimeout = TimeSpan.FromSeconds(10),
                    });
                    
                    localDB = mongoClient.GetDatabase(Valores.Mongo.local);
                    
                    localDB.RunCommand<BsonDocument>(new BsonDocument("ping", 1));
                }
                catch (TimeoutException)
                {
                    botConectadoAoMongo = false;

                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{logExceptionsToDiscord.RetornaDataAtualParecidoComODSharpPlus()} {prefixoBotConsole} [Erro] {prefixoMensagens} Não foi possível conectar ao MongoDB! Alguns comandos podem estar indisponíveis.");
                    Console.ResetColor();
                }
                
                mySqlConnection = new MySqlConnection($"Server={ubgeBotConfig.ubgeBotDatabasesConfig.mySQLIP};Database={ubgeBotConfig.ubgeBotDatabasesConfig.mySQLDatabase};Uid={ubgeBotConfig.ubgeBotDatabasesConfig.mySQLUsuario};Pwd={ubgeBotConfig.ubgeBotDatabasesConfig.mySQLSenha}");

                try
                {
                    mySqlConnection.Open();
                }
                catch (Exception)
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{logExceptionsToDiscord.RetornaDataAtualParecidoComODSharpPlus()} {prefixoBotConsole} [Erro] {prefixoMensagens} Não foi possível conectar ao MySQL para pegar dados do rank dos jogadores de Counter-Strike 1.6.");
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

                Console.Title = $"UBGE-Bot online! {versaoBot}";
            }
            catch (Exception exception)
            {
                logExceptionsToDiscord.ExceptionToTxt(exception);
                Program.ShutdownBot();
            }
        }
    }
}