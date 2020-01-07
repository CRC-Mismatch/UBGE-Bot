using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UBGE_Bot.Carregamento;
using UBGE_Bot.Main;
using UBGE_Bot.MongoDB.Modelos;
using UBGE_Bot.LogExceptions;
using UBGE_Bot.Utilidades;

namespace UBGE_Bot.Sistemas.Discord
{
    public sealed class ReactRoleUBGE : IAplicavelAoCliente
    {
        public void AplicarAoBot(DiscordClient discordClient, bool botConectadoAoMongo, bool sistemaAtivo)
        {
            if (botConectadoAoMongo && sistemaAtivo)
            {
                discordClient.MessageReactionAdded += ReacaoAdicionadaReactRole;
                discordClient.MessageReactionRemoved += ReacaoRemovidaReactRole;
            }
        }

        private async Task ReacaoAdicionadaReactRole(MessageReactionAddEventArgs messageReactionAddEventArgs)
        {
            if (messageReactionAddEventArgs == null || messageReactionAddEventArgs.Channel.IsPrivate || messageReactionAddEventArgs.User.IsBot)
                return;

            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    IMongoDatabase db = Program.ubgeBot.localDB;

                    IMongoCollection<Reacts> reacts = db.GetCollection<Reacts>(Valores.Mongo.reacts);
                    List<Reacts> resultadoReacts = await (await reacts.FindAsync(Builders<Reacts>.Filter.Eq(x => x.idDoCanal, messageReactionAddEventArgs.Channel.Id))).ToListAsync();

                    if (resultadoReacts.Count != 0)
                    {
                        IMongoCollection<Jogos> jogos = db.GetCollection<Jogos>(Valores.Mongo.jogos);

                        DiscordEmoji emojiReacao = messageReactionAddEventArgs.Emoji;

                        FilterDefinition<Jogos> filtroJogos = Builders<Jogos>.Filter.Eq(x => x.idDoEmoji, emojiReacao.Id);
                        List<Jogos> resultadoJogos = await (await jogos.FindAsync(filtroJogos)).ToListAsync();

                        if (resultadoJogos.Count == 0)
                            return;

                        DiscordGuild guildReaction = messageReactionAddEventArgs.Guild;
                        DiscordMessage mensagemEmoji = messageReactionAddEventArgs.Message;
                        DiscordChannel canalReaction = messageReactionAddEventArgs.Channel;

                        IReadOnlyList<DiscordUser> reacoesMembro = await mensagemEmoji.GetReactionsAsync(emojiReacao);

                        Jogos ultimoResultadoJogos = resultadoJogos.LastOrDefault();
                        Reacts ultimoResultadoReacts = resultadoReacts.LastOrDefault();

                        bool resultadoDiferente = true;

                        if (resultadoJogos.Count == 0 || (resultadoJogos.Count != 0 && guildReaction.GetRole(ultimoResultadoJogos.idDoCargo) == null))
                            resultadoDiferente = false;

                        DiscordGuild UBGE = await messageReactionAddEventArgs.Client.GetGuildAsync(Valores.Guilds.UBGE);

                        if (!resultadoDiferente && emojiReacao != null)
                        {
                            DiscordChannel ubgeBot_ = UBGE.GetChannel(Valores.ChatsUBGE.canalUBGEBot);

                            await mensagemEmoji.DeleteReactionAsync(emojiReacao, Program.ubgeBot.discordClient.CurrentUser);

                            DiscordMessage mensagemEmbed = await canalReaction.GetMessageAsync(resultadoReacts.LastOrDefault().idDaMensagem);

                            DiscordEmbed embedMensagem = mensagemEmbed.Embeds.LastOrDefault();

                            if (embedMensagem.Description.Contains(emojiReacao.ToString()))
                            {
                                DiscordEmbedBuilder novoEmbed = new DiscordEmbedBuilder(embedMensagem);
                                string descricaoEmbed = embedMensagem.Description;

                                List<string> lista = descricaoEmbed.Split('\n').ToList();

                                StringBuilder strEmbedFinal = new StringBuilder();

                                for (int linha = 0; linha < lista.Count; linha++)
                                {
                                    if (lista[linha].Contains(emojiReacao.ToString()))
                                        lista.RemoveAt(linha);

                                    strEmbedFinal.Append($"{lista[linha]}\n");
                                }

                                novoEmbed.Description = strEmbedFinal.ToString();
                                novoEmbed.WithAuthor(embedMensagem.Author.Name, null, Valores.logoUBGE);
                                novoEmbed.WithColor(new DiscordColor(0x32363c));

                                await mensagemEmbed.ModifyAsync(embed: novoEmbed.Build());

                                await jogos.DeleteOneAsync(filtroJogos);
                            }
                            else if (!embedMensagem.Description.Contains(emojiReacao.ToString()) && emojiReacao != null)
                            {
                                await ExcluiEAtualizaReactionDoEmoji(Program.ubgeBot, mensagemEmoji, emojiReacao, reacoesMembro, ultimoResultadoReacts, canalReaction, guildReaction, ubgeBot_);

                                return;
                            }

                            await ExcluiEAtualizaReactionDoEmoji(Program.ubgeBot, mensagemEmoji, emojiReacao, reacoesMembro, ultimoResultadoReacts, canalReaction, guildReaction, ubgeBot_);

                            return;
                        }

                        DiscordMember membro = guildReaction == UBGE ? await UBGE.GetMemberAsync(messageReactionAddEventArgs.User.Id) : await guildReaction.GetMemberAsync(messageReactionAddEventArgs.User.Id);

                        if (ultimoResultadoJogos.idDoCargo == Valores.Cargos.cargoAcessoGeral)
                        {
                            DiscordRole acessoGeral = UBGE.GetRole(Valores.Cargos.cargoAcessoGeral);

                            await membro.RevokeRoleAsync(acessoGeral);

                            Program.ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, $"O membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\" removeu o cargo de \"{acessoGeral.Name}\".");

                            await Program.ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.ReactRoleUBGE, "Cargo Removido!", $"{emojiReacao} | O membro: {Program.ubgeBot.utilidadesGerais.MencaoMembro(membro)} removeu o cargo de: {acessoGeral.Mention}.\n\nOu:\n- `@{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}`\n- `@{acessoGeral.Name}`", Program.ubgeBot.discordClient.CurrentUser.AvatarUrl, membro);

                            return;
                        }

