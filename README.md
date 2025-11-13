# GS2_MicroServices

Perfeito ✅
Abaixo está a **documentação completa do projeto GS2_MicroServices**, seguindo **exatamente o mesmo modelo e nível de detalhamento** do exemplo que você enviou (com ícones, seções bem estruturadas e foco técnico).

---

# 🚀 GS2 MicroServices

📚 **Documentação Técnica Completa**

---

## 📋 Visão Geral

O projeto **GS2_MicroServices** segue uma arquitetura em camadas bem definida, separando responsabilidades entre **Controllers**, **Services**, **Repositories** e **Domain Models**.
Essa estrutura visa garantir **baixo acoplamento**, **alta coesão** e **facilidade de manutenção**, além de suportar futuras expansões com outros microserviços.

### 🧩 Objetivo

Fornecer uma API modular capaz de realizar operações com **Prompts (IAs)**, utilizando:

* **MySQL** como banco de dados relacional.
* **Redis** para cache de consultas.
* **Dapper** como micro-ORM para execução de queries SQL.
* **ASP.NET Core Web API** como camada de exposição de endpoints RESTful.

---

## 🏗️ Arquitetura do Projeto

```
📁 GS2_MicroServices/
├── 📁 Controllers/
│   ├── 📄 IAController.cs                # Endpoint principal de IAs
│   └── 📄 WeatherForecastController.cs   # Exemplo gerado automaticamente
│
├── 📁 Domain/
│   ├── 📄 Prompt.cs                      # Modelo de domínio da IA
│   └── 📄 Domain.csproj
│
├── 📁 Repository/
│   ├── 📄 IPromptRepository.cs           # Interface do repositório
│   ├── 📄 PromptRepository.cs            # Implementação com Dapper/MySQL
│   └── 📄 Repository.csproj
│
├── 📁 Service/
│   ├── 📄 ICacheService.cs               # Interface do serviço de cache
│   ├── 📄 CacheService.cs                # Implementação do serviço Redis
│   ├── 📄 PromptService.cs               # Serviço de negócio para Prompts
│   └── 📄 Service.csproj
│
├── 📄 Program.cs                         # Configuração do pipeline e DI
├── 📄 appsettings.json                   # Configurações de banco e cache
├── 📄 GS2_MicroServices.csproj
└── 📄 README.md                          # Esta documentação
```

---

# ⚙️ Camadas da Aplicação

---

## 🧠 Domain Layer

### 📍 Visão Geral

A camada **Domain** define as **entidades** e **modelos de dados** utilizados pela aplicação.
É a base que representa o **modelo de negócio** e não deve conter regras de persistência ou lógica de aplicação.

### 📄 Classe: `Prompt`

```csharp
namespace Domain;

public class Prompt
{
    public int Id { get; set; }
    public string prompt { get; set; }
}
```

### 🧩 Responsabilidades

✅ Representar entidades do sistema.
✅ Garantir estrutura e tipagem dos dados.
✅ Servir como contrato entre camadas (Repository, Service e Controller).

---

## 🏦 Repository Layer

### 📍 Visão Geral

A camada **Repository** é responsável por toda a **persistência de dados**, conectando-se ao **MySQL** e utilizando o **Dapper** para consultas assíncronas.

### 🧱 Estrutura

```
📁 Repository/
├── 📄 IPromptRepository.cs
└── 📄 PromptRepository.cs
```

### 🧩 Interface: `IPromptRepository`

```csharp
using Domain;

namespace Repository
{
    public interface IPromptRepository
    {
        Task<IEnumerable<Prompt>> GetAllPromptsAsync();
        Task<Prompt> AddPromptAsync(Prompt prompt);
        Task UpdatePromptAsync(Prompt Prompt);
        Task DeletePromptAsync(int id);
    }
}
```

### 🛠️ Implementação: `PromptRepository`

```csharp
using Dapper;
using Domain;
using MySqlConnector;

namespace Repository
{
    public class PromptRepository : IPromptRepository
    {
        private readonly MySqlConnection _connection;

        public PromptRepository(string connectionString)
        {
            _connection = new MySqlConnection(connectionString);
        }

        public async Task<IEnumerable<Prompt>> GetPrompts()
        {
            await _connection.OpenAsync();
            string query = @"SELECT * FROM IAs;";
            var prompts = await _connection.QueryAsync<Prompt>(query);
            return prompts;
        }

        public async Task<Prompt> AddPromptAsync(Prompt prompt)
        {
            await _connection.OpenAsync();
            string sql = @"
                INSERT INTO IAs (Prompt)
                VALUES (@Prompt);
                SELECT LAST_INSERT_ID();
            ";
            int newId = await _connection.QuerySingleAsync<int>(sql, prompt);
            prompt.Id = newId;
            return prompt;
        }

        public async Task UpdatePromptAsync(Prompt prompt)
        {
            await _connection.OpenAsync();
            string sql = @"
                UPDATE IAs
                SET Prompt = @Prompt
                WHERE Id = @Id;
            ";
            await _connection.ExecuteAsync(sql, prompt);
        }

        public async Task DeletePromptAsync(int id)
        {
            await _connection.OpenAsync();
            string sql = "DELETE FROM IAs WHERE Id = @Id;";
            await _connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}
```

