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
    class StateTrialModeLimitGameplay : MBHEngine.StateMachine.FSMState
    {
        /// <summary>
        /// Objects managed by this state.
        /// </summary>
        private GameObject mTrialLimitReached;

        /// <summary>
        /// Preallocated messages.
        /// </summary>
        private FiniteStateMachine.SetStateMessage mSetStateMsg;

        /// <summary>
        /// Constructor.
        /// </summary>
        public StateTrialModeLimitGameplay()
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

            mTrialLimitReached = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\TrialModeLimit\\TrialModeInputDisabled\\TrialModeInputDisabled");
            GameObjectManager.pInstance.Add(mTrialLimitReached);
        }

        /// <summary>
        /// Called repeatedly until it returns a valid new state to transition to.
        /// </summary>
        /// <returns>Identifier of a state to transition to.  This is the same name passed into AddState 
        /// in the owning FiniteStateMachine.</returns>
        public override string OnUpdate()
        {
            return base.OnUpdate();
        }

        /// <summary>
        /// Called once when leaving this state.  Called the frame after the Update which returned
        /// a valid state to transition to.  This is a chance to do any clean up needed.
        /// </summary>
        public override void OnEnd()
        {
            if (mTrialLimitReached != null)
            {
                GameObjectManager.pInstance.Remove(mTrialLimitReached);
                mTrialLimitReached = null;
            }

            base.OnEnd();
        }

        /// <summary>
        /// The main interface for communicating between behaviours.  Using polymorphism, we
        /// define a bunch of different messages deriving from BehaviourMessage.  Each behaviour
        /// can then check for particular upcasted messahe types, and either grab some data 
        /// from it (set message) or store some data in it (get message).
        /// </summary>
        /// <param name="msg">The message being communicated to the behaviour.</param>
        public override void OnMessage(ref BehaviourMessage msg)
        {
            base.OnMessage(ref msg);

            if (msg is Player.OnGameRestartMessage || msg is Player.OnMatchRestartMessage)
            {
                mSetStateMsg.Reset();
                mSetStateMsg.mNextState_In = "StateEmpty";
                pParentGOH.OnMessage(mSetStateMsg);
            }
        }
    }
}
