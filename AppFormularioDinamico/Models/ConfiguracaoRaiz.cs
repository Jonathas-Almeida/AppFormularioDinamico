using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AppFormularioDinamico.Models
{
    public class ConfiguracaoRaiz
    {
        [JsonPropertyName("items")]
        public List<ConfiguracaoItem> Itens { get; set; }
    }
}