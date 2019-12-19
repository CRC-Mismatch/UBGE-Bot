using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Management;
using System.Net;
using UBGE_Bot.Main;
using UBGE_Bot.LogExceptions;
using UBGE_Bot.Utilidades;
using MongoDB.Driver;

namespace UBGE_Bot.Comandos.Gerais
{
    [Group("dev")]

    public sealed class StaffControlled : BaseCommandModule
    {
        [Command("recarregarbot"), Aliases("reload", "restart", "recarregarvariáveis", "recarregarvariaveis"), RequireOwner]

        public async Task RecarregarVariaveisDoBoTAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    var novaConfigDeValores = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.Build();

                    Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig = novaConfigDeValores;

                    await ctx.RespondAsync($"O JSON de variáveis foi recarregado e todas as possíveis alterações no bot de valores de variáveis foi realizada! :smile: {await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "UBGE")}");
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("viewrole"), Aliases("vr", "vercargo"), RequireOwner]

        public async Task VerCargoAsync(CommandContext ctx, DiscordRole cargo = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    if (cargo == null)
                    {
                        embed.WithColor(Program.ubgeBot.utilidadesGerais.CorHelpComandos())
                                .WithAuthor("Como executar este comando:", null, Valores.infoLogo)
                                .AddField("PC/Mobile", $"{ctx.Prefix}dev viewrole[ID/Nome entre \"\"]")
                                .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                                .WithTimestamp(DateTime.Now)
                                .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                        await ctx.RespondAsync(embed: embed.Build());

                        return;
                    }

                    List<DiscordMember> membrosUBGE = (await ctx.Guild.GetAllMembersAsync()).ToList();
                    var membrosComOCargo = membrosUBGE.FindAll(x => x.Roles.ToList().Contains(cargo));

                    embed.AddField("Admin?:", cargo.CheckPermission(Permissions.Administrator) == PermissionLevel.Allowed ? "Sim" : "Não", false)
                        .AddField("Mencionável?:", cargo.IsMentionable ? "Sim" : "Não", false)
                        .AddField("ID:", cargo.Id.ToString(), false)
                        .AddField("Quantidade de membros com este cargo:", $"{membrosComOCargo.Count} membros", false)
                        .AddField("Cor:", $"R: {cargo.Color.R} | G: {cargo.Color.G} | B: {cargo.Color.B}{(cargo.Color.R == 0 && cargo.Color.B == 0 && cargo.Color.G == 0 ? " - Este cargo não tem cor." : string.Empty)}", false)
                        .AddField("Dia em que foi criado:", cargo.CreationTimestamp.DateTime.ToString(), false)
                        .AddField("Posição na hierarquia dos cargos:", cargo.Position.ToString(), false)
                        .WithAuthor($"Informações do cargo: \"{cargo.Name}\"", null, Valores.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithTimestamp(DateTime.Now)
                        .WithColor(cargo.Color)
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    await (await ctx.Member.CreateDmChannelAsync()).SendMessageAsync(embed: embed.Build());
                    await ctx.RespondAsync("Olhe seu privado! :wink:");
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("viewemoji"), Aliases("ve", "veremoji"), UBGE_Staff]

        public async Task VerEmojiAsync(CommandContext ctx, string emojiNome = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    if (string.IsNullOrWhiteSpace(emojiNome))
                    {
                        embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                            .WithAuthor($"Diga o nome do emoji!", null, Valores.logoUBGE)
                            .WithThumbnailUrl(ctx.Member.AvatarUrl)
                            .WithDescription(":warning:")
                            .WithTimestamp(DateTime.Now)
                            .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                        await ctx.RespondAsync(embed: embed.Build());
                        return;
                    }

                    var emojo = await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, emojiNome);

                    embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithAuthor($"Informações do Emoji: \"{emojo.Name}\" - ({emojo.Name})", null, Valores.logoUBGE)
                        .WithThumbnailUrl(emojo.Url)
                        .WithDescription($"ID: {emojo.Id}\n\n" +
                        $"É um gif?: {(emojo.IsAnimated ? "Sim" : "Não")}\n\n" +
                        $"Colocado no dia: {emojo.CreationTimestamp.DateTime.ToString()}")
                        .WithTimestamp(DateTime.Now)
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    await ctx.RespondAsync(embed: embed.Build());
                }
                catch (Exception)
                {
                    await ctx.RespondAsync($"{ctx.Member.Mention}, este emoji não foi encontrado!");
                }
            }).Start();
        }

        [Command("status"), UBGE_Staff]

        public async Task BotStatusAsync(CommandContext ctx)
        {
            var watch = Stopwatch.StartNew();
            await ctx.TriggerTypingAsync();
            watch.Stop();

            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    Process p = Process.GetCurrentProcess();
                    ManagementObjectSearcher mos = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
                    ManagementObjectSearcher mram = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PhysicalMemory");

                    p.Refresh();

                    embed.WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithAuthor("Status do Bot:", null, Program.ubgeBot.discordClient.CurrentUser.AvatarUrl)
                        .AddField("Status Gerais:", $"**Ping:** {ctx.Client.Ping.ToString()}ms\n" +
                        $"**Rest:** {watch.ElapsedMilliseconds}ms\n" +
                        $"**PID:** {p.Id}\n" +
                        $"**Prioridade do processo:** {p.PriorityClass.ToString()}\n" +
                        $"**Threads do bot:** {p.Threads.Count.ToString()}\n" +
                        $"**O processo está respondendo?:** {(p.Responding ? "Sim" : "Não")}\n" +
                        $"**Nome do processo:** {p.ProcessName}.exe\n" +
                        $"**Interação com o usuário?:** {(Environment.UserInteractive ? "Sim" : "Não")}\n" +
                        $"**Uptime (Ligado desde quando):** {p.StartTime.ToString()}\n" +
                        $"**Dia e hora neste computador:** {DateTime.Now.ToString()}\n" +
                        $"**Versão:** {Valores.versao_Bot}")
                        .AddField("Especificações do computador onde estou hospedado:", $"**Nome:** {Environment.UserName}\n" +
                        $"**Nome do computador:** {Environment.MachineName}\n" +
                        $"**Versão do windows:** {Environment.OSVersion.VersionString} - ({NomeDoSistemaOperacional()})\n" +
                        $"**Sistema operacional de 64 bits?:** {(Environment.Is64BitOperatingSystem ? "Sim" : "Não").ToString()}\n" +
                        $"**Processador:** {NomeDoProcessador(mos)}\n" +
                        $"**Número de Núcleos:** {Environment.ProcessorCount.ToString()}\n" +
                        $"**Este é um processo 64 bits?:** {(Environment.Is64BitProcess ? "Sim" : "Não")}\n" +
                        $"**Memória ram do computador:** {MemoriaRamDoPC(mram)}\n")
                        .AddField("Biblioteca(s):", $"**Versão do DSharpPlus:** {ctx.Client.VersionString}\n")
                        .WithThumbnailUrl(Valores.csharpLogo)
                        .WithTimestamp(DateTime.Now)
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed());

                    await ctx.RespondAsync(embed: embed.Build());
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        private string MemoriaRamDoPC(ManagementObjectSearcher managementObjectSearcher)
        {
            ulong memoriaRam = 0;

            foreach (var objeto in managementObjectSearcher.Get())
                memoriaRam += ulong.Parse(objeto.Properties["Capacity"].Value.ToString());

            return $"{memoriaRam / 1024 / 1024 / 1024}gb ou {memoriaRam / 1024 / 1024}mb";
        }

        private string NomeDoProcessador(ManagementObjectSearcher managementObjectSearcher)
        {
            foreach (var objeto in managementObjectSearcher.Get())
                return objeto["Name"].ToString();

            return null;
        }

        private string NomeDoSistemaOperacional()
        {
            return Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion").GetValue("ProductName").ToString();
        }

        [Command("reiniciar"), Aliases("reboot", "restart"), RequireOwner]

        public async Task ReiniciarAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    await ctx.RespondAsync($"Já se foi o disco voadooooorrr");

                    string caminhoBot = Directory.GetCurrentDirectory() + @"\UBGE-Bot.exe";

                    Process.Start(caminhoBot);

                    Environment.Exit(1);
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("shutdown"), Aliases("off", "desligar"), RequireOwner]

        public async Task DesligarAsync(CommandContext ctx, string tempo = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(tempo))
                    {
                        await ctx.RespondAsync($"Adiós Muchacho! :wave:");

                        Environment.Exit(1);
                    }
                    else
                    {

                        var tempoConvert = Program.ubgeBot.utilidadesGerais.ConverterTempo(tempo);

                        await ctx.Client.DisconnectAsync();

                        await Task.Delay(tempoConvert);

                        await ctx.Client.ConnectAsync();

                        await ctx.RespondAsync($"I'm back!");
                    }
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("listarservidores"), Aliases("listarservidor"), UBGE_Staff]

        public async Task ListarServidorAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {

                    StringBuilder strServidores = new StringBuilder();

                    foreach (var guilds in ctx.Client.Guilds.Values)
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
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }
    }

    [Group("mongo"), Aliases("mongodb", "mtk"), RequireOwner]

    public sealed class MongoStaffControlled : BaseCommandModule
    {
        [Command("export"), Aliases("e", "exportar")]

        public async Task ExportarAsync(CommandContext ctx, [RemainingText] string colecao = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
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

                    string Comando = $"/c mongoexport --db local --collection {colecao} --out \"{Directory.GetCurrentDirectory()}\\Exports\\{colecao}.json\"";

                    Process.Start("cmd.exe", Comando).WaitForExit();

                    FileStream Arquivo = new FileStream($@"{Directory.GetCurrentDirectory()}\Exports\{colecao}.json", FileMode.Open);

                    builder.WithAuthor($"Coleção exportada com sucesso de local.{colecao} para {colecao}.json", null, Valores.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithDescription(await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "UBGE"))
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                        .WithThumbnailUrl(ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    await ctx.RespondAsync(embed: builder.Build());
                    await ctx.RespondWithFileAsync(Arquivo);

                    Arquivo.Close();
                    Arquivo.Dispose();
                    File.Delete($@"{Directory.GetCurrentDirectory()}\Exports\{colecao}.json");
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("import"), Aliases("i", "importar")]

        public async Task ImportarAsync(CommandContext ctx, [RemainingText] string colecao = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
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
                            using (WebClient client = new WebClient())
                                client.DownloadFile(ctx.Message.Attachments.ToList().FirstOrDefault().Url, $@"{Directory.GetCurrentDirectory()}\Exports\{colecao}.json");

                            string command = $"/c mongoimport --db local --collection {colecao} --file \"{Directory.GetCurrentDirectory()}\\Exports\\{colecao}.json\"";
                            Process.Start("cmd.exe", command).WaitForExit();

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
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("createcollection"), Aliases("cc", "criarcolecao", "criarcoleção")]

        public async Task CriarColecaoAsync(CommandContext ctx, [RemainingText] string collection = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
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

                    var local = Program.ubgeBot.localDB;
                    await local.CreateCollectionAsync(collection);

                    builder.WithAuthor($"Coleção criada com sucesso em: local.{collection}", null, Valores.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithDescription(await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "UBGE"))
                        .WithTimestamp(DateTime.Now)
                        .WithThumbnailUrl(ctx.Member.AvatarUrl)
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    await ctx.RespondAsync(embed: builder.Build());
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("dropcollection"), Aliases("dc", "droparcolecao", "droparcoleção", "apagarcolecao", "apagarcoleção")]

        public async Task ExcluirColecaoAsync(CommandContext ctx, [RemainingText] string collection = null)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
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

                    var local = Program.ubgeBot.localDB;
                    await local.DropCollectionAsync(collection);

                    builder.WithAuthor($"Coleção excluída com sucesso em: local.{collection}", null, Valores.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithDescription(await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "UBGE"))
                        .WithThumbnailUrl(ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now)
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl);

                    await ctx.RespondAsync(embed: builder.Build());
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }

        [Command("viewcollections"), Aliases("vercolecoes", "vercoleções", "vc")]

        public async Task VerColecoesAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            new Thread(async () =>
            {
                try
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                    var local = Program.ubgeBot.localDB;

                    var listas = await (await local.ListCollectionNamesAsync()).ToListAsync();

                    if (listas.Count() == 0)
                    {
                        embed.WithAuthor("Nenhuma coleção disponível!", null, Valores.logoUBGE)
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

                    foreach (var nome in listas)
                        str.Append($"`{++index}.` - **{nome}**\n");

                    embed.WithAuthor("Coleções atuais:", null, Valores.logoUBGE)
                        .WithColor(Program.ubgeBot.utilidadesGerais.CorAleatoriaEmbed())
                        .WithDescription(await Program.ubgeBot.utilidadesGerais.ProcuraEmoji(ctx, "UBGE"))
                        .WithDescription(str.ToString())
                        .WithFooter($"Comando requisitado pelo: {Program.ubgeBot.utilidadesGerais.RetornaNomeDiscord(ctx.Member)}", iconUrl: ctx.Member.AvatarUrl)
                        .WithTimestamp(DateTime.Now);

                    await ctx.RespondAsync(embed: embed.Build());
                }
                catch (Exception exception)
                {
                    await Program.ubgeBot.logExceptionsToDiscord.Error(LogExceptionsToDiscord.TipoErro.Comandos, exception);
                }
            }).Start();
        }
    }
}