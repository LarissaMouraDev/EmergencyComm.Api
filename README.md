Larissa De Freitas Moura -RM555136 João Victor Rebello de Santis -RM555287 Guilherme Francisco Silva Pereira -RM557843

# Emergency Communication API 🚨

Uma API robusta desenvolvida em .NET 8 para comunicação de emergência em situações de crise, permitindo comunicação mesh entre dispositivos mesmo quando a infraestrutura tradicional está comprometida.

## 🎯 Sobre o Projeto

O **Emergency Communication API** é um sistema de comunicação descentralizada projetado para funcionar em cenários de emergência onde a infraestrutura de comunicação tradicional pode estar indisponível. O sistema utiliza comunicação mesh entre dispositivos móveis para garantir que informações críticas possam ser transmitidas mesmo em condições adversas.

### Problemas que Resolve

- **Comunicação em Desastres**: Permite comunicação quando torres de celular estão fora do ar
- **Coordenação de Recursos**: Facilita o compartilhamento de recursos de emergência
- **Localização de Perigos**: Sistema de alertas sobre zonas perigosas
- **Rede Mesh**: Comunicação ponto-a-ponto entre dispositivos próximos

## ✨ Funcionalidades

### 🔄 Gerenciamento de Dispositivos
- Registro automático de dispositivos na rede mesh
- Rastreamento de localização em tempo real
- Monitoramento de status de bateria e conectividade
- Detecção automática de dispositivos próximos

### 💬 Sistema de Mensagens
- Mensagens diretas entre dispositivos
- Broadcasts de emergência para múltiplos dispositivos
- Sistema de roteamento inteligente com hop-count
- Confirmação de entrega e leitura

### 🏠 Recursos Compartilhados
- Registro de abrigos temporários
- Pontos de distribuição de água e alimentos
- Recursos médicos disponíveis
- Sistema de busca por proximidade

### ⚠️ Zonas de Perigo
- Mapeamento de áreas perigosas
- Alertas automáticos por proximidade
- Sistema de verificação comunitária
- Instruções de evacuação

### 🔄 Comunicação em Tempo Real
- WebSocket com SignalR para updates instantâneos
- Heartbeat para manter conexões ativas
- Notificações push para eventos críticos

## 🛠️ Tecnologias Utilizadas

### Backend
- **.NET 8** - Framework principal
- **ASP.NET Core** - API Web
- **Entity Framework Core** - ORM
- **SQLite** - Banco de dados
- **SignalR** - Comunicação em tempo real
- **Serilog** - Sistema de logs
- **FluentValidation** - Validação de dados

### Ferramentas de Desenvolvimento
- **Docker** - Containerização
- **Swagger/OpenAPI** - Documentação da API
- **xUnit** - Framework de testes (configurado)

### Padrões e Práticas
- **Repository Pattern** via Entity Framework
- **Dependency Injection** nativo do .NET
- **Service Layer Pattern**
- **RESTful API Design**
- **Clean Architecture**

## 🏗️ Arquitetura

```
EmergencyComm.Api/
├── Controllers/           # Endpoints da API
│   ├── DevicesController.cs
│   ├── MessagesController.cs
│   ├── ResourcesController.cs
│   ├── DangerZonesController.cs
│   └── TestController.cs
├── Models/               # Modelos de dados
│   ├── Device.cs
│   ├── Message.cs
│   ├── Resource.cs
│   └── DangerZone.cs
├── Services/             # Lógica de negócio
│   ├── DeviceService.cs
│   ├── MessageService.cs
│   └── ResourceService.cs
├── DTOs/                 # Objetos de transferência
├── Data/                 # Contexto do banco
├── Hubs/                 # SignalR Hubs
└── Properties/           # Configurações
```

### Principais Componentes

1. **Controllers**: Exposição dos endpoints REST
2. **Services**: Implementação da lógica de negócio
3. **Models**: Entidades do domínio
4. **DTOs**: Contratos de entrada/saída da API
5. **Hubs**: Comunicação em tempo real via SignalR
6. **Data Context**: Acesso ao banco de dados

## 🚀 Instalação e Configuração

### Pré-requisitos

- .NET 8 SDK ou superior
- Git
- Docker (opcional)

### Instalação Local

1. **Clone o repositório**
```bash
git clone https://github.com/seu-usuario/emergency-comm-api.git
cd emergency-comm-api
```

2. **Restaure as dependências**
```bash
dotnet restore
```

3. **Configure o banco de dados**
```bash
dotnet ef database update
```

4. **Execute a aplicação**
```bash
dotnet run
```

5. **Acesse a documentação**
```
https://localhost:7259/swagger
```

