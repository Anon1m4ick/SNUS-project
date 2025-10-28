using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSNUS
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class Service1 : IService1
    {
        private readonly Dictionary<int, ISensorCallback> _sensors = new Dictionary<int, ISensorCallback>();
        private readonly object _lock = new object();

        // Simulation settings
        private readonly Random _random = new Random();
        private readonly bool _simulateDelay = true; // set to false to disable delay simulation
        private readonly int _maxDelayMs =20000; // maximum random delay in milliseconds

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

                // Take a snapshot of current sensors to avoid modifying collection while iterating
                var sensorsSnapshot = _sensors.ToList();

                foreach (var sensor in sensorsSnapshot)
                {
                    var sensorId = sensor.Key;
                    var callback = sensor.Value;

                    // Deliver asynchronously with optional delay to simulate network
                    Task.Run(async () =>
                    {
                        int delay =0;
                        lock (_random)
                        {
                            if (_simulateDelay)
                            {
                                delay = _random.Next(_maxDelayMs +1);
                            }
                        }

                        if (delay >0)
                        {
                            Console.WriteLine($"Delaying delivery to Sensor {sensorId} by {delay} ms");
                            await Task.Delay(delay).ConfigureAwait(false);
                        }

                        try
                        {
                            callback.ReceiveMessage(message);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to send message to Sensor {sensorId}: {ex.Message}");
                            // Remove dead callback
                            lock (_lock)
                            {
                                if (_sensors.ContainsKey(sensorId))
                                {
                                    _sensors.Remove(sensorId);
                                }
                            }
                        }
                    });
                }
            }
        }
    }
}
