/*
 * 若要人不知，除非己莫为！
                ┏┓　　　┏┓ 
　　　　　　　┏┛┻━━━┛┻┓ 
　　　　　　　┃　　　　　　　┃ 　 
　　　　　　　┃　　　━　　　┃ 
　　　　　　　┃　┳┛　┗┳　┃ 
　　　　　　　┃　　　　　　　┃ 
　　　　　　　┃　　　┻　　　┃ 
　　　　　　　┃　　　　　　　┃ 
　　　　　　　┗━┓　　　┏━┛ 
　　　　　　　　　┃　　　┃　　　　　　　　　　　 
　　　　　　　　　┃　　　┃ 
　　　　　　　　　┃　　　┗━━━┓ 
　　　　　　　　　┃　　　　　　　┣┓ 
　　　　　　　　　┃　　　　　　　┏┛ 
　　　　　　　　　┗┓┓┏━┳┓┏┛ 
　　　　　　　　　　┃┫┫　┃┫┫ 
　　　　　　　　　　┗┻┛　┗┻┛
 * 嘿嘿,上帝保佑！

*/



using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net.Mail;
using System.Runtime.InteropServices;

namespace HEX_INFO_COLLECTOR
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists("info.log"))
            {
                File.Delete("info.log");
            }
            //解决跨线程调用窗体的问题，用于后边progressBar1在其他线程中的调用
            Control.CheckForIllegalCrossThreadCalls = false;
            //加载配置文件
            if (File.Exists("app.ini"))
            {
                StringBuilder stringBud = new StringBuilder(50);
                GetPrivateProfileString("INFO", "Uni_URL", "NotSet", stringBud, 50, ".\\app.ini");
                textBox1.Text = stringBud.ToString();
                GetPrivateProfileString("INFO", "Tel_URL", "NotSet", stringBud, 50, ".\\app.ini");
                textBox2.Text = stringBud.ToString();
                GetPrivateProfileString("INFO", "Port", "NotSet", stringBud, 50, ".\\app.ini");
                textBox3.Text = stringBud.ToString();
                GetPrivateProfileString("INFO", "DNS", "NotSet", stringBud, 50, ".\\app.ini");
                textBox4.Text = stringBud.ToString();
                GetPrivateProfileString("INFO", "Email", "NotSet", stringBud, 50, ".\\app.ini");
                textBox5.Text = stringBud.ToString();
                if (textBox5.Text != "")
                {
                    checkBox1.Checked = true;
                }
                else
                {
                    checkBox1.Checked = false;
                }
            }
            else
            {
                MessageBox.Show("配置文件不存在，请手动填写内容！", "警告！");
            }

        }


        //引用DLL用于读取ini配置文件
        [DllImport("kernel32")]//返回0表示失败，非0为成功
        private static extern long WritePrivateProfileString(string section, string key,
            string val, string filePath);

        [DllImport("kernel32")]//返回取得字符串缓冲区的长度
        private static extern long GetPrivateProfileString(string section, string key,
            string def, StringBuilder retVal, int size, string filePath);


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(System.Environment.ExitCode);
        }


        //定义临时文件夹
        string temp_dir = System.IO.Path.GetTempPath();
        //定义换行
        string endl = System.Environment.NewLine;

        private void button1_Click(object sender, EventArgs e)
        {
            //改变按钮文字
            button1.Text = "正在检测";
            //设置进度条
            progressBar1.Maximum = 100;
            progressBar1.Minimum = 0;
            progressBar1.Step = 1;
            timer1.Interval = 1500;
            timer1.Enabled = true;
            //设定文件名
            string filename = "info.log";
            //获取系统时间
            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;
            string time_rec = "当前系统时间：" + currentTime.ToString() + endl;
            //删除（关闭）防火墙规则
            //string cmd = "netsh advfirewall firewall delete rule name=all";
            string cmd_off = "netsh advfirewall set allprofiles state off";
            Execute(cmd_off, 1);
            //测试网络连通性
            string InetCon = "外网连通性：" + InternetCon() + endl;
            string PublicIP = "外网地址是：" + GetIP() + endl;
            Task.Factory.StartNew(() =>
            {
                this.FileRec(time_rec + InetCon + PublicIP, filename);
            });
            Thread.Sleep(500);


            //尝试解析域名
            if (!IsIP(textBox1.Text))
            {
                try
                {
                    string UniIP = "联通域名" + "(" + textBox1.Text + ")" + "IP：" + getAddress(textBox1.Text) + endl;
                    Task.Factory.StartNew(() =>
                    {
                        this.FileRec(UniIP, filename);
                    });
                    Thread.Sleep(500);
                }
                catch (Exception)
                {
                    string errmsg = "联通域名" + "(" + textBox1.Text + ")" + "IP：" + "无法解析！" + endl;
                    Task.Factory.StartNew(() =>
                    {
                        this.FileRec(errmsg, filename);
                    });
                    Thread.Sleep(500);
                }
            }

            if (!IsIP(textBox2.Text))
            {
                try
                {
                    string TelIP = "电信域名" + "(" + textBox2.Text + ")" + "IP：" + getAddress(textBox2.Text) + endl;
                    Task.Factory.StartNew(() =>
                    {
                        //Thread.Sleep(1000);
                        this.FileRec(TelIP, filename);
                    });
                    Thread.Sleep(500);
                }
                catch (Exception)
                {
                    string errmsg = "电信域名" + "(" + textBox2.Text + ")" + "IP：" + "无法解析！" + endl;
                    Task.Factory.StartNew(() =>
                    {
                        //Thread.Sleep(1000);
                        this.FileRec(errmsg, filename);
                    });
                    Thread.Sleep(500);
                }
            }



            //设置ping和trace的task，并生成文件
            Task t1 = Task.Factory.StartNew(() =>
            {
                string command1 = "ping -n 100 " + textBox1.Text + ">" + temp_dir + "uniping.txt";
                int seconds = 1;
                this.Execute(command1, seconds);
            });


            Task t2 = Task.Factory.StartNew(() =>
            {
                string command2 = "ping -n 100 " + textBox2.Text + ">" + temp_dir + "telping.txt";
                int seconds = 1;
                this.Execute(command2, seconds);
            });

            Task t3 = Task.Factory.StartNew(() =>
            {
                string command3 = "tracert -d " + textBox1.Text + ">" + temp_dir + "unitrace.txt";
                int seconds = 1;
                this.Execute(command3, seconds);
            });

            Task t4 = Task.Factory.StartNew(() =>
            {
                string command4 = "tracert -d " + textBox2.Text + ">" + temp_dir + "teltrace.txt";
                int seconds = 1;
                this.Execute(command4, seconds);
            });

            Task t5 = Task.Factory.StartNew(() =>
            {
                string command5 = "ipconfig/all>" + temp_dir + "ipconfig.txt";
                int seconds = 1;
                this.Execute(command5, seconds);
            });

            string Uni_URL = textBox1.Text.Trim();
            string Tel_URL = textBox2.Text.Trim();
            string DNS = textBox4.Text.Trim();

            Task t6 = Task.Factory.StartNew(() =>
            {
                if (!IsIP(Uni_URL))
                {

                    string command6 = "nslookup " + Uni_URL + " " + DNS + ">" + temp_dir + "uninslookup.txt";
                    int seconds = 1;
                    this.Execute(command6, seconds);

                }
            });

            Task t7 = Task.Factory.StartNew(() =>
            {
                if (!IsIP(Tel_URL))
                {

                    string command7 = "nslookup " + Tel_URL + " " + DNS + ">" + temp_dir + "telnslookup.txt";
                    int seconds = 1;
                    this.Execute(command7, seconds);

                }
            });

            Task t8 = Task.Factory.StartNew(() =>
            {
                string command8 = "systeminfo>" + temp_dir + "sysinfo.txt";
                int seconds = 1;
                this.Execute(command8, seconds);
            });
            //wmic path win32_VideoController get /all /format:htable >> c:\VGA.html

            //端口探测
            Task t9 = Task.Factory.StartNew(() =>
            {
                FileRec("######端口探测######" + endl, temp_dir + "portinfo.log");
                if (!IsIP(textBox1.Text))
                {
                    string Uni_IP = getAddress(textBox1.Text.Trim());
                    port_info(Uni_IP);
                }
                else
                {
                    string Uni_IP = textBox1.Text.Trim();
                    port_info(Uni_IP);
                }

                if (!IsIP(textBox1.Text))
                {
                    string Tel_IP = getAddress(textBox2.Text.Trim());
                    port_info(Tel_IP);
                }
                else
                {
                    string Tel_IP = textBox2.Text.Trim();
                    port_info(Tel_IP);
                }
            });
            Thread.Sleep(500);


            //检测线程执行情况的线程
            Task logger = Task.Factory.StartNew(() =>
            {
                if (checkBox1.Checked)
                {
                    while (true)
                    {
                        if (t1.IsCompleted && t2.IsCompleted && t3.IsCompleted && t4.IsCompleted && t5.IsCompleted && t6.IsCompleted && t7.IsCompleted && t8.IsCompleted && t9.IsCompleted)
                        {
                            MessageBox.Show("Trace&Ping完成！", "提示！");
                            CombineFile(temp_dir + "portinfo.log");
                            File.Delete(temp_dir + "portinfo.log");
                            FileRec("######Unicom IP Ping&Trace######", filename);
                            CombineFile(temp_dir + "uniping.txt");
                            File.Delete(temp_dir + "uniping.txt");
                            CombineFile(temp_dir + "unitrace.txt");
                            File.Delete(temp_dir + "unitrace.txt");
                            FileRec("######Telecom IP Ping&Trace######", filename);
                            CombineFile(temp_dir + "telping.txt");
                            File.Delete(temp_dir + "telping.txt");
                            CombineFile(temp_dir + "teltrace.txt");
                            File.Delete(temp_dir + "teltrace.txt");
                            FileRec("######IP Configuration######", filename);
                            CombineFile(temp_dir + "ipconfig.txt");
                            File.Delete(temp_dir + "ipconfig.txt");
                            FileRec("######NSLOOKUP RESULT######" + endl, filename);
                            if (!IsIP(textBox1.Text))
                            {
                                CombineFile(temp_dir + "uninslookup.txt");
                                File.Delete(temp_dir + "uninslookup.txt");
                            }
                            if (!IsIP(textBox2.Text))
                            {
                                CombineFile(temp_dir + "telnslookup.txt");
                                File.Delete(temp_dir + "telnslookup.txt");
                            }
                            FileRec("######SYSTEMINFO######" + endl, filename);
                            CombineFile(temp_dir + "sysinfo.txt");
                            File.Delete(temp_dir + "sysinfo.txt");
                            MessageBox.Show("文件合并结束！", "提示！");
                            MessageBox.Show("日志已生成！", "提示！");
                            break;
                        }
                        Thread.Sleep(500);
                    }
                }
                else
                {
                    while (true)
                    {
                        if (t1.IsCompleted && t2.IsCompleted && t3.IsCompleted && t4.IsCompleted && t5.IsCompleted && t6.IsCompleted && t7.IsCompleted && t8.IsCompleted && t9.IsCompleted)
                        {
                            MessageBox.Show("Trace&Ping完成！", "提示！");
                            CombineFile(temp_dir + "portinfo.log");
                            File.Delete(temp_dir + "portinfo.log");
                            FileRec("######Unicom IP Ping&Trace######", filename);
                            CombineFile(temp_dir + "uniping.txt");
                            File.Delete(temp_dir + "uniping.txt");
                            CombineFile(temp_dir + "unitrace.txt");
                            File.Delete(temp_dir + "unitrace.txt");
                            FileRec("######Telecom IP Ping&Trace######", filename);
                            CombineFile(temp_dir + "telping.txt");
                            File.Delete(temp_dir + "telping.txt");
                            CombineFile(temp_dir + "teltrace.txt");
                            File.Delete(temp_dir + "teltrace.txt");
                            FileRec("######IP Configuration######", filename);
                            CombineFile(temp_dir + "ipconfig.txt");
                            File.Delete(temp_dir + "ipconfig.txt");
                            FileRec("######NSLOOKUP RESULT######" + endl, filename);
                            if (!IsIP(textBox1.Text))
                            {
                                CombineFile(temp_dir + "uninslookup.txt");
                                File.Delete(temp_dir + "uninslookup.txt");
                            }
                            if (!IsIP(textBox2.Text))
                            {
                                CombineFile(temp_dir + "telnslookup.txt");
                                File.Delete(temp_dir + "telnslookup.txt");
                            }
                            FileRec("######SYSTEMINFO######" + endl, filename);
                            CombineFile(temp_dir + "sysinfo.txt");
                            File.Delete(temp_dir + "sysinfo.txt");
                            MessageBox.Show("文件合并结束！", "提示！");
                            //设置timer和进度条
                            timer1.Enabled = false;
                            progressBar1.Value = progressBar1.Maximum;
                            //重置按钮文字
                            button1.Text = "开始检测";
                            MessageBox.Show("日志已生成！", "提示！");
                            //重置进度条
                            progressBar1.Value = 0;
                            //开启防火墙
                            string cmd_on = "netsh advfirewall set allprofiles state on";
                            Execute(cmd_on, 1);
                            break;
                        }
                        Thread.Sleep(500);
                    }
                }

            });


            //邮件发送task
            if (checkBox1.Checked)
            {
                Task mailer = Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        if (logger.IsCompleted)
                        {
                            string from = "xxx@xxx.com";
                            string fromer = "玩家";
                            string to = textBox5.Text.Trim();
                            string toer = "xxx";
                            string Subject = "玩家网络测试结果";
                            string file = "info.log";
                            string Body = "玩家日志，请查收！";
                            string SMTPHost = "smtp.xxx.com";
                            string SMTPuser = "xxx@xxx.com";
                            string SMTPpass = "xxxxxxx";
                            if (sendmail(from, fromer, to, toer, Subject, Body, file, SMTPHost, SMTPuser, SMTPpass))
                            {
                                //设置timer和进度条
                                timer1.Enabled = false;
                                progressBar1.Value = progressBar1.Maximum;
                                //重置按钮文字
                                button1.Text = "开始检测";
                                MessageBox.Show("邮件发送成功", "提示！");
                                //重置进度条
                                progressBar1.Value = 0;
                                //开启防火墙
                                string cmd_on = "netsh advfirewall set allprofiles state on";
                                Execute(cmd_on, 1);
                            }
                            else
                            {
                                MessageBox.Show("邮件发送错误，发送失败！", "错误！");
                            }
                            break;
                        }
                        Thread.Sleep(500);
                    }
                });
            }


            //progressBar1.Value = 0;
        }

        //联通域名
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        //电信域名
        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        //端口号
        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        //自定义DNS获取nslookup结果
        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        //checkbox和Email地址
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

            if (checkBox1.Checked)
            {
                textBox5.Enabled = true;
            }
            else
            {
                textBox5.Enabled = false;
            }
        }
        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        //自定义函数
        //获取域名地址
        private string getAddress(string url)
        {
            if (url.Trim() == "")
            {
                return "您输入的域名为空！";
            }
            IPHostEntry hostinfo = Dns.GetHostEntry(url);
            IPAddress[] aryIP = hostinfo.AddressList;
            string result = aryIP[0].ToString();
            return result;
        }


        //判断是否为IP地址
        private bool IsIP(string text)
        {
            System.Net.IPAddress ipAddress;
            if (System.Net.IPAddress.TryParse(text, out ipAddress))
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        //获取外网IP
        private string GetIP()
        {
            string tempip = "";
            try
            {
                WebRequest wr = WebRequest.Create("http://www.ip138.com/ips138.asp");
                Stream s = wr.GetResponse().GetResponseStream();
                StreamReader sr = new StreamReader(s, Encoding.Default);
                string all = sr.ReadToEnd(); //读取网站的数据
                int start = all.IndexOf("您的IP地址是：[") + 9;
                int end = all.IndexOf("]", start);
                tempip = all.Substring(start, end - start);
                sr.Close();
                s.Close();
            }
            catch
            {
            }
            return tempip;
        }


        //判断是否联网
        private string InternetCon()
        {
            String URL = "http://www.baidu.com"; //定义要获取http头的网址
            string statusCode;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(URL));
                req.Method = "HEAD";//设置请求方式为请求头，这样就不需要把整个网页下载下来 
                //req.Timeout = 2000; //这里设置超时时间，如果不设置，默认为10000 
                req.Timeout = 10000; //这里设置超时时间，如果不设置，默认为10000 
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                statusCode = res.StatusCode.ToString();
                return statusCode;
            }
            catch (WebException e)//使用try catch方式，如果正常，则返回OK，不正常就返回对应的错误。 
            {
                return e.Message;
            }
        }

        //timer
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (progressBar1.Value < progressBar1.Maximum)
            {
                progressBar1.Value++;
            }
        }


        //内容写入文件
        private void FileRec(string input, string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Append);
            StreamWriter sw = new StreamWriter(fs, Encoding.GetEncoding("GB2312"));
            //开始写入
            sw.Write(input);
            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();
        }


        //内容合并入文件
        private void CombineFile(string filename)
        {
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("GB2312"));
                string s = sr.ReadToEnd();
                FileRec(s, "info.log");
                sr.Close();
                fs.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("文件合并错误！", "错误！");
            }

        }



        //判断端口打开状态
        private void port_info(string address)
        {
            try
            {
                foreach (string port in textBox3.Text.Split(','))
                {
                    string msg1 = check_port(address, port) + endl;
                    FileRec(msg1, temp_dir + "portinfo.log");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("警告！域名内容或为空！", "警告！");
            }
        }




        //判断远程端口是否打开
        private string check_port(string ipaddr, string port)
        {
            IPAddress ip = IPAddress.Parse(ipaddr);
            IPEndPoint point = new IPEndPoint(ip, int.Parse(port));
            try
            {
                TcpClient tcp = new TcpClient();
                tcp.Connect(point);
                return ipaddr + "端口" + port + "打开";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        //执行系统命令的函数
        private string Execute(string command, int seconds)
        {
            string output = ""; //输出字符串  
            if (command != null && !command.Equals(""))
            {
                Process process = new Process();//创建进程对象  
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe";//设定需要执行的命令  
                startInfo.Arguments = "/C " + command;//“/C”表示执行完命令后马上退出  
                startInfo.UseShellExecute = false;//不使用系统外壳程序启动  
                startInfo.RedirectStandardInput = false;//不重定向输入  
                startInfo.RedirectStandardOutput = true; //重定向输出  
                startInfo.CreateNoWindow = true;//不创建窗口  
                process.StartInfo = startInfo;
                try
                {
                    if (process.Start())//开始进程  
                    {
                        if (seconds == 0)
                        {
                            process.WaitForExit();//这里无限等待进程结束  
                        }
                        else
                        {
                            process.WaitForExit(seconds); //等待进程结束，等待时间为指定的毫秒  
                        }
                        output = process.StandardOutput.ReadToEnd();//读取进程的输出  
                    }
                }
                catch
                {
                }
                finally
                {
                    if (process != null)
                        process.Close();
                }
            }
            return output;
        }


        //邮件发送函数
        public bool sendmail(string sfrom, string sfromer, string sto, string stoer, string sSubject, string sBody, string sfile, string sSMTPHost, string sSMTPuser, string sSMTPpass)
        {
            ////设置from和to地址
            MailAddress from = new MailAddress(sfrom, sfromer);
            MailAddress to = new MailAddress(sto, stoer);
            ////创建一个MailMessage对象
            MailMessage oMail = new MailMessage(from, to);
            //// 添加附件
            if (sfile != "")
            {
                oMail.Attachments.Add(new Attachment(sfile));
            }
            ////邮件标题
            oMail.Subject = sSubject;
            ////邮件内容
            oMail.Body = sBody;
            ////邮件格式
            oMail.IsBodyHtml = false;
            ////邮件采用的编码
            oMail.BodyEncoding = System.Text.Encoding.GetEncoding("GB2312");
            ////设置邮件的优先级为高
            oMail.Priority = MailPriority.High;
            ////发送邮件
            SmtpClient client = new SmtpClient();
            ////client.UseDefaultCredentials = false; 
            client.Host = sSMTPHost;
            client.Credentials = new NetworkCredential(sSMTPuser, sSMTPpass);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            try
            {
                client.Send(oMail);
                return true;
            }
            catch (Exception err)
            {
                return false;
            }
            finally
            {
                ////释放资源
                oMail.Dispose();
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(System.Environment.ExitCode);
        }
    }
}
