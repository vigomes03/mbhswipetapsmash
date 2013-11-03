using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBHEngine.GameObject;
using MBHEngine.Render;
using MBHEngine.Math;
using MBHEngine.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework;
using MBHEngine.Debug;
using MBHEngine.Behaviour;
using MBHEngineContentDefs;
using MBHEngine.IO;
using Microsoft.Xna.Framework.Audio;

namespace BumpSetSpike.Gameflow
{
    /// <summary>
    /// Simple singleton used to track the status of the tutorial at a global level.
    /// </summary>
    public class TutorialManager
    {
        public enum State
        {
            NONE = 0,
            RECEIVING_START,
            RECEIVING_TXT,
            RECEIVING_END,
            BUMP_TXT,
            SET,
            SET_TXT,
            SWIPE,
            SWIPE_TXT_BALL_HIGH,
            SWIPE_TXT_JUMP,
            TAP_START,
            TAP_TXT,
            TAP_END,
            PLAYER_TRY,
            PLAYER_TRYING,
            TRY_AGAIN,
            TRYING_AGAIN,
            COMPLETE_WELL_DONE,
            COMPLETE_THATS_ALL,
            COMPLETE_RECAP,
            COMPLETE_RULES,
            COMPLETE_GET_READY,
            COMPLETE,

            COUNT,

            FIRST = RECEIVING_START,
        }

        /// <summary>
        /// Base message used for highlighting objects during Tutorial.
        /// </summary>
        public class HighlightObjectMessage : BehaviourMessage
        {
            public Boolean mEnable;

            public override void Reset()
            {
                mEnable = false;
            }
        }
        public class HighlightPlayerMessage : HighlightObjectMessage { }
        public class HighlightPartnerMessage : HighlightObjectMessage { }
        public class HighlightBallMessage : HighlightObjectMessage { }

        /// <summary>
        /// Wrapper for some of the really common states types.
        /// </summary>
        public struct TutorialState
        {
            /// <summary>
            /// A list of objects that will be shown when this state starts and then
            /// will be removed when the state ends.
            /// </summary>
            private List<GameObject> mObjectsToShow;

            /// <summary>
            /// A list of messages to send when the state starts and ends. Objects will be
            /// highlighted on start and unhighlighted on end.
            /// </summary>
            private List<HighlightObjectMessage> mHighlighMsgs;

            /// <summary>
            /// Used to delay the time before we start allowing the user to skip the state
            /// so that they don't skip is by accident.
            /// </summary>
            private StopWatch mInputDelay;

            /// <summary>
            /// If true, this state will enter the TUTORIAL_PAUSE update state.
            /// </summary>
            private Boolean mPauseOnEnter;

            /// <summary>
            /// If true, this state will enter the GAMEPLAY state when exiting.
            /// </summary>
            private Boolean mResumeOnExit;

            /// <summary>
            /// When a tap is detected, this state will be returned from Update().
            /// </summary>
            private TutorialManager.State mNextStateOnTap;

            /// <summary>
            /// Preallocate to avoid GC.
            /// </summary>
            private GestureSample mGesture;

            /// <summary>
            /// The sound that plays when you user advances through the tutorial.
            /// </summary>
            private SoundEffect mFxMenuSelect;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="objectsToShow"></param>
            /// <param name="highlighMsgs"></param>
            /// <param name="inputDelay"></param>
            /// <param name="pauseOnEnter"></param>
            /// <param name="resumeOnExit"></param>
            /// <param name="nextStateOnTap"></param>
            public TutorialState(List<GameObject> objectsToShow, 
                List<HighlightObjectMessage> highlighMsgs, 
                StopWatch inputDelay,
                Boolean pauseOnEnter,
                Boolean resumeOnExit,
                TutorialManager.State nextStateOnTap)
            {
                mObjectsToShow = objectsToShow;
                mHighlighMsgs = highlighMsgs;
                mInputDelay = inputDelay;
                mPauseOnEnter = pauseOnEnter;
                mResumeOnExit = resumeOnExit;
                mNextStateOnTap = nextStateOnTap;
                mGesture = new GestureSample();
                mFxMenuSelect = GameObjectManager.pInstance.pContentManager.Load<SoundEffect>("Audio\\FX\\MenuSelect");
            }

