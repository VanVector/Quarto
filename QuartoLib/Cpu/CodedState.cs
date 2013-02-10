using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartoLib.Cpu
{
    public class CodedState
    {
        public long CodedFigurePlaced;
        public short CodedCellsAreOccupied;
        public CodedState(State state)
        {
            CodedFigurePlaced = 0;
            CodedCellsAreOccupied = 0;
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    CodedFigurePlaced <<= 4;
                    CodedCellsAreOccupied <<= 1;
                    if (state.GameField[i][j] != Figure.NO_FIGURE)
                    {
                        CodedCellsAreOccupied |= 1;
                        CodedFigurePlaced |= state.GameField[i][j];
                    }
                }
        }
        public CodedState(CodedState s)
        {
            CodedFigurePlaced = s.CodedFigurePlaced;
            CodedCellsAreOccupied = s.CodedCellsAreOccupied;
        }
    }
}
