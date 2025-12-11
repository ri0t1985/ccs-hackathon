# Seed Data Script Usage

This directory contains `SeedData.sql`, a script to populate the BoardGameNight database with test data for demos.

## Running the Script

### PowerShell (Windows)

**If sqlite3 is in your PATH:**
```powershell
Get-Content CcsHackathon/Data/SeedData.sql | sqlite3 boardgamenight.db
```

**If sqlite3 is not in your PATH:**
```powershell
# Find sqlite3.exe location first, then:
Get-Content CcsHackathon/Data/SeedData.sql | & "C:\path\to\sqlite3.exe" boardgamenight.db
```

**Alternative using Invoke-SqliteQuery (if you have the SQLite PowerShell module):**
```powershell
$dbPath = "boardgamenight.db"
$script = Get-Content CcsHackathon/Data/SeedData.sql -Raw
Invoke-SqliteQuery -Query $script -Database $dbPath
```

### Bash/Linux/Mac

```bash
sqlite3 boardgamenight.db < CcsHackathon/Data/SeedData.sql
```

### Using a SQLite GUI Tool

1. Open the database file (`boardgamenight.db`) in a SQLite GUI tool like:
   - [DB Browser for SQLite](https://sqlitebrowser.org/)
   - [DBeaver](https://dbeaver.io/)
   - [SQLiteStudio](https://sqlitestudio.pl/)
2. Open the `SeedData.sql` file
3. Execute the script

### Using Entity Framework (Programmatic)

If you prefer to run it programmatically from the application, you can read and execute the SQL file:

```csharp
var sql = await File.ReadAllTextAsync("Data/SeedData.sql");
await dbContext.Database.ExecuteSqlRawAsync(sql);
```

## What the Script Does

The script populates:
- **7 users** (as UserId strings in registrations)
- **18 board games** (8 deck-building, 10 economic strategy)
- **7 sessions** (4 historical, 3 upcoming)
- **Registrations** linking users to sessions
- **Game registrations** linking games to sessions
- **26 ratings** (varied 1-5 stars) for historical sessions

## Notes

- The script is **idempotent** - it uses `INSERT OR REPLACE`, so it's safe to run multiple times
- All GUIDs are **deterministic** - the same data will be created each time
- The script respects **foreign key constraints** and inserts data in the correct order
- Dates are relative (e.g., `date('now', '-30 days')`) so they stay current