### Configuração de Ambiente

#### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=emergency_comm.db"
  },
  "EmergencySettings": {
    "MaxMessageHops": 10,
    "DefaultBroadcastRadius": 10.0,
    "MessageRetentionDays": 30,
    "DeviceTimeoutHours": 24
  },
  "NetworkSettings": {
    "BluetoothRange": 0.1,
    "WiFiDirectRange": 0.2,
    "RadioRange": 50.0,
    "MeshNetworkEnabled": true
  }
}
```

## 📱 Como Usar

### 1. Registro de Dispositivo

```bash
curl -X POST "https://localhost:7259/api/devices/register" \
-H "Content-Type: application/json" \
-d '{
  "id": "device-001",
  "name": "Dispositivo de Emergência",
  "deviceType": "Mobile",
  "latitude": -23.5505,
  "longitude": -46.6333,
  "batteryLevel": 85
}'
```

### 2. Envio de Mensagem de Emergência

```bash
curl -X POST "https://localhost:7259/api/messages" \
-H "Content-Type: application/json" \
-d '{
  "senderId": "device-001",
  "content": "Preciso de ajuda médica urgente!",
  "isEmergency": true,
  "priority": "Critical",
  "latitude": -23.5505,
  "longitude": -46.6333
}'
```

### 3. Registro de Recurso

```bash
curl -X POST "https://localhost:7259/api/resources" \
-H "Content-Type: application/json" \
-d '{
  "name": "Abrigo Temporário - Escola",
  "type": "Shelter",
  "description": "Abrigo com capacidade para 200 pessoas",
  "latitude": -23.5525,
  "longitude": -46.6353,
  "providerId": "device-001",
  "capacity": 200,
  "priority": "High"
}'
```

### 4. Conectar via SignalR (JavaScript)

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/communicationHub?deviceId=device-001")
    .build();

// Conectar
await connection.start();

// Escutar mensagens de emergência
connection.on("EmergencyBroadcast", (message) => {
    console.log("Emergência recebida:", message);
});

// Enviar mensagem direta
await connection.invoke("SendDirectMessage", "device-002", "Olá!");
```

## 📊 Endpoints da API

### Dispositivos
- `GET /api/devices` - Lista todos os dispositivos
- `GET /api/devices/{id}` - Obtém dispositivo específico
- `POST /api/devices/register` - Registra novo dispositivo
- `PUT /api/devices/{id}/location` - Atualiza localização
- `PUT /api/devices/{id}/availability` - Atualiza disponibilidade
- `GET /api/devices/nearby` - Dispositivos próximos
- `DELETE /api/devices/{id}` - Remove dispositivo

### Mensagens
- `GET /api/messages` - Lista todas as mensagens
- `GET /api/messages/{id}` - Obtém mensagem específica
- `POST /api/messages` - Cria nova mensagem
- `PUT /api/messages/{id}/status` - Atualiza status
- `GET /api/messages/emergency` - Mensagens de emergência
- `GET /api/messages/device/{deviceId}` - Mensagens por dispositivo
- `DELETE /api/messages/{id}` - Exclui mensagem

### Recursos
- `GET /api/resources` - Lista todos os recursos
- `GET /api/resources/{id}` - Obtém recurso específico
- `POST /api/resources` - Cria novo recurso
- `PUT /api/resources/{id}` - Atualiza recurso
- `GET /api/resources/type/{type}` - Recursos por tipo
- `GET /api/resources/nearby` - Recursos próximos
- `GET /api/resources/search` - Busca recursos
- `DELETE /api/resources/{id}` - Exclui recurso

### Zonas de Perigo
- `GET /api/dangerzones` - Lista zonas de perigo
- `GET /api/dangerzones/{id}` - Obtém zona específica
- `POST /api/dangerzones` - Cria nova zona
- `PUT /api/dangerzones/{id}` - Atualiza zona
- `GET /api/dangerzones/nearby` - Zonas próximas
- `GET /api/dangerzones/severity/{level}` - Por severidade
- `DELETE /api/dangerzones/{id}` - Desativa zona

### SignalR Hub Events
- `DeviceOnline/Offline` - Status de dispositivos
- `EmergencyBroadcast` - Alertas de emergência
- `NewMessage` - Novas mensagens
- `HelpRequested` - Pedidos de ajuda
- `LocationUpdated` - Atualizações de localização

## 🧪 Testes

### Estrutura de Testes
```bash
EmergencyComm.Tests/
├── Unit/
│   ├── Services/
│   ├── Controllers/
│   └── Models/
├── Integration/
│   ├── Endpoints/
│   └── Database/
└── Fixtures/
```

