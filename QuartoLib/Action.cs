using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartoLib
{
    public enum ActionType
    {
        REGULAR_MOVE = 0,
        TIE_OFFER = 1,
        SURRENDER = 2,
        QUARTO_SAYING = 3,
        ACCEPT_TIE = 4,
        DECLINE_TIE = 5
    };

    public class Action
    {
        /// <summary>
        /// Defines type of action taken by a player.
        /// </summary>
        private ActionType _actionType;
        public ActionType ActionType {
            get { return _actionType; }
            private set { _actionType = value; }
        }

        /// <summary>
        /// Defines move taken by a player.
        /// </summary>
        private Move _move;
        public Move Move
        {
            get { return _move; }
            private set { _move = value; }
        }

        /// <summary>
        /// Default state action constructor.
        /// </summary>
        public Action() {
            ActionType = ActionType.REGULAR_MOVE;
        }

        /// <summary>
        /// Parameter action constructor.
        /// </summary>
        /// <param name="move"></param>
        /// <param name="actionType"></param>
        public Action(Move move, ActionType actionType) {
            Move = move;
            ActionType = actionType;
        }
    }

}
