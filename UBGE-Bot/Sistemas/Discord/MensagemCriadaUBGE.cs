using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UBGE_Bot.Main;
using UBGE_Bot.MongoDB.Modelos;
using UBGE_Bot.LogExceptions;
using UBGE_Bot.Utilidades;

namespace UBGE_Bot.Sistemas.Discord
{
    public sealed class MensagemCriadaUBGE : IAplicavelAoCliente
    {
        public void AplicarAoBot(DiscordClient discordClient, bool botConectadoAoMongo)
        {
            if (botConectadoAoMongo)
                discordClient.MessageCreated += MensagemCriada;
        }

        private async Task MensagemCriada(MessageCreateEventArgs messageCreateEventArgs)
        {
            if (messageCreateEventArgs == null)
                return;

            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    var db = Program.ubgeBot.localDB;

                    DiscordClient clientDiscord = messageCreateEventArgs.Client;
                    DiscordChannel canalMensagem = messageCreateEventArgs.Channel;
                    DiscordUser donoMensagem = messageCreateEventArgs.Author;
                    DiscordMessage mensagem = messageCreateEventArgs.Message;

                    DiscordGuild UBGE = await clientDiscord.GetGuildAsync(Valores.Guilds.UBGE);

                    DiscordMember donoMensagem_ = null;

                    if (UBGE.Members.Keys.Contains(donoMensagem.Id))
                        donoMensagem_ = await UBGE.GetMemberAsync(donoMensagem.Id);

                    var collectionModMail = db.GetCollection<ModMail>(Valores.Mongo.modMail);

                    if (!canalMensagem.IsPrivate)
                    {
                        if (canalMensagem.Id == Valores.ChatsUBGE.canalFormularioAlerta)
                        {
                            if (donoMensagem.IsBot)
                            {
                                await mensagem.CreateReactionAsync(DiscordEmoji.FromName(clientDiscord, ":white_check_mark:"));
                                await Task.Delay(200);
                                await mensagem.CreateReactionAsync(DiscordEmoji.FromName(clientDiscord, ":negative_squared_cross_mark:"));
                            }
                        }

                        if (canalMensagem.Id == Valores.ChatsUBGE.canalRecomendacoesPromocoes)
                        {
                            if (mensagem.Content.Contains("http") || mensagem.Content.Contains("https"))
                            {
                                await mensagem.CreateReactionAsync(DiscordEmoji.FromName(clientDiscord, ":thumbsup:"));
                                await Task.Delay(200);
                                await mensagem.CreateReactionAsync(DiscordEmoji.FromName(clientDiscord, ":thumbsdown:"));
                            }
                        }

                        var resultadoModMailIdDoCanal = await (await collectionModMail.FindAsync(Builders<ModMail>.Filter.Eq(x => x.idDoCanal, canalMensagem.Id))).ToListAsync();

                        if (resultadoModMailIdDoCanal.Count != 0 && !donoMensagem.IsBot && (mensagem.Content.ToLower().StartsWith("//responder") || mensagem.Content.ToLower().StartsWith("ubge!responder") || mensagem.Content.ToLower().StartsWith("//r") || mensagem.Content.ToLower().StartsWith("ubge!r")))
                        {
                            var ultimoResultadoModMailIdDoCanal = resultadoModMailIdDoCanal.LastOrDefault();

                            bool canalFechadoIdDoCanal = true;

                            if (ultimoResultadoModMailIdDoCanal.denuncia == null)
                                canalFechadoIdDoCanal = ultimoResultadoModMailIdDoCanal.contato.oCanalFoiFechado;
                            else if (ultimoResultadoModMailIdDoCanal.contato == null)
                                canalFechadoIdDoCanal = ultimoResultadoModMailIdDoCanal.denuncia.oCanalFoiFechado;

                            if (!canalFechadoIdDoCanal && canalMensagem.Id == ultimoResultadoModMailIdDoCanal.idDoCanal)
                            {
                                DiscordDmChannel pvMembro = await (await UBGE.GetMemberAsync(resultadoModMailIdDoCanal.LastOrDefault().idDoMembro)).CreateDmChannelAsync();

                                var nomeMembroNoDiscordModMail = Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(donoMensagem_);

                                var mensagemAnexadas = mensagem.Attachments;

                                if (mensagemAnexadas.Count != 0)
                                {
                                    foreach (var arquivos in mensagemAnexadas)
                                    {
                                        if (string.IsNullOrWhiteSpace(mensagem.Content))
                                        {
                                            await pvMembro.SendMessageAsync($"**{nomeMembroNoDiscordModMail}#{donoMensagem_.Discriminator}** - **[{donoMensagem_.Roles.OrderByDescending(x => x.Position).FirstOrDefault().Name}]** → {arquivos.Url}");

                                            continue;
                                        }

                                        await pvMembro.SendMessageAsync($"**{nomeMembroNoDiscordModMail}#{donoMensagem_.Discriminator}** - **[{donoMensagem_.Roles.OrderByDescending(x => x.Position).FirstOrDefault().Name}]** → {mensagem.Content.Replace("//r ", "").Replace("ubge!r ", "").Replace("//responder ", "").Replace("ubge!responder ", "")}\n\n*Mensagem anexada ao link.*");
                                        await pvMembro.SendMessageAsync(arquivos.Url);
                                    }

                                    return;
                                }

                                await pvMembro.SendMessageAsync($"**{nomeMembroNoDiscordModMail}#{donoMensagem_.Discriminator}** - **[{donoMensagem_.Roles.OrderByDescending(x => x.Position).FirstOrDefault().Name}]** → {mensagem.Content.Replace("//r ", "").Replace("ubge!r ", "").Replace("//responder ", "").Replace("ubge!responder ", "")}");
                            }
                        }

                        if (canalMensagem.Id != Valores.ChatsUBGE.canalTesteDoBot || canalMensagem.Id != Valores.ChatsUBGE.canalPRServidor ||
                            canalMensagem.Id != Valores.ChatsUBGE.canalComandosBot || canalMensagem.Id != Valores.ChatsUBGE.canalCrieSuaSalaAqui ||
                            canalMensagem.Id != Valores.ChatsUBGE.canalUBGEBot || !donoMensagem.IsBot)
                        {
                            var colecao = db.GetCollection<Levels>(Valores.Mongo.levels);
                            var filtro = Builders<Levels>.Filter.Eq(x => x.idDoMembro, messageCreateEventArgs.Author.Id);

                            var lista = await (await colecao.FindAsync(filtro)).ToListAsync();

                            var xpAleatorio = ulong.Parse(new Random().Next(1, 20).ToString());

                            ulong numeroLevel = 1;

                            if (lista.Count == 0)
                                await colecao.InsertOneAsync(new Levels { idDoMembro = messageCreateEventArgs.Author.Id, xpDoMembro = 1, nomeDoLevel = $"{numeroLevel}", diaEHora = DateTime.Now.ToString() });
                            else
                            {
                                ulong xpFinal = 0;
                                ulong xpForeach = 0;

                                foreach (var Level in lista)
                                {
                                    if ((DateTime.Now - Convert.ToDateTime(Level.diaEHora)).TotalMinutes < 1)
                                        return;

                                    xpFinal = Level.xpDoMembro;
                                    xpForeach = Level.xpDoMembro;
                                    numeroLevel = ulong.Parse(Level.nomeDoLevel);
                                }

                                xpFinal = xpAleatorio + xpFinal;

                                if (xpFinal >= 2800 * numeroLevel)
                                    numeroLevel++;

                                await colecao.UpdateOneAsync(filtro, Builders<Levels>.Update.Set(x => x.xpDoMembro, xpFinal)
                                .Set(x => x.nomeDoLevel, $"{numeroLevel}")
                                .Set(x => x.diaEHora, DateTime.Now.ToString()));
                            }
                        }

                        return;
                    }

                    if (donoMensagem.IsBot || !UBGE.Members.Keys.Contains(donoMensagem.Id))
                        return;

                    var resultadoModMail = await (await collectionModMail.FindAsync(Builders<ModMail>.Filter.Eq(x => x.idDoMembro, donoMensagem.Id))).ToListAsync();
                    var ultimoResultadoModMail = resultadoModMail.LastOrDefault();

                    var nomeMembroNoDiscord = Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(donoMensagem_);

                    DiscordRole cargoModeradorDiscord = UBGE.GetRole(Valores.Cargos.cargoModeradorDiscord);
                    DiscordRole cargoComiteComunitario = UBGE.GetRole(Valores.Cargos.cargoComiteComunitario);
                    DiscordRole cargoConselheiro = UBGE.GetRole(Valores.Cargos.cargoConselheiro);

                    DiscordChannel votacoesConselho = UBGE.GetChannel(Valores.ChatsUBGE.canalVotacoesConselho);

                    var modMailUBGE = UBGE.GetChannel(Valores.ChatsUBGE.Categorias.categoriaModMailBot);

                    if (mensagem.Content.ToLower() != "modmail")
                    {
                        if (resultadoModMail.Count != 0)
                        {
                            bool canalFechado = true;

                            if (ultimoResultadoModMail.denuncia == null)
                                canalFechado = ultimoResultadoModMail.contato.oCanalFoiFechado;
                            else if (ultimoResultadoModMail.contato == null)
                                canalFechado = ultimoResultadoModMail.denuncia.oCanalFoiFechado;

                            if (!canalFechado)
                            {
                                var canalModMail = UBGE.GetChannel(ultimoResultadoModMail.idDoCanal);

                                var mensagensAnexadas = mensagem.Attachments;

                                if (mensagensAnexadas.Count != 0)
                                {
                                    foreach (var arquivos in mensagensAnexadas)
                                    {
                                        await canalModMail.SendMessageAsync($"**{nomeMembroNoDiscord}#{donoMensagem_.Discriminator}** → {(string.IsNullOrWhiteSpace(mensagem.Content) ? string.Empty : $"{mensagem.Content}\n\n*Mensagem anexada com o link.*")}");
                                        await canalModMail.SendMessageAsync(arquivos.Url);
                                    }

                                    return;
                                }

                                await canalModMail.SendMessageAsync($"**{nomeMembroNoDiscord}#{donoMensagem_.Discriminator}** → {mensagem.Content}");

                                return;
                            }
                            else
                                return;
                        }
                        else
                            return;
                    }

                    new Thread(async () =>
                    {
                        try
                        {
                            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                            InteractivityExtension interactivity = clientDiscord.GetInteractivity();

                            DiscordEmoji emojiDenuncia = DiscordEmoji.FromName(clientDiscord, ":oncoming_police_car:");
                            DiscordEmoji emojiSugestao = DiscordEmoji.FromName(clientDiscord, ":star:");
                            DiscordEmoji emojiContatoStaff = await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(clientDiscord, ":LOGO_UBGE_2:");

                            embed.WithAuthor("O que você deseja fazer?", null, Valores.logoUBGE)
                                .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                                .WithDescription($"{emojiDenuncia} - Denúncia\n" +
                                $"{emojiSugestao} - Sugestão\n" +
                                $"{emojiContatoStaff} - Contato")
                                .WithTimestamp(DateTime.Now)
                                .WithThumbnailUrl(donoMensagem.AvatarUrl);

                            DiscordMessage msgEscolhaMembro = await canalMensagem.SendMessageAsync(embed: embed.Build());
                            await msgEscolhaMembro.CreateReactionAsync(emojiDenuncia);
                            await Task.Delay(200);
                            await msgEscolhaMembro.CreateReactionAsync(emojiSugestao);
                            await Task.Delay(200);
                            await msgEscolhaMembro.CreateReactionAsync(emojiContatoStaff);

                            var emojiResposta = (await interactivity.WaitForReactionAsync(msgEscolhaMembro, donoMensagem, TimeSpan.FromMinutes(1))).Result.Emoji;

                            ModMail modMail = new ModMail();
                            modMail.idDoMembro = donoMensagem_.Id;

                            var nickMembroNoCanal = nomeMembroNoDiscord.Replace("[", "").Replace("]", "").Replace("'", "").Replace("\"", "").Replace("!", "").Replace("_", "").Replace("-", "").Replace("=", "").Replace("<", "").Replace("<", "").Replace(".", "").Replace(",", "").Replace("`", "").Replace("´", "").Replace("+", "").Replace("/", "").Replace("\\", "").Replace(":", "").Replace(";", "").Replace("{", "").Replace("}", "").Replace("ª", "").Replace("º", "").Replace(" ", "");

                            if (emojiResposta == emojiDenuncia)
                            {
                                Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                                embed.WithAuthor("Qual motivo de sua denúncia?", null, Valores.logoUBGE)
                                    .WithDescription("Digite ela abaixo para entendermos melhor e você entrará em contato direto com a staff.")
                                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                                DiscordMessage perguntaMotivoDenuncia = await canalMensagem.SendMessageAsync(embed: embed.Build());
                                var esperaRespostaMotivoDenuncia = await Program.ubgeBot.utilidadesGerais.PegaRespostaPrivado(interactivity, donoMensagem_, canalMensagem);

                                Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                                embed.WithAuthor("Qual a sua denúncia?", null, Valores.logoUBGE)
                                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                                DiscordMessage perguntaDenuncia = await canalMensagem.SendMessageAsync(embed: embed.Build());
                                var esperaRespostaDenuncia = await Program.ubgeBot.utilidadesGerais.PegaRespostaPrivado(interactivity, donoMensagem_, canalMensagem);

                                var diaHoraDenunciaDoMembro = DateTime.Now.ToString();

                                var canalMembroDaDenuncia = await UBGE.CreateTextChannelAsync($"{nickMembroNoCanal}-{donoMensagem_.Discriminator}", modMailUBGE);

                                await canalMembroDaDenuncia.AddOverwriteAsync(cargoComiteComunitario, Permissions.None, Permissions.AccessChannels | Permissions.SendMessages);
                                await canalMembroDaDenuncia.AddOverwriteAsync(cargoConselheiro, Permissions.None, Permissions.AccessChannels | Permissions.SendMessages);

                                modMail.idDoCanal = canalMembroDaDenuncia.Id;
                                modMail.denuncia = new Denuncia
                                {
                                    denunciaDoMembro = esperaRespostaDenuncia.Content,
                                    motivoDaDenunciaDoMembro = esperaRespostaMotivoDenuncia.Content,
                                    diaHoraDenuncia = diaHoraDenunciaDoMembro,
                                    oCanalFoiFechado = false,
                                };

                                await collectionModMail.InsertOneAsync(modMail);

                                Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                                embed.WithAuthor($"Denúncia do membro: \"{nomeMembroNoDiscord}#{donoMensagem_.Discriminator}\"", null, Valores.logoUBGE)
                                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                                    .WithDescription($"Quando foi feita esta denúncia: **{diaHoraDenunciaDoMembro}**\n\n" +
                                    $"Motivo da denúncia: **{modMail.denuncia.motivoDaDenunciaDoMembro}**\n" +
                                    $"Denúncia: **{modMail.denuncia.denunciaDoMembro}**")
                                    .WithThumbnailUrl(donoMensagem_.AvatarUrl)
                                    .WithTimestamp(DateTime.Now);

                                await canalMembroDaDenuncia.SendMessageAsync(embed: embed.Build(), content: cargoModeradorDiscord.Mention);

                                Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                                embed.WithAuthor("Sua denúncia foi enviada a staff da UBGE!", null, Valores.logoUBGE)
                                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                                    .WithDescription("A staff irá ler e provalmente irá fazer diversas perguntas, e qualquer mensagem enviada por eles eu enviarei para você, fique atento a seu privado e responda normalmente! :wink:")
                                    .WithThumbnailUrl(donoMensagem_.AvatarUrl)
                                    .WithTimestamp(DateTime.Now);

                                await canalMensagem.SendMessageAsync(embed: embed.Build());
                            }
                            else if (emojiResposta == emojiSugestao)
                            {
                                Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                                embed.WithAuthor("Digite aqui sua sugestão para a UBGE!", null, Valores.logoUBGE)
                                    .WithDescription("Digite aqui sua sugestão, logo após ela irá entrar em votação no conselho comunitário!\n\n**ATENÇÃO! DIGITE UMA ÚNICA MENSAGEM, NÃO ENVIE A MENSAGEM PELA METADE!**")
                                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                                DiscordMessage perguntaSugestao = await canalMensagem.SendMessageAsync(embed: embed.Build());
                                var esperaSugestao = await Program.ubgeBot.utilidadesGerais.PegaRespostaPrivado(interactivity, donoMensagem_, canalMensagem);

                                var diaHoraSugestaoDoMembro = DateTime.Now.ToString();

                                modMail.sugestao = new Sugestao
                                {
                                    diaHoraSugestao = diaHoraSugestaoDoMembro,
                                    sugestaoDoMembro = esperaSugestao.Content,
                                };

                                await collectionModMail.InsertOneAsync(modMail);

                                Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                                embed.WithAuthor($"Sugestão do membro: \"{nomeMembroNoDiscord}#{donoMensagem_.Discriminator}\"", null, Valores.logoUBGE)
                                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                                    .WithDescription(esperaSugestao.Content)
                                    .WithThumbnailUrl(donoMensagem_.AvatarUrl)
                                    .WithTimestamp(DateTime.Now);

                                var mensagemSugestao = await votacoesConselho.SendMessageAsync(embed: embed.Build(), content: cargoConselheiro.Mention);
                                await mensagemSugestao.CreateReactionAsync(DiscordEmoji.FromName(clientDiscord, ":white_check_mark:"));
                                await Task.Delay(200);
                                await mensagemSugestao.CreateReactionAsync(DiscordEmoji.FromName(clientDiscord, ":negative_squared_cross_mark:"));

                                Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                                embed.WithAuthor("Sua sugestão foi enviada para a staff da UBGE!", null, Valores.logoUBGE)
                                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                                    .WithDescription($"Obrigado por fazer um servidor agradável a todos os membros! {await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(clientDiscord, ":UBGE:")}")
                                    .WithThumbnailUrl(donoMensagem_.AvatarUrl)
                                    .WithTimestamp(DateTime.Now);

                                await canalMensagem.SendMessageAsync(embed: embed.Build());
                            }
                            else if (emojiResposta == emojiContatoStaff)
                            {
                                Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                                embed.WithAuthor("Digite aqui seu motivo para entrar em contato com a staff da UBGE!", null, Valores.logoUBGE)
                                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                                DiscordMessage perguntaMotivoContato = await canalMensagem.SendMessageAsync(embed: embed.Build());
                                var esperaMotivoContato = await Program.ubgeBot.utilidadesGerais.PegaRespostaPrivado(interactivity, donoMensagem_, canalMensagem);

                                var diaHoraContatoDoMembro = DateTime.Now.ToString();

                                var canalMembroContato = await UBGE.CreateTextChannelAsync($"{nickMembroNoCanal}-{donoMensagem_.Discriminator}", modMailUBGE);

                                await canalMembroContato.AddOverwriteAsync(cargoModeradorDiscord, Permissions.None, Permissions.AccessChannels | Permissions.SendMessages);
                                await canalMembroContato.AddOverwriteAsync(cargoConselheiro, Permissions.None, Permissions.AccessChannels | Permissions.SendMessages);

                                modMail.idDoCanal = canalMembroContato.Id;
                                modMail.contato = new Contato
                                {
                                    diaHoraContato = diaHoraContatoDoMembro,
                                    motivoDoContatoDoMembro = esperaMotivoContato.Content,
                                    oCanalFoiFechado = false,
                                };

                                await collectionModMail.InsertOneAsync(modMail);

                                Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                                embed.WithAuthor($"O membro: \"{nomeMembroNoDiscord}#{donoMensagem_.Discriminator}\" quer falar com a staff", null, Valores.logoUBGE)
                                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                                    .WithDescription($"Quando foi feito este contato com a staff: **{diaHoraContatoDoMembro}**\n\n" +
                                    $"Motivo: **{esperaMotivoContato.Content}**")
                                    .WithThumbnailUrl(donoMensagem_.AvatarUrl)
                                    .WithTimestamp(DateTime.Now);

                                await canalMembroContato.SendMessageAsync(embed: embed.Build(), content: cargoComiteComunitario.Mention);

                                Program.ubgeBot.utilidadesGerais.LimpaEmbed(embed);

                                embed.WithAuthor("Seu pedido de contato foi enviado para a staff da UBGE!", null, Valores.logoUBGE)
                                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                                    .WithDescription("A staff irá ler e provalmente irá fazer diversas perguntas, e qualquer mensagem enviada por eles eu enviarei para você, fique atento a seu privado e responda normalmente! :wink:")
                                    .WithThumbnailUrl(donoMensagem_.AvatarUrl)
                                    .WithTimestamp(DateTime.Now);

                                await canalMensagem.SendMessageAsync(embed: embed.Build());
                            }
                            else
                                return;

                            return;
                        }
                        catch (NullReferenceException) { }
                        catch (Exception exception)
                        {
                            await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                        }
                    }).Start();
                }
                catch (NullReferenceException) { }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Discord, exception);
                }
            }).Start();
        }
    }
}