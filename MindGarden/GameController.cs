using System.Windows;
using System.Windows.Controls;

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
                    GrowthMultiplier = Math.Max(0.5, GrowthMultiplier - 0.05);
                }
                else
                {
                    _isSpotActive = false;
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
                GrowthMultiplier = Math.Max(0.5, GrowthMultiplier - 0.05);
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

                // Exclusion zone for top UI
                double centerX = _canvas.ActualWidth / 2;
                if (y < 120 && x > centerX - 350 && x < centerX + 350)
                {
                    valid = false;
                }
                else
                {
                    foreach (var loc in _plantLocations)
                    {
                        if ((p - loc).Length < 110) // 110px spacing
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

            _spot.ShowAt(p, r);
            _isSpotActive = true;
        }
    }

}