using Client1.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Client1
{
    /// <summary>
    /// Middleware for implementing Causal + FIFO Broadcast
/// Ensures messages are delivered in causal order using vector clocks
    /// </summary>
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

        /// <summary>
        /// Get current vector clock state
        /// </summary>
        public int[] GetVectorClock()
        {
     lock (_lock)
            {
  return (int[])_vectorClock.Clone();
}
   }

        /// <summary>
  /// Get current buffer size
        /// </summary>
        public int GetBufferSize()
        {
            lock (_lock)
            {
         return _messageBuffer.Count;
            }
      }

   /// <summary>
  /// Create a new message with updated vector clock
        /// </summary>
        public SensorMessage CreateMessage(string data)
   {
    lock (_lock)
            {
                // Increment own vector clock
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

     /// <summary>
        /// Receive a message and attempt to deliver it
  /// If causality is violated, buffer the message
        /// </summary>
     public void ReceiveMessage(SensorMessage message)
        {
      lock (_lock)
            {
                // Add to buffer
           _messageBuffer.Add(message);

     // Try to deliver messages from buffer
    TryDeliverMessages();
            }
      }

        /// <summary>
     /// Try to deliver messages from buffer that satisfy causal ordering
        /// </summary>
        private void TryDeliverMessages()
        {
  bool delivered;
            do
         {
  delivered = false;
        SensorMessage messageToDeliver = null;

 // Find a message that can be delivered
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
         // Remove from buffer
         _messageBuffer.Remove(messageToDeliver);

      // Update vector clock
   UpdateVectorClock(messageToDeliver);

          // Deliver to application
OnMessageDelivered?.Invoke(messageToDeliver);
           }
            } while (delivered);
        }

        /// <summary>
        /// Check if message can be delivered according to causal ordering
        /// Causal ordering rule:
        /// 1. VC[sender] at message = local VC[sender] + 1 (FIFO from sender)
    /// 2. For all k != sender: VC[k] at message <= local VC[k] (causality)
    /// </summary>
        private bool CanDeliver(SensorMessage message)
        {
       int senderId = message.SenderId;

            // Rule 1: FIFO from sender - next expected message from this sender
   if (message.VectorClock[senderId] != _vectorClock[senderId] + 1)
       {
         return false;
            }

// Rule 2: Causal dependency - no messages from other senders are missing
   for (int i = 0; i < _totalSensors; i++)
    {
        if (i != senderId)
     {
          if (message.VectorClock[i] > _vectorClock[i])
      {
            return false; // Missing messages from sensor i
      }
    }
         }

      return true;
 }

     /// <summary>
        /// Update local vector clock after delivering a message
        /// </summary>
        private void UpdateVectorClock(SensorMessage message)
     {
          // Take element-wise maximum
            for (int i = 0; i < _totalSensors; i++)
            {
        _vectorClock[i] = Math.Max(_vectorClock[i], message.VectorClock[i]);
            }
        }

        /// <summary>
        /// Get all buffered messages (for debugging/visualization)
        /// </summary>
        public List<SensorMessage> GetBufferedMessages()
        {
   lock (_lock)
            {
       return new List<SensorMessage>(_messageBuffer);
            }
        }

        /// <summary>
        /// Get vector clock as formatted string
        /// </summary>
        public string GetVectorClockString()
        {
      lock (_lock)
   {
          return $"[{string.Join(", ", _vectorClock)}]";
       }
        }
  }
}
