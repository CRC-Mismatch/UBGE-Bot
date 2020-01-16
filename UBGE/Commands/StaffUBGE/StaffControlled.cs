using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Log = UBGE.Logger.Logger;
using UBGE.MongoDB.Models;
using UBGE.Services.Google;
using UBGE.Utilities;

namespace UBGE.Commands.StaffUBGE
{
    [Group("staff"), Aliases("s", "ubge"), UBGEStaff]

    public sealed class StaffControlled : BaseCommandModule
    {
        CancellationTokenSource cancellationTokenSource = null;
        GoogleSheetsService googleSheetsRead { get; set; } = new GoogleSheetsService();

        [Command("check"), Aliases("c"), ConnectedToMongo, Description("Membro[ID/Menção]`\nCheca se um membro pode ter o cargo de Membro Registrado e mostra informações extras.\n\n")]

        public async Task CheckAsync(CommandContext ctx, DiscordMember membro = null)
        {
            DiscordMessage msgAguarde = null;

            try
            {
                await ctx.TriggerTypingAsync();

                if (membro == null)
                    membro = ctx.Member;

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                DiscordEmoji addMembroRegistrado = DiscordEmoji.FromName(ctx.Client, ":large_blue_circle:"),
                cancelaEmbed = DiscordEmoji.FromName(ctx.Client, ":red_circle:"),
                removerMembroRegistrado = DiscordEmoji.FromName(ctx.Client, ":x:");

                InteractivityExtension interact = ctx.Client.GetInteractivity();

                DiscordRole membroRegistrado = ctx.Guild.GetRole(Values.Roles.roleMembroRegistrado);

                string estado = string.Empty, comoChegouAUBGE = string.Empty, idade = string.Empty, idiomas = string.Empty,
                    jogosMaisJogados = string.Empty, builderFezCenso = string.Empty, diasMembroEntrou = string.Empty,
                    diasContaCriada = string.Empty, statusMembro = string.Empty, builder = string.Empty, fezOCensoNoDia = string.Empty,
                    estadoMembro = string.Empty, comoChegouAUBGEMembro = string.Empty, idadeMembro = string.Empty, jogosMaisJogadosMembro = string.Empty,
                    idiomasMembro = string.Empty, fezOCensoNoDiaMembro = string.Empty;

                bool membroEncontrado = false, nicknameCerto = false;

                int index = 0, anosQuePassaram = 0, nForeach = 0;

                IMongoDatabase local = Program.Bot.LocalDB;

                IMongoCollection<Infracao> collectionInfracao = local.GetCollection<Infracao>(Values.Mongo.infracoes);
                FilterDefinition<Infracao> filtroInfracao = Builders<Infracao>.Filter.Eq(xm => xm.idInfrator, membro.Id);
                List<Infracao> listaInfracao = await (await collectionInfracao.FindAsync(filtroInfracao)).ToListAsync();

                IMongoCollection<Censo> collectionCenso = local.GetCollection<Censo>(Values.Mongo.censo);
                FilterDefinition<Censo> filtroCenso = Builders<Censo>.Filter.Eq(x => x.idNoDiscord, membro.Id);
                List<Censo> listaCenso = await (await collectionCenso.FindAsync(filtroCenso)).ToListAsync();

                StringBuilder str = new StringBuilder(), strCargos = new StringBuilder(), strServidores = new StringBuilder();

                if (listaInfracao.Count == 0)
                    str.Append("Este membro não possui nenhuma infração.");
                else
                {
                    foreach (Infracao i in listaInfracao)
                    {
                        index = listaInfracao.IndexOf(i);
                        builder += $"**{++index}ª** - {i.motivoInfracao}\n";
                    }

                    if (index == 0)
                        str.Append($"Este membro não possui infrações.");
                    else if (index == 1)
                        str.Append($"Este membro possui **{index}** infração:\n\n{builder}");
                    else if (index > 1)
                        str.Append($"Este membro possui **{index}** infrações:\n\n{builder}");
                }

                if (listaCenso.Count != 0)
                    goto membroFezOCensoNoBot;

                msgAguarde = await ctx.RespondAsync($"Buscando dados na planilha, aguarde... {Program.Bot.Utilities.FindEmoji(ctx, "leofsjal")}");

                IList<IList<object>> respostaPlanilhaCenso = await googleSheetsRead.ReadSheet(Program.Bot.BotConfig.GoogleAPIConfig.CensusID, Program.Bot.BotConfig.GoogleAPIConfig.CensusRange);

                IReadOnlyCollection<DiscordMember> membrosUBGE = await ctx.Guild.GetAllMembersAsync();

                List<DiscordMember> membroNoCenso = new List<DiscordMember>();

                foreach (IList<object> resposta in respostaPlanilhaCenso)
                {
                    string nickDoMembro = string.Empty;

                    try
                    {
                        nickDoMembro = resposta[3].ToString().ToLower().Split('#')[0];

                        if (nickDoMembro == membro.Nickname?.ToLower())
                            nicknameCerto = true;

                        if (!nicknameCerto)
                        {
                            if (nickDoMembro == membro.Username?.ToLower())
                            { }
                            else
                                continue;
                        }

                        membroEncontrado = true;

                        fezOCensoNoDiaMembro = resposta[0].ToString();
                        comoChegouAUBGEMembro = resposta[2].ToString();
                        idadeMembro = resposta[4].ToString();
                        jogosMaisJogadosMembro = resposta[5].ToString();
                        estadoMembro = resposta[6].ToString();

                        if (resposta[7] != null)
                            idiomasMembro = resposta[7].ToString();
                    }
                    catch (Exception)
                    {
                        if (string.IsNullOrWhiteSpace(nickDoMembro))
                            continue;
                    }

                    if (membroEncontrado)
                        break;
                }

                await msgAguarde.DeleteAsync();

            membroFezOCensoNoBot:
                {
                    membroEncontrado = true;

                    foreach (Censo censo in listaCenso)
                    {
                        estadoMembro = censo.estado;
                        comoChegouAUBGEMembro = censo.chegouNaUBGE;
                        idadeMembro = censo.idade.ToString();
                        idiomasMembro = censo.idiomas;
                        jogosMaisJogadosMembro = censo.jogosMaisJogados;
                        fezOCensoNoDiaMembro = censo.timestamp.ToString();
                    }
                }

                builderFezCenso = membroEncontrado ? "Sim" : "Não";

                estado = string.IsNullOrWhiteSpace(estadoMembro) ? "Não especificado." : estadoMembro;
                comoChegouAUBGE = string.IsNullOrWhiteSpace(comoChegouAUBGEMembro) ? "Não especificado." : comoChegouAUBGEMembro;
                idade = string.IsNullOrWhiteSpace(idadeMembro) ? "Não especificado." : idadeMembro;
                idiomas = string.IsNullOrWhiteSpace(idiomasMembro) ? "Não especificado." : idiomasMembro;
                jogosMaisJogados = string.IsNullOrWhiteSpace(jogosMaisJogadosMembro) ? "Não especificado." : jogosMaisJogadosMembro;
                fezOCensoNoDia = string.IsNullOrWhiteSpace(fezOCensoNoDiaMembro) ? "Não especificado." : fezOCensoNoDiaMembro;

                diasMembroEntrou = (int)(DateTime.Now - membro.JoinedAt.DateTime).TotalDays == 0 ? "**Hoje**" : $"**{(int)(DateTime.Now - membro.JoinedAt.DateTime).TotalDays}** dias";
                diasContaCriada = (int)(DateTime.Now - membro.CreationTimestamp.DateTime).TotalDays == 0 ? "**Hoje**" : $"**{(int)(DateTime.Now - membro.CreationTimestamp.DateTime).TotalDays}** dias";

                foreach (DiscordRole cargo in membro.Roles.Where(x => x != null).OrderByDescending(x => x.Position))
                    strCargos.Append($"{cargo.Mention} | ");

                strCargos.Append($"\n\n{(membro.Roles.Count() > 1 ? $"**{membro.Roles.Count()}** cargos ao total." : $"**{membro.Roles.Count()}** cargo ao total.")}");

                if (strCargos.Length > 1024)
                {
                    strCargos.Clear();
                    strCargos.Append("Os cargos excederam o limite de 1024 caracteres.");
                }

                statusMembro = Program.Bot.Utilities.StatusToName(membro);

                foreach (DiscordGuild servidoresBot in ctx.Client.Guilds.Values)
                {
                    if (servidoresBot.Members.Keys.Contains(membro.Id) && servidoresBot.Id != Values.Guilds.guildUBGE)
                    {
                        strServidores.Append($"{servidoresBot.Name}, ");

                        ++nForeach;
                    }
                }

                if (string.IsNullOrWhiteSpace(strServidores.ToString()))
                    strServidores.Append("Este membro não está em nenhum outro servidor que eu estou.");

                estado = Program.Bot.Utilities.ReturnState(estado);

                if (fezOCensoNoDia != "Não especificado.")
                    anosQuePassaram = int.Parse((DateTime.Now.Year - DateTime.Parse(fezOCensoNoDia).Year).ToString());

                DiscordMessage msgEmbed = null;

                if (membro.Roles.Contains(membroRegistrado))
                {
                    string footer = $"Para remover o cargo de Membro Registrado > {removerMembroRegistrado} | " +
                        $"Para cancelar > {cancelaEmbed} - Vermelha";

                    embed.WithAuthor($"Informações do membro: \"{Program.Bot.Utilities.DiscordNick(membro)}#{membro.Discriminator}\"", null, Values.logoUBGE)
                        .WithThumbnailUrl(membro.AvatarUrl)
                        .WithColor(ctx.Member.Color)
                        .AddField($"Entrou na {ctx.Guild.Name} em:", $"{membro.JoinedAt.ToString("dd/MM/yyyy HH:mm:ss tt")} - {diasMembroEntrou}", false)
                        .AddField("Conta criada em:", $"{membro.CreationTimestamp.ToString("dd/MM/yyyy HH:mm:ss tt")} - {diasContaCriada}", false)
                        .AddField("Fez o censo comunitário?:", builderFezCenso == "Sim" ? "**Sim**" : "**Não** → `(Acesse o link da planilha do censo para procurar o membro manualmente. Para isso aperte Ctrl + F e pesquise o nome do membro.)`", false)
                        .AddField("Membro registrado?:", "**Sim**", false)
                        .AddField("Infrações:", str.ToString(), false)
                        .AddField("Status:", $"{(membro.IsBot ? Program.Bot.Utilities.FindEmoji(ctx, "Bot") : "")}{(membro.IsOwner ? ":crown:" : "")}{Program.Bot.Utilities.StatusToEmoji(ctx, membro)}- {statusMembro}", false)
                        .AddField("Está em mais algum outro servidor? (Em que eu estou):", $"{(strServidores.ToString().EndsWith(", ") ? strServidores.ToString().Remove(strServidores.ToString().Length - 2) : strServidores.ToString())}", false)
                        .AddField("Respostas do censo:", $"Fez o censo no dia: **{fezOCensoNoDia}**\n\n" +
                        $"Estado: **{estado}**\n\n" +
                        $"Idade: {(idade == "Não especificado." ? "**Não especificado.**" : $"**{idade} anos** (No censo)")} {(idade == "Não especificado." ? string.Empty : $"{(anosQuePassaram == 0 ? "A diferença de anos entre hoje e a data que foi feita o censo é zero. Por isso a idade é a mesma." : $"| **{int.Parse(idade) + anosQuePassaram} anos** (__Aproximado__, pois na teoria a idade atual deste membro é esta.)")}")}\n\n" +
                        $"Como chegou a UBGE: **{comoChegouAUBGE}**\n\n" +
                        $"Idiomas: **{idiomas}**\n\n" +
                        $"Jogos mais jogados: **{jogosMaisJogados}**", false)
                        .AddField("Cargos:", $"{(strCargos.ToString() == "Os cargos excederam o limite de 1024 caracteres." ? $"{strCargos.ToString()}, mas o membro tem **{membro.Roles.Count()} cargos.**" : strCargos.ToString())}", false)
                        .WithFooter(membro.IsBot ? footer = string.Empty : footer)
                        .WithTimestamp(DateTime.Now);

                    msgEmbed = await ctx.RespondAsync(embed: embed.Build());

                    if (membro.IsBot)
                        return;

                    await msgEmbed.CreateReactionAsync(removerMembroRegistrado);
                    await msgEmbed.CreateReactionAsync(cancelaEmbed);
                }
                else
                {
                    string footer = $"Para adicionar o cargo de Membro Registrado > {addMembroRegistrado} | " +
                        $"Para cancelar > {cancelaEmbed} | Para remover o cargo de Membro Registrado > {removerMembroRegistrado}";

                    embed.WithAuthor($"Informações do membro: \"{Program.Bot.Utilities.DiscordNick(membro)}#{membro.Discriminator}\"", null, Values.logoUBGE)
                        .WithThumbnailUrl(membro.AvatarUrl)
                        .WithColor(ctx.Member.Color)
                        .AddField($"Entrou na {ctx.Guild.Name} em:", $"{membro.JoinedAt.ToString("dd/MM/yyyy HH:mm:ss tt")} - **{diasMembroEntrou}**", false)
                        .AddField("Conta criada em:", $"{membro.CreationTimestamp.ToString("dd/MM/yyyy HH:mm:ss tt")} - **{diasContaCriada}**", false)
                        .AddField("Fez o censo comunitário?:", builderFezCenso == "Sim" ? "**Sim**" : "**Não** → `(Acesse o link da planilha do censo para procurar o membro manualmente. Para isso aperte Ctrl + F e pesquise o nome do membro.)`", false)
                        .AddField("Membro registrado?:", "**Não**", false)
                        .AddField("Infrações:", str.ToString(), false)
                        .AddField("Status:", $"{(membro.IsBot ? Program.Bot.Utilities.FindEmoji(ctx, "Bot") : "")}{(membro.IsOwner ? ":crown:" : "")}{Program.Bot.Utilities.StatusToEmoji(ctx, membro)}- {statusMembro}", false)
                        .AddField("Está em mais algum outro servidor? (Em que eu estou):", $"{(strServidores.ToString().EndsWith(", ") ? strServidores.ToString().Remove(strServidores.ToString().Length - 2) : strServidores.ToString())}", false)
                        .AddField("Respostas do Censo:", $"Fez o censo no dia: **{fezOCensoNoDia}**\n\n" +
                        $"Estado: **{estado}**\n\n" +
                        $"Idade: {(idade == "Não especificado." ? "**Não especificado.**" : $"**{idade} anos** (No censo)")} {(idade == "Não especificado." ? string.Empty : $"{(anosQuePassaram == 0 ? "A diferença de anos entre hoje e a data que foi feita o censo é zero. Por isso a idade é a mesma." : $"| **{int.Parse(idade) + anosQuePassaram} anos** (__Aproximado__, pois na teoria a idade atual deste membro é esta.)")}")}\n\n" +
                        $"Como chegou a UBGE: **{comoChegouAUBGE}**\n\n" +
                        $"Idiomas: **{idiomas}**\n\n" +
                        $"Jogos mais jogados: **{jogosMaisJogados}**", false)
                        .AddField("Cargos:", $"{(strCargos.ToString() == "Os cargos excederam o limite de 1024 caracteres." ? $"{str.ToString()}, mas o membro tem **{membro.Roles.Count()} cargos.**" : strCargos.ToString())}", false)
                        .WithFooter(membro.IsBot ? footer = string.Empty : footer)
                        .WithTimestamp(DateTime.Now);

                    msgEmbed = await ctx.RespondAsync(embed: embed.Build());

                    if (membro.IsBot)
                        return;

                    if ((int)(DateTime.Now - membro.JoinedAt.DateTime).TotalDays >= 7 && builderFezCenso == "Sim")
                        await msgEmbed.CreateReactionAsync(addMembroRegistrado);

                    await msgEmbed.CreateReactionAsync(cancelaEmbed);
                }

                DiscordEmoji emojo = (await interact.WaitForReactionAsync(msgEmbed, ctx.User, TimeSpan.FromMinutes(5))).Result?.Emoji;

                if (emojo == addMembroRegistrado)
                {
                    Program.Bot.Utilities.ClearEmbed(embed);

                    await msgEmbed.DeleteAllReactionsAsync();

                    embed.WithAuthor($"Cargo adicionado com sucesso!", null, Values.logoUBGE)
                        .WithThumbnailUrl(membro.AvatarUrl)
                        .WithColor(membroRegistrado.Color)
                        .WithDescription($"O cargo de: {membroRegistrado.Mention} foi adicionado com sucesso no membro: {Program.Bot.Utilities.MemberMention(membro)}.")
                        .WithFooter($"✅ Cargo Adicionado ao {Program.Bot.Utilities.DiscordNick(membro)}#{membro.Discriminator} com sucesso!");

                    await membro.GrantRoleAsync(membroRegistrado);
                    await msgEmbed.ModifyAsync(embed: embed.Build());
                }
                else if (emojo == cancelaEmbed)
                    await msgEmbed.DeleteAllReactionsAsync();
                else if (emojo == removerMembroRegistrado)
                {
                    Program.Bot.Utilities.ClearEmbed(embed);

                    await msgEmbed.DeleteAllReactionsAsync();

                    embed.WithAuthor($"Cargo removido com sucesso!", null, Values.logoUBGE)
                        .WithThumbnailUrl(membro.AvatarUrl)
                        .WithColor(DiscordColor.Red)
                        .WithDescription($"O cargo de: {membroRegistrado.Mention} foi removido com sucesso de: {Program.Bot.Utilities.MemberMention(membro)}.")
                        .WithFooter($"❌ Cargo Removido de {Program.Bot.Utilities.DiscordNick(membro)}#{membro.Discriminator} com sucesso!");

                    await membro.RevokeRoleAsync(membroRegistrado);
                    await msgEmbed.ModifyAsync(embed: embed.Build());
                }
                else
                    return;
            }
            catch (NotFoundException)
            {
                await ctx.RespondAsync($"{ctx.Member.Mention}, por motivos desconhecidos, o Discord não encontrou este membro.");
            }
            catch (Exception exception)
            {
                await Program.Bot.Logger.Error(Log.TypeError.Commands, exception);

                await msgAguarde.ModifyAsync("Ocorreu um erro ao decorrer deste comando, por favor execute ele novamente mais tarde. :cry:");
            }
        }

