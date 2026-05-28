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
        throw new NotImplementedException();
    }
    
    public async Task<List<Photo>> GetPhotosInCategory(int categoryId)
    {
        throw new NotImplementedException();
    }
    
    public async Task<List<Photo>> GetAllPhotos()
    {
        throw new NotImplementedException();
    }
    
    public async Task<Photo> GetPhotoById(int id)
    {
        throw new NotImplementedException();
    }
    
    public async Task SavePhoto(Photo photo)
    {
        throw new NotImplementedException();
    }
    
    public async Task DeletePhoto(int id)
    {
        throw new NotImplementedException();
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
                DateStart = reader.GetDateTime(2),
                DateEnd = reader.GetDateTime(3),
                Notes = reader.GetString(4)
            });
        }
        return shoots;
    }
    
    public Task SaveShoot(Shoot shoot)
    {
        throw new NotImplementedException();
    }
    
    public Task<List<Category>> GetAllCategories()
    {
        throw new NotImplementedException();
    }
    
    public Task SaveCategory(Category category)
    {
        throw new NotImplementedException();
    }
}