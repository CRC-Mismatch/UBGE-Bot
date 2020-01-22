using DSharpPlus.Entities;
using Ionic.Zip;
using System;
using Timer = System.Timers.Timer;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Log = UBGE.Logger.Logger;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UBGE.MongoDB.Models;
using UBGE.Services.Google;
using UBGE.Utilities;
using DSharpPlus;

namespace UBGE
{
    public static class Program
    {
        public static UBGE_Bot Bot { get; private set; } = new UBGE_Bot();
        
        static async Task Main(string[] args)
        {
            if (!PCIsConnected())
            {
                Bot.Logger.Error(Log.TypeError.PC, "Foi detectado que este computador não está conectado a Internet! Aguardando uma conexão para conectar o bot ao Discord...");
                
                await CheckInternetAsync(Bot);

                return;
            }

            var timerAutoUpdater = new Timer()
            {
                Interval = TimeSpan.FromSeconds(20).TotalMilliseconds,
            };
            timerAutoUpdater.Elapsed += async delegate
            {
                if (Bot.GuildsDownloadWasCompleted)
                    await AutoUpdater(Bot);
            };
            timerAutoUpdater.Start();

            var timerModuloChecarBotAberto = new Timer()
            {
                Interval = 10000,
            };
            timerModuloChecarBotAberto.Elapsed += async delegate
            {
                if (Bot.ConnectedToMongo)
                    await CheckIfTheBotIsOpen(Bot);
            };
            timerModuloChecarBotAberto.Start();

            var checaMembrosNaPrisao = new Timer()
            {
                Interval = 30000,
            };
            checaMembrosNaPrisao.Elapsed += async delegate
            {
                if (Bot.ChannelsCheckWasStarted)
                    await CheckPrisionsWhenTheBotIsOff(Bot);
            };
            checaMembrosNaPrisao.Start();

            var timerServidores = new Timer()
            {
                Interval = 15000,
            };
            timerServidores.Elapsed += async delegate
            {
                if (Bot.ConnectedToMongo)
                    await SearchServersProjectReality(Bot);
            };
            timerServidores.Start();

            var timerDownloadFilesForMongo = new Timer()
            {
                Interval = TimeSpan.FromMinutes(5).TotalMilliseconds,
            };
            timerDownloadFilesForMongo.Elapsed += async delegate
            {
                if (Bot.GuildsDownloadWasCompleted)
                    await MongoDownloadFiles(Bot);
            };
            timerDownloadFilesForMongo.Start();

            //var timerCheckIfCanFinishTheVotingOfPresence = new Timer()
            //{
            //    Interval = TimeSpan.FromSeconds(30).TotalMilliseconds,
            //};
            //timerCheckIfCanFinishTheVotingOfPresence.Elapsed += async delegate
            //{
            //    if (Bot.ConnectedToMongo)
            //        await CheckIfThePollIsFinished(Bot);
            //};
            //timerCheckIfCanFinishTheVotingOfPresence.Start();
         
            //var timerToCheckTimeOfReunion = new Timer()
            //{
            //    Interval = TimeSpan.FromSeconds(30).TotalMilliseconds
            //};
            //timerToCheckTimeOfReunion.Elapsed += async delegate
            //{
            //    if (Bot.ConnectedToMongo)
            //        await CheckTimeOfReunion(Bot);
            //};
            //timerToCheckTimeOfReunion.Start();

            await ConnectDiscordAsync(Bot);
        }

        static async Task CheckInternetAsync(UBGE_Bot bot)
        {
            while (true)
            {
                if (!PCIsConnected())
                    await Task.Delay(TimeSpan.FromSeconds(5));
                else
                {
                    bot.Logger.Warning(Log.TypeWarning.PC, "Foi detectado que este computador se conectou a Internet, conectando o bot ao Discord...");

                    await ConnectDiscordAsync(bot);

                    bot.Logger.Warning(Log.TypeWarning.PC, "O bot foi conectado com sucesso!");

                    break;
                }
            }
        }