### 🧩 Tecnologias Utilizadas

* **Dapper** — micro ORM para acesso rápido e seguro a dados.
* **MySqlConnector** — driver assíncrono para MySQL.

---

## 🧮 Service Layer

### 📍 Visão Geral

A camada **Service** implementa a **lógica de negócio** e **regras da aplicação**, além de integrar o **cache Redis** com o banco MySQL via repositório.

### 🧱 Estrutura

```
📁 Service/
├── 📄 ICacheService.cs
├── 📄 CacheService.cs
└── 📄 PromptService.cs
```

### 🧩 Interface: `ICacheService`

```csharp
public interface ICacheService
{
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value, TimeSpan? expiry = null);
    Task DeleteAsync(string key);
    Task<bool> KeyExistsAsync(string key);
    Task<bool> SetExpiryAsync(string key, TimeSpan expiry);
}
```

### 🧩 Serviço: `PromptService`

```csharp
using Domain;

public class PromptService
{
    private readonly ICacheService _cacheService;

    public async Task<Prompt?> GetPromptAsync(int id)
    {
        var cacheKey = $"prompt:{id}";

        var cachedPrompt = await _cacheService.GetAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedPrompt))
        {
            return JsonConvert.DeserializeObject<Prompt>(cachedPrompt);
        }

        var prompt = await _repository.GetByIdAsync(id);
        if (prompt != null)
        {
            var promptJson = JsonConvert.SerializeObject(prompt);
            await _cacheService.SetAsync(cacheKey, promptJson, TimeSpan.FromMinutes(30));
        }

        return prompt;
    }
}
```

### 🧩 Responsabilidades

✅ Aplicar lógica de cache para otimizar consultas.
✅ Integrar Redis e MySQL de forma transparente.
✅ Orquestrar repositórios e dados externos.

---

## 🌐 Controller Layer

### 📍 Visão Geral

A camada **Controller** expõe os **endpoints HTTP** da aplicação, recebe e responde requisições RESTful e orquestra os serviços internos.

### 🧱 Estrutura

```
📁 Controllers/
├── 📄 IAController.cs
└── 📄 WeatherForecastController.cs
```

### 🧩 Controller: `IAController`

```csharp
using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Newtonsoft.Json;
using StackExchange.Redis;
using Domain;

namespace GS2_MicroServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IAController : ControllerBase
    {
        private static ConnectionMultiplexer redis;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string key = "getIAs";

            redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase db = redis.GetDatabase();
            await db.KeyExpireAsync(key, TimeSpan.FromSeconds(20));
            string PromptValue = await db.StringGetAsync(key);

            if (!string.IsNullOrEmpty(PromptValue))
            {
                return Ok(PromptValue);
            }

            using var connection = new MySqlConnection("Server=localhost;Database=fiap;IA=root;Password=123");
            await connection.OpenAsync();
            string query = @"SELECT * FROM IAs;";
            var Prompts = await connection.QueryAsync<Prompt>(query);
            string PromptJson = JsonConvert.SerializeObject(Prompts);
            await db.StringSetAsync(key, PromptJson);

            return Ok(Prompts);
        }
    }
}
```

### 🧩 Responsabilidades

✅ Expor endpoints públicos da API.
✅ Utilizar Redis para otimizar respostas.
✅ Chamar os repositórios de dados quando necessário.

---

## ⚙️ Program & Configuração

### 📄 Program.cs

```csharp
using Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPromptRepository, PromptRepository>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=localhost;Database=fiap;User=root;Password=123;Port=3306;";

    return new PromptRepository(connectionString);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 📦 Configuração no `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=fiap;User=root;Password=123;Port=3306;",
    "RedisConnection": "localhost:6379"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

---

## 🔧 Tecnologias Utilizadas

| Categoria              | Tecnologia                               |
| ---------------------- | ---------------------------------------- |
| API                    | ASP.NET Core Web API                     |
| Banco de Dados         | MySQL                                    |
| ORM                    | Dapper                                   |
| Cache                  | Redis (StackExchange.Redis)              |
| Serialização           | Newtonsoft.Json                          |
| Injeção de Dependência | Microsoft.Extensions.DependencyInjection |

---

## 🧪 Testes e Debugging

✅ **Swagger UI** integrado para testar endpoints (`/swagger`).
✅ Testes de cache podem ser validados via `redis-cli`.
✅ Logs configuráveis via `appsettings.json`.

---

## 🎯 Conclusão

O projeto **GS2_MicroServices** implementa uma arquitetura limpa, modular e extensível, que segue os princípios de **separação de responsabilidades**:

✅ **Domain** — Modelo de dados.
✅ **Repository** — Persistência com MySQL.
✅ **Service** — Lógica de negócio e cache.
✅ **Controller** — Interface HTTP.

Essa estrutura facilita:

* Testes unitários.
* Manutenção e expansão.
* Integração com novos microserviços.
