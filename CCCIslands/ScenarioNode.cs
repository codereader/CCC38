using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IslandsLib;

namespace CCCIslands;

class ScenarioNode
{
    public ObservableCollection<ScenarioNode> Children { get; set; } = new();

    public Scenario Scenario { get; set; }

}
