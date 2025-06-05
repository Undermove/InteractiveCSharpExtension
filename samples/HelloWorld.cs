// Sample C# script for .NET 10 Preview 4
// Run with: dotnet run HelloWorld.cs

Console.WriteLine("Hello, World from C# Script!");

// Print current date and time
Console.WriteLine($"Current date and time: {DateTime.Now}");

// Print command line arguments
Console.WriteLine("Command line arguments:");
foreach (var arg in args)
{
    Console.WriteLine($"  - {arg}");
}