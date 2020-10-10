using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Config.Net;
using ControlzEx.Theming;
using System.IO.IsolatedStorage;
using System.IO;
using MahApps.Metro.Controls;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using TextColorLibrary;
using Color = System.Drawing.Color;

namespace CaptureGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static Color NormalTextColor = Color.White;
        private object locker = new object();
        AppSettings config= new ConfigurationBuilder<AppSettings>().UseJsonFile(System.IO.Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\AmongUsCapture\\AmongUsGUI", "Settings.json")).Build();
        public MainWindow()
        {
            InitializeComponent();
            Paragraph p = ConsoleTextBox.Document.Blocks.FirstBlock as Paragraph;
            p.LineHeight = 1;
            this.DataContext = config;
            SetDefaultThemeColor();
            //ApplyDarkMode();
            ThemeManager.Current.ChangeThemeColorScheme(this, "Red");
            ApplyDarkMode();

        }
        private void SetDefaultThemeColor()
        {
            if (!config.ranBefore)
            {
                config.ranBefore = true;
                var currentTheme = ThemeManager.Current.DetectTheme();
                ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
                ThemeManager.Current.SyncTheme();
                var newTheme = ThemeManager.Current.DetectTheme();
                config.DarkMode = newTheme.BaseColorScheme == ThemeManager.BaseColorDark;
                ThemeManager.Current.ChangeTheme(this, currentTheme ?? throw new InvalidOperationException());
            }
            

        }
        private void ApplyDarkMode()
        {
            if (config.DarkMode)
            {
                ThemeManager.Current.ChangeThemeBaseColor(this, ThemeManager.BaseColorDark);
                NormalTextColor = Color.White;
            }
            else
            {
                NormalTextColor = Color.Black;
                ThemeManager.Current.ChangeThemeBaseColor(this, ThemeManager.BaseColorLight);
            }
        }
        private void Settings(object sender, RoutedEventArgs e)
        {
            // Open up the settings flyout
            SettingsFlyout.IsOpen = true;
        }

        private void Darkmode_Toggled(object sender, RoutedEventArgs e)
        {
            if (!(sender is ToggleSwitch toggleSwitch)) return;
            ThemeManager.Current.ChangeThemeBaseColor(this,
                toggleSwitch.IsOn == true ? ThemeManager.BaseColorDark : ThemeManager.BaseColorLight);
        }

        private bool connected = false;
        private void ManualConnect_Click(object sender, RoutedEventArgs e)
        {
            //this.ManualConnectButton.IsEnabled = false;
            this.setConnectionStatus(connected);
            connected = !connected;
        }

        private async void GameCodeBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            await this.ShowMessageAsync("Gamecode copied to clipboard!", "");
            Clipboard.SetText(GameCodeBox.Text);
        }

        public void setGameCode(string gamecode)
        {
            GameCodeBox.Text = gamecode;
        }

        public void setCurrentState(string state)
        {
            StatusBox.Text = state;
        }

        public void setConnectionStatus(bool connected)
        {
            if (connected)
            {
                ThemeManager.Current.ChangeThemeColorScheme(this,"Green");
            }
            else
            {
                ThemeManager.Current.ChangeThemeColorScheme(this, "Red");
            }
        }

        public void WriteConsoleLineFormatted(String moduleName, Color moduleColor, String message)
        {
            //Outputs a message like this: [{ModuleName}]: {Message}
            WriteColoredText($"{NormalTextColor.ToTextColor()}[{moduleColor.ToTextColor()}{moduleName}{NormalTextColor.ToTextColor()}]: {message}");
        }

        public void WriteColoredText(string ColoredText)
        {
            lock (locker)
            {
                foreach (var part in TextColor.toParts(ColoredText))
                {
                    this.AppendText(part.text, part.textColor, false);
                }
                this.AppendText("", Color.White, true);
            }
            ConsoleTextBox.ScrollToEnd();
        }
        public void AppendText(string text, Color color, bool addNewLine = false)
        {
            TextRange tr = new TextRange(ConsoleTextBox.Document.ContentEnd, ConsoleTextBox.Document.ContentEnd)
            {
                Text = (addNewLine ? $"{text}{Environment.NewLine}" : text)
        };
            try
            {
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B)));
            }
            catch (FormatException) { }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //TestFillConsole(10);
        }
        private void TestFillConsole(int entries) //Helper test method to see if filling console works.
        {
            for (int i = 0; i < entries; i++)
            {
                var nonString = "Wow! Look at this pretty text!";
                WriteConsoleLineFormatted("Rainbow", TextColor.Rainbow((float)i / entries), TextColor.getRainbowText(nonString, i));
            };
            //this.WriteColoredText(getRainbowText("This is a Pre-Release from Carbon's branch."));
        }

        private void MainWindow_OnContentRendered(object? sender, EventArgs e)
        {
            TestFillConsole(10);
        }
    }

}
