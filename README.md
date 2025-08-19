# Chat em Tempo Real - Blazor + SignalR

Aplicação de chat em tempo real feita com Blazor Server e SignalR. Simula funcionalidades do WhatsApp com múltiplas salas e usuários online.

## O que é este projeto

Este projeto demonstra como criar aplicações web interativas com:
- Interface de chat similar ao WhatsApp
- Comunicação em tempo real entre usuários
- Múltiplas salas de chat
- Lista de usuários online
- Envio de mensagens instantâneo
- Design moderno e responsivo

## Tecnologias usadas

- **Blazor Server** - Framework para aplicações web interativas com C#
- **SignalR** - Para comunicação em tempo real
- **Bootstrap 5** - Framework CSS para interface moderna
- **Entity Framework Core** - Para salvar mensagens e usuários
- **SQLite** - Banco de dados

## Como usar

### Executar o projeto

```bash
cd WhatsAppClone
dotnet restore
dotnet run
```

### Acessar

- Aplicação: https://localhost:5031
- Chat: https://localhost:5031/chat

### Teste o chat

1. Abra a aplicação em duas abas do navegador
2. Faça login com usuários diferentes
3. Envie mensagens e veja atualizando em tempo real
4. Teste diferentes salas de chat

## O que a aplicação faz

**Página Inicial:**
- Apresentação do projeto
- Demonstração das tecnologias
- Preview animado do chat

**Sistema de Chat:**
- Login com nome de usuário
- Múltiplas salas (Geral, Tecnologia, Random)
- Lista de usuários online
- Envio de mensagens em tempo real
- Histórico de mensagens salvo

**Funcionalidades em Tempo Real:**
- Mensagens aparecem instantaneamente
- Notificação quando alguém entra/sai
- Lista de usuários atualizada automaticamente
- Indicadores visuais de atividade

## Estrutura do código

```
WhatsAppClone/
├── Components/
│   ├── Layout/          # Layout da aplicação
│   └── Pages/
│       ├── Home.razor   # Página inicial
│       └── Chat.razor   # Interface do chat
├── Hubs/               # SignalR Hubs
├── Models/             # Classes de dados
├── Data/               # Configuração do banco
└── Services/           # Lógica de negócio
```

## Como funciona o SignalR

### Hub de Chat
```csharp
public class ChatHub : Hub
{
    // Usuário entra na sala
    public async Task JoinRoom(string roomName)
    
    // Enviar mensagem para a sala
    public async Task SendMessage(string message, string roomName)
    
    // Usuário sai da sala
    public async Task LeaveRoom(string roomName)
}
```

### Frontend Blazor
```csharp
// Conectar ao hub
hubConnection = new HubConnectionBuilder()
    .WithUrl("/chatHub")
    .Build();

// Receber mensagens
hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
{
    // Atualizar interface
});
```

## Funcionalidades implementadas

**Chat Multi-Salas:**
- Sala Geral (para todos)
- Sala Tecnologia (discussões técnicas)
- Sala Random (conversas diversas)

**Gerenciamento de Usuários:**
- Login simples com nome
- Lista de usuários online por sala
- Notificações de entrada/saída

**Interface Responsiva:**
- Funciona bem no celular
- Design similar ao WhatsApp
- Cores e layout modernos

**Tempo Real:**
- Mensagens instantâneas
- Atualizações automáticas
- Sincronização entre abas

## Exemplos de uso

### Entrar no chat
1. Acesse /chat
2. Digite seu nome
3. Escolha uma sala
4. Comece a conversar

### Testar tempo real
1. Abra duas abas
2. Entre com nomes diferentes
3. Envie mensagens em uma aba
4. Veja aparecer na outra instantaneamente

## Observações

- Mensagens são salvas no banco de dados
- Usuários online são gerenciados em memória
- Interface inspirada no WhatsApp
- Funciona melhor com múltiplos usuários
- Demonstração das capacidades do Blazor

## Próximos passos

Para um chat real, adicionaria:
- Autenticação com login/senha
- Mensagens privadas entre usuários
- Upload de imagens e arquivos
- Emojis e reações
- Notificações push
- Criptografia das mensagens
- Grupos privados
- Status online/offline/ausente
├── wwwroot/
│   ├── app.css                    # Estilos personalizados
│   └── favicon.png                # Ícone da aplicação
├── Program.cs                     # Configuração da aplicação
└── BlazorChatSimple.csproj       # Arquivo de projeto
```

## Como Executar

### Pré-requisitos
- .NET 9 SDK
- IDE compatível (Visual Studio, VS Code, Rider)

### Executar a Aplicação
1. Clone o repositório
2. Navegue até a pasta do projeto:
   ```bash
   cd BlazorChatSimple
   ```
3. Execute a aplicação:
   ```bash
   dotnet run
   ```
4. Abra o navegador em `https://localhost:5001` ou `http://localhost:5000`

##  Recursos de Design

### Paleta de Cores
- **Primária**: Gradiente roxo-azul (#667eea → #764ba2)
- **Secundária**: Tons de cinza para neutralidade
- **Acentos**: Bootstrap colors para estados e feedback

### Componentes Visuais
- **Cards com Hover Effects** - Elevação e sombras dinâmicas
- **Botões com Gradientes** - Estados visuais aprimorados
- **Animações CSS** - Transições suaves e feedback visual
- **Icons Font Awesome** - Iconografia moderna e consistente

### Responsividade
- **Mobile First** - Design adaptativo para dispositivos móveis
- **Breakpoints Bootstrap** - Compatibilidade com diferentes telas
- **Flexbox Layout** - Distribuição eficiente de espaço

## 🔮 Evoluções Futuras

Este projeto serve como base para implementações mais avançadas:

### SignalR Real
- Implementação completa do SignalR Hub
- Comunicação bidirecional real entre clientes
- Grupos de chat persistentes

### Autenticação
- Sistema de login/registro
- Perfis de usuário personalizáveis
- Autorização por salas

### Persistência
- Entity Framework Core para dados
- Histórico de mensagens
- Configurações de usuário

### Funcionalidades Avançadas
- Upload de arquivos e imagens
- Mensagens privadas
- Notificações push
- Moderação de chat

##  Conceitos Demonstrados

### Blazor Server
- **Componentes reutilizáveis** - Estruturação modular
- **Binding bidirecional** - Sincronização automática UI/dados
- **Event handling** - Resposta a interações do usuário
- **State management** - Gerenciamento de estado da aplicação
- **Lifecycle hooks** - Controle do ciclo de vida dos componentes

### Padrões de UI
- **Component communication** - Comunicação entre componentes
- **Conditional rendering** - Renderização baseada em estado
- **List rendering** - Exibição dinâmica de coleções
- **Form handling** - Manipulação de formulários e validação

## 🤝 Contribuições

Este é um projeto de demonstração para fins educacionais e de portfólio. Sugestões e melhorias são sempre bem-vindas!

##  Licença

Este projeto está sob licença MIT - veja o arquivo LICENSE para detalhes.

---

**Desenvolvido com ❤ usando ASP.NET Blazor Server**

*Projeto criado para demonstrar as capacidades do Blazor Server na criação de aplicações web interativas e modernas.*
