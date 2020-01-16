using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UBGE;
using UBGE.MongoDB.Models;
using UBGE.Utilities;
using Log = UBGE.Logger.Logger;

namespace UBGE.Commands.ServersUBGE
{
    public sealed class MemberControlled : BaseCommandModule
    {
        [Command("servidores"), Aliases("sv"), ConnectedToMongo]

        public async Task ServidoresUBGEAsync(CommandContext ctx, [RemainingText] string jogo = null)
        {
            try
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                if (string.IsNullOrWhiteSpace(jogo))
                {
                    embed.WithColor(Program.Bot.Utilities.HelpCommandsColor())
                        .WithAuthor("Como executar este comando:", null, Values.infoLogo)
                        .AddField("PC/Mobile", $"Digite: `{ctx.Prefix}servidores list` para eu listar todos os servidores disponíveis!")
                        .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    await ctx.RespondAsync(embed: embed.Build());
                    return;
                }

                jogo = jogo.ToLower();

                IMongoCollection<ServidoresUBGE> servidoresUBGE = Program.Bot.LocalDB.GetCollection<ServidoresUBGE>(Values.Mongo.servidoresUBGE);

                FilterDefinition<ServidoresUBGE> filtroServidores = Builders<ServidoresUBGE>.Filter.Eq(x => x.jogo, jogo);
                List<ServidoresUBGE> resultadoServidoresUBGE = await (await servidoresUBGE.FindAsync(filtroServidores)).ToListAsync();

                if (jogo != "list")
                {
                    if (resultadoServidoresUBGE.Count == 0)
                    {
                        string[] jogoSplit = RetornaNomeDoServidorEFoto(jogo).Split(',');

                        if (jogo == "pr" || jogo == "ce" || jogo == "dyz" || jogo == "os" || jogo == "cs" ||
                        jogo == "unturned" || jogo == "mordhau")
                        {
                            embed.WithDescription($":warning: | A UBGE não possui servidores oficiais no {jogoSplit[0]} e/ou este servidor está offline.\n\n" +
                                $"Digite `{ctx.Prefix}servidores list` para saber quais servidores da UBGE estão online/disponíveis.")
                                .WithThumbnailUrl(jogoSplit[1])
                                .WithColor(DiscordColor.Red)
                                .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                                .WithTimestamp(DateTime.Now);

                            await ctx.RespondAsync(embed: embed.Build());
                        }
                        else
                        {
                            embed.WithDescription($":warning: | {jogoSplit[0]}\n\nDigite: `{ctx.Prefix}servidores list` para saber quais servidores da UBGE estão online/disponíveis.")
                                .WithThumbnailUrl(jogoSplit[1])
                                .WithColor(DiscordColor.Red)
                                .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                                .WithTimestamp(DateTime.Now);

                            await ctx.RespondAsync(embed: embed.Build());
                        }
                    }
                    else
                    {
                        int nForeach = 0;

                        if (jogo == "pr")
                        {
                            foreach (ServidoresUBGE servidorPR in resultadoServidoresUBGE)
                            {
                                embed.WithAuthor($"Servidores de {servidorPR.nomeServidorParaComando} da UBGE:", null, servidorPR.fotoDoServidor)
                                    .WithThumbnailUrl(servidorPR.thumbnailDoServidor)
                                    .AddField($"{++nForeach}. {servidorPR.nomeDoServidor}", $"Players: **{servidorPR.jogadoresDoServidor}**/**{servidorPR.maximoDePlayers}**\n" +
                                        $"Mapa: **{servidorPR.mapaDoServidor}**\n" +
                                        $"País: :flag_{servidorPR.paisDoServidor.ToLower()}:\n" +
                                        $"Status do Servidor: **{servidorPR.statusDoServidor}**\n" +
                                        $"Modo de Jogo: **{servidorPR.modoDeJogo}**");
                            }
                        }
                        else
                        {
                            foreach (ServidoresUBGE servidor in resultadoServidoresUBGE)
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
                        }

                        embed.WithColor(Program.Bot.Utilities.RandomColorEmbed())
                            .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                }
                else
                {
                    FilterDefinition<ServidoresUBGE> filtroDisponivel = Builders<ServidoresUBGE>.Filter.Empty;
                    List<ServidoresUBGE> resultadoServidoresUBGENovoFiltro = await (await servidoresUBGE.FindAsync(filtroDisponivel)).ToListAsync();

                    StringBuilder strDisponivel = new StringBuilder();

                    foreach (ServidoresUBGE servidor in resultadoServidoresUBGENovoFiltro)
                    {
                        string[] servidorSplit = servidor.servidorDisponivel.Split('=');

                        string[] comandoServidor = servidorSplit[1].Split(' ');

                        if (!strDisponivel.ToString().Contains($"{servidorSplit[0]} = `{ctx.Prefix}{comandoServidor[1].Replace(" ", "").Replace("`", "")} {comandoServidor[2].Replace("`", "")}`\n"))
                            strDisponivel.Append($"{servidorSplit[0]} = `{ctx.Prefix}{comandoServidor[1].Replace(" ", "").Replace("`", "")} {comandoServidor[2].Replace("`", "")}`\n");
                    }

                    embed.WithAuthor($"A UBGE possui servidores nos seguintes jogos:", null, Values.logoUBGE)
                        .WithColor(Program.Bot.Utilities.RandomColorEmbed())
                        .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                        .WithDescription(strDisponivel.ToString())
                        .WithTimestamp(DateTime.Now);

                    await ctx.RespondAsync(embed: embed.Build());
                }
            }
            catch (Exception exception)
            {
                await Program.Bot.Logger.Error(Log.TypeError.Commands, exception);
            }
        }

        private string RetornaNomeDoServidorEFoto(string jogoUBGE)
        {
            if (jogoUBGE == "pr")
                return $"Project Reality, {Values.prLogoSecretary}";
            else if (jogoUBGE == "ce")
                return $"Conan Exiles, {Values.conanExilesLogo}";
            else if (jogoUBGE == "dyz")
                return $"Day Z, {Values.dayZLogo}";
            else if (jogoUBGE == "os")
                return $"OpenSpades, {Values.openSpadesLogo}";
            else if (jogoUBGE == "cs")
                return $"Counter-Strike, {Values.counterStrikeLogo}";
            else if (jogoUBGE == "unturned")
                return $"Unturned, {Values.unturnedLogo}";
            else if (jogoUBGE == "mordhau")
                return $"Mordhau, {Values.mordhauLogo}";
            else
                return $"Erro! Este jogo não encontrado em nosso banco de dados., {Values.notFoundImage}";
        }
    }
}