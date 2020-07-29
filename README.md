# uniswap-data-api
Provide token ticker data in format consumable by various exchanges/sites.

### Details
- Azure Function Runtime v3.0
- .NET Core v3.1

### API Endpoints
#### Uniswap Market Data
- https://uniswapdataapi.azurewebsites.net/api/market
- https://uniswapdataapi.azurewebsites.net/api/orderbook/hex
- https://uniswapdataapi.azurewebsites.net/api/cmc/orderbook/HEX_ETH

#### HEX Market Data (References Uniswap V2 HEX/ETH, Binance ETH/BTC, and Coinbase ETH/USD)
- https://uniswapdataapi.azurewebsites.net/api/hexPrice