### Executar Testes
```bash
# Todos os testes
dotnet test

# Testes específicos
dotnet test --filter "Category=Unit"
dotnet test --filter "ClassName=DeviceServiceTests"

# Com cobertura
dotnet test --collect:"XPlat Code Coverage"
```

### Exemplos de Testes

#### Teste Unitário - DeviceService
```csharp
[Fact]
public async Task RegisterDevice_NewDevice_ShouldCreateSuccessfully()
{
    // Arrange
    var registerDto = new RegisterDeviceDto
    {
        Id = "test-device",
        Name = "Test Device",
        DeviceType = "Mobile",
        Latitude = -23.5505,
        Longitude = -46.6333
    };

    // Act
    var result = await _deviceService.RegisterDeviceAsync(registerDto);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("test-device", result.Id);
    Assert.True(result.IsOnline);
}
```

#### Teste de Integração - API
```csharp
[Fact]
public async Task GetDevices_ShouldReturnOkWithDevices()
{
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/devices");

    // Assert
    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsStringAsync();
    var devices = JsonSerializer.Deserialize<List<Device>>(content);
    Assert.NotNull(devices);
}
```

## 🐳 Docker

### Dockerfile
O projeto inclui um Dockerfile otimizado para produção:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EmergencyComm.Api.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EmergencyComm.Api.dll"]
```

### Docker Compose
```yaml
version: '3.8'

services:
  emergency-api:
    build: .
    ports:
      - "8080:80"
      - "8081:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Data Source=/app/data/emergency_comm.db
    volumes:
      - emergency-data:/app/data
    restart: unless-stopped

volumes:
  emergency-data:
```

### Comandos Docker

```bash
# Build da imagem
docker build -t emergency-comm-api .

# Executar container
docker run -d -p 8080:80 --name emergency-api emergency-comm-api

# Com docker-compose
docker-compose up -d

# Logs
docker logs emergency-api

# Parar
docker-compose down
```

## 🔧 Configurações Avançadas

### Variáveis de Ambiente
```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Data Source=/app/data/emergency_comm.db
EmergencySettings__MaxMessageHops=15
NetworkSettings__MeshNetworkEnabled=true
Serilog__MinimumLevel__Default=Information
```

### Configuração de Logs
```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "./Logs/emergency-comm-.log",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

## 🚦 Monitoramento e Saúde

### Health Checks
```bash
# Verificar saúde da API
curl http://localhost:5265/api/test/health

# Resposta esperada:
{
  "status": "Healthy",
  "service": "Emergency Communication API",
  "version": "1.0.0"
}
```

### Métricas Importantes
- Dispositivos online
- Mensagens por segundo
- Taxa de entrega de mensagens
- Recursos disponíveis
- Zonas de perigo ativas

## 🔒 Segurança

### Considerações de Segurança
- Validação rigorosa de entrada
- Rate limiting para APIs críticas
- Sanitização de dados geográficos
- Logs de auditoria para ações críticas
- Criptografia de mensagens sensíveis (futuro)

## 📈 Performance

### Otimizações Implementadas
- Consultas otimizadas com Entity Framework
- Índices no banco de dados
- Cache em memória para dados frequentes
- Paginação automática em listas grandes
- Conexões assíncronas

## 🤝 Contribuição

### Como Contribuir

1. **Fork** o projeto
2. **Crie** uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. **Commit** suas mudanças (`git commit -m 'Add: nova funcionalidade'`)
4. **Push** para a branch (`git push origin feature/AmazingFeature`)
5. **Abra** um Pull Request

### Padrões de Commit
```
feat: nova funcionalidade
fix: correção de bug
docs: documentação
style: formatação
refactor: refatoração
test: testes
chore: tarefas gerais
```

### Desenvolvimento Local

1. Configure o ambiente de desenvolvimento
2. Execute os testes antes de commits
3. Mantenha cobertura de testes > 80%
4. Siga as convenções de código C#
5. Documente APIs com XML comments

## 📝 Licença

Este projeto está licenciado sob a Licença MIT - veja o arquivo [LICENSE](LICENSE) para detalhes.

## 🎯 Roadmap

### v1.1 (Próxima Release)
- [ ] Autenticação JWT
- [ ] Criptografia end-to-end
- [ ] Interface web para monitoramento
- [ ] Integração com GPS

### v1.2 (Futuro)
- [ ] Suporte a imagens/vídeos
- [ ] IA para otimização de rotas
- [ ] Integração com serviços de emergência
- [ ] App mobile nativo

---

**Emergency Communication API** - Conectando pessoas em momentos críticos 🚨
