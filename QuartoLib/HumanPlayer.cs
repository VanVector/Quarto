using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartoLib
{
    public class HumanPlayer : IPlayer
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

        public HumanPlayer(PlayerName playerName)
        {
            Name = playerName;
        }

        public delegate void OrderedToMakeFigureTakeMoveEventHandler(HumanPlayer player);
        public event OrderedToMakeFigureTakeMoveEventHandler OrderedToMakeFigureTakeMoveEvent;
        public void MakeFigureTakeMove()
        {
            if (OrderedToMakeFigureTakeMoveEvent != null)
                OrderedToMakeFigureTakeMoveEvent(this);
        }

        public delegate void OrderedToMakeFigurePlaceMoveEventHandler(HumanPlayer player);
        public event OrderedToMakeFigurePlaceMoveEventHandler OrderedToMakeFigurePlaceMoveEvent;
        public void MakeFigurePlaceMove()
        {
            if (OrderedToMakeFigurePlaceMoveEvent != null)
                OrderedToMakeFigurePlaceMoveEvent(this);
        }

        public delegate void OrderedToMakeTieAnswerMoveEventHandler(HumanPlayer player);
        public event OrderedToMakeTieAnswerMoveEventHandler OrderedToMakeTieAnswerMoveEvent;
        public void MakeTieAnswerMove()
        {
            if (OrderedToMakeTieAnswerMoveEvent != null)
                OrderedToMakeTieAnswerMoveEvent(this);
        }

        public event MoveMadeEventHandler<FigureTakeMove> FigureTakeMoveMadeEvent;
        public event MoveMadeEventHandler<FigurePlaceMove> FigurePlaceMoveMadeEvent;
        public event MoveMadeEventHandler<TieAnswerMove> TieAnswerMoveMadeEvent;
        public event MoveMadeEventHandler<TieOfferMove> TieOfferMoveMadeEvent;
        public event MoveMadeEventHandler<SurrenderMove> SurrenderMoveMadeEvent;
        public event MoveMadeEventHandler<QuartoSayingMove> QuartoSayingMoveMadeEvent;

        public void FigureTakeMoveMade(byte Figure)
        {
            if (FigureTakeMoveMadeEvent != null)
                FigureTakeMoveMadeEvent(new MoveMadeEventArgs<FigureTakeMove>(new FigureTakeMove(Figure)));
        }
        public void FigurePlaceMoveMade(byte x, byte y)
        {
            if (FigurePlaceMoveMadeEvent != null)
                FigurePlaceMoveMadeEvent(new MoveMadeEventArgs<FigurePlaceMove>(new FigurePlaceMove(x, y)));
        }
        public void TieAnswerMoveMade(TieAnswer tieAnswer)
        {
            if (TieAnswerMoveMadeEvent != null)
                TieAnswerMoveMadeEvent(new MoveMadeEventArgs<TieAnswerMove>(new TieAnswerMove(tieAnswer)));
        }
        public void TieOfferMoveMade()
        {
            if (TieOfferMoveMadeEvent != null)
                TieOfferMoveMadeEvent(new MoveMadeEventArgs<TieOfferMove>(new TieOfferMove()));
        }
        public void SurrenderMoveMade()
        {
            if (SurrenderMoveMadeEvent != null)
                SurrenderMoveMadeEvent(new MoveMadeEventArgs<SurrenderMove>(new SurrenderMove()));
        }
        public void QuartoSayingMoveMade()
        {
            if (QuartoSayingMoveMadeEvent != null)
                QuartoSayingMoveMadeEvent(new MoveMadeEventArgs<QuartoSayingMove>(new QuartoSayingMove()));
        }

        public event MoveMadeEventHandler<FigureTakeMove> OpponentFigureTakeMoveMadeEvent;
        public event MoveMadeEventHandler<FigurePlaceMove> OpponentFigurePlaceMoveMadeEvent;
        public event MoveMadeEventHandler<TieAnswerMove> OpponentTieAnswerMoveMadeEvent;
        public event MoveMadeEventHandler<TieOfferMove> OpponentTieOfferMoveMadeEvent;
        public event MoveMadeEventHandler<SurrenderMove> OpponentSurrenderMoveMadeEvent;
        public event MoveMadeEventHandler<QuartoSayingMove> OpponentQuartoSayingMoveMadeEvent;
        public void InformAboutMove(Move opponentMove)
        {
            if (opponentMove is FigureTakeMove)
            {
                if (OpponentFigureTakeMoveMadeEvent != null)
                    OpponentFigureTakeMoveMadeEvent(new MoveMadeEventArgs<FigureTakeMove>(opponentMove as FigureTakeMove));
            }
            else if (opponentMove is FigurePlaceMove)
            {
                if (OpponentFigurePlaceMoveMadeEvent != null)
                    OpponentFigurePlaceMoveMadeEvent(new MoveMadeEventArgs<FigurePlaceMove>(opponentMove as FigurePlaceMove));
            }
            else if (opponentMove is TieAnswerMove)
            {
                if (OpponentTieAnswerMoveMadeEvent != null)
                    OpponentTieAnswerMoveMadeEvent(new MoveMadeEventArgs<TieAnswerMove>(opponentMove as TieAnswerMove));
            }
            else if (opponentMove is TieOfferMove)
            {
                if (OpponentTieOfferMoveMadeEvent != null)
                    OpponentTieOfferMoveMadeEvent(new MoveMadeEventArgs<TieOfferMove>(opponentMove as TieOfferMove));
            }
            else if (opponentMove is SurrenderMove)
            {
                if (OpponentSurrenderMoveMadeEvent != null)
                    OpponentSurrenderMoveMadeEvent(new MoveMadeEventArgs<SurrenderMove>(opponentMove as SurrenderMove));
            }
            else if (opponentMove is QuartoSayingMove)
            {
                if (OpponentQuartoSayingMoveMadeEvent != null)
                    OpponentQuartoSayingMoveMadeEvent(new MoveMadeEventArgs<QuartoSayingMove>((QuartoSayingMove)opponentMove));
            }
        }

        public delegate void GameOverEventHandler(byte line, byte sign, string message);
        public event GameOverEventHandler WinEvent;
        public event GameOverEventHandler LoseEvent;
        public event GameOverEventHandler TieEvent;
        public void Lose(byte line, byte sign, string message)
        {
            if(WinEvent != null)
                WinEvent(line, sign, message);
        }
        public void Win(byte line, byte sign, string message)
        {
            if (LoseEvent != null)
                LoseEvent(line, sign, message);
        }
        public void HaveATie(string message)
        {
            if (TieEvent != null)
                TieEvent(QuartoLib.Figure.NO_FIGURE, QuartoLib.Figure.NO_FIGURE, message);
        }
    }
}
