using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MBHEngine.GameObject;
using MBHEngine.IO;
using MBHEngine.Math;
using MBHEngine.Input;
using MBHEngine.Debug;
using MBHEngine.Render;
using MBHEngine.World;
using Microsoft.Xna.Framework.Input;
using MBHEngine.Behaviour;
using Microsoft.Xna.Framework.Input.Touch;
using BumpSetSpike.Behaviour;
using BumpSetSpike.Gameflow;

namespace BumpSetSpike
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager mGraphics;
        private SpriteBatch mSpriteBatch;

        /// <summary>
        /// Whether or not to draw debug information.
        /// </summary>
        private Boolean mDebugDrawEnabled;

        /// <summary>
        /// Debug controls for skipping updates calls to help debug in "slow motion".
        /// </summary>
        private Int32 mFrameSkip = 0;
        private Int32 mFameSkipCount = 0;
        private Boolean mSkipKeyIncDown = false;
        private Boolean mSkipKeyDecDown = false;
        private Boolean mFreeze = false;
        private Boolean mFreezeKeyDown = false;
        
        /// <summary>
        /// Constuctor
        /// </summary>
        /// <param name="args">Command-line arguments passed to the executable.</param>
        public Game1()
        {
            //CommandLineManager.pInstance.pArgs = args;

            mGraphics = new GraphicsDeviceManager(this);
            
            // WINDOWS_PHONE = 800x480

#if SMALL_WINDOW
            mGraphics.PreferredBackBufferWidth = 640;
            mGraphics.PreferredBackBufferHeight = 360;
#else
            mGraphics.PreferredBackBufferWidth = 1280; // 1366; // 1280;
            mGraphics.PreferredBackBufferHeight = 720; // 768; // 720;
#endif
            //mGraphics.IsFullScreen = true;
            Content.RootDirectory = "Content";

            // Avoid the "jitter".
            // http://forums.create.msdn.com/forums/p/9934/53561.aspx#53561
            // Set to TRUE so that we can target 30fps to match windows phone.
            // Should be FALSE to fix jitter issue.
            IsFixedTimeStep = true;

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            //mGraphics.GraphicsDevice.PresentationParameters.MultiSampleType = MultiSampleType.TwoSamples;
            //mGraphics.GraphicsDevice.RenderState.MultiSampleAntiAlias = true;
            mGraphics.PreferMultiSampling = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            mSpriteBatch = new SpriteBatch(GraphicsDevice);

            GameObjectManager.pInstance.Initialize(Content, mGraphics);
            GameObject.AddBehaviourCreator(new ClientBehaviourCreator());
            GameObjectFactory.pInstance.Initialize();
            CameraManager.pInstance.Initialize(mGraphics.GraphicsDevice);
            CameraManager.pInstance.pNumBlendFrames = 30;
            StopWatchManager.pInstance.Initialize();

            CameraManager.pInstance.pTargetPosition = new Vector2(0, -100.0f); // -30

            // enable the gestures we care about. you must set EnabledGestures before
            // you can use any of the other gesture APIs.
            // we use both Tap and DoubleTap to workaround a bug in the XNA GS 4.0 Beta
            // where some Taps are missed if only Tap is specified.
            TouchPanel.EnabledGestures =
                GestureType.Hold |
                GestureType.Tap |
                GestureType.DoubleTap |
                GestureType.FreeDrag |
                GestureType.Flick |
                GestureType.Pinch;

#if DEBUG
            // By default, in DEBUG the debug drawing is enabled.
            mDebugDrawEnabled = true;
#else
            // In release it is not.
            mDebugDrawEnabled = true;
#endif
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            mSpriteBatch = new SpriteBatch(GraphicsDevice);

            // Add any objects desired to the Game Object Factory.  These will be allocated now and can
            // be retrived later without any heap allocations.
            //
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\Items\\Dust\\Dust", 64);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\Items\\Kabooom\\Kabooom", 4);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\Items\\Blood\\Blood", 4);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\NumFont\\NumFont", 128);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\NumFontUI\\NumFontUI", 128);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\PointDisplay\\PointDisplay", 32);
            
            // The tiled background image that travels will the player creating the illusion of
            // an infinite background image.
            GameObject bg = new GameObject();
            MBHEngine.Behaviour.Behaviour t = new InfiniteBG(bg, null);
            bg.AttachBehaviour(t);
            bg.pRenderPriority = 20;
            GameObjectManager.pInstance.Add(bg);
            

            // Create the level.
            WorldManager.pInstance.Initialize();

            // Debug display for different states in the game.  This by creating new behaviours, additional
            // stats can be displayed.
            GameObject debugStatsDisplay = new GameObject();
            MBHEngine.Behaviour.Behaviour fps = new MBHEngine.Behaviour.FrameRateDisplay(debugStatsDisplay, null);
            debugStatsDisplay.AttachBehaviour(fps);
            GameObjectManager.pInstance.Add(debugStatsDisplay);

            // The player himself.
            GameObject player = new GameObject("GameObjects\\Characters\\Player\\Player");
            GameObjectManager.pInstance.Add(player);

            // Store the player for easy access.
            GameObjectManager.pInstance.pPlayer = player;

            GroundShadow.SetTargetMessage setTarg = new GroundShadow.SetTargetMessage();

            GameObject shadow = new GameObject("GameObjects\\Items\\PlayerShadow\\PlayerShadow");
            GameObjectManager.pInstance.Add(shadow);
            setTarg.mTarget_In = player;
            shadow.OnMessage(setTarg);

            GameObject ball = new GameObject("GameObjects\\Items\\Ball\\Ball");
            GameObjectManager.pInstance.Add(ball);
            shadow = new GameObject("GameObjects\\Items\\BallShadow\\BallShadow");
            GameObjectManager.pInstance.Add(shadow);
            setTarg.mTarget_In = ball;
            shadow.OnMessage(setTarg);

            GameObject partner = new GameObject("GameObjects\\Characters\\Partner\\Partner");
            GameObjectManager.pInstance.Add(partner); 
            shadow = new GameObject("GameObjects\\Items\\PlayerShadow\\PlayerShadow");
            GameObjectManager.pInstance.Add(shadow);
            setTarg.mTarget_In = partner;
            shadow.OnMessage(setTarg);

            GameObject opponent = new GameObject("GameObjects\\Characters\\Opponent\\Opponent");
            GameObjectManager.pInstance.Add(opponent);
            shadow = new GameObject("GameObjects\\Items\\PlayerShadow\\PlayerShadow");
            GameObjectManager.pInstance.Add(shadow);
            setTarg.mTarget_In = opponent;
            shadow.OnMessage(setTarg);

            opponent = new GameObject("GameObjects\\Characters\\Opponent\\Opponent");
            opponent.pPosX = 75.0f;
            GameObjectManager.pInstance.Add(opponent);
            shadow = new GameObject("GameObjects\\Items\\PlayerShadow\\PlayerShadow");
            GameObjectManager.pInstance.Add(shadow);
            setTarg.mTarget_In = opponent;
            shadow.OnMessage(setTarg);

            GameObjectManager.pInstance.Add(new GameObject("GameObjects\\Items\\Net\\Net"));
            GameObjectManager.pInstance.Add(new GameObject("GameObjects\\Items\\Court\\Court"));

            GameObjectManager.pInstance.Add(new GameObject("GameObjects\\UI\\TitleScreen\\TitleScreen"));

            Single x = (mGraphics.GraphicsDevice.Viewport.Width / CameraManager.pInstance.pZoomScale) - 20.0f;
            Single y = (mGraphics.GraphicsDevice.Viewport.Height / CameraManager.pInstance.pZoomScale) - 4.0f;

            GameObject label = new GameObject("GameObjects\\UI\\ScoreLabel\\ScoreLabel");
            label.pPosY = y;
            GameObjectManager.pInstance.Add(label);

            GameObject count = new GameObject("GameObjects\\UI\\HitCountDisplay\\HitCountDisplay");
            count.pPosY = y;
            GameObjectManager.pInstance.Add(count);

            GameObject recordLabel = new GameObject("GameObjects\\UI\\HiScoreLabel\\HiScoreLabel");
            recordLabel.pPosX = x;
            recordLabel.pPosY = y;
            GameObjectManager.pInstance.Add(recordLabel);

            GameObject record = new GameObject("GameObjects\\UI\\HitCountDisplayRecord\\HitCountDisplayRecord");
            record.pPosX = x;
            record.pPosY = y;
            GameObjectManager.pInstance.Add(record);
                        
            // The vingette effect used to dim out the edges of the screen.
            //GameObject ving = new GameObject("GameObjects\\Interface\\Vingette\\Vingette");
