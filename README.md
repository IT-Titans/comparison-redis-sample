# comparison-redis-sample

An ASP.NET Core minimal API that caches a slowly computed result in Redis. It reaches the cache in one of two ways, selected at start-up: directly through `StackExchange.Redis`, or through the `IDistributedCache` abstraction. This repository is one half of a pair. [comparison-valkey-sample](https://github.com/IT-Titans/comparison-valkey-sample) contains the same application against Valkey, so the two can be diffed to see what a switch between the servers means for a .NET application. The result: the application code differs only in names. What differs is the container image, the name of the server binary and the license of the server. The details are in [Redis and Valkey compared](#redis-and-valkey-compared).

**Status:** frozen example. It is complete and no longer maintained, and it receives no dependency updates.

**Last verified:** 2026-11-16 with .NET SDK 10.0.404, `StackExchange.Redis` 3.4.35, `Microsoft.Extensions.Caching.StackExchangeRedis` 10.0.15 and the image `redis:8.10.3`. Verified on that date against the running container, in both cache modes: a cache miss, a cache hit, an invalidation, and the expiry of an entry after one minute. Also verified: the Release build (no warnings) and that the server writes its snapshot to the volume.

## Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0), version 10.0.400 or a later 10.0 feature band. `global.json` pins this.
- [Docker with the Compose plugin](https://docs.docker.com/compose/install/)

## Quick start

Start the cache and the application:

```sh
git clone https://github.com/IT-Titans/comparison-redis-sample.git
cd comparison-redis-sample
docker compose up -d
dotnet run --project ./Redis.Api/Redis.Api.csproj
```

Then, in a second terminal:

```sh
curl http://localhost:5250/weatherforecast/Berlin
curl http://localhost:5250/weatherforecast/Berlin
curl http://localhost:5250/invalidate/Berlin
```

The first request takes about three seconds, because the application computes the forecast and the computation contains a simulated delay. The second request returns the same forecast from the cache. The third request removes the cached entry, so the next forecast request computes a new one. Cached entries expire after one minute.

The commands above use `StackExchange.Redis` directly. To use `IDistributedCache` instead, stop the application and start it with the mode argument:

```sh
dotnet run --project ./Redis.Api/Redis.Api.csproj -- distributed
```

To stop the cache:

```sh
docker compose down
```

## Configuration

| Setting | Where | Default | Effect |
|---|---|---|---|
| `REDIS_VERSION` | `.env` | `8.10.3` | Tag of the `redis` image |
| `REDIS_PORT` | `.env` | `6379` | Host port of the cache, published on `127.0.0.1` |
| `ConnectionStrings:Redis` | `Redis.Api/appsettings.json`, or the environment variable `ConnectionStrings__Redis` | `localhost:6379` | Endpoint the application connects to, in the [configuration string format](https://seredis.dev/Configuration) of `StackExchange.Redis` |
| Cache mode | First command-line argument | `direct` | `direct` uses `StackExchange.Redis`, `distributed` uses `IDistributedCache` |

`docker compose up` works without a `.env` file, because `docker-compose.yml` contains the defaults. To change them, copy `.env.example` to `.env` and edit the copy. If you change `REDIS_PORT`, set the connection string to the same port.

The cache persists its data: the compose file starts the server with `--save 60 1`, which writes a snapshot every 60 seconds if at least one write happened. The snapshot is stored in the named volume `redis-data` and survives `docker compose down`. Use `docker compose down -v` to delete it. See the [Redis persistence documentation](https://redis.io/docs/latest/operate/oss_and_stack/management/persistence/) for other options.

## How it works

| File | Role |
|---|---|
| `Redis.Api/Program.cs` | Two endpoints, and the registration of one of the two cache implementations |
| `Redis.Api/Services/WeatherForecastService.cs` | Cache-aside logic: read from the cache, compute on a miss, store with a one-minute expiry |
| `Redis.Api/Services/Caching/ICachingService.cs` | The interface both implementations share |
| `Redis.Api/Services/Caching/RedisCachingService.cs` | Implementation on `IConnectionMultiplexer` from `StackExchange.Redis` |
| `Redis.Api/Services/Caching/RedisDistributedCachingService.cs` | Implementation on `IDistributedCache`, registered with `AddStackExchangeRedisCache` |

## Redis and Valkey compared

This section is identical in both repositories. The license and compatibility statements were checked on 2026-11-13. The commands were recorded on 2026-11-16.

| | comparison-redis-sample | comparison-valkey-sample |
|---|---|---|
| Server image | `redis:8.10.3` | `valkey/valkey:9.1.3` |
| Server start command | `redis-server --save 60 1 --loglevel warning` | `valkey-server --save 60 1 --loglevel warning` |
| Client packages | `StackExchange.Redis` 3.4.35, `Microsoft.Extensions.Caching.StackExchangeRedis` 10.0.15 | The same packages in the same versions |
| Connection | `localhost:6379`, configuration key `ConnectionStrings:Redis` | `localhost:6379`, configuration key `ConnectionStrings:Valkey` |
| Commands in `direct` mode | `GET`, `SETEX`, `UNLINK` | The same |
| Commands in `distributed` mode | `HMGET`, `HMSET`, `EXPIRE`, `UNLINK` | The same |
| License of the server | RSALv2, SSPLv1 or AGPLv3, at the choice of the user | BSD-3-Clause |

What is identical: after replacing `Redis` with `Valkey` in identifiers, namespaces and file names, every C# file, the project file and the configuration files of the two repositories are the same. Both use the client library `StackExchange.Redis`; there is no Valkey-specific client package in either repository.

What differs: the image, the name of the server binary in the compose file, the names, and the links to the documentation of the respective server.

Notes on the table:

- **Commands.** The command names were recorded with `MONITOR` on 2026-11-16 while the quick start ran against `redis:8.10.3` and against `valkey/valkey:9.1.3`. Both servers received the same commands. The [Valkey command reference](https://valkey.io/commands/) documents each of them.
- **Compatibility.** The Valkey documentation states: "Valkey is compatible with Redis OSS 7.2 and all earlier open-source Redis versions, as Valkey 7.2.4 is a fork of Redis OSS 7.2.4." It also states: "Existing Redis client libraries can connect to Valkey without code changes." The same page says that data files written by Redis 7.4 and later are not compatible with Valkey, so persisted data cannot be moved from a current Redis to Valkey by copying the snapshot file. Source: [Migration from Redis to Valkey](https://valkey.io/topics/migration/).
- **License of Redis.** Redis 7.2 and earlier are BSD-3-Clause. Versions 7.4 to 7.8 are under RSALv2 or SSPLv1. Version 8.0 and later are under RSALv2, SSPLv1 or AGPLv3. Sources: the [README of Redis 8.10.3](https://github.com/redis/redis/blob/8.10.3/README.md) and the [Redis license overview](https://redis.io/legal/licenses/).
- **License of Valkey.** BSD-3-Clause, according to the [COPYING file of Valkey 9.1.3](https://github.com/valkey-io/valkey/blob/9.1.3/COPYING). The project describes itself as "forked from the open source Redis project right before the transition to their new source available licenses" ([README of Valkey 9.1.3](https://github.com/valkey-io/valkey/blob/9.1.3/README.md)).
- **Earlier revision.** Until 2026-11-13 the Valkey example threw a `NotSupportedException` in `distributed` mode, and its README attributed this to `IDistributedCache`. The exception was raised by the example's own code. It was not an observation about Valkey: implemented like the Redis example, the mode works against `valkey/valkey:9.1.3`.

Not covered by this comparison: performance, cluster and replication behavior, and features either server added after the fork.

## Known limitations

- There is no fallback when the cache is unreachable. A request then fails with HTTP 500 and a `RedisConnectionException`.
- There are no automated tests.
- The cache mode is read only when exactly one command-line argument is given. With any other number of arguments the application uses `direct`.
- In the default launch profile the application logs one warning about HTTPS redirection, because that profile configures no HTTPS port. The requests above are not affected.

## Security

The cache in this example runs without authentication and without TLS. Both server images turn protected mode off by default, so the compose file publishes the port on `127.0.0.1` only. Do not widen that binding, and do not use this configuration outside a development machine.

To report a vulnerability, see the [security policy](https://github.com/IT-Titans/comparison-redis-sample/security/policy).
