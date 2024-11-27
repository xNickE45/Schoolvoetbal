using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Schoolvoetbalapi;
using Schoolvoetbalapi.Data;
using Schoolvoetbalapi.Model;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
VoetbalContext context = new VoetbalContext();

if (context.Users.Count() == 0)
{
    Console.WriteLine("Wat is je Email?");
    string email = Console.ReadLine();
    Console.WriteLine("Wat is je Password");
    string pass = Console.ReadLine();
    pass = User.ComputeSha256Hash(pass);
    User root = new User { Name = "root", Email = email, Password = pass, Admin = true };
    context.Users.Add(root);
    context.SaveChanges();
    Console.WriteLine($"Root user created with token {root.Token}");
}

app.MapGet("/", () => "Hello World!");

app.MapGet("/users/login", (string email, string password) =>
{
    foreach (User u in context.Users)
    {
        if (u.Email == email && u.CheckPassword(password))
        {
            return Results.Ok(u);
        }
    }
    return Results.BadRequest();
});

app.MapGet("/users/validate", (string token) =>
{
    if (IsAdmin(context, token))
    {
        return Results.Ok("Admin");
    }
    return Results.BadRequest();
});

app.MapGet("/users", (string token) =>
{
    Console.WriteLine($"Token received: {token}");
    if (IsAdmin(context, token))
    {
        return Results.Ok(context.Users.ToArray());
    }
    return Results.BadRequest();
});

app.MapGet("/users/{id}", (int id, string token) =>
{
    var requestingUser = context.Users.FirstOrDefault(u => u.Token == token);

    if (requestingUser == null)
    {
        return Results.BadRequest("Invalid token.");
    }

    if (requestingUser.Admin || requestingUser.Id == id)
    {
        var user = context.Users.Find(id);
        if (user == null)
        {
            return Results.NotFound("User not found.");
        }
        return Results.Ok(user);
    }
    else
    {
        return Results.BadRequest("You can only see your own information.");
    }
});

app.MapDelete("/users/{id}", (int id, string token) =>
{
    if (!IsAdmin(context, token))
    {
        return Results.BadRequest("Only admins can delete users.");
    }

    User? usertodelete = context.Users.Find(id);
    if (usertodelete != null)
    {
        context.Users.Remove(usertodelete);
        context.SaveChanges();
    }
    return Results.NoContent();
});

app.MapPost("/users/register", (string name, string email, string password) =>
{
    foreach (User u in context.Users)
    {
        if (u.Email == email)
        {
            return Results.BadRequest("User already exists");
        }
    }
    User newUser = new User { Name = name, Email = email, Password = User.ComputeSha256Hash(password) };
    context.Users.Add(newUser);
    context.SaveChanges();
    return Results.Created($"/users/{newUser.Id}", newUser);
});


app.Run();

static bool IsAdmin(VoetbalContext context, string token)
{
    foreach (User u in context.Users)
    {
        Console.WriteLine($"Checking user: {u.Email}, Token: {u.Token}, Admin: {u.Admin}");
        if (u.Token == token && u.Admin)
        {
            return true;
        }
    }
    return false;
}
