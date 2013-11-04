using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBHEngine.Behaviour;
using MBHEngine.GameObject;
using MBHEngine.Input;
using BumpSetSpike.Gameflow;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Audio;
using MBHEngine.Render;
using Microsoft.Xna.Framework;
using MBHEngineContentDefs;

namespace BumpSetSpike.Behaviour.FSM
{
    class StateMainMenuCredits : MBHEngine.StateMachine.FSMState
    {
        /// <summary>
        /// GameObjects managed by this state.
        /// </summary>
        private GameObject mLeaveCreditsButton;

        /// <summary>
        /// The sound that plays when you user selects and item on the menu.
        /// </summary>
        private SoundEffect mFxMenuSelect;

        /// <summary>
        /// Preallocated to avoid GC.
        /// </summary>
        private FiniteStateMachine.SetStateMessage mSetStateMsg;

        /// <summary>
        /// Constructor.
        /// </summary>
        public StateMainMenuCredits() 
            : base()
        {
            mFxMenuSelect = GameObjectManager.pInstance.pContentManager.Load<SoundEffect>("Audio\\FX\\MenuSelect");

            mSetStateMsg = new FiniteStateMachine.SetStateMessage();
        }

        /// <summary>
        /// Called once when the state starts.  This is a chance to do things that should only happen once
        /// during a particular state.
        /// </summary>
        public override void OnBegin()
        {
            base.OnBegin();

            GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.CREDITS;

            mLeaveCreditsButton = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\LeaveCreditsButton\\LeaveCreditsButton");
            GameObjectManager.pInstance.Add(mLeaveCreditsButton);
        }

        /// <summary>
        /// Called repeatedly until it returns a valid new state to transition to.
        /// </summary>
        /// <returns>Identifier of a state to transition to.  This is the same name passed into AddState 
        /// in the owning FiniteStateMachine.</returns>
        public override string OnUpdate()
        {
            if (InputManager.pInstance.CheckAction(InputManager.InputActions.BACK, true))
            {
                return "StateMainMenuRoot";
            }

            return base.OnUpdate();
        }

        /// <summary>
        /// Called once when leaving this state.  Called the frame after the Update which returned
        /// a valid state to transition to.  This is a chance to do any clean up needed.
        /// </summary>
        public override void OnEnd()
        {
            GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.MAIN_MENU;

            GameObjectManager.pInstance.Remove(mLeaveCreditsButton);

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

            if (msg is Button.OnButtonPressedMessage)
            {
                mFxMenuSelect.Play();

                if (msg.pSender == mLeaveCreditsButton)
                {
                    mSetStateMsg.Reset();
                    mSetStateMsg.mNextState_In = "StateMainMenuRoot";
                    pParentGOH.OnMessage(mSetStateMsg, pParentGOH);
                }
            }
        }
    }
}
