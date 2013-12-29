using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MBHEngine.GameObject;
using MBHEngine.Debug;

namespace MBHEngine.Render
{
    /// <summary>
    /// For now we just have the camera work wrapped up into a signleton so that is can be accessed and manipulated
    /// easily from across the game.
    /// 
    /// Eventually this might need to be broken up into behaviours and attached to game objects to allow for more varying
    /// types of cameras.
    /// </summary>
    public class CameraManager
    {
        /// <summary>
        /// Static instance of the class; this is a singleton class.
        /// </summary>
        private static CameraManager mInstance = null;

        /// <summary>
        /// The current transform of the camera.
        /// </summary>
        private Matrix mTransform;

        /// <summary>
        /// The camera transform as it should be used for UI.  Essentially just the scale,
        /// without any translation or centering.
        /// </summary>
        private Matrix mTransformUI;

        /// <summary>
        /// The center of the screen, used for offsetting the matrix.
        /// </summary>
        private Matrix mScreenCenter;

        /// <summary>
        /// The position which the camera is trying to get to.
        /// </summary>
        private Vector2 mTargetPosition;

        /// <summary>
        /// The last target position successfully reached.
        /// </summary>
        private Vector2 mLastPosition;

        /// <summary>
        /// The amount of frames it takes to get from the last position to the target position.
        /// </summary>
        private Single mBlendFrames;

        /// <summary>
        /// The current number of frames that have passed while blending between the last and target transform.
        /// </summary>
        private Single mCurBlendFrames;

        /// <summary>
        /// The amount of zoom to scale the camera shot by.  1 means default zoom, higher numbers mean more zoomed in.
        /// </summary>
        private Single mZoomAmount;

        /// <summary>
        /// A zoom amount requested by a client that will be blended to over mZoomBlendFrames.
        /// </summary>
        private Single mTargetZoomAmount;

        /// <summary>
        /// The default zoom amount of the game. This can be used to restore mZoomAmount after changing.
        /// </summary>
        private Single mDefaultZoomAmount;

        /// <summary>
        /// Used to track the zoom amount at the moment mTargetZoomAmount was set, so that we can blend to
        /// the new mZoomAmount over a number of frames.
        /// </summary>
        private Single mLastZoomAmount;

        /// <summary>
        /// How many frames it should take to move from mTargetZoomAmount to mZoomAmount.
        /// </summary>
        private Single mZoomBlendFrames;

        /// <summary>
        /// How many frames have passed since mTargetZoomAmount.
        /// </summary>
        private Single mCurZoomBlendFrames;

        /// <summary>
        /// Keeps track of the viewable world space.
        /// </summary>
        private MBHEngine.Math.Rectangle mViewRectangle;

        /// <summary>
        /// Keeps track of the viewport, including scale (which isn't included in the device viewport).
        /// </summary>
        private MBHEngine.Math.Rectangle mScreenRectangle;

