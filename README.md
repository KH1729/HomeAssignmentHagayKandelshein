# Currency Exchange API

A .NET Core Web API for real-time currency exchange rates, focusing on specific currency pairs.

## Features

- Real-time currency exchange rates using CurrencyLayer API
- Support for specific currency pairs (USD/ILS, EUR/ILS, GBP/ILS, EUR/USD, EUR/GBP)
- Automatic rate updates every 10 seconds
- Historical rate storage
- Swagger UI for API documentation

## Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or Visual Studio Code
- CurrencyLayer API key (free tier available at [CurrencyLayer](https://currencylayer.com/))

## Setup

1. Clone the repository:
```bash
git clone https://github.com/KH1729/HomeAssignmentHagayKandelshein.git 
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

The API will be available at `http://localhost:5051`

## API Endpoints

### Get Latest Exchange Rate
```
GET /api/exchangerates/latest/{pairName}
```

Example:
```
GET /api/exchangerates/latest/USD/ILS
```

Response:
```json
{
  "pairName": "USD/ILS",
  "rate": 3.65,
  "lastUpdateTime": "2024-03-20T10:30:00Z"
}
```

### Get Exchange Rate History
```
GET /api/exchangerates/history/{pairName}
```

Example:
```
GET /api/exchangerates/history/USD/ILS
```

## Supported Currency Pairs

- USD/ILS (US Dollar to Israeli Shekel)
- EUR/ILS (Euro to Israeli Shekel)
- GBP/ILS (British Pound to Israeli Shekel)
- EUR/USD (Euro to US Dollar)
- EUR/GBP (Euro to British Pound)

## Database

The application uses SQLite for storing historical exchange rates. The database file (`CurrencyExchange.db`) will be created automatically when the application runs for the first time.

## Background Service

The application includes a background service that updates exchange rates every 10 seconds. The rates are stored in the SQLite database for historical tracking.

## Swagger Documentation

API documentation is available through Swagger UI at:
```
http://localhost:5051/swagger
```

## Error Handling

The API returns appropriate HTTP status codes and error messages:
- 200: Successful operation
- 400: Bad request (invalid currency pair)
- 404: Resource not found
- 500: Internal server error

## Logging

The application logs important events and errors. Logs can be found in the console output when running the application.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details. 