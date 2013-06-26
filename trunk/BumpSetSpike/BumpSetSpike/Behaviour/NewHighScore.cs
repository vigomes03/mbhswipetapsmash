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

namespace BumpSetSpike.Behaviour
{
    /// <summary>
    /// Shows a big text image letting the player know that they have achieved a high score.
    /// </summary>
    class NewHighScore : MBHEngine.Behaviour.Behaviour
    {
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

            mGetCurrentHitCountMsg = new HitCountDisplay.GetCurrentHitCountMessage();
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

            if (mGetCurrentHitCountMsg.mCount_Out > LeaderBoardManager.pInstance.pTopHits)
            {
                mParentGOH.pDoRender = true;
            }
        }
    }
}
