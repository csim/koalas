namespace TestFileScopedNamespace;

// This is the preferred file-scoped namespace

public class GoodExample
{
    // Correct: Block body property with accessors
    public static string Name
    {
        get
        {
            return "Good Example";
        }
    }

    // Correct: Block body method
    public int Value
    {
        get
        {
            return _value;
        }
    }

    private readonly int _value = 42;

    // Correct: Block body constructor
    public GoodExample()
    {
        Console.WriteLine("Created with file-scoped namespace");
    }
}
