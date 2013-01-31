﻿using System;
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

namespace Quarto
{
    public enum GameType { 
        PvP = 0,
        PvC = 1,
        CvP = 2
    };

    public enum PlayerName {
        Red = 0,
        Blue = 1
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class HumanPlayer : IPlayer {

            private MainWindow _mainWindow;
            public MainWindow MainWindow
            {
                get { return _mainWindow; }
                set { _mainWindow = value; }
            }

            private Brush _brush;
            public Brush Brush {
                get { return _brush; }
                set { _brush = value; }
            }

            public PlayerName PlayerName;
            public HumanPlayer(PlayerName playerName, MainWindow mainWindow) {
                PlayerName = playerName;
                MainWindow = mainWindow;
                Brush = (PlayerName == PlayerName.Red)?
                    new SolidColorBrush(Color.FromRgb(0xCC, 0x55, 0x55)) :
                    new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0xCC)) ;

            }

            public void MakeFigureTakeMove() {
                MainWindow.MovePhase = MovePhase.TAKE; // possible solution MainWindow.WaitForFigureTakeMoveMade(this,MovePhase.TAKE)
                MainWindow.ActiveHumanPlayer = this;
            }
            public void MakeFigurePlaceMove() {
                MainWindow.MovePhase = MovePhase.PLACE;
                MainWindow.ActiveHumanPlayer = this;
            }
            public void MakeTieAnswerMove() {
                MainWindow.ActiveHumanPlayer = this;
                MainWindow.OfferActivePlayerATie();
            }

            public event MoveMadeEventHandler<FigureTakeMove> FigureTakeMoveMadeEvent;
            public event MoveMadeEventHandler<FigurePlaceMove> FigurePlaceMoveMadeEvent;
            public event MoveMadeEventHandler<TieAnswerMove> TieAnswerMoveMadeEvent;
            public event MoveMadeEventHandler<TieOfferMove> TieOfferMoveMadeEvent;
            public event MoveMadeEventHandler<SurrenderMove> SurrenderMoveMadeEvent;
            public event MoveMadeEventHandler<QuartoSayingMove> QuartoSayingMoveMadeEvent;

            public void FigureTakeMoveMade(byte Figure) {
                FigureTakeMoveMadeEvent( new MoveMadeEventArgs<FigureTakeMove>( new FigureTakeMove( Figure) ) );
            }
            public void FigurePlaceMoveMade(byte x, byte y) {
                FigurePlaceMoveMadeEvent(new MoveMadeEventArgs<FigurePlaceMove>(new FigurePlaceMove(x, y)));
            }
            public void TieAnswerMoveMade(TieAnswer tieAnswer) {
                TieAnswerMoveMadeEvent(new MoveMadeEventArgs<TieAnswerMove>(new TieAnswerMove(tieAnswer)));
            }
            public void TieOfferMoveMade() {
                TieOfferMoveMadeEvent(new MoveMadeEventArgs<TieOfferMove>(new TieOfferMove()));
            }
            public void SurrenderMoveMade() {
                SurrenderMoveMadeEvent(new MoveMadeEventArgs<SurrenderMove>(new SurrenderMove()));
            }
            public void QuartoSayingMoveMade() {
                QuartoSayingMoveMadeEvent(new MoveMadeEventArgs<QuartoSayingMove>(new QuartoSayingMove()));
            }


            public void InformAboutMove(Move opponentMove)
            {
                if (opponentMove is FigureTakeMove) {

                }
                else if (opponentMove is FigurePlaceMove) { 
                
                }
                else if (opponentMove is TieAnswerMove)
                {

                }
                else if (opponentMove is TieOfferMove)
                {

                }
                else if (opponentMove is SurrenderMove)
                {

                }
                else if (opponentMove is QuartoSayingMove)
                {

                }
            }

