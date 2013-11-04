using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBHEngine.GameObject;
using Microsoft.Xna.Framework;
using MBHEngine.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MBHEngine.Render;
using BumpSetSpikeContentDefs;
using MBHEngine.Behaviour;

namespace BumpSetSpike.Behaviour
{
    class ButtonMultiStage : Button
    {
        /// <summary>
        /// Preallocated to avoid GC.
        /// </summary>
        private GestureSample mGesture;

        private Int32 mCurClickCount;

        private Int32 mMaxClickCount;

        private SpriteRender.SetActiveAnimationMessage mSetActiveAnimationMsg;

        /// <summary>
        /// Constructor which also handles the process of loading in the Behaviour
        /// Definition information.
        /// </summary>
        /// <param name="parentGOH">The game object that this behaviour is attached to.</param>
        /// <param name="fileName">The file defining this behaviour.</param>
        public ButtonMultiStage(GameObject parentGOH, String fileName)
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

            ButtonMultiStageDefinition def = GameObjectManager.pInstance.pContentManager.Load<ButtonMultiStageDefinition>(fileName);

            mCurClickCount = 0;
            mMaxClickCount = def.mNumStages;

            mSetActiveAnimationMsg = new SpriteRender.SetActiveAnimationMessage();
        }

        /// <summary>
        /// See parent.
        /// </summary>
        public override Boolean HandleUIInput()
        {
            if (InputManager.pInstance.CheckGesture(GestureType.Tap, ref mGesture))
            {
                // The position is in screen space, but our screen is scaled up so we need to convert.
                // Assuming that all buttons will be in UI scale.
                Vector2 scaledPos = mGesture.Position / CameraManager.pInstance.pDefaultZoomScale;

                // Did they tap on this object?
                if (mParentGOH.pCollisionRect.Intersects(scaledPos))
                {
                    if (mCurClickCount < mMaxClickCount - 1)
                    {
                        mCurClickCount++;

                        mSetActiveAnimationMsg.Reset();

                        mSetActiveAnimationMsg.mAnimationSetName_In = mCurClickCount.ToString();
                        mSetActiveAnimationMsg.mDoNotRestartIfCompleted_In = true;
                        mParentGOH.OnMessage(mSetActiveAnimationMsg, mParentGOH);

                        return true;
                    }
                }
                else
                {
                    mCurClickCount = 0; 
                    
                    mSetActiveAnimationMsg.Reset();

                    mSetActiveAnimationMsg.mAnimationSetName_In = mCurClickCount.ToString();
                    mSetActiveAnimationMsg.mDoNotRestartIfCompleted_In = true;
                    mParentGOH.OnMessage(mSetActiveAnimationMsg, mParentGOH);
                }
            }

            return base.HandleUIInput();
        }
    }
}
