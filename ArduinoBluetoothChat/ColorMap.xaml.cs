using System;
using System.Linq;
using System.Numerics;
using ColorPicker.Shared;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using static ColorPicker.Shared.ColorHelper;

namespace ColorPickerUwp
{
    public sealed partial class ColorMap : UserControl
    {
        private PointerPoint lastPoint;
        private double colorX, colorY;
        private WriteableBitmap bmp3;

        private readonly LinearGradientBrush LightnessGradient;
        private readonly GradientStop LightnessStart;
        private readonly GradientStop LightnessMid;
        private readonly GradientStop LightnessEnd;

        private bool settingColor;
        private bool settingLightness;

        public ColorMap()
        {
            this.InitializeComponent();

            this.Loaded += MeshCanvas_Loaded;

            this.ellipse.PointerMoved += Image3_PointerMoved;
            this.ellipse.PointerPressed += Image3_PointerPressed;
            this.ellipse.PointerReleased += Image3_PointerReleased;

            this.LightnessGradient = new LinearGradientBrush();
            LightnessGradient.StartPoint = new Point(0, 0);
            LightnessGradient.EndPoint = new Point(0, 1);
            LightnessStart = new GradientStop();
            LightnessMid = new GradientStop() { Offset = 0.5 };
            LightnessEnd = new GradientStop() { Offset = 1 };
            LightnessGradient.GradientStops = new GradientStopCollection()
            {
                LightnessStart, LightnessMid, LightnessEnd,
            };
            this.LightnessBackground.Fill = this.LightnessGradient;
        }

        public event Action<Color> ColorChanged;

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(ColorMap), new PropertyMetadata(new Color(), HandleColorChanged));
        private bool isloaded;

        private static void HandleColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var map = d as ColorMap;
            if (map != null && !map.settingColor)
            {
                var col = (Color)e.NewValue;
                var hsv = ToHSV(col);

                map.settingLightness = true;
                map.LightnessSlider.Value = hsv.Z;
                map.settingLightness = false;
                map.LightnessMid.Color = FromHSV(new Vector4(hsv.X, 1, 0.5f, 1));

                double angle = Math.PI * 2 * hsv.X;
                Vector2 normalized = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                Vector2 halfSize = new Vector2(
                    (float)map.ellipse.ActualWidth / 2,
                    (float)map.ellipse.ActualHeight / 2);
                Vector2 pos = (hsv.Y/2) * normalized * halfSize * new Vector2(1, -1) + halfSize;

                map.colorX = pos.X;
                map.colorY = pos.Y;
                map.UpdateThumb();

                map.ColorChanged(col);
            }
        }

        private void Image3_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            ellipse.CapturePointer(e.Pointer);
            this.lastPoint = e.GetCurrentPoint(ellipse);
            this.colorX = lastPoint.Position.X;
            this.colorY = lastPoint.Position.Y;
            this.UpdateColor();
            this.UpdateThumb();
            e.Handled = true;
        }

        private void Image3_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ellipse.ReleasePointerCapture(e.Pointer);
            this.lastPoint = null;
            e.Handled = true;
        }

        private void Image3_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (ellipse.PointerCaptures?.Any(p => p.PointerId == e.Pointer.PointerId) == true)
            {
                this.lastPoint = e.GetCurrentPoint(ellipse);
                this.colorX = lastPoint.Position.X;
                this.colorY = lastPoint.Position.Y;
                var bounds = new Rect(0, 0, ellipse.ActualWidth, ellipse.ActualHeight);
                if (bounds.Contains(lastPoint.Position) && UpdateColor())
                {
                    UpdateThumb();
                    e.Handled = true;
                }
            }
        }

        private void UpdateThumb()
        {
            Canvas.SetLeft(thumb, this.colorX - thumb.ActualWidth / 2);
            Canvas.SetTop(thumb, this.colorY - thumb.ActualHeight / 2);
            thumb.Visibility = Visibility.Visible;
        }

        private bool UpdateColor()
        {
            if (!this.isloaded) return false;
            var x = this.colorX / ellipse.ActualWidth;
            var y = 1 - this.colorY / ellipse.ActualHeight;
            var selectedColor = HueWheel.CalcWheelColor((float)x, 1 - (float)y, (float)this.LightnessSlider.Value);

            if (selectedColor.A > 0)
            {
                this.SetColor(selectedColor);
                this.LightnessStart.Color = Colors.White;
                this.LightnessMid.Color = HueWheel.CalcWheelColor((float)x, 1 - (float)y, 0.5f);
                this.LightnessEnd.Color = Colors.Black;
                return true;
            }

            return false;
        }

        private void SetColor(Color color)
        {
            this.settingColor = true;
            this.Color = color;
            this.ColorChanged?.Invoke(color);
            this.settingColor = false;
        }

        private async void MeshCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            bmp3 = new WriteableBitmap(1000, 1000);
            await HueWheel.CreateHueCircle(bmp3, 0.5f);
            this.image3.ImageSource = bmp3;
            this.isloaded = true;
        }

        private void lightnessChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (settingLightness) return;
            this.UpdateColor();
        }
    }
}
