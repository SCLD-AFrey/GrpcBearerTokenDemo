using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommonFiles;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace FunctionServer.Services
{
    public class SampleInterceptor : Interceptor
    {


        private void VerifyDevice(ServerCallContext context)
        {
        
            var deviceIdMetaEntry = context.RequestHeaders.FirstOrDefault(e => e.Key == "deviceid");
            var machineNameMetaEntry = context.RequestHeaders.FirstOrDefault(e => e.Key == "machinename");

            if (deviceIdMetaEntry == null || machineNameMetaEntry == null)
            {
                context.Status = new Status(StatusCode.Unauthenticated, $"Device Not Recognized. Incorrect Headers.");
            }
            
            var json = File.ReadAllText(Path.Combine(Constants.CommonPath, Constants.DeviceFile));
            var machines = JsonSerializer.Deserialize<List<Machine>>(json);
            context.Status = new Status(StatusCode.Unauthenticated, $"Device Not Recognized");
            if (machines != null)
            {
                foreach (var machine in machines)
                {
                    if (machine.MachineName == machineNameMetaEntry.Value &&
                        machine.DeviceId == deviceIdMetaEntry.Value)
                    {
                        context.Status = new Status(StatusCode.OK, $"Device Recognized");
                    }
                }
            }
            if (context.Status.StatusCode == StatusCode.Unauthenticated)
            {
                throw new RpcException(context.Status);
            }
        } 
        public SampleInterceptor() { }

        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            VerifyDevice(context);
            return base.UnaryServerHandler(request, context, continuation);
        }

        public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext context, ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            VerifyDevice(context);
            return base.ClientStreamingServerHandler(requestStream, context, continuation);
        }

        public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            VerifyDevice(context);
            return base.ServerStreamingServerHandler(request, responseStream, context, continuation);
        }

        public override Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            VerifyDevice(context);
            return base.DuplexStreamingServerHandler(requestStream, responseStream, context, continuation);
        }


    }
}