using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.IO;
using WinRT.Interop;

namespace DownloadTimer
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainWindow()
        {
            this.InitializeComponent();

            // Pencere Tutamacýný (Handle) al
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            // 1. Ýkonu Ayarla (Sol üst köþe ve Görev Çubuðu)
            // AppIcon.ico dosyasýnýn uygulama klasöründe olduðunu varsayar
            string iconPath = Path.Combine(AppContext.BaseDirectory, "AppIcon.ico");
            if (File.Exists(iconPath))
            {
                appWindow.SetIcon(iconPath);
            }

            // 2. Boyutu Sabitle (420x620)
            appWindow.Resize(new Windows.Graphics.SizeInt32(420, 620));

            // 3. Yeniden boyutlandýrmayý ve Tam ekraný kapat
            var presenter = appWindow.Presenter as OverlappedPresenter;
            if (presenter != null)
            {
                presenter.IsResizable = false;
                presenter.IsMaximizable = false;
            }

            // 4. Mica Efekti (Windows 11 Þeffaflýk)
            SystemBackdrop = new MicaBackdrop();

            this.Title = "Download Timer";
        }
    }
}