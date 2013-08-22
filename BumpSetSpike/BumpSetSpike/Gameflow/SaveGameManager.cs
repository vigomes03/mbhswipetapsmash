using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;
using MBHEngine.Debug;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using MBHEngine.Behaviour;

namespace BumpSetSpike.Gameflow
{
    /// <summary>
    /// Used for saving and loading the game.
    /// </summary>
    public class SaveGameManager
    {
        /// <summary>
        /// Anything that gets saved needs to go in here.
        /// </summary>
        public struct SaveGameData
        {
            /// <summary>
            /// Top scores in the game.
            /// </summary>
            public LeaderBoardManager.Records mRecords;

            /// <summary>
            /// Has the tutorial been played yet?
            /// </summary>
            public Boolean mTutorialComplete;
        }

        /// <summary>
        /// A message that can be broadcast to all objects to let them know a forced save
        /// is about to happen and they should update any relavent data as needed.
        /// </summary>
        public class ForceUpdateSaveDataMessage : BehaviourMessage
        {
            /// <summary>
            /// See parent.
            /// </summary>
            public override void Reset()
            {
            }
        }

        /// <summary>
        /// Singleton.
        /// </summary>
        private static SaveGameManager mInstance;

#if WINDOWS_PHONE
        /// <summary>
        /// Name of the file where we will save our data.
        /// </summary>
        private String fileName = "SwipeTapSmash";

        /// <summary>
        /// The data that will get saved and loaded.
        /// </summary>
        private SaveGameData mSaveData;
#endif

        /// <summary>
        /// Call this before using the singleton.
        /// </summary>
        public void Inititalize()
        {
#if WINDOWS_PHONE
            mSaveData = new SaveGameData();
#endif
        }

        /// <summary>
        /// Access to the singleton.
        /// </summary>
        public static SaveGameManager pInstance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new SaveGameManager();
                }

                return mInstance;
            }
        }

        /// <summary>
        /// Load the save game.
        /// </summary>
        /// <remarks>Only works on Windows Phone.</remarks>
        public void ReadSaveGameXML()
        {
#if WINDOWS_PHONE
            try
            {
                using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream stream = myIsolatedStorage.OpenFile(fileName, FileMode.Open))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(SaveGameData));
                        mSaveData = (SaveGameData)serializer.Deserialize(stream);
                        LeaderBoardManager.pInstance.SetRecords(mSaveData.mRecords);
                        TutorialManager.pInstance.pTutorialCompleted = mSaveData.mTutorialComplete;
                    }
                }
            }
            catch
            {
                // Something
            }
#endif
        }

        /// <summary>
        /// Saves the current state of the game.
        /// </summary>
        /// <remarks>Only works on Windows Phone.</remarks>
        public void WriteSaveGameXML()
        {
#if WINDOWS_PHONE
            // Write to the Isolated Storage
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;

            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = myIsolatedStorage.OpenFile(fileName, FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SaveGameData));
                    using (XmlWriter xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
                    {
                        mSaveData.mRecords = LeaderBoardManager.pInstance.GetRecords();
                        mSaveData.mTutorialComplete = TutorialManager.pInstance.pTutorialCompleted;
                        serializer.Serialize(xmlWriter, mSaveData);
                    }
                }
            }
#endif
        }

#if false
        public void ReadSaveGame()
        {
            // open isolated storage, and load data from the savefile if it exists.
#if WINDOWS_PHONE
            using (IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication())
#else
            using (IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForDomain())
#endif
            {
                if (savegameStorage.FileExists(fileName))
                {
                    using (IsolatedStorageFileStream fs = savegameStorage.OpenFile(fileName, System.IO.FileMode.Open))
                    {
                        if (fs != null)
                        {
                            // Reload the saved high-score data.
                            byte[] saveBytes = new byte[4];
                            int count = fs.Read(saveBytes, 0, 4);
                            if (count > 0)
                            {
                                Int32 score = System.BitConverter.ToInt32(saveBytes, 0);

                                DebugMessageDisplay.pInstance.AddConstantMessage("Score loaded: " + score);
                            }
                        }
                    }
                }
            }
        }

        public void WriteSaveGame()
        {
            // Save the game state (in this case, the high score).
#if WINDOWS_PHONE
            IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication();
#else
            IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForDomain();
#endif

            // open isolated storage, and write the savefile.
            IsolatedStorageFileStream fs = null;
            using (fs = savegameStorage.CreateFile(fileName))
            {
                if (fs != null)
                {
                    // just overwrite the existing info for this example.
                    byte[] bytes = System.BitConverter.GetBytes(50);
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
        }
#endif
    }
}
