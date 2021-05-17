using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace Gauge_interface
{
    public partial class Form1 : Form
    {
        public static string[] netArray = { 
            "192.168.2.11:10004",
            "192.168.2.11:10003",
            "192.168.2.11:10001",
            "192.168.2.11:10002" };
        public static bool[] cArray = {false,true,false,false };
        public static string[] labelArray = { "Ion Trap", "Beam 1", "Beam 2", "REMPI" };

        public string net1 = netArray[0];
        public string net2 = netArray[1];
        public string net3 = netArray[2];
        public string net4 = netArray[3];
        public bool c1 = cArray[0];
        public bool c2 = cArray[1];
        public bool c3 = cArray[2];
        public bool c4 = cArray[3];
        public Socket tcp1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public Socket tcp2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public Socket tcp3 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public Socket tcp4 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public const int TcpBufferSize = 64;
        public byte[] TcpBuffer1 = new byte[TcpBufferSize];
        public byte[] TcpBuffer2 = new byte[TcpBufferSize];
        public byte[] TcpBuffer3 = new byte[TcpBufferSize];
        public byte[] TcpBuffer4 = new byte[TcpBufferSize];
        CancellationTokenSource source = new CancellationTokenSource();
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void networkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2(
                label1.Text, label2.Text, label3.Text, label4.Text,
                net1, net2, net3, net4, c1, c2, c3, c4);
            form2.ShowDialog(this);
        }
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3();
            form3.ShowDialog();
        }
        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = fontDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                textBox1.Font = fontDialog1.Font;
                textBox2.Font = fontDialog1.Font;
                textBox3.Font = fontDialog1.Font;
                textBox4.Font = fontDialog1.Font;
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            close();
        }
        private void disconnect(Socket tcp) {
            try
            {
                tcp.Close();
                tcp.Dispose();
            }
            catch { }
        }
        private string open(string net,Socket tcp)
        {
            try
            {
                string[] ad = net.Split(':');
                IPAddress ip = IPAddress.Parse(ad[0]);
                int port = Convert.ToInt32(ad[1]);
                IPEndPoint ipe = new IPEndPoint(ip, port);
                tcp.Connect(ipe);
                return "TCP OPENED";
            }
            catch (Exception e)
            {
                return "Set Error"+e.ToString();
            }
        }
        public void ini() {
            textBox1.Text = "Connecting...";
            textBox2.Text = "Connecting...";
            textBox3.Text = "Connecting...";
            textBox4.Text = "Connecting...";
            Task task = go();
        }
        public void close() {
            disconnect(tcp1); disconnect(tcp2); disconnect(tcp3); disconnect(tcp4);
        }

        private void open1() {
            textBox1.Text = open(net1, tcp1);
        }
        private void open2()
        {
            textBox2.Text = open(net2, tcp2);
        }
        private void open3()
        {
            textBox3.Text = open(net3, tcp3);
        }
        private void open4()
        {
            textBox4.Text = open(net4, tcp4);
        }
        private void rec1()
        {
            textBox1.Text = receve( tcp1, TcpBuffer1,c1);
        }
        private void rec2()
        {
            textBox2.Text = receve( tcp2, TcpBuffer1,c2);
        }
        private void rec3()
        {
            textBox3.Text = receve( tcp3, TcpBuffer1,c3);
        }
        private void rec4()
        {
            textBox4.Text = receve(tcp4, TcpBuffer4,c4);
        }
        public void canceltask() {
            
            source.Cancel(true);
        }
        private string receve( Socket tcp,byte[] tcpbuffer,bool con) {
            try
            {
                Array.Clear(tcpbuffer,0,tcpbuffer.Length);
                Byte[] ins = new Byte[64];
                if (con == true)
                {
                    ins= System.Text.Encoding.UTF8.GetBytes("?V913\r".ToCharArray());
                    tcp.Send(ins,ins.Length,0);
                }
                else
                {
                    ins = System.Text.Encoding.UTF8.GetBytes("RPV3\r".ToCharArray()); 
                    tcp.Send(ins, ins.Length, 0);
                }
                tcp.Receive(tcpbuffer,tcpbuffer.Length,0);
                string str = System.Text.Encoding.UTF8.GetString(tcpbuffer);
                try
                {
                    return scan(str, con);
                }
                catch (Exception e){
                    return "Char Error:" + e.ToString();
                }
                
            }
            catch (Exception e){
                return "Get Error:"+e.ToString();
            }
        }
        private async Task gonext() {
            await Task.Run(() => {
                Parallel.Invoke(rec1, rec2, rec3, rec4);
            });
        }
        public async Task go()
        {
            Thread.Sleep(1000);
            await Task.Run(() => {
                Parallel.Invoke(open1, open2, open3, open4);
                Thread.Sleep(2000);
                while (!source.IsCancellationRequested) {
                    Task task = gonext();
                    Thread.Sleep(1000);
                }
            });

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Text = labelArray[0];
            label2.Text = labelArray[1];
            label3.Text = labelArray[2];
            label4.Text = labelArray[3];
            ini();
        }

        private string scan(string value, bool con)
        {
                if (con == false)
                {
                    int a = int.Parse(value.Substring(0, 1));
                    if (a == 5)
                        return "OFF";
                    else if (a == 9)
                        return "NO SENSER";
                    else if (a == 7)
                        return "SENSER ERR";
                    else
                    {   
                        double b = Convert.ToDouble(value.Substring(3, 10));
                        return string.Format("{0:E2} Torr", b);
                    } 
                }
                else
                {
                    double b = Convert.ToDouble(value.Substring(6, 10));
                    b = b * 0.0075006;
                    return string.Format("{0:E2} Torr", b);
                }
            
        }
        private string reconnect(Socket tcp, string net)
        {
            tcp.Close();
            try
            {
                tcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                string[] ad = net.Split(':');
                IPAddress ip = IPAddress.Parse(ad[0]);
                int port = Convert.ToInt32(ad[1]);
                IPEndPoint ipe = new IPEndPoint(ip, port);
                tcp.Connect(ipe);
                return "Connnected";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
        public async Task asyreconnect()
        {
            await Task.Run(() =>
            {
                Parallel.Invoke(reo1, reo2, reo3, reo4);
                Thread.Sleep(2000);
                while (!source.IsCancellationRequested)
                {
                    Task task = gonext();
                    Thread.Sleep(1000);
                }
            });
        }
        private void reo1 (){
            textBox1.Text = reconnect(tcp1, net1);
        }
        private void reo2()
        {
            textBox2.Text = reconnect(tcp2, net2);
        }
        private void reo3()
        {
            textBox3.Text = reconnect(tcp3, net3);
        }
        private void reo4()
        {
            textBox4.Text = reconnect(tcp4, net4);
        }
    }
}
