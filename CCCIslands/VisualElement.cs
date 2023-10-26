using System;
using CommonWPF;

namespace CCCIslands;

public class VisualElement : ViewModelBase
{
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public string Value { get; set; }

    public void UpdateVisuals()
    {
        RaisePropertyChanged(nameof(Value));
    }

}
