# ChallengeBet

API .NET 6 para gerenciamento de **jogadores**, **carteiras**, **apostas** e **bônus** — com:
- **Autenticação JWT**
- **EF Core (SQL Server / Azure SQL Edge)** + **controle de concorrência** via `rowversion`
- **RTP configurável** (probabilidade dinâmica conforme retorno-alvo)
- **Bônus por perda** (regras por pontos, com `BonusClaims` para idempotência)
- **Atualização em tempo real** do saldo via **SignalR (WebSockets)**
- **Erros padronizados** (middleware global)
- **Testes** (xUnit + FluentAssertions)
- **Migrations** com scripts e **docker-compose** do banco

---

## Sumário
- [Arquitetura](#arquitetura)
- [Requisitos](#requisitos)
- [Configuração](#configuração)
- [Subir ambiente](#subir-ambiente)
- [Migrations](#migrations)
- [Execução](#execução)
- [Autenticação](#autenticação)
- [Endpoints principais](#endpoints-principais)
- [WebSockets (SignalR)](#websockets-signalr)
- [Regras de negócio](#regras-de-negócio)
- [Erros padronizados](#erros-padrão)
- [Testes](#testes)

---

## Arquitetura

**Clean/Hexagonal light**:
```
ChallengeBet.Domain/          # Entidades e Enums
ChallengeBet.Application/     # DTOs, interfaces, validações, AppException/ErrorCodes
ChallengeBet.Infrastructure/  # EF Core (DbContext, Services), JWT, RTP, RNG
ChallengeBet.Api/             # Endpoints, DI, Middleware de erros, SignalR Hub
ChallengeBet.Tests/           # Testes (xUnit + FluentAssertions)
```

Banco: **Azure SQL Edge** (SQL Server compatível) em Docker.

Concorrência: `Wallet.RowVersion` (TIMESTAMP/rowversion) + **transação** + **retry** em `BetService`.

---

## Requisitos
- **.NET 6 SDK**
- **Docker** + **Docker Compose**
- macOS/Windows/Linux (em macOS, use **azure-sql-edge**)

---

## Configuração

### 1) Banco (Docker)
Arquivo `docker-compose.yml` na raiz:
```yaml
version: "3.9"
services:
  sqledge:
    image: mcr.microsoft.com/azure-sql-edge:latest
    container_name: sqledge
    ports: ["1433:1433"]
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "Your_password123"
    volumes:
      - ./_data/mssql:/var/opt/mssql
    healthcheck:
      test: ["CMD", "/opt/mssql-tools/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "Your_password123", "-Q", "SELECT 1"]
      interval: 10s
      timeout: 3s
      retries: 10
    restart: unless-stopped
```

### 2) `appsettings.Development.json` (API)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=ChallengeBetDb;User Id=sa;Password=Your_password123;Encrypt=false;TrustServerCertificate=true"
  },
  "Jwt": {
    "Key": "SUA-DEV-KEY-AQUI-TROQUE-DEPOIS",
    "Issuer": "ChallengeBet",
    "ExpiresMinutes": 120
  },
  "Rtp": {
    "Value": "0.95",
    "DefaultMultiplier": "2.0"
  }
}
```

---

## Subir ambiente
```bash
docker compose up -d           # sobe banco
dotnet tool install --global dotnet-ef
```

---

## Migrations

Aplicar migrations automaticamente em **Development** (já configurado no `Program.cs`) ou manualmente:

```bash
dotnet ef database update -p ChallengeBet.Infrastructure -s ChallengeBet.Api
```

Criar nova migration:
```bash
dotnet ef migrations add AddWhatever -p ChallengeBet.Infrastructure -s ChallengeBet.Api
```


---

## Execução
```bash
dotnet run --project ChallengeBet.Api
# Console mostrará as URLs, ex:
# Now listening on: https://localhost:7133
# Now listening on: http://localhost:5223
```

Swagger: `https://localhost:7133/swagger`

---

## Autenticação

- **Registro**: `POST /api/players`
- **Login**: `POST /api/auth/login` → retorna `token`, `player`, `wallet`
- Usar o **JWT** como `Authorization: Bearer <token>` nos endpoints protegidos.

---

## Endpoints principais

### Players
- `POST /api/players`  
  **Body**:
  ```json
  { "name":"Lucas", "email":"lucas@ex.com", "password":"123456", "initialBalance":100, "currency":"BRL" }
  ```
  **201** → `{ "player": {...}, "wallet": {...} }`

### Auth
- `POST /api/auth/login`  
  **Body**:
  ```json
  { "email":"lucas@ex.com", "password":"123456" }
  ```
  **200** → `{ "token": "...", "player": {...}, "wallet": {...} }`

### Wallets (JWT)
- `GET /api/wallets/me` → `{ "balance": 1000.00, "currency": "BRL" }`
- `GET /api/wallets/me/transactions?page=1&pageSize=20` → paginação de transações

### Bets (JWT)
- `POST /api/bets`  
  **Body**:
  ```json
  { "amount": 10, "multiplier": 2.0, "autoSettle": true }
  ```
  **201** → `BetDto` (se `autoSettle=false`, fica `Pending` e pode cancelar)

- `POST /api/bets/{id}/cancel`  
  Cancela **somente** apostas `Pending`.

- `GET /api/bets/me?status=Won|Lost|Pending|Cancelled&page=1&pageSize=20`  
  Lista as minhas apostas.

---

## WebSockets (SignalR)

Hub: `/hubs/wallet` (JWT obrigatório). O servidor envia `walletUpdated` quando o saldo muda (aposta, prêmio, bônus, estorno).

---

## Regras de negócio

### Concorrência/Saldo
- `Wallet.RowVersion` + **transações** e **retry** → evita saldo negativo em apostas simultâneas.
- Em conflito (`DbUpdateConcurrencyException`) revalida saldo e retorna erro adequado.

### RTP Dinâmico (configurável)
- Probabilidade de vitória: `p = RTP / multiplier` (clamp `[0,1]`).
- Prêmio: `amount * multiplier`.
- `Rtp:Value` e `Rtp:DefaultMultiplier` em `appsettings`.

### Bônus por perda
- **1 ponto por R$1 perdido** (parte inteira de `amount`).
- Tabela `BonusRules(pointsThreshold, rewardValue)`.
- Ao atingir metas, crédito automático + `BonusClaim` (único por `PlayerId, RuleId`).

**Inserir regras de exemplo:**
```sql
INSERT INTO BonusRules(PointsThreshold, RewardValue) VALUES (50,5), (150,20), (300,50);
```

---

## Erros padrão

Middleware global retorna JSON consistente:
```json
{
  "status": 409,
  "code": "BET_NOT_PENDING",
  "message": "Só é possível cancelar apostas pendentes.",
  "traceId": "00-....-01",
  "details": null
}
```

Códigos comuns:
- `VALIDATION_ERROR` (400)
- `INVALID_CREDENTIALS` (401)
- `WALLET_NOT_FOUND` (404)
- `BET_NOT_FOUND` (404)
- `MIN_BET_NOT_MET` (400)
- `INSUFFICIENT_FUNDS` (422)
- `CONCURRENCY_CONFLICT` (409)
- `UNEXPECTED_ERROR` (500)

---

## Testes

### Rodar
```bash
dotnet Run
```
