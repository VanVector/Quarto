using System;

namespace QuartoLib.Cpu
{
    public class CpuPlayer : IPlayer
    {

        /// <summary>
        /// Player color or name
        /// </summary>
        private PlayerName _name;
        public PlayerName Name
        {
            get { return _name; }
            private set { _name = value; }
        }

        /// <summary>
        ///  State given to the player
        /// </summary>
        private State _currentState;
        public State CurrentState
        {
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
        public CpuPlayer(State state, PlayerName name)
        {
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
        /// Defines if current state assumes this cpuPlayer take move
        /// </summary>
        private bool _IsMyTurn(State s)
        {
            int n = s.FiguresPlaced * 2 + ((s.FigureToPlace == Figure.NO_FIGURE) ? 0 : 1);
            if (Name == PlayerName.Red)
                if ((((n + 1) / 2) & 1) == 0)
                    return true;
            if (Name == PlayerName.Blue)
                if ((((n + 1) / 2) & 1) == 1)
                    return true;
            return false;
        }

        private static bool _StateIsWinningBySayingQuarto(State s)
        {
            if (s.LastFigurePlaced == Figure.NO_FIGURE)
                return false;
            // find last figure placed
            int x = -1; int y = -1; int i; int j;
            bool found = false;
            for (i = 0; i < 4 && !found; i++)
                for (j = 0; j < 4 && !found; j++)
                    if (s.LastFigurePlaced == s.GameField[i][j])
                    {
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

        protected sbyte dfs_placeMove(ExtendedState s, int depth)
        {
            if (depth == 0)
                // consider temporary state as a tie
                return 0;

            bool hasTieMove = false;
            byte i, j;
            for (i = 0; i < 4; i++)
                for (j = 0; j < 4; j++)
                    if (s.GameField[i][j] == Figure.NO_FIGURE)
                    {
                        FigurePlaceMove tmove = new FigurePlaceMove(i, j); // current move
                        sbyte tprice = dfs_takeMove(new ExtendedState(s, tmove), depth - 1);
                        if (tprice == 1)
                        {
                            return 1;
                        }
                        else if (tprice == 0)
                        {
                            hasTieMove = true;
                        }
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

        protected sbyte dfs_takeMove(ExtendedState s, int depth)
        {
            if (_gameStates.ContainsKey(s.CodedState))
                return _gameStates[s.CodedState];

            if (_StateIsWinningBySayingQuarto(s))
            {
                _gameStates.Add(s.CodedState, 1);
                return 1;
            }

            if (s.FiguresPlaced == 16)
                return 0; // all figures placed

            if (depth == 0)
            {
                // consider temporary state as a tie
                return 0;
            }

            bool hasTieMove = false;
            byte f;
            for (f = 0; f < 16; f++)
                if (((s.Figures >> f) & 1) == 0)
                {
                    FigureTakeMove tmove = new FigureTakeMove(f); // current move
                    sbyte tprice = dfs_placeMove(new ExtendedState(s, tmove), depth - 1);
                    if (tprice == -1)
                    {
                        _gameStates.Add(s.CodedState, 1);
                        return 1;
                    }
                    else if (tprice == 0)
                    {
                        hasTieMove = true;
                    }
                }

            if (!hasTieMove)
            {
                // no tie moves / winning moves, then lose
                _gameStates.Add(s.CodedState, -1);
                return -1;
            }
            else
            {
                // no winning moves, but there is a tie move
                _gameStates.Add(s.CodedState, 0);
                return 0;
            }
        }

        public void MakeFigureTakeMove()
        {
            if (_StateIsWinningBySayingQuarto(CurrentState))
            {
                // say "Quarto"
                QuartoSayingMoveMadeEvent(new MoveMadeEventArgs<QuartoSayingMove>(new QuartoSayingMove()));
                return;
            }

            if (CurrentState.FiguresPlaced == 16)
            {
                // offer a tie
                TieOfferMoveMadeEvent(new MoveMadeEventArgs<TieOfferMove>(new TieOfferMove()));
                return;
            }

            ExtendedState extendedCurrentState = new ExtendedState(CurrentState);
            int iterationDepth = _GetIterationDepth(CurrentState);
            Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            FigureTakeMove anyTieMove = null;
            FigureTakeMove anyLoseMove = null;
            FigureTakeMove anyWinMove = null;
            int nTieMoves = 0;
            int nLoseMoves = 0;
            int nWinMoves = 0;
            byte f;
            for (f = 0; f < 16; f++)
                if (((CurrentState.Figures >> f) & 1) == 0)
                {

                    FigureTakeMove tmove = new FigureTakeMove(f); // temp possible move
                    sbyte tprice = dfs_placeMove(new ExtendedState(extendedCurrentState, tmove), iterationDepth);

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
                    else
                    {
                        nLoseMoves++;
                        if (anyLoseMove == null)
                            anyLoseMove = tmove;
                        else
                        {
                            bool tmoveIsStupid = _IsStupidTakeMove(tmove);
                            bool anyLoseMoveIsStupid = _IsStupidTakeMove(anyLoseMove);
                            if (tmoveIsStupid && anyLoseMoveIsStupid || !tmoveIsStupid && !anyLoseMoveIsStupid)
                                anyLoseMove = (r.Next(nLoseMoves) == nLoseMoves - 1) ? tmove : anyLoseMove;
                            else if (!tmoveIsStupid && anyLoseMoveIsStupid)
                                anyLoseMove = tmove;
                        }
                    }
                }
            FigureTakeMove move;
            if (anyWinMove != null) move = anyWinMove; // randomly taken win move
            else if (anyTieMove != null) move = anyTieMove; // randomly taken tie move
            else move = anyLoseMove; // randomly taken lose move
            CurrentState = new State(CurrentState, move);
            FigureTakeMoveMadeEvent(new MoveMadeEventArgs<FigureTakeMove>(move));
        }

        public bool _IsStupidTakeMove(FigureTakeMove ftm)
        {

            State s = new State(CurrentState, ftm);
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (s.GameField[i][j] == Figure.NO_FIGURE)
                    {
                        State ts = new State(s, new FigurePlaceMove((byte)i, (byte)j));
                        if (_StateIsWinningBySayingQuarto(ts))
                            return true; // this move is stupid, don't make such moves.
                    }
            return false;
        }

        public event MoveMadeEventHandler<FigureTakeMove> FigureTakeMoveMadeEvent;
        public void MakeFigurePlaceMove()
        {
            if (_StateIsWinningBySayingQuarto(CurrentState))
            {
                // say "Quarto"
                QuartoSayingMoveMadeEvent(new MoveMadeEventArgs<QuartoSayingMove>(new QuartoSayingMove()));
                return;
            }

            //_gameStates = new ExtendedDictionary();
            ExtendedState extendedCurrentState = new ExtendedState(CurrentState);
            int iterationDepth = _GetIterationDepth(CurrentState);
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

                        FigurePlaceMove tmove = new FigurePlaceMove(i, j); // temp possible move
                        sbyte tprice = dfs_takeMove(new ExtendedState(extendedCurrentState, tmove), iterationDepth);

                        nMoves++;
                        if (r.Next(nMoves) == nMoves - 1) anyMove = tmove;
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
            else move = anyMove; // randomly taken move
            CurrentState = new State(CurrentState, move);
            FigurePlaceMoveMadeEvent(new MoveMadeEventArgs<FigurePlaceMove>(move));
        }
        public event MoveMadeEventHandler<FigurePlaceMove> FigurePlaceMoveMadeEvent;
        public void MakeTieAnswerMove()
        {

            if (CurrentState.FigureToPlace == Figure.NO_FIGURE)
            {
                if (CurrentState.FiguresPlaced == 16 || dfs_takeMove(new ExtendedState(CurrentState), _GetIterationDepth(CurrentState)) == 1)
                {
                    // accept the tie
                    TieAnswerMoveMadeEvent(new MoveMadeEventArgs<TieAnswerMove>(new TieAnswerMove(TieAnswer.ACCEPT)));
                    return;
                }
            }
            if (CurrentState.FigureToPlace != Figure.NO_FIGURE)
            {
                if (dfs_placeMove(new ExtendedState(CurrentState), _GetIterationDepth(CurrentState)) == 1)
                {
                    // accept the tie
                    TieAnswerMoveMadeEvent(new MoveMadeEventArgs<TieAnswerMove>(new TieAnswerMove(TieAnswer.ACCEPT)));
                    return;
                }
            }

            TieAnswerMoveMadeEvent(new MoveMadeEventArgs<TieAnswerMove>(new TieAnswerMove(TieAnswer.DECLINE)));
        }
        public event MoveMadeEventHandler<TieAnswerMove> TieAnswerMoveMadeEvent;

        public event MoveMadeEventHandler<TieOfferMove> TieOfferMoveMadeEvent;
        public event MoveMadeEventHandler<SurrenderMove> SurrenderMoveMadeEvent;
        public event MoveMadeEventHandler<QuartoSayingMove> QuartoSayingMoveMadeEvent;

        public void InformAboutMove(Move opponentMove)
        {
            if (opponentMove is FigurePlaceMove)
            {
                CurrentState = new State(CurrentState, (FigurePlaceMove)opponentMove);
            }
            else if (opponentMove is FigureTakeMove)
            {
                CurrentState = new State(CurrentState, (FigureTakeMove)opponentMove);
            }
            else if (opponentMove is TieAnswerMove)
            {
                if (((TieAnswerMove)opponentMove).TieAnswer == TieAnswer.ACCEPT)
                    ;// nice to play with you;
                else
                    ;// waste of your time
            }
            else if (opponentMove is TieOfferMove)
            {
                // I will think about it
            }
            else if (opponentMove is SurrenderMove)
            {
                // haha I won!
            }
            else if (opponentMove is QuartoSayingMove)
            {
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
        private int[] _iterationDepth = new int[33] { 0, 0, 0, 0, 0, 0, 1, 5, 5, 5, 7, 8, 8, 8, 8, 12, 
            14, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16};
        private int _GetIterationDepth(State s)
        {
            int n = s.FiguresPlaced * 2 + ((s.FigureToPlace == Figure.NO_FIGURE) ? 0 : 1);
            return _iterationDepth[n];
        }
    }
}
