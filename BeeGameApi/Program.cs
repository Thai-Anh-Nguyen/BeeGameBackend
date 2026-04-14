using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURATION ---
// Get your MongoDB URI from an Environment Variable (for security on Render)
var connectionString = builder.Configuration["MONGODB_URI"];
var client = new MongoClient(connectionString);
var database = client.GetDatabase("BeeGameDB");
var players = database.GetCollection<BsonDocument>("users");

// --- 2. ALLOW ITCH.IO (CORS) ---
builder.Services.AddCors(options => {
    options.AddPolicy("AllowGame", policy => {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCors("AllowGame");

// --- 3. ENDPOINTS ---

// REGISTER / LOGIN (Upsert pattern)
app.MapPost("/auth", async ([FromBody] BsonDocument body) => {
    var username = body["Username"].AsString;
    var password = body["Password"].AsString; // In a real app, hash this!
    
    var filter = Builders<BsonDocument>.Filter.Eq("Username", username);
    var user = await players.Find(filter).FirstOrDefaultAsync();

    if (user == null) {
        // Create new user if they don't exist
        await players.InsertOneAsync(body);
        return Results.Ok("Account Created");
    }
    return Results.Ok("Logged In");
});

// SAVE GAME
app.MapPost("/save", async ([FromBody] BsonDocument data) => {
    var filter = Builders<BsonDocument>.Filter.Eq("Username", data["Username"].AsString);
    await players.ReplaceOneAsync(filter, data, new ReplaceOptions { IsUpsert = true });
    return Results.Ok("Cloud Saved");
});

// LOAD GAME
app.MapGet("/load/{username}", async (string username) => {
    var filter = Builders<BsonDocument>.Filter.Eq("Username", username);
    var user = await players.Find(filter).FirstOrDefaultAsync();
    return user != null ? Results.Content(user.ToJson(), "application/json") : Results.NotFound();
});

app.Run();