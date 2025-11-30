using System.Windows;
using System.Windows.Controls;

namespace MindGarden
{
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

}