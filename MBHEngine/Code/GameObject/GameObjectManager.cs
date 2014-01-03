using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MBHEngine.Debug;
using MBHEngineContentDefs;
using MBHEngine.Render;
using MBHEngine.Behaviour;

namespace MBHEngine.GameObject
{
    /// <summary>
    /// Singleton class which manages all the GameObject instances of the engine.
    /// By calling update and Render on this class, all the GameObjects it contains
    /// will be updated and rendered.
    /// </summary>
    public class GameObjectManager : IComparer<GameObject>
    {
        /// <summary>
        /// Tracks what is going on in Update() so that different callback functions can
        /// act differently depending on the UpdatePhase.
        /// </summary>
        private enum UpdatePhase
        {
            None = 0,
            Update,
            Remove,
            Add,
            OnRemove,
            OnAdd,
        }

        /// <summary>
        /// Classes can implement this interface and register with RegisterUpdatePassChangeReceiver to 
        /// be notified with the Update Pass changes.
        /// </summary>
        public abstract class IUpdatePassChangeReciever
        {
            public abstract void OnStateChange(BehaviourDefinition.Passes newState, BehaviourDefinition.Passes oldState);
        }

        /// <summary>
        /// The static instance of this class, making this a singleton.
        /// </summary>
        static private GameObjectManager mInstance = null;

        /// <summary>
        /// A list of all the GameObjects in the game.  The main purpose
        /// of this class is to manage this list.
        /// </summary>
        private List<GameObject> mGameObjects;

        /// <summary>
        /// A subset of mGameObjects holding references to all of the objects that are not
        /// flaged as being static.
        /// </summary>
        private List<GameObject> mDynamicGameObjects;

        /// <summary>
        /// A subset of mGameObjects holding references to all of the objects that are
        /// flaged as being static. This 2D array is index by the X,Y position of the 
        /// object scaled by mCellSize.
        /// </summary>
        private List<GameObject> [,] mStaticGameObjects;

        /// <summary>
        /// A list of all game object which were defined to have a Classifications. This allows
        /// for much quicker look up of all GameObjects of a particular type.
        /// </summary>
        private Dictionary<Int32, List<GameObject>> mGameObjectsByClassification;

        /// <summary>
        /// A list of all the GameObjects that need to be added at the next possible
        /// time.  We can't just add the objects right away because we could be 
        /// interating through the list at the time.
        /// </summary>
        private List<GameObject> mGameObjectsToAdd;

        /// <summary>
        /// A list of all the GameObjects that need to be removed at the next possible
        /// time.  We can't just remove the objects right away because we could be 
        /// interating through the list at the time.
        /// </summary>
        private List<GameObject> mGameObjectsToRemove;

        /// <summary>
        /// If a client attempts to add an object while we are in the middle of adding objects, it needs to be
        /// queued up for next frame in a different list, since mGameObjectsToAdd will be cleared at the end
        /// of the frame.
        /// </summary>
        private List<GameObject> mGameObjectsToAddNextFrame;

        /// <summary>
        /// If a client attempts to remove an object while we are in the middle of removing objects, it needs to be
        /// queued up for next frame in a different list, since mGameObjectsToRemove will be cleared at the end
        /// of the frame.
        /// </summary>
        private List<GameObject> mGameObjectsToRemoveNextFrame;

        /// <summary>
        /// Holds on to an instance of the ContentManager so that we can load new objects
        /// on the fly.
        ///
        /// This probably shouldn't be here.  For one, we probably want to preload most of
        /// our content.  In the cases where we don't we should probably have some sort of
        /// ContentManager...Manager.  That feels a little weird right now, so I am just 
        /// putting this here for now, and we can re-examine this concent once things are a
        /// little more flushed out.
        /// </summary>
        private ContentManager mContent;

        /// <summary>
        /// Similar to the ContentManager.  Store this here for conventience.
        /// </summary>
        private GraphicsDeviceManager mGraphics;

        /// <summary>
        /// Special case handling for the player object.  This is just to make things easier and quicker.
        /// </summary>
        private GameObject mPlayer;

        /// <summary>
        /// Reder state for rendering clear, crisp sprites.
        /// </summary>
        private RasterizerState mSpriteRasterState; // Prevent the edge of the sprite showing garabage.
        private SamplerState mSpriteSamplerState; // Keep the sprites looking Crisp.

        /// <summary>
        /// This will defined as a multiply blend state.
        /// </summary>
        private BlendState mMultiply;

        /// <summary>
        /// The current update pass.
        /// </summary>
        private BehaviourDefinition.Passes mCurrentUpdatePass;

        /// <summary>
        /// The size of a single cell. Cells are square.
        /// </summary>
        private Int32 mCellSize;

        /// <summary>
        /// The number of cells in a single row. Assumes a square board.
        /// </summary>
        private Int32 mNumCells;

        /// <summary>
        /// The one and only Effect this game uses. Right now this is all very hard coded, and there
        /// should be some way to control it from Game side.
        /// </summary>
        private Effect mDefaultEffect;

        /// <summary>
        /// Tracks which phase of Update we are in so that we can detect errors like trying to remove
        /// objects while in the Remove phase.
        /// </summary>
        private UpdatePhase mCurrentUpdateState;

