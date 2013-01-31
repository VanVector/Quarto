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
        }

        protected sbyte dfs(State state) {
            Move winMove = _GetWinMove(state);
            if (winMove != null)
                return 1;
            if (state.FiguresPlaced == 16)
                return 0; // all figures placed

            bool hasTieMove = false;
            for (byte i = 0; i < 4; i++)
                for (byte j = 0; j < 4; j++)
                    if (CurrentState.GameField[i][j] == 16) {
                        byte x = i;
                        byte y = j;
                        for (byte f = 0; f < 16; f++)
                            if (CurrentState.FigureToPlace != f
                                && ((CurrentState.Figures >> f) & 1) != 1)
                            {
                                Move tmove = new Move(x, y, f); // current move
                                sbyte tprice = dfs(new State(state, tmove));
                                if (tprice == -1)
                                    return 1;
                                else if (tprice == 0)
                                    hasTieMove = true;
                            }
                    }
            if (!hasTieMove)
            {
                // no tie moves / winning moves, then lose
                return -1;
            }
            else {
                // no winning moves, but there is tie move
                return 0;
            }
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

        protected sbyte dfs(State s, MovePhase movePhase)
        {
            if (s.FiguresPlaced == 16)
                return 0; // all figures placed

            CodedState codedState = new CodedState(s);
            if(_gameStates.ContainsKey( codedState ))
                return _gameStates[codedState];

            if (_StateIsWinningBySayingQuarto(s)) {
                _gameStates.Add(codedState, 1);
                return 1;
            }

            bool hasTieMove = false;
            if (movePhase == MovePhase.PLACE)
            {
                byte i, j;
                for(i = 0; i < 4; i++)
                    for(j = 0; j < 4; j++)
                        if (CurrentState.GameField[i][j] == Figure.NO_FIGURE) {
                            FigurePlaceMove tmove = new FigurePlaceMove(i, j); // current move
                            sbyte tprice = dfs(new State(s, tmove), MovePhase.TAKE);
                            if (tprice == 1)
                            {
                                _gameStates.Add(codedState, 1);
                                return 1;
                            }
                            else if (tprice == 0)
                                hasTieMove = true;
                        }
            }
            else 
            {
                byte f;
                for (f = 0; f < 16; f++)
                        if (((CurrentState.Figures >> f) & 1) != 0)
                        {
                            FigureTakeMove tmove = new FigureTakeMove(f); // current move
                            sbyte tprice = dfs(new State(s, tmove),MovePhase.PLACE);
                            if (tprice == -1)
                            {
                                _gameStates.Add(codedState, 1);
                                return 1;
                            }
                            else if (tprice == 0)
                                hasTieMove = true;
                        }
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

        void MakeFigureTakeMove() {
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

            Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            FigureTakeMove anyTieMove = null;
            FigureTakeMove anyMove = null;
            FigureTakeMove anyWinMove = null;
            int nTieMoves = 0;
            int nMoves = 0;
            int nWinMoves = 0;
            byte f;
            for (f = 0; f < 16; f++)
                if (((CurrentState.Figures >> f) & 1) != 0)
                {
                    nMoves++;
                    FigureTakeMove tmove = new FigureTakeMove(f); // temp possible move
                    if (r.Next(nMoves) == nMoves - 1) anyMove = tmove;
                    sbyte tprice = dfs(new State(CurrentState, tmove), MovePhase.PLACE);
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
            FigureTakeMoveMadeEvent(new MoveMadeEventArgs<FigureTakeMove>(move));
        }
        event MoveMadeEventHandler<FigureTakeMove> FigureTakeMoveMadeEvent;
        void MakeFigurePlaceMove() {
            if (_StateIsWinningBySayingQuarto(CurrentState))
            {
                // say "Quarto"
                QuartoSayingMoveMadeEvent(new MoveMadeEventArgs<QuartoSayingMove>(new QuartoSayingMove()));
                return;
            }
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
                        sbyte tprice = dfs(new State(CurrentState, tmove), MovePhase.TAKE);
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
            FigurePlaceMoveMadeEvent(new MoveMadeEventArgs<FigurePlaceMove>(move));
        }
        event MoveMadeEventHandler<FigurePlaceMove> FigurePlaceMoveMadeEvent;
        void MakeTieAnswerMove() {
            if (_StateIsWinningBySayingQuarto(CurrentState) || _gameStates[new CodedState( CurrentState)] == 1)
                // accept the tie
                TieAnswerMoveMadeEvent(new MoveMadeEventArgs<TieAnswerMove>(new TieAnswerMove(TieAnswer.ACCEPT)));
            else
                // decline the tie
                TieAnswerMoveMadeEvent(new MoveMadeEventArgs<TieAnswerMove>(new TieAnswerMove(TieAnswer.DECLINE)));
        }
        event MoveMadeEventHandler<TieAnswerMove> TieAnswerMoveMadeEvent;

        event MoveMadeEventHandler<TieOfferMove> TieOfferMoveMadeEvent;
        event MoveMadeEventHandler<SurrenderMove> SurrenderMoveMadeEvent;
        event MoveMadeEventHandler<QuartoSayingMove> QuartoSayingMoveMadeEvent;

        void InformAboutMove(Move opponentMove) {
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
        void Lose(byte line, byte sign, string message);
        void Win(byte line, byte sign, string message);
        void HaveATie(string message);

        private Dictionary<CodedState, sbyte> _gameStates;

        public class CodedState {
            public byte[][] CodedField;
            public byte FigureToPlace;
            public CodedState( State state ) {
                CodedField = new byte[4][];
                for (int i = 0; i < 4; i++)
                {
                    CodedField[i] = new byte[4];
                    for (int j = 0; j < 4; j++)
                        CodedField[i][j] = state.GameField[i][j];
                }
                FigureToPlace = state.FigureToPlace;
            }
        }
    }
}
