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

        var brush = new SolidColorBrush(Colors.SaddleBrown);
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
        var brush = (SolidColorBrush)_ellipse.Fill;
        var to = Color.FromRgb(96, 160, 80);
        var colorAnim = new ColorAnimation
        {
            To = to,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };
        brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

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

    private void Grow(object? sender, EventArgs e)
    {
        _sproutTimer.Stop();
        _preCueTimer.Stop();
        StopShake();

        _canvas.Children.Remove(_ellipse);

        byte[][] choices = { Assets.grass, Assets.plant, Assets.tree };
        byte[] blob = choices[_rand.Next(choices.Length)];

        var img = new Image
        {
            Width = 64,
            Height = 64,
            Stretch = Stretch.Uniform,
            Source = LoadFromBytes(blob),
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
    }
    private static ImageSource LoadFromBytes(byte[] data)
    {
        using var ms = new MemoryStream(data);
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.StreamSource = ms;
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }
}