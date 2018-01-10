using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Color = System.Windows.Media.Color;

namespace DesktopColorPicker.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly InterceptMouse _interceptMouse;
        private Color _selectedColor = Colors.White;

        public Color SelectedColor
        {
            get { return _selectedColor; }
            set
            {
                if (_selectedColor == value) return;
                _selectedColor = value;
                RaisePropertyChanged();
            }
        }

        public bool Hooked => _interceptMouse.IsHooked;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            CloseCommand = new RelayCommand<Window>(CloseClick);
            HookCommand = new RelayCommand(HookClick);
            _interceptMouse = new InterceptMouse();
            if (IsInDesignMode) return;
            HookClick();
            _interceptMouse.MouseHookLeftButtonDown +=
                (sender, args) =>
                {
                    Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                                                             {
                                                                 _dv?.Close();
                                                                 _dv = null;
                                                                 GetPixel(args);
                                                             });
                    _interceptMouse.Unhook();
                    RaisePropertyChanged(nameof(Hooked));
                };
            _interceptMouse.MouseHookMove +=
                (sender, args) => Dispatcher.CurrentDispatcher.InvokeAsync(() => GetPixel(args));
        }

        public ICommand CloseCommand { get; }

        public void CloseClick(Window w)
        {
            _dv?.Close();
            _dv = null;
            w.Close();
        }

        public ICommand HookCommand { get; }

        private DummyView _dv;
        public void HookClick()
        {
            if (_interceptMouse.IsHooked || _dv != null) return;
            _dv = new DummyView();
            _dv.Show();
            _interceptMouse.SetHook();
            RaisePropertyChanged(nameof(Hooked));
        }

        private void GetPixel(PointEventArgs args)
        {
            int c;
            using (var b = new Bitmap(1, 1))
            {
                using (var g = Graphics.FromImage(b))
                    g.CopyFromScreen((int)args.Point.X, (int)args.Point.Y, 0, 0, b.Size);
                c = b.GetPixel(0, 0).ToArgb();
            }/*
            MessageBox.Show(Application.Current.MainWindow,
                            $"The pixel at {args.Point.X}, {args.Point.Y} has the ARGB color #{c:X8}",
                            Application.Current.MainWindow.Title, MessageBoxButton.OK, MessageBoxImage.Information);*/
            SelectedColor = Color.FromArgb((byte)((c >> 24) & 0xFF), (byte)((c >> 16) & 0xFF), (byte)((c >> 8) & 0xFF),
                                           (byte)(c & 0xFF));
        }

        public override void Cleanup()
        {
            // Clean up if needed
            if (_interceptMouse.IsHooked) _interceptMouse.Unhook();
            base.Cleanup();
        }
    }
}