using Newtonsoft.Json;
using System;
using System.IO;

namespace UBGE_Bot.UBGEBotConfig
{
    public sealed class UBGEBotValores
    {
        [JsonProperty("csharpLogo")]
        public string csharpLogo { get; set; }

        [JsonProperty("infoLogo")]
        public string infoLogo { get; set; }

        [JsonProperty("logoUBGE")]
        public string logoUBGE { get; set; }

        [JsonProperty("prLogoNucleo")]
        public string prLogoNucleo { get; set; }

        [JsonProperty("prFotoThumb")]
        public string prFotoThumb { get; set; }

        [JsonProperty("conanExilesLogo")]
        public string conanExilesLogo { get; set; }

        [JsonProperty("dayZLogo")]
        public string dayZLogo { get; set; }

        [JsonProperty("openSpadesLogo")]
        public string openSpadesLogo { get; set; }

        [JsonProperty("notFoundImagem")]
        public string notFoundImagem { get; set; }

        [JsonProperty("unturnedLogo")]
        public string unturnedLogo { get; set; }

        [JsonProperty("cbprLogo")]
        public string cbprLogo { get; set; }

        [JsonProperty("counterStrikeLogo")]
        public string counterStrikeLogo { get; set; }

        [JsonProperty("mordhauLogo")]
        public string mordhauLogo { get; set; }

        [JsonProperty("conanExilesRuinasDeAstaporLogo")]
        public string conanExilesLogoRuinasDeAstaporLogo { get; set; }

        

        [JsonProperty("canalBotUBGE")]
        public ulong canalBotUBGE { get; set; }

        [JsonProperty("canalFormularioAlerta")]
        public ulong canalFormularioAlerta { get; set; }

        [JsonProperty("canalCrieSuaSalaAqui")]
        public ulong canalCrieSuaSalaAqui { get; set; }

        [JsonProperty("canalCentroDeReabilitacao")]
        public ulong canalCentroDeReabilitacao { get; set; }

        [JsonProperty("canalPRServidor")]
        public ulong canalPRServidor { get; set; }

        [JsonProperty("canalComandosBot")]
        public ulong canalComandosBot { get; set; }

        [JsonProperty("canalTesteDoBot")]
        public ulong canalTesteDoBot { get; set; }

        [JsonProperty("canalCliqueAqui")]
        public ulong canalCliqueAqui { get; set; }

        [JsonProperty("canalBatePapo")]
        public ulong canalBatePapo { get; set; }

        [JsonProperty("canalLog")]
        public ulong canalLog { get; set; }

        [JsonProperty("canalSelecioneSeusCargos")]
        public ulong canalSelecioneSeusCargos { get; set; }

        [JsonProperty("canalListaSecretarias")]
        public ulong canalListaSecretarias { get; set; }

        [JsonProperty("canalListaPioneiros")]
        public ulong canalListaPioneiros { get; set; }

        [JsonProperty("canalOrganogramaECargosDoAlbion")]
        public ulong canalOrganogramaECargosDoAlbion { get; set; }

        [JsonProperty("canalVotacoesConselho")]
        public ulong canalVotacoesConselho { get; set; }

        [JsonProperty("canalDeVozCentroDeReabilitacao")]
        public ulong canalDeVozCentroDeReabilitacao { get; set; }

        [JsonProperty("canalRecomendacoesPromocoes")]
        public ulong canalRecomendacoesPromocoes { get; set; }



        [JsonProperty("categoriaCanalDeVozPersonalizado")]
        public ulong categoriaCanalDeVozPersonalizado { get; set; }

        [JsonProperty("categoriaModMailBot")]
        public ulong categoriaModMailBot { get; set; }

        [JsonProperty("categoriaUBGE")]
        public ulong categoriaUBGE { get; set; }

        [JsonProperty("categoriaConselhoComunitario")]
        public ulong categoriaConselhoComunitario { get; set; }



        [JsonProperty("cargoAcessoGeral")]
        public ulong cargoAcessoGeral { get; set; }

        [JsonProperty("cargoMembroRegistrado")]
        public ulong cargoMembroRegistrado { get; set; }

        [JsonProperty("cargoPrisioneiro")]
        public ulong cargoPrisioneiro { get; set; }

        [JsonProperty("cargoBots")]
        public ulong cargoBots { get; set; }

        [JsonProperty("cargoSecretarioLider")]
        public ulong cargoSecretarioLider { get; set; }

        [JsonProperty("cargoBotsMusicais")]
        public ulong cargoBotsMusicais { get; set; }

