// SQLite persistence for the Quiz Platform.
// Schema (created on first open):
//   users(student_name TEXT PRIMARY KEY, total_quizzes_taken INTEGER, total_score INTEGER)
//   quizzes(id INTEGER PRIMARY KEY, title TEXT, subject TEXT)
//   questions(id INTEGER PRIMARY KEY, quiz_id INTEGER, prompt TEXT,
//             options_json TEXT, correct_option_index INTEGER)

using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace InteractiveStudyPlatform;

public sealed class Storage : IDisposable
{
    private readonly SqliteConnection _connection;

    public Storage(string dbPath)
    {
        _connection = new SqliteConnection($"Data Source={dbPath}");
        _connection.Open();
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS users (
                student_name TEXT PRIMARY KEY,
                total_quizzes_taken INTEGER NOT NULL,
                total_score INTEGER NOT NULL);
            CREATE TABLE IF NOT EXISTS quizzes (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                title TEXT NOT NULL, subject TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS questions (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                quiz_id INTEGER NOT NULL REFERENCES quizzes(id),
                prompt TEXT NOT NULL, options_json TEXT NOT NULL,
                correct_option_index INTEGER NOT NULL);
            """;
        cmd.ExecuteNonQuery();
    }

    public void Save(IEnumerable<Quiz> quizzes, IEnumerable<UserProfile> users)
    {
        using var tx = _connection.BeginTransaction();
        using var clear = _connection.CreateCommand();
        clear.Transaction = tx;
        clear.CommandText = "DELETE FROM questions; DELETE FROM quizzes; DELETE FROM users;";
        clear.ExecuteNonQuery();

        foreach (var quiz in quizzes)
        {
            long quizId;
            using (var insert = _connection.CreateCommand())
            {
                insert.Transaction = tx;
                insert.CommandText = "INSERT INTO quizzes (title, subject) VALUES ($title, $subject); SELECT last_insert_rowid();";
                insert.Parameters.AddWithValue("$title", quiz.Title);
                insert.Parameters.AddWithValue("$subject", quiz.Subject);
                quizId = (long)insert.ExecuteScalar()!;
            }
            foreach (var q in quiz.Questions)
            {
                using var insert = _connection.CreateCommand();
                insert.Transaction = tx;
                insert.CommandText = "INSERT INTO questions (quiz_id, prompt, options_json, correct_option_index) VALUES ($quizId, $prompt, $options, $correct);";
                insert.Parameters.AddWithValue("$quizId", quizId);
                insert.Parameters.AddWithValue("$prompt", q.Prompt);
                insert.Parameters.AddWithValue("$options", JsonSerializer.Serialize(q.Options));
                insert.Parameters.AddWithValue("$correct", q.CorrectOptionIndex);
                insert.ExecuteNonQuery();
            }
        }

        foreach (var user in users)
        {
            using var insert = _connection.CreateCommand();
            insert.Transaction = tx;
            insert.CommandText = "INSERT INTO users (student_name, total_quizzes_taken, total_score) VALUES ($name, $taken, $score);";
            insert.Parameters.AddWithValue("$name", user.StudentName);
            insert.Parameters.AddWithValue("$taken", user.TotalQuizzesTaken);
            insert.Parameters.AddWithValue("$score", user.TotalScore);
            insert.ExecuteNonQuery();
        }

        tx.Commit();
    }

    public (List<Quiz> Quizzes, List<UserProfile> Users) Load()
    {
        var quizzes = new List<Quiz>();
        var byId = new Dictionary<long, Quiz>();

        using (var cmd = _connection.CreateCommand())
        {
            cmd.CommandText = "SELECT id, title, subject FROM quizzes ORDER BY id;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var quiz = new Quiz(reader.GetString(1), reader.GetString(2));
                quizzes.Add(quiz);
                byId[reader.GetInt64(0)] = quiz;
            }
        }

        using (var cmd = _connection.CreateCommand())
        {
            cmd.CommandText = "SELECT quiz_id, prompt, options_json, correct_option_index FROM questions ORDER BY id;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (!byId.TryGetValue(reader.GetInt64(0), out var quiz)) continue;
                var options = JsonSerializer.Deserialize<List<string>>(reader.GetString(2)) ?? new List<string>();
                quiz.AddQuestion(new Question(reader.GetString(1), options, reader.GetInt32(3)));
            }
        }

        var users = new List<UserProfile>();
        using (var cmd = _connection.CreateCommand())
        {
            cmd.CommandText = "SELECT student_name, total_quizzes_taken, total_score FROM users ORDER BY student_name;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var user = new UserProfile(reader.GetString(0));
                user.RestoreTotals(reader.GetInt32(1), reader.GetInt32(2));
                users.Add(user);
            }
        }

        return (quizzes, users);
    }

    public void Dispose() => _connection.Dispose();
}
