using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Management;
using UBGE_Bot.Main;
using UBGE_Bot.LogExceptions;
using UBGE_Bot.Utilidades;

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

        [Command("viewrole"), Aliases("vr", "vercargo"), RequireOwner, RequireBotPermissions(Permissions.ManageRoles)]

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
                        embed.WithColor(new DiscordColor(0x32363c))
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

        [Command("status"), UBGE]

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
                        $"**Versão do windows:** {Environment.OSVersion.VersionString} - ()\n" +
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

        private string NomeDoSistemaOperacional(ManagementObjectSearcher managementObjectSearcher)
        {
            foreach (var objeto in managementObjectSearcher.Get())
                return objeto["Name"].ToString();

            return "";
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

        [Command("listarservidores"), Aliases("listarservidor")]

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
}