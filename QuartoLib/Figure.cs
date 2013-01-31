using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartoLib
{
    public enum FigureSigns { 
        White   = 0,
        Small   = 1,
        Solid   = 2,
        Square  = 3,
        Black   = 4,
        Big     = 5,
        Hollow  = 6,
        Round   = 7
    }

    public class Figure
    {
        public const byte NO_FIGURE = 16;

        private byte _code;
        public byte Code
        {
            get { return _code; }
            set { _code = value; }
        }

        private bool _IsValid(int figure) {
            bool isValid = true;
            for(int i = 0; i < 4; i++)
                if (((figure >> i) & 1) == ((figure >> i + 4) & 1))
                    isValid = false;
            return isValid;
        }

        /// <summary>
        /// Creates a figure object.
        /// Throws exceptions if (i+4)-th and i-th bits are same
        /// </summary>
        /// <param name="figure">i-th bit shows if figure has i-th sign</param>
        public Figure(byte figure)
        {
            if (!_IsValid(figure))
                throw new ArgumentException("Figure is invalid.");
            Code = figure;
        }
    }
}
