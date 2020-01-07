using Autofac;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reflection;
using UBGE_Bot.APIs;
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

        public IContainer servicesIContainer { get; private set; }

        public UtilidadesGerais utilidadesGerais { get; private set; }

        public bool botConectadoAoMongo { get; set; } = true;

        public string prefixoMensagens { get; private set; } = "[Config]";
        public string versaoBot { get; private set; } = $"v{Assembly.GetEntryAssembly().GetName().Version.ToString()}-beta5";

        public UBGEBot_()
        {
            try
            {
                ubgeBotConfig = new UBGEBotConfig_();
                ubgeBotConfig.ubgeBotDatabasesConfig = ubgeBotConfig.ubgeBotDatabasesConfig.Build();
                ubgeBotConfig.ubgeBotGoogleAPIConfig = ubgeBotConfig.ubgeBotGoogleAPIConfig.Build();
                ubgeBotConfig.ubgeBotServidoresConfig = ubgeBotConfig.ubgeBotServidoresConfig.Build();
                ubgeBotConfig.ubgeBotValoresConfig = ubgeBotConfig.ubgeBotValoresConfig.Build();

                logExceptionsToDiscord = new LogExceptionsToDiscord();
                utilidadesGerais = new UtilidadesGerais();

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
                catch (Exception)
                {
                    botConectadoAoMongo = false;

                    logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Mongo, "Não foi possível conectar ao MongoDB! Alguns comandos e sistemas podem estar indisponíveis.", prefixoMensagens);
                }

                try
                {
                    mySqlConnection = new MySqlConnection($"Server={ubgeBotConfig.ubgeBotDatabasesConfig.mySQLIP};Database={ubgeBotConfig.ubgeBotDatabasesConfig.mySQLDatabase};Uid={ubgeBotConfig.ubgeBotDatabasesConfig.mySQLUsuario};Pwd={ubgeBotConfig.ubgeBotDatabasesConfig.mySQLSenha}");
                    mySqlConnection.Open();
                }
                catch (Exception)
                {
                    logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.MySQL, "Não foi possível conectar ao MySQL para pegar dados do rank dos jogadores de Counter-Strike 1.6.", prefixoMensagens);
                }

                ContainerBuilder containerBuilder = new ContainerBuilder();
                {
                    containerBuilder.RegisterType<GoogleSheets.Read>().SingleInstance();
                    containerBuilder.RegisterType<GoogleSheets.Write>().SingleInstance();
                    containerBuilder.RegisterType<GoogleDrive.Main>().SingleInstance();
                }
                servicesIContainer = containerBuilder.Build();

                discordClient = new DiscordClient(new DiscordConfiguration(ubgeBotConfig.ubgeBotDiscordConfig.Build()));
                commandsNext = discordClient.UseCommandsNext(new CommandsNextConfiguration(ubgeBotConfig.ubgeBotCommandsNextConfig.Build(
                    new ServiceCollection()
                    .AddSingleton(this)
                    .AddSingleton(discordClient)
                    .BuildServiceProvider(true))));
                interactivityExtension = discordClient.UseInteractivity(new InteractivityConfiguration(ubgeBotConfig.ubgeBotInteractivityConfig.Build()));
                commandsNext.RegisterCommands(Assembly.GetEntryAssembly());

                foreach (Type sistema in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetInterfaces().Contains(typeof(IAplicavelAoCliente))))
                {
                    try
                    {
                        JObject jsonSerialize = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(ubgeBotConfig.ubgeBotValoresConfig));

                        if (jsonSerialize.SelectToken($"sistema{sistema.Name}") != null)
                        {
                            bool.TryParse(jsonSerialize.SelectToken($"sistema{sistema.Name}").ToString(), out bool sistemaAtivo);

                            if (!sistemaAtivo)
                                logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Sistemas, $"O sistema \"{sistema.Name}\" não foi ativo, pois está desabilitado no JSON.");

                            ((IAplicavelAoCliente)Activator.CreateInstance(sistema)).AplicarAoBot(discordClient, botConectadoAoMongo, sistemaAtivo);

                            continue;
                        }

                        ((IAplicavelAoCliente)Activator.CreateInstance(sistema)).AplicarAoBot(discordClient, botConectadoAoMongo, true);
                    }
                    catch (Exception exception)
                    {
                        logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Sistemas, $"O sistema: \"{sistema.Name}\" não foi iniciado! Exceção: {exception.Message}", prefixoMensagens);
                    }
                }

                Console.Title = $"UBGE-Bot online! {versaoBot}";
            }
            catch (Exception exception)
            {
                logExceptionsToDiscord.ExceptionToTxt(exception);

                Program.DesligarBot();
            }
        }
    }
}