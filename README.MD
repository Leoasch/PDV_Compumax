
# Iniciar o projeto
## Dependencias
``` bash
dotnet restore
```
## Backend
``` bash
dotnet run --project Pos.Api
```
## Frontend
``` bash
dotnet run --project Pos.Client
```

O frontend deve iniciar na porta local 5191 e o backend em 5199

O banco de dados em sql vai ser criado e seedado automaticamente

O frontend foi feito com o Blazor com o template para formato SPA

Para o visual do frontend, usei principalmente a biblioteca MudBlazor que possui componentes com um visual moderno

O backend usa algumas dependenciais para se comunicar e gerar o banco de dados SQL e é feito com .NET 10 como requisitado

O projeto falta algumas funções que um PDV real faltaria, visto que essa é uma aplicação mock. Ex.: Scan de código de barras

O sistema não tem um formato de registro padrão, ao invés disso, os usuários são criados e controlados pelos usuários admins.

Um usuário admin é criado por padrão com o login sendo:
nome: admin
senha: admin123

Toda a interface do site pode ser navegada apenas com as teclas do teclado com legendas e padrões (Esc sempre fecha modal, Tab pula para o próximo foco). 

Foi feito uso de IA nesse projeto para ajudar com as novas tecnologias aprendidas nesse projeto. A IA deu informações sobre boas práticas (Pos.Shared, Domain e Infrastructure), correções de grámatica e ajuda com alguns componentes mais complicados (problema com sistema de teclas de atalhos).