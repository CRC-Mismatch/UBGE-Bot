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

namespace UBGE_Bot.Utilidades
{
    public sealed class UtilidadesGerais
    {
        /// <summary>
        /// Retorna um tempo em string (Ex: 30s) para 00:00:30 .
        /// </summary>
        /// <param name="Tempo"></param>
        /// <returns></returns>
        public TimeSpan ConverterTempo(string tempoParaConverter)
        {
            if (tempoParaConverter.ToLower().Contains('s')) return TimeSpan.FromSeconds(Convert.ToInt32(tempoParaConverter.Split('s').FirstOrDefault()));
            if (tempoParaConverter.ToLower().Contains('m')) return TimeSpan.FromMinutes(Convert.ToInt32(tempoParaConverter.Split('m').FirstOrDefault()));
            if (tempoParaConverter.ToLower().Contains('h')) return TimeSpan.FromHours(Convert.ToInt32(tempoParaConverter.Split('h').FirstOrDefault()));
            if (tempoParaConverter.ToLower().Contains('d')) return TimeSpan.FromDays(Convert.ToInt32(tempoParaConverter.Split('d').FirstOrDefault()));
            
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
        public async Task<DiscordMessage> PegaResposta(InteractivityExtension interactivityExtension, CommandContext commandContext)
        {
            return (await interactivityExtension.WaitForMessageAsync(m => m.Author == commandContext.User && m.Channel.Id == commandContext.Channel.Id, TimeSpan.FromMinutes(30))).Result;
        }

        /// <summary>
        /// Pega a resposta digitada no privado de um membro do Discord.
        /// </summary>
        /// <param name="Interactivity"></param>
        /// <param name="CommandContext"></param>
        /// <returns></returns>
        public async Task<DiscordMessage> PegaRespostaPrivado(InteractivityExtension interactivityExtension, CommandContext commandContext)
        {
            DiscordChannel dm = await commandContext.Member.CreateDmChannelAsync();
            
            return (await interactivityExtension.WaitForMessageAsync(m => m.Author == commandContext.User && m.Channel == dm, TimeSpan.FromMinutes(30))).Result;
        }

        /// <summary>
        /// Pega a resposta digitada no privado de um membro do Discord.
        /// </summary>
        /// <param name="Interactivity"></param>
        /// <param name="CommandContext"></param>
        /// <returns></returns>
        public async Task<DiscordMessage> PegaRespostaPrivado(InteractivityExtension interactivityExtension, DiscordUser membro, DiscordChannel canal)
        {
            return (await interactivityExtension.WaitForMessageAsync(m => m.Author == membro && m.Channel == canal, TimeSpan.FromMinutes(30))).Result;
        }

        /// <summary>
        /// Procura o emoji que foi especificado na Task. O bot procurará em todos os servidores que ele está.
        /// </summary>
        /// <param name="CommandContext"></param>
        /// <param name="NomeDoEmoji"></param>
        /// <returns></returns>
        public async Task<DiscordEmoji> ProcuraEmoji(CommandContext commandContext, string nomeDoEmoji)
        {
            DiscordEmoji de = null;

            if (commandContext.Guild.Emojis.Values.ToList().Find(x => x.Name.ToLower() == nomeDoEmoji.ToLower()) == null)
            {
                foreach (var servidor in commandContext.Client.Guilds.Values)
                {
                    await Task.Delay(200);

                    de = servidor.Emojis.Values.ToList().Find(x => x.Name.ToLower() == nomeDoEmoji.ToLower());

                    if (de != null)
                        return de;
                }
            }

            if (commandContext.Guild.Emojis.Values.ToList().Find(x => x.Name.ToLower() == nomeDoEmoji.ToLower()) != null)
                return commandContext.Guild.Emojis.Values.ToList().Find(x => x.Name.ToLower() == nomeDoEmoji.ToLower());
            else
            {
                if (nomeDoEmoji.StartsWith(":") && nomeDoEmoji.EndsWith(":"))
                    de = DiscordEmoji.FromName(commandContext.Client, nomeDoEmoji);
                else
                    de = DiscordEmoji.FromName(commandContext.Client, $":{nomeDoEmoji}:");
            }

            return de;
        }

        /// <summary>
        /// Procura o emoji que foi especificado na Task. O bot procurará em todos os servidores que ele está.
        /// </summary>
        /// <param name="CommandContext"></param>
        /// <param name="NomeDoEmoji"></param>
        /// <returns></returns>
        public async Task<DiscordEmoji> ProcuraEmoji(DiscordClient discordClient, string nomeDoEmoji)
        {
            DiscordEmoji de = null;

            foreach (var servidor in discordClient.Guilds.Values)
            {
                if (servidor.Emojis.Values.ToList().Find(x => x.Name.ToLower() == nomeDoEmoji.ToLower()) == null)
                {
                    await Task.Delay(200);

                    de = servidor.Emojis.Values.ToList().Find(x => x.Name.ToLower() == nomeDoEmoji.ToLower());

                    if (de != null)
                        return de;
                }

                if (servidor.Emojis.Values.ToList().Find(x => x.Name.ToLower() == nomeDoEmoji.ToLower()) != null)
                    return servidor.Emojis.Values.ToList().Find(x => x.Name.ToLower() == nomeDoEmoji.ToLower());
                else
                {
                        if (nomeDoEmoji.StartsWith(":") && nomeDoEmoji.EndsWith(":"))
                            de = DiscordEmoji.FromName(discordClient, nomeDoEmoji);
                        else
                            de = DiscordEmoji.FromName(discordClient, $":{nomeDoEmoji}:");
                }
            }

            return de;
        }

        /// <summary>
        /// Checa se a string que foi especificada contem números.
        /// </summary>
        /// <param name="Texto"></param>
        /// <returns></returns>
        public bool ChecaSeAStringContemNumeros(string texto)
        {
            return texto.Where(c => char.IsNumber(c)).Count() > 0;
        }

        /// <summary>
        /// Checa se a string especificada contem letras.
        /// </summary>
        /// <param name="Texto"></param>
        /// <returns></returns>
        public bool ChecaSeAStringContemLetras(string texto)
        {
            return texto.Where(c => char.IsLetter(c)).Count() > 0;
        }

        /// <summary>
        /// Função que limpa embed e o retorna vazio para ser usado novamente, assim o comando será mais otimizado e rápido, e será só usado 1 embed em todo o comando.
        /// </summary>
        /// <param name="Embed"></param>
        /// <returns></returns>
        public DiscordEmbedBuilder LimpaEmbed(DiscordEmbedBuilder embed)
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

        /// <summary>
        /// Menciona o membro de acordo com o nickname do mesmo, assim as menções não ficarão bugadas.
        /// </summary>
        /// <param name="Membro"></param>
        /// <returns></returns>
        public string MencaoMembro(DiscordMember membro) 
            => $"{(!string.IsNullOrWhiteSpace(membro.Nickname) ? $"<@!{membro.Id}>" : $"<@{membro.Id}>")}";

        /// <summary>
        /// Checa se a string contem mais de 18 números.
        /// Checagem para comparar se o nick, (ex: "Luiz123"), tem mais de 18 números (Número de caracteres do ID do membro do Discord).
        /// </summary>
        /// <param name="Texto"></param>
        /// <returns></returns>
        public bool ChecaSeAStringContemMaisOuEIgualA18Numeros(string texto)
        {
            return texto.Where(c => char.IsNumber(c)).Count() >= 18;
        }

        /// <summary>
        /// Procura o emoji que corresponde a o status do membro no Discord.
        /// </summary>
        /// <param name="CommandContext"></param>
        /// <param name="Membro"></param>
        /// <returns></returns>
        public async Task<DiscordEmoji> ConverteStatusParaEmoji(CommandContext commandContext, DiscordMember membro)
        {
            if (membro.Presence == null)
                return await ProcuraEmoji(commandContext, "status_offline");
            else if (membro.Presence.Status == UserStatus.Online)
                return await ProcuraEmoji(commandContext, "status_online");
            else if (membro.Presence.Status == UserStatus.DoNotDisturb)
                return await ProcuraEmoji(commandContext, "status_ocupado");
            else if (membro.Presence.Status == UserStatus.Offline)
                return await ProcuraEmoji(commandContext, "status_offline");
            else if (membro.Presence.Status == UserStatus.Invisible)
                return await ProcuraEmoji(commandContext, "status_offline");
            else if (membro.Presence.Status == UserStatus.Idle)
                return await ProcuraEmoji(commandContext, "status_ausente");

            return null;
        }

        /// <summary>
        /// Retorna o "s", "m", "h" e "d" do tempo especificado.
        /// </summary>
        /// <param name="Texto"></param>
        /// <returns></returns>
        public string RetornaSegundosMinutosHorasDiasDaPunicao(string texto)
        {
            if (texto.Contains("s")) 
                return "s";
            else if (texto.Contains("m")) 
                return "m";
            else if (texto.Contains("h")) 
                return "h";
            else if (texto.Contains("d")) 
                return "d";

            return null;
        }

        /// <summary>
        /// Converte o status do membro no Discord para nome.
        /// </summary>
        /// <param name="Membro"></param>
        /// <returns></returns>
        public string ConverteStatusPraNome(DiscordMember membro)
        {
            if (membro.Presence == null)
                return "Offline";
            else if (membro.Presence.Status == UserStatus.Online)
                return "Online";
            else if (membro.Presence.Status == UserStatus.DoNotDisturb)
                return "Não pertube";
            else if (membro.Presence.Status == UserStatus.Offline)
                return "Offline";
            else if (membro.Presence.Status == UserStatus.Invisible)
                return "Offline";
            else if (membro.Presence.Status == UserStatus.Idle)
                return "Ausente";

            return null;
        }

        /// <summary>
        /// Converte byte para string.
        /// </summary>
        /// <param name="byteParaString"></param>
        /// <returns></returns>
        public string ByteParaString(byte[] byteParaString)
        {
            return Encoding.UTF8.GetString(byteParaString, 0, byteParaString.Length);
        }

        /// <summary>
        /// Faz o check para retornar o nick correto do membro no Discord.
        /// </summary>
        /// <param name="membro"></param>
        /// <returns></returns>
        public string RetornaNomeDiscord(DiscordMember membro)
            => $"{(string.IsNullOrWhiteSpace(membro.Nickname) ? membro.Username : membro.Nickname)}";

        /// <summary>
        /// Retorna uma lista de emoji a partir de uma lista de string com os nomes dos mesmos.
        /// </summary>
        /// <param name="commandContext"></param>
        /// <param name="emojisParaBuscar"></param>
        /// <returns></returns>
        public async Task<List<DiscordEmoji>> RetornaEmojis(CommandContext commandContext, List<string> emojisParaBuscar) 
        {
            List<DiscordEmoji> emojis = new List<DiscordEmoji>();

            foreach (var nomeEmoji in emojisParaBuscar)
                    emojis.Add(await ProcuraEmoji(commandContext, nomeEmoji));

            return emojis;
        }

        /// <summary>
        /// Retorna uma lista de emoji a partir de uma lista de string com os nomes dos mesmos.
        /// </summary>
        /// <param name="discordClient"></param>
        /// <param name="emojisParaBuscar"></param>
        /// <returns></returns>
        public async Task<List<DiscordEmoji>> RetornaEmojis(DiscordClient discordClient, List<string> emojisParaBuscar) 
        {
            List<DiscordEmoji> emojis = new List<DiscordEmoji>();

            foreach (var nomeEmoji in emojisParaBuscar)
                    emojis.Add(await ProcuraEmoji(discordClient, nomeEmoji));

            return emojis;
        }

        /// <summary>
        /// Retorna o nome do Estado quando é enviado a sigla do mesmo.
        /// </summary>
        /// <param name="siglaEstado"></param>
        /// <returns></returns>
        public string RetornaEstado(string siglaEstado)
        {
            if (siglaEstado.ToUpper() == "AM")
                return "Amazonas";
            else if (siglaEstado.ToUpper() == "RR")
                return "Roraima";
            else if (siglaEstado.ToUpper() == "AP")
                return "Amapá";
            else if (siglaEstado.ToUpper() == "PA")
                return "Pará";
            else if (siglaEstado.ToUpper() == "TO")
                return "Tocantins";
            else if (siglaEstado.ToUpper() == "RO")
                return "Rondônia";
            else if (siglaEstado.ToUpper() == "AC")
                return "Acre";
            else if (siglaEstado.ToUpper() == "MA")
                return "Maranhão";
            else if (siglaEstado.ToUpper() == "PI")
                return "Piauí";
            else if (siglaEstado.ToUpper() == "CE")
                return "Ceará";
            else if (siglaEstado.ToUpper() == "RN")
                return "Rio Grande do Norte";
            else if (siglaEstado.ToUpper() == "PE")
                return "Pernambuco";
            else if (siglaEstado.ToUpper() == "PB")
                return "Paraíba";
            else if (siglaEstado.ToUpper() == "SE")
                return "Sergipe";
            else if (siglaEstado.ToUpper() == "AL")
                return "Alagoas";
            else if (siglaEstado.ToUpper() == "BA")
                return "Bahia";
            else if (siglaEstado.ToUpper() == "MT")
                return "Mato Grosso";
            else if (siglaEstado.ToUpper() == "MS")
                return "Mato Grosso do Sul";
            else if (siglaEstado.ToUpper() == "GO")
                return "Goiás";
            else if (siglaEstado.ToUpper() == "DF")
                return "Distrito Federal";
            else if (siglaEstado.ToUpper() == "SP")
                return "São Paulo";
            else if (siglaEstado.ToUpper() == "RJ")
                return "Rio de Janeiro";
            else if (siglaEstado.ToUpper() == "ES")
                return "Espírito Santo";
            else if (siglaEstado.ToUpper() == "MG")
                return "Minas Gerais";
            else if (siglaEstado.ToUpper() == "PR")
                return "Paraná";
            else if (siglaEstado.ToUpper() == "RS")
                return "Rio Grande do Sul";
            else if (siglaEstado.ToUpper() == "SC")
                return "Santa Catarina";
            else
                return "Não especificado.";
        }

        /// <summary>
        /// Exclui as reações de um emoji de acordo com a mensagem e lista de membros que reagiram.
        /// </summary>
        /// <param name="mensagemEmbedReact"></param>
        /// <param name="emoji"></param>
        /// <param name="membrosQueReagiram"></param>
        /// <returns></returns>
        public async Task ExcluiReacoesDeUmaListaDeMembros(DiscordMessage mensagemEmbedReact, DiscordEmoji emoji, IReadOnlyList<DiscordUser> membrosQueReagiram)
        {
            foreach (var membroDaReacao in membrosQueReagiram)
            {
                try
                {
                    await Task.Delay(200);

                    await mensagemEmbedReact.DeleteReactionAsync(emoji, membroDaReacao);
                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// Cor para o embed de ajuda nos comandos da staff/gerais.
        /// </summary>
        /// <returns></returns>
        public DiscordColor CorHelpComandos()
            => new DiscordColor(54, 57, 64);
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

    public sealed class UBGE_StaffAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            return Task.FromResult(ctx.Guild.Id == Valores.Guilds.UBGE && ctx.Member.Roles.ToList().FindAll(x => x.Permissions.HasFlag(Permissions.KickMembers)).Count != 0 || Debugger.IsAttached);
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

    public sealed class UBGE_CrieSuaSalaAquiAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            return Task.FromResult(ctx.Guild.Id == Valores.Guilds.UBGE && ctx.Channel.Id == Valores.ChatsUBGE.canalCrieSuaSalaAqui);
        }
    }
}