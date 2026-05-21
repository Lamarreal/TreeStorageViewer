using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using StorageApp.Classes;
using System.IO;

public class FileIconConverter : IMultiValueConverter
{
    #region P/Invoke for Standard Icons (Fallback)
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    private const uint SHGFI_ICON = 0x000000100;
    private const uint SHGFI_SMALLICON = 0x000000001;
    private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
    private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
    #endregion

    #region P/Invoke for Image Thumbnails (IShellItemImageFactory)
    [ComImport]
    [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IShellItemImageFactory
    {
        [PreserveSig]
        int GetImage([In] SIZE size, [In] SIIGBF flags, [Out] out IntPtr phbm);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SIZE
    {
        public int cx;
        public int cy;
        public SIZE(int cx, int cy) { this.cx = cx; this.cy = cy; }
    }

    [Flags]
    public enum SIIGBF
    {
        SIIGBF_RESIZETOFIT = 0x00,
        SIIGBF_BIGGERSIZEOK = 0x01,
        SIIGBF_MEMORYONLY = 0x02,
        SIIGBF_ICONONLY = 0x04,
        SIIGBF_THUMBNAILONLY = 0x08,
        SIIGBF_INCACHEONLY = 0x10,
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    public static extern void SHCreateItemFromParsingName(
        [In][MarshalAs(UnmanagedType.LPWStr)] string pszPath,
        [In] IntPtr pbc,
        [In][MarshalAs(UnmanagedType.LPStruct)] Guid riid,
        [Out][MarshalAs(UnmanagedType.Interface, IidParameterIndex = 2)] out IShellItemImageFactory ppv);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool DeleteObject(IntPtr hObject);
    #endregion

    private static readonly string[] AllowedThumbnailExtensions = new[] 
    { 
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".ico" 
    };

    private ImageSource GetThumbnailOrIcon(string path)
    {
        string extension = Path.GetExtension(path);
    
        if (!string.IsNullOrEmpty(extension) && 
            AllowedThumbnailExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) && 
            File.Exists(path))
        {
            try
            {
                Guid iid = new Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b");
                SHCreateItemFromParsingName(path, IntPtr.Zero, iid, out IShellItemImageFactory factory);

                if (factory != null)
                {
                    IntPtr hBitmap = IntPtr.Zero;
                    try
                    {
                        factory.GetImage(new SIZE(64, 64), SIIGBF.SIIGBF_RESIZETOFIT, out hBitmap);
                    
                        if (hBitmap != IntPtr.Zero)
                        {
                            var bitmap = Imaging.CreateBitmapSourceFromHBitmap(
                                hBitmap,
                                IntPtr.Zero,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions());

                            bitmap.Freeze();
                            return bitmap;
                        }
                    }
                    finally
                    {
                        if (hBitmap != IntPtr.Zero) DeleteObject(hBitmap);
                        Marshal.ReleaseComObject(factory);
                    }
                }
            }
            catch
            {
               
            }
        }

        return GetIconFromPath(path);
    }

    private ImageSource GetIconFromPath(string path)
    {
        SHFILEINFO shinfo = new SHFILEINFO();
        uint flags = SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES;

        IntPtr result = SHGetFileInfo(path, FILE_ATTRIBUTE_NORMAL, out shinfo, (uint)Marshal.SizeOf(shinfo), flags);

        if (shinfo.hIcon == IntPtr.Zero) return null;

        try
        {
            var bitmap = Imaging.CreateBitmapSourceFromHIcon(
                shinfo.hIcon,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            bitmap.Freeze();
            return bitmap;
        }
        finally
        {
            DestroyIcon(shinfo.hIcon);
        }
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var path = values[0] as string;
        var type = (StorageItemType)values[1];
        
        if (type == StorageItemType.Folder)
        {
            return new BitmapImage(new Uri("pack://application:,,,/Images/Folder.png"));
        }
        
        if (type == StorageItemType.File && !string.IsNullOrEmpty(path))
        {
            return GetThumbnailOrIcon(path); 
        }

        return null;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}