using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartoLib
{
    /// <summary>
    /// A serie of moves the player does
    /// during his turn.
    /// </summary>
    public class Move
    {
        public byte XFigurePlacedTo;
        public byte YFigurePlacedTo;
        public byte FigureGivenToOpponent;
        public Move(byte x, byte y, byte figure)
        {
            XFigurePlacedTo = x;
            XFigurePlacedTo = y;
            FigureGivenToOpponent = figure;
        }
        public Move()
        {
            XFigurePlacedTo = 0;
            XFigurePlacedTo = 0;
            FigureGivenToOpponent = 0;
        }
        public Move(byte x, byte y)
        {
            XFigurePlacedTo = x;
            XFigurePlacedTo = y;
        }
    }
}
