using DSharpPlus;
using DSharpPlus.Entities;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Text;
using Timer = System.Timers.Timer;
using System.Threading;
using System.Threading.Tasks;
using UBGE_Bot.Carregamento;
using UBGE_Bot.Main;
using UBGE_Bot.MongoDB.Modelos;
using UBGE_Bot.LogExceptions;
using UBGE_Bot.Utilidades;

namespace UBGE_Bot.Sistemas.Bot
{
    public sealed class VerificaPrisoesQuandoOBotInicia : IAplicavelAoCliente
    {
        public void AplicarAoBot(DiscordClient discordClient, bool botConectadoAoMongo)
        {
            Timer checaMembrosNaPrisao = new Timer()
            {
                Interval = 30000,
            };
            checaMembrosNaPrisao.Elapsed += async delegate
            {
                if (Program.checkDosCanaisFoiIniciado)
                    await VerificaPrisoesQuandoOBotIniciaTask(Program.ubgeBot);
            };
            checaMembrosNaPrisao.Start();
        }

        private static async Task VerificaPrisoesQuandoOBotIniciaTask(UBGEBot_ ubgeBotClient)
        {
            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    var db = ubgeBotClient.localDB;
                    var collectionInfracoes = db.GetCollection<Infracao>(Valores.Mongo.infracoes);

                    DiscordGuild UBGE = await ubgeBotClient.discordClient.GetGuildAsync(Valores.Guilds.UBGE);

                    DiscordRole prisioneiroCargo = UBGE.GetRole(Valores.Cargos.cargoPrisioneiro), cargosMembroForeach = null;

                    DiscordChannel ubgeBot = UBGE.GetChannel(Valores.ChatsUBGE.canalUBGEBot);

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    StringBuilder strCargos = new StringBuilder();

                    foreach (var membroPrisao in UBGE.Members.Values.Where(x => x.Roles.Contains(prisioneiroCargo)))
                    {
                        var filtro = Builders<Infracao>.Filter.Eq(x => x.idInfrator, membroPrisao.Id);
                        var listaInfracoes = await (await collectionInfracoes.FindAsync(filtro)).ToListAsync();

                        if (listaInfracoes.Count == 0 || !listaInfracoes.LastOrDefault().oMembroFoiPreso)
                            continue;

                        var ultimaPrisao = listaInfracoes.LastOrDefault();

                        var diaHoraInfracao = Convert.ToDateTime(ultimaPrisao.dataInfracao.ToString());
                        var tempoPrisao = ubgeBotClient.utilidadesGerais.ConverterTempo(ultimaPrisao.dadosPrisao.tempoDoMembroNaPrisao);

                        if (diaHoraInfracao.Add(tempoPrisao) < DateTime.Now)
                        {
                            await membroPrisao.RevokeRoleAsync(prisioneiroCargo);

                            foreach (var cargos in ultimaPrisao.dadosPrisao.cargosDoMembro)
                            {
                                await Task.Delay(200);

                                cargosMembroForeach = UBGE.GetRole(cargos);

                                await membroPrisao.GrantRoleAsync(cargosMembroForeach);

                                strCargos.Append($"{cargosMembroForeach.Mention} | ");
                            }

                            embed.WithAuthor($"O membro: \"{ubgeBotClient.utilidadesGerais.RetornaNomeDiscord(membroPrisao)}#{membroPrisao.Discriminator}\", saiu da prisão.", null, Valores.logoUBGE)
                                .WithColor(ubgeBotClient.utilidadesGerais.CorAleatoriaEmbed())
                                .WithDescription($"Cargos devolvidos: {strCargos.ToString()}")
                                .WithThumbnailUrl(membroPrisao.AvatarUrl)
                                .WithTimestamp(DateTime.Now)
                                .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(membroPrisao)}", iconUrl: membroPrisao.AvatarUrl);

                            ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Discord, $"O membro: {ubgeBotClient.utilidadesGerais.RetornaNomeDiscord(membroPrisao)}#{membroPrisao.Discriminator}, saiu da prisão.");
                            await ubgeBot.SendMessageAsync(embed: embed.Build());
                        }
                    }
                }
                catch (Exception exception)
                {
                    await ubgeBotClient.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Sistemas, exception);
                }
            }).Start();
        }
    }
}