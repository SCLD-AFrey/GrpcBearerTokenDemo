namespace CommonFiles
{
    public static class Constants
    {
        public const string Host = "localhost";
        public const string DeviceFile = "machine.json";

        public class Ports
        {
            public const int FunctionInsecure = 50051;
            public const int FunctionSecure = 50052;
            
            public const int WinAgentInsecure = 30051;
            public const int WinAgentSecure = 30052;
        }
    }
}