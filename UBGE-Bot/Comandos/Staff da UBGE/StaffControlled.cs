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
using System.IO;
using System.Net;
using UBGE_Bot.Main;
using UBGE_Bot.Utilidades;
using UBGE_Bot.LogExceptions;
using UBGE_Bot.MongoDB.Modelos;

namespace UBGE_Bot.Comandos.Staff_da_UBGE
{
    [Group("staff"), Aliases("s", "ubge"), UBGE_Staff]

    public sealed class StaffControlled : BaseCommandModule
    {
        private CancellationTokenSource cancellationTokenSource = null;

        [Command("check"), Aliases("c"), Description("Membro[ID/Menção]`\nCheca se um membro pode ter o cargo de Membro Registrado e mostra informações extras.\n\n")]

        public async Task CheckAsync(CommandContext ctx, DiscordMember membro = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    if (membro == null)
                    {
                        embed.WithColor(new DiscordColor(0x32363c))
                            .WithAuthor("Como executar este comando:", null, Valores.infoLogo)
                            .AddField("PC/Mobile", $"{ctx.Prefix}s c Membro[ID/Menção]")
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    DiscordEmoji addMembroRegistrado = DiscordEmoji.FromName(ctx.Client, ":large_blue_circle:"),
                    cancelaEmbed = DiscordEmoji.FromName(ctx.Client, ":red_circle:"),
                    removerMembroRegistrado = DiscordEmoji.FromName(ctx.Client, ":x:");

                    InteractivityExtension interact = ctx.Client.GetInteractivity();

                    DiscordRole membroRegistrado = ctx.Guild.GetRole(ctx.Guild.Channels.Values.ToList().Find(x=> x.Name.ToUpper().Contains(Valores.Cargos.cargoMembroRegistrado)).Id);

                    string estado = string.Empty,comoChegouAUBGE = string.Empty, idade = string.Empty, idiomas = string.Empty,
                        jogosMaisJogados = string.Empty, builderFezCenso = string.Empty, diasMembroEntrou = string.Empty, 
                        diasContaCriada = string.Empty, statusMembro = string.Empty, builder = string.Empty;

                    int nForeach = 0;

                    var local = Program.ubgeBot.localDB;

                    var collectionInfracao = local.GetCollection<Infracao>(Valores.Mongo.infracoes);
                    //var collectionCenso = local.GetCollection<Censo>(Valores.Mongo.censo);
                    var collectionMembrosQuePegaramCargos = local.GetCollection<MembrosQuePegaramOCargoDeMembroRegistrado>(Valores.Mongo.membrosQuePegaramOCargoDeMembroRegistrado);

                    var filtroInfracao = Builders<Infracao>.Filter.Eq(xm => xm.idInfrator, membro.Id);
                    var buscarInfracao = await collectionInfracao.FindAsync(filtroInfracao);
                    var listaInfracao = await buscarInfracao.ToListAsync();

                    //var filtroCenso = Builders<Censo>.Filter.Eq(xm => xm.idNoDiscord, membro.Id);
                    //var buscarCenso = await collectionCenso.FindAsync(filtroCenso);
                    //var listaCenso = await buscarCenso.ToListAsync();

                    var filtroMembrosQuePegaramOCargoDeMembroRegistrado = Builders<MembrosQuePegaramOCargoDeMembroRegistrado>.Filter.Empty;
                    var listaMembrosQuePegaramOCargoDeMembroRegistrado = await (await collectionMembrosQuePegaramCargos.FindAsync<MembrosQuePegaramOCargoDeMembroRegistrado>(filtroMembrosQuePegaramOCargoDeMembroRegistrado)).ToListAsync();

                    int index = 0;
                    
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

                    //if (listaCenso.Count == 0)
                    //{
                    //    builderFezCenso = "Não";
                        
                    //    estado = "Não especificado.";
                    //    comoChegouAUBGE = "Não especificado.";
                    //    idade = "Não especificado.";
                    //    idiomas = "Não especificado.";
                    //    jogosMaisJogados = "Não especificado.";
                    //}
                    //else
                    //{
                    //    builderFezCenso = "Sim";

                    //    estado = listaCenso.LastOrDefault().estado;
                    //    comoChegouAUBGE = listaCenso.LastOrDefault().chegouNaUBGE;
                    //    idade = listaCenso.LastOrDefault().idade.ToString();
                    //    idiomas = listaCenso.LastOrDefault().idiomas;
                    //    jogosMaisJogados = listaCenso.LastOrDefault().jogosMaisJogados;
                    //}

                    if ((int)(DateTime.Now - membro.JoinedAt.DateTime).TotalDays == 0)
                        diasMembroEntrou = "Hoje";
                    else
                        diasMembroEntrou = $"{(int)(DateTime.Now - membro.JoinedAt.DateTime).TotalDays} dias";

                    if ((int)(DateTime.Now - membro.CreationTimestamp.DateTime).TotalDays == 0)
                        diasContaCriada = "Hoje";
                    else
                        diasContaCriada = $"{(int)(DateTime.Now - membro.CreationTimestamp.DateTime).TotalDays} dias";

                    foreach (var Cargo in membro.Roles.OrderByDescending(x => x.Position))
                        strCargos.Append($"{Cargo.Mention} | ");
                    
                    strCargos.Append($"\n\n{(membro.Roles.Count() > 1 ? $"**{membro.Roles.Count()}** cargos ao total." : $"**{membro.Roles.Count()}** cargo ao total.")}");

                    if (strCargos.Length > 1024)
                    {
                        strCargos.Clear();
                        strCargos.Append("Os cargos excederam o limite de 1024 caracteres.");
                    }

                    statusMembro = Program.ubgeBot.utilidadesGerais.ConverteStatusPraNome(membro);

                    foreach (var servidoresBot in ctx.Client.Guilds.Values)
                    {
                        if (servidoresBot.Members.Keys.Contains(membro.Id) && servidoresBot.Id != Valores.Guilds.UBGE)
                        {
                            strServidores.Append($"{servidoresBot.Name}, ");

                            ++nForeach;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(strServidores.ToString()))
                        strServidores.Append("Este membro não está em nenhum outro servidor que eu estou.");

                    //estado = Program.ubgeBot.utilidadesGerais.RetornaEstado(estado);

                    DiscordMessage msgEmbed = null;

                    if (membro.Roles.Contains(membroRegistrado))
                    {
                        //string footer = $"Para remover o cargo de Membro Registrado > {removerMembroRegistrado} | " +
                        //    $"Para cancelar > {cancelaEmbed} - Vermelha";

                        embed.WithAuthor($"Informações do membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\"", null, Valores.logoUBGE)
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithColor(ctx.Member.Color)
                            .AddField($"Entrou na {ctx.Guild.Name} em:", $"{membro.JoinedAt.ToString("dd/MM/yyyy HH:mm:ss tt")} - **{diasMembroEntrou}**", false)
                            .AddField("Conta criada em:", $"{membro.CreationTimestamp.ToString("dd/MM/yyyy HH:mm:ss tt")} - **{diasContaCriada}**", false)
                            //.AddField("Fez o censo comunitário?:", $"**{builderFezCenso}**", false)
                            .AddField("Membro registrado?:", membro.Roles.Contains(membroRegistrado) ? "**Sim**" : "**Não**", false)
                            .AddField("Infrações:", str.ToString(), false)
                            .AddField("Status:", $"{(membro.IsBot ? await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "Bot") : "")}{(membro.IsOwner ? ":crown:" : "")}{await Program.ubgeBot.utilidadesGerais.ConverteStatusParaEmoji(ctx, membro)}- {statusMembro}", false)
                            .AddField("Está em mais algum outro servidor?:", $"{(strServidores.ToString().EndsWith(", ") ? strServidores.ToString().Remove(strServidores.ToString().Length - 2) : strServidores.ToString())}", false)
                            //.AddField("Respostas do censo:", $"Estado: **{estado}**\n\n" +
                            //$"Idade: **{idade}**\n\n" +
                            //$"Como chegou a UBGE: **{comoChegouAUBGE}**\n\n" +
                            //$"Idiomas: **{idiomas}**\n\n" +
                            //$"Jogos mais jogados: **{jogosMaisJogados}**", false)
                            .AddField("Cargos:", $"{(strCargos.ToString() == "Os cargos excederam o limite de 1024 caracteres." ? $"{strCargos.ToString()}, mas o membro tem **{membro.Roles.Count()} cargos.**" : strCargos.ToString())}", false)
                            //.WithFooter(membro.IsBot ? footer = string.Empty : footer)
                            .WithTimestamp(DateTime.Now);

                        msgEmbed = await ctx.RespondAsync(embed: embed.Build());

                        if (membro.IsBot)
                            return;

                        //await msgEmbed.CreateReactionAsync(removerMembroRegistrado);
                        //await Task.Delay(200);
                        //await msgEmbed.CreateReactionAsync(cancelaEmbed);
                    }
                    else
                    {
                        string footer = $"Para adicionar o cargo de Membro Registrado > {addMembroRegistrado} | " +
                            $"Para cancelar > {cancelaEmbed} | Para remover o cargo de Membro Registrado > {removerMembroRegistrado}";

                        embed.WithAuthor($"Informações do membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\"", null, Valores.logoUBGE)
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithColor(ctx.Member.Color)
                            .AddField($"Entrou na {ctx.Guild.Name} em:", $"{membro.JoinedAt.ToString("dd/MM/yyyy HH:mm:ss tt")} - **{diasMembroEntrou}**", false)
                            .AddField("Conta criada em:", $"{membro.CreationTimestamp.ToString("dd/MM/yyyy HH:mm:ss tt")} - **{diasContaCriada}**", false)
                            //.AddField("Fez o censo comunitário?:", $"**{builderFezCenso}**", false)
                            .AddField("Membro registrado?:", membro.Roles.Contains(membroRegistrado) ? "**Sim**" : "**Não**", false)
                            .AddField("Infrações:", str.ToString(), false)
                            .AddField("Status:", $"{(membro.IsBot ? await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "Bot") : "")}{(membro.IsOwner ? ":crown:" : "")}{await Program.ubgeBot.utilidadesGerais.ConverteStatusParaEmoji(ctx, membro)}- {statusMembro}", false)
                            .AddField("Está em mais algum outro servidor?:", $"{(strServidores.ToString().EndsWith(", ") ? strServidores.ToString().Remove(strServidores.ToString().Length - 2) : strServidores.ToString())}", false)
                            //.AddField("Respostas do Censo:", $"Estado: **{estado}**\n\n" +
                            //$"Idade: **{idade}**\n\n" +
                            //$"Como chegou a UBGE: **{comoChegouAUBGE}**\n\n" +
                            //$"Idiomas: **{idiomas}**\n\n" +
                            //$"Jogos mais jogados: **{jogosMaisJogados}**", false)
                            .AddField("Cargos:", $"{(strCargos.ToString() == "Os cargos excederam o limite de 1024 caracteres." ? $"{str.ToString()}, mas o membro tem **{membro.Roles.Count()} cargos.**" : strCargos.ToString())}", false)
                            //.WithFooter(membro.IsBot ? footer = string.Empty : footer)
                            .WithTimestamp(DateTime.Now);

                        msgEmbed = await ctx.RespondAsync(embed: embed.Build());

                        if (membro.IsBot)
                            return;

                        //if ((int)(DateTime.Now - membro.JoinedAt.DateTime).TotalDays >= 7 && builderFezCenso == "Sim")
                        //    await msgEmbed.CreateReactionAsync(addMembroRegistrado);

                        //await Task.Delay(200);
                        //await msgEmbed.CreateReactionAsync(cancelaEmbed);
                    }

                    var emojo = (await interact.WaitForReactionAsync(msgEmbed, ctx.User, TimeSpan.FromMinutes(5))).Result.Emoji;

                    if (emojo == addMembroRegistrado)
                    {
                        Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                        await msgEmbed.DeleteAllReactionsAsync();

                        embed.WithAuthor($"Cargo adicionado com sucesso!", null, Valores.logoUBGE)
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithColor(membroRegistrado.Color)
                            .WithDescription($"O cargo de: {membroRegistrado.Mention} foi adicionado com sucesso no membro: {Program.ubgeBot.utilidadesGerais.MencaoMembro(membro)}.")
                            .WithFooter($"✅ Cargo Adicionado ao {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator} com sucesso!");

                        await membro.GrantRoleAsync(membroRegistrado);

                        if (listaMembrosQuePegaramOCargoDeMembroRegistrado.Count == 0)
                            await collectionMembrosQuePegaramCargos.InsertOneAsync(new MembrosQuePegaramOCargoDeMembroRegistrado { numeroPessoasQuePegaramOCargoDeMembroRegistrado = 1, idsMembrosQuePegaramOCargo = new List<ulong> { membro.Id } });
                        else
                        {
                            if (listaMembrosQuePegaramOCargoDeMembroRegistrado.FirstOrDefault().idsMembrosQuePegaramOCargo.Contains(membro.Id))
                            {
                                await collectionMembrosQuePegaramCargos.UpdateOneAsync(filtroMembrosQuePegaramOCargoDeMembroRegistrado, 
                                    Builders<MembrosQuePegaramOCargoDeMembroRegistrado>.Update.Set(x => x.numeroPessoasQuePegaramOCargoDeMembroRegistrado, listaMembrosQuePegaramOCargoDeMembroRegistrado.FirstOrDefault().numeroPessoasQuePegaramOCargoDeMembroRegistrado + 1));
                            }
                            else
                            {
                                await collectionMembrosQuePegaramCargos.UpdateOneAsync(filtroMembrosQuePegaramOCargoDeMembroRegistrado, Builders<MembrosQuePegaramOCargoDeMembroRegistrado>.Update.Set(x => x.numeroPessoasQuePegaramOCargoDeMembroRegistrado, listaMembrosQuePegaramOCargoDeMembroRegistrado.FirstOrDefault().numeroPessoasQuePegaramOCargoDeMembroRegistrado + 1)
                                    .Set(y => y.idsMembrosQuePegaramOCargo, listaMembrosQuePegaramOCargoDeMembroRegistrado.FirstOrDefault().idsMembrosQuePegaramOCargo.Append(membro.Id)));
                            }
                        }

                        await msgEmbed.ModifyAsync(embed: embed.Build());
                    }
                    else if (emojo == cancelaEmbed)
                        await msgEmbed.DeleteAllReactionsAsync();
                    else if (emojo == removerMembroRegistrado)
                    {
                        Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                        await msgEmbed.DeleteAllReactionsAsync();

                        embed.WithAuthor($"Cargo removido com sucesso!", null, Valores.logoUBGE)
                           .WithThumbnailUrl(membro.AvatarUrl)
                           .WithColor(DiscordColor.Red)
                           .WithDescription($"O cargo de: {membroRegistrado.Mention} foi removido com sucesso de: {Program.ubgeBot.utilidadesGerais.MencaoMembro(membro)}.")
                           .WithFooter($"❌ Cargo Removido de {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator} com sucesso!");

                        await membro.RevokeRoleAsync(membroRegistrado);

                        if (listaMembrosQuePegaramOCargoDeMembroRegistrado.Count == 0)
                            await collectionMembrosQuePegaramCargos.InsertOneAsync(new MembrosQuePegaramOCargoDeMembroRegistrado { numeroPessoasQuePegaramOCargoDeMembroRegistrado = 0, idsMembrosQuePegaramOCargo = new List<ulong>() });
                        else
                        {
                            if (listaMembrosQuePegaramOCargoDeMembroRegistrado.FirstOrDefault().idsMembrosQuePegaramOCargo.Contains(membro.Id))
                            {
                                listaMembrosQuePegaramOCargoDeMembroRegistrado.FirstOrDefault().idsMembrosQuePegaramOCargo.Remove(membro.Id);
                                
                                await collectionMembrosQuePegaramCargos.UpdateOneAsync(filtroMembrosQuePegaramOCargoDeMembroRegistrado, Builders<MembrosQuePegaramOCargoDeMembroRegistrado>.Update.Set(x => x.numeroPessoasQuePegaramOCargoDeMembroRegistrado, listaMembrosQuePegaramOCargoDeMembroRegistrado.FirstOrDefault().numeroPessoasQuePegaramOCargoDeMembroRegistrado - 1)
                                    .Set(y => y.idsMembrosQuePegaramOCargo, listaMembrosQuePegaramOCargoDeMembroRegistrado.FirstOrDefault().idsMembrosQuePegaramOCargo));
                            }
                            else
                            {
                                await collectionMembrosQuePegaramCargos.UpdateOneAsync(filtroMembrosQuePegaramOCargoDeMembroRegistrado, 
                                    Builders<MembrosQuePegaramOCargoDeMembroRegistrado>.Update.Set(x => x.numeroPessoasQuePegaramOCargoDeMembroRegistrado, listaMembrosQuePegaramOCargoDeMembroRegistrado.FirstOrDefault().numeroPessoasQuePegaramOCargoDeMembroRegistrado - 1));
                            }
                        }

                        await msgEmbed.ModifyAsync(embed: embed.Build());
                    }
                }
                catch (DSharpPlus.Exceptions.NotFoundException)
                {
                    await ctx.RespondAsync($"{ctx.Member.Mention}, por motivos desconhecidos, o Discord não encontrou este membro.");
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("infracoes"), Aliases("i", "infrações"), Description("[Add/Log]`\nAdd: Prende um membro por tempo indeterminado, Log: Mostra as infrações do membro.\n\n")]

        public async Task InfracoesAsync(CommandContext ctx, DiscordMember membro = null, string addtive = null, [RemainingText] string infracao = null)
        {
            await ctx.TriggerTypingAsync();
            
            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    if (membro == null)
                    {
                        embed.WithColor(new DiscordColor(0x32363c))
                            .WithAuthor("Como executar este comando:", null, Valores.infoLogo)
                            .AddField("PC/Mobile", $"{ctx.Prefix}s i Membro[ID/Menção) add[Infração] ou log[Ver infrações]")
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }
                    
                    if (string.IsNullOrWhiteSpace(addtive))
                    {
                        embed.WithColor(new DiscordColor(0x32363c))
                            .WithAuthor("Como executar este comando:", null, Valores.infoLogo)
                            .AddField("PC/Mobile", $"{ctx.Prefix}s i Membro[ID/Menção] add[Infração] ou log[Ver infrações]")
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    var interact = ctx.Client.GetInteractivity();

                    DiscordRole prisioneiro = ctx.Guild.GetRole(ctx.Guild.Roles.Values.ToList().Find(x => x.Name.ToUpper().Contains(Valores.Cargos.cargoPrisioneiro)).Id);

                    var cargosMembro = membro.Roles;

                    var emojo = await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "leofsjal");

                    List<DiscordChannel> canaisUBGE = ctx.Guild.Channels.Values.ToList();

                    DiscordChannel centroReabilitacao = ctx.Guild.GetChannel(canaisUBGE.Find(x => x.Name.ToUpper().Contains(Valores.ChatsUBGE.canalCentroDeReabilitacao)).Id),
                    logChat = ctx.Guild.GetChannel(canaisUBGE.Find(x => x.Name.ToUpper().Contains(Valores.ChatsUBGE.canalLog)).Id);

                    DiscordEmoji marcarSim = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"),
                    marcarNao = DiscordEmoji.FromName(ctx.Client, ":negative_squared_cross_mark:"),
                    pastaDeArquivo = DiscordEmoji.FromName(ctx.Client, ":file_folder:");

                    List<ulong> cargosLista = new List<ulong>();

                    var local = Program.ubgeBot.localDB;

                    if (addtive.ToLowerInvariant() == "add")
                    {
                        var infracoesCollection = local.GetCollection<Infracao>(Valores.Mongo.infracoes);

                        Infracao ni = new Infracao
                        {
                            idInfrator = membro.Id,
                            idStaff = ctx.Member.Id,
                            motivoInfracao = infracao,
                        };

                        embed.WithAuthor($"Preparando...", null, Valores.logoUBGE)
                            .WithDescription($"Para enviar: {membro.Mention} para o {centroReabilitacao.Mention}, aperte: ✅\n" +
                            $"Para CANCELAR, aperte: ❎\n" +
                            $"Para somente adicionar a infração, aperte: :file_folder:")
                            .WithFooter("✅ Sim / Não ❎ / Armazenar Infração 📁")
                            .WithColor(ctx.Member.Color)
                            .WithTimestamp(DateTime.Now)
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                        DiscordMessage msgEmbed = await ctx.RespondAsync(embed: embed.Build());
                        await msgEmbed.CreateReactionAsync(marcarSim);
                        await Task.Delay(200);
                        await msgEmbed.CreateReactionAsync(marcarNao);
                        await Task.Delay(200);
                        await msgEmbed.CreateReactionAsync(pastaDeArquivo);

                        var emoji = (await interact.WaitForReactionAsync(msgEmbed, ctx.User)).Result.Emoji;

                        if (emoji == marcarSim)
                        {
                            ni.dataInfracao = DateTime.Now.ToString();
                            ni.oMembroFoiPreso = true;

                            await msgEmbed.DeleteAllReactionsAsync();

                            Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                            embed.WithAuthor($"O membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}\" foi preso com sucesso!", null, Valores.logoUBGE)
                                .WithColor(prisioneiro.Color)
                                .WithTimestamp(DateTime.Now)
                                .WithThumbnailUrl(membro.AvatarUrl)
                                .WithDescription(emojo)
                                .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                            foreach (var cargo in cargosMembro)
                            {
                                cargosLista.Add(cargo.Id);

                                await Task.Delay(200);

                                await membro.RevokeRoleAsync(cargo);
                            }

                            LogPrisao log = new LogPrisao
                            {
                                cargosDoMembro = cargosLista,
                            };

                            ni.dadosPrisao = log;

                            await infracoesCollection.InsertOneAsync(ni);
                            
                            await msgEmbed.ModifyAsync(embed: embed.Build());

                            await membro.GrantRoleAsync(prisioneiro);

                            Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                            embed.WithColor(prisioneiro.Color)
                                .WithAuthor($"Você foi preso pelo membro: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}!")
                                .WithDescription($"Sua infração: {infracao}")
                                .WithTimestamp(DateTime.Now)
                                .WithThumbnailUrl(membro.AvatarUrl)
                                .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                            await centroReabilitacao.SendMessageAsync(embed: embed.Build(), content: membro.Mention);

                            Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                            embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                                .WithAuthor($"O membro: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)} foi levado para o {centroReabilitacao.Mention}.", null, Valores.logoUBGE)
                                .WithDescription($"Infração: {infracao}")
                                .WithThumbnailUrl(membro.AvatarUrl)
                                .WithTimestamp(DateTime.Now)
                                .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                            await logChat.SendMessageAsync(embed: embed.Build());
                        }
                        else if (emoji == marcarNao)
                        {
                            await msgEmbed.DeleteAllReactionsAsync();

                            Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                            embed.WithAuthor($"Comando concluído!", null, Valores.logoUBGE)
                                .WithColor(prisioneiro.Color)
                                .WithTimestamp(DateTime.Now)
                                .WithThumbnailUrl(membro.AvatarUrl)
                                .WithDescription($"O membro: {membro.Mention} não foi enviado pra gulag.")
                                .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                            await msgEmbed.ModifyAsync(embed: embed.Build());
                        }
                        else if (emoji == pastaDeArquivo)
                        {
                            ni.dataInfracao = DateTime.Now.ToString();
                            ni.oMembroFoiPreso = false;

                            await msgEmbed.DeleteAllReactionsAsync();

                            Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                            embed.WithAuthor($"A infração foi armazenada com sucesso!", null, Valores.logoUBGE)
                                .WithColor(prisioneiro.Color)
                                .WithTimestamp(DateTime.Now)
                                .WithThumbnailUrl(membro.AvatarUrl)
                                .WithDescription($"Membro: {membro.Mention}\n\nInfração: {infracao}\n\n{emojo}")
                                .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                            foreach (var cargo in cargosMembro)
                                cargosLista.Add(cargo.Id);

                            LogPrisao log = new LogPrisao
                            {
                                cargosDoMembro = cargosLista,
                            };

                            ni.dadosPrisao = log;

                            await infracoesCollection.InsertOneAsync(ni);

                            await msgEmbed.ModifyAsync(embed: embed.Build());
                        }
                    }
                    else if (addtive.ToLowerInvariant() == "log")
                    {
                        var infracaoDB = local.GetCollection<Infracao>(Valores.Mongo.infracoes);
                        var lista = await (await infracaoDB.FindAsync(Builders<Infracao>.Filter.Eq(x => x.idInfrator, membro.Id))).ToListAsync();

                        if (lista.Count == 0)
                        {
                            embed.WithThumbnailUrl(membro.AvatarUrl)
                                .WithColor(DiscordColor.Green)
                                .WithAuthor($"O usuário: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}\" possui uma ficha limpa!")
                                .WithTimestamp(DateTime.Now)
                                .WithDescription(emojo)
                                .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                            await ctx.RespondAsync(embed: embed.Build());
                        }
                        else
                        {
                            if (lista.Count == 0)
                                embed.WithAuthor($"O usuário: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}\" não possui infração.", null, Valores.logoUBGE);
                            else if (lista.Count == 1)
                                embed.WithAuthor($"O usuário: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}\" possui {lista.Count} infração.", null, Valores.logoUBGE);
                            else if (lista.Count >= 2)
                                embed.WithAuthor($"O usuário: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}\" possui {lista.Count} infrações.", null, Valores.logoUBGE);

                            embed.WithThumbnailUrl(membro.AvatarUrl)
                                .WithColor(prisioneiro.Color);

                            StringBuilder strCargos = new StringBuilder();

                            foreach (var infra in lista)
                            {
                                var nForeach = lista.IndexOf(infra);

                                foreach (var cargosMembroForeach in infra.dadosPrisao.cargosDoMembro)
                                    strCargos.Append($"<@&{cargosMembroForeach}> | ");

                                if (strCargos.Length >= 700)
                                {
                                    strCargos.Clear();
                                    strCargos.Append("Limite de caracteres atingido.");
                                }

                                var Conversao = infra.dataInfracao.ToString();

                                string Dias = string.Empty;

                                if (Conversao.Contains("T") || Conversao.Contains("Z"))
                                    Conversao = Convert.ToDateTime(Conversao).ToString();

                                if (infra.dadosPrisao.tempoDoMembroNaPrisao == "Sem dados")
                                    infra.dadosPrisao.tempoDoMembroNaPrisao = string.Empty;

                                if (string.IsNullOrWhiteSpace(infra.dadosPrisao.tempoDoMembroNaPrisao))
                                    infra.dadosPrisao.tempoDoMembroNaPrisao = "Não especificado.";

                                if (infra.dadosPrisao.tempoDoMembroNaPrisao.Contains("h"))
                                    Dias = infra.dadosPrisao.tempoDoMembroNaPrisao;

                                embed.AddField($"{++nForeach} - {infra.motivoInfracao}", $"Punido por: {Program.ubgeBot.utilidadesGerais.MencaoMembro(await ctx.Guild.GetMemberAsync(infra.idStaff))}\n" +
                                    $"Data: {Conversao}\n" +
                                    $"Foi preso: {(infra.oMembroFoiPreso ? "Sim" : "Não")}\n" +
                                    $"Tempo: {(!string.IsNullOrWhiteSpace(infra.dadosPrisao.tempoDoMembroNaPrisao) ? infra.dadosPrisao.tempoDoMembroNaPrisao : "Não especificado.")}{(!string.IsNullOrWhiteSpace(Dias) ? $" ou {Math.Round(double.Parse(Dias.Replace("h", "")) / 24, 2)} dias" : "")}\n" +
                                    $"Cargos: {(!string.IsNullOrWhiteSpace(strCargos.ToString()) ? strCargos.ToString() : "Não especificado.")}");
                            }

                            await ctx.RespondAsync(embed: embed.Build());
                        }
                    }
                    else
                    {
                        embed.WithColor(DiscordColor.Red)
                            .WithAuthor($"Erro! Comando não reconhecido!", null, Valores.logoUBGE)
                            .WithThumbnailUrl(ctx.Member.AvatarUrl)
                            .WithDescription($"Comandos disponíveis:\n\n`{ctx.Prefix}s i {membro.Id} add`\n`{ctx.Prefix}s i {membro.Id} log`")
                            .WithTimestamp(DateTime.Now)
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                }
                catch (DSharpPlus.Exceptions.NotFoundException)
                {
                    await ctx.RespondAsync($"{ctx.Member.Mention}, por motivos desconhecidos, o Discord não encontrou este membro.");
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("clear"), Aliases("apagar", "clean", "limpar", "limparchat"), Description("Nº de Mensagens[De 1 a 100] Membro[Opcional]`\nApaga as mensagens do chat que foi executado o comando, pode ser de um membro específico ou a quantidade que foi colocada no comando.\n\n")]

        public async Task ApagarMessagensAsync(CommandContext ctx, int numeroMensagens, DiscordMember membro = null)
        {
            await Task.Delay(200);

            new Thread(async () =>
            {
                try
                {
                    var interactivity = ctx.Client.GetInteractivity();

                    if (membro == null)
                    {
                        for (int i = 1; i < numeroMensagens + 1; i++)
                        {
                            await Task.Delay(200);

                            await (await ctx.Channel.GetMessagesAsync(i)).FirstOrDefault().DeleteAsync();
                        }
                    }
                    else
                    {
                        for (int i = 1; i < numeroMensagens + 1; i++)
                        {
                            await Task.Delay(200);

                            await (await ctx.Channel.GetMessagesAsync(i)).Where(x => x.Author.Id == membro.Id).FirstOrDefault().DeleteAsync();
                        }
                    }
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("apagarinfração"), Aliases("ai", "deletarinfração", "di", "removerinfração", "ri"), Description("Membro[ID ou Menção] Infração[Motivo]`\nApaga uma determinada infração do membro.\n\n")]

        public async Task DeletarInfracoesAsync(CommandContext ctx, DiscordMember membro = null, [RemainingText] string infracao = null)
        {
            await ctx.TriggerTypingAsync();
            
            new Thread(async () =>
            {
                try
                {

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    if (membro == null)
                    {
                        embed.WithColor(new DiscordColor(0x32363c))
                                .WithAuthor("Como executar este comando:", null, Valores.infoLogo)
                                .AddField("PC/Mobile", $"{ctx.Prefix}s ai Membro[Menção/ID] Infração[Motivo]")
                                .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                                .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    var db = Program.ubgeBot.localDB;
                    var infracaoDB = db.GetCollection<Infracao>(Valores.Mongo.infracoes);

                    var filtro = Builders<Infracao>.Filter.Eq(x => x.motivoInfracao, infracao);
                    var lista = await (await infracaoDB.FindAsync(filtro)).ToListAsync();

                    var emojo = await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "leofsjal");

                    if (lista.Count == 0)
                    {
                        embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithAuthor("Infração não encontrada e/ou este membro não tem infrações.")
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithTimestamp(DateTime.Now)
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                    else
                    {
                        await infracaoDB.DeleteOneAsync(filtro);

                        embed.WithColor(DiscordColor.Green)
                            .WithAuthor("Infração apagada com sucesso!")
                            .WithTimestamp(DateTime.Now)
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithDescription($"Infração apagada: {infracao}\n\nBoa 06, fatiou passou... {emojo}");

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                }
                catch (DSharpPlus.Exceptions.NotFoundException)
                {
                    await ctx.RespondAsync($"{ctx.Member.Mention}, por motivos desconhecidos, o Discord não encontrou este membro.");
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        //[Command("list"), Aliases("lists"), Description("`\nLista e dá o cargo automaticamente de membro registrado para os membros que tem + de 7 dias na UBGE.\n\n")]

        public async Task ListAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            DiscordMessage msgAguarde = await ctx.RespondAsync($"Aguarde... {await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "leofsjal")}");

            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    var local = Program.ubgeBot.localDB;
                    var collectionCenso = local.GetCollection<Censo>(Valores.Mongo.censo);

                    await msgAguarde.ModifyAsync($"Aguarde... 30% {await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "leofsjal")}");

                    var listaCenso = await (await collectionCenso.FindAsync(Builders<Censo>.Filter.Empty)).ToListAsync();

                    List<DiscordRole> cargosUBGE = ctx.Guild.Roles.Values.ToList();

                    DiscordRole cargoMembroRegistrado = ctx.Guild.GetRole(cargosUBGE.Find(x => x.Name.ToUpper().Contains(Valores.Cargos.cargoMembroRegistrado)).Id),
                    prisioneiroCargo = ctx.Guild.GetRole(cargosUBGE.Find(x => x.Name.ToUpper().Contains(Valores.Cargos.cargoPrisioneiro)).Id);

                    var membrosTotaisUBGE = (await ctx.Guild.GetAllMembersAsync()).ToList();

                    StringBuilder strFinal = new StringBuilder();

                    DiscordMember pegaMembroServidor = null;

                    await msgAguarde.ModifyAsync($"Aguarde... 60% {await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "leofsjal")}");

                    foreach (var m in listaCenso)
                    {
                        try
                        {
                            pegaMembroServidor = await ctx.Guild.GetMemberAsync(m.idNoDiscord);

                            if ((DateTime.Now - pegaMembroServidor.JoinedAt).TotalDays >= 7 && !pegaMembroServidor.Roles.Contains(cargoMembroRegistrado) && !pegaMembroServidor.Roles.Contains(prisioneiroCargo))
                            {
                                await pegaMembroServidor.GrantRoleAsync(cargoMembroRegistrado);

                                var collectionMembrosQuePegaramOCargoDeMembroRegistrado = local.GetCollection<MembrosQuePegaramOCargoDeMembroRegistrado>(Valores.Mongo.membrosQuePegaramOCargoDeMembroRegistrado);
                                var filtroMembrosQuePegaramOCargoDeMembroRegistrado = Builders<MembrosQuePegaramOCargoDeMembroRegistrado>.Filter.Empty;
                                var listaMembrosQuePegaramOCargoDeMembroRegistrado = await (await collectionMembrosQuePegaramOCargoDeMembroRegistrado.FindAsync<MembrosQuePegaramOCargoDeMembroRegistrado>(filtroMembrosQuePegaramOCargoDeMembroRegistrado)).ToListAsync();

                                if (listaMembrosQuePegaramOCargoDeMembroRegistrado.Count == 0)
                                    await collectionMembrosQuePegaramOCargoDeMembroRegistrado.InsertOneAsync(new MembrosQuePegaramOCargoDeMembroRegistrado { numeroPessoasQuePegaramOCargoDeMembroRegistrado = 1, idsMembrosQuePegaramOCargo = new List<ulong> { pegaMembroServidor.Id } });
                                else
                                {
                                    var update = Builders<MembrosQuePegaramOCargoDeMembroRegistrado>.Update.Set(x => x.idsMembrosQuePegaramOCargo, listaMembrosQuePegaramOCargoDeMembroRegistrado.FirstOrDefault().idsMembrosQuePegaramOCargo.Append(pegaMembroServidor.Id))
                                    .Set(y => y.numeroPessoasQuePegaramOCargoDeMembroRegistrado, listaMembrosQuePegaramOCargoDeMembroRegistrado.FirstOrDefault().numeroPessoasQuePegaramOCargoDeMembroRegistrado + 1);

                                    await collectionMembrosQuePegaramOCargoDeMembroRegistrado.UpdateOneAsync(filtroMembrosQuePegaramOCargoDeMembroRegistrado, update);
                                }

                                strFinal.Append($"{Program.ubgeBot.utilidadesGerais.MencaoMembro(pegaMembroServidor)} - Nome no Discord: **{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(pegaMembroServidor)}#{pegaMembroServidor.Discriminator}** - ID: `{pegaMembroServidor.Id}` - {(int)(DateTime.Now - pegaMembroServidor.JoinedAt).TotalDays} dias |\n");
                            }

                            await Task.Delay(200);
                        }
                        catch (Exception) { }
                    }

                    await msgAguarde.ModifyAsync($"Aguarde... 80% {await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "leofsjal")}");

                    embed.WithAuthor($"Lista de membros que foi adicionado o cargo de Membro Registrado:", null, Valores.logoUBGE);

                    var MembrosTotaisCargoMembroRegistrado = membrosTotaisUBGE.FindAll(xm => xm.Roles.ToList().Contains(cargoMembroRegistrado)).Count;

                    decimal Porcentagem = Math.Round(((decimal)MembrosTotaisCargoMembroRegistrado * 100) / membrosTotaisUBGE.Count, 2);

                    await msgAguarde.DeleteAsync();

                    embed.AddField($"Verificação realizada às: {DateTime.Now.ToString()}", string.IsNullOrWhiteSpace(strFinal.ToString()) ? $"Bom Trabalho!\nNão há nenhum cargo que não foi adicionado! {await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "UBGE")}" : strFinal.ToString());
                    embed.WithFooter($"{Porcentagem}% da {ctx.Guild.Name} tem o cargo de Membro Registrado");
                    embed.WithTimestamp(DateTime.Now);
                    embed.WithColor(new DiscordColor(0x32363c));

                    await ctx.RespondAsync(embed: embed.Build());
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("mute"), Aliases("m"), Description("Tempo[Xs/Xm/Xh/Xd] Membro[ID/Menção] Infração[Motivo do Mute]`\nMuta um membro específico por um tempo.\n\n")]

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
                        embed.WithColor(new DiscordColor(0x32363c))
                            .WithAuthor("Como executar este comando:", null, Valores.infoLogo)
                            .AddField("PC/Mobile", $"{ctx.Prefix}s m Tempo[Xs/Xm/Xh/Xd] Membro[ID/Menção] Infração[Motivo]")
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build(), content: $"{ctx.Member.Mention}, digite o tempo da prisão deste membro!");
                        return;
                    }

                    if (!(tempo.Contains("s") || tempo.Contains("m") || tempo.Contains("h") || tempo.Contains("d")))
                    {
                        embed.WithColor(new DiscordColor(0x32363c))
                            .WithAuthor("Como executar este comando:", null, Valores.infoLogo)
                            .AddField("PC/Mobile", $"{ctx.Prefix}s m Tempo[Xs/Xm/Xh/Xd] Membro[ID/Menção] Infração[Motivo]")
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build(), content: $"{ctx.Member.Mention}, digite o sufixo do tempo! `[s, m, h, d]`");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(infracao))
                    {
                        embed.WithColor(new DiscordColor(0x32363c))
                            .WithAuthor("Como executar este comando:", null, Valores.infoLogo)
                            .AddField("PC/Mobile", $"{ctx.Prefix}s m Tempo[Xs/Xm/Xh/Xd] Membro[ID/Menção] Infração[Motivo]")
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build(), content: $"{ctx.Member.Mention}, digite a infração deste membro!");
                        return;
                    }

                    DiscordRole prisioneiroCargo = ctx.Guild.GetRole(ctx.Guild.Roles.Values.ToList().Find(x => x.Name.ToUpper().Contains(Valores.Cargos.cargoPrisioneiro)).Id);

                    var db = Program.ubgeBot.localDB;
                    var collectionInfracoes = db.GetCollection<Infracao>(Valores.Mongo.infracoes);

                    var filtro = Builders<Infracao>.Filter.Eq(x => x.idInfrator, membro.Id);
                    var listaInfracoes = await (await collectionInfracoes.FindAsync(filtro)).ToListAsync();

                    if (membro.Roles.Contains(prisioneiroCargo))
                    {
                        embed.WithColor(prisioneiroCargo.Color)
                            .WithAuthor($"O membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\" já está na prisão!", null, Valores.logoUBGE)
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithDescription($"Infração: {listaInfracoes.LastOrDefault().motivoInfracao}\n\n" +
                            $"Tempo preso: {listaInfracoes.LastOrDefault().dadosPrisao.tempoDoMembroNaPrisao}\n\n" +
                            $"Dia e hora da infração: {listaInfracoes.LastOrDefault().dataInfracao}\n\n" +
                            $"Membro da staff que aplicou a punição: {(await ctx.Guild.GetMemberAsync(listaInfracoes.LastOrDefault().idStaff)).Mention}")
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    var cargosMembro = membro.Roles.ToList();

                    var tempoDaPrisao = Program.ubgeBot.utilidadesGerais.ConverterTempo(tempo);
                    var horaAtual = DateTime.Now;
                    var horarioDeSaida = horaAtual.Add(tempoDaPrisao).ToString();

                    List<DiscordChannel> canaisUBGE = ctx.Guild.Channels.Values.ToList();

                    DiscordChannel centroReabilitacao = ctx.Guild.GetChannel(canaisUBGE.Find(x => x.Name.ToUpper().Contains(Valores.ChatsUBGE.canalCentroDeReabilitacao)).Id),
                    logChat = ctx.Guild.GetChannel(canaisUBGE.Find(x => x.Name.ToUpper().Contains(Valores.ChatsUBGE.canalLog)).Id);

                    List<ulong> cargos = new List<ulong>();

                    if (cargosMembro.Count() != 0)
                    {
                        foreach (var cargosForeach in cargosMembro)
                        {
                            cargos.Add(cargosForeach.Id);

                            await Task.Delay(200);

                            await membro.RevokeRoleAsync(cargosForeach);
                        }
                    }

                    await membro.GrantRoleAsync(prisioneiroCargo);

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
                        .WithAuthor($"O membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\" foi mutado.", null, Valores.logoUBGE)
                        .WithThumbnailUrl(membro.AvatarUrl)
                        .WithTimestamp(DateTime.Now)
                        .WithDescription($"Infração: **{infracao}**\n\n" +
                        $"Tempo: **{tempo}**\n\n" +
                        $"Horário da Entrada na Prisão: **{horaAtual}**\n\n" +
                        $"Horário de Saída da Prisão: **{horarioDeSaida}**\n\n" +
                        $"Menção do Membro: {membro.Mention}\n\n" +
                        $"Membro da Staff que Mutou: {ctx.Member.Mention}")
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    await ctx.RespondAsync(embed: embed.Build());

                    Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                    embed.WithColor(ctx.Member.Color)
                        .WithAuthor($"O membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\" foi mutado.", null, Valores.logoUBGE)
                        .WithThumbnailUrl(membro.AvatarUrl)
                        .WithTimestamp(DateTime.Now)
                        .WithDescription($"Infração: **{infracao}**\n\n" +
                        $"Tempo: **{tempo}**\n\n" +
                        $"Horário da Entrada na Prisão: **{horaAtual}**\n\n" +
                        $"Horário de Saída da Prisão: **{horarioDeSaida}**\n\n" +
                        $"Menção do Membro: {membro.Mention}\n\n" +
                        $"Membro da Staff que Mutou: {ctx.Member.Mention}")
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    await logChat.SendMessageAsync(embed: embed.Build());

                    Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                    embed.WithColor(prisioneiroCargo.Color)
                        .WithAuthor($"Você foi mutado por: {tempo}!", null, Valores.logoUBGE)
                        .WithDescription($"Sua infração: **{infracao}**\n\n" +
                        $"Horário de Saída: **{horarioDeSaida}**\n\n" +
                        $"Membro da staff que lhe mutou: {ctx.Member.Mention}")
                        .WithTimestamp(DateTime.Now)
                        .WithThumbnailUrl(membro.AvatarUrl);

                    await centroReabilitacao.SendMessageAsync(embed: embed.Build(), content: membro.Mention);

                    cancellationTokenSource = new CancellationTokenSource();

                    await Task.Delay(tempoDaPrisao, cancellationTokenSource.Token);

                    await membro.RevokeRoleAsync(prisioneiroCargo);

                    foreach (var cargosMembroForeach in cargosMembro)
                    {
                        await Task.Delay(200);

                        await membro.GrantRoleAsync(cargosMembroForeach);
                    }

                    Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                    embed.WithColor(ctx.Member.Color)
                        .WithAuthor($"O membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\" foi desmutado.", null, Valores.logoUBGE)
                        .WithThumbnailUrl(membro.AvatarUrl)
                        .WithTimestamp(DateTime.Now)
                        .WithDescription($"Horário de Saída: **{horarioDeSaida}**\n\n" +
                        $"Menção do Membro: {membro.Mention}\n\n" +
                        $"Membro da Staff que Mutou: {ctx.Member.Mention}")
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    await ctx.RespondAsync(embed: embed.Build());

                    Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                    embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithAuthor($"O membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\" foi desmutado.", null, Valores.logoUBGE)
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
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("devolvercargos"), Aliases("dc"), Description("Membro[ID/Menção]`\nDevolve os cargos de um membro que foi preso, caso o bot caia durante a prisão do mesmo. **SÓ EXECUTE ESTE COMANDO CASO O BOT TENHA CAIDO E FICADO OFFLINE.**\n\n")]

        public async Task DevolverCargosAsync(CommandContext ctx, DiscordMember membro = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    if (membro == null)
                    {
                        embed.WithColor(new DiscordColor(0x32363c))
                            .WithAuthor("Como executar este comando:", null, Valores.infoLogo)
                            .AddField("PC/Mobile", $"{ctx.Prefix}s dc Membro[ID/Menção]")
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    var local = Program.ubgeBot.localDB;
                    var collectionInfracoes = local.GetCollection<Infracao>(Valores.Mongo.infracoes);

                    var filtro = Builders<Infracao>.Filter.Eq(x => x.idInfrator, membro.Id);
                    var listaInfracoes = await (await collectionInfracoes.FindAsync(filtro)).ToListAsync();

                    DiscordRole prisioneiroCargo = ctx.Guild.GetRole(ctx.Guild.Roles.Values.ToList().Find(x => x.Name.ToUpper().Contains(Valores.Cargos.cargoPrisioneiro)).Id), cargo = null;

                    if (listaInfracoes.Count == 0)
                    {
                        embed.WithColor(DiscordColor.Red)
                            .WithAuthor($"O membro não contem infrações!", null, Valores.logoUBGE)
                            .WithDescription(":thinking:")
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                    else
                    {
                        if (membro.Roles.Contains(prisioneiroCargo))
                            await membro.RevokeRoleAsync(prisioneiroCargo);

                        ulong cargoUlong = 0;
                        StringBuilder strCargos = new StringBuilder();

                        foreach (var cargoForeach in listaInfracoes.LastOrDefault().dadosPrisao.cargosDoMembro)
                        {
                            cargoUlong = Convert.ToUInt64(cargoForeach);
                            cargo = ctx.Guild.GetRole(cargoUlong);

                            await Task.Delay(200);

                            await membro.GrantRoleAsync(cargo);

                            strCargos.Append($"{cargo.Mention} | ");
                        }

                        embed.WithColor(DiscordColor.Green)
                            .WithAuthor($"Cargos do membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}\" devolvidos com sucesso!", null, Valores.logoUBGE)
                            .WithDescription($"Cargos: {strCargos.ToString()}")
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                }
                catch (DSharpPlus.Exceptions.NotFoundException)
                {
                    await ctx.RespondAsync($"{ctx.Member.Mention}, por motivos desconhecidos, o Discord não encontrou este membro.");
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("análisecenso"), Aliases("dadoscenso", "analisecenso", "gráficosdoléo", "graficosdoleo"), Description("`\nMostra os dados do censo, como os jogos mais jogados.\n\n")]

        public async Task GraficoCensoAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    string caminho = Environment.GetFolderPath(Environment.SpecialFolder.Desktop), caminhoFinal = string.Empty;
                    WebClient webClient = new WebClient();

                    List<string> linksPlanilha = new List<string> 
                    { 
                        "https://docs.google.com/spreadsheets/d/e/2PACX-1vQvqbjnVbMKsGrgbm5vMFaQQZ8cLYC_aPlrch3xpdOy2qOpi8wyBEy1_GdRGY1T2Fp7s3HXMeIYELjq/pubchart?oid=2004269944&format=image",
                        "https://docs.google.com/spreadsheets/d/e/2PACX-1vQvqbjnVbMKsGrgbm5vMFaQQZ8cLYC_aPlrch3xpdOy2qOpi8wyBEy1_GdRGY1T2Fp7s3HXMeIYELjq/pubchart?oid=2120692605&format=image",
                        "https://docs.google.com/spreadsheets/d/e/2PACX-1vQvqbjnVbMKsGrgbm5vMFaQQZ8cLYC_aPlrch3xpdOy2qOpi8wyBEy1_GdRGY1T2Fp7s3HXMeIYELjq/pubchart?oid=702791241&format=image",
                        "https://docs.google.com/spreadsheets/d/e/2PACX-1vQvqbjnVbMKsGrgbm5vMFaQQZ8cLYC_aPlrch3xpdOy2qOpi8wyBEy1_GdRGY1T2Fp7s3HXMeIYELjq/pubchart?oid=565139007&format=image",
                        "https://docs.google.com/spreadsheets/d/e/2PACX-1vQvqbjnVbMKsGrgbm5vMFaQQZ8cLYC_aPlrch3xpdOy2qOpi8wyBEy1_GdRGY1T2Fp7s3HXMeIYELjq/pubchart?oid=963901726&format=image",
                        "https://docs.google.com/spreadsheets/d/e/2PACX-1vQvqbjnVbMKsGrgbm5vMFaQQZ8cLYC_aPlrch3xpdOy2qOpi8wyBEy1_GdRGY1T2Fp7s3HXMeIYELjq/pubchart?oid=862681103&format=image",
                        "https://docs.google.com/spreadsheets/d/e/2PACX-1vQvqbjnVbMKsGrgbm5vMFaQQZ8cLYC_aPlrch3xpdOy2qOpi8wyBEy1_GdRGY1T2Fp7s3HXMeIYELjq/pubchart?oid=671621379&format=image",
                        "https://docs.google.com/spreadsheets/d/e/2PACX-1vQvqbjnVbMKsGrgbm5vMFaQQZ8cLYC_aPlrch3xpdOy2qOpi8wyBEy1_GdRGY1T2Fp7s3HXMeIYELjq/pubchart?oid=798069448&format=image"
                    };

                    double NomeArquivoContagemRespostas = DateTime.Now.ToOADate();

                    int nForeach = 10, aumentaUm = 1;

                    await ctx.RespondAsync($"Aqui estão todos os dados do censo: {ctx.Member.Mention} :point_down:");

                    foreach (var link in linksPlanilha)
                    {
                        nForeach = 10 * aumentaUm;
                        
                        caminhoFinal = caminho + $@"\{NomeArquivoContagemRespostas + nForeach}.png";

                        webClient.DownloadFile(link, caminhoFinal);

                        await ctx.RespondWithFileAsync(caminhoFinal);

                        File.Delete(caminhoFinal);

                        ++aumentaUm;
                    }

                    webClient.Dispose();
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("unmute"), Aliases("desmutar"), Description("Membro[ID/Menção]`\nCancela o mute do membro e devolve os cargos. **SÓ EXECUTE ESTE COMANDO SE O BOT ESTIVER ONLINE DESDE A PRISÃO DO MEMBRO.**\n\n")]

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
                        embed.WithColor(new DiscordColor(0x32363c))
                            .WithAuthor("Como executar este comando:", null, Valores.infoLogo)
                            .AddField("PC/Mobile", $"{ctx.Prefix}s unmute Membro[ID/Menção]")
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    DiscordRole cargoPrisioneiro = ctx.Guild.GetRole(ctx.Guild.Channels.Values.ToList().Find(x => x.Name.ToUpper().Contains(Valores.Cargos.cargoPrisioneiro)).Id);

                    if (!membro.Roles.Contains(cargoPrisioneiro))
                    {
                        embed.WithColor(cargoPrisioneiro.Color)
                            .WithAuthor("Este membro não está na prisão!", null, Valores.logoUBGE)
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithDescription(":thinking:")
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    var local = Program.ubgeBot.localDB;
                    var collectionInfracao = local.GetCollection<Infracao>(Valores.Mongo.infracoes);

                    var filtro = Builders<Infracao>.Filter.Eq(m => m.idInfrator, membro.Id);
                    var listaInfracoes = await (await collectionInfracao.FindAsync(filtro)).ToListAsync();

                    if (listaInfracoes.Count == 0)
                    {
                        embed.WithAuthor($"Este membro não contêm infrações.", null, Valores.logoUBGE)
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithDescription(":thinking:")
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
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
                            embed.WithAuthor($"Erro!", null, Valores.logoUBGE)
                                .WithThumbnailUrl(membro.AvatarUrl)
                                .WithDescription($"O bot provavelmente deve ter caído depois que esta prisão foi efetuada, caso queira somente devolver os " +
                                $"cargos do membro: {membro.Mention}, digite: `{ctx.Prefix}dc {membro.Id}`. {await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "leofsjal")}")
                                .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                                .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                                .WithTimestamp(DateTime.Now);

                            await ctx.RespondAsync(embed: embed.Build());
                            return;
                        }

                        DiscordMessage msgAguarde = await ctx.RespondAsync("Aguarde...");

                        if (membro.Roles.Contains(cargoPrisioneiro))
                            await membro.RevokeRoleAsync(cargoPrisioneiro);

                        DiscordRole cargoParaDevolver = null;

                        StringBuilder strCargos = new StringBuilder();

                        foreach (var cargosForeach in listaInfracoes.LastOrDefault().dadosPrisao.cargosDoMembro)
                        {
                            cargoParaDevolver = ctx.Guild.GetRole(cargosForeach);

                            await Task.Delay(200);

                            await membro.GrantRoleAsync(cargoParaDevolver);

                            strCargos.Append($"{cargoParaDevolver.Mention} | ");
                        }

                        embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithAuthor($"Todos os cargos do membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}\" foram devolvidos com sucesso!", null, Valores.logoUBGE)
                            .WithDescription($"Cargos: {strCargos.ToString()}\n\n**PS:** A prisão do membro foi cancelada, mas a infração ainda está guardada.")
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await msgAguarde.DeleteAsync();
                        await ctx.RespondAsync(embed: embed.Build());

                        cancellationTokenSource.Dispose();
                    }
                }
                catch (DSharpPlus.Exceptions.NotFoundException)
                {
                    await ctx.RespondAsync($"{ctx.Member.Mention}, por motivos desconhecidos, o Discord não encontrou este membro.");
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("prisioneiros"), Aliases("prisioneiro"), Description("`\nMostra todos os membros que estão com o cargo de prisioneiro.\n\n")]

        public async Task ListarPrisioneirosAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordRole prisioneiroCargo = ctx.Guild.GetRole(ctx.Guild.Channels.Values.ToList().Find(x => x.Name.ToUpper().Contains(Valores.Cargos.cargoPrisioneiro)).Id);

                    StringBuilder strPrisioneiros = new StringBuilder();

                    var membrosUBGE = (await ctx.Guild.GetAllMembersAsync()).ToList().Where(Membro => Membro.Roles.Contains(prisioneiroCargo));

                    foreach (var membro in membrosUBGE)
                        strPrisioneiros.Append($"{Program.ubgeBot.utilidadesGerais.MencaoMembro(membro)} - {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator} - ID: `{membro.Id}`\n");

                    await ctx.TriggerTypingAsync();

                    DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { Name = "Membros que estão na prisão:", IconUrl = Valores.logoUBGE },
                        Description = strPrisioneiros.ToString(),
                        Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}" },
                        Color = Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed(),
                    };

                    await ctx.RespondAsync(embed: Embed.Build());
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception); ;
                }
            }).Start();
        }
    }
}