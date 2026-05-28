using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Darkbox.Core.Domain;
using Darkbox.Core.Interfaces;
using Darkbox.ViewModels;

namespace Darkbox.Modules.Library.ViewModels;

public partial class LibraryViewModel : ViewModelBase
{
	private readonly ICatalogRepository _catalog;
	
	public LibraryViewModel(ICatalogRepository catalog)
	{
		_catalog = catalog;
	}
	
	[ObservableProperty]
	private List<Shoot> _shoots = new();	
	
	[ObservableProperty] 
	private List<Category> _categories = new();

	[ObservableProperty]
	private List<Photo> _photos = new();

	[ObservableProperty] 
	private Shoot? _selectedShoot;

	[ObservableProperty] 
	private Shoot? _selectedPhoto;

	public async Task LoadAsync()
	{
		Shoots = await _catalog.GetAllShoots();
		Categories = await _catalog.GetAllCategories();
	}

	public async Task LoadPhotosForShoot(Shoot shoot)
	{
		SelectedShoot = shoot;
		Photos = await _catalog.GetPhotosInShoot(shoot.Id);
	}
}