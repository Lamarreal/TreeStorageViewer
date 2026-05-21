using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StorageApp.Classes;

public partial class TitleBarButton : UserControl
{
    public event Action Clicked;
    
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(TitleBarButton), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty HasDropdownProperty =
        DependencyProperty.Register(nameof(HasDropdown), typeof(bool), typeof(TitleBarButton), new PropertyMetadata(false));
    
    public static readonly DependencyProperty IsHighlightedProperty =
        DependencyProperty.Register(nameof(IsHighlighted), typeof(bool), typeof(TitleBarButton), new PropertyMetadata(false));
    
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool HasDropdown
    {
        get => (bool)GetValue(HasDropdownProperty);
        set => SetValue(HasDropdownProperty, value);
    }
    
    public bool IsHighlighted 
    {
        get => (bool)GetValue(IsHighlightedProperty);
        set => SetValue(IsHighlightedProperty, value);
    }
    
    public TitleBarButton(string label) : this()
    {
        Title = label;
    }
    
    public TitleBarButton()
    {
        InitializeComponent();
    }

    private void Bg_MouseEnter(object sender, MouseEventArgs e)
    {
        IsHighlighted = true;
    }

    private void Bg_MouseLeave(object sender, MouseEventArgs e)
    {
        IsHighlighted = false;
    }
       
    private void Bg_MouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        Clicked?.Invoke();
        
        if (this.ContextMenu != null)
        {
            this.ContextMenu.PlacementTarget = this;
            this.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            this.ContextMenu.StaysOpen = true;
            this.ContextMenu.IsOpen = true;
        }
    }
}