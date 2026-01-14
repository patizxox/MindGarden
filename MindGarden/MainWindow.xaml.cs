using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;
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
            if (SaveManager.HasSave()) 
            {
                ContinueButton.Visibility = Visibility.Visible;
                JournalButton.Visibility = Visibility.Visible;
            }
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
            _game.QuoteUnlocked += OnQuoteUnlocked;
            _game.GuestInteraction += OnGuestInteraction;

            _menuOpen = false;
            MenuOverlay.Visibility = Visibility.Collapsed;
            ResumeButton.Visibility = Visibility.Visible;
            ContinueButton.Visibility = Visibility.Collapsed;
            ViewGardenButton.Visibility = Visibility.Collapsed;
            JournalButton.Visibility = Visibility.Visible;
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

        private void OnQuoteUnlocked(Quote quote)
        {
            PopupQuoteText.Text = $"\"{quote.Text}\"";
            PopupAuthorText.Text = $"- {quote.Author}";
            
            QuotePopup.Visibility = Visibility.Visible;
            
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500));
            QuotePopup.BeginAnimation(OpacityProperty, fadeIn);


            Task.Delay(5000).ContinueWith(_ => Dispatcher.Invoke(() => 
            {
                 var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));
                 fadeOut.Completed += (s, e) => QuotePopup.Visibility = Visibility.Collapsed;
                 QuotePopup.BeginAnimation(OpacityProperty, fadeOut);
            }));
        }

        private void OnGuestInteraction(bool success)
        {
            if (success)
            {
                ShowCalmMessage("Gość zostawił pyłek! +0.15 do mnożnika na 5 sekund.");
            }
            else
            {
                ShowCalmMessage("Wystraszyłaś gościa! -0.15 do mnożnika.");
            }
            SaveProgress();
            UpdateHud();
        }

        private void GardenCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_game == null) return;
            var p = e.GetPosition(GardenCanvas);
            _game.OnMouseMove(p);
        }

        private void JournalButton_Click(object sender, RoutedEventArgs e)
        {
            PopulateJournal();
            JournalOverlay.Visibility = Visibility.Visible;
            MenuOverlay.Visibility = Visibility.Collapsed;
        }

        private void CloseJournal_Click(object sender, RoutedEventArgs e)
        {
            JournalOverlay.Visibility = Visibility.Collapsed;
            MenuOverlay.Visibility = Visibility.Visible;
        }

        private void RulesButton_Click(object sender, RoutedEventArgs e)
        {
            RulesOverlay.Visibility = Visibility.Visible;
            MenuOverlay.Visibility = Visibility.Collapsed;
        }

        private void CloseRules_Click(object sender, RoutedEventArgs e)
        {
            RulesOverlay.Visibility = Visibility.Collapsed;
            MenuOverlay.Visibility = Visibility.Visible;
        }

        private void PopulateJournal()
        {
            JournalStack.Children.Clear();
            var state = SaveManager.Load();
            var unlockedIds = state?.UnlockedQuotes ?? new List<int>();

            if (unlockedIds.Count == 0)
            {
                 var empty = new TextBlock 
                 { 
                     Text = "Jeszcze nie odkryłaś żadnych myśli.\nSzukaj rzadkich, kolorowych świateł w ogrodzie.",
                     TextAlignment = TextAlignment.Center,
                     FontSize = 18,
                     Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                     Margin = new Thickness(0, 50, 0, 0),
                     FontFamily = new FontFamily("Gabriola")
                 };
                 JournalStack.Children.Add(empty);
                 return;
            }

            foreach (var id in unlockedIds)
            {
                var quote = QuotesData.All.FirstOrDefault(q => q.Id == id);
                if (quote != null)
                {
                    var border = new Border 
                    { 
                        Background = new SolidColorBrush(Color.FromRgb(255, 243, 224)),
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(20),
                        Margin = new Thickness(0, 0, 0, 15),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(255, 204, 128)),
                        BorderThickness = new Thickness(1)
                    };

                    var sp = new StackPanel();
                    var txt = new TextBlock 
                    { 
                        Text = $"\"{quote.Text}\"",
                        FontSize = 20,
                        FontFamily = new FontFamily("Gabriola"),
                        Foreground = new SolidColorBrush(Color.FromRgb(191, 54, 12)),
                        TextWrapping = TextWrapping.Wrap,
                         TextAlignment = TextAlignment.Center
                    };
                    var auth = new TextBlock 
                    { 
                        Text = $"- {quote.Author}",
                        FontSize = 16,
                        Foreground = new SolidColorBrush(Color.FromRgb(230, 81, 0)),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Margin = new Thickness(0, 5, 0, 0),
                        FontStyle = FontStyles.Italic
                    };

                    sp.Children.Add(txt);
                    sp.Children.Add(auth);
                    border.Child = sp;
                    JournalStack.Children.Add(border);
                }
            }
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
            var oldState = SaveManager.Load();
            var quotes = oldState?.UnlockedQuotes;

            var state = new GameState(
                _game.Stage,
                _plantCount,
                _game.GrowthMultiplier,
                _game.Hits,
                _game.Misses,
                quotes
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