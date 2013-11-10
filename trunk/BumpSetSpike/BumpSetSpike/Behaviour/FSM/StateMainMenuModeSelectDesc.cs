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

namespace BumpSetSpike.Behaviour.FSM
{
    class StateMainMenuModeSelectDesc : MBHEngine.StateMachine.FSMState
    {

        /// <summary>
        /// The sound that plays when you user selects and item on the menu.
        /// </summary>
        private SoundEffect mFxMenuSelect;

        /// <summary>
        /// GameObjects this state manages.
        /// </summary>
        private GameObject mEnduranceModeBG;
        private GameObject mEnduranceModeButton;
        private GameObject mModeSelectBG;
        private GameObject mScoreAttackModeBG;
        private GameObject mScoreAttackModeButton;
        private GameObject mModeSelectTitle;
        private GameObject mModeDesc;
        private GameObject mGoButton;

        /// <summary>
        /// Preallocated to avoid GC.
        /// </summary>
        private FiniteStateMachine.SetStateMessage mSetStateMsg;
        private SpriteRender.SetColorMessage mSetColorMsg;

        /// <summary>
        /// Constructor.
        /// </summary>
        public StateMainMenuModeSelectDesc()
            : base()
        {
            mFxMenuSelect = GameObjectManager.pInstance.pContentManager.Load<SoundEffect>("Audio\\FX\\MenuSelect");

            mSetStateMsg = new FiniteStateMachine.SetStateMessage();
            mSetColorMsg = new SpriteRender.SetColorMessage();
        }

        /// <summary>
        /// Called once when the state starts.  This is a chance to do things that should only happen once
        /// during a particular state.
        /// </summary>
        public override void OnBegin()
        {
            base.OnBegin();

            mEnduranceModeBG = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\MainMenu\\ModeSelect\\EnduranceModeBG\\EnduranceModeBG");
            mEnduranceModeButton = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\MainMenu\\ModeSelect\\EnduranceModeButton\\EnduranceModeButton");
            mModeSelectBG = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\MainMenu\\ModeSelect\\ModeSelectBG\\ModeSelectBG");
            mScoreAttackModeBG = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\MainMenu\\ModeSelect\\ScoreAttackModeBG\\ScoreAttackModeBG");
            mScoreAttackModeButton = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\MainMenu\\ModeSelect\\ScoreAttackModeButton\\ScoreAttackModeButton");
            mModeSelectTitle = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\MainMenu\\ModeSelect\\ModeSelectTitle\\ModeSelectTitle");
            mGoButton = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\MainMenu\\ModeSelect\\GoButton\\GoButton");

            System.Diagnostics.Debug.Assert(GameModeManager.pInstance.pMode != GameModeManager.GameMode.None, "Game Mode is still None. It should have been set in previous state.");

            Single unselectedAlpha = 0.25f;

            if (GameModeManager.pInstance.pMode == GameModeManager.GameMode.Endurance)
            {
                mModeDesc = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\MainMenu\\ModeSelect\\EnduranceModeDesc\\EnduranceModeDesc");

                mSetColorMsg.mColor_In = new Microsoft.Xna.Framework.Color(unselectedAlpha, unselectedAlpha, unselectedAlpha, unselectedAlpha);
                mScoreAttackModeButton.OnMessage(mSetColorMsg, pParentGOH);
                mScoreAttackModeBG.OnMessage(mSetColorMsg, pParentGOH);
            }
            else if (GameModeManager.pInstance.pMode == GameModeManager.GameMode.TrickAttack)
            {
                mModeDesc = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\MainMenu\\ModeSelect\\ScoreAttackModeDesc\\ScoreAttackModeDesc");

                mSetColorMsg.mColor_In = new Microsoft.Xna.Framework.Color(unselectedAlpha, unselectedAlpha, unselectedAlpha, unselectedAlpha);
                mEnduranceModeButton.OnMessage(mSetColorMsg, pParentGOH);
                mEnduranceModeBG.OnMessage(mSetColorMsg, pParentGOH);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, "Unhandled mode type.");
            }

            GameObjectManager.pInstance.Add(mEnduranceModeBG);
            GameObjectManager.pInstance.Add(mEnduranceModeButton);
            GameObjectManager.pInstance.Add(mModeSelectBG);
            GameObjectManager.pInstance.Add(mScoreAttackModeBG);
            GameObjectManager.pInstance.Add(mScoreAttackModeButton);
            GameObjectManager.pInstance.Add(mModeSelectTitle);
            GameObjectManager.pInstance.Add(mGoButton);
            GameObjectManager.pInstance.Add(mModeDesc);
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
                return "StateMainMenuModeSelect";
            }

            return base.OnUpdate();
        }

        /// <summary>
        /// Called once when leaving this state.  Called the frame after the Update which returned
        /// a valid state to transition to.  This is a chance to do any clean up needed.
        /// </summary>
        public override void OnEnd()
        {
            GameObjectManager.pInstance.Remove(mEnduranceModeBG);
            GameObjectManager.pInstance.Remove(mEnduranceModeButton);
            GameObjectManager.pInstance.Remove(mModeSelectBG);
            GameObjectManager.pInstance.Remove(mScoreAttackModeBG);
            GameObjectManager.pInstance.Remove(mScoreAttackModeButton);
            GameObjectManager.pInstance.Remove(mModeSelectTitle);
            GameObjectManager.pInstance.Remove(mGoButton);
            GameObjectManager.pInstance.Remove(mModeDesc);

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
                if (msg.pSender == mGoButton)
                {
                    mFxMenuSelect.Play();

                    mSetStateMsg.Reset();
                    mSetStateMsg.mNextState_In = "StateMainMenuCameraPan";
                    pParentGOH.OnMessage(mSetStateMsg, pParentGOH);
                }
                else if (msg.pSender == mEnduranceModeButton && GameModeManager.pInstance.pMode != GameModeManager.GameMode.Endurance)
                {
                    mFxMenuSelect.Play();

                    GameModeManager.pInstance.pMode = GameModeManager.GameMode.Endurance;

                    // If they click a different game mode, just restart this state.
                    mSetStateMsg.Reset();
                    mSetStateMsg.mNextState_In = "StateMainMenuModeSelectDesc";
                    pParentGOH.OnMessage(mSetStateMsg, pParentGOH);
                }
                else if (msg.pSender == mScoreAttackModeButton && GameModeManager.pInstance.pMode != GameModeManager.GameMode.TrickAttack)
                {
                    mFxMenuSelect.Play();

                    GameModeManager.pInstance.pMode = GameModeManager.GameMode.TrickAttack;

                    // If they click a different game mode, just restart this state.
                    mSetStateMsg.Reset();
                    mSetStateMsg.mNextState_In = "StateMainMenuModeSelectDesc";
                    pParentGOH.OnMessage(mSetStateMsg, pParentGOH);
                }
            }
        }
    }
}
