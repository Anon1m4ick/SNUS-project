using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ProjectSNUS
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class Service1 : IService1
    {
        private readonly Dictionary<int, ISensorCallback> _sensors = new Dictionary<int, ISensorCallback>();
        private readonly object _lock = new object();

        public string GetData(string value)
        {
            return string.Format("You entered: {0}", value);
        }

        public void RegisterSensor(int sensorId)
        {
            lock (_lock)
            {
                var callback = OperationContext.Current.GetCallbackChannel<ISensorCallback>();
                if (!_sensors.ContainsKey(sensorId))
                {
                    _sensors[sensorId] = callback;
                    Console.WriteLine($"Sensor {sensorId} registerd. Total sensors: {_sensors.Count}");
                }
            }
        }

        public void UnregisterSensor(int sensorId)
        {
            lock (_lock)
            {
                if (_sensors.ContainsKey(sensorId))
                {
                    _sensors.Remove(sensorId);
                    Console.WriteLine($"Sensor {sensorId} unregistered. Total sensors: {_sensors.Count}");
                }
            }
        }

        public void BroadcastMessage(SensorMessage message)
        {
            lock (_lock)
            {
                Console.WriteLine($"Broadcasting message from Sensor {message.SenderId}: {message.Data}");
                Console.WriteLine($"Vector Clock: [{string.Join(", ", message.VectorClock)}]");

                var deadCallbacks = new List<int>();

                foreach (var sensor in _sensors)
                {
                    try
                    {
                        sensor.Value.ReceiveMessage(message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send message to Sensor {sensor.Key}: {ex.Message}");
                        deadCallbacks.Add(sensor.Key);
                    }
                }

                foreach (var id in deadCallbacks)
                {
                    _sensors.Remove(id);
                }
            }
        }
    }
}
