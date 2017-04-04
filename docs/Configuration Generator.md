# Configuration generator
The configuration generator is a tool that simplifies generation of [XML configuration files](Page-specification).
The generator options menu has the following entries:
* _Interface_: If a configuration has been loaded on the main page, the same communication interface is also used by the generator as default. With this menu entry a different communication interface could be selected.
* _Adapter_: With this menu the [Bluetooth adapter](#SupportedAdapters) could be selected. If the device is not coupled already, searching for new devices is possible. This menu is only enabled if interface type _BLUETOOTH_ has been selected.
* _Adapter configuration_: When using a FTDI USB or Bluetooth (non ELM327) adapter with this menu the adapter could be configured. Depending on the adapter type only the CAN baudrate/K-Line interface or more specific parameters could be specified.
* _Add errors page_: When this checkbox is selected (which is the default) and _Errors_ page will be generated when writing the configuration file. This page will read the error memory of all detected ECUs.
* _Configuration_: With this submenu you could select if the configuration is created automatically or manually:
	* _Automatic_: The ECU configuration will be read from the vehicle with the _Read_ button. This option is only available in BMW mode.
	* {anchor:ManualConfiguration}_ Manual X_: A manual configuration is stored in the storage with the number X. The ECUs for the configuration have to be added manually with the submenu from the _Edit_ button. Therefore you have to identify the required .GRP or .PRG files on the info page of each ECU in INPA.
![INPA info page](Configuration Generator_InpaInfo.png)
.GRP file name is _{"D_MOTOR.GRP"}_ and .PRG file name is _{"D60M47A0.PRG"}_
* _Data logging_: Selecting this menu entry will open a sub menu with multiple data logging options:
	* _Create trace file_: If the checkbox of this menu is active, a _ifh.trc_ file will be created when executing jobs. The trace file will be created in the _LogConfigTools_ subdirectory.
	* _Append trace file_: If this checkbox is enabled the trace file is always appended. Otherwise the trace file will be overridden after selection of a vehicle type.
* _Translations_: (Only for non German languages and BMW mode) This menu opens a submenu that allows configuration of automatic ECU text translation with Yandex.Translate:
	* _Translate ECU text_: If this menu item is checked, automatic ECU text translation is active.
	* _Yandex API Key_: For automatic translation with Yantex.Translate a free API Key is required, that allows a limited amount of translations per day. To get this key, a Yandex account is required. This menu provides a GUI that assists in obtaining the API Key.
	* _Clear translation cache_: The translations are stored together with the ECU configuration files. To enforce a new translation this menu resets the translation cache.
* _Online help_: Displays this help page.
The vehicle type is detected automatically when pressing the _Read_ button (_automatic mode_).
If the _manual mode_ is used this button is named _Edit_ and opens a submenu that allows to add or remove ECU files. Therefore you have to identify the required .GRP or .PRG files on the info page of each ECU in INPA. In VAG mode it's possible to search for ECUs, but this process is very time consuming.

![Generator menu](Configuration Generator_AppGeneratorMenuSmall.png)
{anchor:EdiabasTool}
If the analysis is successful, the detected ECUs are listed and the VIN is displayed in the title bar. The VIN will later be used as subdirectory name for storing the generated configuration.
If an ECU is completely silent (defective) it will not show up in the list!
After selecting an ECU list entry it's Job page will be displayed. This is the main configuration page for the jobs results that will be added to the configuration file.
The page has the following properties:
* _Page name_: This is the title (tab) name for the page in the configuration file.
* _ECU name_: With this field the name of ECU on the _Errors_ page could be changed.
* _Job list_: Here all available jobs are listed. Only if the job has a check mark it will be executed later. In the area _Comments for job_ the comments for the selected job will be shown.
* _Job result_: Here one ore more job results could be selected (with a check mark) that will be displayed later on the page) For the currently selected result the data type and comment will be shown below.
* _Display text_: This is the text that will be displayed on the page beside the job result.
* _Tag for data logging_: If the _Log data_ option is enabled on the main page the result data will get this tag in the log file. If the entry is empty no data will be logged.
* _Output format_: Here the output format of the result data could be modified. The format specification is in the form of [Ediabas result types and formats](EDIABAS-result-types-and-formats). Depending on the job data type more or less result types will be listed.
* _Read_: This button executes the selected job and displays the result in the specified format next to the button.
With the _Save_ button the configuration will be stored and used as default on the main page.
Hint: A long click on the ECU list opens a submenu that allows to change the order of the ECU entries in the list.

![ECU list](Configuration Generator_AppGeneratorEcusSmall.png) ![Job selection](Configuration Generator_AppGeneratorJobSmall.png)
