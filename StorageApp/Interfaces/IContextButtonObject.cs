using System.Windows.Controls;

namespace StorageApp.Interfaces;

public interface IContextButtonObject
{
    public static string Header  { get; set; }
    public static string IconPath { get; }
    public void Action();

}