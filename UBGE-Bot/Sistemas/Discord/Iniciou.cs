using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Threading.Tasks;
using UBGE_Bot.Utilidades;

namespace UBGE_Bot.Sistemas.Discord
{
    public sealed class Iniciou : IAplicavelAoCliente
    {
        public void AplicarAoBot(DiscordClient discordClient, bool botConectadoAoMongo, bool sistemaAtivo)
            => discordClient.Ready += DiscordIniciado;

        private async Task DiscordIniciado(ReadyEventArgs readyEventArgs)
            => await readyEventArgs.Client.UpdateStatusAsync(new DiscordActivity { Name = "Bem-Vindo a UBGE!", ActivityType = ActivityType.Playing });
    }
}