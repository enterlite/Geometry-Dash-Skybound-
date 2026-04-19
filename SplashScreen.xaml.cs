using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SkyboundLauncher
{
    /// <summary>
    /// Splash screen s animací loadingu a pohyblivými kostkami
    /// </summary>
    public partial class SplashScreen : Window
    {
        private Stopwatch _stopwatch = new();
        private const double TotalDurationSeconds = 11.0;
        private Random _random = new();

        public SplashScreen()
        {
            InitializeComponent();
            this.Loaded += SplashScreen_Loaded;
        }

        private void SplashScreen_Loaded(object sender, RoutedEventArgs e)
        {
            // Spusť loader animaci
            StartLoading();
        }

        private void StartLoading()
        {
            // Fade in animace
            var fadeInStoryboard = this.FindResource("FadeInAnimation") as Storyboard;
            if (fadeInStoryboard != null)
            {
                fadeInStoryboard.Begin();
            }

            // Inicializuj animace pro krychle
            InitializeCubesAnimation();
            
            // Spusť postupné zobrazování kostek
            StartCubesSequentially();

            // Spusť timer pro aktualizaci procent
            _stopwatch.Start();
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 100; // Update každých 100ms
            timer.Tick += (s, e) =>
            {
                double elapsed = _stopwatch.Elapsed.TotalSeconds;
                double percentage = Math.Min(100, (elapsed / TotalDurationSeconds) * 100);
                
                var progressText = this.FindName("ProgressText") as TextBlock;
                if (progressText != null)
                {
                    progressText.Text = $"{(int)percentage}%";
                }

                // Když je načítání hotovo
                if (elapsed >= TotalDurationSeconds)
                {
                    timer.Stop();
                    timer.Dispose();
                    
                    // Fade out animace pro krychle
                    var cubesFadeOut = this.FindResource("CubesFadeOutAnimation") as Storyboard;
                    if (cubesFadeOut != null)
                    {
                        cubesFadeOut.Begin();
                    }
                    
                    // Fade out animace pro logo
                    var fadeOutStoryboard = this.FindResource("FadeOutAnimation") as Storyboard;
                    if (fadeOutStoryboard != null)
                    {
                        fadeOutStoryboard.Completed += (s2, e2) =>
                        {
                            // Otevři MainWindow a zavři splash screen
                            var mainWindow = new MainWindow();
                            mainWindow.Show();
                            this.Close();
                        };
                        fadeOutStoryboard.Begin();
                    }
                }
            };
            timer.Start();
        }

        private void InitializeCubesAnimation()
        {
            string[] cubeImages = new[]
            {
                "pack://application:,,,/image/cube.png",
                "pack://application:,,,/image/cube2.png",
                "pack://application:,,,/image/cube3.png",
                "pack://application:,,,/image/cube4.png",
                "pack://application:,,,/image/cube5.png"
            };

            Image?[] cubes = new[] 
            { 
                this.FindName("Cube1") as Image,
                this.FindName("Cube2") as Image,
                this.FindName("Cube3") as Image,
                this.FindName("Cube4") as Image,
                this.FindName("Cube5") as Image
            };

            RotateTransform?[] rotations = new[]
            {
                this.FindName("Cube1Rotation") as RotateTransform,
                this.FindName("Cube2Rotation") as RotateTransform,
                this.FindName("Cube3Rotation") as RotateTransform,
                this.FindName("Cube4Rotation") as RotateTransform,
                this.FindName("Cube5Rotation") as RotateTransform
            };

            // Vygeneruj animace pro každou krychli
            for (int i = 0; i < cubes.Length; i++)
            {
                if (cubes[i] != null && rotations[i] != null)
                {
                    // Nastavit obrázek
                    cubes[i]!.Source = new System.Windows.Media.Imaging.BitmapImage(
                        new Uri(cubeImages[i], UriKind.Absolute));

                    // Nastavit na neviditelné - budou se postupně zobrazovat
                    cubes[i]!.Opacity = 0;
                }
            }
        }

        private void StartCubesSequentially()
        {
            // Spusť první kostku okamžitě
            ShowAndAnimateCube(0, 0);
            
            // Plánuj zbývající kostky s random intervaly
            ScheduleNextCube(1);
        }

        private void ScheduleNextCube(int cubeIndex)
        {
            if (cubeIndex >= 5) return; // Všechny kostky jsou spuštěny

            // Kratší delay - 0.5 až 2 sekundy mezi kostkami
            int delayMs = _random.Next(500, 2000);
            
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = delayMs;
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                timer.Dispose();
                
                double elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
                if (elapsedSeconds < TotalDurationSeconds) // Jenom pokud není načítání hotovo
                {
                    ShowAndAnimateCube(cubeIndex, elapsedSeconds);
                    
                    // Naplánuj další kostku
                    ScheduleNextCube(cubeIndex + 1);
                }
            };
            timer.Start();
        }

        private void ShowAndAnimateCube(int cubeIndex, double delayFromStart)
        {
            Image? cube = this.FindName($"Cube{cubeIndex + 1}") as Image;
            RotateTransform? rotation = this.FindName($"Cube{cubeIndex + 1}Rotation") as RotateTransform;

            if (cube == null || rotation == null) return;

            Debug.WriteLine($"[SplashScreen] Showing Cube{cubeIndex + 1}");

            // Fade in animace
            var fadeInAnimation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            cube.BeginAnimation(Image.OpacityProperty, fadeInAnimation);

            // Orbitalní animace - kostka se točí kolem loga
            CreateOrbitalAnimation(cube, rotation, cubeIndex);
        }

        private void CreateOrbitalAnimation(Image cube, RotateTransform rotation, int index)
        {
            // Random parametry pro každou krychli
            double radiusX = _random.Next(120, 180); // Radius X pro orbitu
            double radiusY = _random.Next(100, 150); // Radius Y pro orbitu
            double duration = _random.Next(8, 14); // 8-14 sekund pro orbitu
            double startAngle = (index * 72) + _random.Next(-15, 15); // Rovnoměrné rozložení
            int direction = _random.Next(2) == 0 ? 1 : -1; // Random směr (CW/CCW)

            var storyboard = new Storyboard();
            storyboard.RepeatBehavior = RepeatBehavior.Forever;

            // Animace X pozice - orbita
            var xAnimation = new DoubleAnimationUsingKeyFrames();
            xAnimation.Duration = TimeSpan.FromSeconds(duration);
            xAnimation.RepeatBehavior = RepeatBehavior.Forever;
            
            for (int i = 0; i <= 360; i += 5)
            {
                double angle = (startAngle + i * direction) * Math.PI / 180;
                double x = radiusX * Math.Cos(angle);
                xAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(x, KeyTime.FromPercent(i / 360.0)));
            }
            
            Storyboard.SetTarget(xAnimation, cube);
            Storyboard.SetTargetProperty(xAnimation, new PropertyPath(Canvas.LeftProperty));
            storyboard.Children.Add(xAnimation);

            // Animace Y pozice - orbita
            var yAnimation = new DoubleAnimationUsingKeyFrames();
            yAnimation.Duration = TimeSpan.FromSeconds(duration);
            yAnimation.RepeatBehavior = RepeatBehavior.Forever;
            
            for (int i = 0; i <= 360; i += 5)
            {
                double angle = (startAngle + i * direction) * Math.PI / 180;
                double y = radiusY * Math.Sin(angle);
                yAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(y, KeyTime.FromPercent(i / 360.0)));
            }
            
            Storyboard.SetTarget(yAnimation, cube);
            Storyboard.SetTargetProperty(yAnimation, new PropertyPath(Canvas.TopProperty));
            storyboard.Children.Add(yAnimation);

            // Rotace krychle kolem vlastního středu
            var rotationAnimation = new DoubleAnimation();
            rotationAnimation.From = 0;
            rotationAnimation.To = 360;
            rotationAnimation.Duration = TimeSpan.FromSeconds(_random.Next(4, 8));
            rotationAnimation.RepeatBehavior = RepeatBehavior.Forever;
            
            Storyboard.SetTarget(rotationAnimation, rotation);
            Storyboard.SetTargetProperty(rotationAnimation, new PropertyPath(RotateTransform.AngleProperty));
            storyboard.Children.Add(rotationAnimation);

            // Spusť animaci
            storyboard.Begin();
        }
    }
}

