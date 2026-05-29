using Microsoft.VisualBasic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace StorageApp.Classes;

public static class DropDownMenu
{
    private static MenuItem GenericDropItem(string label, ItemsControl Parent, string ImagePath = null)
    {
        MenuItem NewItem = new MenuItem() { Header = label };
        Parent.Items.Add(NewItem);
        
        if (ImagePath != null)
        {
            var iconImage = new Image
            {
                Source = new BitmapImage(new Uri(ImagePath,UriKind.Relative)),
                Width = 16,
                Height = 16
            };
            NewItem.Icon = iconImage;
        }
        
        return NewItem;
    }
    public static MenuItem CreateDropDownItem(string label, UserControl Parent, string ImagePath = null)
    {
        if (Parent.ContextMenu == null)
            Parent.ContextMenu = new ContextMenu();
        return GenericDropItem(label,  Parent.ContextMenu, ImagePath);
    }
    public static MenuItem CreateDropDownItem(string label, ContextMenu Parent, string ImagePath = null)
    {
        return GenericDropItem(label,  Parent, ImagePath);
    }
    
    public static MenuItem CreateDropDownItem(string label, MenuItem Parent, string ImagePath = null)
    {
        return GenericDropItem(label,  Parent, ImagePath);
    }
}