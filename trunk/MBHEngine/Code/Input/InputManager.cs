using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using MBHEngine.IO;
using MBHEngine.Debug;
using Microsoft.Xna.Framework.Input.Touch;
using MBHEngine.Render;
using System.Collections;

namespace MBHEngine.Input
{
    /// <summary>
    /// This helper class allows us to wrap multiple input devices into a single group a functions.  
    /// This way we don't need to check for each device seperatly.
    /// </summary>
    public class InputManager
    {
        /// <summary>
        /// Static instance of the class; this is a singleton.
        /// </summary>
        private static InputManager mInstance = null;

        /// <summary>
        /// A list of all the actions we can check for.
        /// </summary>
        public enum InputActions 
        { 
            LA_LEFT = 0, LA_RIGHT, LA_UP, LA_DOWN,
            RA_LEFT, RA_RIGHT, RA_UP, RA_DOWN, 
            DP_LEFT, DP_RIGHT, DP_UP, DP_DOWN,
            A, B, X, Y, 
            L1, L2, L3, R1, R2, 
            START, BACK,
            COUNT
        };
        
        /// <summary>
        /// Maximum number of controllers.
        /// </summary>
        private int MAX_CONTROLLER_COUNT = 4;

        /// <summary>
        /// A key value pair which matches InputActions to keyboard keys.
        /// </summary>
        private Keys[] mKeyboardActionMap;

        /// <summary>
        /// The state of the keyboard last update.
        /// </summary>
        private KeyboardState mPreviousKeyboardState;

        /// <summary>
        /// The state of the gamepad last update.
        /// </summary>
        private GamePadState mPreviousGamePadState;

        /// <summary>
        /// The state of the gamepad at the start of this frame. It seems
        /// to be able to change mid update, so it is inportant to only
        /// retrive it once to avoid mismatched state info.
        /// </summary>
        private GamePadState mCurrentGamePadState;

        /// <summary>
        /// Store the state of the mouse for the current frame.
        /// </summary>
        private MouseState mCurrentMouseState;

        /// <summary>
        /// Store the state of the mouse as it was last frame.
        /// </summary>
        private MouseState mPreviousMouseState;

        /// <summary>
        /// Stores the recent history of mouse state while the left button is being held. Used to 
        /// determine flick direction.
        /// </summary>
        private Queue<MouseState> mMouseHoldHistory;

        /// <summary>
        /// Used for drawing the mouse history. We dont just use mMouseHoldHistory because that gets cleared
        /// too soon.
        /// </summary>
#if ALLOW_GARBAGE
        private Queue<MouseState> mDebugMouseHist;
#endif

        /// <summary>
        /// Stores all the Gestures that we pending at the start of a frame.
        /// </summary>
        private List<GestureSample> mCurrentGestureSamples;

        /// <summary>
        /// Has the user been locked to a controller yet?
        /// </summary>
        public bool mIsControllerLocked;

        /// <summary>
        /// Which controller are the locked to?
        /// </summary>
        public PlayerIndex mActiveControllerIndex;

