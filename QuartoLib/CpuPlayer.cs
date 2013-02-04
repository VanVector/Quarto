using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartoLib
{
    public class CpuPlayer: IPlayer
    {
        
        /// <summary>
        ///  State given to the player
        /// </summary>
        private State _currentState;
        public State CurrentState {
            get { return _currentState; }
            private set { _currentState = value; }
        }

        /// <summary>
        /// Initialize Cpu player with current state.
        /// </summary>
        public CpuPlayer(State state) {
            CurrentState = state;
            _gameStates = new ExtendedDictionary();
        }

        /// <summary>
        ///  checks if figure of the state fits k-th line 
        ///  that means do not reduce its winning abilities
        ///  to zero.
        /// </summary>
        /// <param name="k">indicates the number of the line.</param>
        /// <returns></returns>
        protected bool FigureFitsToLine(State state, byte k)
        {
            return ((state.WinableBySign[k] & State.FigureToSigns(state.FigureToPlace)) != 0);
        }

        /// <summary>
        /// fills up data whether current state is winning for
        /// current player.
        /// </summary>
        /// <returns></returns>
        /*
        private FigurePlaceMove _GetWinPlaceMove(State state)
        {
            Move winMove = null;
            // check rows
            for (byte i = 0; i < 4; i++)
            {
                byte placed = 0;
                byte freeCellColumn = 4;
                for (byte j = 0; j < 4; j++)
                    if (state.GameField[i][j] != 16)
                        placed++;
                    else
                        freeCellColumn = j;
                if (placed == 3 && FigureFitsToLine(state, i))
                {
                    winMove = new Move(i, freeCellColumn);
                }
            }
            // check cols
            if (winMove == null)
                for (byte j = 0; j < 4; j++)
                {
                    byte placed = 0;
                    byte freeCellRow = 4;
                    for (byte i = 0; i < 4; i++)
                        if (state.GameField[i][j] != 16)
                            placed++;
                        else
                            freeCellRow = i;
                    if (placed == 3 && FigureFitsToLine(state, (byte)(j + 4)))
                    {
                        winMove = new Move(freeCellRow, j);
                    }
                }
            // main diag
            if (winMove == null)
                for (byte i = 0, j = 0; i < 4; i++, j++)
                {
                    byte placed = 0;
                    if (placed == 3 && FigureFitsToLine(state, 8))
                    {
                        winMove = new Move(i, j);
                    }
                }
            // another diag
            if (winMove == null)
                for (byte i = 0, j = 3; i < 4; i++, j--)
                {
                    byte placed = 0;
                    if (placed == 3 && FigureFitsToLine(state, 9))
                    {
                        winMove = new Move(i, j);
                    }
                }

            return winMove;
        }
        */

        private static bool _StateIsWinningBySayingQuarto(State s) {
            if (s.LastFigurePlaced == Figure.NO_FIGURE)
                return false;
            // find last figure placed
            int x = -1; int y = -1; int i; int j;
            bool found = false;
            for(i = 0; i < 4 && !found; i++)
                for(j = 0; j < 4 && !found; j++)
                    if(s.LastFigurePlaced == s.GameField[i][j]) {
                        found = true;
                        x = i; 
                        y = j;
                        break;
                    }

            // check row
            bool hasFreePlace = false;
            for (j = 0; j < 4; j++)
                if (s.GameField[x][j] == Figure.NO_FIGURE)
                {
                    hasFreePlace = true; break;
                }
            if (!hasFreePlace && s.WinableBySign[x] != 0)
            {
                return true; // row is winning
            }
            // check column
            hasFreePlace = false;
            for (i = 0; i < 4; i++)
                if (s.GameField[i][y] == Figure.NO_FIGURE)
                {
                    hasFreePlace = true; break;
                }
            if (!hasFreePlace && s.WinableBySign[4 + y] != 0)
            {
                return true; // col is winning
            }

            //check diag
            if (x == y)
            {
                hasFreePlace = false;
                for (i = 0, j = 0; i < 4; ++i, ++j)
                    if (s.GameField[i][j] == Figure.NO_FIGURE)
                    {
                        hasFreePlace = true; break;
                    }
                if (!hasFreePlace && s.WinableBySign[8] != 0)
                {
                    return true; // main diag is winning
                }
            }
            else if (y + x == 3)
            {
                hasFreePlace = false;
                for (i = 0, j = 3; i < 4; ++i, --j)
                    if (s.GameField[i][j] == Figure.NO_FIGURE)
                    {
                        hasFreePlace = true; break;
                    }
                if (!hasFreePlace && s.WinableBySign[9] != 0)
                {
                    return true; // other diag is winning
                }
            }
            return false;
        }

        protected sbyte dfs_placeMove(State s) {
            bool hasTieMove = false;
            byte i, j;
            for (i = 0; i < 4; i++)
                for (j = 0; j < 4; j++)
                    if (s.GameField[i][j] == Figure.NO_FIGURE)
                    {
                        FigurePlaceMove tmove = new FigurePlaceMove(i, j); // current move
                        sbyte tprice = dfs_takeMove(new State(s, tmove));
                        if (tprice == 1)
                        {
                            return 1;
                        }
                        else if (tprice == 0)
                            hasTieMove = true;
                    }
            if (!hasTieMove)
            {
                // no tie moves / winning moves, then lose
                return -1;
            }
            else
            {
                // no winning moves, but there is a tie move
                return 0;
            }
        }

        protected sbyte dfs_takeMove(State s)
        {
            if (s.FiguresPlaced == 16)
                return 0; // all figures placed

            CodedState codedState = new CodedState(s);
            if (_gameStates.ContainsKey(codedState))
                return _gameStates[codedState];

            if (_StateIsWinningBySayingQuarto(s))
            {
                _gameStates.Add(codedState, 1);
                return 1;
            }

            bool hasTieMove = false;
            byte f;
            for (f = 0; f < 16; f++)
                if (((s.Figures >> f) & 1) == 0)
                {
                    FigureTakeMove tmove = new FigureTakeMove(f); // current move
                    sbyte tprice = dfs_placeMove(new State(s, tmove));
                    if (tprice == -1)
                    {
                        _gameStates.Add(codedState, 1);
                        return 1;
                    }
                    else if (tprice == 0)
                        hasTieMove = true;
                }

            if (!hasTieMove)
            {
                // no tie moves / winning moves, then lose
                _gameStates.Add(codedState, -1);
                return -1;
            }
            else
            {
                // no winning moves, but there is a tie move
                _gameStates.Add(codedState, 0);
                return 0;
            }
        }

        public void MakeFigureTakeMove() {
            if (_StateIsWinningBySayingQuarto(CurrentState))
            {
                // say "Quarto"
                QuartoSayingMoveMadeEvent(new MoveMadeEventArgs<QuartoSayingMove>(new QuartoSayingMove()));
                return;
            }

            if (CurrentState.FiguresPlaced == 16) {
                // offer a tie
                TieOfferMoveMadeEvent(new MoveMadeEventArgs<TieOfferMove>(new TieOfferMove()));
                return;
            }

            // possibly not significant moves
            if (CurrentState.FiguresPlaced < 3) { 
                
            }

            ExtendedState extendedCurrentState = new ExtendedState( CurrentState );
            Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            FigureTakeMove anyTieMove = null;
            FigureTakeMove anyMove = null;
            FigureTakeMove anyWinMove = null;
            int nTieMoves = 0;
            int nMoves = 0;
            int nWinMoves = 0;
            byte f;
            for (f = 0; f < 16; f++)
                if (((CurrentState.Figures >> f) & 1) == 0)
                {
                    nMoves++;
                    FigureTakeMove tmove = new FigureTakeMove(f); // temp possible move
                    if (r.Next(nMoves) == nMoves - 1) anyMove = tmove;
                    sbyte tprice = dfs_placeMove(new ExtendedState( extendedCurrentState, tmove));
                    if (tprice == -1)
                    {
                        nWinMoves++;
                        if (r.Next(nWinMoves) == nWinMoves - 1) anyWinMove = tmove;
                    }
                    else if (tprice == 0)
                    {
                        nTieMoves++;
                        if (r.Next(nTieMoves) == nTieMoves - 1) anyTieMove = tmove;
                    }
                }
            FigureTakeMove move;
            if (anyWinMove != null) move = anyWinMove; // randomly taken win move
            else if (anyTieMove != null) move = anyTieMove; // randomly taken tie move
            else move = anyMove; // randomly taken lose move
            CurrentState = new State(CurrentState, move);
            FigureTakeMoveMadeEvent(new MoveMadeEventArgs<FigureTakeMove>(move));
        }
        public event MoveMadeEventHandler<FigureTakeMove> FigureTakeMoveMadeEvent;
        public void MakeFigurePlaceMove() {
            if (_StateIsWinningBySayingQuarto(CurrentState))
            {
                // say "Quarto"
                QuartoSayingMoveMadeEvent(new MoveMadeEventArgs<QuartoSayingMove>(new QuartoSayingMove()));
                return;
            }

            // possibly not significant moves
            if (CurrentState.FiguresPlaced < 3)
            {

            }

            ExtendedState extendedCurrentState = new ExtendedState( CurrentState );
            Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            FigurePlaceMove anyTieMove = null;
            FigurePlaceMove anyMove = null;
            FigurePlaceMove anyWinMove = null;
            int nTieMoves = 0;
            int nMoves = 0;
            int nWinMoves = 0;
            byte i, j;
            for (i = 0; i < 4; i++)
                for (j = 0; j < 4; j++)
                    if (CurrentState.GameField[i][j] == Figure.NO_FIGURE)
                    {
                        nMoves++;
                        FigurePlaceMove tmove = new FigurePlaceMove(i, j); // temp possible move
                        if (r.Next(nMoves) == nMoves - 1) anyMove = tmove;
                        sbyte tprice = dfs_takeMove(new ExtendedState(extendedCurrentState, tmove));
                        if (tprice == 1)
                        {
                            nWinMoves++;
                            if (r.Next(nWinMoves) == nWinMoves - 1) anyWinMove = tmove;
                        }
                        else if (tprice == 0)
                        {
                            nTieMoves++;
                            if (r.Next(nTieMoves) == nTieMoves - 1) anyTieMove = tmove;
                        }
                    }

            FigurePlaceMove move;
            if (anyWinMove != null) move = anyWinMove; // randomly taken win move
            else if (anyTieMove != null) move = anyTieMove; // randomly taken tie move
            else move = anyMove; // randomly taken lose move
            CurrentState = new State(CurrentState, move);
            FigurePlaceMoveMadeEvent(new MoveMadeEventArgs<FigurePlaceMove>(move));
        }
        public event MoveMadeEventHandler<FigurePlaceMove> FigurePlaceMoveMadeEvent;
        public void MakeTieAnswerMove()
        {
            if (_StateIsWinningBySayingQuarto(CurrentState) || _gameStates[new CodedState( CurrentState)] == 1)
                // accept the tie
                TieAnswerMoveMadeEvent(new MoveMadeEventArgs<TieAnswerMove>(new TieAnswerMove(TieAnswer.ACCEPT)));
            else
                // decline the tie
                TieAnswerMoveMadeEvent(new MoveMadeEventArgs<TieAnswerMove>(new TieAnswerMove(TieAnswer.DECLINE)));
        }
        public event MoveMadeEventHandler<TieAnswerMove> TieAnswerMoveMadeEvent;

        public event MoveMadeEventHandler<TieOfferMove> TieOfferMoveMadeEvent;
        public event MoveMadeEventHandler<SurrenderMove> SurrenderMoveMadeEvent;
        public event MoveMadeEventHandler<QuartoSayingMove> QuartoSayingMoveMadeEvent;

        public void InformAboutMove(Move opponentMove)
        {
            if (opponentMove is FigurePlaceMove) {
                CurrentState = new State(CurrentState, (FigurePlaceMove)opponentMove);
            }
            else if (opponentMove is FigureTakeMove) {
                CurrentState = new State(CurrentState, (FigureTakeMove)opponentMove);
            }
            else if (opponentMove is TieAnswerMove) {
                if (((TieAnswerMove)opponentMove).TieAnswer == TieAnswer.ACCEPT)
                    ;// nice to play with you;
                else
                    ;// waste of your time
            }
            else if (opponentMove is TieOfferMove) {
                // I will think about it
            }
            else if (opponentMove is SurrenderMove) { 
                // haha I won!
            }
            else if (opponentMove is QuartoSayingMove) { 
                // lets see who is the winner
            }
        }
        public void Lose(byte line, byte sign, string message) { }
        public void Win(byte line, byte sign, string message) { }
        public void HaveATie(string message) { }

        private ExtendedDictionary _gameStates;
    }

    public class CodedState {
        public long CodedFigurePlaced;
        public short CodedCellsAreOccupied;
        public CodedState( State state ) {
            CodedFigurePlaced = CodedCellsAreOccupied = 0;
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if(state.GameField[i][j] == Figure.NO_FIGURE) {
                        CodedFigurePlaced <<= 4;
                        CodedCellsAreOccupied <<= 1;
                    } else {
                        CodedCellsAreOccupied |= 1;
                        CodedCellsAreOccupied <<= 1;
                        CodedFigurePlaced |= (state.GameField[i][j] & 0x0F);
                        CodedFigurePlaced <<= 4;
                    }
        }
        public CodedState(CodedState s) {
            CodedFigurePlaced = s.CodedFigurePlaced;
            CodedCellsAreOccupied = s.CodedCellsAreOccupied;
        }
    }

    public class ExtendedDictionary {
        private Dictionary<short, Dictionary<long, sbyte>> innerDictionary;
        public void Add(CodedState s, sbyte value) {
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
            get {
                return innerDictionary[s.CodedCellsAreOccupied][s.CodedFigurePlaced];
            }
        }
        public ExtendedDictionary()
        {
            innerDictionary = new Dictionary<short, Dictionary<long, sbyte>>();
        }
    }

    public class ExtendedState: State {
        private CodedState _codedState;
        public CodedState CodedState {
            get { return _codedState; }
            private set { _codedState = value; }
        }

        public ExtendedState() : base() {
            CodedState = new CodedState(this);
        }

        public ExtendedState(State s) : base(s) {
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
            CodedState.CodedCellsAreOccupied |= (short)(1 << n);
            // put the figure on the cell
            CodedState.CodedFigurePlaced |= ((long)f << n);
        }

        public ExtendedState(ExtendedState s, FigureTakeMove move)
            : base(s, move)
        {
            CodedState = new CodedState(s.CodedState);
        }
    }
}
