using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Darkbox.Core.Domain;
using Darkbox.ViewModels;

namespace Darkbox.Modules.Library.ViewModels;

public partial class CategoryTreeItemViewModel : ViewModelBase
{
    public Category Category { get; }

    [ObservableProperty] private bool _isExpanded = true;
    [ObservableProperty]  private bool _isSelected;
    
    public string Name => Category.Name;
    public int Id => Category.Id;
    
    public ObservableCollection<CategoryTreeItemViewModel> Children { get; } = new();

    public CategoryTreeItemViewModel(Category category)
    {
        Category = category;
    }

}