using PassMaster.Contracts;

namespace PassMaster.Runner.Simulation;

public class Teacher
{
    public Subject Subject { get; }
    public TeacherRule Rule { get; }
    
    public Subject? SubjectA { get; }
    public Subject? SubjectB { get; }

    public Teacher(Subject subject, Random rnd)
    {
        Subject = subject;

        Rule = (TeacherRule)rnd.Next(Constants.RulesCount);
        
        if (Rule == TeacherRule.Random)
        {
            SubjectA = null;
            SubjectB = null;
        }
        else
        {
            Subject a = (Subject)rnd.Next(Constants.SubjectsCount);
            Subject b;
            do
            {
                b = (Subject)rnd.Next(Constants.SubjectsCount);
            } while (b == a);
            
            SubjectA = a;
            SubjectB = b;
        }
    }

    public bool ShouldAsk(bool[] yesterdayAsked, Random rnd)
    {
        switch (Rule)
        {
            case TeacherRule.Random:
                return rnd.Next(2) == 0;
            case TeacherRule.AskIfA:
                return  yesterdayAsked[(int)SubjectA.Value];
            case TeacherRule.AskIfXorAB:
                bool a = yesterdayAsked[(int)SubjectA.Value];
                bool b = yesterdayAsked[(int)SubjectB.Value];
                return a ^ b;
            default:
                throw new InvalidOperationException($"Bad rule state: {Rule}");
        }
    }
    
}