            public void Lose(byte line, byte sign, string message)
            { 
                //TODO display lose text
                
            }
            public void Win(byte line, byte sign, string message)
            {
                //TODO display win text
                MainWindow.ActiveHumanPlayer = this;
                MainWindow.InformAboutWin( line,  sign,  message);
            }
            public void HaveATie(string message)
            {
                //TODO display tie text
                MainWindow.InformAboutTie();
            }
        }

        GameType gameType;
        FigureWrapper[] Wrappers;
        GamePlatform GamePlatform;
        const byte NF = 16;

        public HumanPlayer _activeHumanPlayer;
        public HumanPlayer ActiveHumanPlayer {
            get { return _activeHumanPlayer;  }
            set {
                ActiveHumanPlayerChanging();
                _activeHumanPlayer = value;
                ActiveHumanPlayerChanged();
            }
        }

        protected void ActiveHumanPlayerChanging() { }
        protected void ActiveHumanPlayerChanged() {
            Rectangle TurnIndicatorRectangle = (Rectangle)FindName( "TurnIndicatorRectangle" );
            TurnIndicatorRectangle.Fill = ActiveHumanPlayer.Brush;
        }

        private MovePhase _movePhase;
        public MovePhase MovePhase {
            get { return _movePhase; }
            set { _movePhase = value; }
        }

        #region PlayerTurn
        /// <summary>
        /// Current Player
        /// </summary>
        private PlayerTurn _playerTurn;
        public PlayerTurn PlayerTurn {
            get { return _playerTurn; }
            set {
                PlayerTurnChanging();
                _playerTurn = value;
                PlayerTurnChanged();
            }
        }

        private void PlayerTurnChanging() { 
        
        }

        private void PlayerTurnChanged()
        {
            /*
            if (PlayerTurn == PlayerTurn.BLUE)
            {
                activePlayerBrush = bluePlayerBrush;
            }
            else if (PlayerTurn == PlayerTurn.RED)
            {
                activePlayerBrush = redPlayerBrush;
            }
            Rectangle TurnIndicatorRectangle = (Rectangle)FindName("TurnIndicatorRectangle");
            TurnIndicatorRectangle.Fill = activePlayerBrush;
             * */
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            // Start Player Vs Player Game
            StartPvP();
        }

        private void CreateNewGame() {
            InitializeFigures();
            InitializeGameField();
            InitializeControls();
        }

        private void InitializeFigures() {

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
                    Wrappers[k] = new FigureWrapper(res[k]);

                    int b;
                    for(b = 0; b < 16; b++)
                        if(Grid.GetRow(FiguresToTakeGrid.Children[b]) == i && Grid.GetColumn(FiguresToTakeGrid.Children[b]) == j)
                            break;

                    ((Border)FiguresToTakeGrid.Children[b]).Child = Wrappers[k];

                    //Wrappers[k].MouseUp += FigureWrapper_MouseUp;
                    //Wrappers[k].MouseEnter += FigureWrapper_MouseEnter;
                    //Wrappers[k].MouseLeave += FigureWrapper_MouseLeave;

                    k++;
                }
        }
        private void InitializeGameField() {
            Grid GameFieldGrid = (Grid)FindName("GameFieldGrid");
            foreach (Border circleBorder in GameFieldGrid.Children)
            {
                circleBorder.Child = null;
                circleBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
            }
        }
        private void InitializeControls() {
            Border takeFigureBorder = (Border)FindName("FigureToPlaceBorder");
            takeFigureBorder.Child = null;
        }

        private void StartPvP()
        {
            CreateNewGame();
            gameType = GameType.PvP;
            PlayerTurn = PlayerTurn.RED;
            MovePhase = MovePhase.TAKE;
            GamePlatform = new GamePlatform(new HumanPlayer(PlayerName.Red,this),new HumanPlayer(PlayerName.Blue,this));
            GamePlatform.StartGame();
        }
        private void StartPvC()
        {
            CreateNewGame();
            gameType = GameType.PvC;
            PlayerTurn = PlayerTurn.RED;
            MovePhase = MovePhase.TAKE;
            throw new NotImplementedException();
        }
        private void StartCvP()
        {
            //TODO
            throw new NotImplementedException();
        }

        #region FPCircleBorderEventHandlers
        /// <summary>
        /// Event handler is utilized when MovePhase == PLACE
        /// </summary>
        private void CircleBorder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (MovePhase == MovePhase.TAKE) return;
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

            ActiveHumanPlayer.FigurePlaceMoveMade(x, y);
        }
        /// <summary>
        /// Event handler is utilized when MovePhase == PLACE
        /// </summary>
        private void CircleBorder_MouseEnter(object sender, MouseEventArgs e) {
            if (MovePhase == MovePhase.TAKE) return;
            Border circleBorder = (Border)sender;
            if (circleBorder.Child != null)
                return;
            circleBorder.Cursor = Cursors.Hand;
            circleBorder.BorderBrush = ActiveHumanPlayer.Brush;
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
            Border figureToTakeBorder = (Border)sender;
            if (figureToTakeBorder.Child == null)
                return;
            Border figureToPlaceBorder = (Border)FindName("FigureToPlaceBorder");

            FigureWrapper figureWrapper = (FigureWrapper)figureToTakeBorder.Child;
            figureToTakeBorder.Child = null;

            figureToPlaceBorder.Child = figureWrapper;

            figureToTakeBorder.Cursor = Cursors.Arrow;
            figureToTakeBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0x00, 0xEE, 0xEE, 0xEE));

            ActiveHumanPlayer.FigureTakeMoveMade(figureWrapper.Figure);
        }
        /// <summary>
        /// Event handler is utilized when MovePhase == TAKE
        /// </summary>
        private void FTCircleBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            if (MovePhase == MovePhase.PLACE) return;
            Border circleBorder = (Border)sender;
            if (circleBorder.Child == null)
                return;
            circleBorder.Cursor = Cursors.Hand;
            circleBorder.BorderBrush = ActiveHumanPlayer.Brush;
        }

        private void FTCircleBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            Border circleBorder = (Border)sender;
            circleBorder.Cursor = Cursors.Arrow;
            circleBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0x00, 0xEE, 0xEE, 0xEE));
        }
        #endregion

        public void OfferActivePlayerATie() {
            TieAnswer tieAnswer = (MessageBox.Show(
                string.Format("{0} player was offered a tie! Does he accept it?", ActiveHumanPlayer.PlayerName),
                "Tie Offer",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)? TieAnswer.ACCEPT: TieAnswer.DECLINE;
            ActiveHumanPlayer.TieAnswerMoveMade(tieAnswer);
        }

        private bool informedAboutTie;
        public void InformAboutTie() {
            if (informedAboutTie)
            {
                informedAboutTie = false;
                return;
            }
            informedAboutTie = true;
            MessageBoxResult result = MessageBox.Show(
                "Tie! Do you want to play again?",
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

        public void InformAboutWin(byte line, byte sign, string message )
        {
            HighLightTheLine(line);
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
                            temp.BorderBrush = ActiveHumanPlayer.Brush;
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
                            temp.BorderBrush = ActiveHumanPlayer.Brush;
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
                            temp.BorderBrush = ActiveHumanPlayer.Brush;
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
                            temp.BorderBrush = ActiveHumanPlayer.Brush;
                            break;
                        }
                    }
            }
        }

        public void Restart() {
            switch (gameType) { 
                case GameType.PvP:
                    StartPvP(); break;
                case GameType.PvC:
                    StartPvC(); break;
                case GameType.CvP:
                    StartCvP(); break;
            }
        }

        #region ActionButtonEventHandlers
        private void SayQuartoButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveHumanPlayer.QuartoSayingMoveMade();
        }

        private void TieOfferButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveHumanPlayer.TieOfferMoveMade();
        }

        private void SurrenderButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveHumanPlayer.SurrenderMoveMade();
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
        #endregion

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
    }
}
