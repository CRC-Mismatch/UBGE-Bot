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
        public string canalBotUBGE { get; set; }

        [JsonProperty("canalFormularioAlerta")]
        public string canalFormularioAlerta { get; set; }

        [JsonProperty("canalCrieSuaSalaAqui")]
        public string canalCrieSuaSalaAqui { get; set; }

        [JsonProperty("canalCentroDeReabilitacao")]
        public string canalCentroDeReabilitacao { get; set; }

        [JsonProperty("canalPRServidor")]
        public string canalPRServidor { get; set; }

        [JsonProperty("canalComandosBot")]
        public string canalComandosBot { get; set; }

        [JsonProperty("canalTesteDoBot")]
        public string canalTesteDoBot { get; set; }

        [JsonProperty("canalCliqueAqui")]
        public string canalCliqueAqui { get; set; }

        [JsonProperty("canalBatePapo")]
        public string canalBatePapo { get; set; }

        [JsonProperty("canalLog")]
        public string canalLog { get; set; }

        [JsonProperty("canalSelecioneSeusCargos")]
        public string canalSelecioneSeusCargos { get; set; }

        [JsonProperty("canalListaSecretarias")]
        public string canalListaSecretarias { get; set; }

        [JsonProperty("canalListaPioneiros")]
        public string canalListaPioneiros { get; set; }

        [JsonProperty("canalOrganogramaECargosDoAlbion")]
        public string canalOrganogramaECargosDoAlbion { get; set; }

        [JsonProperty("canalVotacoesConselho")]
        public string canalVotacoesConselho { get; set; }


        [JsonProperty("categoriaCanalDeVozPersonalizado")]
        public string categoriaCanalDeVozPersonalizado { get; set; }

        [JsonProperty("categoriaModMailBot")]
        public string categoriaModMailBot { get; set; }



        [JsonProperty("cargoAcessoGeral")]
        public string cargoAcessoGeral { get; set; }

        [JsonProperty("cargoMembroRegistrado")]
        public string cargoMembroRegistrado { get; set; }

        [JsonProperty("cargoPrisioneiro")]
        public string cargoPrisioneiro { get; set; }

        [JsonProperty("cargoBots")]
        public string cargoBots { get; set; }

        [JsonProperty("cargoSecretarioLider")]
        public string cargoSecretarioLider { get; set; }

        [JsonProperty("cargoBotsMusicais")]
        public string cargoBotsMusicais { get; set; }

        [JsonProperty("cargoModeradorDiscord")]
        public string cargoModeradorDiscord { get; set; }

        [JsonProperty("cargoConselheiro")]
        public string cargoConselheiro { get; set; }

        [JsonProperty("cargoComiteComunitario")]
        public string cargoComiteComunitario { get; set; }

        public UBGEBotValores Build()
        {
            var jsonConfig = JsonConvert.DeserializeObject<UBGEBotValores>(File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "JsonUBGE_Bot", "ValoresConfig.json")));

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
            };
        }
    }
}