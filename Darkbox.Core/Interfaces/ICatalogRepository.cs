using Darkbox.Core.Domain;

namespace Darkbox.Core.Interfaces;

public interface ICatalogRepository
{
    Task<List<Photo>> GetPhotosInShoot(int shootId);
    Task<List<Photo>> GetPhotosInCategory(int categoryId);
    Task<List<Photo>> GetAllPhotos();
    Task<Photo?> GetPhotoById(int id);
    Task SavePhoto(Photo photo);
    Task DeletePhoto(int id);
    Task<List<Shoot>> GetAllShoots();
    Task SaveShoot(Shoot shoot);
    Task<List<Category>> GetAllCategories();
    Task SaveCategory(Category category);
    Task AddShootToCategory(int shootId, Category category);
}