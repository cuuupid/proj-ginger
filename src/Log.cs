using System;
using System.Collections.Generic;

internal class Log
{
	public static bool enabled;

	public static void It(string s)
	{
		if (enabled)
		{
			Console.WriteLine(s);
		}
	}

	public static void Exception(Exception exception)
	{
		Console.WriteLine(exception.ToString() ?? "");
	}

	public static void Exception(Exception exception, string someText)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["block"] = someText;
		Console.WriteLine(someText + " " + exception.ToString());
	}
}
