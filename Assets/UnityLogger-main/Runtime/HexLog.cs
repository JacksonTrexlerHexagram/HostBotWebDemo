using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
//using System.IO.FileInfo;
//For file size check
namespace Hexagram.Logger
{
	
	
    public class HexLog : MonoBehaviour
    {
		
        static int verbosityLevel = 7;
        //static IDictionary<int, string> levelNames = new Dictionary<int, string>();
        static IDictionary<string, int> levelNames = new Dictionary<string, int>{{"critical_error", 1}, {"error", 2}, {"warn", 3}, {"info", 4}, {"debug", 5}};
        HexLogConfig logConfigObject;
        static int currentRotation = 0;
        static int maxLogSizeMB = 500;
        
        //This defines places log messages can be sent, Universal should use all of them.
        public enum LoggerTarget
        {
            File, Universal, Console
        }

        //Not much to store as of right now, stores file path config stuff should save to, and what messages should be logged for now.
        //In hindsight this might work better as a matrix or dictionary, but that might make JSON serialization harder.
        public class HexLogConfig
        {
            //IDictionary<int, string> levelNamesToConfig = levelNames;
            public int storedVerbosity;
            public string storedFilePath;
            public int storedMaxLogSizeMB;

            public HexLogConfig(int getVerbosity, string getFilePath, int getMaxLogSizeMB)
            {
                storedVerbosity = getVerbosity;
                storedFilePath = getFilePath;
                storedMaxLogSizeMB = getMaxLogSizeMB;
            }
        }

        public class Config
        {
            //protected readonly object lockObj = new object();
            //Locking won't work on static I think
            public static string configFilePath = Application.persistentDataPath + "HexLogConfig.txt";

            //Set current settings to config JSON object then file.
            public static void setConfig(HexLogConfig logConfigObj)
            {
                //lock (lockObj)
                //Doesn't like that this isn't static but I don't like this unlocked, might want to use different implementation
                {
                    string jsonLogConfig = JsonUtility.ToJson(logConfigObj);
                    using (StreamWriter  streamWriter = File.AppendText(configFilePath))
                    {
                        streamWriter.WriteLine(jsonLogConfig);
                        streamWriter.Close();
                    }
                }
            }

            //This retrieves config settings, if you want to add more values saved, add them to HexLogConfig class and in function in format: thing = logConfigObject.storedThing
            public static void getConfig()
            {
                //Get json object from file
                
                //deserialize and set dictionary / verbosity

                if (File.Exists(configFilePath))
                {
                    HexLogConfig logConfigObject = JsonUtility.FromJson<HexLogConfig>(string.Join("",System.IO.File.ReadAllLines(configFilePath)));
					if (Debug.isDebugBuild)
					{
                    	Debug.Log("config read");
					}
                    Info("Config file found after invoking getConfig");
                    verbosityLevel = logConfigObject.storedVerbosity;
                    configFilePath = logConfigObject.storedFilePath;
                    maxLogSizeMB = logConfigObject.storedMaxLogSizeMB;
                    //Can only really be changed in file itself for now.

                }
                else
                {
                    //HexLog.Error("No config file found at " + filePath);
					if (Debug.isDebugBuild)
					{
                    	Debug.Log("No file found");
					}
                    //Error?
                    Error("No config file found, despite getConfig() being explicitly invoked, not doing anything. Remember that the config file MUST be at default location.");
                }
            }

            //Sets up default options with high Verbosity.
            public static void defaultConfig()
            {
				if (Debug.isDebugBuild)
				{
                	Debug.Log("Default config");
				}
                verbosityLevel = 5;
                maxLogSizeMB = 500;
                //Default file path already set from earlier


                HexLogConfig logConfigObject = new HexLogConfig(verbosityLevel, configFilePath, maxLogSizeMB);

                setConfig(logConfigObject);
            }
            
            //Checks for config file, defaults if it's not there, gets it if it's there.
            public static void startConfig()
            {
                if (File.Exists(configFilePath))
                {
                    try
                    {
                        getConfig();
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Config file renamed and new file generated: Config read exception: " + e);
                        //Move broken file to keep settings saved while new one generates.
                        System.IO.File.Move(configFilePath, Application.persistentDataPath + "HexLogConfigBroken.txt");
                        defaultConfig();
                    }
                }
                else
                {
                    defaultConfig();
                }
            }
        }


        //Contains all elements of each log message, which for now has a text message, log level, time stamp, and the name of the app that generated it. The time and name are added automatically,
        //The level is added automatically when using Level statements like Warn("") and Error("")
        public class HexLogObj
        {

            public string message;
            public string level;
            public string timestamp;
            public string app_role;

            public HexLogObj(string argMessage, string argLevel)
            {
                message = argMessage;
                level = argLevel;

                timestamp = System.DateTime.UtcNow.ToLocalTime().ToString();

                string[] appNameGet = Application.dataPath.Split('/');
                app_role = "unity_app_" + appNameGet[appNameGet.Length - 2];
            }
        }

        public abstract class LoggerBase
        {
            protected readonly object lockObj = new object();
            public abstract void SendLog(HexLogObj logObj);
        }

        //Log to file, mostly for telegraf
        public class FileLogger : LoggerBase
        {
            public string filePath = Application.persistentDataPath + "CurrentHexLog.txt";
            public override void SendLog(HexLogObj logObj)
            {
                lock (lockObj)
                {
                    string jsonLog = JsonUtility.ToJson(logObj);
                    using (StreamWriter streamWriter = File.AppendText(filePath))
                    {
                        streamWriter.WriteLine(jsonLog);
                        streamWriter.Close();
                    }
                }
            }
        }

