using Autofac;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using UBGEBot.Carregamento;
using UBGEBot.Utilidades;
using UBGEBot.MongoDB.Modelos;
using UBGEBot.LogExceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Timer = System.Timers.Timer;
using MongoDB.Driver;
using System.IO;
using System.Text;

namespace UBGEBot.Main
{
    public sealed class Program
    {
        public static Program instanciaMain { get; private set; }
        public static UBGEBot_ ubgeBot { get; private set; } = new UBGEBot_();

        private static async Task Main(string[] args)
        {
            try 
            {
                //ContainerBuilder containerBuilder = new ContainerBuilder
                //{

                //};

                //bot.services = containerBuilder.Build();

                Timer TimerServidores = new Timer()
                {
                    Interval = 10000,
                };
                TimerServidores.Elapsed += delegate
                {
                    //BuscaServidoresPR(Client);
                    //BuscaServidoresDayZ(Client);
                    //BuscaServidoresConanExiles(Client);
                    //BuscaServidoresOpenSpades(Client);
                    //BuscaServidoresCounterStrike(Client);
                    //BuscaServidoresUnturned(Client);
                    //BuscaServidoresMordhau(Client);
                };
                TimerServidores.Start();

                Timer TimerComandos = new Timer()
                {
                    Interval = 43200000, //1 dia = 86400000ms | 12 horas = 43200000ms
                };
                TimerComandos.Elapsed += async delegate
                {
                    await ExecutaListAutomaticamente(ubgeBot);
                };
                TimerComandos.Start();

                Timer TimerVoiceState = new Timer()
                {
                    Interval = 30000,
                };
                TimerVoiceState.Elapsed += async delegate
                {
                    await ChecaCanaisAutoCreate(ubgeBot);
                };
                TimerVoiceState.Start();

                Timer TimerModuloChecarBotAberto = new Timer()
                {
                    Interval = 10000,
                };
                TimerModuloChecarBotAberto.Elapsed += async delegate
                {
                    await ModuloBotAberto(ubgeBot);
                };
                TimerModuloChecarBotAberto.Start();

                await EnviaDadosDoReactRole(ubgeBot);

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
            ubgeBotClient.discordClient.Ready += DiscordIniciado;
            ubgeBotClient.discordClient.MessageReactionAdded += ReacaoAdicionadaReactRole;
            ubgeBotClient.discordClient.MessageReactionRemoved += ReacaoRemovidaReactRole;
            ubgeBotClient.discordClient.MessageCreated += MensagemCriada;
            ubgeBotClient.discordClient.VoiceStateUpdated += CanalDeVozPersonalizado;
            ubgeBotClient.discordClient.GuildBanAdded += NovoBan;
            ubgeBotClient.discordClient.GuildBanRemoved += BanRetirado;
            ubgeBotClient.discordClient.SocketClosed += BotCaiu;
            ubgeBotClient.discordClient.SocketErrored += BotCaiuEErroNoSocket;
            ubgeBotClient.discordClient.GuildMemberAdded += MembroEntra;
            ubgeBotClient.discordClient.GuildDownloadCompleted += DownloadsDosServidoresFoiConcluido;
        }

        public async Task DiscordIniciado(ReadyEventArgs readyEventArgs) 
            => await readyEventArgs.Client.UpdateStatusAsync(new DiscordActivity { Name = "Bem-Vindo a UBGE!", ActivityType = ActivityType.Playing });

        public async Task BotCaiu(SocketCloseEventArgs socketCloseEventArgs) 
            => await ConectarEReconectarBotAoDiscordAsync(ubgeBot, true);

        public async Task BotCaiuEErroNoSocket(SocketErrorEventArgs socketErrorEventArgs) 
            => await ConectarEReconectarBotAoDiscordAsync(ubgeBot, true);
        
        public async Task DownloadsDosServidoresFoiConcluido(GuildDownloadCompletedEventArgs guildDownloadCompletedEventArgs) 
        {
            await ConfiguracaoDiscord(ubgeBot);
            await CheckReacoesMarcadasQuandoOBotEstavaOfflineNoReactRole(ubgeBot);
        }
        
        public async Task ReacaoAdicionadaReactRole(MessageReactionAddEventArgs messageReactionAddEventArgs)
        {
            await Task.Delay(200);

            new Thread(async () => 
            {
                try 
                {
                    if (!messageReactionAddEventArgs.Channel.IsPrivate && messageReactionAddEventArgs.Guild.Id == Valores.Guilds.UBGE)
                    {
                        var client = ubgeBot.mongoClient;
                        var db = client.GetDatabase(Valores.Mongo.local);
                        
                        var msgs = db.GetCollection<Reacts>(Valores.Mongo.reacts);
                        var dbContar = db.GetCollection<ContaMembrosQuePegaramCargos>(Valores.Mongo.contaMembrosQuePegaramCargos);
                        
                        var resultadosMensagens = await (await msgs.FindAsync(Builders<Reacts>.Filter.Empty)).ToListAsync();

                        DiscordMember member = await messageReactionAddEventArgs.Channel.Guild.GetMemberAsync(messageReactionAddEventArgs.User.Id);
                        DiscordRole acessoGeral = messageReactionAddEventArgs.Channel.Guild.GetRole(Valores.Cargos.cargoAcessoGeral);

                        ConcurrentDictionary<ulong, string> dict = new ConcurrentDictionary<ulong, string>();
                        ConcurrentDictionary<ulong, ulong> e2r = new ConcurrentDictionary<ulong, ulong>();

                        foreach (var x in resultadosMensagens)
                            dict.TryAdd(x.IdMensagens, x.Categoria);

                        if (dict.Keys.Contains(messageReactionAddEventArgs.Message.Id))
                        {
                            if (!messageReactionAddEventArgs.User.IsBot)
                            {
                                var jogos = db.GetCollection<Jogos>(Valores.Mongo.jogos);
                                var filtro = Builders<Jogos>.Filter.Eq(x => x.NomeDaCategoria, dict[messageReactionAddEventArgs.Message.Id]);
                                var resultados = await (await jogos.FindAsync(filtro)).ToListAsync();

                                foreach (var y in resultados)
                                    e2r.TryAdd(y.IdDoEmoji, y.CargoID);

                                if (e2r.ContainsKey(messageReactionAddEventArgs.Emoji.Id))
                                {
                                    if (messageReactionAddEventArgs.Emoji.Id == Valores.Guilds.emojiUBGERemoverCargo && member.Roles.Contains(acessoGeral))
                                    {
                                        await member.RevokeRoleAsync(acessoGeral);

                                        return;
                                    }
                                    else if (messageReactionAddEventArgs.Emoji.Id == Valores.Guilds.emojiUBGERemoverCargo && !member.Roles.Contains(acessoGeral))
                                        return;
                                    else
                                    {
                                        DiscordRole role = messageReactionAddEventArgs.Channel.Guild.GetRole(e2r[messageReactionAddEventArgs.Emoji.Id]);

                                        if (member.IsBot)
                                            return;
                                        else
                                        {
                                            await member.GrantRoleAsync(role);

                                            var filtroDBContar = Builders<ContaMembrosQuePegaramCargos>.Filter.Eq(x => x.Jogo, role.Name);
                                            var resultadosDBContar = await (await dbContar.FindAsync(filtroDBContar)).ToListAsync();

                                            if (resultadosDBContar.Count == 0)
                                                await dbContar.InsertOneAsync(new ContaMembrosQuePegaramCargos { Jogo = role.Name, NumeroPessoas = 1, IdsMembrosQuePegaramOCargo = new List<ulong> { member.Id } });
                                            else
                                            {
                                                if (!resultadosDBContar.FirstOrDefault().IdsMembrosQuePegaramOCargo.Contains(member.Id))
                                                {
                                                    if (resultadosDBContar.FirstOrDefault().IdsMembrosQuePegaramOCargo.Contains(1))
                                                        resultadosDBContar.FirstOrDefault().IdsMembrosQuePegaramOCargo.Remove(1);

                                                    var Update = Builders<ContaMembrosQuePegaramCargos>.Update
                                                        .Set(x => x.NumeroPessoas, resultadosDBContar.FirstOrDefault().NumeroPessoas + 1)
                                                        .Set(y => y.IdsMembrosQuePegaramOCargo, resultadosDBContar.FirstOrDefault().IdsMembrosQuePegaramOCargo.Append(member.Id));

                                                    await dbContar.UpdateOneAsync(filtroDBContar, Update);
                                                }
                                            }

                                            if (messageReactionAddEventArgs.Channel.Guild.Id == Valores.Guilds.UBGE)
                                                await ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.ReactRoleUBGE, 
                                                    "Cargo Adicionado!", $"{messageReactionAddEventArgs.Emoji} | O usuário: {member.Mention} pegou o cargo de: {role.Mention}.\n\n" +
                                                    $"Ou:\n- `@{(string.IsNullOrWhiteSpace(member.Nickname) ? member.Username : member.Nickname)}#{member.Discriminator}`\n" +
                                                    $"- `@{role.Name}`", (await messageReactionAddEventArgs.Channel.Guild.GetMemberAsync(Valores.Guilds.Membros.ubgeBot)).AvatarUrl, 
                                                    member);
                                            else
                                                await ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.ReactRoleForaDaUBGE, 
                                                    "Cargo Adicionado!", $"{messageReactionAddEventArgs.Emoji} | O usuário: **{(string.IsNullOrWhiteSpace(member.Nickname) ? member.Username : member.Nickname)}#{member.Discriminator}** pegou o cargo de: **{role.Name}**.", 
                                                    messageReactionAddEventArgs.Channel.Guild.IconUrl, member);
                                        }
                                    }
                                }
                                else 
                                {
                                    int i = 0;

                                    foreach (var membrosDaReacao in await messageReactionAddEventArgs.Message.GetReactionsAsync(messageReactionAddEventArgs.Emoji))
                                    {
                                        try
                                        {
                                            await messageReactionAddEventArgs.Message.DeleteReactionAsync(messageReactionAddEventArgs.Emoji, membrosDaReacao);
                                        }
                                        catch (Exception) 
                                        { 
                                            ++i;
                                        }
                                    }
                                    
                                    await (messageReactionAddEventArgs.Guild.GetChannel(Valores.ChatsUBGE.ubgeBot)).SendMessageAsync($"`{i}` reações não foram removidas porque os membros saíram da UBGE, por favor, alguém remova estas reações restantes do emoji: {messageReactionAddEventArgs.Emoji}, {Formatter.MaskedUrl("clique aqui", messageReactionAddEventArgs.Message.JumpLink, "clique aqui")} para ir a mensagem especificada.");
                                }
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                }
            }).Start();
        } 

        public async Task ReacaoRemovidaReactRole(MessageReactionRemoveEventArgs messageReactionRemoveEventArgs)
        {
            await Task.Delay(200);

            new Thread(async () => 
            {
                try
                {
                    if (!messageReactionRemoveEventArgs.Channel.IsPrivate && messageReactionRemoveEventArgs.Channel.Guild.Id == Valores.Guilds.UBGE)
                    {
                        var client = ubgeBot.mongoClient;
                        var db = client.GetDatabase(Valores.Mongo.local);
                        
                        var msgs = db.GetCollection<Reacts>(Valores.Mongo.reacts);
                        var dbContar = db.GetCollection<ContaMembrosQuePegaramCargos>(Valores.Mongo.contaMembrosQuePegaramCargos);
                        
                        var resultadosMensagens = await (await msgs.FindAsync(Builders<Reacts>.Filter.Empty)).ToListAsync();

                        DiscordMember member = await messageReactionRemoveEventArgs.Channel.Guild.GetMemberAsync(messageReactionRemoveEventArgs.User.Id);
                        DiscordRole acessoGeral = messageReactionRemoveEventArgs.Channel.Guild.GetRole(Valores.Cargos.cargoAcessoGeral);

                        ConcurrentDictionary<ulong, string> dict = new ConcurrentDictionary<ulong, string>();
                        ConcurrentDictionary<ulong, ulong> e2r = new ConcurrentDictionary<ulong, ulong>();

                        foreach (var x in resultadosMensagens)
                            dict.TryAdd(x.IdMensagens, x.Categoria);

                        if (dict.Keys.Contains(messageReactionRemoveEventArgs.Message.Id))
                        {
                            if (!messageReactionRemoveEventArgs.User.IsBot)
                            {
                                var jogos = db.GetCollection<Jogos>(Valores.Mongo.jogos);
                                var filtro = Builders<Jogos>.Filter.Eq(x => x.NomeDaCategoria, dict[messageReactionRemoveEventArgs.Message.Id]);
                                var resultados = await (await jogos.FindAsync(filtro)).ToListAsync();

                                foreach (var y in resultados)
                                    e2r.TryAdd(y.IdDoEmoji, y.CargoID);

                                if (e2r.ContainsKey(messageReactionRemoveEventArgs.Emoji.Id))
                                {
                                    DiscordRole role = messageReactionRemoveEventArgs.Channel.Guild.GetRole(e2r[messageReactionRemoveEventArgs.Emoji.Id]);

                                    if (role == acessoGeral)
                                    {
                                        await member.GrantRoleAsync(acessoGeral);

                                        return;
                                    }

                                    await member.RevokeRoleAsync(role);

                                    var filtroDBContar = Builders<ContaMembrosQuePegaramCargos>.Filter.Eq(x => x.Jogo, role.Name);
                                    var resultadosDBContar = await (await dbContar.FindAsync(filtroDBContar)).ToListAsync();

                                    if (resultadosDBContar.Count == 0)
                                        await dbContar.InsertOneAsync(new ContaMembrosQuePegaramCargos { Jogo = role.Name, NumeroPessoas = 0, IdsMembrosQuePegaramOCargo = new List<ulong>() });
                                    else
                                    {
                                        if (resultadosDBContar.FirstOrDefault().IdsMembrosQuePegaramOCargo.Contains(member.Id))
                                        {
                                            resultadosDBContar.FirstOrDefault().IdsMembrosQuePegaramOCargo.Remove(member.Id);

                                            var Update = Builders<ContaMembrosQuePegaramCargos>.Update
                                                .Set(x => x.NumeroPessoas, resultadosDBContar.FirstOrDefault().NumeroPessoas - 1)
                                                .Set(y => y.IdsMembrosQuePegaramOCargo, resultadosDBContar.FirstOrDefault().IdsMembrosQuePegaramOCargo);

                                            await dbContar.UpdateOneAsync(filtroDBContar, Update);
                                        }
                                    }

                                    if (messageReactionRemoveEventArgs.Channel.Guild.Id == Valores.Guilds.UBGE)
                                        await ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.ReactRoleUBGE, 
                                            "Cargo Removido!", $"{messageReactionRemoveEventArgs.Emoji} | O usuário: {member.Mention} removeu o cargo de: {role.Mention}.\n\n" +
                                            $"Ou:\n- `@{(string.IsNullOrWhiteSpace(member.Nickname) ? member.Username : member.Nickname)}#{member.Discriminator}`\n" +
                                            $"- `@{role.Name}`", (await messageReactionRemoveEventArgs.Channel.Guild.GetMemberAsync(Valores.Guilds.Membros.ubgeBot)).AvatarUrl, member);
                                    else
                                        await ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.ReactRoleForaDaUBGE, "Cargo Adicionado!", 
                                            $"{messageReactionRemoveEventArgs.Emoji} | O usuário: **{(string.IsNullOrWhiteSpace(member.Nickname) ? member.Username : member.Nickname)}#{member.Discriminator}** pegou o cargo de: **{role.Name}**.",
                                            messageReactionRemoveEventArgs.Channel.Guild.IconUrl, member);
                                }
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                }
            }).Start();
        } 
        
        public async Task MensagemCriada(MessageCreateEventArgs messageCreateEventArgs)
        {
            await Task.Delay(200);

            new Thread(async () =>
            {
                try
                {
                    if (messageCreateEventArgs.Guild.Id == Valores.Guilds.UBGE)
                    {
                        var mongo = ubgeBot.mongoClient;
                        var db = mongo.GetDatabase(Valores.Mongo.local);

                        if (messageCreateEventArgs.Channel.Id == Valores.ChatsUBGE.canalSugestoes)
                        {
                            if (messageCreateEventArgs.Message.Content.ToLower().Contains("sugestão:"))
                            {
                                await messageCreateEventArgs.Message.CreateReactionAsync(DiscordEmoji.FromName(messageCreateEventArgs.Client, ":thumbsup:"));
                                await messageCreateEventArgs.Message.CreateReactionAsync(DiscordEmoji.FromName(messageCreateEventArgs.Client, ":thumbsdown:"));
                            }
                        }
                        else if (messageCreateEventArgs.Channel.Id == Valores.ChatsUBGE.canalRecomendacoesPromocoes)
                        {
                            if (messageCreateEventArgs.Message.Content.ToLower().Contains("http") || messageCreateEventArgs.Message.Content.ToLower().Contains("https"))
                            {
                                await messageCreateEventArgs.Message.CreateReactionAsync(DiscordEmoji.FromName(messageCreateEventArgs.Client, ":thumbsup:"));
                                await messageCreateEventArgs.Message.CreateReactionAsync(DiscordEmoji.FromName(messageCreateEventArgs.Client, ":thumbsdown:"));
                            }
                        }
                        else if (messageCreateEventArgs.Channel.Id == Valores.ChatsUBGE.formularioAlerta)
                        {
                            if (messageCreateEventArgs.Author.IsBot)
                            {
                                await messageCreateEventArgs.Message.CreateReactionAsync(DiscordEmoji.FromName(messageCreateEventArgs.Client, ":white_check_mark:"));
                                await messageCreateEventArgs.Message.CreateReactionAsync(DiscordEmoji.FromName(messageCreateEventArgs.Client, ":negative_squared_cross_mark:"));
                            }
                        }
                        else if (messageCreateEventArgs.Channel.Id == Valores.ChatsUBGE.crieSuaSalaAqui)
                        {
                            if (messageCreateEventArgs.Message.Content.ToLower().StartsWith("//sala") || messageCreateEventArgs.Message.Content.ToLower().StartsWith($"ubge!sala") ||
                            messageCreateEventArgs.Message.Content.ToLower().StartsWith("//tutorial") || messageCreateEventArgs.Message.Content.ToLower().StartsWith($"ubge!tutorial") ||
                            messageCreateEventArgs.Message.Attachments.Count != 0) 
                            { }
                            else
                            {
                                if (!messageCreateEventArgs.Author.IsBot)
                                    await messageCreateEventArgs.Message.DeleteAsync();
                            }
                        }
                        
                        if (messageCreateEventArgs.Message.Content.ToLower().Contains("sinais vitais") && messageCreateEventArgs.Author.Id == Valores.Guilds.Membros.luiz)
                        {
                            var procuraComando = messageCreateEventArgs.Client.GetCommandsNext().FindCommand("bot status", out var args);
                            var comando = messageCreateEventArgs.Client.GetCommandsNext().CreateFakeContext(messageCreateEventArgs.Author, messageCreateEventArgs.Channel, "", "//", procuraComando, args);

                            await messageCreateEventArgs.Client.GetCommandsNext().ExecuteCommandAsync(comando);
                        }

                        if (messageCreateEventArgs.Channel.Id != Valores.ChatsUBGE.testeDoBot || messageCreateEventArgs.Channel.Id != Valores.ChatsUBGE.centroDeReabilitacao ||
                                messageCreateEventArgs.Channel.Id != Valores.ChatsUBGE.chatPRServidor || messageCreateEventArgs.Channel.Id != Valores.ChatsUBGE.comandosBot || 
                                !messageCreateEventArgs.Author.IsBot)
                        {
                            var colecao = db.GetCollection<Levels>(Valores.Mongo.levels);
                            var filtro = Builders<Levels>.Filter.Eq(x => x.IdMembro, messageCreateEventArgs.Author.Id);

                            var lista = await (await colecao.FindAsync(filtro)).ToListAsync();

                            var xpAleatorio = ulong.Parse(new Random().Next(1, 20).ToString());

                            ulong numeroLevel = 1;

                            if (lista.Count == 0)
                                await colecao.InsertOneAsync(new Levels { IdMembro = messageCreateEventArgs.Author.Id, XP = 1, NomeLevel = $"{numeroLevel}", DiaEHora = DateTime.Now.ToString() });
                            else
                            {
                                ulong xpFinal = 0;
                                ulong xpForeach = 0;

                                foreach (var Level in lista)
                                {
                                    if ((DateTime.Now - Convert.ToDateTime(Level.DiaEHora)).TotalMinutes < 1)
                                        return;

                                    xpFinal = Level.XP;
                                    xpForeach = Level.XP;
                                    numeroLevel = ulong.Parse(Level.NomeLevel);
                                }

                                xpFinal = xpAleatorio + xpFinal;

                                if (xpFinal >= 2800 * numeroLevel)
                                    numeroLevel++;

                                var updates = Builders<Levels>.Update.Set(x => x.XP, xpFinal)
                                    .Set(x => x.NomeLevel, $"{numeroLevel}")
                                    .Set(x => x.DiaEHora, DateTime.Now.ToString());

                                await colecao.UpdateOneAsync(filtro, updates);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                }
            }).Start();
        } 
        
        public async Task CanalDeVozPersonalizado(VoiceStateUpdateEventArgs voiceStateUpdateEventArgs)
        {
            if (voiceStateUpdateEventArgs.Guild.Id == Valores.Guilds.UBGE)
            {
                await Task.Delay(200);

                new Thread(async () =>
                {
                    try
                    {
                        var client = ubgeBot.mongoClient;
                        var local = client.GetDatabase(Valores.Mongo.local);

                        DiscordMember membro = await voiceStateUpdateEventArgs.Guild.GetMemberAsync(voiceStateUpdateEventArgs.User.Id);

                        DiscordRole membroRegistradoCargo = voiceStateUpdateEventArgs.Guild.GetRole(Valores.Cargos.cargoMembroRegistrado);
                        DiscordRole prisioneiroCargo = voiceStateUpdateEventArgs.Guild.GetRole(Valores.Cargos.cargoPrisioneiro);
                        DiscordChannel loggerCanal = voiceStateUpdateEventArgs.Guild.GetChannel(Valores.ChatsUBGE.canalLog);

                        await Task.Delay(200);

                        DiscordRole ajudantesComunitariosCargo = voiceStateUpdateEventArgs.Guild.GetRole(Valores.Cargos.cargoAjudante);
                        DiscordRole administradoresCargo = voiceStateUpdateEventArgs.Guild.GetRole(Valores.Cargos.cargoAdemir);
                        DiscordRole diretoresCargo = voiceStateUpdateEventArgs.Guild.GetRole(Valores.Cargos.cargoAdemir);

                        await Task.Delay(200);

                        DiscordRole botsMusicaisCargo = voiceStateUpdateEventArgs.Guild.GetRole(Valores.Cargos.botsMusicais);
                        DiscordRole acessoGeralCargo = voiceStateUpdateEventArgs.Guild.GetRole(Valores.Cargos.cargoAcessoGeral);

                        if (voiceStateUpdateEventArgs.After?.Channel != null && voiceStateUpdateEventArgs.After?.Channel.Id == Valores.ChatsUBGE.cliqueAqui)
                        {
                            if (membro.Roles.Contains(membroRegistradoCargo))
                            {
                                var salas = local.GetCollection<Salas>(Valores.Mongo.salas);
                                var filtroSalas = Builders<Salas>.Filter.Eq(s => s.DonoId, membro.Id);
                                var resultadoSalas = await (await salas.FindAsync(filtroSalas)).ToListAsync();

                                DiscordChannel vc = null;
                                DiscordMember m = null;
                                DiscordChannel categoria = voiceStateUpdateEventArgs.Channel.Parent;
                                string nomeAntigo = "🎮 Clique aqui!";

                                await voiceStateUpdateEventArgs.Channel.ModifyAsync(c => { c.Name = $"🎮 Sala criada!"; });

                                new Thread(async () =>
                                {
                                    await Task.Delay(TimeSpan.FromSeconds(4));

                                    await voiceStateUpdateEventArgs.Channel.ModifyAsync(c => { c.Name = nomeAntigo; });
                                }).Start();

                                if (resultadoSalas.Count == 0)
                                {
                                    if (!string.IsNullOrWhiteSpace(membro?.Presence?.Activity?.Name))
                                        vc = await voiceStateUpdateEventArgs.Guild.CreateChannelAsync(membro.Presence.Activity.Name, ChannelType.Voice, categoria);
                                    else
                                        vc = await voiceStateUpdateEventArgs.Guild.CreateChannelAsync($"Sala do: {(membro.Nickname != null ? membro.Nickname : membro.Username)}", ChannelType.Voice, categoria);

                                    await vc.AddOverwriteAsync(membro, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice);
                                    await vc.AddOverwriteAsync(botsMusicaisCargo, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice);

                                    await salas.InsertOneAsync(new Salas
                                    {
                                        DonoId = membro.Id,
                                        IdsPermitidos = new List<ulong> { membro.Id },
                                        LimiteUsuarios = 0,
                                        NomeSala = vc.Name,
                                        Trancada = false,
                                        IdSala = vc.Id,
                                    });

                                    await vc.PlaceMemberAsync(membro);

                                    return;
                                }
                                else
                                {
                                    if (!string.IsNullOrWhiteSpace(membro?.Presence?.Activity?.Name))
                                        vc = await voiceStateUpdateEventArgs.Guild.CreateChannelAsync(membro.Presence.Activity.Name, ChannelType.Voice, categoria, string.Empty, null, resultadoSalas[0].LimiteUsuarios);
                                    else
                                        vc = await voiceStateUpdateEventArgs.Guild.CreateChannelAsync(resultadoSalas[0].NomeSala, ChannelType.Voice, categoria, string.Empty, null, resultadoSalas[0].LimiteUsuarios);

                                    if (resultadoSalas[0].Trancada)
                                    {
                                        await vc.AddOverwriteAsync(voiceStateUpdateEventArgs.Guild.EveryoneRole, Permissions.AccessChannels, Permissions.UseVoice | Permissions.Speak);
                                        await vc.AddOverwriteAsync(ajudantesComunitariosCargo, Permissions.AccessChannels, Permissions.UseVoice | Permissions.Speak);
                                        await vc.AddOverwriteAsync(administradoresCargo, Permissions.AccessChannels, Permissions.UseVoice | Permissions.Speak);
                                        await vc.AddOverwriteAsync(diretoresCargo, Permissions.AccessChannels, Permissions.UseVoice | Permissions.Speak);
                                        await vc.AddOverwriteAsync(membro, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice);
                                        await vc.AddOverwriteAsync(botsMusicaisCargo, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice);
                                        await vc.AddOverwriteAsync(acessoGeralCargo, Permissions.AccessChannels, Permissions.UseVoice | Permissions.Speak);

                                        await salas.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(u => u.Trancada, true));

                                        foreach (ulong u in resultadoSalas[0].IdsPermitidos)
                                        {
                                            m = await voiceStateUpdateEventArgs.Guild.GetMemberAsync(u);

                                            await vc.AddOverwriteAsync(m, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice);
                                        }
                                    }
                                    else
                                    {
                                        await vc.AddOverwriteAsync(voiceStateUpdateEventArgs.Guild.EveryoneRole, Permissions.AccessChannels | Permissions.UseVoice | Permissions.Speak);
                                        await vc.AddOverwriteAsync(botsMusicaisCargo, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice);

                                        await salas.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(u => u.Trancada, false));
                                    }

                                    if (resultadoSalas[0].LimiteUsuarios != 0)
                                        await vc.ModifyAsync(x => { x.Userlimit = resultadoSalas[0].LimiteUsuarios; });

                                    await salas.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(x => x.IdSala, vc.Id));

                                    await vc.PlaceMemberAsync(membro);

                                    return;
                                }
                            }
                            else
                            {
                                try
                                {
                                    DiscordChannel Comandos_Bot = voiceStateUpdateEventArgs.Guild.GetChannel(Valores.ChatsUBGE.comandosBot);
                                    DiscordChannel BatePapo = voiceStateUpdateEventArgs.Guild.GetChannel(Valores.ChatsUBGE.batePapoVozUBGE);

                                    await membro.PlaceInAsync(BatePapo);

                                    await Comandos_Bot.SendMessageAsync($"{membro.Mention} Você precisa ter o cargo de membro registrado para criar salas de voz!\n\nPara isso, " +
                                    $"digite o comando `//fazercenso` para fazer o censo comunitário e ter acesso à salas privadas!");
                                    await (await membro.CreateDmChannelAsync()).SendMessageAsync($"{membro.Mention}, na UBGE você precisa ter o cargo de membro registrado para criar salas de voz!\n\nPara isso, " +
                                    $"digite o comando `//fazercenso` no {Comandos_Bot.Mention} para fazer o censo comunitário e ter acesso à salas privadas!");

                                    return;
                                }
                                catch (Exception) { }
                            }
                        }
                        else if (voiceStateUpdateEventArgs.Before?.Channel != null && voiceStateUpdateEventArgs.Before?.Channel.Users.Count() == 0)
                        {
                            var salas = local.GetCollection<Salas>(Valores.Mongo.salas);
                            var resultadoSalas = await (await salas.FindAsync(Builders<Salas>.Filter.Eq(s => s.DonoId, membro.Id))).ToListAsync();

                            DiscordChannel vc;

                            if (resultadoSalas.Count() != 0)
                            {
                                vc = voiceStateUpdateEventArgs.Guild.GetChannel(resultadoSalas[0].IdSala);

                                if ((voiceStateUpdateEventArgs.Before?.Channel?.Id == vc.Id || 
                                voiceStateUpdateEventArgs.Before?.Channel?.Id == vc.Id && 
                                voiceStateUpdateEventArgs.After?.Channel == null) &&
                                voiceStateUpdateEventArgs.Before?.Channel?.Id != Valores.ChatsUBGE.cliqueAqui &&
                                voiceStateUpdateEventArgs.Before.Channel.Parent.Name.ToUpper().Contains("UBGE") &&
                                voiceStateUpdateEventArgs.Before?.Channel?.Parent?.Position <= 7)
                                    await vc.DeleteAsync();

                                return;
                            }
                            else if (resultadoSalas.Count() == 0)
                            {
                                vc = voiceStateUpdateEventArgs.Guild.GetChannel(voiceStateUpdateEventArgs.Before.Channel.Id);

                                if (vc.Id == Valores.ChatsUBGE.cliqueAqui || 
                                !vc.Parent.Name.ToUpper().Contains("UBGE") || 
                                vc.Parent.Position >= 7)
                                    return;

                                await vc.DeleteAsync();
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                    }
                }).Start();
            }
        }
        
        public async Task NovoBan(GuildBanAddEventArgs guildBanAddEventArgs)
        {
            if (guildBanAddEventArgs.Guild.Id == Valores.Guilds.UBGE)
            {
                await Task.Delay(200);

                new Thread(async () =>
                {
                    try
                    {
                        DiscordChannel logChat = guildBanAddEventArgs.Guild.GetChannel(Valores.ChatsUBGE.canalLog);

                        DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                        {
                            Color = ubgeBot.utilidadesGerais.CorAleatoriaEmbed(),
                            Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"O membro: \"{(string.IsNullOrWhiteSpace(guildBanAddEventArgs.Member.Nickname) ? guildBanAddEventArgs.Member.Username : guildBanAddEventArgs.Member.Nickname)}#{guildBanAddEventArgs.Member.Discriminator}\" foi banido.", IconUrl = Valores.logoUBGE },
                            Description = $"Dia e Hora: {DateTime.Now.ToString()}\n\n" +
                                    $"ID do Membro: {guildBanAddEventArgs.Member.Id}",
                            Timestamp = DateTime.Now,
                            ThumbnailUrl = guildBanAddEventArgs.Member.AvatarUrl,
                        };

                        await logChat.SendMessageAsync(embed: embed.Build());
                    }
                    catch (Exception exception)
                    {
                        await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Sistemas, exception);
                    }
                }).Start();
            }
        }

        public async Task BanRetirado(GuildBanRemoveEventArgs guildBanRemoveEventArgs)
        {
            if (guildBanRemoveEventArgs.Guild.Id == Valores.Guilds.UBGE)
            {
                await Task.Delay(200);

                new Thread(async () =>
                {
                    try
                    {
                        DiscordChannel logChat = guildBanRemoveEventArgs.Guild.GetChannel(Valores.ChatsUBGE.canalLog);

                        DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                        {
                            Color = new UtilidadesGerais().CorAleatoriaEmbed(),
                            Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"O membro: \"{(string.IsNullOrWhiteSpace(guildBanRemoveEventArgs.Member.Nickname) ? guildBanRemoveEventArgs.Member.Username : guildBanRemoveEventArgs.Member.Nickname)}#{guildBanRemoveEventArgs.Member.Discriminator}\" foi desbanido.", IconUrl = Valores.logoUBGE },
                            Description = $"Dia e Hora: {DateTime.Now.ToString()}\n\n" +
                                $"ID do Membro: {guildBanRemoveEventArgs.Member.Id}",
                            Timestamp = DateTime.Now,
                            ThumbnailUrl = guildBanRemoveEventArgs.Member.AvatarUrl,
                        };

                        await logChat.SendMessageAsync(embed: embed.Build());
                    }
                    catch (Exception exception)
                    {
                        await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Sistemas, exception);
                    }
                }).Start();
            }
        }

        public async Task MembroEntra(GuildMemberAddEventArgs guildMemberAddEventArgs)
        {
            if (guildMemberAddEventArgs.Guild.Id == Valores.Guilds.UBGE)
            {
                await Task.Delay(200);

                new Thread(async () =>
                {
                    try
                    {
                        var client = ubgeBot.mongoClient;
                        var local = client.GetDatabase(Valores.Mongo.local);
                        var prisao = local.GetCollection<Infracao>(Valores.Mongo.infracoes);
                        var listaPrisao = await (await prisao.FindAsync(Builders<Infracao>.Filter.Eq(x => x.idInfrator, guildMemberAddEventArgs.Member.Id))).ToListAsync();

                        if (guildMemberAddEventArgs.Member.IsBot)
                            return;

                        DiscordChannel ubgeBotCanal = guildMemberAddEventArgs.Guild.GetChannel(Valores.ChatsUBGE.ubgeBot);

                        if (listaPrisao.Count != 0 && (DateTime.Now.Add(new UtilidadesGerais().ConverterTempo(listaPrisao.LastOrDefault().dadosPrisao.tempo)) < Convert.ToDateTime(listaPrisao.LastOrDefault().dataInfracao.ToString())))
                        {
                            foreach (DiscordRole cargo in guildMemberAddEventArgs.Guild.Roles.Values)
                                await guildMemberAddEventArgs.Member.RevokeRoleAsync(cargo);

                            DiscordRole prisioneiroCargo = guildMemberAddEventArgs.Guild.GetRole(Valores.Cargos.cargoPrisioneiro);

                            await guildMemberAddEventArgs.Member.GrantRoleAsync(prisioneiroCargo);

                            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                            {
                                Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"O membro: {(string.IsNullOrWhiteSpace(guildMemberAddEventArgs.Member.Nickname) ? guildMemberAddEventArgs.Member.Username : guildMemberAddEventArgs.Member.Nickname)} tentou escapar da punição.", IconUrl = Valores.logoUBGE },
                                Description = ":smile:",
                                Color = new UtilidadesGerais().CorAleatoriaEmbed(),
                                ThumbnailUrl = guildMemberAddEventArgs.Member.AvatarUrl,
                                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Comando requisitado pelo: {guildMemberAddEventArgs.Member.Username}" },
                            };

                            await ubgeBotCanal.SendMessageAsync(embed: embed.Build());
                        }

                        DiscordRole acessoGeralCargo = guildMemberAddEventArgs.Guild.GetRole(Valores.Cargos.cargoAcessoGeral);
                        DiscordDmChannel privadoMembro = await guildMemberAddEventArgs.Member.CreateDmChannelAsync();
                        DiscordChannel comandosBot = guildMemberAddEventArgs.Guild.GetChannel(Valores.ChatsUBGE.comandosBot);

                        await guildMemberAddEventArgs.Member.GrantRoleAsync(acessoGeralCargo);
                        await privadoMembro.SendMessageAsync($"*{guildMemberAddEventArgs.Member.Mention}, Bem-Vindo a UBGE!*\n\n" +
                        $"Leia a mensagem que o Mee6 lhe enviou no seu privado, ele lhe ajudará a dar os seus primeiros passos na UBGE.\n\n" +
                        $"Para registrar-se como membro registrado na UBGE, digite: `//fazercenso` no {comandosBot.Mention}.\n\n" +
                        $"Para qualquer dúvida sobre mim, digite `//ajuda`.\n\n" +
                        $"Obrigado por ler isso, e antes de tudo, sinta-se em casa! :smile:");
                    }
                    catch (Exception exception)
                    {
                        await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Sistemas, exception);
                    }
                }).Start();
            }
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
                    ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, $"A conexão com o Discord foi encerrada!\nReconectando em 10 segundos...");

