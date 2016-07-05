using Microsoft.Win32;
using network_appli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Net;

public enum ServiceState
{
    SERVICE_STOPPED = 0x00000001,
    SERVICE_START_PENDING = 0x00000002,
    SERVICE_STOP_PENDING = 0x00000003,
    SERVICE_RUNNING = 0x00000004,
    SERVICE_CONTINUE_PENDING = 0x00000005,
    SERVICE_PAUSE_PENDING = 0x00000006,
    SERVICE_PAUSED = 0x00000007,
}

[StructLayout(LayoutKind.Sequential)]
public struct ServiceStatus
{
    public uint dwServiceType;
    public ServiceState dwCurrentState;
    public uint dwControlsAccepted;
    public uint dwWin32ExitCode;
    public uint dwServiceSpecificExitCode;
    public uint dwCheckPoint;
    public uint dwWaitHint;
};

public struct Params
{
    public string URL;
    public string Port;
    public string APIKey;
    public int RefreshMin;
    public string ApplicationPath;
    public string lpk;
};

public class ReturnJson
{
    public string Appli { get; set; }
    public string Version { get; set; }
    public string Return { get; set; }
    public string Data { get; set; }
    public string Order { get; set; }
    public string Binary { get; set; }
}

namespace FastIR_Agent
{
    public partial class FastIR : ServiceBase
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        System.Timers.Timer timer = new System.Timers.Timer();
        Params P;

