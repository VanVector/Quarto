using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartoLib
{
    public enum PlayerTurn
    {
        RED = 0,
        BLUE = 1
    };

    public enum MovePhase
    {
        PLACE = 0,
        TAKE = 1
    };

    public class GamePlatform
    {
        const string SurrenderMessage = "{1} player has surrendered. Congratulations to {0} player! He is the winner. ";
        const string QuartoCorrectSayingMessage = "{0} player has won by finding a winning line and saying \"Quarto\".";
        const string QuartoIncorrectSayingMessage = "{0} player is the winner! {1} player has lost by saying \"Quarto\" when there were no winning lines.";
        const string HaveATieMessage = "Tie! Both players have accepted it.";

        const byte NO_LINE = 10;
        const byte NO_SIGN = 8;

        /// <summary>
        /// The first player to make move in game.
        /// </summary>
        private IPlayer _redPlayer;
        public IPlayer RedPlayer {
            get { return _redPlayer; }
            private set { _redPlayer = value; }
        }

        /// <summary>
        /// The second player to make move in game.
        /// </summary>
        private IPlayer _bluePlayer;
        public IPlayer BluePlayer
        {
            get { return _bluePlayer; }
            private set { _bluePlayer = value; }
        }

        /// <summary>
        /// Defines current active player.
        /// </summary>
        private PlayerTurn _playerTurn;
        public PlayerTurn PlayerTurn
        {
            get { return _playerTurn; }
            private set { _playerTurn = value; }
        }

        /// <summary>
        /// Defines current move phase.
        /// </summary>
        private MovePhase _movePhase;
        public MovePhase MovePhase
        {
            get { return _movePhase; }
            private set { _movePhase = value; }
        }

        /// <summary>
        /// Defines current game state.
        /// </summary>
        private State _currentState;
        public State CurrentState
        {
            get { return _currentState; }
            private set { _currentState = value; }
        }

        /// <summary>
        /// Player alternation.
        /// </summary>
        private void AlternatePlayer()
        {
            PlayerTurn = (PlayerTurn == PlayerTurn.BLUE) ? PlayerTurn.RED : PlayerTurn.BLUE;
        }

        /// <summary>
        /// Phase alternation.
        /// </summary>
        private void AlternatePhase()
        {
            MovePhase = (MovePhase == MovePhase.PLACE) ? MovePhase.TAKE : MovePhase.PLACE;
        }

        /// <summary>
        /// Game initialization constructor.
        /// </summary>
        /// <param name="red"></param>
        /// <param name="blue"></param>
        public GamePlatform(IPlayer red, IPlayer blue) {
            RedPlayer = red;
            BluePlayer = blue;
            SubscriberRedPlayerEvents();
            SubscriberBluePlayerEvents();

            PlayerTurn = PlayerTurn.RED;
            CurrentState = new State();
            MovePhase = MovePhase.TAKE;
        }

        private void SubscriberRedPlayerEvents() 
        {
            RedPlayer.FigureTakeMoveMadeEvent += RedPlayerFigureTakeMoveMade;
            RedPlayer.FigurePlaceMoveMadeEvent += RedPlayerFigurePlaceMoveMade;
            RedPlayer.TieAnswerMoveMadeEvent += RedPlayerTieAnswerMoveMade;
            RedPlayer.TieOfferMoveMadeEvent += RedPlayerTieOfferMoveMade;
            RedPlayer.SurrenderMoveMadeEvent += RedPlayerSurrenderMoveMade;
            RedPlayer.QuartoSayingMoveMadeEvent += RedPlayerQuartoSayingMoveMade;
        }

        private void SubscriberBluePlayerEvents()
        {
            BluePlayer.FigureTakeMoveMadeEvent += BluePlayerFigureTakeMoveMade;
            BluePlayer.FigurePlaceMoveMadeEvent += BluePlayerFigurePlaceMoveMade;
            BluePlayer.TieAnswerMoveMadeEvent += BluePlayerTieAnswerMoveMade;
            BluePlayer.TieOfferMoveMadeEvent += BluePlayerTieOfferMoveMade;
            BluePlayer.SurrenderMoveMadeEvent += BluePlayerSurrenderMoveMade;
            BluePlayer.QuartoSayingMoveMadeEvent += BluePlayerQuartoSayingMoveMade;
        }

        /// <summary>
        /// Starts the game process.
        /// Contains main game loop.
        /// </summary>
        public void StartGame()
        {
            // Red player starts from taking FigureTakeMove.
            RedPlayer.MakeFigureTakeMove();
        }

        #region RedPlayerEventHandlers 
        protected void RedPlayerFigureTakeMoveMade(MoveMadeEventArgs<FigureTakeMove> args) {
            CurrentState = new State(CurrentState, args.MadeMove);
            BluePlayer.InformAboutMove(args.MadeMove);
            AlternatePhase();
            AlternatePlayer();
            BluePlayer.MakeFigurePlaceMove();
        }
        protected void RedPlayerFigurePlaceMoveMade( MoveMadeEventArgs<FigurePlaceMove> args )
        {
            CurrentState = new State(CurrentState, args.MadeMove);
            BluePlayer.InformAboutMove(args.MadeMove);
            AlternatePhase();
            RedPlayer.MakeFigureTakeMove();
        }
        protected void RedPlayerTieAnswerMoveMade( MoveMadeEventArgs<TieAnswerMove> args )
        {
            TieAnswerMove move = args.MadeMove;
            BluePlayer.InformAboutMove(move);
            if (move.TieAnswer == TieAnswer.ACCEPT)
            {
                RedPlayer.HaveATie(HaveATieMessage);
                BluePlayer.HaveATie(HaveATieMessage);
            }
            else // move.TieAnswer == TieAnswer.DECLINE
            {
                // continue game
                if (MovePhase == MovePhase.TAKE)
                    BluePlayer.MakeFigureTakeMove();
                else
                    BluePlayer.MakeFigurePlaceMove();
            }
        }
        protected void RedPlayerTieOfferMoveMade( MoveMadeEventArgs<TieOfferMove> args )
        {
            BluePlayer.InformAboutMove(args.MadeMove);
            BluePlayer.MakeTieAnswerMove();
        }
        protected void RedPlayerSurrenderMoveMade( MoveMadeEventArgs<SurrenderMove> args )
        {
            BluePlayer.InformAboutMove(args.MadeMove);
            BluePlayer.Win(NO_LINE, NO_SIGN, string.Format(SurrenderMessage, "Blue", "Red"));
            RedPlayer.Lose(NO_LINE, NO_SIGN, string.Format(SurrenderMessage, "Blue", "Red"));
        }
        protected void RedPlayerQuartoSayingMoveMade( MoveMadeEventArgs<QuartoSayingMove> args )
        {
            BluePlayer.InformAboutMove(args.MadeMove);
            FindTheWinner();
        }
        #endregion

        #region BluePlayerEventHandlers
        protected void BluePlayerFigureTakeMoveMade(MoveMadeEventArgs<FigureTakeMove> args)
        {
            CurrentState = new State(CurrentState, args.MadeMove);
            RedPlayer.InformAboutMove(args.MadeMove);
            AlternatePhase();
            AlternatePlayer();
            RedPlayer.MakeFigurePlaceMove();
        }
        protected void BluePlayerFigurePlaceMoveMade(MoveMadeEventArgs<FigurePlaceMove> args)
        {
            CurrentState = new State(CurrentState, args.MadeMove);
            RedPlayer.InformAboutMove(args.MadeMove);
            AlternatePhase();
            BluePlayer.MakeFigureTakeMove();
        }
        protected void BluePlayerTieAnswerMoveMade(MoveMadeEventArgs<TieAnswerMove> args)
        {
            TieAnswerMove move = args.MadeMove;
            RedPlayer.InformAboutMove(move);
            if (move.TieAnswer == TieAnswer.ACCEPT)
            {
                RedPlayer.HaveATie(HaveATieMessage);
                BluePlayer.HaveATie(HaveATieMessage);
            }
            else // move.TieAnswer == TieAnswer.DECLINE
            {
                // continue game
                if (MovePhase == MovePhase.TAKE)
                    RedPlayer.MakeFigureTakeMove();
                else
                    RedPlayer.MakeFigurePlaceMove();
            }
        }
        protected void BluePlayerTieOfferMoveMade(MoveMadeEventArgs<TieOfferMove> args)
        {
            RedPlayer.InformAboutMove(args.MadeMove);
            RedPlayer.MakeTieAnswerMove();
        }
        protected void BluePlayerSurrenderMoveMade(MoveMadeEventArgs<SurrenderMove> args)
        {
            RedPlayer.InformAboutMove(args.MadeMove);
            RedPlayer.Win(NO_LINE, NO_SIGN, string.Format(SurrenderMessage, "Red", "Blue"));
            BluePlayer.Lose(NO_LINE, NO_SIGN, string.Format(SurrenderMessage, "Red", "Blue"));
        }
        protected void BluePlayerQuartoSayingMoveMade(MoveMadeEventArgs<QuartoSayingMove> args)
        {
            RedPlayer.InformAboutMove(args.MadeMove);
            FindTheWinner();
        }
        #endregion

        /// <summary>
        /// Find the winner after Quarto was said.
        /// </summary>
        private void FindTheWinner() {

            IPlayer activePlayer = (PlayerTurn == PlayerTurn.BLUE) ? BluePlayer : RedPlayer;
            IPlayer passivePlayer = (PlayerTurn == PlayerTurn.BLUE) ? RedPlayer : BluePlayer;

            int i = 0,j = 0;
            byte line = 0;
            byte winline = NO_LINE;
            byte winsign = NO_SIGN;

            if (CurrentState.LastFigurePlaced == Figure.NO_FIGURE) {
                string winnerName = (PlayerTurn == PlayerTurn.BLUE) ? "Red" : "Blue";
                string loserName = (PlayerTurn == PlayerTurn.BLUE) ? "Blue" : "Red";
                activePlayer.Lose(NO_LINE, NO_SIGN, string.Format(QuartoIncorrectSayingMessage, winnerName, loserName));
                passivePlayer.Win(NO_LINE, NO_SIGN, string.Format(QuartoIncorrectSayingMessage, winnerName, loserName));
                return;
            }

            bool found = false;
            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 4; j++)
                    if (CurrentState.GameField[i][j] == CurrentState.LastFigurePlaced)
                    {
                        found = true;
                        break;
                    }
                if (found)
                    break;
            }

            int x = i; int y = j;
            // check row
            line = (byte)x;
            bool hasFreePlace = false;
            for (j = 0; j < 4; j++)
                if (CurrentState.GameField[x][j] == Figure.NO_FIGURE)
                {
                    hasFreePlace = true; break;
                }
            if (!hasFreePlace && CurrentState.WinableBySign[line] != 0)
            {
                winline = line;
                for (byte b = 0; b < 8; b++)
                    if (((CurrentState.WinableBySign[line] >> b) & 1) != 0)
                    {
                        winsign = b; break;
                    }
            }
            if(winline == NO_LINE) {
                // check column
                line = (byte)(4 + y);
                hasFreePlace = false;
                for (i = 0; i < 4; i++)
                    if (CurrentState.GameField[i][y] == Figure.NO_FIGURE)
                    {
                        hasFreePlace = true; break;
                    }
                if (!hasFreePlace && CurrentState.WinableBySign[line] != 0)
                {
                    winline = line;
                    for (byte b = 0; b < 8; b++)
                        if (((CurrentState.WinableBySign[line] >> b) & 1) != 0)
                        {
                            winsign = b; break;
                        }
                }
            }
            if(winline == NO_LINE) {
                //check diag
                if (x == y) {
                    line = 8;
                    hasFreePlace = false;
                    for (i = 0, j = 0; i < 4; ++i, ++j)
                        if (CurrentState.GameField[i][j] == Figure.NO_FIGURE)
                        {
                            hasFreePlace = true; break;
                        }
                    if (!hasFreePlace && CurrentState.WinableBySign[line] != 0)
                    {
                        winline = line;
                        for (byte b = 0; b < 8; b++)
                            if (((CurrentState.WinableBySign[line] >> b) & 1) != 0)
                            {
                                winsign = b; break;
                            }
                    }
                }
                else if (y + x == 3)
                {
                    line = 9;
                    hasFreePlace = false;
                    for (i = 0, j = 3; i < 4; ++i, --j)
                        if (CurrentState.GameField[i][j] == Figure.NO_FIGURE)
                        {
                            hasFreePlace = true; break;
                        }
                    if (!hasFreePlace && CurrentState.WinableBySign[line] != 0)
                    {
                        winline = line;
                        for (byte b = 0; b < 8; b++)
                            if (((CurrentState.WinableBySign[line] >> b) & 1) != 0)
                            {
                                winsign = b; break;
                            }
                    }
                }
                else { 
                    // LastPlacedFigure does not belong to any diag
                }
            }

            if (winline == NO_LINE)
            {
                string winnerName = (PlayerTurn == PlayerTurn.BLUE) ? "Red" : "Blue";
                string loserName = (PlayerTurn == PlayerTurn.BLUE) ? "Blue" : "Red";
                activePlayer.Lose(NO_LINE, NO_SIGN, string.Format(QuartoIncorrectSayingMessage, winnerName, loserName));
                passivePlayer.Win(NO_LINE, NO_SIGN, string.Format(QuartoIncorrectSayingMessage, winnerName, loserName));
            }
            else {
                string winnerName = (PlayerTurn == PlayerTurn.BLUE) ? "Blue" : "Red";
                string loserName = (PlayerTurn == PlayerTurn.BLUE) ? "Red" : "Blue";
                activePlayer.Win(winline, winsign, string.Format(QuartoCorrectSayingMessage, winnerName, loserName));
                passivePlayer.Lose(winline, winsign, string.Format(QuartoCorrectSayingMessage, winnerName, loserName));
            }

        }
    }
}
