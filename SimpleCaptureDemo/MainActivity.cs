/// Simple Capture Demo
/// 
/// First phase of a simplified Xamarin Android demo program
/// to show how to use the Capture SDK.
/// 
/// The sample shows how to get the Capture service up and running. To confirm
/// it is working, pull down your notifications and look for one that says:
/// 
/// Companion
/// Companion service is running
/// 
/// Service startup times can vary on different devices and platforms. Most
/// notably, the service may need as much as 10 seconds or more to start if
/// the app is run immediately after a device reset. Your times may vary.

using Android.App;
using Android.OS;
using Android.Content;

namespace SimpleCaptureDemo
{
    [Activity(Label = "Simple Capture Demo", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {

        private string CaptureServiceAction = "com.socketmobile.capture.START_SERVICE";
        private string CaptureServicePackage = "com.socketmobile.companion";
        private string CaptureServiceStartCmd = "com.socketmobile.capture.StartService";

        // Start the Capture service by constructing and broadcasting an intent
        private void StartCaptureService()
        {
            Intent intent = new Intent(CaptureServiceAction);
            intent.SetComponent(new ComponentName(CaptureServicePackage, CaptureServiceStartCmd));
            intent.AddFlags(ActivityFlags.ReceiverForeground);
            SendBroadcast(intent);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Start the Capture service
            StartCaptureService();
        }
    }
}

