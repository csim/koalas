namespace Koalas;

public static class Program {
    public static void Main(string[] args) {
        var items = new[] {
                              new { Name = "Name1", Count = 4d },
                              new { Name = "Name2", Count = 5d },
                              new { Name = "Name3", Count = 6d },
                              new { Name = "Name4", Count = 7d },
                              new { Name = "Name5", Count = 8d },
                              new { Name = "Name6", Count = 9d },
                              new { Name = "Name7", Count = 10d },
                              new { Name = "Name8", Count = 11d },
                              new { Name = "Name9", Count = 12d }
                          };

        items.PrintCount()
             .Print(i => i.Name)
             .PrintLiteral();

        new[] { "1", "2", "3", "4", "5", "6" }.PrintJson();
    }
}
