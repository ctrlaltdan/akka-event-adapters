namespace Application.Persistence
{
    public class ProtobufContract
    {
        public string Type { get; set; }
        public byte[] Body { get; set; }

        public ProtobufContract(string type, byte[] body)
        {
            Type = type;
            Body = body;
        }
    }
}