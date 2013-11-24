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
    /// Displays the players core break down at the end of a round of Trick Attack.
    /// </summary>
    class RecentTrickDisplay : MBHEngine.Behaviour.Behaviour
    {
        /// <summary>
        /// The font object we use for rendering.
        /// </summary>
        private SpriteFont mFont;

        private String mTricks;

        /// <summary>
        /// Constructor which also handles the process of loading in the Behaviour
        /// Definition information.
        /// </summary>
        /// <param name="parentGOH">The game object that this behaviour is attached to.</param>
        /// <param name="fileName">The file defining this behaviour.</param>
        public RecentTrickDisplay(GameObject parentGOH, String fileName)
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

            // Create the font
            mFont = GameObjectManager.pInstance.pContentManager.Load<SpriteFont>("Fonts\\TrickDisplay");

            mTricks = "SPEED DEMON + JUMP ...";
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            mTricks = "";
            Int32[] comboData = ScoreManager.pInstance.pCurrentCombo;

            for (Int32 i = 0; i < (Int32)ScoreManager.ScoreType.Count; i++)
            {
                if (comboData[i] > 0)
                {
                    mTricks += ((ScoreManager.ScoreType)i).ToString() + "(" + comboData[i] + ")" + " ";
                }
            }
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="effect"></param>
        public override void Render(SpriteBatch batch, Effect effect)
        {
            base.Render(batch, effect);

            Vector2 strSize = mFont.MeasureString(mTricks);

            Vector2 pos = new Vector2(mParentGOH.pPosX - (strSize.X * 0.5f), mParentGOH.pPosY);
            Vector2 offset = new Vector2(0.0f, 1.0f);

            batch.DrawString(mFont, mTricks, pos + offset, Color.Black);
            batch.DrawString(mFont, mTricks, pos, Color.White);
        }
    }
}
