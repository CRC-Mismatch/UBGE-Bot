using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UBGE_Bot.Main;
using UBGE_Bot.MongoDB.Modelos;
using UBGE_Bot.Utilidades;
using UBGE_Bot.LogExceptions;

namespace UBGE_Bot.Comandos.Salas_de_Voz
{
    [Group("sala"), UBGE]

    public class MemberControlled : BaseCommandModule
    {
        [Command("addmembro"), Aliases("adicionarmembro"), Description("[<@Amigo>]`\nAdiciona um amigo à lista branca.\n\n")]

        public async Task AddAsync(CommandContext ctx, DiscordMember membro = null)
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
                            .AddField("PC/Mobile", $"{ctx.Prefix}sala addmembro Membro[ID/Menção]")
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    var local = Program.ubgeBot.localDB;
                    var salasCollection = local.GetCollection<Salas>(Valores.Mongo.salas);

                    var filtroSalas = Builders<Salas>.Filter.Eq(x => x.idDoDono, ctx.Member.Id);
                    var resultadoSalas = await (await salasCollection.FindAsync(filtroSalas)).ToListAsync();

                    DiscordChannel voiceChannel = null;

                    if (resultadoSalas.Count == 0)
                    {
                        embed.WithAuthor($"❎ - Você não possui uma sala ativa!", null, Valores.logoUBGE);
                        embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                    else
                    {
                        DiscordRole registrado = ctx.Guild.GetRole(ctx.Guild.Roles.Values.ToList().Find(x => x.Name.ToUpper().Contains(Valores.Cargos.cargoMembroRegistrado)).Id);
                        voiceChannel = ctx.Guild.GetChannel(resultadoSalas[0].idDaSala);
                        
                        var lista = resultadoSalas[0].idsPermitidos;

                        if (!lista.Contains(membro.Id))
                        {
                            lista.Add(membro.Id);

                            await voiceChannel.AddOverwriteAsync(membro, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice);

                            await salasCollection.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(x => x.idsPermitidos, lista));

                            embed.WithAuthor($"✅ O usuário: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}\" foi adicionado à lista branca!", null, Valores.logoUBGE);
                            embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                            await ctx.RespondAsync(embed: embed.Build());
                        }
                        else
                        {
                            embed.WithAuthor($"✅ O usuário: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}\" já foi adicionado à lista branca!", null, Valores.logoUBGE);
                            embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                            await ctx.RespondAsync(embed: embed.Build());
                        }
                    }
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("delmembro"), Aliases("deletarmembro", "apagarmembro", "removemembro", "removermembro"), Description("[<@Amigo>]`\nRemove um amigo da lista branca.\n\n")]

        public async Task DelAsync(CommandContext ctx, DiscordMember membro = null)
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
                            .AddField("PC/Mobile", $"{ctx.Prefix}sala editar del Membro[ID/Menção]")
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    var local = Program.ubgeBot.localDB;
                    var salasCollection = local.GetCollection<Salas>(Valores.Mongo.salas);

                    var filtroSalas = Builders<Salas>.Filter.Eq(x => x.idDoDono, ctx.Member.Id);
                    var resultadoSalas = await (await salasCollection.FindAsync(filtroSalas)).ToListAsync();

                    DiscordChannel voiceChannel = null;

