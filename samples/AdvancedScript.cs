#:package Newtonsoft.Json@13.0.3
#:property LangVersion preview

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

// This script demonstrates using NuGet packages in a C# script
// Run with: dotnet run AdvancedScript.cs

// Define a class to deserialize JSON
class TodoItem
{
    public int UserId { get; set; }
    public int Id { get; set; }
    public string Title { get; set; }
    public bool Completed { get; set; }
}

// Main script execution
Console.WriteLine("Fetching data from JSONPlaceholder API...");

// Create HTTP client
using var client = new HttpClient();
client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");

// Fetch data
var response = await client.GetAsync("todos/1");
response.EnsureSuccessStatusCode();

// Read and parse JSON
var json = await response.Content.ReadAsStringAsync();
var todo = JsonConvert.DeserializeObject<TodoItem>(json);

// Display results
Console.WriteLine("\nTodo Item:");
Console.WriteLine($"  User ID: {todo.UserId}");
Console.WriteLine($"  ID: {todo.Id}");
Console.WriteLine($"  Title: {todo.Title}");
Console.WriteLine($"  Completed: {todo.Completed}");

Console.WriteLine("\nRaw JSON:");
Console.WriteLine(json);