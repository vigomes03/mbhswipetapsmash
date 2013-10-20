using System;
using MBHEngine.Behaviour;
using Microsoft.Xna.Framework;
using MBHEngine.GameObject;
using MBHEngine.Math;
using System.Diagnostics;
using BumpSetSpikeContentDefs;
using Microsoft.Xna.Framework.Input.Touch;
using MBHEngine.Render;
using MBHEngine.Debug;
using System.Collections.Generic;
using BumpSetSpike.Gameflow;
using MBHEngine.Input;
using MBHEngineContentDefs;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace BumpSetSpike.Behaviour
{
    /// <summary>
    /// Simple class fordisplaying the game over screen.
    /// </summary>
    class GameOver : MBHEngine.Behaviour.Behaviour
    {
        /// <summary>
        /// Preallocated to avoid GC.
        /// </summary>
        private GestureSample mGesture;

        /// <summary>
        /// The sound that plays when you user selects and item on the menu.
        /// </summary>
        private SoundEffect mFxMenuSelect;

        /// <summary>
        /// In score attack mode we need to bring up the score summany screen.
        /// </summary>
        private GameObject mScoreSummary;

        /// <summary>
        /// Object that displays the "New High Score" text.
        /// </summary>
        private GameObject mHighScore;

        /// <summary>
        /// Preallocated to avoid GC.
        /// </summary>
        private Player.OnGameRestartMessage mGameRestartMsg;
        private Player.GetCurrentStateMessage mGetCurrentStateMsg;

        /// <summary>
        /// Constructor which also handles the process of loading in the Behaviour
        /// Definition information.
        /// </summary>
        /// <param name="parentGOH">The game object that this behaviour is attached to.</param>
        /// <param name="fileName">The file defining this behaviour.</param>
        public GameOver(GameObject parentGOH, String fileName)
            : base(parentGOH, fileName)
        {
        }

        /// <summary>
        /// Call this to initialize a Behaviour with data supplied in a file.
        /// </summary>
        /// <param name="fileName">The file to load from.</param>
        public override void LoadContent(String fileName)
        {
            base.LoadContent(fileName);

            mGesture = new GestureSample();

            mFxMenuSelect = GameObjectManager.pInstance.pContentManager.Load<SoundEffect>("Audio\\FX\\MenuSelect");

            mGameRestartMsg = new Player.OnGameRestartMessage();
            mGetCurrentStateMsg = new Player.GetCurrentStateMessage();
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <returns></returns>
        public override bool HandleUIInput()
        {
            Boolean handled = false;

            if (InputManager.pInstance.CheckGesture(GestureType.Tap, ref mGesture))
            {
                #region DEBUG_COMBO_OUTPUT
                /*
                String combo = "";

                Int32[] comboData = ScoreManager.pInstance.pCurrentCombo;
                Dictionary<Int32, Int32> scoreMapping = ScoreManager.pInstance.pScoreMapping;

                for (Int32 i = 0; i < (Int32)ScoreManager.ScoreType.Count; i++)
                {
                    if (comboData[i] > 0)
                    {
                        combo += ((ScoreManager.ScoreType)i).ToString() + "(" + scoreMapping[(Int32)i] + ") x " + comboData[i] + " ";
                    }
                }

                DebugMessageDisplay.pInstance.AddConstantMessage(combo);
                */
                #endregion

                mGetCurrentStateMsg.Reset();
                GameObjectManager.pInstance.pPlayer.OnMessage(mGetCurrentStateMsg, mParentGOH);

                // Don't leave game over until the player is on the ground.
                if (mGetCurrentStateMsg.mState_Out == Player.State.Idle)
                {
                    mFxMenuSelect.Play();

                    // Restart the game
                    GameObjectManager.pInstance.BroadcastMessage(mGameRestartMsg, mParentGOH);
                    GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.GAME_PLAY;

                    // The Score Summary Behaviour removes itself from the GameObjectManager.
                    mScoreSummary = null;

                    if (mHighScore != null)
                    {
                        GameObjectManager.pInstance.Remove(mHighScore);
                        mHighScore = null;
                    }
                }

                handled = true;
            }

            return handled;
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (mScoreSummary == null && 
                GameModeManager.pInstance.pMode == GameModeManager.GameMode.TrickAttack &&
                GameObjectManager.pInstance.pCurUpdatePass != BehaviourDefinition.Passes.GAME_OVER_LOSS)
            {
                mScoreSummary = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\ScoreSummary\\ScoreSummary");

                GameObjectManager.pInstance.Add(mScoreSummary);
            }
            else if (mHighScore == null &&
                GameModeManager.pInstance.pMode == GameModeManager.GameMode.Endurance &&
                GameObjectManager.pInstance.pCurUpdatePass != BehaviourDefinition.Passes.GAME_OVER_LOSS)
            {
                mHighScore = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\NewHighScore\\NewHighScore");
                GameObjectManager.pInstance.Add(mHighScore);
            }
        }
    }
}
