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
/// 
/// Third phase builds on the previous two. It shows how to get event
/// notifications from Capture and how to process them. Scanner arrival,
/// scanner removal, and scanned data handling are demonstrated. It also shows
/// how to query the connected scanner for device information.

using System;
using System.Threading.Tasks;
using System.Timers;

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
        Timer BatteryPollingTimer = null;

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

#if DEBUG
        // This lets us get Capture debug messages in the Xamarin Application Output window
        class DebugConsole : CaptureHelperDebug
        {
            public void PrintLine(string message)
            {
                DateTimeOffset dtNow = System.DateTimeOffset.Now;
                Console.WriteLine("[CAPTURE DEMO DEBUG] " + dtNow.ToString("HH:mm:ss:fff") + ": " + message);
            }
        }
#endif

        void DebugPrint(string message)
        {
#if DEBUG
            Capture.DebugConsole.PrintLine(message);
#endif
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

#if DEBUG
            Capture.DebugConsole = new DebugConsole();
#endif
            Capture.DoNotUseWebSocket = true;
            Capture.DeviceArrival += OnDeviceArrival;
            Capture.DeviceRemoval += OnDeviceRemoval;
            Capture.DecodedData += OnDecodedData;

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

        void StartBatteryPolling()
        {
            DebugPrint("Begin StartBatteryPolling()");
            BatteryPollingTimer = new Timer();
            BatteryPollingTimer.AutoReset = true;
            BatteryPollingTimer.Interval = 5000;
            BatteryPollingTimer.Elapsed += async (object sender, ElapsedEventArgs e) =>
            {
                DebugPrint("Begin BatteryPollingTimer elapsed event");
                if (Capture.GetDevicesList().Count != 0)
                {
                    DebugPrint("Capture device list count != 0");
                    CaptureHelperDevice ConnectedScanner = Capture.GetDevicesList()[0];
                    if (ConnectedScanner != null)
                    {
                        DebugPrint("ConnectedScanner is not null");
                        CaptureHelperDevice.BatteryLevelResult resultBattery = await ConnectedScanner.GetBatteryLevelAsync();
                        if (resultBattery.IsSuccessful())
                        {
                            DebugPrint("GetBatteryLevelAsync() has returned");
                            RunOnUiThread(() =>
                            {
                                Android.Widget.Toast.MakeText(this, "Battery level: " + resultBattery.Percentage, Android.Widget.ToastLength.Short).Show();
                            });
                        }
                        else
                        {
                            DebugPrint("GetBatteryLevelAsync() returned failure code: " + resultBattery.Result);
                        }
                    }
                }
                DebugPrint("End BatteryPollingTimer elapsed event");
            };
            DebugPrint("Starting the battery polling timer");
            BatteryPollingTimer.Start();
            DebugPrint("End StartBatteryPolling()");
        }

        void StopBatteryPolling()
        {
            DebugPrint("Begin StopBatteryPolling()");
            if (BatteryPollingTimer != null)
            {
                DebugPrint("BatteryPollingTimer is not null");
                BatteryPollingTimer.Stop();
                BatteryPollingTimer = null;
            }
            DebugPrint("End StopBatteryPolling()");
        }

        public async void OnDeviceArrival(object sender, CaptureHelper.DeviceArgs arrivedDevice)
        {
            // There are a few interesting things you can find out about the arrived scanner
            // by calling the GetDeviceInfo() member function. It returns a DeviceInfo
            // structure which, among other things, contains the FriendlyName of the device.
            string FriendlyName = arrivedDevice.CaptureDevice.GetDeviceInfo().Name;

            // Other CapturHelper functions can be used to learn even more about 
            // the connected device, such as the battery level (and many other 
            // device properties).
            CaptureHelperDevice.BatteryLevelResult resultBattery = await arrivedDevice.CaptureDevice.GetBatteryLevelAsync();

            // You can easily modify this routine to report the BDADDR of the 
            // connected device, too - arrivedDevice.CaptureDevice.GetBluetoothAddressAsync()
            // would do the trick.

            // And since CaptureHelper is given to you in the form of C# source
            // code, you can add your own helper functions and extend the
            // capabilities by studying the implementation of the existing functions.
            // If you decide to do this, you should create an extension of CaptureHelper
            // and implement the additions in a new source file. That way, when you
            // update the CaptureHelper SDK at a later date, you won't lose your 
            // changes when the new CaptureHelper.cs file is written to your workstation.

            // Now show it all in a toast - be sure to run on the UI thread since this callback (and other CaptureHelper callbacks) do not come in on the UI thread!
            RunOnUiThread(() =>
            {
                Android.Widget.Toast.MakeText(this, "Arrival:\n" + FriendlyName + "\nBattery: " + resultBattery.Percentage,  Android.Widget.ToastLength.Long).Show();
            });

            // Get the scanner's firmware version
            CaptureHelper.VersionResult version = await arrivedDevice.CaptureDevice.GetFirmwareVersionAsync();
            Console.WriteLine("Scanner firmware version is: " + version.ToStringVersion());

            await Capture.SetDataConfirmationModeAsync(ICaptureProperty.Values.ConfirmationMode.kApp); 
            //StartBatteryPolling();
        }

        public void OnDeviceRemoval(object sender, CaptureHelper.DeviceArgs removedDevice)
        {
            //StopBatteryPolling();
            RunOnUiThread(() =>
            {
                Android.Widget.Toast.MakeText(this, "Scanner removed.", Android.Widget.ToastLength.Short).Show();
            });
        }

        public void OnDecodedData(object sender, CaptureHelper.DecodedDataArgs decodedData)
        {
            string Data = decodedData.DecodedData.DataToUTF8String;
            CaptureHelperDevice CurrentDevice = Capture.GetDevicesList()[0]; 
            RunOnUiThread(() =>
            {
                Android.Widget.Toast.MakeText(this, "Scanned data: " + decodedData.DecodedData.DataToUTF8String, Android.Widget.ToastLength.Short).Show();
            });
            if (Data.EndsWith('6'))             {                 CurrentDevice.SetDataConfirmationAsync(ICaptureProperty.Values.DataConfirmation.kBeepBad, ICaptureProperty.Values.DataConfirmation.kLedRed, ICaptureProperty.Values.DataConfirmation.kRumbleBad);             }             else             {                 CurrentDevice.SetDataConfirmationAsync(ICaptureProperty.Values.DataConfirmation.kBeepGood, ICaptureProperty.Values.DataConfirmation.kLedGreen, ICaptureProperty.Values.DataConfirmation.kRumbleGood);             }
        }
    }
}

