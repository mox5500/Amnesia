using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;
using System.IO;
using System.Security.Principal;
using System.Net;


namespace Amnesia
{
    public partial class Form2 : Form
    {
        private Thread connect;
        private Thread disconnect;
        private Thread getIP;
        public Form2()
        {
            InitializeComponent();
            FetchInfo();
            startIPFetch();
        }

        private void startIPFetch()
        {
            getIP = new Thread(new ThreadStart(GetIP));
            getIP.Start();
        }

        void GetIP()
        {
            WebClient wc = new WebClient();
            while (true)
            {
                string fanny = wc.DownloadString("http://dropleak.ml/ip.php");
                toolStripStatusLabel1.Text = "IP: " + fanny;
                Thread.Sleep(30000);
            }
        }

        private void FetchInfo()
        {
            ComputerInfo ci = new ComputerInfo();
            string os = ci.OSFullName;
            log("Running : " + os);
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                log(Environment.UserName.ToString() + " : is an administrator.");
            }
            else
            {
                log(Environment.UserName.ToString() + " : is not an administrator.");
            }
        }

        private void log(string str)
        {
            richTextBox1.AppendText("[" + DateTime.Now.ToLongTimeString() + "] : " + str + "\n"); 
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/c taskkill /f /im Amnesia.exe";
            process.StartInfo = startInfo;
            process.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox2.Text != "")
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.Arguments = "/C net user /add " + textBox1.Text + " " + textBox2.Text;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                info.CreateNoWindow = true;
                info.FileName = "cmd.exe";
                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;
                using (Process process = Process.Start(info))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        log(result);
                    }
                } 

                if (checkBox1.Checked == true)
                {
                    ProcessStartInfo info2 = new ProcessStartInfo();
                    info2.Arguments = "/C net localgroup administrators " + textBox1.Text + " /add";
                    info2.WindowStyle = ProcessWindowStyle.Hidden;
                    info2.CreateNoWindow = true;
                    info2.FileName = "cmd.exe";
                    info2.UseShellExecute = false;
                    info2.RedirectStandardOutput = true;
                    using (Process process = Process.Start(info))
                    {
                        using (StreamReader reader2 = process.StandardOutput)
                        {
                            string result2 = reader2.ReadToEnd();
                            log(result2);
                        }
                    } 
                    
                }
            }
            else
            {
                MessageBox.Show("Try entering some data in the textboxes?", "No data!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void startconnect()
        {
            connect = new Thread(new ThreadStart(connecttoVPN));
            connect.Start();
        }

        private void startdisconnect()
        {
            disconnect = new Thread(new ThreadStart(disconnectfromVPN));
            disconnect.Start();
        }

        private void button2_Click(object sender, EventArgs e) // connect
        {
            startconnect();
        }

        void connecttoVPN()
        {
            string str1 = "euro214.vpnbook.com";
            string str2 = "vpnbook";
            string str3 = "8uWrepAw";
            try
            {
                log("Attempting to connect to VPN...");
                if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\vpnconnector"))
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\vpnconnector");
                System.IO.File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\vpnconnector\\connection.pbk", "[VPN]" + Environment.NewLine + "MEDIA=rastapi" + Environment.NewLine + "Port=VPN2-0" + Environment.NewLine + "Device=WAN Miniport (IKEv2)" + Environment.NewLine + "DEVICE=vpn" + Environment.NewLine + "PhoneNumber=" + str1);
                System.IO.File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\vpnconnector\\connection.bat", "rasdial \"VPN\" " + str2 + " " + str3 + " /phonebook:\"" + Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\vpnconnector\\connection.pbk\"");

                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\vpnconnector\\connection.bat";
                info.WindowStyle = ProcessWindowStyle.Normal;
                info.CreateNoWindow = true;
                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                using (Process process2 = Process.Start(info))
                {
                    using (StreamReader reader = process2.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        log(result);
                    }
                }

                button2.Enabled = false;
                button3.Enabled = true;
                connect.Abort();
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error connecting to VPN! Check below for more detail. \n \n" + ex.ToString(), "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void disconnectfromVPN()
        {
            try
            {
                log("Attempting to disconnect from VPN...");
                System.IO.File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\vpnconnector\\disconnect.bat", "rasdial/d");

                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\vpnconnector\\disconnect.bat";
                info.WindowStyle = ProcessWindowStyle.Normal;
                info.CreateNoWindow = true;
                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                using (Process process2 = Process.Start(info))
                {
                    using (StreamReader reader = process2.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        log(result);
                    }
                }

                button2.Enabled = true;
                button3.Enabled = false;
                disconnect.Abort();
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error disconnecting from VPN! Check below for more detail. \n \n" + ex.ToString(), "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            disconnectfromVPN();
        }

        private void richTextBox1_SelectionChanged(object sender, EventArgs e)
        {
            richTextBox1.ScrollToCaret();
        }
    }
}
