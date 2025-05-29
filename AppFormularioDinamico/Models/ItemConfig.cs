using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AppFormularioDinamico.Models
{
    public class ItemConfig
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("ismandatory")]
        public bool? IsMandatory { get; set; }

        [JsonPropertyName("initialvalue")]
        public string InitialValue { get; set; }

        [JsonPropertyName("opcoes")]
        public List<string> Opcoes { get; set; }

        [JsonPropertyName("items")]
        public List<ItemConfig> Items { get; set; }

        [JsonPropertyName("itemnamemask")]
        public string ItemNameMask { get; set; }

        [JsonPropertyName("addclonebutton")]
        public string AddCloneButton { get; set; }
    }
}