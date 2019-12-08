using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using UBGE_Bot.Main;
using UBGE_Bot.MongoDB.Modelos;
using UBGE_Bot.LogExceptions;
using UBGE_Bot.Utilidades;

namespace UBGE_Bot.Comandos.Staff_da_UBGE.React_Role
{
    [Group("selecioneseuscargos"), Aliases("ssc"), UBGE]

    public sealed class StaffControlled : BaseCommandModule
    {
        [Command("cargo.add"), Description("[<Emoji> (:leothinks:) ou em caso de emojis de outro servidor, usa-se (leothinks), sem o :] <@Cargo ou \"Nome do Cargo\"> <Categoria>`\nAdiciona um jogo na categoria especificada.\n\n")]

        public async Task AdicionaCargoReactRoleAsync(CommandContext ctx, DiscordEmoji emojiServidor = null, DiscordRole cargoServidor = null, [RemainingText] string categoriaReact = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    var local = Program.ubgeBot.localDB;

                    var jogos = local.GetCollection<Jogos>(Valores.Mongo.jogos);
                    var reacts = local.GetCollection<Reacts>(Valores.Mongo.reacts);

                    if (emojiServidor == null)
                    {
                        await ctx.RespondAsync($"{ctx.Member.Mention}, digite um emoji!");

                        return;
                    }
                    else if (string.IsNullOrWhiteSpace(emojiServidor.Url))
                    {
                        await ctx.RespondAsync($"{ctx.Member.Mention}, este emoji não é válido!");

                        return;
                    }

                    if (cargoServidor == null)
                    {
                        await ctx.RespondAsync($"{ctx.Member.Mention}, digite um cargo!");

                        return;
                    }

                    if (string.IsNullOrWhiteSpace(categoriaReact))
                    {
                        await ctx.RespondAsync($"{ctx.Member.Mention}, digite o nome da categoria do ReactRole!");

                        return;
                    }

                    Jogos jogosClass = new Jogos
                    {
                        nomeDaCategoria = categoriaReact,
                        idDoCargo = cargoServidor.Id,
                        idDoEmoji = emojiServidor.Id
                    };

                    DiscordEmbedBuilder main = new DiscordEmbedBuilder();

                    var filtroJogos = Builders<Jogos>.Filter.Eq(x => x.nomeDaCategoria, cargoServidor.Name);
                    var filtroReacts = Builders<Reacts>.Filter.Eq(x => x.categoria, categoriaReact);
                    var respostaReacts = await (await reacts.FindAsync(filtroReacts)).ToListAsync();
                    var respostaJogos = await (await jogos.FindAsync(filtroJogos)).ToListAsync();

                    if (respostaReacts.Count() != 0 && respostaJogos.Count() == 0)
                    {
                        DiscordMessage mensagem = await ctx.Guild.GetChannel(ctx.Guild.Channels.Values.ToList().Find(x => x.Name.ToUpper().Contains(Valores.ChatsUBGE.canalSelecioneSeusCargos)).Id).GetMessageAsync(respostaReacts.LastOrDefault().idDaMensagem);
                        var embed = mensagem.Embeds.LastOrDefault();

                        DiscordEmbedBuilder builder = new DiscordEmbedBuilder(embed);

                        if (embed.Description.Contains($"{emojiServidor.ToString()} - {cargoServidor.Name}"))
                        {
                            var descricaoEmbed = embed.Description;
                            var novaDescricaoEmbed = descricaoEmbed += $"\n{emojiServidor.ToString()} - {cargoServidor.Name}";

                            builder.WithDescription(novaDescricaoEmbed);
                            builder.WithAuthor(builder.Author.Name, null, Valores.logoUBGE);
                            builder.WithColor(new DiscordColor(0x32363c));

                            await mensagem.ModifyAsync(embed: builder.Build());
                        }

                        await jogos.InsertOneAsync(jogosClass);

                        main.WithDescription($"Cargo do jogo: {cargoServidor.Name}\n\n" +
                            $"Categoria: \"{jogosClass.nomeDaCategoria}\"\n\n" +
                            $"Cargo: {cargoServidor.Mention}")
                            .WithThumbnailUrl(emojiServidor.Url)
                            .WithAuthor("Jogo Adicionado!", null, Valores.logoUBGE)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await mensagem.CreateReactionAsync(emojiServidor);
                        await ctx.RespondAsync(embed: main.Build());
                    }
                    else if (respostaJogos.Count() != 0)
                        await ctx.RespondAsync($"{ctx.Member.Mention}, este jogo já existe!");
                    else if (respostaReacts.Count() == 0)
                        await ctx.RespondAsync($"{ctx.Member.Mention}, esta categoria não existe!");
                }
                catch (ArgumentException)
                {
                    await ctx.RespondAsync($"{ctx.Member.Mention}, este emoji não foi encontrado!");
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("cargo.del"), Description("[<Emoji> (:leothinks:) ou em caso de emojis de outro servidor, usa-se (leothinks), sem o :]`\nRemove um jogo.\n\n")]

