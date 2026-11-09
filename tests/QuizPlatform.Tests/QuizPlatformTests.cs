using InteractiveStudyPlatform;
using Xunit;

namespace QuizPlatform.Tests;

public class QuizPlatformTests
{
    private static Quiz SampleQuiz()
    {
        var quiz = new Quiz("C# & Object-Oriented Design Basics", "Computer Science");
        quiz.AddQuestion(new Question("Which OOP principle wraps data and methods into a single unit?",
            new List<string> { "Inheritance", "Encapsulation", "Polymorphism", "Abstraction" }, 1));
        quiz.AddQuestion(new Question("What keyword is used to create an instance of a class in C#?",
            new List<string> { "new", "this", "create", "base" }, 0));
        return quiz;
    }

    [Fact]
    public void Question_ValidatesCorrectAndWrongAnswers()
    {
        var q = new Question("2 + 2?", new List<string> { "3", "4", "5" }, 1);
        Assert.True(q.ValidateAnswer(1));
        Assert.False(q.ValidateAnswer(0));
        Assert.False(q.ValidateAnswer(2));
    }

    [Fact]
    public void Quiz_AddQuestion_GrowsQuestionList()
    {
        var quiz = new Quiz("Sample", "CS");
        Assert.Empty(quiz.Questions);
        quiz.AddQuestion(new Question("Q?", new List<string> { "a", "b" }, 0));
        Assert.Single(quiz.Questions);
    }

    [Fact]
    public void UserProfile_AverageIsZeroBeforeAnyQuiz()
    {
        var user = new UserProfile("Abel Wolde");
        Assert.Equal(0, user.TotalQuizzesTaken);
        Assert.Equal(0, user.TotalScore);
        Assert.Equal(0.0, user.CalculateAverageScore());
    }

    [Fact]
    public void UserProfile_RecordQuizResult_AccumulatesTotals()
    {
        var user = new UserProfile("Abel Wolde");
        user.RecordQuizResult(20);
        user.RecordQuizResult(10);
        Assert.Equal(2, user.TotalQuizzesTaken);
        Assert.Equal(30, user.TotalScore);
        Assert.Equal(15.0, user.CalculateAverageScore());
    }

    [Fact]
    public void ConductQuiz_AllCorrectScoresTwentyPoints()
    {
        var platform = new QuizPlatformService();
        platform.AddQuiz(SampleQuiz());
        var student = new UserProfile("Abel Wolde");
        platform.RegisterStudent(student);

        platform.ConductQuiz("C# & Object-Oriented Design Basics", student, new List<int> { 1, 0 });

        Assert.Equal(1, student.TotalQuizzesTaken);
        Assert.Equal(20, student.TotalScore);
    }

    [Fact]
    public void ConductQuiz_OneCorrectScoresTenPoints()
    {
        var platform = new QuizPlatformService();
        platform.AddQuiz(SampleQuiz());
        var student = new UserProfile("Jane Smith");
        platform.RegisterStudent(student);

        platform.ConductQuiz("C# & Object-Oriented Design Basics", student, new List<int> { 1, 2 });

        Assert.Equal(1, student.TotalQuizzesTaken);
        Assert.Equal(10, student.TotalScore);
    }

    [Fact]
    public void ConductQuiz_UnknownQuizDoesNotRecordAResult()
    {
        var platform = new QuizPlatformService();
        var student = new UserProfile("Abel Wolde");
        platform.RegisterStudent(student);

        platform.ConductQuiz("No Such Quiz", student, new List<int> { 0 });

        Assert.Equal(0, student.TotalQuizzesTaken);
        Assert.Equal(0, student.TotalScore);
    }

    [Fact]
    public void Leaderboard_OrdersByTotalScoreDescending()
    {
        var platform = new QuizPlatformService();
        platform.AddQuiz(SampleQuiz());
        var abel = new UserProfile("Abel Wolde");
        var jane = new UserProfile("Jane Smith");
        platform.RegisterStudent(abel);
        platform.RegisterStudent(jane);
        platform.ConductQuiz("C# & Object-Oriented Design Basics", jane, new List<int> { 1, 2 });
        platform.ConductQuiz("C# & Object-Oriented Design Basics", abel, new List<int> { 1, 0 });

        var ordered = platform.Students.OrderByDescending(s => s.TotalScore).ToList();
        Assert.Equal("Abel Wolde", ordered[0].StudentName);
        Assert.Equal("Jane Smith", ordered[1].StudentName);
    }

    [Fact]
    public void Sqlite_RoundTrip_PreservesQuizzesQuestionsAndScores()
    {
        var db = Path.Combine(Path.GetTempPath(), $"quiz-test-{Guid.NewGuid():N}.db");
        try
        {
            var platform = new QuizPlatformService();
            platform.AddQuiz(SampleQuiz());
            var abel = new UserProfile("Abel Wolde");
            var jane = new UserProfile("Jane Smith");
            platform.RegisterStudent(abel);
            platform.RegisterStudent(jane);
            platform.ConductQuiz("C# & Object-Oriented Design Basics", abel, new List<int> { 1, 0 });
            platform.ConductQuiz("C# & Object-Oriented Design Basics", jane, new List<int> { 1, 2 });
            platform.Save(db);

            var reloaded = new QuizPlatformService();
            reloaded.Load(db);

            Assert.Single(reloaded.AvailableQuizzes);
            var quiz = reloaded.AvailableQuizzes[0];
            Assert.Equal("C# & Object-Oriented Design Basics", quiz.Title);
            Assert.Equal("Computer Science", quiz.Subject);
            Assert.Equal(2, quiz.Questions.Count);
            Assert.Equal(1, quiz.Questions[0].CorrectOptionIndex);
            Assert.Equal(0, quiz.Questions[1].CorrectOptionIndex);
            Assert.Equal(new List<string> { "Inheritance", "Encapsulation", "Polymorphism", "Abstraction" },
                quiz.Questions[0].Options);

            Assert.Equal(2, reloaded.Students.Count);
            var reloadedAbel = reloaded.Students.Single(s => s.StudentName == "Abel Wolde");
            var reloadedJane = reloaded.Students.Single(s => s.StudentName == "Jane Smith");
            Assert.Equal(20, reloadedAbel.TotalScore);
            Assert.Equal(10, reloadedJane.TotalScore);
            Assert.Equal(1, reloadedAbel.TotalQuizzesTaken);
        }
        finally
        {
            if (File.Exists(db)) File.Delete(db);
        }
    }
}
