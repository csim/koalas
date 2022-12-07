namespace Koalas.CommandLine;

public static class Program {
    public static string X = "11";

    public static string Pro1 { get; set; }

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

        items //.PrintCount()
            //.Print(limit: 6, select: i => i.Name)
            //.Print(limit: 6, where: i => i.Name.Contains("7"), select: i => i.Name)
            //.PrintJsonLine(limit: 6, where: i => i.Name.Contains("7"), select: i => new { i.Name })
            //.PrintJsonLine()
           .PrintJsonLine(where: i => i.Count > 5)
            ;

        //new[] { "1", "2", "3", "4", "5", "6" }.PrintJson();

        //new[] { 1 }.PrintMessage("Done.");
    }
}