            /// <summary>
            /// Call this when the associated state is entered.
            /// </summary>
            public void OnEnterState()
            {
                // Start rendering and updating all the associated objects.
                for (Int32 i = 0; i < mObjectsToShow.Count; i++)
                {
                    GameObjectManager.pInstance.Add(mObjectsToShow[i]);
                }

                // Send highlight messages out.
                for (Int32 i = 0; i < mHighlighMsgs.Count; i++)
                {
                    mHighlighMsgs[i].Reset();
                    mHighlighMsgs[i].mEnable = true;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlighMsgs[i]);
                }

                // Restart the input delay so that we don't start checking for taps right away.
                mInputDelay.Restart();

                // If requested, enter the TUTORIAL_PAUSE state.
                if (mPauseOnEnter)
                {
                    GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.TUTORIAL_PAUSE;
                }
            }

            /// <summary>
            /// Call this every frame that the associated state is active.
            /// </summary>
            /// <returns></returns>
            public TutorialManager.State OnUpdate()
            {
                // We use NONE to mean that it does not want to change states. This may need to change
                // if NONE ever becomes a valid case.
                if (mNextStateOnTap != State.NONE)
                {
                    if (mInputDelay.IsExpired())
                    {
                        if (InputManager.pInstance.CheckGesture(GestureType.Tap, ref mGesture))
                        {
                            mFxMenuSelect.Play();

                            return mNextStateOnTap;
                        }
                    }
                }

                return State.NONE;
            }

            /// <summary>
            /// Call this when the associated state ends.
            /// </summary>
            public void OnExitState()
            {
                for (Int32 i = 0; i < mObjectsToShow.Count; i++)
                {
                    GameObjectManager.pInstance.Remove(mObjectsToShow[i]);
                }

                for (Int32 i = 0; i < mHighlighMsgs.Count; i++)
                {
                    mHighlighMsgs[i].Reset();
                    mHighlighMsgs[i].mEnable = false;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlighMsgs[i]);
                }

