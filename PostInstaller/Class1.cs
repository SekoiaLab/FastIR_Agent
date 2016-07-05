using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;


[RunInstaller(true)]
public class Install : Installer
{
    public Install()
    {
    }

    protected override void OnBeforeUninstall(IDictionary savedState)
    {
        //Unregister the service
        using (Process process = new Process())
        {
            if (Environment.Is64BitOperatingSystem)
                process.StartInfo.FileName = "C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\installutil.exe";
            else
                process.StartInfo.FileName = "C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\installutil.exe";
            process.StartInfo.Arguments = "/u \"c:\\Program Files\\SEKOIA\\FastIR Agent\\FastIR-agent.exe\"";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.Start();
            process.WaitForExit();
        }

        base.OnAfterInstall(savedState);
    }

    protected override void OnAfterInstall(IDictionary savedState)
    {

        //Register the service
        using (Process process = new Process())
        {
            process.StartInfo.FileName = "C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\installutil.exe";
            process.StartInfo.Arguments = "\"c:\\Program Files\\SEKOIA\\FastIR Agent\\FastIR-agent.exe\"";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.Start();
            process.WaitForExit();
        }


        //Add parameters got from the interface
        string URL = Context.Parameters["EDITC1"];
        string Port = Context.Parameters["EDITC2"];
        string APIKey = Context.Parameters["EDITC3"];
        string SSL = Context.Parameters["EDITC4"];

        const string HKLMRoot = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\FastIR";
        try { Registry.SetValue(HKLMRoot, "URL", URL); }
        catch { }
        try { Registry.SetValue(HKLMRoot, "Port", Port); }
        catch { }
        try { Registry.SetValue(HKLMRoot, "APIKey", APIKey); }
        catch { }
        try { Registry.SetValue(HKLMRoot, "PUBLIC_SSL", SSL); }
        catch { }
        try { Registry.SetValue(HKLMRoot, "RefreshMin", "60"); }
        catch { }

        //Start the service
        using (ServiceController sc = new ServiceController("FastIR"))
        {
            sc.Start();
        }

        base.OnAfterInstall(savedState);
    }
}
