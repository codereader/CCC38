using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommonWPF;
using IslandsLib;

namespace CCCIslands;

class MainViewModel : ViewModelBase
{

    public ObservableCollection<ScenarioNode> Scenarios { get; set; } = new();
    public Scenario CurrentScenario
    {
        get => GetValue<Scenario>();
        set
        {
            SetValue(value);
            _currentInputIndex = 0;
            if (value != null)
            {
                ParseMap(value);
                CurrentInput = value.InputLines[_currentInputIndex];
            }
        }
    }

    public Image MapImage
    {
        get => GetValue<Image>();
        set => SetValue(value);
    }

    public WriteableBitmap MapBitmap { get; set; }

    private int _currentInputIndex = 0;
    public string CurrentInput
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    public string CurrentOutput
    {
        get => GetValue<string>();
        set => SetValue(value);
    }



    private void ParseMap(Scenario currentScenario)
    {
        MapBitmap = new WriteableBitmap(
                9 * currentScenario.MapWidth,
                9 * currentScenario.MapHeight,
                96,
                96,
                PixelFormats.Bgr32,
                null);

        // Reserve the back buffer for updates.
        MapBitmap.Lock();

        try
        {
            for (int y = 0; y < currentScenario.MapHeight; y++)
            {
                var currentLine = currentScenario.Map[y];
                for (int x = 0; x < currentScenario.MapWidth; x++)
                {
                    var color = new Color();
                    color = currentLine[x] switch
                    {
                        'L' => Color.FromRgb(255, 155, 75),
                        'W' => color = Color.FromRgb(30, 110, 255),
                        _ => Color.FromRgb(255, 255, 255)
                    };

                    DrawRectangle(x, y, 9, color);
                }
            }

            // Specify the area of the bitmap that changed.
            MapBitmap.AddDirtyRect(new Int32Rect(0, 0, MapBitmap.PixelWidth, MapBitmap.PixelHeight));
        }
        finally
        {
            // Release the back buffer and make it available for display.
            MapBitmap.Unlock();
        }

        var image = new Image();

        image.Stretch = Stretch.None;
        image.Margin = new Thickness(0);

        image.Source = MapBitmap;

        MapImage = image;

        RaisePropertyChanged(nameof(MapImage));
    }


    public void DrawRectangle(int mapX, int mapY, int size, Color color)
    {
        // start positions on bitmap
        var startX = mapX * size;
        var startY = mapY * size;

        // rectangle
        for (var deltaX = 0; deltaX < size; deltaX++)
        {
            for (var deltaY = 0; deltaY < size; deltaY++)
            {
                var posX = startX + deltaX;
                var posY = startY + deltaY;
                SetPixel(posX, posY, color);
            }
        }
    }


    public void SetPixel(int x, int y, Color color)
    {
        var bytesPerPixel = (MapBitmap.Format.BitsPerPixel + 7) / 8;
        var stride = MapBitmap.PixelWidth * bytesPerPixel;

        int posX = x * bytesPerPixel;
        int posY = y * stride;
        unsafe
        {
            // Get a pointer to the back buffer.
            var backBuffer = MapBitmap.BackBuffer;

            // Find the address of the pixel to draw.
            backBuffer += y * MapBitmap.BackBufferStride;
            backBuffer += x * 4;

            // Compute the pixel's color.
            int color_data = color.R << 16; // R
            color_data |= color.G << 8;   // G
            color_data |= color.B << 0;   // B

            // Assign the color data to the pixel.
            *((int*)backBuffer) = color_data;
        }

    }





    public ObservableCollection<VisualElement> VisualCollection { get; set; } = new();

    public MainViewModel()
    {
        ParseScenarios();

        CurrentInput = string.Empty;


        Solve = new RelayCommand(CanSolve, DoSolve);
        PreviousInput = new RelayCommand(CanPreviousInput, DoPreviousInput);
        NextInput = new RelayCommand(CanNextInput, DoNextInput);


    }

    public RelayCommand Solve { get; }
    public bool CanSolve()
    {
        return CurrentScenario != null;
    }
    public void DoSolve()
    {
       CurrentOutput = Navigator.Solve(CurrentScenario, CurrentInput);
    }

    public RelayCommand PreviousInput { get; }
    public bool CanPreviousInput()
    {
        return CurrentScenario != null && _currentInputIndex > 0;
    }
    public void DoPreviousInput()
    {
        CurrentInput = CurrentScenario.InputLines[--_currentInputIndex];
    }

    public RelayCommand NextInput { get; }
    public bool CanNextInput()
    {
        return CurrentScenario != null && _currentInputIndex < CurrentScenario.InputLines.Count - 1;
    }
    public void DoNextInput()
    {
        CurrentInput = CurrentScenario.InputLines[++_currentInputIndex];

    }


    private void ParseScenarios()
    {
        var inputPath = $"../../../../Files";
        var inputFiles = Directory.GetFiles(inputPath, "*.in");

        var regex = new Regex(@"level(\d)_(\d)");

        foreach (var file in inputFiles)
        {
            if (!file.Contains("example"))
            {
                // level2_5.in
                var fileName = Path.GetFileNameWithoutExtension(file);
                var match = regex.Match(fileName);
                var level = match.Groups[1].Value;
                var fileNumber = match.Groups[2].Value;

                var levelName = $"Level {level}";
                var scenarioName = $"Scenario {fileNumber}";

                var scenario = new Scenario(int.Parse(level), int.Parse(fileNumber));

                var levelNode = Scenarios.FirstOrDefault(n => n.Name == levelName);
                if (levelNode == null)
                {
                    levelNode = new ScenarioNode()
                    {
                        Name = levelName
                    };
                    Scenarios.Add(levelNode);
                }

                levelNode.Children.Add(new ScenarioNode()
                {
                    Name = scenarioName,
                    Scenario = scenario
                });

            }
        }
    }

    public void UpdateVisuals()
    {
        foreach (var location in VisualCollection)
        {
            location.UpdateVisuals();
        }
    }

}
