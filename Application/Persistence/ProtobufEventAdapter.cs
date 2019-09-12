using Akka.Persistence.Journal;

namespace Application.Persistence
{
    public class ProtobufEventAdapter : IEventAdapter
    {
        public string Manifest(object evt) 
            => ProtobufSerialization.ProtoContractType;
        
        public object ToJournal(object evt) 
            => ProtobufSerialization.ToProtobufContract(evt);

        public IEventSequence FromJournal(object evt, string manifest) 
            => EventSequence.Single(ProtobufSerialization.FromProtobufContract(evt));
    }
}