using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartoLib
{
    public class CpuPlayer: IPlayer
    {

        /// <summary>
        /// Player color or name
        /// </summary>
        private PlayerName _name;
        public PlayerName Name {
            get { return _name; }
            private set { _name = value; }
        }

        /// <summary>
        ///  State given to the player
        /// </summary>
        private State _currentState;
        public State CurrentState {
            get { return _currentState; }
            private set { _currentState = value; }
        }

        /// <summary>
        /// Currently acceptin tie strategy
        /// </summary>
        private bool _acceptTie;

        /// <summary>
        /// Initialize Cpu player with current state and name.
        /// </summary>
        public CpuPlayer(State state, PlayerName name) {
            _acceptTie = false;
            CurrentState = state;
            Name = name;
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

        private bool _IsMyTurn(State s) { 
            int n = s.FiguresPlaced * 2 + ((s.FigureToPlace == Figure.NO_FIGURE)? 0: 1);
            if (Name == PlayerName.Red)
                if ((((n + 1) / 2) & 1) == 0)
                    return true;
            if (Name == PlayerName.Blue)
                if ((((n + 1) / 2) & 1) == 1)
                    return true;
            return false;
        }

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

        protected StateValue dfs_placeMove(ExtendedState s, int depth)
        {
            if (depth == 0)
                // consider temporary state as a tie
                return StateValue.TieStateValue;

            bool hasTieMove = false;
            StateValue longestLoseMove = new StateValue(-1,100);
            byte i, j;
            for (i = 0; i < 4; i++)
                for (j = 0; j < 4; j++)
                    if (s.GameField[i][j] == Figure.NO_FIGURE)
                    {
                        FigurePlaceMove tmove = new FigurePlaceMove(i, j); // current move
                        StateValue sValue = dfs_takeMove(new ExtendedState(s, tmove), depth - 1);
                        if (sValue.Price == 1)
                        {
                            return new StateValue(sValue.Price, (byte)(sValue.StepLen + 1));
                        }
                        else if (sValue.Price == 0)
                        {
                            hasTieMove = true;
                        }
                        else 
                        {
                            if (longestLoseMove.StepLen > sValue.StepLen)
                                longestLoseMove = sValue;
                        }
                    }
            if (!hasTieMove)
            {
                // no tie moves / winning moves, then lose
                return new StateValue(-1, (byte)(longestLoseMove.StepLen + 1));
            }
            else
            {
                // no winning moves, but there is a tie move
                return StateValue.TieStateValue;
            }
        }

        protected StateValue dfs_takeMove(ExtendedState s, int depth)
        {
            if (s.FiguresPlaced == 16)
                return StateValue.TieStateValue; // all figures placed

            if (_gameStates.ContainsKey(s.CodedState))
                return _gameStates[s.CodedState];

            if (_StateIsWinningBySayingQuarto(s))
            {
                StateValue stv = new StateValue(1, 0);
                _gameStates.Add(s.CodedState, stv);
                return stv;
            }

            if (depth == 0)
            {
                // consider temporary state as a tie
                return StateValue.TieStateValue;
            }

            StateValue longestLoseMove = new StateValue(-1,100);
            bool hasTieMove = false;
            byte f;
            for (f = 0; f < 16; f++)
                if (((s.Figures >> f) & 1) == 0)
                {
                    FigureTakeMove tmove = new FigureTakeMove(f); // current move
                    StateValue sValue = dfs_placeMove(new ExtendedState(s, tmove), depth - 1);
                    if (sValue.Price == -1)
                    {
                        StateValue winStateValue = new StateValue(1, (byte)(sValue.StepLen + 1));
                        _gameStates.Add(s.CodedState, winStateValue);
                        return winStateValue;
                    }
                    else if (sValue.Price == 0)
                    {
                        hasTieMove = true;
                    }
                    else { 
                        if (longestLoseMove.StepLen > sValue.StepLen)
                            longestLoseMove = sValue;
                    }
                }

            if (!hasTieMove)
            {
                // no tie moves / winning moves, then lose
                StateValue loseStateValue = new StateValue(-1, (byte)(longestLoseMove.StepLen + 1));
                _gameStates.Add(s.CodedState, loseStateValue);
                return loseStateValue;
            }
            else
            {
                // no winning moves, but there is a tie move
                _gameStates.Add(s.CodedState, StateValue.TieStateValue);
                return StateValue.TieStateValue;
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

            ExtendedState extendedCurrentState = new ExtendedState( CurrentState );
            int iterationDepth = _GetIterationDepth(CurrentState);
            Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);

            StateValue longestLoseMoveSV = new StateValue(-1, 0);
            StateValue shortestWinMoveSV = new StateValue(-1, 100);
            FigureTakeMove anyTieMove = null;
            FigureTakeMove longestLoseMove = null;
            FigureTakeMove shortestWinMove = null;

            int nTieMoves = 0;
            int nLongestMoves = 0;
            int nShortestWinMoves = 0;
            byte f;
            for (f = 0; f < 16; f++)
                if (((CurrentState.Figures >> f) & 1) == 0)
                {
                    
                    FigureTakeMove tmove = new FigureTakeMove(f); // temp possible move
                    StateValue sValue = dfs_placeMove(new ExtendedState( extendedCurrentState, tmove),iterationDepth);

                    if (sValue.Price == -1)
                    {
                        if (sValue.StepLen < shortestWinMoveSV.StepLen)
                        {
                            nShortestWinMoves = 1;
                            shortestWinMove = tmove;
                            shortestWinMoveSV = sValue;
                        }
                        else if (sValue.StepLen == shortestWinMoveSV.StepLen) {
                            nShortestWinMoves++;
                            if (r.Next(nShortestWinMoves) == nShortestWinMoves - 1) shortestWinMove = tmove;
                        }
                    }
                    else if (sValue.Price == 0)
                    {
                        nTieMoves++;
                        if (r.Next(nTieMoves) == nTieMoves - 1) anyTieMove = tmove;
                    }
                    else 
                    {
                        if (sValue.StepLen > longestLoseMoveSV.StepLen)
                        {
                            nLongestMoves = 1;
                            longestLoseMove = tmove;
                            longestLoseMoveSV = sValue;
                        }
                        else if (sValue.StepLen == longestLoseMoveSV.StepLen)
                        {
                            nLongestMoves++;
                            if (r.Next(nLongestMoves) == nLongestMoves - 1) longestLoseMove = tmove;
                        }
                    }
                }

            FigureTakeMove move;
            if (shortestWinMove != null) move = shortestWinMove; // randomly taken win move
            else if (anyTieMove != null) move = anyTieMove; // randomly taken tie move
            else move = longestLoseMove; // randomly taken move
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

            //_gameStates = new ExtendedDictionary();
            ExtendedState extendedCurrentState = new ExtendedState( CurrentState );
            int iterationDepth = _GetIterationDepth(CurrentState);
            Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);

            StateValue longestLoseMoveSV = new StateValue(-1, 0);
            StateValue shortestWinMoveSV = new StateValue(-1, 100);
            FigurePlaceMove anyTieMove = null;
            FigurePlaceMove longestLoseMove = null;
            FigurePlaceMove shortestWinMove = null;

            int nTieMoves = 0;
            int nLongestMoves = 0;
            int nShortestWinMoves = 0;

            byte i, j;
            for (i = 0; i < 4; i++)
                for (j = 0; j < 4; j++)
                    if (CurrentState.GameField[i][j] == Figure.NO_FIGURE)
                    {
                        FigurePlaceMove tmove = new FigurePlaceMove(i, j); // temp possible move
                        StateValue sValue = dfs_takeMove(new ExtendedState(extendedCurrentState, tmove), iterationDepth);

                        if (sValue.Price == 1)
                        {
                            if (sValue.StepLen < shortestWinMoveSV.StepLen)
                            {
                                nShortestWinMoves = 1;
                                shortestWinMove = tmove;
                                shortestWinMoveSV = sValue;
                            }
                            else if (sValue.StepLen == shortestWinMoveSV.StepLen)
                            {
                                nShortestWinMoves++;
                                if (r.Next(nShortestWinMoves) == nShortestWinMoves - 1) shortestWinMove = tmove;
                            }
                        }
                        else if (sValue.Price == 0)
                        {
                            nTieMoves++;
                            if (r.Next(nTieMoves) == nTieMoves - 1) anyTieMove = tmove;
                        }
                        else {
                            if (sValue.StepLen > longestLoseMoveSV.StepLen)
                            {
                                nLongestMoves = 1;
                                longestLoseMove = tmove;
                                longestLoseMoveSV = sValue;
                            }
                            else if (sValue.StepLen == longestLoseMoveSV.StepLen)
                            {
                                nLongestMoves++;
                                if (r.Next(nLongestMoves) == nLongestMoves - 1) longestLoseMove = tmove;
                            }
                        }
                    }

            FigurePlaceMove move;
            if (shortestWinMove != null) move = shortestWinMove; // randomly taken win move
            else if (anyTieMove != null) move = anyTieMove; // randomly taken tie move
            else move = longestLoseMove; // randomly taken move
            CurrentState = new State(CurrentState, move);
            FigurePlaceMoveMadeEvent(new MoveMadeEventArgs<FigurePlaceMove>(move));
        }
        public event MoveMadeEventHandler<FigurePlaceMove> FigurePlaceMoveMadeEvent;
        public void MakeTieAnswerMove()
        {
            if (_StateIsWinningBySayingQuarto(CurrentState) ||
                _gameStates.ContainsKey(new CodedState(CurrentState)) && _gameStates[new CodedState(CurrentState)].Price == 1)
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

        /// <summary>
        /// Array contains precalculated values for iterative deepening while 
        /// using dfs. This values should reflect suitable number of states and
        /// move taking time.
        /// </summary>
        private int[] _iterationDepth = new int[32] { 0, 0, 0, 0, 0, 0, 1, 6, 7, 7, 7, 8, 8, 9, 9, 12, 
            14, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16};
        private int _GetIterationDepth(State s) {
            int n = s.FiguresPlaced * 2 + ((s.FigureToPlace == Figure.NO_FIGURE)? 0: 1);
            return _iterationDepth[n];
        }
    }

    public class StateValue {
        public static StateValue TieStateValue = new StateValue(0, 0);

        private sbyte _price;
        public sbyte Price {
            get { return _price; }
            private set { _price = value; }
        }

        private byte _stepLen;
        public byte StepLen
        {
            get { return _stepLen; }
            private set { _stepLen = value; }
        }

        public StateValue(sbyte price, byte stepLen) {
            if (price == 0)
                if (stepLen != 0)
                    throw new ArgumentException("state value can have StepLen property set to 0 in case Price is 0");
            _stepLen = stepLen;
            _price = price;
        }
    }

    public class CodedState {
        public long CodedFigurePlaced;
        public short CodedCellsAreOccupied;
        public CodedState( State state ) {
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
        public CodedState(CodedState s) {
            CodedFigurePlaced = s.CodedFigurePlaced;
            CodedCellsAreOccupied = s.CodedCellsAreOccupied;
        }
    }

    public class ExtendedDictionary {
        private Dictionary<short, Dictionary<long, StateValue>> innerDictionary;
        public void Add(CodedState s, StateValue stv) {
            if (!innerDictionary.ContainsKey(s.CodedCellsAreOccupied))
                innerDictionary.Add(s.CodedCellsAreOccupied, new Dictionary<long, StateValue>());
            innerDictionary[s.CodedCellsAreOccupied].Add(s.CodedFigurePlaced, stv);
        }
        public bool ContainsKey(CodedState s)
        {
            if (innerDictionary.ContainsKey(s.CodedCellsAreOccupied))
                if (innerDictionary[s.CodedCellsAreOccupied].ContainsKey(s.CodedFigurePlaced))
                    return true;
            return false;
        }

        public StateValue this[CodedState s]
        {
            get {
                return innerDictionary[s.CodedCellsAreOccupied][s.CodedFigurePlaced];
            }
        }
        public ExtendedDictionary()
        {
            innerDictionary = new Dictionary<short, Dictionary<long, StateValue>>();
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
            if((CodedState.CodedCellsAreOccupied & (short)(1 << (15 - n))) != 0)
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
