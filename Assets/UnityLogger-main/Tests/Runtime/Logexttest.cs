using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hexagram.Logger;
using static StaticHexLog;
using System.IO;
using System;

public class Logexttest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		//Check if StaticHexLog imported correctly, these can be run with "using static StaticHexLog" by just typing <NameOfLevel>(<message>)
		Info("External static info test");
        Warn("External static warning test");
		Error("External static error test");
		
		//Check if using implementation from HexLog.cs works, which can be run with "using Hexagram.Logger" by typing HexLog.<NameOfLevel>(<message>)
		HexLog.Info("External info test");
		HexLog.Warn("External warning test");
		HexLog.Error("External error test");
		
		//Check sendlog, which is a much more verbose function that the Level functions apply default values to. Use this to only send messages to certain places.
		//Use syntax HexLog.SendLog(HexLog.LoggerTarget.<Target>, <Message>, <level>); <-- That's lowercase whatever level you want
		//If anyone ends up using this a lot I can throw this into StaticHexLog too to cut down on needed text.
		HexLog.SendLog(HexLog.LoggerTarget.Console, "SendLog test with parameters Console, some string, and info", "info");
		HexLog.SendLog(HexLog.LoggerTarget.File, "SendLog test with parameters File, some string, and warn", "warn");
		HexLog.SendLog(HexLog.LoggerTarget.Universal, "SendLog test with parameters Universal, some string, and error", "error");
    }
	
	 
}