        /// <summary>
        /// Tracks the number of objects that were rendered last frame. Used for debug display which
        /// must happen outside the Render loop.
        /// </summary>
        private Int32 mLastNumObjectsRendered;

        /// <summary>
        /// List of objects that wish to be notified when the GameObjectManager Update Pass changes.
        /// </summary>
        private List<IUpdatePassChangeReciever> mUpdatePassChangeReceivers;

        /// <summary>
        /// We make the constructor private so that no one accidentally creates
        /// an instance of the class.
        /// </summary>
        private GameObjectManager()
        {

        }

        /// <summary>
        /// Needs to be called before any of the usage methods are called.
        /// </summary>
        /// <param name="content">GameObjects will load into this conent manager.</param>
        /// <param name="graphics">Used for rendering.</param>
        public void Initialize(ContentManager content, GraphicsDeviceManager graphics)
        {
            mGameObjects = new List<GameObject>(128);
            mDynamicGameObjects = new List<GameObject>(128);
            mGameObjectsByClassification = new Dictionary<Int32, List<GameObject>>(128);
            Int32 numEnum = (Int32)GameObjectDefinition.Classifications.COUNT;
            for (Int32 i = 0; i < numEnum; i++)
            {
                mGameObjectsByClassification[i] = new List<GameObject>();
            }
            // Note: mStaticGameObject is allocated in OnMapInfoChange.
            mGameObjectsToAdd = new List<GameObject>(32);
            mGameObjectsToRemove = new List<GameObject>(32);
            mGameObjectsToAddNextFrame = new List<GameObject>(32);
            mGameObjectsToRemoveNextFrame = new List<GameObject>(32);

            mContent = content;
            mGraphics = graphics;

            mSpriteRasterState = new RasterizerState();
            mSpriteSamplerState = new SamplerState();

            // Prevent the edge of the sprite showing garabage.
            mSpriteRasterState.MultiSampleAntiAlias = false;
            mSpriteRasterState.CullMode = CullMode.None;

            // Keep the sprites looking Crisp.
            mSpriteSamplerState.Filter = TextureFilter.Point;

            mMultiply = new BlendState();
            mMultiply.ColorSourceBlend = Blend.Zero;
            mMultiply.ColorDestinationBlend = Blend.SourceColor;

#if !WINDOWS_PHONE && !MONOGL && !__ANDROID__
            // For now just hard code it. Eventually this should be driven by the Game side.
            mDefaultEffect = content.Load<Effect>("Shaders\\Default");
#endif
            mCurrentUpdatePass = BehaviourDefinition.Passes.MAIN_MENU;

            // Just an arbitrary choice.
            mCellSize = 100;

            mCurrentUpdateState = UpdatePhase.None;

            mUpdatePassChangeReceivers = new List<IUpdatePassChangeReciever>();
        }

        /// <summary>
        /// Implementation of the IComparer interface. 
        /// </summary>
        /// <remarks>
        /// We implemented this interface (rather than using delegates) because using a delegate
        /// (eg. passinging the sort fuction itself to List.Sort) causes memory allocations which
        /// will eventually trigger the GC.
        /// </remarks>
        /// <param name="x">Left side.</param>
        /// <param name="y">Right side.</param>
        /// <returns>0 The objects have equal render priority.</returns>
        /// <returns>-1 x has a higher render priority than y.</returns>
        /// <returns>1 y has a higher render priority than x.</returns>
        public int Compare(GameObject x, GameObject y)
        {
            return CompareByRenderPriority(x, y);
        }

        /// <summary>
        /// Used to sort the game objects based on render priority.  We want the higher
        /// priority (which is a smaller number) to appear later on the list.
        /// This implementation also sorts top to bottom in screen space.  This is for 
        /// top down view points.
        /// </summary>
        /// <param name="x">Left side.</param>
        /// <param name="y">Right side.</param>
        /// <returns>0 The objects have equal render priority.</returns>
        /// <returns>-1 x has a higher render priority than y.</returns>
        /// <returns>1 y has a higher render priority than x.</returns>
        private int CompareByRenderPriority(GameObject x, GameObject y)
        {
            // Is x allocated?
            if (x == null)
            {
                if (y == null)
                {
                    // Y also is not allocated, so they are considered equal
                    return 0;
                }
                else
                {
                    // Y is not null but x is, so that means Y is considered greater.
                    return 1;
                }
            }
            else if (y == null)
            {
                // X is not null by y is, so x is greater.
                return -1;
            }
            else
            {
                if (x.pRenderPriority == y.pRenderPriority)
                {
                    // The have the same render priority, so now we should check who is
                    // closer to the bottom of the screen and render them on top of the
                    // other.

                    if (x.pPosition.Y < y.pPosition.Y)
                    {
                        // Y is closer to the bottom.
                        return -1;
                    }
                    else if (x.pPosition.Y > y.pPosition.Y)
                    {
                        // X is closer to the bottom.
                        return 1;
                    }
                    else
                    {
                        // The are at the exact same position and have the same render priority.
                        return 0;
                    }
                }
                else if (x.pRenderPriority > y.pRenderPriority)
                {
                    // Y is greater
                    return 1;
                }
                else
                {
                    // X is greater
                    return -1;
                }
            }
        }