        /// <summary>
        /// Constructor.
        /// </summary>
        private InputManager()
        {
            mIsControllerLocked = false;


            //LA_LEFT = 0, LA_RIGHT, LA_UP, LA_DOWN,
            //RA_LEFT, RA_RIGHT, RA_UP, RA_DOWN, 
            //DP_LEFT, DP_RIGHT, DP_UP, DP_DOWN,
            //A, B, X, Y, 
            //L1, L2, L3, R1, R2, 
            //START, BACK 

            mKeyboardActionMap = new Keys[] { 
                                                Keys.Left,
                                                Keys.Right,
                                                Keys.Up,
                                                Keys.Down,
                                                Keys.OemComma, 
                                                Keys.OemQuestion,
                                                Keys.OemSemicolon,
                                                Keys.OemPeriod,
                                                Keys.Left,
                                                Keys.Right,
                                                Keys.Up,
                                                Keys.Down,
                                                Keys.A,
                                                Keys.B,
                                                Keys.X,
                                                Keys.Y,
                                                Keys.LeftControl,
                                                Keys.LeftShift,
                                                Keys.F4,
                                                Keys.RightControl,
                                                Keys.RightShift,
                                                Keys.Enter,
                                                Keys.Escape
                                            };


            System.Diagnostics.Debug.Assert(mKeyboardActionMap.Length == (Int32)InputActions.COUNT, "Keyboard mapping does not match InputActions.  Have you added new InputActions but not updated the keyboard mapping?");

            mPreviousKeyboardState = Keyboard.GetState();

            mCurrentGestureSamples = new List<GestureSample>(16);

            mPreviousMouseState = mCurrentMouseState = Mouse.GetState();

            mMouseHoldHistory = new Queue<MouseState>();
#if ALLOW_GARBAGE
            mDebugMouseHist = new Queue<MouseState> ();
#endif
            
            bool cheat_selection = CommandLineManager.pInstance["CheatGamePadSelection"] != null;

#if WINDOWS_PHONE || __ANDROID__
            // On WP just always use controller one, which should be the phone itself.
            cheat_selection = true;
#endif
            
            if (cheat_selection)
            {
                mIsControllerLocked = true;
                mActiveControllerIndex = PlayerIndex.One;
                mPreviousGamePadState = mCurrentGamePadState = GamePad.GetState(mActiveControllerIndex);
            }
        }

        /// <summary>
        /// This needs to be called at the start of each update.  It gives the InputManager a chance
        /// to store the current state of the controller so that we have a consistant
        /// state for the entire frame.
        /// </summary>
        public void UpdateBegin()
        {
            mCurrentGamePadState = GamePad.GetState(mActiveControllerIndex);

#if WINDOWS_PHONE
            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample g = TouchPanel.ReadGesture();
				
                mCurrentGestureSamples.Add(g);
            }
#elif WINDOWS || __ANDROID__

