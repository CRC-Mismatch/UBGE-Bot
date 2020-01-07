using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Microsoft.Win32;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Management;
using System.Net;
using UBGE_Bot.Main;
using UBGE_Bot.Utilidades;

namespace UBGE_Bot.Comandos.Gerais
{
    [Group("dev")]

    public sealed class StaffControlled : BaseCommandModule
    {
        [Command("alterarjson"), Aliases("aj", "alterarvalores", "av"), RequireOwner]

        public async Task AlterarVariaveisNoJsonAsync(CommandContext ctx, string nomeVariavel = null, string valorVariavel = null)
        {
            await ctx.TriggerTypingAsync();

            if (string.IsNullOrWhiteSpace(nomeVariavel))
            {
                await ctx.RespondAsync($"{ctx.Member.Mention}, digite o nome da variável!");

                return;
            }
            else if (string.IsNullOrWhiteSpace(valorVariavel))
            {
                await ctx.RespondAsync($"{ctx.Member.Mention}, digite o valor da variável!");

                return;
            }

            string caminhoJson = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\ValoresConfig.json";

            JObject json = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(caminhoJson));

            if (json[nomeVariavel] != null)
                json[nomeVariavel] = valorVariavel;
            else
            {
                await ctx.RespondAsync($"Essa variável não existe no JSON!");

                return;
            }

            File.WriteAllBytes(caminhoJson, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(json, Formatting.Indented)));