        static async Task ConnectDiscordAsync(UBGE_Bot bot)
        {
            await bot.DiscordClient.ConnectAsync();

            await Task.Delay(-1);
        }

        [DllImport("wininet.dll")]
        extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        public static bool PCIsConnected() => InternetGetConnectedState(out int _, 0);

        static async Task AutoUpdater(UBGE_Bot bot)
        {
            string gitHubTxtVersion = await Bot.HttpClient.GetStringAsync("https://raw.githubusercontent.com/LuizFernandoNB/UBGE-Bot/master/VERSION.txt");

            int.TryParse(gitHubTxtVersion.Replace(".", "").Replace("-beta", ""), out int gitHubVersion);
            int.TryParse(bot.BOT_VERSION.Replace("v", "").Replace(".", "").Replace("-beta", ""), out int botVersion);

            var logger = bot.Logger;

            if (gitHubVersion > botVersion)
            {
                logger.Warning(Log.TypeWarning.Systems, $"Foi encontrada uma nova versão do bot! Versão: v{gitHubTxtVersion}, fazendo as devidas atualizações...", "AutoUpdater");
                await logger.EmbedLogMessages(Log.TypeEmbed.Systems, "Uma nova versão do bot foi encontrada!", $"Versão: `v{gitHubTxtVersion}`");

                string caminhoDownloadZip = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                var GoogleDrive = new GoogleDriveService();

                using (var servicoDrive = GoogleDrive.ServicoDoDrive(await GoogleDrive.Autenticar()))
                {
                    var arquivoZip = await GoogleDrive.ProcurarArquivo(servicoDrive, "UBGE-Bot.zip", false);

                    using (var fs = new FileStream(caminhoDownloadZip + $@"\{gitHubTxtVersion}.zip", FileMode.Create, FileAccess.Write))
                        await servicoDrive.Files.Get(arquivoZip.FirstOrDefault()).DownloadAsync(fs);
                }

                using (var zip = new ZipFile(caminhoDownloadZip + $@"\{gitHubTxtVersion}.zip", Encoding.UTF8))
                    zip.ExtractAll(caminhoDownloadZip + $@"\{gitHubTxtVersion}", ExtractExistingFileAction.OverwriteSilently);

                logger.Warning(Log.TypeWarning.Systems, $"A nova versão foi baixada, extraída e está pronta para o uso! Reiniciando o bot para iniciar a nova versão...", "AutoUpdater");
                await logger.EmbedLogMessages(Log.TypeEmbed.Warning, "A nova versão foi baixada, extraída e está pronta para o uso! Reiniciando o bot para iniciar a nova versão...", $":smile:");

                Process.Start(caminhoDownloadZip + $@"\{gitHubTxtVersion}\publish\UBGE-Bot.exe");

                ShutdownBot();
            }
        }
        
