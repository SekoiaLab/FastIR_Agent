using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace utils
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    public partial class MainWindow : Window
    {
        Deps d = new Deps();

        public MainWindow()
        {
            InitializeComponent();
            UpdateStatus();
        }

        private void UpdateTab(object sender, SelectionChangedEventArgs e)
        {  
            if (status.IsSelected) 
                UpdateStatus(); 
            if (configuration.IsSelected) 
                UpdateConfiguration();
            if (MISC.IsSelected)
                UpdateMisc();
        }

        public void UpdateMisc()
        {

        }

        public void UpdateConfiguration()
        {
            string _RefreshMin = d.ReadKey("RefreshMin");
            string _URL = d.ReadKey("URL");
            string _Port = d.ReadKey("Port");
            string _APIKey = d.ReadKey("APIKey");
            string _SSL = d.ReadKey("PUBLIC_SSL");
            URL.Text = _URL;
            Port.Text = _Port;
            APIKey.Text = _APIKey;
            SSLKey.Text = _SSL;
            RefreshMin.Text = _RefreshMin;
        }

        public void UpdateStatus()
        {
            bool service = false;
            var converter = new System.Windows.Media.BrushConverter();

            try
            {
                ServiceController sc = new ServiceController("FastIR");
                ServiceControllerStatus scs = sc.Status;
                if (scs.Equals(ServiceControllerStatus.Running))
                {
                    Label_FastIR_Service.Foreground = (Brush)converter.ConvertFromString("#008000");
                    Label_FastIR_Service.Text = "RUNNING";
                    service = true;
                    StopSC.IsEnabled = true;
                    StartSC.IsEnabled = false;
                }
                else
                {
                    Label_FastIR_Service.Foreground = (Brush)converter.ConvertFromString("#FF0000");
                    Label_FastIR_Service.Text = "STOPPED";
                    service = false;
                    StopSC.IsEnabled = false;
                    StartSC.IsEnabled = true;
                }
                sc.Close();
            } catch {
                Label_FastIR_Service.Foreground = (Brush)converter.ConvertFromString("#008B8B");
                Label_FastIR_Service.Text = "does not exist";
                service = false;
                StopSC.IsEnabled = false;
                StartSC.IsEnabled = false;
            }

            if (service.Equals(true))
            {
                if (d.CheckURL())
                {
                    Label_FastIR_Network.Foreground = (Brush)converter.ConvertFromString("#008000");
                    Label_FastIR_Network.Text = "OK";
                }
                else
                {
                    Label_FastIR_Network.Foreground = (Brush)converter.ConvertFromString("#FF0000");
                    Label_FastIR_Network.Text = "KO";
                }
                Final.Text = "FastIR Agent is working on the system.";
            }
            else
            {
                Label_FastIR_Network.Foreground = (Brush)converter.ConvertFromString("#008B8B");
                Label_FastIR_Network.Text = "N/A";
                Final.Text = "FastIR Agent does work on the system...";
            }
        }

        private void UpdateConfBtn(object sender, RoutedEventArgs e)
        {
            const string HKLMRoot = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\FastIR";
            try { Registry.SetValue(HKLMRoot, "URL", URL.Text); }
            catch { }
            try { Registry.SetValue(HKLMRoot, "Port", Port.Text); }
            catch { }
            try { Registry.SetValue(HKLMRoot, "APIKey", APIKey.Text); }
            catch { }
            try { Registry.SetValue(HKLMRoot, "PUBLIC_SSL", SSLKey.Text); }
            catch { }
            try { Registry.SetValue(HKLMRoot, "RefreshMin", RefreshMin.Text); }
            catch { }
            Label_UpdateBtn.Content = "Configuration updated";
        }

        private void GetSSLBtn(object sender, RoutedEventArgs e)
        {
            if (d.GetPublicKey(SSL_URL.Text))
            {
                if (d.G_SSL.Equals(null))
                    SSL_RESPONSE.Text = "Bad URL (SSL is mandatory)";
                else
                    SSL_RESPONSE.Text = d.G_SSL;
                d.G_SSL = null;
            } else
            {
                SSL_RESPONSE.Text = "Bad URL (SSL is mandatory)";
            }
        }

        private void StartSCBtn(object sender, RoutedEventArgs e)
        {
            try
            {
                using (ServiceController sc = new ServiceController("FastIR"))
                {
                    sc.Start();
                }
            } catch { }
            UpdateStatus();
        }

        private void StopSCBtn(object sender, RoutedEventArgs e)
        {
            try
            {
                using (ServiceController sc = new ServiceController("FastIR"))
                {
                    sc.Stop();
                }
            } catch { }
            UpdateStatus();
        }
    }
}