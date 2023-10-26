using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CommonWPF
{
    internal class ColourMapping
    {
        public double Number { get; internal set; }

        public string HexColour { get; internal set; }

        internal Color RGBColour { get; }

        public ColourMapping(double num, string hexColour)
        {
            Number = num;
            HexColour = hexColour;
            RGBColour = ColorTranslator.FromHtml(HexColour);
        }


    }
}
