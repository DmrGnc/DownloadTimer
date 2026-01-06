using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

namespace DownloadTimer
{
    // [ObservableProperty] kullanımında 'AOT' uyarısı alabilirsin (Sarı ünlem). 
    // Bu sadece programın açılış hızıyla ilgili bir optimizasyon uyarısıdır, 
    // çalışmaya engel DEĞİLDİR. Kırmızı hataları bu kodla bitiriyoruz.
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty] private double _fileSize = 10;
        [ObservableProperty] private string _fileUnit = "GB";
        [ObservableProperty] private double _internetSpeed = 100;
        [ObservableProperty] private string _speedUnit = "Mbps";
        [ObservableProperty] private string _resultTime = "00:00:00";
        [ObservableProperty] private bool _isTesting = false;
        [ObservableProperty] private string _testStatus = "Check Speed";

        partial void OnFileSizeChanged(double value) => Calculate();
        partial void OnFileUnitChanged(string value) => Calculate();
        partial void OnInternetSpeedChanged(double value) => Calculate();
        partial void OnSpeedUnitChanged(string value) => Calculate();

        private void Calculate()
        {
            double bytes = FileSize * FileUnit switch
            {
                "KB" => 1024,
                "MB" => Math.Pow(1024, 2),
                "GB" => Math.Pow(1024, 3),
                "TB" => Math.Pow(1024, 4),
                _ => 1
            };

            double bps = InternetSpeed * SpeedUnit switch
            {
                "Kbps" => 1000,
                "Mbps" => Math.Pow(1000, 2),
                "Gbps" => Math.Pow(1000, 3),
                _ => 1
            };

            if (bps > 0)
            {
                double totalSeconds = (bytes * 8) / bps;
                var t = TimeSpan.FromSeconds(totalSeconds);

                ResultTime = t.TotalDays >= 1
                    ? $"{(int)t.TotalDays}d {t.Hours:D2}h {t.Minutes:D2}m"
                    : $"{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";
            }
        }

        [RelayCommand]
        private async Task RunAutoSpeedtest()
        {
            if (IsTesting) return;

            IsTesting = true;
            TestStatus = "Testing...";

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "speedtest.exe",
                    Arguments = "--format=json --accept-license --accept-gdpr",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // '?' işareti ve null kontrolü ile olası çökmeleri engelliyoruz
                using (Process? process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        using (var reader = process.StandardOutput)
                        {
                            string result = await reader.ReadToEndAsync();
                            if (!string.IsNullOrEmpty(result))
                            {
                                using (JsonDocument doc = JsonDocument.Parse(result))
                                {
                                    if (doc.RootElement.TryGetProperty("download", out JsonElement downloadElement))
                                    {
                                        double bandwidth = downloadElement.GetProperty("bandwidth").GetDouble();
                                        double mbps = (bandwidth * 8) / 1_000_000;

                                        InternetSpeed = Math.Round(mbps, 2);
                                        SpeedUnit = "Mbps";
                                    }
                                }
                            }
                        }
                    }
                }
                TestStatus = "Success!";
            }
            catch (Exception)
            {
                TestStatus = "CLI Not Found";
            }
            finally
            {
                IsTesting = false;
                await Task.Delay(2000);
                TestStatus = "Check Speed";
            }
        }
    }
}