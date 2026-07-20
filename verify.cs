using System;

class Program
{
    static void Main()
    {
        string hash = "$2a$11$K14TmBcESKpC3MD.4zpm1eh4TKV362ToOjBUh6T2g4pNyQPyGOZW.";
        string pass = "Instructor123!";
        bool result = BCrypt.Net.BCrypt.Verify(pass, hash);
        Console.WriteLine($"Is match? {result}");
    }
}
