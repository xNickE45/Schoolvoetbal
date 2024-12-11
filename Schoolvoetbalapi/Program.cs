using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Schoolvoetbalapi;
using Schoolvoetbalapi.Data;
using Schoolvoetbalapi.Migrations;
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

app.MapGet("/tourney", () =>
{
    var tourneys = context.Tourneys.Include(t => t.Matches).ToList();
    return Results.Ok(tourneys);
});

app.MapGet("/tourney/{id}", (int id) =>
{
    var tourney = context.Tourneys.Include(t => t.Matches).FirstOrDefault(t => t.Id == id);
    if (tourney == null)
    {
        return Results.NotFound("Tourney not found.");
    }
    return Results.Ok(tourney);
});

app.MapPost("/tourney", (string name, string token) =>
{
    if (!IsAdmin(context, token))
    {
        var tourney = new Tourney { Name = name };
        context.Tourneys.Add(tourney);
        context.SaveChanges();
        return Results.Created($"/tourney/{tourney.Id}", tourney);
    }
    return Results.BadRequest("Only admins can create tourneys.");
});

app.MapDelete("/tourney/{id}", (int id, string token) =>
{
    if (!IsAdmin(context, token))
    {
        return Results.BadRequest("Only admins can delete tourneys.");
    }
    var tourney = context.Tourneys.Find(id);
    if (tourney == null)
    {
        return Results.NotFound("Tourney not found.");
    }
    context.Tourneys.Remove(tourney);
    context.SaveChanges();
    return Results.NoContent();
});

app.MapPost("/tourney/{id}/match", (int id, string token, int team1Id, int team2Id, int? team1Score, int? team2Score, DateTime startTime, bool finished) =>
{
    if (!IsAdmin(context, token))
    {
        return Results.BadRequest("Only admins can create matches.");
    }
    var tourney = context.Tourneys.Include(t => t.Matches).FirstOrDefault(t => t.Id == id);
    if (tourney == null)
    {
        return Results.NotFound("Tourney not found.");
    }
    var match = new Match
    {
        Team1Id = team1Id,
        Team2Id = team2Id,
        Team1Score = team1Score,
        Team2Score = team2Score,
        StartTime = startTime,
        Finished = finished
    };
    tourney.Matches.Add(match);
    context.SaveChanges();
    return Results.Created($"/tourney/{id}/match/{match.Id}", match);
});

app.MapDelete("/tourney/{tourneyId}/match/{matchId}", (int tourneyId, int matchId, string token) =>
{
    if (!IsAdmin(context, token))
    {
        return Results.BadRequest("Only admins can delete matches.");
    }
    var tourney = context.Tourneys.Include(t => t.Matches).FirstOrDefault(t => t.Id == tourneyId);
    if (tourney == null)
    {
        return Results.NotFound("Tourney not found.");
    }
    var match = tourney.Matches.FirstOrDefault(m => m.Id == matchId);
    if (match == null)
    {
        return Results.NotFound("Match not found.");
    }
    tourney.Matches.Remove(match);
    context.SaveChanges();
    return Results.NoContent();
});

app.MapPut("/tourney/{tourneyId}/match/{matchId}", (int tourneyId, int matchId, string token, int team1Id, int team2Id, int? team1Score, int? team2Score, DateTime startTime, bool finished) =>
{
    if (!IsAdmin(context, token))
    {
        return Results.BadRequest("Only admins can update matches.");
    }
    var tourney = context.Tourneys.Include(t => t.Matches).FirstOrDefault(t => t.Id == tourneyId);
    if (tourney == null)
    {
        return Results.NotFound("Tourney not found.");
    }
    var match = tourney.Matches.FirstOrDefault(m => m.Id == matchId);
    if (match == null)
    {
        return Results.NotFound("Match not found.");
    }
    match.Team1Id = team1Id;
    match.Team2Id = team2Id;
    match.Team1Score = team1Score;
    match.Team2Score = team2Score;
    match.StartTime = startTime;
    match.Finished = finished;
    context.SaveChanges();
    return Results.Ok(match);
});

app.MapPut("/tourney/{id}", (int id, string token, string name) =>
{
    if (!IsAdmin(context, token))
    {
        return Results.BadRequest("Only admins can update tourneys.");
    }
    var tourney = context.Tourneys.Find(id);
    if (tourney == null)
    {
        return Results.NotFound("Tourney not found.");
    }
    tourney.Name = name;
    context.SaveChanges();
    return Results.Ok(tourney);
});

app.MapDelete("/tourney/{id}", (int id, string token) =>
{
    if (!IsAdmin(context, token))
    {
        return Results.BadRequest("Only admins can delete tourneys.");
    }
    var tourney = context.Tourneys.Find(id);
    if (tourney == null)
    {
        return Results.NotFound("Tourney not found.");
    }
    context.Tourneys.Remove(tourney);
    context.SaveChanges();
    return Results.NoContent();
});

app.MapPut("/users/{id}", (int id, string token, string name, string email, string password) =>
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
        user.Name = name;
        user.Email = email;
        user.Password = User.ComputeSha256Hash(password);
        context.SaveChanges();
        return Results.Ok(user);
    }
    else
    {
        return Results.BadRequest("You can only update your own information.");
    }
});

app.MapGet("/teams", () =>
{
    var teams = context.Teams.ToList();
    return Results.Ok(teams);
});

app.MapPost("/teams", (string name, string token) =>
{
    if (!IsAdmin(context, token))
    {
        return Results.BadRequest("Only admins can create teams.");
    }
    var team = new Team { Name = name };
    context.Teams.Add(team);
    context.SaveChanges();
    return Results.Created($"/teams/{team.Id}", team);
});

app.MapDelete("/teams/{id}", (int id, string token) =>
{
    if (!IsAdmin(context, token))
    {
        return Results.BadRequest("Only admins can delete teams.");
    }
    var team = context.Teams.Find(id);
    if (team == null)
    {
        return Results.NotFound("Team not found.");
    }
    context.Teams.Remove(team);
    context.SaveChanges();
    return Results.NoContent();
});

app.MapPut("/teams/{id}", (int id, string token, string name) =>
{
    if (!IsAdmin(context, token))
    {
        return Results.BadRequest("Only admins can update teams.");
    }
    var team = context.Teams.Find(id);
    if (team == null)
    {
        return Results.NotFound("Team not found.");
    }
    team.Name = name;
    context.SaveChanges();
    return Results.Ok(team);
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
