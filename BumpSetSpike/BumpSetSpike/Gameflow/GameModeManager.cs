using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BumpSetSpike.Gameflow
{
    /// <summary>
    /// Tracks which game mode is currently active.
    /// </summary>
    class GameModeManager
    {
        /// <summary>
        /// Static instance of this singleton.
        /// </summary>
        private static GameModeManager mInstance;

        /// <summary>
        /// The different modes this game has.
        /// </summary>
        public enum GameMode
        {
            Endurance = 0,
            TrickAttack,
        }

        /// <summary>
        /// Which mode is currently active.
        /// </summary>
        private GameMode mCurrentMode;

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameModeManager()
        {
            // Pick one by default.
            mCurrentMode = GameMode.Endurance;
        }

        /// <summary>
        /// Accessor to the currnet mode.
        /// </summary>
        public GameMode pMode
        {
            get
            {
                return mCurrentMode;
            }
            set
            {
                mCurrentMode = value;
            }
        }

        /// <summary>
        /// Access to the static instance of this class.
        /// </summary>
        public static GameModeManager pInstance
        {
            get
            {
                if (null == mInstance)
                {
                    mInstance = new GameModeManager();
                }

                return mInstance;
            }
        }
    }
}
