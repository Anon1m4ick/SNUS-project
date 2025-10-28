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
        private CausalMiddleware _middleware;
     
        public Form1() : this(0) // Default constructor for designer
     {
      }

   public Form1(int sensorId)
    {
          InitializeComponent();
         _sensorId = sensorId;
      InitializeSensor();
        }

        private void InitializeSensor()
        {
            // Initialize middleware
            _middleware = new CausalMiddleware(_sensorId, totalSensors: 4);
            _middleware.OnMessageDelivered += OnMessageDelivered;

            var context = new InstanceContext(this);
            _client = new ServiceReference1.Service1Client(context, "WSDualHttpBinding_IService1");

            _client.RegisterSensor(_sensorId);
  
            // Update window title
            this.Text = $"Sensor {_sensorId} - Causal Broadcast";

            AppendLog($"=== Sensor {_sensorId} initialized ===");
            AppendLog($"Vector Clock: {_middleware.GetVectorClockString()}");
            AppendLog("---");
        }

        public void ReceiveMessage(SensorMessage message)
        {
            // Skip own messages
            if (message.SenderId == _sensorId)
                return;

            // Pass to middleware for causal ordering
            _middleware.ReceiveMessage(message);
    
            // Update buffer size display
            UpdateBufferInfo();
        }

        private void OnMessageDelivered(SensorMessage message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<SensorMessage>(OnMessageDelivered), message);
                return;
            }

            AppendLog($"[DELIVERED from Sensor {message.SenderId}] {message.Data}");
            AppendLog($"Message VC: [{string.Join(", ", message.VectorClock)}]");
            AppendLog($"Local VC: {_middleware.GetVectorClockString()}");
            AppendLog($"Buffer size: {_middleware.GetBufferSize()}");
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
            try
            {
                // Create message through middleware (updates vector clock)
                var message = _middleware.CreateMessage(data);

                AppendLog($"[SENDING] {data}");
                AppendLog($"Vector Clock: {_middleware.GetVectorClockString()}");
                AppendLog("---");

                // Broadcast to all sensors via WCF (async to avoid UI blocking)
                _ = _client.BroadcastMessageAsync(message);
            }
            catch (Exception ex)
            {
                AppendLog($"ERROR: {ex.Message}");
            }
        }

        private void UpdateBufferInfo()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateBufferInfo));
                return;
            }

            // Could update a label showing buffer size if needed
            // For now, it's displayed in the log
        }

        private void AppendLog(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AppendLog), text);
                return;
            }

            // Keep a reasonable maximum size
            if (label1.TextLength > 100000)
            {
                label1.Clear();
            }

            // Append and scroll to end
            label1.AppendText(text + Environment.NewLine);
            label1.SelectionStart = label1.TextLength;
            label1.ScrollToCaret();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
   
            try
            {
                if (_client != null && _client.State == CommunicationState.Opened)
                {
                    _client.UnregisterSensor(_sensorId);
                    _client.Close();
                }
            }
            catch
            {
                // Ignore errors on close
            }
        }
    }
}
