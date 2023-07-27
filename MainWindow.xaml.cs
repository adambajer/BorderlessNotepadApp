using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Media3D;
using System.Drawing;
using System.Windows.Interop;

namespace BorderlessNotepadApp
{

    public partial class MainWindow : Window    
    {
        private bool isDragging;
        private System.Windows.Point clickPosition;
        private const int WM_NCHITTEST = 0x0084;
        private const int HTCLIENT = 0x01;
        private const int HTCAPTION = 0x02;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;
        private const int RESIZE_HANDLE_SIZE = 10;
        private const string V = "Icon1.ico";

        public MainWindow()
        {
            InitializeComponent();
            this.WindowStyle = WindowStyle.None;  // remove system chrome

            this.Loaded += MainWindow_Loaded;
            TitleBar.Visibility = Visibility.Visible;
            StateChanged += HandleStateChanged;
             MyNotifyIcon.ToolTipText = "BorderlessNotepadApp"; 
            DynamicTitle.Text = "BorderlessNotepadApp";
            txtEditor.IsReadOnly = false;  // Add this line


        }
        public ImageSource ConvertBitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EnableBlur();
            var helper = new WindowInteropHelper(this);
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_NCHITTEST:
                    System.Windows.Point cursorPos = this.PointFromScreen(new System.Windows.Point((int)lParam & 0xffff, (int)lParam >> 16));



                    IntPtr hitResult = IntPtr.Zero;

                    if (new Rect(0, 0, RESIZE_HANDLE_SIZE, this.ActualHeight).Contains(cursorPos))
                    {
                        hitResult = new IntPtr(HTLEFT);
                    }
                    else if (new Rect(this.ActualWidth - RESIZE_HANDLE_SIZE, 0, RESIZE_HANDLE_SIZE, this.ActualHeight).Contains(cursorPos))
                    {
                        hitResult = new IntPtr(HTRIGHT);
                    }
                    else if (new Rect(0, 0, this.ActualWidth, RESIZE_HANDLE_SIZE).Contains(cursorPos))
                    {
                        hitResult = new IntPtr(HTTOP);
                    }
                    else if (new Rect(0, this.ActualHeight - RESIZE_HANDLE_SIZE, this.ActualWidth, RESIZE_HANDLE_SIZE).Contains(cursorPos))
                    {
                        hitResult = new IntPtr(HTBOTTOM);
                    }
                    else if (new Rect(0, 0, this.ActualWidth, this.ActualHeight).Contains(cursorPos))
                    {
                        hitResult = new IntPtr(HTCAPTION);
                    }

                    if (hitResult != IntPtr.Zero)
                    {
                        handled = true;
                        return hitResult;
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        private void NewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Create a new instance of MainWindow here when "New" is clicked
            MainWindow newWindow = new MainWindow();
            newWindow.Show();

            // Clear the current editor
            txtEditor.Document.Blocks.Clear();
            LineNumbers.Text = "¨0";
            statusInfo.Text = "Lines: 0 Chars: 0";
        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string text = File.ReadAllText(openFileDialog.FileName);
                txtEditor.Document.Blocks.Clear();
                txtEditor.Document.Blocks.Add(new Paragraph(new Run(text)));
            }
        }

        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                string text = new TextRange(txtEditor.Document.ContentStart, txtEditor.Document.ContentEnd).Text;
                File.WriteAllText(saveFileDialog.FileName, text);
            }
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the current window instance
            this.Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            clickPosition = e.GetPosition(this);
            this.DragMove();
        }

        private void TitleBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
        }

        private void HandleStateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                MyNotifyIcon.Visibility = Visibility.Visible;
            }
            else
            {
                this.ShowInTaskbar = true;
                MyNotifyIcon.Visibility = Visibility.Collapsed;
            }
        }

        private void MyNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate();
                this.Topmost = true;  // important
                this.Topmost = false; // important
                this.Focus();         // important
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Instantiate Random object
            Random rnd = new Random();

            // Generate random RGB values
            byte red = (byte)rnd.Next(256);
            byte green = (byte)rnd.Next(256);
            byte blue = (byte)rnd.Next(256);

            // Create SolidColorBrush with the random color and set its opacity
            SolidColorBrush brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(red, green, blue))
            {
                Opacity = 0.6
            };

            // Create a Bitmap and Graphics object to draw the color
            using (Bitmap bitmap = new Bitmap(16, 16))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(System.Drawing.Color.FromArgb(red, green, blue));
                }

                // Convert the Bitmap to a BitmapSource
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                    bitmap.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                // Set the IconSource of MyNotifyIcon
MyNotifyIcon.Icon = Properties.Resources.Icon1;
            }

            // Set the background of DynamicTitle and enable blur
            DynamicTitle.Background = brush;
            EnableBlur();
        }





        private void TextEditorScrollView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            LineNumberScrollView.ScrollToVerticalOffset(e.VerticalOffset);
        }
        private void txtEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLineNumbers();
            UpdateStatusInfo();

            var text = new TextRange(txtEditor.Document.ContentStart, txtEditor.Document.ContentEnd).Text;
            var firstChars = text.Substring(0, Math.Min(50, text.Length));

            string newTitle = string.IsNullOrEmpty(firstChars) ? "BorderlessNotepad" : firstChars;
            this.Title = newTitle;
            MyNotifyIcon.ToolTipText = newTitle;
            DynamicTitle.Text = newTitle;
        }


        private void UpdateLineNumbers()
        {
            int lineCount = txtEditor.Document.Blocks.Count;
            LineNumbers.Text = string.Empty;

            for (int line = 1; line <= lineCount; line++)
            {
                LineNumbers.Text += line + "\n";
            }
        }

        private void UpdateStatusInfo()
        {
            int lineCount = txtEditor.Document.Blocks.Count;
            var text = new TextRange(txtEditor.Document.ContentStart, txtEditor.Document.ContentEnd).Text;
            int charCount = text.Length;
            statusInfo.Text = $"Lines: {lineCount} Chars: {charCount}";
        }

        private void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        private void ResizeGrip_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (this.Width + e.HorizontalChange > this.MinWidth)
                this.Width += e.HorizontalChange;
            if (this.Height + e.VerticalChange > this.MinHeight)
                this.Height += e.VerticalChange;
        }

   


    }

    internal enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
        ACCENT_INVALID_STATE = 5
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public uint AccentFlags;
        public uint GradientColor;
        public uint AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        // ...
        WCA_ACCENT_POLICY = 19
        // ...
    }
  
}
