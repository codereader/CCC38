using System.Windows.Media;

namespace CommonWPF
{
    public interface IGridItem
    {
        int PositionX { get; }
        int PositionY { get; }

        Brush BackGroundColor { get; }
    }
}