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
using Microsoft.Xna.Framework.Media;

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
        /// Background music for the game.
        /// </summary>
        private Song mMusic;

        /// <summary>
        /// Whether or not to draw debug information.
        /// </summary>
        private Boolean mDebugDrawEnabled;

        /// <summary>
        /// Debug controls for skipping updates calls to help debug in "slow motion".
        /// </summary>
        private Int32 mFrameSkip = 0;
        private Int32 mFameSkipCount = 0;
        private Boolean mFreeze = false;
#if DEBUG
        private Boolean mSkipKeyIncDown = false;
        private Boolean mSkipKeyDecDown = false;
        private Boolean mFreezeKeyDown = false;
#endif

        public Game1()
        {
            Reset(null);
        }

        /// <summary>
        /// Constuctor
        /// </summary>
        /// <param name="args">Command-line arguments passed to the executable.</param>
        public Game1(string[] args)
        {
            Reset(args);
        }

        public void Reset(string[] args)
        {
            CommandLineManager.pInstance.pArgs = args;

            mGraphics = new GraphicsDeviceManager(this);

            // WINDOWS_PHONE = 800x480

#if WINDOWS
#if SMALL_WINDOW
            mGraphics.PreferredBackBufferWidth = 640;
            mGraphics.PreferredBackBufferHeight = 360;
#else
            mGraphics.PreferredBackBufferWidth = 1280; // 1366; // 1280;
            mGraphics.PreferredBackBufferHeight = 720; // 768; // 720;
#endif
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

            // THIS BREAKS ON WP8!
            //mGraphics.PreferMultiSampling = true;
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
            ScoreManager.pInstance.Initialize();
            TutorialManager.pInstance.Initialize();
            DebugShapeDisplay.pInstance.Initialize();
            DebugMessageDisplay.pInstance.Initialize();

            CameraManager.pInstance.pTargetPosition = new Vector2(0, -100.0f); // -30

            // enable the gestures we care about. you must set EnabledGestures before
            // you can use any of the other mGesture APIs.
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
            mDebugDrawEnabled = false;
#endif

            IsMouseVisible = mDebugDrawEnabled;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Add any objects desired to the Game Object Factory.  These will be allocated now and can
            // be retrived later without any heap allocations.
            //
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\Items\\Dust\\Dust", 64);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\Items\\Kabooom\\Kabooom", 4);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\Items\\Blood\\Blood", 4);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\NumFont\\NumFont", 128);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\NumFontUI\\NumFontUI", 128);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\PointDisplay\\PointDisplay", 32);
            GameObjectFactory.pInstance.AddTemplate("GameObjects\\UI\\TapStart\\TapStart", 1);
            
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
            GameObjectManager.pInstance.Add(new GameObject("GameObjects\\UI\\MusicCredit\\MusicCredit"));

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

            x = ((mGraphics.GraphicsDevice.Viewport.Width * 0.5f) / CameraManager.pInstance.pZoomScale);
            y = ((mGraphics.GraphicsDevice.Viewport.Height * 0.5f) / CameraManager.pInstance.pZoomScale);

            GameObject gameOver = new GameObject("GameObjects\\UI\\GameOver\\GameOver");
            gameOver.pPosX = x;
            gameOver.pPosY = y;
            GameObjectManager.pInstance.Add(gameOver);

            GameObject newHighScore = new GameObject("GameObjects\\UI\\NewHighScore\\NewHighScore");
            newHighScore.pPosX = x;
            newHighScore.pPosY = y + 32;
            GameObjectManager.pInstance.Add(newHighScore);

            // The vingette effect used to dim out the edges of the screen.
            //GameObject ving = new GameObject("GameObjects\\Interface\\Vingette\\Vingette");
#if SMALL_WINDOW
            //ving.pScale = new Vector2(0.5f, 0.5f);
#endif
            //GameObjectManager.pInstance.Add(ving);

            // Add the HUD elements.
            //
            //GameObjectManager.pInstance.Add(new GameObject("GameObjects\\Interface\\HUD\\PlayerHealthBar\\PlayerHealthBar"));

            mMusic = Content.Load<Song>("Audio\\Music\\RoccoW_-_Pumped");

            Boolean musicDisabled = CommandLineManager.pInstance["DisableMusic"] != null;

#if WINDOWS_PHONE && DEBUG
            musicDisabled = true;
#endif

            // Only play music if the user isn't already listening to something.
            if (MediaPlayer.State == MediaState.Stopped && !musicDisabled)
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(mMusic);
            }

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

            // Toggle the debug drawing with a click of the left stick.
            if (InputManager.pInstance.CheckAction(InputManager.InputActions.L3, true))
            {
                mDebugDrawEnabled ^= true;

                // When debug draw is enabled, turn on the hardware mouse so that things like the
                // GameObjectPicker work better.
                IsMouseVisible = mDebugDrawEnabled;
            }
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

/*
            GestureSample samp = new GestureSample();


            if (InputManager.pInstance.CheckGesture(GestureType.Tap, ref samp))
            {
                Vector2 proj = CameraManager.pInstance.ProjectMouseToWorldSpace(new Vector2(samp.Position.X, samp.Position.Y));
                DebugMessageDisplay.pInstance.AddConstantMessage("Touch: " + samp.Position + "/" + proj);

                GameObject t = GameObjectFactory.pInstance.GetTemplate("GameObjects\\Items\\Blood\\Blood");
                t.pPosition = proj;
                GameObjectManager.pInstance.Add(t);
            }
*/
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

            TutorialManager.pInstance.Update(gameTime);
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
                mSpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                DebugMessageDisplay.pInstance.Render(mSpriteBatch);
                mSpriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
