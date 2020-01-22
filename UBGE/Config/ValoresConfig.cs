using Newtonsoft.Json;
using System;
using System.IO;

namespace UBGE.Config
{
    public sealed class ValoresConfig
    {
        [JsonProperty("csharpLogo")]
        public string CsharpLogo { get; set; }

        [JsonProperty("infoLogo")]
        public string InfoLogo { get; set; }

        [JsonProperty("logoUBGE")]
        public string LogoUBGE { get; set; }

        [JsonProperty("prLogoNucleo")]
        public string PRLogoSecretary { get; set; }

        [JsonProperty("prFotoThumb")]
        public string PRPhotoThumb { get; set; }

        [JsonProperty("conanExilesLogo")]
        public string ConanExilesLogo { get; set; }

        [JsonProperty("dayZLogo")]
        public string DayZLogo { get; set; }

        [JsonProperty("openSpadesLogo")]
        public string OpenSpadesLogo { get; set; }

        [JsonProperty("notFoundImagem")]
        public string NotFoundImage { get; set; }

        [JsonProperty("unturnedLogo")]
        public string UnturnedLogo { get; set; }

        [JsonProperty("cbprLogo")]
        public string CBPRLogo { get; set; }

        [JsonProperty("counterStrikeLogo")]
        public string CounterStrikeLogo { get; set; }

        [JsonProperty("mordhauLogo")]
        public string MordhauLogo { get; set; }

        [JsonProperty("conanExilesRuinasDeAstaporLogo")]
        public string ConanExilesLogoRuinasDeAstaporLogo { get; set; }



        [JsonProperty("canalBotUBGE")]
        public ulong ChannelBotUBGE { get; set; }

        [JsonProperty("canalFormularioAlerta")]
        public ulong ChannelFormularioAlerta { get; set; }

        [JsonProperty("canalCrieSuaSalaAqui")]
        public ulong ChannelCrieSuaSalaAqui { get; set; }

        [JsonProperty("canalCentroDeReabilitacao")]
        public ulong ChannelCentroDeReabilitacao { get; set; }

        [JsonProperty("canalPRServidor")]
        public ulong ChannelPRServidor { get; set; }

        [JsonProperty("canalComandosBot")]
        public ulong ChannelComandosBot { get; set; }

        [JsonProperty("canalTesteDoBot")]
        public ulong ChannelTesteDoBot { get; set; }

        [JsonProperty("canalCliqueAqui")]
        public ulong ChannelCliqueAqui { get; set; }

        [JsonProperty("canalBatePapo")]
        public ulong ChannelBatePapo { get; set; }

        [JsonProperty("canalLog")]
        public ulong ChannelLog { get; set; }

        [JsonProperty("canalSelecioneSeusCargos")]
        public ulong ChannelSelecioneSeusCargos { get; set; }

        [JsonProperty("canalListaSecretarias")]
        public ulong ChannelListaSecretarias { get; set; }

        [JsonProperty("canalListaPioneiros")]
        public ulong ChannelListaPioneiros { get; set; }

        [JsonProperty("canalOrganogramaECargosDoAlbion")]
        public ulong ChannelOrganogramaECargosDoAlbion { get; set; }

        [JsonProperty("canalAnunciosConselho")]
        public ulong ChannelAnunciosConselho { get; set; }

        [JsonProperty("canalDeVozCentroDeReabilitacao")]
        public ulong ChannelDeVozCentroDeReabilitacao { get; set; }

        [JsonProperty("canalRecomendacoesPromocoes")]
        public ulong ChannelRecomendacoesPromocoes { get; set; }

        [JsonProperty("canalModeracaoDiscord")]
        public ulong ChannelModeracaoDiscord { get; set; }



        [JsonProperty("categoriaCanalDeVozPersonalizado")]
        public ulong CategoryCanalDeVozPersonalizado { get; set; }

        [JsonProperty("categoriaModMailBot")]
        public ulong CategoryModMail { get; set; }

        [JsonProperty("categoriaUBGE")]
        public ulong CategoryUBGE { get; set; }

        [JsonProperty("categoriaConselhoComunitario")]
        public ulong CategoryConselhoComunitario { get; set; }

        [JsonProperty("categoriaMundoDaInformatica")]
        public ulong CategoryMundoDaInformatica { get; set; }

        [JsonProperty("categoriaPrisao")]
        public ulong CategoryPrision { get; set; }



        [JsonProperty("cargoVerificado")]
        public ulong RoleVerificado { get; set; }

        [JsonProperty("cargoMembroRegistrado")]
        public ulong RoleMembroRegistrado { get; set; }

        [JsonProperty("cargoPrisioneiro")]
        public ulong RolePrisioneiro { get; set; }

        [JsonProperty("cargoBots")]
        public ulong RoleBots { get; set; }

        [JsonProperty("cargoSecretarioLider")]
        public ulong RoleSecretarioLider { get; set; }

        [JsonProperty("cargoBotsMusicais")]
        public ulong RoleBotsMusicais { get; set; }

        [JsonProperty("cargoModeradorDiscord")]
        public ulong RoleModeradorDiscord { get; set; }

        [JsonProperty("cargoConselheiro")]
        public ulong RoleConselheiro { get; set; }

        [JsonProperty("cargoComiteComunitario")]
        public ulong RoleComiteComunitario { get; set; }

        [JsonProperty("cargoNitroBooster")]
        public ulong RoleNitroBooster { get; set; }

        [JsonProperty("cargoDoador")]
        public ulong RoleDoador { get; set; }

