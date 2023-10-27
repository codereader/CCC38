using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
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

    private WriteableBitmap? _scenarioBitmap;
    private WriteableBitmap? _inputBitmap;
    public Image MapImage
    {
        get => GetValue<Image>();
        set => SetValue(value);
    }


    private int _currentInputIndex = 0;
    public string CurrentInput
    {
        get => GetValue<string>();
        set
        {
            SetValue(value);
            if (CurrentScenario != null && !string.IsNullOrEmpty(value))
            {
                CurrentOutput = Navigator.Solve(CurrentScenario, value);
                DrawOutput(CurrentScenario, value, CurrentOutput);
            }
        }
    }


    public string CurrentOutput
    {
        get => GetValue<string>();
        set => SetValue(value);
    }


    public MainViewModel()
    {
        ParseScenarios();

        CurrentInput = string.Empty;

        PreviousInput = new RelayCommand(CanPreviousInput, DoPreviousInput);
        NextInput = new RelayCommand(CanNextInput, DoNextInput);
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

    private void ParseMap(Scenario currentScenario)
    {
        _scenarioBitmap = new WriteableBitmap(
                9 * currentScenario.MapWidth,
                9 * currentScenario.MapHeight,
                96,
                96,
                PixelFormats.Bgr32,
                null);

        // Reserve the back buffer for updates.
        _scenarioBitmap.Lock();

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

                    DrawRectangle(_scenarioBitmap, x, y, 9, color);
                }
            }

            // Specify the area of the bitmap that changed.
            _scenarioBitmap.AddDirtyRect(new Int32Rect(0, 0, _scenarioBitmap.PixelWidth, _scenarioBitmap.PixelHeight));
        }
        finally
        {
            // Release the back buffer and make it available for display.
            _scenarioBitmap.Unlock();
        }

        var image = new Image();

        image.Stretch = Stretch.None;
        image.Margin = new Thickness(0);

        image.Source = _scenarioBitmap;

        MapImage = image;

        RaisePropertyChanged(nameof(MapImage));
    }

    private void DrawRectangle(WriteableBitmap writeableBitmap, int mapX, int mapY, int size, Color color)
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
                SetPixel(writeableBitmap, posX, posY, color);
            }
        }
    }

    private void DrawX(WriteableBitmap writeableBitmap, int mapX, int mapY, int size, Color color)
    {
        // start positions on bitmap
        var startX = mapX * size;
        var startY = mapY * size;

        var half = (int)(size / 2);

        SetPixel(writeableBitmap, startX + half, startY + half, color);


        // rectangle
        for (var i = 1; i < half; i++)
        {
            SetPixel(writeableBitmap, startX + i, startY + i, color);
            SetPixel(writeableBitmap, startX + half + i, startY + half + i, color);

            SetPixel(writeableBitmap, startX + half - i, startY + half + i, color);
            SetPixel(writeableBitmap, startX + half + i, startY + half - i, color);


        }
    }

    private void DrawLines(WriteableBitmap writeableBitmap, List<Vector2> linepositions, int size, Color color)
    {
        var half = (int)(size / 2);

        // line start position on bitmap
        for (int i = 0; i < linepositions.Count - 1; i++)
        {
            var startMapPos = linepositions[i];
            var endMappos = linepositions[i + 1];

            var lineStart = startMapPos * size + new Vector2(half, half);
            var lineEnd = endMappos * size + new Vector2(half, half);
            var linePoints = Navigator.GetPointsOnLine(lineStart, lineEnd);

            foreach (var point in linePoints)
            {
                SetPixel(writeableBitmap, (int)point.X, (int)point.Y, color);
            }
        }
    }


    private void SetPixel(WriteableBitmap writeableBitmap, int x, int y, Color color)
    {
        var bytesPerPixel = (writeableBitmap.Format.BitsPerPixel + 7) / 8;
        var stride = writeableBitmap.PixelWidth * bytesPerPixel;

        int posX = x * bytesPerPixel;
        int posY = y * stride;
        unsafe
        {
            // Get a pointer to the back buffer.
            var backBuffer = writeableBitmap.BackBuffer;

            // Find the address of the pixel to draw.
            backBuffer += y * writeableBitmap.BackBufferStride;
            backBuffer += x * 4;

            // Compute the pixel's color.
            int color_data = color.R << 16; // R
            color_data |= color.G << 8;   // G
            color_data |= color.B << 0;   // B

            // Assign the color data to the pixel.
            *((int*)backBuffer) = color_data;
        }

    }

    private void DrawOutput(Scenario currentScenario, string input, string output)
    {
        _inputBitmap = new WriteableBitmap(_scenarioBitmap);

        var level = currentScenario.Level;

        var drawPositions = new List<Vector2>();
        var linePositions = new List<Vector2>();

        var positioncolor = new Color();
        positioncolor = Color.FromRgb(255, 0, 0);
        var linecolor = new Color();
        linecolor = Color.FromRgb(255, 255, 255);

        switch (level)
        {
            case 1:
                drawPositions.Add(Navigator.ParseVector2(input));
                break;

            case 2:
                drawPositions = ParseVectors(input);
                break;

            case 3:
                linePositions = ParseVectors(input);
                break;

            case 4:
                drawPositions = ParseVectors(input);
                linePositions = ParseVectors(output);
                break;

            case 5:
                linePositions = ParseVectors(output);
                break;

            case 6:
                linePositions = ParseVectors(output);
                break;

            case 7:
                linePositions = ParseVectors(output);
                break;


        }

        _inputBitmap.Lock();

        DrawLines(_inputBitmap, linePositions, 9, linecolor);

        foreach (var drawPosition in drawPositions)
        {
            DrawX(_inputBitmap, (int)drawPosition.X, (int)drawPosition.Y, 9, positioncolor);
        }

        _inputBitmap.Unlock();

        MapImage.Source = _inputBitmap;
        RaisePropertyChanged(nameof(MapImage));
    }

    private List<Vector2> ParseVectors(string input)
    {
        var vectors = new List<Vector2>();
        var numberPairs = input.Split(' ').ToArray();
        foreach (var pair in numberPairs)
        {
            vectors.Add(Navigator.ParseVector2(pair));
        }
        return vectors;
    }
}
