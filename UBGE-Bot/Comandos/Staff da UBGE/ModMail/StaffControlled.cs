using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using ModMail_ = UBGE_Bot.MongoDB.Modelos.ModMail;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UBGE_Bot.LogExceptions;
using UBGE_Bot.Main;
using UBGE_Bot.Utilidades;

namespace UBGE_Bot.Comandos.Staff_da_UBGE.ModMail
{
    [Group("modmail"), Aliases("mm"), UBGE]

    public sealed class StaffControlled : BaseCommandModule
    {
        [Command("fecharcanal"), Aliases("fecharcaso", "fc")]

        public async Task FecharCanalAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try 
                {
                    var db = Program.ubgeBot.localDB;

                    var collectionModMail = db.GetCollection<ModMail_>(Valores.Mongo.modMail);
                    var filtroCollectionModMail = Builders<ModMail_>.Filter.Eq(x => x.idDoCanal, ctx.Channel.Id);
                    var resultadoCollectionModMail = await (await collectionModMail.FindAsync(filtroCollectionModMail)).ToListAsync();

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
                        var ultimoResultadoModMail = resultadoCollectionModMail.LastOrDefault();

                        await ctx.Guild.GetChannel(ultimoResultadoModMail.idDoCanal).DeleteAsync();

                        if (ultimoResultadoModMail.denuncia == null)
                            await collectionModMail.UpdateOneAsync(filtroCollectionModMail, Builders<ModMail_>.Update.Set(x => x.contato.oCanalFoiFechado, true));
                        else if (ultimoResultadoModMail.contato == null)
                            await collectionModMail.UpdateOneAsync(filtroCollectionModMail, Builders<ModMail_>.Update.Set(x => x.denuncia.oCanalFoiFechado, true));

                        DiscordMember membro = await ctx.Guild.GetMemberAsync(ultimoResultadoModMail.idDoMembro);

                        embed.WithAuthor("Obrigado!", null, Valores.logoUBGE)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithDescription("Seu caso foi armazenado pela staff da UBGE e foi declarado como resolvido, em meio a isso, venho agradecer por colaborar com um servidor mais agradável a todos os membros! Que a ética esteja com você :wink:")
                            .WithThumbnailUrl(membro.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await (await membro.CreateDmChannelAsync()).SendMessageAsync(embed: embed.Build());
                    }
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);    
                }
            }).Start();
        }
    }
}