// Entry point — the original demo, unchanged, plus persistence.
// On first run (empty quiz.db) it seeds the original demo data: students
// Abel Wolde and Jane Smith, and the "C# & Object-Oriented Design Basics"
// quiz with its two questions, then runs both quiz attempts and shows the
// leaderboard. On later runs it loads the saved data instead of re-seeding,
// so scores accumulate instead of duplicating. Everything is saved back
// to quiz.db (SQLite) on exit.

namespace InteractiveStudyPlatform;

class Program
{
    static void Main(string[] args)
    {
        QuizPlatformService platform = new QuizPlatformService();
        platform.Load();

        if (platform.AvailableQuizzes.Count == 0 && platform.Students.Count == 0)
        {
            // Setup Students
            UserProfile student1 = new UserProfile("Abel Wolde");
            UserProfile student2 = new UserProfile("Jane Smith");

            platform.RegisterStudent(student1);
            platform.RegisterStudent(student2);

            // Create Quiz
            Quiz csQuiz = new Quiz("C# & Object-Oriented Design Basics", "Computer Science");

            csQuiz.AddQuestion(new Question("Which OOP principle wraps data and methods into a single unit?",
                new List<string> { "Inheritance", "Encapsulation", "Polymorphism", "Abstraction" }, 1));

            csQuiz.AddQuestion(new Question("What keyword is used to create an instance of a class in C#?",
                new List<string> { "new", "this", "create", "base" }, 0));

            platform.AddQuiz(csQuiz);

            // Simulate Taking Quizzes
            platform.ConductQuiz("C# & Object-Oriented Design Basics", student1, new List<int> { 1, 0 }); // Both correct
            platform.ConductQuiz("C# & Object-Oriented Design Basics", student2, new List<int> { 1, 2 }); // One correct
        }

        // Display Statistics & Leaderboard
        platform.DisplayLeaderboard();
        platform.Save();
    }
}
