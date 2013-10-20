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

namespace BumpSetSpike.Behaviour
{
    /// <summary>
    /// Shows a big text image letting the player know that they have achieved a high score.
    /// </summary>
    class NewHighScore : MBHEngine.Behaviour.Behaviour
    {
        /// <summary>
        /// Has the high score sound been played already.
        /// </summary>
        private Boolean mHighScoreSoundPlayed;

        /// <summary>
        /// Sound which plays when a high score is achieved.
        /// </summary>
        private SoundEffect mFxHighScore;

        /// <summary>
        /// The sound that plays if the user did not get a high score.
        /// </summary>
        private SoundEffect mFxNoHighScore;

        /// <summary>
        /// Preallocated messages to avoid GC.
        /// </summary>
        private HitCountDisplay.GetCurrentHitCountMessage mGetCurrentHitCountMsg;

        /// <summary>
        /// Constructor which also handles the process of loading in the Behaviour
        /// Definition information.
        /// </summary>
        /// <param name="parentGOH">The game object that this behaviour is attached to.</param>
        /// <param name="fileName">The file defining this behaviour.</param>
        public NewHighScore(GameObject parentGOH, String fileName)
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

            mFxHighScore = GameObjectManager.pInstance.pContentManager.Load<SoundEffect>("Audio\\FX\\HighScore");
            mFxNoHighScore = GameObjectManager.pInstance.pContentManager.Load<SoundEffect>("Audio\\FX\\GameOver");

            Reset();

            mGetCurrentHitCountMsg = new HitCountDisplay.GetCurrentHitCountMessage();
        }

        /// <summary>
        /// See parent.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            mHighScoreSoundPlayed = false;
            mParentGOH.pDoRender = false;
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            mParentGOH.pDoRender = false;

            // This thing only gets displayed if we have a new highscore. The high score
            // isn't overwritten until the game starts again, so we just compare the current
            // score the the current high score.
            GameObjectManager.pInstance.BroadcastMessage(mGetCurrentHitCountMsg, mParentGOH);

            if (GameObjectManager.pInstance.pCurUpdatePass != BehaviourDefinition.Passes.GAME_OVER_LOSS)
            {
                if (mGetCurrentHitCountMsg.mCount_Out > LeaderBoardManager.pInstance.GetCurrentModeTopScore())
                {
                    if (!mHighScoreSoundPlayed)
                    {
                        mHighScoreSoundPlayed = true;

                        mFxHighScore.Play();
                    }

                    mParentGOH.pDoRender = true;
                }
                else
                {
                    if (!mHighScoreSoundPlayed)
                    {
                        mHighScoreSoundPlayed = true;

                        mFxNoHighScore.Play();
                    }
                }
            }
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="msg"></param>
        public override void OnMessage(ref BehaviourMessage msg)
        {
            if (msg is Player.OnGameRestartMessage || msg is Player.OnMatchRestartMessage)
            {
                mHighScoreSoundPlayed = false;
            }
        }
    }
}
