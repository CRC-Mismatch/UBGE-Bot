using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Threading.Tasks;
using UBGE_Bot.Main;
using UBGE_Bot.Utilidades;

namespace UBGE_Bot.Sistemas.Discord
{
    public class BansUBGE : IAplicavelAoCliente
    {
        public void AplicarAoBot(DiscordClient discordClient, bool botConectadoAoMongo, bool sistemaAtivo)
        {
            discordClient.GuildBanAdded += NovoBan;
            discordClient.GuildBanRemoved += BanRetirado;
        }

        private async Task NovoBan(GuildBanAddEventArgs guildBanAddEventArgs)
        {
            if (guildBanAddEventArgs.Guild.Id != Valores.Guilds.UBGE)
                return;

            DiscordChannel logChat = guildBanAddEventArgs.Guild.GetChannel(Valores.ChatsUBGE.canalLog);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Color = Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed(),
                Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"O membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(guildBanAddEventArgs.Member)}#{guildBanAddEventArgs.Member.Discriminator}\" foi banido.", IconUrl = Valores.logoUBGE },
                Description = $"Dia e Hora: {DateTime.Now.ToString()}\n\n" +
                        $"ID do Membro: {guildBanAddEventArgs.Member.Id}",
                Timestamp = DateTime.Now,
                ThumbnailUrl = guildBanAddEventArgs.Member.AvatarUrl,
            };

            await logChat.SendMessageAsync(embed: embed.Build());
        }

        private async Task BanRetirado(GuildBanRemoveEventArgs guildBanRemoveEventArgs)
        {
            if (guildBanRemoveEventArgs.Guild.Id != Valores.Guilds.UBGE)
                return;

            DiscordChannel logChat = guildBanRemoveEventArgs.Guild.GetChannel(Valores.ChatsUBGE.canalLog);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Color = Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed(),
                Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"O membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(guildBanRemoveEventArgs.Member)}#{guildBanRemoveEventArgs.Member.Discriminator}\" foi desbanido.", IconUrl = Valores.logoUBGE },
                Description = $"Dia e Hora: {DateTime.Now.ToString()}\n\n" +
                    $"ID do Membro: {guildBanRemoveEventArgs.Member.Id}",
                Timestamp = DateTime.Now,
                ThumbnailUrl = guildBanRemoveEventArgs.Member.AvatarUrl,
            };

            await logChat.SendMessageAsync(embed: embed.Build());
        }
    }
}