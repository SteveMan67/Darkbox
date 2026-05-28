using Darkbox.Core.Domain;
using Darkbox.Core.Interfaces;
using Microsoft.Data.Sqlite;

namespace Darkbox.Catalog.Repositories;

public class SqliteCatalogRepository : ICatalogRepository
{
    private readonly string _connectionString;

    public SqliteCatalogRepository(string connectionString)
    {
        _connectionString = connectionString;    
    }

    private SqliteConnection GetConnection()
    {
        return new SqliteConnection(_connectionString);
    }
    
    public async Task<List<Photo>> GetPhotosInShoot(int shootId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = """
                                SELECT Id, FilePath, CaptureTime, FileSizeInBytes, Width, Height,
                                         ShootId, Camera, Lens, FocalLength, Aperture, ShutterSpeed,
                                         Iso, Latitude, Longitude, IsMissing, DriveSerial, ImportedAt,
                                         Rating, IsFlagged FROM Photos WHERE ShootId = $shootId
                              """;
        command.Parameters.AddWithValue("$shootId", shootId);
        
        var photos = new List<Photo>();
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            photos.Add(new Photo
            {
                Id = reader.GetInt32(0),
                FilePath = reader.GetString(1),
                CaptureTime = DateTime.Parse(reader.GetString(2)),
                FileSizeInBytes = reader.GetInt64(3),
                Width = reader.GetInt32(4),
                Height = reader.GetInt32(5),
                ShootId = reader.GetInt32(6),
                Camera = reader.GetString(7),
                Lens = reader.GetString(8),
                FocalLength = reader.GetDouble(9),
                Aperture = reader.GetDouble(10),
                ShutterSpeed = reader.GetDouble(11),
                Iso = reader.GetInt32(12),
                Latitude = reader.IsDBNull(13) ? null : (double?)reader.GetDouble(13),
                Longitude = reader.IsDBNull(14) ? null : (double?)reader.GetDouble(14),
                IsMissing = reader.GetInt32(15) == 1,
                DriveSerial = reader.GetString(16),
                ImportedAt = DateTime.Parse(reader.GetString(17)),
                Rating = reader.GetInt32(18),
                IsFlagged = reader.GetInt32(19) == 1
            });
        }
        
