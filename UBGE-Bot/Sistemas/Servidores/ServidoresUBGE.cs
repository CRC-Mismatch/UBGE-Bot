using DSharpPlus;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
        public void AplicarAoBot(DiscordClient discordClient, bool botConectadoAoMongo)
        {
            Timer timerServidores = new Timer()
            {
                Interval = 15000,
            };
            timerServidores.Elapsed += async delegate
            {
                if (botConectadoAoMongo)
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
                    var linkPRSpy = await httpClient.GetStringAsync("https://www.realitymod.com/prspy/json/serverdata.json");
                    var resposta = (JObject)JsonConvert.DeserializeObject(linkPRSpy);
                    var listaResposta = (JArray)resposta.SelectToken("Data");

                    var db = ubgeBotClient.localDB;
                    var servidoresDB = db.GetCollection<ServidoresUBGE_>(Valores.Mongo.servidoresUBGE);

                    var Filtro = Builders<ServidoresUBGE_>.Filter.Eq(x => x.jogo, "pr");
                    var resultadosLista = await (await servidoresDB.FindAsync(Filtro)).ToListAsync();

                    if (resultadosLista.Count > 0)
                        await servidoresDB.DeleteManyAsync(Filtro);

                    int N = 0;

                    foreach (var server in listaResposta)
                    {
                        var ipServidor = server.SelectToken("IPAddress").ToString();
                        var queryPort = server.SelectToken("QueryPort").ToString();
                        var paisServidor = server.SelectToken("Country").ToString();
                        var nomeServidor = server.SelectToken("ServerName").ToString();
                        var nomeJogo = server.SelectToken("GameName").ToString();
                        var versaoJogo = server.SelectToken("GameVersion").ToString();
                        var jogoPorta = server.SelectToken("GamePort").ToString();
                        var nomeMapa = server.SelectToken("MapName").ToString();
                        var modoDeJogo = server.SelectToken("GameMode").ToString();
                        var playersJogando = server.SelectToken("NumPlayers").ToString();
                        var maxPlayers = server.SelectToken("MaxPlayers").ToString();
                        var statusJogo = server.SelectToken("GameStatus").ToString();

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
                    var linkBattleMetrics = await httpClient.GetStringAsync("https://api.battlemetrics.com/servers?filter[game]=conanexiles");
                    var respostaJson = (JObject)JsonConvert.DeserializeObject(linkBattleMetrics);
                    var listaResposta = (JArray)respostaJson.SelectToken("data");

                    var db = ubgeBotClient.localDB;
                    var servidoresUBGE = db.GetCollection<ServidoresUBGE_>(Valores.Mongo.servidoresUBGE);
                    var Filtro = Builders<ServidoresUBGE_>.Filter.Eq(x => x.jogo, "ce");
                    var resultados = await (await servidoresUBGE.FindAsync(Filtro)).ToListAsync();

                    if (resultados.Count > 0)
                        await servidoresUBGE.DeleteManyAsync(Filtro);

                    foreach (var servidor in listaResposta)
                    {
                        var propServidor = servidor.SelectToken("attributes");
                        var nomeServidor = propServidor.SelectToken("name").ToString();
                        var ipServidor = propServidor.SelectToken("ip").ToString();
                        var portaServidor = propServidor.SelectToken("port").ToString();
                        var playersJogando = propServidor.SelectToken("players").ToString();
                        var maxPlayers = propServidor.SelectToken("maxPlayers").ToString();
                        var statusServidor = propServidor.SelectToken("status").ToString();
                        var portaQuery = propServidor.SelectToken("portQuery").ToString();
                        var paisServidor = propServidor.SelectToken("country").ToString();

                        foreach (var nomeServidorConan in ubgeBotClient.ubgeBotConfig.ubgeBotServidoresConfig.nomeDoServidorDeConan)
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
                    var linkBattleMetrics = await httpClient.GetStringAsync("https://api.battlemetrics.com/servers?filter[game]=dayz");
                    var resposta = (JObject)JsonConvert.DeserializeObject(linkBattleMetrics);
                    var listaResposta = (JArray)resposta.SelectToken("data");

                    var db = ubgeBotClient.localDB;
                    var servidoresUBGE = db.GetCollection<ServidoresUBGE_>(Valores.Mongo.servidoresUBGE);
                    var Filtro = Builders<ServidoresUBGE_>.Filter.Eq(x => x.jogo, "dyz");
                    var resultados = await (await servidoresUBGE.FindAsync(Filtro)).ToListAsync();

                    if (resultados.Count > 0)
                        await servidoresUBGE.DeleteManyAsync(Filtro);

                    foreach (var servidor in listaResposta)
                    {
                        var propServidor = servidor.SelectToken("attributes");
                        var nomeServidor = propServidor.SelectToken("name").ToString();
                        var ipServidor = propServidor.SelectToken("ip").ToString();
                        var portaServidor = propServidor.SelectToken("port").ToString();
                        var playersJogando = propServidor.SelectToken("players").ToString();
                        var maxPlayers = propServidor.SelectToken("maxPlayers").ToString();
                        var statusServidor = propServidor.SelectToken("status").ToString();
                        var portaQuery = propServidor.SelectToken("portQuery").ToString();
                        var paisServidor = propServidor.SelectToken("country").ToString();
                        var detalhesServidor = propServidor.SelectToken("details");
                        var versaoServidor = detalhesServidor.SelectToken("version").ToString();

                        foreach (var NomeServidorDayZ in ubgeBotClient.ubgeBotConfig.ubgeBotServidoresConfig.nomeDoServidorDeDayZ)
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
                    var respostaBuildAndShoot = ubgeBotClient.utilidadesGerais.ByteParaString(await (await httpClient.GetAsync($"http://services.buildandshoot.com/serverlist.json")).Content.ReadAsByteArrayAsync());
                    var jArray = (JArray)JsonConvert.DeserializeObject(respostaBuildAndShoot);

                    var db = ubgeBotClient.localDB;
                    var servidoresUBGE = db.GetCollection<ServidoresUBGE_>(Valores.Mongo.servidoresUBGE);
                    var filtro = Builders<ServidoresUBGE_>.Filter.Eq(x => x.jogo, "os");
                    var resultados = await (await servidoresUBGE.FindAsync(filtro)).ToListAsync();

                    if (resultados.Count > 0)
                        await servidoresUBGE.DeleteManyAsync(filtro);

                    foreach (var propServidor in jArray)
                    {
                        var nomeServidor = propServidor.SelectToken("name").ToString();
                        var ipServidor = propServidor.SelectToken("identifier").ToString();
                        var mapaServidor = propServidor.SelectToken("map").ToString();
                        var modoDeJogoServidor = propServidor.SelectToken("game_mode").ToString();
                        var playersJogando = propServidor.SelectToken("players_current").ToString();
                        var maxPlayers = propServidor.SelectToken("players_max").ToString();
                        var versaoJogo = propServidor.SelectToken("game_version").ToString();
                        var paisServidor = propServidor.SelectToken("country").ToString();

                        foreach (var nomeServidorOpenSpades in ubgeBotClient.ubgeBotConfig.ubgeBotServidoresConfig.nomeDoServidorDeOpenSpades)
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
                    var respostaBattleMetrics = await httpClient.GetStringAsync("https://api.battlemetrics.com/servers?filter[game]=cs");
                    var deserializeJson = (JObject)JsonConvert.DeserializeObject(respostaBattleMetrics);
                    var listaServidores = (JArray)deserializeJson.SelectToken("data");

                    var db = ubgeBotClient.localDB;
                    var servidoresUBGE = db.GetCollection<ServidoresUBGE_>(Valores.Mongo.servidoresUBGE);
                    var filtro = Builders<ServidoresUBGE_>.Filter.Eq(x => x.jogo, "cs");
                    var resultados = await (await servidoresUBGE.FindAsync(filtro)).ToListAsync();

                    if (resultados.Count > 0)
                        await servidoresUBGE.DeleteManyAsync(filtro);

                    foreach (var servidor in listaServidores)
                    {
                        var propServidor = servidor.SelectToken("attributes");
                        var nomeServidor = propServidor.SelectToken("name").ToString();
                        var ipServidor = propServidor.SelectToken("ip").ToString();
                        var portaServidor = propServidor.SelectToken("port").ToString();
                        var playersJogando = propServidor.SelectToken("players").ToString();
                        var maxPlayers = propServidor.SelectToken("maxPlayers").ToString();
                        var statusServidor = propServidor.SelectToken("status").ToString();
                        var paisServidor = propServidor.SelectToken("country").ToString();
                        var detalhes = propServidor.SelectToken("details");
                        var nomeMapa = detalhes.SelectToken("map").ToString();
                        var versaoJogo = detalhes.SelectToken("version").ToString();

                        foreach (var NomeServidorCounterStrike in ubgeBotClient.ubgeBotConfig.ubgeBotServidoresConfig.nomeDoServidorDeCounterStrike)
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
                    var respostaBattleMetrics = await httpClient.GetStringAsync("https://api.battlemetrics.com/servers?filter[game]=unturned");
                    var deserializeJson = (JObject)JsonConvert.DeserializeObject(respostaBattleMetrics);
                    var listaServidores = (JArray)deserializeJson.SelectToken("data");

                    var db = ubgeBotClient.localDB;
                    var servidoresUBGE = db.GetCollection<ServidoresUBGE_>(Valores.Mongo.servidoresUBGE);
                    var filtro = Builders<ServidoresUBGE_>.Filter.Eq(x => x.jogo, "unturned");
                    var resultados = await (await servidoresUBGE.FindAsync(filtro)).ToListAsync();

                    if (resultados.Count > 0)
                        await servidoresUBGE.DeleteManyAsync(filtro);

                    foreach (var Servidor in listaServidores)
                    {
                        var propServidor = Servidor.SelectToken("attributes");
                        var nomeServidor = propServidor.SelectToken("name").ToString();
                        var ipServidor = propServidor.SelectToken("ip").ToString();
                        var portaServidor = propServidor.SelectToken("port").ToString();
                        var playersJogando = propServidor.SelectToken("players").ToString();
                        var maxPlayers = propServidor.SelectToken("maxPlayers").ToString();
                        var statusServidor = propServidor.SelectToken("status").ToString();
                        var portaQuery = propServidor.SelectToken("portQuery").ToString();
                        var paisServidor = propServidor.SelectToken("country").ToString();
                        var detalhes = propServidor.SelectToken("details");
                        var versaoServidor = detalhes.SelectToken("version").ToString();
                        var modoDeJogo = detalhes.SelectToken("gameMode").ToString();
                        var mapaServidor = detalhes.SelectToken("map").ToString();

                        foreach (var NomeServidorUnturned in ubgeBotClient.ubgeBotConfig.ubgeBotServidoresConfig.nomeDoServidorDeUnturned)
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
                    var respostaBattleMetrics = await httpClient.GetStringAsync("https://api.battlemetrics.com/servers?filter[game]=mordhau");
                    var deserializeJson = (JObject)JsonConvert.DeserializeObject(respostaBattleMetrics);
                    var listaServidores = (JArray)deserializeJson.SelectToken("data");

                    var db = ubgeBotClient.localDB;
                    var servidoresUBGE = db.GetCollection<ServidoresUBGE_>(Valores.Mongo.servidoresUBGE);
                    var filtro = Builders<ServidoresUBGE_>.Filter.Eq(x => x.jogo, "mordhau");
                    var resultados = await (await servidoresUBGE.FindAsync(filtro)).ToListAsync();

                    if (resultados.Count > 0)
                        await servidoresUBGE.DeleteManyAsync(filtro);

                    foreach (var servidor in listaServidores)
                    {
                        var propServidor = servidor.SelectToken("attributes");
                        var nomeServidor = propServidor.SelectToken("name").ToString();
                        var ipServidor = propServidor.SelectToken("ip").ToString();
                        var portaServidor = propServidor.SelectToken("port").ToString();
                        var playersJogando = propServidor.SelectToken("players").ToString();
                        var maxPlayers = propServidor.SelectToken("maxPlayers").ToString();
                        var statusServidor = propServidor.SelectToken("status").ToString();
                        var portaQuery = propServidor.SelectToken("portQuery").ToString();
                        var paisServidor = propServidor.SelectToken("country").ToString();
                        var detalhesServidor = propServidor.SelectToken("details");
                        var mapaServidor = detalhesServidor.SelectToken("map").ToString();
                        var modoDeJogo = detalhesServidor.SelectToken("gameMode").ToString();

                        foreach (var NomeServidorMordhau in ubgeBotClient.ubgeBotConfig.ubgeBotServidoresConfig.nomeDoServidorDeMordhau)
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