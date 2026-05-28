using Microsoft.Data.Sqlite;

namespace Darkbox.Catalog.Database;

public class DarkboxDatabase
{
    private readonly string _connectionString;

    public DarkboxDatabase(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InitializeAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = """
          CREATE TABLE IF NOT EXISTS Shoots (
              Id INTEGER PRIMARY KEY AUTOINCREMENT,
              Name TEXT NOT NULL,
              DateStart TEXT NOT NULL,
              DateEnd TEXT NOT NULL,
              Notes TEXT NOT NULL,
              ImportedAt TEXT NOT NULL
          );
          
          CREATE TABLE IF NOT EXISTS Categories (
              Id INTEGER PRIMARY KEY AUTOINCREMENT,
              name TEXT NOT NULL,
              ParentId INTEGER,
              FOREIGN KEY (ParentId) REFERENCES Categories(Id)
          );
          
          CREATE TABLE IF NOT EXISTS Photos (
              Id INTEGER PRIMARY KEY AUTOINCREMENT,
              FilePath TEXT NOT NULL,
              CaptureTime TEXT NOT NULL,
              FileSizeInBytes INTEGER NOT NULL,
              Width INTEGER NOT NULL,
              Height INTEGER NOT NULL,
              ShootId INTEGER NOT NULL,
              Camera TEXT NOT NULL,
              Lens TEXT NOT NULL,
              FocalLength REAL NOT NULL,
              Aperture REAL NOT NULL,
              ShutterSpeed REAL NOT NULL,
              Iso INTEGER NOT NULL,
              Latitude REAL,
              Longitude REAL,
              IsMissing INTEGER NOT NULL DEFAULT 0,
              DriveSerial TEXT NOT NULL,
              ImportedAt TEXT NOT NULL,
              Rating INTEGER NOT NULL DEFAULT 0,
              IsFlagged INTEGER NOT NULL DEFAULT 0,
              FOREIGN KEY (ShootId) REFERENCES Shoots(Id)
          );
          
          CREATE TABLE IF NOT EXISTS Tags (
              Id INTEGER PRIMARY KEY AUTOINCREMENT,
              Name TEXT NOT NULL
          );
          
          CREATE TABLE IF NOT EXISTS PhotoTags (
              PhotoId INTEGER NOT NULL,
              TagId INTEGER NOT NULL,
              PRIMARY KEY (PhotoId, TagId),
              FOREIGN KEY (PhotoId) REFERENCES Photos(Id),
              FOREIGN KEY (TagId) REFERENCES Tags(Id)
          );

          CREATE TABLE IF NOT EXISTS ShootCategories (
              ShootId INTEGER NOT NULL,
              CategoryId INTEGER NOT NULL,
              PRIMARY KEY (ShootId, CategoryId),
              FOREIGN KEY (ShootId)  REFERENCES Shoots(Id),
              FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
          );
          """;
        
        await command.ExecuteNonQueryAsync();
    }
}