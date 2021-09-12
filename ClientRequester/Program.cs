using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using FunctionServerProto;
using CommonFiles;

namespace ClientRequester
{
    class Program
    {
        private static string _bearerToken;
        private static string _username;
        private const string Address = "https://localhost:5001";
        private static Metadata _clientHeader;
        
        public static async Task Main(string[] args)
        {
            ConsoleKey key;
            using var channel = GrpcChannel.ForAddress(Address);
            var client = new FunctionsService.FunctionsServiceClient(channel);

            Console.WriteLine("gRPC Bearer Token Demo");
            Console.WriteLine();
            Console.WriteLine("Press a key:");
            Console.WriteLine($"1: Authenticate as...");
            Console.WriteLine($"2: Get Current User Info [ADMIN]");
            Console.WriteLine($"3: Get All Users         [POWERUSER]");
            Console.WriteLine($"4: Return UTC Date       [PRIVATE_USER]");
            Console.WriteLine($"Q: Quit");

            do
            {
                key = Console.ReadKey(true).Key;
                
                if (_bearerToken != null)
                {
                    _clientHeader = new Metadata();
                    _clientHeader.Add("Authorization", $"Bearer {_bearerToken}");
                }

                string input;
                switch (key)
                {
                    case ConsoleKey.D1:
                        Console.Write("Enter username: ");
                        _username = Console.ReadLine();
                        if (_username.Length == 0) _username = Environment.UserName;
                        DoAuthentication(_username);
                        break;
                    case ConsoleKey.D2:
                        GetUserInfo(client, _username);
                        break;
                    case ConsoleKey.D3:
                        GetAllUsers(client);
                        break;
                    case ConsoleKey.D4:

                        GetUtcDate(client);

                        
                        break;
                    case ConsoleKey.Q:
                        Console.WriteLine($"...Shutdown");
                        break;
                    
                    default:
                        Console.WriteLine($"{key.ToString()} is not recognized");
                        break;
                }
                
                

                Console.WriteLine("-------------------");


            } while (!key.Equals(ConsoleKey.Q));
        }

        private async static void GetUtcDate(FunctionsService.FunctionsServiceClient p_client)
        {
            var response = await p_client.ReturnUtcDateAsync(new Empty(), _clientHeader);
            Console.WriteLine($"The current UTC Date is {DateTime.Parse(response.Content).ToString("mm/dd/yyyy")}");
        }

        private async static void GetAllUsers(FunctionsService.FunctionsServiceClient p_client)
        {            
            var response = await p_client.GetUserAllUsersAsync(new Empty(), _clientHeader);
            int cnt = 0;
            foreach (var user in response.Users)
            {
                cnt++;
                Console.WriteLine($"{cnt}. {user.Username}");
            }
        }

        private async static void GetUserInfo(FunctionsService.FunctionsServiceClient p_client, string p_username)
        {
            var response = await p_client.GetUserInfoRpcAsync(new UserRequest()
            {
                Username = _username
            }, _clientHeader);
                        
            Console.WriteLine($"Username: {response.Username}");
            foreach (var role in response.Roles)
                Console.WriteLine($"Role(s): {role.Name}");
            Console.WriteLine($"Email Address: {response.Dob}");
            Console.WriteLine($"Birthdate: {response.Dob}");
        }

        private async static void DoAuthentication(string p_username)
        {
            _bearerToken = await AuthenticateUser(_username);
        }

        private static async Task<string> AuthenticateUser(string p_username)
        {
            Console.WriteLine($"Authenticating as {p_username}...");
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{Address}/generateJwtToken?name={p_username}"),
                Method = HttpMethod.Get,
                Version = new Version(2, 0)
            };
            using var tokenResponse = await httpClient.SendAsync(request);
            tokenResponse.EnsureSuccessStatusCode();

            var token = await tokenResponse.Content.ReadAsStringAsync();
            Console.WriteLine("Successfully authenticated.");

            return token;
        }
    }
}
