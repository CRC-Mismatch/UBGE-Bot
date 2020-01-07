using Autofac;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using UBGE_Bot.Main;
using UBGE_Bot.Utilidades;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace UBGE_Bot.Comandos.Gerais
{
    public sealed class MemberControlled : BaseCommandModule
    {
        [Command("help"), Aliases("ajuda", "?")]

        public async Task HelpAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            DiscordEmoji videoGameEmoji = DiscordEmoji.FromName(ctx.Client, ":video_game:"),
                comandosExtrasEmoji = DiscordEmoji.FromName(ctx.Client, ":wrench:"),
                comandosStaff = DiscordEmoji.FromName(ctx.Client, ":cop:"),
                comandosReactRole = DiscordEmoji.FromName(ctx.Client, ":joystick:"),
                comandosModMail = DiscordEmoji.FromName(ctx.Client, ":envelope:"),
                comandosDoador = DiscordEmoji.FromName(ctx.Client, ":dollar:");

            if (ctx.Guild.Id == Valores.Guilds.UBGE)
            {
            inicioHelp:

                DiscordRole cargoMembroRegistrado = ctx.Guild.GetRole(Valores.Cargos.cargoMembroRegistrado),
                    cargoModeradorDiscord = ctx.Guild.GetRole(Valores.Cargos.cargoModeradorDiscord);

                DiscordChannel canalCliqueAqui = ctx.Guild.GetChannel(Valores.ChatsUBGE.canalCliqueAqui);

                DiscordColor corEmbed = Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed();

                bool membroTemOCargoDeMembroRegistrado = ctx.Member.Roles.Contains(cargoMembroRegistrado);

                if (membroTemOCargoDeMembroRegistrado)
                    embed.AddField($"{videoGameEmoji} - Sistema de criação de salas:", $"Crie canais de voz personalizados e chame seus amigos para se divertir!\nBasta entrar no canal de voz `#{canalCliqueAqui.Name}`");

                embed.WithColor(corEmbed)
                    .WithAuthor($"{(membroTemOCargoDeMembroRegistrado ? "Olá" : "Oi")}, {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}!", null, Valores.logoUBGE)
                    .WithDescription("Aqui abaixo estão os sistemas/comandos disponíveis para você! Clique no emoji referente a cada tópico descrito.")
                    .AddField($"{comandosExtrasEmoji} - Comandos extras:", "Comandos extras que podem ser úteis para você que está usando o bot.")
                    .WithThumbnailUrl(ctx.Member.AvatarUrl);

                bool membroEStaff = ctx.Member.Roles.Where(x => x.Permissions.HasFlag(Permissions.KickMembers)).Count() != 0;

                if (membroEStaff)
                {
                    embed.AddField($"[Staff] {comandosStaff} - Comandos de moderação:", $"Comandos de moderação para os {ctx.Guild.GetRole(Valores.Cargos.cargoAdministradorDiscord).Mention} e os {cargoModeradorDiscord.Mention}.")
                        .AddField($"[Staff] {comandosReactRole} - Comandos do react role:", "Comandos do react role para adicionar e remover cargos de jogos.")
                        .AddField($"[Staff] {comandosModMail} - Comandos do ModMail:", "Comandos relacionados ao sistema do ModMail.")
                        .AddField($"[Staff] {comandosDoador} - Comandos dos doadores:", "Comandos relacionados a adicionar e remover doadores.");
                }

                DiscordMessage primeiraMensagemEmbed = await ctx.RespondAsync(embed: embed.Build());

                Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                embed.WithAuthor("Um breve tutorial de como ler e entender os meus comandos!")
                    .WithColor(corEmbed)
                    .WithDescription($"- `{ctx.Prefix}[Nome do grupo onde ele está, se existir no comando.] [Nome do comando] [Opção facultativa/obrigatória (Depende de cada comando)]`\n\n" +
                    $"Caso não tenha entendido, reaja nas reações abaixo para entender os comandos na prática ou chame algum {cargoModeradorDiscord.Mention}. {await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "UBGE")}")
                    .WithTimestamp(DateTime.Now)
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                DiscordMessage segundaMensagemEmbed = await ctx.RespondAsync(embed: embed.Build());
                await segundaMensagemEmbed.CreateReactionAsync(videoGameEmoji);
                await segundaMensagemEmbed.CreateReactionAsync(comandosExtrasEmoji);

                if (membroEStaff)
                {
                    await segundaMensagemEmbed.CreateReactionAsync(comandosStaff);
                    await segundaMensagemEmbed.CreateReactionAsync(comandosReactRole);
                    await segundaMensagemEmbed.CreateReactionAsync(comandosModMail);
                    await segundaMensagemEmbed.CreateReactionAsync(comandosDoador);
                }

                InteractivityExtension interactivity = ctx.Client.GetInteractivity();

                DiscordEmoji emojiResposta = (await interactivity.WaitForReactionAsync(segundaMensagemEmbed, ctx.User, TimeSpan.FromMinutes(5))).Result.Emoji;

                DiscordEmoji emojiVoltar = DiscordEmoji.FromName(ctx.Client, ":arrow_backward:");

                if (emojiResposta == videoGameEmoji)
                {
                    await primeiraMensagemEmbed.DeleteAsync();
                    await segundaMensagemEmbed.DeleteAsync();

                    StringBuilder strComandosCriarSala = new StringBuilder();

                    foreach (Command comando in ctx.CommandsNext.RegisteredCommands.Values)
                    {
                        if (comando is CommandGroup grupo)
                        {
                            foreach (Command comandoNoGrupo in grupo.Children)
                            {
                                if (strComandosCriarSala.ToString().Contains(comandoNoGrupo.QualifiedName))
                                    break;

                                if (!comandoNoGrupo.QualifiedName.Contains("sala"))
                                    break;

                                strComandosCriarSala.Append($"`{ctx.Prefix}{comandoNoGrupo.QualifiedName} {comandoNoGrupo.Description}");
                            }
                        }
                    }

                    DiscordChannel canalCrieSuaSalaAqui = ctx.Guild.GetChannel(Valores.ChatsUBGE.canalCrieSuaSalaAqui);

                    embed.WithAuthor("Sistema de criação de salas:", null, Valores.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithDescription($"Para criar uma sala no PC:\n" +
                        $"`1.` - Copie isso: `#{canalCrieSuaSalaAqui.Name}`.\n" +
                        $"`2.` - Aperte `Ctrl + T` e cole no espaço.\n" +
                        $"`3.` - Aperte `Enter`.\n" +
                        $"`4.` - Digite `{ctx.Prefix}criar`.\n\n" +
                        $"Para criar uma sala no celular:\n" +
                        $"`1.` - Procure pelo canal de texto: `#{canalCrieSuaSalaAqui.Name}`\n" +
                        $"`2.` - Digite `{ctx.Prefix}criar`.\n\n" +
                        $"Comandos:\n{strComandosCriarSala.ToString()}")
                        .WithThumbnailUrl(ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now)
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    DiscordMessage msgCriarSala = await ctx.RespondAsync(embed: embed.Build());
                    await msgCriarSala.CreateReactionAsync(emojiVoltar);

                    DiscordEmoji emojiRespostaCriarSala = (await interactivity.WaitForReactionAsync(msgCriarSala, ctx.User, TimeSpan.FromMinutes(5))).Result.Emoji;

                    if (emojiRespostaCriarSala == emojiVoltar)
                    {
                        await msgCriarSala.DeleteAsync();

                        goto inicioHelp;
                    }
                }
                else if (emojiResposta == comandosExtrasEmoji)
                {
                    await primeiraMensagemEmbed.DeleteAsync();
                    await segundaMensagemEmbed.DeleteAsync();

                    embed.WithAuthor("Comandos extras:", null, Valores.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithDescription($"`{ctx.Prefix}ping`\nVê o ping do bot.\n\n" +
                        $"`{ctx.Prefix}meuid`\nMostra o id do membro que digitou o comando.\n\n" +
                        $"`{ctx.Prefix}uptime`\nVê há quanto tempo o bot está ligado.\n\n" +
                        $"`{ctx.Prefix}avatar/foto Membro[ID/Menção]`\nVê a sua foto ou a foto de outro membro (Quando usado a menção).\n\n" +
                        $"`{ctx.Prefix}userinfo/usuário Membro[ID/Menção]`\nVê as suas informações ou a informações de outros membros (Quando usado a menção).\n\n" +
                        $"`{ctx.Prefix}dólar`\nVê o preço do dólar.\n\n" +
                        $"`{ctx.Prefix}euro`\nVê o preço do euro.\n\n" +
                        $"`{ctx.Prefix}servidorinfo/guildinfo/serverinfo`\nVê as informações do servidor (Pode ser executado em outros servidores).\n\n" +
                        $"`{ctx.Prefix}fotoservidor/avatarservidor/avatarguild`\nVê a foto do servidor (Pode ser executado em outros servidores).\n\n" +
                        $"`{ctx.Prefix}procuramembros Jogo[Nome]`\nO bot procurará membros que estão jogando determinado jogo (Ele procurará em todos os servidores que ele está).\n\n" +
                        $"`{ctx.Prefix}listar Canal[ID]`\nO bot listará todos os membros que estão em um canal de voz.\n\n" +
                        $"`{ctx.Prefix}servidores`\nO bot mostrará os servidores da UBGE de determinado jogo. *Se ele existir obviamente.*")
                        .WithThumbnailUrl(ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now)
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    DiscordMessage msgComandosExtras = await ctx.RespondAsync(embed: embed.Build());
                    await msgComandosExtras.CreateReactionAsync(emojiVoltar);

                    DiscordEmoji emojiRespostaComandosExtras = (await interactivity.WaitForReactionAsync(msgComandosExtras, ctx.User, TimeSpan.FromMinutes(5))).Result.Emoji;

                    if (emojiRespostaComandosExtras == emojiVoltar)
                    {
                        await msgComandosExtras.DeleteAsync();

                        goto inicioHelp;
                    }
                }
                else if (emojiResposta == comandosStaff)
                {
                    await primeiraMensagemEmbed.DeleteAsync();
                    await segundaMensagemEmbed.DeleteAsync();

                    StringBuilder strComandos = new StringBuilder();

                    foreach (Command comando in ctx.CommandsNext.RegisteredCommands.Values)
                    {
                        if (comando is CommandGroup grupo)
                        {
                            foreach (Command comandoNoGrupo in grupo.Children)
                            {
                                if (strComandos.ToString().Contains($"{ctx.Prefix}{(grupo.Aliases.Count == 0 ? grupo.Name : grupo.Aliases[0])} {(comandoNoGrupo.Aliases.Count != 0 ? comandoNoGrupo.Aliases[0] : comandoNoGrupo.Name)}"))
                                    break;

                                if (!comandoNoGrupo.QualifiedName.Contains("staff"))
                                    break;

                                if (!string.IsNullOrWhiteSpace(comandoNoGrupo.Description))
                                    strComandos.Append($"`{ctx.Prefix}{(grupo.Aliases.Count == 0 ? grupo.Name : grupo.Aliases[0])} {(comandoNoGrupo.Aliases.Count != 0 ? comandoNoGrupo.Aliases[0] : comandoNoGrupo.Name)} {comandoNoGrupo.Description}");
                            }
                        }
                    }

                    embed.WithAuthor("Comandos da staff:", null, Valores.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithDescription(strComandos.ToString())
                        .WithThumbnailUrl(ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now)
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    DiscordMessage msgStaff = await ctx.RespondAsync(embed: embed.Build());
                    await msgStaff.CreateReactionAsync(emojiVoltar);

                    DiscordEmoji emojiStaff = (await interactivity.WaitForReactionAsync(msgStaff, ctx.User, TimeSpan.FromMinutes(5))).Result.Emoji;

                    if (emojiStaff == emojiVoltar)
                    {
                        await msgStaff.DeleteAsync();

                        goto inicioHelp;
                    }
                }
                else if (emojiResposta == comandosReactRole)
                {
                    await primeiraMensagemEmbed.DeleteAsync();
                    await segundaMensagemEmbed.DeleteAsync();

                    StringBuilder strComandos = new StringBuilder();

                    foreach (Command comando in ctx.CommandsNext.RegisteredCommands.Values)
                    {
                        if (comando is CommandGroup grupo)
                        {
                            foreach (Command comandoNoGrupo in grupo.Children)
                            {
                                if (strComandos.ToString().Contains($"{ctx.Prefix}{(grupo.Aliases.Count == 0 ? grupo.Name : grupo.Aliases[0])} {(comandoNoGrupo.Aliases.Count != 0 ? comandoNoGrupo.Aliases[0] : comandoNoGrupo.Name)} {comandoNoGrupo.Description}"))
                                    break;

                                if (!comandoNoGrupo.QualifiedName.Contains("reactrole"))
                                    break;

                                strComandos.Append($"`{ctx.Prefix}{(grupo.Aliases.Count == 0 ? grupo.Name : grupo.Aliases[0])} {(comandoNoGrupo.Aliases.Count != 0 ? comandoNoGrupo.Aliases[0] : comandoNoGrupo.Name)} {comandoNoGrupo.Description}");
                            }
                        }
                    }

                    embed.WithAuthor("Comandos do react role:", null, Valores.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithDescription(strComandos.ToString())
                        .WithThumbnailUrl(ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now)
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    DiscordMessage msgReactRole = await ctx.RespondAsync(embed: embed.Build());
                    await msgReactRole.CreateReactionAsync(emojiVoltar);

                    DiscordEmoji emojiReactRole = (await interactivity.WaitForReactionAsync(msgReactRole, ctx.User, TimeSpan.FromMinutes(5))).Result.Emoji;

                    if (emojiReactRole == emojiVoltar)
                    {
                        await msgReactRole.DeleteAsync();

                        goto inicioHelp;
                    }
                }
                else if (emojiResposta == comandosModMail)
                {
                    await primeiraMensagemEmbed.DeleteAsync();
                    await segundaMensagemEmbed.DeleteAsync();

                    StringBuilder strComandos = new StringBuilder();

                    foreach (Command comando in ctx.CommandsNext.RegisteredCommands.Values)
                    {
                        if (comando is CommandGroup grupo)
                        {
                            foreach (Command comandoNoGrupo in grupo.Children)
                            {
                                if (strComandos.ToString().Contains($"{ctx.Prefix}{(grupo.Aliases.Count == 0 ? grupo.Name : grupo.Aliases[0])} {(comandoNoGrupo.Aliases.Count != 0 ? comandoNoGrupo.Aliases[0] : comandoNoGrupo.Name)} {comandoNoGrupo.Description}"))
                                    break;

                                if (!comandoNoGrupo.QualifiedName.Contains("modmail"))
                                    break;

                                strComandos.Append($"`{ctx.Prefix}{(grupo.Aliases.Count == 0 ? grupo.Name : grupo.Aliases[0])} {(comandoNoGrupo.Aliases.Count != 0 ? comandoNoGrupo.Aliases[0] : comandoNoGrupo.Name)} {comandoNoGrupo.Description}");
                            }
                        }
                    }

                    embed.WithAuthor("Comandos do ModMail:", null, Valores.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithDescription(strComandos.ToString())
                        .WithThumbnailUrl(ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now)
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    DiscordMessage msgComandosModMail = await ctx.RespondAsync(embed: embed.Build());
                    await msgComandosModMail.CreateReactionAsync(emojiVoltar);

                    DiscordEmoji emojiRespostaComandosModMail = (await interactivity.WaitForReactionAsync(msgComandosModMail, ctx.User, TimeSpan.FromMinutes(5))).Result.Emoji;

                    if (emojiRespostaComandosModMail == emojiVoltar)
                    {
                        await msgComandosModMail.DeleteAsync();

                        goto inicioHelp;
                    }
                }
                else if (emojiResposta == comandosDoador)
                {
                    await primeiraMensagemEmbed.DeleteAsync();
                    await segundaMensagemEmbed.DeleteAsync();

                    StringBuilder strComandos = new StringBuilder();

                    foreach (Command comando in ctx.CommandsNext.RegisteredCommands.Values)
                    {
                        if (comando is CommandGroup grupo)
                        {
                            foreach (Command comandoNoGrupo in grupo.Children)
                            {
                                if (strComandos.ToString().Contains($"{ctx.Prefix}{(grupo.Aliases.Count == 0 ? grupo.Name : grupo.Aliases[0])} {(comandoNoGrupo.Aliases.Count != 0 ? comandoNoGrupo.Aliases[0] : comandoNoGrupo.Name)} {comandoNoGrupo.Description}"))
                                    break;

                                if (!comandoNoGrupo.QualifiedName.Contains("doador"))
                                    break;

                                strComandos.Append($"`{ctx.Prefix}{(grupo.Aliases.Count == 0 ? grupo.Name : grupo.Aliases[0])} {(comandoNoGrupo.Aliases.Count != 0 ? comandoNoGrupo.Aliases[0] : comandoNoGrupo.Name)} {comandoNoGrupo.Description}");
                            }
                        }
                    }

                    embed.WithAuthor("Comandos para os doadores:", null, Valores.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithDescription(strComandos.ToString())
                        .WithThumbnailUrl(ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now)
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    DiscordMessage msgComandosDoador = await ctx.RespondAsync(embed: embed.Build());
                    await msgComandosDoador.CreateReactionAsync(emojiVoltar);

                    DiscordEmoji emojiRespostaComandosDoador = (await interactivity.WaitForReactionAsync(msgComandosDoador, ctx.User, TimeSpan.FromMinutes(5))).Result.Emoji;

                    if (emojiRespostaComandosDoador == emojiVoltar)
                    {
                        await msgComandosDoador.DeleteAsync();

                        goto inicioHelp;
                    }
                }
                else
                    return;
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

        [Command("ping")]

        public async Task PingAsync(CommandContext ctx)
            => await ctx.RespondAsync($"Meu ping é: **{ctx.Client.Ping}ms**! {await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "leofsjal")}");

        [Command("meuid")]

        public async Task MeuIDAsync(CommandContext ctx)
            => await ctx.RespondAsync($"{ctx.Member.Mention}, seu ID é: {ctx.Member.Id}.");

        [Command("uptime"), Aliases("tempobotligado")]

        public async Task UptimeBotAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            Process p = Process.GetCurrentProcess();

            TimeSpan dataFinal = DateTime.Now - p.StartTime;

            await ctx.RespondAsync($"Estou ligado há: {(dataFinal.Days == 0 ? string.Empty : $"{(dataFinal.Days > 1 ? $"**{dataFinal.Days} dias**, " : $"**{dataFinal.Days} dia**, ")}")}{(dataFinal.Hours == 0 ? string.Empty : $"{(dataFinal.Hours > 1 ? $"**{dataFinal.Hours} horas**, " : $"**{dataFinal.Hours} hora**, ")}")}{(dataFinal.Minutes == 0 ? string.Empty : $"{(dataFinal.Minutes > 1 ? $"**{dataFinal.Minutes} minutos** e " : $"**{dataFinal.Minutes} minuto** e ")}")}{(dataFinal.Seconds == 0 ? string.Empty : $"{(dataFinal.Seconds > 1 ? $"**{dataFinal.Seconds} segundos**" : $"**{dataFinal.Seconds} segundo**")}")}.");
        }

        [Command("avatar"), Aliases("foto")]

        public async Task AvatarMembroAsync(CommandContext ctx, DiscordMember membro = null)
        {
            await ctx.TriggerTypingAsync();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            if (membro == null)
            {
                embed.WithAuthor($"Aqui está seu avatar: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}#{ctx.Member.Discriminator}\"", null, Valores.logoUBGE)
                    .WithImageUrl(ctx.Member.GetAvatarUrl(ImageFormat.Png, 2048))
                    .WithDescription($"Para baixar, {Formatter.MaskedUrl("clique aqui", new Uri(ctx.Member.GetAvatarUrl(ImageFormat.Png, 2048)), "clique aqui")}.")
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: embed.Build(), content: ctx.Member.Mention);
            }
            else
            {
                embed.WithAuthor($"Avatar do membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\"", null, Valores.logoUBGE)
                    .WithImageUrl(membro.GetAvatarUrl(ImageFormat.Png, 2048))
                    .WithDescription($"Para baixar, {Formatter.MaskedUrl("clique aqui", new Uri(membro.GetAvatarUrl(ImageFormat.Png, 2048)), "clique aqui")}.")
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: embed.Build(), content: ctx.Member.Mention);
            }
        }

        [Command("userinfo"), Aliases("usuario", "usuário"), UBGE]

        public async Task UserInfoAsync(CommandContext ctx, DiscordMember membro = null)
        {
            await ctx.TriggerTypingAsync();

            if (membro == null)
                membro = ctx.Member;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();
            DiscordRole cargoMembroRegistrado = ctx.Guild.GetRole(Valores.Cargos.cargoMembroRegistrado);

            string status = string.Empty, statusDiscord = string.Empty, statusFinal = string.Empty;

            StringBuilder str = new StringBuilder();

            status = $"{await Program.ubgeBot.utilidadesGerais.ConverteStatusParaEmoji(ctx, membro)}- {Program.ubgeBot.utilidadesGerais.ConverteStatusPraNome(membro)}";
            statusDiscord = Program.ubgeBot.utilidadesGerais.ConverteStatusPraNome(membro);

            if (statusDiscord != "Offline" && membro.Presence != null)
            {
                statusDiscord = string.Empty;

                if (membro.Presence.Activities.Count == 1)
                {
                    if (membro.Presence.Activity.ActivityType == ActivityType.ListeningTo)
                    {
                        statusDiscord += $"Escutando no {membro.Presence.Activity.Name} a música: " +
                            $"**{membro.Presence.Activity.RichPresence.Details}** de **{membro.Presence.Activity.RichPresence.State}**, " +
                            $"do álbum: **{membro.Presence.Activity.RichPresence.LargeImageText}**";
                    }
                    else if (membro.Presence.Activity.ActivityType == ActivityType.Playing)
                        statusDiscord += $"Jogando: {(string.IsNullOrWhiteSpace(membro.Presence.Activity.Name) ? "**Nada no momento.**" : $"**{membro.Presence.Activity.Name}**{(string.IsNullOrWhiteSpace(membro.Presence.Activity.RichPresence?.Details) ? string.Empty : $"\n- **{membro.Presence.Activity.RichPresence?.Details}**")}{(string.IsNullOrWhiteSpace(membro.Presence.Activity.RichPresence?.State) ? string.Empty : $"\n- **{membro.Presence.Activity.RichPresence?.State}**")}")}";
                    else if (membro.Presence.Activity.ActivityType == ActivityType.Streaming)
                        statusDiscord += $"Streamando{(string.IsNullOrWhiteSpace(membro.Presence.Activity.Name) ? ": **Nada no momento.**" : $"na: **{membro.Presence.Activity.Name}**{(string.IsNullOrWhiteSpace(membro.Presence.Activity.RichPresence?.Details) ? string.Empty : $"\n- **{membro.Presence.Activity.RichPresence?.Details}**")}{(string.IsNullOrWhiteSpace(membro.Presence.Activity.StreamUrl) ? string.Empty : $"\n- {membro.Presence.Activity.StreamUrl}")}")}";
                    else if (membro.Presence.Activity.ActivityType == ActivityType.Watching)
                        statusDiscord += $"Assistindo: {(string.IsNullOrWhiteSpace(membro.Presence.Activity.Name) ? "**Nada no momento.**" : $"**{membro.Presence.Activity.Name}**")}";
                    else if (membro.Presence.Activity.ActivityType == ActivityType.Custom)
                        statusDiscord += $"Status personalizado: {(membro.Presence.Activity.CustomStatus == null ? "**Nada no momento.**" : $"**{(membro.Presence.Activity.CustomStatus.Emoji == null ? string.Empty : membro.Presence.Activity.CustomStatus.Emoji.ToString())}{(string.IsNullOrWhiteSpace(membro.Presence.Activity.CustomStatus.Name) ? string.Empty : $"{(membro.Presence.Activity.CustomStatus.Emoji == null ? string.Empty : " - ")}{membro.Presence.Activity.CustomStatus.Name}")}**")}";
                }
                else
                {
                    foreach (DiscordActivity atividade in membro.Presence.Activities)
                    {
                        if (atividade.ActivityType == ActivityType.ListeningTo)
                        {
                            statusDiscord += $"Escutando no {atividade.Name} a música: " +
                                $"**{atividade.RichPresence.Details}** de **{atividade.RichPresence.State}**, " +
                                $"do álbum: **{atividade.RichPresence.LargeImageText}**\n";
                        }
                        else if (atividade.ActivityType == ActivityType.Playing)
                            statusDiscord += $"Jogando: {(string.IsNullOrWhiteSpace(atividade.Name) ? "**Nada no momento.**" : $"**{atividade.Name}**{(string.IsNullOrWhiteSpace(atividade.RichPresence?.Details) ? string.Empty : $"\n- **{atividade.RichPresence?.Details}**")}{(string.IsNullOrWhiteSpace(atividade.RichPresence?.State) ? string.Empty : $"\n- **{atividade.RichPresence?.State}**")}")}\n";
                        else if (atividade.ActivityType == ActivityType.Streaming)
                            statusDiscord += $"Streamando{(string.IsNullOrWhiteSpace(atividade.Name) ? ": **Nada no momento.**" : $"na: **{atividade.Name}**{(string.IsNullOrWhiteSpace(atividade.RichPresence?.Details) ? string.Empty : $"\n- **{atividade.RichPresence?.Details}**")}{(string.IsNullOrWhiteSpace(atividade.StreamUrl) ? string.Empty : $"\n- {atividade.StreamUrl}")}")}\n";
                        else if (atividade.ActivityType == ActivityType.Watching)
                            statusDiscord += $"Assistindo: {(string.IsNullOrWhiteSpace(atividade.Name) ? "**Nada no momento.**" : $"**{atividade.Name}**")}\n";
                        else if (atividade.ActivityType == ActivityType.Custom)
                            statusDiscord += $"Status personalizado: {(atividade.CustomStatus == null ? "**Nada no momento.**" : $"**{(atividade.CustomStatus.Emoji == null ? string.Empty : atividade.CustomStatus.Emoji.ToString())}{(string.IsNullOrWhiteSpace(atividade.CustomStatus.Name) ? string.Empty : $"{(atividade.CustomStatus.Emoji == null ? string.Empty : " - ")}{atividade.CustomStatus.Name}")}**")}\n";
                    }
                }
            }
            else
                statusDiscord = string.Empty;

            foreach (DiscordRole cargosForeach in membro.Roles.OrderByDescending(x => x.Position))
                str.Append($"{cargosForeach.Mention} | ");

            str.Append($"\n\n{(membro.Roles.Count() > 1 ? $"**{membro.Roles.Count()}** cargos" : $"**{membro.Roles.Count()}** cargo")} ao total.");

            if (str.Length > 1024)
            {
                str.Clear();
                str.Append("Os cargos excederam o limite de 1024 caracteres.");
            }

            embed.WithAuthor($"{(membro == ctx.Member ? "Suas informações" : "Informações do membro")}: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\"", null, Valores.logoUBGE)
                .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                .AddField("Conta criada no dia:", $"{membro.CreationTimestamp.DateTime.ToString()} - Há **{(int)(DateTime.Now - membro.CreationTimestamp.DateTime).TotalDays}** dias")
                .AddField($"Entrou na {ctx.Guild.Name} no dia:", $"{membro.JoinedAt.DateTime.ToString()} - Há **{(int)(DateTime.Now - membro.JoinedAt.DateTime).TotalDays}** dias")
                .AddField("Cargos:", $"{(str.ToString() == "Os cargos excederam o limite de 1024 caracteres." ? $"{str.ToString()} Mas o membro tem {(membro.Roles.Count() > 1 ? $"**{membro.Roles.Count()}** cargos." : $"**{membro.Roles.Count()}** cargo.")}" : str.ToString())}")
                .AddField("Sala de Voz:", membro.VoiceState == null ? "Este membro não está em nenhum canal de voz." : membro.VoiceState.Channel.Name)
                .AddField("Membro Registrado?:", membro.Roles.Contains(cargoMembroRegistrado) ? "Sim" : "Não")
                .AddField("Status atual:", $"{status}{(statusDiscord == "Não especificado." ? string.Empty : $"\n\n{statusDiscord}")}")
                .AddField("Dono do servidor?:", membro.IsOwner ? $":crown: - **Sim**" : "**Não**")
                .AddField("Bot?:", membro.IsBot ? $"{await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "bot")} - **Sim**" : "**Não**")
                .WithThumbnailUrl(membro.AvatarUrl)
                .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                .WithTimestamp(DateTime.Now);

            await ctx.RespondAsync(embed: embed.Build());
        }

        [Command("dólar"), Aliases("dolar")]

        public async Task CotacaoDolarAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            JObject resposta = (JObject)JsonConvert.DeserializeObject(await Program.httpClient.GetStringAsync("https://api.hgbrasil.com/finance/quotations?format=json&key=27044a14"));

            JToken resultados = resposta.SelectToken("results"),
            currencies = resultados.SelectToken("currencies"),
            dolar = currencies.SelectToken("USD"),
            valorDolar = dolar.SelectToken("buy");

            DiscordEmbedBuilder Embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Valor do Dólar às: \"{DateTime.Now.ToString()}\": ${valorDolar} ou ${Math.Round(double.Parse(valorDolar.ToString()), 2)}", IconUrl = Valores.logoUBGE },
                Color = Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed(),
            };

            await ctx.RespondAsync(embed: Embed.Build());
        }

        [Command("euro")]

        public async Task CotacaoEuroAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            JObject resposta = (JObject)JsonConvert.DeserializeObject(await Program.httpClient.GetStringAsync("https://api.hgbrasil.com/finance/quotations?format=json&key=27044a14"));

            JToken Resultados = resposta.SelectToken("results"),
            currencies = Resultados.SelectToken("currencies"),
            euro = currencies.SelectToken("EUR"),
            valorEuro = euro.SelectToken("buy");

            DiscordEmbedBuilder Embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Valor do Euro às: \"{DateTime.Now.ToString()}\": ${valorEuro} ou ${Math.Round(double.Parse(valorEuro.ToString()), 2)}", IconUrl = Valores.logoUBGE },
                Color = Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed(),
            };

            await ctx.RespondAsync(embed: Embed.Build());
        }

        [Command("servidorinfo"), Aliases("guildinfo", "serverinfo")]

        public async Task ServidorInfoAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Informações do servidor: {ctx.Guild.Name}", IconUrl = Valores.logoUBGE },
                ThumbnailUrl = $"{ctx.Guild.IconUrl}?size=2048",
                Color = Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed(),
                Timestamp = DateTime.Now,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", IconUrl = ctx.Member.AvatarUrl },
            };

            embed.AddField("Criado no dia:", $"{ctx.Guild.CreationTimestamp.DateTime.ToString()} - **{(int)(DateTime.Now - ctx.Guild.CreationTimestamp.DateTime).TotalDays}** dias")
                .AddField("Dono:", ctx.Guild.Owner.Mention)
                .AddField("Quantidade de membros:", $"**{ctx.Guild.MemberCount}** membros")
                .AddField("Número de cargos:", $"**{ctx.Guild.Roles.Count}** cargos")
                .AddField("Cargo mais alto na hierarquia:", ctx.Guild.Roles.OrderByDescending(P => P.Value.Position).First().Value.Mention)
                .AddField("Quantidade de canais:", $"**{ctx.Guild.Channels.Count}** canais")
                .AddField("Quantidade de emojis:", $"**{ctx.Guild.Emojis.Count}** emojis")
                .AddField("ID:", ctx.Guild.Id.ToString());

            await ctx.RespondAsync(embed: embed.Build());
        }

        [Command("fotoservidor"), Aliases("servidorfoto", "avatarservidor", "servidoravatar", "avatarguild", "guildavatar")]

        public async Task AvatarServidorAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

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

        [Command("procuramembros")]

        public async Task ProcuraMembrosPeloNomeDoJogo(CommandContext ctx, [RemainingText] string jogo = null)
        {
            await ctx.TriggerTypingAsync();

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

            foreach (DiscordGuild servidor in ctx.Client.Guilds.Values)
            {
                foreach (DiscordMember membroForeach in servidor.Members.Values)
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
                            foreach (DiscordActivity JogoDiscord in membroForeach.Presence.Activities)
                            {
                                if (!string.IsNullOrWhiteSpace(JogoDiscord.Name) && JogoDiscord.Name.ToLower() == jogo.ToLower())
                                {
                                    if (membroForeach != ctx.Member)
                                        str.Append($"{(membroForeach.Guild != ctx.Guild ? $"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membroForeach)}#{membroForeach.Discriminator}" : Program.ubgeBot.utilidadesGerais.MencaoMembro(membroForeach))} - {membroForeach.Guild.Name} | ");
                                }
                            }
                        }
                        else if (membroForeach.Presence.Activity != null)
                        {
                            if (!string.IsNullOrWhiteSpace(membroForeach.Presence.Activity.Name) && membroForeach.Presence.Activity.Name.ToLower() == jogo.ToLower())
                            {
                                if (membroForeach != ctx.Member)
                                    str.Append($"{(membroForeach.Guild != ctx.Guild ? $"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membroForeach)}#{membroForeach.Discriminator}" : Program.ubgeBot.utilidadesGerais.MencaoMembro(membroForeach))} - {membroForeach.Guild.Name} | ");
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

        [Command("listar"), UBGE]

        public async Task ListarMembrosEmUmCanalDeVox(CommandContext ctx, DiscordChannel canal = null)
        {
            await ctx.TriggerTypingAsync();

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

            if (canal.Type != ChannelType.Voice)
            {
                embed.WithAuthor("Este canal não é valido!", null, Valores.logoUBGE)
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
                int i = 0;

                foreach (DiscordMember membro in canal.Users)
                    str.Append($"`{++i}.` - {Program.ubgeBot.utilidadesGerais.MencaoMembro(membro)}\n");
            }

            embed.WithAuthor($"Membros que estão no canal: \"{canal.Name}\"", null, ctx.Guild.IconUrl)
                .WithDescription(string.IsNullOrWhiteSpace(str.ToString()) ? "Nenhum há membro está nesse canal." : $"{str.ToString()}\nEsse canal contêm {(canal.Users.Count() > 1 ? $"**{canal.Users.Count()}** membros." : $"**{canal.Users.Count()}** membro.")}")
                .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                .WithThumbnailUrl(ctx.Member.AvatarUrl)
                .WithTimestamp(DateTime.Now)
                .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

            await ctx.RespondAsync(embed: embed.Build());
        }
    }
}