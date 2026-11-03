# Append-only JSONL Timer Log as the source of truth

The Timer Log is stored as an append-only JSONL file in the user's `%LOCALAPPDATA%`, and Daily Session totals are derived from it rather than stored separately. This keeps every total explainable by the log, tolerates a torn last line after a crash, and adds no production dependency. SQLite via `Microsoft.Data.Sqlite` was rejected because transactions and queries would add a dependency that only future reporting could justify, and reporting is out of scope.

## Consequences

- Each entry carries a unique ID, a schema version, and a UTC timestamp with its local offset, because these cannot be reconstructed later: the offset fixes the local day, and the ID and version allow a future export without duplicates.
- Collecting data across users would need a separate central service with its own database, identity, and transport. The local file format is not the obstacle, and nothing in the current solution anticipates that service beyond these fields.
