using System.Reflection;
using UBGE_Bot.Main;

namespace UBGE_Bot.Utilidades
{
    public sealed class Valores
    {
        public static string versao_Bot = Program.ubgeBot.versaoBot;
        public static string prefixoBot = Program.ubgeBot.prefixoBotConsole;
        
        public static string csharpLogo = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.csharpLogo;
        public static string infoLogo = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.infoLogo;
        public static string logoUBGE = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.logoUBGE;
        public static string prLogoNucleo = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.prLogoNucleo;
        public static string prFotoThumb = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.prFotoThumb;
        public static string conanExilesLogo = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.conanExilesLogo;
        public static string dayZLogo = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.dayZLogo;
        public static string openSpadesLogo = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.openSpadesLogo;
        public static string notFoundImage = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.notFoundImagem;
        public static string unturnedLogo = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.unturnedLogo;
        public static string cbprLogo = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cbprLogo;
        public static string counterStrikeLogo = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.counterStrikeLogo;
        public static string mordhauLogo = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.mordhauLogo;
        public static string conanExilesLogoRuinasDeAstapor = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.conanExilesLogoRuinasDeAstaporLogo;

        public sealed class ChatsUBGE
        {
            public static ulong canalLog = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalLog;
            public static ulong canalSelecioneSeusCargos = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalSelecioneSeusCargos;
            public static ulong canalCentroDeReabilitacao = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalCentroDeReabilitacao;
            public static ulong canalFormularioAlerta = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalFormularioAlerta;
            public static ulong canalCrieSuaSalaAqui = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalCrieSuaSalaAqui;
            public static ulong canalPRServidor = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalPRServidor;
            public static ulong canalComandosBot = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalComandosBot;
            public static ulong canalTesteDoBot = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalTesteDoBot;
            public static ulong canalUBGEBot = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalBotUBGE;
            public static ulong canalCliqueAqui = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalCliqueAqui;
            public static ulong canalBatePapo = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalBatePapo;
            public static ulong canalListaSecretarias = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalListaSecretarias;
            public static ulong canalListaPioneiros = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalListaPioneiros;
            public static ulong canalOrganogramaECargosDoAlbion = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalOrganogramaECargosDoAlbion;
            public static ulong canalVotacoesConselho = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalVotacoesConselho;
            public static ulong canalDeVozCentroDeReabilitacao = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalDeVozCentroDeReabilitacao;
            public static ulong canalRecomendacoesPromocoes = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalRecomendacoesPromocoes;

            public sealed class Categorias 
            { 
                public static ulong categoriaCliqueAqui = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.categoriaCanalDeVozPersonalizado;
                public static ulong categoriaModMailBot = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.categoriaModMailBot;
                public static ulong categoriaUBGE = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.categoriaUBGE;
                public static ulong categoriaConselhoComunitario = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.categoriaConselhoComunitario;
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
        }

        public sealed class Cargos
        {
            public static ulong cargoPrisioneiro = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoPrisioneiro;
            public static ulong cargoAcessoGeral = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoAcessoGeral;
            public static ulong cargoMembroRegistrado = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoMembroRegistrado;
            public static ulong cargosBotsMusicais = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoBotsMusicais;
            public static ulong cargoBots = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoBots;
            public static ulong cargoSecretarioLider = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoSecretarioLider;
            public static ulong cargoModeradorDiscord = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoModeradorDiscord;
            public static ulong cargoConselheiro = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoConselheiro;
            public static ulong cargoComiteComunitario = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoComiteComunitario;
            public static ulong cargoNitroBooster = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoNitroBooster;
            public static ulong cargoDoador = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoDoador;
            public static ulong cargoUBGEBot = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoUBGEBot;
            public static ulong cargoAdministradorDiscord = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoAdministradorDiscord;
        }

        public sealed class Guilds
        {
            public const ulong UBGE = 194925640888221698;
            public const ulong emoji = 441027730398511119;
            public const ulong emojiUBGERemoverCargo = 373527310789246976;
            public const ulong testesDoLuiz = 443159405991821321;
            public const ulong CBPR = 540545227283234816;
            public const ulong emojos = 541713470199169057;
            public const ulong ruinasDeAstapor = 451056177846026240;
            public const ulong ubgeAlbion = 572177050673217562;

            public class Membros
            {
                public const ulong luiz = 322745409074102282;
                public const ulong ubgeBot = 536705947712749576;
            }
        }
    }
}