using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using File = Google.Apis.Drive.v3.Data.File;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MimeTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UBGE.Services.Google
{
    public sealed class GoogleDriveService
    {
        static readonly string[] Scopes = { DriveService.Scope.Drive };
        static readonly string NomeBot = "UBGE-Bot";

        public async Task<UserCredential> Autenticar()
        {
            UserCredential credenciais = null;

            using (var stream = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\credentials.json", FileMode.Open, FileAccess.Read))
            {
                credenciais = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "credential"), true));
            }

            return credenciais;
        }

        public DriveService ServicoDoDrive(UserCredential credenciais) => new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credenciais,
            ApplicationName = NomeBot,
        });

        public async Task<string[]> ProcurarArquivo(DriveService Servico, string Nome, bool ProcurarNaLixeira = false)
        {
            var retorno = new ConcurrentBag<string>();

            var resposta = Servico.Files.List();
            resposta.Q = $"name = '{Nome}' {(ProcurarNaLixeira ? "and trashed = true" : "and trashed = false")}";
            resposta.Fields = "files(id)";

            var resultado = await resposta.ExecuteAsync();

            if (resultado.Files != null && resultado.Files.Any())
            {
                foreach (var arquivo in resultado.Files)
                    retorno.Add(arquivo.Id);
            }

            return retorno.ToArray();
        }

        public async Task UploadArquivo(DriveService Servico, string CaminhoArquivo, string PastaGoogleDrive, bool ProcurarNaLixeira)
        {
            var arquivo = new File()
            {
                Name = Path.GetFileName(CaminhoArquivo),
                MimeType = MimeTypeMap.GetMimeType(Path.GetExtension(CaminhoArquivo)),
                Parents = new List<string>(new string[] { (await ProcurarArquivo(Servico, PastaGoogleDrive, ProcurarNaLixeira)).FirstOrDefault() }),
            };

            using (var stream = new FileStream(CaminhoArquivo, FileMode.Open, FileAccess.Read))
            {
                var request = Servico.Files.Create(arquivo, stream, arquivo.MimeType);
                request.Fields = "id, parents";

                await request.UploadAsync();
            }
        }
    }
}