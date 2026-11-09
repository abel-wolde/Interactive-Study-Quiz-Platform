// Domain models — logic unchanged from the original single-file Program.cs.

namespace InteractiveStudyPlatform;

public class Question
{
    public string Prompt { get; set; }
    public List<string> Options { get; set; }
    public int CorrectOptionIndex { get; set; }

    public Question(string prompt, List<string> options, int correctOptionIndex)
    {
        Prompt = prompt;
        Options = options;
        CorrectOptionIndex = correctOptionIndex;
    }

    public bool ValidateAnswer(int userChoice)
    {
        return userChoice == CorrectOptionIndex;
    }
}

public class Quiz
{
    public string Title { get; set; }
    public string Subject { get; set; }
    public List<Question> Questions { get; set; }

    public Quiz(string title, string subject)
    {
        Title = title;
        Subject = subject;
        Questions = new List<Question>();
    }

    public void AddQuestion(Question question)
    {
        Questions.Add(question);
    }
}

public class UserProfile
{
    public string StudentName { get; set; }
    public int TotalQuizzesTaken { get; private set; }
    public int TotalScore { get; private set; }

    public UserProfile(string studentName)
    {
        StudentName = studentName;
        TotalQuizzesTaken = 0;
        TotalScore = 0;
    }

    public void RecordQuizResult(int score)
    {
        TotalQuizzesTaken++;
        TotalScore += score;
    }

    public double CalculateAverageScore()
    {
        return TotalQuizzesTaken == 0 ? 0.0 : (double)TotalScore / TotalQuizzesTaken;
    }

    // Used only when restoring a saved profile from the database.
    internal void RestoreTotals(int quizzesTaken, int totalScore)
    {
        TotalQuizzesTaken = quizzesTaken;
        TotalScore = totalScore;
    }
}
