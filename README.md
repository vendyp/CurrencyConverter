## CurrencyConverter

[![.NET](https://github.com/vendyp/CurrencyConverter/actions/workflows/dotnet.yml/badge.svg)](https://github.com/vendyp/CurrencyConverter/actions/workflows/dotnet.yml)

CurrencyConverter is a .NET-based application designed to facilitate currency conversions. Built with C#, it aims to
provide accurate and efficient currency exchange functionalities using free currency rate provider.

## Installation

- Install [.NET9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- Install [Docker](https://docs.docker.com/get-started/get-docker/)
- After docker installed, run command ``docker-compose up -d``
- Then open ``appsettings.json`` and change the value of `RedisConnection` with `localhost:6379`

```json
{
  "ConnectionStrings": {
    "RedisConnection": "localhost:6379"
  }
}
```

- Change directory to WebApi, then type `dotnet run`
- It will run under `localhost:5000`

![](https://github.com/vendyp/CurrencyConverter/blob/main/.github/images/running.png)

## How to test if the project successfully running

1. Get the token

```curl
curl --request GET \
  --url http://localhost:5000/api/test-generate-token \
  --header 'Content-Type: application/json' \
  --header 'User-Agent: your-machine'
```

2. After then the token

```curl
curl --request POST \
  --url http://localhost:5000/api/currency/conversion \
  --header 'Authorization: Bearer your-token' \
  --header 'Content-Type: application/json' \
  --header 'User-Agent: your-machine' \
  --data '{
	"baseCurrency":"IDR",
	"amount":758232,
	"ToCurrency":"EUR"
}'
```

as response

```json

{
  "amount": 758232,
  "toAmount": 40.94
}
```

## Built-in Features

1. JWT Token
2. Rate Limiting Middleware
3. Use ProblemDetails [RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807) and ExceptionHandler
4. Response Time Logging Middleware