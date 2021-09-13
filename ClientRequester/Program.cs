using System;
using System.Net.Http;
using System.Threading.Tasks;
using CommonFiles;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using FunctionServerProto;

namespace ClientRequester
{
    static class Program
    {
        private static string _bearerToken;
        private static string _username;
        private static Metadata _clientHeader;
        
        public static async Task Main(string[] args)
        {
            ConsoleKey key;
            using var channel = GrpcChannel.ForAddress($"https://{Constants.Host}:{Constants.Port}");
            var client = new FunctionsService.FunctionsServiceClient(channel);
            

            string inst = "gRPC Bearer Token Demo" + Environment.NewLine
                                                   + Environment.NewLine
                                                   + "Press a key:"+ Environment.NewLine
                                                   + "1: Authenticate as..." + Environment.NewLine
                                                   + "2: Get All Users         [ADMIN]" + Environment.NewLine
                                                   + "3: Get Current User Info [ADMIN, POWERUSER, PRIVATE_USER]" + Environment.NewLine
                                                   + "4: Return UTC Date       [ADMIN, POWERUSER, PRIVATE_USER]" + Environment.NewLine
                                                   + "5: Get Current Timestamp [ALL AUTH USERS]" + Environment.NewLine
                                                   + "6: View Bearer Token     [LOCAL]" + Environment.NewLine
                                                   + "7: Clear Token" + Environment.NewLine
                                                   + "H: See Help" + Environment.NewLine
                                                   + "Q: Quit" + Environment.NewLine
                                                   + "-------------------" + Environment.NewLine;

            Console.Write(inst);

            do
            {
                key = Console.ReadKey(true).Key;
                
                if (_bearerToken != null)
                {
                    _clientHeader = new Metadata
                    {
                        {
                            "Authorization", $"Bearer {_bearerToken}"
                        }
                    };
                }
                else
                {
                    _clientHeader = new Metadata();
                }

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
                    case ConsoleKey.D7:
                        _bearerToken = null;
                        Console.WriteLine($"Token Cleared");
                        Console.WriteLine("-------------------");
                        break;
                    case ConsoleKey.H:
                        Console.Write(inst);
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

        private static async void GetCurrentTimestamp(FunctionsService.FunctionsServiceClient p_client)
        {
            try
            {
                var response = await p_client.ReturnCurrentTimestampAsync(new Empty(), _clientHeader);
                Console.WriteLine($"The current server time is  {response.ToDateTime():HH:mm:ss tt zz}");
            }
            catch (RpcException exception)
            {
                Console.WriteLine($"Request Failed: {exception.StatusCode}");
            }
            Console.WriteLine("-------------------");
        }

        private static async void GetUtcDate(FunctionsService.FunctionsServiceClient p_client)
        {
            try
            {
                var response = await p_client.ReturnUtcDateAsync(new Empty(), _clientHeader);
                Console.WriteLine($"The current UTC Date is {DateTime.Parse(response.Content):mm/dd/yyyy}");
            }
            catch (RpcException exception)
            {
                Console.WriteLine($"Request Failed: {exception.StatusCode}");
            }
            Console.WriteLine("-------------------");
        }

        private static async void GetAllUsers(FunctionsService.FunctionsServiceClient p_client)
        {
            try
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
            }
            catch (RpcException exception)
            {
                Console.WriteLine($"Request Failed: {exception.StatusCode}");
            }
            Console.WriteLine("-------------------");
        }

        private static async void GetUserInfo(FunctionsService.FunctionsServiceClient p_client, string p_username)
        {
            try
            {
                if (p_username == null)
                {
                    throw new Exception("User not authenticated");
                }

                var response = await p_client.GetUserInfoRpcAsync(new UserRequest()
                {
                    Username = p_username
                }, _clientHeader);

                Console.WriteLine($"Username: {response.Username}");
                foreach (var role in response.Roles)
                    Console.WriteLine($"Role(s): {role.Name}");
                Console.WriteLine($"Email Address: {response.Emailaddress}");
                Console.WriteLine($"Birthdate: {response.Dob}");
            }
            catch (RpcException exception)
            {
                Console.WriteLine($"Request Failed: {exception.StatusCode}");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Request Failed: {exception.Message}");
            }
            Console.WriteLine("-------------------");
        }

        private static async void DoAuthentication(string p_username)
        {
            
            using var channel = GrpcChannel.ForAddress($"https://{Constants.Host}:{Constants.Port}");
            var client = new FunctionsService.FunctionsServiceClient(channel);
            
            
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
            try
            {
                using var httpClient = new HttpClient();
                using var request = new HttpRequestMessage
                {
                    RequestUri = new Uri($"https://{Constants.Host}:{Constants.Port}/generateJwtToken?name={p_username}"),
                    Method = HttpMethod.Get,
                    Version = new Version(2, 0)
                };
                using var tokenResponse = await httpClient.SendAsync(request);
                tokenResponse.EnsureSuccessStatusCode();

                var token = await tokenResponse.Content.ReadAsStringAsync();

                return token;
            }
            catch (Exception exception)
            {
                return null;
            }

        }
    }
}