                    await Task.Delay(TimeSpan.FromSeconds(10));

                    await ubgeBotClient.discordClient.ReconnectAsync(true);

                    ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, $"Reconectado!");
                }
            }
            catch (Exception exception)
            {
                ubgeBot.logExceptionsToDiscord.ExceptionToTxt(exception);
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
                    var client = ubgeBotClient.mongoClient;
                    var db = client.GetDatabase(Valores.Mongo.local);

                    var reacts = db.GetCollection<Reacts>(Valores.Mongo.reacts);
                    var roles = db.GetCollection<Jogos>(Valores.Mongo.jogos);

                    var resultadoReacts = await (await reacts.FindAsync(Builders<Reacts>.Filter.Empty)).ToListAsync();

                    DiscordGuild UBGE = await ubgeBotClient.discordClient.GetGuildAsync(Valores.Guilds.UBGE);
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

                    ConcurrentDictionary<string, string> categorias = new ConcurrentDictionary<string, string>();
                    foreach (var React in resultadoReacts)
                        categorias.TryAdd($"{React.Guild} {N++} {React.CanalId}", $"{React.Categoria}@ {React.IdMensagens}");

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

                        var Cargos = await (await roles.FindAsync(Builders<Jogos>.Filter.Eq(x => x.NomeDaCategoria, categoria.Value.Split('@')[0]))).ToListAsync();

                        ConcurrentDictionary<ulong, ulong> EmojiRole = new ConcurrentDictionary<ulong, ulong>();
                        foreach (var r in Cargos)
                            if (!string.IsNullOrEmpty(r.NomeDaCategoria))
                                EmojiRole.TryAdd(r.IdDoEmoji, r.CargoID);

                        ConcurrentDictionary<DiscordEmoji, IReadOnlyList<DiscordUser>> Usuarios = new ConcurrentDictionary<DiscordEmoji, IReadOnlyList<DiscordUser>>();
                        foreach (DiscordReaction discordReaction in mensagem.Reactions)
                        {
                            Usuarios.TryAdd(discordReaction.Emoji, await mensagem.GetReactionsAsync(discordReaction.Emoji));
                            
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
                                                await ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.SAC, 
                                                    "[S.A.C] - Sistema de Adicionar Cargos", 
                                                    $"Foi adicionado o cargo: {cargo.Mention} no: {membro.Mention}.", servidor.IconUrl, membro);
                                            else
                                                await ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.SAC, 
                                                    "[S.A.C] - Sistema de Adicionar Cargos", 
                                                    $"Foi adicionado o cargo: **{cargo.Name}** no: **{(string.IsNullOrWhiteSpace(membro.Nickname) ? membro.Username : membro.Nickname)}#{membro.Discriminator}**.", servidor.IconUrl, membro);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    ubgeBot.logExceptionsToDiscord.Log(LogExceptionsToDiscord.TipoLog.Sistemas, "A sincronização de cargos foi finalizada!");
                    await ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.Aviso, "A sincronização de cargos foi finalizada!", ":wink:", ubgeBotClient.discordClient.CurrentUser.AvatarUrl);
                }
                catch (Exception exception)
                {
                    await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Sistemas, exception);
                }
            }).Start();
        } 

        private async Task ConfiguracaoDiscord(UBGEBot_ ubgeBotClient)
        {
            await Task.Delay(150);

            new Thread(async () => 
            {
                try
                {
                    var clientMongo = ubgeBotClient.mongoClient;
                    var local = clientMongo.GetDatabase(Valores.Mongo.local);
                    var reacts = local.GetCollection<Reacts>(Valores.Mongo.reacts);
                    var filtro = Builders<Reacts>.Filter.Empty;
                    var listaReacts = await (await reacts.FindAsync(filtro)).ToListAsync();

                    if (listaReacts.Count == 0)
                    {
                        DiscordGuild UBGE = await ubgeBotClient.discordClient.GetGuildAsync(Valores.Guilds.UBGE);
                        DiscordChannel canalMenuReacts = UBGE.GetChannel(Valores.ChatsUBGE.canalReacts);
                        DiscordMessage mensagemMenu = await canalMenuReacts.SendMessageAsync("Carregando Menu...");

                        DiscordEmbedBuilder menu = new DiscordEmbedBuilder 
                        {
                            Author = new DiscordEmbedBuilder.EmbedAuthor { Name = "Menu de reações de vários canais:", IconUrl = Valores.logoUBGE },
                            Description = $"{Formatter.MaskedUrl("[Voltar ao Menu]", mensagemMenu.JumpLink, "[Voltar ao Menu]")}",
                            Color = ubgeBotClient.utilidadesGerais.CorAleatoriaEmbed()
                        };

                        await mensagemMenu.ModifyAsync(content: string.Empty, embed: menu.Build());

                        await reacts.InsertOneAsync(new Reacts { Categoria = "Menu das Reações", IdMensagens = mensagemMenu.Id });
                    }
                }
                catch (Exception exception)
                {
                    await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Sistemas, exception);
                }
            }).Start();
        }

        private static async Task ExecutaListAutomaticamente(UBGEBot_ ubgeBotClient)
        {
            await Task.Delay(200);

            new Thread(async () =>
            {
                try
                {
                    DiscordGuild UBGE = await ubgeBotClient.discordClient.GetGuildAsync(Valores.Guilds.UBGE);
                    DiscordChannel ubgeBot = UBGE.GetChannel(Valores.ChatsUBGE.ubgeBot);
                    DiscordMember ubgeBotMember = await UBGE.GetMemberAsync(Valores.Guilds.Membros.ubgeBot);

                    DiscordMessage mensagemAviso = await ubgeBot.SendMessageAsync($":warning: | Executando o **//list** automaticamente... Dia e Hora: `{DateTime.Now.ToString()}`.");

                    var procuraComando = ubgeBotClient.discordClient.GetCommandsNext().FindCommand("list", out var Args);
                    var comandoList = ubgeBotClient.discordClient.GetCommandsNext().CreateFakeContext(ubgeBotMember, ubgeBot, "", "//", procuraComando, Args);

                    await ubgeBotClient.discordClient.GetCommandsNext().ExecuteCommandAsync(comandoList);

                    await mensagemAviso.DeleteAsync();
                }
                catch (Exception exception)
                {
                    await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Sistemas, exception);
                }
            }).Start();
        }

        private static async Task EnviaDadosDoReactRole(UBGEBot_ ubgeBotClient)
        {
            await Task.Delay(200);

            new Thread(async () =>
            {
                try
                {
                    while (true)
                    {
                        if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 55)
                        {
                            var mongo = ubgeBotClient.mongoClient;
                            var db = mongo.GetDatabase(Valores.Mongo.local);
                            var dbMembros = db.GetCollection<ContaMembrosQuePegaramCargos>(Valores.Mongo.contaMembrosQuePegaramCargos);
                            var dbMembrosRegistrados = db.GetCollection<MembrosQuePegaramOCargoDeMembroRegistrado>(Valores.Mongo.membrosQuePegaramOCargoDeMembroRegistrado);

                            var filtroDBMembros = Builders<ContaMembrosQuePegaramCargos>.Filter.Empty;
                            var filtroDBMembrosRegistrados = Builders<MembrosQuePegaramOCargoDeMembroRegistrado>.Filter.Empty;

                            var resultadosDBMembros = await (await dbMembros.FindAsync(filtroDBMembros)).ToListAsync();
                            var resultadosDBMembrosRegistrados = await (await dbMembrosRegistrados.FindAsync(filtroDBMembrosRegistrados)).ToListAsync();

                            DiscordGuild UBGE = await ubgeBotClient.discordClient.GetGuildAsync(Valores.Guilds.UBGE);
                            DiscordChannel BotUBGE = UBGE.GetChannel(Valores.ChatsUBGE.ubgeBot);

                            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                            if (resultadosDBMembros.Count == 0 && resultadosDBMembrosRegistrados.Count == 0)
                            {
                                embed.WithAuthor("Não tenho dados para apresentar :/", null, Valores.logoUBGE)
                                    .WithColor(new UtilidadesGerais().CorAleatoriaEmbed())
                                    .WithDescription(":pensive:")
                                    .WithFooter($"Nada há declarar. Às: {DateTime.Now.ToString()}")
                                    .WithThumbnailUrl((await new UtilidadesGerais().ProcuraEmoji(ubgeBot.discordClient, "gatu")).Url);

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
                                            streamWriter.WriteLine($"Jogo: \"{Dados.Jogo}\"");
                                            streamWriter.WriteLine($"Número de pessoas que pegaram o cargo: \"{(Dados.NumeroPessoas == 1 ? $"{Dados.NumeroPessoas} membro." : $"{Dados.NumeroPessoas} membros.")}\".");
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
                                            streamWriter.WriteLine($"Número de pessoas que pegaram o cargo de membro registrado: \"{Dados_.NumeroPessoasQuePegaramOCargoDeMembroRegistrado}\".");
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

                                embed.WithAuthor("Dados do React Role e dos Membros Registrados:", null, Valores.logoUBGE)
                                    .WithColor(new UtilidadesGerais().CorAleatoriaEmbed())
                                    .WithDescription($":smile: {await new UtilidadesGerais().ProcuraEmoji(ubgeBotClient.discordClient, "ubge")}")
                                    .WithThumbnailUrl((await new UtilidadesGerais().ProcuraEmoji(ubgeBotClient.discordClient, "uhu")).Url)
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
                    var mongo = ubgeBotClient.mongoClient;
                    var db = mongo.GetDatabase(Valores.Mongo.local);
                    var collectionCheckBotAberto = db.GetCollection<CheckBotAberto>(Valores.Mongo.checkBotAberto);

                    var filtro = Builders<CheckBotAberto>.Filter.Empty;

                    var lista = await (await collectionCheckBotAberto.FindAsync<CheckBotAberto>(filtro)).ToListAsync();

                    if (lista.Count == 0)
                        await collectionCheckBotAberto.InsertOneAsync(new CheckBotAberto { Numero = 0, DiaHora = DateTime.Now });
                    else
                        await collectionCheckBotAberto.UpdateOneAsync(filtro, Builders<CheckBotAberto>.Update.Set(x => x.Numero, 0).Set(y => y.DiaHora, DateTime.Now));
                }
                catch (Exception exception)
                {
                    await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Sistemas, exception);
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
                    DiscordGuild UBGE = await ubgeBotClient.discordClient.GetGuildAsync(Valores.Guilds.UBGE);
                    await Task.Delay(200);
                    DiscordChannel cliqueAquiVoz = UBGE.GetChannel(Valores.ChatsUBGE.cliqueAqui);
                    await Task.Delay(200);
                    DiscordChannel batePapo = UBGE.GetChannel(Valores.ChatsUBGE.batePapoVozUBGE);

                    var CanaisVozJogosGerais = UBGE.Channels.Values.ToList().FindAll(x => x.Parent != null && x.Parent.Name.ToUpper().Contains("UBGE") && x.Type == ChannelType.Voice && x.Parent.Position <= 7);
                    CanaisVozJogosGerais.Remove(cliqueAquiVoz);
                    CanaisVozJogosGerais.Remove(batePapo);
                    CanaisVozJogosGerais.Remove(CanaisVozJogosGerais.Find(x => x.Id == Valores.ChatsUBGE.radio));

                    if (CanaisVozJogosGerais.Count == 0)
                        return;
                    else
                    {
                        foreach (var Canal in CanaisVozJogosGerais)
                        {
                            if (Canal.Users.Count() == 0)
                            { 
                                await Canal.DeleteAsync();
                                await Task.Delay(200);
                            }
                        }
                    }

                    if (cliqueAquiVoz.Users.Count() != 0)
                    {
                        foreach (var Membro in cliqueAquiVoz.Users)
                        {
                            await Membro.PlaceInAsync(batePapo);
                            await Task.Delay(200);
                        }
                    }
                }
                catch (Exception exception)
                {
                    await ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Sistemas, exception);
                }
            }).Start();
        }

        public static void ShutdownBot()
            => Environment.Exit(1);
    }
}