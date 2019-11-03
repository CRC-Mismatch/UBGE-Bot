using System.Reflection;

namespace UBGEBot.Utilidades
{
    public class Valores
    {
        public static string versao_Bot = Assembly.GetEntryAssembly().GetName().Version.ToString();
        public const string csharpLogo = "https://cdn.discordapp.com/attachments/478612177511645212/526127458064400446/csharp-logo-58C6C6F67A-seeklogo.com.png";
        public const string infoLogo = "https://i.imgur.com/qUYvjKw.png";
        public const string logoUBGE = "https://cdn.discordapp.com/attachments/443159405991821323/468136624736174080/Logo_UBGE_2.png";
        public const string prLogoNucleo = "https://cdn.discordapp.com/attachments/443159405991821323/468136534810558474/Project_Reality_Dogtags_Logo_256.png";
        public const string prFotoThumb = "https://cdn.discordapp.com/attachments/525779964537339926/525780103112687636/UBGE_PR_2.png";
        public const string conanExilesLogo = "https://cdn.discordapp.com/attachments/438402141132947456/525764200770043907/conan-exiles.png";
        public const string dayZLogo = "https://cdn.discordapp.com/attachments/438402141132947456/530114774063644703/DayZ.png";
        public const string openSpadesLogo = "https://cdn.discordapp.com/attachments/443159405991821323/471879195685814273/images.png";
        public const string conanExilesLogoRuinasdeAstapor = "https://cdn.discordapp.com/attachments/443912219252490251/539423063738023946/g2282.png";
        public const string notFoundImage = "https://cdn.discordapp.com/attachments/478612177511645212/544209246321901583/NotFoundImage.jpg";
        public const string homemCagando = "https://cdn.discordapp.com/attachments/443912219252490251/545709941554675735/cagando-315x400.png";
        public const string unturnedLogo = "https://cdn.discordapp.com/attachments/478612177511645212/546521401385943071/UnturnedLogo.png";
        public const string cbprLogo = "https://cdn.discordapp.com/icons/540545227283234816/f979bf3e081adb43d37d6e1eb02a4e87.jpg";
        public const string counterStrikeLogo = "https://images-wixmp-ed30a86b8c4ca887773594c2.wixmp.com/f/01470275-8572-4123-a35f-56864bed384f/d4b3gmu-189a36ee-6a2f-48a6-a81d-fb3043caaf8a.png?token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ1cm46YXBwOjdlMGQxODg5ODIyNjQzNzNhNWYwZDQxNWVhMGQyNmUwIiwiaXNzIjoidXJuOmFwcDo3ZTBkMTg4OTgyMjY0MzczYTVmMGQ0MTVlYTBkMjZlMCIsIm9iaiI6W1t7InBhdGgiOiJcL2ZcLzAxNDcwMjc1LTg1NzItNDEyMy1hMzVmLTU2ODY0YmVkMzg0ZlwvZDRiM2dtdS0xODlhMzZlZS02YTJmLTQ4YTYtYTgxZC1mYjMwNDNjYWFmOGEucG5nIn1dXSwiYXVkIjpbInVybjpzZXJ2aWNlOmZpbGUuZG93bmxvYWQiXX0.EXrlwugNa4TdxAqgBmiZEFAbNzB81Y7Ekqk9snia-IA";
        public const string mordhauLogo = "https://cdn.discordapp.com/attachments/478612177511645212/609195489300185096/Mordhau.jpg";
        public const string prefixoBot = "[UBGE-Bot]";

        public class ChatsUBGE
        {
            public const ulong canalLog = 468771851301290004;
            public const ulong canalReacts = 505797244767698945;
            public const ulong canalGeral = 194925640888221698;
            public const ulong selecioneSeusJogos = 537703993472974849;
            public const ulong centroDeReabilitacao = 328868853335588865;
            public const ulong canalRecomendacoesPromocoes = 358403275541970944;
            public const ulong canalSugestoes = 421353118475747328;
            public const ulong formularioAlerta = 514231229743235094;
            public const ulong crieSuaSalaAqui = 441032422822510602;
            public const ulong radio = 376851056027631619;
            public const ulong chatPRServidor = 550106467718398016;
            public const ulong comandosBot = 206808743986462720;
            public const ulong canalInstrucoesInformaticas = 558806468221206568;
            public const ulong testeDoBot = 541385509776392203;
            public const ulong albionInformacoes = 574264521259089960;
            public const ulong albionOffline = 562515753685745674;
            public const ulong crieSuaParty = 573331437340721153;
            public const ulong cliqueAqui = 594320738790408192;
            public const ulong cargosEFuncoes = 589600626590351441;
            public const ulong ubgeBot = 450327484953788452;
            public const ulong batePapoVozUBGE = 550459809279770627;

            public class Categorias { }
        }

        public class ChatsForadaUBGE
        {
            public const ulong canalConanSugestoes = 545259195675443220;
        }

        public class Mongo
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
            public const string modMail = "Reports";
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
        }

        public class Cargos
        {
            public const ulong cargoPrisioneiro = 286136346932936704;
            public const ulong cargoAcessoGeral = 524290376291581952;
            public const ulong cargoMembroRegistrado = 313354564570972162;
            public const ulong secretariaOpenSpades = 366756439013851137;
            public const ulong cargoDiretor = 194926715896725505;
            public const ulong cargoAdemir = 242421268530331659;
            public const ulong cargoAjudante = 497104545017626624;
            public const ulong moderadoresDeProjectReality = 517126254869086258;
            public const ulong secretariaUnturned = 457959780640882690;
            public const ulong cargoInformatica = 558803534020083719;
            public const ulong cargoSecretariaDeAlbion = 561730932135034920;
            public const ulong cargoSecretariaDeFoxhole = 316723010818277376;
            public const ulong cargoSecretariaDeHH = 550343559052525581;
            public const ulong cargoSecretariaDeHeroesGenerals = 560569662102831138;
            public const ulong albionMembroDoCla = 561735586621292597;
            public const ulong albionOnlineCargo = 560965259364663327;
            public const ulong albionConselheiroCargo = 561959244182716454;
            public const ulong albionRecrutadorCargo = 564223249920032788;
            public const ulong membrosCadastrados = 574233360793206784;
            public const ulong cargosBloqueados = 579088815050981446;
            public const ulong ubgeBotCargo = 536981327325429761;
            public const ulong botsMusicais = 497109547513675786;
            public const ulong bots = 497163990217261076;
        }

        public partial class Guilds
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