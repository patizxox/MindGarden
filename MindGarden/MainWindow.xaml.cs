using System.Security.AccessControl;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MindGarden
{
    public enum GameStage { Stage1, Stage2, Stage3, Finished }
    public record StageConfig(
    TimeSpan LightMinDelay,
    TimeSpan LightMaxDelay,
    int LightsPerStage,
    TimeSpan GrowMin,
    TimeSpan GrowMax
    );
    public static class GamePresets
    {
        public static Dictionary<GameStage, StageConfig> GetPresets()
        {
            var stages = new Dictionary<GameStage, StageConfig>
            {
                [GameStage.Stage1] = new(TimeSpan.FromSeconds(1.2), TimeSpan.FromSeconds(2.5), 5, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5)),
                [GameStage.Stage2] = new(TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(3.5), 7, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(7)),
                [GameStage.Stage3] = new(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(4.5), 9, TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(10)),
            };

            return stages;
        }
    }

    public class GameController
    {
        private readonly Canvas _canvas;
        private readonly Dictionary<GameStage, StageConfig> _cfg;
        private readonly Random _rand;
        private readonly LightSpot _spot;

        public GameStage Stage { get; private set; } = GameStage.Stage1;

        public GameController(Canvas canvas, Dictionary<GameStage, StageConfig> cfg, Random rand)
        {
            _canvas = canvas;
            _cfg = cfg;
            _rand = rand;
            _spot = new LightSpot(canvas);
            ScheduleNextSpot();
        }

        public void OnClick(Point p, Action<Point> onPlant)
        {
            if (_spot.Hit(p))
            {
                onPlant(p);
                ScheduleNextSpot();
            }
            else
            {
                // na razie nic
            }
        }

        private async void ScheduleNextSpot()
        {
            _spot.Hide();
            var s = _cfg[Stage];
            var delay = _rand.Next((int)s.LightMinDelay.TotalMilliseconds, (int)s.LightMaxDelay.TotalMilliseconds);
            await Task.Delay(delay);

            double r = 40;
            double x = _rand.Next((int)r, (int)Math.Max(r, _canvas.ActualWidth - r));
            double y = _rand.Next((int)(r + 40), (int)Math.Max(r + 40, _canvas.ActualHeight - r));

            _spot.ShowAt(new Point(x, y), r);
        }
    }
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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _menuOpen = true;
        private GameController? _game;
        private Dictionary<GameStage, StageConfig> _stages = null!;
        public MainWindow()
        {
            InitializeComponent();
            ShowCalmMessage("Witaj w Mind Garden. Zacznij, gdy będziesz gotowa.");
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            _menuOpen = false;
            MenuOverlay.Visibility = Visibility.Collapsed;
            ResumeButton.Visibility = Visibility.Visible;

            _stages = GamePresets.GetPresets();
            _game = new GameController(GardenCanvas, _stages, new Random());

            ShowCalmMessage("Rozpoczynasz pierwszy etap. Poczekaj na sygnał.");
        }


        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ResumeButton_Click(Object sender, RoutedEventArgs e)
        {
            _menuOpen = false;
            MenuOverlay.Visibility = Visibility.Collapsed;
            ShowCalmMessage("Wróciłaś do ogrodu.");
        }

        private void ShowCalmMessage(string text)
        {
            CalmMessageText.Text = text;
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;

            if (_menuOpen)
            {
                _menuOpen = false;
                MenuOverlay.Visibility = Visibility.Collapsed;
                ShowCalmMessage("Wróciłaś do ogrodu.");
            }
            else
            {
                _menuOpen = true;
                MenuOverlay.Visibility = Visibility.Visible;
                ShowCalmMessage("Zatrzymałaś grę. Możesz odpocząć albo wrócić, gdy będziesz gotowa.");
            }
        }

        private void GardenCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(_menuOpen || _game == null)
            {
                
                return;
            }

            var p = e.GetPosition(GardenCanvas);
            _game.OnClick(p, pos =>
            {
                ShowCalmMessage("Kliknięto w światełko");
            });

        }
    }

}