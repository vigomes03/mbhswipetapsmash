using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BumpSetSpike.Gameflow
{
    /// <summary>
    /// Singleton for helping manage the state of the game.
    /// </summary>
    public class GameflowManager
    {
        /// <summary>
        /// The current state of the game.
        /// </summary>
        public enum State
        {
            Undefined = -1,
            MainMenu = 0,
            GamePlay,
            Lose,
        }

        /// <summary>
        /// Singleton.
        /// </summary>
        private static GameflowManager mInstance;

        /// <summary>
        /// The current state of the game.
        /// </summary>
        private State mCurrentState;

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameflowManager()
        {
            mCurrentState = State.MainMenu;
        }

        /// <summary>
        /// Access to the singleton.
        /// </summary>
        public static GameflowManager pInstance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new GameflowManager();
                }

                return mInstance;
            }
        }

        /// <summary>
        /// Access to the current state of the game.
        /// </summary>
        public State pState
        {
            get
            {
                return mCurrentState;
            }
            set
            {
                mCurrentState = value;
            }
        }
    }
}
