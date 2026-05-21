using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using StorageApp.Attributes;
using StorageApp.Interfaces;
using StorageApp.Pages;

namespace StorageApp.Classes.ContextMenuObjects;

[ContextTab("File")]
public class SelectDrive : IContextButtonObject
{
    public static string Header { get; set; } =  "Select Directory";
    public static string IconPath { get; set; } = "Images/HardDrive.png";
    
    public SelectDrive(MenuItem CorespondingItem)
    {
       
    }
    
    public void Action()
    {
        var folderDialog = new OpenFolderDialog();
    
      
        folderDialog.Title = "Select a Storage Directory";
        folderDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
        
        if (folderDialog.ShowDialog() == true)
        {
            string selectedPath = folderDialog.FolderName;
            
            Console.WriteLine($"Selected Folder: {selectedPath}");
            if (Directory.Exists(selectedPath))
            {
                if (Application.Current.MainWindow is MainWindow mainWin)
                {
                   if (mainWin.PageContent.Content != null && mainWin.PageContent.Content is HomeScreen HomeScreen)
                       HomeScreen.LoadDirectory(selectedPath);
                }
            }
        }
    }
}