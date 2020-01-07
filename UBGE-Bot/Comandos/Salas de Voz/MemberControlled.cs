using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UBGE_Bot.Main;
using UBGE_Bot.MongoDB.Modelos;
using UBGE_Bot.Utilidades;

namespace UBGE_Bot.Comandos.Salas_de_Voz
{
    [Group("sala"), UBGE, BotConectadoAoMongo]

    public class MemberControlled : BaseCommandModule
    {
        [Command("addmembro"), Aliases("adicionarmembro"), Description("[<@Amigo>]`\nAdiciona um amigo à lista branca.\n\n")]

        public async Task AddMembroNaSalaAsync(CommandContext ctx, DiscordMember membro = null)
        {
            await ctx.TriggerTypingAsync();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            if (membro == null)
            {
                embed.WithColor(Program.ubgeBot.utilidadesGerais.CorHelpComandos())
                    .WithAuthor("Como executar este comando:", null, Valores.infoLogo)
                    .AddField("PC/Mobile", $"{ctx.Prefix}sala addmembro Membro[ID/Menção]")
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: embed.Build());
                return;
            }

            if (ctx.Message.MentionedUsers.Count > 1)
            {
                embed.WithAuthor($"❎ - Adicione um membro por vez!", null, Valores.logoUBGE);
                embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                await ctx.RespondAsync(embed: embed.Build());

                return;
            }

            IMongoDatabase local = Program.ubgeBot.localDB;
            IMongoCollection<Salas> salasCollection = local.GetCollection<Salas>(Valores.Mongo.salas);

            FilterDefinition<Salas> filtroSalas = Builders<Salas>.Filter.Eq(x => x.idDoDono, ctx.Member.Id);
            List<Salas> resultadoSalas = await (await salasCollection.FindAsync(filtroSalas)).ToListAsync();

            if (resultadoSalas.Count == 0 || ctx.Guild.GetChannel(resultadoSalas[0].idDaSala) == null)
            {
                embed.WithAuthor($"❎ - Você não possui uma sala ativa!", null, Valores.logoUBGE);
                embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                await ctx.RespondAsync(embed: embed.Build());
            }
            else
            {
                if (resultadoSalas[0].idDoDono == membro.Id)
                {
                    embed.WithAuthor("❎ - Você não pode adicionar a si mesmo!", null, Valores.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                    await ctx.RespondAsync(embed: embed.Build());

                    return;
                }

                DiscordChannel voiceChannel = ctx.Guild.GetChannel(resultadoSalas[0].idDaSala);

                List<ulong> lista = resultadoSalas[0].idsPermitidos;

                if (!lista.Contains(membro.Id))
                {
                    lista.Add(membro.Id);

                    await voiceChannel.AddOverwriteAsync(membro, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice);

                    await salasCollection.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(x => x.idsPermitidos, lista));

                    embed.WithAuthor($"✅ O usuário: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\" foi adicionado à lista branca!", null, Valores.logoUBGE);
                    embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                    await ctx.RespondAsync(embed: embed.Build());
                }
                else
                {
                    embed.WithAuthor($"✅ O usuário: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\" já está adicionado na lista branca!", null, Valores.logoUBGE);
                    embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                    await ctx.RespondAsync(embed: embed.Build());
                }
            }
        }

        [Command("delmembro"), Aliases("deletarmembro", "apagarmembro", "removemembro", "removermembro"), Description("[<@Amigo>]`\nRemove um amigo da lista branca.\n\n")]

        public async Task DelMembroNaSalaAsync(CommandContext ctx, DiscordMember membro = null)
        {
            await ctx.TriggerTypingAsync();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            if (membro == null)
            {
                embed.WithColor(Program.ubgeBot.utilidadesGerais.CorHelpComandos())
                    .WithAuthor("Como executar este comando:", null, Valores.infoLogo)
                    .AddField("PC/Mobile", $"{ctx.Prefix}sala editar del Membro[ID/Menção]")
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: embed.Build());
                return;
            }

            if (ctx.Message.MentionedUsers.Count > 1)
            {
                embed.WithAuthor($"❎ - Remova um membro por vez!", null, Valores.logoUBGE);
                embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                await ctx.RespondAsync(embed: embed.Build());

                return;
            }

            IMongoDatabase local = Program.ubgeBot.localDB;
            IMongoCollection<Salas> salasCollection = local.GetCollection<Salas>(Valores.Mongo.salas);

            FilterDefinition<Salas> filtroSalas = Builders<Salas>.Filter.Eq(x => x.idDoDono, ctx.Member.Id);
            List<Salas> resultadoSalas = await (await salasCollection.FindAsync(filtroSalas)).ToListAsync();