        [JsonProperty("cargoModeradorDiscord")]
        public ulong cargoModeradorDiscord { get; set; }

        [JsonProperty("cargoConselheiro")]
        public ulong cargoConselheiro { get; set; }

        [JsonProperty("cargoComiteComunitario")]
        public ulong cargoComiteComunitario { get; set; }

        [JsonProperty("cargoNitroBooster")]
        public ulong cargoNitroBooster { get; set; }

        [JsonProperty("cargoDoador")]
        public ulong cargoDoador { get; set; }

        [JsonProperty("cargoUBGEBot")]
        public ulong cargoUBGEBot { get; set; }

        [JsonProperty("cargoAdministradorDiscord")]
        public ulong cargoAdministradorDiscord { get; set; }

        public UBGEBotValores Build()
        {
            var jsonConfig = JsonConvert.DeserializeObject<UBGEBotValores>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\ValoresConfig.json"));

            return new UBGEBotValores
            {
                csharpLogo = jsonConfig.csharpLogo,
                infoLogo = jsonConfig.infoLogo,
                logoUBGE = jsonConfig.logoUBGE,
                prLogoNucleo = jsonConfig.prLogoNucleo,
                prFotoThumb = jsonConfig.prFotoThumb,
                conanExilesLogo = jsonConfig.conanExilesLogo,
                dayZLogo = jsonConfig.dayZLogo,
                openSpadesLogo = jsonConfig.openSpadesLogo,
                notFoundImagem = jsonConfig.notFoundImagem,
                unturnedLogo = jsonConfig.unturnedLogo,
                cbprLogo = jsonConfig.cbprLogo,
                counterStrikeLogo = jsonConfig.counterStrikeLogo,
                mordhauLogo = jsonConfig.mordhauLogo,
                canalBotUBGE = jsonConfig.canalBotUBGE,
                cargoAcessoGeral = jsonConfig.cargoAcessoGeral,
                canalFormularioAlerta = jsonConfig.canalFormularioAlerta,
                canalCrieSuaSalaAqui = jsonConfig.canalCrieSuaSalaAqui,
                canalCentroDeReabilitacao = jsonConfig.canalCentroDeReabilitacao,
                canalPRServidor = jsonConfig.canalPRServidor,
                canalComandosBot = jsonConfig.canalComandosBot,
                canalTesteDoBot = jsonConfig.canalTesteDoBot,
                canalCliqueAqui = jsonConfig.canalCliqueAqui,
                cargoMembroRegistrado = jsonConfig.cargoMembroRegistrado,
                cargoPrisioneiro = jsonConfig.cargoPrisioneiro,
                cargoBots = jsonConfig.cargoBots,
                canalBatePapo = jsonConfig.canalBatePapo,
                canalLog = jsonConfig.canalLog,
                conanExilesLogoRuinasDeAstaporLogo = jsonConfig.conanExilesLogoRuinasDeAstaporLogo,
                categoriaCanalDeVozPersonalizado = jsonConfig.categoriaCanalDeVozPersonalizado,
                cargoSecretarioLider = jsonConfig.cargoSecretarioLider,
                cargoBotsMusicais = jsonConfig.cargoBotsMusicais,
                cargoModeradorDiscord = jsonConfig.cargoModeradorDiscord,
                canalListaPioneiros = jsonConfig.canalListaPioneiros,
                canalListaSecretarias = jsonConfig.canalListaSecretarias,
                canalSelecioneSeusCargos = jsonConfig.canalSelecioneSeusCargos,
                canalOrganogramaECargosDoAlbion = jsonConfig.canalOrganogramaECargosDoAlbion,
                categoriaModMailBot = jsonConfig.categoriaModMailBot,
                canalVotacoesConselho = jsonConfig.canalVotacoesConselho,
                cargoConselheiro = jsonConfig.cargoConselheiro,
                cargoComiteComunitario = jsonConfig.cargoComiteComunitario,
                cargoNitroBooster = jsonConfig.cargoNitroBooster,
                cargoDoador = jsonConfig.cargoDoador,
                canalDeVozCentroDeReabilitacao = jsonConfig.canalDeVozCentroDeReabilitacao,
                canalRecomendacoesPromocoes = jsonConfig.canalRecomendacoesPromocoes,
                categoriaUBGE = jsonConfig.categoriaUBGE,
                categoriaConselhoComunitario = jsonConfig.categoriaConselhoComunitario,
                cargoUBGEBot = jsonConfig.cargoUBGEBot,
                cargoAdministradorDiscord = jsonConfig.cargoAdministradorDiscord,
            };
        }
    }
}