        /// <summary>
        /// Should be called once per frame.
        /// </summary>
        /// <param name="gameTime">The amount of time passed since last update.</param>
        public void Update(GameTime gameTime)
        {
#if DEBUG
            // Draw cell boundaries.
            //

            Int32 size = mNumCells * mCellSize;

            for (Int32 y = 0; y < mNumCells; y++)
            {
                DebugShapeDisplay.pInstance.AddSegment(new Vector2(0, y * mCellSize), new Vector2(size, y * mCellSize), Color.Black);
            }

            for (Int32 x = 0; x < mNumCells; x++)
            {
                DebugShapeDisplay.pInstance.AddSegment(new Vector2(x * mCellSize, 0), new Vector2(x * mCellSize, size), Color.Black);
            }
#endif
            DebugMessageDisplay.pInstance.AddDynamicMessage("Objects Rendered: " + mLastNumObjectsRendered);

            mCurrentUpdateState = UpdatePhase.Update;

            // Keep track of how many objects were updated this frame.
            int count = 0;

            // Some behaviours require some logic to be done prior to the standard update.  This is their chance.
            //
            for (int i = 0; i < mGameObjects.Count; i++)
            {
                if (mGameObjects[i].pDoUpdate)
                {
                    mGameObjects[i].PreUpdate(gameTime);
                }
            }

            // Update every object we are managing.
            //
            for (int i = 0; i < mGameObjects.Count; i++)
            {
                if (mGameObjects[i].pDoUpdate)
                {
                    mGameObjects[i].Update(gameTime);
                }

                count++;
            }

            // Allow UI objects to handle input in an ordered way. In this case we go from front to
            // back, since that is what the user will expect when clicking things.
            //
            for (int i = mGameObjects.Count - 1; i >= 0; --i)
            {
                // Input handling follow the same rules as updates since traditionally
                // input is handled in Update().
                if (mGameObjects[i].pDoUpdate)
                {
                    if (mGameObjects[i].HandleUIInput())
                    {
                        break;
                    }
                }

                count++;
            }

            // A final chance to update behaviours after all the updates have been completed.
            //
            for (int i = 0; i < mGameObjects.Count; i++)
            {
                if (mGameObjects[i].pDoUpdate)
                {
                    mGameObjects[i].PostUpdate(gameTime);
                }
            }

            DebugMessageDisplay.pInstance.AddDynamicMessage("Updated " + count + " GameObjects");

            mCurrentUpdateState = UpdatePhase.Remove;

            // Now loop through our list of objects which need to be removed and
            // remove them.
            //
            for (int i = 0; i < mGameObjectsToRemove.Count; i++)
            {
                // If this is a GameObject that was spawned through a Factory then it needs to be
                // returned to that Factory.
                if (mGameObjectsToRemove[i].pFactoryInfo.pIsManaged)
                {
                    GameObjectFactory.pInstance.RecycleTemplate(mGameObjectsToRemove[i]);
                }

                // See if this is also going to be referenced in the dynamic objects list.
                if (!mGameObjectsToRemove[i].pIsStatic)
                {
                    mDynamicGameObjects.Remove(mGameObjectsToRemove[i]);
                }

                // See if this is going to be reference in the static objects list.
                if (mGameObjectsToRemove[i].pIsStatic)
                {
                    // Figure out which cell this object would be in.
                    Vector2 index = CellIndexFromPosition(mGameObjectsToRemove[i].pPosition);

                    // Remove it from the cell it should be in.
                    mStaticGameObjects[(Int32)index.X, (Int32)index.Y].Remove(mGameObjectsToRemove[i]);
                }

                for (Int32 tag = 0; tag < mGameObjectsToRemove[i].pClassifications.Count; tag++)
                {
                    mGameObjectsByClassification[tag].Remove(mGameObjectsToRemove[i]);
                }

                // What happens if someone adds and removes an element within the same
                // update?  It would mean we are about to remove an item that hasn't
                // actually been added yet!  To get around this flaw, we will attempt to
                // remove the item from the main list and if that fails, try to remove it 
                // from the list of items about to be added.
                if (mGameObjectsToRemove[i] != null && mGameObjects.Remove(mGameObjectsToRemove[i]) == false)
                {
                    if (mGameObjectsToAdd.Remove(mGameObjectsToRemove[i]) == false)
                    {
                        //System.Diagnostics.Debug.Assert(false, "Attempting to remove a game object which isn't in any of the managed lists.");
                    }
                }
            }

            mCurrentUpdateState = UpdatePhase.Add;

            // Loop through all the game objects that exist.  We want to insert the new game objects
            // in the order that they were added, based on render priority.  If the new object shares
            // a render priority with another object, it is inserted in front of the first same-priority
            // object it hits.
            //
            // This bit of code assumes that the mGameObjectsToAdd and mGameObjects are both sorted 
            // based on render priority.
            //
            int curIndex = 0;
            for (int i = 0; i < mGameObjectsToAdd.Count; i++)
            {
                // Check if this is a dynamic object which isn't already being managed by this list.
                if (!mGameObjectsToAdd[i].pIsStatic)
                {
                    System.Diagnostics.Debug.Assert(!mDynamicGameObjects.Contains(mGameObjectsToAdd[i]), "Attempting to add GameObject already in mDynamicGameObjects.");

                    mDynamicGameObjects.Add(mGameObjectsToAdd[i]);
                }

                // Has this object been flagged as being static?
                if (mGameObjectsToAdd[i].pIsStatic)
                {
                    // Figure out which cell this object would be in.
                    Vector2 index = CellIndexFromPosition(mGameObjectsToAdd[i].pPosition);

                    System.Diagnostics.Debug.Assert(!mStaticGameObjects[(Int32)index.X, (Int32)index.Y].Contains(mGameObjectsToAdd[i]), "Attempting to add GameObject already in mStaticGameObjects.");

                    mStaticGameObjects[(Int32)index.X, (Int32)index.Y].Add(mGameObjectsToAdd[i]);
                }

                for (Int32 tagIndex = 0; tagIndex < mGameObjectsToAdd[i].pClassifications.Count; tagIndex++)
                {
                    GameObjectDefinition.Classifications tag = mGameObjectsToAdd[i].pClassifications[tagIndex];

                    mGameObjectsByClassification[(Int32)tag].Add(mGameObjectsToAdd[i]);
                }

                // If this game object is already in the list, don't add it again.
                if (!mGameObjects.Contains(mGameObjectsToAdd[i]))
                {
                    bool alreadyAdded = false;

                    // Loop through all the currently exisiting game objects.  We continue moving
                    // forward even after inserting a new object.  This can be done because we assume
                    // the mGameObjectsToAdd is also sorted by render priority, which means the next
                    // element must be placed somewhere after the current one.
                    for (; curIndex < mGameObjects.Count; curIndex++)
                    {
                        if (mGameObjectsToAdd[i].pRenderPriority < mGameObjects[curIndex].pRenderPriority)
                        {
                            // We have found the proper place for this element.
                            mGameObjects.Insert(curIndex, mGameObjectsToAdd[i]);

                            // We don't want to test against the elemt we just added.  Since it was
                            // inserted at i, the object we just compared against is actually at i + 1
                            // now.  Let's start the next comparison there.
                            curIndex++;

                            alreadyAdded = true;

                            break;
                        }
                    }

                    if (!alreadyAdded)
                    {
                        // If we make it to this point all the remaining elements have a greater or equal
                        // render priority to the highest priority item already existing.
                        // This will also take care of the cases where this is the first item being added.
                        mGameObjects.Add(mGameObjectsToAdd[i]);

                        // We don't want to test against the element we just added.  Since it was
                        // inserted at i, the object we just compared against is actually at i + 1
                        // now.  Let's start the next comparison there.
                        curIndex++;
                    }
                }
            }

            mCurrentUpdateState = UpdatePhase.OnRemove;

            if (mGameObjectsToRemove.Count != 0)
            {
                // At this point all objects for the frame have been removed from
                // the GameObjectManager. This is an ideal time to give objects
                // a chance to do some cleanup which requires objects to 
                // be removed (eg. Delete stuff).
                for (Int32 i = 0; i < mGameObjectsToRemove.Count; i++)
                {
                    mGameObjectsToRemove[i].OnRemove();
                }

                mGameObjectsToRemove.Clear();
            }

            mCurrentUpdateState = UpdatePhase.OnAdd;

            if (mGameObjectsToAdd.Count != 0)
            {
                // At this point all objects for the frame have been added to
				// the GameObjectManager. This is an ideal time to give objects
				// a chance to do some initization which requires objects to 
				// be added (eg. BroadCastMessage).
                for (Int32 i = 0; i < mGameObjectsToAdd.Count; i++)
                {
                    mGameObjectsToAdd[i].OnAdd();
                }

                mGameObjectsToAdd.Clear();
            }
			
            mCurrentUpdateState = UpdatePhase.None;

            if (mGameObjectsToRemoveNextFrame.Count > 0)
            {
                mGameObjectsToRemove.AddRange(mGameObjectsToRemoveNextFrame);

                mGameObjectsToRemoveNextFrame.Clear();
            }

            if (mGameObjectsToAddNextFrame.Count > 0)
            {
                mGameObjectsToAdd.AddRange(mGameObjectsToAddNextFrame);

                mGameObjectsToAddNextFrame.Clear();
            }
        }

