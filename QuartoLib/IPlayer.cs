using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartoLib
{
    public interface IPlayer
    {
        void MakeFigureTakeMove();
        event MoveMadeEventHandler<FigureTakeMove> FigureTakeMoveMadeEvent;
        void MakeFigurePlaceMove();
        event MoveMadeEventHandler<FigurePlaceMove> FigurePlaceMoveMadeEvent;
        void MakeTieAnswerMove();
        event MoveMadeEventHandler<TieAnswerMove> TieAnswerMoveMadeEvent;

        event MoveMadeEventHandler<TieOfferMove> TieOfferMoveMadeEvent;
        event MoveMadeEventHandler<SurrenderMove> SurrenderMoveMadeEvent;
        event MoveMadeEventHandler<QuartoSayingMove> QuartoSayingMoveMadeEvent;

        void InformAboutMove(Move opponentMove);
        void Lose( byte line, byte sign, string message);
        void Win(byte line, byte sign, string message);
        void HaveATie(string message);
    }
}
