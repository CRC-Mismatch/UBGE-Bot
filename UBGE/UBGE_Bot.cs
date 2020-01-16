using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Net.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Bson;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UBGE.Config;
using Log = UBGE.Logger.Logger;
using UBGE.MongoDB.Models;
using UBGE.Services.Google;
using UBGE.Utilities;
using Utility = UBGE.Utilities.Utilities;

namespace UBGE
{
    public sealed class UBGE_Bot
    {
        public BotConfig BotConfig { get; private set; }
        public Log Logger { get; private set; }

        public DiscordClient DiscordClient { get; private set; }
        public CommandsNextExtension CommandsNext { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }

        public IMongoDatabase LocalDB { get; private set; }
        public MySqlConnection MySqlConnection { get; private set; }

        private IServiceProvider ServicesProvider { get; set; }

        public Utility Utilities { get; private set; }
        public HttpClient HttpClient { get; private set; }
        
        public bool ConnectedToMongo { get; private set; }
        public bool ChannelsCheckWasStarted { get; set; }

        public readonly string BOT_VERSION = $"v{Assembly.GetEntryAssembly().GetName().Version.ToString()}-beta6";
        readonly string PREFIX_MESSAGES = "[Config]";

        public UBGE_Bot()
        {
            try
            {
                this.BotConfig = new BotConfig();

                this.HttpClient = new HttpClient();

                this.Logger = new Log();
                this.Utilities = new Utility();

                try
                {
                    var mongoClient = new MongoClient(new MongoClientSettings
                    {
                        Server = new MongoServerAddress(this.BotConfig.DatabasesConfig.MongoDBIP, int.Parse(this.BotConfig.DatabasesConfig.MongoDBPort)),
                        ConnectTimeout = TimeSpan.FromSeconds(5),
                    });

                    this.LocalDB = mongoClient.GetDatabase(Values.Mongo.local);

                    this.LocalDB.RunCommand<BsonDocument>(new BsonDocument("ping", 1));

                    this.ConnectedToMongo = true;
                }
                catch (Exception)
                {
                    this.ConnectedToMongo = false;

                    this.Logger.Error(Log.TypeError.Mongo, "Não foi possível conectar ao MongoDB! Alguns comandos e sistemas podem estar indisponíveis.", this.PREFIX_MESSAGES);
                }

                try
                {
                    this.MySqlConnection = new MySqlConnection($"Server={this.BotConfig.DatabasesConfig.MySQLIP};Database={this.BotConfig.DatabasesConfig.MySQLDatabase};Uid={this.BotConfig.DatabasesConfig.MySQLUser};Pwd={this.BotConfig.DatabasesConfig.MySQLPassword}");
                    this.MySqlConnection.Open();
                }
                catch (Exception)
                {
                    this.Logger.Error(Log.TypeError.MySQL, "Não foi possível conectar ao MySQL para pegar dados do rank dos jogadores de Counter-Strike 1.6.", this.PREFIX_MESSAGES);
                }

                var discordConfiguration = new DiscordConfiguration
                {
                    AutoReconnect = true,
                    GatewayCompressionLevel = GatewayCompressionLevel.Stream,
                    HttpTimeout = TimeSpan.FromSeconds(30),
                    LargeThreshold = 5000,
                    LogLevel = LogLevel.Info,
                    ReconnectIndefinitely = false,
                    Token = this.BotConfig.DiscordConfig.Token,
                    UseInternalLogHandler = true,
                    WebSocketClientFactory = WebSocket4NetCoreClient.CreateNew,
                };

                this.DiscordClient = new DiscordClient(discordConfiguration);

                this.ServicesProvider = new ServiceCollection()
                    .AddSingleton(this)
                    .AddSingleton(this.DiscordClient)
                    .AddSingleton(new GoogleDriveService())
                    .AddSingleton(new GoogleSheetsService())
                    .BuildServiceProvider(true);

                var commandsNextConfiguration = new CommandsNextConfiguration
                {
                    EnableDms = true,
                    EnableMentionPrefix = true,
                    EnableDefaultHelp = false,
                    StringPrefixes = this.BotConfig.CommandsNextConfig.Prefix,
                    Services = this.ServicesProvider,
                };

                this.CommandsNext = this.DiscordClient.UseCommandsNext(commandsNextConfiguration);

                var interactivityConfiguration = new InteractivityConfiguration
                {
                    PaginationBehaviour = PaginationBehaviour.WrapAround,
                    Timeout = TimeSpan.FromMinutes(5),
                };

                this.Interactivity = this.DiscordClient.UseInteractivity(interactivityConfiguration);

                this.CommandsNext.RegisterCommands(typeof(UBGE_Bot).Assembly);

                this.DiscordClient.GuildBanAdded += this.NewBan;
                this.DiscordClient.GuildBanRemoved += this.RevogedBan;
                this.DiscordClient.GuildDownloadCompleted += this.GuildsDownloadCompleted;
                this.DiscordClient.Ready += this.Ready;
                this.DiscordClient.GuildMemberUpdated += this.MemberChanged;
                this.DiscordClient.GuildMemberAdded += this.MemberAddedOnServer;
                this.DiscordClient.VoiceStateUpdated += this.CreateYourRoomHere;
                this.DiscordClient.VoiceStateUpdated += this.HideVoiceChannels;
                this.DiscordClient.MessageCreated += this.MessageCreated;
                this.DiscordClient.MessageReactionAdded += this.ReactionAdded;
                this.DiscordClient.MessageReactionRemoved += this.ReactionRemoved;
                this.DiscordClient.DebugLogger.LogMessageReceived += this.MessageLoggerDSharpPlus;

                Console.Title = $"UBGE-Bot online! {this.BOT_VERSION}";
            }
            catch (Exception exception)
            {
                this.Logger.ExceptionTxt(exception);

                Program.ShutdownBot();
            }
        }

        async Task NewBan(GuildBanAddEventArgs e)
        {
            if (e.Guild.Id != Values.Guilds.guildUBGE)
                return;

            var embed = new DiscordEmbedBuilder
            {
                Color = this.Utilities.RandomColorEmbed(),
                Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"O membro: \"{this.Utilities.DiscordNick(e.Member)}#{e.Member.Discriminator}\" foi banido.", IconUrl = Values.logoUBGE },
                Description = $"Dia e Hora: {DateTime.Now.ToString()}\n\nID do Membro: {e.Member.Id}",
                Timestamp = DateTime.Now,
                ThumbnailUrl = e.Member.AvatarUrl,
            };

            await (await e.Client.GetChannelAsync(Values.Chats.channelLog)).SendMessageAsync(embed: embed.Build());
        }

        async Task RevogedBan(GuildBanRemoveEventArgs e)
        {
            if (e.Guild.Id != Values.Guilds.guildUBGE)
                return;

            var embed = new DiscordEmbedBuilder
            {
                Color = this.Utilities.RandomColorEmbed(),
                Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"O membro: \"{this.Utilities.DiscordNick(e.Member)}#{e.Member.Discriminator}\" foi desbanido.", IconUrl = Values.logoUBGE },
                Description = $"Dia e Hora: {DateTime.Now.ToString()}\n\nID do Membro: {e.Member.Id}",
                Timestamp = DateTime.Now,
                ThumbnailUrl = e.Member.AvatarUrl,
            };

