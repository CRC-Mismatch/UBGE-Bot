using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UBGE_Bot.Main;
using UBGE_Bot.MongoDB.Modelos;
using UBGE_Bot.LogExceptions;
using UBGE_Bot.Utilidades;

namespace UBGE_Bot.Sistemas.Discord
{
    public sealed class CanalDeVozUBGE : IAplicavelAoCliente
    {
        public void AplicarAoBot(DiscordClient discordClient, bool botConectadoAoMongo, bool sistemaAtivo)
        {
            if (botConectadoAoMongo && sistemaAtivo)
                discordClient.VoiceStateUpdated += CanalDeVozPersonalizado;
        }

        private async Task CanalDeVozPersonalizado(VoiceStateUpdateEventArgs voiceStateUpdateEventArgs)
        {
            if (voiceStateUpdateEventArgs.Guild.Id != Valores.Guilds.UBGE)
                return;

            try
            {
                IMongoDatabase local = Program.ubgeBot.localDB;
                IMongoCollection<Salas> salas = local.GetCollection<Salas>(Valores.Mongo.salas);

                DiscordGuild UBGE = voiceStateUpdateEventArgs.Guild;

                DiscordChannel cliqueAqui = UBGE.GetChannel(Valores.ChatsUBGE.canalCliqueAqui);

                if (cliqueAqui == null)
                    return;

                DiscordMember membro = null;

                if (voiceStateUpdateEventArgs.User == null)
                {
                    if (voiceStateUpdateEventArgs.Before?.User == null)
                        membro = await UBGE.GetMemberAsync(voiceStateUpdateEventArgs.After.User.Id);
                    else if (voiceStateUpdateEventArgs.After?.User == null)
                        membro = await UBGE.GetMemberAsync(voiceStateUpdateEventArgs.Before.User.Id);
                }
                else
                    membro = await UBGE.GetMemberAsync(voiceStateUpdateEventArgs.User.Id);

                FilterDefinition<Salas> filtroSalas = Builders<Salas>.Filter.Eq(s => s.idDoDono, membro.Id);
                List<Salas> resultadoSalas = await (await salas.FindAsync(filtroSalas)).ToListAsync();

                if (voiceStateUpdateEventArgs.Before?.Channel != null && resultadoSalas.Count != 0 &&
                voiceStateUpdateEventArgs.Before?.Channel?.Id == resultadoSalas.LastOrDefault().idDaSala &&
                voiceStateUpdateEventArgs.After?.Channel != null &&
                voiceStateUpdateEventArgs.After?.Channel?.Id == cliqueAqui.Id)
                {
                    await membro.PlaceInAsync(UBGE.GetChannel(resultadoSalas.LastOrDefault().idDaSala));

                    return;
                }

                if (voiceStateUpdateEventArgs.Before?.Channel != null)
                {
                    if (voiceStateUpdateEventArgs.Before.Channel == voiceStateUpdateEventArgs.After?.Channel)
                        return;

                    FilterDefinition<Salas> filtroSala = Builders<Salas>.Filter.Eq(s => s.idDaSala, voiceStateUpdateEventArgs.Before.Channel.Id);
                    List<Salas> respostaSala = await (await salas.FindAsync(filtroSala)).ToListAsync();

                    if (respostaSala.Count != 0 && voiceStateUpdateEventArgs.Before.Channel.Id == respostaSala.LastOrDefault().idDaSala)
                    {
                        await salas.UpdateOneAsync(filtroSala, Builders<Salas>.Update.Set(x => x.membrosNaSala, respostaSala.LastOrDefault().membrosNaSala - 1));

                        List<Salas> respostaSalaAtualizada = await (await salas.FindAsync(filtroSala)).ToListAsync();

                        if (respostaSala.Count != 0 && respostaSalaAtualizada.LastOrDefault().membrosNaSala == 0)
                            await UBGE.GetChannel(respostaSalaAtualizada.LastOrDefault().idDaSala).DeleteAsync();

                        return;
                    }
                    else if (respostaSala.Count == 0 && voiceStateUpdateEventArgs.Before.Channel.Parent == cliqueAqui.Parent && voiceStateUpdateEventArgs.Before.Channel.Id != cliqueAqui.Id)
                    {
                        await UBGE.GetChannel(voiceStateUpdateEventArgs.Before.Channel.Id).DeleteAsync();

                        return;
                    }
                }

                if (voiceStateUpdateEventArgs.Before?.Channel != null && voiceStateUpdateEventArgs.Before?.Channel?.Id != cliqueAqui.Id && voiceStateUpdateEventArgs.Before?.Channel?.Parent == cliqueAqui.Parent)
                {
                    List<Salas> resultadoSalas_ = await (await salas.FindAsync(filtroSalas)).ToListAsync();

                    if (resultadoSalas_.Count != 0)
                    {
                        if (resultadoSalas_.LastOrDefault().membrosNaSala == 0 || UBGE.GetChannel(resultadoSalas_.LastOrDefault().idDaSala).Users.Count() == 0)
                        {
                            await UBGE.GetChannel(resultadoSalas_.LastOrDefault().idDaSala).DeleteAsync();

                            if (UBGE.GetChannel(resultadoSalas_.LastOrDefault().idDaSala).Users.Count() == 0)
                                await salas.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(x => x.membrosNaSala, ulong.Parse("0")));
                        }
                    }
                    else if (resultadoSalas_.Count == 0)
                    {
                        FilterDefinition<Salas> filtroSalaCanalId = Builders<Salas>.Filter.Eq(s => s.idDaSala, voiceStateUpdateEventArgs.Before.Channel.Id);
                        List<Salas> resultadoSalaCanalId = await (await salas.FindAsync(filtroSalaCanalId)).ToListAsync();

                        if (resultadoSalaCanalId.Count != 0)
                        {
                            if (resultadoSalaCanalId.LastOrDefault().membrosNaSala == 0 || UBGE.GetChannel(resultadoSalaCanalId.LastOrDefault().idDaSala).Users.Count() == 0)
                            {
                                await UBGE.GetChannel(resultadoSalaCanalId.LastOrDefault().idDaSala).DeleteAsync();

                                if (UBGE.GetChannel(resultadoSalaCanalId.LastOrDefault().idDaSala).Users.Count() == 0)
                                    await salas.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(x => x.membrosNaSala, ulong.Parse("0")));
                            }
                        }
                        else if (resultadoSalaCanalId.Count == 0)
                        {
                            if (voiceStateUpdateEventArgs.Before.Channel.Id != cliqueAqui.Id)
                                await UBGE.GetChannel(voiceStateUpdateEventArgs.Before.Channel.Id).DeleteAsync();
                        }
                    }
                }