                        DiscordRole cargo = guildReaction.GetRole(ultimoResultadoJogos.idDoCargo);

                        await membro.GrantRoleAsync(cargo);

                        Program.ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, $"O membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\" pegou o cargo de: \"{cargo.Name}\".");

                        if (guildReaction.Id == Valores.Guilds.UBGE)
                            await Program.ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.ReactRoleUBGE, "Cargo Adicionado!", $"{emojiReacao} | O membro: {Program.ubgeBot.utilidadesGerais.MencaoMembro(membro)} pegou o cargo de: {cargo.Mention}.\n\nOu:\n- `@{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}`\n- `@{cargo.Name}`", Program.ubgeBot.discordClient.CurrentUser.AvatarUrl, membro);
                        else
                            await Program.ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.ReactRoleForaDaUBGE, "Cargo Adicionado!", $"{emojiReacao} | O membro: **{Program.ubgeBot.utilidadesGerais.MencaoMembro(membro)}** pegou o cargo de: **{cargo.Name}**.", guildReaction.IconUrl, membro);
                    }
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                }
            }).Start();
        }

        private async Task ReacaoRemovidaReactRole(MessageReactionRemoveEventArgs messageReactionRemoveEventArgs)
        {
            if (messageReactionRemoveEventArgs == null || messageReactionRemoveEventArgs.Channel.IsPrivate || messageReactionRemoveEventArgs.User.IsBot)
                return;

            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    IMongoDatabase db = Program.ubgeBot.localDB;

                    IMongoCollection<Reacts> reacts = db.GetCollection<Reacts>(Valores.Mongo.reacts);
                    List<Reacts> resultadoReacts = await (await reacts.FindAsync(Builders<Reacts>.Filter.Eq(x => x.idDoCanal, messageReactionRemoveEventArgs.Channel.Id))).ToListAsync();

                    if (resultadoReacts.Count != 0)
                    {
                        IMongoCollection<Jogos> jogos = db.GetCollection<Jogos>(Valores.Mongo.jogos);

                        DiscordEmoji emojiReacao = messageReactionRemoveEventArgs.Emoji;

                        FilterDefinition<Jogos> filtroJogos = Builders<Jogos>.Filter.Eq(x => x.idDoEmoji, emojiReacao.Id);
                        List<Jogos> resultadoJogos = await (await jogos.FindAsync(filtroJogos)).ToListAsync();

                        if (resultadoJogos.Count == 0)
                            return;

                        DiscordGuild guildReaction = messageReactionRemoveEventArgs.Guild;
                        DiscordMessage mensagemEmoji = messageReactionRemoveEventArgs.Message;
                        DiscordChannel canalReaction = messageReactionRemoveEventArgs.Channel;

                        IReadOnlyList<DiscordUser> reacoesMembro = await mensagemEmoji.GetReactionsAsync(emojiReacao);

                        Jogos ultimoResultadoJogos = resultadoJogos.LastOrDefault();
                        Reacts ultimoResultadoReacts = resultadoReacts.LastOrDefault();

                        bool resultadoDiferente = true;

                        if (resultadoJogos.Count == 0 || (resultadoJogos.Count != 0 && guildReaction.GetRole(ultimoResultadoJogos.idDoCargo) == null))
                            resultadoDiferente = false;

                        DiscordGuild UBGE = await messageReactionRemoveEventArgs.Client.GetGuildAsync(Valores.Guilds.UBGE);

                        if (!resultadoDiferente && emojiReacao != null)
                        {
                            DiscordChannel ubgeBot_ = UBGE.GetChannel(Valores.ChatsUBGE.canalUBGEBot);

                            await mensagemEmoji.DeleteReactionAsync(emojiReacao, Program.ubgeBot.discordClient.CurrentUser);

                            DiscordMessage mensagemEmbed = await canalReaction.GetMessageAsync(resultadoReacts.LastOrDefault().idDaMensagem);

                            DiscordEmbed embedMensagem = mensagemEmbed.Embeds.LastOrDefault();

                            if (embedMensagem.Description.Contains(emojiReacao.ToString()))
                            {
                                DiscordEmbedBuilder novoEmbed = new DiscordEmbedBuilder(embedMensagem);
                                string descricaoEmbed = embedMensagem.Description;

                                List<string> lista = descricaoEmbed.Split('\n').ToList();

                                StringBuilder strEmbedFinal = new StringBuilder();

                                for (int linha = 0; linha < lista.Count; linha++)
                                {
                                    if (lista[linha].Contains(emojiReacao.ToString()))
                                        lista.RemoveAt(linha);

                                    strEmbedFinal.Append($"{lista[linha]}\n");
                                }

                                novoEmbed.Description = strEmbedFinal.ToString();
                                novoEmbed.WithAuthor(embedMensagem.Author.Name, null, Valores.logoUBGE);
                                novoEmbed.WithColor(new DiscordColor(0x32363c));

                                await mensagemEmbed.ModifyAsync(embed: novoEmbed.Build());

                                await jogos.DeleteOneAsync(filtroJogos);
                            }
                            else if (!embedMensagem.Description.Contains(emojiReacao.ToString()) && emojiReacao != null)
                            {
                                await ExcluiEAtualizaReactionDoEmoji(Program.ubgeBot, mensagemEmoji, emojiReacao, reacoesMembro, ultimoResultadoReacts, canalReaction, guildReaction, ubgeBot_);

                                return;
                            }

                            await ExcluiEAtualizaReactionDoEmoji(Program.ubgeBot, mensagemEmoji, emojiReacao, reacoesMembro, ultimoResultadoReacts, canalReaction, guildReaction, ubgeBot_);

                            return;
                        }

                        DiscordMember membro = guildReaction == UBGE ? await UBGE.GetMemberAsync(messageReactionRemoveEventArgs.User.Id) : await guildReaction.GetMemberAsync(messageReactionRemoveEventArgs.User.Id);

                        if (ultimoResultadoJogos.idDoCargo == Valores.Cargos.cargoAcessoGeral)
                        {
                            DiscordRole acessoGeral = UBGE.GetRole(Valores.Cargos.cargoAcessoGeral);

                            await membro.GrantRoleAsync(acessoGeral);

                            Program.ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, $"O membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\" pegou o cargo de \"{acessoGeral.Name}\".");

                            await Program.ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.ReactRoleUBGE, "Cargo Adicionado!", $"{emojiReacao} | O membro: {Program.ubgeBot.utilidadesGerais.MencaoMembro(membro)} pegou o cargo de: {acessoGeral.Mention}.\n\nOu:\n- `@{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}`\n- `@{acessoGeral.Name}`", Program.ubgeBot.discordClient.CurrentUser.AvatarUrl, membro);

                            return;
                        }

                        DiscordRole cargo = guildReaction.GetRole(ultimoResultadoJogos.idDoCargo);

                        await membro.RevokeRoleAsync(cargo);

                        Program.ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, $"O membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\" removeu o cargo de: \"{cargo.Name}\".");

                        if (guildReaction.Id == Valores.Guilds.UBGE)
                            await Program.ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.ReactRoleUBGE, "Cargo removido!", $"{emojiReacao} | O membro: {Program.ubgeBot.utilidadesGerais.MencaoMembro(membro)} removeu o cargo de: {cargo.Mention}.\n\nOu:\n- `@{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}`\n- `@{cargo.Name}`", Program.ubgeBot.discordClient.CurrentUser.AvatarUrl, membro);
                        else
                            await Program.ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.ReactRoleForaDaUBGE, "Cargo removido!", $"{emojiReacao} | O membro: **{Program.ubgeBot.utilidadesGerais.MencaoMembro(membro)}** removeu o cargo de: **{cargo.Name}**.", guildReaction.IconUrl, membro);
                    }
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                }
            }).Start();
        }

        private async Task ExcluiEAtualizaReactionDoEmoji(UBGEBot_ ubgeBotClient, DiscordMessage mensagemEmoji, DiscordEmoji emojiReacao, IReadOnlyList<DiscordUser> reacoesMembro, Reacts resultadoReacts, DiscordChannel canalReaction, DiscordGuild guildReaction, DiscordChannel ubgeBot)
        {
            try
            {
                await ubgeBotClient.utilidadesGerais.ExcluiReacoesDeUmaListaDeMembros(mensagemEmoji, emojiReacao, reacoesMembro);

                DiscordMessage atualizaMensagem = await canalReaction.GetMessageAsync(resultadoReacts.idDaMensagem);
                IReadOnlyList<DiscordUser> mensagemEmojiAtualizado = await atualizaMensagem.GetReactionsAsync(emojiReacao);

                if (mensagemEmojiAtualizado.Count() != 0)
                    await ubgeBot.SendMessageAsync($":warning:, existem **{mensagemEmojiAtualizado.Count()}** reações sobrando no emoji: {emojiReacao.ToString()} no canal: {canalReaction.Mention}, elas não foram removidas pois os membros saíram da(o): **{guildReaction.Name}**");

                return;
            }
            catch (Exception exception)
            {
                await ubgeBotClient.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
            }
        }
    }
}