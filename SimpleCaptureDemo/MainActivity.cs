/// Simple Capture Demo
/// 
/// First phase of a simplified Xamarin Android demo program
/// to show how to use the Capture SDK.
/// 
/// First phase shows how to get the Capture service up and running. To confirm
/// it is working, pull down your notifications and look for one that says:
/// 
/// Companion
/// Companion service is running
/// 
/// Service startup times can vary on different devices and platforms. Most
/// notably, the service may need as much as 10 seconds or more to start if
/// the app is run immediately after a device reset. Your times may vary.
/// 
/// Second phase builds on first phase. It adds the code necessary for
/// starting a CaptureHelper client, that the application will use to
/// communicate with the Capture service.

using System.Threading.Tasks;

using Android.App;
using Android.OS;
using Android.Content;

using SocketMobile.Capture;

namespace SimpleCaptureDemo
{
    [Activity(Label = "Simple Capture Demo", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {

        public string CaptureServiceAction = "com.socketmobile.capture.START_SERVICE";
        public string CaptureServicePackage = "com.socketmobile.companion";
        public string CaptureServiceStartCmd = "com.socketmobile.capture.StartService";

        public CaptureHelper Capture = null;

        // Start the Capture service by constructing and broadcasting an intent
        private void StartCaptureService()
        {
            Intent intent = new Intent(CaptureServiceAction);
            intent.SetComponent(new ComponentName(CaptureServicePackage, CaptureServiceStartCmd));
            intent.AddFlags(ActivityFlags.ReceiverForeground);
            SendBroadcast(intent);
        }

        // This actually opens a Capture client session
        //
        // An AppKey is required. One can be obtained for your own projects
        // if you are registered on the Socket Developer site. Use your
        // package ID and your Developer ID to get your own AppKey from here:
        // 
        // https://www.socketmobile.com/developer/portal/appkey-registration
        //
        // Note: This example program runs as-is using a sample AppKey.

        public async Task<long> OpenCapture()
        {
            long result = await Capture.OpenAsync("android:com.socketmobile.simplecapturedemo",                          // the name of our package
                                                  "bb57d8e1-f911-47ba-b510-693be162686a",                                // sample code Developer ID
                                                  "MC4CFQC76uXj3J36NLgYLaZP7YevE/A4pgIVAPqOydqV4fv4Gh5v01DJGbaSbY61");   // our AppKey

            return result;
        }

        // Open the CaptureHelper client
        //
        // Since we don't know how long it may take for the Capture service to
        // start, we need to retry our OpenCapture() command until we get a
        // result different than ESKT_UNABLEOPENDEVICE (-27). 60 retries with
        // and interval of a half second between each attempt is arbitrary.
        //
        // Once started, the service stays running, even after the sample app
        // terminates. This is by design. Then the sample app (and any other
        // Capture-enabled apps) should succeed almost immediately on the very
        // first call to OpenCapture().

        private async Task<long> StartCaptureClient()
        {
            int Retries = 0;
            const int MaxRetries = 60;
            const int RetryIntervalMs = 500;
            long result;
            Capture = new CaptureHelper();

            while ((result = await OpenCapture()) == SktErrors.ESKT_UNABLEOPENDEVICE)
            {
                Retries++;
                if (Retries == MaxRetries)
                {
                    break;
                }
                await Task.Delay(RetryIntervalMs);
            }

            // Report the result
            Android.Widget.Toast.MakeText(this, "StartCaptureClient result is: " + result + " Retries: " + Retries, Android.Widget.ToastLength.Short).Show();
            return result;
        }

        // Stop the CaptureHelper client
        private async void StopCaptureClient()
        {
            await Capture.CloseAsync();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Start the Capture service
            StartCaptureService();

            // Start the Capture client
            StartCaptureClient();
        }

        protected override void OnDestroy()
        {
            StopCaptureClient();
            base.OnDestroy();
        }
    }
}

