// -----------------------------------------------------------------------------
// PROJECT   : Shannon Calculator
// COPYRIGHT : Andy Thomas 2021
// LICENSE   : GPLv3
// HOMEPAGE  : https://kuiper.zone
// -----------------------------------------------------------------------------

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace KuiperZone
{
    public class App : Application
    {
        private static bool _isDarkMode;
        private static IStyle? _darkStyle;
        private static IStyle? _darkOverride;
        private static IStyle? _lightStyle;
        private static IStyle? _lightOverride;

        /// <summary>
        /// Gets or sets dark theme mode. App must be initialized for this to work and
        /// axaml file must contain both dark and light themes.
        /// </summary>
        public static bool IsDarkMode
        {
            get { return _isDarkMode; }

            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;

                    var s = App.Current.Styles;

                    if (value && _darkStyle != null && _darkOverride != null)
                    {
                        s[0] = _darkStyle;
                        s[1] = _darkOverride;
                    }
                    else
                    if (!value && _lightStyle != null && _lightOverride != null)
                    {
                        s[0] = _lightStyle;
                        s[1] = _lightOverride;
                    }
                }
            }
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            // Must contain at least 4 styles in expected order
            var s = App.Current.Styles;

            _darkStyle = s[0];
            _darkOverride = s[1];

            _lightStyle = s[2];
            _lightOverride = s[3];

            // Initial is light,
            // so remove dark for now.
            s.RemoveRange(0, 2);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
                App.Current.Name = desktop.MainWindow.Title;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}