        public FastIR()
        {
            InitializeComponent();
            if (!System.Diagnostics.EventLog.SourceExists("FastIR"))
            {
                System.Diagnostics.EventLog.CreateEventSource("FastIR", "Application");
            }
            eventLog.Source = "FastIR";
            eventLog.Log = "Application";
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                SetServiceState(ServiceState.SERVICE_START_PENDING, 100000);
                try
                {
                    P.RefreshMin = Int32.Parse(ReadKey("RefreshMin"));
                }
                catch { P.RefreshMin = 60; }
                eventLog.WriteEntry("FastIR Agent: the service is started. Refresh every "+ P.RefreshMin +" minutes", EventLogEntryType.Information);               
                timer.Interval = 60000 * P.RefreshMin; // 60 seconds
                timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
                timer.Start();
                SetServiceState(ServiceState.SERVICE_RUNNING);
            } catch {SetServiceState(ServiceState.SERVICE_STOPPED); }
        }

        protected override void OnStop()
        {
            try
            {
                SetServiceState(ServiceState.SERVICE_STOP_PENDING, 100000);
                eventLog.WriteEntry("FastIR Agent: the service is stopped", EventLogEntryType.Information);
                timer.Close();
                SetServiceState(ServiceState.SERVICE_STOPPED);
            } catch { SetServiceState(ServiceState.SERVICE_STOPPED); }
        }

        private void SetServiceState(ServiceState state, uint waitHint = 0)
        {
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = state;
            serviceStatus.dwWaitHint = waitHint;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        private string GetSHA256(string fileName)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(fileName))
                {
                    return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
        }

        private void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            EventLog.WriteEntry("FastIR Agent: check for order...", EventLogEntryType.Information);
            //Get keys
            P.URL = ReadKey("URL");
            P.Port = ReadKey("Port");
            P.APIKey = ReadKey("APIKey");
            P.lpk = ReadKey("PUBLIC_SSL");
            string exe = Process.GetCurrentProcess().MainModule.FileName;
            P.ApplicationPath = Path.GetDirectoryName(exe); 
            //
            if (P.Port.Equals(null) || P.URL.Equals(null) || P.lpk.Equals(null) || P.APIKey.Equals(null))
            {
                EventLog.WriteEntry("FastIR Agent: Bad URL, Port, or SSL Public key in the configuration...", EventLogEntryType.Error);
                ServiceController sc = new ServiceController("FastIR");
                sc.Stop();
                sc.Close();
            }
            else
            {
                EventLog.WriteEntry("FastIR Agent: URL: " + P.URL + ":" + P.Port + ".", EventLogEntryType.Information);
                NetworkFastIR prop = new NetworkFastIR(P.URL, P.Port, P.lpk);
                ret r = prop.query("");
                if (r.SSL_status)
                {
                    ReturnJson rj;
                    try
                    {
                        rj = JsonConvert.DeserializeObject<ReturnJson>(r.Web_return);
                        EventLog.WriteEntry("FastIR Agent: Connection established to the server", EventLogEntryType.Information);
                        //Get new FastIR binary
                        string arch = null;
                        if (Environment.Is64BitOperatingSystem)
                            arch = "x64";
                        else
                            arch = "x86";
                        string sha256 = "";
                        try
                        {
                            sha256 = GetSHA256(P.ApplicationPath+"\\FastIR.exe");
                        }
                        catch { }
                        string POST = "APIKey=" + P.APIKey + "&sha256=" + sha256 + "&arch=" + arch;
                        r = prop.query("getbinary/", POST);
                        if (r.SSL_status)
                        {
                            try
                            {
                                rj = JsonConvert.DeserializeObject<ReturnJson>(r.Web_return);
                            }
                            catch { rj = null; }

                            if (rj.Equals(null))
                                EventLog.WriteEntry("FastIR Agent: bad json from the server", EventLogEntryType.Error);
                            else if (rj.Return.Equals("KO"))
                                EventLog.WriteEntry("FastIR Agent: bad request: "+rj.Data, EventLogEntryType.Error);
                            else
                            {
                                if (rj.Binary.Equals("1"))
                                {
                                    EventLog.WriteEntry("FastIR Agent: new FastIR binary available", EventLogEntryType.Information);
                                    try
                                    {
                                        byte[] data = Convert.FromBase64String(rj.Data);
                                        File.WriteAllBytes(P.ApplicationPath + "\\FastIR.exe", data);
                                    }
                                    catch { EventLog.WriteEntry("FastIR Agent: cannot download the new FastIR binary", EventLogEntryType.Error); }
                                }
                                else
                                    EventLog.WriteEntry("FastIR Agent: no new FastIR binary", EventLogEntryType.Information);
                            }
                        }
                        else
                            EventLog.WriteEntry("FastIR Agent: Bad SSL or URL", EventLogEntryType.Error);

                        //Get Order
                        POST = "APIKey=" + P.APIKey + "&hostname=" + Dns.GetHostName();
                        r = prop.query("getorder/", POST);
                        if (r.SSL_status)
                        {
                            try
                            {
                                rj = JsonConvert.DeserializeObject<ReturnJson>(r.Web_return);
                            }
                            catch { rj = null; }

                            if (rj.Equals(null))
                                EventLog.WriteEntry("FastIR Agent: bad json from the server", EventLogEntryType.Error);
                            else if (rj.Return.Equals("KO"))
                                EventLog.WriteEntry("FastIR Agent: bad request: " + rj.Data, EventLogEntryType.Error);
                            else
                            {
                                if (rj.Order.Equals("1"))
                                {
                                    EventLog.WriteEntry("FastIR Agent: an order exist for the machine: "+Dns.GetHostName(), EventLogEntryType.Information);
                                    try
                                    {
                                        PECheck pe = new PECheck();
                                        byte[] data = Convert.FromBase64String(rj.Data);
                                        File.WriteAllBytes(P.ApplicationPath + "\\FastIR.conf", data);
                                        EventLog.WriteEntry("FastIR Agent: the new config file is created.", EventLogEntryType.Information);
                                        if (pe.checkfile(P.ApplicationPath + "\\FastIR.exe"))
                                        {
                                            using(Process process = new Process())
                                            {
                                                EventLog.WriteEntry("FastIR Agent: execution of the collect.", EventLogEntryType.Information);
                                                process.StartInfo.FileName = P.ApplicationPath + "\\FastIR.exe";
                                                process.StartInfo.Arguments = "--profile " + P.ApplicationPath + "\\FastIR.conf";
                                                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                                process.Start();
                                            }
                                        } else
                                        {
                                            EventLog.WriteEntry("FastIR Agent: bad signature.", EventLogEntryType.Error);
                                        }
                                    }
                                    catch { EventLog.WriteEntry("FastIR Agent: cannot download the config file", EventLogEntryType.Error); }
                                }
                                else
                                    EventLog.WriteEntry("FastIR Agent: no order for the machine: "+ Dns.GetHostName(), EventLogEntryType.Information);
                            }
                        }
                        else
                            EventLog.WriteEntry("FastIR Agent: Bad SSL or URL", EventLogEntryType.Error);
                    }
                    catch { EventLog.WriteEntry("FastIR Agent: bad json from the server", EventLogEntryType.Error); }
                }
                else
                    EventLog.WriteEntry("FastIR Agent: Bad SSL or URL", EventLogEntryType.Error);
            }
        }

        private string ReadKey(string KeyName)
        {
            const string HKLMRoot = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\FastIR";
            try
            {
                string Value = (string)Registry.GetValue(HKLMRoot, KeyName, null);
                return Value;
            }
            catch
            {
                return null;
            }
        }
    }
}
