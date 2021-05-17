using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gauge_interface
{
    public partial class Form2 : Form
    {
        public Form2(string label1, string label2, string label3, string label4, 
            string net1, string net2, string net3, string net4,
            bool c1,bool c2,bool c3,bool c4)
        {
            InitializeComponent();
            textBox1.Text = label1;
            textBox2.Text = label2;
            textBox3.Text = label3;
            textBox4.Text = label4;
            textBox8.Text = net1;
            textBox7.Text = net2;
            textBox6.Text = net3;
            textBox5.Text = net4;
            checkBox1.Checked = c1;
            checkBox2.Checked = c2;
            checkBox3.Checked = c3;
            checkBox4.Checked = c4;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 form1 = (Form1)this.Owner;
            form1.canceltask();
            form1.Controls["label1"].Text = textBox1.Text;
            form1.Controls["label2"].Text = textBox2.Text;
            form1.Controls["label3"].Text = textBox3.Text;
            form1.Controls["label4"].Text = textBox4.Text;
            form1.net1 = textBox8.Text;
            form1.net2 = textBox7.Text;
            form1.net3 = textBox6.Text;
            form1.net4 = textBox5.Text;
            form1.c1 = checkBox1.Checked;
            form1.c2 = checkBox2.Checked;
            form1.c3 = checkBox3.Checked;
            form1.c4 = checkBox4.Checked;
            Task task = form1.asyreconnect();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = Form1.labelArray[0];
            textBox2.Text = Form1.labelArray[1];
            textBox3.Text = Form1.labelArray[2];
            textBox4.Text = Form1.labelArray[3];
            textBox5.Text = Form1.netArray[3];
            textBox6.Text = Form1.netArray[2];
            textBox7.Text = Form1.netArray[1];
            textBox8.Text = Form1.netArray[0];
            checkBox1.Checked = Form1.cArray[0];
            checkBox2.Checked = Form1.cArray[1];
            checkBox3.Checked = Form1.cArray[2];
            checkBox4.Checked = Form1.cArray[3];
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