        /// <summary>
        /// Initialize the singleton.  Call before first use.
        /// </summary>
        /// <param name="device">The initialized graphics device.  Used to calculate screen position.</param>
        public void Initialize(GraphicsDevice device)
        {
            mTransform = Matrix.Identity;
            mTargetPosition = new Vector2(GameObjectManager.pInstance.pGraphics.PreferredBackBufferWidth * 0.5f, GameObjectManager.pInstance.pGraphics.PreferredBackBufferHeight * 0.5f);
            mLastPosition = new Vector2();
            mCurBlendFrames = 0;
            mCurZoomBlendFrames = 0;

            mBlendFrames = 10;
            mZoomBlendFrames = 0;

            mScreenCenter = Matrix.CreateTranslation(GameObjectManager.pInstance.pGraphics.PreferredBackBufferWidth * 0.5f, GameObjectManager.pInstance.pGraphics.PreferredBackBufferHeight * 0.5f, 0);
#if WINDOWS_PHONE
            mZoomAmount = 4.0f;
#elif SMALL_WINDOW
            mZoomAmount = 3.2f;
#else
            mZoomAmount = 6.4f;
#endif

            mTargetZoomAmount = mDefaultZoomAmount = mZoomAmount;

            mTransform =
                Matrix.CreateTranslation(-new Vector3(0f, 0f, 0.0f)) *
                //Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(new Vector3(mZoomAmount)) *
                mScreenCenter;

            mTransformUI = Matrix.CreateScale(new Vector3(mDefaultZoomAmount));

            mViewRectangle = new Math.Rectangle();

            // Find the center of the screen.
            Single x = ((GameObjectManager.pInstance.pGraphics.PreferredBackBufferWidth * 0.5f) / pZoomScale);
            Single y = ((GameObjectManager.pInstance.pGraphics.PreferredBackBufferHeight * 0.5f) / pZoomScale);

            // Since the screen always has 0,0 at the top left of the screen, we can get the width and height simply
            // by doubling the center point.
            Single width = x * 2;
            Single height = y * 2;

            // This rectangle should never change (unless we change resolutions).
            mScreenRectangle = new Math.Rectangle(new Vector2(width, height), new Vector2(x, y));
        }

        /// <summary>
        /// Call this once per frame to keep the camera up to date.
        /// </summary>
        /// <param name="gameTime">The amount of time that has passed this frame.</param>
        public void Update(GameTime gameTime)
        {
            mCurBlendFrames += 1;
            mCurZoomBlendFrames += 1;

            // Calculate the percent of the tween that has been completed.
            Single percent = (Single)mCurBlendFrames / (Single)mBlendFrames;

#if WINDOWS_PHONE || WINDOWS
            Vector2 curPos = Vector2.SmoothStep(mLastPosition, mTargetPosition, percent);
            curPos.X = (Single)System.Math.Round(curPos.X);
            curPos.Y = (Single)System.Math.Round(curPos.Y);
#else
            Vector2 curPos = mTargetPosition;
#endif
            // Calculate the percent of the tween that has been completed.
            Single zoom_percent = (Single)mCurZoomBlendFrames / (Single)mZoomBlendFrames;

            mZoomAmount = MathHelper.SmoothStep(mLastZoomAmount, mTargetZoomAmount, zoom_percent);

            mTransform =
                Matrix.CreateTranslation(-new Vector3(curPos, 0.0f)) * // change this to curPos to bring back blend
                //Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(new Vector3(mZoomAmount)) *
                //Matrix.CreateScale(new Vector3(1.0f, 1.0f, 1.0f)) *
                mScreenCenter;

            // Ideally this would only happen when the zoom property changes, but I am worried I will forget and
            // set it directly from within this class.
            mTransformUI = Matrix.CreateScale(new Vector3(mDefaultZoomAmount));

            // Update the view area.
            mViewRectangle.pCenterPoint = curPos;
            mViewRectangle.pDimensions = new Vector2(
                (GameObjectManager.pInstance.pGraphicsDevice.Viewport.Width) / mZoomAmount,
                (GameObjectManager.pInstance.pGraphicsDevice.Viewport.Height) / mZoomAmount);

            //DebugShapeDisplay.pInstance.AddAABB(mViewRectangle, Color.Orange);
        }

        /// <summary>
        /// Checks if a Rectangle is on screen at all.
        /// </summary>
        /// <param name="rect">The rectangle to check.</param>
        /// <returns></returns>
        public Boolean IsOnCamera(MBHEngine.Math.Rectangle rect)
        {
            return mViewRectangle.Intersects(rect);
        }

        /// <summary>
        /// Takes a 2D mouse position and transforms it into a 2D world position.
        /// </summary>
        /// <param name="mouse">The x, y of the mouse in screen space.</param>
        /// <returns>The mouse position converted into 2D world space.</returns>
        public Vector2 ProjectMouseToWorldSpace(Vector2 mouse)
        {
            // http://gamedev.stackexchange.com/questions/25692/picking-2d-objects-after-transforming-camera-in-xna
            //

            Matrix inverseViewMatrix = Matrix.Invert(pFinalTransform);
            Vector2 worldMousePosition = Vector2.Transform(mouse, inverseViewMatrix);

            return worldMousePosition;
        }

