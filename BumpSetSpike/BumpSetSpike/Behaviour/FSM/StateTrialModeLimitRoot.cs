using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBHEngine.Behaviour;
using MBHEngine.GameObject;
using MBHEngine.Input;
using BumpSetSpike.Gameflow;
using Microsoft.Xna.Framework.Input.Touch;
using MBHEngine.Trial;

namespace BumpSetSpike.Behaviour.FSM
{
    class StateTrialModeLimitRoot : MBHEngine.StateMachine.FSMState
    {
        /// <summary>
        /// Objects managed by this state.
        /// </summary>
        private GameObject mTrialLimitReached;
        private GameObject mTrialLimitReachedBG;
        private GameObject mPurchaseButton;
        private GameObject mContinueButton;

        private FiniteStateMachine.SetStateMessage mSetStateMsg;

        /// <summary>
        /// Constructor.
        /// </summary>
        public StateTrialModeLimitRoot()
        {
            mSetStateMsg = new FiniteStateMachine.SetStateMessage();
        }

        /// <summary>
        /// Called once when the state starts.  This is a chance to do things that should only happen once
        /// during a particular state.
        /// </summary>
        public override void OnBegin()
        {
            base.OnBegin();

            mTrialLimitReached = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\TrialModeLimit\\TrialModeLimitReached\\TrialModeLimitReached");
            GameObjectManager.pInstance.Add(mTrialLimitReached);

            mTrialLimitReachedBG = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\TrialModeLimit\\TrialModeLimitReachedBG\\TrialModeLimitReachedBG");
            GameObjectManager.pInstance.Add(mTrialLimitReachedBG);

            mPurchaseButton = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\TrialModeLimit\\TrialModePurchaseButton\\TrialModePurchaseButton");
            GameObjectManager.pInstance.Add(mPurchaseButton);

            mContinueButton = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\TrialModeLimit\\TrialModeEndGameButton\\TrialModeEndGameButton");
            GameObjectManager.pInstance.Add(mContinueButton);

            GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.TRIAL_LIMIT_REACHED;
        }

        /// <summary>
        /// Called once when leaving this state.  Called the frame after the Update which returned
        /// a valid state to transition to.  This is a chance to do any clean up needed.
        /// </summary>
        public override void OnEnd()
        {
            GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.GAME_PLAY;

            if (mTrialLimitReached != null)
            {
                GameObjectManager.pInstance.Remove(mTrialLimitReached);
                mTrialLimitReached = null;
            }

            if (mTrialLimitReachedBG != null)
            {
                GameObjectManager.pInstance.Remove(mTrialLimitReachedBG);
                mTrialLimitReachedBG = null;
            }

            if (mPurchaseButton != null)
            {
                GameObjectManager.pInstance.Remove(mPurchaseButton);
                mPurchaseButton = null;
            }

            if (mContinueButton != null)
            {
                GameObjectManager.pInstance.Remove(mContinueButton);
                mContinueButton = null;
            }

            base.OnEnd();
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="msg"></param>
        public override void OnMessage(ref BehaviourMessage msg)
        {
            base.OnMessage(ref msg);

            if (msg is Button.OnButtonPressedMessage)
            {
                Button.OnButtonPressedMessage temp = (Button.OnButtonPressedMessage)msg;

                if (temp.pSender == mContinueButton)
                {
                    temp.mHandled_Out = true;

                    mSetStateMsg.Reset();
                    mSetStateMsg.mNextState_In = "StateTrialModeLimitGameplay";
                    pParentGOH.OnMessage(mSetStateMsg);
                }
            }
            else if (msg is TrialModeManager.OnTrialModeChangedMessage)
            {
                System.Diagnostics.Debug.Assert(!TrialModeManager.pInstance.pIsTrialMode, "Not expecting to reach this point and still be in Trial Mode.");

                mSetStateMsg.Reset();
                mSetStateMsg.mNextState_In = "StateEmpty";
                pParentGOH.OnMessage(mSetStateMsg);
            }
        }
    }
}
