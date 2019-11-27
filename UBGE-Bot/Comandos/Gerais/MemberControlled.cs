using Autofac;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using UBGE_Bot.APIs;
using UBGE_Bot.Main;
using UBGE_Bot.MongoDB.Modelos;
using UBGE_Bot.Utilidades;
using UBGE_Bot.LogExceptions;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace UBGE_Bot.Comandos.Gerais
{
    public sealed class MemberControlled : BaseCommandModule
    {
        [Command("?"), Aliases("ajuda", "help")]

        public async Task HelpAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    if (ctx.Guild.Id == Valores.Guilds.UBGE)
                    {
                        embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithAuthor($"Olá, {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}!", null, Valores.logoUBGE)
                            //.WithDescription($"Digite: `{ctx.Prefix}tutorial` para você receber ajuda com todos os sistemas implementados.")
                            .WithThumbnailUrl(ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now)
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                    else
                    {
                        embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithAuthor($"Olá, {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}!", null, Valores.logoUBGE)
                            //.AddField("Aqui vão meus comandos gerais (Parte 1):", $"`{ctx.Prefix}bot ping`\nMostra o ping do bot.\n\n" +
                            //$"`{ctx.Prefix}meuid`\nMostra o id do membro que executou o comando.\n\n" +
                            //$"`{ctx.Prefix}avatar Membro(Nulo ou Menção)`\nMostra o avatar do membro que executou o comando, mas caso o comando foi executado com a " +
                            //$"menção de um membro, o bot mostrará o avatar do membro que foi mencionado.\n\n" +
                            //$"`{ctx.Prefix}userinfo Membro(Nulo ou Menção)`\nMostra informações sobre o membro que executou o comando, mas caso o comando foi executado com a " +
                            //$"menção de um membro, o bot mostrará o informações do membro que foi mencionado.\n\n" +
                            //$"`{ctx.Prefix}dólar`\nMostra o preço do dólar.\n\n" +
                            //$"`{ctx.Prefix}euro`\nMostra o preço do euro.\n\n" +
                            //$"`{ctx.Prefix}servidorinfo`\nMostra informações referentes ao servidor onde o comando foi executado.\n\n" +
                            //$"`{ctx.Prefix}fotoservidor`\nMostra a foto do servidor onde o comando foi executado.\n\n" +
                            //$"`{ctx.Prefix}procuramembros Jogo(Nome de um Jogo)`\nO bot mostrará todos os membros que estão jogando o jogo que foi especificado no comando.\n\n" +
                            //$"`{ctx.Prefix}listar ID(ID de um Canal de Voz)`\nO bot mostrará todos os membros que estão em um canal de voz.\n\n")
                            //.WithDescription($"Para mais comandos, execute `{ctx.Prefix}{ctx.Command.Name}` na UBGE.")
                            .WithThumbnailUrl(ctx.Member.AvatarUrl)                            
                            .WithTimestamp(DateTime.Now)
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("ping")]

        public async Task PingAsync(CommandContext ctx)
            => await ctx.RespondAsync($"Meu ping é: **{ctx.Client.Ping}ms**! {await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "leofsjal")}");

        [Command("meuid")]

        public async Task MeuIDAsync(CommandContext ctx)
            => await ctx.RespondAsync($"{ctx.Member.Mention}, seu ID é: {ctx.Member.Id}.");

        [Command("fazercenso")]

        public async Task FazerCensoAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                DiscordMessage avisoCensoNoPV = null;

                try
                {
                    var local = Program.ubgeBot.localDB;
                    var collectionCenso = local.GetCollection<Censo>(Valores.Mongo.censo);

                    DiscordDmChannel pvMembro = await ctx.Member.CreateDmChannelAsync();
                    var interact = ctx.Client.GetInteractivity();
                    string logoUBGEFoto = Directory.GetCurrentDirectory() + @"\Logos\LogoUBGE.png";
                    DiscordChannel log = ctx.Guild.GetChannel(Valores.ChatsUBGE.canalLog), ubgeBot = ctx.Guild.GetChannel(Valores.ChatsUBGE.ubgeBot);
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();
                    DiscordMember luiz = await ctx.Guild.GetMemberAsync(Valores.Guilds.Membros.luiz);

                    string respostaFinalComoChegouAUBGE = string.Empty,
                    respostaFinalJogosMaisJogados = string.Empty,
                    respostaFinalEstadoOndeOMembroMora = string.Empty,
                    respostaFinalEmail = string.Empty;
                    int respostaIdadeFinal = 0;

                    var apiGoogle = Program.ubgeBot.servicesIContainer.Resolve<Google_Sheets.Write>();

                    var listaEmojis = Program.emojisCache;

                    var emojiVoltar = DiscordEmoji.FromName(ctx.Client, ":arrow_backward:");

                    var filtro = Builders<Censo>.Filter.Eq(x => x.idNoDiscord, ctx.Member.Id);
                    var listaFezOCenso = await (await collectionCenso.FindAsync(filtro)).ToListAsync();

                    bool censoDenovo = false;

                    if (listaFezOCenso.Count != 0)
                    {
                        censoDenovo = true;

                        var marcarSim = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
                        var marcarNao = DiscordEmoji.FromName(ctx.Client, ":negative_squared_cross_mark:");

                        embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                             .WithAuthor($"Você já fez o censo! Deseja fazer denovo?", null, Valores.logoUBGE)
                             .WithDescription($"Você já respondeu o censo no dia e hora: **{listaFezOCenso.LastOrDefault().timestamp}**.\n\n" +
                             $"Clique na reação :white_check_mark: para refazer o censo\n" +
                             $"Clique na reação :negative_squared_cross_mark: para não refazer o censo.")
                             .WithThumbnailUrl(ctx.Member.AvatarUrl)
                             .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                             .WithTimestamp(DateTime.Now);

                        DiscordMessage msgEmbed = await ctx.RespondAsync(embed: embed.Build());
                        await msgEmbed.CreateReactionAsync(marcarSim);
                        await Task.Delay(200);
                        await msgEmbed.CreateReactionAsync(marcarNao);

                        var respostaEmoji = (await interact.WaitForReactionAsync(msgEmbed, ctx.User)).Result.Emoji;

                        if (respostaEmoji == marcarSim)
                        {
                            await ctx.Message.DeleteAsync();
                            await msgEmbed.DeleteAsync();
                        }
                        else if (respostaEmoji == marcarNao)
                            return;
                    }

                    Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                    embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithAuthor("O censo será respondido no privado.", null, Valores.logoUBGE)
                        .WithThumbnailUrl(ctx.Member.AvatarUrl)
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    avisoCensoNoPV = await ctx.RespondAsync(ctx.Member.Mention, embed: embed.Build());

                    Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                    embed.WithAuthor("Digite sua idade. (ESCREVA EM NÚMERO)", null, Valores.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                    DiscordMessage PerguntaIdade = await pvMembro.SendMessageAsync(embed: embed.Build());
                    DiscordMessage inputIdade = await Program.ubgeBot.utilidadesGerais.PegaRespostaPrivado(interact, ctx), inputIdade_ = null;

                    await avisoCensoNoPV.DeleteAsync();

                    if (inputIdade.Content == "0" || inputIdade.Content == "00" || inputIdade.Content.Length >= 3 || Program.ubgeBot.utilidadesGerais.ChecaSeAStringContemLetras(inputIdade.Content))
                    {
                        inputIdade = null;

                        Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                        embed.WithAuthor("Digite uma idade válida!", null, Valores.logoUBGE)
                            .WithColor(DiscordColor.Red);

                        while (true)
                        {
                            DiscordMessage perguntaIdade_ = await pvMembro.SendMessageAsync(embed: embed.Build());
                            inputIdade_ = await Program.ubgeBot.utilidadesGerais.PegaRespostaPrivado(interact, ctx);

                            if (inputIdade_.Content == "0" || inputIdade_.Content == "00" || inputIdade_.Content.Length >= 3 || Program.ubgeBot.utilidadesGerais.ChecaSeAStringContemLetras(inputIdade_.Content))
                                continue;
                            else
                                break;
                        }
                    }

                    Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                    var emojiRegiaoNorte = listaEmojis.Find(x => x.Name.ToUpper().Contains("NORTE"));
                    var emojiRegiaoNordeste = listaEmojis.Find(x => x.Name.ToUpper().Contains("NORDESTE"));
                    var emojiRegiaoCentroOeste = listaEmojis.Find(x => x.Name.ToUpper().Contains("CENTRO_OESTE"));
                    var emojiRegiaoSudeste = listaEmojis.Find(x => x.Name.ToUpper().Contains("SUDESTE"));
                    var emojiRegiaoSul = listaEmojis.Find(x => x.Name.ToUpper().Contains("REGIAO_SUL"));
                    var emojiRegiaoForaDoBrasil = listaEmojis.Find(x => x.Name.ToUpper().Contains("EXTERIOR"));

                    botaoVoltarEscolhaRegiao:

                    embed.WithAuthor("Selecione a região onde você mora.", null, Valores.logoUBGE)
                        .WithThumbnailUrl(ctx.Member.AvatarUrl)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithDescription($"{emojiRegiaoNorte} - Região Norte\n" +
                        $"{emojiRegiaoNordeste} - Região Nordeste\n" +
                        $"{emojiRegiaoCentroOeste} - Região Centro-Oeste\n" +
                        $"{emojiRegiaoSudeste} - Região Sudeste\n" +
                        $"{emojiRegiaoSul} - Região Sul\n\n" +
                        $"{emojiRegiaoForaDoBrasil} - Exterior (Fora do Brasil)")
                        .WithFooter("Você tem 1 minuto pra escolher!")
                        .WithTimestamp(DateTime.Now);

                    DiscordMessage msgEmbedRegiaoReact = await pvMembro.SendMessageAsync(embed: embed.Build());
                    await msgEmbedRegiaoReact.CreateReactionAsync(emojiRegiaoNorte);
                    await Task.Delay(200);
                    await msgEmbedRegiaoReact.CreateReactionAsync(emojiRegiaoNordeste);
                    await Task.Delay(200);
                    await msgEmbedRegiaoReact.CreateReactionAsync(emojiRegiaoCentroOeste);
                    await Task.Delay(200);
                    await msgEmbedRegiaoReact.CreateReactionAsync(emojiRegiaoSudeste);
                    await Task.Delay(200);
                    await msgEmbedRegiaoReact.CreateReactionAsync(emojiRegiaoSul);
                    await Task.Delay(200);
                    await msgEmbedRegiaoReact.CreateReactionAsync(emojiRegiaoForaDoBrasil);
                    await Task.Delay(200);

                    var emojiDoEstado = (await interact.WaitForReactionAsync(msgEmbedRegiaoReact, ctx.User, TimeSpan.FromMinutes(1))).Result.Emoji;

                    Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                    if (emojiDoEstado == emojiRegiaoNorte)
                    {
                        await msgEmbedRegiaoReact.DeleteAsync();

                        var emojiEstadoAmazonas = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_AMAZONAS"));
                        var emojiEstadoRoraima = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_RORAIMA"));
                        var emojiEstadoAmapa = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_AMAPA"));
                        var emojiEstadoPara = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_PARA"));
                        var emojiEstadoTocantins = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_TOCANTINS"));
                        var emojiEstadoRondonia = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_RONDONIA"));
                        var emojiEstadoAcre = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_ACRE"));

                        embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithAuthor("Clique no estado onde você mora.", null, Valores.logoUBGE)
                            .WithDescription($"{emojiEstadoAmazonas} - Amazonas (AM)\n" +
                            $"{emojiEstadoRoraima} - Roraima (RR)\n" +
                            $"{emojiEstadoAmapa} - Amapá (AP)\n" +
                            $"{emojiEstadoPara} - Pará (PA)\n" +
                            $"{emojiEstadoTocantins} - Tocantins (TO)\n" +
                            $"{emojiEstadoRondonia} - Rondônia (RO)\n" +
                            $"{emojiEstadoAcre} - Acre (AC)\n\n" +
                            $"{emojiVoltar} - Voltar para a escolha dos estados")
                            .WithFooter("Você tem 1 minuto pra escolher!")
                            .WithThumbnailUrl(ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        DiscordMessage msgEmbedEstadoReact = await pvMembro.SendMessageAsync(embed: embed.Build());
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoAmazonas);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoRoraima);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoAmapa);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoPara);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoTocantins);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoRondonia);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoAcre);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiVoltar);
                        await Task.Delay(200);

                        var emojiEstadoDoNorte = (await interact.WaitForReactionAsync(msgEmbedEstadoReact, ctx.User, TimeSpan.FromMinutes(1))).Result.Emoji;

                        if (emojiEstadoDoNorte == emojiEstadoAmazonas)
                            respostaFinalEstadoOndeOMembroMora = "AM";
                        else if (emojiEstadoDoNorte == emojiEstadoRoraima)
                            respostaFinalEstadoOndeOMembroMora = "RR";
                        else if (emojiEstadoDoNorte == emojiEstadoAmapa)
                            respostaFinalEstadoOndeOMembroMora = "AP";
                        else if (emojiEstadoDoNorte == emojiEstadoPara)
                            respostaFinalEstadoOndeOMembroMora = "PA";
                        else if (emojiEstadoDoNorte == emojiEstadoTocantins)
                            respostaFinalEstadoOndeOMembroMora = "TO";
                        else if (emojiEstadoDoNorte == emojiEstadoRondonia)
                            respostaFinalEstadoOndeOMembroMora = "RO";
                        else if (emojiEstadoDoNorte == emojiEstadoAcre)
                            respostaFinalEstadoOndeOMembroMora = "AC";
                        else if (emojiEstadoDoNorte == emojiVoltar)
                        {
                            await msgEmbedEstadoReact.DeleteAsync();
                            goto botaoVoltarEscolhaRegiao;
                        }
                        else
                            respostaFinalEstadoOndeOMembroMora = "Não especificado.";
                    }
                    else if (emojiDoEstado == emojiRegiaoNordeste) 
                    {
                        await msgEmbedRegiaoReact.DeleteAsync();

                        var emojiEstadoMaranhao = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_MARANHAO"));
                        var emojiEstadoPiaui = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_PIAUI"));
                        var emojiEstadoCeara = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_CEARA"));
                        var emojiEstadoRioGrandeDoNorte = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_RIO_GRANDE_DO_NORTE"));
                        var emojiEstadoPernambuco = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_PERNAMBUCO"));
                        var emojiEstadoParaiba = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_PARAIBA"));
                        var emojiEstadoSergipe = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_SERGIPE"));
                        var emojiEstadoAlagoas = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_ALAGOAS"));
                        var emojiEstadoBahia = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_BAHIA"));

                        embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithAuthor("Clique no estado onde você mora.", null, Valores.logoUBGE)
                            .WithDescription($"{emojiEstadoMaranhao} - Maranhão (MA)\n" +
                            $"{emojiEstadoPiaui} - Piauí (PI)\n" +
                            $"{emojiEstadoCeara} - Ceará (CE)\n" +
                            $"{emojiEstadoRioGrandeDoNorte} - Rio Grande do Norte (RN)\n" +
                            $"{emojiEstadoPernambuco} - Pernambuco (PE)\n" +
                            $"{emojiEstadoParaiba} - Paraíba (PB)\n" +
                            $"{emojiEstadoSergipe} - Sergipe (SE)\n" +
                            $"{emojiEstadoAlagoas} - Alagoas (AL)\n" +
                            $"{emojiEstadoBahia} - Bahia (BA)\n\n" +
                            $"{emojiVoltar} - Voltar para a escolha dos estados")
                            .WithFooter("Você tem 1 minuto pra escolher!")
                            .WithThumbnailUrl(ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        DiscordMessage msgEmbedEstadoReact = await pvMembro.SendMessageAsync(embed: embed.Build());
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoMaranhao);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoPiaui);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoCeara);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoRioGrandeDoNorte);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoPernambuco);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoParaiba);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoSergipe);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoAlagoas);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoBahia);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiVoltar);
                        await Task.Delay(200);

                        var emojiEstadoDoNordeste = (await interact.WaitForReactionAsync(msgEmbedEstadoReact, ctx.User, TimeSpan.FromMinutes(1))).Result.Emoji;

                        if (emojiEstadoDoNordeste == emojiEstadoMaranhao)
                            respostaFinalEstadoOndeOMembroMora = "MA";
                        else if (emojiEstadoDoNordeste == emojiEstadoPiaui)
                            respostaFinalEstadoOndeOMembroMora = "PI";
                        else if (emojiEstadoDoNordeste == emojiEstadoCeara)
                            respostaFinalEstadoOndeOMembroMora = "CE";
                        else if (emojiEstadoDoNordeste == emojiEstadoRioGrandeDoNorte)
                            respostaFinalEstadoOndeOMembroMora = "RN";
                        else if (emojiEstadoDoNordeste == emojiEstadoPernambuco)
                            respostaFinalEstadoOndeOMembroMora = "PE";
                        else if (emojiEstadoDoNordeste == emojiEstadoParaiba)
                            respostaFinalEstadoOndeOMembroMora = "PB";
                        else if (emojiEstadoDoNordeste == emojiEstadoSergipe)
                            respostaFinalEstadoOndeOMembroMora = "SE";
                        else if (emojiEstadoDoNordeste == emojiEstadoAlagoas)
                            respostaFinalEstadoOndeOMembroMora = "AL";
                        else if (emojiEstadoDoNordeste == emojiEstadoBahia)
                            respostaFinalEstadoOndeOMembroMora = "BA";
                        else if (emojiEstadoDoNordeste == emojiVoltar)
                        {
                            await msgEmbedEstadoReact.DeleteAsync();
                            goto botaoVoltarEscolhaRegiao;
                        }
                        else
                            respostaFinalEstadoOndeOMembroMora = "Não especificado.";
                    }
                    else if (emojiDoEstado == emojiRegiaoCentroOeste) 
                    {
                        await msgEmbedRegiaoReact.DeleteAsync();

                        var emojiEstadoMatoGrosso = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_MATO_GROSSO"));
                        var emojiEstadoMatoGrossoDoSul = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_MATO_GROSSO_DO_SUL"));
                        var emojiEstadoGoias = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_GOIAS"));
                        var emojiEstadoDistritoFederal = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_DISTRITO_FEDERAL"));

                        embed.WithAuthor("Clique no estado onde você mora.", null, Valores.logoUBGE)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithDescription($"{emojiEstadoMatoGrosso} - Mato Grosso (MT)\n" +
                            $"{emojiEstadoMatoGrossoDoSul} - Mato Grosso do Sul (MS)\n" +
                            $"{emojiEstadoGoias} - Goiás (GO)\n" +
                            $"{emojiEstadoDistritoFederal} - Distrito Federal (DF)\n\n" +
                            $"{emojiVoltar} - Voltar para a escolha dos estados")
                            .WithFooter("Você tem 1 minuto pra escolher!")
                            .WithThumbnailUrl(ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        DiscordMessage msgEmbedEstadoReact = await pvMembro.SendMessageAsync(embed: embed.Build());
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoMatoGrosso);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoMatoGrossoDoSul);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoGoias);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoDistritoFederal);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiVoltar);
                        await Task.Delay(200);

                        var emojiEstadoDoCentroOeste = (await interact.WaitForReactionAsync(msgEmbedEstadoReact, ctx.User, TimeSpan.FromMinutes(1))).Result.Emoji;

                        if (emojiEstadoDoCentroOeste == emojiEstadoMatoGrosso)
                            respostaFinalEstadoOndeOMembroMora = "MT";
                        else if (emojiEstadoDoCentroOeste == emojiEstadoMatoGrossoDoSul)
                            respostaFinalEstadoOndeOMembroMora = "MS";
                        else if (emojiEstadoDoCentroOeste == emojiEstadoGoias)
                            respostaFinalEstadoOndeOMembroMora = "GO";
                        else if (emojiEstadoDoCentroOeste == emojiEstadoDistritoFederal)
                            respostaFinalEstadoOndeOMembroMora = "DF";
                        else if (emojiEstadoDoCentroOeste == emojiVoltar)
                        {
                            await msgEmbedEstadoReact.DeleteAsync();
                            goto botaoVoltarEscolhaRegiao;
                        }
                        else
                            respostaFinalEstadoOndeOMembroMora = "Não especificado.";
                    }
                    else if (emojiDoEstado == emojiRegiaoSudeste)
                    {
                        await msgEmbedRegiaoReact.DeleteAsync();

                        var emojiEstadoSaoPaulo = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_SAO_PAULO"));
                        var emojiEstadoRioDeJaneiro = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_RIO_DE_JANEIRO"));
                        var emojiEstadoEspiritoSanto = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_ESPIRITO_SANTO"));
                        var emojiEstadoMinasGerais = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_MINAS_GERAIS"));

                        embed.WithAuthor("Clique no estado onde você mora.", null, Valores.logoUBGE)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithDescription($"{emojiEstadoSaoPaulo} - São Paulo (SP)\n" +
                            $"{emojiEstadoRioDeJaneiro} - Rio de Janeiro (RJ)\n" +
                            $"{emojiEstadoEspiritoSanto} - Espírito Santo (ES)\n" +
                            $"{emojiEstadoMinasGerais} - Minas Gerais (MG)\n\n" +
                            $"{emojiVoltar} - Voltar para a escolha dos estados")
                            .WithFooter("Você tem 1 minuto pra escolher!")
                            .WithThumbnailUrl(ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        DiscordMessage msgEmbedEstadoReact = await pvMembro.SendMessageAsync(embed: embed.Build());
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoSaoPaulo);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoRioDeJaneiro);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoEspiritoSanto);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoMinasGerais);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiVoltar);
                        await Task.Delay(200);

                        var emojiEstadoDoSudeste = (await interact.WaitForReactionAsync(msgEmbedEstadoReact, ctx.User, TimeSpan.FromMinutes(1))).Result.Emoji;

                        if (emojiEstadoDoSudeste == emojiEstadoSaoPaulo)
                            respostaFinalEstadoOndeOMembroMora = "SP";
                        else if (emojiEstadoDoSudeste == emojiEstadoRioDeJaneiro)
                            respostaFinalEstadoOndeOMembroMora = "RJ";
                        else if (emojiEstadoDoSudeste == emojiEstadoEspiritoSanto)
                            respostaFinalEstadoOndeOMembroMora = "ES";
                        else if (emojiEstadoDoSudeste == emojiEstadoMinasGerais)
                            respostaFinalEstadoOndeOMembroMora = "MG";
                        else if (emojiEstadoDoSudeste == emojiVoltar)
                        {
                            await msgEmbedEstadoReact.DeleteAsync();
                            goto botaoVoltarEscolhaRegiao;
                        }
                        else
                            respostaFinalEstadoOndeOMembroMora = "Não especificado.";
                    }
                    else if (emojiDoEstado == emojiRegiaoSul)
                    {
                        await msgEmbedRegiaoReact.DeleteAsync();

                        var emojiEstadoParana = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_PARANA"));
                        var emojiEstadoRioGrandeDoSul = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_RIO_GRANDE_DO_SUL"));
                        var emojiEstadoSantaCatarina = listaEmojis.Find(x => x.Name.ToUpper().Contains("BANDEIRA_SANTA_CATARINA"));

                        embed.WithAuthor("Clique no estado onde você mora.", null, Valores.logoUBGE)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithDescription($"{emojiEstadoParana} - Paraná (PR)\n" +
                            $"{emojiEstadoRioGrandeDoSul} - Rio Grande do Sul (RS)\n" +
                            $"{emojiEstadoSantaCatarina} - Santa Catarina (SC\n\n" +
                            $"{emojiVoltar} - Voltar para a escolha dos estados")
                            .WithFooter("Você tem 1 minuto pra escolher!")
                            .WithThumbnailUrl(ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        DiscordMessage msgEmbedEstadoReact = await pvMembro.SendMessageAsync(embed: embed.Build());
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoParana);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoRioGrandeDoSul);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiEstadoSantaCatarina);
                        await Task.Delay(200);
                        await msgEmbedEstadoReact.CreateReactionAsync(emojiVoltar);
                        await Task.Delay(200);

                        var emojiEstadoDoSul = (await interact.WaitForReactionAsync(msgEmbedEstadoReact, ctx.User, TimeSpan.FromMinutes(1))).Result.Emoji;

                        if (emojiEstadoDoSul == emojiEstadoParana)
                            respostaFinalEstadoOndeOMembroMora = "SP";
                        else if (emojiEstadoDoSul == emojiEstadoRioGrandeDoSul)
                            respostaFinalEstadoOndeOMembroMora = "RJ";
                        else if (emojiEstadoDoSul == emojiEstadoSantaCatarina)
                            respostaFinalEstadoOndeOMembroMora = "ES";
                        else if (emojiEstadoDoSul == emojiVoltar)
                        {
                            await msgEmbedEstadoReact.DeleteAsync();
                            goto botaoVoltarEscolhaRegiao;
                        }
                        else
                            respostaFinalEstadoOndeOMembroMora = "Não especificado.";
                    }
                    else if (emojiDoEstado == emojiRegiaoForaDoBrasil)
                    {
                        await msgEmbedRegiaoReact.DeleteAsync();

                        embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithAuthor($"Digite o país em que você mora.", null, Valores.logoUBGE);

                        DiscordMessage perguntaPaisExterior = await pvMembro.SendMessageAsync(embed: embed.Build());
                        DiscordMessage inputPaisExterior = await new UtilidadesGerais().PegaRespostaPrivado(interact, ctx);

                        respostaFinalEstadoOndeOMembroMora = inputPaisExterior.Content;
                    }

                    Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                    embed.WithAuthor("Digite seu endereço de email.", null, Valores.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                    DiscordMessage perguntaEmail = await pvMembro.SendMessageAsync(embed: embed.Build());
                    DiscordMessage inputEmail = await new UtilidadesGerais().PegaRespostaPrivado(interact, ctx), inputEmail_ = null;

                    if (!inputEmail.Content.Contains("@"))
                    {
                        inputEmail = null;

                        Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                        embed.WithAuthor("Digite um email válido!", null, Valores.logoUBGE)
                            .WithColor(DiscordColor.Red);

                        while (true)
                        {
                            DiscordMessage perguntaEmail_ = await pvMembro.SendMessageAsync(embed: embed.Build());
                            inputEmail_ = await Program.ubgeBot.utilidadesGerais.PegaRespostaPrivado(interact, ctx);

                            if (!inputEmail_.Content.Contains("@"))
                                continue;
                            else
                                break;
                        }
                    }

                    Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                    embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithAuthor("Digite os idiomas você fala, (Ex: Português, Inglês - [Fluentemente], Inglês - [Mediano])\n" +
                        "Pode ser vários idiomas.", null, Valores.logoUBGE);

                    DiscordMessage perguntaIdiomas = await pvMembro.SendMessageAsync(embed: embed.Build());
                    DiscordMessage inputIdiomas = await new UtilidadesGerais().PegaRespostaPrivado(interact, ctx);

                    Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                    var youtubeEmoji = listaEmojis.Find(x => x.Name.ToUpper().Contains("YOUTUBELOGO"));
                    var discordPublicos = listaEmojis.Find(x => x.Name.ToUpper().Contains("DISCORDLOGO"));
                    var siteEmoji = listaEmojis.Find(x => x.Name.ToUpper().Contains("SITELOGO"));
                    var redeSociaisEmoji = listaEmojis.Find(x => x.Name.ToUpper().Contains("REDESOCIAISLOGO"));
                    var steamEmoji = listaEmojis.Find(x => x.Name.ToUpper().Contains("STEAMLOGO"));
                    var amigosEmoji = listaEmojis.Find(x => x.Name.ToUpper().Contains("AMIGOSLOGO"));
                    var jogosEmoji = listaEmojis.Find(x => x.Name.ToUpper().Contains("GOOGLEPLAYLOGO"));
                    var parceirosEmoji = listaEmojis.Find(x => x.Name.ToUpper().Contains("PARCEIROS"));
                    var servidoresUBGEEmoji = listaEmojis.Find(x => x.Name.ToUpper().Contains("NOSSOS_SERVIDORES"));
                    var outrasFormas = DiscordEmoji.FromName(ctx.Client, ":earth_americas:");

                    embed.WithAuthor("Como chegou a UBGE? Clique na reação que lhe trouxe até aqui.", null, Valores.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithDescription($"{youtubeEmoji} - Canal do Youtube\n" +
                        $"{discordPublicos} - Discords Públicos\n" +
                        $"{siteEmoji} - Site\n" +
                        $"{redeSociaisEmoji} - Redes sociais\n" +
                        $"{steamEmoji} - Steam\n" +
                        $"{amigosEmoji} - Amigos\n" +
                        $"{jogosEmoji} - Contatos em jogos\n" +
                        $"{parceirosEmoji} - Parceiros da UBGE\n" +
                        $"{servidoresUBGEEmoji} - Nossos Servidores\n\n" +
                        $"{outrasFormas} - Outras formas")
                        .WithTimestamp(DateTime.Now)
                        .WithFooter("Você tem 1 minuto para responder!")
                        .WithThumbnailUrl(ctx.Member.AvatarUrl);

                    DiscordMessage msgEmbedChegouUBGE = await pvMembro.SendMessageAsync(embed: embed.Build());
                    await msgEmbedChegouUBGE.CreateReactionAsync(youtubeEmoji);
                    await Task.Delay(200);
                    await msgEmbedChegouUBGE.CreateReactionAsync(discordPublicos);
                    await Task.Delay(200);
                    await msgEmbedChegouUBGE.CreateReactionAsync(siteEmoji);
                    await Task.Delay(200);
                    await msgEmbedChegouUBGE.CreateReactionAsync(redeSociaisEmoji);
                    await Task.Delay(200);
                    await msgEmbedChegouUBGE.CreateReactionAsync(steamEmoji);
                    await Task.Delay(200);
                    await msgEmbedChegouUBGE.CreateReactionAsync(amigosEmoji);
                    await Task.Delay(200);
                    await msgEmbedChegouUBGE.CreateReactionAsync(jogosEmoji);
                    await Task.Delay(200);
                    await msgEmbedChegouUBGE.CreateReactionAsync(parceirosEmoji);
                    await Task.Delay(200);
                    await msgEmbedChegouUBGE.CreateReactionAsync(servidoresUBGEEmoji);
                    await Task.Delay(200);
                    await msgEmbedChegouUBGE.CreateReactionAsync(outrasFormas);
                    await Task.Delay(200);

                    var emojiComoChegouAUBGE = (await interact.WaitForReactionAsync(msgEmbedChegouUBGE, ctx.User, TimeSpan.FromMinutes(1))).Result.Emoji;

                    if (emojiComoChegouAUBGE == youtubeEmoji)
                        respostaFinalComoChegouAUBGE = "Canal do Youtube";
                    else if (emojiComoChegouAUBGE == discordPublicos)
                        respostaFinalComoChegouAUBGE = "Discords Públicos";
                    else if (emojiComoChegouAUBGE == siteEmoji)
                        respostaFinalComoChegouAUBGE = "Site";
                    else if (emojiComoChegouAUBGE == redeSociaisEmoji)
                        respostaFinalComoChegouAUBGE = "Redes sociais";
                    else if (emojiComoChegouAUBGE == steamEmoji)
                        respostaFinalComoChegouAUBGE = "Steam";
                    else if (emojiComoChegouAUBGE == amigosEmoji)
                        respostaFinalComoChegouAUBGE = "Amigos";
                    else if (emojiComoChegouAUBGE == jogosEmoji)
                        respostaFinalComoChegouAUBGE = "Contatos em jogos";
                    else if (emojiComoChegouAUBGE == parceirosEmoji)
                        respostaFinalComoChegouAUBGE = "Parceiros";
                    else if (emojiComoChegouAUBGE == servidoresUBGEEmoji)
                        respostaFinalComoChegouAUBGE = "Nossos Servidores";
                    else if (emojiComoChegouAUBGE == outrasFormas)
                        respostaFinalComoChegouAUBGE = "Outras formas";
                    else
                        respostaFinalComoChegouAUBGE = "Não especificado.";

                    Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                    var pr = listaEmojis.Find(x => x.Name.ToUpper().Contains("PR"));
                    var foxhole = listaEmojis.Find(x => x.Name.ToUpper().Contains("FOXHOLE"));
                    var battlefield = listaEmojis.Find(x => x.Name.ToUpper().Contains("BF"));
                    var openSpades = listaEmojis.Find(x => x.Name.ToUpper() == "OS");
                    var squad = listaEmojis.Find(x => x.Name.ToUpper().Contains("SQUAD"));
                    var minecraft = listaEmojis.Find(x => x.Name.ToUpper().Contains("MINECRAFT"));
                    var unturned = listaEmojis.Find(x => x.Name.ToUpper().Contains("UNTURNED"));
                    var gmod = listaEmojis.Find(x => x.Name.ToUpper().Contains("GMOD"));
                    var warThunder = listaEmojis.Find(x => x.Name.ToUpper().Contains("WARTHUNDER"));
                    var pubg = listaEmojis.Find(x => x.Name.ToUpper().Contains("PUBG"));
                    var lol = listaEmojis.Find(x => x.Name.ToUpper().Contains("LOL"));
                    var rust = listaEmojis.Find(x => x.Name.ToUpper().Contains("RUST"));
                    var csgo = listaEmojis.Find(x => x.Name.ToUpper().Contains("CSGO"));
                    var paladins = listaEmojis.Find(x => x.Name.ToUpper().Contains("PALADINS"));
                    var wildTerra = listaEmojis.Find(x => x.Name.ToUpper().Contains("WILDTERRA"));
                    var lif = listaEmojis.Find(x => x.Name.ToUpper().Contains("LIF"));
                    var battleRush = listaEmojis.Find(x => x.Name.ToUpper().Contains("BATTLERUSH"));
                    var heroesGenerals = listaEmojis.Find(x => x.Name.ToUpper().Contains("HG"));
                    var albionOnline = listaEmojis.Find(x => x.Name.ToUpper().Contains("ALBIONONLINE"));
                    var rts = listaEmojis.Find(x => x.Name.ToUpper().Contains("RTS"));

                    embed.WithAuthor("Clique nos seus jogos mais jogados.", null, Valores.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithDescription($"{pr} - Project Reality\n" +
                        $"{foxhole} - Foxhole\n" +
                        $"{battlefield} - Battlefields\n" +
                        $"{openSpades} - OpenSpades/Ace of Spades\n" +
                        $"{squad} - Squad\n" +
                        $"{minecraft} - Minecraft\n" +
                        $"{unturned} - Unturned\n" +
                        $"{gmod} - Garry's Mod\n" +
                        $"{warThunder} - War Thunder\n" +
                        $"{pubg} - Playerunknown's Battlegrounds\n" +
                        $"{lol} - League Of Legends\n" +
                        $"{rust} - Rust\n" +
                        $"{csgo} - Counter Strike: Global Offensive\n" +
                        $"{paladins} - Paladins\n" +
                        $"{wildTerra} - Wild Terra Online\n" +
                        $"{lif} - Life is Feudal\n" +
                        $"{battleRush} - BattleRush\n" +
                        $"{heroesGenerals} - Heroes&Generals\n" +
                        $"{albionOnline} - Albion Online\n" +
                        $"{rts} - RTS's (Jogos de Estratégia)")
                        .WithTimestamp(DateTime.Now)
                        .WithFooter("Você tem 30 segundos para responder!")
                        .WithThumbnailUrl(ctx.Member.AvatarUrl);

                    DiscordMessage msgEmbedPerguntaJogo = await pvMembro.SendMessageAsync(embed: embed.Build());
                    await msgEmbedPerguntaJogo.CreateReactionAsync(pr);
                    await Task.Delay(200);
                    await msgEmbedPerguntaJogo.CreateReactionAsync(foxhole);
                    await Task.Delay(200);
                    await msgEmbedPerguntaJogo.CreateReactionAsync(battlefield);
                    await Task.Delay(200);
                    await msgEmbedPerguntaJogo.CreateReactionAsync(openSpades);
                    await Task.Delay(200);
                    await msgEmbedPerguntaJogo.CreateReactionAsync(squad);
                    await Task.Delay(200);
                    await msgEmbedPerguntaJogo.CreateReactionAsync(minecraft);
                    await Task.Delay(200);
                    await msgEmbedPerguntaJogo.CreateReactionAsync(unturned);
                    await Task.Delay(200);
                    await msgEmbedPerguntaJogo.CreateReactionAsync(gmod);
                    await Task.Delay(200);
                    await msgEmbedPerguntaJogo.CreateReactionAsync(warThunder);
                    await Task.Delay(200);
                    await msgEmbedPerguntaJogo.CreateReactionAsync(pubg);
                    await Task.Delay(200);
                    await msgEmbedPerguntaJogo.CreateReactionAsync(lol);
                    await Task.Delay(200);
                    await msgEmbedPerguntaJogo.CreateReactionAsync(rust);
                    await Task.Delay(200);
                    await msgEmbedPerguntaJogo.CreateReactionAsync(csgo);
                    await Task.Delay(200);
                    await msgEmbedPerguntaJogo.CreateReactionAsync(paladins);
                    await Task.Delay(200);
                    await msgEmbedPerguntaJogo.CreateReactionAsync(wildTerra);
                    await Task.Delay(200);
                    await msgEmbedPerguntaJogo.CreateReactionAsync(lif);
                    await Task.Delay(200);
                    await msgEmbedPerguntaJogo.CreateReactionAsync(battleRush);
                    await Task.Delay(200);
                    await msgEmbedPerguntaJogo.CreateReactionAsync(heroesGenerals);
                    await Task.Delay(200);
                    await msgEmbedPerguntaJogo.CreateReactionAsync(albionOnline);
                    await Task.Delay(200);
                    await msgEmbedPerguntaJogo.CreateReactionAsync(rts);
                    await Task.Delay(200);

                    msgEmbedPerguntaJogo = await (await ctx.Client.GetChannelAsync(msgEmbedPerguntaJogo.Channel.Id)).GetMessageAsync(msgEmbedPerguntaJogo.Id);

                    await Task.Delay(TimeSpan.FromSeconds(30));

                    Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                    embed.WithAuthor($"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}, obrigado por responder o Censo Comunitário!", null, ctx.Member.AvatarUrl)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithImageUrl(Valores.logoUBGE)
                            .WithTimestamp(DateTime.Now);

                    await pvMembro.SendMessageAsync(embed: embed.Build());

                    var horaDiaCensoFeito = DateTime.Now;
                    
                    StringBuilder strReacoes = new StringBuilder();

                    foreach (var reacoesMensagem in msgEmbedPerguntaJogo.Reactions)
                    {
                        await Task.Delay(200);

                        foreach (var membroReagiu in await msgEmbedPerguntaJogo.GetReactionsAsync(reacoesMensagem.Emoji))
                        {
                            if (membroReagiu.Id == ctx.User.Id)
                                strReacoes.Append($"{RetornaNomeDoJogo(reacoesMensagem.Emoji.Name)}, ");
                        }
                    }

                    respostaFinalJogosMaisJogados = strReacoes.ToString();

                    if (string.IsNullOrWhiteSpace(respostaFinalJogosMaisJogados))
                        respostaFinalJogosMaisJogados = "Não especificado.";

                    if (respostaFinalJogosMaisJogados.EndsWith(", "))
                        respostaFinalJogosMaisJogados = respostaFinalJogosMaisJogados.Remove(respostaFinalJogosMaisJogados.Length - 2);

                    if (inputIdade == null)
                        respostaIdadeFinal = int.Parse(inputIdade_.Content);
                    else
                        respostaIdadeFinal = int.Parse(inputIdade.Content);

                    if (inputEmail == null)
                        respostaFinalEmail = inputEmail_.Content;
                    else
                        respostaFinalEmail = inputEmail.Content;

                    if (censoDenovo)
                    {
                        await collectionCenso.DeleteOneAsync(filtro);

                        await collectionCenso.InsertOneAsync(new Censo
                        {
                            emailMembro = respostaFinalEmail,
                            chegouNaUBGE = respostaFinalComoChegouAUBGE,
                            idade = respostaIdadeFinal.ToString(),
                            jogosMaisJogados = respostaFinalJogosMaisJogados,
                            idiomas = inputIdiomas.Content,
                            estado = respostaFinalEstadoOndeOMembroMora,
                            timestamp = horaDiaCensoFeito.ToString(),
                            idNoDiscord = ctx.Member.Id,
                            fezOCenso = true,
                        });

                        await Program.ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.Aviso, $"O membro: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)} acabou de fazer o censo e foi inserido no Mongo!", ":warning:", ctx.Member.AvatarUrl, ctx.User);

                        await apiGoogle.EscrevePlanilhaDoCenso(Program.ubgeBot.ubgeBotConfig.ubgeBotGoogleAPIConfig.censoID, Program.ubgeBot.ubgeBotConfig.ubgeBotGoogleAPIConfig.censoRange, horaDiaCensoFeito.ToOADate(),
                        ctx.Member.Id.ToString(), respostaIdadeFinal, respostaFinalEstadoOndeOMembroMora, respostaFinalEmail, inputIdiomas.Content, respostaFinalComoChegouAUBGE, respostaFinalJogosMaisJogados);

                        await Program.ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.Aviso, $"O membro: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)} acabou de fazer o censo e também foi inserido na planilha do Google!", ":warning:", ctx.Member.AvatarUrl, ctx.User);

                        return;
                    }

                    await collectionCenso.InsertOneAsync(new Censo
                    {
                        emailMembro = respostaFinalEmail,
                        chegouNaUBGE = respostaFinalComoChegouAUBGE,
                        idade = respostaIdadeFinal.ToString(),
                        jogosMaisJogados = respostaFinalJogosMaisJogados,
                        idiomas = inputIdiomas.Content,
                        estado = respostaFinalEstadoOndeOMembroMora,
                        timestamp = horaDiaCensoFeito.ToString(),
                        idNoDiscord = ctx.Member.Id,
                        fezOCenso = true,
                    });

                    await Program.ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.Aviso, $"O membro: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)} acabou de fazer o censo e foi inserido no Mongo!", ":warning:", ctx.Member.AvatarUrl, ctx.User);

                    await apiGoogle.EscrevePlanilhaDoCenso(Program.ubgeBot.ubgeBotConfig.ubgeBotGoogleAPIConfig.censoID, Program.ubgeBot.ubgeBotConfig.ubgeBotGoogleAPIConfig.censoRange, horaDiaCensoFeito.ToOADate(),
                        ctx.Member.Id.ToString(), respostaIdadeFinal, respostaFinalEstadoOndeOMembroMora, respostaFinalEmail, inputIdiomas.Content, respostaFinalComoChegouAUBGE, respostaFinalJogosMaisJogados);

                    await Program.ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.Aviso, $"O membro: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)} acabou de fazer o censo e também foi inserido na planilha do Google!", ":warning:", ctx.Member.AvatarUrl, ctx.User);
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        public string RetornaNomeDoJogo(string nomeEmoji)
        {
            if (nomeEmoji.ToUpper() == "PR")
                return "Project Reality";
            else if (nomeEmoji.ToUpper() == "FOXHOLE")
                return "Foxhole";
            else if (nomeEmoji.ToUpper() == "BF")
                return "Battlefields";
            else if (nomeEmoji.ToUpper() == "OS")
                return "Open Spades/Ace of Spades";
            else if (nomeEmoji.ToUpper() == "SQUAD")
                return "Squad";
            else if (nomeEmoji.ToUpper() == "MINECRAFT")
                return "Minecraft";
            else if (nomeEmoji.ToUpper() == "UNTURNED")
                return "Unturned";
            else if (nomeEmoji.ToUpper() == "GMOD")
                return "Gmod";
            else if (nomeEmoji.ToUpper() == "WARTHUNDER")
                return "War Thunder";
            else if (nomeEmoji.ToUpper() == "PUBG")
                return "PUBG";
            else if (nomeEmoji.ToUpper() == "LOL")
                return "LoL";
            else if (nomeEmoji.ToUpper() == "RUST")
                return "Rust";
            else if (nomeEmoji.ToUpper() == "CSGO")
                return "CS:GO";
            else if (nomeEmoji.ToUpper() == "PALADINS")
                return "Paladins";
            else if (nomeEmoji.ToUpper() == "WILDTERRA")
                return "Wild Terra Online";
            else if (nomeEmoji.ToUpper() == "BATTLERUSH")
                return "BattleRush";
            else if (nomeEmoji.ToUpper() == "HG")
                return "Heroes&Generals";
            else if (nomeEmoji.ToUpper() == "RTS")
                return "RTSs (Jogos de estratégia)";
            else if (nomeEmoji.ToUpper() == "ALBIONONLINE")
                return "Albion Online";
            else
                return "Não especificado.";
        }

        [Command("avatar"), Aliases("foto")]

        public async Task AvatarMembroAsync(CommandContext ctx, DiscordMember membro = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder Embed = new DiscordEmbedBuilder();

                    if (membro == null)
                    {
                        Embed.WithAuthor($"Avatar do membro: \"{ctx.Member.Username}\"", null, Valores.logoUBGE)
                            .WithImageUrl(ctx.Member.GetAvatarUrl(ImageFormat.Png, 2048))
                            .WithDescription($"Para baixar, {Formatter.MaskedUrl("clique aqui", new Uri(ctx.Member.GetAvatarUrl(ImageFormat.Png, 2048)), "clique aqui")}.")
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: Embed.Build(), content: ctx.Member.Mention);
                    }
                    else
                    {
                        Embed.WithAuthor($"Avatar do membro: \"{membro.Username}\"", null, Valores.logoUBGE)
                            .WithImageUrl(membro.GetAvatarUrl(ImageFormat.Png, 2048))
                            .WithDescription($"Para baixar, {Formatter.MaskedUrl("clique aqui", new Uri(membro.GetAvatarUrl(ImageFormat.Png, 2048)), "clique aqui")}.")
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: Embed.Build(), content: ctx.Member.Mention);
                    }
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("userinfo"), Aliases("usuario", "usuário")]

        public async Task UserInfoAsync(CommandContext ctx, DiscordMember membro = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();
                    DiscordRole cargoMembroRegistrado = ctx.Guild.GetRole(Valores.Cargos.cargoMembroRegistrado);
                    
                    string status = string.Empty, statusDiscord = string.Empty, statusFinal = string.Empty;
                    
                    StringBuilder str = new StringBuilder();

                    if (membro == null)
                    {
                        status = $"{await Program.ubgeBot.utilidadesGerais.ConverteStatusParaEmoji(ctx, ctx.Member)}- {Program.ubgeBot.utilidadesGerais.ConverteStatusPraNome(ctx.Member)}";
                        statusDiscord = Program.ubgeBot.utilidadesGerais.ConverteStatusPraNome(ctx.Member);

                        if (statusDiscord != "Offline" && ctx.Member.Presence != null)
                        {
                            statusDiscord = string.Empty;

                            if (ctx.Member.Presence.Activity.ActivityType == ActivityType.ListeningTo)
                                statusDiscord += $"Escutando no {ctx.Member.Presence.Activity.Name} a música: " +
                                    $"**{ctx.Member.Presence.Activity.RichPresence.Details}** de **{ctx.Member.Presence.Activity.RichPresence.State}**, " +
                                    $"do álbum: **{ctx.Member.Presence.Activity.RichPresence.LargeImageText}**";
                            else if (ctx.Member.Presence.Activity.ActivityType == ActivityType.Playing)
                                statusDiscord += $"Jogando: **{(string.IsNullOrWhiteSpace(ctx.Member.Presence.Activity.Name) ? "Nada no momento." : ctx.Member.Presence.Activity.Name)}**";
                            else if (ctx.Member.Presence.Activity.ActivityType == ActivityType.Streaming)
                                statusDiscord += $"Streamando: **{(string.IsNullOrWhiteSpace(ctx.Member.Presence.Activity.Name) ? "Nada no momento." : ctx.Member.Presence.Activity.Name)}**";
                            else if (ctx.Member.Presence.Activity.ActivityType == ActivityType.Watching)
                                statusDiscord += $"Assistindo: **{(string.IsNullOrWhiteSpace(ctx.Member.Presence.Activity.Name) ? "Nada no momento." : ctx.Member.Presence.Activity.Name)}**";
                        }

                        foreach (var cargosForeach in ctx.Member.Roles.OrderByDescending(x => x.Position))
                            str.Append($"{cargosForeach.Mention} | ");

                        str.Append($"\n\n**{ctx.Member.Roles.Count()}** cargos ao total.");

                        if (str.Length > 1024)
                        {
                            str.Clear();
                            str.Append("Os cargos excederam o limite de 1024 caracteres.");
                        }

                        embed.WithAuthor($"Informações do membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}\"", null, Valores.logoUBGE)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .AddField("Conta criada no dia:", $"{ctx.Member.CreationTimestamp.DateTime.ToString()} - Há {(int)(DateTime.Now - ctx.Member.CreationTimestamp.DateTime).TotalDays} dias")
                            .AddField($"Entrou na {ctx.Guild.Name} no dia:", $"{ctx.Member.JoinedAt.DateTime.ToString()} - Há {(int)(DateTime.Now - ctx.Member.JoinedAt.DateTime).TotalDays} dias")
                            .AddField("Cargos:", $"{(str.ToString() == "Os cargos excederam o limite de 1024 caracteres." ? $"{str.ToString()} Mas o membro tem {ctx.Member.Roles.Count()} cargos." : str.ToString())}")
                            .AddField("Sala de Voz:", ctx.Member.VoiceState == null ? "Este membro não está em nenhum canal de voz." : ctx.Member.VoiceState.Channel.Name)
                            .AddField("Membro Registrado?:", ctx.Member.Roles.Contains(cargoMembroRegistrado) ? "Sim" : "Não")
                            .AddField("Status atual:", $"{status}\n\n" +
                            $"{(statusDiscord == "Offline" ? statusDiscord = string.Empty : statusDiscord)}")
                            .AddField("Dono do servidor?:", ctx.Member.IsOwner ? $":crown: - Sim" : "Não")
                            .AddField("Bot?:", ctx.Member.IsBot ? $"{await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "bot")} - Sim" : "Não")
                            .WithThumbnailUrl(ctx.Member.AvatarUrl)
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                    else
                    {
                        status = $"{await Program.ubgeBot.utilidadesGerais.ConverteStatusParaEmoji(ctx, membro)}- {Program.ubgeBot.utilidadesGerais.ConverteStatusPraNome(membro)}";
                        statusDiscord = Program.ubgeBot.utilidadesGerais.ConverteStatusPraNome(membro);

                        if (statusDiscord != "Offline" && membro.Presence != null)
                        {
                            statusDiscord = string.Empty;

                            if (membro.Presence.Activity.ActivityType == ActivityType.ListeningTo)
                                statusDiscord += $"Escutando no {membro.Presence.Activity.Name} a música: " +
                                    $"**{membro.Presence.Activity.RichPresence.Details}** de **{membro.Presence.Activity.RichPresence.State}**, " +
                                    $"do álbum: **{membro.Presence.Activity.RichPresence.LargeImageText}**";
                            else if (membro.Presence.Activity.ActivityType == ActivityType.Playing)
                                statusDiscord += $"Jogando: **{(string.IsNullOrWhiteSpace(membro.Presence.Activity.Name) ? "Nada no momento." : membro.Presence.Activity.Name)}**";
                            else if (membro.Presence.Activity.ActivityType == ActivityType.Streaming)
                                statusDiscord += $"Streamando: **{(string.IsNullOrWhiteSpace(membro.Presence.Activity.Name) ? "Nada no momento." : membro.Presence.Activity.Name)}**";
                            else if (membro.Presence.Activity.ActivityType == ActivityType.Watching)
                                statusDiscord += $"Assistindo: **{(string.IsNullOrWhiteSpace(membro.Presence.Activity.Name) ? "Nada no momento." : membro.Presence.Activity.Name)}**";
                        }

                        foreach (var cargosForeach in membro.Roles.OrderByDescending(x => x.Position))
                            str.Append($"{cargosForeach.Mention} | ");

                        str.Append($"\n\n**{membro.Roles.Count()} cargos ao total.**");

                        if (str.Length > 1024)
                        {
                            str.Clear();
                            str.Append("Os cargos excederam o limite de 1024 caracteres.");
                        }

                        embed.WithAuthor($"Informações do membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}\"", null, Valores.logoUBGE)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .AddField("Conta criada no dia:", $"{membro.CreationTimestamp.DateTime.ToString()} - Há {(int)(DateTime.Now - membro.CreationTimestamp.DateTime).TotalDays} dias")
                            .AddField($"Entrou na {ctx.Guild.Name} no dia:", $"{membro.JoinedAt.DateTime.ToString()} - Há {(int)(DateTime.Now - membro.JoinedAt.DateTime).TotalDays} dias")
                            .AddField("Cargos:", $"{(str.ToString() == "Os cargos excederam o limite de 1024 caracteres." ? $"{str.ToString()} Mas o membro tem {membro.Roles.Count()} cargos." : str.ToString())}")
                            .AddField("Sala de Voz:", membro.VoiceState == null ? "Este membro não está em nenhum canal de voz." : membro.VoiceState.Channel.Name)
                            .AddField("Membro Registrado?:", membro.Roles.Contains(cargoMembroRegistrado) ? "Sim" : "Não")
                            .AddField("Status atual:", $"{status}\n\n" +
                            $"{(statusDiscord == "Não especificado." ? statusDiscord = string.Empty : statusDiscord)}")
                            .AddField("Dono do servidor?:", membro.IsOwner ? $":crown: - Sim" : "Não")
                            .AddField("Bot?:", membro.IsBot ? $"{await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "bot")} - Sim" : "Não")
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("dólar"), Aliases("dolar")]

        public async Task CotacaoDolarAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    var Json = await Program.httpClientMain.GetStringAsync("https://api.hgbrasil.com/finance/quotations?format=json&key=27044a14");
                    var Resposta = (JObject)JsonConvert.DeserializeObject(Json);
                    
                    var Resultados = Resposta.SelectToken("results");
                    var Currencies = Resultados.SelectToken("currencies");
                    var Dolar = Currencies.SelectToken("USD");
                    var ValorDolar = Dolar.SelectToken("buy");

                    DiscordEmbedBuilder Embed = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Valor do Dólar às: \"{DateTime.Now.ToString()}\": ${ValorDolar} ou ${Math.Round(double.Parse(ValorDolar.ToString()), 2)}", IconUrl = Valores.logoUBGE },
                        Color = Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed(),
                    };

                    await ctx.RespondAsync(embed: Embed.Build());
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("euro")]

        public async Task CotacaoEuroAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    var Json = await Program.httpClientMain.GetStringAsync("https://api.hgbrasil.com/finance/quotations?format=json&key=27044a14");
                    var Resposta = (JObject)JsonConvert.DeserializeObject(Json);

                    var Resultados = Resposta.SelectToken("results");
                    var Currencies = Resultados.SelectToken("currencies");
                    var Dolar = Currencies.SelectToken("EUR");
                    var ValorDolar = Dolar.SelectToken("buy");

                    DiscordEmbedBuilder Embed = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Valor do Euro às: \"{DateTime.Now.ToString()}\": ${ValorDolar} ou ${Math.Round(double.Parse(ValorDolar.ToString()), 2)}", IconUrl = Valores.logoUBGE },
                        Color = Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed(),
                    };

                    await ctx.RespondAsync(embed: Embed.Build());
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("servidorinfo"), Aliases("guildinfo", "serverinfo")]

        public async Task ServidorInfoAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                { 
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { Name = "Informações do servidor: {ctx.Guild.Name}", IconUrl = Valores.logoUBGE },
                        ThumbnailUrl = $"{ctx.Guild.IconUrl}?size=2048",
                        Color = Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed(),
                        Timestamp = DateTime.Now,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", IconUrl = ctx.Member.AvatarUrl },
                    };

                    embed.AddField("Criado no dia:", $"{ctx.Guild.CreationTimestamp.DateTime.ToString()} - {(int)(DateTime.Now - ctx.Guild.CreationTimestamp.DateTime).TotalDays} dias")
                        .AddField("Dono:", ctx.Guild.Owner.Mention)
                        .AddField("Quantidade de membros:", $"{ctx.Guild.MemberCount} membros")
                        .AddField("Número de cargos:", $"{ctx.Guild.Roles.Count} cargos")
                        .AddField("Cargo mais alto na hierarquia:", ctx.Guild.Roles.OrderByDescending(P => P.Value.Position).First().Value.Mention)
                        .AddField("Quantidade de canais:", $"{ctx.Guild.Channels.Count} canais")
                        .AddField("Quantidade de emojis:", $"{ctx.Guild.Emojis.Count} emojis")
                        .AddField("ID:", ctx.Guild.Id.ToString());

                    await ctx.RespondAsync(embed: embed.Build());
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("fotoservidor"), Aliases("servidorfoto", "avatarservidor", "servidoravatar", "avatarguild", "guildavatar")]

        public async Task AvatarServidorAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Foto do servidor: {ctx.Guild.Name}", IconUrl = Valores.logoUBGE },
                        ImageUrl = $"{ctx.Guild.IconUrl}?size=2048",
                        Color = Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed(),
                        Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", IconUrl = ctx.Member.AvatarUrl },
                        Timestamp = DateTime.Now,
                    };

                    await ctx.RespondAsync(embed: embed.Build());
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("procuramembros")]

        public async Task ProcuraMembrosPeloNomeDoJogo(CommandContext ctx, [RemainingText] string jogo = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    if (string.IsNullOrWhiteSpace(jogo))
                    {
                        embed.WithAuthor("Digite um nome de um jogo!", null, Valores.logoUBGE)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithDescription($"{await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "leofsjal")}")
                            .WithThumbnailUrl(ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now)
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    StringBuilder str = new StringBuilder();
                    List<string> membrosServidor = new List<string>();

                    foreach (var servidor in ctx.Client.Guilds.Values)
                    {
                        foreach (var membroForeach in servidor.Members.Values)
                        {
                            if (membroForeach.IsBot)
                                continue;

                            if (membrosServidor.Contains(membroForeach.Mention))
                                continue;

                            membrosServidor.Add(membroForeach.Mention);

                            if (membroForeach.Presence != null)
                            {
                                if (membroForeach.Presence.Activities != null)
                                {
                                    foreach (var JogoDiscord in membroForeach.Presence.Activities)
                                    {
                                        if (!string.IsNullOrWhiteSpace(JogoDiscord.Name) && JogoDiscord.Name.ToLower() == jogo.ToLower())
                                        {
                                            if (membroForeach != ctx.Member)
                                                str.Append($"{Program.ubgeBot.utilidadesGerais.MencaoMembro(membroForeach)} - {membroForeach.Guild.Name} | ");
                                        }
                                    }
                                }
                                else if (membroForeach.Presence.Activity != null)
                                {
                                    if (!string.IsNullOrWhiteSpace(membroForeach.Presence.Activity.Name) && membroForeach.Presence.Activity.Name.ToLower() == jogo.ToLower())
                                    {
                                        if (membroForeach != ctx.Member)
                                            str.Append($"{Program.ubgeBot.utilidadesGerais.MencaoMembro(membroForeach)} - {membroForeach.Guild.Name} | ");
                                    }
                                }
                            }
                        }
                    }

                    embed.WithAuthor($"Membros que estão jogando: \"{jogo}\"", null, ctx.Guild.IconUrl)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithDescription(string.IsNullOrWhiteSpace(str.ToString()) ? $"Ninguém no momento está jogando isso ou só você está jogando: \"{jogo}\"." : str.ToString())
                        .WithThumbnailUrl(ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now)
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    await ctx.RespondAsync(content: $"Eu procurei em todos os servidores que eu estou, que no caso são: `{ctx.Client.Guilds.Values.Count()}`.", embed: embed.Build());
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("listar")]

        public async Task ListarMembrosEmUmCanalDeVox(CommandContext ctx, DiscordChannel canal = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    if (canal == null)
                    {
                        embed.WithAuthor("Digite um ID de um canal!", null, Valores.logoUBGE)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithDescription(":thinking:")
                            .WithThumbnailUrl(ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now)
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    StringBuilder str = new StringBuilder();

                    if (canal.Users.Count() != 0)
                    {
                        foreach (var membro in canal.Users)
                            str.Append($"{Program.ubgeBot.utilidadesGerais.MencaoMembro(membro)}\n");
                    }

                    embed.WithAuthor($"Membros que estão no canal: \"{canal.Name}\"", null, ctx.Guild.IconUrl)
                        .WithDescription(string.IsNullOrWhiteSpace(str.ToString()) ? "Nenhum membro está nesse canal." : $"{str.ToString()}\nEsse canal contêm {(canal.Users.Count() > 1 ? $"**{canal.Users.Count()}** membros." : $"**{canal.Users.Count()}** membro.")}")
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithThumbnailUrl(ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now)
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    await ctx.RespondAsync(embed: embed.Build());
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("botslivres"), UBGE]

        public async Task BotsLivresAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordRole botsMusicais = ctx.Guild.GetRole(Valores.Cargos.botsMusicais);

                    StringBuilder str = new StringBuilder();

                    foreach (var membros in ctx.Guild.Members.Values.Where(x => x.Roles.Contains(botsMusicais)))
                    {
                        if (membros.IsBot && membros.VoiceState == null)
                            str.Append($"{Program.ubgeBot.utilidadesGerais.MencaoMembro(membros)}\n");
                    }

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { Name = "BOTs de música que estão livres:", IconUrl = Valores.logoUBGE },
                        Description = str.ToString(),
                        Color = Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed(),
                        Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", IconUrl = ctx.Member.AvatarUrl },
                        ThumbnailUrl = ctx.Member.AvatarUrl,
                    };

                    await ctx.RespondAsync(embed: embed.Build());
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("ajudabots"), UBGE]

        public async Task AjudaDeOutrosBotsAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordRole bots = ctx.Guild.GetRole(Valores.Cargos.bots), botsMusicais = ctx.Guild.GetRole(Valores.Cargos.botsMusicais);

                    StringBuilder primeiroSTR = new StringBuilder(), segundoSTR = new StringBuilder();

                    int i = 0;

                    var botsServidor = (await ctx.Guild.GetAllMembersAsync()).Where(x => x.Roles.Contains(bots));

                    List<DiscordMember> botsFinal = new List<DiscordMember>();

                    foreach (var botsCargo in botsServidor)
                    {
                        if (i == 15)
                            break;

                        if (string.IsNullOrWhiteSpace(botsCargo.Nickname))
                            continue;

                        if (!botsCargo.IsBot)
                            continue;

                        if (botsCargo.Presence == null)
                            continue;

                        if (botsCargo.Nickname.Contains("prefix"))
                            primeiroSTR.Append($"`{++i}`. {Program.ubgeBot.utilidadesGerais.MencaoMembro(botsCargo)} | {await Program.ubgeBot.utilidadesGerais.ConverteStatusParaEmoji(ctx, botsCargo)}: " +
                                $"{Program.ubgeBot.utilidadesGerais.ConverteStatusPraNome(botsCargo)} | {(botsCargo.Roles.Contains(botsMusicais) ? $"{(string.IsNullOrWhiteSpace(botsCargo.VoiceState?.Channel?.Name) ? "Este bot de música está livre." : $"Este bot é de música está no canal: **{botsCargo.VoiceState.Channel.Name}**")}" : "Este bot não é de música.")}\n");

                        botsFinal.Add(botsCargo);
                    }

                    foreach (var BotRemover in botsServidor)
                    {
                        if (botsFinal.Contains(BotRemover))
                            botsFinal.Remove(BotRemover);
                        else
                            botsFinal.Add(BotRemover);
                    }

                    foreach (var BotsParaOSegundoEmbed in botsFinal)
                    {
                        if (string.IsNullOrWhiteSpace(BotsParaOSegundoEmbed.Nickname))
                            continue;

                        if (!BotsParaOSegundoEmbed.IsBot)
                            continue;

                        if (BotsParaOSegundoEmbed.Presence == null)
                            continue;

                        if (BotsParaOSegundoEmbed.Nickname.Contains("prefix"))
                            segundoSTR.Append($"`{++i}`. {Program.ubgeBot.utilidadesGerais.MencaoMembro(BotsParaOSegundoEmbed)} | {await Program.ubgeBot.utilidadesGerais.ConverteStatusParaEmoji(ctx, BotsParaOSegundoEmbed)}: " +
                                $"{Program.ubgeBot.utilidadesGerais.ConverteStatusPraNome(BotsParaOSegundoEmbed)} | {(BotsParaOSegundoEmbed.Roles.Contains(botsMusicais) ? $"{(string.IsNullOrWhiteSpace(BotsParaOSegundoEmbed.VoiceState?.Channel?.Name) ? "Este bot de música está livre." : $"Este bot é de música está no canal: **{BotsParaOSegundoEmbed.VoiceState.Channel.Name}**")}" : "Este bot não é de música.")}\n");
                    }

                    var Cor = Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed();

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Ajuda para os outros bots da UBGE:", IconUrl = Valores.logoUBGE },
                        Color = Cor,
                        Description = primeiroSTR.ToString(),
                        ThumbnailUrl = ctx.Member.AvatarUrl,
                    };

                    DiscordEmbedBuilder segundoEmbed = new DiscordEmbedBuilder
                    {
                        Color = Cor,
                        Description = segundoSTR.ToString(),
                        Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Comando requisitado pelo: {ctx.Member.Username}", IconUrl = ctx.Member.AvatarUrl },
                    };

                    await ctx.RespondAsync(embed: embed.Build());
                    await ctx.RespondAsync(embed: segundoEmbed.Build());
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }
    }
}