        [Command("infracoes"), Aliases("i", "infrações"), ConnectedToMongo, Description("Membro[ID] Extra[Add/Log] | Add[Infração]`\nAdd: Prende um membro por tempo indeterminado, Log: Mostra as infrações do membro.\n\n")]

        public async Task InfracoesAsync(CommandContext ctx, string membroId = null, string addtive = null, [RemainingText] string infracao = null)
        {
            try
            {
                await ctx.TriggerTypingAsync();

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                DiscordMember membro = null;

                ulong membroNull = 0;

                IMongoCollection<Infracao> infracaoDB = Program.Bot.LocalDB.GetCollection<Infracao>(Values.Mongo.infracoes);

                List<Infracao> lista = new List<Infracao>();

                if (!string.IsNullOrWhiteSpace(membroId))
                {
                    if (!(membroId.Contains("<@") && membroId.Contains(">") || membroId.Contains("<@!") && membroId.Contains(">")))
                    {
                        ulong.TryParse(membroId, out ulong membro_);

                        if (membro_ != 0 && ctx.Guild.Members.Keys.Contains(membro_))
                        {
                            membro = await ctx.Guild.GetMemberAsync(membro_);

                            foreach (Infracao infracao_ in await (await infracaoDB.FindAsync(Builders<Infracao>.Filter.Eq(x => x.idInfrator, membro == null ? membro_ : membro.Id))).ToListAsync())
                                lista.Add(infracao_);

                            membroNull = 1;
                        }
                        else if (membro_ != 0 && (await (await infracaoDB.FindAsync(Builders<Infracao>.Filter.Eq(x => x.idInfrator, membro == null ? membro_ : membro.Id))).ToListAsync()).Count != 0)
                        {
                            foreach (Infracao infracao_ in await (await infracaoDB.FindAsync(Builders<Infracao>.Filter.Eq(x => x.idInfrator, membro == null ? membro_ : membro.Id))).ToListAsync())
                                lista.Add(infracao_);

                            membroNull = membro_;
                        }
                        else
                        {
                            embed.WithColor(Program.Bot.Utilities.RandomColorEmbed())
                                .WithAuthor("Como executar este comando:", null, Values.infoLogo)
                                .AddField("PC/Mobile", $"{ctx.Prefix}s i Membro[ID/Menção] add[Infração] ou log[Ver infrações]")
                                .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                                .WithTimestamp(DateTime.Now);

                            await ctx.RespondAsync(embed: embed.Build());
                            return;
                        }
                    }
                    else
                    {
                        IReadOnlyList<DiscordUser> membrosMencionados = ctx.Message.MentionedUsers;

                        if (membrosMencionados.Count == 1)
                        {
                            membro = await ctx.Guild.GetMemberAsync(membrosMencionados.LastOrDefault().Id);

                            foreach (Infracao infracao_ in await (await infracaoDB.FindAsync(Builders<Infracao>.Filter.Eq(x => x.idInfrator, membro.Id))).ToListAsync())
                                lista.Add(infracao_);

                            membroNull = 1;
                        }
                        else
                        {
                            embed.WithColor(Program.Bot.Utilities.HelpCommandsColor())
                                .WithAuthor("Como executar este comando:", null, Values.infoLogo)
                                .AddField("PC/Mobile", $"{ctx.Prefix}s i Membro[ID/Menção] add[Infração] ou log[Ver infrações]")
                                .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                                .WithTimestamp(DateTime.Now);

                            await ctx.RespondAsync(embed: embed.Build());
                            return;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(membroId) || membroNull == 0)
                {
                    embed.WithColor(Program.Bot.Utilities.HelpCommandsColor())
                        .WithAuthor("Como executar este comando:", null, Values.infoLogo)
                        .AddField("PC/Mobile", $"{ctx.Prefix}s i Membro[ID/Menção] add[Infração] ou log[Ver infrações]")
                        .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    await ctx.RespondAsync(embed: embed.Build());
                    return;
                }

                if (string.IsNullOrWhiteSpace(addtive))
                {
                    embed.WithColor(Program.Bot.Utilities.HelpCommandsColor())
                        .WithAuthor("Como executar este comando:", null, Values.infoLogo)
                        .AddField("PC/Mobile", $"{ctx.Prefix}s i Membro[ID/Menção] add[Infração] ou log[Ver infrações]")
                        .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    await ctx.RespondAsync(embed: embed.Build());
                    return;
                }

                InteractivityExtension interact = ctx.Client.GetInteractivity();

                DiscordRole prisioneiro = ctx.Guild.GetRole(Values.Roles.rolePrisioneiro),
                cargoModeracaoDiscord = ctx.Guild.GetRole(Values.Roles.roleModeradorDiscord);

                ConcurrentBag<DiscordRole> cargosMembro = new ConcurrentBag<DiscordRole>();

                if (membro != null)
                {
                    foreach (DiscordRole cargo in membro.Roles)
                        cargosMembro.Add(cargo);
                }

                DiscordEmoji emojo = Program.Bot.Utilities.FindEmoji(ctx, "leofsjal");

                List<DiscordChannel> canaisUBGE = ctx.Guild.Channels.Values.ToList();

                DiscordChannel centroReabilitacao = ctx.Guild.GetChannel(Values.Chats.channelCentroDeReabilitacao),
                logChat = ctx.Guild.GetChannel(Values.Chats.channelLog),
                canalModeracaoDiscord = ctx.Guild.GetChannel(Values.Chats.channelModeracaoDiscord);

                DiscordEmoji marcarSim = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"),
                marcarNao = DiscordEmoji.FromName(ctx.Client, ":negative_squared_cross_mark:"),
                pastaDeArquivo = DiscordEmoji.FromName(ctx.Client, ":file_folder:");

                List<ulong> cargosLista = new List<ulong>();

                if (addtive.ToLowerInvariant() == "add")
                {
                    Infracao ni = new Infracao
                    {
                        idInfrator = membro.Id,
                        idStaff = ctx.Member.Id,
                        motivoInfracao = infracao,
                    };

                    embed.WithAuthor($"Preparando...", null, Values.logoUBGE)
                        .WithDescription($"Para enviar: {membro.Mention} para o {centroReabilitacao.Mention}, aperte: ✅\n" +
                        $"Para CANCELAR, aperte: ❎\n" +
                        $"Para somente adicionar a infração, aperte: :file_folder:")
                        .WithFooter("✅ Sim / Não ❎ / Armazenar Infração 📁")
                        .WithColor(ctx.Member.Color)
                        .WithTimestamp(DateTime.Now)
                        .WithThumbnailUrl(membro.AvatarUrl)
                        .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    DiscordMessage msgEmbed = await ctx.RespondAsync(embed: embed.Build());
                    await msgEmbed.CreateReactionAsync(marcarSim);
                    await msgEmbed.CreateReactionAsync(marcarNao);
                    await msgEmbed.CreateReactionAsync(pastaDeArquivo);

                    DiscordEmoji emoji = (await interact.WaitForReactionAsync(msgEmbed, ctx.User)).Result?.Emoji;

                    if (emoji == marcarSim)
                    {
                        ni.dataInfracao = DateTime.Now.ToString();
                        ni.oMembroFoiPreso = true;

                        await msgEmbed.DeleteAllReactionsAsync();

                        Program.Bot.Utilities.ClearEmbed(embed);

                        embed.WithAuthor($"O membro: \"{Program.Bot.Utilities.DiscordNick(membro)}\" foi preso com sucesso!", null, Values.logoUBGE)
                            .WithColor(prisioneiro.Color)
                            .WithTimestamp(DateTime.Now)
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithDescription(emojo)
                            .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                        foreach (DiscordRole cargo in cargosMembro)
                        {
                            await membro.RevokeRoleAsync(cargo);

                            cargosLista.Add(cargo.Id);
                        }

                        LogPrisao log = new LogPrisao
                        {
                            cargosDoMembro = cargosLista,
                        };

                        ni.dadosPrisao = log;

                        if (lista.Count == 4)
                            await canalModeracaoDiscord.SendMessageAsync($"{cargoModeracaoDiscord.Mention}, atenção! O membro {membro.Mention} está indo para o {centroReabilitacao.Mention} pela 5ª vez!");
                        else if (lista.Count > 10)
                            await canalModeracaoDiscord.SendMessageAsync($"{cargoModeracaoDiscord.Mention}, o membro {membro.Mention} está indo para o {centroReabilitacao.Mention} e já ultrapassou 10 infrações! __**POR FAVOR**__, marquem uma reunião com este membro/conversem/enviem um ultimato a este membro. *Não aguento mais prender esse cara.*");
                        else if (lista.Count > 5)
                            await canalModeracaoDiscord.SendMessageAsync($"{cargoModeracaoDiscord.Mention}, o membro {membro.Mention} está indo para o {centroReabilitacao.Mention} e já ultrapassou do limite de 5 infrações, façam algo mais severo!");

                        await infracaoDB.InsertOneAsync(ni);

                        await msgEmbed.ModifyAsync(embed: embed.Build());

                        await membro.GrantRoleAsync(prisioneiro);

                        Program.Bot.Utilities.ClearEmbed(embed);

                        embed.WithColor(prisioneiro.Color)
                            .WithAuthor($"Você foi preso pelo membro: {Program.Bot.Utilities.DiscordNick(ctx.Member)}!")
                            .WithDescription($"Sua infração: {infracao}")
                            .WithTimestamp(DateTime.Now)
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                        await centroReabilitacao.SendMessageAsync(embed: embed.Build(), content: membro.Mention);

                        Program.Bot.Utilities.ClearEmbed(embed);

                        embed.WithColor(Program.Bot.Utilities.RandomColorEmbed())
                            .WithAuthor($"O membro: {Program.Bot.Utilities.DiscordNick(membro)} foi levado para o {centroReabilitacao.Mention}.", null, Values.logoUBGE)
                            .WithDescription($"Infração: {infracao}")
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithTimestamp(DateTime.Now)
                            .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                        await logChat.SendMessageAsync(embed: embed.Build());
                    }
                    else if (emoji == marcarNao)
                    {
                        await msgEmbed.DeleteAllReactionsAsync();

                        Program.Bot.Utilities.ClearEmbed(embed);

                        embed.WithAuthor($"Comando concluído!", null, Values.logoUBGE)
                            .WithColor(prisioneiro.Color)
                            .WithTimestamp(DateTime.Now)
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithDescription($"O membro: {membro.Mention} não foi enviado pra gulag.")
                            .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                        await msgEmbed.ModifyAsync(embed: embed.Build());
                    }
                    else if (emoji == pastaDeArquivo)
                    {
                        ni.dataInfracao = DateTime.Now.ToString();
                        ni.oMembroFoiPreso = false;

                        await msgEmbed.DeleteAllReactionsAsync();

                        Program.Bot.Utilities.ClearEmbed(embed);

                        embed.WithAuthor($"A infração foi armazenada com sucesso!", null, Values.logoUBGE)
                            .WithColor(prisioneiro.Color)
                            .WithTimestamp(DateTime.Now)
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithDescription($"Membro: {membro.Mention}\n\nInfração: {infracao}\n\n{emojo}")
                            .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                        foreach (DiscordRole cargo in cargosMembro)
                            cargosLista.Add(cargo.Id);

                        LogPrisao log = new LogPrisao
                        {
                            cargosDoMembro = cargosLista,
                        };

                        ni.dadosPrisao = log;

                        if (lista.Count == 4)
                            await canalModeracaoDiscord.SendMessageAsync($"{cargoModeracaoDiscord.Mention}, atenção! Foi armazenada uma infração do membro {membro.Mention} pela 5ª vez!");
                        else if (lista.Count > 10)
                            await canalModeracaoDiscord.SendMessageAsync($"{cargoModeracaoDiscord.Mention}, o membro {membro.Mention} já ultrapassou 10 infrações! __**POR FAVOR**__, marquem uma reunião com este membro/conversem/enviem um ultimato a este membro. *Não aguento mais prender esse cara.*");
                        else if (lista.Count > 5)
                            await canalModeracaoDiscord.SendMessageAsync($"{cargoModeracaoDiscord.Mention}, o membro {membro.Mention} já ultrapassou do limite de 5 infrações, façam algo mais severo!");

                        await infracaoDB.InsertOneAsync(ni);

                        await msgEmbed.ModifyAsync(embed: embed.Build());
                    }
                    else
                        return;
                }
                else if (addtive.ToLowerInvariant() == "log")
                {
                    if (lista.Count == 0)
                    {
                        embed.WithThumbnailUrl(membro == null ? string.Empty : membro.AvatarUrl)
                            .WithColor(DiscordColor.Green)
                            .WithAuthor($"O usuário: \"{(membro == null ? "Desconhecido#0000" : Program.Bot.Utilities.DiscordNick(membro))}\" possui uma ficha limpa!", null, Values.logoUBGE)
                            .WithTimestamp(DateTime.Now)
                            .WithDescription(emojo)
                            .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                    else
                    {
                        if (lista.Count == 0)
                            embed.WithAuthor($"O usuário: \"{(membro == null ? "Desconhecido#0000" : Program.Bot.Utilities.DiscordNick(membro))}\" não possui infração.", null, Values.logoUBGE);
                        else if (lista.Count == 1)
                            embed.WithAuthor($"O usuário: \"{(membro == null ? "Desconhecido#0000" : Program.Bot.Utilities.DiscordNick(membro))}\" possui {lista.Count} infração.", null, Values.logoUBGE);
                        else if (lista.Count >= 2)
                            embed.WithAuthor($"O usuário: \"{(membro == null ? "Desconhecido#0000" : Program.Bot.Utilities.DiscordNick(membro))}\" possui {lista.Count} infrações.", null, Values.logoUBGE);

                        embed.WithThumbnailUrl(membro == null ? string.Empty : membro.AvatarUrl)
                            .WithColor(prisioneiro.Color);

                        StringBuilder strCargos = new StringBuilder();

                        foreach (Infracao infra in lista)
                        {
                            int nForeach = lista.IndexOf(infra);

                            if (infra.dadosPrisao.cargosDoMembro != null)
                            {
                                foreach (ulong cargosMembroForeach in infra.dadosPrisao.cargosDoMembro)
                                    strCargos.Append($"<@&{cargosMembroForeach}> | ");
                            }

                            if (strCargos.Length >= 700)
                            {
                                strCargos.Clear();
                                strCargos.Append("**Limite de caracteres atingido.**");
                            }

                            string Conversao = infra.dataInfracao.ToString();

                            if (Conversao.Contains("T") || Conversao.Contains("Z"))
                                Conversao = Convert.ToDateTime(Conversao).ToString();

                            if (infra.dadosPrisao.tempoDoMembroNaPrisao == "Sem dados")
                                infra.dadosPrisao.tempoDoMembroNaPrisao = string.Empty;

                            if (string.IsNullOrWhiteSpace(infra.dadosPrisao.tempoDoMembroNaPrisao))
                                infra.dadosPrisao.tempoDoMembroNaPrisao = "Não especificado.";

                            embed.AddField($"{++nForeach} - {infra.motivoInfracao}", $"Punido por: {Program.Bot.Utilities.MemberMention(await ctx.Guild.GetMemberAsync(infra.idStaff))}\n" +
                                $"Data: **{Conversao}**\n" +
                                $"Foi preso: **{(infra.oMembroFoiPreso ? "Sim" : "Não")}**\n" +
                                $"Tempo: **{(!string.IsNullOrWhiteSpace(infra.dadosPrisao.tempoDoMembroNaPrisao) ? infra.dadosPrisao.tempoDoMembroNaPrisao : "Não especificado.")}**\n" +
                                $"Cargos: {(!string.IsNullOrWhiteSpace(strCargos.ToString()) ? strCargos.ToString() : "**Não especificado.**")}");

                            strCargos.Clear();
                        }

                        embed.WithTimestamp(DateTime.Now)
                            .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                }
                else
                {
                    embed.WithColor(DiscordColor.Red)
                        .WithAuthor($"Erro! Comando não reconhecido!", null, Values.logoUBGE)
                        .WithThumbnailUrl(ctx.Member.AvatarUrl)
                        .WithDescription($"Comandos disponíveis:\n\n`{ctx.Prefix}s i {(membro == null ? $"{membroId}" : $"{membro.Id}")} add`\n`{ctx.Prefix}s i {(membro == null ? $"{membroId}" : $"{membro.Id}")} log`")
                        .WithTimestamp(DateTime.Now)
                        .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    await ctx.RespondAsync(embed: embed.Build());
                }
            }
            catch (NotFoundException)
            {
                await ctx.RespondAsync($"{ctx.Member.Mention}, por motivos desconhecidos, o Discord não encontrou este membro.");
            }
            catch (Exception exception)
            {
                await Program.Bot.Logger.Error(Log.TypeError.Commands, exception);
            }
        }

        [Command("clear"), Aliases("apagar", "clean", "limpar", "limparchat"), Description("Nº de Mensagens[1/100] Membro[Opcional]`\nApaga as mensagens do chat que foi executado o comando, pode ser de um membro específico ou a quantidade que foi colocada no comando.\n\n")]

        public async Task ApagarMessagensAsync(CommandContext ctx, int numeroMensagens, DiscordMember membro = null)
        {
            try
            {
                InteractivityExtension interactivity = ctx.Client.GetInteractivity();

                if (membro == null)
                {
                    if (numeroMensagens < 0 || numeroMensagens == 0)
                        return;

                    for (int i = 1; i < numeroMensagens + 2; i++)
                        await (await ctx.Channel.GetMessagesAsync(i)).FirstOrDefault().DeleteAsync();
                }
                else
                {
                    await ctx.Message.DeleteAsync();

                    List<DiscordMessage> msgMembro = (await ctx.Channel.GetMessagesAsync(100)).Where(x => x.Author.Id == membro.Id).ToList();

                    if (numeroMensagens < 100)
                    {
                        for (int i = 0; i < numeroMensagens; i++)
                            await msgMembro[i].DeleteAsync();
                    }
                    else if (numeroMensagens > 100)
                        await (await ctx.Member.CreateDmChannelAsync()).SendMessageAsync($"O limite de mensagens que eu posso pegar para excluir é 100!");
                    else
                        return;
                }
            }
            catch (Exception exception)
            {
                await Program.Bot.Logger.Error(Log.TypeError.Commands, exception);
            }
        }

        [Command("apagarinfração"), Aliases("ai", "deletarinfração", "di", "removerinfração", "ri"), ConnectedToMongo, Description("Membro[ID ou Menção] Infração[Motivo]`\nApaga uma determinada infração do membro.\n\n")]

        public async Task DeletarInfracoesAsync(CommandContext ctx, DiscordMember membro = null, [RemainingText] string infracao = null)
        {
            try
            {
                await ctx.TriggerTypingAsync();

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                if (membro == null)
                {
                    embed.WithColor(Program.Bot.Utilities.RandomColorEmbed())
                            .WithAuthor("Como executar este comando:", null, Values.infoLogo)
                            .AddField("PC/Mobile", $"{ctx.Prefix}s ai Membro[Menção/ID] Infração[Motivo]")
                            .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                    await ctx.RespondAsync(embed: embed.Build());
                    return;
                }

                IMongoCollection<Infracao> infracaoDB = Program.Bot.LocalDB.GetCollection<Infracao>(Values.Mongo.infracoes);

                FilterDefinition<Infracao> filtro = Builders<Infracao>.Filter.Eq(x => x.motivoInfracao, infracao);
                List<Infracao> lista = await (await infracaoDB.FindAsync(filtro)).ToListAsync();

                DiscordEmoji emojo = Program.Bot.Utilities.FindEmoji(ctx, "leofsjal");

                if (lista.Count == 0)
                {
                    embed.WithColor(Program.Bot.Utilities.RandomColorEmbed())
                        .WithAuthor("Infração não encontrada e/ou este membro não tem infrações.")
                        .WithThumbnailUrl(membro.AvatarUrl)
                        .WithTimestamp(DateTime.Now)
                        .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    await ctx.RespondAsync(embed: embed.Build());
                }
                else
                {
                    await infracaoDB.DeleteOneAsync(filtro);

                    embed.WithColor(DiscordColor.Green)
                        .WithAuthor("Infração apagada com sucesso!")
                        .WithTimestamp(DateTime.Now)
                        .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                        .WithThumbnailUrl(membro.AvatarUrl)
                        .WithDescription($"Infração apagada: {infracao}\n\nBoa 06, fatiou passou... {emojo}");

                    await ctx.RespondAsync(embed: embed.Build());
                }
            }
            catch (NotFoundException)
            {
                await ctx.RespondAsync($"{ctx.Member.Mention}, por motivos desconhecidos, o Discord não encontrou este membro.");
            }
            catch (Exception exception)
            {
                await Program.Bot.Logger.Error(Log.TypeError.Commands, exception);
            }
        }

        [Command("list"), Aliases("check", "lists"), ConnectedToMongo, Description("`\nLista e dá o cargo automaticamente de membro registrado para os membros que tem + de 7 dias na UBGE.\n\n")]

        public async Task ListAsync(CommandContext ctx)
        {
            try
            {
                await ctx.TriggerTypingAsync();

                DiscordMessage msgAguarde = await ctx.RespondAsync($"Aguarde... {Program.Bot.Utilities.FindEmoji(ctx, "leofsjal")}");

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                await msgAguarde.ModifyAsync($"Buscando dados na planilha... {Program.Bot.Utilities.FindEmoji(ctx, "leofsjal")}");

                IList<IList<object>> respostaPlanilhaCenso = await googleSheetsRead.ReadSheet(Program.Bot.BotConfig.GoogleAPIConfig.CensusID, Program.Bot.BotConfig.GoogleAPIConfig.CensusRange);

                await msgAguarde.ModifyAsync($"Busca de dados concluída! Fazendo checagem... {Program.Bot.Utilities.FindEmoji(ctx, "leofsjal")}");

                List<DiscordMember> membrosTotaisUBGE = (await ctx.Guild.GetAllMembersAsync()).ToList();

                DiscordRole cargoMembroRegistrado = ctx.Guild.GetRole(Values.Roles.roleMembroRegistrado),
                cargoPrisioneiro = ctx.Guild.GetRole(Values.Roles.rolePrisioneiro);

                StringBuilder strFinal = new StringBuilder(),
                    strMembrosRepetidos = new StringBuilder();

                DiscordMember membro = null;

                string[] nickDoMembro = null;

                IMongoDatabase db = Program.Bot.LocalDB;
                IMongoCollection<Censo> colecaoCenso = db.GetCollection<Censo>(Values.Mongo.censo);

                List<Censo> respostaCenso = await (await colecaoCenso.FindAsync(Builders<Censo>.Filter.Empty)).ToListAsync();

                foreach (IList<object> resposta in respostaPlanilhaCenso)
                {
                    try
                    {
                        nickDoMembro = resposta[3].ToString().ToLower().Split('#');

                        List<DiscordMember> membroDiscriminatorUBGE = membrosTotaisUBGE.FindAll(x => x.Discriminator == nickDoMembro[1]);

                        List<DiscordMember> membroUsernameUBGE = membroDiscriminatorUBGE.FindAll(x => !string.IsNullOrWhiteSpace(x.Username) && x.Username.ToLower().Contains(nickDoMembro[0]));
                        List<DiscordMember> membroNicknameUBGE = membroDiscriminatorUBGE.FindAll(x => !string.IsNullOrWhiteSpace(x.Nickname) && x.Nickname.ToLower().Contains(nickDoMembro[0]));

                        if (membroUsernameUBGE != null && membroUsernameUBGE.Count == 1)
                        {
                            membro = membroUsernameUBGE.LastOrDefault();

                            if ((DateTime.Now - membro.JoinedAt).TotalDays >= 7 && !membro.Roles.Contains(cargoMembroRegistrado) && !membro.Roles.Contains(cargoPrisioneiro))
                            {
                                await membro.GrantRoleAsync(cargoMembroRegistrado);

                                strFinal.Append($"{Program.Bot.Utilities.MemberMention(membro)} - Nome no Discord: **{Program.Bot.Utilities.DiscordNick(membro)}#{membro.Discriminator}** - ID: `{membro.Id}` - {(int)(DateTime.Now - membro.JoinedAt).TotalDays} dias |\n");
                            }
                        }
                        else if (membroUsernameUBGE != null && membroUsernameUBGE.Count > 1)
                        {
                            if (strMembrosRepetidos.ToString().Contains($"{Program.Bot.Utilities.MemberMention(membro)} - Nome no Discord: **{Program.Bot.Utilities.DiscordNick(membro)}#{membro.Discriminator}** - ID: `{membro.Id}` |"))
                            {
                                foreach (DiscordMember membroForeach in membroUsernameUBGE)
                                    strMembrosRepetidos.Append($"{Program.Bot.Utilities.MemberMention(membro)} - Nome no Discord: **{Program.Bot.Utilities.DiscordNick(membro)}#{membro.Discriminator}** - ID: `{membro.Id}` |\n");
                            }
                        }
                        else if (membroNicknameUBGE != null && membroNicknameUBGE.Count == 1)
                        {
                            membro = membroNicknameUBGE.LastOrDefault();

                            if ((DateTime.Now - membro.JoinedAt).TotalDays >= 7 && !membro.Roles.Contains(cargoMembroRegistrado) && !membro.Roles.Contains(cargoPrisioneiro))
                            {
                                await membro.GrantRoleAsync(cargoMembroRegistrado);

                                strFinal.Append($"{Program.Bot.Utilities.MemberMention(membro)} - Nome no Discord: **{Program.Bot.Utilities.DiscordNick(membro)}#{membro.Discriminator}** - ID: `{membro.Id}` - {(int)(DateTime.Now - membro.JoinedAt).TotalDays} dias |\n");
                            }
                        }
                        else if (membroNicknameUBGE != null && membroNicknameUBGE.Count > 1)
                        {
                            if (strMembrosRepetidos.ToString().Contains($"{Program.Bot.Utilities.MemberMention(membro)} - Nome no Discord: **{Program.Bot.Utilities.DiscordNick(membro)}#{membro.Discriminator}** - ID: `{membro.Id}` |"))
                            {
                                foreach (DiscordMember membroForeach in membroNicknameUBGE)
                                    strMembrosRepetidos.Append($"{Program.Bot.Utilities.MemberMention(membro)} - Nome no Discord: **{Program.Bot.Utilities.DiscordNick(membro)}#{membro.Discriminator}** - ID: `{membro.Id}` |\n");
                            }
                        }
                        else
                            continue;
                    }
                    catch (Exception) { }
                }

                membro = null;

                foreach (Censo censoBot in respostaCenso)
                {
                    try
                    {
                        membro = await ctx.Guild.GetMemberAsync(censoBot.idNoDiscord);

                        if (censoBot.fezOCenso && (DateTime.Now - membro.JoinedAt).TotalDays >= 7 && !membro.Roles.Contains(cargoMembroRegistrado) && !membro.Roles.Contains(cargoPrisioneiro))
                        {
                            await membro.GrantRoleAsync(cargoMembroRegistrado);

                            strFinal.Append($"{Program.Bot.Utilities.MemberMention(membro)} - Nome no Discord: **{Program.Bot.Utilities.DiscordNick(membro)}#{membro.Discriminator}** - ID: `{membro.Id}` - {(int)(DateTime.Now - membro.JoinedAt).TotalDays} dias |\n");
                        }
                    }
                    catch (Exception) { }
                }

                await msgAguarde.ModifyAsync($"Preparando embed... {Program.Bot.Utilities.FindEmoji(ctx, "leofsjal")}");

                embed.WithAuthor($"Lista de membros que foi adicionado o cargo de Membro Registrado:", null, Values.logoUBGE);

                int MembrosTotaisCargoMembroRegistrado = membrosTotaisUBGE.FindAll(xm => xm.Roles.ToList().Contains(cargoMembroRegistrado)).Count;

                decimal Porcentagem = Math.Round(((decimal)MembrosTotaisCargoMembroRegistrado * 100) / membrosTotaisUBGE.Count, 2);

                await msgAguarde.DeleteAsync();

                embed.AddField($"Verificação realizada às: {DateTime.Now.ToString()}", string.IsNullOrWhiteSpace(strFinal.ToString()) ? $"Bom Trabalho!\nNão há nenhum cargo que não foi adicionado! {Program.Bot.Utilities.FindEmoji(ctx, ":UBGE:")}" : $"{(strFinal.ToString().Length > 1024 ? "A quantidade máxima de 1024 caracteres foi excedida." : strFinal.ToString())}")
                    .AddField("Membros que foram encontrados duplicados:", string.IsNullOrWhiteSpace(strMembrosRepetidos.ToString()) ? $"Não houve membros duplicados." : $"{(strMembrosRepetidos.ToString().Length > 1024 ? "A quantidade máxima de 1024 caracteres foi excedida." : $"Fazer a checagem manual na planilha dos membros:\n{strMembrosRepetidos.ToString()}")}")
                    .WithFooter($"{Porcentagem}% da {ctx.Guild.Name} tem o cargo de Membro Registrado")
                    .WithTimestamp(DateTime.Now)
                    .WithColor(new DiscordColor(0x32363c));

                await ctx.RespondAsync(embed: embed.Build());
            }
            catch (Exception exception)
            {
                await Program.Bot.Logger.Error(Log.TypeError.Commands, exception);
            }
        }

        [Command("mute"), Aliases("m"), ConnectedToMongo, Description("Tempo[Xs/Xm/Xh/Xd] Membro[ID/Menção] Infração[Motivo]`\nMuta um membro específico por um tempo.\n\n")]

        public async Task MuteAsync(CommandContext ctx, string tempo = null, DiscordMember membro = null, [RemainingText] string infracao = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    if (string.IsNullOrWhiteSpace(tempo))
                    {
                        embed.WithColor(Program.Bot.Utilities.HelpCommandsColor())
                            .WithAuthor("Como executar este comando:", null, Values.infoLogo)
                            .AddField("PC/Mobile", $"{ctx.Prefix}s m Tempo[Xs/Xm/Xh/Xd] Membro[ID/Menção] Infração[Motivo]")
                            .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build(), content: $"{ctx.Member.Mention}, digite o tempo da prisão deste membro!");
                        return;
                    }

                    if (!(tempo.Contains("s") || tempo.Contains("m") || tempo.Contains("h") || tempo.Contains("d")))
                    {
                        embed.WithColor(Program.Bot.Utilities.HelpCommandsColor())
                            .WithAuthor("Como executar este comando:", null, Values.infoLogo)
                            .AddField("PC/Mobile", $"{ctx.Prefix}s m Tempo[Xs/Xm/Xh/Xd] Membro[ID/Menção] Infração[Motivo]")
                            .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build(), content: $"{ctx.Member.Mention}, digite o sufixo do tempo! `[s, m, h, d]`");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(infracao))
                    {
                        embed.WithColor(Program.Bot.Utilities.HelpCommandsColor())
                            .WithAuthor("Como executar este comando:", null, Values.infoLogo)
                            .AddField("PC/Mobile", $"{ctx.Prefix}s m Tempo[Xs/Xm/Xh/Xd] Membro[ID/Menção] Infração[Motivo]")
                            .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build(), content: $"{ctx.Member.Mention}, digite a infração deste membro!");
                        return;
                    }

                    DiscordRole prisioneiroCargo = ctx.Guild.GetRole(Values.Roles.rolePrisioneiro),
                    cargoModeracaoDiscord = ctx.Guild.GetRole(Values.Roles.roleModeradorDiscord);

                    IMongoDatabase db = Program.Bot.LocalDB;
                    IMongoCollection<Infracao> collectionInfracoes = db.GetCollection<Infracao>(Values.Mongo.infracoes);

                    FilterDefinition<Infracao> filtro = Builders<Infracao>.Filter.Eq(x => x.idInfrator, membro.Id);
                    List<Infracao> listaInfracoes = await (await collectionInfracoes.FindAsync(filtro)).ToListAsync();

                    if (membro.Roles.Contains(prisioneiroCargo))
                    {
                        embed.WithColor(prisioneiroCargo.Color)
                            .WithAuthor($"O membro: \"{Program.Bot.Utilities.DiscordNick(membro)}#{membro.Discriminator}\" já está na prisão!", null, Values.logoUBGE)
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithDescription($"Infração: {listaInfracoes.LastOrDefault().motivoInfracao}\n\n" +
                            $"Tempo preso: {listaInfracoes.LastOrDefault().dadosPrisao.tempoDoMembroNaPrisao}\n\n" +
                            $"Dia e hora da infração: {listaInfracoes.LastOrDefault().dataInfracao}\n\n" +
                            $"Membro da staff que aplicou a punição: {(await ctx.Guild.GetMemberAsync(listaInfracoes.LastOrDefault().idStaff)).Mention}")
                            .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    DiscordChannel centroReabilitacao = ctx.Guild.GetChannel(Values.Chats.channelCentroDeReabilitacao),
                    logChat = ctx.Guild.GetChannel(Values.Chats.channelLog),
                    canalModeracaoDiscord = ctx.Guild.GetChannel(Values.Chats.channelModeracaoDiscord);

                    if (listaInfracoes.Count == 4)
                        await canalModeracaoDiscord.SendMessageAsync($"{cargoModeracaoDiscord.Mention}, atenção! O membro {membro.Mention} está indo para o {centroReabilitacao.Mention} pela 5ª vez!");
                    else if (listaInfracoes.Count > 10)
                        await canalModeracaoDiscord.SendMessageAsync($"{cargoModeracaoDiscord.Mention}, o membro {membro.Mention} está indo para o {centroReabilitacao.Mention} e já ultrapassou 10 infrações! __**POR FAVOR**__, marquem uma reunião com este membro/conversem/enviem um ultimato a este membro. *Não aguento mais prender esse cara.*");
                    else if (listaInfracoes.Count > 5)
                        await canalModeracaoDiscord.SendMessageAsync($"{cargoModeracaoDiscord.Mention}, o membro {membro.Mention} está indo para o {centroReabilitacao.Mention} e já ultrapassou do limite de 5 infrações, façam algo mais severo!");

                    List<DiscordRole> cargosMembro = membro.Roles.ToList();

                    TimeSpan tempoDaPrisao = Program.Bot.Utilities.ConvertTime(tempo);
                    DateTime horaAtual = DateTime.Now;
                    string horarioDeSaida = horaAtual.Add(tempoDaPrisao).ToString();

                    List<ulong> cargos = new List<ulong>();

                    if (cargosMembro.Count() != 0)
                    {
                        foreach (DiscordRole cargosForeach in cargosMembro)
                        {
                            cargos.Add(cargosForeach.Id);

                            await Task.Delay(200);

                            if (cargosForeach.Id != Values.Roles.roleNitroBooster)
                                await membro.RevokeRoleAsync(cargosForeach);
                        }
                    }

                    await membro.GrantRoleAsync(prisioneiroCargo);

                    if (membro.VoiceState?.Channel != null)
                    {
                        DiscordChannel canalDeVozCentroDeReabilitacao = ctx.Guild.GetChannel(Values.Chats.channelDeVozCentroDeReabilitacao);

                        await membro.PlaceInAsync(canalDeVozCentroDeReabilitacao);
                    }

                    await collectionInfracoes.InsertOneAsync(new Infracao
                    {
                        dataInfracao = DateTime.Now.ToString(),
                        idInfrator = membro.Id,
                        idStaff = ctx.Member.Id,
                        motivoInfracao = infracao,
                        oMembroFoiPreso = true,
                        dadosPrisao = new LogPrisao
                        {
                            cargosDoMembro = cargos,
                            tempoDoMembroNaPrisao = tempo,
                        },
                    });

                    embed.WithColor(ctx.Member.Color)
                        .WithAuthor($"O membro: \"{Program.Bot.Utilities.DiscordNick(membro)}#{membro.Discriminator}\" foi mutado.", null, Values.logoUBGE)
                        .WithThumbnailUrl(membro.AvatarUrl)
                        .WithTimestamp(DateTime.Now)
                        .WithDescription($"Infração: **{infracao}**\n\n" +
                        $"Tempo: **{tempo}**\n\n" +
                        $"Horário da Entrada na Prisão: **{horaAtual}**\n\n" +
                        $"Horário de Saída da Prisão: **{horarioDeSaida}**\n\n" +
                        $"Menção do Membro: {membro.Mention}\n\n" +
                        $"Membro da Staff que Mutou: {ctx.Member.Mention}")
                        .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    await ctx.RespondAsync(embed: embed.Build());

                    Program.Bot.Utilities.ClearEmbed(embed);

                    embed.WithColor(ctx.Member.Color)
                        .WithAuthor($"O membro: \"{Program.Bot.Utilities.DiscordNick(membro)}#{membro.Discriminator}\" foi mutado.", null, Values.logoUBGE)
                        .WithThumbnailUrl(membro.AvatarUrl)
                        .WithTimestamp(DateTime.Now)
                        .WithDescription($"Infração: **{infracao}**\n\n" +
                        $"Tempo: **{tempo}**\n\n" +
                        $"Horário da Entrada na Prisão: **{horaAtual}**\n\n" +
                        $"Horário de Saída da Prisão: **{horarioDeSaida}**\n\n" +
                        $"Menção do Membro: {membro.Mention}\n\n" +
                        $"Membro da Staff que Mutou: {ctx.Member.Mention}")
                        .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    await logChat.SendMessageAsync(embed: embed.Build());

                    Program.Bot.Utilities.ClearEmbed(embed);

                    embed.WithColor(prisioneiroCargo.Color)
                        .WithAuthor($"Você foi mutado por: {tempo}!", null, Values.logoUBGE)
                        .WithDescription($"Sua infração: **{infracao}**\n\n" +
                        $"Horário de Saída: **{horarioDeSaida}**\n\n" +
                        $"Membro da staff que lhe mutou: {ctx.Member.Mention}")
                        .WithTimestamp(DateTime.Now)
                        .WithThumbnailUrl(membro.AvatarUrl);

                    await centroReabilitacao.SendMessageAsync(embed: embed.Build(), content: membro.Mention);

                    cancellationTokenSource = new CancellationTokenSource();

                    await Task.Delay(tempoDaPrisao, cancellationTokenSource.Token);

                    await membro.RevokeRoleAsync(prisioneiroCargo);

                    foreach (DiscordRole cargosMembroForeach in cargosMembro)
                    {
                        await Task.Delay(200);

                        await membro.GrantRoleAsync(cargosMembroForeach);
                    }

                    Program.Bot.Utilities.ClearEmbed(embed);

                    embed.WithColor(ctx.Member.Color)
                        .WithAuthor($"O membro: \"{Program.Bot.Utilities.DiscordNick(membro)}#{membro.Discriminator}\" foi desmutado.", null, Values.logoUBGE)
                        .WithThumbnailUrl(membro.AvatarUrl)
                        .WithTimestamp(DateTime.Now)
                        .WithDescription($"Horário de Saída: **{horarioDeSaida}**\n\n" +
                        $"Menção do Membro: {membro.Mention}\n\n" +
                        $"Membro da Staff que Mutou: {ctx.Member.Mention}")
                        .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    await ctx.RespondAsync(embed: embed.Build());

                    Program.Bot.Utilities.ClearEmbed(embed);

                    embed.WithColor(Program.Bot.Utilities.RandomColorEmbed())
                        .WithAuthor($"O membro: \"{Program.Bot.Utilities.DiscordNick(membro)}#{membro.Discriminator}\" foi desmutado.", null, Values.logoUBGE)
                        .WithThumbnailUrl(membro.AvatarUrl)
                        .WithDescription($"Horário de Saída: {horarioDeSaida}\n\n" +
                        $"Menção do Membro: {membro.Mention}\n\n" +
                        $"Membro da Staff que Mutou: {ctx.Member.Mention}")
                        .WithTimestamp(DateTime.Now);

                    await logChat.SendMessageAsync(embed: embed.Build());
                }
                catch (TaskCanceledException) { }
                catch (Exception exception)
                {
                    await Program.Bot.Logger.Error(Log.TypeError.Commands, exception);
                }
            }).Start();
        }

        [Command("devolvercargos"), Aliases("dc"), ConnectedToMongo, Description("Membro[ID/Menção]`\nDevolve os cargos de um membro que foi preso, caso o bot caia durante a prisão do mesmo. **SÓ EXECUTE ESTE COMANDO CASO O BOT TENHA CAÍDO E FICADO OFFLINE.**\n\n")]

        public async Task DevolverCargosAsync(CommandContext ctx, DiscordMember membro = null)
        {
            try
            {
                await ctx.TriggerTypingAsync();

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                if (membro == null)
                {
                    embed.WithColor(Program.Bot.Utilities.HelpCommandsColor())
                        .WithAuthor("Como executar este comando:", null, Values.infoLogo)
                        .AddField("PC/Mobile", $"{ctx.Prefix}s dc Membro[ID/Menção]")
                        .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    await ctx.RespondAsync(embed: embed.Build());
                    return;
                }

                IMongoDatabase local = Program.Bot.LocalDB;
                IMongoCollection<Infracao> collectionInfracoes = local.GetCollection<Infracao>(Values.Mongo.infracoes);

                FilterDefinition<Infracao> filtro = Builders<Infracao>.Filter.Eq(x => x.idInfrator, membro.Id);
                List<Infracao> listaInfracoes = await (await collectionInfracoes.FindAsync(filtro)).ToListAsync();

                DiscordRole prisioneiroCargo = ctx.Guild.GetRole(Values.Roles.rolePrisioneiro), cargo = null;

                if (listaInfracoes.Count == 0)
                {
                    embed.WithColor(DiscordColor.Red)
                        .WithAuthor($"O membro não contem infrações!", null, Values.logoUBGE)
                        .WithDescription(":thinking:")
                        .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    await ctx.RespondAsync(embed: embed.Build());
                }
                else
                {
                    if (membro.Roles.Contains(prisioneiroCargo))
                        await membro.RevokeRoleAsync(prisioneiroCargo);

                    ulong cargoUlong = 0;
                    StringBuilder strCargos = new StringBuilder();

                    foreach (ulong cargoForeach in listaInfracoes.LastOrDefault().dadosPrisao.cargosDoMembro)
                    {
                        cargoUlong = Convert.ToUInt64(cargoForeach);
                        cargo = ctx.Guild.GetRole(cargoUlong);

                        await Task.Delay(200);

                        await membro.GrantRoleAsync(cargo);

                        strCargos.Append($"{cargo.Mention} | ");
                    }

                    embed.WithColor(DiscordColor.Green)
                        .WithAuthor($"Cargos do membro: \"{Program.Bot.Utilities.DiscordNick(membro)}\" devolvidos com sucesso!", null, Values.logoUBGE)
                        .WithDescription($"Cargos: {strCargos.ToString()}")
                        .WithThumbnailUrl(membro.AvatarUrl)
                        .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    await ctx.RespondAsync(embed: embed.Build());
                }
            }
            catch (NotFoundException)
            {
                await ctx.RespondAsync($"{ctx.Member.Mention}, por motivos desconhecidos, o Discord não encontrou este membro.");
            }
            catch (Exception exception)
            {
                await Program.Bot.Logger.Error(Log.TypeError.Commands, exception);
            }
        }

        [Command("unmute"), Aliases("desmutar"), ConnectedToMongo, Description("Membro[ID/Menção]`\nCancela o mute do membro e devolve os cargos. **SÓ EXECUTE ESTE COMANDO SE O BOT ESTIVER ONLINE DESDE A PRISÃO DO MEMBRO.**\n\n")]

        public async Task UnmuteAsync(CommandContext ctx, DiscordMember membro = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    if (membro == null)
                    {
                        embed.WithColor(Program.Bot.Utilities.HelpCommandsColor())
                            .WithAuthor("Como executar este comando:", null, Values.infoLogo)
                            .AddField("PC/Mobile", $"{ctx.Prefix}s unmute Membro[ID/Menção]")
                            .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    DiscordRole cargoPrisioneiro = ctx.Guild.GetRole(Values.Roles.rolePrisioneiro);

                    if (!membro.Roles.Contains(cargoPrisioneiro))
                    {
                        embed.WithColor(cargoPrisioneiro.Color)
                            .WithAuthor("Este membro não está na prisão!", null, Values.logoUBGE)
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithDescription(":thinking:")
                            .WithColor(Program.Bot.Utilities.RandomColorEmbed())
                            .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    IMongoDatabase local = Program.Bot.LocalDB;
                    IMongoCollection<Infracao> collectionInfracao = local.GetCollection<Infracao>(Values.Mongo.infracoes);

                    FilterDefinition<Infracao> filtro = Builders<Infracao>.Filter.Eq(m => m.idInfrator, membro.Id);
                    List<Infracao> listaInfracoes = await (await collectionInfracao.FindAsync(filtro)).ToListAsync();

                    if (listaInfracoes.Count == 0)
                    {
                        embed.WithAuthor($"Este membro não contêm infrações.", null, Values.logoUBGE)
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithDescription(":thinking:")
                            .WithColor(Program.Bot.Utilities.RandomColorEmbed())
                            .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                    else
                    {
                        try
                        {
                            cancellationTokenSource.Cancel();
                        }
                        catch (Exception)
                        {
                            embed.WithAuthor($"Erro!", null, Values.logoUBGE)
                                .WithThumbnailUrl(membro.AvatarUrl)
                                .WithDescription($"O bot provavelmente deve ter caído depois que esta prisão foi efetuada, caso queira somente devolver os " +
                                $"cargos do membro: {membro.Mention}, digite: `{ctx.Prefix}dc {membro.Id}`. {Program.Bot.Utilities.FindEmoji(ctx, "leofsjal")}")
                                .WithColor(Program.Bot.Utilities.RandomColorEmbed())
                                .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                                .WithTimestamp(DateTime.Now);

                            await ctx.RespondAsync(embed: embed.Build());
                            return;
                        }

                        DiscordMessage msgAguarde = await ctx.RespondAsync("Aguarde...");

                        if (membro.Roles.Contains(cargoPrisioneiro))
                            await membro.RevokeRoleAsync(cargoPrisioneiro);

                        DiscordRole cargoParaDevolver = null;

                        StringBuilder strCargos = new StringBuilder();

                        foreach (ulong cargosForeach in listaInfracoes.LastOrDefault().dadosPrisao.cargosDoMembro)
                        {
                            cargoParaDevolver = ctx.Guild.GetRole(cargosForeach);

                            await Task.Delay(200);

                            await membro.GrantRoleAsync(cargoParaDevolver);

                            strCargos.Append($"{cargoParaDevolver.Mention} | ");
                        }

                        embed.WithColor(Program.Bot.Utilities.RandomColorEmbed())
                            .WithAuthor($"Todos os cargos do membro: \"{Program.Bot.Utilities.DiscordNick(membro)}\" foram devolvidos com sucesso!", null, Values.logoUBGE)
                            .WithDescription($"Cargos: {strCargos.ToString()}\n\n**PS:** A prisão do membro foi cancelada, mas a infração ainda está guardada.")
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await msgAguarde.DeleteAsync();
                        await ctx.RespondAsync(embed: embed.Build());

                        cancellationTokenSource.Dispose();
                    }
                }
                catch (NotFoundException)
                {
                    await ctx.RespondAsync($"{ctx.Member.Mention}, por motivos desconhecidos, o Discord não encontrou este membro.");
                }
                catch (Exception exception)
                {
                    await Program.Bot.Logger.Error(Log.TypeError.Commands, exception);
                }
            }).Start();
        }

        [Command("prisioneiros"), Aliases("prisioneiro"), Description("`\nMostra todos os membros que estão com o cargo de prisioneiro.\n\n")]

        public async Task ListarPrisioneirosAsync(CommandContext ctx)
        {
            try
            {
                await ctx.TriggerTypingAsync();

                DiscordRole prisioneiroCargo = ctx.Guild.GetRole(Values.Roles.rolePrisioneiro);

                StringBuilder strPrisioneiros = new StringBuilder();

                IEnumerable<DiscordMember> membrosUBGE = (await ctx.Guild.GetAllMembersAsync()).ToList().Where(Membro => Membro.Roles.Contains(prisioneiroCargo));

                foreach (DiscordMember membro in membrosUBGE)
                    strPrisioneiros.Append($"{Program.Bot.Utilities.MemberMention(membro)} - {Program.Bot.Utilities.DiscordNick(membro)}#{membro.Discriminator} - ID: `{membro.Id}`\n");

                await ctx.TriggerTypingAsync();

                DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { Name = "Membros que estão na prisão:", IconUrl = Values.logoUBGE },
                    Description = string.IsNullOrWhiteSpace(strPrisioneiros.ToString()) ? "Não há membros na prisão." : strPrisioneiros.ToString(),
                    ThumbnailUrl = ctx.Member.AvatarUrl,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}" },
                    Color = Program.Bot.Utilities.RandomColorEmbed(),
                };

                await ctx.RespondAsync(embed: Embed.Build());
            }
            catch (Exception exception)
            {
                await Program.Bot.Logger.Error(Log.TypeError.Commands, exception);
            }
        }

        [Command("transferirinfracoes"), Aliases("ti", "transferirinfrações"), ConnectedToMongo, Description("Membro que tem as infrações[ID/Menção] Membro que vai receber as infrações[ID/Menção]`\nTransfere as infrações de um membro para o outro.\n\n")]

        public async Task TransferirInfracoesAsync(CommandContext ctx, string membroIdInfracoes = null, string membroIdQueVaiReceberAsInfracoes = null)
        {
            try
            {
                await ctx.TriggerTypingAsync();

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                if (string.IsNullOrWhiteSpace(membroIdInfracoes) || string.IsNullOrWhiteSpace(membroIdQueVaiReceberAsInfracoes))
                {
                    embed.WithColor(Program.Bot.Utilities.HelpCommandsColor())
                        .WithAuthor("Como executar este comando:", null, Values.infoLogo)
                        .AddField("PC/Mobile", $"{ctx.Prefix}s ti Membro[ID/Menção] Membro[ID/Menção]")
                        .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    await ctx.RespondAsync(embed: embed.Build());
                    return;
                }

                IReadOnlyList<DiscordUser> membrosMencionados = ctx.Message.MentionedUsers;

                bool mencaoDeMembros = false;

                if (membrosMencionados.Count == 2)
                    mencaoDeMembros = true;

                ulong.TryParse(!mencaoDeMembros ? membroIdInfracoes : membrosMencionados[0].Id.ToString(), out ulong membroDasInfracoes);
                ulong.TryParse(!mencaoDeMembros ? membroIdQueVaiReceberAsInfracoes : membrosMencionados[1].Id.ToString(), out ulong membroQueVaiReceberAsInfracoes);

                if (membroDasInfracoes == 0 || membroQueVaiReceberAsInfracoes == 0)
                {
                    embed.WithColor(Program.Bot.Utilities.HelpCommandsColor())
                        .WithAuthor("Como executar este comando:", null, Values.infoLogo)
                        .AddField("PC/Mobile", $"{ctx.Prefix}s ti Membro[ID/Menção] Membro[ID/Menção]")
                        .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    await ctx.RespondAsync(embed: embed.Build());
                    return;
                }

                IMongoCollection<Infracao> infracoesDB = Program.Bot.LocalDB.GetCollection<Infracao>(Values.Mongo.infracoes);

                FilterDefinition<Infracao> filtroInfracoes = Builders<Infracao>.Filter.Eq(x => x.idInfrator, membroDasInfracoes);

                List<Infracao> respostaInfracoes = await (await infracoesDB.FindAsync(filtroInfracoes)).ToListAsync();

                if (respostaInfracoes.Count == 0)
                {
                    embed.WithAuthor($"O membro que contém o ID: \"{membroDasInfracoes}\" não tem infrações!", null, Values.logoUBGE)
                        .WithColor(Program.Bot.Utilities.RandomColorEmbed())
                        .WithThumbnailUrl(ctx.Member.AvatarUrl)
                        .WithDescription(":thinking:")
                        .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    await ctx.RespondAsync(embed: embed.Build());
                    return;
                }
                else
                {
                    await infracoesDB.UpdateManyAsync(filtroInfracoes, Builders<Infracao>.Update.Set(x => x.idInfrator, membroQueVaiReceberAsInfracoes));

                    embed.WithAuthor($"As infrações foram transferidas com sucesso!", null, Values.logoUBGE)
                        .WithColor(Program.Bot.Utilities.RandomColorEmbed())
                        .WithThumbnailUrl(ctx.Member.AvatarUrl)
                        .WithDescription(Program.Bot.Utilities.FindEmoji(ctx, ":UBGE:"))
                        .WithFooter($"Comando requisitado pelo: {Program.Bot.Utilities.DiscordNick(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    await ctx.RespondAsync(embed: embed.Build());
                }
            }
            catch (Exception exception)
            {
                await Program.Bot.Logger.Error(Log.TypeError.Commands, exception);
            }
        }
    }

    /*
    [Group("doador"), Aliases("d"), UBGE_Staff, BotConectadoAoMongo]

    public sealed class DoadorStaffControlled : BaseCommandModule
    {
        [Command("novo"), Aliases("new", "n"), Description("Tempo[Xs, Xm, Xh, Xd] Membro[ID/Menção]`\nArmazena este membro como doador.")]

        public async Task NovoDoadorAsync(CommandContext ctx, string tempoComoDoador = null, DiscordMember membro = null)
        {
            await ctx.TriggerTypingAsync();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            if (string.IsNullOrWhiteSpace(tempoComoDoador) || membro == null)
            {
                embed.WithColor(Program.ubgeBot.utilidadesGerais.CorHelpComandos())
                    .WithAuthor("Como executar este comando:", null, Values.infoLogo)
                    .AddField("PC/Mobile", $"{ctx.Prefix}d n Tempo[Xs, Xm, Xh, Xd] Membro[ID/Menção]")
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: embed.Build());

                return;
            }

            IMongoDatabase local = Program.ubgeBot.localDB;

            IMongoCollection<Doador> collectionDoador = local.GetCollection<Doador>(Values.Mongo.doador);

            FilterDefinition<Doador> filtroDoador = Builders<Doador>.Filter.Eq(x => x.idDoMembro, membro.Id);

            List<Doador> respostaListaDoadores = await (await collectionDoador.FindAsync(filtroDoador)).ToListAsync();

            int numeroDeVezesQueOMembroDoou = respostaListaDoadores.Count;

            DiscordRole cargoDoador = ctx.Guild.GetRole(Values.Cargos.cargoDoador);

            if (numeroDeVezesQueOMembroDoou == 0)
            {
                await membro.GrantRoleAsync(cargoDoador);

                string diaHoraVirouDoador = DateTime.Now.ToString();

                await collectionDoador.InsertOneAsync(new Doador
                {
                    idDoMembro = membro.Id,
                    jaDoou = "Sim",
                    diasEHorasQueOMembroVirouDoador = new List<string> { $"{diaHoraVirouDoador} - {tempoComoDoador}" },
                });

                embed.WithAuthor("O novo doador foi adicionado com sucesso!", null, Values.logoUBGE)
                    .WithDescription($"{Program.ubgeBot.utilidadesGerais.MencaoMembro(membro)}, foi adicionado com sucesso aos doadores!\n\n" +
                    $"Tempo: **{tempoComoDoador}**")
                    .WithThumbnailUrl(membro.AvatarUrl)
                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: embed.Build());
            }
            else
            {
                if (!membro.Roles.Contains(cargoDoador))
                    await membro.GrantRoleAsync(cargoDoador);

                string diaHoraVirouDoador = DateTime.Now.ToString();

                await collectionDoador.UpdateOneAsync(filtroDoador, Builders<Doador>.Update.Set(x => x.diasEHorasQueOMembroVirouDoador, respostaListaDoadores.LastOrDefault().diasEHorasQueOMembroVirouDoador.Append($"{diaHoraVirouDoador} - {tempoComoDoador}")).Set(y => y.jaDoou, $"Sim, {numeroDeVezesQueOMembroDoou + 1} vezes"));

                embed.WithAuthor("A nova doação foi adicionada com sucesso!", null, Values.logoUBGE)
                    .WithDescription($"{Program.ubgeBot.utilidadesGerais.MencaoMembro(membro)}, foi re-adicionado com sucesso aos doadores!\n\n" +
                    $"Tempo: **{tempoComoDoador}**")
                    .WithThumbnailUrl(membro.AvatarUrl)
                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: embed.Build());
            }
        }

        [Command("remover"), Aliases("retirar", "r"), Description("Membro[ID/Menção]`\nRetira o membro que está como doador.")]

        public async Task RemoverDoadorAsync(CommandContext ctx, DiscordMember membro = null)
        {
            await ctx.TriggerTypingAsync();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            if (membro == null)
            {
                embed.WithColor(Program.ubgeBot.utilidadesGerais.CorHelpComandos())
                    .WithAuthor("Como executar este comando:", null, Values.infoLogo)
                    .AddField("PC/Mobile", $"{ctx.Prefix}d r Membro[ID/Menção]")
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: embed.Build());

                return;
            }

            IMongoDatabase local = Program.ubgeBot.localDB;

            IMongoCollection<Doador> collectionDoador = local.GetCollection<Doador>(Values.Mongo.doador);

            FilterDefinition<Doador> filtroDoador = Builders<Doador>.Filter.Eq(x => x.idDoMembro, membro.Id);

            List<Doador> respostaListaDoadores = await (await collectionDoador.FindAsync(filtroDoador)).ToListAsync();

            if (respostaListaDoadores.Count == 0)
            {
                embed.WithAuthor("Este membro não fez nenhuma doação!", null, Values.logoUBGE)
                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithDescription(":thinking:")
                    .WithThumbnailUrl(membro.AvatarUrl)
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: embed.Build());

                return;
            }
            else
            {
                await collectionDoador.DeleteOneAsync(filtroDoador);

                embed.WithAuthor($"O membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\" foi removido dos doadores com sucesso!", null, Values.logoUBGE)
                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithDescription(await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, ":UBGE:"))
                    .WithThumbnailUrl(membro.AvatarUrl)
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: embed.Build());
            }
        }

        [Command("listar"), Aliases("ver", "v"), Description("Membro[ID/Menção]`\nVê todos os doadores, tanto os que estão armazenados e os que estão somente com o cargo.")]

        public async Task VerDoadoresAsync(CommandContext ctx, DiscordMember membro = null)
        {
            await ctx.TriggerTypingAsync();

            IMongoDatabase local = Program.ubgeBot.localDB;

            IMongoCollection<Doador> collectionDoador = local.GetCollection<Doador>(Values.Mongo.doador);

            DiscordRole cargoDoador = ctx.Guild.GetRole(Values.Cargos.cargoDoador);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            if (membro == null)
            {
                IEnumerable<DiscordMember> membrosUBGE = (await ctx.Guild.GetAllMembersAsync()).Where(x => x.Roles.Contains(cargoDoador));

                if (membrosUBGE.Count() == 0)
                {
                    embed.WithAuthor("Não há membros com o cargo de Doador!", null, Values.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithThumbnailUrl(ctx.Member.AvatarUrl)
                        .WithDescription(":thinking:")
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    await ctx.RespondAsync(embed: embed.Build());
                    return;
                }

                StringBuilder str = new StringBuilder();

                DiscordMessage msgAguarde = await ctx.RespondAsync($"Aguarde... {await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "leofsjal")}");

                foreach (DiscordMember membro_ in membrosUBGE)
                {
                    FilterDefinition<Doador> filtroDoador = Builders<Doador>.Filter.Eq(x => x.idDoMembro, membro_.Id);

                    List<Doador> respostaListaDoadores = await (await collectionDoador.FindAsync(filtroDoador)).ToListAsync();

                    str.Append($"{Program.ubgeBot.utilidadesGerais.MencaoMembro(membro_)} - Armazenado no bot?: **{(respostaListaDoadores.Count != 0 ? "Sim" : "Não")}**\n");
                }

                await msgAguarde.DeleteAsync();

                embed.WithAuthor("Lista dos atuais doadores da UBGE:", null, Values.logoUBGE)
                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithThumbnailUrl(ctx.Member.AvatarUrl)
                    .WithDescription(string.IsNullOrWhiteSpace(str.ToString()) ? "Não há membros doadores na UBGE." : str.ToString())
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: embed.Build());
            }
            else
            {
                FilterDefinition<Doador> filtroDoador = Builders<Doador>.Filter.Eq(x => x.idDoMembro, membro.Id);

                List<Doador> respostaListaDoadores = await (await collectionDoador.FindAsync(filtroDoador)).ToListAsync();

                if (respostaListaDoadores.Count == 0)
                {
                    embed.WithAuthor("Este membro não fez nenhuma doação!", null, Values.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithThumbnailUrl(ctx.Member.AvatarUrl)
                        .WithDescription(":thinking:")
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    await ctx.RespondAsync(embed: embed.Build());
                    return;
                }

                Doador ultimaRespostaDoadores = respostaListaDoadores.LastOrDefault();

                string[] diaHoraDoacao = ultimaRespostaDoadores.diasEHorasQueOMembroVirouDoador.LastOrDefault().Split('-');

                embed.WithAuthor($"Informações do doador: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\"", null, Values.logoUBGE)
                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithThumbnailUrl(membro.AvatarUrl)
                    .WithDescription($"Já doou?: **{ultimaRespostaDoadores.jaDoou}{(ultimaRespostaDoadores.jaDoou.Contains("Sim") ? " e foi armazenado no bot" : string.Empty)}.**\n\n" +
                    $"Dia e hora que o membro doou (Última doação): **{diaHoraDoacao[0]}**\n\n" +
                    $"Tempo do membro como doador: **{diaHoraDoacao[1].Replace(" ", "")}**")
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: embed.Build());
            }
        }
    }
    */
}