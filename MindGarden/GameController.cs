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

        public GameStage Stage { get; private set; } = GameStage.Stage1;
        public int PlantsThisStage { get; private set; } = 0;
        public double GrowthMultiplier { get; private set; } = 1.0;

        public event Action? StageAdvanced;
        public event Action? Finished;

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
            if (_paused) return;

            if (_spot.Hit(p))
            {
                onPlant(p);
                PlantsThisStage++;
                GrowthMultiplier = Math.Min(1.5, GrowthMultiplier + 0.02);
                if (PlantsThisStage >= _cfg[Stage].LightsPerStage) NextStage();
                else ScheduleNextSpot();
            }
            else
            {
                GrowthMultiplier = Math.Max(0.5, GrowthMultiplier - 0.05);
            }
        }
        public void Pause()
        {
            _paused = true;
            _spot.Hide();
        }

        public void Resume()
        {
            if (!_paused || Stage == GameStage.Finished) return;
            _paused = false;
            ScheduleNextSpot();
        }

        private void NextStage()
        {
            _spot.Hide();
            PlantsThisStage = 0;
            Stage = Stage switch
            {
                GameStage.Stage1 => GameStage.Stage2,
                GameStage.Stage2 => GameStage.Stage3,
                GameStage.Stage3 => GameStage.Finished,
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

        private async void ScheduleNextSpot()
        {
            _spot.Hide();
            var s = _cfg[Stage];
            var delay = _rand.Next((int)s.LightMinDelay.TotalMilliseconds, (int)s.LightMaxDelay.TotalMilliseconds);
            await Task.Delay(delay);
            if (_paused || Stage == GameStage.Finished) return;

            double r = 40;
            double x = _rand.Next((int)r, (int)Math.Max(r, _canvas.ActualWidth - r));
            double y = _rand.Next((int)(r + 40), (int)Math.Max(r + 40, _canvas.ActualHeight - r));
            _spot.ShowAt(new Point(x, y), r);
        }
    }

}