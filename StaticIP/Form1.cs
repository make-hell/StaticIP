using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StaticIP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            get_interfaces();
            //button4_Click(new object(), new EventArgs());
        }

        public class Adapters
        {
            public System.Collections.Generic.List<String> net_adapters()
            {
                List<String> values = new List<String>();
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    values.Add(nic.Name);
                }
                return values;
            }
        }

        private void get_interfaces()
        {
            var obj = new Adapters();
            var values = obj.net_adapters();
            foreach (var value in values)
            {
                comboBox1.Items.Add(value);
            }
        }

        private void change_interface(string command)
        {
            //string command = "netsh interface ip set address \"" + adaptername + "\" static \"" + ip + "\" \"" + mask + "\" \"" + gateway + "\"";

            // Create a new Process instance
            Process process = new Process();

            // Configure the process start info
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c {command}";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            try
            {
                // Start the process
                process.Start();

                // Read the output (optional)
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                // Wait for the process to exit
                process.WaitForExit();

                // Display the output and errors (if any)
                Console.WriteLine("Output: " + output);
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine("Error: " + error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
            finally
            {
                // Ensure the process resources are cleaned up
                if (process != null)
                {
                    process.Close();
                }
            }
        }

        private bool CheckIPValid(string strIP)
        {
            IPAddress address;
            if (IPAddress.TryParse(strIP, out address))
            {
                switch (address.AddressFamily)
                {
                    case System.Net.Sockets.AddressFamily.InterNetwork:
                        // we have IPv4
                        return true;
                    //break;
                    case System.Net.Sockets.AddressFamily.InterNetworkV6:
                        // we have IPv6
                        return true;
                    //break;
                    default:
                        return false;
                        //break;
                }
            }
            return false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            if (comboBox1.Items.Count == 0)
            {
                richTextBox1.AppendText("No Adapter Interface selected...");
                richTextBox1.AppendText(Environment.NewLine);
                richTextBox1.AppendText(Environment.NewLine);
                return;
            }

            if (CheckIPValid(textBox1.Text) == false)
            {
                richTextBox1.AppendText("IP Address is not Valid...");
                richTextBox1.AppendText(Environment.NewLine);
                richTextBox1.AppendText(Environment.NewLine);
                return;
            }

            bool netmaskValid = IsValidNetmask(textBox2.Text);
            bool gatewayValid = IsValidGateway(textBox1.Text, textBox2.Text, textBox3.Text);

            if (netmaskValid == false)
            {
                richTextBox1.AppendText("Netmask is not valid. Please check!!");
                richTextBox1.AppendText(Environment.NewLine);
                richTextBox1.AppendText(Environment.NewLine);
                return;
            }else if (gatewayValid == false)
            {
                richTextBox1.AppendText("Netmask is not valid. Please check!!");
                richTextBox1.AppendText(Environment.NewLine);
                richTextBox1.AppendText(Environment.NewLine);
                return;
            }
            else
            {
                richTextBox1.AppendText("Configuration is Valid. Applying these settings...");
                richTextBox1.AppendText(Environment.NewLine);
            }

            string statement = "netsh interface ip set address \"" + comboBox1.Text + "\" static \"" + textBox1.Text + "\" \"" + textBox2.Text + "\" \"" + textBox3.Text + "\"";
            change_interface(statement);
            richTextBox1.AppendText("The settings have been applied.");
            richTextBox1.AppendText(Environment.NewLine);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            // Add a check that there is an interface selected
            richTextBox1.AppendText("Setting the Network Interface \"" + comboBox1.Text + "\" to DHCP Settings...");
            richTextBox1.AppendText(Environment.NewLine);
            string statement = "netsh interface ipv4 set address name=\"" + comboBox1.Text + "\" source=dhcp";
            change_interface(statement);
            richTextBox1.AppendText("The settings have been applied.");
            richTextBox1.AppendText(Environment.NewLine);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //string statement = "ipconfig | find /i \"IPv4\"";
            //change_interface(statement);
            string type;
            int xx = 1;
            richTextBox1.Clear();
            //string[] NwDesc = { "TAP", "VMware", "Windows"};  // Adapter types (Description) to be ommited
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                //if (ni.NetworkInterfaceType != NetworkInterfaceType.Ethernet)  // check for adapter type and its description
                //{

                if (ni.GetIPProperties().GetIPv4Properties().IsDhcpEnabled)
                {
                    type = "DHCP";
                }
                else
                {
                    type = "STATIC";
                }

                foreach (UnicastIPAddressInformation ips in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ips.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) //to exclude automatic ips
                    {
                        string ip = ips.Address.ToString();
                        string EthName = ni.Name;
                        comboBox1.Items.Add(EthName);
                        string gw = DisplayGatewayAddresses(EthName);
                        string sub = ips.IPv4Mask.ToString();

                        richTextBox1.AppendText("ETH  =  " + EthName);
                        richTextBox1.AppendText(Environment.NewLine);
                        richTextBox1.AppendText("TYPE =  " + type);
                        richTextBox1.AppendText(Environment.NewLine);
                        richTextBox1.AppendText("IP:  =  " + ip);
                        richTextBox1.AppendText(Environment.NewLine);
                        richTextBox1.AppendText("NM:  =  " + sub);
                        richTextBox1.AppendText(Environment.NewLine);
                        richTextBox1.AppendText("GW:  =  " + gw);
                        richTextBox1.AppendText(Environment.NewLine);
                    }
                }

                foreach (IPAddress dnsAdress in ni.GetIPProperties().DnsAddresses)
                {
                    if (dnsAdress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        string dns = dnsAdress.ToString();
                        richTextBox1.AppendText("DNS" + xx + " =  " + dns);
                        richTextBox1.AppendText(Environment.NewLine);
                        xx++;
                    }
                }
                richTextBox1.AppendText(Environment.NewLine);
                //}
            }
        }

        public string DisplayGatewayAddresses(string nic)
        {
            //Console.WriteLine("Gateways");
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                GatewayIPAddressInformationCollection addresses = adapterProperties.GatewayAddresses;
                if (addresses.Count > 0)
                {
                    //Console.WriteLine(adapter.Name);
                    //Console.WriteLine(nic);
                    foreach (GatewayIPAddressInformation address in addresses)
                    {
                        if (adapter.Name == nic)
                        {
                            //Console.WriteLine("  Gateway Address ......................... : {0}",address.Address.ToString());
                            return address.Address.ToString();
                        }
                    }
                }
            }
            return "";
        }

        // Function to check if a netmask is valid
        static bool IsValidNetmask(string netmask)
        {
            string[] validNetmasks = {
            "255.0.0.0", "255.255.0.0", "255.255.255.0",
            "255.255.255.128", "255.255.255.192", "255.255.255.224",
            "255.255.255.240", "255.255.255.248", "255.255.255.252"
        };

            return Array.Exists(validNetmasks, m => m == netmask);
        }

        // Function to check if the gateway is within the network defined by IP and netmask
        static bool IsValidGateway(string ipAddress, string netmask, string gateway)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            IPAddress mask = IPAddress.Parse(netmask);
            IPAddress gw = IPAddress.Parse(gateway);

            byte[] ipBytes = ip.GetAddressBytes();
            byte[] maskBytes = mask.GetAddressBytes();
            byte[] gwBytes = gw.GetAddressBytes();

            // Calculate network address by applying the netmask
            byte[] networkBytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
            }

            // Check if gateway is within the network
            bool isValid = true;
            for (int i = 0; i < 4; i++)
            {
                if ((gwBytes[i] & maskBytes[i]) != networkBytes[i])
                {
                    isValid = false;
                    break;
                }
            }

            return isValid;
        }


    }
}
