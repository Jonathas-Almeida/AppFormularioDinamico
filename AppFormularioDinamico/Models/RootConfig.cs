using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AppFormularioDinamico.Models
{
    public class RootConfig
    {
        [JsonPropertyName("items")]
        public List<ItemConfig> Items { get; set; }
    }
}