        /// <summary>
        /// Should be called once per render update.
        /// </summary>
        /// <param name="batch">Where sprites will be rendered to.</param>
        /// <param name="showDebugInfo">True if debug information should be shown this frame.</param>
        public void Render(SpriteBatch batch, Boolean showDebugInfo)
        {
            mGameObjects.Sort(this);

            // Keep track of the blend modes so that we can detect when it needs to change.
            GameObjectDefinition.BlendMode currentBlend = GameObjectDefinition.BlendMode.UNDEFINED;

            mLastNumObjectsRendered = 0;

            for (int i = 0; i < mGameObjects.Count; i++)
            {
                GameObject obj = mGameObjects[i];

                GameObjectDefinition.BlendMode blend = obj.pBlendMode;

                if (obj.pDoRender == true)
                {
                    if (obj.pCollisionRect.pDimensions == Vector2.Zero || 
                        blend == GameObjectDefinition.BlendMode.STANDARD_UI || 
                        blend == GameObjectDefinition.BlendMode.MULTIPLY_UI ||
                        CameraManager.pInstance.IsOnCamera(obj.pRenderRect))
                    {
                        // Has the blend mode changed from the previous game object to this one?
                        if (currentBlend != blend)
                        {
                            if (blend == GameObjectDefinition.BlendMode.UNDEFINED)
                            {
                                System.Diagnostics.Debug.Assert(false, "Attempting to rendering Game Object with UNDEFINED blend mode.");
                            }

                            // Did the last game object set the blend mode, meaning we have to end it?
                            // This is true for the first game object being rendered.
                            if (currentBlend != GameObjectDefinition.BlendMode.UNDEFINED)
                            {
                                batch.End();
                            }

                            // Update the current blend mode so that we can tell when it changes.
                            currentBlend = blend;

                            if (blend == GameObjectDefinition.BlendMode.STANDARD)
                            {
#if WINDOWS_PHONE || MONOGL || __ANDROID__
                                
                                batch.Begin(SpriteSortMode.Deferred, 
                                    BlendState.AlphaBlend,
                                    mSpriteSamplerState,
                                    null,
                                    mSpriteRasterState,
                                    null,
                                    CameraManager.pInstance.pFinalTransform);
#else
                                batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, mDefaultEffect, CameraManager.pInstance.pFinalTransform);

                                // Keep the sprites looking crisp.
                                batch.GraphicsDevice.SamplerStates[0] = mSpriteSamplerState;
                                batch.GraphicsDevice.RasterizerState = mSpriteRasterState;
#endif
                            }
                            else if (blend == GameObjectDefinition.BlendMode.TEST)
                            {
                                // Keeping the TEST blend mode around because it will be handy for trying out new stuff, but
                                // in the mean time we want to catch GameObjects using it by accident.
                                System.Diagnostics.Debug.Assert(false, "BlendMode.TEST in use but not currently implemented.");
                            }
                            else if (blend == GameObjectDefinition.BlendMode.MULTIPLY)
                            {
                                batch.Begin(SpriteSortMode.Immediate, mMultiply, null, null, null, mDefaultEffect, CameraManager.pInstance.pFinalTransform);

                                // Keep the sprites looking crisp.
                                batch.GraphicsDevice.SamplerStates[0] = mSpriteSamplerState;
                                batch.GraphicsDevice.RasterizerState = mSpriteRasterState;
                            }
                            else if (blend == GameObjectDefinition.BlendMode.MULTIPLY_UI)
                            {
                                // Use the Multiply blend mode but ignore the camera, so that it renders in screen
                                // space.
                                batch.Begin(SpriteSortMode.Immediate, mMultiply, null, null, null, mDefaultEffect);

                                // Keep the sprites looking crisp.
                                batch.GraphicsDevice.SamplerStates[0] = mSpriteSamplerState;
                                batch.GraphicsDevice.RasterizerState = mSpriteRasterState;
                            }
                            else if (blend == GameObjectDefinition.BlendMode.STANDARD_UI)
                            {
#if WINDOWS_PHONE || MONOGL || __ANDROID__
                                batch.Begin(SpriteSortMode.Deferred, 
                                    BlendState.AlphaBlend,
                                    mSpriteSamplerState,
                                    null,
                                    null,
                                    null,
                                    CameraManager.pInstance.pFinalTransformUI);
#else
                                batch.Begin(
                                    SpriteSortMode.Immediate,
                                    BlendState.NonPremultiplied,
                                    null, null, null, mDefaultEffect,
                                    CameraManager.pInstance.pFinalTransformUI);

                                // Keep the sprites looking crisp.
                                batch.GraphicsDevice.SamplerStates[0] = mSpriteSamplerState;
                                batch.GraphicsDevice.RasterizerState = mSpriteRasterState;
#endif
                            }
                            else if (blend != GameObjectDefinition.BlendMode.UNDEFINED)
                            {
                                System.Diagnostics.Debug.Assert(false, "Unhandled blend mode.");
                            }
                        }

                        obj.Render(batch, mDefaultEffect);

                        mLastNumObjectsRendered++;

                        if (showDebugInfo)
                        {
                            DebugShapeDisplay.pInstance.AddAABB(obj.pCollisionRect, Color.Green, true);
                            DebugShapeDisplay.pInstance.AddTransform(obj.pPosition);
                        }
                    }
                }
            }

