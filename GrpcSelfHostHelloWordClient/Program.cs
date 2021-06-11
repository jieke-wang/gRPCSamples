using System;
using System.Net;
using System.Net.Http;
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

            HttpClient.DefaultProxy = new WebProxy(); // 避免开启代理导致无法访问的情况
            // The port number(5000) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("http://localhost:5000", new GrpcChannelOptions
            {
                MaxReceiveMessageSize = int.MaxValue,
                MaxSendMessageSize = int.MaxValue,
            });

            //CallInvoker callInvoker = channel.CreateCallInvoker();
            //var client = new Greeter.GreeterClient(callInvoker);
            var client = new Greeter.GreeterClient(channel);

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

// 连接的时候,不要开代理,代理可能不支持http2协议
// c# - .NET 5 GRPC client call throws exception: Requesting HTTP version 2.0 with version policy RequestVersionOrHigher while HTTP/2 is not enabled https://stackoverflow.com/questions/66500195/net-5-grpc-client-call-throws-exception-requesting-http-version-2-0-with-versi