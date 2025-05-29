# App Formulário Dinâmico - .NET MAUI

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
