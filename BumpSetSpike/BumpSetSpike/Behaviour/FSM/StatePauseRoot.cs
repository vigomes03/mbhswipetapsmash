using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBHEngine.Behaviour;
using MBHEngine.GameObject;
using MBHEngine.Input;
using BumpSetSpike.Gameflow;
using MBHEngine.Trial;

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
        private GameObject mPurchaseButton;
        private GameObject mAchievementsButton;

        private SaveGameManager.ForceUpdateSaveDataMessage mForceUpdateSaveGameDataMsg;

        /// <summary>
        /// Called once when the state starts.  This is a chance to do things that should only happen once
        /// during a particular state.
        /// </summary>
        public override void OnBegin()
        {
            base.OnBegin();

            // Android does not support quiting to OS.
#if !__ANDROID__
            mQuitButton = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\PauseQuitButton\\PauseQuitButton");
            GameObjectManager.pInstance.Add(mQuitButton);
#endif // __ANDROID__

#if __ANDROID__
            mAchievementsButton = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\PauseAchievementsButton\\PauseAchievementsButton");
            GameObjectManager.pInstance.Add(mAchievementsButton);
#endif // __ANDROID__

            mResumeButton = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\PauseResumeButton\\PauseResumeButton");
            GameObjectManager.pInstance.Add(mResumeButton);

            mMainMenuButton = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\PauseMainMenuButton\\PauseMainMenuButton");
            GameObjectManager.pInstance.Add(mMainMenuButton);

            if (TrialModeManager.pInstance.pIsTrialMode)
            {
                mPurchaseButton = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\PauseTrialModePurchaseButton\\PauseTrialModePurchaseButton");
                GameObjectManager.pInstance.Add(mPurchaseButton);
            }

            mForceUpdateSaveGameDataMsg = new SaveGameManager.ForceUpdateSaveDataMessage();
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
            if (mPurchaseButton != null)
            {
                GameObjectManager.pInstance.Remove(mPurchaseButton);
                mPurchaseButton = null;
            }
            if (mAchievementsButton != null)
            {
                GameObjectManager.pInstance.Remove(mAchievementsButton);
                mAchievementsButton = null;
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
                    // Save the score when going back to the main menu. This will be filtered out
                    // of modes that could be exploited (eg. trick attack).
                    GameObjectManager.pInstance.BroadcastMessage(mForceUpdateSaveGameDataMsg, pParentGOH);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.MAIN_MENU;

                    TutorialManager.pInstance.StopTutorial();

                    GameObject mainMenu = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\MainMenu\\FSMMainMenu\\FSMMainMenu");
                    GameObjectManager.pInstance.Add(mainMenu);

                    GameObjectManager.pInstance.Remove(pParentGOH);
                }
                else if (msg.pSender == mAchievementsButton)
                {
#if __ANDROID__
                    BumpSetSpike_Android.Activity1 activity = Game1.Activity as BumpSetSpike_Android.Activity1;
                    //activity.StartActivityForResult(activity.pGooglePlayClient.AllLeaderboardsIntent, BumpSetSpike_Android.Activity1.REQUEST_ACHIEVEMENTS);
                    activity.StartActivityForResult(activity.pGooglePlayClient.AchievementsIntent, BumpSetSpike_Android.Activity1.REQUEST_ACHIEVEMENTS);
#endif // __ANDROID__
                }
            }
            else if (msg is TrialModeManager.OnTrialModeChangedMessage)
            {
                if (mPurchaseButton != null)
                {
                    GameObjectManager.pInstance.Remove(mPurchaseButton);
                    mPurchaseButton = null;
                }
            }
        }
    }
}
