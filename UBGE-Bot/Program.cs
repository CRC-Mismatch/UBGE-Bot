using Autofac;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Text;
using System.Net.Http;
using Timer = System.Timers.Timer;
using UBGE_Bot.APIs;
using UBGE_Bot.Carregamento;
using UBGE_Bot.Utilidades;
using UBGE_Bot.MongoDB.Modelos;
using UBGE_Bot.LogExceptions;

namespace UBGE_Bot.Main
{
    public sealed class Program
    {
        public static Program instanciaMain { get; private set; }
        public static UBGEBot_ ubgeBot { get; private set; } = new UBGEBot_();

        public static HttpClient httpClientMain { get; private set; } = new HttpClient();

        private static bool checkDosCanaisFoiIniciado = false;

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

                Timer timerServidores = new Timer()
                {
                    Interval = 10000,
                };
                timerServidores.Elapsed += async delegate
                {
                    if (ubgeBot.botConectadoAoMongo)
                    {
                        await BuscaServidoresPR(ubgeBot, httpClientMain);
                        await BuscaServidoresConanExiles(ubgeBot, httpClientMain);
                        await BuscaServidoresCounterStrike(ubgeBot, httpClientMain);
                        await BuscaServidoresDayZ(ubgeBot, httpClientMain);
                        await BuscaServidoresMordhau(ubgeBot, httpClientMain);
                        await BuscaServidoresOpenSpades(ubgeBot, httpClientMain);
                        await BuscaServidoresUnturned(ubgeBot, httpClientMain);
                    }
                };
                timerServidores.Start();

                Timer timerComandos = new Timer()
                {
                    Interval = 43200000, //1 dia = 86400000ms | 12 horas = 43200000ms
                };
                timerComandos.Elapsed += async delegate
                {
                    if (ubgeBot.botConectadoAoMongo)
                        await ExecutaList(ubgeBot, "Executando o **//list** automaticamente...");
                };
                //timerComandos.Start();

                Timer timerModuloChecarBotAberto = new Timer()
                {
                    Interval = 10000,
                };
                timerModuloChecarBotAberto.Elapsed += async delegate
                {
                    if (ubgeBot.botConectadoAoMongo)
                        await ModuloBotAberto(ubgeBot);
                };
                timerModuloChecarBotAberto.Start();

                Timer checaCanaisAutoCreate = new Timer()
                {
                    Interval = 30000,
                };
                timerModuloChecarBotAberto.Elapsed += async delegate
                {
                    if (checkDosCanaisFoiIniciado) 
                        await ChecaCanaisAutoCreate(ubgeBot);
                };
                timerModuloChecarBotAberto.Start();

                Timer checaMembrosNaPrisao = new Timer()
                {
                    Interval = 30000,
                };
                checaMembrosNaPrisao.Elapsed += async delegate
                {
                    if (checkDosCanaisFoiIniciado) 
                        await VerificaPrisoesQuandoOBotInicia(ubgeBot);
                };
                checaMembrosNaPrisao.Start();

                if (ubgeBot.botConectadoAoMongo)
                {
                    await EnviaDadosDiarios(ubgeBot);
                    //await ChecaSeFazAEleicaoDeSecretarioLider(ubgeBot);
                }

