using DSharpPlus;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using Timer = System.Timers.Timer;
using System.Threading.Tasks;
using UBGE_Bot.Carregamento;
using UBGE_Bot.Main;
using UBGE_Bot.MongoDB.Modelos;
using UBGE_Bot.Utilidades;

namespace UBGE_Bot.Sistemas.Bot
{
    public sealed class ModuloBotAberto : IAplicavelAoCliente
    {
        public void AplicarAoBot(DiscordClient discordClient, bool botConectadoAoMongo, bool sistemaAtivo)
        {
            Timer timerModuloChecarBotAberto = new Timer()
            {
                Interval = 10000,
            };
            timerModuloChecarBotAberto.Elapsed += async delegate
            {
                if (botConectadoAoMongo && sistemaAtivo)
                    await ModuloBotAbertoTask(Program.ubgeBot);
            };
            timerModuloChecarBotAberto.Start();
        }

        private static async Task ModuloBotAbertoTask(UBGEBot_ ubgeBotClient)
        {
            IMongoDatabase db = ubgeBotClient.localDB;
            IMongoCollection<CheckBotAberto> collectionCheckBotAberto = db.GetCollection<CheckBotAberto>(Valores.Mongo.checkBotAberto);

            FilterDefinition<CheckBotAberto> filtro = Builders<CheckBotAberto>.Filter.Empty;

            List<CheckBotAberto> lista = await (await collectionCheckBotAberto.FindAsync<CheckBotAberto>(filtro)).ToListAsync();

            if (lista.Count == 0)
                await collectionCheckBotAberto.InsertOneAsync(new CheckBotAberto { numero = 0, diaEHora = DateTime.Now });
            else
                await collectionCheckBotAberto.UpdateOneAsync(filtro, Builders<CheckBotAberto>.Update.Set(x => x.numero, 0).Set(y => y.diaEHora, DateTime.Now));
        }
    }
}