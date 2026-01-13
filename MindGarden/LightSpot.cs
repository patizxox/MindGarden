using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MindGarden
{
    public class LightSpot
    {
        private readonly Canvas _canvas;
        private readonly Ellipse _shape = new();
        private Point _pos;
        private double _radius;

        public Rect Bounds => new(_pos.X - _radius, _pos.Y - _radius, _radius * 2, _radius * 2);

        public LightSpot(Canvas canvas)
        {
            _canvas = canvas;
            _shape.Width = _shape.Height = 80;

            var brush = new RadialGradientBrush();
            brush.GradientStops.Add(new GradientStop(Colors.White, 0.0));
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(255, 238, 88), 0.4));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 255, 215, 0), 1.0));
            _shape.Fill = brush;

            _shape.Stroke = null;
            _shape.IsHitTestVisible = false;

            var glow = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Gold,
                BlurRadius = 30,
                ShadowDepth = 0,
                Opacity = 0.8
            };
            _shape.Effect = glow;
        }

        public void ShowAt(Point p, double radius = 40)
        {
            _pos = p;
            _radius = radius;
            _shape.Width = _shape.Height = radius * 2.5;
            if (!_canvas.Children.Contains(_shape)) _canvas.Children.Add(_shape);
            Canvas.SetLeft(_shape, p.X - _shape.Width / 2);
            Canvas.SetTop(_shape, p.Y - _shape.Height / 2);

            var anim = new DoubleAnimation
            {
                From = 0.6,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(800),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            _shape.BeginAnimation(UIElement.OpacityProperty, anim);
        }

        public void Hide()
        {
            if (_canvas.Children.Contains(_shape)) _canvas.Children.Remove(_shape);
            _shape.BeginAnimation(UIElement.OpacityProperty, null);
        }

        public bool Hit(Point click) =>
            (click.X - _pos.X) * (click.X - _pos.X) + (click.Y - _pos.Y) * (click.Y - _pos.Y) <= _radius * _radius;
    }

}