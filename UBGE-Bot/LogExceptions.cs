using DSharpPlus.Entities;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UBGE_Bot.Utilidades;
using UBGE_Bot.Main;

namespace UBGE_Bot.LogExceptions
{
    public sealed class LogExceptionsToDiscord
    {
        public enum TipoAviso { Comandos, Discord, Servidores, Sistemas, Lavalink, Mongo }
        public enum TipoLog { Comandos, Discord, Servidores, Sistemas, Lavalink, Mongo }
        public enum TipoErro { Comandos, Discord, Servidores, Sistemas, Lavalink, Mongo }
        public enum TipoEmbed { ReactRoleUBGE, Aviso, SAC, ReactRoleForaDaUBGE }

        public void ExceptionToTxt(Exception exception)
        {
            using (StreamWriter streamWriter = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + $@"\Exceção-{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}_{DateTime.Now.Hour}-{DateTime.Now.Minute}.txt", false, Encoding.UTF8))
                streamWriter.WriteLine(exception.ToString());
        }

        public void Aviso(TipoAviso tipo, string mensagem)
        {
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{RetornaDataAtualParecidoComODSharpPlus()} {Valores.prefixoBot} [Aviso] [{tipo}] {mensagem}");
            Console.ResetColor();
        }

        public void Log(TipoLog tipo, string mensagem)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{RetornaDataAtualParecidoComODSharpPlus()} {Valores.prefixoBot} [Log] [{tipo}] {mensagem}");
            Console.ResetColor();
        }

        public void Error(TipoErro tipo, string mensagem)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{RetornaDataAtualParecidoComODSharpPlus()} {Valores.prefixoBot} [Erro] [{tipo}] {mensagem}");
            Console.ResetColor();
        }

        private void Error_(TipoErro tipo, Exception exception)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{RetornaDataAtualParecidoComODSharpPlus()} {Valores.prefixoBot} [Erro] [{tipo}] Exceção: {exception}");
            Console.ResetColor();
        }

        public async Task Error(TipoErro tipo, Exception exception)
        {
            try
            {
                if (exception.StackTrace.Length > 2000 || exception.Message.Length > 250)
                {
                    Error_(tipo, exception);

                    return;
                }

                DiscordGuild guildUBGE = await Program.ubgeBot.discordClient.GetGuildAsync(Valores.Guilds.UBGE);
                DiscordChannel logUBGEBot = guildUBGE.GetChannel(guildUBGE.Channels.Values.ToList().Find(x => x.Name.ToUpper().Contains(Valores.ChatsUBGE.canalLog)).Id);
                DiscordMember Luiz = await guildUBGE.GetMemberAsync(Valores.Guilds.Membros.luiz);

                DiscordEmbedBuilder Embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { Name = exception.Message, IconUrl = Valores.logoUBGE },
                    Color = DiscordColor.Red,
                    Description = exception.StackTrace,
                    Timestamp = DateTime.Now,
                };

                Error_(tipo, exception);
                
                await logUBGEBot.SendMessageAsync(embed: Embed.Build(), content: Luiz.Mention);
            }
            catch (Exception) { }
        }

        public async Task EmbedLogMessages(TipoEmbed tipo, string embedAuthor = null, string embedDescription = null, string fotoAuthorEmbed = null, DiscordUser membro = null)
        {
            try
            {
                DiscordGuild UBGE = await Program.ubgeBot.discordClient.GetGuildAsync(Valores.Guilds.UBGE);
                DiscordMember ubgeBot = await UBGE.GetMemberAsync(Valores.Guilds.Membros.ubgeBot);
                DiscordChannel canalLog = UBGE.GetChannel(UBGE.Channels.Values.ToList().Find(x => x.Name.ToUpper().Contains(Valores.ChatsUBGE.canalLog)).Id);

                DiscordEmbedBuilder Embed = new DiscordEmbedBuilder();

                if (tipo == TipoEmbed.Aviso)
                {
                    Embed.WithColor(DiscordColor.Yellow)
                        .WithAuthor(embedAuthor, null, ubgeBot.AvatarUrl)
                        .WithDescription(embedDescription)
                        .WithTimestamp(DateTime.Now);

                    await canalLog.SendMessageAsync(embed: Embed.Build());
                }
                else if (tipo == TipoEmbed.ReactRoleUBGE)
                {
                    Embed.WithColor(DiscordColor.Green)
                        .WithAuthor(embedAuthor, null, ubgeBot.AvatarUrl)
                        .WithDescription(embedDescription)
                        .WithThumbnailUrl(membro.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    await canalLog.SendMessageAsync(embed: Embed.Build());
                }
                else if (tipo == TipoEmbed.SAC)
                {
                    Embed.WithColor(DiscordColor.MidnightBlue)
                        .WithAuthor(embedAuthor, null, fotoAuthorEmbed)
                        .WithDescription(embedDescription)
                        .WithThumbnailUrl(membro.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    await canalLog.SendMessageAsync(embed: Embed.Build());
                }
                else if (tipo == TipoEmbed.ReactRoleForaDaUBGE)
                {
                    Embed.WithColor(DiscordColor.Green)
                        .WithAuthor(embedAuthor, null, fotoAuthorEmbed)
                        .WithDescription(embedDescription)
                        .WithThumbnailUrl(membro.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    await canalLog.SendMessageAsync(embed: Embed.Build());
                }
            }
            catch (Exception) { }
        }

        public string RetornaDataAtualParecidoComODSharpPlus()
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