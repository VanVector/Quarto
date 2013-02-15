using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using QuartoLib;

namespace QuartoLibTests
{
    [TestFixture]
    public class TestCpuPlayer
    {
        

        [Test]
        public void TestMakeFigureTakeMoveNotToBeStupid()
        {
            State s = new State();
            s = new State(s, new FigureTakeMove(5));
            s = new State(s, new FigurePlaceMove(1,1));

            s = new State(s, new FigureTakeMove(15));
            s = new State(s, new FigurePlaceMove(2, 2));

            s = new State(s, new FigureTakeMove(14));
            s = new State(s, new FigurePlaceMove(2, 3));

            s = new State(s, new FigureTakeMove(8));
            s = new State(s, new FigurePlaceMove(1, 3));

            s = new State(s, new FigureTakeMove(9));
            s = new State(s, new FigurePlaceMove(3, 3));

            s = new State(s, new FigureTakeMove(2));
            s = new State(s, new FigurePlaceMove(2, 0));

            s = new State(s, new FigureTakeMove(0));
            s = new State(s, new FigurePlaceMove(0, 1));

            CpuPlayer cpuPlayer = new CpuPlayer(s, PlayerName.Blue);
            cpuPlayer.FigureTakeMoveMadeEvent += new MoveMadeEventHandler<FigureTakeMove>(move => Assert.AreEqual(4,move.MadeMove.FigureGivenToOpponent));
            cpuPlayer.MakeFigureTakeMove();
        }

        [Test]
        public void TestMakeTieAnswerDeclineMove()
        {
            State s = new State();
            s = new State(s, new FigureTakeMove(5));
            s = new State(s, new FigurePlaceMove(1, 1));

            s = new State(s, new FigureTakeMove(15));
            s = new State(s, new FigurePlaceMove(2, 2));

            s = new State(s, new FigureTakeMove(14));
            s = new State(s, new FigurePlaceMove(2, 3));

            s = new State(s, new FigureTakeMove(8));
            s = new State(s, new FigurePlaceMove(1, 3));

            s = new State(s, new FigureTakeMove(9));
            s = new State(s, new FigurePlaceMove(3, 3));

            s = new State(s, new FigureTakeMove(2));
            s = new State(s, new FigurePlaceMove(2, 0));

            s = new State(s, new FigureTakeMove(0));
            s = new State(s, new FigurePlaceMove(0, 1));

            CpuPlayer cpuPlayer = new CpuPlayer(s, PlayerName.Red);
            cpuPlayer.TieAnswerMoveMadeEvent += new MoveMadeEventHandler<TieAnswerMove>(move => Assert.AreEqual(TieAnswer.DECLINE, move.MadeMove.TieAnswer));
            cpuPlayer.MakeTieAnswerMove();
        }

        [Test]
        public void TestMakeTieAnswerAcceptMove()
        {
            State s = new State();
            s = new State(s, new FigureTakeMove(5));
            s = new State(s, new FigurePlaceMove(1, 1));

            s = new State(s, new FigureTakeMove(15));
            s = new State(s, new FigurePlaceMove(2, 2));

            s = new State(s, new FigureTakeMove(14));
            s = new State(s, new FigurePlaceMove(2, 3));

            s = new State(s, new FigureTakeMove(8));
            s = new State(s, new FigurePlaceMove(1, 3));

            s = new State(s, new FigureTakeMove(9));
            s = new State(s, new FigurePlaceMove(3, 3));

            s = new State(s, new FigureTakeMove(2));
            s = new State(s, new FigurePlaceMove(2, 0));

            s = new State(s, new FigureTakeMove(0));
            s = new State(s, new FigurePlaceMove(0, 1));

            s = new State(s, new FigureTakeMove(4));

            CpuPlayer cpuPlayer = new CpuPlayer(s, PlayerName.Blue);
            cpuPlayer.TieAnswerMoveMadeEvent += new MoveMadeEventHandler<TieAnswerMove>(move => Assert.AreEqual(TieAnswer.ACCEPT, move.MadeMove.TieAnswer));
            cpuPlayer.MakeTieAnswerMove();
        }

