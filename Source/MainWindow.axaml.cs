// -----------------------------------------------------------------------------
// PROJECT   : Shannon Calculator
// COPYRIGHT : Andy Thomas 2021
// LICENSE   : GPLv3
// HOMEPAGE  : https://kuiper.zone
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace KuiperZone
{
    public partial class MainWindow : Window
    {
        private readonly TextBox _inputBox;
        private readonly TextBox _resultBox;
        private readonly TextBlock _linkText;
        private readonly NumericUpDown _countNumeric;
        private readonly NumericUpDown _baseNumeric;
        private readonly ToggleButton _natsButton;
        private readonly Button _coinsButton;
        private readonly Button _diceButton;
        private readonly CheckBox _metricCheck;
        private readonly CheckBox _ignoreCaseCheck;
        private readonly CheckBox _ignoreSpaceCheck;
        private readonly CheckBox _consecSpaceCheck;
        private readonly ToggleButton _darkButton;

        private readonly Random _random = new();
        private byte[]? _rawContent;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            _inputBox = FindOrThrow<TextBox>("inputBox");
            _resultBox = FindOrThrow<TextBox>("resultBox");
            _coinsButton = FindOrThrow<Button>("coinsButton");
            _diceButton = FindOrThrow<Button>("diceButton");
            _natsButton = FindOrThrow<ToggleButton>("natsButton");

            _countNumeric = FindOrThrow<NumericUpDown>("countNumeric");
            _countNumeric.ValueChanged += OnNumericChanged;

            _baseNumeric = FindOrThrow<NumericUpDown>("baseNumeric");
            _baseNumeric.ValueChanged += OnNumericChanged;

            _metricCheck = FindOrThrow<CheckBox>("metricCheck");
            _ignoreCaseCheck = FindOrThrow<CheckBox>("ignoreCaseCheck");
            _ignoreSpaceCheck = FindOrThrow<CheckBox>("ignoreSpaceCheck");
            _consecSpaceCheck = FindOrThrow<CheckBox>("consecSpaceCheck");

            _darkButton = FindOrThrow<ToggleButton>("darkButton");
            _darkButton.IsChecked = App.IsDarkMode;

            _linkText = FindOrThrow<TextBlock>("linkText");
            _linkText.PointerPressed += OnLinkClick;

            Opened += OnOpened;

            // Fix issue in 0.10.7
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _inputBox.FontFamily = new FontFamily("Courier New");
                _resultBox.FontFamily = new FontFamily("Courier New");
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private T FindOrThrow<T>(string name) where T : class, IControl
        {
            return this.FindControl<T>(name) ?? throw new InvalidOperationException("Control not found " + name);
        }

        private void OnOpened(object? sender, EventArgs e)
        {
            ResetState();
            _inputBox.Focus();
        }

        private async void OnLinkClick(object? sender, EventArgs e)
        {
            if (sender != null)
            {
                try
                {
                    var url = ((TextBlock)sender).Text;
                    ShellOpen.Start(url);
                }
                catch (Exception x)
                {
                    await MessageBox.ShowDialog(this, x);
                }
            }
        }

        private void OnNumericChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            var control = (NumericUpDown?)sender;

            if (control != null && e.NewValue != (int)e.NewValue)
            {
                control.Value = (int)e.NewValue;
            }
        }

        private async void OnOpenTextClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog();
                var path = await dialog.ShowAsync(this);

                if (path != null && path.Length != 0)
                {
                    SetTextContent(new FileLoader(path[0]));
                }
            }
            catch (Exception x)
            {
                await MessageBox.ShowDialog(this, x);
            }
        }

        private async void OnOpenBinaryClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog();
                var path = await dialog.ShowAsync(this);

                if (path != null && path.Length != 0)
                {
                    SetRawContent(new FileLoader(path[0]));
                }
            }
            catch (Exception x)
            {
                await MessageBox.ShowDialog(this, x);
            }
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            _inputBox.Clear();
            ResetState();
        }

        private void OnWrapClick(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton?)sender;

            if (btn != null && btn.IsChecked == true)
            {
                _inputBox.TextWrapping = TextWrapping.Wrap;
            }
            else
            {
                _inputBox.TextWrapping = TextWrapping.NoWrap;
            }
        }

        private void OnSpaceClick(object sender, RoutedEventArgs e)
        {
            _consecSpaceCheck.IsEnabled = _ignoreSpaceCheck.IsChecked == false;
        }

        private void OnNatsClick(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton?)sender;

            if (btn != null && btn.IsChecked == true)
            {
                _baseNumeric.IsEnabled = false;
            }
            else
            {
                _baseNumeric.IsEnabled = true;
            }
        }

        private void OnCoinsClick(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            var count = _countNumeric.Value;

            for (int n = 0; n < count; ++n)
            {
                sb.Append(_random.Next(0, 2) == 0 ? "H" : "T");
            }

            _inputBox.Text = _inputBox.Text + sb.ToString();
        }

        private void OnDiceClick(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            var count = _countNumeric.Value;

            for (int n = 0; n < count; ++n)
            {
                sb.Append(_random.Next(1, 7).ToString());
            }

            _inputBox.Text = _inputBox.Text + sb.ToString();
        }

        private async void OnCalculateClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var calc = new ShannonCalculator();
                calc.IsMetricEntropy = _metricCheck.IsChecked == true;

                if (_natsButton.IsChecked == true)
                {
                    calc.LogBase = 0;
                }
                else
                {
                    calc.LogBase = (int)_baseNumeric.Value;
                }

                if (_rawContent != null)
                {
                    calc.Add(_rawContent);
                }
                else
                {
                    string text = _inputBox.Text ?? string.Empty;

                    if (_ignoreCaseCheck.IsChecked == true)
                    {
                        text = text.ToLowerInvariant();
                    }

                    if (_ignoreSpaceCheck.IsChecked == true || _consecSpaceCheck.IsChecked == true)
                    {
                        text = text.Trim();

                        bool last = false;
                        bool ignoreAll = _ignoreSpaceCheck.IsChecked == true;

                        var msg = new List<char>(text.Length);

                        for (int n = 0; n < text.Length; ++n)
                        {
                            var c = text[n];

                            // Also detect line ends
                            if (c <= ' ')
                            {
                                if (!last && !ignoreAll)
                                {
                                    msg.Add(c);
                                    last = true;
                                }

                                continue;
                            }

                            msg.Add(c);
                            last = false;
                        }

                        calc.Add(msg);
                    }
                    else
                    {
                        calc.Add(text);
                    }
                }

                _resultBox.Text = calc.ToString();
            }
            catch (Exception x)
            {
                await MessageBox.ShowDialog(this, x);
            }
        }

        private void OnThemeClick(object sender, RoutedEventArgs e)
        {
            App.IsDarkMode = _darkButton.IsChecked == true;
        }

        private async void OnAboutClick(object sender, RoutedEventArgs e)
        {
            var win = new AboutWindow();
            await win.ShowDialog(this);
        }

        private void ResetState()
        {
            _inputBox.IsReadOnly = false;
            _inputBox.TextAlignment = TextAlignment.Left;

            _coinsButton.IsEnabled = true;
            _diceButton.IsEnabled = true;
            _ignoreCaseCheck.IsEnabled = true;
            _ignoreSpaceCheck.IsEnabled = true;
            _consecSpaceCheck.IsEnabled = _ignoreSpaceCheck.IsChecked == false;

            _rawContent = null;
        }

        private void SetRawContent(FileLoader loader)
        {
            _inputBox.IsReadOnly = true;
            _inputBox.TextAlignment = TextAlignment.Center;

            _coinsButton.IsEnabled = false;
            _diceButton.IsEnabled = false;
            _ignoreCaseCheck.IsEnabled = false;
            _ignoreSpaceCheck.IsEnabled = false;
            _consecSpaceCheck.IsEnabled = false;

            var sb = new StringBuilder();
            sb.AppendLine();

            if (loader.ByteCount != 0)
            {
                sb.AppendLine("BINARY");
                sb.AppendLine(loader.Filename);
                sb.AppendLine("Bytes: " + loader.ByteCount);
            }
            else
            {
                sb.AppendLine("EMPTY");
                sb.AppendLine(loader.Filename);
            }

            sb.Append("(Clear to reset)");

            _inputBox.Text = sb.ToString();
            _rawContent = loader.ToArray();
        }

        private void SetTextContent(FileLoader loader)
        {
            if (loader.IsText)
            {
                ResetState();
                _inputBox.Text = loader.ToString();
            }
            else
            {
                // Fall back
                SetRawContent(loader);
            }
        }
    }
}