            await (await e.Client.GetChannelAsync(Values.Chats.channelLog)).SendMessageAsync(embed: embed.Build());
        }

        async Task CreateYourRoomHere(VoiceStateUpdateEventArgs e)
        {
            if (e.Guild.Id != Values.Guilds.guildUBGE)
                return;

            try
            {
                var salas = this.LocalDB.GetCollection<Salas>(Values.Mongo.salas);

                var ubgeServidor = e.Guild;
                var cliqueAqui = ubgeServidor.GetChannel(Values.Chats.channelCliqueAqui);

                if (cliqueAqui == null)
                    return;

                DiscordRole membroRegistradoCargo = ubgeServidor.GetRole(Values.Roles.roleMembroRegistrado),
                prisioneiroCargo = ubgeServidor.GetRole(Values.Roles.rolePrisioneiro),
                botsMusicaisCargo = ubgeServidor.GetRole(Values.Roles.roleBots),
                acessoGeralCargo = ubgeServidor.GetRole(Values.Roles.roleAcessoGeral),
                moderadorDiscordCargo = ubgeServidor.GetRole(Values.Roles.roleModeradorDiscord);

                DiscordMember membro = null, membrosForeach = null;

                DiscordChannel canalDoMembro = null, comandosBot = null, batePapo = null;

                if (e.User == null)
                {
                    if (e.Before?.User == null)
                        membro = await ubgeServidor.GetMemberAsync(e.After.User.Id);
                    else if (e.After?.User == null)
                        membro = await ubgeServidor.GetMemberAsync(e.Before.User.Id);
                }
                else
                    membro = await ubgeServidor.GetMemberAsync(e.User.Id);

                var pvMembro = await membro.CreateDmChannelAsync();

                var filtroSalas = Builders<Salas>.Filter.Eq(s => s.idDoDono, membro.Id);
                var resultadoSalas = await (await salas.FindAsync(filtroSalas)).ToListAsync();

                if (e.Before?.Channel != null && resultadoSalas.Count != 0 && e.Before?.Channel?.Id == resultadoSalas.LastOrDefault().idDaSala &&
                e.After?.Channel != null && e.After?.Channel?.Id == cliqueAqui.Id)
                {
                    await membro.PlaceInAsync(ubgeServidor.GetChannel(resultadoSalas.LastOrDefault().idDaSala));

                    return;
                }

                if (e.Before?.Channel != null)
                {
                    if (e.Before.Channel == e.After?.Channel)
                        return;

                    var filtroSala = Builders<Salas>.Filter.Eq(s => s.idDaSala, e.Before.Channel.Id);
                    var respostaSala = await (await salas.FindAsync(filtroSala)).ToListAsync();

                    if (respostaSala.Count != 0 && e.Before.Channel.Id == respostaSala.LastOrDefault().idDaSala)
                    {
                        await salas.UpdateOneAsync(filtroSala, Builders<Salas>.Update.Set(x => x.membrosNaSala, respostaSala.LastOrDefault().membrosNaSala - 1));

                        var respostaSalaAtualizada = await (await salas.FindAsync(filtroSala)).ToListAsync();

                        if (respostaSala.Count != 0 && respostaSalaAtualizada.LastOrDefault().membrosNaSala == 0)
                            await ubgeServidor.GetChannel(respostaSalaAtualizada.LastOrDefault().idDaSala).DeleteAsync();

                        return;
                    }
                    else if (respostaSala.Count == 0 && e.Before.Channel.Parent == cliqueAqui.Parent && e.Before.Channel.Id != cliqueAqui.Id)
                    {
                        await ubgeServidor.GetChannel(e.Before.Channel.Id).DeleteAsync();

                        return;
                    }
                }

                if (e.Before?.Channel != null && e.Before?.Channel?.Id != cliqueAqui.Id && e.Before?.Channel?.Parent == cliqueAqui.Parent)
                {
                    var resultadoSalas_ = await (await salas.FindAsync(filtroSalas)).ToListAsync();

                    if (resultadoSalas_.Count != 0)
                    {
                        if (resultadoSalas_.LastOrDefault().membrosNaSala == 0 || ubgeServidor.GetChannel(resultadoSalas_.LastOrDefault().idDaSala).Users.Count() == 0)
                        {
                            await ubgeServidor.GetChannel(resultadoSalas_.LastOrDefault().idDaSala).DeleteAsync();

                            if (ubgeServidor.GetChannel(resultadoSalas_.LastOrDefault().idDaSala).Users.Count() == 0)
                                await salas.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(x => x.membrosNaSala, ulong.Parse("0")));
                        }
                    }
                    else if (resultadoSalas_.Count == 0)
                    {
                        var filtroSalaCanalId = Builders<Salas>.Filter.Eq(s => s.idDaSala, e.Before.Channel.Id);
                        var resultadoSalaCanalId = await (await salas.FindAsync(filtroSalaCanalId)).ToListAsync();

                        if (resultadoSalaCanalId.Count != 0)
                        {
                            if (resultadoSalaCanalId.LastOrDefault().membrosNaSala == 0 || ubgeServidor.GetChannel(resultadoSalaCanalId.LastOrDefault().idDaSala).Users.Count() == 0)
                            {
                                await ubgeServidor.GetChannel(resultadoSalaCanalId.LastOrDefault().idDaSala).DeleteAsync();

                                if (ubgeServidor.GetChannel(resultadoSalaCanalId.LastOrDefault().idDaSala).Users.Count() == 0)
                                    await salas.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(x => x.membrosNaSala, ulong.Parse("0")));
                            }
                        }
                        else if (resultadoSalaCanalId.Count == 0)
                        {
                            if (e.Before.Channel.Id != cliqueAqui.Id)
                                await ubgeServidor.GetChannel(e.Before.Channel.Id).DeleteAsync();
                        }
                    }
                }

                var embed = new DiscordEmbedBuilder();

                if (e.Before?.Channel == null && e.After?.Channel?.Id == cliqueAqui.Id || e.Before?.Channel != null && e.After?.Channel?.Id == cliqueAqui.Id)
                {
                    if (membro.Roles.Contains(membroRegistradoCargo))
                    {
                        string nomeAntigo = "📌 Clique aqui!";
                        await cliqueAqui.ModifyAsync(c => c.Name = $"📌 Sala criada!");

                        new Thread(async () =>
                        {
                            await Task.Delay(TimeSpan.FromSeconds(4));

                            await cliqueAqui.ModifyAsync(c => c.Name = nomeAntigo);
                        }).Start();

                        canalDoMembro = membro.Presence?.Activity?.ActivityType != ActivityType.Custom ? await ubgeServidor.CreateChannelAsync(!string.IsNullOrWhiteSpace(membro.Presence?.Activity?.Name) ? membro.Presence.Activity.Name : $"Sala do: {this.Utilities.DiscordNick(membro)}#{membro.Discriminator}", ChannelType.Voice, cliqueAqui.Parent) : await ubgeServidor.CreateChannelAsync($"Sala do: {this.Utilities.DiscordNick(membro)}#{membro.Discriminator}", ChannelType.Voice, cliqueAqui.Parent);

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
                                membrosNaSala = 1,
                                _id = new ObjectId(),
                            });

                            await canalDoMembro.PlaceMemberAsync(membro);
                        }
                        else
                        {
                            if (resultadoSalas[0].salaTrancada)
                            {
                                await canalDoMembro.AddOverwriteAsync(ubgeServidor.EveryoneRole, Permissions.AccessChannels, Permissions.UseVoice | Permissions.Speak | Permissions.UseVoiceDetection);
                                await canalDoMembro.AddOverwriteAsync(membro, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice | Permissions.UseVoiceDetection);
                                await canalDoMembro.AddOverwriteAsync(botsMusicaisCargo, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice | Permissions.UseVoiceDetection);
                                await canalDoMembro.AddOverwriteAsync(acessoGeralCargo, Permissions.AccessChannels, Permissions.UseVoice | Permissions.Speak | Permissions.UseVoiceDetection);
                                await canalDoMembro.AddOverwriteAsync(moderadorDiscordCargo, Permissions.AccessChannels, Permissions.UseVoice | Permissions.Speak | Permissions.UseVoiceDetection);
                                await canalDoMembro.AddOverwriteAsync(membroRegistradoCargo, Permissions.AccessChannels, Permissions.UseVoice | Permissions.Speak | Permissions.UseVoiceDetection);

                                await salas.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(u => u.salaTrancada, true));

                                foreach (var idsMembros in resultadoSalas[0].idsPermitidos)
                                {
                                    membrosForeach = await ubgeServidor.GetMemberAsync(idsMembros);

                                    await canalDoMembro.AddOverwriteAsync(membrosForeach, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice | Permissions.UseVoiceDetection);
                                }
                            }
                            else
                            {
                                await canalDoMembro.AddOverwriteAsync(ubgeServidor.EveryoneRole, Permissions.AccessChannels | Permissions.UseVoice | Permissions.Speak | Permissions.UseVoiceDetection);
                                await canalDoMembro.AddOverwriteAsync(botsMusicaisCargo, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice | Permissions.UseVoiceDetection);

                                await salas.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(u => u.salaTrancada, false));
                            }

                            if (resultadoSalas.LastOrDefault().limiteDeUsuarios != 0)
                                await canalDoMembro.ModifyAsync(x => x.Userlimit = resultadoSalas.LastOrDefault().limiteDeUsuarios);

                            await salas.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(x => x.idDaSala, canalDoMembro.Id).Set(y => y.membrosNaSala, ulong.Parse("1")));

                            await canalDoMembro.PlaceMemberAsync(membro);
                        }
                    }
                    else
                    {
                        try
                        {
                            comandosBot = ubgeServidor.GetChannel(Values.Chats.channelComandosBot);
                            batePapo = ubgeServidor.GetChannel(Values.Chats.channelBatePapo);

                            await batePapo.PlaceMemberAsync(membro);

                            embed.WithAuthor("Você precisa ter o cargo de membro registrado para criar salas de voz!", null, Values.logoUBGE)
                                .WithDescription("Para isso, digite o comando `!membro` para fazer o censo comunitário e ter acesso à salas privadas!");

                            await comandosBot.SendMessageAsync(embed: embed.Build(), content: membro.Mention);
                            await pvMembro.SendMessageAsync(embed: embed.Build(), content: membro.Mention);
                        }
                        catch (UnauthorizedException)
                        {
                            this.Logger.Warning(Log.TypeWarning.Discord, "Não foi possível enviar a mensagem de pedido para fazer o censo no privado do membro.");

                            await this.Logger.EmbedLogMessages(Log.TypeEmbed.Warning, "Erro!", "Não foi possível enviar a mensagem de pedido para fazer o censo no privado do membro.");
                        }
                        catch (Exception exception)
                        {
                            await this.Logger.Error(Log.TypeError.Discord, exception);
                        }
                    }
                }
                else if (e.After?.Channel != null)
                {
                    var filtro = Builders<Salas>.Filter.Eq(s => s.idDaSala, e.After.Channel.Id);
                    var respostaSalas = (await (await salas.FindAsync(filtro)).ToListAsync());

                    if (respostaSalas.Count != 0 && e.After?.Channel?.Id == respostaSalas.LastOrDefault().idDaSala)
                    {
                        if (e.Before == null || e.Before?.Channel?.Id != cliqueAqui.Id)
                        {
                            var filtroSala = Builders<Salas>.Filter.Eq(s => s.idDaSala, e.After.Channel.Id);
                            var respostaSala = await (await salas.FindAsync(filtroSala)).ToListAsync();

                            await salas.UpdateOneAsync(filtroSala, Builders<Salas>.Update.Set(x => x.membrosNaSala, respostaSala.LastOrDefault().membrosNaSala + 1));
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                await this.Logger.Error(Log.TypeError.Discord, exception);
            }
        }

        async Task GuildsDownloadCompleted(GuildDownloadCompletedEventArgs e)
        {
            if (ConnectedToMongo)
            {
                await CheckReactionsOnReactRoleWhenStart(this);
                await CheckChannelsCreateYourRoomHere(this);
            }

            await CheckConnecionOnMongo(this);
        }

        async Task CheckReactionsOnReactRoleWhenStart(UBGE_Bot bot)
        {
            var discord = bot.DiscordClient;

            var rolesCollection = LocalDB.GetCollection<Jogos>(Values.Mongo.jogos);
            var reactsCollection = LocalDB.GetCollection<Reacts>(Values.Mongo.reacts);
            var resultReacts = await (await reactsCollection.FindAsync(Builders<Reacts>.Filter.Empty)).ToListAsync();
            

            DiscordChannel channel = null;
            DiscordMessage messageInChannel = null;
            DiscordRole roleInServer = null, prisioneiroRole = null;
            DiscordGuild server = null;
            DiscordMember memberInServer = null;

            int n = 0;
            string categoryName = string.Empty;

            var roles = new List<Jogos>();
            var categories = new ConcurrentDictionary<string, string>();
            var emojiRole = new ConcurrentDictionary<ulong, ulong>();
            var members = new ConcurrentDictionary<DiscordEmoji, IReadOnlyList<DiscordUser>>();

            
            foreach (var r in resultReacts)
                categories.TryAdd($"{r.servidor} {n++} {r.idDoCanal}", $"{r.categoria}@ {r.idDaMensagem}");

            foreach (var category in categories)
            {
                ulong.TryParse(category.Key.Split(' ')[2].Replace(" ", ""), out ulong idChannel);

                channel = await discord.GetChannelAsync(idChannel);
                server = channel.Guild;
                messageInChannel = await channel.GetMessageAsync(ulong.Parse(category.Value.Split('@')[1].Replace(" ", "")));

                categoryName = category.Value.Split('@')[0];

                roles = await (await rolesCollection.FindAsync(Builders<Jogos>.Filter.Eq(x => x.nomeDaCategoria, categoryName))).ToListAsync();

                foreach (var c in roles)
                {
                    if (!string.IsNullOrWhiteSpace(c.nomeDaCategoria))
                        emojiRole.TryAdd(c.idDoEmoji, c.idDoCargo);
                }

                foreach (var reaction in messageInChannel.Reactions)
                {
                    var emoji = reaction.Emoji;

                    members.TryAdd(emoji, await messageInChannel.GetReactionsAsync(emoji));
                }

                foreach (var emoji in members.Keys)
                {
                    foreach (var member in members[emoji])
                    {
                        if (!member.IsBot)
                        {
                            roleInServer = server.GetRole(emojiRole[emoji.Id]);

                            if (roleInServer == null)
                                continue;

                            if (server.Members.Keys.Contains(member.Id) && server.Roles.Keys.Contains(roleInServer.Id))
                            {
                                memberInServer = await server.GetMemberAsync(member.Id);

                                prisioneiroRole = server.Id == Values.Guilds.guildUBGE ? server.GetRole(Values.Roles.rolePrisioneiro) : null;

                                if (!memberInServer.Roles.Contains(roleInServer) && prisioneiroRole != null && !memberInServer.Roles.Contains(prisioneiroRole))
                                {
                                    await memberInServer.GrantRoleAsync(roleInServer);

                                    this.Logger.Warning(Log.TypeWarning.SAC, $"[S.A.C] - Sistema de Adicionar Cargos | Foi adicionado o cargo de: \"{roleInServer.Name}\" no: \"{this.Utilities.DiscordNick(memberInServer)}#{memberInServer.Discriminator}\".");

                                    await this.Logger.EmbedLogMessages(Log.TypeEmbed.SAC, "[S.A.C] - Sistema de Adicionar Cargos", $"Foi adicionado o cargo de: {roleInServer.Mention} no: {this.Utilities.DiscordNick(memberInServer)}.", server.IconUrl, member);
                                }
                            }
                        }
                    }
                }

                roles.Clear();
                categories.Clear();
                emojiRole.Clear();
                members.Clear();

                this.Logger.Warning(Log.TypeWarning.SAC, $"A sincronização de cargos da categoria: \"{categoryName}\" foi concluída!");
                await this.Logger.EmbedLogMessages(Log.TypeEmbed.SAC, $"A sincronização de cargos da categoria: \"{categoryName}\" foi concluída!", ":wink:", discord.CurrentUser.AvatarUrl, discord.CurrentUser);
            }

            this.Logger.Warning(Log.TypeWarning.SAC, "A sincronização de cargos foi finalizada!");
            await this.Logger.EmbedLogMessages(Log.TypeEmbed.SAC, "A sincronização de cargos foi finalizada!", ":wink:", discord.CurrentUser.AvatarUrl, discord.CurrentUser);
        }

        async Task CheckChannelsCreateYourRoomHere(UBGE_Bot bot)
        {
            try
            {
                if (!ChannelsCheckWasStarted)
                    ChannelsCheckWasStarted = true;

                var discord = bot.DiscordClient;

                var guildUBGE = await discord.GetGuildAsync(Values.Guilds.guildUBGE);
                var categoriaOutrosCanais = guildUBGE.GetChannel(Values.Chats.Categories.categoryCliqueAqui);
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

                var cliqueAquiVoz = canaisDaCategoria.Find(x => x.Name == nomeCliqueAqui);

                var batePapo = guildUBGE.GetChannel(Values.Chats.channelBatePapo);

                if (cliqueAquiVoz == null)
                {
                    var canalCliqueAqui_ = await guildUBGE.CreateChannelAsync(nomeCliqueAqui, ChannelType.Voice, categoriaOutrosCanais);

                    var cargoMembroRegistrado = guildUBGE.GetRole(Values.Roles.roleMembroRegistrado);

                    await canalCliqueAqui_.AddOverwriteAsync(cargoMembroRegistrado, Permissions.AccessChannels | Permissions.UseVoice, Permissions.Speak);
                    await canalCliqueAqui_.AddOverwriteAsync(guildUBGE.EveryoneRole, Permissions.None, Permissions.AccessChannels | Permissions.UseVoice | Permissions.Speak);

                    string caminhoJson = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\ValoresConfig.json";

                    var json = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(caminhoJson));

                    json["canalCliqueAqui"] = canalCliqueAqui_.Id;

                    File.WriteAllBytes(caminhoJson, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(json, Formatting.Indented)));

                    Program.RestartBot();
                }

                canaisDaCategoria.Remove(cliqueAquiVoz);
                canaisDaCategoria.Remove(batePapo);

                if (cliqueAquiVoz.Users.Count() != 0)
                {
                    foreach (var membro in cliqueAquiVoz.Users)
                        await membro.PlaceInAsync(batePapo);
                }

                if (canaisDaCategoria.Count() != 0)
                {
                    var permissoes = new DiscordOverwriteBuilder
                    {
                        Allowed = Permissions.ManageChannels,
                        Denied = Permissions.None,
                    };

                    foreach (var canal in canaisDaCategoria)
                    {
                        var permissoesDoCanal = canal.PermissionOverwrites.ToList();

                        if (canal.Users.Count() == 0 && permissoesDoCanal.Exists(x => x.Type == OverwriteType.Role && x.Allowed == permissoes.Allowed && x.Id == Values.Roles.roleUBGEBot))
                            await canal.DeleteAsync();
                    }
                }
            }
            catch (Exception exception)
            {
                await this.Logger.Error(Log.TypeError.Discord, exception);
            }
        }

        async Task CheckConnecionOnMongo(UBGE_Bot bot)
        {
            if (!ConnectedToMongo)
            {
                var discord = bot.DiscordClient;

                var canal = (await discord.GetGuildAsync(Values.Guilds.guildUBGE)).GetChannel(Values.Chats.channelUBGEBot);

                if (!(await canal.GetMessagesAsync(1)).LastOrDefault().Content.Contains("Não foi possível conectar ao MongoDB!"))
                    await canal.SendMessageAsync("Não foi possível conectar ao MongoDB! Alguns comandos e sistemas podem estar indisponíveis. :cry:");

                await this.Logger.EmbedLogMessages(Log.TypeEmbed.Warning, "Não foi possível conectar ao MongoDB! Alguns comandos e sistemas podem estar indisponíveis.", ":cry:");
            }
        }

        async Task HideVoiceChannels(VoiceStateUpdateEventArgs e)
        {
            if (e.Guild.Id != Values.Guilds.guildUBGE)
                return;

            try
            {
                var UBGE = e.Guild;
                var ubgeBot = await UBGE.GetMemberAsync(Values.Guilds.Members.memberUBGEBot);

                var everyoneUBGE = UBGE.EveryoneRole;

                DiscordChannel canalAntes = e.Before?.Channel, canalDepois = e.After?.Channel;

                var canaisDeVozDaUBGE = UBGE.Channels.Values.Where(x => x.Type == ChannelType.Voice && x.Parent.Id != Values.Chats.Categories.categoryUBGE && x.Parent.Id != Values.Chats.Categories.categoryCliqueAqui && x.Parent.Id != Values.Chats.Categories.categoryConselhoComunitario);

                var permissao = new DiscordOverwriteBuilder
                {
                    Allowed = Permissions.None,
                    Denied = Permissions.AccessChannels | Permissions.UseVoice,
                };

                foreach (var canal in canaisDeVozDaUBGE)
                {
                    var permissoesDoCanal = canal.PermissionOverwrites.ToList();

                    if (canal.Users.Count() == 0 && !permissoesDoCanal.Exists(x => x.Type == OverwriteType.Role && x.Id == everyoneUBGE.Id && x.Denied == permissao.Denied))
                        await canal.AddOverwriteAsync(everyoneUBGE, Permissions.None, Permissions.AccessChannels | Permissions.UseVoice);
                    else if (canal.Users.Count() != 0 && permissoesDoCanal.Exists(x => x.Type == OverwriteType.Role && x.Id == everyoneUBGE.Id && x.Denied == permissao.Denied))
                        await canal.AddOverwriteAsync(everyoneUBGE, Permissions.AccessChannels, Permissions.UseVoice);
                    else
                        continue;
                }
            }
            catch (Exception exception)
            {
                await this.Logger.Error(Log.TypeError.Discord, exception);
            }
        }

        async Task Ready(ReadyEventArgs e)
            => await e.Client.UpdateStatusAsync(new DiscordActivity { ActivityType = ActivityType.Playing, Name = "Bem-Vindo a UBGE!" });

        async Task MemberChanged(GuildMemberUpdateEventArgs e)
        {
            if (e.Guild.Id != Values.Guilds.guildUBGE)
                return;

            var UBGE = e.Guild;
            var membroDiscord = e.Member;

            DiscordRole cargoNitroBooster = UBGE.GetRole(Values.Roles.roleNitroBooster), cargoDoador = UBGE.GetRole(Values.Roles.roleDoador);

            if (!e.RolesBefore.Contains(cargoNitroBooster) && e.RolesAfter.Contains(cargoNitroBooster))
                await membroDiscord.GrantRoleAsync(cargoDoador);
            else if (e.RolesBefore.Contains(cargoNitroBooster) && !e.RolesAfter.Contains(cargoNitroBooster))
                await membroDiscord.RevokeRoleAsync(cargoDoador);
        }

        async Task MemberAddedOnServer(GuildMemberAddEventArgs e)
        {
            if (e.Guild.Id != Values.Guilds.guildUBGE)
                return;

            try
            {
                var acessoGeralCargo = e.Guild.GetRole(Values.Roles.roleAcessoGeral);
                var privadoMembro = await e.Member.CreateDmChannelAsync();
                var comandosBot = e.Guild.GetChannel(Values.Chats.channelComandosBot);

                await e.Member.GrantRoleAsync(acessoGeralCargo);
                await privadoMembro.SendMessageAsync($"*{e.Member.Mention}, Bem-Vindo a UBGE!*\n\n" +
                $"Leia a mensagem que o Mee6 lhe enviou no seu privado, ele lhe ajudará a dar os seus primeiros passos na UBGE.\n\n" +
                $"Para qualquer dúvida sobre mim, digite `//ajuda`.\n\n" +
                $"Obrigado por ler isso, e antes de tudo, sinta-se em casa! :smile:");
            }
            catch (UnauthorizedException)
            {
                this.Logger.Warning(Log.TypeWarning.Discord, "Não foi possível enviar a mensagem de pedido para fazer o censo no privado do membro.");

                await this.Logger.EmbedLogMessages(Log.TypeEmbed.Warning, "Erro!", "Não foi possível enviar a mensagem de pedido para fazer o censo no privado do membro.");
            }
            catch (Exception exception)
            {
                await this.Logger.Error(Log.TypeError.Discord, exception);
            }
        }

        async Task MessageCreated(MessageCreateEventArgs e)
        {
            try
            {
                var clientDiscord = e.Client;
                var canalMensagem = e.Channel;
                var donoMensagem = e.Author;
                var mensagem = e.Message;
                var servidor = e.Guild;

                var UBGE = await clientDiscord.GetGuildAsync(Values.Guilds.guildUBGE);

                DiscordMember donoMensagem_ = null;

                if (UBGE.Members.Keys.Contains(donoMensagem.Id))
                    donoMensagem_ = await UBGE.GetMemberAsync(donoMensagem.Id);

                var collectionModMail = this.LocalDB.GetCollection<ModMail>(Values.Mongo.modMail);

                if (!canalMensagem.IsPrivate)
                {
                    if (canalMensagem.Id == Values.Chats.channelFormularioAlerta)
                    {
                        if (donoMensagem.IsBot)
                        {
                            await mensagem.CreateReactionAsync(DiscordEmoji.FromName(clientDiscord, ":white_check_mark:"));
                            await mensagem.CreateReactionAsync(DiscordEmoji.FromName(clientDiscord, ":negative_squared_cross_mark:"));
                        }
                    }
                    else if (canalMensagem.Id == Values.Chats.channelRecomendacoesPromocoes)
                    {
                        if (mensagem.Content.ToLower().Contains("http") || mensagem.Content.ToLower().Contains("https"))
                        {
                            await mensagem.CreateReactionAsync(DiscordEmoji.FromName(clientDiscord, ":thumbsup:"));
                            await mensagem.CreateReactionAsync(DiscordEmoji.FromName(clientDiscord, ":thumbsdown:"));
                        }
                    }

                    var resultadoModMailIdDoCanal = await (await collectionModMail.FindAsync(Builders<ModMail>.Filter.Eq(x => x.idDoCanal, canalMensagem.Id))).ToListAsync();

                    if (resultadoModMailIdDoCanal.Count != 0 && !donoMensagem.IsBot && (mensagem.Content.ToLower().StartsWith("//responder") || 
                        mensagem.Content.ToLower().StartsWith("ubge!responder") || 
                        mensagem.Content.ToLower().StartsWith("//r") || 
                        mensagem.Content.ToLower().StartsWith("ubge!r")))
                    {
                        var ultimoResultadoModMailIdDoCanal = resultadoModMailIdDoCanal.LastOrDefault();

                        bool canalFechadoIdDoCanal = true;

                        if (ultimoResultadoModMailIdDoCanal.denuncia == null)
                            canalFechadoIdDoCanal = ultimoResultadoModMailIdDoCanal.contato.oCanalFoiFechado;
                        else if (ultimoResultadoModMailIdDoCanal.contato == null)
                            canalFechadoIdDoCanal = ultimoResultadoModMailIdDoCanal.denuncia.oCanalFoiFechado;

                        if (!canalFechadoIdDoCanal && canalMensagem.Id == ultimoResultadoModMailIdDoCanal.idDoCanal)
                        {
                            var pvMembro = await (await UBGE.GetMemberAsync(resultadoModMailIdDoCanal.LastOrDefault().idDoMembro)).CreateDmChannelAsync();

                            string nomeMembroNoDiscordModMail = this.Utilities.DiscordNick(donoMensagem_);

                            var mensagemAnexadas = mensagem.Attachments;

                            var embed = new DiscordEmbedBuilder();

                            embed.WithAuthor($"Mensagem enviada por: \"{nomeMembroNoDiscordModMail}#{donoMensagem_.Discriminator}\"", null, Values.logoUBGE)
                                .WithColor(this.Utilities.HelpCommandsColor())
                                .WithThumbnailUrl(donoMensagem_.AvatarUrl)
                                .WithTimestamp(DateTime.Now);

                            string msg = mensagem.Content.Replace("//r ", "").Replace("//R ", "").Replace("//r", "")
                            .Replace("//R", "").Replace("ubge!r", "").Replace("UBGE!r", "").Replace("ubge!r ", "")
                            .Replace("UBGE!R ", "").Replace("//responder", "").Replace("//responder ", "")
                            .Replace("//RESPONDER", "").Replace("//RESPONDER ", "").Replace("ubge!responder", "")
                            .Replace("ubge!responder ", "").Replace("UBGE!RESPONDER", "").Replace("UBGE!RESPONDER ", "");

                            if (mensagemAnexadas.Count != 0)
                            {
                                foreach (var arquivos in mensagemAnexadas)
                                {
                                    string anexo = string.Empty;

                                    if (string.IsNullOrWhiteSpace(msg))
                                    {
                                        await pvMembro.SendMessageAsync($"**{nomeMembroNoDiscordModMail}#{donoMensagem_.Discriminator}** - **[{donoMensagem_.Roles.OrderByDescending(x => x.Position).FirstOrDefault().Name}]** → *Nenhuma mensagem foi anexada a este link.*\n{arquivos.Url}");
                                        await mensagem.DeleteAsync();

                                        if (arquivos.Url.ToLower().Contains(".jpg") || arquivos.Url.ToLower().Contains(".png") || arquivos.Url.ToLower().Contains(".gif") || arquivos.Url.ToLower().Contains(".jpeg") || arquivos.Url.ToLower().Contains(".bmp"))
                                            embed.WithImageUrl(arquivos.Url);
                                        else
                                            anexo += $"{arquivos.Url}, ";

                                        embed.WithDescription($"Nenhuma mensagem foi anexada a este link.{(string.IsNullOrWhiteSpace(anexo) ? string.Empty : $"\nLinks que não foram reconhecidos por mim: {(anexo.EndsWith(", ") ? anexo.Remove(anexo.Length - 2) : anexo)}")}");

                                        await canalMensagem.SendMessageAsync(embed: embed.Build());

                                        continue;
                                    }

                                    await pvMembro.SendMessageAsync($"**{nomeMembroNoDiscordModMail}#{donoMensagem_.Discriminator}** - **[{donoMensagem_.Roles.OrderByDescending(x => x.Position).FirstOrDefault().Name}]** → {msg}");
                                    await pvMembro.SendMessageAsync(arquivos.Url);

                                    await mensagem.DeleteAsync();

                                    if (arquivos.Url.ToLower().Contains(".jpg") || arquivos.Url.ToLower().Contains(".png") || arquivos.Url.ToLower().Contains(".gif") || arquivos.Url.ToLower().Contains(".jpeg") || arquivos.Url.ToLower().Contains(".bmp"))
                                        embed.WithImageUrl(arquivos.Url);
                                    else
                                        anexo += $"{arquivos.Url}, ";

                                    embed.WithDescription($"{msg}{(string.IsNullOrWhiteSpace(anexo) ? string.Empty : $"\n\nLinks que não foram reconhecidos por mim: {(anexo.EndsWith(", ") ? anexo.Remove(anexo.Length - 2) : anexo)}")}");

                                    await canalMensagem.SendMessageAsync(embed: embed.Build());
                                }

                                return;
                            }

                            await pvMembro.SendMessageAsync($"**{nomeMembroNoDiscordModMail}#{donoMensagem_.Discriminator}** - **[{donoMensagem_.Roles.OrderByDescending(x => x.Position).FirstOrDefault().Name}]** → {msg}");

                            await mensagem.DeleteAsync();

                            embed.WithDescription(msg);

                            await canalMensagem.SendMessageAsync(embed: embed.Build());
                        }
                    }

                    if (canalMensagem.Id != Values.Chats.channelTesteDoBot || 
                        canalMensagem.Id != Values.Chats.channelPRServidor ||
                        canalMensagem.Id != Values.Chats.channelComandosBot || 
                        canalMensagem.Id != Values.Chats.channelCrieSuaSalaAqui ||
                        canalMensagem.Id != Values.Chats.channelUBGEBot || 
                        !donoMensagem.IsBot)
                    {
                        var colecao = this.LocalDB.GetCollection<Levels>(Values.Mongo.levels);
                        var filtro = Builders<Levels>.Filter.Eq(x => x.idDoMembro, donoMensagem.Id);

                        var lista = await (await colecao.FindAsync(filtro)).ToListAsync();

                        ulong xpAleatorio = ulong.Parse(new Random().Next(1, 20).ToString());

                        ulong numeroLevel = 1;

                        if (lista.Count == 0)
                            await colecao.InsertOneAsync(new Levels { idDoMembro = donoMensagem.Id, xpDoMembro = 1, nomeDoLevel = $"{numeroLevel}", diaEHora = DateTime.Now.ToString() });
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

                string nomeMembroNoDiscord = this.Utilities.DiscordNick(donoMensagem_);

                DiscordRole cargoModeradorDiscord = UBGE.GetRole(Values.Roles.roleModeradorDiscord),
                cargoComiteComunitario = UBGE.GetRole(Values.Roles.roleComiteComunitario),
                cargoConselheiro = UBGE.GetRole(Values.Roles.roleConselheiro);

                DiscordChannel votacoesConselho = UBGE.GetChannel(Values.Chats.channelVotacoesConselho),
                modMailUBGE = UBGE.GetChannel(Values.Chats.Categories.categoryModMailBot);

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
                            var embed = new DiscordEmbedBuilder();
                            var canalModMail = UBGE.GetChannel(ultimoResultadoModMail.idDoCanal);

                            var mensagensAnexadas = mensagem.Attachments;

                            string anexo = string.Empty;

                            embed.WithAuthor($"Mensagem recebida de: \"{nomeMembroNoDiscord}#{donoMensagem_.Discriminator}\"", null, Values.logoUBGE)
                                .WithColor(this.Utilities.HelpCommandsColor())
                                .WithThumbnailUrl(donoMensagem_.AvatarUrl)
                                .WithTimestamp(DateTime.Now);

                            if (mensagensAnexadas.Count != 0)
                            {
                                foreach (var arquivos in mensagensAnexadas)
                                {
                                    if (arquivos.Url.ToLower().Contains(".jpg") || arquivos.Url.ToLower().Contains(".png") || arquivos.Url.ToLower().Contains(".gif") || arquivos.Url.ToLower().Contains(".jpeg") || arquivos.Url.ToLower().Contains(".bmp"))
                                        embed.WithImageUrl(arquivos.Url);
                                    else
                                        anexo += $"{arquivos.Url}, ";

                                    embed.WithDescription(string.IsNullOrWhiteSpace(mensagem.Content) ? $"Não há alguma outra mensagem.{(string.IsNullOrWhiteSpace(anexo) ? string.Empty : $"\n\nLinks que não foram reconhecidos por mim: {(anexo.EndsWith(", ") ? anexo.Remove(anexo.Length - 2) : anexo)}")}" : $"{mensagem.Content}{(string.IsNullOrWhiteSpace(anexo) ? string.Empty : $"\n\nLinks que não foram reconhecidos por mim: {(anexo.EndsWith(", ") ? anexo.Remove(anexo.Length - 2) : anexo)}")}");

                                    await canalModMail.SendMessageAsync(embed: embed.Build());
                                }

                                return;
                            }

                            embed.WithDescription(mensagem.Content);

                            await canalModMail.SendMessageAsync(embed: embed.Build());

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
                        var embed = new DiscordEmbedBuilder();

                        var interactivity = clientDiscord.GetInteractivity();

                        DiscordEmoji emojiDenuncia = DiscordEmoji.FromName(clientDiscord, ":oncoming_police_car:"),
                        emojiSugestao = DiscordEmoji.FromName(clientDiscord, ":star:"),
                        emojiContatoStaff = this.Utilities.FindEmoji(clientDiscord, "LOGO_UBGE_2");

                        embed.WithAuthor("O que você deseja fazer?", null, Values.logoUBGE)
                            .WithColor(this.Utilities.RandomColorEmbed())
                            .WithDescription($"{emojiDenuncia} - Denúncia\n" +
                            $"{emojiSugestao} - Sugestão\n" +
                            $"{emojiContatoStaff} - Contato")
                            .WithTimestamp(DateTime.Now)
                            .WithThumbnailUrl(donoMensagem.AvatarUrl);

                        var msgEscolhaMembro = await canalMensagem.SendMessageAsync(embed: embed.Build());
                        await msgEscolhaMembro.CreateReactionAsync(emojiDenuncia);
                        await msgEscolhaMembro.CreateReactionAsync(emojiSugestao);
                        await msgEscolhaMembro.CreateReactionAsync(emojiContatoStaff);

                        var emojiResposta = (await interactivity.WaitForReactionAsync(msgEscolhaMembro, donoMensagem, TimeSpan.FromMinutes(1))).Result?.Emoji;

                        var modMail = new ModMail
                        {
                            idDoMembro = donoMensagem_.Id
                        };

                        string nickMembroNoCanal = nomeMembroNoDiscord.Replace("[", "").Replace("]", "").Replace("'", "").Replace("\"", "").Replace("!", "").Replace("_", "").Replace("-", "").Replace("=", "").Replace("<", "").Replace("<", "").Replace(".", "").Replace(",", "").Replace("`", "").Replace("´", "").Replace("+", "").Replace("/", "").Replace("\\", "").Replace(":", "").Replace(";", "").Replace("{", "").Replace("}", "").Replace("ª", "").Replace("º", "").Replace(" ", "");

                        if (emojiResposta == emojiDenuncia)
                        {
                            this.Utilities.ClearEmbed(embed);

                            embed.WithAuthor("Qual motivo de sua denúncia?", null, Values.logoUBGE)
                                .WithDescription("Digite ela abaixo para entendermos melhor e você entrará em contato direto com a staff.")
                                .WithColor(this.Utilities.RandomColorEmbed());

                            var perguntaMotivoDenuncia = await canalMensagem.SendMessageAsync(embed: embed.Build());
                            string esperaRespostaMotivoDenuncia = await this.Utilities.GetAnswerDM(interactivity, donoMensagem_, canalMensagem);

                            this.Utilities.ClearEmbed(embed);

                            embed.WithAuthor("Qual a sua denúncia?", null, Values.logoUBGE)
                                .WithColor(this.Utilities.RandomColorEmbed());

                            var perguntaDenuncia = await canalMensagem.SendMessageAsync(embed: embed.Build());
                            string esperaRespostaDenuncia = await this.Utilities.GetAnswerDM(interactivity, donoMensagem_, canalMensagem);

                            string diaHoraDenunciaDoMembro = DateTime.Now.ToString();

                            var canalMembroDaDenuncia = await UBGE.CreateTextChannelAsync($"{nickMembroNoCanal}-{donoMensagem_.Discriminator}", modMailUBGE);

                            await canalMembroDaDenuncia.AddOverwriteAsync(cargoComiteComunitario, Permissions.None, Permissions.AccessChannels | Permissions.SendMessages);
                            await canalMembroDaDenuncia.AddOverwriteAsync(cargoConselheiro, Permissions.None, Permissions.AccessChannels | Permissions.SendMessages);

                            modMail.idDoCanal = canalMembroDaDenuncia.Id;
                            modMail.denuncia = new Denuncia
                            {
                                denunciaDoMembro = esperaRespostaDenuncia,
                                motivoDaDenunciaDoMembro = esperaRespostaMotivoDenuncia,
                                diaHoraDenuncia = diaHoraDenunciaDoMembro,
                                oCanalFoiFechado = false,
                            };

                            await collectionModMail.InsertOneAsync(modMail);

                            this.Utilities.ClearEmbed(embed);

                            embed.WithAuthor($"Denúncia do membro: \"{nomeMembroNoDiscord}#{donoMensagem_.Discriminator}\"", null, Values.logoUBGE)
                                .WithColor(this.Utilities.RandomColorEmbed())
                                .WithDescription($"Quando foi feita esta denúncia: **{diaHoraDenunciaDoMembro}**\n\n" +
                                $"Motivo da denúncia: **{modMail.denuncia.motivoDaDenunciaDoMembro}**\n" +
                                $"Denúncia: **{modMail.denuncia.denunciaDoMembro}**")
                                .WithThumbnailUrl(donoMensagem_.AvatarUrl)
                                .WithTimestamp(DateTime.Now);

                            await canalMembroDaDenuncia.SendMessageAsync(embed: embed.Build(), content: cargoModeradorDiscord.Mention);

                            this.Utilities.ClearEmbed(embed);

                            embed.WithAuthor("Sua denúncia foi enviada a staff da UBGE!", null, Values.logoUBGE)
                                .WithColor(this.Utilities.RandomColorEmbed())
                                .WithDescription("A staff irá ler e provalmente irá fazer diversas perguntas, e qualquer mensagem enviada por eles eu enviarei para você, fique atento a seu privado e responda normalmente! :wink:")
                                .WithThumbnailUrl(donoMensagem_.AvatarUrl)
                                .WithTimestamp(DateTime.Now);

                            await canalMensagem.SendMessageAsync(embed: embed.Build());
                        }
                        else if (emojiResposta == emojiSugestao)
                        {
                            this.Utilities.ClearEmbed(embed);

                            embed.WithAuthor("Digite aqui sua sugestão para a UBGE!", null, Values.logoUBGE)
                                .WithDescription("Digite aqui sua sugestão, logo após ela irá entrar em votação no conselho comunitário!\n\n**ATENÇÃO! DIGITE UMA ÚNICA MENSAGEM, NÃO ENVIE A MENSAGEM PELA METADE!**")
                                .WithColor(this.Utilities.RandomColorEmbed());

                            var perguntaSugestao = await canalMensagem.SendMessageAsync(embed: embed.Build());
                            string esperaSugestao = await this.Utilities.GetAnswerDM(interactivity, donoMensagem_, canalMensagem);

                            string diaHoraSugestaoDoMembro = DateTime.Now.ToString();

                            modMail.sugestao = new Sugestao
                            {
                                diaHoraSugestao = diaHoraSugestaoDoMembro,
                                sugestaoDoMembro = esperaSugestao,
                            };

                            await collectionModMail.InsertOneAsync(modMail);

                            this.Utilities.ClearEmbed(embed);

                            embed.WithAuthor($"Sugestão do membro: \"{nomeMembroNoDiscord}#{donoMensagem_.Discriminator}\"", null, Values.logoUBGE)
                                .WithColor(this.Utilities.RandomColorEmbed())
                                .WithDescription(esperaSugestao)
                                .WithThumbnailUrl(donoMensagem_.AvatarUrl)
                                .WithTimestamp(DateTime.Now);

                            var mensagemSugestao = await votacoesConselho.SendMessageAsync(embed: embed.Build(), content: cargoConselheiro.Mention);
                            await mensagemSugestao.CreateReactionAsync(DiscordEmoji.FromName(clientDiscord, ":white_check_mark:"));
                            await mensagemSugestao.CreateReactionAsync(DiscordEmoji.FromName(clientDiscord, ":negative_squared_cross_mark:"));

                            this.Utilities.ClearEmbed(embed);

                            embed.WithAuthor("Sua sugestão foi enviada para a staff da UBGE!", null, Values.logoUBGE)
                                .WithColor(this.Utilities.RandomColorEmbed())
                                .WithDescription($"Obrigado por fazer um servidor agradável a todos os membros! {this.Utilities.FindEmoji(clientDiscord, "UBGE")}")
                                .WithThumbnailUrl(donoMensagem_.AvatarUrl)
                                .WithTimestamp(DateTime.Now);

                            await canalMensagem.SendMessageAsync(embed: embed.Build());
                        }
                        else if (emojiResposta == emojiContatoStaff)
                        {
                            this.Utilities.ClearEmbed(embed);

                            embed.WithAuthor("Digite aqui seu motivo para entrar em contato com a staff da UBGE!", null, Values.logoUBGE)
                                .WithColor(this.Utilities.RandomColorEmbed());

                            var perguntaMotivoContato = await canalMensagem.SendMessageAsync(embed: embed.Build());
                            string esperaMotivoContato = await this.Utilities.GetAnswerDM(interactivity, donoMensagem_, canalMensagem);

                            string diaHoraContatoDoMembro = DateTime.Now.ToString();

                            var canalMembroContato = await UBGE.CreateTextChannelAsync($"{nickMembroNoCanal}-{donoMensagem_.Discriminator}", modMailUBGE);

                            await canalMembroContato.AddOverwriteAsync(cargoModeradorDiscord, Permissions.None, Permissions.AccessChannels | Permissions.SendMessages);
                            await canalMembroContato.AddOverwriteAsync(cargoConselheiro, Permissions.None, Permissions.AccessChannels | Permissions.SendMessages);

                            modMail.idDoCanal = canalMembroContato.Id;
                            modMail.contato = new Contato
                            {
                                diaHoraContato = diaHoraContatoDoMembro,
                                motivoDoContatoDoMembro = esperaMotivoContato,
                                oCanalFoiFechado = false,
                            };

                            await collectionModMail.InsertOneAsync(modMail);

                            this.Utilities.ClearEmbed(embed);

                            embed.WithAuthor($"O membro: \"{nomeMembroNoDiscord}#{donoMensagem_.Discriminator}\" quer falar com a staff", null, Values.logoUBGE)
                                .WithColor(this.Utilities.RandomColorEmbed())
                                .WithDescription($"Quando foi feito este contato com a staff: **{diaHoraContatoDoMembro}**\n\n" +
                                $"Motivo: **{esperaMotivoContato}**")
                                .WithThumbnailUrl(donoMensagem_.AvatarUrl)
                                .WithTimestamp(DateTime.Now);

                            await canalMembroContato.SendMessageAsync(embed: embed.Build(), content: cargoComiteComunitario.Mention);

                            this.Utilities.ClearEmbed(embed);

                            embed.WithAuthor("Seu pedido de contato foi enviado para a staff da UBGE!", null, Values.logoUBGE)
                                .WithColor(this.Utilities.RandomColorEmbed())
                                .WithDescription("A staff irá ler e provalmente irá fazer diversas perguntas, e qualquer mensagem enviada por eles eu enviarei para você, fique atento a seu privado e responda normalmente! :wink:")
                                .WithThumbnailUrl(donoMensagem_.AvatarUrl)
                                .WithTimestamp(DateTime.Now);

                            await canalMensagem.SendMessageAsync(embed: embed.Build());
                        }
                        else
                            return;

                        return;
                    }
                    catch (Exception exception)
                    {
                        await this.Logger.Error(Log.TypeError.Discord, exception);
                    }
                }).Start();
            }
            catch (Exception exception)
            {
                await this.Logger.Error(Log.TypeError.Discord, exception);
            }
        }

        async Task ReactionAdded(MessageReactionAddEventArgs e)
        {
            var canalMensagem = e.Channel;
            var membro = e.User;

            if (e == null || canalMensagem.IsPrivate || membro.IsBot)
                return;

            var discord = this.DiscordClient;

            var reacts = this.LocalDB.GetCollection<Reacts>(Values.Mongo.reacts);
            var resultadoReacts = await (await reacts.FindAsync(Builders<Reacts>.Filter.Eq(x => x.idDoCanal, canalMensagem.Id))).ToListAsync();

            if (resultadoReacts.Count != 0)
            {
                var jogos = this.LocalDB.GetCollection<Jogos>(Values.Mongo.jogos);

                var emojiReacao = e.Emoji;

                var filtroJogos = Builders<Jogos>.Filter.Eq(x => x.idDoEmoji, emojiReacao.Id);
                var resultadoJogos = await (await jogos.FindAsync(filtroJogos)).ToListAsync();

                if (resultadoJogos.Count == 0)
                    return;

                var guildReaction = e.Guild;
                var mensagemEmoji = e.Message;
                var canalReaction = e.Channel;

                var reacoesMembro = await mensagemEmoji.GetReactionsAsync(emojiReacao);

                var ultimoResultadoJogos = resultadoJogos.LastOrDefault();
                var ultimoResultadoReacts = resultadoReacts.LastOrDefault();

                bool resultadoDiferente = true;

                if (resultadoJogos.Count == 0 || (resultadoJogos.Count != 0 && guildReaction.GetRole(ultimoResultadoJogos.idDoCargo) == null))
                    resultadoDiferente = false;

                var UBGE = await e.Client.GetGuildAsync(Values.Guilds.guildUBGE);

                if (!resultadoDiferente && emojiReacao != null)
                {
                    var ubgeBot_ = UBGE.GetChannel(Values.Chats.channelUBGEBot);

                    await mensagemEmoji.DeleteReactionAsync(emojiReacao, discord.CurrentUser);

                    var mensagemEmbed = await canalReaction.GetMessageAsync(resultadoReacts.LastOrDefault().idDaMensagem);

                    var embedMensagem = mensagemEmbed.Embeds.LastOrDefault();

                    if (embedMensagem.Description.Contains(emojiReacao.ToString()))
                    {
                        var novoEmbed = new DiscordEmbedBuilder(embedMensagem);
                        string descricaoEmbed = embedMensagem.Description;

                        var lista = descricaoEmbed.Split('\n').ToList();

                        var strEmbedFinal = new StringBuilder();

                        for (int linha = 0; linha < lista.Count; linha++)
                        {
                            if (lista[linha].Contains(emojiReacao.ToString()))
                                lista.RemoveAt(linha);

                            strEmbedFinal.Append($"{lista[linha]}\n");
                        }

                        novoEmbed.Description = strEmbedFinal.ToString();
                        novoEmbed.WithAuthor(embedMensagem.Author.Name, null, Values.logoUBGE);
                        novoEmbed.WithColor(new DiscordColor(0x32363c));

                        await mensagemEmbed.ModifyAsync(embed: novoEmbed.Build());

                        await jogos.DeleteOneAsync(filtroJogos);
                    }
                    else if (!embedMensagem.Description.Contains(emojiReacao.ToString()) && emojiReacao != null)
                    {
                        await ExcludeAndRefreshReactions(this, mensagemEmoji, emojiReacao, reacoesMembro, ultimoResultadoReacts, canalReaction, guildReaction, ubgeBot_);

                        return;
                    }

                    await ExcludeAndRefreshReactions(this, mensagemEmoji, emojiReacao, reacoesMembro, ultimoResultadoReacts, canalReaction, guildReaction, ubgeBot_);

                    return;
                }

                var membro_ = guildReaction == UBGE ? await UBGE.GetMemberAsync(membro.Id) : await guildReaction.GetMemberAsync(membro.Id);

                var cargo = guildReaction.GetRole(ultimoResultadoJogos.idDoCargo);

                await membro_.GrantRoleAsync(cargo);

                this.Logger.Warning(Log.TypeWarning.Discord, $"O membro: \"{this.Utilities.DiscordNick(membro_)}#{membro.Discriminator}\" pegou o cargo de: \"{cargo.Name}\".");
                
                await this.Logger.EmbedLogMessages(Log.TypeEmbed.ReactRole, "Cargo Adicionado!", $"{emojiReacao} | O membro: {this.Utilities.DiscordNick(membro_)} pegou o cargo de: {cargo.Mention}.\n\nOu:\n- `@{this.Utilities.DiscordNick(membro_)}#{membro.Discriminator}`\n- `@{cargo.Name}`", discord.CurrentUser.AvatarUrl, membro);
            }
        }

        async Task ReactionRemoved(MessageReactionRemoveEventArgs e)
        {
            var canalMensagem = e.Channel;
            var membro = e.User;

            if (e == null || canalMensagem.IsPrivate || membro.IsBot)
                return;

            var discord = this.DiscordClient;

            var reacts = this.LocalDB.GetCollection<Reacts>(Values.Mongo.reacts);
            var resultadoReacts = await (await reacts.FindAsync(Builders<Reacts>.Filter.Eq(x => x.idDoCanal, canalMensagem.Id))).ToListAsync();

            if (resultadoReacts.Count != 0)
            {
                var jogos = this.LocalDB.GetCollection<Jogos>(Values.Mongo.jogos);

                var emojiReacao = e.Emoji;

                var filtroJogos = Builders<Jogos>.Filter.Eq(x => x.idDoEmoji, emojiReacao.Id);
                var resultadoJogos = await (await jogos.FindAsync(filtroJogos)).ToListAsync();

                if (resultadoJogos.Count == 0)
                    return;

                var guildReaction = e.Guild;
                var mensagemEmoji = e.Message;
                var canalReaction = e.Channel;

                var reacoesMembro = await mensagemEmoji.GetReactionsAsync(emojiReacao);

                var ultimoResultadoJogos = resultadoJogos.LastOrDefault();
                var ultimoResultadoReacts = resultadoReacts.LastOrDefault();

                bool resultadoDiferente = true;

                if (resultadoJogos.Count == 0 || (resultadoJogos.Count != 0 && guildReaction.GetRole(ultimoResultadoJogos.idDoCargo) == null))
                    resultadoDiferente = false;

                var UBGE = await e.Client.GetGuildAsync(Values.Guilds.guildUBGE);

                if (!resultadoDiferente && emojiReacao != null)
                {
                    var ubgeBot_ = UBGE.GetChannel(Values.Chats.channelUBGEBot);

                    await mensagemEmoji.DeleteReactionAsync(emojiReacao, discord.CurrentUser);

                    var mensagemEmbed = await canalReaction.GetMessageAsync(resultadoReacts.LastOrDefault().idDaMensagem);

                    var embedMensagem = mensagemEmbed.Embeds.LastOrDefault();

                    if (embedMensagem.Description.Contains(emojiReacao.ToString()))
                    {
                        var novoEmbed = new DiscordEmbedBuilder(embedMensagem);
                        string descricaoEmbed = embedMensagem.Description;

                        var lista = descricaoEmbed.Split('\n').ToList();

                        var strEmbedFinal = new StringBuilder();

                        for (int linha = 0; linha < lista.Count; linha++)
                        {
                            if (lista[linha].Contains(emojiReacao.ToString()))
                                lista.RemoveAt(linha);

                            strEmbedFinal.Append($"{lista[linha]}\n");
                        }

                        novoEmbed.Description = strEmbedFinal.ToString();
                        novoEmbed.WithAuthor(embedMensagem.Author.Name, null, Values.logoUBGE);
                        novoEmbed.WithColor(new DiscordColor(0x32363c));

                        await mensagemEmbed.ModifyAsync(embed: novoEmbed.Build());

                        await jogos.DeleteOneAsync(filtroJogos);
                    }
                    else if (!embedMensagem.Description.Contains(emojiReacao.ToString()) && emojiReacao != null)
                    {
                        await this.ExcludeAndRefreshReactions(this, mensagemEmoji, emojiReacao, reacoesMembro, ultimoResultadoReacts, canalReaction, guildReaction, ubgeBot_);

                        return;
                    }

                    await this.ExcludeAndRefreshReactions(this, mensagemEmoji, emojiReacao, reacoesMembro, ultimoResultadoReacts, canalReaction, guildReaction, ubgeBot_);

                    return;
                }

                var membro_ = guildReaction == UBGE ? await UBGE.GetMemberAsync(membro.Id) : await guildReaction.GetMemberAsync(membro.Id);

                var cargo = guildReaction.GetRole(ultimoResultadoJogos.idDoCargo);

                await membro_.RevokeRoleAsync(cargo);

                this.Logger.Warning(Log.TypeWarning.Discord, $"O membro: \"{this.Utilities.DiscordNick(membro_)}#{membro.Discriminator}\" removeu o cargo de: \"{cargo.Name}\".");

                await this.Logger.EmbedLogMessages(Log.TypeEmbed.ReactRole, "Cargo removido!", $"{emojiReacao} | O membro: {this.Utilities.MemberMention(membro_)} removeu o cargo de: {cargo.Mention}.\n\nOu:\n- `@{this.Utilities.DiscordNick(membro_)}#{membro.Discriminator}`\n- `@{cargo.Name}`", discord.CurrentUser.AvatarUrl, membro);
            }
        }

        async Task ExcludeAndRefreshReactions(UBGE_Bot bot, DiscordMessage mensagemEmoji, DiscordEmoji emojiReacao, IReadOnlyList<DiscordUser> reacoesMembro, Reacts resultadoReacts, DiscordChannel canalReaction, DiscordGuild guildReaction, DiscordChannel ubgeBot)
        {
            try
            {
                await bot.Utilities.ExcludeReactionFromAMemberList(mensagemEmoji, emojiReacao, reacoesMembro);

                var atualizaMensagem = await canalReaction.GetMessageAsync(resultadoReacts.idDaMensagem);
                var mensagemEmojiAtualizado = await atualizaMensagem.GetReactionsAsync(emojiReacao);

                if (mensagemEmojiAtualizado.Count() != 0)
                    await ubgeBot.SendMessageAsync($":warning:, existem **{mensagemEmojiAtualizado.Count()}** reações sobrando no emoji: {emojiReacao.ToString()} no canal: {canalReaction.Mention}, elas não foram removidas pois os membros saíram da(o): **{guildReaction.Name}**");

                return;
            }
            catch (Exception exception)
            {
                await bot.Logger.Error(Log.TypeError.Discord, exception);
            }
        }
    
        void MessageLoggerDSharpPlus(object sender, DebugLogMessageEventArgs e)
        {
            if (e.Message.ToLower().Contains("socket connection terminated"))
            {
                this.Logger.Warning(Log.TypeWarning.Discord, "A conexão com o Discord foi perdida/encerrada! Reiniciando o bot...");

                Program.RestartBot();
            }
            else if (e.Message.ToLower().Contains("connection attempt failed"))
            {
                this.Logger.Warning(Log.TypeWarning.Discord, "A conexão com o Discord foi perdida/encerrada! Reiniciando o bot...");

                Program.RestartBot();
            }
        }


    }
}