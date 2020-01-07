using Autofac;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Google.Apis.Drive.v3;
using Ionic.Zip;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UBGE_Bot.APIs;
using UBGE_Bot.Main;
using UBGE_Bot.LogExceptions;
using UBGE_Bot.Utilidades;

namespace UBGE_Bot.Sistemas.Bot
{
    public sealed class DownloadArquivosMongo : IAplicavelAoCliente
    {
        public void AplicarAoBot(DiscordClient discordClient, bool botConectadoAoMongo, bool sistemaAtivo)
            => discordClient.GuildDownloadCompleted += FazODownload;

        public async Task FazODownload(GuildDownloadCompletedEventArgs guildDownloadCompletedEventArgs)
        {
            string diretorioBot = Directory.GetCurrentDirectory();

            if (!File.Exists(diretorioBot + @"\mongoimport.exe") || !File.Exists(diretorioBot + @"\mongoexport.exe") || !File.Exists(diretorioBot + @"\libeay32.dll") || !File.Exists(diretorioBot + @"\ssleay32.dll"))
            {
                await Program.ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.Aviso, "Os arquivos de funcionamento dos comandos do Mongo não foram encontrados, fazendo o download dos mesmos...", await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(guildDownloadCompletedEventArgs.Client, "UBGE"));

                Program.ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Sistemas, "Os arquivos de funcionamento dos comandos do Mongo não foram encontrados, fazendo o download dos mesmos...");
            }
            else
                return;

            GoogleDrive.Main credenciais = Program.ubgeBot.servicesIContainer.Resolve<GoogleDrive.Main>();

            using (DriveService servico = credenciais.ServicoDoDrive(await credenciais.Autenticar()))
            {
                string[] ids = await credenciais.ProcurarArquivo(servico, "Mongo.zip", false);

                if (ids != null && ids.Any())
                {
                    using (FileStream stream = new FileStream(diretorioBot + @"\Mongo.zip", FileMode.Create, FileAccess.Write))
                        await servico.Files.Get(ids.FirstOrDefault()).DownloadAsync(stream);
                }
            }

            using (ZipFile Zip = new ZipFile(diretorioBot + @"\Mongo.zip", Encoding.UTF8))
                Zip.ExtractAll(diretorioBot + @"\Mongo", ExtractExistingFileAction.OverwriteSilently);

            File.Move(diretorioBot + @"\Mongo\mongoexport.exe", diretorioBot + @"\mongoexport.exe", true);
            File.Move(diretorioBot + @"\Mongo\mongoimport.exe", diretorioBot + @"\mongoimport.exe", true);
            File.Move(diretorioBot + @"\Mongo\libeay32.dll", diretorioBot + @"\libeay32.dll", true);
            File.Move(diretorioBot + @"\Mongo\ssleay32.dll", diretorioBot + @"\ssleay32.dll", true);

            Directory.Delete(diretorioBot + @"\Mongo");
            File.Delete(diretorioBot + @"\Mongo.zip");

            await Program.ubgeBot.logExceptionsToDiscord.EmbedLogMessages(LogExceptionsToDiscord.TipoEmbed.Aviso, "Os arquivos foram baixados e extraídos com sucesso!");

            Program.ubgeBot.logExceptionsToDiscord.Aviso(LogExceptionsToDiscord.TipoAviso.Sistemas, "Os arquivos foram baixados e extraídos com sucesso!");
        }
    }
}