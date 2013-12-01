using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBHEngine.GameObject;

namespace MBHEngine.Trial
{
    public class TrialModeManager
    {
        public class OnTrialModeChangedMessage : MBHEngine.Behaviour.BehaviourMessage
        {
            public override void Reset()
            {
                
            }
        }

        private static TrialModeManager mInstance;

        private Boolean mIsTrialMode;

        private OnTrialModeChangedMessage mOnTrialModeChangedMsg;

        private TrialModeManager()
        {
        }

        public void Initialize()
        {
            mIsTrialMode = false;
            mOnTrialModeChangedMsg = new OnTrialModeChangedMessage();
        }

        public static TrialModeManager pInstance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new TrialModeManager();
                }

                return mInstance;
            }
        }

        public Boolean pIsTrialMode
        {
            get
            {
                return mIsTrialMode;
            }

            set
            {
                if (value != pIsTrialMode)
                {
                    mIsTrialMode = value;
                    GameObjectManager.pInstance.BroadcastMessage(mOnTrialModeChangedMsg);
                }
            }
        }
    }
}
