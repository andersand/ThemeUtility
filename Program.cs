using Microsoft.Win32;
using System.Reflection;

public class Program
{
    static string nl = Environment.NewLine;
    static string nlt = Environment.NewLine + "\t";
    static string appName = Assembly.GetEntryAssembly().GetName().Name + ".exe";
    static string usageStr = $"{nl}Usage: {appName} arg1 arg2{nl}{nl}Where arg1 is for app theme," +
        $" arg2 is for sys theme.{nl}Allowed argument values:{nlt}X = unchanged{nlt}L = Light{nlt}D = dark{nlt}T = Toggle{nl}{nl}" +
        $"This example usage shows how to toggle the system theme between dark and light, while leaving the app theme unchanged:{nlt}{appName} X T{nl}";


    [STAThread]
    public static void Main(string[] args)
    {

        try
        {
            Console.WriteLine($"Theme utility{nl}-------------{nl}");

            bool app = GetValue("AppsUseLightTheme");
            bool sys = GetValue("SystemUsesLightTheme");

            Console.WriteLine("App theme is currently " + (app ? "LIGHT" : "DARK"));
            Console.WriteLine("Sys theme is currently " + (sys ? "LIGHT" : "DARK"));
            Console.WriteLine();

            ParseArgs(args);

            SetValue("AppsUseLightTheme", app, args[0]);
            SetValue("SystemUsesLightTheme", sys, args[1]);

        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("An error has occurred!");
            Console.Error.WriteLine(ex.Message);
        }

        
    }

    static void ParseArgs(string[] args)
    {
        if (args.Length != 2)
        {
            throw new ArgumentException($"The application needs to be invoked with two arguments.{nl}{usageStr}");
        }
        string app = args[0]; // Command line argument for App Theme: X = unchanged L = Light D = dark T = Toggle 
        string sys = args[1]; // Command line argument for Sys Theme: X = unchanged L = Light D = dark T = Toggle 

        bool valid1 = ValidateArg(app);
        bool valid2 = ValidateArg(sys);

        if (!valid1 || !valid2)
        {
            string invalid = !valid1 ? app : sys;
            throw new ArgumentException($"Invalid argument provided: {invalid} {nl}{usageStr}");
        }
    }

    static bool ValidateArg(string arg)
    {
        switch (arg)
        {
            case "X":
            case "L":
            case "D":
            case "T":
                return true;
            default:
                return false;
        }
    }

    static void SetValue(string key, bool isLight, string arg)
    {
        try
        {
            int val = -1;
            switch(arg)
            {
                case "X":
                    Console.WriteLine($"Leaving {key} unchanged");
                    return;
                case "L":
                    val = 1;
                    Console.WriteLine($"Setting {key} LIGHT");
                    break;
                case "D":
                    val = 0;
                    Console.WriteLine($"Setting {key} DARK");
                    break;
                case "T":
                    val = isLight ? 0 : 1;
                    Console.WriteLine($"Toggling {key} --> " + (val == 1 ? "LIGHT" : "DARK") );
                    break;
                default:
                    throw new Exception("Error! Illegal argument encountered: " + arg);
            }

            using RegistryKey? subKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", true);
            if (subKey != null)
            {
                subKey.SetValue(key, val);
            }
        }
        //    catch () 
        //    { 
        //          // notify user if missing elevated privileges which is required to write to registry..
        //    }
        catch (Exception ex)
        {
            throw new Exception("Unable to read from registry", ex);
        }
    }

    static bool GetValue(string? key)
    {

        try
        {
            using RegistryKey? subKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", false);
            if (subKey != null)
            {
                Object? appTheme = subKey.GetValue(key);
                if (appTheme != null)
                {
                    int value = (int)appTheme;  //  REG_DWORD
                    return value == 0 ? false : true;
                }
            }
        }
        //    catch () 
        //    { 
        //          // notify user if missing elevated privileges which is required to write to registry..
        //    }
        catch (Exception ex)
        {
            throw new Exception("Unable to read from registry", ex);
        }
        return false;
    }
}