			#if WINDOWS
            mCurrentMouseState = Mouse.GetState();
			#elif __ANDROID__
			TouchCollection touch = TouchPanel.GetState();
			if (touch.Count > 0)
			{
				//for (Int32 i = 0; i < TouchPanel.GetState().Count; i++)
				{
					TouchLocation state = touch[0];
					Vector2 pos = state.Position;
					mCurrentMouseState = new MouseState((Int32)pos.X, (Int32)pos.Y, 0, ButtonState.Pressed, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
				}
			}
			else
			{
				mCurrentMouseState = new MouseState((Int32)mPreviousMouseState.Position.X, (Int32)mPreviousMouseState.Position.Y, 0, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
			}
			#endif

            // While the left mouse button is being held, store the recent history.
            if (mCurrentMouseState.LeftButton == ButtonState.Pressed)
            {
                // Android duplicates the final index, so this really means use the last 2 pieces of data.
                int maxHist = 5; 

#if ALLOW_GARBAGE
                if (mMouseHoldHistory.Count == 0)
                {
                    mDebugMouseHist.Clear();
                }
#endif

                // Old store the most recent presses, so that if the user moves the mouse in a 
                // non-straight line, we only react the last few moments, as you would expect with
                // flicking something.
                if (mMouseHoldHistory.Count >= maxHist)
                {
                    // Get rid of the oldest item which should be at the front of the queue.
                    mMouseHoldHistory.Dequeue();
#if ALLOW_GARBAGE
                    mDebugMouseHist.Dequeue();
#endif
                }

                // Add the new mouse state to the back of the cue.
                mMouseHoldHistory.Enqueue(mCurrentMouseState);
#if ALLOW_GARBAGE
                mDebugMouseHist.Enqueue(mCurrentMouseState);
#endif
            }

            // Mouse Tap. We use the press event on windows and android because it feels more responsive when hitting.
            // However, this means when you are navigating men
            if (mCurrentMouseState.LeftButton == ButtonState.Pressed && 
                mPreviousMouseState.LeftButton != ButtonState.Pressed)
            {
                // Mouse stores its info as int. Needs to be converted to vector.
                Vector2 newMouse = new Vector2(mCurrentMouseState.X, mCurrentMouseState.Y);

                // Create a new Tap Gesture, since the mouse was pressed and released.
                // Time Stamp is missing.
                GestureSample g = new GestureSample(GestureType.Tap, TimeSpan.Zero, newMouse, Vector2.Zero, Vector2.Zero, Vector2.Zero);
                //DebugMessageDisplay.pInstance.AddConstantMessage("Mouse Clicked: " + CameraManager.pInstance.ProjectMouseToWorldSpace(newMouse) + ", " + mCurrentMouseState);

                mCurrentGestureSamples.Add(g);
            }


            // Mouse Released.
            if (mCurrentMouseState.LeftButton == ButtonState.Released && 
                mPreviousMouseState.LeftButton == ButtonState.Pressed)
            {
                // Mouse stores its info as int. Needs to be converted to vector.
                Vector2 newMouse = new Vector2(mCurrentMouseState.X, mCurrentMouseState.Y);

                // Create a new Tap Gesture, since the mouse was pressed and released.
                // Time Stamp is missing.
                //GestureSample g = new GestureSample(GestureType.Tap, TimeSpan.Zero, newMouse, Vector2.Zero, Vector2.Zero, Vector2.Zero);
                //DebugMessageDisplay.pInstance.AddConstantMessage("Mouse Clicked: " + CameraManager.pInstance.ProjectMouseToWorldSpace(newMouse) + ", " + mCurrentMouseState);

                //mCurrentGestureSamples.Add(g);

                ///

                MouseState start = mMouseHoldHistory.Dequeue();

                // Where was the mouse when the button was first clicked? This will become the start
                // of the flick delta.
                Vector2 startClick = new Vector2(start.X, start.Y);

                // A magic number to put actual delta into WP scale.
                const Single WPMagicNumber = 10.0f;

                // Find the delta between where we started and where we released. This gets multiplied by
                // a magic number because on Windows Phone, the delta is huge, and this simplifies code everywhere
                // else in the game if we have a consistent scale.
                // TODO: This should be based on a recent history not the starting point to avoid curve cases
                //       giving odd results. It would almost mean velocity of the flick mattters which it does not
                //       right now.
                Vector2 flickDelta = (newMouse - startClick) * WPMagicNumber;

                // Limit really small flicks.
                Single minFlickLengthSq = (Single)System.Math.Pow(100.0, 2.0);
                if (flickDelta.LengthSquared() >= minFlickLengthSq)
                {
                    // Build the Flick Gesture. 
                    // Time Stamp is missing.
                    GestureSample g = new GestureSample(GestureType.Flick, TimeSpan.Zero, Vector2.Zero, Vector2.Zero, flickDelta, Vector2.Zero);
                    //DebugMessageDisplay.pInstance.AddConstantMessage("Mouse Flicked: " + flickDelta);

                    mCurrentGestureSamples.Add(g);
                }

                // Clear the history regardless, since the mouse has been released.
                mMouseHoldHistory.Clear();
            }
#endif
        }

        /// <summary>
        /// This needs to be called at the end of each update.  It gives the InputManager a chance
        /// to store some data about what happened this frame.
        /// </summary>
        public void UpdateEnd()
        {
            #if ALLOW_GARBAGE
            MouseState[] array = new MouseState[mDebugMouseHist.Count];
            mDebugMouseHist.CopyTo(array, 0);

            for (int i = 0; i < array.Length - 1; i++) 
            {
                MBHEngine.Math.LineSegment line = new MBHEngine.Math.LineSegment ();
                line.pPointA = CameraManager.pInstance.ProjectMouseToWorldSpace(new Vector2 (array [i].X, array [i].Y));
                line.pPointB = CameraManager.pInstance.ProjectMouseToWorldSpace(new Vector2 (array [i+1].X, array [i+1].Y));
                DebugShapeDisplay.pInstance.AddSegment (line, Color.Blue);
            }
            #endif

            mPreviousKeyboardState = Keyboard.GetState();
			
            if (mIsControllerLocked == true)
            {
                mPreviousGamePadState = mCurrentGamePadState;
            }

            mPreviousMouseState = mCurrentMouseState;

            mCurrentGestureSamples.Clear();
        }

        public bool CheckGesture(GestureType type, ref GestureSample gesture)
        {
            for (Int32 i = 0; i < mCurrentGestureSamples.Count; i++)
            {
                // I'm assuming there can only be one of each type per frame?
                if (mCurrentGestureSamples[i].GestureType == type)
                {
                    gesture = mCurrentGestureSamples[i];

                    return true;
                }
            }

            return false;
        }

        // Same as the regular CheckAction but assumes that input will not be
        // buffered.
        /// <summary>
        /// Same as the regular CheckAction but assumes that input will not bebuffered.
        /// </summary>
        /// <param name="action">Which action to check, as defined in InputManager.InputActions.</param>
        /// <returns>True if that action happened this frame.</returns>
        public bool CheckAction(InputActions action)
        {
            return CheckAction(action, false);
        }

        // This is the main purpose of this class.  It allows us to check
        // for an action rather than a specific device button.
        //
        /// <summary>
        /// This is the main purpose of this class.  It allows us to check for an action rather 
        /// than a specific device button.
        /// </summary>
        /// <param name="action">Which action to check, as defined in InputManager.InputActions.</param>
        /// <param name="buffer">True if input should only be registers once per press (prevent spamming).</param>
        /// <returns>True if that action happened this frame.</returns>
        public bool CheckAction(InputActions action, bool buffer)
        {
#if WINDOWS
            // First let's check the keyboard.
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(mKeyboardActionMap[(int)action]))
            {
                // The button has been pressed this frame, but that might not be the end our checks
                // if the user has requested the input to be buffered.
                if(buffer)
                {
                    // Was this key down last frame?
                    if (mPreviousKeyboardState.IsKeyDown(mKeyboardActionMap[(int)action]))
                    {
                        // If it was then it needs to be ignored this frame.
                        return false;
                    }
                }

                // If we make it to here, the button press is valid.
                return true;
            }
#endif // !WINDOWS

            // Now let's do the gamepad
            if (mIsControllerLocked == false)
            {
                // Need to detect which controller has pressed start
                for (int i = 0; i < MAX_CONTROLLER_COUNT; i++)
                {
                    if (action == InputActions.START &&
                        GamePad.GetState((PlayerIndex)i).Buttons.Start == ButtonState.Pressed)
                    {
                        mIsControllerLocked = true;
                        mActiveControllerIndex = (PlayerIndex)i;
                        mPreviousGamePadState = mCurrentGamePadState = GamePad.GetState(mActiveControllerIndex);
                        return true;
                    }
                }
                return false;
            }
            
            // Dump brute force checks.  Not as simple as keyboard because we
            // need to handle buttons, dpad, and thumbstick.
            //
            switch (action)
            {
                case InputActions.A:
                    {
                        return CheckButtonState(mCurrentGamePadState.Buttons.A, 
                                                mPreviousGamePadState.Buttons.A, 
                                                ButtonState.Pressed, 
                                                buffer);
                    }
                case InputActions.B:
                    {
                        return CheckButtonState(mCurrentGamePadState.Buttons.B,
                                                mPreviousGamePadState.Buttons.B,
                                                ButtonState.Pressed,
                                                buffer);
                    }
                case InputActions.X:
                    {
                        return CheckButtonState(mCurrentGamePadState.Buttons.X,
                                                mPreviousGamePadState.Buttons.X,
                                                ButtonState.Pressed,
                                                buffer);
                    }
                case InputActions.Y:
                    {
                        return CheckButtonState(mCurrentGamePadState.Buttons.Y,
                                                mPreviousGamePadState.Buttons.Y,
                                                ButtonState.Pressed,
                                                buffer);
                    }
                case InputActions.START:
                    {
                        return CheckButtonState(mCurrentGamePadState.Buttons.Start,
                                                mPreviousGamePadState.Buttons.Start,
                                                ButtonState.Pressed,
                                                buffer);
                    }
                case InputActions.BACK:
                    {
                        return CheckButtonState(mCurrentGamePadState.Buttons.Back,
                                                mPreviousGamePadState.Buttons.Back,
                                                ButtonState.Pressed,
                                                buffer);
                    }
                case InputActions.R1:
                    {
                        return CheckButtonState(mCurrentGamePadState.Buttons.RightShoulder,
                                                mPreviousGamePadState.Buttons.RightShoulder,
                                                ButtonState.Pressed,
                                                buffer);
                    }
                case InputActions.L1:
                    {
                        return CheckButtonState(mCurrentGamePadState.Buttons.LeftShoulder,
                                                mPreviousGamePadState.Buttons.LeftShoulder,
                                                ButtonState.Pressed,
                                                buffer);
                    }
                case InputActions.R2:
                    {
                        if (mCurrentGamePadState.Triggers.Right >= 0.1f)
                        {
                            if (!buffer || mPreviousGamePadState.Triggers.Right < 0.1f)
                            {
                                return true;
                            }
                        }
                        
                        return false;
                    }
                case InputActions.L2:
                    {
                        if (mCurrentGamePadState.Triggers.Left >= 0.1f)
                        {
                            if (!buffer || mPreviousGamePadState.Triggers.Left < 0.1f)
                            {
                                return true;
                            }
                        }

                        return false;
                    }
                case InputActions.L3:
                    {
                        return CheckButtonState(mCurrentGamePadState.Buttons.LeftStick,
                                                mPreviousGamePadState.Buttons.LeftStick,
                                                ButtonState.Pressed,
                                                buffer);
                    }
                case InputActions.DP_LEFT:
                    {
                        return CheckButtonState(mCurrentGamePadState.DPad.Left,
                                                mPreviousGamePadState.DPad.Left,
                                                ButtonState.Pressed,
                                                buffer);
                    }
                case InputActions.DP_RIGHT:
                    {
                        return CheckButtonState(mCurrentGamePadState.DPad.Right,
                                                mPreviousGamePadState.DPad.Right,
                                                ButtonState.Pressed,
                                                buffer);
                    }
                case InputActions.DP_UP:
                    {
                        return CheckButtonState(mCurrentGamePadState.DPad.Up,
                                                mPreviousGamePadState.DPad.Up,
                                                ButtonState.Pressed,
                                                buffer);
                    }
                case InputActions.DP_DOWN:
                    {
                        return CheckButtonState(mCurrentGamePadState.DPad.Down,
                                                mPreviousGamePadState.DPad.Down,
                                                ButtonState.Pressed,
                                                buffer);
                    }
                case InputActions.LA_LEFT:
                    {
                        return CheckAnalogState(mCurrentGamePadState.ThumbSticks.Left.X, 
                                                mPreviousGamePadState.ThumbSticks.Left.X,
                                                -0.1f,
                                                buffer) ||
                               CheckButtonState(mCurrentGamePadState.DPad.Left,
                                                mPreviousGamePadState.DPad.Left,
                                                ButtonState.Pressed,
                                                buffer);
                    }
                case InputActions.LA_RIGHT:
                    {
                        return CheckAnalogState(mCurrentGamePadState.ThumbSticks.Left.X,
                                                mPreviousGamePadState.ThumbSticks.Left.X,
                                                0.1f,
                                                buffer) ||
                               CheckButtonState(mCurrentGamePadState.DPad.Right,
                                                mPreviousGamePadState.DPad.Right,
                                                ButtonState.Pressed,
                                                buffer);
                    }
                case InputActions.LA_UP:
                    {
                        return CheckAnalogState(mCurrentGamePadState.ThumbSticks.Left.Y,
                                                mPreviousGamePadState.ThumbSticks.Left.Y,
                                                0.1f,
                                                buffer) ||
                               CheckButtonState(mCurrentGamePadState.DPad.Up,
                                                mPreviousGamePadState.DPad.Up,
                                                ButtonState.Pressed,
                                                buffer);
                    }
                case InputActions.LA_DOWN:
                    {
                        return CheckAnalogState(mCurrentGamePadState.ThumbSticks.Left.Y,
                                                mPreviousGamePadState.ThumbSticks.Left.Y,
                                                -0.1f,
                                                buffer) ||
                               CheckButtonState(mCurrentGamePadState.DPad.Down,
                                                mPreviousGamePadState.DPad.Down,
                                                ButtonState.Pressed,
                                                buffer);
                    }
                case InputActions.RA_LEFT:
                    {
                        return CheckAnalogState(mCurrentGamePadState.ThumbSticks.Right.X,
                                                mPreviousGamePadState.ThumbSticks.Right.X,
                                                -0.1f,
                                                buffer);
                    }
                case InputActions.RA_RIGHT:
                    {
                        return CheckAnalogState(mCurrentGamePadState.ThumbSticks.Right.X,
                                                mPreviousGamePadState.ThumbSticks.Right.X,
                                                0.1f,
                                                buffer);
                    }
                case InputActions.RA_UP:
                    {
                        return CheckAnalogState(mCurrentGamePadState.ThumbSticks.Right.Y,
                                                mPreviousGamePadState.ThumbSticks.Right.Y,
                                                0.1f,
                                                buffer);
                    }
                case InputActions.RA_DOWN:
                    {
                        return CheckAnalogState(mCurrentGamePadState.ThumbSticks.Right.Y,
                                                mPreviousGamePadState.ThumbSticks.Right.Y,
                                                -0.1f,
                                                buffer);
                    }
            };            

            return false;
        }