            // Prevent crash when batch.Begin was not called. Technically that could still happen since 
            // this value is not tied directly to begin calls.
            if (mLastNumObjectsRendered > 0)
            {
                batch.End();
            }
        }

        /// <summary>
        /// Interface for adding a game object to the manger.
        /// </summary>
        /// <param name="go">The object to add.</param>
        public void Add(GameObject go)
        {
            // We need this list to be sorted based on render priority.
            // See the Update method in this class for more details why.
            //

            List<GameObject> objList = mGameObjectsToAdd;

            // If we are in the middle of adding objects, this one needs to wait for next frame.
            if (mCurrentUpdateState == UpdatePhase.OnAdd)
            {
                objList = mGameObjectsToAddNextFrame;
            }

            for (int i = 0; i < objList.Count; i++)
            {
                if (go.pRenderPriority < objList[i].pRenderPriority)
                {
                    objList.Insert(i, go);

                    return;
                }
            }

            // If we make it to this point than the object has a higher priority than any
            // of the existing elements (or the same as the highest).
            // This will also take care of the case where the list is empty.
            objList.Add(go);
        }

        /// <summary>
        /// Interface for implicitly removing a game object from the manager.
        /// </summary>
        /// <param name="go">The object to remove.</param>
        public void Remove(GameObject go)
        {
            // This is to terminate the Countdown timer object
            if (go != null)
            {
                // Make sure this object isn't removed more than once (can cause
                // significant issues with GameObjectFactory.
                if (!mGameObjectsToRemove.Contains(go) && !mGameObjectsToRemoveNextFrame.Contains(go))
                {
                    List<GameObject> objList = mGameObjectsToRemove;

                    // If we are in the middle of removing objects, this one needs to wait for next frame.
                    if (mCurrentUpdateState == UpdatePhase.OnRemove)
                    {
                        objList = mGameObjectsToRemoveNextFrame;
                    }

                    objList.Add(go);
                }
            }
        }

