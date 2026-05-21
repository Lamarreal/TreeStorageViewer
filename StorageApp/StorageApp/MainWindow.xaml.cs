using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using StorageApp.Classes;
using StorageApp.Pages;
namespace StorageApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public ObservableCollection<TitleBarButton> TitleBarButtons { get; set; }
    
    
    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = this; 
        
        TitleBarButtons = new ObservableCollection<TitleBarButton>();
    }
    
    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            var window = Window.GetWindow(this);
            if (window == null) return;
            
            if (e.ClickCount == 2)
            {
                ToggleMaximize(window);
                return;
            }
            
            if (window.WindowState == WindowState.Maximized)
            {
                Point screenMousePos = window.PointToScreen(e.GetPosition(window));
                Point relativeMousePos = e.GetPosition(window);
                
                double restoreWidth = window.RestoreBounds.Width;
                if (restoreWidth <= 0) restoreWidth = window.Width; 
                
                double targetX = relativeMousePos.X * (restoreWidth / window.ActualWidth);
                double targetY = relativeMousePos.Y;
                
                window.WindowState = WindowState.Normal;
                window.UpdateLayout();
                
                window.Left = screenMousePos.X - targetX;
                window.Top = screenMousePos.Y - targetY;
            }
            
            try
            {
                window.DragMove();
            }
            catch (InvalidOperationException ex)
            {
               Console.WriteLine(ex.Message);
            }
        }
    }

    public TitleBarButton CreateTitleButton(string Title, UserControl Page)
    {
        TitleBarButton Button = new TitleBarButton(Title) {HasDropdown =  false};
        Button.Clicked += () =>
        {
            PageContent.Content = Page;
        };
        TitleBarButtons.Add(Button);
        return Button;
    }

    private void PopulateNavBar()
    {
        //----------------------------------------------
        
        UserControl BasePage = new HomeScreen();
        PageContent.Content = BasePage;
        CreateTitleButton("Home page", BasePage);
        
        //----------------------------------------------

        ContextMenuBuilder Builder = new ContextMenuBuilder(this);
        Builder.BuildDynamicMenus();
        
        //----------------------------------------------
        
    }
    
    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        PopulateNavBar();
    }
    
   

    public void MaxButton(object sender, MouseButtonEventArgs e)
    {
        var window = Window.GetWindow(this);
        if (window != null)
        {
            ToggleMaximize(window);
            e.Handled = true;
        }
    }
    
    private void ToggleMaximize(Window window)
    {
        if (window.WindowState == WindowState.Maximized)
        {
            window.WindowState = WindowState.Normal;
        }
        else
        {
            window.WindowState = WindowState.Maximized;
        }
    }
    
    public void MinButton(object sender, MouseButtonEventArgs e)
    {
        var window = Window.GetWindow(this);
        if (window != null)
        {
            window.WindowState = WindowState.Minimized;
            e.Handled = true;
        }
    }
    
    public void CloseButton(object sender, MouseButtonEventArgs e)
    {
        App.Current.Shutdown();
    }
}