        [JsonProperty("cargoUBGEBot")]
        public ulong RoleUBGEBot { get; set; }

        [JsonProperty("cargoAdministradorDiscord")]
        public ulong RoleAdministradorDiscord { get; set; }



        [JsonProperty("iniciaSistemasQueDependemDoMongo")]
        public bool StartSystemsThatDependOnTheMongo { get; private set; }

        [JsonProperty("sistemaCrieSuaSalaAqui")]
        public bool SystemCreateYourRoomHere { get; private set; }

        [JsonProperty("sistemaMensagemCriada")]
        public bool SystemMessageCreated { get; private set; }

        [JsonProperty("sistemaReacaoAdicionada")]
        public bool SystemReactionAdded { get; private set; }

        [JsonProperty("sistemaReacaoRemovida")]
        public bool SystemReactionRemoved { get; private set; }

        public ValoresConfig Build()
        {
            var jsonConfig = JsonConvert.DeserializeObject<ValoresConfig>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\ValoresConfig.json"));

            return new ValoresConfig
            {
                CsharpLogo = jsonConfig.CsharpLogo,
                InfoLogo = jsonConfig.InfoLogo,
                LogoUBGE = jsonConfig.LogoUBGE,
                PRLogoSecretary = jsonConfig.PRLogoSecretary,
                PRPhotoThumb = jsonConfig.PRPhotoThumb,
                ConanExilesLogo = jsonConfig.ConanExilesLogo,
                DayZLogo = jsonConfig.DayZLogo,
                OpenSpadesLogo = jsonConfig.OpenSpadesLogo,
                NotFoundImage = jsonConfig.NotFoundImage,
                UnturnedLogo = jsonConfig.UnturnedLogo,
                CBPRLogo = jsonConfig.CBPRLogo,
                CounterStrikeLogo = jsonConfig.CounterStrikeLogo,
                MordhauLogo = jsonConfig.MordhauLogo,
                ChannelBotUBGE = jsonConfig.ChannelBotUBGE,
                RoleVerificado = jsonConfig.RoleVerificado,
                ChannelFormularioAlerta = jsonConfig.ChannelFormularioAlerta,
                ChannelCrieSuaSalaAqui = jsonConfig.ChannelCrieSuaSalaAqui,
                ChannelCentroDeReabilitacao = jsonConfig.ChannelCentroDeReabilitacao,
                ChannelPRServidor = jsonConfig.ChannelPRServidor,
                ChannelComandosBot = jsonConfig.ChannelComandosBot,
                ChannelTesteDoBot = jsonConfig.ChannelTesteDoBot,
                ChannelCliqueAqui = jsonConfig.ChannelCliqueAqui,
                RoleMembroRegistrado = jsonConfig.RoleMembroRegistrado,
                RolePrisioneiro = jsonConfig.RolePrisioneiro,
                RoleBots = jsonConfig.RoleBots,
                ChannelBatePapo = jsonConfig.ChannelBatePapo,
                ChannelLog = jsonConfig.ChannelLog,
                ConanExilesLogoRuinasDeAstaporLogo = jsonConfig.ConanExilesLogoRuinasDeAstaporLogo,
                CategoryCanalDeVozPersonalizado = jsonConfig.CategoryCanalDeVozPersonalizado,
                RoleSecretarioLider = jsonConfig.RoleSecretarioLider,
                RoleBotsMusicais = jsonConfig.RoleBotsMusicais,
                RoleModeradorDiscord = jsonConfig.RoleModeradorDiscord,
                ChannelListaPioneiros = jsonConfig.ChannelListaPioneiros,
                ChannelListaSecretarias = jsonConfig.ChannelListaSecretarias,
                ChannelSelecioneSeusCargos = jsonConfig.ChannelSelecioneSeusCargos,
                ChannelOrganogramaECargosDoAlbion = jsonConfig.ChannelOrganogramaECargosDoAlbion,
                CategoryModMail = jsonConfig.CategoryModMail,
                ChannelAnunciosConselho = jsonConfig.ChannelAnunciosConselho,
                RoleConselheiro = jsonConfig.RoleConselheiro,
                RoleComiteComunitario = jsonConfig.RoleComiteComunitario,
                RoleNitroBooster = jsonConfig.RoleNitroBooster,
                RoleDoador = jsonConfig.RoleDoador,
                ChannelDeVozCentroDeReabilitacao = jsonConfig.ChannelDeVozCentroDeReabilitacao,
                ChannelRecomendacoesPromocoes = jsonConfig.ChannelRecomendacoesPromocoes,
                CategoryUBGE = jsonConfig.CategoryUBGE,
                CategoryConselhoComunitario = jsonConfig.CategoryConselhoComunitario,
                RoleUBGEBot = jsonConfig.RoleUBGEBot,
                RoleAdministradorDiscord = jsonConfig.RoleAdministradorDiscord,
                ChannelModeracaoDiscord = jsonConfig.ChannelModeracaoDiscord,
                CategoryMundoDaInformatica = jsonConfig.CategoryMundoDaInformatica,
                CategoryPrision = jsonConfig.CategoryPrision,
                StartSystemsThatDependOnTheMongo = jsonConfig.StartSystemsThatDependOnTheMongo,
                SystemCreateYourRoomHere = jsonConfig.SystemCreateYourRoomHere,
                SystemMessageCreated = jsonConfig.SystemMessageCreated,
                SystemReactionAdded = jsonConfig.SystemReactionAdded,
                SystemReactionRemoved = jsonConfig.SystemReactionRemoved
            };
        }
    }
}