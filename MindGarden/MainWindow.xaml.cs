using System.Security.AccessControl;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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
        private readonly Dictionary<GameStage, StageConfig> _cfg;
        private readonly Random _rand;
        private readonly DispatcherTimer _timer;

        public GameStage Stage { get; private set; } = GameStage.Stage1;

        public event Action? TickHappened;

        public GameController(Dictionary<GameStage, StageConfig> cfg, Random rand)
        {
            _cfg = cfg;
            _rand = rand;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += OnTick;
            _timer.Start();
        }

        private void OnTick(object? sender, EventArgs e)
        {
            TickHappened?.Invoke();
        }
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
            _game = new GameController(_stages, new Random());
            _game.TickHappened += () =>
            {
                ShowCalmMessage("To byłby moment, gdy pojawia się sygnał w grze.");
            };

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
                ShowCalmMessage("Gra jeszcze nie działa lub jest w menu.");
                return;
            }

            var p = e.GetPosition(GardenCanvas);
            ShowCalmMessage($"Kliknęłaś w ogród w punkcie ({p.X:F0}, {p.Y:F0}). Prawdziwa logika pojawi się później.");

        }
    }
}