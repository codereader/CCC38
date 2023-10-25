using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonWPF;

namespace CCCIslands;

class MainViewModel : ViewModelBase
{
    
    public int CurrentLevel
    {
        get => GetValue<int>(); 
        set => SetValue(value);
    }

    public ObservableCollection<ScenarioNode> Scenarios { get; set; } = new();

    public int CurrentFile
    {
        get => GetValue<int>();
        set => SetValue(value);
    }

    public ObservableCollection<VisualElement> VisualCollection { get; set; } = new();

}
