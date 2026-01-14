using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using System.Windows.Media.Imaging;
using MindGarden;

internal class Seed
{
    private readonly Canvas _canvas;
    private readonly double _x;
    private readonly double _y;
    private readonly Random _rand;

    private readonly Ellipse _ellipse;
    private readonly DispatcherTimer _sproutTimer;
    private readonly DispatcherTimer _preCueTimer;

    private readonly TranslateTransform _tt = new TranslateTransform();
    private readonly RotateTransform _rt = new RotateTransform(0);
    private readonly ScaleTransform _st = new ScaleTransform(1, 1);

    public Seed(Canvas canvas, double x, double y, Random rand, TimeSpan growTime)
    {
        _canvas = canvas;
        _x = x;
        _y = y;
        _rand = rand;


        var brush = new RadialGradientBrush();
        brush.GradientOrigin = new System.Windows.Point(0.3, 0.3);
        brush.Center = new System.Windows.Point(0.5, 0.5);
        brush.RadiusX = 0.5;
        brush.RadiusY = 0.5;
        brush.GradientStops.Add(new GradientStop(Color.FromRgb(161, 136, 127), 0.0));
        brush.GradientStops.Add(new GradientStop(Color.FromRgb(93, 64, 55), 1.0));
        _ellipse = new Ellipse
        {
            Width = 10,
            Height = 15,
            Fill = brush,
            RenderTransformOrigin = new System.Windows.Point(0.5, 0.5),
            RenderTransform = new TransformGroup
            {
                Children = new TransformCollection { _st, _rt, _tt }
            }
        };

        Canvas.SetLeft(_ellipse, x - _ellipse.Width / 2);
        Canvas.SetTop(_ellipse, y - _ellipse.Height / 2);
        _canvas.Children.Add(_ellipse);

        int totalMs = (int)growTime.TotalMilliseconds;
        int preCueMs = Math.Max(200, totalMs - 500);

        StartShake(1.5, 140);

        _preCueTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(preCueMs) };
        _preCueTimer.Tick += (s, e) =>
        {
            _preCueTimer.Stop();
            PreSproutCue();
        };
        _preCueTimer.Start();

        _sproutTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(totalMs) };
        _sproutTimer.Tick += Grow;
        _sproutTimer.Start();
    }

    private void StartShake(double amplitudePx, double periodMs)
    {
        var shakeX = new DoubleAnimation
        {
            From = -amplitudePx,
            To = amplitudePx,
            Duration = TimeSpan.FromMilliseconds(periodMs),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        _tt.BeginAnimation(TranslateTransform.XProperty, shakeX);

        var rot = new DoubleAnimation
        {
            From = -3,
            To = 3,
            Duration = TimeSpan.FromMilliseconds(periodMs * 1.4),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        _rt.BeginAnimation(RotateTransform.AngleProperty, rot);
    }

    private void StopShake()
    {
        _tt.BeginAnimation(TranslateTransform.XProperty, null);
        _rt.BeginAnimation(RotateTransform.AngleProperty, null);
    }

    private void PreSproutCue()
    {
        var brush = (RadialGradientBrush)_ellipse.Fill;
        var to = Color.FromRgb(96, 160, 80);


        var colorAnim = new ColorAnimation
        {
            To = to,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };
        brush.GradientStops[0].BeginAnimation(GradientStop.ColorProperty, colorAnim);


        var colorAnim2 = new ColorAnimation
        {
            To = Color.FromRgb(50, 90, 40),
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };
        brush.GradientStops[1].BeginAnimation(GradientStop.ColorProperty, colorAnim2);

        var scaleUp = new DoubleAnimation
        {
            From = 1.0,
            To = 1.12,
            Duration = TimeSpan.FromMilliseconds(180),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(2),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        _st.BeginAnimation(ScaleTransform.ScaleXProperty, scaleUp);
        _st.BeginAnimation(ScaleTransform.ScaleYProperty, scaleUp);

        StartShake(3.0, 100);
    }

    public void Stop()
    {
        _sproutTimer.Stop();
        _preCueTimer.Stop();
        StopShake();
        if (_canvas.Children.Contains(_ellipse)) _canvas.Children.Remove(_ellipse);
    }

    private void Grow(object? sender, EventArgs e)
    {
        _sproutTimer.Stop();
        _preCueTimer.Stop();
        StopShake();

        if (_canvas.Children.Contains(_ellipse)) _canvas.Children.Remove(_ellipse);

        string[] files = Directory.GetFiles(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources"), "*.png");
        string randomFile = files[_rand.Next(files.Length)];

        var img = new Image
        {
            Width = 64,
            Height = 64,
            Stretch = Stretch.Uniform,
            Source = LoadFromFile(randomFile),
            SnapsToDevicePixels = true,
            RenderTransformOrigin = new System.Windows.Point(0.5, 0.5),
            RenderTransform = new ScaleTransform(0, 0)
        };

        Canvas.SetLeft(img, _x - img.Width / 2);
        Canvas.SetTop(img, _y - img.Height / 2);
        _canvas.Children.Add(img);

        var pop = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(220),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.6 }
        };
        ((ScaleTransform)img.RenderTransform).BeginAnimation(ScaleTransform.ScaleXProperty, pop);
        ((ScaleTransform)img.RenderTransform).BeginAnimation(ScaleTransform.ScaleYProperty, pop);
        
        OnGrown?.Invoke(this);
    }
    
    public event Action<Seed>? OnGrown;

    private static ImageSource LoadFromFile(string path)
    {
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.UriSource = new Uri(path, UriKind.Absolute);
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }
}