#if SMALL_WINDOW
            //ving.pScale = new Vector2(0.5f, 0.5f);
#endif
            //GameObjectManager.pInstance.Add(ving);

            // Add the HUD elements.
            //
            //GameObjectManager.pInstance.Add(new GameObject("GameObjects\\Interface\\HUD\\PlayerHealthBar\\PlayerHealthBar"));

            LeaderBoardManager.pInstance.Initialize();

            SaveGameManager.pInstance.Inititalize();

            SaveGameManager.pInstance.ReadSaveGameXML();
            //SaveGameManager.pInstance.WriteSaveGameXML();
            //SaveGameManager.pInstance.ReadSaveGameXML();

            DebugMessageDisplay.pInstance.AddConstantMessage("Game Load Complete.");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            InputManager.pInstance.UpdateBegin();

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                SaveGameManager.pInstance.WriteSaveGameXML();

                this.Exit();
            }

            // we use raw touch points for selection, since they are more appropriate
            // for that use than gestures. so we need to get that raw touch data.
            TouchCollection touches = TouchPanel.GetState();

            // see if we have a new primary point down. when the first touch
            // goes down, we do hit detection to try and select one of our sprites.
            if (touches.Count > 0 && touches[0].State == TouchLocationState.Pressed)
            {
                // convert the touch position into a Point for hit testing
                Point touchPoint = new Point((int)touches[0].Position.X, (int)touches[0].Position.Y);

                //CameraManager.pInstance.pTargetPosition = touches[0].Position;
            }

            /*
            // next we handle all of the gestures. since we may have multiple gestures available,
            // we use a loop to read in all of the gestures. this is important to make sure the 
            // TouchPanel's queue doesn't get backed up with old data
            while (TouchPanel.IsGestureAvailable)
            {
                // read the next gesture from the queue
                GestureSample gesture = TouchPanel.ReadGesture();

                // we can use the type of gesture to determine our behavior
                switch (gesture.GestureType)
                {

                    // on drags, we just want to move the selected sprite with the drag
                    case GestureType.FreeDrag:
                    {
                        CameraManager.pInstance.pTargetPosition -= gesture.Delta / CameraManager.pInstance.pZoomScale;
                    }
                    break;
                }
            }
            */
