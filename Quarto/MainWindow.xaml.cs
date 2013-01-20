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

namespace Quarto
{
    public enum PlayerTurn { 
        RED = 0,
        BLUE = 1
    };

    public enum MovePhase
    {
        PLACE = 0,
        TAKE = 1,
    };

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        byte[][] FiguresToTake;
        FigureWrapper[][] Wrappers;
        byte[][] GameField;
        byte LastFigurePlaced;

        const byte NF = 16;
        const byte NO_FIGURE = 16;

        Brush bluePlayerBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0xCC));
        Brush redPlayerBrush = new SolidColorBrush(Color.FromRgb(0xCC, 0x55, 0x55));
        Brush activePlayerBrush;

        #region PlayerTurn
        /// <summary>
        /// Current Player
        /// </summary>
        private PlayerTurn playerTurn;
        private PlayerTurn PlayerTurn {
            get { return playerTurn; }
            set {
                PlayerTurnChanging();
                playerTurn = value;
                PlayerTurnChanged();
            }
        }

        private void PlayerTurnChanging() { 
        
        }

        private void PlayerTurnChanged()
        {
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
        }
        #endregion

        /// <summary>
        /// Current move phase
        /// </summary>
        MovePhase movePhase;

        public MainWindow()
        {
            CreateNewGame();
        }

        private void CreateNewGame() {
            InitializeComponent();
            InitializeFigures();
            InitializeGameField();
            InitializePlayers();
            InitializeControls();
        }

        private void InitializeFigures() {
            LastFigurePlaced = NO_FIGURE;

            byte i, j;
            // Shuffle figures randomly
            FiguresToTake = new byte[4][];
            Wrappers = new FigureWrapper[4][];
            for (i = 0; i < 4; i++)
            {
                FiguresToTake[i] = new byte[4];
                Wrappers[i] = new FigureWrapper[4];
            }

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
            FiguresToTakeGrid.Children.Clear();

            int k = 0;
            for (i = 0; i < 4; i++)
                for (j = 0; j < 4; j++) 
                {
                    Wrappers[i][j] = new FigureWrapper(res[k]);

                    FiguresToTakeGrid.Children.Add(Wrappers[i][j]);

                    Grid.SetRow(Wrappers[i][j], i);
                    Grid.SetColumn(Wrappers[i][j], j);

                    Wrappers[i][j].MouseUp += FigureWrapper_MouseUp;
                    Wrappers[i][j].MouseEnter += FigureWrapper_MouseEnter;
                    Wrappers[i][j].MouseLeave += FigureWrapper_MouseLeave;

                    k++;
                }
        }

        private void InitializePlayers() {
            PlayerTurn = PlayerTurn.RED;
            movePhase = MovePhase.TAKE;
        }

        private void InitializeGameField() {
            GameField = new byte[4][];
            for (byte i = 0; i < 4; i++)
                GameField[i] = new byte[4];

            Grid GameFieldGrid = (Grid)FindName("GameFieldGrid");
            foreach (Border circleBorder in GameFieldGrid.Children) {
                circleBorder.Background = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
                circleBorder.Child = null;
                circleBorder.MouseUp += CircleBorder_MouseUp;
                circleBorder.MouseEnter += CircleBorder_MouseEnter;
                circleBorder.MouseLeave += CircleBorder_MouseLeave;
            }
        }

        private void InitializeControls() {
            Border takeFigureBorder = (Border)FindName("FigureToPlaceBorder");
            takeFigureBorder.Child = null;
        }

        private void FigureWrapper_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (movePhase == MovePhase.PLACE) return;
            // Event is utilized when movePhase == PLACE
            // Place or chosen wrappers do not have this event wired
            FigureWrapper wrapper = (FigureWrapper)sender;

            ChooseFigure(wrapper);

            AlternatePhase();
            AlternatePlayer();
        }

        private void AlternatePhase() {
            movePhase = (movePhase == MovePhase.PLACE) ? MovePhase.TAKE : MovePhase.PLACE;
        }

        private void AlternatePlayer()
        {
            PlayerTurn = (PlayerTurn == PlayerTurn.BLUE) ? PlayerTurn.RED : PlayerTurn.BLUE;
        }

        private void ChooseFigure(FigureWrapper wrapper)
        {
            Grid figuresToTakeGrid = (Grid)FindName("FiguresToTakeGrid");
            figuresToTakeGrid.Children.Remove(wrapper);

            Border figureToPlaceBorder = (Border)FindName("FigureToPlaceBorder");
            figureToPlaceBorder.Child = wrapper;

            wrapper.MouseUp -= FigureWrapper_MouseUp;
            wrapper.FigurePlacedOrChosen = true;

            wrapper.Cursor = Cursors.Arrow;
            Rectangle rec = (Rectangle)wrapper.FindName("FigureRectangle");
            rec.Stroke = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));

            byte i,j;
            for(i = 0; i < 4; i++)
                for(j = 0; j < 4; j++)
                    if (FiguresToTake[i][j] == wrapper.Figure)
                    {
                        FiguresToTake[i][j] = NO_FIGURE;
                        i = 5;
                        j = 5;
                    }
        }

        private void FigureWrapper_MouseEnter(object sender, MouseEventArgs e)
        {
            if (movePhase == MovePhase.PLACE) return;
            FigureWrapper figureWrapper = (FigureWrapper)sender;
            if (figureWrapper.FigurePlacedOrChosen) return;
            figureWrapper.Cursor = Cursors.Hand;
            Rectangle rec = (Rectangle)figureWrapper.FindName("FigureRectangle");
            rec.Stroke = (playerTurn == PlayerTurn.RED)? redPlayerBrush: bluePlayerBrush;
        }

        private void FigureWrapper_MouseLeave(object sender, MouseEventArgs e)
        {
            if (movePhase == MovePhase.PLACE) return;
            FigureWrapper figureWrapper = (FigureWrapper)sender;
            if (figureWrapper.FigurePlacedOrChosen) return;
            figureWrapper.Cursor = Cursors.Arrow;
            Rectangle rec = (Rectangle)(figureWrapper.FindName("FigureRectangle"));
            rec.Stroke = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
        }

        private void CircleBorder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (movePhase == MovePhase.TAKE) return;
            Border placeFigureBorder = (Border)sender;
            Border takeFigureBorder = (Border)FindName("FigureToPlaceBorder");

            FigureWrapper figureWrapper = (FigureWrapper)takeFigureBorder.Child;
            takeFigureBorder.Child = null;

            placeFigureBorder.Child = figureWrapper;

            placeFigureBorder.Cursor = Cursors.Arrow;
            placeFigureBorder.Background = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));

            int row = Grid.GetRow(placeFigureBorder);
            int column = Grid.GetColumn(placeFigureBorder);

            GameField[row][column] = figureWrapper.Figure;

            AlternatePhase();
        }

        private void CircleBorder_MouseEnter(object sender, MouseEventArgs e) {
            if (movePhase == MovePhase.TAKE) return;
            Border circleBorder = (Border)sender;
            if (circleBorder.Child != null)
                return;
            circleBorder.Cursor = Cursors.Hand;
            circleBorder.Background = (playerTurn == PlayerTurn.RED) ? redPlayerBrush : bluePlayerBrush;
        }

        private void CircleBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            if (movePhase == MovePhase.TAKE) return;
            Border circleBorder = (Border)sender;
            circleBorder.Cursor = Cursors.Arrow;
            circleBorder.Background = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            CreateNewGame();
        }

        private void SayQuartoButton_Click(object sender, RoutedEventArgs e)
        {
            if (LastFigurePlaced == NO_FIGURE) {
                VictoryTo(playerTurn == PlayerTurn.RED);
                return;
            }

            if (CanWinWithLastFigurePlaced())
            {
                VictoryTo(playerTurn == PlayerTurn.BLUE);
            }
            else
            {
                VictoryTo(playerTurn == PlayerTurn.RED);
            }
        }

        private bool CanWinWithLastFigurePlaced() {
            byte i, j;
            for (i = 0; i < 4; i++)
                for (j = 0; j < 4; j++)
                    if (GameField[i][j] == LastFigurePlaced) { 
                        
                    }
            throw new Exception("Figure is not found");
        }

        private void VictoryTo(bool blue)
        {
            string winningPlayer = (blue)? "Blue" : "Red";
            MessageBoxResult result = MessageBox.Show("Do you want to play again?", string.Format("{0} player wins! Congratulations!", winningPlayer), MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
                CreateNewGame();
            else
                this.Close();
        }
    }
}
