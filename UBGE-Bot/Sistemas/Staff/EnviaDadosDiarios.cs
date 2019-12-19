using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using MongoDB.Driver;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UBGE_Bot.Carregamento;
using UBGE_Bot.Main;
using UBGE_Bot.MongoDB.Modelos;
using UBGE_Bot.LogExceptions;
using UBGE_Bot.Utilidades;


namespace UBGE_Bot.Sistemas.Staff
{
    public sealed class EnviaDadosDiarios : IAplicavelAoCliente
    {
        public void AplicarAoBot(DiscordClient discordClient, bool botConectadoAoMongo)
        {
            if (botConectadoAoMongo)
                EnviaDadosDiariosTask(Program.ubgeBot).GetAwaiter().GetResult();
        }

        private async Task EnviaDadosDiariosTask(UBGEBot_ ubgeBotClient)
        {
            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    while (true)
                    {
                        if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 55 && DateTime.Now.Second == 00)
                            await ExecutaList(ubgeBotClient, "Executando o **//list** para a execução do método para enviar os dados díários...");

                        if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 55)
                        {
                            var db = ubgeBotClient.localDB;
                            var dbMembros = db.GetCollection<ContaMembrosQuePegaramCargos>(Valores.Mongo.contaMembrosQuePegaramCargos);
                            var dbMembrosRegistrados = db.GetCollection<MembrosQuePegaramOCargoDeMembroRegistrado>(Valores.Mongo.membrosQuePegaramOCargoDeMembroRegistrado);

                            var filtroDBMembros = Builders<ContaMembrosQuePegaramCargos>.Filter.Empty;
                            var filtroDBMembrosRegistrados = Builders<MembrosQuePegaramOCargoDeMembroRegistrado>.Filter.Empty;

                            var resultadosDBMembros = await (await dbMembros.FindAsync(filtroDBMembros)).ToListAsync();
                            var resultadosDBMembrosRegistrados = await (await dbMembrosRegistrados.FindAsync(filtroDBMembrosRegistrados)).ToListAsync();

                            DiscordGuild UBGE = await ubgeBotClient.discordClient.GetGuildAsync(Valores.Guilds.UBGE);
                            DiscordChannel BotUBGE = UBGE.GetChannel(Valores.ChatsUBGE.canalUBGEBot);

                            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                            if (resultadosDBMembros.Count == 0 && resultadosDBMembrosRegistrados.Count == 0)
                            {
                                embed.WithAuthor("Não tenho dados para apresentar :/", null, Valores.logoUBGE)
                                    .WithColor(ubgeBotClient.utilidadesGerais.CorAleatoriaEmbed())
                                    .WithDescription(":pensive:")
                                    .WithFooter($"Nada há declarar. Às: {DateTime.Now.ToString()}")
                                    .WithThumbnailUrl((await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ubgeBotClient.discordClient, "gatu")).Url);

                                await BotUBGE.SendMessageAsync(embed: embed.Build());
                            }
                            else
                            {
                                string caminhoTxt = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + $@"\DadosReactRole.txt";

                                using (StreamWriter streamWriter = new StreamWriter(caminhoTxt, false, Encoding.UTF8))
                                {
                                    if (resultadosDBMembros.Count != 0)
                                    {
                                        foreach (var Dados in resultadosDBMembros)
                                        {
                                            streamWriter.WriteLine($"Jogo: \"{Dados.jogo}\"");
                                            streamWriter.WriteLine($"Número de pessoas que pegaram o cargo: \"{(Dados.numeroDePessoas == 1 ? $"{Dados.numeroDePessoas} membro." : $"{Dados.numeroDePessoas} membros.")}\".");
                                            streamWriter.WriteLine(string.Empty);
                                        }
                                    }
                                    else
                                    {
                                        streamWriter.WriteLine("Um total de 0 pessoas pegaram algum cargo de jogo.");
                                        streamWriter.WriteLine(string.Empty);
                                        streamWriter.WriteLine(string.Empty);
                                    }

                                    if (resultadosDBMembrosRegistrados.Count != 0)
                                    {
                                        foreach (var Dados_ in resultadosDBMembrosRegistrados)
                                        {
                                            streamWriter.WriteLine($"Número de pessoas que pegaram o cargo de membro registrado: \"{Dados_.numeroPessoasQuePegaramOCargoDeMembroRegistrado}\".");
                                            streamWriter.WriteLine(string.Empty);
                                        }
                                    }
                                    else
                                    {
                                        streamWriter.WriteLine("Um total de 0 pessoas pegaram o cargo de Membro Registrado.");
                                        streamWriter.WriteLine(string.Empty);
                                        streamWriter.WriteLine(string.Empty);
                                    }

                                    streamWriter.WriteLine($"Esses dados do React Role e de Membros Registrados são do dia e hora: \"{DateTime.Now.ToString()}\".");
                                }

                                embed.WithAuthor("Dados do React Role:", null, Valores.logoUBGE)
                                    .WithColor(ubgeBotClient.utilidadesGerais.CorAleatoriaEmbed())
                                    .WithDescription($":smile: {await ubgeBotClient.utilidadesGerais.ProcuraEmoji(ubgeBotClient.discordClient, "ubge")}")
                                    .WithThumbnailUrl((await ubgeBotClient.utilidadesGerais.ProcuraEmoji(ubgeBotClient.discordClient, "uhu")).Url)
                                    .WithFooter($"Esses dados são enviados automaticamente pelo bot para fins de estatísticas da UBGE.");

                                await BotUBGE.SendMessageAsync(embed: embed.Build());
                                await BotUBGE.SendFileAsync(caminhoTxt);

                                await dbMembros.DeleteManyAsync(filtroDBMembros);
                                await dbMembrosRegistrados.DeleteManyAsync(filtroDBMembrosRegistrados);

                                File.Delete(caminhoTxt);
                            }

                            await Task.Delay(1000);
                        }
                    }
                }
                catch (Exception exception)
                {
                    await ubgeBotClient.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Sistemas, exception);
                }
            }).Start();
        }

        private async Task ExecutaList(UBGEBot_ ubgeBotClient, string mensagemDoAviso)
        {
            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    DiscordGuild UBGE = await ubgeBotClient.discordClient.GetGuildAsync(Valores.Guilds.UBGE);
                    DiscordChannel ubgeBot = UBGE.GetChannel(Valores.ChatsUBGE.canalUBGEBot);
                    DiscordMember ubgeBotMembro = await UBGE.GetMemberAsync(Valores.Guilds.Membros.ubgeBot);

                    DiscordMessage mensagemAviso = await ubgeBot.SendMessageAsync($":warning: | {mensagemDoAviso} Dia e Hora: `{DateTime.Now.ToString()}`.");

                    var procuraComando = ubgeBotClient.discordClient.GetCommandsNext().FindCommand("staff list", out var Args);
                    var comandoList = ubgeBotClient.discordClient.GetCommandsNext().CreateFakeContext(ubgeBotMembro, ubgeBot, "", "//", procuraComando, Args);

                    await ubgeBotClient.discordClient.GetCommandsNext().ExecuteCommandAsync(comandoList);

                    await mensagemAviso.DeleteAsync();
                }
                catch (Exception exception)
                {
                    await ubgeBotClient.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Sistemas, exception);
                }
            }).Start();
        }
    }
}