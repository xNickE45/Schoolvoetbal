using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

app.MapGet("/", () => "Dit is de Api voor de schoolvoetbalapp!");

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
    if (IsAdmin(context, token))
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

app.MapGet("/tourneys", () =>
{
    // Retrieve tournaments including matches and their associated teams
    var tourneys = context.Tourneys
        .Include(t => t.Matches) // Include matches
        .ThenInclude(m => m.Team1) // Include Team1 in matches
        .Include(t => t.Matches)
        .ThenInclude(m => m.Team2) // Include Team2 in matches
        .ToArray();

    return Results.Ok(tourneys);
});


app.MapGet("/tourneys/{id}", (int id, string token) =>
{
    var user = context.Users.FirstOrDefault(u => u.Token == token);
    if (user == null)
    {
        return Results.BadRequest("Invalid token.");
    }
    var tourney = context.Tourneys.Include(t => t.Matches).FirstOrDefault(t => t.Id == id);
    if (tourney == null)
    {
        return Results.NotFound("Tourney not found.");
    }
    return Results.Ok(tourney);
});

app.MapPost("/tourneys", (string name, string token) =>
{
    if (IsAdmin(context, token))
    {
        var tourney = new Tourney { Name = name };
        context.Tourneys.Add(tourney);
        context.SaveChanges();
        return Results.Created($"/tourney/{tourney.Id}", tourney);
    }
    return Results.BadRequest("Only admins can create tourneys.");
});

app.MapDelete("/tourneys/{id}", (int id, string token) =>
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
    return Results.Ok();
});

app.MapPost("/tourneys/{id}/match", (int id,string token,Match match) =>
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
 
    tourney.Matches.Add(match);
    context.SaveChanges();
    return Results.Created($"/tourney/{id}/match/{match.Id}", match);
});

app.MapDelete("/tourneys/{tourneyId}/match/{matchId}", (int tourneyId, int matchId, string token) =>
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

app.MapPut("/tourneys/{tourneyId}/match/{matchId}", (int tourneyId, int matchId, string token, Match match) =>
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
    var existingMatch = tourney.Matches.FirstOrDefault(m => m.Id == matchId);
    if (existingMatch == null)
    {
        return Results.NotFound("Match not found.");
    }
    match.Id = matchId;
    context.Entry(existingMatch).CurrentValues.SetValues(match);
    context.SaveChanges();
    return Results.Ok(match);
});

