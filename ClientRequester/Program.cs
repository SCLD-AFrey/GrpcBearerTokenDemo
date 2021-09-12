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
            Console.WriteLine($"2: Get All Users         [POWERUSER]");
            Console.WriteLine($"3: Get Current User Info [ADMIN]");
            Console.WriteLine($"4: Return UTC Date       [PRIVATE_USER]");
            Console.WriteLine($"5: Get Current Timestamp [ALL AUTH USERS]");
            Console.WriteLine($"6: View Bearer Token");
            Console.WriteLine($"Q: Quit");
            Console.WriteLine("-------------------");

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
                        GetAllUsers(client);
                        break;
                    case ConsoleKey.D3:
                        GetUserInfo(client, _username);
                        break;
                    case ConsoleKey.D4:
                        GetUtcDate(client);
                        break;
                    case ConsoleKey.D5:
                        GetCurrentTimestamp(client);
                        break;
                    case ConsoleKey.D6:
                        Console.WriteLine($"Bearer Token: {_bearerToken}");
                        Console.WriteLine("-------------------");
                        break;
                    case ConsoleKey.Q:
                        Console.WriteLine($"...Shutdown");
                        break;
                    
                    default:
                        Console.WriteLine($"{key.ToString()} is not recognized");
                        break;
                }
                
                



            } while (!key.Equals(ConsoleKey.Q));
        }

        private async static void GetCurrentTimestamp(FunctionsService.FunctionsServiceClient p_client)
        {
            var response = p_client.ReturnCurrentTimestamp(new Empty(), _clientHeader);
            Console.WriteLine($"The current server time is  {response.ToDateTime().ToString("HH:mm:ss tt zz")}");
            Console.WriteLine("-------------------");
        }

        private async static void GetUtcDate(FunctionsService.FunctionsServiceClient p_client)
        {
            var response = await p_client.ReturnUtcDateAsync(new Empty(), _clientHeader);
            Console.WriteLine($"The current UTC Date is {DateTime.Parse(response.Content).ToString("mm/dd/yyyy")}");
            Console.WriteLine("-------------------");
        }

        private async static void GetAllUsers(FunctionsService.FunctionsServiceClient p_client)
        {            
            var response = await p_client.GetUserAllUsersAsync(new Empty(), _clientHeader);
            int cnt = 0;
            foreach (var user in response.Users)
            {
                cnt++;
                Console.Write($"{cnt}. {user.Username}");
                foreach (var role in user.Roles)
                    Console.Write($" -{role.Name}");
                Console.Write(Environment.NewLine);
            }
            Console.WriteLine("-------------------");
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
            Console.WriteLine("-------------------");
        }

        private async static void DoAuthentication(string p_username)
        {
            Console.WriteLine($"Authenticating as {p_username}...");
            _bearerToken = null;
            _bearerToken = await AuthenticateUser(_username);
            if (_bearerToken != null)
            {
                Console.WriteLine($"Successfully authenticated. ");
                Console.WriteLine($" -Token ({_bearerToken.Length} chars): {_bearerToken.Substring(0,25)}...");
            }
            else
            {
                Console.WriteLine("Authentication Failed.");
            }
            Console.WriteLine("-------------------");
        }

        private static async Task<string> AuthenticateUser(string p_username)
        {
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

            return token;
        }
    }
}
