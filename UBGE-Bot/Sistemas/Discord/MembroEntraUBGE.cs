using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using UBGE_Bot.Main;
using UBGE_Bot.LogExceptions;
using UBGE_Bot.Utilidades;

namespace UBGE_Bot.Sistemas.Discord
{
    public sealed class MembroEntraUBGE : IAplicavelAoCliente
    {
        public void AplicarAoBot(DiscordClient discordClient, bool botConectadoAoMongo, bool sistemaAtivo)
            => discordClient.GuildMemberAdded += MembroEntra;

        private async Task MembroEntra(GuildMemberAddEventArgs guildMemberAddEventArgs)
        {
            if (guildMemberAddEventArgs.Guild.Id != Valores.Guilds.UBGE)
                return;

            await Task.Delay(0);

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
                catch (UnauthorizedException)
                {
                    Program.ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, "Não foi possível enviar a mensagem de pedido para fazer o censo no privado do membro.");

                    await Program.ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.Aviso, "Erro!", "Não foi possível enviar a mensagem de pedido para fazer o censo no privado do membro.");
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                }
            }).Start();
        }
    }
}