        public static async Task MongoDownloadFiles(UBGE_Bot bot)
        {
            try
            {
                string diretorioBot = Directory.GetCurrentDirectory();

                var logger = bot.Logger;

                if (!File.Exists(diretorioBot + @"\mongoimport.exe") || !File.Exists(diretorioBot + @"\mongoexport.exe") || !File.Exists(diretorioBot + @"\libeay32.dll") || !File.Exists(diretorioBot + @"\ssleay32.dll"))
                {
                    await logger.EmbedLogMessages(Log.TypeEmbed.Warning, "Os arquivos de funcionamento dos comandos do Mongo não foram encontrados, fazendo o download dos mesmos...", bot.Utilities.FindEmoji(bot.DiscordClient, ":UBGE:"));

                    logger.Warning(Log.TypeWarning.Systems, "Os arquivos de funcionamento dos comandos do Mongo não foram encontrados, fazendo o download dos mesmos...");
                }
                else
                    return;

                var GoogleDrive = new GoogleDriveService();

                using (var servico = GoogleDrive.ServicoDoDrive(await GoogleDrive.Autenticar()))
                {
                    string[] ids = await GoogleDrive.ProcurarArquivo(servico, "Mongo.zip", false);

                    if (ids != null && ids.Any())
                    {
                        using (var stream = new FileStream(diretorioBot + @"\Mongo.zip", FileMode.Create, FileAccess.Write))
                            await servico.Files.Get(ids.FirstOrDefault()).DownloadAsync(stream);
                    }
                }

                using (var Zip = new ZipFile(diretorioBot + @"\Mongo.zip"))
                    Zip.ExtractAll(diretorioBot + @"\Mongo", ExtractExistingFileAction.OverwriteSilently);

                File.Move(diretorioBot + @"\Mongo\mongoexport.exe", diretorioBot + @"\mongoexport.exe", true);
                File.Move(diretorioBot + @"\Mongo\mongoimport.exe", diretorioBot + @"\mongoimport.exe", true);
                File.Move(diretorioBot + @"\Mongo\libeay32.dll", diretorioBot + @"\libeay32.dll", true);
                File.Move(diretorioBot + @"\Mongo\ssleay32.dll", diretorioBot + @"\ssleay32.dll", true);

                Directory.Delete(diretorioBot + @"\Mongo");
                File.Delete(diretorioBot + @"\Mongo.zip");

                await logger.EmbedLogMessages(Log.TypeEmbed.Warning, "Os arquivos foram baixados e extraídos com sucesso!");

                logger.Warning(Log.TypeWarning.Systems, "Os arquivos foram baixados e extraídos com sucesso!");
            }
            catch (Exception) { }
        }

        static async Task CheckIfTheBotIsOpen(UBGE_Bot bot)
        {
            var collectionCheckBotAberto = bot.LocalDB.GetCollection<CheckBotAberto>(Values.Mongo.checkBotAberto);
            var filtro = Builders<CheckBotAberto>.Filter.Empty;
            var lista = await (await collectionCheckBotAberto.FindAsync<CheckBotAberto>(filtro)).ToListAsync();

            if (lista.Count == 0)
                await collectionCheckBotAberto.InsertOneAsync(new CheckBotAberto { numero = 0, diaEHora = DateTime.Now });
            else
                await collectionCheckBotAberto.UpdateOneAsync(filtro, Builders<CheckBotAberto>.Update.Set(x => x.numero, 0).Set(y => y.diaEHora, DateTime.Now));
        }