        /// <summary>
        /// Populates a list of all the objects within a certain range of a position.
        /// </summary>
        /// <param name="centerPoint">The position to test from.</param>
        /// <param name="radius">The radius from that position that the other objects must be within.</param>
        /// <param name="refObjects">A preallocated list of objects.  This is to avoid GC.</param>
        /// <param name="classifications">The types of objects to check for.</param>
        public void GetGameObjectsInRange(Vector2 centerPoint, Single radius, ref List<GameObject> refObjects, List<GameObjectDefinition.Classifications> classifications)
        {
            // Calculate which cell the source object is in. We only need to test collision
            // against objects in that cell.
            // TODO: The should likely include surrounding cells.
            Vector2 index = CellIndexFromPosition(centerPoint);

            // Loop through the cell we are in and the surround cells for safety.
            for (Int32 y = (Int32)index.Y - 1; y <= (Int32)index.Y + 1; y++)
            {
                for (Int32 x = (Int32)index.X - 1; x <= (Int32)index.X + 1; x++)
                {
                    // It is possible this object is outside of the world.
                    if (IsValidCellIndex(x, y))
                    {
                        GetGameObjectsInRange(centerPoint, radius, mStaticGameObjects[x, y], ref refObjects, classifications);
                    }
                }
            }

            GetGameObjectsInRange(centerPoint, radius, mDynamicGameObjects, ref refObjects, classifications);
        }

