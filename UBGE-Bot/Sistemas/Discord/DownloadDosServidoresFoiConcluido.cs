using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using Timer = System.Timers.Timer;
using System.Threading;
using System.Threading.Tasks;
using UBGE_Bot.Carregamento;
using UBGE_Bot.Main;
using UBGE_Bot.MongoDB.Modelos;
using UBGE_Bot.LogExceptions;
using UBGE_Bot.Utilidades;

namespace UBGE_Bot.Sistemas.Discord
{
    public sealed class DownloadDosServidoresFoiConcluido : IAplicavelAoCliente
    {
        private bool botConectadoAoMongo_ { get; set; }

        public void AplicarAoBot(DiscordClient discordClient, bool botConectadoAoMongo)
        {
            botConectadoAoMongo_ = botConectadoAoMongo;

            discordClient.GuildDownloadCompleted += DownloadDosServidoresFoiConcluidoTask;

            Timer checaCanaisAutoCreate = new Timer()
            {
                Interval = 10000,
            };
            checaCanaisAutoCreate.Elapsed += async delegate
            {
                if (Program.checkDosCanaisFoiIniciado)
                    await ChecaCanaisAutoCreate(Program.ubgeBot);
            };
            checaCanaisAutoCreate.Start();
        }

        private async Task DownloadDosServidoresFoiConcluidoTask(GuildDownloadCompletedEventArgs guildDownloadCompletedEventArgs)
        {
            if (botConectadoAoMongo_)
            {
                await CheckReacoesMarcadasQuandoOBotEstavaOfflineNoReactRole(Program.ubgeBot);
                await FazOCacheDosEmojis(Program.ubgeBot);
                await ChecaCanaisAutoCreate(Program.ubgeBot);
            }

            await EnviaMensagemPraODiscordDeConexaoNoMongo(Program.ubgeBot);
        }

        private async Task CheckReacoesMarcadasQuandoOBotEstavaOfflineNoReactRole(UBGEBot_ ubgeBotClient)
        {
            await Task.Delay(0);

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

                        string nomeDaCategoria = categoria.Value.Split('@')[0];

                        var Cargos = await (await roles.FindAsync(Builders<Jogos>.Filter.Eq(x => x.nomeDaCategoria, nomeDaCategoria))).ToListAsync();

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

                        ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Sistemas, $"A sincronização de cargos da categoria: \"{nomeDaCategoria}\" foi concluída!");
                        await ubgeBotClient.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.SAC, $"A sincronização de cargos da categoria: \"{nomeDaCategoria}\" foi concluída!", ":wink:", ubgeBotClient.discordClient.CurrentUser.AvatarUrl, ubgeBotClient.discordClient.CurrentUser);
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

        private async Task FazOCacheDosEmojis(UBGEBot_ ubgeBotClient)
        {
            await Task.Delay(0);

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

                    Program.emojisCache.AddRange(await ubgeBotClient.utilidadesGerais.RetornaEmojis(ubgeBotClient.discordClient, nomesEmojis));
                }
                catch (Exception exception)
                {
                    await ubgeBotClient.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Sistemas, exception);
                }
            }).Start();
        }

        private async Task ChecaCanaisAutoCreate(UBGEBot_ ubgeBotClient)
        {
            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    if (!Program.checkDosCanaisFoiIniciado)
                        Program.checkDosCanaisFoiIniciado = true;

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
                        DiscordOverwriteBuilder permissoes = new DiscordOverwriteBuilder
                        {
                            Allowed = Permissions.ManageChannels,
                            Denied = Permissions.None,
                        };

                        foreach (var canal in canaisDaCategoria)
                        {
                            var permissoesDoCanal = canal.PermissionOverwrites.ToList();

                            if (canal.Users.Count() == 0 && permissoesDoCanal.Exists(x => x.Type == OverwriteType.Role && x.Allowed == permissoes.Allowed && x.Id == Valores.Cargos.cargoUBGEBot))
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

        private async Task EnviaMensagemPraODiscordDeConexaoNoMongo(UBGEBot_ ubgeBotClient)
        {
            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    if (!botConectadoAoMongo_)
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
    }
}