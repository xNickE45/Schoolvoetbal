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

app.MapGet("/users/validate", (HttpContext httpContext) =>
{
    var token = httpContext.Request.Query["token"].ToString();
    if (IsAdmin(context, token))
    {
        return Results.Ok("Admin");
    }
    return Results.BadRequest();
});

app.MapGet("/users", (HttpContext httpContext) =>
{
    var token = httpContext.Request.Query["token"].ToString();
    Console.WriteLine($"Token received: {token}");
    if (IsAdmin(context, token))
    {
        return Results.Ok(context.Users.ToArray());
    }
    return Results.BadRequest();
});

app.MapGet("/users/{id}", (int id, HttpContext httpContext) =>
{
    var token = httpContext.Request.Query["token"].ToString();
    if (!IsAdmin(context, token))
        return Results.BadRequest();
    return Results.Ok(context.Users.Find(id));
});

app.MapPost("/users", (User u) =>
{
    context.Users.Add(u);
    context.SaveChanges();
    return u;
});

app.MapDelete("/users/{id}", (int id) =>
{
    User? usertodelete = context.Users.Find(id);
    if (usertodelete != null)
        context.Users.Remove(usertodelete);
    context.SaveChanges();
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

app.MapGet("/teams", () =>
{
    return Results.Ok(context.Teams.ToArray());
});

app.MapGet("/teams/{id}", (int id) =>
{
    return Results.Ok(context.Teams.Find(id));
});

app.MapPost("/teams", (Team team) =>
{
    context.Teams.Add(team);
    context.SaveChanges();
    return Results.Created($"/teams/{team.Id}", team);
});

app.MapPut("/teams/{id}", (int id, Team updatedTeam) =>
{
    var team = context.Teams.Find(id);
    if (team == null)
    {
        return Results.NotFound();
    }
    team.Name = updatedTeam.Name;
    team.Points = updatedTeam.Points;
    team.CreatorId = updatedTeam.CreatorId;
    context.SaveChanges();
    return Results.Ok(team);
});

app.MapDelete("/teams/{id}", (int id) =>
{
    var team = context.Teams.Find(id);
    if (team != null)
    {
        context.Teams.Remove(team);
        context.SaveChanges();
    }
    return Results.NoContent();
});

app.MapGet("/matches", () =>
{
    return Results.Ok(context.Matches.ToArray());
});

app.MapGet("/matches/{id}", (int id) =>
{
    return Results.Ok(context.Matches.Find(id));
});

app.MapPost("/matches", (Match match) =>
{
    context.Matches.Add(match);
    context.SaveChanges();
    return Results.Created($"/matches/{match.Id}", match);
});

app.MapPut("/matches/{id}", (int id, Match updatedMatch) =>
{
    var match = context.Matches.Find(id);
    if (match == null)
    {
        return Results.NotFound();
    }
    match.Team1Id = updatedMatch.Team1Id;
    match.Team2Id = updatedMatch.Team2Id;
    match.Team1Score = updatedMatch.Team1Score;
    match.Team2Score = updatedMatch.Team2Score;
    match.Field = updatedMatch.Field;
    match.RefereeId = updatedMatch.RefereeId;
    match.Time = updatedMatch.Time;
    context.SaveChanges();
    return Results.Ok(match);
});

app.MapDelete("/matches/{id}", (int id) =>
{
    var match = context.Matches.Find(id);
    if (match != null)
    {
        context.Matches.Remove(match);
        context.SaveChanges();
    }
    return Results.NoContent();
});

app.MapGet("/goals", () =>
{
    return Results.Ok(context.Goals.ToArray());
});

app.MapGet("/goals/{id}", (int id) =>
{
    return Results.Ok(context.Goals.Find(id));
});

app.MapPost("/goals", (Goal goal) =>
{
    context.Goals.Add(goal);
    context.SaveChanges();
    return Results.Created($"/goals/{goal.Id}", goal);
});

app.MapPut("/goals/{id}", (int id, Goal updatedGoal) =>
{
    var goal = context.Goals.Find(id);
    if (goal == null)
    {
        return Results.NotFound();
    }
    goal.PlayerId = updatedGoal.PlayerId;
    goal.MatchId = updatedGoal.MatchId;
    goal.Minute = updatedGoal.Minute;
    context.SaveChanges();
    return Results.Ok(goal);
});

app.MapDelete("/goals/{id}", (int id) =>
{
    var goal = context.Goals.Find(id);
    if (goal != null)
    {
        context.Goals.Remove(goal);
        context.SaveChanges();
    }
    return Results.NoContent();
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
