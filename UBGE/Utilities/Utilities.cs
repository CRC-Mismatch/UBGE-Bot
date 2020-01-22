using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UBGE.Utilities
{
    public sealed class Utilities
    {
        public TimeSpan ConvertTime(string convertTime)
        {
            if (convertTime.ToLower().Contains('s'))
                return TimeSpan.FromSeconds(Convert.ToInt32(convertTime.Split('s').FirstOrDefault()));
            else if (convertTime.ToLower().Contains('m'))
                return TimeSpan.FromMinutes(Convert.ToInt32(convertTime.Split('m').FirstOrDefault()));
            else if (convertTime.ToLower().Contains('h'))
                return TimeSpan.FromHours(Convert.ToInt32(convertTime.Split('h').FirstOrDefault()));
            else if (convertTime.ToLower().Contains('d'))
                return TimeSpan.FromDays(Convert.ToInt32(convertTime.Split('d').FirstOrDefault()));

            return TimeSpan.FromSeconds(0);
        }

        public DiscordColor RandomColorEmbed()
        {
            var _r = new Random(DateTime.Now.Ticks.GetHashCode());

            int r = _r.Next(0, 255);
            int g = _r.Next(0, 255);
            int b = _r.Next(0, 255);

            string rHex = r.ToString("X");
            string gHex = g.ToString("X");
            string bHex = b.ToString("X");

            return new DiscordColor(Convert.ToInt32(rHex + gHex + bHex, 16));
        }

        public async Task<string> GetAnswer(InteractivityExtension interactivity, CommandContext commandContext)
            => (await interactivity.WaitForMessageAsync(m => m.Author == commandContext.User && m.Channel.Id == commandContext.Channel.Id, TimeSpan.FromMinutes(30))).Result?.Content;

        public async Task<string> GetAnswerDM(InteractivityExtension interactivity, CommandContext commandContext)
        {
            DiscordChannel dm = await commandContext.Member.CreateDmChannelAsync();

            return (await interactivity.WaitForMessageAsync(m => m.Author == commandContext.User && m.Channel == dm, TimeSpan.FromMinutes(30))).Result?.Content;
        }

        public async Task<string> GetAnswerDM(InteractivityExtension interactivity, DiscordUser member, DiscordChannel channel)
            => (await interactivity.WaitForMessageAsync(m => m.Author == member && m.Channel == channel, TimeSpan.FromMinutes(30))).Result?.Content;

        public DiscordEmoji FindEmoji(CommandContext commandContext, string emojiName)
        {
            DiscordEmoji de = null;

            if (emojiName.Contains("<") && emojiName.Contains(":") && emojiName.Contains(">"))
                return commandContext.Guild.Emojis.Values.ToList().Find(x => x.ToString().ToLower().Contains(emojiName.ToLower()));
            else if (emojiName.Contains(":"))
            {
                emojiName = emojiName.Replace(":", "");

                return commandContext.Guild.Emojis.Values.ToList().Find(x => x.Name.ToLower() == emojiName.ToLower());
            }

            foreach (var emoji in commandContext.Guild.Emojis.Values)
            {
                if (emoji.Name.ToLower().Contains(emojiName.ToLower()))
                {
                    de = emoji;

                    break;
                }
            }

            return de ?? FindEmoji(commandContext.Client, emojiName);
        }

        public DiscordEmoji FindEmoji(DiscordClient discordClient, string emojiName)
        {
            DiscordEmoji de = null;

            if (emojiName.Contains(":"))
                emojiName = emojiName.Replace(":", "");

            foreach (var servidor in discordClient.Guilds.Values)
            {
                de = servidor.Emojis.Values.ToList().Find(x => x.Name.ToLower() == emojiName.ToLower());

                if (de != null)
                    return de;
            }

            return de;
        }

        public DiscordEmbedBuilder ClearEmbed(DiscordEmbedBuilder embed)
        {
            embed.ClearFields();
            embed.Description = string.Empty;
            embed.Footer = new DiscordEmbedBuilder.EmbedFooter { Text = string.Empty, IconUrl = string.Empty };
            embed.Timestamp = null;
            embed.ThumbnailUrl = string.Empty;
            embed.Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = string.Empty, Name = string.Empty, Url = string.Empty };
            embed.Color = new DiscordColor();
            embed.ImageUrl = string.Empty;
            embed.Title = string.Empty;
            embed.Url = string.Empty;

            return embed;
        }

        public string MemberMention(DiscordMember member)
            => $"{(!string.IsNullOrWhiteSpace(member.Nickname) ? $"<@!{member.Id}>" : $"<@{member.Id}>")}";

        public DiscordEmoji StatusToEmoji(CommandContext commandContext, DiscordMember member)
        {
            if (member.Presence == null)
                return FindEmoji(commandContext, "status_offline");
            else if (member.Presence.Status == UserStatus.Online)
                return FindEmoji(commandContext, "status_online");
            else if (member.Presence.Status == UserStatus.DoNotDisturb)
                return FindEmoji(commandContext, "status_ocupado");
            else if (member.Presence.Status == UserStatus.Offline)
                return FindEmoji(commandContext, "status_offline");
            else if (member.Presence.Status == UserStatus.Invisible)
                return FindEmoji(commandContext, "status_offline");
            else if (member.Presence.Status == UserStatus.Idle)
                return FindEmoji(commandContext, "status_ausente");

            return null;
        }

        public string PunishmentTime(string text)
        {
            if (text.Contains("s"))
                return "s";
            else if (text.Contains("m"))
                return "m";
            else if (text.Contains("h"))
                return "h";
            else if (text.Contains("d"))
                return "d";

            return null;
        }

        public string StatusToName(DiscordMember member)
        {
            if (member.Presence == null)
                return "Offline";
            else if (member.Presence.Status == UserStatus.Online)
                return "Online";
            else if (member.Presence.Status == UserStatus.DoNotDisturb)
                return "Não pertube";
            else if (member.Presence.Status == UserStatus.Offline)
                return "Offline";
            else if (member.Presence.Status == UserStatus.Invisible)
                return "Offline";
            else if (member.Presence.Status == UserStatus.Idle)
                return "Ausente";

            return null;
        }

        public string ByteToString(byte[] byteToString)
            => Encoding.UTF8.GetString(byteToString, 0, byteToString.Length);

        public string DiscordNick(DiscordMember member)
            => $"{(string.IsNullOrWhiteSpace(member.Nickname) ? member.Username : member.Nickname)}";

        public List<DiscordEmoji> ReturnEmojis(CommandContext commandContext, List<string> emojisToSearch)
        {
            var emojis = new List<DiscordEmoji>();

            foreach (string emojiName in emojisToSearch)
                emojis.Add(FindEmoji(commandContext, emojiName));

            return emojis;
        }

        public List<DiscordEmoji> ReturnEmojis(DiscordClient discordClient, List<string> emojisToSearch)
        {
            var emojis = new List<DiscordEmoji>();

            foreach (string emojiName in emojisToSearch)
                emojis.Add(FindEmoji(discordClient, emojiName));

            return emojis;
        }

        public string ReturnState(string stateAbbreviation)
        {
            stateAbbreviation = stateAbbreviation.ToUpper();

            if (stateAbbreviation == "AM")
                return "Amazonas";
            else if (stateAbbreviation == "RR")
                return "Roraima";
            else if (stateAbbreviation == "AP")
                return "Amapá";
            else if (stateAbbreviation == "PA")
                return "Pará";
            else if (stateAbbreviation == "TO")
                return "Tocantins";
            else if (stateAbbreviation == "RO")
                return "Rondônia";
            else if (stateAbbreviation == "AC")
                return "Acre";
            else if (stateAbbreviation == "MA")
                return "Maranhão";
            else if (stateAbbreviation == "PI")
                return "Piauí";
            else if (stateAbbreviation == "CE")
                return "Ceará";
            else if (stateAbbreviation == "RN")
                return "Rio Grande do Norte";
            else if (stateAbbreviation == "PE")
                return "Pernambuco";
            else if (stateAbbreviation == "PB")
                return "Paraíba";
            else if (stateAbbreviation == "SE")
                return "Sergipe";
            else if (stateAbbreviation == "AL")
                return "Alagoas";
            else if (stateAbbreviation == "BA")
                return "Bahia";
            else if (stateAbbreviation == "MT")
                return "Mato Grosso";
            else if (stateAbbreviation == "MS")
                return "Mato Grosso do Sul";
            else if (stateAbbreviation == "GO")
                return "Goiás";
            else if (stateAbbreviation == "DF")
                return "Distrito Federal";
            else if (stateAbbreviation == "SP")
                return "São Paulo";
            else if (stateAbbreviation == "RJ")
                return "Rio de Janeiro";
            else if (stateAbbreviation == "ES")
                return "Espírito Santo";
            else if (stateAbbreviation == "MG")
                return "Minas Gerais";
            else if (stateAbbreviation == "PR")
                return "Paraná";
            else if (stateAbbreviation == "RS")
                return "Rio Grande do Sul";
            else if (stateAbbreviation == "SC")
                return "Santa Catarina";
            else
                return "Não especificado.";
        }

        public async Task ExcludeReactionFromAMemberList(DiscordMessage messageEmbedReact, DiscordEmoji emoji, IReadOnlyList<DiscordUser> memberReact)
        {
            foreach (var member in memberReact)
            {
                try
                {
                    await messageEmbedReact.DeleteReactionAsync(emoji, member);
                }
                catch (Exception) { }
            }
        }

        public DiscordColor HelpCommandsColor()
            => new DiscordColor(54, 57, 64);
    }

    public sealed class UBGEAndOthersAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
            => Task.FromResult(ctx.Guild.Id == Values.Guilds.guildUBGE || ctx.Guild.Id == Values.Guilds.guildTestesDoLuiz || ctx.Guild.Id == Values.Guilds.guildCBPR
                || ctx.Guild.Id == Values.Guilds.guildEmojos || ctx.Guild.Id == Values.Guilds.guildRuinasDeAstapor || ctx.Guild.Id == Values.Guilds.guildEmoji);
    }

    public sealed class OnlyUBGEAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) => Task.FromResult(ctx.Guild.Id == Values.Guilds.guildUBGE);
    }

    public sealed class UBGEStaffAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) => Task.FromResult(ctx.Guild.Id == Values.Guilds.guildUBGE && ctx.Member.Roles.ToList().FindAll(x => x.Permissions.HasFlag(Permissions.KickMembers)).Count != 0 || Debugger.IsAttached || ctx.Member.Id == Values.Guilds.Members.memberUBGEBot);
    }

    public sealed class RuinasDeAstaporAndUBGEAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) => Task.FromResult(ctx.Guild.Id == Values.Guilds.guildUBGE || ctx.Guild.Id == Values.Guilds.guildRuinasDeAstapor);
    }

    public sealed class OnlyAlbionAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) => Task.FromResult(ctx.Guild.Id == Values.Guilds.guildUBGEAlbion);
    }

    public sealed class CreateYourRoomHereAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) => Task.FromResult(ctx.Guild.Id == Values.Guilds.guildUBGE && ctx.Channel.Id == Values.Chats.channelCrieSuaSalaAqui);
    }

    public sealed class ConnectedToMongo : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (!Program.Bot.ConnectedToMongo)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { Name = "Erro!", IconUrl = Values.logoUBGE },
                    Description = "Não foi possível executar este comando pois o bot não está conectado ao Mongo! :cry:",
                    ThumbnailUrl = ctx.Member.AvatarUrl,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}" },
                    Color = Program.Bot.Utilities.RandomColorEmbed(),
                };

                ctx.RespondAsync(embed: embed.Build());

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }

    public sealed class CommitteeAndCouncil : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) => Task.FromResult(ctx.Member.Roles.Any(x => x.Id == Values.Roles.roleComiteComunitario) || ctx.Member.Roles.Any(x => x.Id == Values.Roles.roleConselheiro));
    }

    public sealed class OnlyCommittee : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            foreach (var role in ctx.Member.Roles)
            {
                if (role.Id != Values.Roles.roleComiteComunitario)
                    continue;

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}