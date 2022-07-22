Hex Logger README
5/9/2022
Jackson Trexler, jackson@hexagram.io

Warning! This package uses Debug.Log to ensure our logger is logging. If your project likes to run for a few days at a time (looking at you Chimera) please disable logging on project start, as unregulated, the file will take up all available storage space. Maybe I should look into the package deleting that every now and then.

This is also my first Unity Package made with the intent of other people using, so it might suck!

Tests:

You can check if the logger is working by placing statements in the scripts Awake() function

Awake()
{
//HexLog.SendLog(LoggerTarget.Console, "Persistent file path:", "info");
//HexLog.SendLog(LoggerTarget.Console, filePath, "info");
//HexLog.SendLog(LoggerTarget.File, "JSON 1", "info");
//HexLog.SendLog(LoggerTarget.File, "JSON 2", "debug");

//Config.startConfig();

Logger
}

and also in the sample scene Example1, or by using the script Logexttest.cs. Should also be a good reference on ways the package can be used.

Install:
This package can either be installed by simply placing the extracted zip file (found at https://github.com/HexagramIO/UnityLogger) in the Assets folder of your project (if you'd like to make changes to the package itself) or by adding a package via git link (git@github.com:HexagramIO/UnityLogger.git) in the Package Manager window.

To log a message in another script, add: "using Hexagram.Logger" to the top and use format: HexLog.Warn("message")
Or to cut down on how much you havet to type, add: "using static StaticHexLog" to the top and use format Warn("message")
or these levels: Warn, Info, Error, Critical_Error

A configuration file for the logger is generated and can be found at your Application.persistentDataPath, which for a windows user would be in ...<USER>/AppData/LocalLow/<PROJECT COMPANY>/<PROJECT NAME>HexLogConfig.txt

Right now there are only two configuration options, how many levels up should be logged, and where stored logfiles should go. The logger will always look for a config file at where Unity returns Application.persistentDataPath, if that doesn't work for you / gives you trouble on other OS please let me know.

Lowering the int associated with storedVerbosity increases the urgency needed to log something, level 1 is reserved for critical errors, level 2 for regular errors, level 3 is for warnings, level 4 is for info, and level 5 is for debugging / general tomfoolery. A good level for unstable production builds is 4, 5 while actively working on a tough feature, and 2 or 3 for stable production builds down the line (though if something does go wrong it might be nice to have a lot of information ¯\_ (ツ)_/¯)