#if DEBUG
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.OemPlus))
            {
                if (!mSkipKeyDecDown)
                {
                    mFrameSkip = Math.Max(mFrameSkip - 1, 0);
                }

                mSkipKeyDecDown = true;
            }
            else
            {
                mSkipKeyDecDown = false;
            }

            if (keyboardState.IsKeyDown(Keys.OemMinus))
            {
                if (!mSkipKeyIncDown)
                {
                    mFrameSkip++;
                }

                mSkipKeyIncDown = true;
            }
            else
            {
                mSkipKeyIncDown = false;
            }

            if (keyboardState.IsKeyDown(Keys.D0))
            {
                if (!mFreezeKeyDown)
                {
                    mFreeze ^= true;
                }

                mFreezeKeyDown = true;
            }
            else
            {
                mFreezeKeyDown = false;
            }
#endif

            // If we are skipping frames, check if enough have passed before doing updates.
            if (mFameSkipCount >= mFrameSkip && !mFreeze)
            {
                DebugMessageDisplay.pInstance.ClearDynamicMessages();
                DebugShapeDisplay.pInstance.Update();

                //DebugMessageDisplay.pInstance.AddDynamicMessage("Game-Time Delta: " + gameTime.ElapsedGameTime.TotalSeconds);
                //DebugMessageDisplay.pInstance.AddDynamicMessage("Path Find - Unused: " + MBHEngine.PathFind.GenericAStar.Planner.pNumUnusedNodes);
                //DebugMessageDisplay.pInstance.AddDynamicMessage("Graph Neighbour - Unused: " + MBHEngine.PathFind.GenericAStar.GraphNode.pNumUnusedNeighbours);
                //DebugMessageDisplay.pInstance.AddDynamicMessage("NavMesh - Unused: " + MBHEngine.PathFind.HPAStar.NavMesh.pUnusedGraphNodes);
                //DebugMessageDisplay.pInstance.AddDynamicMessage("Frame Skip: " + mFrameSkip);

                mFameSkipCount = 0;
                StopWatchManager.pInstance.Update();
                GameObjectManager.pInstance.Update(gameTime);
            }
            else
            {
                mFameSkipCount++; 
            }

            if (mDebugDrawEnabled)
            {
                // This does some pretty expensive stuff, so only do it when it is really useful.
                GameObjectPicker.pInstance.Update(gameTime, (mFameSkipCount == 0));
            }

            InputManager.pInstance.UpdateEnd();
            CameraManager.pInstance.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // First draw all the objects managed by the game object manager.
            GameObjectManager.pInstance.Render(mSpriteBatch, (mFameSkipCount == 0));

            if (mDebugDrawEnabled)
            {
                DebugShapeDisplay.pInstance.Render();

                // We need to go back to standard alpha blend before drawing the debug layer.
                mSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                DebugMessageDisplay.pInstance.Render(mSpriteBatch);
                mSpriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
