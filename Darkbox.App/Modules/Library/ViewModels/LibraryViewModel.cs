using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Darkbox.Core.Domain;
using Darkbox.Core.Interfaces;
using Darkbox.ViewModels;

namespace Darkbox.Modules.Library.ViewModels;

public partial class LibraryViewModel : ViewModelBase
{
	private readonly ICatalogRepository _catalog;
	
	public ImportDialogViewModel ImportDialog { get; }
	
	public LibraryViewModel(ICatalogRepository catalog,  ImportDialogViewModel importDialog)
	{
		_catalog = catalog;
		ImportDialog = importDialog;
		_ = LoadAsync();
	}
	
	public event Action?  ImportDialogRequested;
	
	[RelayCommand]
	private void OpenImportDialog()
	{	
		ImportDialogRequested?.Invoke();
	}
	
	[ObservableProperty]
	private ObservableCollection<Shoot> _shoots = new();	
	
	[ObservableProperty] 
	private ObservableCollection<Category> _categories = new();

	[ObservableProperty]
	private ObservableCollection<Photo> _photos = new();

	[ObservableProperty] 
	private Shoot? _selectedShoot;

	[ObservableProperty] 
	private Photo? _selectedPhoto;
    
	[ObservableProperty]
	private ObservableCollection<CategoryTreeItemViewModel> _categoryTree = new();

	public async Task LoadAsync()
	{
		var shoots = await _catalog.GetAllShoots();
		Shoots = new ObservableCollection<Shoot>(shoots);
		
		var categories = await _catalog.GetAllCategories();
		Categories = new ObservableCollection<Category>(categories);
		CategoryTree = BuildCategoryTree(categories);
	}

	public async Task LoadPhotosForShoot(Shoot shoot)
	{
		SelectedShoot = shoot;
		var photos = await _catalog.GetPhotosInShoot(shoot.Id);
		Photos = new ObservableCollection<Photo>(photos);

	}
	
	private ObservableCollection<CategoryTreeItemViewModel> BuildCategoryTree(
		IEnumerable<Category> categories)
	{
		var allItems = categories
			.Select(c => new CategoryTreeItemViewModel(c))
			.ToList();

		var lookup = allItems.ToDictionary(i => i.Id);
		var roots = new ObservableCollection<CategoryTreeItemViewModel>();

		foreach (var item in allItems)
		{
			if (item.Category.ParentId.HasValue)
				lookup[item.Category.ParentId.Value].Children.Add(item);
			else
				roots.Add(item);
		}
        
		return roots;
	}
	
}