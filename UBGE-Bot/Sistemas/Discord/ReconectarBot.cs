using DSharpPlus;
using DSharpPlus.EventArgs;
using System;
using System.Threading.Tasks;
using UBGE_Bot.Main;
using UBGE_Bot.LogExceptions;
using UBGE_Bot.Utilidades;

namespace UBGE_Bot.Sistemas.Discord
{
    public class ReconectarBot : IAplicavelAoCliente
    {
        public void AplicarAoBot(DiscordClient discordClient, bool botConectadoAoMongo)
        {
            discordClient.SocketClosed += BotCaiu;
            discordClient.SocketErrored += BotCaiuEErroNoSocket;
        }

        private async Task BotCaiu(SocketCloseEventArgs socketCloseEventArgs)
        {
            try
            {
                Program.ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, $"A conexão com o Discord foi encerrada! Reconectando...");

                await Program.ubgeBot.discordClient.ReconnectAsync(false);

                Program.ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, $"Reconectado!");
            }
            catch (Exception exception)
            {
                Program.ubgeBot.logExceptionsToDiscord.ExceptionToTxt(exception);
                Program.ShutdownBot();
            }
        }

        private async Task BotCaiuEErroNoSocket(SocketErrorEventArgs socketErrorEventArgs)
        {
            try
            {
                Program.ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, $"A conexão com o Discord foi encerrada! Reconectando...");

                await Program.ubgeBot.discordClient.ReconnectAsync(false);

                Program.ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, $"Reconectado!");
            }
            catch (Exception exception)
            {
                Program.ubgeBot.logExceptionsToDiscord.ExceptionToTxt(exception);
                Program.ShutdownBot();
            }
        }
    }
}