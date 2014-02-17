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
    class StateMainMenuRoot : MBHEngine.StateMachine.FSMState
    {
        /// <summary>
        /// The sound that plays when you user selects and item on the menu.
        /// </summary>
        private SoundEffect mFxMenuSelect;

        /// <summary>
        /// Objects managed by this state.
        /// </summary>
        private GameObject mTapStart;
        private GameObject mFacebookButton;
        private GameObject mIndieDBButton;
        private GameObject mCreditsButton;

        /// <summary>
        /// Preallocated to avoid GC.
        /// </summary>
        private Player.OnGameRestartMessage mGameRestartMsg;
        private HitCountDisplay.ResetGameMessage mResetGameMsg; 
        private FiniteStateMachine.SetStateMessage mSetStateMsg;

        /// <summary>
        /// Constructor.
        /// </summary>
        public StateMainMenuRoot() 
            : base()
        {
            mFxMenuSelect = GameObjectManager.pInstance.pContentManager.Load<SoundEffect>("Audio\\FX\\MenuSelect");

            mGameRestartMsg = new Player.OnGameRestartMessage();
            mResetGameMsg = new HitCountDisplay.ResetGameMessage();
            mSetStateMsg = new FiniteStateMachine.SetStateMessage();
        }

        /// <summary>
        /// Called once when the state starts.  This is a chance to do things that should only happen once
        /// during a particular state.
        /// </summary>
        public override void OnBegin()
        {
            base.OnBegin();

            GameModeManager.pInstance.pMode = GameModeManager.GameMode.None;
            GameObjectManager.pInstance.BroadcastMessage(mResetGameMsg, pParentGOH);

            CameraManager.pInstance.pTargetPosition = new Vector2(0, -100.0f); 

            mTapStart = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\TapStart\\TapStart");
            mFacebookButton = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\FacebookButton\\FacebookButton");
            mIndieDBButton = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\IndieDBButton\\IndieDBButton");
            mCreditsButton = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\CreditsButton\\CreditsButton");

            GameObjectManager.pInstance.Add(mTapStart);
            GameObjectManager.pInstance.Add(mFacebookButton);
            GameObjectManager.pInstance.Add(mIndieDBButton);
            GameObjectManager.pInstance.Add(mCreditsButton);
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
            GameObjectManager.pInstance.Remove(mTapStart);
            GameObjectManager.pInstance.Remove(mFacebookButton);
            GameObjectManager.pInstance.Remove(mIndieDBButton);
            GameObjectManager.pInstance.Remove(mCreditsButton);

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

                if (msg.pSender == mTapStart)
                {
#if __ANDROID__
                    BumpSetSpike_Android.Activity1 activity = Game1.Activity as BumpSetSpike_Android.Activity1;
                    activity.LoginToGoogle();
                    activity.pGooglePlayClient.UnlockAchievement (activity.Resources.GetString (Resource.String.achievement_the_ubi));
                    //(Game1.Activity as BumpSetSpike_Android.Activity1).StartActivityForResult((Game1.Activity as BumpSetSpike_Android.Activity1).pGooglePlayClient.AchievementsIntent, BumpSetSpike_Android.Activity1.REQUEST_ACHIEVEMENTS);
#endif //__ANDROID__

                    mSetStateMsg.Reset();
                    mSetStateMsg.mNextState_In = "StateMainMenuModeSelect";
                    pParentGOH.OnMessage(mSetStateMsg, pParentGOH);
                }
                if (msg.pSender == mCreditsButton)
                {
                    mSetStateMsg.Reset();
                    mSetStateMsg.mNextState_In = "StateMainMenuCredits";
                    pParentGOH.OnMessage(mSetStateMsg, pParentGOH);
                }
            }
        }
    }
}