        /// <summary>
        /// Populates a list of all the objects within a certain range of a position.
        /// </summary>
        /// <param name="centerPoint">The position to test from.</param>
        /// <param name="radius">The radius from that position that the other objects must be within.</param>
        /// <param name="candidates">A list of all possible candidates source might be colliding with.</param>
        /// <param name="refObjects">A preallocated list of objects.  This is to avoid GC.</param>
        /// <param name="classifications">The types of objects to check for.</param>
        private void GetGameObjectsInRange(Vector2 centerPoint, Single radius, List<GameObject> candidates, ref List<GameObject> refObjects, List<GameObjectDefinition.Classifications> classifications)
        {
            Single radSqr = radius * radius;
            for (int i = 0; i < candidates.Count; i++)
            {
                for (Int32 j = 0; j < classifications.Count; j++)
                {
                    for (Int32 k = 0; k < candidates[i].pClassifications.Count; k++)
                    {
                        // Does this game object have one of the classifications we are interested in?
                        if (candidates[i].pClassifications[k] == classifications[j])
                        {
                            if (Vector2.DistanceSquared(centerPoint, candidates[i].pPosition) < radSqr)
                            {
                                refObjects.Add(candidates[i]);

                                // This object has already been added, so move on to the next object.
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Populates a list of all the objects within a certain range of a position.
        /// </summary>
        /// <param name="rect">A collision rectangle to check against.</param>
        /// <param name="refObjects">A preallocated list of objects.  This is to avoid GC.</param>
        public void GetGameObjectsInRange(Math.Rectangle rect, ref List<GameObject> refObjects)
        {
            // Calculate which cell the source object is in. We only need to test collision
            // against objects in that cell.
            // TODO: The should likely include surrounding cells.
            Vector2 index = CellIndexFromPosition(rect.pCenterPoint);

            // Loop through the cell we are in and the surround cells for safety.
            for (Int32 y = (Int32)index.Y - 1; y <= (Int32)index.Y + 1; y++)
            {
                for (Int32 x = (Int32)index.X - 1; x <= (Int32)index.X + 1; x++)
                {
                    // It is possible this object is outside of the world.
                    if (IsValidCellIndex(x, y))
                    {
                        GetGameObjectsInRange(rect, mStaticGameObjects[x, y], ref refObjects);
                    }
                }
            }

            GetGameObjectsInRange(rect, mDynamicGameObjects, ref refObjects);
        }

        /// <summary>
        /// Populates a list of all the objects within a certain range of a position.
        /// </summary>
        /// <param name="rect">A collision rectangle to check against.</param>
        /// <param name="candidates">A list of all possible candidates source might be colliding with.</param>
        /// <param name="refObjects">A preallocated list of objects.  This is to avoid GC.</param>
        private void GetGameObjectsInRange(Math.Rectangle rect, List<GameObject> candidates, ref List<GameObject> refObjects)
        {
            for (int i = 0; i < mGameObjects.Count; i++)
            {
                if (rect.Intersects(mGameObjects[i].pCollisionRect))
                {
                    refObjects.Add(mGameObjects[i]);
                }
            }
        }

        /// <summary>
        /// Populates a list of all the objects that overlap the source gameobject.
        /// </summary>
        /// <param name="source">The object to test all other game objects against.</param>
        /// <param name="refObjects">The list which will be populated with game objects that overlap "source".</param>
        /// <param name="classifications">Since we likely don't want to test against every object in the world, 
        /// these classifications narrow down the search.  This data is set in the Game Object definition XML.</param>
        public void GetGameObjectsInRange(GameObject source, ref List<GameObject> refObjects, List<GameObjectDefinition.Classifications> classifications)
        {
            // Calculate which cell the source object is in. We only need to test collision
            // against objects in that cell.
            // TODO: The should likely include surrounding cells.
            Vector2 index = CellIndexFromPosition(source.pPosition);

            // Loop through the cell we are in and the surround cells for safety.
            for (Int32 y = (Int32)index.Y - 1; y <= (Int32)index.Y + 1; y++)
            {
                for (Int32 x = (Int32)index.X - 1; x <= (Int32)index.X + 1; x++)
                {
                    // It is possible this object is outside of the world.
                    if (IsValidCellIndex(x, y))
                    {
                        // Get the list of object in this cell.
                        List<GameObject> staticObjs = mStaticGameObjects[x, y];

                        GetGameObjectsInRange(source, staticObjs, ref refObjects, classifications);
                    }
                }
            }
            
            // Dynamic objects all need to be tested against.
            GetGameObjectsInRange(source, mDynamicGameObjects, ref refObjects, classifications);
        }

        /// <summary>
        /// Populates a list of all the objects that overlap the source gameobject. Only tests against
        /// list of candidates passed into this function.
        /// </summary>
        /// <param name="source">The object checking collision.</param>
        /// <param name="candidates">A list of all possible candidates source might be colliding with.</param>
        /// <param name="refObjects">A preallocated list of objects.  This is to avoid GC.</param>
        /// <param name="classifications">Since we likely don't want to test against every object in the world, 
        /// these classifications narrow down the search.  This data is set in the Game Object definition XML.</param>
        private void GetGameObjectsInRange(GameObject source, List<GameObject> candidates, ref List<GameObject> refObjects, List<GameObjectDefinition.Classifications> classifications)
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                // Make sure we aren't finding ourselves.
                if (candidates[i] == source)
                {
                    continue;
                }

                // For each object, check each of its classifications (if it has any).
                for (Int32 j = 0; j < classifications.Count; j++)
                {
                    for (Int32 k = 0; k < candidates[i].pClassifications.Count; k++)
                    {
                        // Does this game object have one of the classifications we are interested in?
                        if (candidates[i].pClassifications[k] == classifications[j])
                        {
                            // Does this game object overlap the source object?
                            if (source.pCollisionRect.Intersects(candidates[i].pCollisionRect))
                            {
                                // Add it to the preallocated list which was passed in.
                                refObjects.Add(candidates[i]);

                                // This object has already been added, so move on to the next object.
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retrieve all GameObject with a particular Classifications.
        /// </summary>
        /// <param name="classification">A classification the GameObject must have (although not exclusively).</param>
        /// <returns>A list of all GameObject which have the specified Classifications.</returns>
        public List<GameObject> GetGameObjectsOfClassification(GameObjectDefinition.Classifications classification)
        {
            return mGameObjectsByClassification[(Int32)classification];
        }

        /// <summary>
        /// Takes a position and figures out which cell that position is in.
        /// </summary>
        /// <param name="pos">A position in the world.</param>
        /// <returns>X and Y indexes into Cell arrays.</returns>
        private Vector2 CellIndexFromPosition(Vector2 pos)
        {
            Vector2 index = new Vector2();

            index.X = (Int32)System.Math.Floor((Double)pos.X / mCellSize);
            index.Y = (Int32)System.Math.Floor((Double)pos.Y / mCellSize);

            return index;
        }

        /// <summary>
        /// Sends a message to all GameObject currently managed by the GameObjectManager.
        /// </summary>
        /// <param name="msg">The message to send.</param>
        public void BroadcastMessage(BehaviourMessage msg)
        {
            BroadcastMessage(msg, null);
        }

        /// <summary>
        /// Sends a message to all GameObject currently managed by the GameObjectManager.
        /// </summary>
        /// <param name="msg">The message to send.</param>
        /// <param name="sender">The object sending the message.</param>
        public void BroadcastMessage(BehaviourMessage msg, GameObject sender)
        {
            if (mGameObjects != null)
            {
                for (Int32 i = 0; i < mGameObjects.Count; i++)
                {
                    mGameObjects[i].OnMessage(msg, sender);
                }
            }

            if (mGameObjectsToAdd != null)
            {
                // These objects haven't been officially added, but they may want to react to events.
                for (Int32 i = 0; i < mGameObjectsToAdd.Count; i++)
                {
                    mGameObjectsToAdd[i].OnMessage(msg, sender);
                }
            }
        }

        /// <summary>
        /// Called by Level so that we can update the Cell information based on the new
        /// size.
        /// </summary>
        /// <param name="info">Information about the current level.</param>
        public void OnMapInfoChange(Level.MapInfo info)
        {
            System.Diagnostics.Debug.Assert(info.mMapWidth == info.mMapHeight, "Cells calculations assume square maps.");
            System.Diagnostics.Debug.Assert(info.mTileWidth == info.mTileHeight, "Cells calculations assume square maps.");

            mNumCells = (info.mMapWidth * info.mTileWidth) / mCellSize;

            System.Diagnostics.Debug.Assert(mStaticGameObjects == null, "Attempting to recallocate cells. Not yet set up to handle this.");

            mStaticGameObjects = new List<GameObject>[mNumCells, mNumCells];

            for (Int32 y = 0; y < mNumCells; y++)
            {
                for (Int32 x = 0; x < mNumCells; x++)
                {
                    mStaticGameObjects[x, y] = new List<GameObject>(100);
                }
            }
        }

        /// <summary>
        /// Gets a list of all the static objects in a cell, based on world position.
        /// </summary>
        /// <param name="postion">The position contained in a cell you want to check.</param>
        /// <returns>A list of all the static objects in the cell at the position specified. null if the position is not inside a valid cell.</returns>
        public List<GameObject> GetObjectsInCell(Vector2 postion)
        {
            Vector2 index = CellIndexFromPosition(postion);

            if (IsValidCellIndex(index))
            {
                return mStaticGameObjects[(Int32)index.X, (Int32)index.Y];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if X and Y indexes are valid indicies inside the cell array.
        /// </summary>
        /// <param name="indexX">The first index into the 2D static object array.</param>
        /// <param name="indexY">The second index into the 2D static object array.</param>
        /// <returns>True if these are valid indicies.</returns>
        public Boolean IsValidCellIndex(Int32 indexX, Int32 indexY)
        {
            return (indexX >= 0 && indexY >= 0 && indexX < mNumCells && indexY < mNumCells);
        }

        /// <summary>
        /// Checks if X and Y indexes are valid indicies inside the cell array.
        /// </summary>
        /// <param name="index">The indicies into the 2D static object array.</param>
        /// <returns>True if these are valid indicies.</returns>
        public Boolean IsValidCellIndex(Vector2 index)
        {
            return IsValidCellIndex((Int32)index.X, (Int32)index.Y);
        }

        /// <summary>
        /// Classes which implement the IUpdatePassChangeReciever may register here to be notified when
        /// the GameObjectManager Update Pass changes.
        /// </summary>
        /// <param name="receiver"></param>
        public void RegisterUpdatePassChangeReceiver(IUpdatePassChangeReciever receiver)
        {
            mUpdatePassChangeReceivers.Add(receiver);
        }

        /// <summary>
        /// Accessor to the pContent property.
        /// </summary>
        public ContentManager pContentManager
        {
            get
            {
                return mContent;
            }
        }

        /// <summary>
        /// Accessor to the pGraphicsDevicemanager property.
        /// </summary>
        public GraphicsDevice pGraphicsDevice
        {
            get
            {
                return mGraphics.GraphicsDevice;
            }
        }

        /// <summary>
        /// The graphics object itself.
        /// </summary>
        public GraphicsDeviceManager pGraphics
        {
            get
            {
                return mGraphics;
            }
        }

        /// <summary>
        /// The player object.  Not guarenteed to be defined during start up.
        /// </summary>
        public GameObject pPlayer
        {
            get
            {
                return mPlayer;
            }

            set
            {
                System.Diagnostics.Debug.Assert(mPlayer == null, "Setting Player more than once.  If this is intentional this assert should be removed.");

                mPlayer = value;
            }
        }

        /// <summary>
        /// The currently set Pass.
        /// </summary>
        public BehaviourDefinition.Passes pCurUpdatePass
        {
            get
            {
                return mCurrentUpdatePass;
            }
            set
            {
#if __ANDROID__
                if (value == MBHEngineContentDefs.BehaviourDefinition.Passes.QUIT)
                {
                    System.Diagnostics.Debug.Assert(false, "Cannot QUIT to OS on Android.");
                    return;
                }
#endif

                if (mCurrentUpdatePass == value)
                {
                    return;
                }

                BehaviourDefinition.Passes old = mCurrentUpdatePass;

                mCurrentUpdatePass = value;

                for (Int32 i = 0; i < mUpdatePassChangeReceivers.Count; i++)
                {
                    mUpdatePassChangeReceivers[i].OnStateChange(mCurrentUpdatePass, old);
                }
            }
        }

        /// <summary>
        /// Accessor to the pInstance property.
        /// </summary>
        public static GameObjectManager pInstance
        {
            get
            {
                // If this is the first time this is called, instantiate our
                // static instance of the class.
                if (mInstance == null)
                {
                    mInstance = new GameObjectManager();
                }

                // Either way, at this point we should have an instantiated version
                // if the class.
                return mInstance;
            }
        }
    }
}
