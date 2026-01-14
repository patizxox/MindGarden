using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace MindGarden
{
    public class GameController
    {
        private bool _paused;
        private readonly Canvas _canvas;
        private readonly Dictionary<GameStage, StageConfig> _cfg;
        private readonly Random _rand;
        private readonly LightSpot _spot;
        public int Hits { get; private set; }
        public int Misses { get; private set; }

        public GameStage Stage { get; private set; } = GameStage.Stage1;
        public int PlantsThisStage { get; private set; } = 0;
        public double GrowthMultiplier { get; private set; } = 1.0;

        public event Action? StageAdvanced;
        public event Action? Finished;
        public event Action<Quote>? QuoteUnlocked;

        private bool _isSpotActive;
        private bool _isPlantGrowing;

        public GameController(Canvas canvas, Dictionary<GameStage, StageConfig> cfg, Random rand, GameState? state = null)
        {
            _canvas = canvas;
            _cfg = cfg;
            _rand = rand;
            _spot = new LightSpot(canvas);

            if (state != null)
            {
                Stage = state.Stage;
                GrowthMultiplier = state.Multiplier;
                Hits = state.Hits;
                Misses = state.Misses;
            }

            ScheduleNextSpot();
        }

        public void OnClick(Point p, Func<Point, TimeSpan> onPlant)
        {
            if (_paused) return;

            if (_isSpotActive && _spot.Hit(p))
            {
                if (_isPlantGrowing)
                {
                    Misses++;
                    GrowthMultiplier = Math.Max(0.25, GrowthMultiplier - 0.05);
                }
                else
                {
                    _isSpotActive = false;
                    
                    if (_spot.IsSpecial)
                    {
                         UnlockRandomQuote();
                         ScheduleNextSpot();
                         return;
                    }

                    Hits++;
                    _plantLocations.Add(p);
                    
                    var growthDuration = onPlant(p);
                    
                    _isPlantGrowing = true;
                    Task.Delay(growthDuration).ContinueWith(_ => _isPlantGrowing = false);

                    PlantsThisStage++;
                    GrowthMultiplier = Math.Min(1.5, GrowthMultiplier + 0.1);
                    if (PlantsThisStage >= _cfg[Stage].LightsPerStage) NextStage();
                    else ScheduleNextSpot();
                }
            }
            else
            {
                Misses++;
                GrowthMultiplier = Math.Max(0.25, GrowthMultiplier - 0.05);
            }
        }
        public void Pause()
        {
            _paused = true;
            _spot.Hide();
            _isSpotActive = false;
        }

        public void Resume()
        {
            if (!_paused || Stage == GameStage.Finished) return;
            _paused = false;
            ScheduleNextSpot();
        }

        private void NextStage()
        {
            _isSpotActive = false;
            _spot.Hide();
            PlantsThisStage = 0;
            Stage = Stage switch
            {
                GameStage.Stage1 => GameStage.Stage2,
                GameStage.Stage2 => GameStage.Stage3,
                GameStage.Stage3 => GameStage.Stage4,
                GameStage.Stage4 => GameStage.Stage5,
                GameStage.Stage5 => GameStage.Finished,
                _ => GameStage.Finished
            };
            if (Stage == GameStage.Finished)
            {
                Finished?.Invoke();
                return;
            }
            StageAdvanced?.Invoke();
            ScheduleNextSpot();
        }

        private readonly List<Point> _plantLocations = new();

        private void UnlockRandomQuote()
        {
            var savedState = SaveManager.Load();
            var unlockedIds = savedState?.UnlockedQuotes ?? new List<int>();

            var lockedQuotes = QuotesData.All.Where(q => !unlockedIds.Contains(q.Id)).ToList();
            
            if (lockedQuotes.Count > 0)
            {
                var q = lockedQuotes[_rand.Next(lockedQuotes.Count)];
                unlockedIds.Add(q.Id);
                
                QuoteUnlocked?.Invoke(q);
                
                if (savedState == null)
                {
                    savedState = new GameState(Stage, PlantsThisStage, GrowthMultiplier, Hits, Misses, unlockedIds);
                }
                else
                {
                    savedState.UnlockedQuotes = unlockedIds;
                    savedState.Stage = Stage;
                    savedState.TotalPlants = PlantsThisStage;
                    savedState.Multiplier = GrowthMultiplier;
                    savedState.Hits = Hits;
                    savedState.Misses = Misses;
                }
                SaveManager.Save(savedState);
            }
        }

        private async void ScheduleNextSpot()
        {
            _isSpotActive = false;
            _spot.Hide();
            var s = _cfg[Stage];
            var delay = _rand.Next((int)s.LightMinDelay.TotalMilliseconds, (int)s.LightMaxDelay.TotalMilliseconds);
            await Task.Delay(delay);
            if (_paused || Stage == GameStage.Finished) return;

            Point p;
            int retries = 0;
            bool valid;
            double r = 40;

            do
            {
                valid = true;
                double x = _rand.Next((int)r, (int)Math.Max(r, _canvas.ActualWidth - r));
                double y = _rand.Next((int)(r + 40), (int)Math.Max(r + 40, _canvas.ActualHeight - r));
                p = new Point(x, y);

                double centerX = _canvas.ActualWidth / 2;
                if (y < 120 && x > centerX - 350 && x < centerX + 350)
                {
                    valid = false;
                }
                else
                {
                    foreach (var loc in _plantLocations)
                    {
                        if ((p - loc).Length < 110)
                        {
                            valid = false;
                            break;
                        }
                    }
                }
                retries++;
            } while (!valid && retries < 200);

            if (!valid)
            {
                await Task.Delay(200);
                ScheduleNextSpot();
                return;
            }

            bool isSpecial = _rand.NextDouble() < 0.10;
            _spot.SetType(isSpecial);

            _spot.ShowAt(p, r);
            _isSpotActive = true;
            
            ScheduleNextGuest();
        }

        private GardenGuest? _guest;
        private bool _guestActive;
        private Point _lastMousePos;
        private DateTime _stillSince;
        private bool _isStill;

        private async void ScheduleNextGuest()
        {
            if (_guestActive) return;

            int delay = _rand.Next(20000, 40000);
            await Task.Delay(delay);

            if (_paused || Stage == GameStage.Finished || _guestActive) return;

            if (_guest == null) _guest = new GardenGuest(_canvas);

            double x = _rand.Next(100, (int)_canvas.ActualWidth - 100);
            double y = _rand.Next(100, (int)_canvas.ActualHeight - 100);
            
            _guest.ShowAt(new Point(x, y));
            _guestActive = true;
            _isStill = true;
            _stillSince = DateTime.Now;
            CheckGuestTame();
        }

        public void OnMouseMove(Point p)
        {
            double speed = (p - _lastMousePos).Length;
            _lastMousePos = p;

            if (!_guestActive) return;

            if (speed > 10.0)
            {
                _isStill = false;
                _guest?.Flee();
                _guestActive = false;
                
                GrowthMultiplier = Math.Max(0.25, GrowthMultiplier - 0.15);
                GuestInteraction?.Invoke(false);
                
                ScheduleNextGuest();
            }
            else
            {
                if (!_isStill)
                {
                    _isStill = true;
                    _stillSince = DateTime.Now;
                    CheckGuestTame();
                }
            }
        }

        private async void CheckGuestTame()
        {
            while (_guestActive && _isStill)
            {
                if ((DateTime.Now - _stillSince).TotalSeconds >= 3.0)
                {
                    _guest?.Success();
                    _guestActive = false;
                    
                    GrowthMultiplier += 0.15;
                    GuestInteraction?.Invoke(true);
                    
                    await Task.Delay(5000);
                    if (!_paused && Stage != GameStage.Finished)
                    {
                        GrowthMultiplier = Math.Max(0.25, GrowthMultiplier - 0.15);
                        StageAdvanced?.Invoke();
                    }

                    ScheduleNextGuest();
                    return;
                }
                await Task.Delay(100);
            }
        }

        public event Action<bool>? GuestInteraction;

}
}