        /// <summary>
        /// Access to the single instance of the class.
        /// </summary>
        public static CameraManager pInstance
        {
            get
            {
                if(mInstance == null)
                {
                    mInstance = new CameraManager();
                }

                return mInstance;
            }
        }

        /// <summary>
        /// Returns the current transform of the camera.
        /// </summary>
        public Matrix pFinalTransform
        {
            get
            {
                return mTransform;
            }
        }

        /// <summary>
        /// Returns the current transform of the camera for use with rendering UI.  This transform
        /// will not include things like translation and screen centering.  It is pretty much just 
        /// scale.
        /// </summary>
        public Matrix pFinalTransformUI
        {
            get
            {
                return mTransformUI;
            }
        }

        /// <summary>
        /// Where the camera is trying to get to. We quickly blend between the current position
        /// to this position.
        /// </summary>
        public Vector2 pTargetPosition
        {
            get
            {
                return mTargetPosition;
            }
            set
            {
                if (value.X != mTargetPosition.X || value.Y != mTargetPosition.Y)
                {
                    // Calculate the percent of the tween that has been completed.
                    Single percent = (Single)mCurBlendFrames / (Single)mBlendFrames;
                    Vector2 curPos = Vector2.SmoothStep(mLastPosition, mTargetPosition, percent);

                    mLastPosition = curPos;
                    mTargetPosition = value;
                    mCurBlendFrames = 0;
                }
            }
        }

        /// <summary>
        /// The position in the world that is currently at the center of the screen.
        /// </summary>
        public Vector2 pScreenCenter
        {
            get
            {
                return new Vector2(mScreenCenter.Translation.X, mScreenCenter.Translation.Y);
            }
        }

        /// <summary>
        /// The amount that the camera is currently zoomed in by. This is a scalar, so 1.0 
        /// means no zoom.
        /// </summary>
        public Single pZoomScale
        {
            get
            {
                return mZoomAmount;
            }
            set
            {
                mZoomAmount = value;
            }
        }

        /// <summary>
        /// Set a zoom amount that will be blended to over pNumZoomBlendFrames.
        /// </summary>
        public Single pTargetZoomScale
        {
            set
            {
                if (mTargetZoomAmount != value)
                {
                    // Store the zoom amount that we started at so that we can blend linearly over time.
                    mLastZoomAmount = mZoomAmount;

                    // A new scale has been set so reset the timer.
                    mCurZoomBlendFrames = 0;

                    // Store the target.
                    mTargetZoomAmount = value;
                }
            }
        }

        /// <summary>
        /// Use the to reset the zoom amount.
        /// </summary>
        public Single pDefaultZoomScale
        {
            get
            {
                return mDefaultZoomAmount;
            }
        }

        /// <summary>
        /// A rectangle defining the viewable area of the world.
        /// </summary>
        public MBHEngine.Math.Rectangle pViewRect
        {
            get
            {
                return mViewRectangle;
            }
        }

        /// <summary>
        /// A rectangle defining the viewport, including scale (which isn't included in the device viewport).
        /// </summary>
        public MBHEngine.Math.Rectangle pScreenViewRect
        {
            get
            {
                return mScreenRectangle;
            }
        }

        /// <summary>
        /// How many frame should it take to blend to mTargetPosition. 
        /// </summary>
        public Single pNumBlendFrames
        {
            get
            {
                return mBlendFrames;
            }
            set
            {
                mBlendFrames = value;
            }
        }

        /// <summary>
        /// How many frames it will take to blend from the current zoom amount 
        /// to the target zoom amount.
        /// </summary>
        public Single pNumZoomBlendFrames
        {
            set
            {
                mZoomBlendFrames = value;
            }
        }
    }
}
