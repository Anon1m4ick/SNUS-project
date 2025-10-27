using Client1.ServiceReference1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client1
{
    public partial class Form1 : Form, ServiceReference1.IService1Callback
    {

        private Service1Client _client;
        private int _sensorId;
        private Timer _sensorTimer;
        
        public Form1()
        {
            InitializeComponent();
            _sensorId = 0; // Change this for each client instance
            InitializeSensor();
        }

        private void InitializeSensor()
        {
            var context = new InstanceContext(this);
            _client = new ServiceReference1.Service1Client(context, "WSDualHttpBinding_IService1");

            _client.RegisterSensor(_sensorId);
        }

        public void ReceiveMessage(SensorMessage message)
        {
            if (message.SenderId == _sensorId)
                return; 

        }

        private void OnMessageDelivered(SensorMessage message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<SensorMessage>(OnMessageDelivered), message);
                return;
            }

            AppendLog($"[RECEIVED from Sensor {message.SenderId}] {message.Data}");
            AppendLog($"Vector Clock: [{string.Join(", ", message.VectorClock)}]");
            //AppendLog($"Buffer size: {}");
            AppendLog("---");

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                SendSensorReading(textBox1.Text);
                textBox1.Clear();
            }
        }

        private void SendSensorReading(string data)
        {

        }

        private void AppendLog(string text)
        {
            if (label1.Text.Length > 5000)
                label1.Text = "";

            label1.Text += text + Environment.NewLine;
        }
    }
}
