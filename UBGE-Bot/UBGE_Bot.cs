using Autofac;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using MongoDB.Driver;
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

                mongoClient = new MongoClient($"mongodb://{ubgeBotConfig.ubgeBotDatabasesConfig.mongoDBIP}:{ubgeBotConfig.ubgeBotDatabasesConfig.mongoDBPorta}");
                mySqlConnection = new MySqlConnection($"Server={ubgeBotConfig.ubgeBotDatabasesConfig.mySQLIP};Database={ubgeBotConfig.ubgeBotDatabasesConfig.mySQLDatabase};Uid={ubgeBotConfig.ubgeBotDatabasesConfig.mySQLUsuario};Pwd={ubgeBotConfig.ubgeBotDatabasesConfig.mySQLSenha}");

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

                Console.Title = $"UBGE-Bot online! v{Valores.versao_Bot}-beta1";
            }
            catch (Exception exception)
            {
                logExceptionsToDiscord.ExceptionToTxt(exception);
                Program.ShutdownBot();
            }
        }
    }
}