using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartoLib.Cpu
{
    public class ExtendedState : State
    {
        private CodedState _codedState;
        public CodedState CodedState
        {
            get { return _codedState; }
            private set { _codedState = value; }
        }

        public ExtendedState()
            : base()
        {
            CodedState = new CodedState(this);
        }

        public ExtendedState(State s)
            : base(s)
        {
            CodedState = new CodedState(this);
        }

        public ExtendedState(ExtendedState s, FigurePlaceMove move)
            : base(s, move)
        {
            CodedState = new CodedState(s.CodedState);
            byte f = s.FigureToPlace;
            byte x = move.XFigurePlacedTo;
            byte y = move.YFigurePlacedTo;
            int n = x * 4 + y;
            // occupy the field cell
            if ((CodedState.CodedCellsAreOccupied & (short)(1 << (15 - n))) != 0)
                throw new Exception("Aha!");
            CodedState.CodedCellsAreOccupied |= (short)(1 << (15 - n));
            // put the figure on the cell
            CodedState.CodedFigurePlaced |= ((long)f << (15 - n) * 4);
        }

        public ExtendedState(ExtendedState s, FigureTakeMove move)
            : base(s, move)
        {
            CodedState = new CodedState(s.CodedState);
        }
    }
}
