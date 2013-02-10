using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using QuartoLib;
using System.Threading;

namespace Quarto
{
    public enum GameType { 
        PvP = 0,
        PvC = 1,
        CvP = 2
    };

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GameType gameType;
        FigureWrapper[] Wrappers;
        GamePlatform GamePlatform;
        const byte NF = 16;

        Brush ActivePlayerBrush;
        public IPlayer _activePlayer;
        public IPlayer ActivePlayer {
            get { return _activePlayer; }
            set {
                ActivePlayerChanging();
                _activePlayer = value;
                ActivePlayerChanged(value);
            }
        }
        protected void ActivePlayerChanging() { }
        protected void ActivePlayerChanged(IPlayer player) {
            ActivePlayerBrush = (player.Name == PlayerName.Red) ?
                    new SolidColorBrush(Color.FromRgb(0xCC, 0x55, 0x55)) :
                    new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0xCC));
            Rectangle TurnIndicatorRectangle = (Rectangle)FindName( "TurnIndicatorRectangle" );
            TurnIndicatorRectangle.Fill = ActivePlayerBrush;
        }

        OpponentThinkingWindow _opponentThinkingWindow;

        private MovePhase _movePhase;
        public MovePhase MovePhase {
            get { return _movePhase; }
            set { _movePhase = value; }
        }

        /// <summary>
        /// Current Player
        /// </summary>
        private PlayerTurn _playerTurn;
        public PlayerTurn PlayerTurn {
            get { return _playerTurn; }
            set {
                _playerTurn = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            // Start Player Vs Player Game
            StartPvP();
        }

        private void CreateNewGame() {
            InitializeGameField();
            InitializeFiguresToTakeField();
            InitializeFigures();
            InitializeControls();
            PlayerTurn = PlayerTurn.RED;
            MovePhase = MovePhase.TAKE;
        }
        
        private Border[] _gameFieldBorders = new Border[16];
        private void InitializeGameField() {
            Grid GameFieldGrid = (Grid)FindName("GameFieldGrid");
            int i = 0;
            foreach (Border circleBorder in GameFieldGrid.Children)
            {
                circleBorder.Child = null;
                circleBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
                _gameFieldBorders[i++] = circleBorder;
            }
        }

        private Border[] _figuresToTakeBorders = new Border[16];
        private void InitializeFiguresToTakeField()
        {
            Grid FiguresToTakeGrid = (Grid)FindName("FiguresToTakeGrid");
            int i = 0;
            foreach (Border circleBorder in FiguresToTakeGrid.Children)
            {
                _figuresToTakeBorders[i++] = circleBorder;
            }
        }
        private void InitializeFigures()
        {
            byte i, j;
            // Shuffle figures randomly
            Wrappers = new FigureWrapper[NF];
            Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);

            byte[] f = new byte[NF];
            for (i = 0; i < NF; i++)
                f[i] = (byte)r.Next(16 - i);

            bool[] u = new bool[NF];
            for (i = 0; i < NF; i++)
                u[i] = false;

            byte[] res = new byte[NF];
            for (i = 0; i < NF; i++)
            {
                byte t = f[i];
                for (j = NF - 1; j >= 0; j--)
                    if (!u[j])
                    {
                        if (t == 0)
                            break;
                        t--;
                    }
                res[i] = j;
                u[j] = true;
            }

            Grid FiguresToTakeGrid = (Grid)FindName("FiguresToTakeGrid");
            //FiguresToTakeGrid.Children.Clear();

            int k = 0;
            for (i = 0; i < 4; i++)
                for (j = 0; j < 4; j++)
                {
                    byte tf = res[k]; // byte tf = k;
                    Wrappers[tf] = new FigureWrapper(tf);
                    _figuresToTakeBorders[tf].Child = Wrappers[tf];
                    Grid.SetRow(_figuresToTakeBorders[tf], i);
                    Grid.SetColumn(_figuresToTakeBorders[tf], j);
                    //Wrappers[k].MouseUp += FigureWrapper_MouseUp;
                    //Wrappers[k].MouseEnter += FigureWrapper_MouseEnter;
                    //Wrappers[k].MouseLeave += FigureWrapper_MouseLeave;

                    k++;
                }
        }
        private void InitializeControls() {
            Border takeFigureBorder = (Border)FindName("FigureToPlaceBorder");
            takeFigureBorder.Child = null;
        }

        public void Restart()
        {
            switch (gameType)
            {
                case GameType.PvP:
                    StartPvP(); break;
                case GameType.PvC:
                    StartPvC(); break;
                case GameType.CvP:
                    StartCvP(); break;
            }
        }
        private void StartPvP()
        {
            CreateNewGame();
            gameType = GameType.PvP;
            HumanPlayer red = new HumanPlayer(PlayerName.Red);
            HumanPlayer blue = new HumanPlayer(PlayerName.Blue);
            SubscribeToPlayerEvents(red);
            SubscribeToPlayerEvents(blue);
            GamePlatform = new GamePlatform( red, blue);
            GamePlatform.StartGame();
        }
        private void StartPvC()
        {
            CreateNewGame();
            gameType = GameType.PvC;
            HumanPlayer red = new HumanPlayer(PlayerName.Red);
            SubscribeToPlayerEvents(red);
            GamePlatform = new GamePlatform(red, new CpuPlayer(new State(), PlayerName.Blue));
            GamePlatform.StartGame();
        }
        private void StartCvP()
        {
            CreateNewGame();
            gameType = GameType.PvC;
            HumanPlayer blue = new HumanPlayer(PlayerName.Blue);
            SubscribeToPlayerEvents(blue);
            GamePlatform = new GamePlatform(new CpuPlayer(new State(), PlayerName.Red), blue);
            GamePlatform.StartGame();
        }

        private void SubscribeToPlayerEvents(HumanPlayer player) { 
            player.OrderedToMakeFigureTakeMoveEvent     += PrepareForFigureTakeMove;
            player.OrderedToMakeFigurePlaceMoveEvent    += PrepareForFigurePlaceMove;
            player.OrderedToMakeTieAnswerMoveEvent      += OfferPlayerATie;

            player.OpponentFigureTakeMoveMadeEvent      += TakeFigure;
            player.OpponentFigurePlaceMoveMadeEvent     += PlaceFigure;
            player.OpponentTieAnswerMoveMadeEvent       += DisplayTieAnswer;
            //player.OpponentTieOfferMoveMadeEvent += ;
            //player.OpponentSurrenderMoveMadeEvent += ;
            //player.OpponentQuartoSayingMoveMadeEvent += ;

            player.WinEvent     += InformAboutWin;
            player.LoseEvent    += InformAboutLose;
            player.TieEvent     += InformAboutTie;
        }
        public void OfferPlayerATie(HumanPlayer player)
        {
            ActivePlayer = player;
            TieAnswer tieAnswer = (MessageBox.Show(
                string.Format("{0} player was offered a tie! Does he accept it?", player.Name.ToString()),
                "Tie Offer",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes) ? TieAnswer.ACCEPT : TieAnswer.DECLINE;
            player.TieAnswerMoveMade(tieAnswer);
        }
        public void PrepareForFigureTakeMove(HumanPlayer player)
        {
            ActivePlayer = player;
            MovePhase = MovePhase.TAKE;
        }
        public void PrepareForFigurePlaceMove(HumanPlayer player)
        {
            ActivePlayer = player;
            MovePhase = MovePhase.PLACE;
        }
        public void TakeFigure(MoveMadeEventArgs<FigureTakeMove> moveArgs)
        {
            var move = moveArgs.MadeMove;
            if (gameType == GameType.PvP)
                return;
            Border figureToTakeBorder = _figuresToTakeBorders[move.FigureGivenToOpponent];
            Border figureToPlaceBorder = (Border)FindName("FigureToPlaceBorder");
            FigureWrapper figureWrapper = (FigureWrapper)figureToTakeBorder.Child;
            figureToTakeBorder.Child = null;
            figureToPlaceBorder.Child = figureWrapper;
        }
        public void PlaceFigure(MoveMadeEventArgs<FigurePlaceMove> moveArgs)
        {
            var move = moveArgs.MadeMove;
            if (gameType == GameType.PvP)
                return;
            Border placeFigureBorder = _gameFieldBorders[move.XFigurePlacedTo * 4 + move.YFigurePlacedTo];
            Border takeFigureBorder = (Border)FindName("FigureToPlaceBorder");
            FigureWrapper figureWrapper = (FigureWrapper)takeFigureBorder.Child;
            takeFigureBorder.Child = null;
            placeFigureBorder.Child = figureWrapper;
        }
        public void DisplayTieAnswer(MoveMadeEventArgs<TieAnswerMove> moveArgs)
        {
            var move = moveArgs.MadeMove;
            if (gameType == GameType.PvP)
                return;
            string message;
            if(move.TieAnswer == TieAnswer.DECLINE)
                message = string.Format("Tie offer was declined.");
            else
                message = string.Format("Tie offer was accepted.");
            MessageBox.Show(message);
        }

        #region FPCircleBorderEventHandlers
        /// <summary>
        /// Event handler is utilized when MovePhase == PLACE
        /// </summary>
        private void CircleBorder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (MovePhase == MovePhase.TAKE) return;
            if (ActivePlayer is HumanPlayer == false)
                return;
            Border placeFigureBorder = (Border)sender;
            if (placeFigureBorder.Child != null)
                return;
            Border takeFigureBorder = (Border)FindName("FigureToPlaceBorder");

            FigureWrapper figureWrapper = (FigureWrapper)takeFigureBorder.Child;
            takeFigureBorder.Child = null;

            placeFigureBorder.Child = figureWrapper;

            placeFigureBorder.Cursor = Cursors.Arrow;
            placeFigureBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));

            byte x = (byte)Grid.GetRow(placeFigureBorder);
            byte y = (byte)Grid.GetColumn(placeFigureBorder);

            (ActivePlayer as HumanPlayer).FigurePlaceMoveMade(x, y);
        }
        /// <summary>
        /// Event handler is utilized when MovePhase == PLACE
        /// </summary>
        private void CircleBorder_MouseEnter(object sender, MouseEventArgs e) {
            if (MovePhase == MovePhase.TAKE) return;
            if (ActivePlayer is HumanPlayer == false)
                return;
            Border circleBorder = (Border)sender;
            if (circleBorder.Child != null)
                return;
            circleBorder.Cursor = Cursors.Hand;
            circleBorder.BorderBrush = ActivePlayerBrush;
        }

        private void CircleBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            Border circleBorder = (Border)sender;
            circleBorder.Cursor = Cursors.Arrow;
            circleBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
        }
        #endregion

        #region FTCircleBorderEventHandlers
        /// <summary>
        /// Event handler is utilized when MovePhase == TAKE
        /// </summary>
        private void FTCircleBorder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (MovePhase == MovePhase.PLACE) return;
            if (ActivePlayer is HumanPlayer == false)
                return;
            Border figureToTakeBorder = (Border)sender;
            if (figureToTakeBorder.Child == null)
                return;
            Border figureToPlaceBorder = (Border)FindName("FigureToPlaceBorder");

            FigureWrapper figureWrapper = (FigureWrapper)figureToTakeBorder.Child;
            figureToTakeBorder.Child = null;

            figureToPlaceBorder.Child = figureWrapper;
            figureToTakeBorder.Cursor = Cursors.Arrow;
            figureToTakeBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0x00, 0xEE, 0xEE, 0xEE));

            //Thread t = new Thread(abc);
            //List<object> paramL = new List<object>();
            //paramL.Add(ActivePlayer);
            //paramL.Add(figureWrapper.Figure);
            //t.Start(paramL);

            (ActivePlayer as HumanPlayer).FigureTakeMoveMade(figureWrapper.Figure);
        }

        public static void abc(object param) {
            if (!(param is List<object>))
                throw new ArgumentException("Invalid parameter");
            List<object> paramL = param as List<object>;
            if(paramL.Count != 2)
                throw new ArgumentException("Invalid parameter");
            if(!(paramL[0] is HumanPlayer) || !(paramL[1] is byte))
                throw new ArgumentException("Invalid parameter");

            HumanPlayer player = paramL[0] as HumanPlayer;
            byte figure = (byte)paramL[1];

            player.FigureTakeMoveMade(figure);
        }
        /// <summary>
        /// Event handler is utilized when MovePhase == TAKE
        /// </summary>
        private void FTCircleBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            if (MovePhase == MovePhase.PLACE) return;
            if (ActivePlayer is HumanPlayer == false)
                return;
            Border circleBorder = (Border)sender;
            if (circleBorder.Child == null)
                return;
            circleBorder.Cursor = Cursors.Hand;
            circleBorder.BorderBrush = ActivePlayerBrush;
        }

        private void FTCircleBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            Border circleBorder = (Border)sender;
            circleBorder.Cursor = Cursors.Arrow;
            circleBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0x00, 0xEE, 0xEE, 0xEE));
        }
        #endregion

        private bool informedAboutTie;
        public void InformAboutTie(byte line, byte sign, string message)
        {
            if (gameType == GameType.PvP)
            {
                if (informedAboutTie)
                {
                    informedAboutTie = false;
                    return;
                }
                informedAboutTie = true;
            }
            OfferToPlayAgain("Tie!");
        }
        public void InformAboutWin(byte line, byte sign, string message )
        {
            HighLightTheLine(line);
            OfferToPlayAgain(message);
        }
        public void InformAboutLose(byte line, byte sign, string message)
        {
            if (GameType.PvP == gameType)
                return;
            HighLightTheLine(line);
            OfferToPlayAgain(message);
        }
        private void OfferToPlayAgain(string message)
        {
            MessageBoxResult result = MessageBox.Show(
                message + "\nDo you want to play again?",
                "Game Over",
                MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Restart();
            }
            else
            {
                Close();
            }
        }

        private void HighLightTheLine(byte line) {
            Grid gameFieldGrid = (Grid)FindName("GameFieldGrid");
            if (line < 4)
                for (int j = 0; j < 4; j++)
                    for (int b = 0; b < NF; b++)
                    {
                        Border temp = (Border)gameFieldGrid.Children[b];
                        int x = Grid.GetRow(temp);
                        int y = Grid.GetColumn(temp);
                        if (x == line && j == y)
                        {
                            temp.BorderBrush = ActivePlayerBrush;
                            break;
                        }
                    }
            else if (line < 8) {
                for (int i = 0; i < 4; i++)
                    for (int b = 0; b < NF; b++)
                    {
                        Border temp = (Border)gameFieldGrid.Children[b];
                        int x = Grid.GetRow(temp);
                        int y = Grid.GetColumn(temp);
                        if (x == i && y == line - 4)
                        {
                            temp.BorderBrush = ActivePlayerBrush;
                            break;
                        }
                    }
            }
            else if (line == 8) {
                for(int i = 0; i < 4; i++)
                    for (int b = 0; b < NF; b++)
                    {
                        Border temp = (Border)gameFieldGrid.Children[b];
                        int x = Grid.GetRow(temp);
                        int y = Grid.GetColumn(temp);
                        if (x == i && y == i)
                        {
                            temp.BorderBrush = ActivePlayerBrush;
                            break;
                        }
                    }
            }
            else if (line == 9) {
                for (int i = 0, j = 3; i < 4; i++, j--)
                    for (int b = 0; b < NF; b++)
                    {
                        Border temp = (Border)gameFieldGrid.Children[b];
                        int x = Grid.GetRow(temp);
                        int y = Grid.GetColumn(temp);
                        if (x == i && y == j)
                        {
                            temp.BorderBrush = ActivePlayerBrush;
                            break;
                        }
                    }
            }
        }

        #region ActionButtonEventHandlers
        private void SayQuartoButton_Click(object sender, RoutedEventArgs e)
        {
            if(ActivePlayer is HumanPlayer == false)
                return;
            (ActivePlayer as HumanPlayer).QuartoSayingMoveMade();
        }

        private void TieOfferButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActivePlayer is HumanPlayer == false)
                return;
            (ActivePlayer as HumanPlayer).TieOfferMoveMade();
        }

        private void SurrenderButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActivePlayer is HumanPlayer == false)
                return;
            (ActivePlayer as HumanPlayer).SurrenderMoveMade();
        }
        #endregion

        #region MenuButtonsEventHandlers
        private void FullScreenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!isInFullscreenMode)
            {
                EnableFullscreen();
            }
            else 
            {
                DisableFullscreen();
            }
        }

        private void EnableFullscreen() {
            if (isInFullscreenMode)
                return;
            isInFullscreenMode = true;
            this.WindowState = System.Windows.WindowState.Maximized;
            this.WindowStyle = System.Windows.WindowStyle.None;
        }

        private void DisableFullscreen()
        {
            if (!isInFullscreenMode)
                return;
            isInFullscreenMode = false;
            this.WindowState = System.Windows.WindowState.Normal;
            this.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
        }

        private void PlayerVsPlayer_Click(object sender, RoutedEventArgs e)
        {
            CreateNewGame();
            StartPvP();
        }
        private void PlayerVsCpu_Click(object sender, RoutedEventArgs e)
        {
            CreateNewGame();
            StartPvC();
        }
        private void CpuVsPlayer_Click(object sender, RoutedEventArgs e)
        {
            CreateNewGame();
            StartCvP();
        }

        private bool isInFullscreenMode = false;
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) {
                DisableFullscreen();
            }
        }
        
        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }
        
        private void RulesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RulesWindow rulesWindow = new RulesWindow();
            rulesWindow.ShowDialog();
        }

        private void StandardPiecesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < NF; i++)
                Wrappers[i].SwitchToPieceView();
        }

        private void CodeRectanglesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < NF; i++)
                Wrappers[i].SwitchToCodeView();
        }
        #endregion
    }
}