                instanciaMain = new Program(ubgeBot);
                await instanciaMain.ConectarEReconectarBotAoDiscordAsync(ubgeBot, false);
            }
            catch (Exception exception)
            {
                ubgeBot.logExceptionsToDiscord.ExceptionToTxt(exception);
                ShutdownBot();
            }
        }

        public Program(UBGEBot_ ubgeBotClient)
        {
            try
            {
                ubgeBotClient.discordClient.Ready += DiscordIniciado;
                ubgeBotClient.discordClient.GuildBanAdded += NovoBan;
                ubgeBotClient.discordClient.GuildBanRemoved += BanRetirado;
                ubgeBotClient.discordClient.SocketClosed += BotCaiu;
                ubgeBotClient.discordClient.SocketErrored += BotCaiuEErroNoSocket;
                ubgeBotClient.discordClient.GuildMemberUpdated += MembroAlterado;
                ubgeBotClient.discordClient.GuildDownloadCompleted += DownloadsDosServidoresFoiConcluido;
                
                if (ubgeBotClient.botConectadoAoMongo)
                {
                    ubgeBotClient.discordClient.MessageReactionAdded += ReacaoAdicionadaReactRole;
                    ubgeBotClient.discordClient.MessageReactionRemoved += ReacaoRemovidaReactRole;
                    ubgeBotClient.discordClient.MessageCreated += MensagemCriada;
                    ubgeBotClient.discordClient.VoiceStateUpdated += CanalDeVozPersonalizado;
                    ubgeBotClient.discordClient.GuildMemberAdded += MembroEntra;
                }
            }
            catch (Exception exception)
            {
                ubgeBotClient.logExceptionsToDiscord.ExceptionToTxt(exception);
                ShutdownBot();
            }
        }

        public async Task DiscordIniciado(ReadyEventArgs readyEventArgs)
            => await readyEventArgs.Client.UpdateStatusAsync(new DiscordActivity { Name = "Bem-Vindo a UBGE!", ActivityType = ActivityType.Playing });

        public async Task BotCaiu(SocketCloseEventArgs socketCloseEventArgs) 
            => await ConectarEReconectarBotAoDiscordAsync(ubgeBot, true);

        public async Task BotCaiuEErroNoSocket(SocketErrorEventArgs socketErrorEventArgs) 
            => await ConectarEReconectarBotAoDiscordAsync(ubgeBot, true);
        
        public async Task DownloadsDosServidoresFoiConcluido(GuildDownloadCompletedEventArgs guildDownloadCompletedEventArgs) 
        {
            if (ubgeBot.botConectadoAoMongo)
            {
                await CheckReacoesMarcadasQuandoOBotEstavaOfflineNoReactRole(ubgeBot);
                await FazOCacheDosEmojis(ubgeBot);
                await ChecaCanaisAutoCreate(ubgeBot);
            }

            await EnviaMensagemPraODiscordDeConexaoNoMongo(ubgeBot);
        }
        


        public async Task ReacaoAdicionadaReactRole(MessageReactionAddEventArgs messageReactionAddEventArgs)
        {
            if (messageReactionAddEventArgs == null || 
                messageReactionAddEventArgs.Channel.IsPrivate || 
                messageReactionAddEventArgs.User.IsBot)
                return;

            await Task.Delay(200);

            new Thread(async () => 
            {
                try
                {
                    var db = ubgeBot.localDB;
                    
                    var jogos = db.GetCollection<Jogos>(Valores.Mongo.jogos);
                    var reacts = db.GetCollection<Reacts>(Valores.Mongo.reacts);
                    
                    var emojiReacao = messageReactionAddEventArgs.Emoji;

                    var filtroJogos = Builders<Jogos>.Filter.Eq(x => x.idDoEmoji, emojiReacao.Id);
                    var resultadoJogos = await (await jogos.FindAsync(filtroJogos)).ToListAsync();

                    if (resultadoJogos.Count == 0)
                        return;

                    DiscordGuild guildReaction = messageReactionAddEventArgs.Guild;
                    var mensagemEmoji = messageReactionAddEventArgs.Message;
                    var canalReaction = messageReactionAddEventArgs.Channel;

                    var reacoesMembro = await mensagemEmoji.GetReactionsAsync(emojiReacao);

                    var dbContar = db.GetCollection<ContaMembrosQuePegaramCargos>(Valores.Mongo.contaMembrosQuePegaramCargos);

                    var resultadoReacts = await (await reacts.FindAsync(Builders<Reacts>.Filter.Eq(x => x.categoria, resultadoJogos.LastOrDefault().nomeDaCategoria))).ToListAsync();

                    var ultimoResultadoJogos = resultadoJogos.LastOrDefault();
                    var ultimoResultadoReacts = resultadoReacts.LastOrDefault();

                    bool resultadoDiferente = true;

                    if (resultadoJogos.Count == 0 || (resultadoJogos.Count != 0 && guildReaction.GetRole(ultimoResultadoJogos.idDoCargo) == null))
                        resultadoDiferente = false;

                    DiscordGuild UBGE = await messageReactionAddEventArgs.Client.GetGuildAsync(Valores.Guilds.UBGE);

                    if (!resultadoDiferente && emojiReacao != null)
                    {
                        DiscordChannel ubgeBot_ = UBGE.GetChannel(Valores.ChatsUBGE.canalUBGEBot);

                        await mensagemEmoji.DeleteReactionAsync(emojiReacao, ubgeBot.discordClient.CurrentUser);

                        var mensagemEmbed = await canalReaction.GetMessageAsync(resultadoReacts.LastOrDefault().idDaMensagem);
                        var embedMensagem = mensagemEmbed.Embeds.LastOrDefault();

                        if (embedMensagem.Description.Contains(emojiReacao.ToString()))
                        {
                            DiscordEmbedBuilder novoEmbed = new DiscordEmbedBuilder(embedMensagem);
                            var descricaoEmbed = embedMensagem.Description;

                            var lista = descricaoEmbed.Split('\n').ToList();

                            StringBuilder strEmbedFinal = new StringBuilder();

                            for (int linha = 0; linha < lista.Count; linha++)
                            {
                                if (lista[linha].Contains(emojiReacao.ToString()))
                                    lista.RemoveAt(linha);

                                strEmbedFinal.Append($"{lista[linha]}\n");
                            }

                            novoEmbed.Description = strEmbedFinal.ToString();
                            novoEmbed.WithAuthor(embedMensagem.Author.Name, null, Valores.logoUBGE);
                            novoEmbed.WithColor(new DiscordColor(0x32363c));

                            await mensagemEmbed.ModifyAsync(embed: novoEmbed.Build());

                            await jogos.DeleteOneAsync(filtroJogos);
                        }
                        else if (!embedMensagem.Description.Contains(emojiReacao.ToString()) && emojiReacao != null)
                        {
                            await ExcluiEAtualizaReactionDoEmoji(ubgeBot, mensagemEmoji, emojiReacao, reacoesMembro, ultimoResultadoReacts, canalReaction, guildReaction, ubgeBot_);
                            
                            return;
                        }

                        await ExcluiEAtualizaReactionDoEmoji(ubgeBot, mensagemEmoji, emojiReacao, reacoesMembro, ultimoResultadoReacts, canalReaction, guildReaction, ubgeBot_);

                        return;
                    }

                    DiscordRole acessoGeral = UBGE.GetRole(Valores.Cargos.cargoAcessoGeral);

                    var membro = guildReaction == UBGE ? await UBGE.GetMemberAsync(messageReactionAddEventArgs.User.Id) : await guildReaction.GetMemberAsync(messageReactionAddEventArgs.User.Id);

                    if (ultimoResultadoJogos.idDoCargo == acessoGeral.Id)
                    {
                        await membro.RevokeRoleAsync(acessoGeral);

                        return;
                    }

                    DiscordRole cargo = guildReaction.GetRole(ultimoResultadoJogos.idDoCargo);

                    var filtroDBContar = Builders<ContaMembrosQuePegaramCargos>.Filter.Eq(x => x.jogo, cargo.Name);
                    var resultadosDBContar = await (await dbContar.FindAsync(filtroDBContar)).ToListAsync();

                    if (resultadosDBContar.Count == 0)
                        await dbContar.InsertOneAsync(new ContaMembrosQuePegaramCargos { jogo = cargo.Name, numeroDePessoas = 1, idsDosMembrosQuePegaramOCargo = new List<ulong> { membro.Id } });
                    else
                    {
                        var primeiroResultado = resultadosDBContar.FirstOrDefault();

                        if (!primeiroResultado.idsDosMembrosQuePegaramOCargo.Contains(membro.Id))
                        {
                            if (primeiroResultado.idsDosMembrosQuePegaramOCargo.Contains(1))
                                primeiroResultado.idsDosMembrosQuePegaramOCargo.Remove(1);

                            await dbContar.UpdateOneAsync(filtroDBContar, Builders<ContaMembrosQuePegaramCargos>.Update
                                .Set(x => x.numeroDePessoas, primeiroResultado.numeroDePessoas + 1)
                                .Set(y => y.idsDosMembrosQuePegaramOCargo, primeiroResultado.idsDosMembrosQuePegaramOCargo.Append(membro.Id)));
                        }
                    }

                    await membro.GrantRoleAsync(cargo);

                    ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, $"O membro: \"{ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\" pegou o cargo de: \"{cargo.Name}\".");

                    if (guildReaction.Id == Valores.Guilds.UBGE)
                        await ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.ReactRoleUBGE, "Cargo Adicionado!", $"{emojiReacao} | O membro: \"{ubgeBot.utilidadesGerais.MencaoMembro(membro)}\" pegou o cargo de: {cargo.Mention}.\n\nOu:\n- `@{ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}`\n- `@{cargo.Name}`", ubgeBot.discordClient.CurrentUser.AvatarUrl, membro);
                    else
                        await ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.ReactRoleForaDaUBGE, "Cargo Adicionado!", $"{emojiReacao} | O membro: **{ubgeBot.utilidadesGerais.MencaoMembro(membro)}** pegou o cargo de: **{cargo.Name}**.", guildReaction.IconUrl, membro);
                }
                catch (Exception exception)
                {
                    await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                }
            }).Start();
        } 

        public async Task ReacaoRemovidaReactRole(MessageReactionRemoveEventArgs messageReactionRemoveEventArgs)
        {
            if (messageReactionRemoveEventArgs == null || 
                messageReactionRemoveEventArgs.Channel.IsPrivate || 
                messageReactionRemoveEventArgs.User.IsBot)
                return;

            await Task.Delay(200);

            new Thread(async () =>
            {
                try
                {
                    var db = ubgeBot.localDB;

                    var dbContar = db.GetCollection<ContaMembrosQuePegaramCargos>(Valores.Mongo.contaMembrosQuePegaramCargos);

                    var jogos = db.GetCollection<Jogos>(Valores.Mongo.jogos);
                    var reacts = db.GetCollection<Reacts>(Valores.Mongo.reacts);

                    var emojiReacao = messageReactionRemoveEventArgs.Emoji;
                    
                    var filtroJogos = Builders<Jogos>.Filter.Eq(x => x.idDoEmoji, emojiReacao.Id);
                    var resultadoJogos = await (await jogos.FindAsync(filtroJogos)).ToListAsync();

                    if (resultadoJogos.Count == 0)
                        return;

                    DiscordGuild guildReaction = messageReactionRemoveEventArgs.Guild;
                    var mensagemEmoji = messageReactionRemoveEventArgs.Message;
                    var canalReaction = messageReactionRemoveEventArgs.Channel;

                    var reacoesMembro = await mensagemEmoji.GetReactionsAsync(emojiReacao);

                    var resultadoReacts = await (await reacts.FindAsync(Builders<Reacts>.Filter.Eq(x => x.categoria, resultadoJogos.LastOrDefault().nomeDaCategoria))).ToListAsync();

                    var ultimoResultadoJogos = resultadoJogos.LastOrDefault();
                    var ultimoResultadoReacts = resultadoReacts.LastOrDefault();

                    bool resultadoDiferente = true;

                    if (resultadoJogos.Count == 0 || (resultadoJogos.Count != 0 && guildReaction.GetRole(ultimoResultadoJogos.idDoCargo) == null))
                        resultadoDiferente = false;

                    DiscordGuild UBGE = await messageReactionRemoveEventArgs.Client.GetGuildAsync(Valores.Guilds.UBGE);

                    if (!resultadoDiferente && emojiReacao != null)
                    {
                        DiscordChannel ubgeBot_ = UBGE.GetChannel(Valores.ChatsUBGE.canalUBGEBot);

                        await mensagemEmoji.DeleteReactionAsync(emojiReacao, ubgeBot.discordClient.CurrentUser);

                        var mensagemEmbed = await canalReaction.GetMessageAsync(resultadoReacts.LastOrDefault().idDaMensagem);
                        var embedMensagem = mensagemEmbed.Embeds.LastOrDefault();

                        if (embedMensagem.Description.Contains(emojiReacao.ToString()))
                        {
                            DiscordEmbedBuilder novoEmbed = new DiscordEmbedBuilder(embedMensagem);
                            var descricaoEmbed = embedMensagem.Description;

                            var lista = descricaoEmbed.Split('\n').ToList();

                            StringBuilder strEmbedFinal = new StringBuilder();

                            for (int linha = 0; linha < lista.Count; linha++)
                            {
                                if (lista[linha].Contains(emojiReacao.ToString()))
                                    lista.RemoveAt(linha);

                                strEmbedFinal.Append($"{lista[linha]}\n");
                            }

                            novoEmbed.Description = strEmbedFinal.ToString();
                            novoEmbed.WithAuthor(embedMensagem.Author.Name, null, Valores.logoUBGE);
                            novoEmbed.WithColor(new DiscordColor(0x32363c));

                            await mensagemEmbed.ModifyAsync(embed: novoEmbed.Build());

                            await jogos.DeleteOneAsync(filtroJogos);
                        }
                        else if (!embedMensagem.Description.Contains(emojiReacao.ToString()) && emojiReacao != null)
                        {
                            await ExcluiEAtualizaReactionDoEmoji(ubgeBot, mensagemEmoji, emojiReacao, reacoesMembro, ultimoResultadoReacts, canalReaction, guildReaction, ubgeBot_);

                            return;
                        }

                        await ExcluiEAtualizaReactionDoEmoji(ubgeBot, mensagemEmoji, emojiReacao, reacoesMembro, ultimoResultadoReacts, canalReaction, guildReaction, ubgeBot_);

                        return;
                    }

                    DiscordRole acessoGeral = UBGE.GetRole(Valores.Cargos.cargoAcessoGeral);

                    var membro = guildReaction == UBGE ? await UBGE.GetMemberAsync(messageReactionRemoveEventArgs.User.Id) : await guildReaction.GetMemberAsync(messageReactionRemoveEventArgs.User.Id);

                    if (ultimoResultadoJogos.idDoCargo == acessoGeral.Id)
                    {
                        await membro.GrantRoleAsync(acessoGeral);

                        return;
                    }

                    DiscordRole cargo = guildReaction.GetRole(ultimoResultadoJogos.idDoCargo);

                    var filtroDBContar = Builders<ContaMembrosQuePegaramCargos>.Filter.Eq(x => x.jogo, cargo.Name);
                    var resultadosDBContar = await (await dbContar.FindAsync(filtroDBContar)).ToListAsync();

                    if (resultadosDBContar.Count == 0)
                        await dbContar.InsertOneAsync(new ContaMembrosQuePegaramCargos { jogo = cargo.Name, numeroDePessoas = 1, idsDosMembrosQuePegaramOCargo = new List<ulong> { membro.Id } });
                    else
                    {
                        var primeiroResultado = resultadosDBContar.FirstOrDefault();

                        if (!primeiroResultado.idsDosMembrosQuePegaramOCargo.Contains(membro.Id))
                        {
                            if (primeiroResultado.idsDosMembrosQuePegaramOCargo.Contains(1))
                                primeiroResultado.idsDosMembrosQuePegaramOCargo.Remove(1);

                            await dbContar.UpdateOneAsync(filtroDBContar, Builders<ContaMembrosQuePegaramCargos>.Update
                                .Set(x => x.numeroDePessoas, primeiroResultado.numeroDePessoas + 1)
                                .Set(y => y.idsDosMembrosQuePegaramOCargo, primeiroResultado.idsDosMembrosQuePegaramOCargo.Append(membro.Id)));
                        }
                    }

                    await membro.RevokeRoleAsync(cargo);

                    ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, $"O membro: \"{ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\" removeu o cargo de: \"{cargo.Name}\".");

                    if (guildReaction.Id == Valores.Guilds.UBGE)
                        await ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.ReactRoleUBGE, "Cargo removido!", $"{emojiReacao} | O membro: \"{ubgeBot.utilidadesGerais.MencaoMembro(membro)}\" removeu o cargo de: {cargo.Mention}.\n\nOu:\n- `@{ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}`\n- `@{cargo.Name}`", ubgeBot.discordClient.CurrentUser.AvatarUrl, membro);
                    else
                        await ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.ReactRoleForaDaUBGE, "Cargo removido!", $"{emojiReacao} | O membro: **{ubgeBot.utilidadesGerais.MencaoMembro(membro)}** removeu o cargo de: **{cargo.Name}**.", guildReaction.IconUrl, membro);
                }
                catch (Exception exception)
                {
                    await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                }
            }).Start();
        } 
        
        public async Task ExcluiEAtualizaReactionDoEmoji(UBGEBot_ ubgeBotClient, DiscordMessage mensagemEmoji, DiscordEmoji emojiReacao, IReadOnlyList<DiscordUser> reacoesMembro, Reacts resultadoReacts, DiscordChannel canalReaction, DiscordGuild guildReaction, DiscordChannel ubgeBot)
        {
            try
            {
                await ubgeBotClient.utilidadesGerais.ExcluiReacoesDeUmaListaDeMembros(mensagemEmoji, emojiReacao, reacoesMembro);

                var atualizaMensagem = await canalReaction.GetMessageAsync(resultadoReacts.idDaMensagem);
                var mensagemEmojiAtualizado = await atualizaMensagem.GetReactionsAsync(emojiReacao);

                if (mensagemEmojiAtualizado.Count() != 0)
                    await ubgeBot.SendMessageAsync($":warning:, existem **{mensagemEmojiAtualizado.Count()}** reações sobrando no emoji: {emojiReacao.ToString()} no canal: {canalReaction.Mention}, elas não foram removidas pois os membros saíram da(o): **{guildReaction.Name}**");

                return;
            }
            catch (Exception exception)
            {
                await ubgeBotClient.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
            }
        }



        public async Task MensagemCriada(MessageCreateEventArgs messageCreateEventArgs)
        {
            if (messageCreateEventArgs == null)
                return;

            await Task.Delay(200);

            new Thread(async () =>
            {
                try
                {
                    var db = ubgeBot.localDB;

                    DiscordClient clientDiscord = messageCreateEventArgs.Client;
                    DiscordChannel canalMensagem = messageCreateEventArgs.Channel;
                    DiscordUser donoMensagem = messageCreateEventArgs.Author;
                    DiscordMessage mensagem = messageCreateEventArgs.Message;

                    DiscordGuild UBGE = await clientDiscord.GetGuildAsync(Valores.Guilds.UBGE);

                    DiscordMember donoMensagem_ = null;

                    if (UBGE.Members.Keys.Contains(donoMensagem.Id))
                        donoMensagem_ = await UBGE.GetMemberAsync(donoMensagem.Id);

                    var collectionModMail = db.GetCollection<ModMail>(Valores.Mongo.modMail);

                    if (!canalMensagem.IsPrivate)
                    {
                        if (canalMensagem.Id == Valores.ChatsUBGE.canalFormularioAlerta)
                        {
                            if (donoMensagem.IsBot)
                            {
                                await mensagem.CreateReactionAsync(DiscordEmoji.FromName(clientDiscord, ":white_check_mark:"));
                                await Task.Delay(200);
                                await mensagem.CreateReactionAsync(DiscordEmoji.FromName(clientDiscord, ":negative_squared_cross_mark:"));
                            }
                        }

                        var resultadoModMailIdDoCanal = await (await collectionModMail.FindAsync(Builders<ModMail>.Filter.Eq(x => x.idDoCanal, canalMensagem.Id))).ToListAsync();

                        if (resultadoModMailIdDoCanal.Count != 0 && !donoMensagem.IsBot && !(mensagem.Content.ToLower().StartsWith("//") || mensagem.Content.ToLower().StartsWith("ubge!")))
                        {
                            var ultimoResultadoModMailIdDoCanal = resultadoModMailIdDoCanal.LastOrDefault();

                            bool canalFechadoIdDoCanal = true;

                            if (ultimoResultadoModMailIdDoCanal.denuncia == null)
                                canalFechadoIdDoCanal = ultimoResultadoModMailIdDoCanal.contato.oCanalFoiFechado;
                            else if (ultimoResultadoModMailIdDoCanal.contato == null)
                                canalFechadoIdDoCanal = ultimoResultadoModMailIdDoCanal.denuncia.oCanalFoiFechado;

                            if (!canalFechadoIdDoCanal && canalMensagem.Id == ultimoResultadoModMailIdDoCanal.idDoCanal)
                            {
                                DiscordDmChannel pvMembro = await (await UBGE.GetMemberAsync(resultadoModMailIdDoCanal.LastOrDefault().idDoMembro)).CreateDmChannelAsync();

                                var nomeMembroNoDiscordModMail = ubgeBot.utilidadesGerais.RetornaNomeDiscord(donoMensagem_);

                                var mensagemAnexadas = mensagem.Attachments;

                                if (mensagemAnexadas.Count != 0)
                                {
                                    foreach (var arquivos in mensagemAnexadas)
                                    {
                                        if (string.IsNullOrWhiteSpace(mensagem.Content))
                                        {
                                            await pvMembro.SendMessageAsync($"**{nomeMembroNoDiscordModMail}#{donoMensagem_.Discriminator}** - **[{donoMensagem_.Roles.OrderByDescending(x => x.Position).FirstOrDefault().Name}]** → {arquivos.Url}");

                                            continue;
                                        }

                                        await pvMembro.SendMessageAsync($"**{nomeMembroNoDiscordModMail}#{donoMensagem_.Discriminator}** - **[{donoMensagem_.Roles.OrderByDescending(x => x.Position).FirstOrDefault().Name}]** → {mensagem.Content}\n\n*Mensagem anexada ao link.*");
                                        await pvMembro.SendMessageAsync(arquivos.Url);
                                    }

                                    return;
                                }

                                await pvMembro.SendMessageAsync($"**{nomeMembroNoDiscordModMail}#{donoMensagem_.Discriminator}** - **[{donoMensagem_.Roles.OrderByDescending(x => x.Position).FirstOrDefault().Name}]** → {mensagem.Content}");
                            }
                        }

                        if (canalMensagem.Id != Valores.ChatsUBGE.canalTesteDoBot ||
                            canalMensagem.Id != Valores.ChatsUBGE.canalPRServidor ||
                            canalMensagem.Id != Valores.ChatsUBGE.canalComandosBot ||
                            canalMensagem.Id != Valores.ChatsUBGE.canalCrieSuaSalaAqui ||
                            canalMensagem.Id != Valores.ChatsUBGE.canalUBGEBot ||
                            !donoMensagem.IsBot)
                        {
                            var colecao = db.GetCollection<Levels>(Valores.Mongo.levels);
                            var filtro = Builders<Levels>.Filter.Eq(x => x.idDoMembro, messageCreateEventArgs.Author.Id);

                            var lista = await (await colecao.FindAsync(filtro)).ToListAsync();

                            var xpAleatorio = ulong.Parse(new Random().Next(1, 20).ToString());

                            ulong numeroLevel = 1;

                            if (lista.Count == 0)
                                await colecao.InsertOneAsync(new Levels { idDoMembro = messageCreateEventArgs.Author.Id, xpDoMembro = 1, nomeDoLevel = $"{numeroLevel}", diaEHora = DateTime.Now.ToString() });
                            else
                            {
                                ulong xpFinal = 0;
                                ulong xpForeach = 0;

                                foreach (var Level in lista)
                                {
                                    if ((DateTime.Now - Convert.ToDateTime(Level.diaEHora)).TotalMinutes < 1)
                                        return;

                                    xpFinal = Level.xpDoMembro;
                                    xpForeach = Level.xpDoMembro;
                                    numeroLevel = ulong.Parse(Level.nomeDoLevel);
                                }

                                xpFinal = xpAleatorio + xpFinal;

                                if (xpFinal >= 2800 * numeroLevel)
                                    numeroLevel++;

                                await colecao.UpdateOneAsync(filtro, Builders<Levels>.Update.Set(x => x.xpDoMembro, xpFinal)
                                .Set(x => x.nomeDoLevel, $"{numeroLevel}")
                                .Set(x => x.diaEHora, DateTime.Now.ToString()));
                            }
                        }

                        return;
                    }

                    if (donoMensagem.IsBot || !UBGE.Members.Keys.Contains(donoMensagem.Id))
                        return;

                    var resultadoModMail = await (await collectionModMail.FindAsync(Builders<ModMail>.Filter.Eq(x => x.idDoMembro, donoMensagem.Id))).ToListAsync();
                    var ultimoResultadoModMail = resultadoModMail.LastOrDefault();

                    var nomeMembroNoDiscord = ubgeBot.utilidadesGerais.RetornaNomeDiscord(donoMensagem_);

                    DiscordRole cargoModeradorDiscord = UBGE.GetRole(Valores.Cargos.cargoModeradorDiscord);
                    DiscordRole cargoComiteComunitario = UBGE.GetRole(Valores.Cargos.cargoComiteComunitario);
                    DiscordRole cargoConselheiro = UBGE.GetRole(Valores.Cargos.cargoConselheiro);

                    DiscordChannel votacoesConselho = UBGE.GetChannel(Valores.ChatsUBGE.canalVotacoesConselho);

                    var modMailUBGE = UBGE.GetChannel(Valores.ChatsUBGE.Categorias.categoriaModMailBot);

                    if (mensagem.Content.ToLower() != "modmail")
                    {
                        if (resultadoModMail.Count != 0)
                        {
                            bool canalFechado = true;

                            if (ultimoResultadoModMail.denuncia == null)
                                canalFechado = ultimoResultadoModMail.contato.oCanalFoiFechado;
                            else if (ultimoResultadoModMail.contato == null)
                                canalFechado = ultimoResultadoModMail.denuncia.oCanalFoiFechado;

                            if (!canalFechado)
                            {
                                var canalModMail = UBGE.GetChannel(ultimoResultadoModMail.idDoCanal);

                                var mensagensAnexadas = mensagem.Attachments;

                                if (mensagensAnexadas.Count != 0)
                                {
                                    foreach (var arquivos in mensagensAnexadas)
                                    {
                                        await canalModMail.SendMessageAsync($"**{nomeMembroNoDiscord}#{donoMensagem_.Discriminator}** → {(string.IsNullOrWhiteSpace(mensagem.Content) ? string.Empty : $"{mensagem.Content}\n\n*Mensagem anexada com o link.*")}");
                                        await canalModMail.SendMessageAsync(arquivos.Url);
                                    }

                                    return;
                                }

                                await canalModMail.SendMessageAsync($"**{nomeMembroNoDiscord}#{donoMensagem_.Discriminator}** → {mensagem.Content}");

                                return;
                            }
                            else
                                return;
                        }
                        else
                            return;
                    }

                    new Thread(async () =>
                    {
                        try
                        {
                            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                            InteractivityExtension interactivity = clientDiscord.GetInteractivity();

                            DiscordEmoji emojiDenuncia = DiscordEmoji.FromName(clientDiscord, ":oncoming_police_car:");
                            DiscordEmoji emojiSugestao = DiscordEmoji.FromName(clientDiscord, ":star:");
                            DiscordEmoji emojiContatoStaff = await ubgeBot.utilidadesGerais.ProcuraEmoji(clientDiscord, ":LOGO_UBGE_2:");

                            embed.WithAuthor("O que você deseja fazer?", null, Valores.logoUBGE)
                                .WithColor(ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                                .WithDescription($"{emojiDenuncia} - Denúnciar um membro\n" +
                                $"{emojiSugestao} - Dar uma sugestão para a UBGE\n" +
                                $"{emojiContatoStaff} - Falar com a staff da UBGE")
                                .WithTimestamp(DateTime.Now)
                                .WithThumbnailUrl(donoMensagem.AvatarUrl);

                            DiscordMessage msgEscolhaMembro = await canalMensagem.SendMessageAsync(embed: embed.Build());
                            await msgEscolhaMembro.CreateReactionAsync(emojiDenuncia);
                            await Task.Delay(200);
                            await msgEscolhaMembro.CreateReactionAsync(emojiSugestao);
                            await Task.Delay(200);
                            await msgEscolhaMembro.CreateReactionAsync(emojiContatoStaff);

                            var emojiResposta = (await interactivity.WaitForReactionAsync(msgEscolhaMembro, donoMensagem, TimeSpan.FromMinutes(1))).Result.Emoji;

                            ModMail modMail = new ModMail();
                            modMail.idDoMembro = donoMensagem_.Id;

                            var nickMembroNoCanal = nomeMembroNoDiscord.Replace("[", "").Replace("]", "").Replace("'", "").Replace("\"", "").Replace("!", "").Replace("_", "").Replace("-", "").Replace("=", "").Replace("<", "").Replace("<", "").Replace(".", "").Replace(",", "").Replace("`", "").Replace("´", "").Replace("+", "").Replace("/", "").Replace("\\", "").Replace(":", "").Replace(";", "").Replace("{", "").Replace("}", "").Replace("ª", "").Replace("º", "").Replace(" ", "");

                            if (emojiResposta == emojiDenuncia)
                            {
                                ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                                embed.WithAuthor("Qual motivo de sua denúncia?", null, Valores.logoUBGE)
                                    .WithDescription("Digite ela abaixo para entendermos melhor e você entrará em contato direto com a staff.")
                                    .WithColor(ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                                DiscordMessage perguntaMotivoDenuncia = await canalMensagem.SendMessageAsync(embed: embed.Build());
                                var esperaRespostaMotivoDenuncia = await ubgeBot.utilidadesGerais.PegaRespostaPrivado(interactivity, donoMensagem_, canalMensagem);

                                ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                                embed.WithAuthor("Qual a sua denúncia?", null, Valores.logoUBGE)
                                    .WithColor(ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                                DiscordMessage perguntaDenuncia = await canalMensagem.SendMessageAsync(embed: embed.Build());
                                var esperaRespostaDenuncia = await ubgeBot.utilidadesGerais.PegaRespostaPrivado(interactivity, donoMensagem_, canalMensagem);

                                var diaHoraDenunciaDoMembro = DateTime.Now.ToString();

                                var canalMembroDaDenuncia = await UBGE.CreateTextChannelAsync($"{nickMembroNoCanal}-{donoMensagem_.Discriminator}", modMailUBGE);

                                await canalMembroDaDenuncia.AddOverwriteAsync(cargoComiteComunitario, Permissions.None, Permissions.AccessChannels | Permissions.SendMessages);
                                await canalMembroDaDenuncia.AddOverwriteAsync(cargoConselheiro, Permissions.None, Permissions.AccessChannels | Permissions.SendMessages);

                                modMail.idDoCanal = canalMembroDaDenuncia.Id;
                                modMail.denuncia = new Denuncia
                                {
                                    denunciaDoMembro = esperaRespostaDenuncia.Content,
                                    motivoDaDenunciaDoMembro = esperaRespostaMotivoDenuncia.Content,
                                    diaHoraDenuncia = diaHoraDenunciaDoMembro,
                                    oCanalFoiFechado = false,
                                };

                                await collectionModMail.InsertOneAsync(modMail);

                                ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                                embed.WithAuthor($"Denúncia do membro: \"{nomeMembroNoDiscord}#{donoMensagem_.Discriminator}\"", null, Valores.logoUBGE)
                                    .WithColor(ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                                    .WithDescription($"Quando foi feita esta denúncia: **{diaHoraDenunciaDoMembro}**\n\n" +
                                    $"Motivo da denúncia: **{modMail.denuncia.motivoDaDenunciaDoMembro}**\n" +
                                    $"Denúncia: **{modMail.denuncia.denunciaDoMembro}**")
                                    .WithThumbnailUrl(donoMensagem_.AvatarUrl)
                                    .WithTimestamp(DateTime.Now);

                                await canalMembroDaDenuncia.SendMessageAsync(embed: embed.Build(), content: cargoModeradorDiscord.Mention);

                                ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                                embed.WithAuthor("Sua denúncia foi enviada a staff da UBGE!", null, Valores.logoUBGE)
                                    .WithColor(ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                                    .WithDescription("A staff irá ler e provalmente irá fazer diversas perguntas, e qualquer mensagem enviada por eles eu enviarei para você, fique atento a seu privado e responda normalmente! :wink:")
                                    .WithThumbnailUrl(donoMensagem_.AvatarUrl)
                                    .WithTimestamp(DateTime.Now);

                                await canalMensagem.SendMessageAsync(embed: embed.Build());
                            }
                            else if (emojiResposta == emojiSugestao)
                            {
                                ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                                embed.WithAuthor("Digite aqui sua sugestão para a UBGE!", null, Valores.logoUBGE)
                                    .WithDescription("Digite aqui sua sugestão, logo após ela irá entrar em votação no conselho comunitário!")
                                    .WithColor(ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                                DiscordMessage perguntaSugestao = await canalMensagem.SendMessageAsync(embed: embed.Build());
                                var esperaSugestao = await ubgeBot.utilidadesGerais.PegaRespostaPrivado(interactivity, donoMensagem_, canalMensagem);

                                var diaHoraSugestaoDoMembro = DateTime.Now.ToString();

                                modMail.sugestao = new Sugestao
                                {
                                    diaHoraSugestao = diaHoraSugestaoDoMembro,
                                    sugestaoDoMembro = esperaSugestao.Content,
                                };

                                await collectionModMail.InsertOneAsync(modMail);

                                ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                                embed.WithAuthor($"Sugestão do membro: \"{nomeMembroNoDiscord}#{donoMensagem_.Discriminator}\"", null, Valores.logoUBGE)
                                    .WithColor(ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                                    .WithDescription(esperaSugestao.Content)
                                    .WithThumbnailUrl(donoMensagem_.AvatarUrl)
                                    .WithTimestamp(DateTime.Now);

                                var mensagemSugestao = await votacoesConselho.SendMessageAsync(embed: embed.Build(), content: cargoConselheiro.Mention);
                                await mensagemSugestao.CreateReactionAsync(DiscordEmoji.FromName(clientDiscord, ":white_check_mark:"));
                                await Task.Delay(200);
                                await mensagemSugestao.CreateReactionAsync(DiscordEmoji.FromName(clientDiscord, ":negative_squared_cross_mark:"));

                                ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                                embed.WithAuthor("Sua sugestão foi enviada para a staff da UBGE!", null, Valores.logoUBGE)
                                    .WithColor(ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                                    .WithDescription($"Obrigado por fazer um servidor agradável a todos os membros! {await ubgeBot.utilidadesGerais.ProcuraEmoji(clientDiscord, ":UBGE:")}")
                                    .WithThumbnailUrl(donoMensagem_.AvatarUrl)
                                    .WithTimestamp(DateTime.Now);

                                await canalMensagem.SendMessageAsync(embed: embed.Build());
                            }
                            else if (emojiResposta == emojiContatoStaff)
                            {
                                ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                                embed.WithAuthor("Digite aqui seu motivo para entrar em contato com a staff da UBGE!", null, Valores.logoUBGE)
                                    .WithColor(ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                                DiscordMessage perguntaMotivoContato = await canalMensagem.SendMessageAsync(embed: embed.Build());
                                var esperaMotivoContato = await ubgeBot.utilidadesGerais.PegaRespostaPrivado(interactivity, donoMensagem_, canalMensagem);

                                var diaHoraContatoDoMembro = DateTime.Now.ToString();

                                var canalMembroContato = await UBGE.CreateTextChannelAsync($"{nickMembroNoCanal}-{donoMensagem_.Discriminator}", modMailUBGE);

                                await canalMembroContato.AddOverwriteAsync(cargoModeradorDiscord, Permissions.None, Permissions.AccessChannels | Permissions.SendMessages);
                                await canalMembroContato.AddOverwriteAsync(cargoConselheiro, Permissions.None, Permissions.AccessChannels | Permissions.SendMessages);

                                modMail.idDoCanal = canalMembroContato.Id;
                                modMail.contato = new Contato
                                {
                                    diaHoraContato = diaHoraContatoDoMembro,
                                    motivoDoContatoDoMembro = esperaMotivoContato.Content,
                                    oCanalFoiFechado = false,
                                };

                                await collectionModMail.InsertOneAsync(modMail);

                                ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                                embed.WithAuthor($"O membro: \"{nomeMembroNoDiscord}#{donoMensagem_.Discriminator}\" quer falar com a staff", null, Valores.logoUBGE)
                                    .WithColor(ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                                    .WithDescription($"Quando foi feito este contato com a staff: **{diaHoraContatoDoMembro}**\n\n" +
                                    $"Motivo: **{esperaMotivoContato.Content}**")
                                    .WithThumbnailUrl(donoMensagem_.AvatarUrl)
                                    .WithTimestamp(DateTime.Now);

                                await canalMembroContato.SendMessageAsync(embed: embed.Build(), content: cargoComiteComunitario.Mention);

                                ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                                embed.WithAuthor("Seu pedido de contato foi enviado para a staff da UBGE!", null, Valores.logoUBGE)
                                    .WithColor(ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                                    .WithDescription("A staff irá ler e provalmente irá fazer diversas perguntas, e qualquer mensagem enviada por eles eu enviarei para você, fique atento a seu privado e responda normalmente! :wink:")
                                    .WithThumbnailUrl(donoMensagem_.AvatarUrl)
                                    .WithTimestamp(DateTime.Now);

                                await canalMensagem.SendMessageAsync(embed: embed.Build());
                            }

                            return;
                        }
                        catch (Exception exception)
                        {
                            await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                        }
                    }).Start();
                }
                catch (NullReferenceException) { }
                catch (Exception exception)
                {
                    await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                }
            }).Start();
        } 
        
        public async Task CanalDeVozPersonalizado(VoiceStateUpdateEventArgs voiceStateUpdateEventArgs)
        {
            if (voiceStateUpdateEventArgs.Guild.Id != Valores.Guilds.UBGE)
                return;

            await Task.Delay(200);

            new Thread(async () =>
            {
                try
                {
                    var local = ubgeBot.localDB;
                    var salas = local.GetCollection<Salas>(Valores.Mongo.salas);

                    DiscordGuild UBGE = voiceStateUpdateEventArgs.Guild;
                    
                    DiscordMember membro = await UBGE.GetMemberAsync(voiceStateUpdateEventArgs.User.Id);

                    DiscordChannel cliqueAqui = UBGE.GetChannel(Valores.ChatsUBGE.canalCliqueAqui);

                    if (cliqueAqui == null)
                        return;

                    var filtroSalas = Builders<Salas>.Filter.Eq(s => s.idDoDono, membro.Id);
                    var resultadoSalas = await (await salas.FindAsync(filtroSalas)).ToListAsync();

                    if (voiceStateUpdateEventArgs.Before?.Channel != null &&
                    voiceStateUpdateEventArgs.Before?.Channel?.Id == resultadoSalas.LastOrDefault().idDaSala &&
                    voiceStateUpdateEventArgs.Before?.Channel?.Users.Count() == 0 &&
                    voiceStateUpdateEventArgs.After?.Channel != null &&
                    voiceStateUpdateEventArgs.After?.Channel?.Id == cliqueAqui.Id)
                    {
                        await membro.PlaceInAsync(UBGE.GetChannel(resultadoSalas.LastOrDefault().idDaSala));

                        return;
                    }

                    if (voiceStateUpdateEventArgs.Before?.Channel != null &&
                    voiceStateUpdateEventArgs.Before?.Channel?.Id == resultadoSalas.LastOrDefault().idDaSala &&
                    voiceStateUpdateEventArgs.Before?.Channel?.Users.Count() == 0)
                    {
                        await UBGE.GetChannel(resultadoSalas.LastOrDefault().idDaSala).DeleteAsync();

                        return;
                    }

                    DiscordRole membroRegistradoCargo = UBGE.GetRole(Valores.Cargos.cargoMembroRegistrado);

                    if (voiceStateUpdateEventArgs.Before?.Channel == null && voiceStateUpdateEventArgs.After?.Channel?.Id == cliqueAqui.Id || voiceStateUpdateEventArgs.Before?.Channel != null && voiceStateUpdateEventArgs.After?.Channel?.Id == cliqueAqui.Id)
                    {
                        if (membro.Roles.Contains(membroRegistradoCargo))
                        {
                            DiscordRole prisioneiroCargo = UBGE.GetRole(Valores.Cargos.cargoPrisioneiro),
                            botsMusicaisCargo = UBGE.GetRole(Valores.Cargos.cargoBots),
                            acessoGeralCargo = UBGE.GetRole(Valores.Cargos.cargoAcessoGeral),
                            moderadorDiscordCargo = UBGE.GetRole(Valores.Cargos.cargoModeradorDiscord);

                            string nomeAntigo = "📌 Clique aqui!";
                            await cliqueAqui.ModifyAsync(c => c.Name = $"📌 Sala criada!");

                            new Thread(async () =>
                            {
                                await Task.Delay(TimeSpan.FromSeconds(4));

                                await cliqueAqui.ModifyAsync(c => c.Name = nomeAntigo);
                            }).Start();

                            DiscordChannel canalDoMembro = await UBGE.CreateChannelAsync(!string.IsNullOrWhiteSpace(membro.Presence?.Activity?.Name) ? membro.Presence.Activity.Name : $"Sala do: {ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}", ChannelType.Voice, cliqueAqui.Parent);

                            if (resultadoSalas.Count == 0)
                            {
                                await canalDoMembro.AddOverwriteAsync(membro, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice | Permissions.UseVoiceDetection);
                                await canalDoMembro.AddOverwriteAsync(botsMusicaisCargo, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice | Permissions.UseVoiceDetection);

                                await salas.InsertOneAsync(new Salas
                                {
                                    idDoDono = membro.Id,
                                    idsPermitidos = new List<ulong> { membro.Id },
                                    limiteDeUsuarios = 0,
                                    nomeDaSala = canalDoMembro.Name,
                                    salaTrancada = false,
                                    idDaSala = canalDoMembro.Id,
                                    _id = new ObjectId(),
                                });

                                await canalDoMembro.PlaceMemberAsync(membro);
                            }
                            else
                            {
                                if (resultadoSalas[0].salaTrancada)
                                {
                                    await canalDoMembro.AddOverwriteAsync(UBGE.EveryoneRole, Permissions.AccessChannels, Permissions.UseVoice | Permissions.Speak | Permissions.UseVoiceDetection);
                                    await canalDoMembro.AddOverwriteAsync(membro, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice | Permissions.UseVoiceDetection);
                                    await canalDoMembro.AddOverwriteAsync(botsMusicaisCargo, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice | Permissions.UseVoiceDetection);
                                    await canalDoMembro.AddOverwriteAsync(acessoGeralCargo, Permissions.AccessChannels, Permissions.UseVoice | Permissions.Speak | Permissions.UseVoiceDetection);
                                    await canalDoMembro.AddOverwriteAsync(moderadorDiscordCargo, Permissions.AccessChannels, Permissions.UseVoice | Permissions.Speak | Permissions.UseVoiceDetection);

                                    await salas.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(u => u.salaTrancada, true));

                                    DiscordMember membrosForeach = null;

                                    foreach (ulong idsMembros in resultadoSalas[0].idsPermitidos)
                                    {
                                        membrosForeach = await UBGE.GetMemberAsync(idsMembros);

                                        await canalDoMembro.AddOverwriteAsync(membrosForeach, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice | Permissions.UseVoiceDetection);
                                    }
                                }
                                else
                                {
                                    await canalDoMembro.AddOverwriteAsync(UBGE.EveryoneRole, Permissions.AccessChannels | Permissions.UseVoice | Permissions.Speak | Permissions.UseVoiceDetection);
                                    await canalDoMembro.AddOverwriteAsync(botsMusicaisCargo, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice | Permissions.UseVoiceDetection);

                                    await salas.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(u => u.salaTrancada, false));
                                }

                                if (resultadoSalas[0].limiteDeUsuarios != 0)
                                    await canalDoMembro.ModifyAsync(x => x.Userlimit = resultadoSalas[0].limiteDeUsuarios);

                                await salas.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(x => x.idDaSala, canalDoMembro.Id));

                                await canalDoMembro.PlaceMemberAsync(membro);
                            }
                        }
                        else
                        {
                            try
                            {
                                DiscordChannel Comandos_Bot = UBGE.GetChannel(Valores.ChatsUBGE.canalComandosBot);
                                DiscordChannel BatePapo = UBGE.GetChannel(Valores.ChatsUBGE.canalBatePapo);

                                await BatePapo.PlaceMemberAsync(membro);

                                await Comandos_Bot.SendMessageAsync($"{membro.Mention} Você precisa ter o cargo de membro registrado para criar salas de voz!\n\nPara isso, " +
                                $"digite o comando `//fazercenso` para fazer o censo comunitário e ter acesso à salas privadas!");
                                await (await membro.CreateDmChannelAsync()).SendMessageAsync($"{membro.Mention}, na UBGE você precisa ter o cargo de membro registrado para criar salas de voz!\n\nPara isso, " +
                                $"digite o comando `//fazercenso` no {Comandos_Bot.Mention} para fazer o censo comunitário e ter acesso à salas privadas!");
                            }
                            catch (UnauthorizedException)
                            {
                                ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, "Não foi possível enviar a mensagem de pedido para fazer o censo no privado do membro.");
                            }
                            catch (Exception) { }
                        }
                    }
                }
                catch (NullReferenceException) { }
                catch (Exception exception)
                {
                    await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                }
            }).Start();
        }
        
        public async Task NovoBan(GuildBanAddEventArgs guildBanAddEventArgs)
        {
            if (guildBanAddEventArgs.Guild.Id != Valores.Guilds.UBGE)
                return;

            await Task.Delay(200);

            new Thread(async () =>
            {
                try
                {
                    DiscordChannel logChat = guildBanAddEventArgs.Guild.GetChannel(Valores.ChatsUBGE.canalLog);

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                    {
                        Color = ubgeBot.utilidadesGerais.CorAleatoriaEmbed(),
                        Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"O membro: \"{ubgeBot.utilidadesGerais.RetornaNomeDiscord(guildBanAddEventArgs.Member)}#{guildBanAddEventArgs.Member.Discriminator}\" foi banido.", IconUrl = Valores.logoUBGE },
                        Description = $"Dia e Hora: {DateTime.Now.ToString()}\n\n" +
                                $"ID do Membro: {guildBanAddEventArgs.Member.Id}",
                        Timestamp = DateTime.Now,
                        ThumbnailUrl = guildBanAddEventArgs.Member.AvatarUrl,
                    };

                    await logChat.SendMessageAsync(embed: embed.Build());
                }
                catch (Exception exception)
                {
                    await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                }
            }).Start();
        }

        public async Task BanRetirado(GuildBanRemoveEventArgs guildBanRemoveEventArgs)
        {
            if (guildBanRemoveEventArgs.Guild.Id != Valores.Guilds.UBGE)
                return;

            await Task.Delay(200);

            new Thread(async () =>
            {
                try
                {
                    DiscordChannel logChat = guildBanRemoveEventArgs.Guild.GetChannel(Valores.ChatsUBGE.canalLog);

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                    {
                        Color = ubgeBot.utilidadesGerais.CorAleatoriaEmbed(),
                        Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"O membro: \"{ubgeBot.utilidadesGerais.RetornaNomeDiscord(guildBanRemoveEventArgs.Member)}#{guildBanRemoveEventArgs.Member.Discriminator}\" foi desbanido.", IconUrl = Valores.logoUBGE },
                        Description = $"Dia e Hora: {DateTime.Now.ToString()}\n\n" +
                            $"ID do Membro: {guildBanRemoveEventArgs.Member.Id}",
                        Timestamp = DateTime.Now,
                        ThumbnailUrl = guildBanRemoveEventArgs.Member.AvatarUrl,
                    };

                    await logChat.SendMessageAsync(embed: embed.Build());
                }
                catch (Exception exception)
                {
                    await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                }
            }).Start();
        }

        public async Task MembroEntra(GuildMemberAddEventArgs guildMemberAddEventArgs)
        {
            if (guildMemberAddEventArgs.Guild.Id != Valores.Guilds.UBGE)
                return;

            await Task.Delay(200);

            new Thread(async () =>
            {
                try
                {
                    DiscordRole acessoGeralCargo = guildMemberAddEventArgs.Guild.GetRole(Valores.Cargos.cargoAcessoGeral);
                    DiscordDmChannel privadoMembro = await guildMemberAddEventArgs.Member.CreateDmChannelAsync();
                    DiscordChannel comandosBot = guildMemberAddEventArgs.Guild.GetChannel(Valores.ChatsUBGE.canalComandosBot);

                    await guildMemberAddEventArgs.Member.GrantRoleAsync(acessoGeralCargo);
                    await privadoMembro.SendMessageAsync($"*{guildMemberAddEventArgs.Member.Mention}, Bem-Vindo a UBGE!*\n\n" +
                    $"Leia a mensagem que o Mee6 lhe enviou no seu privado, ele lhe ajudará a dar os seus primeiros passos na UBGE.\n\n" +
                    $"Para qualquer dúvida sobre mim, digite `//ajuda`.\n\n" +
                    $"Obrigado por ler isso, e antes de tudo, sinta-se em casa! :smile:");
                }
                catch (Exception exception)
                {
                    await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                }
            }).Start();
        }

        private async Task MembroAlterado(GuildMemberUpdateEventArgs guildMemberUpdateEventArgs)
        {
            if (guildMemberUpdateEventArgs.Guild.Id != Valores.Guilds.UBGE)
                return;

            await Task.Delay(200);

            new Thread(async () =>
            {
                try
                {
                    DiscordGuild UBGE = guildMemberUpdateEventArgs.Guild;
                    DiscordMember membroDiscord = guildMemberUpdateEventArgs.Member;

                    DiscordRole cargoNitroBooster = UBGE.GetRole(Valores.Cargos.cargoNitroBooster);
                    DiscordRole cargoDoador = UBGE.GetRole(Valores.Cargos.cargoDoador);

                    if (!guildMemberUpdateEventArgs.RolesBefore.Contains(cargoNitroBooster) && guildMemberUpdateEventArgs.RolesAfter.Contains(cargoNitroBooster))
                        await membroDiscord.GrantRoleAsync(cargoDoador);
                    else if (guildMemberUpdateEventArgs.RolesBefore.Contains(cargoNitroBooster) && !guildMemberUpdateEventArgs.RolesAfter.Contains(cargoNitroBooster))
                        await membroDiscord.RevokeRoleAsync(cargoDoador);
                }
                catch (Exception exception)
                {
                    await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                }
            }).Start();
        }

        public async Task ConectarEReconectarBotAoDiscordAsync(UBGEBot_ ubgeBotClient, bool novaSessao = false)
        {
            try
            {
                if (!novaSessao)
                {
                    await ubgeBotClient.discordClient.ConnectAsync();
                    await Task.Delay(-1);
                }
                else
                {
                    ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, $"A conexão com o Discord foi encerrada! Reconectando...");

                    await ubgeBotClient.discordClient.ReconnectAsync(true);

                    ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, $"Reconectado!");
                }
            }
            catch (Exception exception)
            {
                ubgeBotClient.logExceptionsToDiscord.ExceptionToTxt(exception);
                ShutdownBot();
            }
        }
        
        private async Task CheckReacoesMarcadasQuandoOBotEstavaOfflineNoReactRole(UBGEBot_ ubgeBotClient)
        {
            await Task.Delay(200);

            new Thread(async () => 
            {
                try
                {
                    var db = ubgeBotClient.localDB;

                    var reacts = db.GetCollection<Reacts>(Valores.Mongo.reacts);
                    var roles = db.GetCollection<Jogos>(Valores.Mongo.jogos);

                    var resultadoReacts = await (await reacts.FindAsync(Builders<Reacts>.Filter.Empty)).ToListAsync();

                    DiscordGuild UBGE = await ubgeBotClient.discordClient.GetGuildAsync(Valores.Guilds.UBGE);

                    List<DiscordRole> cargosUBGE = UBGE.Roles.Values.ToList(); 

                    DiscordRole acessoGeral = UBGE.GetRole(Valores.Cargos.cargoAcessoGeral);
                    DiscordRole prisioneiro = UBGE.GetRole(Valores.Cargos.cargoPrisioneiro);

                    DiscordMessage mensagem = null;
                    DiscordGuild servidor = null;
                    DiscordChannel canalServidor = null;
                    DiscordUser usuario = null;
                    DiscordRole cargo = null;
                    DiscordMember membro = null;
                    DiscordGuild servidorMembro = null;
                    int N = 0;

                    Dictionary<string, string> categorias = new Dictionary<string, string>();
                    foreach (var React in resultadoReacts)
                        categorias.Add($"{React.servidor} {N++} {React.idDoCanal}", $"{React.categoria}@ {React.idDaMensagem}");

                    await Task.Delay(200);

                    foreach (var categoria in categorias)
                    {
                        var idServidorDictionary = ulong.Parse(categoria.Key.Split(' ')[0].Replace(" ", ""));

                        if (idServidorDictionary == Valores.Guilds.UBGE)
                            servidor = UBGE;
                        else
                            servidor = await ubgeBotClient.discordClient.GetGuildAsync(idServidorDictionary);

                        canalServidor = servidor.GetChannel(ulong.Parse(categoria.Key.Split(' ')[2].Replace(" ", "")));
                        mensagem = await canalServidor.GetMessageAsync(ulong.Parse(categoria.Value.Split('@')[1].Replace(" ", "")));

                        await Task.Delay(200);

                        var Cargos = await (await roles.FindAsync(Builders<Jogos>.Filter.Eq(x => x.nomeDaCategoria, categoria.Value.Split('@')[0]))).ToListAsync();

                        Dictionary<ulong, ulong> EmojiRole = new Dictionary<ulong, ulong>();
                        foreach (var r in Cargos)
                            if (!string.IsNullOrEmpty(r.nomeDaCategoria))
                                EmojiRole.Add(r.idDoEmoji, r.idDoCargo);

                        Dictionary<DiscordEmoji, IReadOnlyList<DiscordUser>> Usuarios = new Dictionary<DiscordEmoji, IReadOnlyList<DiscordUser>>();
                        foreach (DiscordReaction discordReaction in mensagem.Reactions)
                        {
                            Usuarios.Add(discordReaction.Emoji, await mensagem.GetReactionsAsync(discordReaction.Emoji));
                            
                            await Task.Delay(200);
                        }

                        foreach (DiscordEmoji emoji in Usuarios.Keys)
                        {
                            for (int i = 0; i < Usuarios[emoji].Count; i++)
                            {
                                usuario = Usuarios[emoji][i];

                                if (!usuario.IsBot)
                                {
                                    cargo = UBGE.GetRole(EmojiRole[emoji.Id]);

                                    if (cargo == null)
                                    {
                                        foreach (var servidoresBot in ubgeBotClient.discordClient.Guilds.Values)
                                        {
                                            cargo = servidoresBot.Roles.Values.ToList().Find(x => x.Id == EmojiRole[emoji.Id]);

                                            if (cargo != null)
                                            {
                                                servidorMembro = servidoresBot;

                                                break;
                                            }

                                            await Task.Delay(200);
                                        }
                                    }

                                    if (cargo == null)
                                        continue;

                                    if (servidor.Members.Values.FirstOrDefault(em => em.Id == usuario.Id) == null) { }
                                    else
                                    {
                                        if (servidorMembro == null)
                                            membro = await UBGE.GetMemberAsync(usuario.Id);
                                        else
                                        {
                                            if (servidorMembro.Roles.Values.Contains(cargo))
                                                membro = await servidorMembro.GetMemberAsync(usuario.Id);
                                            else
                                                membro = await UBGE.GetMemberAsync(usuario.Id);
                                        }

                                        await Task.Delay(200);
                                        
                                        if (!membro.Roles.Contains(cargo) && cargo != acessoGeral && !membro.Roles.Contains(prisioneiro))
                                        {
                                            await membro.GrantRoleAsync(cargo);

                                            if (membro.Guild.Id == Valores.Guilds.UBGE)
                                                await ubgeBotClient.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.SAC, 
                                                    "[S.A.C] - Sistema de Adicionar Cargos", 
                                                    $"Foi adicionado o cargo: {cargo.Mention} no: {ubgeBotClient.utilidadesGerais.MencaoMembro(membro)}.", servidor.IconUrl, membro);
                                            else
                                                await ubgeBotClient.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.SAC, 
                                                    "[S.A.C] - Sistema de Adicionar Cargos", 
                                                    $"Foi adicionado o cargo: **{cargo.Name}** no: **{ubgeBotClient.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}**.", servidor.IconUrl, membro);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Sistemas, "A sincronização de cargos foi finalizada!");
                    await ubgeBotClient.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.SAC, "A sincronização de cargos foi finalizada!", ":wink:", ubgeBotClient.discordClient.CurrentUser.AvatarUrl, ubgeBotClient.discordClient.CurrentUser);
                }
                catch (Exception exception)
                {
                    await ubgeBotClient.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Sistemas, exception);
                }
            }).Start();
        } 

        private static async Task ExecutaList(UBGEBot_ ubgeBotClient, string mensagemDoAviso)
        {
            await Task.Delay(200);

            new Thread(async () =>
            {
                try
                {
                    DiscordGuild UBGE = await ubgeBotClient.discordClient.GetGuildAsync(Valores.Guilds.UBGE);
                    DiscordChannel ubgeBot = UBGE.GetChannel(Valores.ChatsUBGE.canalUBGEBot);
                    DiscordMember ubgeBotMembro = await UBGE.GetMemberAsync(Valores.Guilds.Membros.ubgeBot);

                    DiscordMessage mensagemAviso = await ubgeBot.SendMessageAsync($":warning: | {mensagemDoAviso} Dia e Hora: `{DateTime.Now.ToString()}`.");

                    var procuraComando = ubgeBotClient.discordClient.GetCommandsNext().FindCommand("s list", out var Args);
                    var comandoList = ubgeBotClient.discordClient.GetCommandsNext().CreateFakeContext(ubgeBotMembro, ubgeBot, "", "//", procuraComando, Args);

                    await ubgeBotClient.discordClient.GetCommandsNext().ExecuteCommandAsync(comandoList);

                    await mensagemAviso.DeleteAsync();
                }
                catch (Exception exception)
                {
                    await ubgeBotClient.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Sistemas, exception);
                }
            }).Start();
        }

        private static async Task EnviaDadosDiarios(UBGEBot_ ubgeBotClient)
        {
            await Task.Delay(200);

            new Thread(async () =>
            {
                try
                {
                    while (true)
                    {
                        //if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 55 && DateTime.Now.Second == 00)
                        //    await ExecutaList(ubgeBotClient, "Executando o **//list** para a execução do método para enviar os dados díários...");

                        if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 55)
                        {
                            var db = ubgeBotClient.localDB;
                            var dbMembros = db.GetCollection<ContaMembrosQuePegaramCargos>(Valores.Mongo.contaMembrosQuePegaramCargos);
                            var dbMembrosRegistrados = db.GetCollection<MembrosQuePegaramOCargoDeMembroRegistrado>(Valores.Mongo.membrosQuePegaramOCargoDeMembroRegistrado);

                            var filtroDBMembros = Builders<ContaMembrosQuePegaramCargos>.Filter.Empty;
                            var filtroDBMembrosRegistrados = Builders<MembrosQuePegaramOCargoDeMembroRegistrado>.Filter.Empty;

                            var resultadosDBMembros = await (await dbMembros.FindAsync(filtroDBMembros)).ToListAsync();
                            var resultadosDBMembrosRegistrados = await (await dbMembrosRegistrados.FindAsync(filtroDBMembrosRegistrados)).ToListAsync();

                            DiscordGuild UBGE = await ubgeBotClient.discordClient.GetGuildAsync(Valores.Guilds.UBGE);
                            DiscordChannel BotUBGE = UBGE.GetChannel(Valores.ChatsUBGE.canalUBGEBot);

                            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                            if (resultadosDBMembros.Count == 0 && resultadosDBMembrosRegistrados.Count == 0)
                            {
                                embed.WithAuthor("Não tenho dados para apresentar :/", null, Valores.logoUBGE)
                                    .WithColor(ubgeBotClient.utilidadesGerais.CorAleatoriaEmbed())
                                    .WithDescription(":pensive:")
                                    .WithFooter($"Nada há declarar. Às: {DateTime.Now.ToString()}")
                                    .WithThumbnailUrl((await ubgeBot.utilidadesGerais.ProcuraEmoji(ubgeBotClient.discordClient, "gatu")).Url);

                                await BotUBGE.SendMessageAsync(embed: embed.Build());
                            }
                            else
                            {
                                string caminhoTxt = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + $@"\DadosReactRole.txt";

                                using (StreamWriter streamWriter = new StreamWriter(caminhoTxt, false, Encoding.UTF8))
                                {
                                    if (resultadosDBMembros.Count != 0)
                                    {
                                        foreach (var Dados in resultadosDBMembros)
                                        {
                                            streamWriter.WriteLine($"Jogo: \"{Dados.jogo}\"");
                                            streamWriter.WriteLine($"Número de pessoas que pegaram o cargo: \"{(Dados.numeroDePessoas == 1 ? $"{Dados.numeroDePessoas} membro." : $"{Dados.numeroDePessoas} membros.")}\".");
                                            streamWriter.WriteLine(string.Empty);
                                        }
                                    }
                                    else
                                    {
                                        streamWriter.WriteLine("Um total de 0 pessoas pegaram algum cargo de jogo.");
                                        streamWriter.WriteLine(string.Empty);
                                        streamWriter.WriteLine(string.Empty);
                                    }

                                    if (resultadosDBMembrosRegistrados.Count != 0)
                                    {
                                        foreach (var Dados_ in resultadosDBMembrosRegistrados)
                                        {
                                            streamWriter.WriteLine($"Número de pessoas que pegaram o cargo de membro registrado: \"{Dados_.numeroPessoasQuePegaramOCargoDeMembroRegistrado}\".");
                                            streamWriter.WriteLine(string.Empty);
                                        }
                                    }
                                    else
                                    {
                                        streamWriter.WriteLine("Um total de 0 pessoas pegaram o cargo de Membro Registrado.");
                                        streamWriter.WriteLine(string.Empty);
                                        streamWriter.WriteLine(string.Empty);
                                    }

                                    streamWriter.WriteLine($"Esses dados do React Role e de Membros Registrados são do dia e hora: \"{DateTime.Now.ToString()}\".");
                                }

                                embed.WithAuthor("Dados do React Role:", null, Valores.logoUBGE)
                                    .WithColor(ubgeBotClient.utilidadesGerais.CorAleatoriaEmbed())
                                    .WithDescription($":smile: {await ubgeBotClient.utilidadesGerais.ProcuraEmoji(ubgeBotClient.discordClient, "ubge")}")
                                    .WithThumbnailUrl((await ubgeBotClient.utilidadesGerais.ProcuraEmoji(ubgeBotClient.discordClient, "uhu")).Url)
                                    .WithFooter($"Esses dados são enviados automaticamente pelo bot para fins de estatísticas da UBGE.");

                                await BotUBGE.SendMessageAsync(embed: embed.Build());
                                await BotUBGE.SendFileAsync(caminhoTxt);

                                await dbMembros.DeleteManyAsync(filtroDBMembros);
                                await dbMembrosRegistrados.DeleteManyAsync(filtroDBMembrosRegistrados);

                                File.Delete(caminhoTxt);
                            }

                            await Task.Delay(1000);
                        }
                    }
                }
                catch (Exception exception)
                {
                    await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Sistemas, exception);
                }
            }).Start();
        }

        private static async Task ModuloBotAberto(UBGEBot_ ubgeBotClient)
        {
            await Task.Delay(200);

            new Thread(async () =>
            {
                try
                {
                    var db = ubgeBotClient.localDB;
                    var collectionCheckBotAberto = db.GetCollection<CheckBotAberto>(Valores.Mongo.checkBotAberto);

                    var filtro = Builders<CheckBotAberto>.Filter.Empty;

                    var lista = await (await collectionCheckBotAberto.FindAsync<CheckBotAberto>(filtro)).ToListAsync();

                    if (lista.Count == 0)
                        await collectionCheckBotAberto.InsertOneAsync(new CheckBotAberto { numero = 0, diaEHora = DateTime.Now });
                    else
                        await collectionCheckBotAberto.UpdateOneAsync(filtro, Builders<CheckBotAberto>.Update.Set(x => x.numero, 0).Set(y => y.diaEHora, DateTime.Now));
                }
                catch (Exception exception)
                {
                    await ubgeBotClient.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Sistemas, exception);
                }
            }).Start();
        }

        private static async Task ChecaCanaisAutoCreate(UBGEBot_ ubgeBotClient)
        {
            await Task.Delay(200);

            new Thread(async () => 
            {
                try
                {
                    if (!checkDosCanaisFoiIniciado)
                        checkDosCanaisFoiIniciado = true;

                    DiscordGuild UBGE = await ubgeBotClient.discordClient.GetGuildAsync(Valores.Guilds.UBGE);

                    DiscordMember botUBGE = await UBGE.GetMemberAsync(Valores.Guilds.Membros.ubgeBot);

                    DiscordChannel categoriaOutrosCanais = UBGE.GetChannel(Valores.ChatsUBGE.Categorias.categoriaCliqueAqui);
                    var canaisDaCategoria = categoriaOutrosCanais.Children.Where(x => x.Type == ChannelType.Voice).ToList();

                    var canalErrado = canaisDaCategoria.Find(x => x.Name.ToUpper().Contains("SALA CRIADA!"));

                    string nomeCliqueAqui = "📌 Clique aqui!";

                    if (canaisDaCategoria.Contains(canalErrado))
                    {
                        foreach (var canal in canaisDaCategoria)
                        {
                            if (canal == canalErrado)
                            {
                                await canal.ModifyAsync(x => x.Name = nomeCliqueAqui);

                                return;
                            }
                        }
                    }

                    DiscordChannel cliqueAquiVoz = canaisDaCategoria.Find(x => x.Id == Valores.ChatsUBGE.canalCliqueAqui);
                    DiscordChannel batePapo = canaisDaCategoria.Find(x => x.Id == Valores.ChatsUBGE.canalBatePapo);

                    if (cliqueAquiVoz == null)
                        await UBGE.CreateChannelAsync(nomeCliqueAqui, ChannelType.Voice, categoriaOutrosCanais.Parent);

                    canaisDaCategoria.Remove(cliqueAquiVoz);
                    canaisDaCategoria.Remove(batePapo);

                    if (cliqueAquiVoz.Users.Count() != 0)
                    {
                        foreach (var membro in cliqueAquiVoz.Users)
                        {
                            await Task.Delay(200);

                            await membro.PlaceInAsync(batePapo);
                        }
                    }

                    if (canaisDaCategoria.Count() != 0)
                    {
                        foreach (var canal in canaisDaCategoria)
                        {
                            if (canal.Users.Count() == 0 && canal.PermissionsFor(botUBGE).HasFlag(Permissions.ManageChannels))
                            {
                                await Task.Delay(200);

                                await canal.DeleteAsync();
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    await ubgeBotClient.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                }
            }).Start();
        }

        private static async Task FazOCacheDosEmojis(UBGEBot_ ubgeBotClient)
        {
            await Task.Delay(200);

            new Thread(async () =>
            {
                try
                {
                    List<string> nomesEmojis = new List<string>
                    {
                        "YoutubeLogo", "DiscordLogo", "SiteLogo", "RedeSociaisLogo", "SteamLogo", "AmigosLogo",
                        "GooglePlayLogo", "parceiros", "nossos_servidores", "PR", "Foxhole", "BF", "Os", "Squad",
                        "Minecraft", "Unturned", "Gmod", "WarThunder", "PUBG", "Lol", "Rust", "csgo", "Paladins",
                        "WildTerra", "BattleRush", "HG", "RTS", "Lif", "AlbionOnline", "regiao_norte", "bandeira_amazonas",
                        "bandeira_roraima", "bandeira_amapa", "bandeira_para", "bandeira_tocantins", "bandeira_rondonia",
                        "bandeira_acre", "regiao_nordeste", "regiao_nordeste", "bandeira_maranhao", "bandeira_piaui",
                        "bandeira_ceara", "bandeira_rio_grande_do_norte", "bandeira_pernambuco", "bandeira_paraiba",
                        "bandeira_sergipe", "bandeira_alagoas", "bandeira_bahia", "regiao_centro_oeste", "bandeira_mato_grosso",
                        "bandeira_mato_grosso_do_sul", "bandeira_goias", "bandeira_distrito_federal", "regiao_sudeste",
                        "bandeira_sao_paulo", "bandeira_rio_de_janeiro", "bandeira_espirito_santo", "bandeira_minas_gerais",
                        "regiao_sul", "bandeira_parana", "bandeira_rio_grande_do_sul", "bandeira_santa_catarina", "exterior"
                    };

                    emojisCache.AddRange(await ubgeBotClient.utilidadesGerais.RetornaEmojis(ubgeBotClient.discordClient, nomesEmojis));
                }
                catch (Exception exception)
                {
                    await ubgeBotClient.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Sistemas, exception);
                }
            }).Start();
        }

        private static async Task VerificaPrisoesQuandoOBotInicia(UBGEBot_ ubgeBotClient)
        {
            await Task.Delay(200);

            new Thread(async () =>
            {
                try
                {
                    var db = ubgeBotClient.localDB;
                    var collectionInfracoes = db.GetCollection<Infracao>(Valores.Mongo.infracoes);

                    DiscordGuild UBGE = await ubgeBotClient.discordClient.GetGuildAsync(Valores.Guilds.UBGE);

                    DiscordRole prisioneiroCargo = UBGE.GetRole(Valores.Cargos.cargoPrisioneiro), cargosMembroForeach = null;

                    DiscordChannel ubgeBot = UBGE.GetChannel(Valores.ChatsUBGE.canalUBGEBot);

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    StringBuilder strCargos = new StringBuilder();

                    foreach (var membroPrisao in UBGE.Members.Values.Where(x => x.Roles.Contains(prisioneiroCargo)))
                    {
                        var filtro = Builders<Infracao>.Filter.Eq(x => x.idInfrator, membroPrisao.Id);
                        var listaInfracoes = await (await collectionInfracoes.FindAsync(filtro)).ToListAsync();

                        if (listaInfracoes.Count == 0 || !listaInfracoes.LastOrDefault().oMembroFoiPreso)
                            continue;

                        var ultimaPrisao = listaInfracoes.LastOrDefault();

                        var diaHoraInfracao = Convert.ToDateTime(ultimaPrisao.dataInfracao.ToString());
                        var tempoPrisao = ubgeBotClient.utilidadesGerais.ConverterTempo(ultimaPrisao.dadosPrisao.tempoDoMembroNaPrisao);

                        if (diaHoraInfracao.Add(tempoPrisao) < DateTime.Now)
                        {
                            await membroPrisao.RevokeRoleAsync(prisioneiroCargo);

                            foreach (var cargos in ultimaPrisao.dadosPrisao.cargosDoMembro)
                            {
                                await Task.Delay(200);

                                cargosMembroForeach = UBGE.GetRole(cargos);

                                await membroPrisao.GrantRoleAsync(cargosMembroForeach);

                                strCargos.Append($"{cargosMembroForeach.Mention} | ");
                            }

                            embed.WithAuthor($"O membro: \"{ubgeBotClient.utilidadesGerais.RetornaNomeDiscord(membroPrisao)}#{membroPrisao.Discriminator}\", saiu da prisão.", null, Valores.logoUBGE)
                                .WithColor(ubgeBotClient.utilidadesGerais.CorAleatoriaEmbed())
                                .WithDescription($"Cargos devolvidos: {strCargos.ToString()}")
                                .WithThumbnailUrl(membroPrisao.AvatarUrl)
                                .WithTimestamp(DateTime.Now)
                                .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membroPrisao)}", iconUrl: membroPrisao.AvatarUrl);

                            ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, $"O membro: {ubgeBotClient.utilidadesGerais.RetornaNomeDiscord(membroPrisao)}#{membroPrisao.Discriminator}, saiu da prisão.");
                            await ubgeBot.SendMessageAsync(embed: embed.Build());
                        }
                    }
                }
                catch (Exception exception) 
                {
                    await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Sistemas, exception);
                }
            }).Start();
        }
        
        private async Task EnviaMensagemPraODiscordDeConexaoNoMongo(UBGEBot_ ubgeBotClient)
        {
            await Task.Delay(200);

            new Thread(async () =>
            {
                try
                {
                    if (!ubgeBotClient.botConectadoAoMongo)
                    {
                        DiscordGuild UBGE = await ubgeBotClient.discordClient.GetGuildAsync(Valores.Guilds.UBGE);

                        DiscordChannel canal = UBGE.GetChannel(Valores.ChatsUBGE.canalUBGEBot);

                        if (!(await canal.GetMessagesAsync(1)).LastOrDefault().Content.Contains("Não foi possível conectar ao MongoDB!"))
                            await canal.SendMessageAsync("Não foi possível conectar ao MongoDB! Alguns comandos podem estar indisponíveis. :cry:");
                    }
                }
                catch (Exception exception)
                {
                    await ubgeBotClient.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Sistemas, exception);
                }
            }).Start();
        }



        private static async Task BuscaServidoresPR(UBGEBot_ ubgeBotClient, HttpClient httpClient)
        {
            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    var linkPRSpy = await httpClient.GetStringAsync("https://www.realitymod.com/prspy/json/serverdata.json");
                    var resposta = (JObject)JsonConvert.DeserializeObject(linkPRSpy);
                    var listaResposta = (JArray)resposta.SelectToken("Data");

                    var db = ubgeBotClient.localDB;
                    var servidoresDB = db.GetCollection<ServidoresUBGE>(Valores.Mongo.servidoresUBGE);

                    var Filtro = Builders<ServidoresUBGE>.Filter.Eq(x => x.jogo, "pr");
                    var resultadosLista = await (await servidoresDB.FindAsync(Filtro)).ToListAsync();

                    if (resultadosLista.Count > 0)
                        await servidoresDB.DeleteManyAsync(Filtro);

                    int N = 0;

                    foreach (var server in listaResposta)
                    {
                        var ipServidor = server.SelectToken("IPAddress").ToString();
                        var queryPort = server.SelectToken("QueryPort").ToString();
                        var paisServidor = server.SelectToken("Country").ToString();
                        var nomeServidor = server.SelectToken("ServerName").ToString();
                        var nomeJogo = server.SelectToken("GameName").ToString();
                        var versaoJogo = server.SelectToken("GameVersion").ToString();
                        var jogoPorta = server.SelectToken("GamePort").ToString();
                        var nomeMapa = server.SelectToken("MapName").ToString();
                        var modoDeJogo = server.SelectToken("GameMode").ToString();
                        var playersJogando = server.SelectToken("NumPlayers").ToString();
                        var maxPlayers = server.SelectToken("MaxPlayers").ToString();
                        var statusJogo = server.SelectToken("GameStatus").ToString();

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
                                fotoDoServidor = Valores.prLogoNucleo,
                                ipDoServidor = ipServidor,
                                portaDoServidor = $"{queryPort} ou {jogoPorta}",
                                statusDoServidor = statusJogo,
                                thumbnailDoServidor = Valores.logoUBGE,
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
                    ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Servidores, $"[ProjectReality-Servidores] A busca dos servidores Project Reality gerou um erro! Tudo normal, ainda continuo procurando...");
                }
            }).Start();
        }

        private static async Task BuscaServidoresConanExiles(UBGEBot_ ubgeBotClient, HttpClient httpClient)
        {
            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    var linkBattleMetrics = await httpClient.GetStringAsync("https://api.battlemetrics.com/servers?filter[game]=conanexiles");
                    var respostaJson = (JObject)JsonConvert.DeserializeObject(linkBattleMetrics);
                    var listaResposta = (JArray)respostaJson.SelectToken("data");

                    var db = ubgeBotClient.localDB;
                    var servidoresUBGE = db.GetCollection<ServidoresUBGE>(Valores.Mongo.servidoresUBGE);
                    var Filtro = Builders<ServidoresUBGE>.Filter.Eq(x => x.jogo, "ce");
                    var resultados = await (await servidoresUBGE.FindAsync(Filtro)).ToListAsync();

                    if (resultados.Count > 0)
                        await servidoresUBGE.DeleteManyAsync(Filtro);

                    foreach (var servidor in listaResposta)
                    {
                        var propServidor = servidor.SelectToken("attributes"); 
                        var nomeServidor = propServidor.SelectToken("name").ToString();
                        var ipServidor = propServidor.SelectToken("ip").ToString();
                        var portaServidor = propServidor.SelectToken("port").ToString();
                        var playersJogando = propServidor.SelectToken("players").ToString();
                        var maxPlayers = propServidor.SelectToken("maxPlayers").ToString();
                        var statusServidor = propServidor.SelectToken("status").ToString();
                        var portaQuery = propServidor.SelectToken("portQuery").ToString();
                        var paisServidor = propServidor.SelectToken("country").ToString();

                        foreach (var nomeServidorConan in ubgeBotClient.ubgeBotConfig.ubgeBotServidoresConfig.nomeDoServidorDeConan)
                        {
                            if (nomeServidor.ToString().ToUpper().Contains(nomeServidorConan.ToUpper()))
                            {
                                await servidoresUBGE.InsertOneAsync(new ServidoresUBGE
                                {
                                    jogo = "ce",
                                    jogadoresDoServidor = playersJogando,
                                    mapaDoServidor = "Não especificado.",
                                    maximoDePlayers = maxPlayers,
                                    modoDeJogo = "Não especificado.",
                                    paisDoServidor = paisServidor,
                                    versaoDoJogo = "Não especificado.",
                                    nomeDoServidor = nomeServidor,
                                    fotoDoServidor = Valores.logoUBGE,
                                    ipDoServidor = ipServidor,
                                    portaDoServidor = $"{portaServidor} ou {portaQuery}",
                                    statusDoServidor = statusServidor,
                                    thumbnailDoServidor = Valores.conanExilesLogoRuinasDeAstapor,
                                    servidorDisponivel = $"Conan Exiles = `servidores ce`",
                                    nomeServidorParaComando = "Conan Exiles",
                                    _id = new ObjectId()
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Servidores, "[ConanExiles-Servidores] A busca dos servidores Conan Exiles gerou um erro! Tudo normal, ainda continuo procurando...");
                }
            }).Start();
        }

        private static async Task BuscaServidoresDayZ(UBGEBot_ ubgeBotClient, HttpClient httpClient)
        {
            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    var linkBattleMetrics = await httpClient.GetStringAsync("https://api.battlemetrics.com/servers?filter[game]=dayz");
                    var resposta = (JObject)JsonConvert.DeserializeObject(linkBattleMetrics);
                    var listaResposta = (JArray)resposta.SelectToken("data");

                    var db = ubgeBotClient.localDB;
                    var servidoresUBGE = db.GetCollection<ServidoresUBGE>(Valores.Mongo.servidoresUBGE);
                    var Filtro = Builders<ServidoresUBGE>.Filter.Eq(x => x.jogo, "dyz");
                    var resultados = await (await servidoresUBGE.FindAsync(Filtro)).ToListAsync();

                    if (resultados.Count > 0)
                        await servidoresUBGE.DeleteManyAsync(Filtro);

                    foreach (var servidor in listaResposta)
                    {
                        var propServidor = servidor.SelectToken("attributes");
                        var nomeServidor = propServidor.SelectToken("name").ToString();
                        var ipServidor = propServidor.SelectToken("ip").ToString();
                        var portaServidor = propServidor.SelectToken("port").ToString();
                        var playersJogando = propServidor.SelectToken("players").ToString();
                        var maxPlayers = propServidor.SelectToken("maxPlayers").ToString();
                        var statusServidor = propServidor.SelectToken("status").ToString();
                        var portaQuery = propServidor.SelectToken("portQuery").ToString();
                        var paisServidor = propServidor.SelectToken("country").ToString();
                        var detalhesServidor = propServidor.SelectToken("details");
                        var versaoServidor = detalhesServidor.SelectToken("version").ToString();

                        foreach (var NomeServidorDayZ in ubgeBotClient.ubgeBotConfig.ubgeBotServidoresConfig.nomeDoServidorDeDayZ)
                        {
                            if (nomeServidor.ToString().ToUpper().Contains(NomeServidorDayZ.ToUpper()))
                            {
                                await servidoresUBGE.InsertOneAsync(new ServidoresUBGE
                                {
                                    jogo = "dyz",
                                    jogadoresDoServidor = playersJogando,
                                    mapaDoServidor = "Não especificado.",
                                    maximoDePlayers = maxPlayers,
                                    modoDeJogo = "Não especificado.",
                                    paisDoServidor = paisServidor,
                                    versaoDoJogo = versaoServidor,
                                    nomeDoServidor = nomeServidor,
                                    fotoDoServidor = Valores.dayZLogo,
                                    ipDoServidor = ipServidor,
                                    portaDoServidor = $"{portaServidor} ou {portaQuery}",
                                    statusDoServidor = statusServidor.ToString(),
                                    thumbnailDoServidor = Valores.logoUBGE,
                                    servidorDisponivel = $"Day Z = `servidores dyz`",
                                    nomeServidorParaComando = "Day Z",
                                    _id = new ObjectId()
                                });
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Servidores, "[DayZ-Servidores] A busca dos servidores Day Z gerou um erro! Tudo normal, ainda continuo procurando...");
                }
            }).Start();
        }

        private static async Task BuscaServidoresOpenSpades(UBGEBot_ ubgeBotClient, HttpClient httpClient)
        {
            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    var respostaBuildAndShoot = ubgeBotClient.utilidadesGerais.ByteParaString(await (await httpClient.GetAsync($"http://services.buildandshoot.com/serverlist.json")).Content.ReadAsByteArrayAsync());
                    var jArray = (JArray)JsonConvert.DeserializeObject(respostaBuildAndShoot);

                    var db = ubgeBotClient.localDB;
                    var servidoresUBGE = db.GetCollection<ServidoresUBGE>(Valores.Mongo.servidoresUBGE);
                    var filtro = Builders<ServidoresUBGE>.Filter.Eq(x => x.jogo, "os");
                    var resultados = await (await servidoresUBGE.FindAsync(filtro)).ToListAsync();

                    if (resultados.Count > 0)
                        await servidoresUBGE.DeleteManyAsync(filtro);

                    foreach (var propServidor in jArray)
                    {
                        var nomeServidor = propServidor.SelectToken("name").ToString();
                        var ipServidor = propServidor.SelectToken("identifier").ToString();
                        var mapaServidor = propServidor.SelectToken("map").ToString();
                        var modoDeJogoServidor = propServidor.SelectToken("game_mode").ToString();
                        var playersJogando = propServidor.SelectToken("players_current").ToString();
                        var maxPlayers = propServidor.SelectToken("players_max").ToString();
                        var versaoJogo = propServidor.SelectToken("game_version").ToString();
                        var paisServidor = propServidor.SelectToken("country").ToString();

                        foreach (var nomeServidorOpenSpades in ubgeBotClient.ubgeBotConfig.ubgeBotServidoresConfig.nomeDoServidorDeOpenSpades)
                        {
                            if (nomeServidor.ToUpper().Contains(nomeServidorOpenSpades.ToUpper()))
                            {
                                await servidoresUBGE.InsertOneAsync(new ServidoresUBGE
                                {
                                    jogo = "os",
                                    jogadoresDoServidor = playersJogando,
                                    mapaDoServidor = mapaServidor,
                                    maximoDePlayers = maxPlayers,
                                    modoDeJogo = modoDeJogoServidor,
                                    paisDoServidor = paisServidor,
                                    versaoDoJogo = versaoJogo,
                                    nomeDoServidor = nomeServidor,
                                    fotoDoServidor = Valores.openSpadesLogo,
                                    ipDoServidor = ipServidor,
                                    portaDoServidor = ipServidor.Split(':')[2],
                                    statusDoServidor = "Online.",
                                    thumbnailDoServidor = Valores.logoUBGE,
                                    servidorDisponivel = $"OpenSpades = `servidores os`",
                                    nomeServidorParaComando = "OpenSpades",
                                    _id = new ObjectId()
                                });
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Servidores, "[OpenSpades-Servidores] A busca dos servidores OpenSpades gerou um erro! Tudo normal, ainda continuo procurando...");
                }
            }).Start();
        }

        private static async Task BuscaServidoresCounterStrike(UBGEBot_ ubgeBotClient, HttpClient httpClient)
        {
            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    var respostaBattleMetrics = await httpClient.GetStringAsync("https://api.battlemetrics.com/servers?filter[game]=cs");
                    var deserializeJson = (JObject)JsonConvert.DeserializeObject(respostaBattleMetrics);
                    var listaServidores = (JArray)deserializeJson.SelectToken("data");

                    var db = ubgeBotClient.localDB;
                    var servidoresUBGE = db.GetCollection<ServidoresUBGE>(Valores.Mongo.servidoresUBGE);
                    var filtro = Builders<ServidoresUBGE>.Filter.Eq(x => x.jogo, "cs");
                    var resultados = await (await servidoresUBGE.FindAsync(filtro)).ToListAsync();

                    if (resultados.Count > 0)
                        await servidoresUBGE.DeleteManyAsync(filtro);

                    foreach (var servidor in listaServidores)
                    {
                        var propServidor = servidor.SelectToken("attributes");
                        var nomeServidor = propServidor.SelectToken("name").ToString();
                        var ipServidor = propServidor.SelectToken("ip").ToString();
                        var portaServidor = propServidor.SelectToken("port").ToString();
                        var playersJogando = propServidor.SelectToken("players").ToString();
                        var maxPlayers = propServidor.SelectToken("maxPlayers").ToString();
                        var statusServidor = propServidor.SelectToken("status").ToString();
                        var paisServidor = propServidor.SelectToken("country").ToString();
                        var detalhes = propServidor.SelectToken("details");
                        var nomeMapa = detalhes.SelectToken("map").ToString();
                        var versaoJogo = detalhes.SelectToken("version").ToString();

                        foreach (var NomeServidorCounterStrike in ubgeBotClient.ubgeBotConfig.ubgeBotServidoresConfig.nomeDoServidorDeCounterStrike)
                        {
                            if (nomeServidor.ToString().ToUpper().Contains(NomeServidorCounterStrike.ToUpper()))
                            {
                                await servidoresUBGE.InsertOneAsync(new ServidoresUBGE
                                {
                                    jogo = "cs",
                                    jogadoresDoServidor = playersJogando,
                                    mapaDoServidor = nomeMapa,
                                    maximoDePlayers = maxPlayers,
                                    modoDeJogo = "Não especificado.",
                                    paisDoServidor = paisServidor,
                                    versaoDoJogo = versaoJogo,
                                    nomeDoServidor = nomeServidor,
                                    fotoDoServidor = Valores.counterStrikeLogo,
                                    ipDoServidor = $"ubge.ddns.net ou {ipServidor}",
                                    portaDoServidor = portaServidor,
                                    statusDoServidor = statusServidor,
                                    thumbnailDoServidor = Valores.logoUBGE,
                                    servidorDisponivel = $"Counter-Strike = `servidores cs`",
                                    nomeServidorParaComando = "Counter-Strike",
                                    _id = new ObjectId()
                                });
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Servidores, "[CounterStrike-Servidores] A busca dos servidores Counter-Strike gerou um erro! Tudo normal, ainda continuo procurando...");
                }
            }).Start();
        }

        private static async Task BuscaServidoresUnturned(UBGEBot_ ubgeBotClient, HttpClient httpClient)
        {
            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    var respostaBattleMetrics = await httpClient.GetStringAsync("https://api.battlemetrics.com/servers?filter[game]=unturned");
                    var deserializeJson = (JObject)JsonConvert.DeserializeObject(respostaBattleMetrics);
                    var listaServidores = (JArray)deserializeJson.SelectToken("data");

                    var db = ubgeBotClient.localDB;
                    var servidoresUBGE = db.GetCollection<ServidoresUBGE>(Valores.Mongo.servidoresUBGE);
                    var filtro = Builders<ServidoresUBGE>.Filter.Eq(x => x.jogo, "unturned");
                    var resultados = await (await servidoresUBGE.FindAsync(filtro)).ToListAsync();

                    if (resultados.Count > 0)
                        await servidoresUBGE.DeleteManyAsync(filtro);

                    foreach (var Servidor in listaServidores)
                    {
                        var propServidor = Servidor.SelectToken("attributes");
                        var nomeServidor = propServidor.SelectToken("name").ToString();
                        var ipServidor = propServidor.SelectToken("ip").ToString();
                        var portaServidor = propServidor.SelectToken("port").ToString();
                        var playersJogando = propServidor.SelectToken("players").ToString();
                        var maxPlayers = propServidor.SelectToken("maxPlayers").ToString();
                        var statusServidor = propServidor.SelectToken("status").ToString();
                        var portaQuery = propServidor.SelectToken("portQuery").ToString();
                        var paisServidor = propServidor.SelectToken("country").ToString();
                        var detalhes = propServidor.SelectToken("details");
                        var versaoServidor = detalhes.SelectToken("version").ToString();
                        var modoDeJogo = detalhes.SelectToken("gameMode").ToString();
                        var mapaServidor = detalhes.SelectToken("map").ToString();

                        foreach (var NomeServidorUnturned in ubgeBotClient.ubgeBotConfig.ubgeBotServidoresConfig.nomeDoServidorDeUnturned)
                        {
                            if (nomeServidor.ToString().ToUpper().Contains(NomeServidorUnturned.ToUpper()))
                            {
                                await servidoresUBGE.InsertOneAsync(new ServidoresUBGE
                                {
                                    jogo = "unturned",
                                    jogadoresDoServidor = playersJogando,
                                    mapaDoServidor = mapaServidor,
                                    maximoDePlayers = maxPlayers,
                                    modoDeJogo = modoDeJogo,
                                    paisDoServidor = paisServidor,
                                    versaoDoJogo = versaoServidor,
                                    nomeDoServidor = nomeServidor,
                                    fotoDoServidor = Valores.unturnedLogo,
                                    ipDoServidor = ipServidor,
                                    portaDoServidor = $"{portaServidor} ou {portaQuery}",
                                    statusDoServidor = statusServidor,
                                    thumbnailDoServidor = Valores.logoUBGE,
                                    servidorDisponivel = $"Unturned = `servidores unturned`",
                                    nomeServidorParaComando = "Unturned",
                                    _id = new ObjectId()
                                });
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Servidores, "[Unturned-Servidores] A busca dos servidores Unturned gerou um erro! Tudo normal, ainda continuo procurando...");
                }
            }).Start();
        }

        private static async Task BuscaServidoresMordhau(UBGEBot_ ubgeBotClient, HttpClient httpClient)
        {
            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    var respostaBattleMetrics = await httpClient.GetStringAsync("https://api.battlemetrics.com/servers?filter[game]=mordhau");
                    var deserializeJson = (JObject)JsonConvert.DeserializeObject(respostaBattleMetrics);
                    var listaServidores = (JArray)deserializeJson.SelectToken("data");

                    var db = ubgeBotClient.localDB;
                    var servidoresUBGE = db.GetCollection<ServidoresUBGE>(Valores.Mongo.servidoresUBGE);
                    var filtro = Builders<ServidoresUBGE>.Filter.Eq(x => x.jogo, "mordhau");
                    var resultados = await (await servidoresUBGE.FindAsync(filtro)).ToListAsync();

                    if (resultados.Count > 0)
                        await servidoresUBGE.DeleteManyAsync(filtro);

                    foreach (var servidor in listaServidores)
                    {
                        var propServidor = servidor.SelectToken("attributes");
                        var nomeServidor = propServidor.SelectToken("name").ToString();
                        var ipServidor = propServidor.SelectToken("ip").ToString();
                        var portaServidor = propServidor.SelectToken("port").ToString();
                        var playersJogando = propServidor.SelectToken("players").ToString();
                        var maxPlayers = propServidor.SelectToken("maxPlayers").ToString();
                        var statusServidor = propServidor.SelectToken("status").ToString();
                        var portaQuery = propServidor.SelectToken("portQuery").ToString();
                        var paisServidor = propServidor.SelectToken("country").ToString();
                        var detalhesServidor = propServidor.SelectToken("details");
                        var mapaServidor = detalhesServidor.SelectToken("map").ToString();
                        var modoDeJogo = detalhesServidor.SelectToken("gameMode").ToString();

                        foreach (var NomeServidorMordhau in ubgeBotClient.ubgeBotConfig.ubgeBotServidoresConfig.nomeDoServidorDeMordhau)
                        {
                            if (nomeServidor.ToString().ToUpper().Contains(NomeServidorMordhau.ToUpper()))
                            {
                                await servidoresUBGE.InsertOneAsync(new ServidoresUBGE
                                {
                                    jogo = "mordhau",
                                    jogadoresDoServidor = playersJogando,
                                    mapaDoServidor = mapaServidor,
                                    maximoDePlayers = maxPlayers,
                                    modoDeJogo = modoDeJogo,
                                    paisDoServidor = paisServidor,
                                    versaoDoJogo = "Não especificado.",
                                    nomeDoServidor = nomeServidor,
                                    fotoDoServidor = Valores.mordhauLogo,
                                    ipDoServidor = ipServidor,
                                    portaDoServidor = $"{portaServidor} ou {portaQuery}",
                                    statusDoServidor = statusServidor.ToString(),
                                    thumbnailDoServidor = Valores.logoUBGE,
                                    servidorDisponivel = $"Mordhau = `servidores mordhau`",
                                    nomeServidorParaComando = "Mordhau",
                                    _id = new ObjectId()
                                });
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Servidores, "[Mordhau-Servidores] A busca dos servidores Mordhau gerou um erro! Tudo normal, ainda continuo procurando...");
                }
            }).Start();
        }


        public static void ShutdownBot()
            => Environment.Exit(1);
    }
}