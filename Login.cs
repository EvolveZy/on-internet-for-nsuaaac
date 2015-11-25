using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Collections.Specialized;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Threading;
using System.Reflection;

namespace MYAIR
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string str;
        string to;
        string ipv4;
        string hostName = Dns.GetHostName();
        string mac;

        private void Form1_Load(object sender, EventArgs e)
        {
            //qxclose();
           // Startup();
            pictureBox2.Enabled = false;
            notifyIcon1.Visible = false;
            ManagementClass mc;
            mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (mo["IPEnabled"].ToString() == "True")
                {
                    mac = mo["MacAddress"].ToString();
                }
            }
            try                                                //可能有异常，放在try块中
            {

                RegistryKey location = Registry.LocalMachine;
                RegistryKey soft = location.OpenSubKey("SOFTWARE", false);
                RegistryKey myPass = soft.OpenSubKey("FTLiang", false);
                string UN = myPass.GetValue("UserID").ToString();
                string PW = myPass.GetValue("UserPW").ToString();
                this.UserID.Text = UN.Trim();
                string id = UserID.Text;
                byte[] result = Encoding.Default.GetBytes(id);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] output = md5.ComputeHash(result);
                String keyMD5 = BitConverter.ToString(output).Replace("-", "");
                String key = keyMD5.Substring(0, 8);
                this.UserPW.Text = Class1.Decrypt(PW.Trim(), key, key);

            }
            catch (Exception)                        //捕获异常
            {
                //显示异常信息
            }
        }
        #region 加开机启动
        public void Startup() 
        {
            try
            {
                string KJLJ = Application.ExecutablePath;

                if (!System.IO.File.Exists(KJLJ))//判断指定文件是否存在

                    return;

                string newKJLJ = KJLJ.Substring(KJLJ.LastIndexOf("\\") + 1);

                RegistryKey Rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                if (Rkey == null)

                    Rkey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");

                Rkey.SetValue(newKJLJ, KJLJ);
            }
            catch { }
        }
        #endregion
        private void login()
        {
            string notice=null;
            this.ms.Items.Clear();
            pictureBox2.Enabled = true;
            this.panel1.Visible = false;
            this.panel2.Visible = true;
            label5.Visible = false;
            pictureBox4.Visible = false;
            pictureBox2.Visible = true;
            pictureBox2.Enabled = true;
            ms.Visible = true;
            Thread.Sleep(900);
            try
            {
                this.ms.Items.Add("航班正在起飞！\r\n");
                string id = UserID.Text;
                byte[] resultbt1 = Encoding.Default.GetBytes(id);
                MD5 md5bt1 = new MD5CryptoServiceProvider();
                byte[] outputid1 = md5bt1.ComputeHash(resultbt1);
                String keyMD5 = BitConverter.ToString(outputid1).Replace("-", "");
                String key = keyMD5.Substring(0, 8);
                string PW = Class1.Encrypt(UserPW.Text, key, key);
                ShowIP();
                string xml = Properties.Resources.Login;
                xml = xml.Replace("$ErrInfo", Class1.ErrInfo);
                xml = xml.Replace("$UserID", UserID.Text);
                xml = xml.Replace("$UserPW", PW);
                xml = xml.Replace("$UserIP", ipv4);
                xml = xml.Replace("$ComputerName", hostName);
                xml = xml.Replace("$MAC", mac);
                xml = xml.Replace("$IsAutoLogin", "false");
                xml = xml.Replace("$ClientVersion", "1.15.9.18");
                xml = xml.Replace("$OSVersion", "Microsoft Windows NT 6.1.7601 Service Pack 1");
                //MessageBox.Show(xml);
                byte[] dataArray = Encoding.Default.GetBytes(xml);
                //创建请求
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Class1.ServerPage);
                request.Timeout = 5000;
                request.Method = "POST";
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; MS Web Services Client Protocol 4.0.30319.18052)";
                request.ContentLength = (long)xml.Length;
                request.ContentType = "text/xml; charset=utf-8";
                request.Headers.Add("SOAPAction", "\"http://tempuri.org/Login\"");
                //创建输入流
                Stream dataStream = null;
                try
                {
                    dataStream = request.GetRequestStream();
                }
                catch (Exception)
                {
                    MessageBox.Show("连接塔台失败!");
                    this.panel1.Visible = true;
                    this.panel2.Visible = false;
                    pictureBox2.Visible = false;
                    pictureBox2.Enabled = false;
                    label5.Visible = true;
                    pictureBox4.Visible = true;
                    pictureBox4.Enabled = false;
                    ms.Visible = false;
                    return;
                }
                //发送请求
                dataStream.Write(dataArray, 0, dataArray.Length);
                dataStream.Close();
                //读取返回消息
                string res = string.Empty;

                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    res = reader.ReadToEnd();
                    str = res;
                  // MessageBox.Show(res);
                }
                catch (Exception)
                {
                    MessageBox.Show("连接塔台失败!");
                    this.panel1.Visible = true;
                    this.panel2.Visible = false;
                    pictureBox2.Visible = false;
                    pictureBox2.Enabled = false;
                    label5.Visible = true;
                    pictureBox4.Visible = true;
                    pictureBox4.Enabled = true;
                    ms.Visible = false;
                    return;
                }

                string flagstr = "</Name><Token>";
                int beginIndex = str.IndexOf(flagstr);
                if (beginIndex > -1)
                {
                    int endIndex = str.IndexOf("</Token>", beginIndex);
                    to = str.Substring(beginIndex + flagstr.Length, endIndex - beginIndex - flagstr.Length);
                    byte[] resultr = Encoding.Default.GetBytes(to);
                    MD5 md5r = new MD5CryptoServiceProvider();
                    byte[] outputr = md5r.ComputeHash(resultr);
                    to = BitConverter.ToString(outputr).Replace("-", "");

                }
                string flagstr5 = "<IsDisable>";
                int beginIndex5 = str.IndexOf(flagstr5);
                if (beginIndex5 > -1)
                {
                    int endIndex5 = str.IndexOf("</IsDisable>", beginIndex5);
                    string disable = str.Substring(beginIndex5 + flagstr5.Length, endIndex5 - beginIndex5 - flagstr5.Length);
                    if ("true" == disable)
                    {
                        string flagstr6 = "<Notice>";
                        int beginIndex6 = str.IndexOf(flagstr6);
                        int endIndex6 = str.IndexOf("</Notice>", beginIndex6);
                        notice = str.Substring(beginIndex6 + flagstr6.Length, endIndex6 - beginIndex6 - flagstr6.Length);
                        MessageBox.Show(notice);

                    }
                }
                string flagstr4 = "<IsLogin>";
                int beginIndex4 = str.IndexOf(flagstr4);
                if (beginIndex4 > -1)
                {
                    int endIndex4 = str.IndexOf("</IsLogin>", beginIndex4);
                    if ("true" == str.Substring(beginIndex4 + flagstr4.Length, endIndex4 - beginIndex4 - flagstr4.Length))
                    {
                        RegistryKey location = Registry.LocalMachine;
                        RegistryKey soft = location.OpenSubKey("SOFTWARE", true);//可写 
                        RegistryKey myPass = soft.CreateSubKey("FTLiang");
                        myPass.SetValue("UserID", UserID.Text);
                        myPass.SetValue("UserPW", PW);
                        this.ms.Items.Add("航班" + UserID.Text + "已成功起飞！\r\n");
                        string flagstr1 = "<Name>";
                        int beginIndex1 = str.IndexOf(flagstr1);
                        if (beginIndex1 > -1)
                        {
                            int endIndex1 = str.IndexOf("</Name>", beginIndex1);
                            this.ms.Items.Add("乘客 " + str.Substring(beginIndex1 + flagstr1.Length, endIndex1 - beginIndex1 - flagstr1.Length) + "，你好!\r\n");
                        }
                        string flagstr2 = "<NetGroup>";
                        int beginIndex2 = str.IndexOf(flagstr2);
                        if (beginIndex2 > -1)
                        {
                            int endIndex2 = str.IndexOf("</NetGroup>", beginIndex2);
                            string NetGroup = str.Substring(beginIndex2 + flagstr2.Length, endIndex2 - beginIndex2 - flagstr2.Length);
                            NetGroup = NetGroup.Substring(0, 2);
                            if ("50" == NetGroup)
                            {
                                this.ms.Items.Add("欢迎乘坐VIP舱(32M)\r\n");
                            }
                            if ("12" == NetGroup)
                            {
                                this.ms.Items.Add("欢迎乘坐头等舱(12M)\r\n");
                            }
                            if ("6M" == NetGroup)
                            {
                                this.ms.Items.Add("欢迎乘坐公务舱(6M)\r\n");
                            }
                            if ("2M" == NetGroup || "1M" == NetGroup)
                            {
                                this.ms.Items.Add("欢迎乘坐经济舱(" + NetGroup + ")\r\n");
                            }
                        }
                        string flagstr3 = "<ExpireTime>";
                        int beginIndex3 = str.IndexOf(flagstr3);
                        if (beginIndex3 > -1)
                        {
                            int endIndex3 = str.IndexOf("</ExpireTime>", beginIndex3);
                            this.ms.Items.Add("航班到达时间：" + str.Substring(beginIndex3 + flagstr3.Length, endIndex3 - beginIndex3 - flagstr3.Length));
                        }
                        timer1.Enabled = true;
                        pictureBox2.Enabled = true;
                        this.panel1.Visible = false;
                        this.panel2.Visible = true;
                        label5.Visible = false;
                        pictureBox4.Visible = false;
                        ms.Visible = true;

                    }
                    else
                    {
                        this.ms.Items.Add("抱歉，起飞失败!");
                        string flagstr3 = "<Notice>";
                        int beginIndex3 = str.IndexOf(flagstr3);
                        if (beginIndex3 > -1)
                        {
                            int endIndex3 = str.IndexOf("</Notice>", beginIndex3);
                            this.ms.Items.Add(str.Substring(beginIndex3 + flagstr3.Length, endIndex3 - beginIndex3 - flagstr3.Length));
                        }
                        Thread.Sleep(2000);
                        this.panel1.Visible = true;
                        this.panel2.Visible = false;
                        label5.Visible = true;
                        pictureBox4.Visible = true;
                        pictureBox4.Enabled = true;
                        ms.Visible = false;
                        //MessageBox.Show(notice);
                        return;
                    }
                   
                   
                }
            }

            catch (Exception)
            {
                MessageBox.Show("连接塔台失败!");
                this.panel1.Visible = true;
                this.panel2.Visible = false;
                pictureBox2.Visible = false;
                pictureBox2.Enabled = false;
                label5.Visible = true;
                pictureBox4.Visible = true;
                pictureBox4.Enabled = true;
                ms.Visible = false;

            }
        }
        private void logout()
        {
            try
            {
                ShowIP();          
                string xml = Properties.Resources.Logout;
                byte[] result1 = Encoding.Default.GetBytes(to);
                MD5 md51 = new MD5CryptoServiceProvider();
                byte[] output1 = md51.ComputeHash(result1);
                to = BitConverter.ToString(output1).Replace("-", "");
                xml = xml.Replace("$UserID", UserID.Text);
                xml = xml.Replace("$UserIP", ipv4);
                string newString = xml.Replace("$Token", to);
                byte[] dataArray = Encoding.Default.GetBytes(newString);
                //创建请求
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Class1.ServerPage);
                request.Method = "POST";
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; MS Web Services Client Protocol 4.0.30319.18052)";
                request.ContentLength = (long)newString.Length;
                request.ContentType = "text/xml; charset=utf-8";
                request.Headers.Add("SOAPAction", "\"http://tempuri.org/Logout\"");
                //创建输入流
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(dataArray, 0, dataArray.Length);
                dataStream.Close();
                //读取返回消息
                string res = string.Empty;
                string str1;
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    res = reader.ReadToEnd();
                    str1 = res;
                    // MessageBox.Show(res);
                    string flagstr1 = "<LogoutResult>";
                    int beginIndex1 = str1.IndexOf(flagstr1);
                    if (beginIndex1 > -1)
                    {
                        int endIndex1 = str1.IndexOf("</LogoutResult>", beginIndex1);
                        if ("true" == str1.Substring(beginIndex1 + flagstr1.Length, endIndex1 - beginIndex1 - flagstr1.Length))
                        {
                            this.ms.Items.Add("航班" + UserID.Text + "已成功降落!");
                            timer1.Enabled = false;
                            this.panel2.Visible = false;
                            this.panel1.Visible = true;
                            ms.Visible = false;
                            pictureBox4.Visible = true;
                            label5.Visible = true;
                        }
                        else
                        {
                            this.ms.Items.Add("航班" + UserID.Text + "降落失败!");
                        }
                    }

                }
                catch (Exception)
                {
                    // return null;//连接服务器失败
                }
            }
            catch (Exception)
            {
                MessageBox.Show("航班" + UserID.Text + "降落失败!");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Class1.ServerPage);
                ShowIP();
                string xml = Properties.Resources.KeepSession;
                byte[] result1 = Encoding.Default.GetBytes(to);
                MD5 md51 = new MD5CryptoServiceProvider();
                byte[] output1 = md51.ComputeHash(result1);
                to = BitConverter.ToString(output1).Replace("-", "");
                xml = xml.Replace("$UserID", UserID.Text);
                xml = xml.Replace("$UserIP", ipv4);
                xml = xml.Replace("$IsHaveNewMessage", "");
                string newString = xml.Replace("$Token", to);
                byte[] dataArray = Encoding.Default.GetBytes(newString);
                request.Method = "POST";
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; MS Web Services Client Protocol 4.0.30319.18052)";
                request.ContentLength = (long)newString.Length;
                request.ContentType = "text/xml; charset=utf-8";
                request.Headers.Add("SOAPAction", "\"http://tempuri.org/KeepSession\"");
                //创建输入流
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(dataArray, 0, dataArray.Length);
                dataStream.Close();
                //读取返回消息
                string res = string.Empty;
                string str1;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                res = reader.ReadToEnd();
                str1 = res;
                //MessageBox.Show(res);
                string flagstr1 = "<KeepSessionResult>";
                int beginIndex1 = str1.IndexOf(flagstr1);
                if (beginIndex1 > -1)
                {
                    int endIndex1 = str1.IndexOf("</KeepSessionResult>", beginIndex1);
                    if ("true" == str1.Substring(beginIndex1 + flagstr1.Length, endIndex1 - beginIndex1 - flagstr1.Length))
                    {
                        this.ms.Items.Add(DateTime.Now+" 航班正常!");
                        ms.TopIndex = ms.Items.Count - 1;
                    }
                    else
                    {
                        this.ms.Items.Add("已坠机!");
                        timer1.Enabled = false;
                        ms.Visible = false;
                        pictureBox4.Enabled = true;
                        pictureBox2.Enabled = false;
                        pictureBox4.Visible = true;
                        label5.Visible = true;
                        this.panel2.Visible = false;
                        this.panel1.Visible = true;
                        MessageBox.Show("已坠机，请重新起飞!");
                        return;

                    }
                }
            }
            catch (Exception)
            {

                this.ms.Items.Add("已坠机!");
                timer1.Enabled = false;
                ms.Visible = false;
                pictureBox4.Enabled = true;
                pictureBox2.Enabled = false;
                pictureBox4.Visible = true;
                label5.Visible = true;
                this.panel2.Visible = false;
                this.panel1.Visible = true;
                MessageBox.Show("已坠机，请重新起飞!");
                return;

            }
        }

        void ShowIP()
        {
            ipv4 = null;
            try
            {
                string HostName = Dns.GetHostName(); //得到主机名
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    //从IP地址列表中筛选出IPv4类型的IP地址
                    //AddressFamily.InterNetwork表示此IP为IPv4,
                    //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        if ("10"==IpEntry.AddressList[i].ToString().Substring(0, 2)&&ipv4==null)
                        {
                            ipv4 = IpEntry.AddressList[i].ToString();
                        }
                        else if ("10" == IpEntry.AddressList[i].ToString().Substring(0, 2)&&ipv4!=null)
                        {
                            ipv4 = ipv4 + "," + IpEntry.AddressList[i].ToString();
                        }
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show("获取本机IP出错:" + ex.Message);
                return;
            }
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            login();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            logout();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.ShowInTaskbar = true;
                this.WindowState = FormWindowState.Normal;
                notifyIcon1.Visible = false;
            }

        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)  //判断是否最小化
            {
                this.ShowInTaskbar = false;  //不显示在系统任务栏
                notifyIcon1.Visible = true;  //托盘图标可见
            }
        }
        private void qxclose()
        {
            string quanxian = null;
            try
            {
               
                RegistryKey location = Registry.LocalMachine;
                RegistryKey soft = location.OpenSubKey("SOFTWARE", false);//可写 
                RegistryKey myPass = soft.OpenSubKey("FTLiang", false);
                quanxian = myPass.GetValue("QX").ToString();
              
            }
            catch (Exception)
            {

            }
            byte[] resultbt2 = Encoding.Default.GetBytes(hostName + "myaaa1886243869");
            MD5 md5bt2 = new MD5CryptoServiceProvider();
            byte[] outputid2 = md5bt2.ComputeHash(resultbt2);
            String keyMD5 = BitConverter.ToString(outputid2).Replace("-", "");
            if (quanxian != keyMD5)
            {
                MessageBox.Show("你无权使用！");
                Application.Exit();
               
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {
            AboutBox1 guanyu = new AboutBox1();
            guanyu.Show();
        }
    }
}
