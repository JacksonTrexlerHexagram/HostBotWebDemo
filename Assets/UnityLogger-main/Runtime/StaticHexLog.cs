using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hexagram.Logger;

public static class StaticHexLog
{
    public static void Warn(string message)
	{
		HexLog.SendLog(HexLog.LoggerTarget.Universal, message, "warn");
	}

	public static void Info(string message)
	{
		HexLog.SendLog(HexLog.LoggerTarget.Universal, message, "info");
	}

	public static void Error(string message)
	{
		HexLog.SendLog(HexLog.LoggerTarget.Universal, message, "error");
	}

	public static void Critical_Error(string message)
	{
		HexLog.SendLog(HexLog.LoggerTarget.Universal, message, "critical_error");
	}
}
