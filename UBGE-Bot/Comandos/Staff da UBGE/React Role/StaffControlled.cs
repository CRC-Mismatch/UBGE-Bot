using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Exceptions;
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
    [Group("reactrole"), Aliases("rr"), UBGE_Staff, BotConectadoAoMongo]

    public sealed class StaffControlled : BaseCommandModule
    {
        [Command("cargo.add"), Description("[<Emoji> (:leothinks:) ou em caso de emojis de outro servidor, usa-se (leothinks), sem o :] <@Cargo ou \"Nome do Cargo\"> <Categoria>`\nAdiciona um jogo na categoria especificada.\n\n")]

        public async Task AdicionaCargoReactRoleAsync(CommandContext ctx, DiscordChannel canalReactRole = null, DiscordEmoji emojiServidor = null, DiscordRole cargoServidor = null, [RemainingText] string categoriaReact = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    if (canalReactRole == null || emojiServidor == null || cargoServidor == null || string.IsNullOrWhiteSpace(categoriaReact))
                    {
                        embed.WithColor(Program.ubgeBot.utilidadesGerais.CorHelpComandos())
                           .WithAuthor("Como executar este comando:", null, Valores.infoLogo)
                           .AddField("PC/Mobile", $"{ctx.Prefix}rr cargo.add Canal[Id] Emoji Cargo[Id/Menção/Nome entre \"\"\"] Nome[Categoria]")
                           .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                           .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(emojiServidor.Url))
                    {
                        embed.WithAuthor("Emoji inválido!", null, Valores.infoLogo)
                           .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                           .WithDescription("Digite um emoji válido!")
                           .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                           .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    IMongoDatabase local = Program.ubgeBot.localDB;

                    IMongoCollection<Jogos> jogos = local.GetCollection<Jogos>(Valores.Mongo.jogos);
                    IMongoCollection<Reacts> reacts = local.GetCollection<Reacts>(Valores.Mongo.reacts);

                    List<Jogos> respostaJogos = await (await jogos.FindAsync(Builders<Jogos>.Filter.Eq(x => x.idDoEmoji, emojiServidor.Id))).ToListAsync();
                    List<Reacts> respostaReacts = await (await reacts.FindAsync(Builders<Reacts>.Filter.Eq(x => x.categoria, categoriaReact))).ToListAsync();

                    if (respostaReacts.Count != 0 && respostaJogos.Count == 0)
                    {
                        DiscordMessage mensagem = await canalReactRole.GetMessageAsync(respostaReacts.LastOrDefault().idDaMensagem);
                        DiscordEmbed embedMensagemReactRole = mensagem.Embeds.LastOrDefault();

                        DiscordEmbedBuilder builder = new DiscordEmbedBuilder(embedMensagemReactRole);

                        if (!embedMensagemReactRole.Description.Contains($"{emojiServidor.ToString()} - {cargoServidor.Name}"))
                        {
                            string descricaoEmbed = embedMensagemReactRole.Description;
                            string novaDescricaoEmbed = descricaoEmbed += $"\n{emojiServidor.ToString()} - {cargoServidor.Name}";

                            builder.WithDescription(novaDescricaoEmbed);
                            builder.WithAuthor(builder.Author.Name, null, Valores.logoUBGE);
                            builder.WithColor(builder.Color.Value);

                            await mensagem.ModifyAsync(embed: builder.Build());
                        }

                        await jogos.InsertOneAsync(new Jogos
                        {
                            nomeDaCategoria = categoriaReact,
                            idDoCargo = cargoServidor.Id,
                            idDoEmoji = emojiServidor.Id,
                        });

                        embed.WithAuthor("Jogo Adicionado!", null, Valores.logoUBGE)
                            .WithDescription($"Categoria: \"{categoriaReact}\"\n\n" +
                            $"Cargo do jogo: {cargoServidor.Mention}")
                            .WithThumbnailUrl(emojiServidor.Url)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await mensagem.CreateReactionAsync(emojiServidor);
                        await ctx.RespondAsync(embed: embed.Build());
                    }
                    else if (respostaJogos.Count != 0)
                        await ctx.RespondAsync($"{ctx.Member.Mention}, este jogo já existe!");
                    else if (respostaReacts.Count == 0)
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

        public async Task RemoveCargoReactRoleAsync(CommandContext ctx, DiscordChannel canalReactRole = null, DiscordEmoji emoji = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    if (canalReactRole == null || emoji == null)
                    {
                        embed.WithColor(Program.ubgeBot.utilidadesGerais.CorHelpComandos())
                           .WithAuthor("Como executar este comando:", null, Valores.infoLogo)
                           .AddField("PC/Mobile", $"{ctx.Prefix}rr cargo.del Canal[Id] Emoji")
                           .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                           .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(emoji.Url))
                    {
                        embed.WithAuthor("Emoji inválido!", null, Valores.infoLogo)
                           .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                           .WithDescription("Digite um emoji válido!")
                           .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                           .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    IMongoDatabase local = Program.ubgeBot.localDB;

                    IMongoCollection<Jogos> jogos = local.GetCollection<Jogos>(Valores.Mongo.jogos);
                    IMongoCollection<Reacts> reacts = local.GetCollection<Reacts>(Valores.Mongo.reacts);

                    FilterDefinition<Jogos> filtroJogos = Builders<Jogos>.Filter.Eq(x => x.idDoEmoji, emoji.Id);
                    List<Jogos> resultadoJogos = await (await jogos.FindAsync(filtroJogos)).ToListAsync();

                    Jogos ultimoResultadoJogo = resultadoJogos.LastOrDefault();

                    List<Reacts> resultadoReacts = await (await reacts.FindAsync(Builders<Reacts>.Filter.Eq(x => x.categoria, ultimoResultadoJogo.nomeDaCategoria))).ToListAsync();

                    Reacts ultimoResultadoReact = resultadoReacts.LastOrDefault();

                    if (resultadoJogos.Count != 0 && resultadoReacts.Count != 0)
                    {
                        DiscordMessage mensagem = await canalReactRole.GetMessageAsync(ultimoResultadoReact.idDaMensagem);
                        DiscordEmbed embedMensagemReactRole = mensagem.Embeds.LastOrDefault();

                        DiscordEmbedBuilder builder = new DiscordEmbedBuilder(embedMensagemReactRole);

                        DiscordRole cargoJogoEmbed = ctx.Guild.GetRole(ultimoResultadoJogo.idDoCargo);

                        IReadOnlyList<DiscordUser> MembrosReacoes = await mensagem.GetReactionsAsync(emoji);

                        string linhaMensagemEmbed = $"{emoji.ToString()} - {cargoJogoEmbed.Name}";

                        if (embedMensagemReactRole.Description.Contains(linhaMensagemEmbed))
                        {
                            string descricaoEmbed = embedMensagemReactRole.Description;

                            List<string> lista = descricaoEmbed.Split('\n').ToList();
                            lista.RemoveAt(lista.FindIndex(linha => linha.Contains(linhaMensagemEmbed)));

                            StringBuilder strEmbedFinal = new StringBuilder();

                            foreach (string linha in lista)
                                strEmbedFinal.Append($"{linha}\n");

                            builder.WithDescription(strEmbedFinal.ToString());
                            builder.WithAuthor(embedMensagemReactRole.Author.Name, null, Valores.logoUBGE);
                            builder.WithColor(embedMensagemReactRole.Color.Value);

                            await mensagem.ModifyAsync(embed: builder.Build());
                        }

                        await jogos.DeleteOneAsync(filtroJogos);

                        DiscordMessage msgAguarde = await ctx.RespondAsync($"Aguarde, estou removendo as reações dos membros que está no {canalReactRole.Mention}...");

                        int i = 0;

                        foreach (DiscordUser Membro in MembrosReacoes)
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

                        await msgAguarde.DeleteAsync();

                        if (i != 0)
                            await ctx.RespondAsync($"Existem **{i}** reações que não foram removidas por que os membros saíram da {ctx.Guild.Name}, por favor, remova-as manualmente. :wink:");

                        embed.WithAuthor("Jogo removido!", null, Valores.logoUBGE)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithThumbnailUrl(emoji.Url)
                            .WithDescription($"O cargo: {cargoJogoEmbed.Mention} e a reação: {emoji.ToString()} foram removidos com sucesso!{(i != 0 ? $"\n\n{i} reações restantes para serem removidas manualmente." : string.Empty)}")
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                    else if (resultadoReacts.Count == 0)
                        await ctx.RespondAsync($"{ctx.Member.Mention}, essa categoria não existe!");
                    else if (resultadoJogos.Count == 0)
                        await ctx.RespondAsync($"{ctx.Member.Mention}, esse jogo não existe!");
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

        [Command("cargo.add"), Description("[<Emoji> (:leothinks:) ou em caso de emojis de outro servidor, usa-se (leothinks), sem o :] <@Cargo ou \"Nome do Cargo\"> <Categoria>`\nAdiciona um jogo na categoria especificada.\n\n")]

        public async Task AdicionaCargoComONomeDoEmojiReactRoleAsync(CommandContext ctx, DiscordChannel canalReactRole = null, string emojiServidor = null, DiscordRole cargoServidor = null, [RemainingText] string categoriaReact = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    CommandsNextExtension commandsNext = ctx.Client.GetCommandsNext();

                    Command ProcuraComando = commandsNext.FindCommand($"reactrole cargo.add {canalReactRole.Id} {await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, emojiServidor)} {cargoServidor.Mention} {categoriaReact}", out string Args);
                    CommandContext Comando = commandsNext.CreateFakeContext(ctx.Member, ctx.Channel, "", "//", ProcuraComando, Args);

                    await commandsNext.ExecuteCommandAsync(Comando);
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

        public async Task RemoveCargoComONomeDoEmojiReactRoleAsync(CommandContext ctx, DiscordChannel canalReactRole = null, string emoji = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    CommandsNextExtension commandsNext = ctx.Client.GetCommandsNext();

                    Command ProcuraComando = commandsNext.FindCommand($"reactrole cargo.del {canalReactRole.Id} {await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, emoji)}", out string Args);
                    CommandContext Comando = commandsNext.CreateFakeContext(ctx.Member, ctx.Channel, "", "//", ProcuraComando, Args);

                    await commandsNext.ExecuteCommandAsync(Comando);
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

        [Command("categoria.add"), Aliases("cat.add"), Description("<Nome>`\nCria uma categoria.\n\n")]

        public async Task AdicionaCategoriaAsync(CommandContext ctx, DiscordChannel canalReactRole = null, [RemainingText] string nomeDaCategoriaReactRole = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    await ctx.TriggerTypingAsync();

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    if (canalReactRole == null || string.IsNullOrWhiteSpace(nomeDaCategoriaReactRole))
                    {
                        embed.WithColor(Program.ubgeBot.utilidadesGerais.CorHelpComandos())
                           .WithAuthor("Como executar este comando:", null, Valores.infoLogo)
                           .AddField("PC/Mobile", $"{ctx.Prefix}rr cat.add Canal[Id] Nome[Jogos: FPS]")
                           .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                           .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    IMongoDatabase local = Program.ubgeBot.localDB;
                    IMongoCollection<Reacts> reacts = local.GetCollection<Reacts>(Valores.Mongo.reacts);
                    IMongoCollection<Jogos> jogos = local.GetCollection<Jogos>(Valores.Mongo.jogos);

                    InteractivityExtension interactivity = ctx.Client.GetInteractivity();

                    embed.WithAuthor("Deseja colocar como descrição da categoria a frase padrão: \"Clique na reação para obter o cargo. Remova a reação para tirar o cargo.\" ou " +
                        "você deseja digitar a frase?", null, Valores.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithDescription("Aperte: :white_check_mark: para selecionar a frase padrão.\n" +
                        "Aperte: :x: para você digitar a frase da categoria.")
                        .WithThumbnailUrl(ctx.Member.AvatarUrl);

                    DiscordMessage MsgEmbed = await ctx.RespondAsync(embed: embed.Build());
                    await MsgEmbed.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                    await MsgEmbed.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));

                    DiscordEmoji Reacao = (await interactivity.WaitForReactionAsync(MsgEmbed, ctx.User, TimeSpan.FromMinutes(30))).Result.Emoji;

                    if (Reacao == DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"))
                    {
                        await MsgEmbed.DeleteAsync();

                        Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                        embed.WithAuthor(nomeDaCategoriaReactRole, null, Valores.logoUBGE)
                            .WithColor(new DiscordColor(0x32363c))
                            .WithDescription("Clique na reação para obter o cargo. Remova a reação para tirar o cargo.\n\n");

                        //string CaminhoImg = Directory.GetCurrentDirectory() + @"\ImagemCategorias.png";
                        //await SelecioneSeusJogos.SendFileAsync(CaminhoImg);

                        DiscordMessage msgEmbedReactRole = await canalReactRole.SendMessageAsync(embed: embed.Build());

                        await reacts.InsertOneAsync(new Reacts
                        {
                            categoria = nomeDaCategoriaReactRole,
                            idDaMensagem = msgEmbedReactRole.Id,
                            idDoCanal = canalReactRole.Id,
                            servidor = ctx.Guild.Id,
                        });

                        await jogos.InsertOneAsync(new Jogos
                        {
                            nomeDaCategoria = nomeDaCategoriaReactRole,
                            idDoEmoji = 0,
                            idDoCargo = 0
                        });

                        Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                        embed.WithAuthor("Categoria adicionada!", null, Valores.logoUBGE)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithDescription($"A categoria: \"{nomeDaCategoriaReactRole}\" foi adicionada com sucesso!\n\n" +
                            $"Para ir até ela, {Formatter.MaskedUrl("clique aqui", msgEmbedReactRole.JumpLink, "clique aqui")}.")
                            .WithThumbnailUrl(ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now)
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                    else if (Reacao == DiscordEmoji.FromName(ctx.Client, ":x:"))
                    {
                        await MsgEmbed.DeleteAsync();

                        Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                        embed.WithAuthor("Digite a frase que irá ficar na descrição da categoria.", null, Valores.logoUBGE)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                        DiscordMessage mensagemFraseEmbedReactRole = await ctx.RespondAsync(embed: embed.Build());
                        DiscordMessage Input = await Program.ubgeBot.utilidadesGerais.PegaResposta(interactivity, ctx);

                        Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                        embed.WithAuthor(nomeDaCategoriaReactRole, null, Valores.logoUBGE)
                            .WithColor(new DiscordColor(0x32363c))
                            .WithDescription($"{Input.Content}\n\n");

                        //string CaminhoImg = Directory.GetCurrentDirectory() + @"\ImagemCategorias.png";
                        //await SelecioneSeusJogos.SendFileAsync(CaminhoImg);

                        DiscordMessage msgEmbedReactRole = await canalReactRole.SendMessageAsync(embed: embed.Build());

                        await reacts.InsertOneAsync(new Reacts
                        {
                            categoria = nomeDaCategoriaReactRole,
                            idDaMensagem = msgEmbedReactRole.Id,
                            idDoCanal = canalReactRole.Id,
                            servidor = ctx.Guild.Id
                        });

                        Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                        embed.WithAuthor("Categoria adicionada!", null, Valores.logoUBGE)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithDescription($"A categoria: \"{nomeDaCategoriaReactRole}\" foi adicionada com sucesso!\n\n" +
                            $"Para ir até ela, {Formatter.MaskedUrl("clique aqui", msgEmbedReactRole.JumpLink, "clique aqui")}.")
                            .WithThumbnailUrl(ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now)
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                }
                catch (UnauthorizedException)
                {
                    await ctx.RespondAsync($"{ctx.Member.Mention}, não tenho autorização para enviar a mensagem do ReactRole nesse canal, por favor ative-a!");
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("categoria.del"), Aliases("cat.del"), Description("<Nome>`\nApaga uma categoria.\n\n")]

        public async Task DeletaCategoriaAsync(CommandContext ctx, DiscordChannel canalReactRole = null, [RemainingText] string nomeDaCategoriaReactRole = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    if (canalReactRole == null || string.IsNullOrWhiteSpace(nomeDaCategoriaReactRole))
                    {
                        embed.WithColor(Program.ubgeBot.utilidadesGerais.CorHelpComandos())
                            .WithAuthor("Como executar este comando:", null, Valores.infoLogo)
                            .AddField("PC/Mobile", $"{ctx.Prefix}rr cat.del Canal[Id] Nome[Jogos: FPS]")
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    IMongoDatabase local = Program.ubgeBot.localDB;
                    IMongoCollection<Reacts> reacts = local.GetCollection<Reacts>(Valores.Mongo.reacts);
                    IMongoCollection<Jogos> jogos = local.GetCollection<Jogos>(Valores.Mongo.jogos);

                    FilterDefinition<Reacts> filtroReacts = Builders<Reacts>.Filter.Eq(x => x.categoria, nomeDaCategoriaReactRole);

                    List<Reacts> resultadoReacts = await (await reacts.FindAsync(filtroReacts)).ToListAsync();

                    if (resultadoReacts.Count != 0)
                    {
                        DiscordMessage msg = await canalReactRole.GetMessageAsync(resultadoReacts.LastOrDefault().idDaMensagem);
                        await msg.DeleteAsync();
                        await reacts.DeleteOneAsync(filtroReacts);
                        await jogos.DeleteManyAsync(Builders<Jogos>.Filter.Eq(x => x.nomeDaCategoria, nomeDaCategoriaReactRole));

                        embed.WithAuthor($"Todos os jogos e a categoria: \"{nomeDaCategoriaReactRole}\" foram apagados!", null, Valores.logoUBGE)
                            .WithDescription(await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "UBGE"))
                            .WithThumbnailUrl(ctx.Member.AvatarUrl)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                    else
                    {
                        embed.WithAuthor($"Esta categoria não existe!", null, Valores.logoUBGE)
                            .WithDescription(":thinking:")
                            .WithThumbnailUrl(ctx.Member.AvatarUrl)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                }
                catch (UnauthorizedException)
                {
                    await ctx.RespondAsync($"{ctx.Member.Mention}, não tenho autorização para enviar a mensagem do ReactRole nesse canal, por favor ative-a!");
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("categoria.edit"), Aliases("cat.edit"), Description("<Nome>`\nEdit o nome de uma categoria.\n\n")]

        public async Task EditCategoriaAsync(CommandContext ctx, DiscordChannel canalReactRole = null, [RemainingText] string nomeDaCategoria = null)
        {
            try
            {
                await ctx.TriggerTypingAsync();

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                if (canalReactRole == null || string.IsNullOrWhiteSpace(nomeDaCategoria))
                {
                    embed.WithAuthor("Como executar este comando:", null, Valores.infoLogo)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorHelpComandos())
                        .AddField("PC/Mobile", $"{ctx.Prefix}rr cat.edit Canal[Id] Nome[Jogos: FPS]")
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    await ctx.RespondAsync(embed: embed.Build());
                    return;
                }

                InteractivityExtension Interact = ctx.Client.GetInteractivity();

                IMongoDatabase local = Program.ubgeBot.localDB;
                IMongoCollection<Reacts> reacts = local.GetCollection<Reacts>(Valores.Mongo.reacts);
                IMongoCollection<Jogos> jogos = local.GetCollection<Jogos>(Valores.Mongo.jogos);

                FilterDefinition<Reacts> filtroMenu = Builders<Reacts>.Filter.Eq(x => x.categoria, nomeDaCategoria);
                FilterDefinition<Jogos> filtroJogos = Builders<Jogos>.Filter.Eq(x => x.nomeDaCategoria, nomeDaCategoria);

                List<Reacts> listaFiltro = await (await reacts.FindAsync(filtroMenu)).ToListAsync();

                if (listaFiltro.Count == 0)
                {
                    embed.WithAuthor("Essa categoria não existe!", null, Valores.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithThumbnailUrl(ctx.Member.AvatarUrl)
                        .WithDescription(":thumbsup:")
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    await ctx.RespondAsync(embed: embed.Build());
                    return;
                }

                Reacts ultimaRespostaLista = listaFiltro.FirstOrDefault();

                DiscordMessage mensagemEmbed = await canalReactRole.GetMessageAsync(ultimaRespostaLista.idDaMensagem);
                DiscordEmbed embedJogo = mensagemEmbed.Embeds.FirstOrDefault();

                DiscordEmbedBuilder embedNovoReactRole = new DiscordEmbedBuilder(embedJogo);

                embed.WithAuthor($"Digite o novo título da categoria: \"{nomeDaCategoria}\"", null, Valores.logoUBGE)
                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                await ctx.RespondAsync(embed: embed.Build());
                DiscordMessage input = await Program.ubgeBot.utilidadesGerais.PegaResposta(Interact, ctx);

                embedNovoReactRole.WithAuthor(input.Content, null, Valores.logoUBGE);

                DiscordMessage msgEmbed = await mensagemEmbed.ModifyAsync(embed: embedNovoReactRole.Build());

                await reacts.UpdateManyAsync(filtroMenu, Builders<Reacts>.Update.Set(x => x.categoria, input.Content));
                await jogos.UpdateManyAsync(filtroJogos, Builders<Jogos>.Update.Set(x => x.nomeDaCategoria, input.Content));

                Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                embed.WithAuthor("Título do embed foi modificado com sucesso!", null, Valores.logoUBGE)
                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithDescription($"Novo título: \"{input.Content}\"")
                    .WithThumbnailUrl(ctx.Member.AvatarUrl)
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: embed.Build());
            }
            catch (Exception exception)
            {
                await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
            }
        }
    }
}