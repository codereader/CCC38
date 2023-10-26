using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CommonWPF
{
    public class ColourGradient
    {
        private List<ColourMapping> _colourMappings = new List<ColourMapping>();

        public double MinNumber => _colourMappings.Min(m => m.Number);
        public double MaxNumber => _colourMappings.Max(m => m.Number);

        public string GetColour(double currentNumber)
        {
            if (currentNumber < _colourMappings[0].Number)
            {
                // below minimum, return min colour
                return _colourMappings[0].HexColour;
            }

            for (int i = 0; i < _colourMappings.Count - 1; i++)
            {

                if (!IsInRange(currentNumber, i, i + 1))
                {
                    continue;
                }
                // within range, calculate colour gradient position
                var numDelta = _colourMappings[i + 1].Number - _colourMappings[i].Number;
                var currentRelativeNumber = (currentNumber - _colourMappings[i].Number) / numDelta;

                var redDelta = _colourMappings[i + 1].RGBColour.R - _colourMappings[i].RGBColour.R;
                var greenDelta = _colourMappings[i + 1].RGBColour.G - _colourMappings[i].RGBColour.G;
                var blueDelta = _colourMappings[i + 1].RGBColour.B - _colourMappings[i].RGBColour.B;

                var newRed = (byte)Math.Round(_colourMappings[i].RGBColour.R + currentRelativeNumber * redDelta);
                var newGreen = (byte)Math.Round(_colourMappings[i].RGBColour.G + currentRelativeNumber * greenDelta);
                var newBlue = (byte)Math.Round(_colourMappings[i].RGBColour.B + currentRelativeNumber * blueDelta);

                var newColour = Color.FromArgb(newRed, newGreen, newBlue);

                return ColorTranslator.ToHtml(newColour);
            }

            // above max, return last colour value
            return _colourMappings[_colourMappings.Count - 1].HexColour;
        }

        private bool IsInRange(double currentNumber, int low, int high)
        {
            return currentNumber >= _colourMappings[low].Number && currentNumber < _colourMappings[high].Number;
        }

        public void AddToColourMappings(double num, string hexColour)
        {
            // create new mapping
            var newMapping = new ColourMapping(num, hexColour);

            // add to list and sort by number
            _colourMappings.Add(newMapping);
            _colourMappings = _colourMappings.OrderBy(m => m.Number).ToList();
        }

        public void RemoveFromColourMappings(double num)
        {
            _colourMappings.Remove(_colourMappings.Find(m => m.Number == num));
        }


    }
}
