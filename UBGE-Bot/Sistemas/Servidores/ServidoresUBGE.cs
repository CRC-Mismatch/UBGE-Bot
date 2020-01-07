using DSharpPlus;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Timer = System.Timers.Timer;
using System.Threading;
using System.Threading.Tasks;
using UBGE_Bot.Carregamento;
using UBGE_Bot.LogExceptions;
using UBGE_Bot.Main;
using ServidoresUBGE_ = UBGE_Bot.MongoDB.Modelos.ServidoresUBGE;
using UBGE_Bot.Utilidades;

namespace UBGE_Bot.Sistemas.Servidores
{
    public sealed class ServidoresUBGE : IAplicavelAoCliente
    {
        public void AplicarAoBot(DiscordClient discordClient, bool botConectadoAoMongo, bool sistemaAtivo)
        {
            Timer timerServidores = new Timer()
            {
                Interval = 15000,
            };
            timerServidores.Elapsed += async delegate
            {
                if (botConectadoAoMongo && sistemaAtivo)
                {
                    await BuscaServidoresPR(Program.ubgeBot, Program.httpClient);
                    await BuscaServidoresConanExiles(Program.ubgeBot, Program.httpClient);
                    await BuscaServidoresCounterStrike(Program.ubgeBot, Program.httpClient);
                    await BuscaServidoresDayZ(Program.ubgeBot, Program.httpClient);
                    await BuscaServidoresMordhau(Program.ubgeBot, Program.httpClient);
                    await BuscaServidoresOpenSpades(Program.ubgeBot, Program.httpClient);
                    await BuscaServidoresUnturned(Program.ubgeBot, Program.httpClient);
                }
            };
            timerServidores.Start();
        }

