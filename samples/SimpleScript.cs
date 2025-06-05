// Simple C# script example for .NET 10 Preview 4
Console.WriteLine("Hello from C# Script!");

// Variables can be declared and used directly
var message = "This is a C# script running in .NET 10 Preview 4";
Console.WriteLine(message);

// You can use loops and other C# features
for (int i = 1; i <= 5; i++)
{
    Console.WriteLine($"Count: {i}");
}

// You can use LINQ
var numbers = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
var evenNumbers = numbers.Where(n => n % 2 == 0);
Console.WriteLine($"Even numbers: {string.Join(", ", evenNumbers)}");

// Return a value from the script
return "Script executed successfully!";