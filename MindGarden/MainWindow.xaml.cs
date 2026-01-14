using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;
namespace MindGarden
{
    public partial class MainWindow : Window
    {
        private bool _menuOpen = true;
        private int _plantCount = 0;
        private readonly Random _rand = new();
        private GameController? _game;
        private Dictionary<GameStage, StageConfig> _stages = null!;
        private MediaPlayer _bgMusic = new();
        private readonly List<Seed> _pendingSeeds = new();

        public MainWindow()
        {
            InitializeComponent();
            StartDayNightCycle();
            InitializeBackgroundMusic();

            ShowCalmMessage("Witaj w Mind Garden. Zacznij, gdy będziesz gotowa.");
            if (SaveManager.HasSave()) ContinueButton.Visibility = Visibility.Visible;
        }

        private void InitializeBackgroundMusic()
        {
            try
            {
                _bgMusic.Open(new Uri("Resources/music.mp3", UriKind.Relative));
                _bgMusic.MediaEnded += (s, e) =>
                {
                    _bgMusic.Position = TimeSpan.Zero;
                    _bgMusic.Play();
                };
                _bgMusic.Volume = 0.5;
                _bgMusic.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing music: {ex.Message}");
            }
        }

        private void StartNewGame(GameState? state = null)
        {
            foreach (var seed in _pendingSeeds) seed.Stop();
            _pendingSeeds.Clear();

            GardenCanvas.Children.Clear();
            _plantCount = state?.TotalPlants ?? 0;
            _stages = GamePresets.GetPresets();
            _game = new GameController(GardenCanvas, _stages, _rand, state);
            _game.StageAdvanced += () => 
            {
                ShowCalmMessage("Świetnie. Widzisz, jak ogród rośnie, kiedy dajesz mu czas.");
                SaveProgress();
            };
            _game.Finished += ShowSummary;
            _menuOpen = false;
            MenuOverlay.Visibility = Visibility.Collapsed;
            ResumeButton.Visibility = Visibility.Visible;
            ContinueButton.Visibility = Visibility.Collapsed;
            ViewGardenButton.Visibility = Visibility.Collapsed;
            UpdateHud();
            ShowCalmMessage("Skup się na światełku, kliknij i poczekaj, aż nasiono wyrośnie.");
        }

        private void GardenCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_menuOpen || _game == null) return;

            var p = e.GetPosition(GardenCanvas);
            _game.OnClick(p, pos =>
            {
                var cfg = _stages[_game.Stage];
                var baseMs = _rand.Next((int)cfg.GrowMin.TotalMilliseconds, (int)cfg.GrowMax.TotalMilliseconds);
                var growMs = (int)(baseMs / Math.Max(0.1, _game.GrowthMultiplier));
                var duration = TimeSpan.FromMilliseconds(growMs);

                var seed = new Seed(GardenCanvas, pos.X, pos.Y, _rand, duration);
                _pendingSeeds.Add(seed);
                seed.OnGrown += s => _pendingSeeds.Remove(s);
                _plantCount++;
                UpdateHud();
                ShowCalmMessage("Trafiłaś w światło. Teraz nasiono potrzebuje czasu, żeby wyrosnąć.");
                
                return duration;
            });
        }

        private void UpdateHud()
        {
            if (_game == null)
            {
                PlantCountText.Text = $"Rośliny: 0";
                return;
            }

            string stageName = _game.Stage switch
            {
                GameStage.Stage1 => "Poziom 1",
                GameStage.Stage2 => "Poziom 2",
                GameStage.Stage3 => "Poziom 3",
                GameStage.Stage4 => "Poziom 4",
                GameStage.Stage5 => "Poziom 5",
                GameStage.Finished => "Koniec",
                _ => "Poziom ?"
            };

            PlantCountText.Text =
                $"Rośliny: {_plantCount}  |  {stageName}  |  Mnożnik: {_game.GrowthMultiplier:F2}x";
        }

        private void ShowCalmMessage(string text)
        {
            CalmMessageText.Text = text;
        }

        private string GetStatsString()
        {
            var hits = _game?.Hits ?? 0;
            var misses = _game?.Misses ?? 0;
            return $"Gratulacje. Zasadziłaś {_plantCount} roślin.\n" +
                   $"Trafione kliknięcia: {hits}, impulsywne kliknięcia: {misses}.";
        }

        private void ShowSummary()
        {
            _menuOpen = true;
            MenuOverlay.Visibility = Visibility.Visible;
            if (_game != null) _game.Pause();
            ResumeButton.Visibility = Visibility.Collapsed;
            ViewGardenButton.Visibility = Visibility.Visible;
            
            ShowCalmMessage(
                $"{GetStatsString()}\n" +
                $"Zatrzymaj się na chwilę i popatrz na swój ogród."
            );
        }

        private void StartDayNightCycle()
        {
            if (GardenCanvas.Background is SolidColorBrush brush)
            {
                var anim = new ColorAnimation
                {
                    From = brush.Color,
                    To = Color.FromRgb(180, 200, 230),
                    Duration = TimeSpan.FromSeconds(20),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };
                brush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
            }
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            var state = SaveManager.Load();
            if (state != null)
            {
                StartNewGame(state);
            }
        }

        private void SaveProgress()
        {
            if (_game == null) return;
            var state = new GameState(
                _game.Stage,
                _plantCount,
                _game.GrowthMultiplier,
                _game.Hits,
                _game.Misses
            );
            SaveManager.Save(state);
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_game == null) return;
            _menuOpen = false;
            MenuOverlay.Visibility = Visibility.Collapsed;
            _game.Resume();
            ShowCalmMessage("Wróciłaś do ogrodu. Działaj tylko wtedy, gdy pojawi się światło.");
        }

        private void ViewGardenButton_Click(object sender, RoutedEventArgs e)
        {
            _menuOpen = false;
            MenuOverlay.Visibility = Visibility.Collapsed;
            ShowCalmMessage($"{GetStatsString()}\nPodziwiaj swój ogród. Naciśnij ESC, aby wrócić do menu.");
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;
            if (_game == null) return;

            if (_menuOpen)
            {
                _menuOpen = false;
                MenuOverlay.Visibility = Visibility.Collapsed;
                _game.Resume();

                if (_game.Stage == GameStage.Finished)
                {
                    ShowCalmMessage($"{GetStatsString()}\nPodziwiaj swój ogród. Naciśnij ESC, aby wrócić do menu.");
                }
                else
                {
                    ShowCalmMessage("Wróciłaś do ogrodu. Działaj tylko wtedy, gdy pojawi się światło.");
                }
            }
            else
            {
                if (_game.Stage == GameStage.Finished)
                {
                    ShowSummary();
                }
                else
                {
                    _menuOpen = true;
                    MenuOverlay.Visibility = Visibility.Visible;
                    _game.Pause();
                    ShowCalmMessage("Zatrzymałaś grę. Możesz odpocząć albo wrócić, gdy będziesz gotowa.");
                }
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_bgMusic != null)
            {
                _bgMusic.Volume = e.NewValue;
            }
        }
    }
}