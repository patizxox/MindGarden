using System.Windows;
using System.Windows.Input;
namespace MindGarden
{
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