using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Gms.Plus;
using Android.Gms.Common;
using Android.Media;
using Android.Gms.Plus.Model.People;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using BumpSetSpike;
using Android.Gms.Games;
using MBHEngine.GameObject;
using System.Diagnostics;
using Xamarin.InAppBilling;
using Xamarin.InAppBilling.Utilities;
using System.Threading.Tasks;
using MBHEngine.Trial;

namespace BumpSetSpike_Android
{
    [Activity (Label = "Swipe Tap Smash", 
		MainLauncher = true,
        Icon = "@drawable/icon_large",
		Theme = "@style/Theme.Splash",
        AlwaysRetainTaskState = true,
        LaunchMode = Android.Content.PM.LaunchMode.SingleTask, // SingleTask means we only run a single instance, but can open other Activities (eg. Google+)
		ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation |
		Android.Content.PM.ConfigChanges.KeyboardHidden |
        Android.Content.PM.ConfigChanges.Keyboard)]
    public class Activity1 : AndroidGameActivity, IGooglePlayServicesClientConnectionCallbacks, IGooglePlayServicesClientOnConnectionFailedListener
    {
        // Aribitrary numbers just used for identifying requests to the Google services.
        public static int REQUEST_CODE_RESOLVE_ERR = 9000;
        public static int REQUEST_LEADERBOARD      = 9001;
        public static int REQUEST_ACHIEVEMENTS     = 9002;

        // The main interface for GooglePlay services.
        private GamesClient mGooglePlayClient;

        // Connection to the Google Play billing service.
        private InAppBillingServiceConnection mBillingConnection;

        // The list of products available for this app.
        private IList<Product> mProducts;

        // Tracks what happened last time we tried to log in (this session).
        private ConnectionResult mConnectionResult;

        private string mPremiumUpgradeProductID = ReservedTestProductIDs.Purchased;
        /*
        "premium_upgrade",
        ReservedTestProductIDs.Purchased,
        ReservedTestProductIDs.Canceled,
        ReservedTestProductIDs.Refunded,
        ReservedTestProductIDs.Unavailable,
        */

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="bundle"></param>
        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            // Create the interface used to interact with Google Play.
            mGooglePlayClient = new GamesClient.Builder(this, this, this)
                .SetGravityForPopups((int)(GravityFlags.Bottom | GravityFlags.CenterHorizontal))
                .Create();

            pGooglePlayClient.RegisterConnectionCallbacks (this);
            pGooglePlayClient.IsConnectionFailedListenerRegistered (this);

            // Create a new connection to the Google Play Service
            mBillingConnection = new InAppBillingServiceConnection (this, InAppBillingKey.Key);
            pBillingConnection.OnConnected += async () => {

                // Clear out the test purchase so that it can be tested again. Remove this 
                // to test starting with test purchase already completed.
                bool response = pBillingConnection.BillingHandler.ConsumePurchase("inapp:"+ PackageName +":android.test.purchased");

                // Attach to the various error handlers to report issues
                pBillingConnection.BillingHandler.OnGetProductsError += (int responseCode, Bundle ownedItems) => {
                    Console.WriteLine("Error getting products");
                };

                pBillingConnection.BillingHandler.OnInvalidOwnedItemsBundleReturned += (Bundle ownedItems) => {
                    Console.WriteLine("Invalid owned items bundle returned");
                };

                pBillingConnection.BillingHandler.OnProductPurchasedError += (int responseCode, string sku) => {
                    Console.WriteLine("Error purchasing item {0}",sku);
                };

                pBillingConnection.BillingHandler.OnPurchaseConsumedError += (int responseCode, string token) => {
                    Console.WriteLine("Error consuming previous purchase");
                };

                pBillingConnection.BillingHandler.InAppBillingProcesingError += (message) => {
                    Console.WriteLine("In app billing processing error {0}",message);
                };

                // Load inventory or available products
                await GetInventory();

                // Load any items already purchased
                UpdatePurchasedItems();
            };

			// Create our OpenGL view, and display it
			Game1.Activity = this;
			var g = new Game1 ();
			SetContentView (g.Window);
			g.Run ();
        }

        protected override void OnDestroy()
        {
            // Are we attached to the Google Play Service?
            if (pGooglePlayClient != null) 
            {
                // Yes, disconnect
                pGooglePlayClient.Disconnect();
            }

            if (pBillingConnection != null)
            {
                pBillingConnection.Disconnect();
            }

            base.OnDestroy();
        }

        /// <summary>
        /// Helper function for attempting to Log into Google Play servers.
        /// </summary>
        public void LoginToGoogle()
        {
            // If we are already connected/connecting, no need to try again.
            if(pGooglePlayClient.IsConnected || pGooglePlayClient.IsConnecting)
                return;

            // If we haven't already tried to login, do so now. Other wise, try to resolve
            // the issue from the last login attempt.
            if (mConnectionResult == null) 
            {
                pGooglePlayClient.Connect();
            } 
            else
            {
                ResolveLogin(mConnectionResult);
            }
        }

