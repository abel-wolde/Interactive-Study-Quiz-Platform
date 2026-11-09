# Interactive Study & Quiz Platform — Upgraded

C# / .NET 8 console app by Abel Wolde. Create quizzes, take them, and rank
students on a leaderboard. Upgraded from the original single-file
`Program.cs`
([abel-wolde/Interactive-Study-Quiz-Platform](https://github.com/abel-wolde/Interactive-Study-Quiz-Platform)).

## What changed in this upgrade

- **SQLite persistence** — quizzes, questions, and student totals are stored
  in `quiz.db` (via `Microsoft.Data.Sqlite`), so data survives between runs.
  The original kept everything in memory and lost it on exit.
- **Split into modules** — `src/QuizPlatform/Models.cs` (`Question`, `Quiz`,
  `UserProfile`), `QuizPlatformService.cs` (quiz/leaderboard logic),
  `Storage.cs` (SQLite layer), `Program.cs` (entry point).
- **Unit tests** — xUnit, 9 tests in `tests/QuizPlatform.Tests/`.
- **Demo seeding** — the original `Main()` always ran the same fixed demo
  (students Abel Wolde and Jane Smith, the "C# & Object-Oriented Design
  Basics" quiz, both attempts). That demo now runs only on first run, when
  `quiz.db` is empty; later runs load the saved data instead of re-seeding,
  so scores accumulate instead of duplicating.

## Accuracy note

The original code has one service class, `QuizPlatformService`, plus the
three models above. There were no separate `CourseManager`, `QuizManager`,
`QuizGenerator`, or `UserManager` classes in the original, so none were
invented here — the split follows the code as written.

## Build & test

Requires .NET 8 SDK.

```bash
dotnet build QuizPlatform.sln
dotnet test QuizPlatform.sln    # 9 xUnit tests
dotnet run --project src/QuizPlatform
```

Note: `dotnet test` on a slow machine can hit the default 90-second
testhost connection timeout; if so, retry with
`VSTEST_CONNECTION_TIMEOUT=300 dotnet test`. On this VM the vstest
testhost process never connected even after 600s (low free memory), so the
9 tests were verified by running the compiled test assembly directly with
a small reflection runner that invokes each `[Fact]` method: **9 passed,
0 failed**. The runner itself is not part of this project.

## Schema

```sql
CREATE TABLE users (
  student_name TEXT PRIMARY KEY,
  total_quizzes_taken INTEGER, total_score INTEGER);
CREATE TABLE quizzes (
  id INTEGER PRIMARY KEY AUTOINCREMENT, title TEXT, subject TEXT);
CREATE TABLE questions (
  id INTEGER PRIMARY KEY AUTOINCREMENT, quiz_id INTEGER,
  prompt TEXT, options_json TEXT, correct_option_index INTEGER);
```

## What's missing before pushing to GitHub

- Push these files into the existing `Interactive-Study-Quiz-Platform` repo
  (replace `Program.cs` with this layout, or add it alongside and update the
  repo README). The original `Program.cs` is preserved in git history.
- Not yet built: interactive quiz-taking from the console (answers are
  currently passed in code), adding new quizzes/users at runtime, per-question
  result history.
