// -----------------------------------------------------------------------------
// PROJECT   : Shannon Calculator
// COPYRIGHT : Andy Thomas 2021
// LICENSE   : GPLv3
// HOMEPAGE  : https://kuiper.zone
// -----------------------------------------------------------------------------

using System;
using System.Reflection;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace KuiperZone
{
    public partial class AboutWindow : Window
    {
        public const string AppName = "Shannon Calculator";

        private readonly TextBlock _title;
        private readonly TextBlock _version;
        private readonly TextBlock _author;

        public AboutWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            _title = this.FindControl<TextBlock>("title");
            _title.Text = AppName;

            _author = this.FindControl<TextBlock>("author");
            _author.Text = "Copyright 2021 Andy Thomas";

            _version = this.FindControl<TextBlock>("version");
            _version.Text = GetVersion();

#if DEBUG
            _version.Text += "-Debug";
#endif
        }

        private string GetVersion()
        {
            try
            {
                var ver = Assembly.GetEntryAssembly()?.GetName()?.Version;

                if (ver != null)
                {
                    return ver.ToString(3);
                }
            }
            catch (Exception e)
            {
                MessageBox.ShowDialog(this, e);
            }

            // Fallback
            return "Unknown";
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnCloseClick(object? _, RoutedEventArgs e)
        {
            Close();
        }

    }
}