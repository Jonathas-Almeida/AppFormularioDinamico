using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Microsoft.Maui.Controls.Shapes;
using AppFormularioDinamico.Models; // Seu namespace onde estão as classes RootConfig e ItemConfig
using Microsoft.Maui.Media; // Adicionado para MediaPicker (câmera/galeria)
using Microsoft.Maui.Devices.Sensors; // Adicionado para Geolocation (GPS)
using System.Linq; // Necessário para .Any() e .Select()

namespace AppFormularioDinamico // MUITO IMPORTANTE: Substitua pelo NOME EXATO do seu projeto MAUI.
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            LoadFormConfig();
        }

        private RootConfig _formConfig;

        // Classe para rastrear o controle MAUI e sua configuração JSON
        public class FieldControl
        {
            public ItemConfig Config { get; set; }
            public View Control { get; set; } // O controle UI real
            public List<FieldControl> NestedControls { get; set; } = new List<FieldControl>(); // Para itens dentro de containers (recursivo)

            // Propriedades para armazenar dados de Photo/GPS
            public string PhotoPath { get; set; }
            public Location GpsLocation { get; set; }
        }

        // Lista para armazenar todos os controles dinâmicos criados
        private List<FieldControl> _dynamicControls = new List<FieldControl>();


        private async void LoadFormConfig()
        {
            try
            {
                var assembly = System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
                // ATENÇÃO: Verifique o nome do recurso incorporado. 
                // A estrutura comum é 'NomeDoProjeto.PastaDoRecurso.SubPastaDoRecurso.NomeDoArquivo.extensao'
                // Certifique-se de que o 'Build Action' do config.json esteja como 'MauiAsset' ou 'Embedded resource'
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
                        _formConfig = JsonSerializer.Deserialize<RootConfig>(jsonString);

                        if (_formConfig != null && _formConfig.Items != null)
                        {
                            var dynamicLayout = this.FindByName<VerticalStackLayout>("DynamicFormLayout");

                            if (dynamicLayout == null)
                            {
                                await DisplayAlert("Erro de Layout", "O VerticalStackLayout com o nome 'DynamicFormLayout' não foi encontrado no XAML. Verifique se 'x:Name=\"DynamicFormLayout\"' está correto.", "OK");
                                return;
                            }

                            _dynamicControls.Clear(); // Limpa controles antigos se o formulário for recarregado
                            // Inicia a construção do formulário a partir dos itens de nível superior
                            BuildDynamicFields(_formConfig.Items, dynamicLayout, _dynamicControls);
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

        // Método recursivo para construir os campos dinamicamente
        private void BuildDynamicFields(List<ItemConfig> itemsToBuild, VerticalStackLayout parentLayout, List<FieldControl> currentLevelControls)
        {
            foreach (var itemConfig in itemsToBuild)
            {
                // Se for uma tabpage, adiciona o título e um novo layout para seus itens
                if (itemConfig.Type == "tabpage")
                {
                    var tabTitleLabel = new Label
                    {
                        Text = itemConfig.Text,
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                        Margin = new Thickness(0, 10, 0, 5),
                        HorizontalOptions = LayoutOptions.Center
                    };
                    parentLayout.Children.Add(tabTitleLabel);

                    var contentLayoutForChildren = new VerticalStackLayout
                    {
                        Padding = new Thickness(10),
                        Spacing = 5,
                        Margin = new Thickness(0, 0, 0, 20) // Espaço após a aba
                    };
                    parentLayout.Children.Add(contentLayoutForChildren);

                    // Chama recursivamente para construir os campos dentro desta tabpage
                    if (itemConfig.Items != null && itemConfig.Items.Any())
                    {
                        BuildDynamicFields(itemConfig.Items, contentLayoutForChildren, currentLevelControls);
                    }
                    continue; // Pula para o próximo item, pois a tabpage já foi processada
                }

                // Cria o label para o campo (se não for tabpage)
                var fieldLabel = new Label
                {
                    Text = itemConfig.Text + (itemConfig.IsMandatory == true ? " *" : ""), // Adiciona '*' se for obrigatório
                    Margin = new Thickness(0, 5, 0, 0),
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Black
                };
                parentLayout.Children.Add(fieldLabel);

                View currentControl = null; // Para armazenar o controle UI que está sendo criado
                FieldControl mediaFieldControl = null; // Específico para Photo/GPS por causa do _dynamicControls

                // Cria o controle UI com base no tipo
                switch (itemConfig.Type)
                {
                    case "textbox":
                        var entry = new Entry { Placeholder = itemConfig.Text, Text = itemConfig.InitialValue, Keyboard = Keyboard.Text, Margin = new Thickness(0, 0, 0, 5) };
                        parentLayout.Children.Add(entry);
                        currentControl = entry;
                        break;

                    case "dropdown":
                        var picker = new Picker { Title = itemConfig.Text, Margin = new Thickness(0, 0, 0, 5) };
                        if (itemConfig.Opcoes != null) { foreach (var option in itemConfig.Opcoes) { picker.Items.Add(option); } }
                        // Define o valor inicial se houver e a opção existir
                        if (!string.IsNullOrEmpty(itemConfig.InitialValue) && picker.Items.Contains(itemConfig.InitialValue)) { picker.SelectedItem = itemConfig.InitialValue; }
                        parentLayout.Children.Add(picker);
                        currentControl = picker;
                        break;

                    case "checkbox":
                        var checkBox = new CheckBox { IsChecked = itemConfig.InitialValue?.ToLower() == "true", Margin = new Thickness(0, 0, 0, 5) };
                        parentLayout.Children.Add(checkBox);
                        currentControl = checkBox;
                        break;

                    case "multilinetextbox":
                        var editor = new Editor { Placeholder = itemConfig.Text, Text = itemConfig.InitialValue, AutoSize = EditorAutoSizeOption.TextChanges, Margin = new Thickness(0, 0, 0, 5) };
                        parentLayout.Children.Add(editor);
                        currentControl = editor;
                        break;

                    case "radiobuttonlist":
                        var radioGroupStack = new VerticalStackLayout { Spacing = 2, Margin = new Thickness(0, 0, 0, 5) };
                        // Itera pelas opções para criar RadioButtons
                        if (itemConfig.Opcoes != null && itemConfig.Opcoes.Any())
                        {
                            foreach (var option in itemConfig.Opcoes)
                            {
                                var rb = new RadioButton { Content = option, GroupName = itemConfig.Id.ToString() };
                                radioGroupStack.Children.Add(rb);
                                if (option == itemConfig.InitialValue) { rb.IsChecked = true; }
                            }
                        }
                        else
                        {
                            // Fallback ou opções de exemplo se 'opcoes' estiver vazio (não deve acontecer se o JSON estiver bem formado)
                            radioGroupStack.Children.Add(new RadioButton { Content = "Opção Padrão A", GroupName = itemConfig.Id.ToString() });
                            radioGroupStack.Children.Add(new RadioButton { Content = "Opção Padrão B", GroupName = itemConfig.Id.ToString() });
                        }
                        parentLayout.Children.Add(radioGroupStack);
                        currentControl = radioGroupStack;
                        break;

                    case "photo":
                        var photoStack = new VerticalStackLayout { Margin = new Thickness(0, 0, 0, 5) };
                        var photoButton = new Button
                        {
                            Text = $"Tirar Foto ou Escolher",
                            BackgroundColor = Colors.LightSteelBlue,
                            TextColor = Colors.Black
                        };
                        var photoPreviewImage = new Image { HeightRequest = 100, WidthRequest = 100, Aspect = Aspect.AspectFit, IsVisible = false, Margin = new Thickness(0, 5, 0, 0) };
                        var photoPathLabel = new Label { Text = "Nenhuma foto selecionada.", FontSize = 10, TextColor = Colors.Gray, Margin = new Thickness(0, 5, 0, 0) };

                        // Cria FieldControl aqui para Photo/GPS pois o evento Clicked precisa dele
                        mediaFieldControl = new FieldControl { Config = itemConfig, Control = photoButton };
                        _dynamicControls.Add(mediaFieldControl); // Adiciona ao rastreador principal

                        photoButton.Clicked += async (s, e) =>
                        {
                            try
                            {
                                // Tenta capturar foto com a câmera, se falhar, tenta pegar da galeria
                                var photo = await MediaPicker.CapturePhotoAsync();
                                if (photo == null)
                                {
                                    photo = await MediaPicker.PickPhotoAsync();
                                }

                                if (photo != null)
                                {
                                    mediaFieldControl.PhotoPath = photo.FullPath; // Armazena o caminho
                                    // CORREÇÃO: Especificar System.IO.Path
                                    photoPathLabel.Text = $"Foto: {System.IO.Path.GetFileName(photo.FullPath)}";
                                    using (var stream = await photo.OpenReadAsync())
                                    {
                                        photoPreviewImage.Source = ImageSource.FromStream(() => stream);
                                    }
                                    photoPreviewImage.IsVisible = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                await DisplayAlert("Erro na Foto", $"Não foi possível obter a foto: {ex.Message}", "OK");
                            }
                        };

                        photoStack.Children.Add(photoButton);
                        photoStack.Children.Add(photoPreviewImage);
                        photoStack.Children.Add(photoPathLabel);
                        parentLayout.Children.Add(photoStack);
                        currentControl = photoStack; // O stack inteiro é o controle
                        break;

                    case "gps":
                        var gpsStack = new VerticalStackLayout { Margin = new Thickness(0, 0, 0, 5) };
                        var gpsButton = new Button
                        {
                            Text = $"Obter Coordenadas GPS",
                            BackgroundColor = Colors.LightSteelBlue,
                            TextColor = Colors.Black
                        };
                        var gpsLabel = new Label { Text = "Coordenadas: N/A", FontSize = 10, TextColor = Colors.Gray, Margin = new Thickness(0, 5, 0, 0) };

                        mediaFieldControl = new FieldControl { Config = itemConfig, Control = gpsButton };
                        _dynamicControls.Add(mediaFieldControl); // Adiciona ao rastreador principal

                        gpsButton.Clicked += async (s, e) =>
                        {
                            try
                            {
                                GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium);
                                Location location = await Geolocation.GetLocationAsync(request);

                                if (location != null)
                                {
                                    mediaFieldControl.GpsLocation = location; // Armazena a localização
                                    gpsLabel.Text = $"Lat: {location.Latitude}, Long: {location.Longitude}, Alt: {location.Altitude ?? 0}";
                                }
                                else
                                {
                                    gpsLabel.Text = "Coordenadas: Não disponível.";
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
                        gpsStack.Children.Add(gpsButton);
                        gpsStack.Children.Add(gpsLabel);
                        parentLayout.Children.Add(gpsStack);
                        currentControl = gpsStack; // O stack inteiro é o controle
                        break;

                    case "itemscontainermaster":
                        var containerBorder = new Border
                        {
                            Padding = 10,
                            Margin = new Thickness(0, 10, 0, 10),
                            StrokeShape = new RoundRectangle { CornerRadius = 5 },
                            StrokeThickness = 2,
                            Stroke = Colors.DarkSalmon,
                            BackgroundColor = Colors.LightSalmon,
                            Content = new VerticalStackLayout { Spacing = 5 } // Layout para os itens internos
                        };
                        parentLayout.Children.Add(containerBorder);

                        var nestedFieldsLayout = containerBorder.Content as VerticalStackLayout;

                        var containerTitleLabel = new Label
                        {
                            Text = itemConfig.Text,
                            FontSize = 16,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Colors.DarkRed,
                            Margin = new Thickness(0, 0, 0, 5)
                        };
                        nestedFieldsLayout.Children.Add(containerTitleLabel);

                        // Cria um FieldControl para o container mestre e o adiciona à lista principal
                        // Os controles aninhados serão adicionados a masterContainerFieldControl.NestedControls
                        FieldControl masterContainerFieldControl = new FieldControl { Config = itemConfig, Control = containerBorder };
                        currentLevelControls.Add(masterContainerFieldControl); // Adiciona ao rastreador do nível atual

                        // Adiciona o primeiro conjunto de itens aninhados
                        if (itemConfig.Items != null && itemConfig.Items.Any())
                        {
                            BuildDynamicFields(itemConfig.Items, nestedFieldsLayout, masterContainerFieldControl.NestedControls);
                        }

                        // Adiciona o botão de clonar se a configuração permitir
                        if (itemConfig.AddCloneButton?.ToLower() == "true")
                        {
                            var addCloneButton = new Button
                            {
                                Text = $"Adicionar outra {itemConfig.Text.ToLower().Replace("(", "").Replace(")", "").Replace("s", "")}", // Ex: "Adicionar outra luminária"
                                Margin = new Thickness(0, 10, 0, 0),
                                HorizontalOptions = LayoutOptions.End,
                                BackgroundColor = Colors.CadetBlue,
                                TextColor = Colors.White,
                                CornerRadius = 5
                            };
                            nestedFieldsLayout.Children.Add(addCloneButton);

                            // Lógica para clonar um conjunto de itens
                            addCloneButton.Clicked += (s, e) =>
                            {
                                // Cria um novo layout para o novo conjunto de campos clonados
                                var newNestedLayout = new VerticalStackLayout { Spacing = 5, Margin = new Thickness(0, 10, 0, 0) };
                                // Cria um novo FieldControl para a instância clonada do container
                                var newNestedContainerFieldControl = new FieldControl { Config = itemConfig, Control = newNestedLayout };
                                masterContainerFieldControl.NestedControls.Add(newNestedContainerFieldControl); // Adiciona a nova instância ao pai

                                // Recursivamente constrói os campos clonados dentro do novo layout
                                BuildDynamicFields(itemConfig.Items, newNestedLayout, newNestedContainerFieldControl.NestedControls);
                                // Insere o novo layout antes do botão "Adicionar" para que o botão fique sempre no final
                                nestedFieldsLayout.Children.Insert(nestedFieldsLayout.Children.IndexOf(addCloneButton), newNestedLayout);
                            };
                        }

                        currentControl = containerBorder;
                        break;

                    default:
                        var unknownTypeLabel = new Label
                        {
                            Text = $"Tipo '{itemConfig.Type}' não reconhecido (ID: {itemConfig.Id})",
                            TextColor = Colors.Red,
                            FontSize = 12,
                            Margin = new Thickness(0, 0, 0, 5)
                        };
                        parentLayout.Children.Add(unknownTypeLabel);
                        break;
                }

                // Adiciona o FieldControl à lista de controles do nível atual,
                // a menos que seja um tipo que já se adiciona (_dynamicControls para photo/gps, ou handled por itemscontainermaster)
                // Isso evita duplicação de rastreamento para Photo/GPS e lida com a estrutura recursiva do itemscontainermaster
                if (currentControl != null && itemConfig.Type != "photo" && itemConfig.Type != "gps" && itemConfig.Type != "itemscontainermaster")
                {
                    currentLevelControls.Add(new FieldControl { Config = itemConfig, Control = currentControl });
                }
            }
        }

        // Método para coletar dados do formulário e armazená-los em um dicionário
        // A chave é o ID do campo, o valor é o dado coletado
        private Dictionary<string, object> CollectFormData()
        {
            var formData = new Dictionary<string, object>();
            CollectDataRecursive(_dynamicControls, formData);
            return formData;
        }

        // Método auxiliar recursivo para coletar dados de todos os níveis de FieldControl
        private void CollectDataRecursive(List<FieldControl> controls, Dictionary<string, object> formData)
        {
            foreach (var fieldControl in controls)
            {
                string fieldKey = fieldControl.Config.Id.ToString();
                object currentValue = null;

                switch (fieldControl.Config.Type)
                {
                    case "textbox":
                    case "multilinetextbox":
                        if (fieldControl.Control is Entry entryControl)
                        {
                            currentValue = entryControl.Text;
                        }
                        else if (fieldControl.Control is Editor editorControl)
                        {
                            currentValue = editorControl.Text;
                        }
                        break;

                    case "dropdown":
                        if (fieldControl.Control is Picker pickerControl)
                        {
                            currentValue = pickerControl.SelectedItem;
                        }
                        break;

                    case "checkbox":
                        if (fieldControl.Control is CheckBox checkBoxControl)
                        {
                            currentValue = checkBoxControl.IsChecked;
                        }
                        break;

                    case "radiobuttonlist":
                        // O controle é um VerticalStackLayout
                        if (fieldControl.Control is VerticalStackLayout radioGroupStack)
                        {
                            foreach (var child in radioGroupStack.Children)
                            {
                                if (child is RadioButton radioButton && radioButton.IsChecked)
                                {
                                    currentValue = radioButton.Content?.ToString();
                                    break;
                                }
                            }
                        }
                        break;

                    case "photo":
                        currentValue = fieldControl.PhotoPath; // Pega o caminho da foto armazenado
                        break;

                    case "gps":
                        if (fieldControl.GpsLocation != null)
                        {
                            currentValue = $"{fieldControl.GpsLocation.Latitude},{fieldControl.GpsLocation.Longitude},{fieldControl.GpsLocation.Altitude}";
                        }
                        else
                        {
                            currentValue = "N/A";
                        }
                        break;

                    case "itemscontainermaster":
                        // Para containers, queremos coletar os dados dos seus filhos aninhados
                        // A chave do container pode ser associada a uma lista de dicionários (para múltiplas instâncias)
                        // ou você pode ter uma estratégia de nomeação diferente (ex: ID_instancia1, ID_instancia2)
                        var nestedDataList = new List<Dictionary<string, object>>();
                        foreach (var nestedControlInstance in fieldControl.NestedControls)
                        {
                            var instanceData = new Dictionary<string, object>();
                            // Chama recursivamente para coletar dados de cada instância aninhada
                            CollectDataRecursive(new List<FieldControl> { nestedControlInstance }, instanceData);
                            nestedDataList.Add(instanceData);
                        }
                        currentValue = nestedDataList;
                        break;
                }

                // Adiciona o valor coletado ao dicionário principal
                // Usamos o indexador [fieldKey] para adicionar ou atualizar, o que é mais seguro
                if (fieldKey != null && currentValue != null)
                {
                    formData[fieldKey] = currentValue;
                }
                else if (fieldKey != null && currentValue == null)
                {
                    formData[fieldKey] = "VAZIO"; // Garante que campos vazios sejam registrados
                }
            }
        }


        private async void OnSubmitButtonClicked(object sender, EventArgs e)
        {
            bool hasValidationErrors = false;
            string errorMessage = "Por favor, preencha os seguintes campos obrigatórios:\n\n";

            // Loop para validar campos obrigatórios
            // Esta validação é feita nos controles do nível principal, 
            // e os "itemscontainermaster" precisariam de uma validação recursiva adicional
            // se os seus sub-campos também puderem ser obrigatórios.
            foreach (var fieldControl in _dynamicControls)
            {
                // Verifica apenas campos que são obrigatórios
                if (fieldControl.Config.IsMandatory == true)
                {
                    switch (fieldControl.Config.Type)
                    {
                        case "textbox":
                        case "multilinetextbox":
                            // Valida Entry e Editor
                            if (fieldControl.Control is Entry entryControl && string.IsNullOrEmpty(entryControl.Text))
                            {
                                hasValidationErrors = true;
                                errorMessage += $"- {fieldControl.Config.Text}\n";
                            }
                            else if (fieldControl.Control is Editor editorControl && string.IsNullOrEmpty(editorControl.Text))
                            {
                                hasValidationErrors = true;
                                errorMessage += $"- {fieldControl.Config.Text}\n";
                            }
                            break;

                        case "dropdown":
                            // Valida Picker
                            if (fieldControl.Control is Picker pickerControl && pickerControl.SelectedItem == null)
                            {
                                hasValidationErrors = true;
                                errorMessage += $"- {fieldControl.Config.Text} (Selecione uma opção)\n";
                            }
                            break;

                        case "checkbox":
                            // Valida CheckBox (se precisa ser marcado)
                            if (fieldControl.Control is CheckBox checkBoxControl && !checkBoxControl.IsChecked)
                            {
                                hasValidationErrors = true;
                                errorMessage += $"- {fieldControl.Config.Text} (Marque a caixa)\n";
                            }
                            break;

                        case "radiobuttonlist":
                            // Valida RadioButtonList (se alguma opção foi selecionada)
                            if (fieldControl.Control is VerticalStackLayout radioGroupStack)
                            {
                                var anyRadioButtonChecked = false;
                                foreach (var child in radioGroupStack.Children)
                                {
                                    if (child is RadioButton radioButton && radioButton.IsChecked)
                                    {
                                        anyRadioButtonChecked = true;
                                        break;
                                    }
                                }
                                if (!anyRadioButtonChecked)
                                {
                                    hasValidationErrors = true;
                                    errorMessage += $"- {fieldControl.Config.Text} (Selecione uma opção)\n";
                                }
                            }
                            break;

                        case "photo":
                            // Valida campo de foto
                            if (string.IsNullOrEmpty(fieldControl.PhotoPath))
                            {
                                hasValidationErrors = true;
                                errorMessage += $"- {fieldControl.Config.Text} (Tire ou selecione uma foto)\n";
                            }
                            break;

                        case "gps":
                            // Valida campo GPS
                            if (fieldControl.GpsLocation == null)
                            {
                                hasValidationErrors = true;
                                errorMessage += $"- {fieldControl.Config.Text} (Obtenha as coordenadas GPS)\n";
                            }
                            break;

                        case "itemscontainermaster":
                            // Se os campos dentro de um itemscontainermaster também forem obrigatórios,
                            // você precisaria de uma lógica recursiva aqui para validar NestedControls.
                            // Por simplicidade, este exemplo foca na estrutura dinâmica e coleta.
                            break;
                    }
                }
            }

            // Exibe resultados da validação
            if (hasValidationErrors)
            {
                await DisplayAlert("Campos Obrigatórios", errorMessage, "OK");
            }
            else
            {
                await DisplayAlert("Sucesso!", "Todos os campos obrigatórios foram preenchidos.", "OK");

                var collectedData = CollectFormData(); // Coleta todos os dados

                Console.WriteLine("\n--- Dados Coletados do Formulário ---");
                foreach (var item in collectedData)
                {
                    // Lidar com o valor para exibição, especialmente se for uma lista de dicionários para containers
                    string displayValue;
                    if (item.Value is List<Dictionary<string, object>> nestedList)
                    {
                        displayValue = "[\n";
                        foreach (var nestedDict in nestedList)
                        {
                            displayValue += "  { " + string.Join(", ", nestedDict.Select(kv => $"{kv.Key}: {kv.Value}")) + " }\n";
                        }
                        displayValue += "]";
                    }
                    else
                    {
                        displayValue = item.Value?.ToString() ?? "NULO/VAZIO";
                    }
                    Console.WriteLine($"ID do Campo: {item.Key}, Valor: {displayValue}");
                }
                Console.WriteLine("-------------------------------------\n");

                await DisplayAlert("Dados Coletados", "Os dados do formulário foram coletados e exibidos no console de saída do Visual Studio.", "OK");
            }
        }
    }
}