        public async Task RemoveCargoReactRoleAsync(CommandContext ctx, DiscordEmoji emoji = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    await ctx.TriggerTypingAsync();

                    var local = Program.ubgeBot.localDB;

                    var jogos = local.GetCollection<Jogos>(Valores.Mongo.jogos);
                    var reacts = local.GetCollection<Reacts>(Valores.Mongo.reacts);

                    if (emoji == null)
                    {
                        await ctx.RespondAsync($"{ctx.Member.Mention}, digite o emoji!");
                        return;
                    }
                    else if (string.IsNullOrWhiteSpace(emoji.Url))
                    {
                        await ctx.RespondAsync($"{ctx.Member.Mention}, este emoji não é válido!");

                        return;
                    }

                    var filtroJogos = Builders<Jogos>.Filter.Eq(x => x.idDoEmoji, emoji.Id);
                    var resultadoJogos = await (await jogos.FindAsync(filtroJogos)).ToListAsync();

                    Jogos jogo = resultadoJogos.LastOrDefault();

                    var filtroReacts = Builders<Reacts>.Filter.Eq(x => x.categoria, jogo.nomeDaCategoria);
                    var resultadoReacts = await (await reacts.FindAsync(filtroReacts)).ToListAsync();

                    Reacts react = resultadoReacts.LastOrDefault();

                    DiscordEmbedBuilder main = new DiscordEmbedBuilder();

                    if (resultadoJogos.Count() != 0)
                    {
                        var canal = ctx.Guild.GetChannel(ctx.Guild.Channels.Values.ToList().Find(x => x.Name.ToUpper().Contains(Valores.ChatsUBGE.canalSelecioneSeusCargos)).Id);
                        var mensagem = await canal.GetMessageAsync(react.idDaMensagem);
                        var embed = mensagem.Embeds.LastOrDefault();

                        DiscordEmbedBuilder builder = new DiscordEmbedBuilder(embed);

                        DiscordRole CargoJogoEmbed = ctx.Guild.GetRole(jogo.idDoCargo);

                        var MembrosReacoes = await mensagem.GetReactionsAsync(emoji);

                        if (embed.Description.Contains($"{emoji.ToString()} - {CargoJogoEmbed.Name}"))
                        {
                            var descricaoEmbed = embed.Description;

                            var lista = descricaoEmbed.Split('\n').ToList();

                            StringBuilder strEmbedFinal = new StringBuilder();

                            for (int linha = 0; linha < lista.Count; linha++)
                            {
                                if (lista[linha].Contains(emoji.ToString()))
                                    lista.RemoveAt(linha);

                                strEmbedFinal.Append($"{lista[linha]}\n");
                            }

                            builder.WithDescription(strEmbedFinal.ToString());
                            builder.WithAuthor(embed.Author.Name, null, Valores.logoUBGE);
                            builder.WithColor(new DiscordColor(0x32363c));

                            await mensagem.ModifyAsync(embed: builder.Build());
                        }

                        await jogos.DeleteOneAsync(filtroJogos);

                        DiscordMessage msgAguarde = await ctx.RespondAsync($"Aguarde, estou removendo as reações dos membros que está no {canal.Mention}...");

                        int i = 0;

                        foreach (var Membro in MembrosReacoes)
                        {
                            try
                            {
                                await Task.Delay(200);

                                await mensagem.DeleteReactionAsync(emoji, Membro);
                            }
                            catch (Exception) 
                            {
                                ++i;
                            }
                        }

                        if (i != 0) 
                        {
                            await msgAguarde.DeleteAsync();

                            await ctx.RespondAsync($"Existem **{i}** reações que não foram removidas por que os membros saíram da {ctx.Guild.Name}, por favor, remova-as manualmente. :wink:");
                        }

                        main.WithAuthor("Jogo removido!", null, Valores.logoUBGE)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithThumbnailUrl(emoji.Url)
                            .WithDescription($"O cargo: {CargoJogoEmbed.Mention} e a reação: {emoji.ToString()} foram removidos com sucesso!{(i != 0 ? $"\n\n{i} reações restantes para serem removidas manualmente." : string.Empty)}")
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: main.Build());
                    }
                }
                catch (ArgumentException)
                {
                    await ctx.RespondAsync($"{ctx.Member.Mention}, este emoji não foi encontrado!");
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }
    }
}