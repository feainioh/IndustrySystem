# External Sync Integration Plan

## Goal

- Real-time sync for material, shelf, and experiment data from external systems.
- Support multiple transport protocols with the same domain processing path.
- Keep local app service contracts as the single write path.

## Architecture

- Transport layer (Infrastructure.Communication)
- IExternalSyncChannel: protocol adapter abstraction.
- IExternalSyncChannelFactory: runtime adapter creation.
- WebApiExternalSyncChannel: polling + auth headers + timeout + ACK callback.
- SocketExternalSyncChannel: long-lived TCP reader + reconnect + buffered drain + ACK callback.
- SignalRExternalSyncChannel: hub subscription + automatic reconnect + buffered drain + ACK callback.

- Application layer
- IExternalDataSyncAppService: orchestration facade.
- ExternalDataSyncAppService lifecycle-safe start/stop loop (async-only, deadlock-safe gates).
- ExternalDataSyncAppService endpoint retry with exponential backoff.
- ExternalDataSyncAppService endpoint + message dedup.
- ExternalDataSyncAppService conflict policy (ExternalWins/LocalWins/NewerWins).
- ExternalDataSyncAppService dead-letter queue (memory + jsonl file).
- ExternalDataSyncAppService structured diagnostics and runtime metrics.

- Configuration
- ExternalSync section in appsettings.json (global + endpoint overrides).

## Message Contract

- MessageId: unique message id for dedup and trace.
- EntityType: Material | Shelf | Experiment.
- OperationType: Upsert | Delete | Snapshot.
- OccurredAt: source event time.
- PayloadJson: domain payload.

## Payload Mapping

- Material Upsert/Snapshot: MaterialDto.
- Material Delete: { id }.
- Shelf Upsert/Snapshot: ShelfConfigDto.
- Shelf Delete: { id }.
- Experiment Upsert/Snapshot: ExperimentConfigUpsertDto.
- Experiment Delete: { id }.

## Development Stages Status

1. Stage 1 (done)

- Contract and channel abstraction.
- WebAPI polling channel.
- Application dispatcher and upsert/delete handlers.
- Config binding and DI registration.

1. Stage 2 (done)

- Dedup store (MessageId + endpoint) and retry policy.
- Endpoint-level auth (apikey/bearer) and timeout settings.
- Structured logs and metrics (processed, failed, latency).

1. Stage 3 (done)

- True push adapters for Socket and SignalR.
- ACK callback support for WebAPI/Socket/SignalR.

1. Stage 4 (done)

- Conflict strategy: source-of-truth precedence + version/timestamp merge.
- Dead-letter queue for malformed payloads.

## Deadlock Avoidance Notes

- No synchronous blocking (.Result, .Wait, lock + await) in sync pipeline.
- Lifecycle operations use async SemaphoreSlim.WaitAsync and release before awaiting long tasks.
- Loop stop path cancels token first, then awaits background task outside lifecycle gate.
- Channel teardown is asynchronous and isolated from sync execution gate.

## Operations and Safety

- ExternalSync.Enabled defaults to false.
- Start with SyncOnceAsync in controlled windows.
- Enable continuous StartAsync only after endpoint and payload validation.

## Testing Checklist

- Contract tests for WebAPI message schema.
- Integration tests for Upsert/Delete per entity.
- Fault tests: malformed payload, endpoint timeout, duplicate message.
- Performance tests: large batch handling and loop stability.
