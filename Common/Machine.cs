namespace CommonFiles
{
    public class Machine
    {
        public string DeviceId { get; set; }
        public string MachineName { get; set; } = System.Environment.MachineName;
    }
}