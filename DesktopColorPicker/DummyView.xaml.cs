using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace DesktopColorPicker
{
    /// <summary>
    /// Description for DummyView.
    /// </summary>
    public partial class DummyView : Window
    {
        /// <summary>
        /// Initializes a new instance of the DummyView class.
        /// </summary>
        public DummyView()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            // Use hacks to disable click-through (AllowsTransparency="False" with Transparent BG)

            var hwnd = (HwndSource)PresentationSource.FromVisual(this);

            // Set the background to transparent from both the WPF and Win32 perspectives
            Background = Brushes.Transparent;
            hwnd.CompositionTarget.BackgroundColor = Colors.Transparent;

            var margins = new NativeMethods.MARGINS(new Thickness(-1));
            NativeMethods.DwmExtendFrameIntoClientArea(hwnd.Handle, ref margins);
            base.OnSourceInitialized(e);
        }
    }
}