        private async Task BuscaServidoresPR(UBGEBot_ ubgeBotClient, HttpClient httpClient)
        {
            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    JObject resposta = (JObject)JsonConvert.DeserializeObject(await httpClient.GetStringAsync("https://www.realitymod.com/prspy/json/serverdata.json"));
                    JArray listaResposta = (JArray)resposta.SelectToken("Data");

                    IMongoDatabase db = ubgeBotClient.localDB;
                    IMongoCollection<ServidoresUBGE_> servidoresDB = db.GetCollection<ServidoresUBGE_>(Valores.Mongo.servidoresUBGE);

                    FilterDefinition<ServidoresUBGE_> Filtro = Builders<ServidoresUBGE_>.Filter.Eq(x => x.jogo, "pr");
                    List<ServidoresUBGE_> resultadosLista = await (await servidoresDB.FindAsync(Filtro)).ToListAsync();

                    if (resultadosLista.Count > 0)
                        await servidoresDB.DeleteManyAsync(Filtro);

                    int N = 0;

                    foreach (JToken server in listaResposta)
                    {
                        string ipServidor = server.SelectToken("IPAddress").ToString(),
                        queryPort = server.SelectToken("QueryPort").ToString(),
                        paisServidor = server.SelectToken("Country").ToString(),
                        nomeServidor = server.SelectToken("ServerName").ToString(),
                        nomeJogo = server.SelectToken("GameName").ToString(),
                        versaoJogo = server.SelectToken("GameVersion").ToString(),
                        jogoPorta = server.SelectToken("GamePort").ToString(),
                        nomeMapa = server.SelectToken("MapName").ToString(),
                        modoDeJogo = server.SelectToken("GameMode").ToString(),
                        playersJogando = server.SelectToken("NumPlayers").ToString(),
                        maxPlayers = server.SelectToken("MaxPlayers").ToString(),
                        statusJogo = server.SelectToken("GameStatus").ToString();

                        if (int.Parse(playersJogando.ToString()) != 0)
                        {
                            await servidoresDB.InsertOneAsync(new ServidoresUBGE_
                            {
                                jogo = "pr",
                                jogadoresDoServidor = playersJogando,
                                mapaDoServidor = nomeMapa,
                                maximoDePlayers = maxPlayers,
                                modoDeJogo = modoDeJogo,
                                paisDoServidor = paisServidor,
                                versaoDoJogo = versaoJogo,
                                nomeDoServidor = nomeServidor,
                                fotoDoServidor = Valores.prLogoNucleo,
                                ipDoServidor = ipServidor,
                                portaDoServidor = $"{queryPort} ou {jogoPorta}",
                                statusDoServidor = statusJogo,
                                thumbnailDoServidor = Valores.logoUBGE,
                                servidorDisponivel = $"Project Reality (PR) = `servidores pr`",
                                nomeServidorParaComando = "Project Reality",
                                _id = new ObjectId()
                            });

                            ++N;
                        }
                    }
                }
                catch (Exception)
                {
                    ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Servidores, $"[ProjectReality-Servidores] A busca dos servidores Project Reality gerou um erro! Tudo normal, ainda continuo procurando...");
                }
            }).Start();
        }

        private async Task BuscaServidoresConanExiles(UBGEBot_ ubgeBotClient, HttpClient httpClient)
        {
            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    JObject respostaJson = (JObject)JsonConvert.DeserializeObject(await httpClient.GetStringAsync("https://api.battlemetrics.com/servers?filter[game]=conanexiles"));
                    JArray listaResposta = (JArray)respostaJson.SelectToken("data");

                    IMongoDatabase db = ubgeBotClient.localDB;
                    IMongoCollection<ServidoresUBGE_> servidoresUBGE = db.GetCollection<ServidoresUBGE_>(Valores.Mongo.servidoresUBGE);
                    FilterDefinition<ServidoresUBGE_> Filtro = Builders<ServidoresUBGE_>.Filter.Eq(x => x.jogo, "ce");
                    List<ServidoresUBGE_> resultados = await (await servidoresUBGE.FindAsync(Filtro)).ToListAsync();

                    if (resultados.Count > 0)
                        await servidoresUBGE.DeleteManyAsync(Filtro);

                    foreach (JToken servidor in listaResposta)
                    {
                        JToken propServidor = servidor.SelectToken("attributes");

                        string nomeServidor = propServidor.SelectToken("name").ToString(),
                        ipServidor = propServidor.SelectToken("ip").ToString(),
                        portaServidor = propServidor.SelectToken("port").ToString(),
                        playersJogando = propServidor.SelectToken("players").ToString(),
                        maxPlayers = propServidor.SelectToken("maxPlayers").ToString(),
                        statusServidor = propServidor.SelectToken("status").ToString(),
                        portaQuery = propServidor.SelectToken("portQuery").ToString(),
                        paisServidor = propServidor.SelectToken("country").ToString();

                        foreach (string nomeServidorConan in ubgeBotClient.ubgeBotConfig.ubgeBotServidoresConfig.nomeDoServidorDeConan)
                        {
                            if (nomeServidor.ToString().ToUpper().Contains(nomeServidorConan.ToUpper()))
                            {
                                await servidoresUBGE.InsertOneAsync(new ServidoresUBGE_
                                {
                                    jogo = "ce",
                                    jogadoresDoServidor = playersJogando,
                                    mapaDoServidor = "Não especificado.",
                                    maximoDePlayers = maxPlayers,
                                    modoDeJogo = "Não especificado.",
                                    paisDoServidor = paisServidor,
                                    versaoDoJogo = "Não especificado.",
                                    nomeDoServidor = nomeServidor,
                                    fotoDoServidor = Valores.logoUBGE,
                                    ipDoServidor = ipServidor,
                                    portaDoServidor = $"{portaServidor} ou {portaQuery}",
                                    statusDoServidor = statusServidor,
                                    thumbnailDoServidor = Valores.conanExilesLogoRuinasDeAstapor,
                                    servidorDisponivel = $"Conan Exiles = `servidores ce`",
                                    nomeServidorParaComando = "Conan Exiles",
                                    _id = new ObjectId()
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Servidores, "[ConanExiles-Servidores] A busca dos servidores Conan Exiles gerou um erro! Tudo normal, ainda continuo procurando...");
                }
            }).Start();
        }

        private async Task BuscaServidoresDayZ(UBGEBot_ ubgeBotClient, HttpClient httpClient)
        {
            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    JObject resposta = (JObject)JsonConvert.DeserializeObject(await httpClient.GetStringAsync("https://api.battlemetrics.com/servers?filter[game]=dayz"));
                    JArray listaResposta = (JArray)resposta.SelectToken("data");

                    IMongoDatabase db = ubgeBotClient.localDB;
                    IMongoCollection<ServidoresUBGE_> servidoresUBGE = db.GetCollection<ServidoresUBGE_>(Valores.Mongo.servidoresUBGE);
                    FilterDefinition<ServidoresUBGE_> Filtro = Builders<ServidoresUBGE_>.Filter.Eq(x => x.jogo, "dyz");
                    List<ServidoresUBGE_> resultados = await (await servidoresUBGE.FindAsync(Filtro)).ToListAsync();

                    if (resultados.Count > 0)
                        await servidoresUBGE.DeleteManyAsync(Filtro);

                    foreach (JToken servidor in listaResposta)
                    {
                        JToken propServidor = servidor.SelectToken("attributes");

                        string nomeServidor = propServidor.SelectToken("name").ToString(),
                        ipServidor = propServidor.SelectToken("ip").ToString(),
                        portaServidor = propServidor.SelectToken("port").ToString(),
                        playersJogando = propServidor.SelectToken("players").ToString(),
                        maxPlayers = propServidor.SelectToken("maxPlayers").ToString(),
                        statusServidor = propServidor.SelectToken("status").ToString(),
                        portaQuery = propServidor.SelectToken("portQuery").ToString(),
                        paisServidor = propServidor.SelectToken("country").ToString();

                        JToken detalhesServidor = propServidor.SelectToken("details");
                        string versaoServidor = detalhesServidor.SelectToken("version").ToString();

                        foreach (string NomeServidorDayZ in ubgeBotClient.ubgeBotConfig.ubgeBotServidoresConfig.nomeDoServidorDeDayZ)
                        {
                            if (nomeServidor.ToString().ToUpper().Contains(NomeServidorDayZ.ToUpper()))
                            {
                                await servidoresUBGE.InsertOneAsync(new ServidoresUBGE_
                                {
                                    jogo = "dyz",
                                    jogadoresDoServidor = playersJogando,
                                    mapaDoServidor = "Não especificado.",
                                    maximoDePlayers = maxPlayers,
                                    modoDeJogo = "Não especificado.",
                                    paisDoServidor = paisServidor,
                                    versaoDoJogo = versaoServidor,
                                    nomeDoServidor = nomeServidor,
                                    fotoDoServidor = Valores.dayZLogo,
                                    ipDoServidor = ipServidor,
                                    portaDoServidor = $"{portaServidor} ou {portaQuery}",
                                    statusDoServidor = statusServidor.ToString(),
                                    thumbnailDoServidor = Valores.logoUBGE,
                                    servidorDisponivel = $"Day Z = `servidores dyz`",
                                    nomeServidorParaComando = "Day Z",
                                    _id = new ObjectId()
                                });
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Servidores, "[DayZ-Servidores] A busca dos servidores Day Z gerou um erro! Tudo normal, ainda continuo procurando...");
                }
            }).Start();
        }

        private async Task BuscaServidoresOpenSpades(UBGEBot_ ubgeBotClient, HttpClient httpClient)
        {
            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    string respostaBuildAndShoot = ubgeBotClient.utilidadesGerais.ByteParaString(await (await httpClient.GetAsync($"http://services.buildandshoot.com/serverlist.json")).Content.ReadAsByteArrayAsync());
                    JArray jArray = (JArray)JsonConvert.DeserializeObject(respostaBuildAndShoot);

                    IMongoDatabase db = ubgeBotClient.localDB;
                    IMongoCollection<ServidoresUBGE_> servidoresUBGE = db.GetCollection<ServidoresUBGE_>(Valores.Mongo.servidoresUBGE);
                    FilterDefinition<ServidoresUBGE_> filtro = Builders<ServidoresUBGE_>.Filter.Eq(x => x.jogo, "os");
                    List<ServidoresUBGE_> resultados = await (await servidoresUBGE.FindAsync(filtro)).ToListAsync();

                    if (resultados.Count > 0)
                        await servidoresUBGE.DeleteManyAsync(filtro);

                    foreach (JToken propServidor in jArray)
                    {
                        string nomeServidor = propServidor.SelectToken("name").ToString(),
                        ipServidor = propServidor.SelectToken("identifier").ToString(),
                        mapaServidor = propServidor.SelectToken("map").ToString(),
                        modoDeJogoServidor = propServidor.SelectToken("game_mode").ToString(),
                        playersJogando = propServidor.SelectToken("players_current").ToString(),
                        maxPlayers = propServidor.SelectToken("players_max").ToString(),
                        versaoJogo = propServidor.SelectToken("game_version").ToString(),
                        paisServidor = propServidor.SelectToken("country").ToString();

                        foreach (string nomeServidorOpenSpades in ubgeBotClient.ubgeBotConfig.ubgeBotServidoresConfig.nomeDoServidorDeOpenSpades)
                        {
                            if (nomeServidor.ToUpper().Contains(nomeServidorOpenSpades.ToUpper()))
                            {
                                await servidoresUBGE.InsertOneAsync(new ServidoresUBGE_
                                {
                                    jogo = "os",
                                    jogadoresDoServidor = playersJogando,
                                    mapaDoServidor = mapaServidor,
                                    maximoDePlayers = maxPlayers,
                                    modoDeJogo = modoDeJogoServidor,
                                    paisDoServidor = paisServidor,
                                    versaoDoJogo = versaoJogo,
                                    nomeDoServidor = nomeServidor,
                                    fotoDoServidor = Valores.openSpadesLogo,
                                    ipDoServidor = ipServidor,
                                    portaDoServidor = ipServidor.Split(':')[2],
                                    statusDoServidor = "Online.",
                                    thumbnailDoServidor = Valores.logoUBGE,
                                    servidorDisponivel = $"OpenSpades = `servidores os`",
                                    nomeServidorParaComando = "OpenSpades",
                                    _id = new ObjectId()
                                });
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Servidores, "[OpenSpades-Servidores] A busca dos servidores OpenSpades gerou um erro! Tudo normal, ainda continuo procurando...");
                }
            }).Start();
        }

        private async Task BuscaServidoresCounterStrike(UBGEBot_ ubgeBotClient, HttpClient httpClient)
        {
            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    JObject deserializeJson = (JObject)JsonConvert.DeserializeObject(await httpClient.GetStringAsync("https://api.battlemetrics.com/servers?filter[game]=cs"));
                    JArray listaServidores = (JArray)deserializeJson.SelectToken("data");

                    IMongoDatabase db = ubgeBotClient.localDB;
                    IMongoCollection<ServidoresUBGE_> servidoresUBGE = db.GetCollection<ServidoresUBGE_>(Valores.Mongo.servidoresUBGE);
                    FilterDefinition<ServidoresUBGE_> filtro = Builders<ServidoresUBGE_>.Filter.Eq(x => x.jogo, "cs");
                    List<ServidoresUBGE_> resultados = await (await servidoresUBGE.FindAsync(filtro)).ToListAsync();

                    if (resultados.Count > 0)
                        await servidoresUBGE.DeleteManyAsync(filtro);

                    foreach (JToken servidor in listaServidores)
                    {
                        JToken propServidor = servidor.SelectToken("attributes");

                        string nomeServidor = propServidor.SelectToken("name").ToString(),
                        ipServidor = propServidor.SelectToken("ip").ToString(),
                        portaServidor = propServidor.SelectToken("port").ToString(),
                        playersJogando = propServidor.SelectToken("players").ToString(),
                        maxPlayers = propServidor.SelectToken("maxPlayers").ToString(),
                        statusServidor = propServidor.SelectToken("status").ToString(),
                        paisServidor = propServidor.SelectToken("country").ToString();

                        JToken detalhes = propServidor.SelectToken("details");

                        string nomeMapa = detalhes.SelectToken("map").ToString(),
                        versaoJogo = detalhes.SelectToken("version").ToString();

                        foreach (string NomeServidorCounterStrike in ubgeBotClient.ubgeBotConfig.ubgeBotServidoresConfig.nomeDoServidorDeCounterStrike)
                        {
                            if (nomeServidor.ToString().ToUpper().Contains(NomeServidorCounterStrike.ToUpper()))
                            {
                                await servidoresUBGE.InsertOneAsync(new ServidoresUBGE_
                                {
                                    jogo = "cs",
                                    jogadoresDoServidor = playersJogando,
                                    mapaDoServidor = nomeMapa,
                                    maximoDePlayers = maxPlayers,
                                    modoDeJogo = "Não especificado.",
                                    paisDoServidor = paisServidor,
                                    versaoDoJogo = versaoJogo,
                                    nomeDoServidor = nomeServidor,
                                    fotoDoServidor = Valores.counterStrikeLogo,
                                    ipDoServidor = $"ubge.ddns.net ou {ipServidor}",
                                    portaDoServidor = portaServidor,
                                    statusDoServidor = statusServidor,
                                    thumbnailDoServidor = Valores.logoUBGE,
                                    servidorDisponivel = $"Counter-Strike = `servidores cs`",
                                    nomeServidorParaComando = "Counter-Strike",
                                    _id = new ObjectId()
                                });
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Servidores, "[CounterStrike-Servidores] A busca dos servidores Counter-Strike gerou um erro! Tudo normal, ainda continuo procurando...");
                }
            }).Start();
        }

        private async Task BuscaServidoresUnturned(UBGEBot_ ubgeBotClient, HttpClient httpClient)
        {
            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    JObject deserializeJson = (JObject)JsonConvert.DeserializeObject(await httpClient.GetStringAsync("https://api.battlemetrics.com/servers?filter[game]=unturned"));
                    JArray listaServidores = (JArray)deserializeJson.SelectToken("data");

                    IMongoDatabase db = ubgeBotClient.localDB;
                    IMongoCollection<ServidoresUBGE_> servidoresUBGE = db.GetCollection<ServidoresUBGE_>(Valores.Mongo.servidoresUBGE);
                    FilterDefinition<ServidoresUBGE_> filtro = Builders<ServidoresUBGE_>.Filter.Eq(x => x.jogo, "unturned");
                    List<ServidoresUBGE_> resultados = await (await servidoresUBGE.FindAsync(filtro)).ToListAsync();

                    if (resultados.Count > 0)
                        await servidoresUBGE.DeleteManyAsync(filtro);

                    foreach (JToken Servidor in listaServidores)
                    {
                        JToken propServidor = Servidor.SelectToken("attributes");

                        string nomeServidor = propServidor.SelectToken("name").ToString(),
                        ipServidor = propServidor.SelectToken("ip").ToString(),
                        portaServidor = propServidor.SelectToken("port").ToString(),
                        playersJogando = propServidor.SelectToken("players").ToString(),
                        maxPlayers = propServidor.SelectToken("maxPlayers").ToString(),
                        statusServidor = propServidor.SelectToken("status").ToString(),
                        portaQuery = propServidor.SelectToken("portQuery").ToString(),
                        paisServidor = propServidor.SelectToken("country").ToString();

                        JToken detalhes = propServidor.SelectToken("details");

                        string versaoServidor = detalhes.SelectToken("version").ToString(),
                        modoDeJogo = detalhes.SelectToken("gameMode").ToString(),
                        mapaServidor = detalhes.SelectToken("map").ToString();

                        foreach (string NomeServidorUnturned in ubgeBotClient.ubgeBotConfig.ubgeBotServidoresConfig.nomeDoServidorDeUnturned)
                        {
                            if (nomeServidor.ToString().ToUpper().Contains(NomeServidorUnturned.ToUpper()))
                            {
                                await servidoresUBGE.InsertOneAsync(new ServidoresUBGE_
                                {
                                    jogo = "unturned",
                                    jogadoresDoServidor = playersJogando,
                                    mapaDoServidor = mapaServidor,
                                    maximoDePlayers = maxPlayers,
                                    modoDeJogo = modoDeJogo,
                                    paisDoServidor = paisServidor,
                                    versaoDoJogo = versaoServidor,
                                    nomeDoServidor = nomeServidor,
                                    fotoDoServidor = Valores.unturnedLogo,
                                    ipDoServidor = ipServidor,
                                    portaDoServidor = $"{portaServidor} ou {portaQuery}",
                                    statusDoServidor = statusServidor,
                                    thumbnailDoServidor = Valores.logoUBGE,
                                    servidorDisponivel = $"Unturned = `servidores unturned`",
                                    nomeServidorParaComando = "Unturned",
                                    _id = new ObjectId()
                                });
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Servidores, "[Unturned-Servidores] A busca dos servidores Unturned gerou um erro! Tudo normal, ainda continuo procurando...");
                }
            }).Start();
        }

        private async Task BuscaServidoresMordhau(UBGEBot_ ubgeBotClient, HttpClient httpClient)
        {
            await Task.Delay(0);

            new Thread(async () =>
            {
                try
                {
                    JObject deserializeJson = (JObject)JsonConvert.DeserializeObject(await httpClient.GetStringAsync("https://api.battlemetrics.com/servers?filter[game]=mordhau"));
                    JArray listaServidores = (JArray)deserializeJson.SelectToken("data");

                    IMongoDatabase db = ubgeBotClient.localDB;
                    IMongoCollection<ServidoresUBGE_> servidoresUBGE = db.GetCollection<ServidoresUBGE_>(Valores.Mongo.servidoresUBGE);
                    FilterDefinition<ServidoresUBGE_> filtro = Builders<ServidoresUBGE_>.Filter.Eq(x => x.jogo, "mordhau");
                    List<ServidoresUBGE_> resultados = await (await servidoresUBGE.FindAsync(filtro)).ToListAsync();

                    if (resultados.Count > 0)
                        await servidoresUBGE.DeleteManyAsync(filtro);

                    foreach (JToken servidor in listaServidores)
                    {
                        JToken propServidor = servidor.SelectToken("attributes");

                        string nomeServidor = propServidor.SelectToken("name").ToString(),
                        ipServidor = propServidor.SelectToken("ip").ToString(),
                        portaServidor = propServidor.SelectToken("port").ToString(),
                        playersJogando = propServidor.SelectToken("players").ToString(),
                        maxPlayers = propServidor.SelectToken("maxPlayers").ToString(),
                        statusServidor = propServidor.SelectToken("status").ToString(),
                        portaQuery = propServidor.SelectToken("portQuery").ToString(),
                        paisServidor = propServidor.SelectToken("country").ToString();

                        JToken detalhesServidor = propServidor.SelectToken("details");

                        string mapaServidor = detalhesServidor.SelectToken("map").ToString(),
                        modoDeJogo = detalhesServidor.SelectToken("gameMode").ToString();

                        foreach (string NomeServidorMordhau in ubgeBotClient.ubgeBotConfig.ubgeBotServidoresConfig.nomeDoServidorDeMordhau)
                        {
                            if (nomeServidor.ToString().ToUpper().Contains(NomeServidorMordhau.ToUpper()))
                            {
                                await servidoresUBGE.InsertOneAsync(new ServidoresUBGE_
                                {
                                    jogo = "mordhau",
                                    jogadoresDoServidor = playersJogando,
                                    mapaDoServidor = mapaServidor,
                                    maximoDePlayers = maxPlayers,
                                    modoDeJogo = modoDeJogo,
                                    paisDoServidor = paisServidor,
                                    versaoDoJogo = "Não especificado.",
                                    nomeDoServidor = nomeServidor,
                                    fotoDoServidor = Valores.mordhauLogo,
                                    ipDoServidor = ipServidor,
                                    portaDoServidor = $"{portaServidor} ou {portaQuery}",
                                    statusDoServidor = statusServidor.ToString(),
                                    thumbnailDoServidor = Valores.logoUBGE,
                                    servidorDisponivel = $"Mordhau = `servidores mordhau`",
                                    nomeServidorParaComando = "Mordhau",
                                    _id = new ObjectId()
                                });
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    ubgeBotClient.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Servidores, "[Mordhau-Servidores] A busca dos servidores Mordhau gerou um erro! Tudo normal, ainda continuo procurando...");
                }
            }).Start();
        }
    }
}