        return photos;
        
    }
    
    public async Task<List<Photo>> GetPhotosInCategory(int categoryId)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = """
                              WITH RECURSIVE CategoryTree AS (
                                  SELECT Id FROM Categories WHERE Id = $categoryId
                                  UNION ALL
                                  SELECT c.Id FROM Categories c
                                  INNER JOIN CategoryTree ct ON c.ParentId = ct.Id
                              )
                              SELECT DISTINCT p.Id, p.FilePath, p.CaptureTime, p.FileSizeInBytes, p.Width, p.Height,
                                              p.ShootId, p.Camera, p.Lens, p.FocalLength, p.Aperture, p.ShutterSpeed,
                                              p.Iso, p.Latitude, p.Longitude, p.IsMissing, p.DriveSerial, p.ImportedAt,
                                              p.Rating, p.IsFlagged
                                FROM Photos p
                                INNER JOIN ShootCategories sc ON p.ShootId =  sc.ShootId
                                WHERE sc.CategoryId IN (SELECT Id FROM CategoryTree)
                              """;
        command.Parameters.AddWithValue("$categoryId", categoryId);
        
        var photos = new List<Photo>();
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            photos.Add(new Photo
            {
                Id = reader.GetInt32(0),
                FilePath = reader.GetString(1),
                CaptureTime = DateTime.Parse(reader.GetString(2)),
                FileSizeInBytes = reader.GetInt64(3),
                Width = reader.GetInt32(4),
                Height = reader.GetInt32(5),
                ShootId = reader.GetInt32(6),
                Camera = reader.GetString(7),
                Lens = reader.GetString(8),
                FocalLength = reader.GetDouble(9),
                Aperture = reader.GetDouble(10),
                ShutterSpeed = reader.GetDouble(11),
                Iso = reader.GetInt32(12),
                Latitude = reader.IsDBNull(13) ? null : (double?)reader.GetDouble(13),
                Longitude = reader.IsDBNull(14) ? null : (double?)reader.GetDouble(14),
                IsMissing = reader.GetInt32(15) == 1,
                DriveSerial = reader.GetString(16),
                ImportedAt = DateTime.Parse(reader.GetString(17)),
                Rating = reader.GetInt32(18),
                IsFlagged = reader.GetInt32(19) == 1
            });
        }
        
        return photos;
    }

    public async Task<List<Photo>> GetAllPhotos()
    {
        using var connection = GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
                                SELECT Id, FilePath, CaptureTime, FileSizeInBytes, Width, Height,
                                         ShootId, Camera, Lens, FocalLength, Aperture, ShutterSpeed,
                                         Iso, Latitude, Longitude, IsMissing, DriveSerial, ImportedAt,
                                         Rating, IsFlagged FROM Photos
                              """;

        var photos = new List<Photo>();

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            photos.Add(new Photo
            {
                Id = reader.GetInt32(0),
                FilePath = reader.GetString(1),
                CaptureTime = DateTime.Parse(reader.GetString(2)),
                FileSizeInBytes = reader.GetInt64(3),
                Width = reader.GetInt32(4),
                Height = reader.GetInt32(5),
                ShootId = reader.GetInt32(6),
                Camera = reader.GetString(7),
                Lens = reader.GetString(8),
                FocalLength = reader.GetDouble(9),
                Aperture = reader.GetDouble(10),
                ShutterSpeed = reader.GetDouble(11),
                Iso = reader.GetInt32(12),
                Latitude = reader.IsDBNull(13) ? null : (double?)reader.GetDouble(13),
                Longitude = reader.IsDBNull(14) ? null : (double?)reader.GetDouble(14),
                IsMissing = reader.GetInt32(15) == 1,
                DriveSerial = reader.GetString(16),
                ImportedAt = DateTime.Parse(reader.GetString(17)),
                Rating = reader.GetInt32(18),
                IsFlagged = reader.GetInt32(19) == 1
            });
        }
        
        return photos;
    }

    public async Task<Photo?> GetPhotoById(int id)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = """
                                SELECT Id, FilePath, CaptureTime, FileSizeInBytes, Width, Height,
                                         ShootId, Camera, Lens, FocalLength, Aperture, ShutterSpeed,
                                         Iso, Latitude, Longitude, IsMissing, DriveSerial, ImportedAt,
                                         Rating, IsFlagged FROM Photos WHERE Id = $id
                              """;
        command.Parameters.AddWithValue("$id", id);
        
        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;
        
        var photo = new Photo
            {
                Id = reader.GetInt32(0),
                FilePath = reader.GetString(1),
                CaptureTime = DateTime.Parse(reader.GetString(2)),
                FileSizeInBytes = reader.GetInt64(3),
                Width = reader.GetInt32(4),
                Height = reader.GetInt32(5),
                ShootId = reader.GetInt32(6),
                Camera = reader.GetString(7),
                Lens = reader.GetString(8),
                FocalLength = reader.GetDouble(9),
                Aperture = reader.GetDouble(10),
                ShutterSpeed = reader.GetDouble(11),
                Iso = reader.GetInt32(12),
                Latitude = reader.IsDBNull(13) ? null : (double?)reader.GetDouble(13),
                Longitude = reader.IsDBNull(14) ? null : (double?)reader.GetDouble(14),
                IsMissing = reader.GetInt32(15) == 1,
                DriveSerial = reader.GetString(16),
                ImportedAt = DateTime.Parse(reader.GetString(17)),
                Rating = reader.GetInt32(18),
                IsFlagged = reader.GetInt32(19) == 1       
            };
        
        return photo;
    }
    
    public async Task SavePhoto(Photo photo)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = """
                              INSERT INTO Photos (
                                     FilePath, CaptureTime, FileSizeBytes, Width, Height,
                                     ShootId, Camera, Lens, FocalLength, Aperture, ShutterSpeed,
                                     Iso, Latitude, Longitude, IsMissing, DriveSerial, ImportedAt,
                                     Rating, IsFlagged
                                 )
                                 VALUES (
                                     $filePath, $captureTime, $fileSizeBytes, $width, $height,
                                     $shootId, $camera, $lens, $focalLength, $aperture, $shutterSpeed,
                                     $iso, $latitude, $longitude, $isMissing, $driveSerial, $importedAt,
                                     $rating, $isFlagged
                                 )
                              """;
        
        command.Parameters.AddWithValue("$filePath", photo.FilePath);
        command.Parameters.AddWithValue("$captureTime", photo.CaptureTime.ToString("O"));
        command.Parameters.AddWithValue("$fileSizeBytes", photo.FileSizeInBytes);
        command.Parameters.AddWithValue("$width", photo.Width);
        command.Parameters.AddWithValue("$height", photo.Height);
        command.Parameters.AddWithValue("$shootId", photo.ShootId);
        command.Parameters.AddWithValue("$camera", photo.Camera);
        command.Parameters.AddWithValue("$lens", photo.Lens);
        command.Parameters.AddWithValue("$focalLength", photo.FocalLength);
        command.Parameters.AddWithValue("$aperture", photo.Aperture);
        command.Parameters.AddWithValue("$shutterSpeed", photo.ShutterSpeed);
        command.Parameters.AddWithValue("$iso", photo.Iso);
        command.Parameters.AddWithValue("$latitude", photo.Latitude.HasValue ? photo.Latitude.Value : DBNull.Value);
        command.Parameters.AddWithValue("$longitude", photo.Longitude.HasValue ? photo.Longitude.Value : DBNull.Value);
        command.Parameters.AddWithValue("$isMissing", photo.IsMissing ? 1 : 0);
        command.Parameters.AddWithValue("$driveSerial", photo.DriveSerial);
        command.Parameters.AddWithValue("$importedAt", photo.ImportedAt.ToString("O"));
        command.Parameters.AddWithValue("$rating", photo.Rating);
        command.Parameters.AddWithValue("$isFlagged", photo.IsFlagged ? 1 : 0);
        
        await command.ExecuteNonQueryAsync();
    }
    
    public async Task DeletePhoto(int id)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Photos WHERE Id = $id";
        
        command.Parameters.AddWithValue("$id", id);
        
        await command.ExecuteNonQueryAsync();
    }
    
    public async Task<List<Shoot>> GetAllShoots()
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, DateStart, DateEnd, Notes FROM shoots";

        var shoots = new List<Shoot>();
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            shoots.Add(new Shoot
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                DateStart = DateTime.Parse(reader.GetString(2)),
                DateEnd = DateTime.Parse(reader.GetString(3)),
                Notes = reader.GetString(4)
            });
        }
        return shoots;
    }
    
    public async Task SaveShoot(Shoot shoot)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = """
          INSERT INTO shoots (Name, DateStart, DateEnd, Notes, ImportedAt) 
          VALUES ($name, $dateStart, $dateEnd, $notes, $importedAt);
          """;
        
        command.Parameters.AddWithValue("$name", shoot.Name);
        command.Parameters.AddWithValue("$dateStart", shoot.DateStart.ToString("O"));
        command.Parameters.AddWithValue("$dateEnd", shoot.DateEnd.ToString("O"));
        command.Parameters.AddWithValue("$notes", shoot.Notes);
        command.Parameters.AddWithValue("$importedAt", shoot.ImportedAt);
        
        await command.ExecuteNonQueryAsync();
    }
    
    public async Task<List<Category>> GetAllCategories()
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        
        command.CommandText = "SELECT Id, Name, ParentId FROM Categories";
        var categories = new List<Category>();
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            categories.Add(new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                ParentId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
            });
        }

        var lookup = categories.ToDictionary(c => c.Id);

        // add all the parents and children to each category
        foreach (var category in categories)
        {
            if (category.ParentId.HasValue)
            {
                var parent = lookup[category.ParentId.Value];
                category.Parent = parent;
                parent.Children.Add(category);
            }
        }
        return categories;
    }
    
    public async Task SaveCategory(Category category)
    {
        using var connection = GetConnection();
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = """
                              INSERT INTO Categories (Name, ParentId)
                              VALUES ($name, $parentId)
                              """;
        
        command.Parameters.AddWithValue("$name", category.Name);
        command.Parameters.AddWithValue("$parentId", category.ParentId.HasValue ? category.ParentId.Value : DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }
}