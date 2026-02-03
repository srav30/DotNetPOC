# DotNetPOC - Portfolio & Account Services

Two interconnected .NET 10.0 Web APIs demonstrating microservices architecture.

## Architecture

**PortfolioAPI** (Port 5131)
- Manages client portfolios and stock holdings
- Calls AccountAPI to verify funds before executing trades

**AccountAPI** (Port 5132)
- Manages client accounts and balances
- Provides fund verification endpoints

## How to Run

### Run Both Services (Recommended)

Open two terminal windows/tabs:

**Terminal 1 - Account Service:**
```bash
cd C:\Users\sravani.singirikonda\Srav_Workspace\LPLFinancialPOC\AccountAPI
dotnet run
```

**Terminal 2 - Portfolio Service:**
```bash
cd C:\Users\sravani.singirikonda\Srav_Workspace\LPLFinancialPOC\PortfolioAPI
dotnet run
```

Both services will start in Development mode with Swagger UI enabled.

## API Endpoints

### Account Service (http://localhost:5132)

**Swagger UI:** `http://localhost:5132/swagger/index.html`

- `GET /api/accounts/{clientId}` - Get account details
- `POST /api/accounts/verify-funds` - Verify if client has funds
- `POST /api/accounts/withdraw` - Withdraw funds
- `POST /api/accounts/deposit` - Deposit funds

**Sample Clients:** 101, 102, 103

### Portfolio Service (http://localhost:5131)

**Swagger UI:** `http://localhost:5131/swagger/index.html`

- `GET /api/portfolio/{clientId}` - Get portfolio details
- `GET /api/portfolio/{clientId}/holdings` - Get stock holdings
- `POST /api/portfolio/buy` - Buy stock (calls Account Service to verify funds)

## Flow Example

1. Client 101 wants to buy 50 shares of MSFT @ $300/share (total cost: $15,000)
2. Portfolio Service receives request
3. Portfolio Service calls Account Service: "Does client 101 have $15,000?"
4. Account Service responds: "Yes" or "No"
5. If yes → Portfolio Service adds holding and returns success
6. If no → Portfolio Service rejects trade

## Sample Request

### Buy Stock (POST /api/portfolio/buy)
```json
{
  "clientId": 101,
  "symbol": "GOOGL",
  "quantity": 25,
  "price": 180.50
}
```

### Verify Funds (POST /api/accounts/verify-funds)
```json
{
  "clientId": 101,
  "requiredAmount": 4512.50
}
```

## Test Data

| Client | Account Type | Balance |
|--------|-------------|---------|
| 101    | Investment  | $50,000 |
| 102    | Investment  | $75,000 |
| 103    | Investment  | $25,000 |

## Technology Stack

- **.NET 10.0** - Framework
- **ASP.NET Core** - Web API framework
- **Swagger/Swashbuckle** - API documentation
- **HttpClient** - Inter-service communication (Portfolio → Account)

## Key .NET Concepts (Spring Boot Mapping)

| .NET | Spring Boot |
|-----|-----------|
| `HttpClient` | `RestTemplate` or `WebClient` |
| `ActionResult<T>` | `ResponseEntity<T>` |
| `[ApiController]` | `@RestController` |
| `[HttpPost]` | `@PostMapping` |
| `builder.Services` | Spring DI Container |
| `dependency injection via constructor` | `@Autowired` |