        static async Task CheckPrisionsWhenTheBotIsOff(UBGE_Bot bot)
        {
            var logger = bot.Logger;
            var discord = bot.DiscordClient;

            try
            {
                var collectionInfracoes = bot.LocalDB.GetCollection<Infracao>(Values.Mongo.infracoes);

                var guildUBGE = await discord.GetGuildAsync(Values.Guilds.guildUBGE);
                DiscordRole prisioneiroCargo = guildUBGE.GetRole(Values.Roles.rolePrisioneiro), cargosMembroForeach = null;
                var ubgeBot = guildUBGE.GetChannel(Values.Chats.channelUBGEBot);

                var embed = new DiscordEmbedBuilder();

                var strCargos = new StringBuilder();

                foreach (var membroPrisao in guildUBGE.Members.Values.Where(x => x.Roles.Contains(prisioneiroCargo)))
                {
                    var filtro = Builders<Infracao>.Filter.Eq(x => x.idInfrator, membroPrisao.Id);
                    var listaInfracoes = await (await collectionInfracoes.FindAsync(filtro)).ToListAsync();

                    if (listaInfracoes.Count == 0 || !listaInfracoes.LastOrDefault().oMembroFoiPreso)
                        continue;

                    var ultimaPrisao = listaInfracoes.LastOrDefault();

                    if (string.IsNullOrWhiteSpace(ultimaPrisao.dadosPrisao?.tempoDoMembroNaPrisao))
                        continue;

                    if (DateTime.Parse(ultimaPrisao.dataInfracao.ToString()).Add(bot.Utilities.ConvertTime(ultimaPrisao.dadosPrisao?.tempoDoMembroNaPrisao)) < DateTime.Now)
                    {
                        await membroPrisao.RevokeRoleAsync(prisioneiroCargo);

                        foreach (ulong cargos in ultimaPrisao.dadosPrisao.cargosDoMembro)
                        {
                            cargosMembroForeach = guildUBGE.GetRole(cargos);
                            await membroPrisao.GrantRoleAsync(cargosMembroForeach);

                            strCargos.Append($"{cargosMembroForeach.Mention} | ");
                        }

                        embed.WithAuthor($"O membro: \"{bot.Utilities.DiscordNick(membroPrisao)}#{membroPrisao.Discriminator}\", saiu da prisão.", null, Values.logoUBGE)
                            .WithColor(bot.Utilities.HelpCommandsColor())
                            .WithDescription($"Cargos devolvidos: {strCargos.ToString()}")
                            .WithThumbnailUrl(membroPrisao.AvatarUrl)
                            .WithTimestamp(DateTime.Now)
                            .WithFooter($"Comando requisitado pelo: {bot.Utilities.DiscordNick(membroPrisao)}", iconUrl: membroPrisao.AvatarUrl);

                        logger.Warning(Log.TypeWarning.Discord, $"O membro: {bot.Utilities.DiscordNick(membroPrisao)}#{membroPrisao.Discriminator}, saiu da prisão.");
                        await ubgeBot.SendMessageAsync(embed: embed.Build());
                    }
                }
            }
            catch (Exception exception)
            {
                await logger.Error(Log.TypeError.Systems, exception);
            }
        }

        static async Task SearchServersProjectReality(UBGE_Bot bot)
        {
            var logger = bot.Logger;

            try
            {
                var resposta = (JObject)JsonConvert.DeserializeObject(await bot.HttpClient.GetStringAsync("https://www.realitymod.com/prspy/json/serverdata.json"));
                var listaResposta = (JArray)resposta.SelectToken("Data");

                var servidoresDB = bot.LocalDB.GetCollection<ServidoresUBGE>(Values.Mongo.servidoresUBGE);

                var filtro = Builders<ServidoresUBGE>.Filter.Eq(x => x.jogo, "pr");
                var resultadosLista = await (await servidoresDB.FindAsync(filtro)).ToListAsync();

                if (resultadosLista.Count > 0)
                    await servidoresDB.DeleteManyAsync(filtro);

                int N = 0;

                foreach (var server in listaResposta)
                {
                    string ipServidor = server.SelectToken("IPAddress").ToString(),
                    queryPort = server.SelectToken("QueryPort").ToString(),
                    paisServidor = server.SelectToken("Country").ToString(),
                    nomeServidor = server.SelectToken("ServerName").ToString(),
                    nomeJogo = server.SelectToken("GameName").ToString(),
                    versaoJogo = server.SelectToken("GameVersion").ToString(),
                    jogoPorta = server.SelectToken("GamePort").ToString(),
                    nomeMapa = server.SelectToken("MapName").ToString(),
                    modoDeJogo = server.SelectToken("GameMode").ToString(),
                    playersJogando = server.SelectToken("NumPlayers").ToString(),
                    maxPlayers = server.SelectToken("MaxPlayers").ToString(),
                    statusJogo = server.SelectToken("GameStatus").ToString();

                    if (int.Parse(playersJogando.ToString()) != 0)
                    {
                        await servidoresDB.InsertOneAsync(new ServidoresUBGE
                        {
                            jogo = "pr",
                            jogadoresDoServidor = playersJogando,
                            mapaDoServidor = nomeMapa,
                            maximoDePlayers = maxPlayers,
                            modoDeJogo = modoDeJogo,
                            paisDoServidor = paisServidor,
                            versaoDoJogo = versaoJogo,
                            nomeDoServidor = nomeServidor,
                            fotoDoServidor = Values.prLogoSecretary,
                            ipDoServidor = ipServidor,
                            portaDoServidor = $"{queryPort} ou {jogoPorta}",
                            statusDoServidor = statusJogo,
                            thumbnailDoServidor = Values.logoUBGE,
                            servidorDisponivel = $"Project Reality (PR) = `servidores pr`",
                            nomeServidorParaComando = "Project Reality",
                            _id = new ObjectId()
                        });

                        ++N;
                    }
                }
            }
            catch (Exception)
            {
                logger.Warning(Log.TypeWarning.Servers, $"[ProjectReality-Servidores] A busca dos servidores Project Reality gerou um erro! Tudo normal, ainda continuo procurando...");
            }
        }

