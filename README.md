# App Formulário Dinâmico - .NET MAUI

![Image](https://github.com/user-attachments/assets/36e729d7-b025-49d8-8627-ff0306841fef)

![Image](https://github.com/user-attachments/assets/729fdae8-0410-48c1-a964-3d306a3de487)

![Image](https://github.com/user-attachments/assets/5209bdda-74e1-4e7f-a87c-59aa71adb524)

Este é um aplicativo exemplo feito em .NET MAUI que gera formulários dinâmicos a partir de um arquivo JSON de configuração. O objetivo é mostrar como criar telas flexíveis e interativas para coleta de dados em diferentes plataformas (Android, iOS e Windows).

## Funcionalidade

- Geração automática de formulários a partir de um arquivo JSON
- Suporte a campos de texto, dropdown, checkbox, foto e GPS
- Validação de campos obrigatórios

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
