# BeeGame Backend

A .NET 10 Web API backend for the Bee Game, providing cloud save/load functionality and user authentication with MongoDB.

## Features

- **User Authentication**: Register/login endpoint with upsert pattern
- **Cloud Save**: Save game progress to MongoDB
- **Cloud Load**: Retrieve saved game data by username
- **CORS Enabled**: Configured for itch.io game hosting

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/auth` | Register new user or login existing user |
| POST | `/save` | Save game data (upserts existing) |
| GET | `/load/{username}` | Load saved game data for user |

## Tech Stack

- **Framework**: .NET 10
- **Database**: MongoDB
- **Dependencies**:
  - MongoDB.Driver (3.7.1)
  - BCrypt.Net-Next (4.1.0)

## Configuration

Set the MongoDB connection string via environment variable:

```bash
MONGODB_URI=mongodb+srv://<user>:<password>@<cluster>.mongodb.net/
```

## Running Locally

1. Restore dependencies:
   ```bash
   dotnet restore
   ```

2. Run the API:
   ```bash
   dotnet run --project BeeGameApi
   ```

3. API will be available at `http://localhost:5000` (or as configured in `launchSettings.json`)

## Project Structure

```
BeeGameBackend/
├── BeeGameApi/           # Main API project
│   ├── Program.cs        # Application entry point & endpoints
│   ├── appsettings.json  # Configuration
│   └── BeeGameApi.csproj # Project dependencies
├── BeeGameBackend.sln    # Solution file
└── README.md
```

## Deployment

This project is configured for deployment on Render with environment variable support for the MongoDB connection string.
