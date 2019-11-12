using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UBGE_Bot.Main;
using UBGE_Bot.MongoDB.Modelos;
using UBGE_Bot.Utilidades;
using UBGE_Bot.LogExceptions;

namespace UBGE_Bot.Comandos.Servidores_da_UBGE
{
    public sealed class MemberControlled : BaseCommandModule
    {
        [Command("servidores"), Aliases("sv")]

        public async Task ServidoresUBGEAsync(CommandContext ctx, [RemainingText] string jogo = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    if (string.IsNullOrWhiteSpace(jogo))
                    {
                        embed.WithColor(new DiscordColor(0x32363c))
                            .WithAuthor("Como executar este comando:", null, Valores.infoLogo)
                            .AddField("PC/Mobile", $"Digite: `{ctx.Prefix}sv list` para eu listar todos os servidores disponíveis!")
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    var client = Program.ubgeBot.mongoClient;
                    var db = client.GetDatabase(Valores.Mongo.local);
                    var servidoresUBGE = db.GetCollection<ServidoresUBGE>(Valores.Mongo.servidoresUBGE);
                    var filtroServidores = Builders<ServidoresUBGE>.Filter.Eq(x => x.jogo, jogo.ToLowerInvariant());
                    var resultadoServidoresUBGE = await (await servidoresUBGE.FindAsync(filtroServidores)).ToListAsync();

                    if (jogo.ToLowerInvariant() != "list")
                    {
                        
                        if (resultadoServidoresUBGE.Count == 0)
                        {
                            var jogoSplit = RetornaNomeDoServidorEFoto(jogo).Split(',');
                            
                            if (jogo.ToLower() == "pr" || jogo.ToLower() == "ce" || jogo.ToLower() == "dyz" ||
                            jogo.ToLower() == "os" || jogo.ToLower() == "cs" || jogo.ToLower() == "unturned" ||
                            jogo.ToLower() == "mordhau")
                            {

                                embed.WithDescription($":warning: | A UBGE não possui servidores oficiais no {jogoSplit[0]} e/ou este servidor está offline.\n\n" +
                                    $"Digite `{ctx.Prefix}servidores list` para saber quais servidores da UBGE estão online/disponíveis.")
                                    .WithThumbnailUrl(jogoSplit[1])
                                    .WithColor(DiscordColor.Red)
                                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                                    .WithTimestamp(DateTime.Now);

                                await ctx.RespondAsync(embed: embed.Build());
                            }
                            else
                            {
                                embed.WithDescription($":warning: | {jogoSplit[0]}\n\nDigite `{ctx.Prefix}servidores list` para saber quais servidores da UBGE estão online/disponíveis.")
                                    .WithThumbnailUrl(jogoSplit[1])
                                    .WithColor(DiscordColor.Red)
                                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                                    .WithTimestamp(DateTime.Now);

                                await ctx.RespondAsync(embed: embed.Build());
                            }
                        }
                        else
                        {
                            int nForeach = 0;

                            foreach (var servidor in resultadoServidoresUBGE)
                            {
                                embed.WithAuthor($"Servidores de {servidor.nomeServidorParaComando} da UBGE:", null, servidor.fotoDoServidor)
                                    .WithThumbnailUrl(servidor.thumbnailDoServidor)
                                    .AddField($"{++nForeach}. {servidor.nomeDoServidor}", $"Players: **{servidor.jogadoresDoServidor}**/**{servidor.maximoDePlayers}**\n" +
                                        $"Mapa: **{servidor.mapaDoServidor}**\n" +
                                        $"País: :flag_{servidor.paisDoServidor.ToLower()}:\n" +
                                        $"Status do Servidor: **{servidor.statusDoServidor}**\n" +
                                        $"Modo de Jogo: **{servidor.modoDeJogo}**\n" +
                                        $"Versão do Jogo: **{servidor.versaoDoJogo}**\n" +
                                        $"IP: **{servidor.ipDoServidor}**\n" +
                                        $"Porta: **{servidor.portaDoServidor}**");
                            }

                            embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                                .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                                .WithTimestamp(DateTime.Now);

                            await ctx.RespondAsync(embed: embed.Build());
                        }
                    }
                    else
                    {
                        var filtroDisponivel = Builders<ServidoresUBGE>.Filter.Empty;
                        var resultadoServidoresUBGENovoFiltro = await (await servidoresUBGE.FindAsync(filtroDisponivel)).ToListAsync();

                        StringBuilder strDisponivel = new StringBuilder();

                        foreach (var servidor in resultadoServidoresUBGENovoFiltro)
                        {
                            if (!strDisponivel.ToString().Contains(servidor.servidorDisponivel))
                                strDisponivel.Append($"{servidor.servidorDisponivel}\n");
                        }

                        embed.WithAuthor($"A UBGE possui servidores nos seguintes jogos:", null, Valores.logoUBGE)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithDescription(strDisponivel.ToString())
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

        private string RetornaNomeDoServidorEFoto(string jogoUBGE)
        {
            if (jogoUBGE.ToLower() == "pr")
                return $"Project Reality, {Valores.prLogoNucleo}";
            else if (jogoUBGE.ToLower() == "ce")
                return $"Conan Exiles, {Valores.conanExilesLogo}";
            else if (jogoUBGE.ToLower() == "dyz")
                return $"Day Z, {Valores.dayZLogo}";
            else if (jogoUBGE.ToLower() == "os")
                return $"OpenSpades, {Valores.openSpadesLogo}";
            else if (jogoUBGE.ToLower() == "cs")
                return $"Counter-Strike, {Valores.counterStrikeLogo}";
            else if (jogoUBGE.ToLower() == "unturned")
                return $"Unturned, {Valores.unturnedLogo}";
            else if (jogoUBGE.ToLower() == "mordhau")
                return $"Mordhau, {Valores.mordhauLogo}";
            else
                return $"Erro! Este jogo não encontrado em nosso banco de dados., {Valores.notFoundImage}";
        }
    }
}
