using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartoLib
{
    public class Player: IPlayer
    {
        /// <summary>
        ///  State given to the player
        /// </summary>
        private State _currentState;
        public State CurrentState {
            get { return _currentState; }
            private set { _currentState = value; }
        }

        public Player(State state) {
            CurrentState = state;
        }

        public Move MakeMove() {
            // if player can win, then he wins
            Move winMove = _GetWinMove(CurrentState);
            if (winMove != null) return winMove;

            Move tieMove = null;
            Move firstMove = null;

            Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            // try all possible moves
            int nTieMoves = 0;
            int nMoves = 0;
            for (byte i = 0; i < 4; i++)
                for (byte j = 0; j < 4; j++)
                    if (CurrentState.GameField[i][j] == 16) { // place free
                        byte x = i;
                        byte y = j;
                        for (byte f = 0; f < 16; f++)
                            if (CurrentState.FigureToPlace != f
                                && ((CurrentState.Figures >> f) & 1) != 1) // figure free
                            {
                                nMoves++;
                                Move tmove = new Move(x, y, f); // temp possible move
                                if (r.Next(nMoves) == nMoves - 1) firstMove = tmove;
                                sbyte tprice = dfs(new State(CurrentState, tmove));
                                if (tprice == -1)
                                    return tmove; // winning move found
                                else if (tprice == 0) {
                                    nTieMoves++;
                                    if(r.Next(nTieMoves) == nTieMoves - 1) tieMove = tmove;
                                }
                            }
                    }
            if (tieMove != null) return tieMove; // randomly taken tie move
            return firstMove; // randomly taken lose move
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
        private Move _GetWinMove(State state)
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
    }
}
