# Currency Exchange API

A .NET 9.0 Web API that provides real-time currency exchange rates and historical data. The service automatically fetches and updates exchange rates every 10 seconds.

## Features

- Real-time currency exchange rates
- Historical exchange rate data
- Support for multiple currencies (USD, EUR, GBP, ILS)
- Automatic rate updates every 10 seconds
- RESTful API endpoints
- Swagger UI for API documentation

## Prerequisites

- .NET 9.0 SDK
- SQLite (included in the project)
- API key for exchange rates (configure in appsettings.json)

## API Endpoints

### Get Latest Exchange Rate
```
GET /api/exchangerates/{fromCurrency}/{toCurrency}
```
Example: `/api/exchangerates/USD/EUR`

### Get All Latest Rates
```
GET /api/exchangerates
```
Returns the latest rates for all currency pairs.

### Get Exchange Rate History
```
GET /api/exchangerates/history/{fromCurrency}/{toCurrency}
```
Example: `/api/exchangerates/history/USD/EUR`

## Running the Application

1. Clone the repository:
```bash
git clone https://github.com/KH1729/HomeAssignmentHagayKandelshein/tree/main
cd CurrencyExchangeAPI
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Run the application:
```bash
dotnet run
```

4. Access the API:
- Swagger UI: `https://localhost:5051/swagger`
- API Base URL: `https://localhost:5051/api/exchangerates`

## Database Management

### Cleaning the Database
To manually clean the database:

1. Stop the application
2. Delete all database files:
```bash
rm CurrencyExchange.db
rm CurrencyExchange.db-shm
rm CurrencyExchange.db-wal
```

The database files are:
- `CurrencyExchange.db` - Main database file
- `CurrencyExchange.db-shm` - Shared memory file (SQLite WAL)
- `CurrencyExchange.db-wal` - Write-Ahead Log file (SQLite WAL)

A new empty database will be created automatically when you restart the application.

## Project Structure

- `Controllers/` - API endpoints
  - `ExchangeRatesController.cs` - Handles all exchange rate requests
- `Services/` - Business logic
  - `ExchangeRateService.cs` - Handles data fetching and storage
  - `ExchangeRateBackgroundService.cs` - Manages periodic updates
- `Models/` - Data models
  - `ExchangeRate.cs` - Exchange rate entity
  - `FxRatesResponse.cs` - API response model
- `Data/` - Database context
  - `ApplicationDbContext.cs` - Entity Framework context

## How It Works

1. The `ExchangeRateBackgroundService` runs every 10 seconds
2. It calls `ExchangeRateService` to fetch new rates from the external API
3. The service calculates all possible currency pairs
4. Rates are stored in the SQLite database
5. API endpoints retrieve rates from the database

## Error Handling

The API includes comprehensive error handling:
- Invalid currency pairs
- API failures
- Database errors
- Input validation

All errors are logged and return appropriate HTTP status codes.

## Development

To modify the application:
1. Update the supported currencies in `ExchangeRateService.cs`
2. Modify the update interval in `ExchangeRateBackgroundService.cs`
3. Add new endpoints in `ExchangeRatesController.cs`
