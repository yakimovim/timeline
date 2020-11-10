using EdlinSoftware.Timeline.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
                "Первая мировая война",
                SpecificDate.AnnoDomini(1914, 7, 28),
                SpecificDate.AnnoDomini(1918, 11, 11)
            ),
            new Event<string>(
                "Вторая мировая война",
                SpecificDate.AnnoDomini(1939, 9, 1),
                SpecificDate.AnnoDomini(1945, 9, 2)
            ),
            new Event<string>(
                "Великая Отечественная Война", 
                SpecificDate.AnnoDomini(1941, 6, 21), 
                SpecificDate.AnnoDomini(1945, 5, 9)
            )
        };

        private readonly LinkedList<Action> _releases = new LinkedList<Action>();
        private TimeRange _timeRange;

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

        private void ReleaseAll()
        {
            foreach (var release in _releases)
            {
                release();
            }

            _releases.Clear();
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
            ReleaseAll();

            canvas.Children.Clear();

            if (canvas.ActualWidth < 1) return;

            DrawTimeLine(canvas, FingerWidthInInches / 2 * UnitsPerInch);

            DrawEvents(canvas, 2 * FingerWidthInInches * UnitsPerInch);
        }

        private void DrawTimeLine(Canvas canvas, double yPos)
        {
            DrawBaseLine(canvas, yPos);

            DrawTicks(canvas, yPos);

            DrawGrips(canvas, yPos);
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

                var tickLabel = new TextBlock();
                tickLabel.TextAlignment = TextAlignment.Center;

                var labelText = tick.ToString();

                var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

                var formattedText = new FormattedText(
                    labelText,
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    tickLabel.FontFamily.GetTypefaces().First(),
                    tickLabel.FontSize,
                    Brushes.Black,
                    pixelsPerDip
                );

                tickLabel.Text = labelText;
                tickLabel.SetValue(Canvas.LeftProperty, tickXPos - (formattedText.Width / 2));
                tickLabel.SetValue(Canvas.TopProperty, yPos + 4);

                canvas.Children.Add(tickLabel);
            }
        }

        private void DrawGrips(Canvas canvas, double yPos)
        {
            var canvasWidth = canvas.ActualWidth;

            var leftGrip = new Rectangle
            {
                Width = 4,
                Height = HalfFingerWidthInUnits,
                Fill = Brushes.Black,
                Cursor = Cursors.SizeWE
            };
            leftGrip.SetValue(Canvas.LeftProperty, HalfFingerWidthInUnits / 2 - 2);
            leftGrip.SetValue(Canvas.TopProperty, HalfFingerWidthInUnits / 2);
            MouseButtonEventHandler leftGripMouseDownHandler = (sender, e) =>
            {
                leftGrip.CaptureMouse();
                e.Handled = true;
            };
            leftGrip.MouseDown += leftGripMouseDownHandler;
            _releases.AddLast(() => { leftGrip.MouseDown -= leftGripMouseDownHandler; });
            MouseButtonEventHandler leftGripMouseUpHandler = (sender, e) => {
                leftGrip.ReleaseMouseCapture();

                var currentX = e.GetPosition(canvas).X;

                var deltaInUnits = currentX - HalfFingerWidthInUnits / 2;

                var canvasWidth = canvas.ActualWidth;

                var timeLineLength = canvasWidth - FingerWidthInUnits;

                var deltaDuration = (_timeRange.Duration / timeLineLength) * deltaInUnits;

                _timeRange = _timeRange.SetStart(_timeRange.Start + deltaDuration);

                DrawEvents();

                e.Handled = true;
            };
            leftGrip.MouseUp += leftGripMouseUpHandler;
            _releases.AddLast(() => { leftGrip.MouseUp -= leftGripMouseUpHandler; });

            canvas.Children.Add(leftGrip);

            var rightGrip = new Rectangle
            {
                Width = 4,
                Height = HalfFingerWidthInUnits,
                Fill = Brushes.Black,
                Cursor = Cursors.SizeWE
            };
            rightGrip.SetValue(Canvas.LeftProperty, canvasWidth - HalfFingerWidthInUnits / 2 - 2);
            rightGrip.SetValue(Canvas.TopProperty, HalfFingerWidthInUnits / 2);
            MouseButtonEventHandler rightGripMouseDownHandler = (sender, e) => {
                rightGrip.CaptureMouse();
                e.Handled = true;
            };
            rightGrip.MouseDown += rightGripMouseDownHandler;
            _releases.AddLast(() => { rightGrip.MouseDown -= rightGripMouseDownHandler; });
            MouseButtonEventHandler rightGripMouseUpHandler = (sender, e) => {
                rightGrip.ReleaseMouseCapture();

                var currentX = e.GetPosition(canvas).X;

                var canvasWidth = canvas.ActualWidth;

                var deltaInUnits = currentX - (canvasWidth - HalfFingerWidthInUnits / 2);

                var timeLineLength = canvasWidth - FingerWidthInUnits;

                var deltaDuration = (_timeRange.Duration / timeLineLength) * deltaInUnits;

                _timeRange = _timeRange.SetEnd(_timeRange.End + deltaDuration);

                DrawEvents();

                e.Handled = true;
            };
            rightGrip.MouseUp += rightGripMouseUpHandler;
            _releases.AddLast(() => { rightGrip.MouseUp -= rightGripMouseUpHandler; });

            canvas.Children.Add(rightGrip);
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
                    DrawIntervalEvents(canvas, yPos, eventsLine.Events, Colors.LightGoldenrodYellow);
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

                MouseButtonEventHandler mouseDownHandler = (sender, e) =>
                {
                    MessageBox.Show(@event.Description);
                    e.Handled = true;
                };
                circle.MouseDown += mouseDownHandler;
                _releases.AddLast(() => { circle.MouseDown -= mouseDownHandler; });

                canvas.Children.Add(circle);
            }
        }

        private void DrawIntervalEvents(
            Canvas canvas, 
            double yPos, 
            NonOverlappintEvents<string> events,
            Color baseColor)
        {
            var canvasWidth = canvas.ActualWidth;

            var timeLineLength = canvasWidth - FingerWidthInUnits;

            var timeLineStart = HalfFingerWidthInUnits;

            foreach (var @event in events)
            {
                var eventStartXPos = timeLineStart + ((@event.Start - _timeRange.Start) / _timeRange.Duration) * timeLineLength;
                var eventEndXPos = timeLineStart + ((@event.End - _timeRange.Start) / _timeRange.Duration) * timeLineLength;

                var noLeftSide = @event.Start < _timeRange.Start;
                var noRightSide = @event.End > _timeRange.End;

                var rectangle = new Border();

                rectangle.Width = noLeftSide && noRightSide
                    ? timeLineLength
                    : noLeftSide
                        ? (eventEndXPos - timeLineStart)
                        : noRightSide
                            ? (timeLineLength + timeLineStart - eventStartXPos)
                            : (eventEndXPos - eventStartXPos);
                rectangle.Height = FingerWidthInUnits - 2;

                rectangle.SetValue(
                    Canvas.LeftProperty, 
                    noLeftSide ? timeLineStart : eventStartXPos
                );
                rectangle.SetValue(Canvas.TopProperty, yPos + 1);

                rectangle.BorderBrush = Brushes.Black;
                rectangle.BorderThickness = new Thickness(
                    noLeftSide ? 0 : 1,
                    1,
                    noRightSide ? 0 : 1,
                    1
                );
                rectangle.Background = new SolidColorBrush(baseColor);

                canvas.Children.Add(rectangle);

                var text = new TextBlock();
                text.Text = @event.Description;
                text.Width = rectangle.Width - 2;
                // text.Height = rectangle.Height - 2;
                text.VerticalAlignment = VerticalAlignment.Center;
                text.SetValue(Canvas.LeftProperty, eventStartXPos + 1);
                text.SetValue(Canvas.TopProperty, yPos + 1 + 1);
                text.TextAlignment = TextAlignment.Center;
                text.FontWeight = FontWeights.Bold;

                MouseButtonEventHandler mouseDownHander = (sender, e) =>
                {
                    MessageBox.Show(@event.Description);
                    e.Handled = true;
                };
                text.MouseDown += mouseDownHander;
                _releases.AddLast(() => { text.MouseDown -= mouseDownHander; });

                rectangle.Child = text;
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

        private void OnScaleTimeLine(object sender, MouseWheelEventArgs e)
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

        private void OnMouseDownOnCanvas(object sender, MouseButtonEventArgs e)
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
