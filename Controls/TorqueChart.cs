using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using HMI_ScrewingMonitor.Models;

namespace HMI_ScrewingMonitor.Controls
{
    public class TorqueChart : Canvas
    {
        public TorqueChart()
        {
            // Enable tooltip functionality
            this.IsHitTestVisible = true;
        }

        public static readonly DependencyProperty TorqueHistoryProperty =
            DependencyProperty.Register("TorqueHistory", typeof(ObservableCollection<TorqueDataPoint>), typeof(TorqueChart),
                new PropertyMetadata(null, OnTorqueHistoryChanged));

        public static readonly DependencyProperty MinTorqueProperty =
            DependencyProperty.Register("MinTorque", typeof(float), typeof(TorqueChart),
                new PropertyMetadata(0f, OnRangeChanged));

        public static readonly DependencyProperty MaxTorqueProperty =
            DependencyProperty.Register("MaxTorque", typeof(float), typeof(TorqueChart),
                new PropertyMetadata(10f, OnRangeChanged));

        public ObservableCollection<TorqueDataPoint> TorqueHistory
        {
            get { return (ObservableCollection<TorqueDataPoint>)GetValue(TorqueHistoryProperty); }
            set { SetValue(TorqueHistoryProperty, value); }
        }

        public float MinTorque
        {
            get { return (float)GetValue(MinTorqueProperty); }
            set { SetValue(MinTorqueProperty, value); }
        }

        public float MaxTorque
        {
            get { return (float)GetValue(MaxTorqueProperty); }
            set { SetValue(MaxTorqueProperty, value); }
        }

        private static void OnTorqueHistoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var chart = (TorqueChart)d;

            // Hủy đăng ký sự kiện từ collection cũ để tránh rò rỉ bộ nhớ
            if (e.OldValue is ObservableCollection<TorqueDataPoint> oldCollection)
            {
                oldCollection.CollectionChanged -= chart.OnCollectionChanged;
            }

            // Đăng ký sự kiện cho collection mới
            if (e.NewValue is ObservableCollection<TorqueDataPoint> newCollection)
            {
                newCollection.CollectionChanged += chart.OnCollectionChanged;
            }

            // Vẽ lại biểu đồ với dữ liệu mới
            chart.UpdateChart();
        }

