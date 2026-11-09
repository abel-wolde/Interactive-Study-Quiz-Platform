// Quiz Platform Management Service — logic unchanged from the original
// single-file Program.cs, plus SQLite persistence (Save/Load) and read
// accessors used by the tests. The original file had one service class,
// QuizPlatformService, holding the quizzes and the leaderboard in memory;
// there were no separate CourseManager / QuizManager / QuizGenerator /
// UserManager classes in the original code, so none were invented here.

namespace InteractiveStudyPlatform;

public class QuizPlatformService
{
    private List<Quiz> availableQuizzes = new List<Quiz>();
    private List<UserProfile> studentLeaderboard = new List<UserProfile>();

    public IReadOnlyList<Quiz> AvailableQuizzes => availableQuizzes;
    public IReadOnlyList<UserProfile> Students => studentLeaderboard;

    public void RegisterStudent(UserProfile profile)
    {
        studentLeaderboard.Add(profile);
    }

    public void AddQuiz(Quiz quiz)
    {
        availableQuizzes.Add(quiz);
    }

    public void ConductQuiz(string quizTitle, UserProfile student, List<int> studentAnswers)
    {
        var quiz = availableQuizzes.FirstOrDefault(q => q.Title.Equals(quizTitle, StringComparison.OrdinalIgnoreCase));
        if (quiz == null)
        {
            Console.WriteLine($"Quiz '{quizTitle}' not found.");
            return;
        }

        int currentScore = 0;

        Console.WriteLine($"\n=== Starting Quiz: {quiz.Title} ({quiz.Subject}) for {student.StudentName} ===");

        for (int i = 0; i < quiz.Questions.Count; i++)
        {
            var q = quiz.Questions[i];
            bool isCorrect = i < studentAnswers.Count && q.ValidateAnswer(studentAnswers[i]);

            if (isCorrect)
            {
                currentScore += 10;
                Console.WriteLine($"Question {i + 1}: Correct! (+10 pts)");
            }
            else
            {
                Console.WriteLine($"Question {i + 1}: Incorrect.");
            }
        }

        student.RecordQuizResult(currentScore);

        Console.WriteLine($"Final Score for {quiz.Title}: {currentScore} pts");
    }

    public void DisplayLeaderboard()
    {
        Console.WriteLine("\n=== Student Performance Leaderboard ===");

        var sortedList = studentLeaderboard.OrderByDescending(s => s.TotalScore).ToList();

        Console.WriteLine($"{"Rank",-6}{"Student Name",-20}{"Quizzes Taken",-15}{"Total Score",-15}{"Avg Score",-10}");

        Console.WriteLine(new string('-', 66));

        for (int i = 0; i < sortedList.Count; i++)
        {
            var s = sortedList[i];

            Console.WriteLine($"#{i + 1,-5}{s.StudentName,-20}{s.TotalQuizzesTaken,-15}{s.TotalScore,-15}{s.CalculateAverageScore(),-10:F1}");
        }
    }

    // Persistence: quizzes, questions and student totals go to SQLite.
    public void Save(string dbPath = "quiz.db")
    {
        using var storage = new Storage(dbPath);
        storage.Save(availableQuizzes, studentLeaderboard);
    }

    public void Load(string dbPath = "quiz.db")
    {
        using var storage = new Storage(dbPath);
        var (quizzes, users) = storage.Load();
        availableQuizzes = quizzes;
        studentLeaderboard = users;
    }
}
