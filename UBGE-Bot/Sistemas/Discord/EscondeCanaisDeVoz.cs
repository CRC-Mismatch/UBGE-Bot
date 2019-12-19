using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UBGE_Bot.Main;
using UBGE_Bot.LogExceptions;
using UBGE_Bot.Utilidades;

namespace UBGE_Bot.Sistemas.Discord
{
    public sealed class EscondeCanaisDeVoz : IAplicavelAoCliente
    {
        public void AplicarAoBot(DiscordClient discordClient, bool botConectadoAoMongo)
        {
            //discordClient.VoiceStateUpdated += EscondeCanaisDeVozTask;
        }

        private async Task EscondeCanaisDeVozTask(VoiceStateUpdateEventArgs voiceStateUpdateEventArgs)
        {
            if (voiceStateUpdateEventArgs.Guild.Id != Valores.Guilds.UBGE)
                return;

            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    DiscordGuild UBGE = await voiceStateUpdateEventArgs.Client.GetGuildAsync(Valores.Guilds.UBGE);

                    DiscordMember ubgeBot = await UBGE.GetMemberAsync(Valores.Guilds.Membros.ubgeBot);

                    DiscordRole everyoneUBGE = UBGE.EveryoneRole;
                    DiscordRole acessoGeral = UBGE.GetRole(Valores.Cargos.cargoAcessoGeral);

                    DiscordChannel canalAntes = voiceStateUpdateEventArgs.Before?.Channel;
                    DiscordChannel canalDepois = voiceStateUpdateEventArgs.After?.Channel;

                    var canaisDeVozDaUBGE = UBGE.Channels.Values.Where(x => x.Type == ChannelType.Voice && x.Parent.Id != Valores.ChatsUBGE.Categorias.categoriaUBGE && x.Parent.Id != Valores.ChatsUBGE.Categorias.categoriaCliqueAqui && x.Parent.Id != Valores.ChatsUBGE.Categorias.categoriaConselhoComunitario);

                    DiscordOverwriteBuilder permissao = new DiscordOverwriteBuilder
                    {
                        Allowed = Permissions.None,
                        Denied = Permissions.AccessChannels | Permissions.UseVoice,
                    };

                    foreach (var canal in canaisDeVozDaUBGE)
                    {
                        var permissoesDoCanal = canal.PermissionOverwrites.ToList();

                        if (canal.Users.Count() == 0 && !permissoesDoCanal.Exists(x => x.Type == OverwriteType.Role && x.Id == everyoneUBGE.Id && x.Denied == permissao.Denied || x.Type == OverwriteType.Role && x.Id == acessoGeral.Id && x.Denied == permissao.Denied))
                        {
                            await canal.AddOverwriteAsync(everyoneUBGE, Permissions.None, Permissions.AccessChannels | Permissions.UseVoice);
                            await canal.AddOverwriteAsync(acessoGeral, Permissions.None, Permissions.AccessChannels | Permissions.UseVoice);
                        }
                        else if (canal.Users.Count() != 0 && permissoesDoCanal.Exists(x => x.Type == OverwriteType.Role && x.Id == everyoneUBGE.Id && x.Denied == permissao.Denied || x.Type == OverwriteType.Role && x.Id == acessoGeral.Id && x.Denied == permissao.Denied))
                        {
                            await canal.AddOverwriteAsync(everyoneUBGE, Permissions.AccessChannels, Permissions.UseVoice);
                            await canal.AddOverwriteAsync(acessoGeral, Permissions.AccessChannels, Permissions.UseVoice);
                        }
                        else
                            continue;
                    }
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                }
            }).Start();
        }
    }
}