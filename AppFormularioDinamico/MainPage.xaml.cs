using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Microsoft.Maui.Controls.Shapes;
using AppFormularioDinamico.Models;
using Microsoft.Maui.Media;
using Microsoft.Maui.Devices.Sensors;
using System.Linq;

namespace AppFormularioDinamico
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            CarregarConfiguracaoFormulario();
        }

        private ConfiguracaoRaiz _configuracaoFormulario;

        public class ControleDeCampo
        {
            public ConfiguracaoItem Config { get; set; }
            public View Controle { get; set; }
            public List<ControleDeCampo> ControlesAninhados { get; set; } = new List<ControleDeCampo>();
            public string CaminhoFoto { get; set; }
            public Location LocalizacaoGps { get; set; }
        }

        private List<ControleDeCampo> _controlesDinamicos = new List<ControleDeCampo>();

        private async void CarregarConfiguracaoFormulario()
        {
            try
            {
                var assembly = System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
                string resourceName = "AppFormularioDinamico.Resources.Raw.config.json";

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        await DisplayAlert("Erro de Carregamento", $"O recurso '{resourceName}' não foi encontrado. Verifique o nome está correto e se o arquivo está configurado como 'Recurso Inserido'.", "OK");
                        return;
                    }

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string jsonString = await reader.ReadToEndAsync();
                        _configuracaoFormulario = JsonSerializer.Deserialize<ConfiguracaoRaiz>(jsonString);

                        if (_configuracaoFormulario != null && _configuracaoFormulario.Itens != null)
                        {
                            var layoutDinamico = this.FindByName<VerticalStackLayout>("DynamicFormLayout");

                            if (layoutDinamico == null)
                            {
                                await DisplayAlert("Erro de Layout", "O VerticalStackLayout com o nome 'DynamicFormLayout' não foi encontrado no XAML. Verifique se 'x:Name=\"DynamicFormLayout\"' está correto.", "OK");
                                return;
                            }

                            _controlesDinamicos.Clear();
                            ConstruirCamposDinamicos(_configuracaoFormulario.Itens, layoutDinamico, _controlesDinamicos);
                        }
                        else
                        {
                            await DisplayAlert("Atenção", "O JSON foi lido, mas a estrutura 'items' principal está vazia ou nula.", "OK");
                        }
                    }
                }
            }
            catch (JsonException jEx)
            {
                await DisplayAlert("Erro de Formato JSON", $"Ocorreu um erro ao ler o JSON: {jEx.Message}", "OK");
                Console.WriteLine($"ERRO JSON: {jEx}");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro Geral", $"Ocorreu um erro inesperado: {ex.Message}", "OK");
                Console.WriteLine($"ERRO GERAL: {ex}");
            }
        }

        private void ConstruirCamposDinamicos(List<ConfiguracaoItem> itemsParaConstruir, VerticalStackLayout layoutPai, List<ControleDeCampo> controlesNivelAtual)
        {
            foreach (var itemConfig in itemsParaConstruir)
            {
                if (itemConfig.Tipo == "tabpage")
                {
                    var rotuloTituloAba = new Label
                    {
                        Text = itemConfig.Texto,
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                        Margin = new Thickness(0, 10, 0, 5),
                        HorizontalOptions = LayoutOptions.Center
                    };
                    layoutPai.Children.Add(rotuloTituloAba);

                    var layoutConteudoFilhos = new VerticalStackLayout
                    {
                        Padding = new Thickness(10),
                        Spacing = 5,
                        Margin = new Thickness(0, 0, 0, 20)
                    };
                    layoutPai.Children.Add(layoutConteudoFilhos);

                    if (itemConfig.Itens != null && itemConfig.Itens.Any())
                    {
                        ConstruirCamposDinamicos(itemConfig.Itens, layoutConteudoFilhos, controlesNivelAtual);
                    }
                    continue;
                }

                var rotuloCampo = new Label
                {
                    Text = itemConfig.Texto + (itemConfig.EhObrigatorio == true ? " *" : ""),
                    Margin = new Thickness(0, 5, 0, 0),
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Black
                };
                layoutPai.Children.Add(rotuloCampo);

                View controleAtual = null;
                ControleDeCampo controleDeMidia = null;

                switch (itemConfig.Tipo)
                {
                    case "textbox":
                        var entradaTexto = new Entry { Placeholder = itemConfig.Texto, Text = itemConfig.ValorInicial, Keyboard = Keyboard.Text, Margin = new Thickness(0, 0, 0, 5) };
                        layoutPai.Children.Add(entradaTexto);
                        controleAtual = entradaTexto;
                        break;

                    case "dropdown":
                        var seletor = new Picker { Title = itemConfig.Texto, Margin = new Thickness(0, 0, 0, 5) };
                        if (itemConfig.Opcoes != null) { foreach (var option in itemConfig.Opcoes) { seletor.Items.Add(option); } }
                        if (!string.IsNullOrEmpty(itemConfig.ValorInicial) && seletor.Items.Contains(itemConfig.ValorInicial)) { seletor.SelectedItem = itemConfig.ValorInicial; }
                        layoutPai.Children.Add(seletor);
                        controleAtual = seletor;
                        break;

                    case "checkbox":
                        var caixaMarcacao = new CheckBox { IsChecked = itemConfig.ValorInicial?.ToLower() == "true", Margin = new Thickness(0, 0, 0, 5) };
                        layoutPai.Children.Add(caixaMarcacao);
                        controleAtual = caixaMarcacao;
                        break;

                    case "multilinetextbox":
                        var editorMultilinha = new Editor { Placeholder = itemConfig.Texto, Text = itemConfig.ValorInicial, AutoSize = EditorAutoSizeOption.TextChanges, Margin = new Thickness(0, 0, 0, 5) };
                        layoutPai.Children.Add(editorMultilinha);
                        controleAtual = editorMultilinha;
                        break;

                    case "radiobuttonlist":
                        var grupoRadio = new VerticalStackLayout { Spacing = 2, Margin = new Thickness(0, 0, 0, 5) };

                        if (itemConfig.Opcoes != null && itemConfig.Opcoes.Any())
                        {
                            foreach (var option in itemConfig.Opcoes)
                            {
                                var botaoRadio = new RadioButton { Content = option, GroupName = itemConfig.Id.ToString() };
                                grupoRadio.Children.Add(botaoRadio);
                                if (option == itemConfig.ValorInicial) { botaoRadio.IsChecked = true; }
                            }
                        }
                        else
                        {
                            grupoRadio.Children.Add(new RadioButton { Content = "Opção Padrão A", GroupName = itemConfig.Id.ToString() });
                            grupoRadio.Children.Add(new RadioButton { Content = "Opção Padrão B", GroupName = itemConfig.Id.ToString() });
                        }
                        layoutPai.Children.Add(grupoRadio);
                        controleAtual = grupoRadio;
                        break;

                    case "photo":
                        var pilhaFoto = new VerticalStackLayout { Margin = new Thickness(0, 0, 0, 5) };
                        var botaoFoto = new Button
                        {
                            Text = $"Tirar Foto ou Escolher",
                            BackgroundColor = Colors.LightSteelBlue,
                            TextColor = Colors.Black
                        };
                        var preVisualizacaoFoto = new Image { HeightRequest = 100, WidthRequest = 100, Aspect = Aspect.AspectFit, IsVisible = false, Margin = new Thickness(0, 5, 0, 0) };
                        var caminhoFotoRotulo = new Label { Text = "Nenhuma foto selecionada.", FontSize = 10, TextColor = Colors.Gray, Margin = new Thickness(0, 5, 0, 0) };

                        controleDeMidia = new ControleDeCampo { Config = itemConfig, Controle = botaoFoto };
                        _controlesDinamicos.Add(controleDeMidia);

                        botaoFoto.Clicked += async (s, e) =>
                        {
                            try
                            {
                                var photo = await MediaPicker.CapturePhotoAsync();
                                if (photo == null)
                                {
                                    photo = await MediaPicker.PickPhotoAsync();
                                }

                                if (photo != null)
                                {
                                    controleDeMidia.CaminhoFoto = photo.FullPath;
                                    caminhoFotoRotulo.Text = $"Foto: {System.IO.Path.GetFileName(photo.FullPath)}";
                                    using (var stream = await photo.OpenReadAsync())
                                    {
                                        preVisualizacaoFoto.Source = ImageSource.FromStream(() => stream);
                                    }
                                    preVisualizacaoFoto.IsVisible = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                await DisplayAlert("Erro na Foto", $"Não foi possível obter a foto: {ex.Message}", "OK");
                            }
                        };

                        pilhaFoto.Children.Add(botaoFoto);
                        pilhaFoto.Children.Add(preVisualizacaoFoto);
                        pilhaFoto.Children.Add(caminhoFotoRotulo);
                        layoutPai.Children.Add(pilhaFoto);
                        controleAtual = pilhaFoto;
                        break;

                    case "gps":
                        var pilhaGps = new VerticalStackLayout { Margin = new Thickness(0, 0, 0, 5) };
                        var botaoGps = new Button
                        {
                            Text = $"Obter Coordenadas GPS",
                            BackgroundColor = Colors.LightSteelBlue,
                            TextColor = Colors.Black
                        };
                        var rotuloGps = new Label { Text = "Coordenadas: N/A", FontSize = 10, TextColor = Colors.Gray, Margin = new Thickness(0, 5, 0, 0) };

                        controleDeMidia = new ControleDeCampo { Config = itemConfig, Controle = botaoGps };
                        _controlesDinamicos.Add(controleDeMidia);

                        botaoGps.Clicked += async (s, e) =>
                        {
                            try
                            {
                                GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium);
                                Location location = await Geolocation.GetLocationAsync(request);

                                if (location != null)
                                {
                                    controleDeMidia.LocalizacaoGps = location;
                                    rotuloGps.Text = $"Lat: {location.Latitude}, Long: {location.Longitude}, Alt: {location.Altitude ?? 0}";
                                }
                                else
                                {
                                    rotuloGps.Text = "Coordenadas: Não disponível.";
                                }
                            }
                            catch (FeatureNotSupportedException)
                            {
                                await DisplayAlert("Erro GPS", "GPS não suportado neste dispositivo.", "OK");
                            }
                            catch (FeatureNotEnabledException)
                            {
                                await DisplayAlert("Erro GPS", "GPS não habilitado. Por favor, habilite o GPS nas configurações do dispositivo.", "OK");
                            }
                            catch (PermissionException)
                            {
                                await DisplayAlert("Erro GPS", "Permissão de localização não concedida. Por favor, conceda a permissão nas configurações do aplicativo.", "OK");
                            }
                            catch (Exception ex)
                            {
                                await DisplayAlert("Erro GPS", $"Erro ao obter localização: {ex.Message}", "OK");
                            }
                        };
                        pilhaGps.Children.Add(botaoGps);
                        pilhaGps.Children.Add(rotuloGps);
                        layoutPai.Children.Add(pilhaGps);
                        controleAtual = pilhaGps;
                        break;

                    case "itemscontainermaster":
                        var bordaContainer = new Border
                        {
                            Padding = 10,
                            Margin = new Thickness(0, 10, 0, 10),
                            StrokeShape = new RoundRectangle { CornerRadius = 5 },
                            StrokeThickness = 2,
                            Stroke = Colors.DarkSalmon,
                            BackgroundColor = Colors.LightSalmon,
                            Content = new VerticalStackLayout { Spacing = 5 }
                        };
                        layoutPai.Children.Add(bordaContainer);

                        var layoutCamposAninhados = bordaContainer.Content as VerticalStackLayout;

                        var rotuloTituloContainer = new Label
                        {
                            Text = itemConfig.Texto,
                            FontSize = 16,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Colors.DarkRed,
                            Margin = new Thickness(0, 0, 0, 5)
                        };
                        layoutCamposAninhados.Children.Add(rotuloTituloContainer);

                        ControleDeCampo controleContainerMestre = new ControleDeCampo { Config = itemConfig, Controle = bordaContainer };
                        controlesNivelAtual.Add(controleContainerMestre);

                        if (itemConfig.Itens != null && itemConfig.Itens.Any())
                        {
                            ConstruirCamposDinamicos(itemConfig.Itens, layoutCamposAninhados, controleContainerMestre.ControlesAninhados);
                        }

                        if (itemConfig.AdicionarBotaoClone?.ToLower() == "true")
                        {
                            var botaoAdicionarClone = new Button
                            {
                                Text = $"Adicionar outra {itemConfig.Texto.ToLower().Replace("(", "").Replace(")", "").Replace("s", "")}",
                                Margin = new Thickness(0, 10, 0, 0),
                                HorizontalOptions = LayoutOptions.End,
                                BackgroundColor = Colors.CadetBlue,
                                TextColor = Colors.White,
                                CornerRadius = 5
                            };
                            layoutCamposAninhados.Children.Add(botaoAdicionarClone);

                            botaoAdicionarClone.Clicked += (s, e) =>
                            {
                                var novoLayoutAninhado = new VerticalStackLayout { Spacing = 5, Margin = new Thickness(0, 10, 0, 0) };
                                var novoControleContainerAninhado = new ControleDeCampo { Config = itemConfig, Controle = novoLayoutAninhado };
                                controleContainerMestre.ControlesAninhados.Add(novoControleContainerAninhado);

                                ConstruirCamposDinamicos(itemConfig.Itens, novoLayoutAninhado, novoControleContainerAninhado.ControlesAninhados);
                                layoutCamposAninhados.Children.Insert(layoutCamposAninhados.Children.IndexOf(botaoAdicionarClone), novoLayoutAninhado);
                            };
                        }

                        controleAtual = bordaContainer;
                        break;

                    default:
                        var rotuloTipoDesconhecido = new Label
                        {
                            Text = $"Tipo '{itemConfig.Tipo}' não reconhecido (ID: {itemConfig.Id})",
                            TextColor = Colors.Red,
                            FontSize = 12,
                            Margin = new Thickness(0, 0, 0, 5)
                        };
                        layoutPai.Children.Add(rotuloTipoDesconhecido);
                        break;
                }

                if (controleAtual != null && itemConfig.Tipo != "photo" && itemConfig.Tipo != "gps" && itemConfig.Tipo != "itemscontainermaster")
                {
                    controlesNivelAtual.Add(new ControleDeCampo { Config = itemConfig, Controle = controleAtual });
                }
            }
        }

        private Dictionary<string, object> ColetarDadosFormulario()
        {
            var dadosFormulario = new Dictionary<string, object>();
            ColetarDadosRecursivamente(_controlesDinamicos, dadosFormulario);
            return dadosFormulario;
        }

        private void ColetarDadosRecursivamente(List<ControleDeCampo> controles, Dictionary<string, object> dadosFormulario)
        {
            foreach (var controleDeCampo in controles)
            {
                string chaveCampo = controleDeCampo.Config.Id.ToString();
                object valorAtual = null;

                switch (controleDeCampo.Config.Tipo)
                {
                    case "textbox":
                    case "multilinetextbox":
                        if (controleDeCampo.Controle is Entry entradaTextoControle)
                        {
                            valorAtual = entradaTextoControle.Text;
                        }
                        else if (controleDeCampo.Controle is Editor editorControle)
                        {
                            valorAtual = editorControle.Text;
                        }
                        break;

                    case "dropdown":
                        if (controleDeCampo.Controle is Picker seletorControle)
                        {
                            valorAtual = seletorControle.SelectedItem;
                        }
                        break;

                    case "checkbox":
                        if (controleDeCampo.Controle is CheckBox caixaMarcacaoControle)
                        {
                            valorAtual = caixaMarcacaoControle.IsChecked;
                        }
                        break;

                    case "radiobuttonlist":
                        if (controleDeCampo.Controle is VerticalStackLayout grupoRadioControle)
                        {
                            foreach (var filho in grupoRadioControle.Children)
                            {
                                if (filho is RadioButton botaoRadio && botaoRadio.IsChecked)
                                {
                                    valorAtual = botaoRadio.Content?.ToString();
                                    break;
                                }
                            }
                        }
                        break;

                    case "photo":
                        valorAtual = controleDeCampo.CaminhoFoto;
                        break;

                    case "gps":
                        if (controleDeCampo.LocalizacaoGps != null)
                        {
                            valorAtual = $"{controleDeCampo.LocalizacaoGps.Latitude},{controleDeCampo.LocalizacaoGps.Longitude},{controleDeCampo.LocalizacaoGps.Altitude}";
                        }
                        else
                        {
                            valorAtual = "N/A";
                        }
                        break;

                    case "itemscontainermaster":
                        var listaDadosAninhados = new List<Dictionary<string, object>>();
                        foreach (var instanciaControleAninhado in controleDeCampo.ControlesAninhados)
                        {
                            var dadosInstancia = new Dictionary<string, object>();
                            ColetarDadosRecursivamente(new List<ControleDeCampo> { instanciaControleAninhado }, dadosInstancia);
                            listaDadosAninhados.Add(dadosInstancia);
                        }
                        valorAtual = listaDadosAninhados;
                        break;
                }

                if (chaveCampo != null && valorAtual != null)
                {
                    dadosFormulario[chaveCampo] = valorAtual;
                }
                else if (chaveCampo != null && valorAtual == null)
                {
                    dadosFormulario[chaveCampo] = "VAZIO";
                }
            }
        }

        private async void AoClicarNoBotaoEnviar(object sender, EventArgs e)
        {
            bool possuiErrosValidacao = false;
            string mensagemErro = "Por favor, preencha os seguintes campos obrigatórios:\n\n";

            foreach (var controleDeCampo in _controlesDinamicos)
            {
                if (controleDeCampo.Config.EhObrigatorio == true)
                {
                    switch (controleDeCampo.Config.Tipo)
                    {
                        case "textbox":
                        case "multilinetextbox":
                            if (controleDeCampo.Controle is Entry entradaTextoControle && string.IsNullOrEmpty(entradaTextoControle.Text))
                            {
                                possuiErrosValidacao = true;
                                mensagemErro += $"- {controleDeCampo.Config.Texto}\n";
                            }
                            else if (controleDeCampo.Controle is Editor editorControle && string.IsNullOrEmpty(editorControle.Text))
                            {
                                possuiErrosValidacao = true;
                                mensagemErro += $"- {controleDeCampo.Config.Texto}\n";
                            }
                            break;

                        case "dropdown":
                            if (controleDeCampo.Controle is Picker seletorControle && seletorControle.SelectedItem == null)
                            {
                                possuiErrosValidacao = true;
                                mensagemErro += $"- {controleDeCampo.Config.Texto} (Selecione uma opção)\n";
                            }
                            break;

                        case "checkbox":
                            if (controleDeCampo.Controle is CheckBox caixaMarcacaoControle && !caixaMarcacaoControle.IsChecked)
                            {
                                possuiErrosValidacao = true;
                                mensagemErro += $"- {controleDeCampo.Config.Texto} (Marque a caixa)\n";
                            }
                            break;

                        case "radiobuttonlist":
                            if (controleDeCampo.Controle is VerticalStackLayout grupoRadioControle)
                            {
                                var algumaOpcaoMarcada = false;
                                foreach (var filho in grupoRadioControle.Children)
                                {
                                    if (filho is RadioButton botaoRadio && botaoRadio.IsChecked)
                                    {
                                        algumaOpcaoMarcada = true;
                                        break;
                                    }
                                }
                                if (!algumaOpcaoMarcada)
                                {
                                    possuiErrosValidacao = true;
                                    mensagemErro += $"- {controleDeCampo.Config.Texto} (Selecione uma opção)\n";
                                }
                            }
                            break;

                        case "photo":
                            if (string.IsNullOrEmpty(controleDeCampo.CaminhoFoto))
                            {
                                possuiErrosValidacao = true;
                                mensagemErro += $"- {controleDeCampo.Config.Texto} (Tire ou selecione uma foto)\n";
                            }
                            break;

                        case "gps":
                            if (controleDeCampo.LocalizacaoGps == null)
                            {
                                possuiErrosValidacao = true;
                                mensagemErro += $"- {controleDeCampo.Config.Texto} (Obtenha as coordenadas GPS)\n";
                            }
                            break;

                        case "itemscontainermaster":
                            break;
                    }
                }
            }

            if (possuiErrosValidacao)
            {
                await DisplayAlert("Campos Obrigatórios", mensagemErro, "OK");
            }
            else
            {
                await DisplayAlert("Sucesso!", "Todos os campos obrigatórios foram preenchidos.", "OK");

                var dadosColetados = ColetarDadosFormulario();

                Console.WriteLine("\n--- Dados Coletados do Formulário ---");
                foreach (var item in dadosColetados)
                {
                    string valorExibicao;
                    if (item.Value is List<Dictionary<string, object>> listaAninhada)
                    {
                        valorExibicao = "[\n";
                        foreach (var dicionarioAninhado in listaAninhada)
                        {
                            valorExibicao += "  { " + string.Join(", ", dicionarioAninhado.Select(kv => $"{kv.Key}: {kv.Value}")) + " }\n";
                        }
                        valorExibicao += "]";
                    }
                    else
                    {
                        valorExibicao = item.Value?.ToString() ?? "NULO/VAZIO";
                    }
                    Console.WriteLine($"ID do Campo: {item.Key}, Valor: {valorExibicao}");
                }
                Console.WriteLine("-------------------------------------\n");

                await DisplayAlert("Dados Coletados", "Os dados do formulário foram coletados e exibidos no console de saída do Visual Studio.", "OK");
            }
        }
    }
}