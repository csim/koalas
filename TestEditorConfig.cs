using System;

// This should trigger a warning - block-scoped namespace instead of file-scoped
namespace TestEditorConfig;

public class TestClass
{
    private readonly int _field = 42;
    private string _name = "Test";

    // These should trigger warnings (expression-bodied accessors)
    public int BadProperty => _field; // Expression-bodied property

    public int BadPropertyWithGetter
    {
        get => _field; // Expression-bodied getter
    }

    public string BadPropertyWithSetter
    {
        get => _name;
        set => _name = value; // Expression-bodied setter
    }

    // These are the preferred styles (block bodies)
    public int GoodProperty
    {
        get
        {
            return _field;
        }
    }

    public string GoodPropertyWithGetterSetter
    {
        get
        {
            return _name;
        }
        set
        {
            _name = value;
        }
    }

    // This should trigger a warning (expression-bodied method)
    public static int BadMethod() => 42;

    // This is the preferred style (block body method)
    public static int GoodMethod()
    {
        return 42;
    }

    // Constructor with expression body (should trigger warning)
    public TestClass() => Console.WriteLine("Created");

    // Preferred constructor style
    public TestClass(string name)
    {
        _name = name;
    }
}