            await ctx.RespondAsync($"{ctx.Member.Mention}, o valor da variável `{nomeVariavel}` foi alterado com sucesso para `{valorVariavel}`. :smile:");
        }

        [Command("verjson"), Aliases("vj"), RequireOwner]

        public async Task VerJsonAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            IEnumerable<JToken> variaveisJson = ((JObject)JsonConvert.DeserializeObject(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\ValoresConfig.json"))).Children();

            if (variaveisJson.Count() == 0)
            {
                await ctx.RespondAsync($"{ctx.Member.Mention}, não existe variáveis nesse JSON!");

                return;
            }

            StringBuilder strVariaveis = new StringBuilder();

            foreach (JObject variavelJson in variaveisJson)
                strVariaveis.Append($"{variavelJson} | ");

            string areaDeTrabalho = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\VariáveisDoJson.txt";

            File.WriteAllText(areaDeTrabalho, strVariaveis.ToString(), Encoding.UTF8);

            await ctx.RespondWithFileAsync(areaDeTrabalho, ctx.Member.Mention);

            File.Delete(areaDeTrabalho);
        }

        [Command("viewrole"), Aliases("vr", "vercargo"), RequireOwner]

        public async Task VerCargoAsync(CommandContext ctx, DiscordRole cargo = null)
        {
            await ctx.TriggerTypingAsync();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            if (cargo == null)
            {
                embed.WithColor(Program.ubgeBot.utilidadesGerais.CorHelpComandos())
                        .WithAuthor("Como executar este comando:", null, Valores.infoLogo)
                        .AddField("PC/Mobile", $"{ctx.Prefix}dev vr[ID/Nome entre \"\"]")
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithTimestamp(DateTime.Now)
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                await ctx.RespondAsync(embed: embed.Build());

                return;
            }

            DiscordMessage msgAguarde = await ctx.RespondAsync($"Aguarde... {await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "leofsjal")}");

            List<DiscordMember> membrosUBGE = (await ctx.Guild.GetAllMembersAsync()).ToList();
            List<DiscordMember> membrosComOCargo = membrosUBGE.FindAll(x => x.Roles.ToList().Contains(cargo));

            await msgAguarde.DeleteAsync();

            embed.AddField("Admin?:", cargo.CheckPermission(Permissions.Administrator) == PermissionLevel.Allowed ? "**Sim**" : "**Não**", false)
                .AddField("Mencionável?:", cargo.IsMentionable ? "**Sim**" : "**Não**", false)
                .AddField("ID:", cargo.Id.ToString(), false)
                .AddField("Quantidade de membros com este cargo:", $"{(membrosComOCargo.Count > 1 ? $"**{membrosComOCargo.Count}** membros." : $"**{membrosComOCargo.Count}** membro.")}", false)
                .AddField("Cor:", $"R: **{cargo.Color.R}** | G: **{cargo.Color.G}** | B: **{cargo.Color.B}**{(cargo.Color.R == 0 && cargo.Color.B == 0 && cargo.Color.G == 0 ? " - Este cargo não tem cor." : string.Empty)}", false)
                .AddField("Dia em que foi criado:", cargo.CreationTimestamp.DateTime.ToString(), false)
                .AddField("Posição na hierarquia dos cargos:", $"**{(ctx.Guild.Roles.Values.OrderByDescending(x => x.Position).ToList()).FindIndex(x => x == cargo) + 1}**", false)
                .WithAuthor($"Informações do cargo: \"{cargo.Name}\"", null, Valores.logoUBGE)
                .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                .WithThumbnailUrl(ctx.Member.AvatarUrl)
                .WithTimestamp(DateTime.Now)
                .WithColor(cargo.Color)
                .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

            await ctx.RespondAsync(embed: embed.Build());
        }

        [Command("viewemoji"), Aliases("ve", "veremoji"), UBGE_Staff]

        public async Task VerEmojiAsync(CommandContext ctx, string emojiNome = null)
        {
            await ctx.TriggerTypingAsync();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            if (string.IsNullOrWhiteSpace(emojiNome))
            {
                embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithAuthor($"Digite o nome do emoji!", null, Valores.logoUBGE)
                    .WithThumbnailUrl(ctx.Member.AvatarUrl)
                    .WithDescription(":warning:")
                    .WithTimestamp(DateTime.Now)
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                await ctx.RespondAsync(embed: embed.Build());
                return;
            }

            DiscordEmoji emojo = await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, emojiNome);

            if (emojo == null)
            {
                await ctx.RespondAsync($"{ctx.Member.Mention}, este emoji não foi encontrado!");

                return;
            }

            embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                .WithAuthor($"Informações do Emoji: \"{emojo.Name}\" - ({emojo.Id})", null, Valores.logoUBGE)
                .WithThumbnailUrl(emojo.Url)
                .WithDescription($"ID: {emojo.Id}\n\n" +
                $"É um gif?: {(emojo.IsAnimated ? "**Sim**" : "**Não**")}\n\n" +
                $"Colocado no dia: {emojo.CreationTimestamp.DateTime.ToString()}")
                .WithTimestamp(DateTime.Now)
                .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

            await ctx.RespondAsync(embed: embed.Build());
        }

        [Command("status"), UBGE_Staff]

        public async Task BotStatusAsync(CommandContext ctx)
        {
            Stopwatch watch = Stopwatch.StartNew();
            await ctx.TriggerTypingAsync();
            watch.Stop();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            Process p = Process.GetCurrentProcess();

            ManagementObjectSearcher mos = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
            ManagementObjectSearcher mram = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PhysicalMemory");
            PerformanceCounter pc = new PerformanceCounter("Process", "% Processor Time", p.ProcessName, Environment.MachineName);

            bool naoEWindows = false;

            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                naoEWindows = true;

            embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                .WithAuthor("Status do Bot:", null, Program.ubgeBot.discordClient.CurrentUser.AvatarUrl)
                .AddField("Status Gerais:", $"**Ping:** {ctx.Client.Ping.ToString()}ms\n" +
                $"**Rest:** {watch.ElapsedMilliseconds}ms\n" +
                $"**PID:** {p.Id}\n" +
                $"**Prioridade do processo:** {p.PriorityClass.ToString()}\n" +
                $"**Threads do bot:** {p.Threads.Count}\n" +
                $"**O processo está respondendo?:** {(p.Responding ? "Sim" : "Não")}\n" +
                $"**Nome do processo:** {p.ProcessName}.exe\n" +
                $"**Interação com o usuário?:** {(Environment.UserInteractive ? "Sim" : "Não")}\n" +
                $"**Uptime (Ligado desde quando):** {p.StartTime.ToString()}\n" +
                $"**Dia e hora neste computador:** {DateTime.Now.ToString()}\n" +
                $"**Versão:** {Valores.versao_Bot}")
                .AddField("Especificações do computador onde estou hospedado:", $"**Nome:** {Environment.UserName}\n" +
                $"**Nome do computador:** {Environment.MachineName}\n" +
                $"**Versão do sistema operacional:** {Environment.OSVersion.VersionString} - ({(naoEWindows ? "Indisponível no momento." : NomeDoSistemaOperacional())})\n" +
                $"**Sistema operacional de 64 bits?:** {(Environment.Is64BitOperatingSystem ? "Sim" : "Não")}\n" +
                $"**Processador:** {(naoEWindows ? "Indisponível no momento." : NomeDoProcessador(mos))}\n" +
                $"**Número de Núcleos:** {Environment.ProcessorCount}\n" +
                $"**Uso de cpu:** {(naoEWindows ? "Indisponível no momento." : UsoDeCPU(pc))}\n" +
                $"**Este é um processo 64 bits?:** {(Environment.Is64BitProcess ? "Sim" : "Não")}\n" +
                $"**Memória ram do computador:** {(naoEWindows ? "Indisponível no momento." : MemoriaRamDoPC(mram))}\n" +
                $"**Uso de ram:** {(naoEWindows ? "Indisponível no momento" : UsoDeRAM(p))}")
                .AddField("Biblioteca(s):", $"**Versão do DSharpPlus:** {ctx.Client.VersionString}\n")
                .WithThumbnailUrl(Valores.csharpLogo)
                .WithTimestamp(DateTime.Now)
                .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

            await ctx.RespondAsync(embed: embed.Build());
        }

        private string MemoriaRamDoPC(ManagementObjectSearcher managementObjectSearcher)
        {
            ulong memoriaRam = 0;

            foreach (ManagementBaseObject objeto in managementObjectSearcher.Get())
                memoriaRam += ulong.Parse(objeto.Properties["Capacity"].Value.ToString());

            return $"{memoriaRam / 1024 / 1024 / 1024}gb ou {memoriaRam / 1024 / 1024}mb";
        }

        private string NomeDoProcessador(ManagementObjectSearcher managementObjectSearcher)
        {
            foreach (ManagementBaseObject objeto in managementObjectSearcher.Get())
                return objeto["Name"].ToString();

            return null;
        }

        private string UsoDeRAM(Process process)
        {
            process.Refresh();

            return $"{decimal.Parse(process.PrivateMemorySize64.Bytes().Humanize("###.##").Split("MB")[0]) - 5} mb";
        }

        private string UsoDeCPU(PerformanceCounter performanceCounter)
            => "Indisponível.";

        private string NomeDoSistemaOperacional()
            => Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion").GetValue("ProductName").ToString();

        [Command("reiniciar"), Aliases("reboot", "restart"), RequireOwner]

        public async Task ReiniciarAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync($"Já se foi o disco voadooooorrr");

            Process.Start(Directory.GetCurrentDirectory() + @"\UBGE-Bot.exe");

            Environment.Exit(1);
        }

        [Command("shutdown"), Aliases("off", "desligar"), RequireOwner]

        public async Task DesligarAsync(CommandContext ctx, string tempo = null)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync($"Adiós Muchacho! :wave:");

            Environment.Exit(1);
        }

        [Command("listarservidores"), Aliases("listarservidor"), UBGE_Staff]

        public async Task ListarServidorAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            StringBuilder strServidores = new StringBuilder();

            foreach (DiscordGuild guilds in ctx.Client.Guilds.Values)
                strServidores.Append($"**{guilds.Name}** - `{guilds.Id}`\n");

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { Name = "Servidores em que estou:", IconUrl = Valores.logoUBGE },
                Color = Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed(),
                Description = strServidores.ToString(),
                ThumbnailUrl = ctx.Member.AvatarUrl,
                Timestamp = DateTime.Now,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", IconUrl = ctx.Member.AvatarUrl }
            };

            await ctx.RespondAsync(embed: embed.Build());
        }
    }

    [Group("mongo"), Aliases("mongodb", "mtk"), RequireOwner, BotConectadoAoMongo]

    public sealed class MongoStaffControlled : BaseCommandModule
    {
        [Command("export"), Aliases("e", "exportar")]

        public async Task ExportarAsync(CommandContext ctx, [RemainingText] string colecao = null)
        {
            await ctx.TriggerTypingAsync();

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();

            if (string.IsNullOrWhiteSpace(colecao))
            {
                builder.WithAuthor("Digite o nome da coleção!", null, Valores.logoUBGE)
                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithDescription(":thinking:")
                    .WithThumbnailUrl(ctx.Member.AvatarUrl)
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: builder.Build());
                return;
            }

            IMongoDatabase db = Program.ubgeBot.localDB;

            string nomeDaColecao = (await (await db.ListCollectionNamesAsync()).ToListAsync()).Find(x => x.ToLower() == colecao.ToLower());

            if (string.IsNullOrWhiteSpace(nomeDaColecao))
            {
                builder.WithAuthor("Essa coleção não existe!", null, Valores.logoUBGE)
                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithDescription(":thinking:")
                    .WithThumbnailUrl(ctx.Member.AvatarUrl)
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: builder.Build());

                return;
            }

            string diretorioExports = Directory.GetCurrentDirectory() + @"\Exports";

            Process.Start("cmd.exe", $"/c mongoexport --db local --collection {colecao} --out \"{diretorioExports}\\{colecao}.json\"").WaitForExit();

            if (!Directory.Exists(diretorioExports))
                Directory.CreateDirectory(diretorioExports);

            builder.WithAuthor($"Coleção exportada com sucesso de local.{colecao} para {colecao}.json", null, Valores.logoUBGE)
                .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                .WithDescription(await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "UBGE"))
                .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                .WithThumbnailUrl(ctx.Member.AvatarUrl)
                .WithTimestamp(DateTime.Now);

            await ctx.RespondAsync(embed: builder.Build());

            using FileStream Arquivo = new FileStream($@"{diretorioExports}\{colecao}.json", FileMode.Open);

            await ctx.RespondWithFileAsync(Arquivo);

            File.Delete($@"{Directory.GetCurrentDirectory()}\Exports\{colecao}.json");
        }

        [Command("import"), Aliases("i", "importar")]

        public async Task ImportarAsync(CommandContext ctx, [RemainingText] string colecao = null)
        {
            await ctx.TriggerTypingAsync();

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();

            if (string.IsNullOrWhiteSpace(colecao))
            {
                builder.WithAuthor("Digite o nome da coleção!", null, Valores.logoUBGE)
                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithDescription(":thinking:")
                    .WithThumbnailUrl(ctx.Member.AvatarUrl)
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: builder.Build());
                return;
            }

            if (ctx.Message.Attachments.ToList().Count != 0)
            {
                if (ctx.Message.Attachments.ToList().First().Url != null)
                {
                    IMongoDatabase db = Program.ubgeBot.localDB;

                    string nomeDaColecao = (await (await db.ListCollectionNamesAsync()).ToListAsync()).Find(x => x.ToLower() == colecao.ToLower());

                    if (string.IsNullOrWhiteSpace(nomeDaColecao))
                    {
                        builder.WithAuthor("Essa coleção não existe!", null, Valores.logoUBGE)
                            .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithDescription(":thinking:")
                            .WithThumbnailUrl(ctx.Member.AvatarUrl)
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                            .WithTimestamp(DateTime.Now);

                        await ctx.RespondAsync(embed: builder.Build());

                        return;
                    }

                    string diretorioExports = Directory.GetCurrentDirectory() + @"\Exports";

                    if (!Directory.Exists(diretorioExports))
                        Directory.CreateDirectory(diretorioExports);

                    using (WebClient client = new WebClient())
                        client.DownloadFileAsync(new Uri(ctx.Message.Attachments.ToList().FirstOrDefault().Url), $@"{diretorioExports}\{colecao}.json");

                    Process.Start("cmd.exe", $"/c mongoimport --db local --collection {colecao} --file \"{diretorioExports}\\{colecao}.json\"").WaitForExit();

                    await ctx.Message.DeleteAsync();

                    builder.WithAuthor($"Coleção importada com sucesso de {colecao}.json para local.{colecao}", null, Valores.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithDescription(await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "UBGE"))
                        .WithThumbnailUrl(ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now)
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    await ctx.RespondAsync(embed: builder.Build());

                    File.Delete($@"{Directory.GetCurrentDirectory()}\Exports\{colecao}.json");
                }
            }
        }

        [Command("createcollection"), Aliases("cc", "criarcolecao", "criarcoleção")]

        public async Task CriarColecaoAsync(CommandContext ctx, [RemainingText] string collection = null)
        {
            await ctx.TriggerTypingAsync();

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();

            if (string.IsNullOrWhiteSpace(collection))
            {
                builder.WithAuthor("Digite o nome da coleção!", null, Valores.logoUBGE)
                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithDescription(":thinking:")
                    .WithTimestamp(DateTime.Now)
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithThumbnailUrl(ctx.Member.AvatarUrl);

                await ctx.RespondAsync(embed: builder.Build());
                return;
            }

            IMongoDatabase local = Program.ubgeBot.localDB;

            string nomeDaColecao = (await (await local.ListCollectionNamesAsync()).ToListAsync()).Find(x => x.ToLower() == collection.ToLower());

            if (!string.IsNullOrWhiteSpace(nomeDaColecao))
            {
                builder.WithAuthor("Essa coleção já existe!", null, Valores.logoUBGE)
                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithDescription(":thinking:")
                    .WithThumbnailUrl(ctx.Member.AvatarUrl)
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: builder.Build());

                return;
            }

            await local.CreateCollectionAsync(collection);

            builder.WithAuthor($"Coleção criada com sucesso em: local.{collection}", null, Valores.logoUBGE)
                .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                .WithDescription(await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "UBGE"))
                .WithTimestamp(DateTime.Now)
                .WithThumbnailUrl(ctx.Member.AvatarUrl)
                .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

            await ctx.RespondAsync(embed: builder.Build());
        }

        [Command("dropcollection"), Aliases("dc", "droparcolecao", "droparcoleção", "apagarcolecao", "apagarcoleção")]

        public async Task ExcluirColecaoAsync(CommandContext ctx, [RemainingText] string collection = null)
        {
            await ctx.TriggerTypingAsync();

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();

            if (string.IsNullOrWhiteSpace(collection))
            {
                builder.WithAuthor("Digite o nome da coleção!", null, Valores.logoUBGE)
                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithDescription(":thinking:")
                    .WithTimestamp(DateTime.Now)
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithThumbnailUrl(ctx.Member.AvatarUrl);

                await ctx.RespondAsync(embed: builder.Build());
                return;
            }

            IMongoDatabase local = Program.ubgeBot.localDB;

            string nomeDaColecao = (await (await local.ListCollectionNamesAsync()).ToListAsync()).Find(x => x.ToLower() == collection.ToLower());

            if (string.IsNullOrWhiteSpace(nomeDaColecao))
            {
                builder.WithAuthor("Essa coleção não existe!", null, Valores.logoUBGE)
                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithDescription(":thinking:")
                    .WithThumbnailUrl(ctx.Member.AvatarUrl)
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: builder.Build());

                return;
            }

            await local.DropCollectionAsync(nomeDaColecao);

            builder.WithAuthor($"Coleção excluída com sucesso em: local.{collection}", null, Valores.logoUBGE)
                .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                .WithDescription(await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "UBGE"))
                .WithThumbnailUrl(ctx.Member.AvatarUrl)
                .WithTimestamp(DateTime.Now)
                .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

            await ctx.RespondAsync(embed: builder.Build());
        }

        [Command("viewcollections"), Aliases("vercolecoes", "vercoleções", "vc")]

        public async Task VerColecoesAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            IMongoDatabase local = Program.ubgeBot.localDB;
            List<string> listas = await (await local.ListCollectionNamesAsync()).ToListAsync();

            if (listas.Count() == 0)
            {
                embed.WithAuthor("Não existe coleções no banco de dados!", null, Valores.logoUBGE)
                    .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                    .WithDescription(":thinking:")
                    .WithThumbnailUrl(ctx.Member.AvatarUrl)
                    .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                    .WithTimestamp(DateTime.Now);

                await ctx.RespondAsync(embed: embed.Build());
                return;
            }

            int index = 0;

            StringBuilder str = new StringBuilder();

            foreach (string nome in listas)
                str.Append($"`{++index}.` - **{nome}**\n");

            embed.WithAuthor("Coleções atuais:", null, Valores.logoUBGE)
                .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                .WithDescription(await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "UBGE"))
                .WithDescription(str.ToString())
                .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                .WithTimestamp(DateTime.Now);

            await ctx.RespondAsync(embed: embed.Build());
        }
    }
}