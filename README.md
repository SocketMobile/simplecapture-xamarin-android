# README #


This repository contains step 1 - 3 of the source code for the Simple Capture Demo app by Socket Mobile for Android. 

Third phase adds CaptureHelper event handling for device arrival, removal, and scanned data. Also shows how to get information about the connected scanner (Bluetooth FriendlyName and battery level).


### System information ###

* Requires Socket Mobile Companion version 1.3.58 or higher (available on the Google Play Store) 
* Developed on a Mac Mini using Visual Studio for Mac


### How to use the app ###

Download and install the Socket Mobile Companion from the Google Play Store. Run the Companion app and configure your Socket scanner for Application mode. Then, compile and run the Simple Capture Demo application on your target device.

This application shows how to get the Capture service and client (CaptureHelper) up and running. There is practically no user interface in this application. To confirm it is working, a toast notification will report the client initialization return code, and the retry count that was needed to get the client started. Toast notifications also show scanner connection/disconnection (or "arrival" and "removal") as well as scanned data received from the connected device.


### License and agreement ###

The app integrates Socket Mobile Capture SDK and is free for adaptation per your own business needs under Socket Mobile license and user agreements. You will need to join the Socket Mobile Developer program to access the SDK and create AppKeys for your own applications. The Socket Mobile Developer program is free for life with a one time registration fee. 


### Contact us ###

For inquiries regarding the development of this app as well as Capture SDK integration, please email developers@socketmobile.com. 


### Screenshots ###

![CaptureHelper Initialization](./img/Screenshot_1.png "CaptureHelper initialization")

![Scanner Connection](./img/Screenshot_2.png "Scanner connection")

![Scanned Data](./img/Screenshot_3.png "Scanned data")

![Scanner Disconnected](./img/Screenshot_4.png "Scanner disconnected")

