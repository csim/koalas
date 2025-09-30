using System;

namespace TestReadonlyFields;

public class ReadonlyFieldTest
{
    // These fields should trigger warnings because they're never reassigned after initialization
    private readonly int _neverChanged = 42;  // Should be readonly
    private string _constantName = "Test";  // Should be readonly
    private readonly int _alreadyReadonly = 100;  // This is correct

    // This field is reassigned, so it should NOT trigger a warning
    private int _mutableField = 0;

    public ReadonlyFieldTest()
    {
        // This assignment means _mutableField should not be readonly
        _mutableField = 10;
    }

    public void ChangeValue()
    {
        // This assignment means _mutableField should not be readonly
        _mutableField = 20;
    }

    public int GetNeverChanged()
    {
        return _neverChanged;
    }

    public string GetConstantName()
    {
        return _constantName;
    }

    public int GetReadonlyValue()
    {
        return _alreadyReadonly;
    }

    public int GetMutableField()
    {
        return _mutableField;
    }
}
