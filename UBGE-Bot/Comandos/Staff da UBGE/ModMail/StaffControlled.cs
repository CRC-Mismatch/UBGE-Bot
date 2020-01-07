﻿using Autofac;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Google.Apis.Drive.v3;
using ModMail_ = UBGE_Bot.MongoDB.Modelos.ModMail;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UBGE_Bot.APIs;
using UBGE_Bot.Main;
using UBGE_Bot.Utilidades;

namespace UBGE_Bot.Comandos.Staff_da_UBGE.ModMail
{
    [Group("modmail"), Aliases("mm"), UBGE, BotConectadoAoMongo]

    public sealed class StaffControlled : BaseCommandModule
    {
        [Command("fecharcanal"), Aliases("fecharcaso", "fc"), Description("`\nFecha o caso e armazena o mesmo.")]

        public async Task FecharCanalAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            IMongoDatabase db = Program.ubgeBot.localDB;

            IMongoCollection<ModMail_> collectionModMail = db.GetCollection<ModMail_>(Valores.Mongo.modMail);
            FilterDefinition<ModMail_> filtroCollectionModMail = Builders<ModMail_>.Filter.Eq(x => x.idDoCanal, ctx.Channel.Id);
            List<ModMail_> resultadoCollectionModMail = await (await collectionModMail.FindAsync(filtroCollectionModMail)).ToListAsync();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            if (resultadoCollectionModMail.Count == 0)
            {
                embed.WithAuthor("Ops!", null, Valores.logoUBGE)
                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithDescription("Acho que você digitou no canal errado! :smile:")
                    .WithThumbnailUrl(ctx.Member.AvatarUrl)
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: embed.Build());
                return;
            }
            else
            {
                GoogleDrive.Main credenciais = Program.ubgeBot.servicesIContainer.Resolve<GoogleDrive.Main>();

                DiscordMessage msgAguarde = await ctx.Channel.SendMessageAsync($"Checando resultado, aguarde... {await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "leofsjal")}"), ultimaMsg = null;

                int nMsgs = 50000, msgRepetida = 1;

                ModMail_ ultimoResultadoModMail = resultadoCollectionModMail.LastOrDefault();

                if (ultimoResultadoModMail.denuncia == null)
                    await collectionModMail.UpdateOneAsync(filtroCollectionModMail, Builders<ModMail_>.Update.Set(x => x.contato.oCanalFoiFechado, true));
                else if (ultimoResultadoModMail.contato == null)
                    await collectionModMail.UpdateOneAsync(filtroCollectionModMail, Builders<ModMail_>.Update.Set(x => x.denuncia.oCanalFoiFechado, true));

                await msgAguarde.ModifyAsync($"Armazenando a conversa em um .txt, aguarde... {await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "leofsjal")}");

                string diretorioTXT = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    diaHoraParaOTXT = $"{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}",
                    nomeDoTXT = $"{ctx.Channel.Name}_{diaHoraParaOTXT}";

                using (StreamWriter sw = new StreamWriter(diretorioTXT + $@"\{nomeDoTXT}.txt", false, Encoding.UTF8))
                {
                    for (int i = 1; i < nMsgs; i++)
                    {
                        if (msgRepetida > 1)
                            break;

                        IReadOnlyList<DiscordMessage> msg = await ctx.Channel.GetMessagesAsync(i);

                        if (msg.LastOrDefault().Content.ToLower().Contains($"//mm") || msg.LastOrDefault().Content.ToLower().Contains($"$mm") || msg.LastOrDefault().Content.ToLower().Contains($"ubge!mm"))
                            continue;

                        if (msg.LastOrDefault().Content.ToLower().Contains($"armazenando a conversa"))
                            continue;

                        if (ultimaMsg == null)
                        {
                            ultimaMsg = msg.LastOrDefault();

                            goto escreveNoTXT;
                        }
                        else
                            ultimaMsg = msg[i - 2];

                        if (msg.LastOrDefault().Content.ToLower() == ultimaMsg.Content.ToLower())
                        {
                            ++msgRepetida;

                            continue;
                        }

                    escreveNoTXT:

                        await sw.WriteLineAsync($"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(await ctx.Guild.GetMemberAsync(msg.LastOrDefault().Author.Id))} ({msg.LastOrDefault().Author.Id}) - [{msg.LastOrDefault().CreationTimestamp.DateTime.ToString()}]");

                        StringBuilder str = new StringBuilder();

                        if (msg.LastOrDefault().Attachments.Count != 0)
                        {
                            foreach (DiscordAttachment anexo in msg.LastOrDefault().Attachments)
                                str.Append($"{anexo.Url}, ");
                        }

                        StringBuilder strDescricaoEmbed = new StringBuilder();

                        if (msg.LastOrDefault().Embeds.Count != 0)
                        {
                            foreach (DiscordEmbed embedForeach in msg.LastOrDefault().Embeds)
                                strDescricaoEmbed.Append($"{embedForeach.Description}");
                        }

                        await sw.WriteLineAsync($"{(string.IsNullOrWhiteSpace(msg.LastOrDefault().Content) ? $"{(string.IsNullOrWhiteSpace(strDescricaoEmbed.ToString()) ? "Está mensagem pode ser um embed, por isso está vazia." : strDescricaoEmbed.ToString())}" : msg.LastOrDefault().Content)}{(string.IsNullOrWhiteSpace(str.ToString()) ? string.Empty : $" - Links anexados a mensagem: {(str.ToString().EndsWith(", ") ? str.ToString().Remove(str.Length - 2) : str.ToString())}")}");
                        await sw.WriteLineAsync(string.Empty);
                    }
                }

                await msgAguarde.ModifyAsync($"Upando no Google Drive, aguarde... {await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "leofsjal")}");

                using (DriveService servico = credenciais.ServicoDoDrive(await credenciais.Autenticar()))
                    await credenciais.UploadArquivo(servico, diretorioTXT + $@"\{nomeDoTXT}.txt", "ModMail | UBGE-Bot", false);

                await msgAguarde.DeleteAsync();

                await ctx.Guild.GetChannel(ultimoResultadoModMail.idDoCanal).DeleteAsync();

                DiscordMember membro = await ctx.Guild.GetMemberAsync(ultimoResultadoModMail.idDoMembro);

                embed.WithAuthor("Obrigado!", null, Valores.logoUBGE)
                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithDescription("Seu caso foi armazenado pela staff da UBGE e foi declarado como resolvido, em meio a isso, venho agradecer por colaborar com um servidor mais agradável a todos os membros! Que a ética esteja com você :wink:")
                    .WithThumbnailUrl(membro.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await (await membro.CreateDmChannelAsync()).SendMessageAsync(embed: embed.Build());
            }
        }
    }
}