            if (resultadoSalas.Count == 0 || ctx.Guild.GetChannel(resultadoSalas[0].idDaSala) == null)
            {
                embed.WithAuthor($"❎ - Você não possui uma sala ativa!", null, Valores.logoUBGE);
                embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                await ctx.RespondAsync(embed: embed.Build());
            }
            else
            {
                if (resultadoSalas[0].idDoDono == membro.Id)
                {
                    embed.WithAuthor("❎ - Você não pode remover a si mesmo!", null, Valores.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                    await ctx.RespondAsync(embed: embed.Build());

                    return;
                }

                DiscordChannel voiceChannel = ctx.Guild.GetChannel(resultadoSalas[0].idDaSala);

                List<ulong> lista = resultadoSalas[0].idsPermitidos;

                if (lista.Contains(membro.Id))
                {
                    lista.Remove(membro.Id);

                    await voiceChannel.AddOverwriteAsync(membro, Permissions.None, Permissions.AccessChannels | Permissions.Speak | Permissions.UseVoice);

                    await salasCollection.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(x => x.idsPermitidos, lista));

                    embed.WithAuthor($"✅ - O usuário: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\" foi removido da lista branca!", null, Valores.logoUBGE);
                    embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                    await ctx.RespondAsync(embed: embed.Build());
                }
                else
                {
                    embed.WithAuthor($"✅ - O usuário: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\" não foi encontrado na lista branca!", null, Valores.logoUBGE);
                    embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                    await ctx.RespondAsync(embed: embed.Build());
                }
            }
        }

        [Command("max"), Aliases("maximo", "máximo"), Description("[Número de 1 a 99]`\nLimite de Membros.\n\n")]

        public async Task MaxSalaAsync(CommandContext ctx, int maxJoin)
        {
            await ctx.TriggerTypingAsync();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            IMongoDatabase local = Program.ubgeBot.localDB;
            IMongoCollection<Salas> salasCollection = local.GetCollection<Salas>(Valores.Mongo.salas);

            FilterDefinition<Salas> filtroSalas = Builders<Salas>.Filter.Eq(x => x.idDoDono, ctx.Member.Id);
            List<Salas> resultadoSalas = await (await salasCollection.FindAsync(filtroSalas)).ToListAsync();

            if (resultadoSalas.Count == 0 || ctx.Guild.GetChannel(resultadoSalas[0].idDaSala) == null)
            {
                embed.WithAuthor($"❎ - Você não possui uma sala ativa!", null, Valores.logoUBGE);
                embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                await ctx.RespondAsync(embed: embed.Build());
            }
            else
            {
                DiscordChannel voiceChannel = ctx.Guild.GetChannel(resultadoSalas[0].idDaSala);

                int oldmaxJoin = resultadoSalas[0].limiteDeUsuarios;

                if (oldmaxJoin != maxJoin && maxJoin != 0 && maxJoin < 101)
                {
                    await voiceChannel.ModifyAsync(x => x.Userlimit = maxJoin);

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
                    await voiceChannel.ModifyAsync(x => x.Userlimit = maxJoin);

                    await salasCollection.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(x => x.limiteDeUsuarios, maxJoin));

                    embed.WithAuthor($"✅ - O limite de usuários foi removido!", null, Valores.logoUBGE);
                    embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                    await ctx.RespondAsync(embed: embed.Build());
                }
                else if (maxJoin > 100)
                {
                    maxJoin = 0;

                    await voiceChannel.ModifyAsync(x => x.Userlimit = maxJoin);

                    UpdateDefinition<Salas> Update = Builders<Salas>.Update.Set(x => x.limiteDeUsuarios, maxJoin);
                    await salasCollection.UpdateOneAsync(filtroSalas, Update);

                    embed.WithAuthor($"❎ - Você colocou um número maior que 100, então o limite de usuários foi retirado.", null, Valores.logoUBGE);
                    embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                    await ctx.RespondAsync(embed: embed.Build());
                }
                else if (maxJoin < 0)
                {
                    maxJoin = 0;

                    await voiceChannel.ModifyAsync(x => x.Userlimit = maxJoin);

                    await salasCollection.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(x => x.limiteDeUsuarios, maxJoin));

                    embed.WithAuthor($"❎ - Você colocou um número menor que 0, então o limite de usuários foi retirado.", null, Valores.logoUBGE);
                    embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                    await ctx.RespondAsync(embed: embed.Build());
                }
            }
        }

        [Command("lock"), Aliases("trancar"), Description("`\nTrava a sua sala.\n\n")]

        public async Task LockSalasync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            IMongoDatabase local = Program.ubgeBot.localDB;
            IMongoCollection<Salas> salasCollection = local.GetCollection<Salas>(Valores.Mongo.salas);

            FilterDefinition<Salas> filtroSalas = Builders<Salas>.Filter.Eq(x => x.idDoDono, ctx.Member.Id);
            List<Salas> resultadoSalas = await (await salasCollection.FindAsync(filtroSalas)).ToListAsync();

            DiscordRole moderadorDiscordCargo = ctx.Guild.GetRole(Valores.Cargos.cargoModeradorDiscord),
            acessoGeralCargo = ctx.Guild.GetRole(Valores.Cargos.cargoAcessoGeral);

            if (resultadoSalas.Count != 0 && ctx.Guild.GetChannel(resultadoSalas[0].idDaSala) != null)
            {
                DiscordChannel voiceChannel = ctx.Guild.GetChannel(resultadoSalas[0].idDaSala);

                if (resultadoSalas[0].salaTrancada)
                {
                    embed.WithAuthor($"❎ - Esta sala já está trancada!", null, Valores.logoUBGE);
                    embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                    await ctx.RespondAsync(embed: embed.Build());
                }
                else if (!resultadoSalas[0].salaTrancada)
                {
                    await salasCollection.UpdateOneAsync(filtroSalas, Builders<Salas>.Update.Set(s => s.salaTrancada, true));

                    DiscordMember m = null;

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

        [Command("unlock"), Aliases("destrancar"), Description("`\nDestrava a sua sala.\n\n")]

        public async Task UnlockSalaAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            IMongoDatabase local = Program.ubgeBot.localDB;
            IMongoCollection<Salas> salasCollection = local.GetCollection<Salas>(Valores.Mongo.salas);

            FilterDefinition<Salas> filtroSalas = Builders<Salas>.Filter.Eq(x => x.idDoDono, ctx.Member.Id);
            List<Salas> resultadoSalas = await (await salasCollection.FindAsync(filtroSalas)).ToListAsync();

            if (resultadoSalas.Count != 0 && ctx.Guild.GetChannel(resultadoSalas[0].idDaSala) != null)
            {
                DiscordChannel voiceChannel = ctx.Guild.GetChannel(resultadoSalas[0].idDaSala);

                DiscordRole moderadorDiscordCargo = ctx.Guild.GetRole(Valores.Cargos.cargoModeradorDiscord),
                acessoGeralCargo = ctx.Guild.GetRole(Valores.Cargos.cargoAcessoGeral);

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

        [Command("nome"), Aliases("name"), Description("[Nome Novo]`\nAltera o nome da sala.\n\n")]

        public async Task NomeSalaAsync(CommandContext ctx, [RemainingText] string nome = null)
        {
            await ctx.TriggerTypingAsync();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            if (string.IsNullOrWhiteSpace(nome))
            {
                embed.WithColor(Program.ubgeBot.utilidadesGerais.CorHelpComandos())
                    .WithAuthor("Como executar este comando:", null, Valores.logoUBGE)
                    .AddField("PC/Mobile", $"{ctx.Prefix}sala nome Nome[Novo nome da sala]")
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: embed.Build());
                return;
            }
            else if (nome.Length > 40 || ctx.Message.MentionedUsers.Count != 0 || ctx.Message.MentionedRoles.Count != 0 ||
                ctx.Message.MentionedChannels.Count != 0 || ctx.Message.MentionEveryone ||
                Uri.IsWellFormedUriString(ctx.Message.Content.ToLower(), UriKind.RelativeOrAbsolute) ||
                ctx.Message.Attachments.Count != 0 || ctx.Message.Content.ToLower().Contains("https") ||
                ctx.Message.Content.ToLower().Contains("http") || ctx.Message.Content.ToLower().Contains("https:") ||
                ctx.Message.Content.ToLower().Contains("http:") || ctx.Message.Content.ToLower().Contains("https:/") ||
                ctx.Message.Content.ToLower().Contains("http:/") || ctx.Message.Content.ToLower().Contains("https://") ||
                ctx.Message.Content.ToLower().Contains("http://") || ctx.Message.Content.ToLower().Contains(".com"))
            {
                embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithAuthor("Erro!", null, Valores.logoUBGE)
                    .WithDescription($"Regras para a criação de nome de canais de voz:\n" +
                    $"- O nome do canal deve ter menos de **40** caracteres\n" +
                    $"- **Não** deve conter **menções** de **membros**\n" +
                    $"- **Não** deve conter **menções** de **cargos**\n" +
                    $"- **Não** deve conter **menções** de **canais**\n" +
                    $"- **Não** deve conter a **menção** do @everyone\n" +
                    $"- **Não** deve conter **links**")
                    .WithThumbnailUrl(ctx.Member.AvatarUrl)
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: embed.Build());
                return;
            }

            IMongoDatabase local = Program.ubgeBot.localDB;
            IMongoCollection<Salas> salasCollection = local.GetCollection<Salas>(Valores.Mongo.salas);

            FilterDefinition<Salas> filtroSalas = Builders<Salas>.Filter.Eq(x => x.idDoDono, ctx.Member.Id);
            List<Salas> resultadoSalas = await (await salasCollection.FindAsync(filtroSalas)).ToListAsync();

            DiscordChannel voiceChannel = null;

            if (resultadoSalas.Count != 0 && ctx.Guild.GetChannel(resultadoSalas[0].idDaSala) != null)
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

        [Command("listar"), Aliases("identificar"), Description("Canal[ID]`\nBusca informações do canal privado.")]

        public async Task IdentificarSalaAsync(CommandContext ctx, DiscordChannel sala = null)
        {
            await ctx.TriggerTypingAsync();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            if (sala == null)
            {
                embed.WithColor(Program.ubgeBot.utilidadesGerais.CorHelpComandos())
                    .WithAuthor("Como executar este comando:", null, Valores.infoLogo)
                    .AddField("PC/Mobile", $"{ctx.Prefix}sala identificar Sala[ID]")
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: embed.Build());
                return;
            }

            if (ctx.Message.MentionedChannels.Count > 1)
            {
                embed.WithColor(Program.ubgeBot.utilidadesGerais.CorHelpComandos())
                    .WithAuthor("Digite só um ID de um canal por vez!", null, Valores.logoUBGE)
                    .WithDescription(":thinking:")
                    .WithThumbnailUrl(ctx.Member.AvatarUrl)
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: embed.Build());
                return;
            }

            IMongoDatabase local = Program.ubgeBot.localDB;
            IMongoCollection<Salas> salasCollection = local.GetCollection<Salas>(Valores.Mongo.salas);

            FilterDefinition<Salas> filtroSalas = Builders<Salas>.Filter.Eq(x => x.idDaSala, sala.Id);
            List<Salas> resultadoSalas = await (await salasCollection.FindAsync(filtroSalas)).ToListAsync();

            if (resultadoSalas.Count != 0 && ctx.Guild.GetChannel(sala.Id) != null)
            {
                DiscordChannel sala_ = ctx.Guild.GetChannel(sala.Id);

                Salas ultimaRespostaSala = resultadoSalas.LastOrDefault();

                StringBuilder str = new StringBuilder();

                foreach (ulong membros in ultimaRespostaSala.idsPermitidos)
                {
                    if (membros != ultimaRespostaSala.idDoDono)
                        str.Append($"{Program.ubgeBot.utilidadesGerais.MencaoMembro(await ctx.Guild.GetMemberAsync(membros))}, ");
                }

                string membrosPermitidos = string.Empty;

                if (!string.IsNullOrWhiteSpace(str.ToString()))
                    membrosPermitidos = str.ToString().EndsWith(", ") ? str.ToString().Remove(str.Length - 2) : str.ToString();
                else
                    membrosPermitidos = "**Não existe restrição de id's neste canal de voz.**";

                embed.WithAuthor($"Informações do canal de voz: \"{sala_.Name}\"", null, Valores.logoUBGE)
                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithThumbnailUrl(ctx.Member.AvatarUrl)
                    .WithDescription($"Dono: {Program.ubgeBot.utilidadesGerais.MencaoMembro(await ctx.Guild.GetMemberAsync(ultimaRespostaSala.idDoDono))}\n\n" +
                    $"Limite de usuários: **{(ultimaRespostaSala.limiteDeUsuarios == 0 ? "Não tem limite." : $"{ultimaRespostaSala.limiteDeUsuarios} membros.")}**\n\n" +
                    $"Esta sala está trancada?: **{(ultimaRespostaSala.salaTrancada ? "Sim" : "Não")}**\n\n" +
                    $"Id's permitidos para entrar na sala: {membrosPermitidos}\n\n" +
                    $"Número de membros no canal: {(sala_.Users.Count() > 1 ? $"**{sala_.Users.Count()}** membros." : $"**{sala_.Users.Count()}** membro.")}");

                await ctx.RespondAsync(embed: embed.Build());
            }
            else
            {
                embed.WithAuthor($"❎ - Esta sala não existe ou você colocou o ID errado!", null, Valores.logoUBGE);
                embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                await ctx.RespondAsync(embed: embed.Build());
            }
        }
    }

    public sealed class HelpDoMemberControlled : BaseCommandModule
    {
        [Command("create"), Aliases("criar", "summon"), UBGE_CrieSuaSalaAqui]

        public async Task CriarCanalAsync(CommandContext ctx)
        {
            DiscordChannel cliqueAqui = ctx.Guild.GetChannel(Valores.ChatsUBGE.canalCliqueAqui);

            await ctx.RespondAsync($"{ctx.Member.Mention}, entre no canal de voz: `{cliqueAqui.Name}` na categoria `{cliqueAqui.Parent.Name}` para criar um canal de voz personalizado!");
        }
    }
}