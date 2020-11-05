using EdlinSoftware.Timeline.Domain;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Timeline.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly double UnitsPerInch = 96;
        private static readonly double InchesPerUnit = 1 / UnitsPerInch;
        private static readonly double FingerWidthInInches = 1.5 / 2.54;
        private static readonly double FingerWidthInUnits = FingerWidthInInches * UnitsPerInch;
        private static readonly double HalfFingerWidthInUnits = FingerWidthInUnits / 2;

        private static readonly Event<string>[] _events =
        {
            new Event<string>(
                "Великая Отечественная Война", 
                SpecificDate.AnnoDomini(1941, 6, 21), 
                SpecificDate.AnnoDomini(1945, 5, 9)
            )
        };

        private TimeRange _timeRange;
        private bool _postponeRedrawing = false;

        public MainWindow()
        {
            InitializeComponent();

            _timeRange = new TimeRange(
                ToExactDateInfo(DateTime.MinValue),
                ToExactDateInfo(DateTime.MaxValue)
            );

            startPicker.SelectedDate = DateTime.MinValue;
            endPicker.SelectedDate = DateTime.MaxValue;
        }

        private static ExactDateInfo ToExactDateInfo(DateTime dateTime)
        {
            return new ExactDateInfo(
                Era.AnnoDomini,
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                dateTime.Hour
            );
        }

        private void CanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawEvents();
        }

        private void DrawEvents()
        {
            canvas.Children.Clear();

            if (canvas.ActualWidth < 1) return;

            DrawTimeLine(canvas, FingerWidthInInches / 2 * UnitsPerInch);

            DrawEvents(canvas, FingerWidthInInches * UnitsPerInch);
        }

        private void DrawTimeLine(Canvas canvas, double yPos)
        {
            DrawBaseLine(canvas, yPos);

            DrawTicks(canvas, yPos);
        }

        private void DrawBaseLine(Canvas canvas, double yPos)
        {
            var canvasWidth = canvas.ActualWidth;

            var line = new Line();

            line.X1 = HalfFingerWidthInUnits;
            line.Y1 = yPos;
            line.X2 = canvasWidth - HalfFingerWidthInUnits;
            line.Y2 = yPos;

            line.Fill = Brushes.Black;
            line.Stroke = Brushes.Black;
            line.StrokeThickness = 2;

            canvas.Children.Add(line);
        }

        private void DrawTicks(Canvas canvas, double yPos)
        {
            var canvasWidth = canvas.ActualWidth;

            var timeLineLength = canvasWidth - FingerWidthInUnits;

            var minimumDuration = (_timeRange.Duration / timeLineLength) * FingerWidthInUnits;

            var tickInterval = TickIntervals.GetFirstTickIntervalWithGreaterDuration(minimumDuration);

            var ticks = tickInterval.GetTicksBetween(_timeRange.Start, _timeRange.End);

            foreach (var tick in ticks)
            {
                var tickXPos = ((tick.Date - _timeRange.Start) / _timeRange.Duration) * timeLineLength;

                var tickLine = new Line();

                tickLine.X1 = tickXPos + HalfFingerWidthInUnits;
                tickLine.Y1 = yPos;
                tickLine.X2 = tickXPos + HalfFingerWidthInUnits;
                tickLine.Y2 = yPos + 4;

                tickLine.Fill = Brushes.Black;
                tickLine.Stroke = Brushes.Black;
                tickLine.StrokeThickness = 2;

                canvas.Children.Add(tickLine);

                var label = new Label();
                label.HorizontalAlignment = HorizontalAlignment.Center;

                var labelText = tick.ToString();

                var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

                var formattedText = new FormattedText(
                    labelText,
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    label.FontFamily.GetTypefaces().First(),
                    label.FontSize,
                    Brushes.Black,
                    pixelsPerDip
                );

                label.Content = labelText;
                label.SetValue(Canvas.LeftProperty, tickXPos + HalfFingerWidthInUnits - (formattedText.Width / 2));
                label.SetValue(Canvas.TopProperty, yPos + 4);

                canvas.Children.Add(label);
            }
        }

        private void DrawEvents(Canvas canvas, double yPos)
        {
            var canvasWidth = canvas.ActualWidth;

            var timeLineLength = canvasWidth - FingerWidthInUnits;

            var minimumDuration = (_timeRange.Duration / timeLineLength) * FingerWidthInUnits;

            var distributor = new EventsDistributor(minimumDuration);

            var eventsInRange = _events.Where(e => {
                if (e.Start >= _timeRange.End) return false;
                if (e.End <= _timeRange.Start) return false;
                return true;
            }).ToArray();

            var distribution = distributor.Distribute(eventsInRange);

            foreach (var eventsLine in distribution.Lines)
            {
                if(eventsLine.IsPointEvents)
                {
                    DrawPointEvents(canvas, yPos, eventsLine.Events);
                }
                else
                {
                    DrawIntervalEvents(canvas, yPos, eventsLine.Events);
                }

                yPos += FingerWidthInUnits;
            }
        }

        private void DrawPointEvents(Canvas canvas, double yPos, NonOverlappintEvents<string> events)
        {
            var canvasWidth = canvas.ActualWidth;

            var timeLineLength = canvasWidth - FingerWidthInUnits;

            foreach (var @event in events)
            {
                var eventXPos = ((@event.Start - _timeRange.Start) / _timeRange.Duration) * timeLineLength;

                var circle = new Ellipse();

                circle.Width = FingerWidthInUnits - 2;
                circle.Height = FingerWidthInUnits - 2;

                circle.SetValue(Canvas.LeftProperty, eventXPos - HalfFingerWidthInUnits + 1);
                circle.SetValue(Canvas.TopProperty, yPos + 1);

                circle.Fill = Brushes.Black;
                circle.Stroke = Brushes.Black;

                circle.MouseDown += (sender, e) =>
                {
                    MessageBox.Show(@event.Description);
                };

                canvas.Children.Add(circle);
            }
        }

        private void DrawIntervalEvents(Canvas canvas, double yPos, NonOverlappintEvents<string> events)
        {
            var canvasWidth = canvas.ActualWidth;

            var timeLineLength = canvasWidth - FingerWidthInUnits;

            foreach (var @event in events)
            {
                var eventStartXPos = ((@event.Start - _timeRange.Start) / _timeRange.Duration) * timeLineLength;
                var eventEndXPos = ((@event.End - _timeRange.Start) / _timeRange.Duration) * timeLineLength;

                var rectangle = new Rectangle();

                rectangle.Width = (eventEndXPos - eventStartXPos);
                rectangle.Height = FingerWidthInUnits - 2;

                rectangle.SetValue(Canvas.LeftProperty, eventStartXPos);
                rectangle.SetValue(Canvas.TopProperty, yPos + 1);

                rectangle.Stroke = Brushes.Black;

                rectangle.MouseDown += (sender, e) =>
                {
                    MessageBox.Show(@event.Description);
                };

                canvas.Children.Add(rectangle);
            }
        }

        private void StartDateChanged(object sender, SelectionChangedEventArgs e)
        {
            _timeRange = _timeRange.SetStart(ToExactDateInfo(startPicker.SelectedDate.Value));

            DrawEvents();
        }

        private void EndDateChanged(object sender, SelectionChangedEventArgs e)
        {
            _timeRange = _timeRange.SetEnd(ToExactDateInfo(endPicker.SelectedDate.Value));

            DrawEvents();
        }
    }
}
