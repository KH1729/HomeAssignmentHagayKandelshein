# Currency Exchange API

A .NET Core Web API project that provides real-time currency exchange rates using the FX Rates API.

## Features

- Real-time currency exchange rate fetching
- Support for USD, EUR, GBP, and ILS currencies
- Automatic cross-rate calculations for non-USD base currencies
- Background service for periodic rate updates
- Interactive web interface for rate monitoring
- Detailed error handling and logging
- Automatic retry mechanism for failed requests

## Prerequisites

- .NET 7.0 SDK or later
- An API key from [FX Rates API](https://fxratesapi.com/)

## Configuration

1. Clone the repository

```

## Running the Application

1. Navigate to the project directory
2. Run the following commands:
```bash
dotnet restore
dotnet run
```
3. The application will be available at:
   - Web Interface: `https://localhost:7216` or `http://localhost:5051`
   - API Endpoints: `https://localhost:7216/api` or `http://localhost:5051/api`

## API Endpoints

### Get Latest Exchange Rate
```
GET /api/exchangerates/latest/{fromCurrency}/{toCurrency}
```
Example: `/api/exchangerates/latest/USD/EUR`

Response:
```json
{
    "baseCurrency": "USD",
    "targetCurrency": "EUR",
    "rate": 0.9234,
    "timestamp": "2024-03-14T12:00:00Z"
}
```

## Web Interface Features

The application includes a user-friendly web interface with the following features:

1. Currency Selection
   - Dropdown menus for selecting source and target currencies
   - Support for USD, EUR, GBP, and ILS

2. Rate Monitoring
   - Start/Stop buttons for controlling rate updates
   - Automatic updates every 10 seconds when active
   - Displays the last 10 exchange rates
   - Shows pair name, rate, and last update time

3. Error Handling
   - Visual error messages for API issues
   - Automatic stopping after 3 consecutive errors
   - Clear status indicators for active/error states

4. User Experience
   - Responsive design using Bootstrap
   - Disabled button states to prevent multiple requests
   - Real-time status updates
   - Formatted rate display with 4 decimal places

## Error Handling

The application includes comprehensive error handling:

1. API Errors
   - Rate limit exceeded detection
   - Invalid API key handling
   - Network error handling
   - JSON parsing error handling

2. User Feedback
   - Clear error messages in the UI
   - Status indicators for current state
   - Automatic recovery attempts
   - Graceful degradation on persistent errors

## Logging

The application logs important events:
- API requests and responses
- Error conditions and details
- Rate calculations
- Cross-rate computations
- Background service operations

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details. 