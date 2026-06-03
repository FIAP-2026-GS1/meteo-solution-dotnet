# Meteo Solution — API .NET

API REST desenvolvida em .NET 10 para gerenciamento de regiões monitoradas do sistema **Sentinel Alert**, uma plataforma inteligente de alertas de desastres naturais baseada em geolocalização e dados climáticos.

Projeto desenvolvido para a **Global Solution 2026/1 — FIAP**.

---

## Integrantes

| Nome | RM |
|-----------------------------------|----------|
|   Ana Carolina Pereira Fontes     | RM 562145
|    João Victor Nascimento Adão    | RM 563409
|    Johnny Dias Mathias Junior     | RM 566516
|    Luisa Ganasevici de Abreu      | RM 563403
|    Matheus Moya de Oliveira       | RM 562822
| 

---

## Tecnologias utilizadas

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core 10
- PostgreSQL 18
- Swagger / OpenAPI

---

## Arquitetura

O projeto segue o padrão **Repository Pattern** com separação em camadas:

```
Controllers/     → recebe requisições HTTP e retorna respostas
  DTOs/          → objetos de entrada (request) e saída (response)
Models/          → entidades que representam as tabelas do banco
Repositories/    → acesso e manipulação dos dados via EF Core
Data/            → configuração do DbContext e mapeamentos
Migrations/      → histórico versionado de mudanças no banco
```

---

## Modelo de dados

O sistema gerencia uma hierarquia geográfica completa:

```
Pais → Estado → Cidade → RegiaoMonitorada
```

Cada `RegiaoMonitorada` contém dados geográficos e ambientais utilizados pelo sistema de IA para calcular o score de risco de desastres.

---

## Como executar localmente

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 18](https://www.postgresql.org/download/)
- [Entity Framework CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

```bash
dotnet tool install --global dotnet-ef
```

### Passo a passo

**1. Clone o repositório**
```bash
git clone https://github.com/FIAP-2026-GS1/meteo-solution-dotnet.git
cd meteo-solution-dotnet
```

**2. Configure a string de conexão**

Crie o arquivo `MeteoSolution.API/appsettings.json` com o seguinte conteúdo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=meteosolution_db;Username=postgres;Password=sua_senha"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**3. Aplique as migrations**
```bash
cd MeteoSolution.API
dotnet ef database update
```

**4. Execute a aplicação**
```bash
dotnet run
```

**5. Acesse o Swagger**
```
http://localhost:{porta}/swagger
```

---

## Endpoints disponíveis

### Pais
| Método | Rota | Descrição |
|---|---|---|
| GET | /api/Pais | Lista todos os países |
| GET | /api/Pais/{id} | Busca país por ID |
| POST | /api/Pais | Cadastra novo país |
| PUT | /api/Pais/{id} | Atualiza país |
| DELETE | /api/Pais/{id} | Remove país |

### Estado
| Método | Rota | Descrição |
|---|---|---|
| GET | /api/Estado | Lista todos os estados |
| GET | /api/Estado/{id} | Busca estado por ID |
| POST | /api/Estado | Cadastra novo estado |
| PUT | /api/Estado/{id} | Atualiza estado |
| DELETE | /api/Estado/{id} | Remove estado |

### Cidade
| Método | Rota | Descrição |
|---|---|---|
| GET | /api/Cidade | Lista todas as cidades |
| GET | /api/Cidade/{id} | Busca cidade por ID |
| POST | /api/Cidade | Cadastra nova cidade |
| PUT | /api/Cidade/{id} | Atualiza cidade |
| DELETE | /api/Cidade/{id} | Remove cidade |

### RegiaoMonitorada
| Método | Rota | Descrição |
|---|---|---|
| GET | /api/RegiaoMonitorada | Lista todas as regiões |
| GET | /api/RegiaoMonitorada/{id} | Busca região por ID |
| POST | /api/RegiaoMonitorada | Cadastra nova região |
| PUT | /api/RegiaoMonitorada/{id} | Atualiza região |
| DELETE | /api/RegiaoMonitorada/{id} | Remove região |

---

## Ordem de cadastro recomendada

Para cadastrar uma `RegiaoMonitorada`, respeite a hierarquia de dependências:

```
1. POST /api/Pais
2. POST /api/Estado  (informar paisId)
3. POST /api/Cidade  (informar estadoId)
4. POST /api/RegiaoMonitorada  (informar cidadeId)
```

---

## Decisões técnicas

**Repository Pattern** — isola a lógica de acesso ao banco dos Controllers, facilitando manutenção e testes.

**DTOs de entrada e saída separados** — o DTO de entrada controla o que o cliente pode enviar. O DTO de saída controla o que a API retorna, evitando exposição desnecessária de dados e ciclos de referência circular.

**Code First com Migrations** — o banco de dados é gerado e versionado a partir das classes C#, garantindo consistência entre ambientes.

**DeleteBehavior.Restrict** — impede deleção de registros pai que possuem filhos vinculados, protegendo a integridade referencial.