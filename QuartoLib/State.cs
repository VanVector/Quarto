﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartoLib
{
    public enum Signs { 
        White,
        Solid,
        Round,
        Big,
        Black,
        Hollow,
        Square,
        Small
    };

    public class State
    {
        /// <summary>
        /// Game field shows if [i][j]-th
        /// position is occupied by a figure GameField[i][j].
        /// If cell is free its value equals -1.
        /// </summary>
        private byte[][] _gameField;
        public byte[][] GameField
        {
            get { return _gameField; }
            private set { _gameField = value; }
        }

        /// <summary>
        /// WinableBySign shows if the line can be completed
        /// with the n-th sign, the n-th bit of each WinableBySign[i]
        /// contains this information.
        /// </summary>
        private byte[] _winableBySign;
        public byte[] WinableBySign
        {
            get { return _winableBySign; }
            private set { _winableBySign = value; }
        }

        /// <summary>
        /// Shows whether i-th figure is already used,
        /// where i is a number of a bit.
        /// i can take values from 0 to 15.
        /// </summary>
        private short _figures;
        public short Figures
        {
            get { return _figures; }
            private set { _figures = value; }
        }

        /// <summary>
        /// Defines what figure player has to place.
        /// </summary>
        private byte _figureToPlace;
        public byte FigureToPlace
        {
            get { return _figureToPlace; }
            private set { _figureToPlace = value; }
        }

        /// <summary>
        /// Number of figures placed.
        /// </summary>
        private byte _figuresPlaced;
        public byte FiguresPlaced {
            get { return _figuresPlaced; }
            private set { _figuresPlaced = value; } 
        }

        /// <summary>
        /// Code of the last figure placed.
        /// </summary>
        private byte _lastFigurePlaced;
        public byte LastFigurePlaced
        {
            get { return _lastFigurePlaced; }
            private set { _lastFigurePlaced = value; }
        }

        /// <summary>
        /// Game start state constructor.
        /// </summary>
        public State() { 
            GameField = new byte[4][];
            for (byte i = 0; i < 4; i++)
            {
                GameField[i] = new byte[4];
                for (byte j = 0; j < 4; j++)
                    GameField[i][j] = Figure.NO_FIGURE;
            }
            Figures = 0;
            WinableBySign = new byte[10];
            for (byte i = 0; i < 10; i++) WinableBySign[i] = 255;
            FigureToPlace = Figure.NO_FIGURE;
            FiguresPlaced = 0;
            LastFigurePlaced = Figure.NO_FIGURE;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public State(State state)
        {
            GameField = new byte[4][];
            for (int i = 0; i < 4; i++)
            {
                GameField[i] = new byte[4];
                state.GameField[i].CopyTo(GameField[i], 0);
            }
            WinableBySign = new byte[10];
            state.WinableBySign.CopyTo(WinableBySign,0);
            Figures = state.Figures;
            FigureToPlace = state.FigureToPlace;
            FiguresPlaced = state.FiguresPlaced;
            LastFigurePlaced = state.LastFigurePlaced;
        }

        /// <summary>
        /// New game state constructor.
        /// State is constructed from previous state and
        /// figure place move that was made.
        /// </summary>
        /// <param name="state">Previous state.</param>
        /// <param name="move">Figure place move.</param>
        public State(State state, FigurePlaceMove move)
            : this(state)
        {
            byte i = move.XFigurePlacedTo;
            byte j = move.YFigurePlacedTo;

            if (i < 0 || i > 4 || j < 0 || j > 4)
                throw new ArgumentException("i,j parameters are incorrect.");
            if (state.GameField[i][j] != Figure.NO_FIGURE)
                throw new ArgumentException(string.Format("GameField[{0}][{1}] is already occupied.", i, j));

            GameField[i][j] = state.FigureToPlace;
            byte figSigns = FigureToSigns(state.FigureToPlace);
            WinableBySign[i] &= figSigns;
            WinableBySign[j + 4] &= figSigns;
            if (i == j)
                WinableBySign[8] &= figSigns;
            if (i + j == 3)
                WinableBySign[9] &= figSigns;

            Figures += (short)(1 << state.FigureToPlace);

            FigureToPlace = Figure.NO_FIGURE;
            FiguresPlaced = (byte)(state.FiguresPlaced + 1);
            LastFigurePlaced = state.FigureToPlace;
        }

        /// <summary>
        /// New game state constructor.
        /// State is constructed from previous state and
        /// figure take move that was made.
        /// </summary>
        /// <param name="state">Previous state.</param>
        /// <param name="move">Figure take move.</param>
        public State(State state, FigureTakeMove move)
            : this(state)
        {
            byte figureToPlace = move.FigureGivenToOpponent;

            if (((state.Figures >> figureToPlace) & 1) == 1)
                throw new ArgumentException("Figure to place is already used.");

            FigureToPlace = figureToPlace;
        }

        /*
        /// <summary>
        /// New state created from gameField matrix, figure to place and last figure
        /// placed.
        /// </summary>
        /// <param name="gameField">4x4 gamefield matrix with figure codes.</param>
        /// <param name="figureToPlace">Code of the figure to place.</param>
        /// <param name="lastFigurePlaced">Code of the last figure placed.</param>
        public State(byte[][] gameField, byte figureToPlace, byte lastFigurePlaced)
        {
            if (gameField.Length != 4)
                throw new ArgumentException("GameField length is incorrect.");
            for (byte i = 0; i < 4; i++)
                if (gameField[i].Length != 4)
                    throw new ArgumentException(string.Format("GameField[{0}] length is incorrect.", i));
            byte fplaced = 0;
            for (byte i = 0; i < 4; i++)
                for (byte j = 0; j < 4; j++)
                {
                    byte tfigure = gameField[i][j];
                    if (tfigure >= Figure.NO_FIGURE)
                        throw new ArgumentException(string.Format("Incorrect figure on [{0}][{1}]-th place.", i, j));
                    if (tfigure != Figure.NO_FIGURE)
                    {
                        if (((Figures >> tfigure) & 1) == 1)
                            throw new ArgumentException(string.Format("Figure {2} on [{0}][{1}]-th place is already used.", i, j, tfigure));
                        Figures += (short)(1 << tfigure);
                        fplaced++;
                    }
                }
            if (((Figures >> figureToPlace) & 1) == 1)
                throw new ArgumentException(string.Format("Figure to place {0} is already used.", figureToPlace));

            FiguresPlaced = fplaced;
            FigureToPlace = figureToPlace;
            GameField = gameField;
            for (byte i = 0; i < 10; i++) WinableBySign[i] = 255;
            FillWinableBySign();
            LastFigurePlaced = lastFigurePlaced;
        }
        */

        /// <summary>
        /// Sets WinableBySign array.
        /// </summary>
        protected void FillWinableBySign() {
           for(byte i = 0; i < 4; i++)
               for (byte j = 0; j < 4; j++)
               {
                   byte figSigns = FigureToSigns(GameField[i][j]);
                   if (figSigns == Figure.NO_FIGURE)
                       continue;
                   WinableBySign[j + 4] &= figSigns;
                   WinableBySign[i] &= figSigns;
                   if(i - j == 0)
                       WinableBySign[8] &= figSigns;
                   if(i + j == 3)
                       WinableBySign[9] &= figSigns;
               }
        }

        /// <summary>
        /// Tranforms 4-bit figure representation
        /// to 8-bit representation, showing whether figure
        /// has the i-th sign, where i is the number of the bit.
        /// </summary>
        /// <param name="figure">4-bit figure representation.</param>
        /// <returns></returns>
        public static byte FigureToSigns(byte figure)
        {
            byte res = 0;
            if ((figure & 1) == 0) res += (1 << 0); else res += (1 << 4);
            if ((figure & 2) == 0) res += (1 << 1); else res += (1 << 5);
            if ((figure & 4) == 0) res += (1 << 2); else res += (1 << 6);
            if ((figure & 8) == 0) res += (1 << 3); else res += (1 << 7);
            return res;
        }
    }
}