app.MapPut("/tourneys/{id}", (int id, string token, string name) =>
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

app.MapDelete("/tourneys/{id}", (int id, string token) =>
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

app.MapGet("/teams", (string token) =>
{
    var user = context.Users.FirstOrDefault(u => u.Token == token);
    if (user == null)
    {
        return Results.BadRequest("Invalid token.");
    }
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
app.MapPost("/matches/{id}/bets", (int id, string token, Bet bet) =>
{
    var user = context.Users.FirstOrDefault(u => u.Token == token);
    if (user == null)
    {
        return Results.BadRequest("Invalid token.");
    }

    if (user.Money < (decimal)bet.MoneyBet)
    {
        return Results.BadRequest("Insufficient funds.");
    }
    var match = context.Matches.Include(m => m.Bets).FirstOrDefault(m => m.Id == id);
    if (match == null)
    {
        return Results.NotFound("Match not found.");
    }
    var existingBet = match.Bets.FirstOrDefault(b => b.User.Id == user.Id);
    if (existingBet != null)
    {
        return Results.BadRequest("You already placed a bet on this match.");
    }
    bet.User = user;
    match.Bets.Add(bet);
    user.Money -= (decimal)bet.MoneyBet;
    context.SaveChanges();
    return Results.Created($"/matches/{id}/bets/{bet.Id}", bet);
});

app.MapPut("/matches/{matchId}/bets/{betId}", (int matchId, int betId, string token, Bet bet) =>
{
    var user = context.Users.FirstOrDefault(u => u.Token == token);
    if (user == null)
    {
        return Results.BadRequest("Invalid token.");
    }
    var match = context.Matches.Include(m => m.Bets).FirstOrDefault(m => m.Id == matchId);
    if (match == null)
    {
        return Results.NotFound("Match not found.");
    }
    var existingBet = match.Bets.FirstOrDefault(b => b.Id == betId);
    if (existingBet == null)
    {
        return Results.NotFound("Bet not found.");
    }
    if (existingBet.User.Id != user.Id)
    {
        return Results.BadRequest("You can only update your own bets.");
    }
    bet.Id = betId;
    context.Entry(existingBet).CurrentValues.SetValues(bet);
    context.SaveChanges();
    return Results.Ok(bet);
});

app.MapDelete("/matches/{matchId}/bets/{betId}", (int matchId, int betId, string token) =>
{
    var user = context.Users.FirstOrDefault(u => u.Token == token);
    if (user == null)
    {
        return Results.BadRequest("Invalid token.");
    }
    var match = context.Matches.Include(m => m.Bets).FirstOrDefault(m => m.Id == matchId);
    if (match == null)
    {
        return Results.NotFound("Match not found.");
    }
    var existingBet = match.Bets.FirstOrDefault(b => b.Id == betId);
    if (existingBet == null)
    {
        return Results.NotFound("Bet not found.");
    }
    if (existingBet.User.Id != user.Id)
    {
        return Results.BadRequest("You can only delete your own bets.");
    }
    match.Bets.Remove(existingBet);
    context.SaveChanges();
    return Results.NoContent();
});

app.MapGet("/matches/{id}/bets", (int id) =>
{
    var match = context.Matches.Include(m => m.Bets).FirstOrDefault(m => m.Id == id);
    if (match == null)
    {
        return Results.NotFound("Match not found.");
    }
    return Results.Ok(match.Bets);
});

app.MapPut("/matches/{matchId}/bets/{betId}", (int matchId, int betId, string token, Bet bet) =>
{
    var match = context.Matches.Include(m => m.Bets).FirstOrDefault(m => m.Id == matchId);
    if (match == null)
    {
        return Results.NotFound("Match not found.");
    }
    var existingBet = match.Bets.FirstOrDefault(b => b.Id == betId);
    if (existingBet == null)
    {
        return Results.NotFound("Bet not found.");
    }

    bet.Id = betId;
    context.Entry(existingBet).CurrentValues.SetValues(bet);
    context.SaveChanges();
    return Results.Ok(bet);
});

app.MapDelete("/matches/{matchId}/bets/{betId}", (int matchId, int betId, string token) =>
{
    var match = context.Matches.Include(m => m.Bets).FirstOrDefault(m => m.Id == matchId);
    if (match == null)
    {
        return Results.NotFound("Match not found.");
    }
    var existingBet = match.Bets.FirstOrDefault(b => b.Id == betId);
    if (existingBet == null)
    {
        return Results.NotFound("Bet not found.");
    }
    if (existingBet.User.Id != context.Users.Where(u => u.Token == token).First().Id)
    {
        return Results.BadRequest("You can only delete your own bets.");
    }
    match.Bets.Remove(existingBet);
    context.SaveChanges();
    return Results.NoContent();
});

app.MapPut("/matches/{id}", (int id, string token, Match match) =>
{
    if (!IsAdmin(context, token))
    {
        return Results.BadRequest("Only admins can update matches.");
    }
    var existingMatch = context.Matches.Find(id);
    if (existingMatch == null)
    {
        return Results.NotFound("Match not found.");
    }
    match.Id = id;
    context.Entry(existingMatch).CurrentValues.SetValues(match);
    context.SaveChanges();
    return Results.Ok(match);
});

app.MapDelete("/matches/{id}", (int id, string token) =>
{
    if (!IsAdmin(context, token))
    {
        return Results.BadRequest("Only admins can delete matches.");
    }
    var match = context.Matches.Find(id);
    if (match == null)
    {
        return Results.NotFound("Match not found.");
    }
    context.Matches.Remove(match);
    context.SaveChanges();
    return Results.NoContent();
});
app.MapGet("/matchesTableInformation", () =>
{
    // Fetching tournaments
    var tourneys = context.Tourneys.ToArray();
    Console.WriteLine("Tournaments:");
    foreach (var tourney in tourneys)
    {
        Console.WriteLine($"ID: {tourney.Id}, Name: {tourney.Name}");
    }

    // Fetching teams
    var teams = context.Teams.ToArray();
    Console.WriteLine("\nTeams:");
    foreach (var team in teams)
    {
        Console.WriteLine($"ID: {team.Id}, Name: {team.Name}");
    }

    // Fetching matches
    var matches = context.Matches.ToArray();
    Console.WriteLine("\nMatches:");
    foreach (var match in matches)
    {
        // Accessing Team1 and Team2 directly from the Match object
        var team1Name = match.Team1?.Name ?? "Unknown";
        var team2Name = match.Team2?.Name ?? "Unknown";
        var tourneyName = tourneys.FirstOrDefault(t => t.Id == match.TourneyId)?.Name ?? "Unknown"; // Corrected line here


        Console.WriteLine($"ID: {match.Id}, Tournament: {tourneyName}, " +
                          $"Team1: {team1Name} (Score: {match.Team1Score}), " +
                          $"Team2: {team2Name} (Score: {match.Team2Score}), " +
                          $"Finished: {match.Finished}, Start Time: {match.StartTime}");
    }

    return Results.Ok(new
    {
        tourneys,
        teams,
        matches
    });
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


