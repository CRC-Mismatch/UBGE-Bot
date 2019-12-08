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
            public static string canalLog = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalLog;
            //public const ulong canalReacts = 505797244767698945;
            public static string canalSelecioneSeusCargos = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalSelecioneSeusCargos;
            public static string canalCentroDeReabilitacao = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalCentroDeReabilitacao;
            public static string canalFormularioAlerta = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalFormularioAlerta;
            public static string canalCrieSuaSalaAqui = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalCrieSuaSalaAqui;
            public static string canalPRServidor = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalPRServidor;
            public static string canalComandosBot = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalComandosBot;
            //public const ulong canalInstrucoesInformaticas = 558806468221206568;
            public static string canalTesteDoBot = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalTesteDoBot;
            public static string canalUBGEBot = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalBotUBGE;
            public static string canalCliqueAqui = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalCliqueAqui;
            public static string canalBatePapo = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalBatePapo;
            public static string canalListaSecretarias = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalListaSecretarias;
            public static string canalListaPioneiros = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalListaPioneiros;
            public static string canalOrganogramaECargosDoAlbion = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalOrganogramaECargosDoAlbion;
            public static string canalVotacoesConselho = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.canalVotacoesConselho;

            public sealed class Categorias 
            { 
                public static string categoriaCliqueAqui = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.categoriaCanalDeVozPersonalizado;
                public static string categoriaModMailBot = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.categoriaModMailBot;
            }
        }

        public sealed class Mongo
        {
            public const string local = "local";
            public const string reacts = "Reacts";
            public const string salas = "Salas";
            public const string users = "Users";
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
        }

        public sealed class Cargos
        {
            public static string cargoPrisioneiro = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoPrisioneiro;
            public static string cargoAcessoGeral = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoAcessoGeral;
            public static string cargoMembroRegistrado = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoMembroRegistrado;
            //public const ulong cargoInformatica = 558803534020083719;
            //public const ulong ubgeBotCargo = 536981327325429761;
            public static string cargosBotsMusicais = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoBotsMusicais;
            public static string cargoBots = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoBots;
            public static string cargoSecretarioLider = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoSecretarioLider;
            public static string cargoModeradorDiscord = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoModeradorDiscord;
            public static string cargoConselheiro = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoConselheiro;
            public static string cargoComiteComunitario = Program.ubgeBot.ubgeBotConfig.ubgeBotValoresConfig.cargoComiteComunitario;
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