using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StorageApp.Classes;

namespace StorageApp.Pages;

public partial class HomeScreen : UserControl, INotifyPropertyChanged
{
    public ObservableCollection<StorageItem> StorageItems { get; set; }
    
    private CancellationTokenSource _cts;
    private string LastPath { get; set; }

    private static readonly HashSet<string> SkippedSystemFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
    { 
        "pagefile.sys", "hiberfil.sys", "swapfile.sys", "dumpstack.log"
    };
    
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
    }

    public HomeScreen(string Path = null)
    {
        InitializeComponent();
        StorageItems = new ObservableCollection<StorageItem>();
        DataContext = this;
        
       if (Path !=  null)
           _ = LoadDirectory(Path);
    }

    private static string FormatSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < suffixes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:F1} {suffixes[order]}";
    }

    public async Task LoadDirectory(string path)
    {
        if (!Directory.Exists(path)) return;
        
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        LastPath = path;
        StorageItems.Clear();

        try
        {
            var dirInfo = new DirectoryInfo(path);
            long driveTotalSize = 0;
            
            try
            {
                DriveInfo drive = new DriveInfo(dirInfo.Root.FullName);
                if (drive.IsReady)
                    driveTotalSize = drive.TotalSize;
            }
            catch { /* Ignored */ }

            IsLoading = true;
            var rootNode = await Task.Run(() => AnalyzeDirectoryTree(dirInfo, null, token), token);
            
            if (rootNode != null && !token.IsCancellationRequested)
            {
                rootNode.AllocatedDisplay = FormatSize(driveTotalSize);
                StorageItems.Add(rootNode);
            }
            IsLoading = false;
            
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Directory scan was canceled.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Scan Crashed: {ex.Message}");
        }
    }
    
    private StorageItem AnalyzeDirectoryTree(DirectoryInfo dirInfo, StorageItem parentNode, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var folderItem = new StorageItem
        {
            Name = string.IsNullOrEmpty(dirInfo.Name) ? dirInfo.FullName : dirInfo.Name,
            Path = dirInfo.FullName,
            Type = StorageItemType.Folder,
            DateModified = dirInfo.LastWriteTime.ToString("dd.MM.yyyy"),
            Parent = parentNode
        };

        long localTotalSize = 0;
        long localFilesCount = 0;
        int localFoldersCount = 0;
        
        var childrenList = new List<StorageItem>();

        try
        {
           
            foreach (var file in dirInfo.EnumerateFiles())
            {
                token.ThrowIfCancellationRequested();
                if (SkippedSystemFiles.Contains(file.Name)) continue;

                long fileLength = file.Length;
                childrenList.Add(new StorageItem
                {
                    Name = file.Name,
                    Path = file.FullName,
                    Type = StorageItemType.File,
                    SizeInBytes = fileLength,
                    SizeDisplay = FormatSize(fileLength),
                    DateModified = file.LastWriteTime.ToString("dd.MM.yyyy"),
                    Parent = folderItem
                });
                
                localTotalSize += fileLength;
                localFilesCount++;
            }

            
            foreach (var subDir in dirInfo.EnumerateDirectories())
            {
                token.ThrowIfCancellationRequested();
                var childFolder = AnalyzeDirectoryTree(subDir, folderItem, token);
                
                if (childFolder != null)
                {
                    childrenList.Add(childFolder);
                 
                    localTotalSize += childFolder.SizeInBytes;
                    localFilesCount += childFolder.FilesCount;
                    localFoldersCount++;
                }
            }
        }
        catch (UnauthorizedAccessException) { }
        catch (DirectoryNotFoundException) { }
        catch (PathTooLongException) { }

        childrenList.Sort((a, b) =>
        {
           
            int folderCmp = (b.Type == StorageItemType.Folder).CompareTo((a.Type == StorageItemType.Folder));
            if (folderCmp != 0) return folderCmp;
            
            return b.SizeInBytes.CompareTo(a.SizeInBytes);
        });

       
        foreach (var child in childrenList)
        {
            folderItem.Children.Add(child);
        }

        folderItem.SizeInBytes = localTotalSize;
        folderItem.SizeDisplay = FormatSize(localTotalSize);
        folderItem.FilesCount = localFilesCount;
        folderItem.FoldersCount = localFoldersCount;

        return folderItem;
    }

    private void OpenNewWindow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.Tag is string itemPath)
        {
            if (!Directory.Exists(itemPath)) return;
            
           
            _cts?.Cancel();

            if (Application.Current.MainWindow is MainWindow mainWin)
            {
                HomeScreen HM = new HomeScreen(itemPath);
                mainWin.CreateTitleButton(itemPath, HM);
                mainWin.PageContent.Content = HM;
            }
        }
    }

    private void ItemRight_Click(object sender, RoutedEventArgs e)
    {
        if (sender is TreeViewItem treeViewItem)
        {
            treeViewItem.IsSelected = true;
            treeViewItem.Focus();
            e.Handled = true; 
        }
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.Tag is string itemPath)
        {
            if (!(Directory.Exists(itemPath) | File.Exists(itemPath))) return;
            Process.Start("explorer.exe", $"/select,\"{itemPath}\"");
        }
    }

    private void OpenProperties_Click(object sender, RoutedEventArgs e)
    { 
        
        if (sender is MenuItem menuItem && menuItem.Tag is string itemPath)
        {
            
           
        }
    }

    private void Reload_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (LastPath != null && LastPath.Length > 0)
        {
            LoadDirectory(LastPath);
        }
    }
}