                DiscordRole membroRegistradoCargo = UBGE.GetRole(Valores.Cargos.cargoMembroRegistrado);

                DiscordDmChannel pvMembro = await membro.CreateDmChannelAsync();

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

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

                        DiscordChannel canalDoMembro = null;

                        if (membro.Presence?.Activity?.ActivityType != ActivityType.Custom)
                            canalDoMembro = await UBGE.CreateChannelAsync(!string.IsNullOrWhiteSpace(membro.Presence?.Activity?.Name) ? membro.Presence.Activity.Name : $"Sala do: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}", ChannelType.Voice, cliqueAqui.Parent);
                        else
                            canalDoMembro = await UBGE.CreateChannelAsync($"Sala do: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}", ChannelType.Voice, cliqueAqui.Parent);

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
                                await canalDoMembro.AddOverwriteAsync(UBGE.EveryoneRole, Permissions.AccessChannels, Permissions.UseVoice | Permissions.Speak | Permissions.UseVoiceDetection);
                                await canalDoMembro.AddOverwriteAsync(membro, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice | Permissions.UseVoiceDetection);
                                await canalDoMembro.AddOverwriteAsync(botsMusicaisCargo, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice | Permissions.UseVoiceDetection);
                                await canalDoMembro.AddOverwriteAsync(acessoGeralCargo, Permissions.AccessChannels, Permissions.UseVoice | Permissions.Speak | Permissions.UseVoiceDetection);
                                await canalDoMembro.AddOverwriteAsync(moderadorDiscordCargo, Permissions.AccessChannels, Permissions.UseVoice | Permissions.Speak | Permissions.UseVoiceDetection);
                                await canalDoMembro.AddOverwriteAsync(membroRegistradoCargo, Permissions.AccessChannels, Permissions.UseVoice | Permissions.Speak | Permissions.UseVoiceDetection);

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
                            DiscordChannel Comandos_Bot = UBGE.GetChannel(Valores.ChatsUBGE.canalComandosBot);
                            DiscordChannel BatePapo = UBGE.GetChannel(Valores.ChatsUBGE.canalBatePapo);

                            await BatePapo.PlaceMemberAsync(membro);

                            embed.WithAuthor("Você precisa ter o cargo de membro registrado para criar salas de voz!", null, Valores.logoUBGE)
                                .WithDescription("Para isso, digite o comando `!membro` para fazer o censo comunitário e ter acesso à salas privadas!");

                            await Comandos_Bot.SendMessageAsync(embed: embed.Build(), content: membro.Mention);
                            await pvMembro.SendMessageAsync(embed: embed.Build(), content: membro.Mention);
                        }
                        catch (UnauthorizedException)
                        {
                            Program.ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, "Não foi possível enviar a mensagem de pedido para fazer o censo no privado do membro.");

                            await Program.ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.Aviso, "Erro!", "Não foi possível enviar a mensagem de pedido para fazer o censo no privado do membro.");
                        }
                        catch (Exception exception)
                        {
                            await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                        }
                    }
                }
                else if (voiceStateUpdateEventArgs.After?.Channel != null)
                {
                    FilterDefinition<Salas> filtro = Builders<Salas>.Filter.Eq(s => s.idDaSala, voiceStateUpdateEventArgs.After.Channel.Id);

                    List<Salas> respostaSalas = (await (await salas.FindAsync(filtro)).ToListAsync());

                    if (respostaSalas.Count != 0 && voiceStateUpdateEventArgs.After?.Channel?.Id == respostaSalas.LastOrDefault().idDaSala)
                    {
                        if (voiceStateUpdateEventArgs.Before == null || voiceStateUpdateEventArgs.Before?.Channel?.Id != cliqueAqui.Id)
                        {
                            FilterDefinition<Salas> filtroSala = Builders<Salas>.Filter.Eq(s => s.idDaSala, voiceStateUpdateEventArgs.After.Channel.Id);
                            List<Salas> respostaSala = await (await salas.FindAsync(filtroSala)).ToListAsync();

                            await salas.UpdateOneAsync(filtroSala, Builders<Salas>.Update.Set(x => x.membrosNaSala, respostaSala.LastOrDefault().membrosNaSala + 1));
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
            }
        }
    }
}