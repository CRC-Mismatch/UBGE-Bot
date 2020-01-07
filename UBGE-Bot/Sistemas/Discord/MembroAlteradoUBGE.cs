using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UBGE_Bot.Main;
using UBGE_Bot.LogExceptions;
using UBGE_Bot.Utilidades;

namespace UBGE_Bot.Sistemas.Discord
{
    public sealed class MembroAlteradoUBGE : IAplicavelAoCliente
    {
        public void AplicarAoBot(DiscordClient discordClient, bool botConectadoAoMongo, bool sistemaAtivo)
            => discordClient.GuildMemberUpdated += MembroAlterado;

        private async Task MembroAlterado(GuildMemberUpdateEventArgs guildMemberUpdateEventArgs)
        {
            if (guildMemberUpdateEventArgs.Guild.Id != Valores.Guilds.UBGE)
                return;

            await Task.Delay(0);

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
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                }
            }).Start();
        }
    }
}