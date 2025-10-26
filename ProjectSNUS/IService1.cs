using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ProjectSNUS
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract(CallbackContract = typeof(ISensorCallback))]
    public interface IService1
    {

        [OperationContract]
        void RegisterSensor(int sensorId);

        [OperationContract]
        void UnregisterSensor(int sensorId);

        [OperationContract]
        void BroadcastMessage(SensorMessage message);

        [OperationContract]
        string GetData(string value);
    }

    [ServiceContract]
    public interface ISensorCallback
    {
        [OperationContract(IsOneWay = true)]
        void ReceiveMessage(SensorMessage message);
    }

    [DataContract]
    public class SensorMessage
    {
        [DataMember]
        public int SenderId { get; set; }

        [DataMember]
        public string Data { get; set; }

        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public int SequenceNumber { get; set; }

        [DataMember]

        public int[] VectorClock { get; set; }
        
        public SensorMessage()
        {
            Timestamp = DateTime.Now;
            VectorClock = new int[4];
        }

    }
}
