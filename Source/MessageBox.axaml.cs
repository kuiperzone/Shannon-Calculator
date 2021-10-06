// -----------------------------------------------------------------------------
// PROJECT   : Shannon Calculator
// COPYRIGHT : Andy Thomas 2021
// LICENSE   : GPLv3
// HOMEPAGE  : https://kuiper.zone
// -----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace KuiperZone
{
    /// <summary>
    /// A MessageBox class. It can be shown by calling on the static ShowDialog() methods, or by constucting
    /// an instance and calling the non-static <see cref="Window.ShowDialog(Window)"/> which returns the value
    /// type <see cref="MessageBox.BoxResult"/>. The default window title is set from the application name.
    /// </summary>
    public class MessageBox : Window
    {
        private readonly StackPanel _panel;
        private readonly TextBlock _text;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MessageBox()
        {
            AvaloniaXamlLoader.Load(this);
            _text = this.FindControl<TextBlock>("Text");
            _panel = this.FindControl<StackPanel>("Buttons");

            Title = App.Current.Name;
            Opened += OnOpened;
        }

        /// <summary>
        /// Button option type.
        /// </summary>
        public enum BoxButtons
        {
            Ok,
            OkCancel,
            YesNo,
            YesNoCancel
        }

        /// <summary>
        /// The result type.
        /// </summary>
        public enum BoxResult
        {
            Ok,
            Cancel,
            Yes,
            No,
        }

        /// <summary>
        /// Gets or sets the message text.
        /// </summary>
        public string Message
        {
            get { return _text.Text; }
            set { _text.Text = value; }
        }

        /// <summary>
        /// Gets or sets the buttons shown.
        /// </summary>
        public BoxButtons Buttons { get; set; } = BoxButtons.Ok;

        /// <summary>
        /// Shows an instance of <see cref="MessageBox"/>. A default is used for the window title.
        /// </summary>
        public static Task<BoxResult> ShowDialog(Window owner, string msg, BoxButtons btns = BoxButtons.Ok)
        {
            return ShowDialog(owner, msg, string.Empty, btns);
        }

        /// <summary>
        /// Shows an instance of <see cref="MessageBox"/>.
        /// </summary>
        public static Task<BoxResult> ShowDialog(Window owner, string msg, string title, BoxButtons btns = BoxButtons.Ok)
        {
            var box = new MessageBox();
            box.Message = msg;
            box.Buttons = btns;

            if (!string.IsNullOrWhiteSpace(title))
            {
                box.Title = title;
            }

            return box.ShowDialog<BoxResult>(owner);
        }

        /// <summary>
        /// Shows an instance of <see cref="MessageBox"/> with exception information. If stack is true,
        /// the full error stack is shown, whereas only the message is shown if false. If null, the
        /// stack is shown only where DEBUG is defined.
        /// </summary>
        public static Task<BoxResult> ShowDialog(Window owner, Exception error, bool? stack = null)
        {
#if DEBUG
            stack ??= true;
#endif
            var msg = error.Message;

            if (stack == true)
            {
                msg += "\n\n";
                msg += error.ToString();
            }

            return ShowDialog(owner, msg, error.GetType().Name);
        }

        private void OnOpened(object? sender, EventArgs e)
        {
            if (Buttons == BoxButtons.Ok || Buttons == BoxButtons.OkCancel)
            {
                AddButton("Ok", BoxResult.Ok);
            }

            if (Buttons == BoxButtons.YesNo || Buttons == BoxButtons.YesNoCancel)
            {
                AddButton("Yes", BoxResult.Yes);
                AddButton("No", BoxResult.No);
            }

            if (Buttons == BoxButtons.OkCancel || Buttons == BoxButtons.YesNoCancel)
            {
                AddButton("Cancel", BoxResult.Cancel);
            }

            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void AddButton(string caption, BoxResult rslt)
        {
            var btn = new Button {Content = caption};

            if (rslt == BoxResult.Ok)
            {
                btn.IsDefault = true;
            }

            if (rslt == BoxResult.Cancel)
            {
                btn.IsCancel = true;
            }

            btn.Click += (_, __) => { this.Close(rslt); };

            _panel.Children.Add(btn);
        }
    }

}