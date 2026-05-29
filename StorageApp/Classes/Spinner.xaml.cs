using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
namespace StorageApp.Classes;

public partial class Spinner : UserControl
{
    public  Spinner()
    {
        InitializeComponent();
        
        this.Loaded += (s, e) => StartAnimations();
    }
    
    private void StartAnimations()
    {
      
        var duration = TimeSpan.FromMilliseconds(600);
        
        void SetupBounce(System.Windows.Shapes.Ellipse target, double startTimeMs)
        {
            var animation = new DoubleAnimationUsingKeyFrames();
            animation.RepeatBehavior = RepeatBehavior.Forever;
            animation.BeginTime = TimeSpan.FromMilliseconds(startTimeMs);

          
            animation.KeyFrames.Add(new SplineDoubleKeyFrame(15, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))));

           
            animation.KeyFrames.Add(new SplineDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.3)), 
                new KeySpline(0.33, 0.66, 0.66, 1.0)));

           
            animation.KeyFrames.Add(new SplineDoubleKeyFrame(15, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.6)), 
                new KeySpline(0.33, 0, 0.66, 0.33)));

           
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Canvas.Top)"));

           
            var sb = new Storyboard();
            sb.Children.Add(animation);
            sb.Begin();
        }
        
        SetupBounce(Dot1, 0);
        SetupBounce(Dot2, 100);
        SetupBounce(Dot3, 200);
    }
}