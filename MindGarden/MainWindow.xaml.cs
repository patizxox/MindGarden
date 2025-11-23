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

namespace MindGarden
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _menuOpen = true;
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
            ShowCalmMessage("Zaczynasz nową sesję. Skup się tylko na tym, co widzisz.");
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
    }
}