# Redis Cache Example

This repository provides an example application using a redis cache hosted locally via docker.

The application computes and provides weather forecasts.  
Computed forecasts will be stored in and retrieved from the cache.  
Expired or manually invalidated forecasts will be computed again.

> This app is not production ready! Always secure your credentials and other sensitive information!

## Requirements

- [.NET 9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [docker compose](https://docs.docker.com/compose/install/)

## Getting Started

As per default, the redis cache will be available locally on its default port `6379`.

You can change the port and the cache version being used by adapting the `.env`-file according to your needs.

### Cache

To run the cache, you need to execute the following command using your terminal:

```sh
docker compose up
```

To stop the cache, execute the following command:

```sh
docker compose down
```

### Example App

The example app uses the `IDistributedCache` interface and a direct approach to connect to the cache.

To run the app using the `IDistributedCache`, you need to execute the following command using your terminal:

```sh
dotnet run --project ./Redis.Api/Redis.Api.csproj distributed
```

To run the app using redis directly, you need to execute the following command using your terminal:

```sh
dotnet run --project ./Redis.Api/Redis.Api.csproj
```

You can then access the app e.g. via your browser. 

To create / retrieve a weather forecast for `Berlin`: `http://localhost:5250/weatherforecast/Berlin`  
To invalidate / delete a cached weather forecast for `Berlin`: `http://localhost:5250/invalidate/Berlin`

## Persistance

In this example, the cache is configured to persist data using snapshots at an interval of 60 seconds if at least one write operation happened.  
See [Redis Persistance](https://redis.io/docs/latest/operate/oss_and_stack/management/persistence/) for more options.
