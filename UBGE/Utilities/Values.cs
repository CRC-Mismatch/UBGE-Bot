namespace UBGE.Utilities
{
    public sealed class Values
    {
        public static string botVersion = Program.Bot.BOT_VERSION;

        public static string csharpLogo = Program.Bot.BotConfig.ValoresConfig.CsharpLogo;
        public static string infoLogo = Program.Bot.BotConfig.ValoresConfig.InfoLogo;
        public static string logoUBGE = Program.Bot.BotConfig.ValoresConfig.LogoUBGE;
        public static string prLogoSecretary = Program.Bot.BotConfig.ValoresConfig.PRLogoSecretary;
        public static string prPhotoThumbnail = Program.Bot.BotConfig.ValoresConfig.PRPhotoThumb;
        public static string conanExilesLogo = Program.Bot.BotConfig.ValoresConfig.ConanExilesLogo;
        public static string dayZLogo = Program.Bot.BotConfig.ValoresConfig.DayZLogo;
        public static string openSpadesLogo = Program.Bot.BotConfig.ValoresConfig.OpenSpadesLogo;
        public static string notFoundImage = Program.Bot.BotConfig.ValoresConfig.NotFoundImage;
        public static string unturnedLogo = Program.Bot.BotConfig.ValoresConfig.UnturnedLogo;
        public static string cbprLogo = Program.Bot.BotConfig.ValoresConfig.CBPRLogo;
        public static string counterStrikeLogo = Program.Bot.BotConfig.ValoresConfig.CounterStrikeLogo;
        public static string mordhauLogo = Program.Bot.BotConfig.ValoresConfig.MordhauLogo;
        public static string conanExilesLogoRuinasDeAstapor = Program.Bot.BotConfig.ValoresConfig.ConanExilesLogoRuinasDeAstaporLogo;

        public sealed class Chats
        {
            public static ulong channelLog = Program.Bot.BotConfig.ValoresConfig.ChannelLog;
            public static ulong channelSelecioneSeusCargos = Program.Bot.BotConfig.ValoresConfig.ChannelSelecioneSeusCargos;
            public static ulong channelCentroDeReabilitacao = Program.Bot.BotConfig.ValoresConfig.ChannelCentroDeReabilitacao;
            public static ulong channelFormularioAlerta = Program.Bot.BotConfig.ValoresConfig.ChannelFormularioAlerta;
            public static ulong channelCrieSuaSalaAqui = Program.Bot.BotConfig.ValoresConfig.ChannelCrieSuaSalaAqui;
            public static ulong channelPRServidor = Program.Bot.BotConfig.ValoresConfig.ChannelPRServidor;
            public static ulong channelComandosBot = Program.Bot.BotConfig.ValoresConfig.ChannelComandosBot;
            public static ulong channelTesteDoBot = Program.Bot.BotConfig.ValoresConfig.ChannelTesteDoBot;
            public static ulong channelUBGEBot = Program.Bot.BotConfig.ValoresConfig.ChannelBotUBGE;
            public static ulong channelCliqueAqui = Program.Bot.BotConfig.ValoresConfig.ChannelCliqueAqui;
            public static ulong channelBatePapo = Program.Bot.BotConfig.ValoresConfig.ChannelBatePapo;
            public static ulong channelListaSecretarias = Program.Bot.BotConfig.ValoresConfig.ChannelListaSecretarias;
            public static ulong channelListaPioneiros = Program.Bot.BotConfig.ValoresConfig.ChannelListaPioneiros;
            public static ulong channelOrganogramaECargosDoAlbion = Program.Bot.BotConfig.ValoresConfig.ChannelOrganogramaECargosDoAlbion;
            public static ulong channelAnunciosConselho = Program.Bot.BotConfig.ValoresConfig.ChannelAnunciosConselho;
            public static ulong channelDeVozCentroDeReabilitacao = Program.Bot.BotConfig.ValoresConfig.ChannelDeVozCentroDeReabilitacao;
            public static ulong channelRecomendacoesPromocoes = Program.Bot.BotConfig.ValoresConfig.ChannelRecomendacoesPromocoes;
            public static ulong channelModeracaoDiscord = Program.Bot.BotConfig.ValoresConfig.ChannelModeracaoDiscord;

            public sealed class Categories
            {
                public static ulong categoryCliqueAqui = Program.Bot.BotConfig.ValoresConfig.CategoryCanalDeVozPersonalizado;
                public static ulong categoryModMailBot = Program.Bot.BotConfig.ValoresConfig.CategoryModMail;
                public static ulong categoryUBGE = Program.Bot.BotConfig.ValoresConfig.CategoryUBGE;
                public static ulong categoryConselhoComunitario = Program.Bot.BotConfig.ValoresConfig.CategoryConselhoComunitario;
                public static ulong categoryMundoDaInformatica = Program.Bot.BotConfig.ValoresConfig.CategoryMundoDaInformatica;
                public static ulong categoryPrision = Program.Bot.BotConfig.ValoresConfig.CategoryPrision;
            }
        }

        public sealed class Mongo
        {
            public const string local = "local";
            public const string reacts = "Reacts";
            public const string salas = "Salas";
            public const string jogos = "Jogos";
            public const string servidoresUBGE = "ServidoresUBGE";
            public const string infracoes = "Infra";
            public const string prisioneiro = "Prisioneiro";
            public const string formGuard = "FormGuardMongo";
            public const string formDesban = "FormDesbanMongo";
            public const string censo = "Censo";
            public const string torneioConan = "TorneioConan";
            public const string mods = "Mods";
            public const string levels = "Levels";
            public const string tempoPrisao = "TempoPrisao";
            public const string eventosUBGE = "Eventos";
            public const string afk = "AFK_Membros";
            public const string partyAlbion = "PartyAlbion";
            public const string blackListReactRole = "BlackListReactRole";
            public const string contaMembrosQuePegaramCargos = "ContaMembrosQuePegaramCargos";
            public const string membrosQuePegaramOCargoDeMembroRegistrado = "MembrosQuePegaramOCargoDeMembroRegistrado";
            public const string checkBotAberto = "CheckBotAberto";
            public const string votacaoSecretarioLider = "VotacaoSecretarioLider";
            public const string modMail = "ModMail";
            public const string doador = "Doador";
            public const string reunion = "ReuniãoStaff";
        }

        public sealed class Roles
        {
            public static ulong rolePrisioneiro = Program.Bot.BotConfig.ValoresConfig.RolePrisioneiro;
            public static ulong roleVerificado = Program.Bot.BotConfig.ValoresConfig.RoleVerificado;
            public static ulong roleMembroRegistrado = Program.Bot.BotConfig.ValoresConfig.RoleMembroRegistrado;
            public static ulong roleBotsMusicais = Program.Bot.BotConfig.ValoresConfig.RoleBotsMusicais;
            public static ulong roleBots = Program.Bot.BotConfig.ValoresConfig.RoleBots;
            public static ulong roleSecretarioLider = Program.Bot.BotConfig.ValoresConfig.RoleSecretarioLider;
            public static ulong roleModeradorDiscord = Program.Bot.BotConfig.ValoresConfig.RoleModeradorDiscord;
            public static ulong roleConselheiro = Program.Bot.BotConfig.ValoresConfig.RoleConselheiro;
            public static ulong roleComiteComunitario = Program.Bot.BotConfig.ValoresConfig.RoleComiteComunitario;
            public static ulong roleNitroBooster = Program.Bot.BotConfig.ValoresConfig.RoleNitroBooster;
            public static ulong roleDoador = Program.Bot.BotConfig.ValoresConfig.RoleDoador;
            public static ulong roleUBGEBot = Program.Bot.BotConfig.ValoresConfig.RoleUBGEBot;
            public static ulong roleAdministradorDiscord = Program.Bot.BotConfig.ValoresConfig.RoleAdministradorDiscord;
        }

        public sealed class Guilds
        {
            public const ulong guildUBGE = 194925640888221698;
            public const ulong guildEmoji = 441027730398511119;
            public const ulong guildEmojiUBGERemoverCargo = 373527310789246976;
            public const ulong guildTestesDoLuiz = 443159405991821321;
            public const ulong guildCBPR = 540545227283234816;
            public const ulong guildEmojos = 541713470199169057;
            public const ulong guildRuinasDeAstapor = 451056177846026240;
            public const ulong guildUBGEAlbion = 572177050673217562;

            public class Members
            {
                public const ulong memberLuiz = 322745409074102282;
                public const ulong memberUBGEBot = 536705947712749576;
            }
        }
    }
}