        [Test]
        public void TestMakeTieAnswerAfterAllFiguresArePlaced1() {
            /// Lose state
            State s = new State();
            s = new State(s, new FigureTakeMove(0));
            s = new State(s, new FigurePlaceMove(0, 0));

            s = new State(s, new FigureTakeMove(1));
            s = new State(s, new FigurePlaceMove(0, 1));

            s = new State(s, new FigureTakeMove(2));
            s = new State(s, new FigurePlaceMove(0, 2));

            s = new State(s, new FigureTakeMove(3));
            s = new State(s, new FigurePlaceMove(0, 3));

            s = new State(s, new FigureTakeMove(4));
            s = new State(s, new FigurePlaceMove(1, 0));

            s = new State(s, new FigureTakeMove(5));
            s = new State(s, new FigurePlaceMove(1, 1));

            s = new State(s, new FigureTakeMove(6));
            s = new State(s, new FigurePlaceMove(1, 2));

            s = new State(s, new FigureTakeMove(7));
            s = new State(s, new FigurePlaceMove(1, 3));

            s = new State(s, new FigureTakeMove(8));
            s = new State(s, new FigurePlaceMove(2, 0));

            s = new State(s, new FigureTakeMove(9));
            s = new State(s, new FigurePlaceMove(2, 1));

            s = new State(s, new FigureTakeMove(10));
            s = new State(s, new FigurePlaceMove(2, 2));

            s = new State(s, new FigureTakeMove(11));
            s = new State(s, new FigurePlaceMove(2, 3));

            s = new State(s, new FigureTakeMove(12));
            s = new State(s, new FigurePlaceMove(3, 0));

            s = new State(s, new FigureTakeMove(13));
            s = new State(s, new FigurePlaceMove(3, 1));

            s = new State(s, new FigureTakeMove(14));
            s = new State(s, new FigurePlaceMove(3, 2));

            s = new State(s, new FigureTakeMove(15));
            s = new State(s, new FigurePlaceMove(3, 3));

            CpuPlayer cpuPlayer = new CpuPlayer(s, PlayerName.Blue);
            cpuPlayer.TieAnswerMoveMadeEvent += new MoveMadeEventHandler<TieAnswerMove>(move => Assert.AreEqual(TieAnswer.ACCEPT, move.MadeMove.TieAnswer));
            cpuPlayer.MakeTieAnswerMove();
        }

        [Test]
        public void TestMakeTieAnswerAfterAllFiguresArePlaced2()
        {
            /// Tie state
            State s = new State();
            s = new State(s, new FigureTakeMove(0));
            s = new State(s, new FigurePlaceMove(0, 0));

            s = new State(s, new FigureTakeMove(1));
            s = new State(s, new FigurePlaceMove(0, 1));

            s = new State(s, new FigureTakeMove(2));
            s = new State(s, new FigurePlaceMove(0, 2));

            s = new State(s, new FigureTakeMove(12));
            s = new State(s, new FigurePlaceMove(0, 3));

            s = new State(s, new FigureTakeMove(4));
            s = new State(s, new FigurePlaceMove(1, 0));

            s = new State(s, new FigureTakeMove(5));
            s = new State(s, new FigurePlaceMove(1, 1));

            s = new State(s, new FigureTakeMove(6));
            s = new State(s, new FigurePlaceMove(1, 2));

            s = new State(s, new FigureTakeMove(11));
            s = new State(s, new FigurePlaceMove(1, 3));

            s = new State(s, new FigureTakeMove(8));
            s = new State(s, new FigurePlaceMove(2, 0));

            s = new State(s, new FigureTakeMove(9));
            s = new State(s, new FigurePlaceMove(2, 1));

            s = new State(s, new FigureTakeMove(10));
            s = new State(s, new FigurePlaceMove(2, 2));

            s = new State(s, new FigureTakeMove(7));
            s = new State(s, new FigurePlaceMove(2, 3));

            s = new State(s, new FigureTakeMove(15));
            s = new State(s, new FigurePlaceMove(3, 0));

            s = new State(s, new FigureTakeMove(14));
            s = new State(s, new FigurePlaceMove(3, 1));

            s = new State(s, new FigureTakeMove(13));
            s = new State(s, new FigurePlaceMove(3, 2));

            s = new State(s, new FigureTakeMove(3));
            s = new State(s, new FigurePlaceMove(3, 3));

            CpuPlayer cpuPlayer = new CpuPlayer(s, PlayerName.Blue);
            cpuPlayer.TieAnswerMoveMadeEvent += new MoveMadeEventHandler<TieAnswerMove>(move => Assert.AreEqual(TieAnswer.ACCEPT, move.MadeMove.TieAnswer));
            cpuPlayer.MakeTieAnswerMove();
        }
    }
}
