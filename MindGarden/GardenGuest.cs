using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MindGarden
{
    public class GardenGuest
    {
        private readonly Canvas _canvas;
        private readonly Ellipse _shape;
        private Point _pos;
        private bool _active;

        public bool IsActive => _active;

        public GardenGuest(Canvas canvas)
        {
            _canvas = canvas;
            _shape = new Ellipse
            {
                Width = 20,
                Height = 20,
                IsHitTestVisible = false
            };


            var brush = new RadialGradientBrush();
            brush.GradientStops.Add(new GradientStop(Colors.White, 0.0));
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(205, 220, 57), 0.5));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 205, 220, 57), 1.0));
            _shape.Fill = brush;

            var glow = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.LimeGreen,
                BlurRadius = 15,
                ShadowDepth = 0,
                Opacity = 0.8
            };
            _shape.Effect = glow;
        }

        public void ShowAt(Point p)
        {
            _pos = p;
            _active = true;
            
            if (!_canvas.Children.Contains(_shape)) _canvas.Children.Add(_shape);
            Canvas.SetLeft(_shape, p.X - 10);
            Canvas.SetTop(_shape, p.Y - 10);


            var bob = new DoubleAnimation
            {
                From = p.Y - 10,
                To = p.Y - 15,
                Duration = TimeSpan.FromSeconds(1.5),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            _shape.BeginAnimation(Canvas.TopProperty, bob);


            _shape.Opacity = 0;
            var fade = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
            _shape.BeginAnimation(UIElement.OpacityProperty, fade);
        }

        public void Flee()
        {
            if (!_active) return;
            _active = false;


            var flyX = new DoubleAnimation(_pos.X + 100, TimeSpan.FromSeconds(0.5));
            var flyY = new DoubleAnimation(_pos.Y - 100, TimeSpan.FromSeconds(0.5));
            var fade = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));

            fade.Completed += (s, e) => 
            {
                if (_canvas.Children.Contains(_shape)) _canvas.Children.Remove(_shape);
                _shape.BeginAnimation(Canvas.LeftProperty, null);
                _shape.BeginAnimation(Canvas.TopProperty, null);
            };

            _shape.BeginAnimation(Canvas.LeftProperty, flyX);
            _shape.BeginAnimation(Canvas.TopProperty, flyY);
            _shape.BeginAnimation(UIElement.OpacityProperty, fade);
        }

        public void Success()
        {
            if (!_active) return;
            _active = false;


            var scale = new ScaleTransform(1, 1);
            _shape.RenderTransformOrigin = new Point(0.5, 0.5);
            _shape.RenderTransform = scale;

            var grow = new DoubleAnimation(2.0, TimeSpan.FromSeconds(0.4));
            var fade = new DoubleAnimation(0, TimeSpan.FromSeconds(0.4));

            fade.Completed += (s, e) => 
            {
                if (_canvas.Children.Contains(_shape)) _canvas.Children.Remove(_shape);
                _shape.RenderTransform = null;
            };

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, grow);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, grow);
            _shape.BeginAnimation(UIElement.OpacityProperty, fade);
        }
    }
}
