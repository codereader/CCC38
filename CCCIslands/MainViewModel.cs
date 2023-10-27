using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
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
            ParseMap(value);
        }
    }

    private void ParseMap(Scenario currentScenario)
    {
       var temp = new ObservableCollection<VisualElement>();


        for (int y = 0; y < currentScenario.MapHeight; y++)
        {
            var currentLine = currentScenario.Map[y];
            for (int x = 0; x < currentScenario.MapWidth; x++)
            {
                temp.Add(new VisualElement()
                {
                    PositionX = x,
                    PositionY = y,
                    Value = currentLine[x].ToString()
                });
            }
        }

        VisualCollection = temp;

        RaisePropertyChanged(nameof(VisualCollection));
    }

    public ObservableCollection<VisualElement> VisualCollection { get; set; } = new();

    public MainViewModel()
    {
        ParseScenarios();
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
