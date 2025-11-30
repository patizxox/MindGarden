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
            _shape.Fill = new SolidColorBrush(Color.FromArgb(90, 255, 255, 180));
            _shape.Stroke = Brushes.White;
            _shape.StrokeThickness = 1.5;
            _shape.IsHitTestVisible = false;
        }

        public void ShowAt(Point p, double radius = 40)
        {
            _pos = p;
            _radius = radius;
            _shape.Width = _shape.Height = radius * 2;
            if (!_canvas.Children.Contains(_shape)) _canvas.Children.Add(_shape);
            Canvas.SetLeft(_shape, p.X - radius);
            Canvas.SetTop(_shape, p.Y - radius);

            var brush = (SolidColorBrush)_shape.Fill;
            var anim = new DoubleAnimation
            {
                From = 0.35,
                To = 0.75,
                Duration = TimeSpan.FromMilliseconds(900),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            brush.BeginAnimation(SolidColorBrush.OpacityProperty, anim);
        }

        public void Hide()
        {
            if (_canvas.Children.Contains(_shape)) _canvas.Children.Remove(_shape);
        }

        public bool Hit(Point click) =>
            (click.X - _pos.X) * (click.X - _pos.X) + (click.Y - _pos.Y) * (click.Y - _pos.Y) <= _radius * _radius;
    }

}