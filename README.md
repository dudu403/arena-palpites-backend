# ⚽ Arena Palpites - Backend

Backend da plataforma **Arena Palpites**, desenvolvido em **.NET 8**, seguindo princípios de **Clean Architecture**, com foco em **segurança**, **performance**, **escalabilidade** e **boas práticas de desenvolvimento**.

O sistema é responsável por toda a lógica de negócio dos bolões, autenticação, palpites, rankings, sincronização dos jogos da Copa do Mundo, cache, monitoramento e integração com aplicativos Flutter.

---

# Tecnologias

* .NET 8
* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* Firebase Authentication (Google Login)
* Azure App Service
* Azure SQL Database
* Serilog
* Memory Cache
* Health Checks
* Rate Limiting
* FluentValidation
* Swagger / OpenAPI

---

# Arquitetura

O projeto foi desenvolvido utilizando **Clean Architecture**, separando responsabilidades em diferentes camadas.

```
Bolao.Api
│
├── Controllers
├── Authentication
├── Authorization
├── Middlewares
├── BackgroundServices
├── Validators
└── Services

Bolao.Application
│
├── Home
├── Boloes
├── Predictions
├── Matches
├── WorldCup
└── Common

Bolao.Domain
│
├── Entities
├── Enums
└── Interfaces

Bolao.Infrastructure
│
├── Persistence
├── ExternalServices
├── Services
└── Migrations
```

---

# Principais funcionalidades

## Autenticação

* Login com Google
* Firebase Authentication
* Middleware de autenticação
* CurrentUserService
* Authorization Policies
* AdminOnly Policy

---

## Bolões

* Criar bolão
* Entrar utilizando código de convite
* Sair do bolão
* Excluir bolão
* Remover participantes
* Dashboard
* Ranking
* Listagem dos membros
* Jogos por rodada

---

## Palpites

* Criar e editar palpites
* Bloqueio automático após o prazo permitido
* Histórico de palpites
* Processamento automático da pontuação
* Cálculo de placar exato
* Cálculo de vencedor
* Cache invalidado automaticamente após alterações

---

## Home

A Home possui informações resumidas do usuário:

* Bolões que participa
* Jogos do dia
* Última chance para palpitar
* Palpites pendentes
* Total de pontos
* Estatísticas pessoais

---

## Copa do Mundo

Integração automática com API Futebol.

Sincroniza:

* Campeonatos
* Times
* Jogos
* Rodadas
* Grupos
* Classificação
* Resultados

---

## Cache

Foi implementado cache em memória para reduzir consultas ao banco.

Atualmente existe cache para:

* Home
* Ranking
* Dashboard
* Match Details

A invalidação ocorre automaticamente quando:

* usuário envia um palpite;
* entra em um bolão;
* sai de um bolão;
* remove participantes;
* exclui um bolão.

---

# Segurança

* Firebase JWT Validation
* Authorization Policies
* Middleware Global de Exceções
* Rate Limiting
* FluentValidation
* Sanitização de entrada
* Consultas parametrizadas (Entity Framework)

---

# Observabilidade

## Serilog

Logs estruturados contendo:

* Requests
* Tempo de execução
* Exceções
* TraceId
* Usuário autenticado

---

## Health Checks

Endpoints disponíveis:

```
/health

/health/live

/health/ready
```

Verificações:

* SQL Server
* API Futebol
* Estado geral da aplicação

---

# Rate Limiting

Políticas configuradas:

| Política   | Limite      |
| ---------- | ----------- |
| General    | 100 req/min |
| Sensitive  | 20 req/min  |
| CreatePool | 5 req/min   |

---

# Banco de Dados

SQL Server

Principais entidades:

* Users
* Boloes
* BolaoMembers
* BolaoRules
* FootballMatches
* FootballTeams
* FootballGroupStandings
* Predictions
* Championships

---

# API

Alguns endpoints disponíveis:

## Home

```
GET /api/home
```

---

## Bolões

```
POST   /api/boloes

POST   /api/boloes/join

GET    /api/boloes/me

GET    /api/boloes/{id}

GET    /api/boloes/{id}/dashboard

GET    /api/boloes/{id}/members

GET    /api/boloes/{id}/ranking

GET    /api/boloes/{id}/matches

DELETE /api/boloes/{id}

DELETE /api/boloes/{id}/leave
```

---

## Palpites

```
POST /api/predictions

GET  /api/predictions/my

GET  /api/predictions/history

POST /api/predictions/process-finished-matches
```

---

# Como executar

Clone o repositório

```bash
git clone https://github.com/seu-usuario/arena-palpites-backend.git
```

Acesse o projeto

```bash
cd ArenaPalpites
```

Configure:

* Connection String
* Firebase Admin SDK
* API Futebol

Execute as migrations

```bash
dotnet ef database update
```

Execute a aplicação

```bash
dotnet run
```

Swagger:

```
https://localhost:5001/swagger
```

---

# Deploy

Projeto preparado para deploy no Azure.

Serviços utilizados:

* Azure App Service
* Azure SQL Database

---

# Características

* Clean Architecture
* SOLID
* CQRS (Use Cases)
* Repository via DbContext
* Cache
* Health Checks
* Logs Estruturados
* Background Services
* Rate Limiting
* Segurança com JWT
* Performance otimizada
* Escalável para milhares de usuários