        static async Task CheckIfThePollIsFinished(UBGE_Bot bot)
        {
            var collectionReunion = bot.LocalDB.GetCollection<Reunion>(Values.Mongo.reunion);
            var filtroReunion = Builders<Reunion>.Filter.Eq(x => x.ReunionIsFinished, false);
            var respostaReunion = await (await collectionReunion.FindAsync(filtroReunion)).ToListAsync();

            if (respostaReunion.Count == 0)
                return;

            var ultimaRespostaReunion = respostaReunion.LastOrDefault();

            if (ultimaRespostaReunion.LastDayToMarkThePresenceReaction < DateTime.Now)
            {
                await collectionReunion.UpdateOneAsync(filtroReunion, Builders<Reunion>.Update.Set(x => x.ReunionIsFinished, true));
            }
        }

        static async Task CheckTimeOfReunion(UBGE_Bot bot)
        {
            var collectionReunion = bot.LocalDB.GetCollection<Reunion>(Values.Mongo.reunion);
            var filtroReunion = Builders<Reunion>.Filter.Eq(x => x.ReunionIsFinished, false);
            var respostaReunion = await (await collectionReunion.FindAsync(filtroReunion)).ToListAsync();

            if (respostaReunion.Count == 0)
                return;

            var ultimaRespostaReunion = respostaReunion.LastOrDefault();

            var guildUBGE = await bot.DiscordClient.GetGuildAsync(Values.Guilds.guildUBGE);
            var channelAnunciosConselho = guildUBGE.GetChannel(Values.Chats.channelAnunciosConselho);
            var roleConselheiros = guildUBGE.GetRole(Values.Roles.roleConselheiro);

            Uri.TryCreate(ultimaRespostaReunion.LinkOfMessage, UriKind.RelativeOrAbsolute, out var messageUri);

            var subtractReunion = ultimaRespostaReunion.DayOfReunion - DateTime.Now;
            var hoursReunion = (int)subtractReunion.TotalHours;
            var minutesReunion = (int)subtractReunion.TotalMinutes;

            if (hoursReunion == 24)
                await channelAnunciosConselho.SendMessageAsync($"{roleConselheiros.Mention}, atenção! A reunião é daqui a 1 dia! {Formatter.MaskedUrl("Clique aqui", messageUri, "Clique aqui")} para o Discord lhe redirecionar para a mensagem que contêm as pautas desta reunião.");
            else if (hoursReunion == 1)
                await channelAnunciosConselho.SendMessageAsync($"{roleConselheiros.Mention}, a reunião começa em 1 hora! {Formatter.MaskedUrl("Clique aqui", messageUri, "Clique aqui")} para ver as pautas da reunião.");
            else if (minutesReunion == 1)
                await channelAnunciosConselho.SendMessageAsync($"{roleConselheiros.Mention}, a reunião está começando.");
            else
                return;
        }

        public static void ShutdownBot() => Environment.Exit(1);

        public static void RestartBot()
        {
            Process.Start(Directory.GetCurrentDirectory() + @"\UBGE-Bot.exe");

            ShutdownBot();
        }
    }
}