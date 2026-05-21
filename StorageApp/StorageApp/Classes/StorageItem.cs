using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace StorageApp.Classes;
public enum StorageItemType
{
    File,
    Folder,
    Video,
    Image,
    Audio
}
public class StorageItem : INotifyPropertyChanged
{
    private string _name;
    private string _sizeDisplay;
    private string _allocatedDisplay;
    private long _filesCount;
    private long _foldersCount;
    private double _percentage;
    private double _sizePercentage;
    private string _dateModified;
    private long _sizeInBytes;

    public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
    public string Path { get; set; }
    public StorageItemType _type;
    public StorageItemType Type { 
        get => _type;
        set 
        {
            _type = value;
            OnPropertyChanged();
        }
    }

    public long SizeInBytes 
    { 
        get => _sizeInBytes; 
        set { _sizeInBytes = value; OnPropertyChanged(); } 
    }

    public string SizeDisplay 
    { 
        get => _sizeDisplay; 
        set { _sizeDisplay = value; OnPropertyChanged(); } 
    }

    public string AllocatedDisplay { get => _allocatedDisplay; set { _allocatedDisplay = value; OnPropertyChanged(); } }
    
    public long FilesCount { get => _filesCount; set { _filesCount = value; OnPropertyChanged(); } }
    public long FoldersCount { get => _foldersCount; set { _foldersCount = value; OnPropertyChanged(); } }
    public double Percentage
    {
        get
        {
            
            if (Parent == null || Parent.SizeInBytes == 0) 
                return 100.0;

          
            double pct = ((double)SizeInBytes / Parent.SizeInBytes) * 100;

        
            if (pct > 100) return 100.0;
        
            return pct;
        }
    }
    public double SizePercentage
    {
        get
        {
            try
            {
                if (!string.IsNullOrEmpty(Path) && Path.EndsWith(":\\") || Path.EndsWith(":/"))
                {
                    var drive = new DriveInfo(Path);
                    if (drive.IsReady)
                    {
                        long totalSpace = drive.TotalSize;
                        long usedSpace = totalSpace - drive.TotalFreeSpace;
                        return ((double)usedSpace / totalSpace) * 100;
                    }
                }
            }
            catch { }
            
            return 0.0;
        }
    }
    
    public string DateModified { get => _dateModified; set { _dateModified = value; OnPropertyChanged(); } }
    
    public StorageItem Parent { get; set; }

    public ObservableCollection<StorageItem> Children { get; set; } = new ObservableCollection<StorageItem>();

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}