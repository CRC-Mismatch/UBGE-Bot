using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
        private bool sistemaAtivo_ { get; set; }

        public void AplicarAoBot(DiscordClient discordClient, bool botConectadoAoMongo, bool sistemaAtivo)
        {
            botConectadoAoMongo_ = botConectadoAoMongo;
            sistemaAtivo_ = sistemaAtivo;

            discordClient.GuildDownloadCompleted += DownloadDosServidoresFoiConcluidoTask;

            Timer checaCanaisAutoCreate = new Timer()
            {
                Interval = 30000,
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
            if (botConectadoAoMongo_ && sistemaAtivo_)
            {
                await CheckReacoesMarcadasQuandoOBotEstavaOfflineNoReactRole(Program.ubgeBot);
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
                    IMongoDatabase db = ubgeBotClient.localDB;

                    IMongoCollection<Reacts> reacts = db.GetCollection<Reacts>(Valores.Mongo.reacts);
                    IMongoCollection<Jogos> roles = db.GetCollection<Jogos>(Valores.Mongo.jogos);

                    List<Reacts> resultadoReacts = await (await reacts.FindAsync(Builders<Reacts>.Filter.Empty)).ToListAsync();

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
                    foreach (Reacts React in resultadoReacts)
                        categorias.Add($"{React.servidor} {N++} {React.idDoCanal}", $"{React.categoria}@ {React.idDaMensagem}");

                    foreach (KeyValuePair<string, string> categoria in categorias)
                    {
                        ulong idServidorDictionary = ulong.Parse(categoria.Key.Split(' ')[0].Replace(" ", ""));

                        if (idServidorDictionary == Valores.Guilds.UBGE)
                            servidor = UBGE;
                        else
                            servidor = await ubgeBotClient.discordClient.GetGuildAsync(idServidorDictionary);

                        canalServidor = servidor.GetChannel(ulong.Parse(categoria.Key.Split(' ')[2].Replace(" ", "")));
                        mensagem = await canalServidor.GetMessageAsync(ulong.Parse(categoria.Value.Split('@')[1].Replace(" ", "")));

                        string nomeDaCategoria = categoria.Value.Split('@')[0];

                        List<Jogos> Cargos = await (await roles.FindAsync(Builders<Jogos>.Filter.Eq(x => x.nomeDaCategoria, nomeDaCategoria))).ToListAsync();

                        Dictionary<ulong, ulong> EmojiRole = new Dictionary<ulong, ulong>();
                        foreach (Jogos r in Cargos)
                            if (!string.IsNullOrEmpty(r.nomeDaCategoria))
                                EmojiRole.Add(r.idDoEmoji, r.idDoCargo);

                        Dictionary<DiscordEmoji, IReadOnlyList<DiscordUser>> Usuarios = new Dictionary<DiscordEmoji, IReadOnlyList<DiscordUser>>();
                        foreach (DiscordReaction discordReaction in mensagem.Reactions)
                            Usuarios.Add(discordReaction.Emoji, await mensagem.GetReactionsAsync(discordReaction.Emoji));

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
                                        foreach (DiscordGuild servidoresBot in ubgeBotClient.discordClient.Guilds.Values)
                                        {
                                            cargo = servidoresBot.Roles.Values.ToList().Find(x => x.Id == EmojiRole[emoji.Id]);

                                            if (cargo != null)
                                            {
                                                servidorMembro = servidoresBot;

                                                break;
                                            }
                                        }
                                    }

                                    if (cargo == null)
                                        continue;

                                    if (servidor.Members.Values.FirstOrDefault(em => em.Id == usuario.Id) == null)
                                    { }
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

                                        if (cargo == acessoGeral && membro.Roles.Contains(acessoGeral))
                                        {
                                            await membro.RevokeRoleAsync(acessoGeral);

                                            ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Sistemas, $"[S.A.C] - Sistema de Adicionar Cargos | Foi removido o cargo de: \"{cargo.Name}\" no: \"{ubgeBotClient.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\".");

                                            await ubgeBotClient.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.SAC,
                                                "[S.A.C] - Sistema de Adicionar Cargos",
                                                $"Foi removido o cargo de: {cargo.Mention} no: {ubgeBotClient.utilidadesGerais.MencaoMembro(membro)}.", servidor.IconUrl, membro);
                                        }

                                        if (!membro.Roles.Contains(cargo) && cargo != acessoGeral && !membro.Roles.Contains(prisioneiro))
                                        {
                                            await membro.GrantRoleAsync(cargo);

                                            ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Sistemas, $"[S.A.C] - Sistema de Adicionar Cargos | Foi adicionado o cargo de: \"{cargo.Name}\" no: \"{ubgeBotClient.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\".");

                                            if (membro.Guild.Id == Valores.Guilds.UBGE)
                                            {
                                                await ubgeBotClient.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.SAC,
                                                    "[S.A.C] - Sistema de Adicionar Cargos",
                                                    $"Foi adicionado o cargo de: {cargo.Mention} no: {ubgeBotClient.utilidadesGerais.MencaoMembro(membro)}.", servidor.IconUrl, membro);
                                            }
                                            else
                                            {
                                                await ubgeBotClient.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.SAC,
                                                    "[S.A.C] - Sistema de Adicionar Cargos",
                                                    $"Foi adicionado o cargo de: **{cargo.Name}** no: **{ubgeBotClient.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}**.", servidor.IconUrl, membro);
                                            }
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
                    List<DiscordChannel> canaisDaCategoria = categoriaOutrosCanais.Children.Where(x => x.Type == ChannelType.Voice).ToList();

                    DiscordChannel canalErrado = canaisDaCategoria.Find(x => x.Name.ToUpper().Contains("SALA CRIADA!"));

                    string nomeCliqueAqui = "📌 Clique aqui!";

                    if (canaisDaCategoria.Contains(canalErrado))
                    {
                        foreach (DiscordChannel canal in canaisDaCategoria)
                        {
                            if (canal == canalErrado)
                            {
                                await canal.ModifyAsync(x => x.Name = nomeCliqueAqui);

                                return;
                            }
                        }
                    }

                    DiscordChannel cliqueAquiVoz = canaisDaCategoria.Find(x => x.Name == nomeCliqueAqui);

                    DiscordChannel batePapo = UBGE.GetChannel(Valores.ChatsUBGE.canalBatePapo);

                    if (cliqueAquiVoz == null)
                    {
                        DiscordChannel canalCliqueAqui_ = await UBGE.CreateChannelAsync(nomeCliqueAqui, ChannelType.Voice, categoriaOutrosCanais);

                        DiscordRole cargoMembroRegistrado = UBGE.GetRole(Valores.Cargos.cargoMembroRegistrado);
                        DiscordRole cargoAcessoGeral = UBGE.GetRole(Valores.Cargos.cargoAcessoGeral);

                        await canalCliqueAqui_.AddOverwriteAsync(cargoMembroRegistrado, Permissions.AccessChannels | Permissions.UseVoice, Permissions.Speak);
                        await canalCliqueAqui_.AddOverwriteAsync(cargoAcessoGeral, Permissions.AccessChannels, Permissions.UseVoice | Permissions.Speak);
                        await canalCliqueAqui_.AddOverwriteAsync(UBGE.EveryoneRole, Permissions.None, Permissions.AccessChannels | Permissions.UseVoice | Permissions.Speak);

                        string caminhoJson = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\ValoresConfig.json";

                        JObject json = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(caminhoJson));

                        json["canalCliqueAqui"] = canalCliqueAqui_.Id;

                        File.WriteAllBytes(caminhoJson, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(json, Formatting.Indented)));

                        Process.Start(Directory.GetCurrentDirectory() + @"\UBGE-Bot.exe");

                        Program.DesligarBot();
                    }

                    canaisDaCategoria.Remove(cliqueAquiVoz);
                    canaisDaCategoria.Remove(batePapo);

                    if (cliqueAquiVoz.Users.Count() != 0)
                    {
                        foreach (DiscordMember membro in cliqueAquiVoz.Users)
                            await membro.PlaceInAsync(batePapo);
                    }

                    if (canaisDaCategoria.Count() != 0)
                    {
                        DiscordOverwriteBuilder permissoes = new DiscordOverwriteBuilder
                        {
                            Allowed = Permissions.ManageChannels,
                            Denied = Permissions.None,
                        };

                        foreach (DiscordChannel canal in canaisDaCategoria)
                        {
                            List<DiscordOverwrite> permissoesDoCanal = canal.PermissionOverwrites.ToList();

                            if (canal.Users.Count() == 0 && permissoesDoCanal.Exists(x => x.Type == OverwriteType.Role && x.Allowed == permissoes.Allowed && x.Id == Valores.Cargos.cargoUBGEBot))
                                await canal.DeleteAsync();
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
            if (!botConectadoAoMongo_)
            {
                DiscordGuild UBGE = await ubgeBotClient.discordClient.GetGuildAsync(Valores.Guilds.UBGE);

                DiscordChannel canal = UBGE.GetChannel(Valores.ChatsUBGE.canalUBGEBot);

                if (!(await canal.GetMessagesAsync(1)).LastOrDefault().Content.Contains("Não foi possível conectar ao MongoDB!"))
                    await canal.SendMessageAsync("Não foi possível conectar ao MongoDB! Alguns comandos e sistemas podem estar indisponíveis. :cry:");

                await Program.ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.Aviso, "Não foi possível conectar ao MongoDB! Alguns comandos e sistemas podem estar indisponíveis.", ":cry:");
            }
        }
    }
}