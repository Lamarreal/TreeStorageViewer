using System.Windows.Controls;
using StorageApp.Attributes;
using StorageApp.Interfaces;

namespace StorageApp.Classes.ContextMenuObjects;

[ContextTab("Edit")]
public class ClearCache : IContextButtonObject
{
    public static string Header { get; set; } =  "DoesntWork right now";
    public static string IconPath { get; set; } = "Images/TrashCan.png";
    
    public ClearCache(MenuItem CorespondingItem)
    {
        var itm = DropDownMenu.CreateDropDownItem("-> clean temp files", CorespondingItem);
        CorespondingItem.Items.Add(new Separator());

        itm.Click += (e, o) =>
        {
            Console.WriteLine("Temp");
        };
    }
    
    public void Action()
    {
        // execute action on click
    }
}