        private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var chart = (TorqueChart)d;
            chart.UpdateChart();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Khi collection thay đổi (thêm/xóa điểm dữ liệu), vẽ lại biểu đồ
            // Sử dụng Dispatcher để đảm bảo việc vẽ lại được thực hiện trên luồng UI
            // DispatcherTimer đã chạy trên luồng UI, nên có thể gọi trực tiếp.
            // Quan trọng nhất là gọi InvalidateVisual() để buộc WPF phải vẽ lại control.
            UpdateChart();
            InvalidateVisual();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateChart();
        }

        private void UpdateChart()
        {
            Children.Clear();

            if (TorqueHistory == null || TorqueHistory.Count == 0 || ActualWidth <= 0 || ActualHeight <= 0)
            {
                return;
            }


            var data = TorqueHistory.ToList();
            if (data.Count < 1)
            {
                return;
            }

            // Calculate chart area with proper margins
            double margin = 10;
            double chartWidth = ActualWidth - 2 * margin;
            double chartHeight = ActualHeight - 2 * margin;

            // Luôn sử dụng toàn bộ chiều rộng cho các điểm dữ liệu, phân bổ đều
            double stepX = data.Count > 1 ? chartWidth / (data.Count - 1) : 0;


            // --- LOGIC MỚI: Cố định trục Y để tránh "nhảy" ---
            // 1. Xác định thang đo hiển thị dựa trên Min/MaxTorque cấu hình
            float configuredRange = MaxTorque - MinTorque;
            if (configuredRange <= 0) configuredRange = 5; // Tránh chia cho 0
            float padding = configuredRange * 0.2f; // Thêm 20% khoảng đệm trên và dưới

            float displayMin = MinTorque - padding;
            float displayMax = MaxTorque + padding;

            // 2. Mở rộng thang đo nếu dữ liệu thực tế vượt ra ngoài khoảng đệm
            float dataMin = data.Min(d => d.Value);
            float dataMax = data.Max(d => d.Value);
            if (dataMin < displayMin) displayMin = dataMin - padding;
            if (dataMax > displayMax) displayMax = dataMax + padding;

            float totalRange = displayMax - displayMin;
            if (totalRange <= 0) totalRange = 1;

            // --- Vẽ các đường giới hạn tại vị trí chính xác ---
            double minY = margin + chartHeight - ((MinTorque - displayMin) / totalRange * chartHeight);
            double maxY = margin + chartHeight - ((MaxTorque - displayMin) / totalRange * chartHeight);

            // Các nhãn Min/Max đã được hiển thị bên ngoài control trong MainWindow.xaml,
            // nên chúng ta chỉ cần vẽ các đường kẻ.
            DrawHorizontalLine(minY, Brushes.Red, 1); // Đường MinTorque
            DrawHorizontalLine(maxY, Brushes.Red, 1); // Đường MaxTorque

            // Create path geometry for the torque line
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure();

            // Calculate first point with proper coordinates (using Value from TorqueDataPoint)
            double firstY = margin + chartHeight - ((data[0].Value - displayMin) / totalRange * chartHeight);
            firstY = Math.Max(margin, Math.Min(margin + chartHeight, firstY));
            pathFigure.StartPoint = new Point(margin, firstY);


            // Add line segments for all remaining points
            var lineSegment = new PolyLineSegment();
            for (int i = 1; i < data.Count; i++)
            {
                double x = margin + i * stepX;
                double y = margin + chartHeight - ((data[i].Value - displayMin) / totalRange * chartHeight);
                y = Math.Max(margin, Math.Min(margin + chartHeight, y)); // Clamp within bounds
                lineSegment.Points.Add(new Point(x, y));

            }

            pathFigure.Segments.Add(lineSegment);
            pathGeometry.Figures.Add(pathFigure);

            // Create the path
            var path = new System.Windows.Shapes.Path
            {
                Data = pathGeometry,
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Fill = null
            };

            Children.Add(path);

            // Add markers for all data points with timestamp information
            for (int i = 0; i < data.Count; i++)
            {
                double x = margin + i * stepX;
                double y = margin + chartHeight - ((data[i].Value - displayMin) / totalRange * chartHeight);
                y = Math.Max(margin, Math.Min(margin + chartHeight, y));

                // Create marker for each data point with larger size for easier hovering
                var marker = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = (i == data.Count - 1) ? Brushes.Red : Brushes.Blue, // Last point is red, others blue
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                // Simple string tooltip works better than custom ToolTip object
                marker.ToolTip = $"Giá trị: {data[i].Value:F1} Nm\nThời gian: {data[i].Timestamp:HH:mm:ss}";

                // Enable tooltip service
                System.Windows.Controls.ToolTipService.SetInitialShowDelay(marker, 500);
                System.Windows.Controls.ToolTipService.SetShowDuration(marker, 5000);

                Canvas.SetLeft(marker, x - 4);
                Canvas.SetTop(marker, y - 4);
                Children.Add(marker);
            }
        }

        private void DrawHorizontalLine(double y, Brush brush, double thickness)
        {
            double margin = 10;
            var line = new Line
            {
                X1 = margin,
                Y1 = y,
                X2 = ActualWidth - margin,
                Y2 = y,
                Stroke = brush,
                StrokeThickness = thickness,
                StrokeDashArray = new DoubleCollection { 3, 2 }
            };
            Children.Add(line);
        }
    }

    // Converter to handle ObservableCollection<TorqueDataPoint> binding
    public class TorqueHistoryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<TorqueDataPoint> collection)
                return collection;
            return new ObservableCollection<TorqueDataPoint>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}