                if (mResumeOnExit)
                {
                    GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.GAME_PLAY;
                }
            }
        }

        /// <summary>
        /// Static instance of the class.
        /// </summary>
        static private TutorialManager mInstance;

        /// <summary>
        /// Objects used throughout the tutorial.
        /// </summary>
        private GameObject mTxtWaitForServe;
        private GameObject mTxtBumpAuto;
        private GameObject mTxtSetAuto;
        private GameObject mTxtWhenBallHigh;
        private GameObject mTxtSwipeJump;
        private GameObject mTxtTap;
        private GameObject mTxtPlayerTry;
        private GameObject mTxtTryAgain;
        private GameObject mTxtWellDone;
        private GameObject mTxtThatsAll;
        private GameObject mTxtRecap;
        private GameObject mTxtRules;
        private GameObject mTxtTitle;
        private GameObject mTxtTapContinue;
        private GameObject mTxtRealThing;
        private GameObject mImgFingerSwipe;
        private GameObject mImgSwipe;
        private GameObject mImgBackdrop;

        /// <summary>
        /// The current state of the tutorial.
        /// </summary>
        private State mCurState;

        /// <summary>
        /// Used to delay the first message during tutorial.
        /// </summary>
        private StopWatch mReceiveWatch;

        /// <summary>
        /// Used to delay input between the time when a tutorial is shown and when 
        /// we allow the user to skip it.
        /// </summary>
        private StopWatch mInputDelay;

        /// <summary>
        /// Preallocate to avoid GC.
        /// </summary>
        private GestureSample mGesture;

        /// <summary>
        /// Collection of TutorialStates to make things a little easier.
        /// </summary>
        public Dictionary<Int32, TutorialState> mTutorialStates;

        /// <summary>
        /// Preallocated messages.
        /// </summary>
        private HighlightPlayerMessage mHighlightPlayerMsg;
        private HighlightBallMessage mHighlightBallMsg;
        private HighlightPartnerMessage mHighlightPartnerMsg;

        /// <summary>
        /// Accessor to the current state.
        /// </summary>
        public State pCurState 
        {
            get
            {
                return mCurState;
            }
            set
            {
                SetState(value);
            }
        }

        /// <summary>
        /// Has the tutorial been completed yet?
        /// </summary>
        public Boolean pTutorialCompleted { get; set; }

        /// <summary>
        /// Private constructor to avoid accidental creation.
        /// </summary>
        private TutorialManager() { }

        /// <summary>
        /// Call this before using the class.
        /// </summary>
        public void Initialize()
        {
            pTutorialCompleted = false;
            pCurState = State.NONE;

            if (CommandLineManager.pInstance["SkipTutorial"] != null)
            {
                pTutorialCompleted = true;
            }

            Single x = ((GameObjectManager.pInstance.pGraphicsDevice.Viewport.Width * 0.5f) / CameraManager.pInstance.pZoomScale);
            Single y = ((GameObjectManager.pInstance.pGraphicsDevice.Viewport.Height * 0.5f) / CameraManager.pInstance.pZoomScale);

            Single bottom = ((GameObjectManager.pInstance.pGraphicsDevice.Viewport.Height) / CameraManager.pInstance.pZoomScale);

            mTxtWaitForServe = new GameObject("GameObjects\\UI\\Tutorial\\WaitForServe\\WaitForServe");
            mTxtWaitForServe.pPosX = x;
            mTxtWaitForServe.pPosY = 30.0f;

            mTxtBumpAuto = new GameObject("GameObjects\\UI\\Tutorial\\BumpAuto\\BumpAuto");
            mTxtBumpAuto.pPosX = 12;
            mTxtBumpAuto.pPosY = y;

            mTxtSetAuto = new GameObject("GameObjects\\UI\\Tutorial\\SetAuto\\SetAuto");
            mTxtSetAuto.pPosX = x - 10;
            mTxtSetAuto.pPosY = y;

            mTxtWhenBallHigh = new GameObject("GameObjects\\UI\\Tutorial\\WhenBallHigh\\WhenBallHigh");
            mTxtWhenBallHigh.pPosX = x;
            mTxtWhenBallHigh.pPosY = y - 8;

            mTxtSwipeJump = new GameObject("GameObjects\\UI\\Tutorial\\SwipeJump\\SwipeJump");
            mTxtSwipeJump.pPosX = x;
            mTxtSwipeJump.pPosY = y + 8;

            mTxtTap = new GameObject("GameObjects\\UI\\Tutorial\\Tap\\Tap");
            mTxtTap.pPosX = x;
            mTxtTap.pPosY = y - 8;

            mTxtPlayerTry = new GameObject("GameObjects\\UI\\Tutorial\\PlayerTry\\PlayerTry");
            mTxtPlayerTry.pPosX = x;
            mTxtPlayerTry.pPosY = y;

            mTxtTryAgain = new GameObject("GameObjects\\UI\\Tutorial\\TryAgain\\TryAgain");
            mTxtTryAgain.pPosX = x;
            mTxtTryAgain.pPosY = y;

            mTxtWellDone = new GameObject("GameObjects\\UI\\Tutorial\\NiceWork\\NiceWork");
            mTxtWellDone.pPosX = x;
            mTxtWellDone.pPosY = y;

            mTxtThatsAll = new GameObject("GameObjects\\UI\\Tutorial\\AllThereIs\\AllThereIs");
            mTxtThatsAll.pPosX = x;
            mTxtThatsAll.pPosY = y;

            mTxtRecap = new GameObject("GameObjects\\UI\\Tutorial\\SwipeTapSmash\\SwipeTapSmash");
            mTxtRecap.pPosX = x;
            mTxtRecap.pPosY = y;

            mTxtRules = new GameObject("GameObjects\\UI\\Tutorial\\InRow\\InRow");
            mTxtRules.pPosX = x;
            mTxtRules.pPosY = y;

            mTxtTitle = new GameObject("GameObjects\\UI\\Tutorial\\Title\\Title");
            mTxtTitle.pPosX = x;
            mTxtTitle.pPosY = 5.0f;

            mTxtRealThing = new GameObject("GameObjects\\UI\\Tutorial\\RealThing\\RealThing");
            mTxtRealThing.pPosX = x;
            mTxtRealThing.pPosY = y;

            mTxtTapContinue = new GameObject("GameObjects\\UI\\Tutorial\\TapToContinue\\TapToContinue");
            mTxtTapContinue.pPosX = x;
            mTxtTapContinue.pPosY = bottom - 16.0f;

            mImgSwipe = new GameObject("GameObjects\\Items\\Tutorial\\Swipe\\Swipe");
            mImgFingerSwipe = new GameObject("GameObjects\\Items\\Tutorial\\FingerSwipe\\FingerSwipe");

            mImgBackdrop = new GameObject("GameObjects\\UI\\Tutorial\\Backdrop\\Backdrop");
            mImgBackdrop.pPosX = x;
            mImgBackdrop.pPosY = y;

            mInputDelay = StopWatchManager.pInstance.GetNewStopWatch();
            mInputDelay.pLifeTime = 15.0f;

            mHighlightPlayerMsg = new HighlightPlayerMessage();
            mHighlightBallMsg = new HighlightBallMessage();
            mHighlightPartnerMsg = new HighlightPartnerMessage();

            mTutorialStates = new Dictionary<Int32, TutorialState>();

            // RECEIVING_TXT
            TutorialState state = new TutorialState(
                new List<GameObject> { mTxtWaitForServe, mImgBackdrop, mTxtTapContinue },
                new List<HighlightObjectMessage> { mHighlightBallMsg }, 
                mInputDelay, 
                true, 
                true,
                State.RECEIVING_END);
            mTutorialStates[(Int32)State.RECEIVING_TXT] = state;

            // BUMP_TXT
            state = new TutorialState(
                new List<GameObject> { mTxtBumpAuto, mImgBackdrop, mTxtTapContinue },
                new List<HighlightObjectMessage> { mHighlightBallMsg, mHighlightPlayerMsg },
                mInputDelay,
                true,
                true,
                State.SET);
            mTutorialStates[(Int32)State.BUMP_TXT] = state;

            // SET_TXT
            state = new TutorialState(
                new List<GameObject> { mTxtSetAuto, mImgBackdrop, mTxtTapContinue },
                new List<HighlightObjectMessage> { mHighlightBallMsg, mHighlightPartnerMsg },
                mInputDelay,
                true,
                true,
                State.SWIPE);
            mTutorialStates[(Int32)State.SET_TXT] = state;

            // SWIPE_TXT_BALL_HIGH
            state = new TutorialState(
                new List<GameObject> { mTxtWhenBallHigh, mImgBackdrop, mTxtTapContinue },
                new List<HighlightObjectMessage> { mHighlightBallMsg },
                mInputDelay,
                true,
                false,
                State.SWIPE_TXT_JUMP);
            mTutorialStates[(Int32)State.SWIPE_TXT_BALL_HIGH] = state;

            // RECIEVING_END
            state = new TutorialState(
                new List<GameObject> { mTxtSwipeJump, mImgBackdrop, mImgSwipe, mImgFingerSwipe },
                new List<HighlightObjectMessage> { mHighlightBallMsg, mHighlightPlayerMsg },
                mInputDelay,
                false,
                true,
                State.NONE);
            mTutorialStates[(Int32)State.SWIPE_TXT_JUMP] = state;

            // TAP_TXT
            state = new TutorialState(
                new List<GameObject> { mTxtTap, mImgBackdrop },
                new List<HighlightObjectMessage> { mHighlightBallMsg, mHighlightPlayerMsg },
                mInputDelay,
                true,
                true,
                State.NONE);
            mTutorialStates[(Int32)State.TAP_TXT] = state;

            // PLAYER_TRY
            state = new TutorialState(
                new List<GameObject> { mTxtPlayerTry, mImgBackdrop, mTxtTapContinue },
                new List<HighlightObjectMessage>( ),
                mInputDelay,
                true,
                true,
                State.PLAYER_TRYING);
            mTutorialStates[(Int32)State.PLAYER_TRY] = state;

            // TRY_AGAIN
            state = new TutorialState(
                new List<GameObject> { mTxtTryAgain, mImgBackdrop, mTxtTapContinue },
                new List<HighlightObjectMessage>(),
                mInputDelay,
                true,
                true,
                State.TRYING_AGAIN);
            mTutorialStates[(Int32)State.TRY_AGAIN] = state;

            // COMPLETE_WELL_DONE
            state = new TutorialState(
                new List<GameObject> { mTxtWellDone, mImgBackdrop, mTxtTapContinue },
                new List<HighlightObjectMessage>(),
                mInputDelay,
                true,
                true,
                State.COMPLETE_THATS_ALL);
            mTutorialStates[(Int32)State.COMPLETE_WELL_DONE] = state;

            // COMPLETE_THATS_ALL
            state = new TutorialState(
                new List<GameObject> { mTxtThatsAll, mImgBackdrop, mTxtTapContinue },
                new List<HighlightObjectMessage>(),
                mInputDelay,
                true,
                true,
                State.COMPLETE_RECAP);
            mTutorialStates[(Int32)State.COMPLETE_THATS_ALL] = state;

            // COMPLETE_RECAP
            state = new TutorialState(
                new List<GameObject> { mTxtRecap, mImgBackdrop, mTxtTapContinue },
                new List<HighlightObjectMessage>(),
                mInputDelay,
                true,
                true,
                State.COMPLETE_RULES);
            mTutorialStates[(Int32)State.COMPLETE_RECAP] = state;

            // COMPLETE_RULES
            state = new TutorialState(
                new List<GameObject> { mTxtRules, mImgBackdrop, mTxtTapContinue },
                new List<HighlightObjectMessage>(),
                mInputDelay,
                true,
                true,
                State.COMPLETE_GET_READY);
            mTutorialStates[(Int32)State.COMPLETE_RULES] = state;

            // COMPLETE_GET_READY
            state = new TutorialState(
                new List<GameObject> { mTxtRealThing, mImgBackdrop, mTxtTapContinue },
                new List<HighlightObjectMessage>(),
                mInputDelay,
                true,
                true,
                State.COMPLETE);
            mTutorialStates[(Int32)State.COMPLETE_GET_READY] = state;
        }

        public void StartTutorial()
        {
            if(!pTutorialCompleted)
            {
                pCurState = State.FIRST;
            }
        }

        /// <summary>
        /// Cancel the tutorial early, and trigger it to clean itself up.
        /// </summary>
        public void StopTutorial()
        {
            // If we are in state NONE then the tutorial is not running right now.
            if (mCurState != State.NONE)
            {
                // Give the current state a chance to clean up.
                ExitState(mCurState);

                // The ExitState of COMPLETE_GET_READY will remove the Tutorial Title Text,
                // but otherwise we need to manually remove it.
                if (mCurState != State.COMPLETE_GET_READY)
                {
                    GameObjectManager.pInstance.Remove(mTxtTitle);
                }

                mCurState = State.NONE;
            }
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="dt"></param>
        public void Update(GameTime dt)
        {
            // Check if we are handling this in one of our states.
            if (mTutorialStates.ContainsKey((Int32)mCurState))
            {
                State newState = mTutorialStates[(Int32)mCurState].OnUpdate();

                if (newState != State.NONE)
                {
                    pCurState = newState;

                    // Return early since we are changing states.
                    return;
                }
            }

            switch (pCurState)
            {
                case State.RECEIVING_START:
                {
                    if (mReceiveWatch.IsExpired())
                    {
                        pCurState = State.RECEIVING_TXT;
                    }

                    break;
                }

                case State.SWIPE_TXT_JUMP:
                {
                    if (mInputDelay.IsExpired())
                    {
                        if (InputManager.pInstance.CheckGesture(GestureType.Flick, ref mGesture))
                        {
                            if (IsValidTutorialSwipe(mGesture.Delta, mGesture.Position))
                            {
                                SetState(State.TAP_START);

                                // Force an update so that the flick gets processed.
                                GameObjectManager.pInstance.pPlayer.Update(dt);
                            }
                        }
                    }

                    break;
                }

                case State.TAP_TXT:
                {
                    if (mInputDelay.IsExpired())
                    {
                        if (InputManager.pInstance.CheckGesture(GestureType.Tap, ref mGesture))
                        {
                            SetState(State.TAP_END);

                            // Force an update so that the tap gets processed.
                            GameObjectManager.pInstance.pPlayer.Update(dt);
                        }
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// Start a new state.
        /// </summary>
        /// <param name="newState">The state to start.</param>
        private void SetState(State newState)
        {
            if(newState == pCurState || pTutorialCompleted)
            {
                return;
            }

            ExitState(pCurState);

            // Avoid pCurState to avoid infinite recursion.
            mCurState = newState;

            EnterState(pCurState);
        }

        /// <summary>
        /// Automatically called when changing states.
        /// </summary>
        /// <param name="oldState"></param>
        private void ExitState(State oldState)
        {
            // Check if we are handling this in one of our states.
            if (mTutorialStates.ContainsKey((Int32)oldState))
            {
                mTutorialStates[(Int32)oldState].OnExitState();
            }

            switch (oldState)
            {
                case State.RECEIVING_START:
                {
                    if (mReceiveWatch != null)
                    {
                        StopWatchManager.pInstance.RecycleStopWatch(mReceiveWatch);
                        mReceiveWatch = null;
                    }

                    break;
                }

                case State.COMPLETE_GET_READY:
                {
                    GameObjectManager.pInstance.Remove(mTxtTitle);

                    break;
                }
            }
        }

        /// <summary>
        /// Automatically called when a new state is started.
        /// </summary>
        /// <param name="newState"></param>
        private void EnterState(State newState)
        {
            // Check if we are handling this in one of our states.
            if (mTutorialStates.ContainsKey((Int32)newState))
            {
                mTutorialStates[(Int32)newState].OnEnterState();

                return;
            }

            switch (newState)
            {
                case State.RECEIVING_START:
                {
                    if (mReceiveWatch == null)
                    {
                        mReceiveWatch = StopWatchManager.pInstance.GetNewStopWatch();
                        mReceiveWatch.pLifeTime = 15;
                    }

                    GameObjectManager.pInstance.Add(mTxtTitle);

                    break;
                }

                case State.COMPLETE:
                {
                    pTutorialCompleted = true;

                    break;
                }
            }
        }

        /// <summary>
        /// Central place for checking a user's swipe is valid for the swipe tutorial.
        /// </summary>
        /// <param name="swipe_delta">The swipe.</param>
        /// <returns>True if the swipe is valid.</returns>
        public Boolean IsValidTutorialSwipe(Vector2 swipe_delta, Vector2 pos)
        {
            const Single length = 5972.28f;
            const Single length_delta = 2000.0f;
            const Single min_length = length - length_delta;
            const Single max_length = length + (length_delta * 0.5f);

            Single angle = MathHelper.ToDegrees((Single)Math.Atan2(swipe_delta.Y, swipe_delta.X));

            DebugMessageDisplay.pInstance.AddConstantMessage("Angle: " + angle);

            DebugMessageDisplay.pInstance.AddConstantMessage("Swipe Length: " + swipe_delta.Length());

            if (angle < -80.0f || angle > -10.0f)
            {
                // Bad angle. Make sure they at least get something close to the right directions (which is
                // about 45 degrees).
                return false;
            }
            else if (swipe_delta.Length() < min_length)
            {
                GameObject txt = GameObjectFactory.pInstance.GetTemplate("GameObjects\\Items\\Tutorial\\Faster\\Faster");
                txt.pPosition = mImgSwipe.pPosition;
                GameObjectManager.pInstance.Add(txt);

                return false;
            }
            else if (swipe_delta.Length() > max_length)
            {
                GameObject txt = GameObjectFactory.pInstance.GetTemplate("GameObjects\\Items\\Tutorial\\Slower\\Slower");
                txt.pPosition = mImgSwipe.pPosition;
                GameObjectManager.pInstance.Add(txt);

                return false;
            }
            else
            {
                return true;
            }
        }


        /// <summary>
        /// Access to the Singleton. Creates the static instance if first time this is called.
        /// </summary>
        static public TutorialManager pInstance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new TutorialManager();
                }

                return mInstance;
            }
        }        
    }
}