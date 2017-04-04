**Project Description**
.NET BMW and VAG Ediabas interpreter library

# Features
This .NET assembly is small and fast replacement for the BWM and VAG Ediabas toolset.
It's running on all platforms that support .NET framework (Windows, Windows CE, Mono).
The code is speed optimized, but improved for multiple calls of the same job.
This means the first job call is slow and the next are faster.

The library can directly read and interpret .GRP and .PRG files, the internal interface is similar to the EDIABAS API or the Tool32.exe GUI interface.
At the moment the D-CAN, BMW-FAST, KWP-2000*, KWP-2000 BMW, DS2, DS1, Concept 3, ISO9141 (Concept 2) and Concept 1 concepts are supported. If you send me EDIABAS log files from other concepts I could add them if possible.
Now a variant that can be used with cars from the VAG group has been released, but it requires an adapter with the [replacement firmware for ELM327](Replacement-firmware-for-ELM327). In this mode the protocols KWP2000, KWP1281 and TP2.0 are supported (cars until 4.2012).

For testing there is a console application [EdiabasTest](EdiabasTest-parameters) with command line parameters.
More informations could be found in the [Documentation](Documentation) section.
# Binary Compatibility
A binary compatible api32.dll wrapper is existing, it's possible to replace the existing EDIABAS api32.dll with the EdiabasLib api32.dll and the applications are still working (The dll requires [.NET framework 4.0](https://www.microsoft.com/de-de/download/details.aspx?id=17718) or higher and [VS2015 C++ runtime](https://www.microsoft.com/de-de/download/details.aspx?id=48145) to be installed)!
This has been tested with Tool32, INPA and Rheingold. Since EdiabasNet is ignoring the EDIABAS.INI file you have to put your own EdiabasLib.config in the api32.dll directory.

A compatible apiNET32.dll is also available, so it should be easy to port existing .NET applications that use the original EDIABAS library.

# Android
Now there is also a library existing that is working with Mono for Android. For communication either FTDI USB or Bluetooth Serial Port Protocol (SSP) protocol could be used.
### Deep OBD for BMW and VAG
Additionally an Android application _[Deep OBD for BMW and VAG](Deep-OBD-for-BMW)_ is existing, that is configurable via [XML](BMW-Diagnostics-page-specification). Generation of XML code could be simplified with the integrated configuration generator.
Download app from Google Play: [https://play.google.com/store/apps/details?id=de.holeschak.bmw_deep_obd](https://play.google.com/store/apps/details?id=de.holeschak.bmw_deep_obd)
_Deep OBD for BMW_ supports several OBD II adapters:
* Standard FTDI based USB "INPA compatible" D-CAN/K-Line adapters (all protocols)
* ELM327 based Bluetooth and WiFi adapters. Recommended ELM327 versions are 1.4b, 1.5 and origin 2.1, which are based on PIC18F2480 processor (no MCP2515 chip) (D-CAN protocol only)
* ELM327 based adapters with [Replacement firmware for ELM327](Replacement-firmware-for-ELM327)  D-CAN and K-Line (all protocols, including VAG KWP2000, KWP1281 and TP2.0). Programmed adapters are now available.
![Bluetooth adapter top](Home_BluetoothAdapterTopSmall.png)
* Custom [Bluetooth D-CAN/K-Line adapter](Build-Bluetooth-D-CAN-adapter) (BMW-FAST protocol over D-CAN and K-Line)
* [ENET WiFi adapters](ENET-WiFi-Adapter) (for BMW F-models)
![ENET adapter](Home_EnetAdapterClosedSmall.png)

![BMW deep OBD and VAG](Home_AppMotorSmall.png)
# Hints
At the moment most BEST/1 commands are implemented, but a few are still missing.
The implemented commands are automatically tested and compared with EDIABAS output.
Because I have no BEST/1 command specification some commands are only guessed. If someone could supply me with a BEST/1 specification I could improve the library.
