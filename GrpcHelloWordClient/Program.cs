using System;
using System.Threading;
using System.Threading.Tasks;

using Grpc.Net.Client;

namespace GrpcHelloWordClient
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
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

            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Greeter.GreeterClient(channel);

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                DateTime startTime = DateTime.Now;
                _ = await client.SayHelloAsync(
                                  new HelloRequest { Name = $"GreeterClient {startTime:yyyy-MM-dd HH:mm:ss.fffff}" });
                //var reply = await client.SayHelloAsync(
                //                  new HelloRequest { Name = $"GreeterClient {startTime:yyyy-MM-dd HH:mm:ss.fffff}" });
                // Console.WriteLine($"Greeting: {reply.Message}; 耗时: {(DateTime.Now - startTime).TotalMilliseconds}ms");
                // await Task.Delay(1000, cancellationTokenSource.Token);
            }
        }
    }
}

// C# 检测 Ctrl+c 和其他程序退出 https://blog.csdn.net/qq_16587307/article/details/107641473