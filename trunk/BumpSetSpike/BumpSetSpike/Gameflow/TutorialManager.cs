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
        /// Preallocate to avoid GC.
        /// </summary>
        private GestureSample mGesture;

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

            mTxtTapContinue = new GameObject("GameObjects\\UI\\Tutorial\\TapToContinue\\TapToContinue");
            mTxtTapContinue.pPosX = x;
            mTxtTapContinue.pPosY = bottom - 16.0f;

            mImgSwipe = new GameObject("GameObjects\\Items\\Tutorial\\Swipe\\Swipe");

            mImgBackdrop = new GameObject("GameObjects\\UI\\Tutorial\\Backdrop\\Backdrop");
            mImgBackdrop.pPosX = x;
            mImgBackdrop.pPosY = y;

            mHighlightPlayerMsg = new HighlightPlayerMessage();
            mHighlightBallMsg = new HighlightBallMessage();
            mHighlightPartnerMsg = new HighlightPartnerMessage();
        }

        public void StartTutorial()
        {
            if(!pTutorialCompleted)
            {
                pCurState = State.FIRST;
            }
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="dt"></param>
        public void Update(GameTime dt)
        {
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

                case State.RECEIVING_TXT:
                {
                    if (InputManager.pInstance.CheckAction(InputManager.InputActions.A, true) ||
                        InputManager.pInstance.CheckGesture(GestureType.Tap, ref mGesture))
                    {
                        SetState(State.RECEIVING_END);
                    }

                    break;
                }

                case State.BUMP_TXT:
                {
                    if (InputManager.pInstance.CheckAction(InputManager.InputActions.A, true) ||
                        InputManager.pInstance.CheckGesture(GestureType.Tap, ref mGesture))
                    {
                        SetState(State.SET);
                    }

                    break;
                }

                case State.SET_TXT:
                {
                    if (InputManager.pInstance.CheckAction(InputManager.InputActions.A, true) ||
                        InputManager.pInstance.CheckGesture(GestureType.Tap, ref mGesture))
                    {
                        SetState(State.SWIPE);
                    }

                    break;
                }

                case State.SWIPE_TXT_BALL_HIGH:
                {
                    if (InputManager.pInstance.CheckAction(InputManager.InputActions.A, true) ||
                        InputManager.pInstance.CheckGesture(GestureType.Tap, ref mGesture))
                    {
                        SetState(State.SWIPE_TXT_JUMP);
                    }

                    break;
                }

                case State.SWIPE_TXT_JUMP:
                {
                    if (InputManager.pInstance.CheckAction(InputManager.InputActions.A, true) ||
                        InputManager.pInstance.CheckGesture(GestureType.Flick, ref mGesture))
                    {
                        if (IsValidTutorialSwipe(mGesture.Delta, mGesture.Position))
                        {
                            SetState(State.TAP_START);

                            // Force an update so that the flick gets processed.
                            GameObjectManager.pInstance.pPlayer.Update(dt);
                        }
                    }

                    break;
                }

                case State.TAP_TXT:
                {
                    if (InputManager.pInstance.CheckAction(InputManager.InputActions.A, true) ||
                        InputManager.pInstance.CheckGesture(GestureType.Tap, ref mGesture))
                    {
                        SetState(State.TAP_END);

                        // Force an update so that the tap gets processed.
                        GameObjectManager.pInstance.pPlayer.Update(dt);
                    }

                    break;
                }

                case State.PLAYER_TRY:
                {
                    if (InputManager.pInstance.CheckAction(InputManager.InputActions.A, true) ||
                        InputManager.pInstance.CheckGesture(GestureType.Tap, ref mGesture))
                    {
                        SetState(State.PLAYER_TRYING);
                    }

                    break;
                }

                case State.TRY_AGAIN:
                {
                    if (InputManager.pInstance.CheckAction(InputManager.InputActions.A, true) ||
                        InputManager.pInstance.CheckGesture(GestureType.Tap, ref mGesture))
                    {
                        SetState(State.TRYING_AGAIN);
                    }

                    break;
                }

                case State.COMPLETE_WELL_DONE:
                {
                    if (InputManager.pInstance.CheckAction(InputManager.InputActions.A, true) ||
                        InputManager.pInstance.CheckGesture(GestureType.Tap, ref mGesture))
                    {
                        SetState(State.COMPLETE_THATS_ALL);
                    }

                    break;
                }

                case State.COMPLETE_THATS_ALL:
                {
                    if (InputManager.pInstance.CheckAction(InputManager.InputActions.A, true) ||
                        InputManager.pInstance.CheckGesture(GestureType.Tap, ref mGesture))
                    {
                        SetState(State.COMPLETE_RECAP);
                    }

                    break;
                }

                case State.COMPLETE_RECAP:
                {
                    if (InputManager.pInstance.CheckAction(InputManager.InputActions.A, true) ||
                        InputManager.pInstance.CheckGesture(GestureType.Tap, ref mGesture))
                    {
                        SetState(State.COMPLETE_RULES);
                    }

                    break;
                }

                case State.COMPLETE_RULES:
                {
                    if (InputManager.pInstance.CheckAction(InputManager.InputActions.A, true) ||
                        InputManager.pInstance.CheckGesture(GestureType.Tap, ref mGesture))
                    {
                        SetState(State.COMPLETE_GET_READY);
                    }

                    break;
                }

                case State.COMPLETE_GET_READY:
                {
                    if (InputManager.pInstance.CheckAction(InputManager.InputActions.A, true) ||
                        InputManager.pInstance.CheckGesture(GestureType.Tap, ref mGesture))
                    {
                        SetState(State.COMPLETE);
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

                case State.RECEIVING_TXT:
                {
                    GameObjectManager.pInstance.Remove(mTxtWaitForServe);

                    GameObjectManager.pInstance.Remove(mImgBackdrop);
                    GameObjectManager.pInstance.Remove(mTxtTapContinue);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.GAME_PLAY;

                    mHighlightBallMsg.mEnable = false;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlightBallMsg);

                    break;
                }

                case State.BUMP_TXT:
                {
                    GameObjectManager.pInstance.Remove(mTxtBumpAuto);

                    GameObjectManager.pInstance.Remove(mImgBackdrop);
                    GameObjectManager.pInstance.Remove(mTxtTapContinue);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.GAME_PLAY;

                    mHighlightBallMsg.mEnable = false;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlightBallMsg);

                    mHighlightPlayerMsg.mEnable = false;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlightPlayerMsg);

                    break;
                }

                case State.SET_TXT:
                {
                    GameObjectManager.pInstance.Remove(mTxtSetAuto);

                    GameObjectManager.pInstance.Remove(mImgBackdrop);
                    GameObjectManager.pInstance.Remove(mTxtTapContinue);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.GAME_PLAY;

                    mHighlightBallMsg.mEnable = false;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlightBallMsg);

                    mHighlightPartnerMsg.mEnable = false;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlightPartnerMsg);

                    break;
                }

                case State.SWIPE_TXT_BALL_HIGH:
                {
                    GameObjectManager.pInstance.Remove(mTxtWhenBallHigh);

                    GameObjectManager.pInstance.Remove(mImgBackdrop);
                    GameObjectManager.pInstance.Remove(mTxtTapContinue);

                    mHighlightBallMsg.mEnable = false;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlightBallMsg);

                    //GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.GAME_PLAY;

                    break;
                }

                case State.SWIPE_TXT_JUMP:
                {
                    GameObjectManager.pInstance.Remove(mTxtSwipeJump);
                    GameObjectManager.pInstance.Remove(mImgSwipe);

                    GameObjectManager.pInstance.Remove(mImgBackdrop);
                    //GameObjectManager.pInstance.Remove(mTxtTapContinue);

                    mHighlightBallMsg.mEnable = false;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlightBallMsg);

                    mHighlightPlayerMsg.mEnable = false;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlightPlayerMsg);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.GAME_PLAY;

                    break;
                }

                case State.TAP_TXT:
                {
                    GameObjectManager.pInstance.Remove(mTxtTap);

                    GameObjectManager.pInstance.Remove(mImgBackdrop);
                    //GameObjectManager.pInstance.Remove(mTxtTapContinue);

                    mHighlightBallMsg.mEnable = false;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlightBallMsg);

                    mHighlightPlayerMsg.mEnable = false;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlightPlayerMsg);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.GAME_PLAY;

                    break;
                }

                case State.PLAYER_TRY:
                {
                    GameObjectManager.pInstance.Remove(mTxtPlayerTry);

                    GameObjectManager.pInstance.Remove(mImgBackdrop);
                    GameObjectManager.pInstance.Remove(mTxtTapContinue);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.GAME_PLAY;

                    break;
                }

                case State.TRY_AGAIN:
                {
                    GameObjectManager.pInstance.Remove(mTxtTryAgain);

                    GameObjectManager.pInstance.Remove(mImgBackdrop);
                    GameObjectManager.pInstance.Remove(mTxtTapContinue);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.GAME_PLAY;

                    break;
                }

                case State.COMPLETE_WELL_DONE:
                {
                    GameObjectManager.pInstance.Remove(mTxtWellDone);

                    GameObjectManager.pInstance.Remove(mImgBackdrop);
                    GameObjectManager.pInstance.Remove(mTxtTapContinue);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.GAME_PLAY;

                    break;
                }

                case State.COMPLETE_THATS_ALL:
                {
                    GameObjectManager.pInstance.Remove(mTxtThatsAll);

                    GameObjectManager.pInstance.Remove(mImgBackdrop);
                    GameObjectManager.pInstance.Remove(mTxtTapContinue);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.GAME_PLAY;

                    break;
                }

                case State.COMPLETE_RECAP:
                {
                    GameObjectManager.pInstance.Remove(mTxtRecap);

                    GameObjectManager.pInstance.Remove(mImgBackdrop);
                    GameObjectManager.pInstance.Remove(mTxtTapContinue);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.GAME_PLAY;

                    break;
                }

                case State.COMPLETE_RULES:
                {
                    GameObjectManager.pInstance.Remove(mTxtRules);

                    GameObjectManager.pInstance.Remove(mImgBackdrop);
                    GameObjectManager.pInstance.Remove(mTxtTapContinue);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.GAME_PLAY;

                    break;
                }

                case State.COMPLETE_GET_READY:
                {
                    GameObjectManager.pInstance.Remove(mTxtPlayerTry);

                    GameObjectManager.pInstance.Remove(mImgBackdrop);
                    GameObjectManager.pInstance.Remove(mTxtTapContinue);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.GAME_PLAY;

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

                case State.RECEIVING_TXT:
                {
                    GameObjectManager.pInstance.Add(mTxtWaitForServe);

                    GameObjectManager.pInstance.Add(mImgBackdrop);
                    GameObjectManager.pInstance.Add(mTxtTapContinue);

                    mHighlightBallMsg.mEnable = true;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlightBallMsg);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.TUTORIAL_PAUSE;

                    break;
                }

                case State.BUMP_TXT:
                {
                    GameObjectManager.pInstance.Add(mTxtBumpAuto);

                    GameObjectManager.pInstance.Add(mImgBackdrop);
                    GameObjectManager.pInstance.Add(mTxtTapContinue);

                    mHighlightBallMsg.mEnable = true;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlightBallMsg);

                    mHighlightPlayerMsg.mEnable = true;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlightPlayerMsg);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.TUTORIAL_PAUSE;

                    break;
                }

                case State.SET_TXT:
                {
                    GameObjectManager.pInstance.Add(mTxtSetAuto);

                    GameObjectManager.pInstance.Add(mImgBackdrop);
                    GameObjectManager.pInstance.Add(mTxtTapContinue);

                    mHighlightBallMsg.mEnable = true;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlightBallMsg);

                    mHighlightPartnerMsg.mEnable = true;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlightPartnerMsg);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.TUTORIAL_PAUSE;

                    break;
                }

                case State.SWIPE_TXT_BALL_HIGH:
                {
                    GameObjectManager.pInstance.Add(mTxtWhenBallHigh);

                    GameObjectManager.pInstance.Add(mImgBackdrop);
                    GameObjectManager.pInstance.Add(mTxtTapContinue);

                    mHighlightBallMsg.mEnable = true;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlightBallMsg);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.TUTORIAL_PAUSE;

                    break;
                }

                case State.SWIPE_TXT_JUMP:
                {
                    GameObjectManager.pInstance.Add(mTxtSwipeJump);
                    GameObjectManager.pInstance.Add(mImgSwipe);

                    GameObjectManager.pInstance.Add(mImgBackdrop);
                    //GameObjectManager.pInstance.Add(mTxtTapContinue);

                    mHighlightBallMsg.mEnable = true;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlightBallMsg);

                    mHighlightPlayerMsg.mEnable = true;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlightPlayerMsg);

                    //GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.TUTORIAL_PAUSE;

                    break;
                }

                case State.TAP_TXT:
                {
                    GameObjectManager.pInstance.Add(mTxtTap);

                    GameObjectManager.pInstance.Add(mImgBackdrop);
                    //GameObjectManager.pInstance.Add(mTxtTapContinue);

                    mHighlightBallMsg.mEnable = true;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlightBallMsg);

                    mHighlightPlayerMsg.mEnable = true;
                    GameObjectManager.pInstance.BroadcastMessage(mHighlightPlayerMsg);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.TUTORIAL_PAUSE;

                    break;
                }

                case State.PLAYER_TRY:
                {
                    GameObjectManager.pInstance.Add(mTxtPlayerTry);

                    GameObjectManager.pInstance.Add(mImgBackdrop);
                    GameObjectManager.pInstance.Add(mTxtTapContinue);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.TUTORIAL_PAUSE;

                    break;
                }

                case State.TRY_AGAIN:
                {
                    GameObjectManager.pInstance.Add(mTxtTryAgain);

                    GameObjectManager.pInstance.Add(mImgBackdrop);
                    GameObjectManager.pInstance.Add(mTxtTapContinue);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.TUTORIAL_PAUSE;

                    break;
                }

                case State.COMPLETE_WELL_DONE:
                {
                    GameObjectManager.pInstance.Add(mTxtWellDone);

                    GameObjectManager.pInstance.Add(mImgBackdrop);
                    GameObjectManager.pInstance.Add(mTxtTapContinue);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.TUTORIAL_PAUSE;

                    break;
                }

                case State.COMPLETE_THATS_ALL:
                {
                    GameObjectManager.pInstance.Add(mTxtThatsAll);

                    GameObjectManager.pInstance.Add(mImgBackdrop);
                    GameObjectManager.pInstance.Add(mTxtTapContinue);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.TUTORIAL_PAUSE;

                    break;
                }

                case State.COMPLETE_RECAP:
                {
                    GameObjectManager.pInstance.Add(mTxtRecap);

                    GameObjectManager.pInstance.Add(mImgBackdrop);
                    GameObjectManager.pInstance.Add(mTxtTapContinue);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.TUTORIAL_PAUSE;

                    break;
                }

                case State.COMPLETE_RULES:
                {
                    GameObjectManager.pInstance.Add(mTxtRules);

                    GameObjectManager.pInstance.Add(mImgBackdrop);
                    GameObjectManager.pInstance.Add(mTxtTapContinue);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.TUTORIAL_PAUSE;

                    break;
                }

                case State.COMPLETE_GET_READY:
                {
                    GameObjectManager.pInstance.Add(mTxtPlayerTry);

                    GameObjectManager.pInstance.Add(mImgBackdrop);
                    GameObjectManager.pInstance.Add(mTxtTapContinue);

                    GameObjectManager.pInstance.pCurUpdatePass = MBHEngineContentDefs.BehaviourDefinition.Passes.TUTORIAL_PAUSE;

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

            DebugMessageDisplay.pInstance.AddConstantMessage("Swipe Length: " + swipe_delta.Length());

            if (swipe_delta.Length() < min_length)
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
