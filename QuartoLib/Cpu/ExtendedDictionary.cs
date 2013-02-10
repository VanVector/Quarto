using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartoLib.Cpu
{
    public class ExtendedDictionary
    {
        private Dictionary<short, Dictionary<long, sbyte>> innerDictionary;
        public void Add(CodedState s, sbyte value)
        {
            if (!innerDictionary.ContainsKey(s.CodedCellsAreOccupied))
                innerDictionary.Add(s.CodedCellsAreOccupied, new Dictionary<long, sbyte>());
            innerDictionary[s.CodedCellsAreOccupied].Add(s.CodedFigurePlaced, value);
        }
        public bool ContainsKey(CodedState s)
        {
            if (innerDictionary.ContainsKey(s.CodedCellsAreOccupied))
                if (innerDictionary[s.CodedCellsAreOccupied].ContainsKey(s.CodedFigurePlaced))
                    return true;
            return false;
        }

        public sbyte this[CodedState s]
        {
            get
            {
                return innerDictionary[s.CodedCellsAreOccupied][s.CodedFigurePlaced];
            }
        }
        public ExtendedDictionary()
        {
            innerDictionary = new Dictionary<short, Dictionary<long, sbyte>>();
        }
    }

}
