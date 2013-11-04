using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBHEngine.Behaviour;
using MBHEngine.GameObject;
using MBHEngine.Input;
using BumpSetSpike.Gameflow;

namespace BumpSetSpike.Behaviour.FSM
{
    class StatePauseRoot : MBHEngine.StateMachine.FSMState
    {
        /// <summary>
        /// Objects managed by this state.
        /// </summary>
        private GameObject mResumeButton;
        private GameObject mQuitButton;
        private GameObject mMainMenuButton;

        /// <summary>
        /// Called once when the state starts.  This is a chance to do things that should only happen once
        /// during a particular state.
        /// </summary>
        public override void OnBegin()
        {
            base.OnBegin();

            mQuitButton = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\PauseQuitButton\\PauseQuitButton");
            GameObjectManager.pInstance.Add(mQuitButton);

            mResumeButton = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\PauseResumeButton\\PauseResumeButton");
            GameObjectManager.pInstance.Add(mResumeButton);

            mMainMenuButton = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\PauseMainMenuButton\\PauseMainMenuButton");
            GameObjectManager.pInstance.Add(mMainMenuButton);
        }

        /// <summary>
        /// Called repeatedly until it returns a valid new state to transition to.
        /// </summary>
        /// <returns>Identifier of a state to transition to.  This is the same name passed into AddState 
        /// in the owning FiniteStateMachine.</returns>
        public override string OnUpdate()
        {
            // Allow them to leave the pause screen with just the back button.
            if (InputManager.pInstance.CheckAction(InputManager.InputActions.BACK, true))
            {
                GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.GAME_PLAY;
                GameObjectManager.pInstance.Remove(pParentGOH);
            }

            return base.OnUpdate();
        }

        /// <summary>
        /// Called once when leaving this state.  Called the frame after the Update which returned
        /// a valid state to transition to.  This is a chance to do any clean up needed.
        /// </summary>
        public override void OnEnd()
        {
            if (mQuitButton != null)
            {
                GameObjectManager.pInstance.Remove(mQuitButton);
                mQuitButton = null;
            }
            if (mResumeButton != null)
            {
                GameObjectManager.pInstance.Remove(mResumeButton);
                mResumeButton = null;
            }
            if (mMainMenuButton != null)
            {
                GameObjectManager.pInstance.Remove(mMainMenuButton);
                mMainMenuButton = null;
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

            if (msg is Button.OnButtonPressedMessage)
            {
                if (msg.pSender == mResumeButton)
                {
                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.GAME_PLAY;

                    GameObjectManager.pInstance.Remove(pParentGOH);
                }
                else if (msg.pSender == mQuitButton)
                {
                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.QUIT;

                    GameObjectManager.pInstance.Remove(pParentGOH);
                }
                else if (msg.pSender == mMainMenuButton)
                {
                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.MAIN_MENU;

                    TutorialManager.pInstance.StopTutorial();

                    GameObject mainMenu = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\MainMenu\\FSMMainMenu\\FSMMainMenu");
                    GameObjectManager.pInstance.Add(mainMenu);

                    GameObjectManager.pInstance.Remove(pParentGOH);
                }
            }
        }
    }
}