                    if (resultadoSalas.Count == 0)
                    {
                        embed.WithAuthor($"❎ - Você não possui uma sala ativa!", null, Valores.logoUBGE);
                        embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                    else
                    {
                        DiscordRole registrado = ctx.Guild.GetRole(ctx.Guild.Roles.Values.ToList().Find(x => x.Name.ToUpper().Contains(Valores.Cargos.cargoMembroRegistrado)).Id);
                        voiceChannel = ctx.Guild.GetChannel(resultadoSalas[0].idDaSala);

                        var lista = resultadoSalas[0].idsPermitidos;

                        if (lista.Contains(membro.Id))
                        {
                            lista.Remove(membro.Id);

                            await voiceChannel.AddOverwriteAsync(membro, Permissions.None, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice);

                            await salasCollection.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(x => x.idsPermitidos, lista));

                            embed.WithAuthor($"✅ - O usuário: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}\" foi removido da lista branca!", null, Valores.logoUBGE);
                            embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                            await ctx.RespondAsync(embed: embed.Build());
                        }
                        else
                        {
                            embed.WithAuthor($"✅ - O usuário: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}\" não foi encontrado na lista branca!", null, Valores.logoUBGE);
                            embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                            await ctx.RespondAsync(embed: embed.Build());
                        }
                    }
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("max"), Aliases("maximo", "máximo"), Description("[Número de 1 a 99]`\nLimite de Membros.\n\n")]

        public async Task MaxAsync(CommandContext ctx, int maxJoin)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    var local = Program.ubgeBot.localDB;
                    var salasCollection = local.GetCollection<Salas>(Valores.Mongo.salas);

                    var filtroSalas = Builders<Salas>.Filter.Eq(x => x.idDoDono, ctx.Member.Id);
                    var resultadoSalas = await (await salasCollection.FindAsync(filtroSalas)).ToListAsync();

                    DiscordChannel voiceChannel = null;

                    if (resultadoSalas.Count == 0)
                    {
                        embed.WithAuthor($"❎ - Você não possui uma sala ativa!", null, Valores.logoUBGE);
                        embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                    else
                    {
                        DiscordRole registrado = ctx.Guild.GetRole(ctx.Guild.Roles.Values.ToList().Find(x => x.Name.ToUpper().Contains(Valores.Cargos.cargoMembroRegistrado)).Id);
                        voiceChannel = ctx.Guild.GetChannel(resultadoSalas[0].idDaSala);

                        var oldmaxJoin = resultadoSalas[0].limiteDeUsuarios;

                        if (oldmaxJoin != maxJoin && maxJoin != 0 && maxJoin < 101)
                        {
                            await voiceChannel.ModifyAsync(x => { x.Userlimit = maxJoin; });

                            await salasCollection.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(x => x.limiteDeUsuarios, maxJoin));

                            embed.WithAuthor($"✅ - O número máximo de usuários da sala foi atualizado!", null, Valores.logoUBGE);
                            embed.WithDescription($"De: **{oldmaxJoin}**\n" +
                                $"Para: **{maxJoin}**");
                            embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                            await ctx.RespondAsync(embed: embed.Build());
                        }
                        else if (oldmaxJoin == maxJoin)
                        {
                            embed.WithAuthor($"✅ - O número máximo de usuários se manteve em: {oldmaxJoin}.", null, Valores.logoUBGE);
                            embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                            await ctx.RespondAsync(embed: embed.Build());
                        }
                        else if (maxJoin == 0)
                        {
                            await voiceChannel.ModifyAsync(x => { x.Userlimit = (int)maxJoin; });

                            await salasCollection.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(x => x.limiteDeUsuarios, maxJoin));

                            embed.WithAuthor($"✅ - O limite de usuários foi removido!", null, Valores.logoUBGE);
                            embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                            await ctx.RespondAsync(embed: embed.Build());
                        }
                        else if (maxJoin > 100)
                        {
                            maxJoin = 0;

                            await voiceChannel.ModifyAsync(x => { x.Userlimit = (int)maxJoin; });

                            var Update = Builders<Salas>.Update.Set(x => x.limiteDeUsuarios, maxJoin);
                            await salasCollection.UpdateOneAsync(filtroSalas, Update);

                            embed.WithAuthor($"❎ - Você colocou um número maior que 100, então o limite de usuários foi retirado.", null, Valores.logoUBGE);
                            embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());
                            embed.WithDescription("O limite de usuários foi retirado.");

                            await ctx.RespondAsync(embed: embed.Build());
                        }
                        else if (maxJoin < 0)
                        {
                            maxJoin = 0;

                            await voiceChannel.ModifyAsync(x => { x.Userlimit = (int)maxJoin; });

                            await salasCollection.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(x => x.limiteDeUsuarios, maxJoin));

                            embed.WithAuthor($"❎ - Você colocou um número menor que 0, então o limite de usuários foi retirado.", null, Valores.logoUBGE);
                            embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                            await ctx.RespondAsync(embed: embed.Build());
                        }
                    }
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("lock"), Aliases("trancar"), Description("`\nTrava a sua sala.\n\n")]

        public async Task LockAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    var local = Program.ubgeBot.localDB;
                    var salasCollection = local.GetCollection<Salas>(Valores.Mongo.salas);

                    var filtroSalas = Builders<Salas>.Filter.Eq(x => x.idDoDono, ctx.Member.Id);
                    var resultadoSalas = await (await salasCollection.FindAsync(filtroSalas)).ToListAsync();

                    DiscordChannel voiceChannel = null;
                    DiscordMember m = null;

                    DiscordRole moderadorDiscordCargo = ctx.Guild.GetRole(ctx.Guild.Roles.Values.ToList().Find(x => x.Name.ToUpper().Contains(Valores.Cargos.cargoModeradorDiscord)).Id),
                    acessoGeralCargo = ctx.Guild.GetRole(ctx.Guild.Roles.Values.ToList().Find(x => x.Name.ToUpper().Contains(Valores.Cargos.cargoAcessoGeral)).Id);

                    if (resultadoSalas.Count != 0)
                    {
                        voiceChannel = ctx.Guild.GetChannel(resultadoSalas[0].idDaSala);

                        if (resultadoSalas[0].salaTrancada)
                        {
                            embed.WithAuthor($"❎ - Esta sala já está trancada!", null, Valores.logoUBGE);
                            embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                            await ctx.RespondAsync(embed: embed.Build());
                        }
                        else if (!resultadoSalas[0].salaTrancada)
                        {
                            await salasCollection.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(s => s.salaTrancada, true));

                            foreach (ulong u in resultadoSalas[0].idsPermitidos)
                            {
                                m = await ctx.Guild.GetMemberAsync(u);

                                await voiceChannel.AddOverwriteAsync(m, Permissions.AccessChannels | Permissions.UseVoice | Permissions.Speak);
                            }

                            await voiceChannel.AddOverwriteAsync(ctx.Guild.EveryoneRole, Permissions.AccessChannels, Permissions.UseVoice | Permissions.Speak);
                            await voiceChannel.AddOverwriteAsync(moderadorDiscordCargo, Permissions.AccessChannels, Permissions.UseVoice | Permissions.Speak);
                            await voiceChannel.AddOverwriteAsync(acessoGeralCargo, Permissions.AccessChannels, Permissions.UseVoice | Permissions.Speak);

                            embed.WithAuthor($"✅ - Sala travada!", null, Valores.logoUBGE);
                            embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                            await ctx.RespondAsync(embed: embed.Build());
                        }
                    }
                    else
                    {
                        embed.WithAuthor($"❎ - Você não possui uma sala ativa!", null, Valores.logoUBGE);
                        embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("unlock"), Aliases("destrancar"), Description("`\nDestrava a sua sala.\n\n")]

        public async Task UnlockAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    var local = Program.ubgeBot.localDB;
                    var salasCollection = local.GetCollection<Salas>(Valores.Mongo.salas);

                    var filtroSalas = Builders<Salas>.Filter.Eq(x => x.idDoDono, ctx.Member.Id);
                    var resultadoSalas = await (await salasCollection.FindAsync(filtroSalas)).ToListAsync();

                    DiscordChannel voiceChannel = null;

                    DiscordRole moderadorDiscordCargo = ctx.Guild.GetRole(ctx.Guild.Roles.Values.ToList().Find(x => x.Name.ToUpper().Contains(Valores.Cargos.cargoModeradorDiscord)).Id),
                    acessoGeralCargo = ctx.Guild.GetRole(ctx.Guild.Roles.Values.ToList().Find(x => x.Name.ToUpper().Contains(Valores.Cargos.cargoAcessoGeral)).Id);

                    if (resultadoSalas.Count != 0)
                    {
                        voiceChannel = ctx.Guild.GetChannel(resultadoSalas[0].idDaSala);

                        if (resultadoSalas[0].salaTrancada)
                        {
                            await salasCollection.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(s => s.salaTrancada, false));

                            await voiceChannel.AddOverwriteAsync(ctx.Guild.EveryoneRole, Permissions.AccessChannels | Permissions.UseVoice | Permissions.Speak);
                            await voiceChannel.AddOverwriteAsync(moderadorDiscordCargo, Permissions.AccessChannels | Permissions.UseVoice | Permissions.Speak);
                            await voiceChannel.AddOverwriteAsync(acessoGeralCargo, Permissions.AccessChannels | Permissions.UseVoice | Permissions.Speak);

                            embed.WithAuthor($"✅ - Sala destravada!", null, Valores.logoUBGE);
                            embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                            await ctx.RespondAsync(embed: embed.Build());
                        }
                        else if (!resultadoSalas[0].salaTrancada)
                        {
                            embed.WithAuthor($"❎ - Esta sala já está destrancada!", null, Valores.logoUBGE);
                            embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                            await ctx.RespondAsync(embed: embed.Build());
                        }
                    }
                    else
                    {
                        embed.WithAuthor($"❎ - Você não possui uma sala ativa!", null, Valores.logoUBGE);
                        embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("nome"), Description("[Nome Novo]`\nAltera o nome da sala.\n\n")]
        [Aliases("name")]

        public async Task NomeAsync(CommandContext ctx, [RemainingText] string nome = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    if (string.IsNullOrWhiteSpace(nome))
                    {
                        embed.WithColor(new DiscordColor(0x32363c))
                            .WithAuthor("Como executar este comando:", null, Valores.infoLogo)
                            .AddField("PC/Mobile", $"{ctx.Prefix}sala nome Nome[Novo nome da sala]")
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    var local = Program.ubgeBot.localDB;
                    var salasCollection = local.GetCollection<Salas>(Valores.Mongo.salas);

                    var filtroSalas = Builders<Salas>.Filter.Eq(x => x.idDoDono, ctx.Member.Id);
                    var resultadoSalas = await (await salasCollection.FindAsync(filtroSalas)).ToListAsync();

                    DiscordChannel voiceChannel = null;

                    if (resultadoSalas.Count != 0)
                    {
                        voiceChannel = ctx.Guild.GetChannel(resultadoSalas[0].idDaSala);

                        await salasCollection.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(s => s.nomeDaSala, nome));

                        await voiceChannel.ModifyAsync(x => x.Name = nome);

                        embed.WithAuthor($"✅ - Sala renomeada para: \"{nome}\".", null, Valores.logoUBGE);
                        embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                    else
                    {
                        embed.WithAuthor($"❎ - Você não possui uma sala ativa!", null, Valores.logoUBGE);
                        embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                        await ctx.RespondAsync(embed: embed.Build());
                    }
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }
    }

    public sealed class Create : BaseCommandModule
    {
        [Command("create"), UBGE]
        [Aliases("criar", "summon")]

        public async Task CriarCanalAsync(CommandContext ctx)
        {
            var dm = await ctx.Member.CreateDmChannelAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordChannel CrieSuaSalaAq = ctx.Guild.Channels.Values.ToList().Find(x => x.Name.ToUpper().Contains("CLIQUE AQUI!"));

                    await dm.SendMessageAsync($"{ctx.Member.Mention}, entre no canal de voz: `{CrieSuaSalaAq.Name}` nesta categoria para criar um canal de voz personalizado!");
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }
    }
}