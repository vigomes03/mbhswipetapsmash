using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BumpSetSpike.Gameflow
{
    public class GameflowManager
    {
        public enum State
        {
            Undefined = -1,
            MainMenu = 0,
            GamePlay,
            Lose,
        }

        private static GameflowManager mInstance;

        private State mCurrentState;

        public GameflowManager()
        {
            mCurrentState = State.MainMenu;
        }

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
