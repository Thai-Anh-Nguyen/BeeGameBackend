using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net; // For secure passwords

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURATION ---
var connectionString = builder.Configuration["MONGODB_URI"];
var client = new MongoClient(connectionString);
var database = client.GetDatabase("BuzzleDB");
var players = database.GetCollection<BsonDocument>("users");

builder.Services.AddCors(options => {
    options.AddPolicy("AllowGame", policy => {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCors("AllowGame");

// --- 2. AUTHENTICATION (Login & Register) ---

app.MapPost("/auth", async ([FromBody] BsonDocument body) => {
    var username = body["Username"].AsString;
    var password = body["Password"].AsString;
    
    var filter = Builders<BsonDocument>.Filter.Eq("Username", username);
    var user = await players.Find(filter).FirstOrDefaultAsync();

    if (user == null) {
        // REGISTER: Hash the password before saving
        body["Password"] = BCrypt.Net.BCrypt.HashPassword(password);
        // Initialize with an empty game data object if it doesn't exist
        if (!body.Contains("GameData")) body["GameData"] = new BsonDocument();
        
        await players.InsertOneAsync(body);
        return Results.Ok(new { message = "Account Created" });
    }

    // LOGIN: Verify the hashed password
    bool isValid = BCrypt.Net.BCrypt.Verify(password, user["Password"].AsString);
    if (isValid) {
        // Return the saved game data upon successful login
        return Results.Ok(new { 
            message = "Logged In", 
            gameData = user.Contains("GameData") ? user["GameData"] : new BsonDocument() 
        });
    }

    return Results.Unauthorized();
});

// --- 3. SAVE GAME ---

app.MapPost("/save", async ([FromBody] BsonDocument body) => {
    var username = body["Username"].AsString;
    var newGameData = body["GameData"].AsBsonDocument;

    var filter = Builders<BsonDocument>.Filter.Eq("Username", username);
    
    // Update ONLY the GameData field to preserve the Password and Username fields
    var update = Builders<BsonDocument>.Update.Set("GameData", newGameData);
    
    var result = await players.UpdateOneAsync(filter, update);
    return result.ModifiedCount > 0 ? Results.Ok("Cloud Saved") : Results.NotFound();
});

app.Run();