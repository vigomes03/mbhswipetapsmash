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
    class ScoreSummary : MBHEngine.Behaviour.Behaviour
    {
        /// <summary>
        /// The font object we use for rendering.
        /// </summary>
        private SpriteFont mFont;

        /// <summary>
        /// Preallocated to avoid GC.
        /// </summary>
        private GestureSample mGesture;

        private GameObject mBG;

        private Int32 mNumShown;

        private Int32 mCountShown;

        private MBHEngine.Math.StopWatch mRevealTimer;

        /// <summary>
        /// Preallocated to avoid GC.
        /// </summary>
        private Player.GetCurrentStateMessage mGetCurrentStateMsg;

        /// <summary>
        /// Constructor which also handles the process of loading in the Behaviour
        /// Definition information.
        /// </summary>
        /// <param name="parentGOH">The game object that this behaviour is attached to.</param>
        /// <param name="fileName">The file defining this behaviour.</param>
        public ScoreSummary(GameObject parentGOH, String fileName)
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

            mGesture = new GestureSample();

            mGetCurrentStateMsg = new Player.GetCurrentStateMessage();
        }

        /// <summary>
        /// See parent.
        /// </summary>
        public override void OnAdd()
        {
            base.OnAdd();

            mBG = GameObjectFactory.pInstance.GetTemplate("GameObjects\\UI\\ScoreSummaryBG\\ScoreSummaryBG");
            GameObjectManager.pInstance.Add(mBG);

            mNumShown = 0;
            mCountShown = 1;

            mRevealTimer = StopWatchManager.pInstance.GetNewStopWatch();
            mRevealTimer.pLifeTime = 15;
        }

        /// <summary>
        /// See parent.
        /// </summary>
        public override void OnRemove()
        {
            base.OnRemove();

            GameObjectManager.pInstance.Remove(mBG);
            mBG = null;

            StopWatchManager.pInstance.RecycleStopWatch(mRevealTimer);
            mRevealTimer = null;
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (mRevealTimer.IsExpired())
            {
                mNumShown++;

                mCountShown = 1;

                mRevealTimer.Restart();
            }
        }

        public override bool HandleUIInput()
        {
            Boolean handled = false;

            if (InputManager.pInstance.CheckGesture(GestureType.Tap, ref mGesture))
            {
                Int32 numMoves = GetNumMoves();

                if (mNumShown < numMoves)
                {
                    mNumShown = numMoves;

                    handled = true;
                }
                else
                {
                    GameObjectManager.pInstance.Remove(mParentGOH);

                    handled = true;
                }
            }

            return handled;
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="effect"></param>
        public override void Render(SpriteBatch batch, Effect effect)
        {
            base.Render(batch, effect);

            Int32[] comboData = ScoreManager.pInstance.pCurrentCombo;
            Dictionary<Int32, Int32> scoreMapping = ScoreManager.pInstance.pScoreMapping;
            MBHEngine.Math.Rectangle screenRect = CameraManager.pInstance.pScreenViewRect;

            Int32 count = 0;
            Single yStart = 10.0f;

            {
                string str = "Summary";

                Vector2 strSize = mFont.MeasureString(str);
                Vector2 pos = new Vector2(screenRect.pCenterPoint.X - strSize.X * 0.5f, 0);
                Vector2 posShadow = new Vector2(pos.X, pos.Y + 1);

                batch.DrawString(mFont, str, posShadow, Color.Purple);
                batch.DrawString(mFont, str, pos, Color.White);
            }

            for (Int32 i = 0; i < (Int32)ScoreManager.ScoreType.Count; i++)
            {
                if (comboData[i] > 0 && count <= mNumShown)
                {
                    string str = ((ScoreManager.ScoreType)i).ToString();
                    Vector2 strSize = mFont.MeasureString(str);
                    Single vertSpace = 9.0f;
                    Single horzOffSet = 14.0f;

                    Vector2 pos = new Vector2(screenRect.pCenterPoint.X - horzOffSet - strSize.X, vertSpace * count + yStart);
                    Vector2 posShadow = new Vector2(pos.X, pos.Y + 1);

                    batch.DrawString(mFont, str, posShadow, Color.Black);
                    batch.DrawString(mFont, str, pos, Color.White);

                    ///

                    Int32 scoreMap = scoreMapping[(Int32)i];

                    str = scoreMap.ToString();
                    strSize = mFont.MeasureString(str);

                    pos = new Vector2(screenRect.pCenterPoint.X - (strSize.X * 0.5f), vertSpace * count + yStart);
                    posShadow = new Vector2(pos.X, pos.Y + 1);

                    batch.DrawString(mFont, str, posShadow, Color.Black);
                    batch.DrawString(mFont, str, pos, Color.White);

                    Int32 hitCount = comboData[i];

                    if (count == mNumShown)
                    {
                        hitCount = Math.Min(mCountShown, hitCount);
                        mCountShown++;
                    }

                    ///

                    str = "x " + hitCount;

                    pos = new Vector2(screenRect.pCenterPoint.X + horzOffSet, vertSpace * count + yStart);
                    posShadow = new Vector2(pos.X, pos.Y + 1);

                    batch.DrawString(mFont, str, posShadow, Color.Black);
                    batch.DrawString(mFont, str, pos, Color.White);

                    ///

                    str = (hitCount * scoreMap).ToString();

                    pos = new Vector2(screenRect.pCenterPoint.X + horzOffSet + 30, vertSpace * count + yStart);
                    posShadow = new Vector2(pos.X, pos.Y + 1);

                    batch.DrawString(mFont, str, posShadow, Color.Black);
                    batch.DrawString(mFont, str, pos, Color.White);

                    count++;
                }
            }

            if (mNumShown >= GetNumMoves())
            {
                string str = "Total";
                Vector2 strSize = mFont.MeasureString(str);
                Single vertSpace = 9.0f;
                Single horzOffSet = 14.0f;
                Single vertOffSet = 4.0f;

                Vector2 pos = new Vector2(screenRect.pCenterPoint.X - horzOffSet - strSize.X, vertSpace * count + vertOffSet + yStart);
                Vector2 posShadow = new Vector2(pos.X, pos.Y + 1);

                batch.DrawString(mFont, str, posShadow, Color.Purple);
                batch.DrawString(mFont, str, pos, Color.White);

                ///

                str = ScoreManager.pInstance.CalcScore().ToString();

                pos = new Vector2(screenRect.pCenterPoint.X + horzOffSet + 30, vertSpace * count + vertOffSet + yStart);
                posShadow = new Vector2(pos.X, pos.Y + 1);

                batch.DrawString(mFont, str, posShadow, Color.Purple);
                batch.DrawString(mFont, str, pos, Color.White);
            }
        }

        private Int32 GetNumMoves()
        {
            Int32 count = 0;
            Int32[] comboData = ScoreManager.pInstance.pCurrentCombo;

            for (Int32 i = 0; i < (Int32)ScoreManager.ScoreType.Count; i++)
            {
                if (comboData[i] > 0)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