        /// <summary>
        /// Helper function for determining if a button has been pressed.
        /// </summary>
        /// <param name="currentState">The current state of a particular button.</param>
        /// <param name="previousState">The state that same button was in last frame.</param>
        /// <param name="targetState">Which state we are checking for.</param>
        /// <param name="buffer">True if we require the state to have changed to count.</param>
        /// <returns>True if this target state was achieved.</returns>
        private bool CheckButtonState(ButtonState currentState, ButtonState previousState, ButtonState targetState, bool buffer)
        {
            if (currentState == targetState)
            {
                if (!buffer || previousState != targetState)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Helper function for determining if a alalog has been pressed.
        /// </summary>
        /// <param name="currentState">The current state of an analog stick.</param>
        /// <param name="previousState">That state of that stick last frame.</param>
        /// <param name="deadZone">The amount of dead zone on the stick.  This should also indicate the direction to check against.</param>
        /// <param name="buffer">True if we require the state to have changed to count.</param>
        /// <returns>True if this target state was achieved.</returns>
        private bool CheckAnalogState(float currentState, float previousState, float deadZone, bool buffer)
        {
            if (System.Math.Abs(currentState) >= System.Math.Abs(deadZone) && CheckAnalogDirection(currentState, deadZone))
            {
                if (!buffer || System.Math.Abs(previousState) < System.Math.Abs(deadZone))
                {
                    return true;
                }
            }

            return false;

        }

        /// <summary>
        /// The CheckAnalogState does its calculations based on absolute values, so we need
        /// an additional check to make sure we are actually pressing in the right directions.
        /// </summary>
        /// <param name="currentState">The currrent state of the analog state, as a signed value.</param>
        /// <param name="direction">The direction to check against.</param>
        /// <returns></returns>
        private bool CheckAnalogDirection(float currentState, float direction)
        {
            if (direction < 0)
            {
                return (currentState < 0);
            }

            if( direction > 0 )
            {
                return (currentState > 0 );
            }

            System.Diagnostics.Debug.Assert(false, "Direction must be a non-zero number.");

            return false;
        }

        /// <summary>
        /// Gets a custom built GamePadThumbSticks object containing directional information which
        /// also incorperates keyboard presses as well (although those are either on or off, but the
        /// value is normalized).
        /// </summary>
        /// <returns>A GamePadThumbSticks state which contains directional informatin of both GamePad and Keyboard.</returns>
        public GamePadThumbSticks GetDirectionalInfo()
        {
            KeyboardState keyboardState = Keyboard.GetState();

            Vector2 leftThumb = Vector2.Zero;
            Vector2 rightThumb = Vector2.Zero;

            Boolean keyboard_used = false;

            // LA as Keyboard.
            //
            if (keyboardState.IsKeyDown(mKeyboardActionMap[(int)InputActions.LA_LEFT]))
            {
                leftThumb.X = -1.0f;
                keyboard_used = true;
            }
            else if (keyboardState.IsKeyDown(mKeyboardActionMap[(int)InputActions.LA_RIGHT]))
            {
                leftThumb.X = 1.0f;
                keyboard_used = true;
            }

            if (keyboardState.IsKeyDown(mKeyboardActionMap[(int)InputActions.LA_DOWN]))
            {
                leftThumb.Y = -1.0f;
                keyboard_used = true;
            }
            else if (keyboardState.IsKeyDown(mKeyboardActionMap[(int)InputActions.LA_UP]))
            {
                leftThumb.Y = 1.0f;
                keyboard_used = true;
            }

            if (keyboard_used)
            {
                // If we got input from the keyboard it may not be normalized (eg. 1,1), which will allow
                // the player to move faster diagonally then left and right seperatly.
                if (leftThumb != Vector2.Zero) leftThumb.Normalize();
            }
            else
            {
                leftThumb = mCurrentGamePadState.ThumbSticks.Left;
            }

            keyboard_used = false;

            // RA as keyboard.
            //
            if (keyboardState.IsKeyDown(mKeyboardActionMap[(int)InputActions.RA_LEFT]))
            {
                rightThumb.X = -1.0f;
                keyboard_used = true;
            }
            else if (keyboardState.IsKeyDown(mKeyboardActionMap[(int)InputActions.RA_RIGHT]))
            {
                rightThumb.X = 1.0f;
                keyboard_used = true;
            }

            if (keyboardState.IsKeyDown(mKeyboardActionMap[(int)InputActions.RA_DOWN]))
            {
                rightThumb.Y = -1.0f;
                keyboard_used = true;
            }
            else if (keyboardState.IsKeyDown(mKeyboardActionMap[(int)InputActions.RA_UP]))
            {
                rightThumb.Y = 1.0f;
                keyboard_used = true;
            }

            if (keyboard_used)
            {
                // If we got input from the keyboard it may not be normalized (eg. 1,1), which will allow
                // the player to move faster diagonally then left and right seperatly.
                if (rightThumb != Vector2.Zero) rightThumb.Normalize();
            }
            else
            {
                rightThumb = mCurrentGamePadState.ThumbSticks.Right;
            }

            return new GamePadThumbSticks(leftThumb, rightThumb);
        }

        /// <summary>
        /// Access to the static instance of this class.
        /// </summary>
        public static InputManager pInstance
        {
            get
            {
                // If this is the first time this instance has been
                // accessed, we need to allocate it.
                if (mInstance == null)
                {
                    mInstance = new InputManager();
                }

                return mInstance;
            }
        }

        /// <summary>
        /// The currently locked controller index.
        /// </summary>
        public PlayerIndex pActiveControllerIndex
        {
            get
            {
                System.Diagnostics.Debug.Assert((true == mIsControllerLocked), "Controller is not locked");

                return mActiveControllerIndex;
            }
            set
            {
                mActiveControllerIndex = value;
            }
        }

        /// <summary>
        /// Check if the controller has been locked yet.
        /// </summary>
        public bool pIsControllerLocked
        {
            get
            {
                return mIsControllerLocked;
            }
            set
            {
                mIsControllerLocked = value;
            }
        }

        /// <summary>
        /// Get the state of the currently active gamepad.  This is helpful for getting detailed information
        /// that the Input Manager doesn't make readily availble, however, it does not handle things like
        /// buffered input so need to be considered in context.
        /// </summary>
        public GamePadState pActiveGamePadState
        {
            get
            { 
#if __ANDROID__
                return GamePad.GetState(pActiveControllerIndex);
#else
				return GamePad.GetState(pActiveControllerIndex, GamePadDeadZone.Circular);
#endif
            }
        }
    }
}
