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
        public void AplicarAoBot(DiscordClient discordClient, bool botConectadoAoMongo)
        {
            if (botConectadoAoMongo)
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
                    var db = Program.ubgeBot.localDB;

                    var jogos = db.GetCollection<Jogos>(Valores.Mongo.jogos);
                    var reacts = db.GetCollection<Reacts>(Valores.Mongo.reacts);

                    var emojiReacao = messageReactionAddEventArgs.Emoji;

                    var filtroJogos = Builders<Jogos>.Filter.Eq(x => x.idDoEmoji, emojiReacao.Id);
                    var resultadoJogos = await (await jogos.FindAsync(filtroJogos)).ToListAsync();

                    if (resultadoJogos.Count == 0)
                        return;

                    DiscordGuild guildReaction = messageReactionAddEventArgs.Guild;
                    var mensagemEmoji = messageReactionAddEventArgs.Message;
                    var canalReaction = messageReactionAddEventArgs.Channel;

                    var reacoesMembro = await mensagemEmoji.GetReactionsAsync(emojiReacao);

                    var dbContar = db.GetCollection<ContaMembrosQuePegaramCargos>(Valores.Mongo.contaMembrosQuePegaramCargos);

                    var resultadoReacts = await (await reacts.FindAsync(Builders<Reacts>.Filter.Eq(x => x.categoria, resultadoJogos.LastOrDefault().nomeDaCategoria))).ToListAsync();

                    var ultimoResultadoJogos = resultadoJogos.LastOrDefault();
                    var ultimoResultadoReacts = resultadoReacts.LastOrDefault();

                    bool resultadoDiferente = true;

                    if (resultadoJogos.Count == 0 || (resultadoJogos.Count != 0 && guildReaction.GetRole(ultimoResultadoJogos.idDoCargo) == null))
                        resultadoDiferente = false;

                    DiscordGuild UBGE = await messageReactionAddEventArgs.Client.GetGuildAsync(Valores.Guilds.UBGE);

                    if (!resultadoDiferente && emojiReacao != null)
                    {
                        DiscordChannel ubgeBot_ = UBGE.GetChannel(Valores.ChatsUBGE.canalUBGEBot);

                        await mensagemEmoji.DeleteReactionAsync(emojiReacao, Program.ubgeBot.discordClient.CurrentUser);

                        DiscordMessage mensagemEmbed = null;

                        if (await canalReaction.GetMessageAsync(resultadoReacts.LastOrDefault().idDaMensagem) == null)
                            return;

                        mensagemEmbed = await canalReaction.GetMessageAsync(resultadoReacts.LastOrDefault().idDaMensagem);

                        var embedMensagem = mensagemEmbed.Embeds.LastOrDefault();

                        if (embedMensagem.Description.Contains(emojiReacao.ToString()))
                        {
                            DiscordEmbedBuilder novoEmbed = new DiscordEmbedBuilder(embedMensagem);
                            var descricaoEmbed = embedMensagem.Description;

                            var lista = descricaoEmbed.Split('\n').ToList();

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

                    DiscordRole acessoGeral = UBGE.GetRole(Valores.Cargos.cargoAcessoGeral);

                    var membro = guildReaction == UBGE ? await UBGE.GetMemberAsync(messageReactionAddEventArgs.User.Id) : await guildReaction.GetMemberAsync(messageReactionAddEventArgs.User.Id);

                    if (ultimoResultadoJogos.idDoCargo == acessoGeral.Id)
                    {
                        await membro.RevokeRoleAsync(acessoGeral);

                        Program.ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, $"O membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\" removeu o cargo de \"{acessoGeral.Name}\".");

                        await Program.ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.ReactRoleUBGE, "Cargo Removido!", $"{emojiReacao} | O membro: {Program.ubgeBot.utilidadesGerais.MencaoMembro(membro)} removeu o cargo de: {acessoGeral.Mention}.\n\nOu:\n- `@{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}`\n- `@{acessoGeral.Name}`", Program.ubgeBot.discordClient.CurrentUser.AvatarUrl, membro);
                        
                        return;
                    }

                    DiscordRole cargo = guildReaction.GetRole(ultimoResultadoJogos.idDoCargo);

                    var filtroDBContar = Builders<ContaMembrosQuePegaramCargos>.Filter.Eq(x => x.jogo, cargo.Name);
                    var resultadosDBContar = await (await dbContar.FindAsync(filtroDBContar)).ToListAsync();

                    if (resultadosDBContar.Count == 0)
                        await dbContar.InsertOneAsync(new ContaMembrosQuePegaramCargos { jogo = cargo.Name, numeroDePessoas = 1, idsDosMembrosQuePegaramOCargo = new List<ulong> { membro.Id } });
                    else
                    {
                        var primeiroResultado = resultadosDBContar.FirstOrDefault();

                        if (!primeiroResultado.idsDosMembrosQuePegaramOCargo.Contains(membro.Id))
                        {
                            if (primeiroResultado.idsDosMembrosQuePegaramOCargo.Contains(1))
                                primeiroResultado.idsDosMembrosQuePegaramOCargo.Remove(1);

                            await dbContar.UpdateOneAsync(filtroDBContar, Builders<ContaMembrosQuePegaramCargos>.Update
                                .Set(x => x.numeroDePessoas, primeiroResultado.numeroDePessoas + 1)
                                .Set(y => y.idsDosMembrosQuePegaramOCargo, primeiroResultado.idsDosMembrosQuePegaramOCargo.Append(membro.Id)));
                        }
                    }

                    await membro.GrantRoleAsync(cargo);

                    Program.ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, $"O membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\" pegou o cargo de: \"{cargo.Name}\".");

                    if (guildReaction.Id == Valores.Guilds.UBGE)
                        await Program.ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.ReactRoleUBGE, "Cargo Adicionado!", $"{emojiReacao} | O membro: {Program.ubgeBot.utilidadesGerais.MencaoMembro(membro)} pegou o cargo de: {cargo.Mention}.\n\nOu:\n- `@{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}`\n- `@{cargo.Name}`", Program.ubgeBot.discordClient.CurrentUser.AvatarUrl, membro);
                    else
                        await Program.ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.ReactRoleForaDaUBGE, "Cargo Adicionado!", $"{emojiReacao} | O membro: **{Program.ubgeBot.utilidadesGerais.MencaoMembro(membro)}** pegou o cargo de: **{cargo.Name}**.", guildReaction.IconUrl, membro);
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
                    var db = Program.ubgeBot.localDB;

                    var dbContar = db.GetCollection<ContaMembrosQuePegaramCargos>(Valores.Mongo.contaMembrosQuePegaramCargos);

                    var jogos = db.GetCollection<Jogos>(Valores.Mongo.jogos);
                    var reacts = db.GetCollection<Reacts>(Valores.Mongo.reacts);

                    var emojiReacao = messageReactionRemoveEventArgs.Emoji;

                    var filtroJogos = Builders<Jogos>.Filter.Eq(x => x.idDoEmoji, emojiReacao.Id);
                    var resultadoJogos = await (await jogos.FindAsync(filtroJogos)).ToListAsync();

                    if (resultadoJogos.Count == 0)
                        return;

                    DiscordGuild guildReaction = messageReactionRemoveEventArgs.Guild;
                    var mensagemEmoji = messageReactionRemoveEventArgs.Message;
                    var canalReaction = messageReactionRemoveEventArgs.Channel;

                    var reacoesMembro = await mensagemEmoji.GetReactionsAsync(emojiReacao);

                    var resultadoReacts = await (await reacts.FindAsync(Builders<Reacts>.Filter.Eq(x => x.categoria, resultadoJogos.LastOrDefault().nomeDaCategoria))).ToListAsync();

                    var ultimoResultadoJogos = resultadoJogos.LastOrDefault();
                    var ultimoResultadoReacts = resultadoReacts.LastOrDefault();

                    bool resultadoDiferente = true;

                    if (resultadoJogos.Count == 0 || (resultadoJogos.Count != 0 && guildReaction.GetRole(ultimoResultadoJogos.idDoCargo) == null))
                        resultadoDiferente = false;

                    DiscordGuild UBGE = await messageReactionRemoveEventArgs.Client.GetGuildAsync(Valores.Guilds.UBGE);

                    if (!resultadoDiferente && emojiReacao != null)
                    {
                        DiscordChannel ubgeBot_ = UBGE.GetChannel(Valores.ChatsUBGE.canalUBGEBot);

                        await mensagemEmoji.DeleteReactionAsync(emojiReacao, Program.ubgeBot.discordClient.CurrentUser);

                        DiscordMessage mensagemEmbed = null;

                        if (await canalReaction.GetMessageAsync(resultadoReacts.LastOrDefault().idDaMensagem) == null)
                            return;

                        mensagemEmbed = await canalReaction.GetMessageAsync(resultadoReacts.LastOrDefault().idDaMensagem); 
                        
                        var embedMensagem = mensagemEmbed.Embeds.LastOrDefault();

                        if (embedMensagem.Description.Contains(emojiReacao.ToString()))
                        {
                            DiscordEmbedBuilder novoEmbed = new DiscordEmbedBuilder(embedMensagem);
                            var descricaoEmbed = embedMensagem.Description;

                            var lista = descricaoEmbed.Split('\n').ToList();

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

                    DiscordRole acessoGeral = UBGE.GetRole(Valores.Cargos.cargoAcessoGeral);

                    var membro = guildReaction == UBGE ? await UBGE.GetMemberAsync(messageReactionRemoveEventArgs.User.Id) : await guildReaction.GetMemberAsync(messageReactionRemoveEventArgs.User.Id);

                    if (ultimoResultadoJogos.idDoCargo == acessoGeral.Id)
                    {
                        await membro.GrantRoleAsync(acessoGeral);

                        Program.ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, $"O membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\" pegou o cargo de \"{acessoGeral.Name}\".");

                        await Program.ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.ReactRoleUBGE, "Cargo Adicionado!", $"{emojiReacao} | O membro: {Program.ubgeBot.utilidadesGerais.MencaoMembro(membro)} pegou o cargo de: {acessoGeral.Mention}.\n\nOu:\n- `@{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}`\n- `@{acessoGeral.Name}`", Program.ubgeBot.discordClient.CurrentUser.AvatarUrl, membro);

                        return;
                    }

                    DiscordRole cargo = guildReaction.GetRole(ultimoResultadoJogos.idDoCargo);

                    var filtroDBContar = Builders<ContaMembrosQuePegaramCargos>.Filter.Eq(x => x.jogo, cargo.Name);
                    var resultadosDBContar = await (await dbContar.FindAsync(filtroDBContar)).ToListAsync();

                    if (resultadosDBContar.Count == 0)
                        await dbContar.InsertOneAsync(new ContaMembrosQuePegaramCargos { jogo = cargo.Name, numeroDePessoas = 1, idsDosMembrosQuePegaramOCargo = new List<ulong> { membro.Id } });
                    else
                    {
                        var primeiroResultado = resultadosDBContar.FirstOrDefault();

                        if (!primeiroResultado.idsDosMembrosQuePegaramOCargo.Contains(membro.Id))
                        {
                            if (primeiroResultado.idsDosMembrosQuePegaramOCargo.Contains(1))
                                primeiroResultado.idsDosMembrosQuePegaramOCargo.Remove(1);

                            await dbContar.UpdateOneAsync(filtroDBContar, Builders<ContaMembrosQuePegaramCargos>.Update
                                .Set(x => x.numeroDePessoas, primeiroResultado.numeroDePessoas + 1)
                                .Set(y => y.idsDosMembrosQuePegaramOCargo, primeiroResultado.idsDosMembrosQuePegaramOCargo.Append(membro.Id)));
                        }
                    }

                    await membro.RevokeRoleAsync(cargo);

                    Program.ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, $"O membro: \"{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}\" removeu o cargo de: \"{cargo.Name}\".");

                    if (guildReaction.Id == Valores.Guilds.UBGE)
                        await Program.ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.ReactRoleUBGE, "Cargo removido!", $"{emojiReacao} | O membro: {Program.ubgeBot.utilidadesGerais.MencaoMembro(membro)} removeu o cargo de: {cargo.Mention}.\n\nOu:\n- `@{Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membro)}#{membro.Discriminator}`\n- `@{cargo.Name}`", Program.ubgeBot.discordClient.CurrentUser.AvatarUrl, membro);
                    else
                        await Program.ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.ReactRoleForaDaUBGE, "Cargo removido!", $"{emojiReacao} | O membro: **{Program.ubgeBot.utilidadesGerais.MencaoMembro(membro)}** removeu o cargo de: **{cargo.Name}**.", guildReaction.IconUrl, membro);
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

                var atualizaMensagem = await canalReaction.GetMessageAsync(resultadoReacts.idDaMensagem);
                var mensagemEmojiAtualizado = await atualizaMensagem.GetReactionsAsync(emojiReacao);

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