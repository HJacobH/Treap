using Microsoft.Win32;
using System.IO;
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

            CmbFloors.SelectionChanged += (s, e) =>
            {
                RefreshOccupiedList();
                UpdateTreeVisualization();
            };

            this.SizeChanged += (s, e) => UpdateTreeVisualization();
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
            var treap = _garage.GetFloorTreap(floor);
            int totalSpots = _garage.SpotsPerFloor; 

            LstOccupiedSpots.Items.Clear();

            for (int spotNumber = 1; spotNumber <= totalSpots; spotNumber++)
            {
                if (_garage.IsSpotOccupied(floor, spotNumber))
                {
                    var vehicleData = treap.GetValue(spotNumber);
                    LstOccupiedSpots.Items.Add($"Místo {spotNumber:D2}: OBSAZENO -> {vehicleData}");
                }
                else
                {
                    LstOccupiedSpots.Items.Add($"Místo {spotNumber:D2}: [ VOLNÉ ]");
                }
            }
        }

        private void BtnOccupy_Click(object sender, RoutedEventArgs e)
        {
            int floor = GetSelectedFloor();

            if (int.TryParse(TxtSpotNumber.Text, out int spotNumber))
            {
                string spz = string.IsNullOrWhiteSpace(TxtLicensePlate.Text) ? "Neznámá SPZ" : TxtLicensePlate.Text;
                string owner = string.IsNullOrWhiteSpace(TxtOwnerName.Text) ? "Neznámý majitel" : TxtOwnerName.Text;

                var data = new VehicleData(spz, owner);

                bool success = _garage.OccupySpot(floor, spotNumber, data);

                if (success)
                {
                    LogMessage($"ÚSPĚCH: Podlaží {floor + 1}, místo {spotNumber} obsazeno ({spz}).");
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
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Vyberte soubor s testovacími daty",
                Filter = "Textové soubory (*.txt;*.csv)|*.txt;*.csv|Všechny soubory (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string[] lines = File.ReadAllLines(openFileDialog.FileName);
                    int loadedCount = 0;

                    foreach (string line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        string[] parts = line.Split(';');

                        if (parts.Length == 4)
                        {
                            if (int.TryParse(parts[0], out int floor) && int.TryParse(parts[1], out int spot))
                            {
                                var data = new VehicleData(parts[2], parts[3]);

                                if (_garage.OccupySpot(floor, spot, data))
                                {
                                    loadedCount++;
                                }
                            }
                        }
                    }

                    LogMessage($"INFO: Úspěšně načteno {loadedCount} záznamů ze souboru.");
                    RefreshOccupiedList();
                    UpdateTreeVisualization();
                }
                catch (Exception ex)
                {
                    LogMessage($"CHYBA při čtení souboru: {ex.Message}");
                }
            }
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
                double startX = 500;
                double startY = 40;
                double initialHorizontalGap = 250;

                DrawNode(root, startX, startY, initialHorizontalGap);
            }
        }

        private void DrawNode(TreapNode<int, VehicleData> node, double x, double y, double horizontalGap)
        {
            double verticalGap = 60;
            double nodeRadius = 20;

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
                FontSize = 10,
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
        private void BtnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Uložit testovací data",
                Filter = "Textové soubory (*.txt)|*.txt|CSV soubory (*.csv)|*.csv|Všechny soubory (*.*)|*.*",
                DefaultExt = ".txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    List<string> linesToSave = new List<string>();

                    for (int floorIndex = 0; floorIndex < _garage.FloorsCount; floorIndex++)
                    {
                        var spots = _garage.GetOccupiedSpots(floorIndex);

                        foreach (var spot in spots)
                        {
                            linesToSave.Add($"{floorIndex};{spot.Key};{spot.Value.LicensePlate};{spot.Value.OwnerName}");
                        }
                    }

                    File.WriteAllLines(saveFileDialog.FileName, linesToSave);
                    LogMessage($"INFO: Úspěšně uloženo {linesToSave.Count} záznamů do souboru.");
                }
                catch (Exception ex)
                {
                    LogMessage($"CHYBA při ukládání souboru: {ex.Message}");
                }
            }
        }
        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            int floor = GetSelectedFloor();
            var treap = _garage.GetFloorTreap(floor);
            int totalSpots = _garage.SpotsPerFloor;

            LogMessage($"\n--- STAV PARKOVÁNÍ (Podlaží {floor + 1}) ---");

            for (int spotNumber = 1; spotNumber <= totalSpots; spotNumber++)
            {
                if (_garage.IsSpotOccupied(floor, spotNumber))
                {
                    var vehicleData = treap.GetValue(spotNumber);
                    LogMessage($"Místo č. {spotNumber:D2} -> {vehicleData}");
                }
                else
                {
                    LogMessage($"Místo č. {spotNumber:D2} -> VOLNÉ");
                }
            }
            LogMessage("--------------------------------------------------");
        }
    }
}