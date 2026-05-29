using System.Reflection;
using System.Windows.Controls;
using StorageApp.Attributes;
using StorageApp.Interfaces;

namespace StorageApp.Classes;

public class ContextMenuBuilder
{
    
    private readonly MainWindow _mainWindow;

  
    public ContextMenuBuilder(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }
    
    public void BuildDynamicMenus()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes() 
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IContextButtonObject).IsAssignableFrom(t));
        
        var createdTabs = new Dictionary<string, TitleBarButton>();
        
        foreach (var type in types)
        {
            var attr = type.GetCustomAttribute<ContextTabAttribute>();
            
            if (attr == null) continue;
            
            string tabName = attr.TabName;
            
            if (!createdTabs.TryGetValue(tabName, out TitleBarButton topLevelButton))
            {
                topLevelButton =  new TitleBarButton(tabName);
                _mainWindow.TitleBarButtons.Add(topLevelButton);
                createdTabs[tabName] = topLevelButton;
            }
            
            string header = type.GetProperty("Header")?.GetValue(null) as string ?? type.Name;
            string iconPath = type.GetProperty("IconPath")?.GetValue(null) as string;
            
            MenuItem dropDownItem = DropDownMenu.CreateDropDownItem(header, topLevelButton, iconPath);
            
            object[] constructorArgs = new object[] { dropDownItem };
            IContextButtonObject actionInstance = (IContextButtonObject)Activator.CreateInstance(type, constructorArgs);
            
            dropDownItem.Click += (s, e) => { actionInstance.Action(); };
        }
    }
}