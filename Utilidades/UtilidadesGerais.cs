using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UBGEBot.Utilidades
{
    public sealed class UtilidadesGerais
    {
        /// <summary>
        /// Retorna um tempo em string (Ex: 30s) para 00:00:30 .
        /// </summary>
        /// <param name="Tempo"></param>
        /// <returns></returns>
        public TimeSpan ConverterTempo(string Tempo)
        {
            if (Tempo.ToLower().Contains('s')) return TimeSpan.FromSeconds(Convert.ToInt32(Tempo.Split('s').FirstOrDefault()));
            if (Tempo.ToLower().Contains('m')) return TimeSpan.FromMinutes(Convert.ToInt32(Tempo.Split('m').FirstOrDefault()));
            if (Tempo.ToLower().Contains('h')) return TimeSpan.FromHours(Convert.ToInt32(Tempo.Split('h').FirstOrDefault()));
            if (Tempo.ToLower().Contains('d')) return TimeSpan.FromDays(Convert.ToInt32(Tempo.Split('d').FirstOrDefault()));
            
            return TimeSpan.FromSeconds(0);
        }

        /// <summary>
        /// Retorna uma cor aleatória do Discord.
        /// </summary>
        /// <returns></returns>
        public DiscordColor CorAleatoriaEmbed()
        {
            Random _r = new Random(DateTime.Now.Ticks.GetHashCode());
            int r = _r.Next(0, 255);
            int g = _r.Next(0, 255);
            int b = _r.Next(0, 255);
            string rHex = r.ToString("X");
            string gHex = g.ToString("X");
            string bHex = b.ToString("X");
            int cor = Convert.ToInt32(rHex + gHex + bHex, 16);

            return new DiscordColor(cor);
        }

        /// <summary>
        /// Pega a resposta digitada em qualquer canal (De preferência onde o comando foi executado) de um membro do Discord.
        /// </summary>
        /// <param name="Interactivity"></param>
        /// <param name="CommandContext"></param>
        /// <returns></returns>
        public async Task<DiscordMessage> PegaResposta(InteractivityExtension Interactivity, CommandContext CommandContext)
        {
            return (await Interactivity.WaitForMessageAsync(m => m.Author == CommandContext.User && m.Channel.Id == CommandContext.Channel.Id, TimeSpan.FromMinutes(30))).Result;
        }

        /// <summary>
        /// Pega a resposta digitada no privado de um membro do Discord.
        /// </summary>
        /// <param name="Interactivity"></param>
        /// <param name="CommandContext"></param>
        /// <returns></returns>
        public async Task<DiscordMessage> PegaRespostaPrivado(InteractivityExtension Interactivity, CommandContext CommandContext)
        {
            DiscordChannel dm = await CommandContext.Member.CreateDmChannelAsync();
            
            return (await Interactivity.WaitForMessageAsync(m => m.Author == CommandContext.User && m.Channel == dm, TimeSpan.FromMinutes(30))).Result;
        }

        /// <summary>
        /// Procura o emoji que foi especificado na Task. O bot procurará em todos os servidores que ele está.
        /// </summary>
        /// <param name="CommandContext"></param>
        /// <param name="NomeDoEmoji"></param>
        /// <returns></returns>
        public async Task<DiscordEmoji> ProcuraEmoji(CommandContext CommandContext, string NomeDoEmoji)
        {
            DiscordEmoji De = null;

            if (CommandContext.Guild.Emojis.Values.ToList().Find(x => x.Name.ToLower() == NomeDoEmoji.ToLower()) == null)
            {
                foreach (ulong u in CommandContext.Client.Guilds.Keys)
                {
                    DiscordGuild server = await CommandContext.Client.GetGuildAsync(u);
                    De = server.Emojis.Values.ToList().Find(x => x.Name.ToLower() == NomeDoEmoji.ToLower());

                    if (De != null)
                        return De;
                }
            }
            if (CommandContext.Guild.Emojis.Values.ToList().Find(x => x.Name.ToLower() == NomeDoEmoji.ToLower()) != null)
                return CommandContext.Guild.Emojis.Values.ToList().Find(x => x.Name.ToLower() == NomeDoEmoji.ToLower());
            else
            {
                if (NomeDoEmoji.StartsWith(":") && NomeDoEmoji.EndsWith(":"))
                    De = DiscordEmoji.FromName(CommandContext.Client, NomeDoEmoji);
                else
                    De = DiscordEmoji.FromName(CommandContext.Client, $":{NomeDoEmoji}:");
            }

            return De;
        }

        /// <summary>
        /// Procura o emoji que foi especificado na Task. O bot procurará em todos os servidores que ele está.
        /// </summary>
        /// <param name="CommandContext"></param>
        /// <param name="NomeDoEmoji"></param>
        /// <returns></returns>
        public async Task<DiscordEmoji> ProcuraEmoji(DiscordClient DiscordClient, string NomeDoEmoji)
        {
            DiscordEmoji De = null;

            foreach (var Servidor in DiscordClient.Guilds.Values)
            {
                if (Servidor.Emojis.Values.ToList().Find(x => x.Name.ToLower() == NomeDoEmoji.ToLower()) == null)
                {
                    DiscordGuild server = await DiscordClient.GetGuildAsync(Valores.Guilds.UBGE);
                    De = server.Emojis.Values.ToList().Find(x => x.Name.ToLower() == NomeDoEmoji.ToLower());

                    if (De != null)
                        return De;
                }

                if (Servidor.Emojis.Values.ToList().Find(x => x.Name.ToLower() == NomeDoEmoji.ToLower()) != null)
                    return Servidor.Emojis.Values.ToList().Find(x => x.Name.ToLower() == NomeDoEmoji.ToLower());
                else
                {
                    if (NomeDoEmoji.StartsWith(":") && NomeDoEmoji.EndsWith(":"))
                        De = DiscordEmoji.FromName(DiscordClient, NomeDoEmoji);
                    else
                        De = DiscordEmoji.FromName(DiscordClient, $":{NomeDoEmoji}:");
                }
            }

            return De;
        }

        /// <summary>
        /// Retorna um embed que é paginado, ou seja, que contem reações para avançar ou retroceder as informações contidas ali.
        /// </summary>
        /// <param name="CommandContext"></param>
        /// <param name="Mensagem"></param>
        /// <param name="TamanhoEmbed"></param>
        /// <returns></returns>
        public IEnumerable<Page> PaginaEmbeds(CommandContext CommandContext, string Mensagem, int TamanhoEmbed)
        {
            if (string.IsNullOrEmpty(Mensagem))
                throw new InvalidOperationException("Você deve fornecer uma string que não seja nula ou vazia!");

            List<Page> result = new List<Page>();
            List<string> split = Mensagem.Split('\n').ToList();
            int page = 1;

            foreach (string s in split)
            {
                result.Add(new Page(string.Empty, new DiscordEmbedBuilder()
                        .WithAuthor($"Página: {page++}", null, Valores.logoUBGE)
                        .WithDescription(s)
                        .WithTimestamp(DateTime.Now)
                        .WithColor(new UtilidadesGerais().CorAleatoriaEmbed())
                        .WithFooter("Comando requisitado pelo: " + CommandContext.Member.Username, iconUrl: CommandContext.Member.AvatarUrl)
                ));
            }

            return result;
        }

        /// <summary>
        /// Checa se a string que foi especificada contem números.
        /// </summary>
        /// <param name="Texto"></param>
        /// <returns></returns>
        public bool ChecaSeAStringContemNumeros(string Texto)
        {
            return Texto.Where(c => char.IsNumber(c)).Count() > 0;
        }

        /// <summary>
        /// Checa se a string especificada contem letras.
        /// </summary>
        /// <param name="Texto"></param>
        /// <returns></returns>
        public bool ChecaSeAStringContemLetras(string Texto)
        {
            return Texto.Where(c => char.IsLetter(c)).Count() > 0;
        }

        /// <summary>
        /// Função que limpa embed e o retorna vazio para ser usado novamente, assim o comando será mais otimizado e rápido, e será só usado 1 embed em todo o comando.
        /// </summary>
        /// <param name="Embed"></param>
        /// <returns></returns>
        public DiscordEmbedBuilder LimpaEmbed(DiscordEmbedBuilder Embed)
        {
            Embed.ClearFields();
            Embed.Description = string.Empty;
            Embed.Footer = new DiscordEmbedBuilder.EmbedFooter { Text = string.Empty, IconUrl = string.Empty };
            Embed.Timestamp = null;
            Embed.ThumbnailUrl = string.Empty;
            Embed.Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = string.Empty, Name = string.Empty, Url = string.Empty };
            Embed.Color = new DiscordColor();
            Embed.ImageUrl = string.Empty;
            Embed.Title = string.Empty;
            Embed.Url = string.Empty;

            return Embed;
        }

        /// <summary>
        /// Menciona o membro de acordo com o nickname do mesmo, assim as menções não ficaram bugadas.
        /// </summary>
        /// <param name="Membro"></param>
        /// <returns></returns>
        public string MencaoMembro(DiscordMember Membro) 
            => $"{(!string.IsNullOrWhiteSpace(Membro.Nickname) ? $"<@!{Membro.Id}>" : $"<@{Membro.Id}>")}";

        /// <summary>
        /// Checa se a string especificada contem um símbolo, ex: @, #, entre outros.
        /// </summary>
        /// <param name="Texto"></param>
        /// <returns></returns>
        public bool ChecaSeAStringContemSimbolos(string Texto)
        {
            List<char> Simbolos = new List<char>
            {
                '@', '#', '$', '%', '¨', '&', '*', '(', ')', '-', '_',
                '+', '=', '`', '~', '^', '[', ']', '{', '}', 'ª', 'º',
                '<', '>', ':', ';', '/', '!', ',', '.', '²', '³', '¹',
                '£', '¢', '¬', '§', '\\',
            };

            return Simbolos.Where(x => Texto.Contains(x)).Count() > 0;
        }

        /// <summary>
        /// Checa se a string contem mais de 18 números.
        /// Checagem para comparar se o nick, (ex: "Luiz123"), tem mais de 18 números (Número de caracteres do ID do membro do Discord).
        /// </summary>
        /// <param name="Texto"></param>
        /// <returns></returns>
        public bool ChecaSeAStringContemMaisOuEIgualA18Numeros(string Texto)
        {
            return Texto.Where(c => char.IsNumber(c)).Count() >= 18;
        }

        /// <summary>
        /// Procura o emoji que corresponde a o status do membro no Discord.
        /// </summary>
        /// <param name="CommandContext"></param>
        /// <param name="Membro"></param>
        /// <returns></returns>
        public async Task<DiscordEmoji> ConverteStatusParaEmoji(CommandContext CommandContext, DiscordMember Membro)
        {
            if (Membro.Presence == null)
                return await ProcuraEmoji(CommandContext, "status_offline");
            else if (Membro.Presence.Status == UserStatus.Online)
                return await ProcuraEmoji(CommandContext, "status_online");
            else if (Membro.Presence.Status == UserStatus.DoNotDisturb)
                return await ProcuraEmoji(CommandContext, "status_ocupado");
            else if (Membro.Presence.Status == UserStatus.Offline)
                return await ProcuraEmoji(CommandContext, "status_offline");
            else if (Membro.Presence.Status == UserStatus.Invisible)
                return await ProcuraEmoji(CommandContext, "status_offline");
            else if (Membro.Presence.Status == UserStatus.Idle)
                return await ProcuraEmoji(CommandContext, "status_ausente");

            return null;
        }

        /// <summary>
        /// Retorna o "s", "m", "h" e "d" do tempo especificado.
        /// </summary>
        /// <param name="Texto"></param>
        /// <returns></returns>
        public string RetornaSegundosMinutosHorasDiasDaPunicao(string Texto)
        {
            if (Texto.Contains("s")) return "s";
            else if (Texto.Contains("m")) return "m";
            else if (Texto.Contains("h")) return "h";
            else if (Texto.Contains("d")) return "d";

            return null;
        }

        /// <summary>
        /// Converte o status do membro no Discord para nome.
        /// </summary>
        /// <param name="Membro"></param>
        /// <returns></returns>
        public string ConverteStatusPraNome(DiscordMember Membro)
        {
            if (Membro.Presence == null)
                return "Offline";
            else if (Membro.Presence.Status == UserStatus.Online)
                return "Online";
            else if (Membro.Presence.Status == UserStatus.DoNotDisturb)
                return "Ocupado";
            else if (Membro.Presence.Status == UserStatus.Offline)
                return "Offline";
            else if (Membro.Presence.Status == UserStatus.Invisible)
                return "Offline";
            else if (Membro.Presence.Status == UserStatus.Idle)
                return "Ausente";

            return null;
        }
    }

    public sealed class UBGE_E_EtcAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            return Task.FromResult(ctx.Guild.Id == Valores.Guilds.UBGE || ctx.Guild.Id == Valores.Guilds.testesDoLuiz || ctx.Guild.Id == Valores.Guilds.CBPR
                || ctx.Guild.Id == Valores.Guilds.emojos || ctx.Guild.Id == Valores.Guilds.ruinasDeAstapor || ctx.Guild.Id == Valores.Guilds.emoji);
        }
    }

    public sealed class UBGEAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            return Task.FromResult(ctx.Guild.Id == Valores.Guilds.UBGE);
        }
    }

    public sealed class RuinasDeAstaporAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            return Task.FromResult(ctx.Guild.Id == Valores.Guilds.UBGE || ctx.Guild.Id == Valores.Guilds.ruinasDeAstapor);
        }
    }

    public sealed class UBGEAlbionAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            return Task.FromResult(ctx.Guild.Id == Valores.Guilds.ubgeAlbion);
        }
    }
}