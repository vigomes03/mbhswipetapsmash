using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBHEngine.Behaviour;
using MBHEngine.GameObject;
using MBHEngine.Input;
using BumpSetSpike.Gameflow;
using Microsoft.Xna.Framework.Input.Touch;

namespace BumpSetSpike.Behaviour.FSM
{
    class StateTrialModeLimitRoot : MBHEngine.StateMachine.FSMState
    {
        /// <summary>
        /// Objects managed by this state.
        /// </summary>
        private GameObject mTrialLimitReached;
        private GameObject mTrialLimitReachedBG;
        private GameObject mTxtTapContinue;

        /// <summary>
        /// Needed to check touch gestures.
        /// </summary>
        GestureSample mGesture;

        /// <summary>
        /// Constructor.
        /// </summary>
        public StateTrialModeLimitRoot()
        {
            mGesture = new GestureSample();
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

            mTxtTapContinue = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\Tutorial\\TapToContinue\\TapToContinue");
            GameObjectManager.pInstance.Add(mTxtTapContinue);

            GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.TRIAL_LIMIT_REACHED;
        }

        /// <summary>
        /// Called repeatedly until it returns a valid new state to transition to.
        /// </summary>
        /// <returns>Identifier of a state to transition to.  This is the same name passed into AddState 
        /// in the owning FiniteStateMachine.</returns>
        public override string OnUpdate()
        {
            // Allow them to leave the pause screen with just the back button.
            if (InputManager.pInstance.CheckAction(InputManager.InputActions.BACK, true) ||
                InputManager.pInstance.CheckGesture(GestureType.Tap, ref mGesture))
            {
                return "StateTrialModeLimitGameplay";
            }

            return base.OnUpdate();
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

            if (mTxtTapContinue != null)
            {
                GameObjectManager.pInstance.Remove(mTxtTapContinue);
                mTxtTapContinue = null;
            }

            base.OnEnd();
        }
    }
}
