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
    public abstract class Move {
        
    }

    public class FigurePlaceMove : Move
    {
        public byte XFigurePlacedTo;
        public byte YFigurePlacedTo;
        
        public FigurePlaceMove(byte x, byte y)
        {
            XFigurePlacedTo = x;
            YFigurePlacedTo = y;
        }
        public FigurePlaceMove()
        {
            XFigurePlacedTo = 0;
            YFigurePlacedTo = 0;
        }
    }

    public class FigureTakeMove : Move {
        public byte FigureGivenToOpponent;
        public FigureTakeMove(byte figure)
        {
            FigureGivenToOpponent = figure;
        }
        public FigureTakeMove()
        {
            FigureGivenToOpponent = 0;
        }
    }

    public class QuartoSayingMove : Move { 
        
    }

    public class TieOfferMove : Move
    {

    }

    public enum TieAnswer { 
        ACCEPT = 0,
        DECLINE = 1
    }

    public class TieAnswerMove : Move
    {
        private TieAnswer _tieAnswer;
        public TieAnswer TieAnswer {
            get { return _tieAnswer; }
            private set { _tieAnswer = value; }
        }

        public TieAnswerMove(TieAnswer tieAnswer)
        {
            TieAnswer = tieAnswer;
        }
    }

    public class SurrenderMove : Move
    {
    }

    public class MoveMadeEventArgs<TMove> : EventArgs where TMove : Move 
    {
        protected TMove _madeMove;
        public virtual TMove MadeMove
        {
            get { return _madeMove; }
            protected set { _madeMove = value; }
        }
        public MoveMadeEventArgs(TMove move)
        {
            MadeMove = move;
        }
    }

    public delegate void MoveMadeEventHandler<TMove>(MoveMadeEventArgs<TMove> args) where TMove: Move;
}
