using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Treap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ParkingGarage _garage;

        public MainWindow()
        {
            InitializeComponent();
            _garage = new ParkingGarage();

            CmbFloors.SelectionChanged += (s, e) => RefreshOccupiedList();
        }
        private void LogMessage(string message)
        {
            TxtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            TxtLog.ScrollToEnd();
        }

        private int GetSelectedFloor()
        {
            return CmbFloors.SelectedIndex;
        }

        private void RefreshOccupiedList()
        {
            if (LstOccupiedSpots == null) return; 

            int floor = GetSelectedFloor();
            var spots = _garage.GetOccupiedSpots(floor);

            LstOccupiedSpots.Items.Clear();
            foreach (var spot in spots)
            {
                LstOccupiedSpots.Items.Add($"Místo {spot.Key} - {spot.Value}");
            }
            UpdateTreeVisualization();
        }

        private void BtnOccupy_Click(object sender, RoutedEventArgs e)
        {
            int floor = GetSelectedFloor();

            if (int.TryParse(TxtSpotNumber.Text, out int spotNumber))
            {
                var data = new VehicleData(TxtVehicleData.Text, "Neznámý");

                bool success = _garage.OccupySpot(floor, spotNumber, data);

                if (success)
                {
                    LogMessage($"ÚSPĚCH: Podlaží {floor + 1}, místo {spotNumber} bylo obsazeno.");
                    RefreshOccupiedList();
                    UpdateTreeVisualization();
                }
                else
                {
                    LogMessage($"CHYBA: Místo {spotNumber} na podlaží {floor + 1} je již obsazené nebo neexistuje!");
                }
            }
            else
            {
                LogMessage("CHYBA: Zadejte platné číslo parkovacího místa.");
            }
        }

        private void BtnFree_Click(object sender, RoutedEventArgs e)
        {
            int floor = GetSelectedFloor();

            if (int.TryParse(TxtSpotNumber.Text, out int spotNumber))
            {
                bool success = _garage.FreeSpot(floor, spotNumber);

                if (success)
                {
                    LogMessage($"ÚSPĚCH: Podlaží {floor + 1}, místo {spotNumber} bylo uvolněno.");
                    RefreshOccupiedList();
                    UpdateTreeVisualization();
                }
                else
                {
                    LogMessage($"CHYBA: Místo {spotNumber} na podlaží {floor + 1} nelze uvolnit (není obsazené).");
                }
            }
            else
            {
                LogMessage("CHYBA: Zadejte platné číslo parkovacího místa.");
            }
        }

        private void BtnCheck_Click(object sender, RoutedEventArgs e)
        {
            int floor = GetSelectedFloor();

            if (int.TryParse(TxtSpotNumber.Text, out int spotNumber))
            {
                bool isOccupied = _garage.IsSpotOccupied(floor, spotNumber);
                if (isOccupied)
                {
                    LogMessage($"INFO: Místo {spotNumber} na podlaží {floor + 1} JE obsazené.");
                }
                else
                {
                    LogMessage($"INFO: Místo {spotNumber} na podlaží {floor + 1} JE volné.");
                }
            }
            else
            {
                LogMessage("CHYBA: Zadejte platné číslo parkovacího místa.");
            }
        }

        private void BtnNearest_Click(object sender, RoutedEventArgs e)
        {
            int floor = GetSelectedFloor();

            if (int.TryParse(TxtSpotNumber.Text, out int spotNumber))
            {
                try
                {
                    int nearestSpot = _garage.FindNearestFreeSpot(floor, spotNumber);

                    if (nearestSpot == -1)
                    {
                        LogMessage($"INFO: Podlaží {floor + 1} je zcela obsazené!");
                    }
                    else if (nearestSpot == spotNumber)
                    {
                        LogMessage($"INFO: Požadované místo {spotNumber} je rovnou volné.");
                    }
                    else
                    {
                        LogMessage($"INFO: Místo {spotNumber} je obsazené. Nejbližší volné je: {nearestSpot}");
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"CHYBA: {ex.Message}");
                }
            }
            else
            {
                LogMessage("CHYBA: Zadejte platné číslo parkovacího místa.");
            }
        }

        private void BtnLoadFile_Click(object sender, RoutedEventArgs e)
        {
            _garage.OccupySpot(0, 2, new VehicleData("1A1-1111", "Karel"));
            _garage.OccupySpot(0, 5, new VehicleData("2B2-2222", "Jana"));
            _garage.OccupySpot(0, 6, new VehicleData("3C3-3333", "Petr")); 
            _garage.OccupySpot(0, 7, new VehicleData("4D4-4444", "Eva"));  
            _garage.OccupySpot(0, 10, new VehicleData("5E5-5555", "Tomáš"));

            LogMessage("INFO: Testovací data úspěšně načtena.");
            RefreshOccupiedList();
            UpdateTreeVisualization();
        }
        private void UpdateTreeVisualization()
        {
            if (CanvasTree == null) return; 

            CanvasTree.Children.Clear(); 

            int floor = GetSelectedFloor();
            var treap = _garage.GetFloorTreap(floor);
            var root = treap.Root;

            if (root != null)
            {
                double startX = CanvasTree.ActualWidth / 2;
                if (startX == 0) startX = 350; 

                double startY = 40;
                double initialHorizontalGap = startX / 1.5; 

                DrawNode(root, startX, startY, initialHorizontalGap);
            }
        }

        private void DrawNode(TreapNode<int, VehicleData> node, double x, double y, double horizontalGap)
        {
            double verticalGap = 70; 
            double nodeRadius = 25;  

            if (node.Left != null)
            {
                double childX = x - horizontalGap;
                double childY = y + verticalGap;
                DrawEdge(x, y, childX, childY);
                DrawNode(node.Left, childX, childY, horizontalGap / 2); 
            }

            if (node.Right != null)
            {
                double childX = x + horizontalGap;
                double childY = y + verticalGap;
                DrawEdge(x, y, childX, childY);
                DrawNode(node.Right, childX, childY, horizontalGap / 2); 
            }

            Ellipse ellipse = new Ellipse
            {
                Width = nodeRadius * 2,
                Height = nodeRadius * 2,
                Fill = Brushes.LightSkyBlue,
                Stroke = Brushes.SteelBlue,
                StrokeThickness = 2
            };
            Canvas.SetLeft(ellipse, x - nodeRadius);
            Canvas.SetTop(ellipse, y - nodeRadius);
            CanvasTree.Children.Add(ellipse);

            TextBlock textBlock = new TextBlock
            {
                Text = $"{node.Key}\n({node.Priority})",
                TextAlignment = TextAlignment.Center,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black
            };

            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(textBlock, x - textBlock.DesiredSize.Width / 2);
            Canvas.SetTop(textBlock, y - textBlock.DesiredSize.Height / 2);
            CanvasTree.Children.Add(textBlock);
        }
        private void DrawEdge(double startX, double startY, double endX, double endY)
        {
            Line line = new Line
            {
                X1 = startX,
                Y1 = startY,
                X2 = endX,
                Y2 = endY,
                Stroke = Brushes.Gray,
                StrokeThickness = 2
            };
            CanvasTree.Children.Add(line);
        }
    }
}