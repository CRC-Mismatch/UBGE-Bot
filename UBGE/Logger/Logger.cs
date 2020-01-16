using DSharpPlus.Entities;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UBGE.Utilities;

namespace UBGE.Logger
{
    public sealed class Logger
    {
        public enum TypeWarning { Commands, Discord, Servers, Systems, Lavalink, Mongo, MySQL, PC, Logger, ReactRole, SAC }
        public enum TypeLog { Commands, Discord, Servers, Systems, Lavalink, Mongo, MySQL, PC, Logger, ReactRole, SAC }
        public enum TypeError { Commands, Discord, Servers, Systems, Lavalink, Mongo, MySQL, PC, Logger, ReactRole, SAC }
        public enum TypeEmbed { ReactRole, Warning, SAC, Systems }

        private readonly string PREFIX_BOT_CONSOLE = "[UBGE-Bot]";

        public void ExceptionTxt(Exception exception)
        {
            using (var sw = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + $@"\Exception-{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}_{DateTime.Now.Hour}-{DateTime.Now.Minute}.txt", false, Encoding.UTF8))
                sw.WriteLine(exception.ToString());
        }

        public void Warning(TypeWarning type, string message, string prefixConfig = null)
        {
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine($"{this.AtualDate()} {this.PREFIX_BOT_CONSOLE}{(string.IsNullOrWhiteSpace(prefixConfig) ? string.Empty : $" [{prefixConfig}]")} [Aviso] [{type}] {message}");
            Console.ResetColor();
        }

        public void Log(TypeLog type, string message, string prefixConfig = null)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{this.AtualDate()} {this.PREFIX_BOT_CONSOLE}{(string.IsNullOrWhiteSpace(prefixConfig) ? string.Empty : $" [{prefixConfig}]")} [Log] [{type}] {message}");
            Console.ResetColor();
        }

        public void Error(TypeError type, string message, string prefixConfig = null)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{this.AtualDate()} {this.PREFIX_BOT_CONSOLE}{(string.IsNullOrWhiteSpace(prefixConfig) ? string.Empty : $" [{prefixConfig}]")} [Erro] [{type}] {message}");
            Console.ResetColor();
        }

        public async Task Error(TypeError type, Exception exception)
        {
            try
            {
                if (exception.StackTrace.Length > 2000 || exception.Message.Length > 250)
                {
                    this.Error(type, exception.ToString());

                    return;
                }

                var ubgeServer = await Program.Bot.DiscordClient.GetGuildAsync(Values.Guilds.guildUBGE);
                var luiz = await ubgeServer.GetMemberAsync(Values.Guilds.Members.memberLuiz);
                var logUBGEBot = ubgeServer.GetChannel(Values.Chats.channelLog);

                var embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { Name = exception.Message, IconUrl = Values.logoUBGE },
                    Color = DiscordColor.Red,
                    Description = exception.StackTrace,
                    Timestamp = DateTime.Now,
                };

                this.Error(type, exception.ToString());

                await logUBGEBot.SendMessageAsync(embed: embed.Build(), content: luiz.Mention);
            }
            catch (Exception exception_) 
            {
                this.Error(TypeError.Logger, $"Erro no logger: {exception_.ToString()}");
            }
        }

        public async Task EmbedLogMessages(TypeEmbed type, string embedAuthor = null, string embedDescription = null, string embedAuthorPhoto = null, DiscordUser member = null)
        {
            try
            {
                var ubgeServer = await Program.Bot.DiscordClient.GetGuildAsync(Values.Guilds.guildUBGE);
                var ubgeBot = await ubgeServer.GetMemberAsync(Values.Guilds.Members.memberUBGEBot);
                var canalLog = ubgeServer.GetChannel(Values.Chats.channelLog);

                var embed = new DiscordEmbedBuilder();

                if (type == TypeEmbed.Warning)
                {
                    embed.WithColor(DiscordColor.Yellow)
                        .WithAuthor(embedAuthor, null, ubgeBot.AvatarUrl)
                        .WithDescription(embedDescription)
                        .WithTimestamp(DateTime.Now);

                    await canalLog.SendMessageAsync(embed: embed.Build());
                }
                else if (type == TypeEmbed.ReactRole)
                {
                    embed.WithColor(DiscordColor.Green)
                        .WithAuthor(embedAuthor, null, ubgeBot.AvatarUrl)
                        .WithDescription(embedDescription)
                        .WithThumbnailUrl(member.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    await canalLog.SendMessageAsync(embed: embed.Build());
                }
                else if (type == TypeEmbed.SAC)
                {
                    embed.WithColor(DiscordColor.MidnightBlue)
                        .WithAuthor(embedAuthor, null, embedAuthorPhoto)
                        .WithDescription(embedDescription)
                        .WithThumbnailUrl(member.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    await canalLog.SendMessageAsync(embed: embed.Build());
                }
            }
            catch (Exception exception) 
            {
                this.Error(TypeError.Logger, $"Erro no logger: {exception.ToString()}");
            }
        }

        public string AtualDate()
        {
            try
            {
                return $"[{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year} {DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second} {DateTime.Now.ToString().Split(' ')[2]} {TimeZoneInfo.Local.DisplayName.Split(' ')[0].Replace("(", "").Replace(")", "").Replace("UTC", "")}]";
            }
            catch (Exception)
            {
                return $"[{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year} {DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}]";
            }
        }
    }
}