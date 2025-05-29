using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AppFormularioDinamico.Models
{
    public class ConfiguracaoItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("type")]
        public string Tipo { get; set; }

        [JsonPropertyName("text")]
        public string Texto { get; set; }

        [JsonPropertyName("ismandatory")]
        public bool? EhObrigatorio { get; set; }

        [JsonPropertyName("initialvalue")]
        public string ValorInicial { get; set; }

        [JsonPropertyName("opcoes")]
        public List<string> Opcoes { get; set; }

        [JsonPropertyName("items")]
        public List<ConfiguracaoItem> Itens { get; set; }

        [JsonPropertyName("itemnamemask")]
        public string MascaraNomeItem { get; set; }

        [JsonPropertyName("addclonebutton")]
        public string AdicionarBotaoClone { get; set; }
    }
}