        /// <param name="requestCode">The integer request code originally supplied to
        ///  startActivityForResult(), allowing you to identify who this
        ///  result came from.</param>
        /// <param name="resultCode">The integer result code returned by the child activity
        ///  through its setResult().</param>
        /// <param name="data">An Intent, which can return result data to the caller
        ///  (various data can be attached to Intent "extras").</param>
        /// <summary>
        /// Called when an activity you launched exits, giving you the requestCode
        ///  you started it with, the resultCode it returned, and any additional
        ///  data from it.
        /// </summary>
        protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult (requestCode, resultCode, data);

            // Check if this was the error code we supplied. That is the only one we care about.
            if (requestCode == REQUEST_CODE_RESOLVE_ERR && resultCode == Result.Ok) 
            {
                pGooglePlayClient.Connect();
                return;
            }

            // Ask the open service connection's billing handler to process this request
            pBillingConnection.BillingHandler.HandleActivityResult (requestCode, resultCode, data);

            //TODO: Use a call back to update the purchased items
            UpdatePurchasedItems();
        }

        /// <summary>
        /// Called onces the player has successfully logged into the Google Play services.
        /// </summary>
        /// <param name="p0">P0.</param>
        public void OnConnected (Bundle p0)
        {
            ShowToasterMessage("Connected!");
        }

        /// <summary>
        /// Called when the player disconnects from Google Play.
        /// </summary>
        public void OnDisconnected ()
        {
            ShowToasterMessage("Disconnected!");
        }

        /// <summary>
        /// We attempted to connect to Google Play, but failed for some reason.
        /// </summary>
        /// <param name="result">Why we failed.</param>
        public void OnConnectionFailed (ConnectionResult result)
        {
            // We actually will always fail the first time we try to connect. When that happens we need
            // to resolve the reason, which is to log into the users Google account.
            ResolveLogin (result);

            // Store the result for later use.
            mConnectionResult = result;

            ShowToasterMessage("Connection Failed!");
        }

        /// <summary>
        /// After we fail to connect to Google Play, it will tell us why. In some cases the
        /// failure reason can be automatically resolved (eg. Login Required).
        /// </summary>
        /// <param name="result">The reason we failed to connect.</param>
        private void ResolveLogin(ConnectionResult result)
        {
            // Does this failure reason have a solution?
            if (result.HasResolution) 
            {
                try 
                {
                    // Try to resolve the problem automatically.
                    result.StartResolutionForResult(this, REQUEST_CODE_RESOLVE_ERR);
                } 
                catch (Android.Content.IntentSender.SendIntentException /*e*/) 
                {
                    // Not really sure why this is here.
                    pGooglePlayClient.Connect();
                }
            }
        }

        /// <summary>
        /// Called when the App first starts, after OnCreate is called.
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();

            // Just try to connect to Google Play right away. No point in waiting.
            pGooglePlayClient.Connect();

            // Attempt to connect to the service
            pBillingConnection.Connect();
        }

        /// <summary>
        /// Called when the app is about to stop.
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
        }

        /// <summary>
        /// Updates the purchased items.
        /// </summary>
        private void UpdatePurchasedItems ()
        {
            bool isTrial = true;

            // Ask the open connection's billing handler to get any purchases
            var purchases = pBillingConnection.BillingHandler.GetPurchases (ItemType.Product);

            Console.WriteLine("Products Owned ( " + purchases.Count + " ) : ");
            foreach (Purchase p in purchases)
            {
                ShowToasterMessage(p.ToString());
                Console.WriteLine(p.ToString());

                if (mPremiumUpgradeProductID == p.ProductId)
                {
                    isTrial = false;
                }
            }

            TrialModeManager.pInstance.pIsTrialMode = isTrial;
        }

        public void PurchasePremiumUpgrade()
        {
            if (mProducts != null)
            {
                foreach (Product p in mProducts)
                {
                    if (p.ProductId == mPremiumUpgradeProductID)
                    {
                        pBillingConnection.BillingHandler.BuyProduct(p);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Connects to the Google Play Service and gets a list of products that are available
        /// for purchase.
        /// </summary>
        /// <returns>The inventory.</returns>
        private async Task GetInventory ()
        {
            // Ask the open connection's billing handler to return a list of avilable products for the 
            // given list of items.
            // NOTE: We are asking for the Reserved Test Product IDs that allow you to test In-App
            // Billing without actually making a purchase.
            mProducts = await pBillingConnection.BillingHandler.QueryInventoryAsync (new List<string> {
                mPremiumUpgradeProductID,
            }, ItemType.Product);

            // Were any products returned?
            if (mProducts == null) 
            {
                // No, abort
                ShowToasterMessage("No products available.");
                return;
            }

            Console.WriteLine("Products Available ( " + mProducts.Count + " ) : ");
            foreach (Product p in mProducts)
            {
                ShowToasterMessage(p.ToString());
                Console.WriteLine(p.ToString());
            }
        }

        //[Conditional("ALLOW_GARBAGE")]
        private void ShowToasterMessage(string msg)
        {
            Toast.MakeText(this, msg, ToastLength.Long).Show();
        }

        public GamesClient pGooglePlayClient
        {
            get 
            {
                return mGooglePlayClient;
            }
        }

        public InAppBillingServiceConnection pBillingConnection
        {
            get
            {
                return mBillingConnection;
            }
        }
    }
}

