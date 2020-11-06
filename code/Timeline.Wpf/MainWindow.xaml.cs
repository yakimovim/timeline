using EdlinSoftware.Timeline.Domain;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

            var timeLineStart = HalfFingerWidthInUnits;

            var minimumDuration = (_timeRange.Duration / timeLineLength) * FingerWidthInUnits;

            var tickInterval = TickIntervals.GetFirstTickIntervalWithGreaterDuration(minimumDuration);

            var ticks = tickInterval.GetTicksBetween(_timeRange.Start, _timeRange.End);

            foreach (var tick in ticks)
            {
                var tickXPos = timeLineStart + ((tick.Date - _timeRange.Start) / _timeRange.Duration) * timeLineLength;

                var tickLine = new Line();

                tickLine.X1 = tickXPos;
                tickLine.Y1 = yPos;
                tickLine.X2 = tickXPos;
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
                label.SetValue(Canvas.LeftProperty, tickXPos - (formattedText.Width / 2));
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

            var timeLineStart = HalfFingerWidthInUnits;

            foreach (var @event in events)
            {
                var eventXPos = ((@event.Start - _timeRange.Start) / _timeRange.Duration) * timeLineLength;

                var circle = new Ellipse();

                circle.Width = 20;
                circle.Height = 20;

                circle.SetValue(Canvas.LeftProperty, timeLineStart + eventXPos - circle.Width / 2);
                circle.SetValue(Canvas.TopProperty, yPos + 1);

                circle.Fill = Brushes.Black;
                circle.Stroke = Brushes.Black;

                circle.MouseDown += (sender, e) =>
                {
                    MessageBox.Show(@event.Description);
                    e.Handled = true;
                };

                canvas.Children.Add(circle);
            }
        }

        private void DrawIntervalEvents(Canvas canvas, double yPos, NonOverlappintEvents<string> events)
        {
            var canvasWidth = canvas.ActualWidth;

            var timeLineLength = canvasWidth - FingerWidthInUnits;

            var timeLineStart = HalfFingerWidthInUnits;

            foreach (var @event in events)
            {
                var eventStartXPos = timeLineStart + ((@event.Start - _timeRange.Start) / _timeRange.Duration) * timeLineLength;
                var eventEndXPos = timeLineStart + ((@event.End - _timeRange.Start) / _timeRange.Duration) * timeLineLength;

                var rectangle = new Rectangle();

                rectangle.Width = (eventEndXPos - eventStartXPos);
                rectangle.Height = FingerWidthInUnits - 2;

                rectangle.SetValue(Canvas.LeftProperty, eventStartXPos);
                rectangle.SetValue(Canvas.TopProperty, yPos + 1);

                rectangle.Stroke = Brushes.Black;
                rectangle.Fill = Brushes.White;

                rectangle.MouseDown += (sender, e) =>
                {
                    MessageBox.Show(@event.Description);
                    e.Handled = true;
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

        private void OnScaleTimeLine(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var canvasWidth = canvas.ActualWidth;

            var timeLineLength = canvasWidth - FingerWidthInUnits;

            var minimumDuration = (_timeRange.Duration / timeLineLength) * FingerWidthInUnits;

            if (e.Delta > 0)
            {
                _timeRange = _timeRange.ScaleUp(minimumDuration);
            }
            else if(e.Delta < 0)
            {
                _timeRange = _timeRange.ScaleDown(minimumDuration);
            }

            if(e.Delta != 0)
            {
                DrawEvents();
            }

            e.Handled = true;
        }

        private void OnMouseDownOnCanvas(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var position = Mouse.GetPosition(canvas);

            DragDrop.DoDragDrop(
                canvas, 
                position, 
                DragDropEffects.Move
            );
        }

        private void OnDropOnCanvas(object sender, DragEventArgs e)
        {
            if (e.OriginalSource != canvas) return;

            var currentPosition = e.GetPosition(canvas);

            var prevPosition = (Point)e.Data.GetData(typeof(Point));

            var deltaInUnits = prevPosition.X - currentPosition.X;

            var canvasWidth = canvas.ActualWidth;

            var timeLineLength = canvasWidth - FingerWidthInUnits;

            var deltaDuration = (_timeRange.Duration / timeLineLength) * deltaInUnits;

            _timeRange = _timeRange.Move(deltaDuration);

            DrawEvents();

            e.Handled = true;
        }
    }
}
