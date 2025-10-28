using Client3.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Client3
{
    public class CausalMiddleware
    {
        private readonly int _sensorId;
        private readonly int _totalSensors;
        private int[] _vectorClock;
        private readonly List<SensorMessage> _messageBuffer;
        private readonly object _lock = new object();

        public event Action<SensorMessage> OnMessageDelivered;

        public CausalMiddleware(int sensorId, int totalSensors = 4)
        {
            _sensorId = sensorId;
            _totalSensors = totalSensors;
            _vectorClock = new int[totalSensors];
            _messageBuffer = new List<SensorMessage>();
        }

        public int[] GetVectorClock()
        {
            lock (_lock)
            {
                return (int[])_vectorClock.Clone();
            }
        }

        public int GetBufferSize()
        {
            lock (_lock)
            {
                return _messageBuffer.Count;
            }
        }

        public SensorMessage CreateMessage(string data)
        {
            lock (_lock)
            {
                _vectorClock[_sensorId]++;

                var message = new SensorMessage
                {
                    SenderId = _sensorId,
                    Data = data,
                    Timestamp = DateTime.Now,
                    SequenceNumber = _vectorClock[_sensorId],
                    VectorClock = (int[])_vectorClock.Clone()
                };

                return message;
            }
        }

        public void ReceiveMessage(SensorMessage message)
        {
            lock (_lock)
            {
                _messageBuffer.Add(message);

                TryDeliverMessages();
            }
        }

        private void TryDeliverMessages()
        {
            bool delivered;
            do
            {
                delivered = false;
                SensorMessage messageToDeliver = null;

                foreach (var message in _messageBuffer)
                {
                    if (CanDeliver(message))
                    {
                        messageToDeliver = message;
                        delivered = true;
                        break;
                    }
                }

                if (messageToDeliver != null)
                {
                    _messageBuffer.Remove(messageToDeliver);

                    UpdateVectorClock(messageToDeliver);

                    OnMessageDelivered?.Invoke(messageToDeliver);
                }
            } while (delivered);
        }

        private bool CanDeliver(SensorMessage message)
        {
            int senderId = message.SenderId;

            if (message.VectorClock[senderId] != _vectorClock[senderId] + 1)
            {
                return false;
            }

            for (int i = 0; i < _totalSensors; i++)
            {
                if (i != senderId)
                {
                    if (message.VectorClock[i] > _vectorClock[i])
                    {
                        return false; 
                    }
                }
            }

            return true;
        }

        private void UpdateVectorClock(SensorMessage message)
        {
            for (int i = 0; i < _totalSensors; i++)
            {
                _vectorClock[i] = Math.Max(_vectorClock[i], message.VectorClock[i]);
            }
        }

        public List<SensorMessage> GetBufferedMessages()
        {
            lock (_lock)
            {
                return new List<SensorMessage>(_messageBuffer);
            }
        }

        public string GetVectorClockString()
        {
            lock (_lock)
            {
                return $"[{string.Join(", ", _vectorClock)}]";
            }
        }
    }
}
