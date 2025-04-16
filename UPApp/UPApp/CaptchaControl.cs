using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UPApp
{
    public class CaptchaControl : Canvas
    {
        private string _captchaText;
        private readonly Random _random = new Random();

        public string CaptchaText => _captchaText;

        public CaptchaControl()
        {
            Width = 200;
            Height = 80;
            Background = Brushes.White;
            GenerateNewCaptcha();
        }

        public void GenerateNewCaptcha()
        {
            Children.Clear();
            _captchaText = GenerateRandomText();
            DrawCaptcha();
        }

        private string GenerateRandomText()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] text = new char[4];
            for (int i = 0; i < 4; i++)
            {
                text[i] = chars[_random.Next(chars.Length)];
            }
            return new string(text);
        }

        private void DrawCaptcha()
        {
            // Draw background noise
            for (int i = 0; i < 50; i++)
            {
                var line = new Line
                {
                    X1 = _random.Next((int)Width),
                    Y1 = _random.Next((int)Height),
                    X2 = _random.Next((int)Width),
                    Y2 = _random.Next((int)Height),
                    Stroke = new SolidColorBrush(Color.FromRgb(
                        (byte)_random.Next(256),
                        (byte)_random.Next(256),
                        (byte)_random.Next(256))),
                    StrokeThickness = 1
                };
                Children.Add(line);
            }

            // Draw characters with random transformations
            for (int i = 0; i < _captchaText.Length; i++)
            {
                var textBlock = new TextBlock
                {
                    Text = _captchaText[i].ToString(),
                    FontSize = 30 + _random.Next(-5, 6),
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(
                        (byte)_random.Next(100),
                        (byte)_random.Next(100),
                        (byte)_random.Next(100)))
                };

                // Position characters with overlap
                double x = (Width / 5) * (i + 0.5) + _random.Next(-10, 11);
                double y = (Height / 2) + _random.Next(-15, 16);

                // Rotate the character
                var transform = new RotateTransform(_random.Next(-30, 31), 0, 0);
                textBlock.RenderTransform = transform;

                SetLeft(textBlock, x);
                SetTop(textBlock, y);

                Children.Add(textBlock);

                // Add strikethrough lines
                var strike = new Line
                {
                    X1 = x - 5,
                    Y1 = y + _random.Next(-10, 11),
                    X2 = x + 20,
                    Y2 = y + _random.Next(-10, 11),
                    Stroke = new SolidColorBrush(Color.FromRgb(
                        (byte)_random.Next(100),
                        (byte)_random.Next(100),
                        (byte)_random.Next(100))),
                    StrokeThickness = 2
                };
                Children.Add(strike);
            }
        }
    }
} 