        public class UniversalLogger : LoggerBase
        {
            public string filePath = Application.persistentDataPath + "CurrentHexLog.txt";
            public override void SendLog(HexLogObj logObj)
            {
                lock (lockObj)
                {
                    //Connect to Saga, send message in JSON
                    string jsonLog = JsonUtility.ToJson(logObj);
					if (!File.Exists(filePath))
					{
						using (StreamWriter streamWriter = File.CreateText(filePath))
						{
							if (Debug.isDebugBuild)
							{
								//Debug.Log("File created at: " + filePath);
                                //Not behaving how I'd like
							}
							streamWriter.WriteLine(jsonLog);
							streamWriter.Close();
						}
					}
					else
					{
                        fileSizeCheck(filePath);
                        //Need to check if file is too big before logging

                        using (StreamWriter streamWriter = File.AppendText(filePath))
						{
							if (Debug.isDebugBuild)
							{
								//Debug.Log("File appended at: " + filePath);
                                //Not behaving how I'd like
							}
                            streamWriter.WriteLine(jsonLog);
							streamWriter.Close();
						}
					}
                }
            }
        }

        public class ConsoleLogger : LoggerBase
        {
            public override void SendLog(HexLogObj logObj)
            {
                lock (lockObj)
                {
					if (Debug.isDebugBuild)
					{
						//This will only fire when Build Settings "Development Build" is enabled to retain performance
						//Debug.Log(logObj.message);
                        //Disabled this because it didn't behave how I wanted it to.
					}
                }
            }
        }

        public static int fileSizeCheck(string filePath)
        {
            //Need to check if file is too big before logging
            FileInfo logFileData = new FileInfo(filePath);
            string oldFilePath = Application.persistentDataPath + "Old" + "HexLog.txt";
            //Cycles and deletes old file if combined 
            //.Length is in bytes
            //Something is very wrong with this debug.log statement and at some point I'd like to know what.
            //Debug.Log(logFileData.Length > maxLogSizeMB * 1000000);
            //if (string.Equals((logFileData.Length > maxLogSizeMB * 1000000), "False"));
            if(logFileData.Length > maxLogSizeMB * 1000000)
            {
                Debug.Log("triggered");
                Debug.Log(logFileData.Length + " > " + maxLogSizeMB*1000000);
                //Rotate log files
                if (currentRotation == 0)
                {
                    currentRotation = 1;
                    Debug.Log("Initial log file full");
                    //filePath = Application.persistentDataPath + "Rotation" + 0 + "HexLog.txt";
                    oldFilePath = Application.persistentDataPath + "InitialOldHexLog.txt";
                }
                else
                {
                    //Rotating between 1 and 2 is residual, eventually I'd like options to save a number of past logs and just start counting upwards to a number specified in config.
                    if (currentRotation == 2)
                    {
                        currentRotation = 1;
                        Debug.Log("Swap1");
                    }
                    else if (currentRotation == 1)
                    {
                        currentRotation = 2;
                        Debug.Log("Swap2");
                    }
                    //Need to update filepath name so that correct file is nuked
                }
                //Kill oldest file, which *just* had rotation switched. Should be fine because this function is locked.
                if (File.Exists(filePath))
                {
                    if(File.Exists(oldFilePath))
                    {
                        Debug.Log("Purging");
                        File.Delete(oldFilePath);
                    }
                    
                    //File.WriteAllText(oldFilePath, string.Empty);
                    System.IO.File.Move(filePath, oldFilePath);
                    return 1;
                }
                else
                {
                    return 0;
                }
                //I don't think this specific snippet can be logged via HexLogger.
            }
            //Debug.Log(logFileData.Length + " < " + maxLogSizeMB * 1000000);
            return 0;
        }

        //Making this inside of main class will make it easier to call outside.
        //Get message from function, pack it into object, send it where it needs to go.
        private static LoggerBase logger = null;
        public static void SendLog(LoggerTarget target, string message, string level)
        {
            if (levelNames[level] <= verbosityLevel){
                HexLogObj logObj = new HexLogObj(message, level);
                switch (target)
                {
                    case LoggerTarget.File:
                        logger = new FileLogger();
                        logger.SendLog(logObj);
                        break;
                    case LoggerTarget.Console:
                        logger = new ConsoleLogger();
                        logger.SendLog(logObj);
                        break;
                    case LoggerTarget.Universal:
                        logger = new UniversalLogger();
                        logger.SendLog(logObj);
                        break;
                    default:
                        return;
                }
            }
        }

        //These functions call SendLog, to the Universal destination and with the specified level. These are what should be used in 99% of logs.
		
		
		public static void Warn(string message)
		{
			SendLog(LoggerTarget.Universal, message, "warn");
		}

		public static void Info(string message)
		{
			SendLog(LoggerTarget.Universal, message, "info");
		}

		public static void Error(string message)
		{
			SendLog(LoggerTarget.Universal, message, "error");
		}

		public static void Critical_Error(string message)
		{
			SendLog(LoggerTarget.Universal, message, "critical_error");
		}
        //Run sanity checks here.
        void Awake()
        {
            string filePath = Application.persistentDataPath;
            Config.startConfig();
            //If you want to use default configuration without deleting config file, comment this out.

            //Uncomment different SendLog or level functions to test different functionality, also som nice examples of how to use.
			//You probably shouldn't see Unity Console output unless your project is configured for debugging in build settings.
            //SendLog(LoggerTarget.Console, "Persistent file path:", "info");
            //SendLog(LoggerTarget.Console, filePath, "info");
            //SendLog(LoggerTarget.File, "JSON 1", "info");
            //SendLog(LoggerTarget.File, "JSON 2", "debug");


            //Warn("Warning warning!");
            //Error("Error Error!");

            Info("Info: Logger Initialized");
			Info("File Path: " + filePath);
        }
    }
}
