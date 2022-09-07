using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text.Json;
using System.Threading.Tasks;
using CommonFiles;
using DeviceId;
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
        private static string _deviceId;
        private static Metadata _clientHeader;
        private static FunctionsService.FunctionsServiceClient _client;
        
        public static async Task Main(string[] args)
        {
            _deviceId = CreateDeviceFile();
            ConsoleKey key;
            using var channel = GrpcChannel.ForAddress($"https://{Constants.Host}:{Constants.Ports.FunctionSecure}");
            _client = new FunctionsService.FunctionsServiceClient(channel);
            
            string inst = "gRPC Bearer Token Demo" + Environment.NewLine
                                                   + Environment.NewLine
                                                   + "Press a key:"+ Environment.NewLine
                                                   + "1: Authenticate as..." + Environment.NewLine
                                                   + "2: Get All Users         [ADMIN]" + Environment.NewLine
                                                   + "3: Get Current User Info [ADMIN, POWERUSER, PRIVATE_USER]" + Environment.NewLine
                                                   + "4: Return UTC Date       [ADMIN, POWERUSER, PRIVATE_USER]" + Environment.NewLine
                                                   + "5: Get Current Timestamp [ALL AUTH USERS]" + Environment.NewLine + Environment.NewLine
                                                   + "6: View Bearer Token" + Environment.NewLine
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
                        {"Authorization", $"Bearer {_bearerToken}"},
                        {"DeviceId", _deviceId},
                        {"MachineName", System.Environment.MachineName}
                    };
                }
                else
                {
                    _clientHeader = new Metadata
                    {
                        {"DeviceId", _deviceId},
                        {"MachineName", System.Environment.MachineName}
                    };
                }

                try
                {

                    switch (key)
                    {
                        case ConsoleKey.D1:
                            Console.Write("Enter username: ");
                            var inUsername = Console.ReadLine();
                            Console.Write("Enter password: ");
                            var inPassword = Console.ReadLine();

                            if (string.IsNullOrEmpty(inUsername) || string.IsNullOrEmpty(inPassword))
                            {
                                throw new Exception("Username and Password Required");
                            }

                            DoLogin(inUsername, inPassword);
                            DoAuthentication(inUsername);
                            break;
                        case ConsoleKey.D2:
                            GetAllUsers();
                            break;
                        case ConsoleKey.D3:
                            GetUserInfo(_username);
                            break;
                        case ConsoleKey.D4:
                            GetUtcDate();
                            break;
                        case ConsoleKey.D5:
                            GetCurrentTimestamp();
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
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ERROR: {e.Message}");
                }

            } while (!key.Equals(ConsoleKey.Q));
        }


        private static string CreateDeviceFile()
        {
            var deviceId = new DeviceIdBuilder().AddMachineName().AddOsVersion().ToString();
            var machineFile = Path.Combine(Path.GetTempPath(), Constants.DeviceFile);
            if (!File.Exists(machineFile))
            {
                List<Machine> machines = new List<Machine>();
                machines.Add(new Machine() {DeviceId = deviceId});
                machines.Add(new Machine() {DeviceId = "test1"});
                machines.Add(new Machine() {DeviceId = "test2"});
                machines.Add(new Machine() {DeviceId = "test3"});

                string json = JsonSerializer.Serialize(machines);
                File.WriteAllText(machineFile, json);
            }

            return deviceId;
        }
        
        private static async void GetCurrentTimestamp()
        {
            try
            {
                var response = await _client.ReturnCurrentTimestampAsync(new Empty(), _clientHeader);
                Console.WriteLine($"The current server time is  {response.ToDateTime():HH:mm:ss tt zz}");
            }
            catch (RpcException exception)
            {
                Console.WriteLine($"Request Failed: {exception.StatusCode}");
            }
            Console.WriteLine("-------------------");
        }

        private static async void GetUtcDate()
        {
            try
            {
                var response = await _client.ReturnUtcDateAsync(new Empty(), _clientHeader);
                Console.WriteLine($"The current UTC Date is {DateTime.Parse(response.Content):mm/dd/yyyy}");
            }
            catch (RpcException exception)
            {
                Console.WriteLine($"Request Failed: {exception.StatusCode}");
            }
            Console.WriteLine("-------------------");
        }

        private static async void GetAllUsers()
        {
            try
            {
                var response = await _client.GetUserAllUsersAsync(new Empty(), _clientHeader);
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

        private static async void GetUserInfo(string p_username)
        {
            try
            {
                if (p_username == null)
                {
                    throw new Exception("User not authenticated");
                }

                var response = await _client.GetUserInfoRpcAsync(new UserRequest()
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

        private static void DoLogin(string? p_inUsername, string? p_inPassword)
        {
            var user = UserRepo.Users().FirstOrDefault(o => o.UserName == p_inUsername);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            if (UserRepo.HashPassword(user.Password) == UserRepo.HashPassword(p_inPassword))
            {
                throw new Exception("Incorrect Password.");     
            }
        }
        private static async void DoAuthentication(string p_username)
        {
            
            using var channel = GrpcChannel.ForAddress($"https://{Constants.Host}:{Constants.Ports.FunctionSecure}");
            var client = new FunctionsService.FunctionsServiceClient(channel);
            
            
            Console.WriteLine($"Authenticating as {p_username}...");
            _bearerToken = null;
            _bearerToken = await AuthenticateUser(p_username);
            
            
            if (_bearerToken != null)
            {
                _username = p_username;
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
                    RequestUri = new Uri($"https://{Constants.Host}:{Constants.Ports.FunctionSecure}/generateJwtToken?name={p_username}"),
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
