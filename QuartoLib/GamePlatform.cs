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

    public class GamePlatform
    {
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
        /// Defines current game state.
        /// </summary>
        private State _currentState;
        public State CurrentState
        {
            get { return _currentState; }
            public set { _currentState = value; }
        }

        /// <summary>
        /// Game initialization constructor.
        /// </summary>
        /// <param name="red"></param>
        /// <param name="blue"></param>
        public GamePlatform(IPlayer red, IPlayer blue) {
            RedPlayer = red;
            BluePlayer = blue;
            PlayerTurn = PlayerTurn.RED;
            CurrentState = new State();
        }

        /// <summary>
        /// Player alternation.
        /// </summary>
        private void AlternatePlayer()
        {
            PlayerTurn = (PlayerTurn == PlayerTurn.BLUE) ? PlayerTurn.RED : PlayerTurn.BLUE;
        }

        private void VictoryTo(bool blue)
        {
            string winningPlayer = (blue) ? "Blue" : "Red";
            MessageBoxResult result = MessageBox.Show("Do you want to play again?", string.Format("{0} player wins! Congratulations!", winningPlayer), MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
                CreateNewGame();
            else
                this.Close();
        }
    }
}
