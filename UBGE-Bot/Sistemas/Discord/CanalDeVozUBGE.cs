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
        public void AplicarAoBot(DiscordClient discordClient, bool botConectadoAoMongo)
        {
            if (botConectadoAoMongo)
                discordClient.VoiceStateUpdated += CanalDeVozPersonalizado;
        }

        private async Task CanalDeVozPersonalizado(VoiceStateUpdateEventArgs voiceStateUpdateEventArgs)
        {
            if (voiceStateUpdateEventArgs.Guild.Id != Valores.Guilds.UBGE)
                return;

            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    var local = Program.ubgeBot.localDB;
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

                            DiscordChannel canalDoMembro = await UBGE.CreateChannelAsync(!string.IsNullOrWhiteSpace(membro.Presence?.Activity?.Name) ? membro.Presence.Activity.Name : $"Sala do: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}", ChannelType.Voice, cliqueAqui.Parent);

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
                                Program.ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, "Não foi possível enviar a mensagem de pedido para fazer o censo no privado do membro.");
                            }
                            catch (Exception) { }
                        }
                    }
                }
                catch (NullReferenceException) { }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                }
            }).Start();
        }
    }
}