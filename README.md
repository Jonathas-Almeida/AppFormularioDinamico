# App Formulário Dinâmico - .NET MAUI

![Image](https://github.com/user-attachments/assets/36e729d7-b025-49d8-8627-ff0306841fef)

![Image](https://github.com/user-attachments/assets/729fdae8-0410-48c1-a964-3d306a3de487)

![Image](https://github.com/user-attachments/assets/5209bdda-74e1-4e7f-a87c-59aa71adb524)

Este é um aplicativo exemplo feito em .NET MAUI que gera formulários dinâmicos a partir de um arquivo JSON de configuração. O objetivo é mostrar como criar telas flexíveis e interativas para coleta de dados em diferentes plataformas (Android, iOS e Windows).

## Funcionalidade

- Geração automática de formulários a partir de um arquivo JSON
- Suporte a campos de texto, dropdown, checkbox, foto e GPS
- Validação de campos obrigatórios

## Dependências

Este projeto utiliza as seguintes bibliotecas:

### .NET MAUI

*   **Microsoft.Maui.Controls**: Contém os controles de interface do usuário (Labels, Buttons, Entries, Pickers, CheckBoxes, etc.) e layouts (VerticalStackLayout, etc.). Essencial para a criação da interface do aplicativo.
*   **Microsoft.Maui.Controls.Shapes**: Fornece formas geométricas como RoundRectangle para estilizar elementos visuais.
*   **Microsoft.Maui.Media**: Permite acessar recursos de mídia, como a câmera e a galeria de fotos, através da classe `MediaPicker`.
*   **Microsoft.Maui.Devices.Sensors**: Oferece acesso a sensores do dispositivo, como o GPS, através das classes `Geolocation` e `GeolocationRequest`.
*   **Microsoft.Maui**: Biblioteca base do .NET MAUI, que fornece funcionalidades essenciais para o aplicativo.

### JSON

*   **System.Text.Json**: Biblioteca padrão do .NET para trabalhar com JSON. Ela é usada para converter strings JSON em objetos C# e vice-versa.
*   **System.Text.Json.Serialization**: Contém atributos e classes para personalizar a serialização e deserialização JSON.

### Outras

*   **System.Collections.Generic**: Fornece interfaces e classes para definir coleções genéricas, como `List<T>` e `Dictionary<TKey, TValue>`.
*   **System.IO**: Permite realizar operações de entrada e saída, como ler arquivos (usado para ler o arquivo `config.json`).
*   **System.Reflection**: Permite obter informações sobre tipos, métodos e campos em tempo de execução (usado para acessar recursos inseridos no assembly).
*   **System.Linq**: Fornece métodos de extensão para trabalhar com coleções, como `Any()` e `Select()`.

---

## Como rodar o projeto

### Pré-requisitos

- [.NET SDK 9.0 ou superior](https://dotnet.microsoft.com/pt-br/download/dotnet/9.0)
- [Visual Studio 2022 Community ](https://visualstudio.microsoft.com/downloads/)
- Android SDK (para Android)
- Xcode (para iOS)

### Passos

1. **Clone o repositório:**
    ```bash
    git clone https://github.com/Jonathas-Almeida/AppFormularioDinamico.git
    cd AppFormularioDinamico
    ```

2. **Restaure as dependências:**
    ```bash
    dotnet restore
    ```

3. **Compile o projeto:**
    ```bash
    dotnet build
    ```

4. **Execute o projeto:**
   - Pelo Visual Studio: abra o `.sln`, selecione a plataforma e clique em Executar.

    - Pela linha de comando:
      - **No Windows:**
        ```bash
        dotnet build -t:Run -f net9.0-windows10.0.19041.0
        ```
      - **No Android:**
        ```bash
        dotnet run -f net9.0-android
        ```

## Observação

O formulário é configurado pelo arquivo `config.json` na pasta `Resources/Raw/`.
Você pode editar esse arquivo para mudar os campos exibidos no app.

---
