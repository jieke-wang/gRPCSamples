using System;
using System.Threading;
using System.Threading.Tasks;

using Grpc.Core;
using Grpc.Net.Client;

namespace GrpcSelfHostHelloWordClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                cancellationTokenSource.Cancel();
                Thread.Sleep(3000);
            };
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
            };

            // The port number(5000) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("http://localhost:5000", new GrpcChannelOptions
            {
                MaxReceiveMessageSize = int.MaxValue,
                MaxSendMessageSize = int.MaxValue,
            });
            CallInvoker callInvoker = channel.CreateCallInvoker();
            //var client = new Greeter.GreeterClient(channel);
            var client = new Greeter.GreeterClient(callInvoker);
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                DateTime startTime = DateTime.Now;
                //_ = await client.SayHelloAsync(
                //                  new HelloRequest { Name = $"GreeterClient {startTime:yyyy-MM-dd HH:mm:ss.fffff}" });
                var reply = await client.SayHelloAsync(
                                  new HelloRequest { Name = $"GreeterClient {startTime:yyyy-MM-dd HH:mm:ss.fffff}" });
                Console.WriteLine($"Greeting: {reply.Message}; 耗时: {(DateTime.Now - startTime).TotalMilliseconds}ms");
                //await Task.Delay(1000, cancellationTokenSource.Token);
            }
        }
    }
}
