using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Grpc.Core;
using Grpc.Net.Client;

namespace SelfHostStreamClient
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
            var client = new StreamSample.StreamSampleClient(channel);

            //await Normal(cancellationTokenSource, client);
            //await ServerSide(cancellationTokenSource, client);
            //await ClientSide(cancellationTokenSource, client);
            //await Bidirectional(cancellationTokenSource, client);

            await Task.WhenAll(Normal(cancellationTokenSource, client), ServerSide(cancellationTokenSource, client), ClientSide(cancellationTokenSource, client), Bidirectional(cancellationTokenSource, client));
        }

        private static async Task Bidirectional(CancellationTokenSource cancellationTokenSource, StreamSample.StreamSampleClient client)
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                DateTime startTime = DateTime.Now;
                using var call = client.Bidirectional();

                var responseReaderTask = Task.Run(async () =>
                {
                    // 逐一取出 response 內容
                    while (await call.ResponseStream.MoveNext(cancellationTokenSource.Token))
                    {
                        var reply = call.ResponseStream.Current;
                        Console.WriteLine(reply.Message);
                    }
                });

                for (int msgSerialNumber = 0; msgSerialNumber < 100; msgSerialNumber++)
                {
                    // 将请求逐一发给服务端
                    await call.RequestStream.WriteAsync(new StreamRequest { Name = $"Jack[{msgSerialNumber}] {DateTime.Now}" });
                    //await Task.Delay(1000, cancellationTokenSource.Token);
                }

                await call.RequestStream.CompleteAsync();
                await responseReaderTask;

                Console.WriteLine($"ClientSide: 耗时: {(DateTime.Now - startTime).TotalMilliseconds}ms");
                await Task.Delay(1000, cancellationTokenSource.Token);
            }
        }

        private static async Task ClientSide(CancellationTokenSource cancellationTokenSource, StreamSample.StreamSampleClient client)
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                DateTime startTime = DateTime.Now;
                using var call = client.ClientSide();
                for (int msgSerialNumber = 0; msgSerialNumber < 100; msgSerialNumber++)
                {
                    // 将请求逐一发给服务端
                    await call.RequestStream.WriteAsync(new StreamRequest { Name = $"Jack[{msgSerialNumber}]" });
                    //await Task.Delay(1000, cancellationTokenSource.Token);
                }

                await call.RequestStream.CompleteAsync();
                var reply = await call.ResponseAsync;
                Console.WriteLine(reply.Message);

                Console.WriteLine($"ClientSide: 耗时: {(DateTime.Now - startTime).TotalMilliseconds}ms");
                await Task.Delay(1000, cancellationTokenSource.Token);
            }
        }

        private static async Task ServerSide(CancellationTokenSource cancellationTokenSource, StreamSample.StreamSampleClient client)
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                DateTime startTime = DateTime.Now;
                using var call = client.ServerSide(new StreamRequest { Name = $"Jack[{DateTime.Now}]" });
                // 逐一取出 response 內容
                while (await call.ResponseStream.MoveNext(cancellationTokenSource.Token))
                {
                    var reply = call.ResponseStream.Current;
                    Console.WriteLine(reply.Message);
                }

                Console.WriteLine($"ServerSide: 耗时: {(DateTime.Now - startTime).TotalMilliseconds}ms");
                await Task.Delay(1000, cancellationTokenSource.Token);
            }
        }

        private static async Task Normal(CancellationTokenSource cancellationTokenSource, StreamSample.StreamSampleClient client)
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                DateTime startTime = DateTime.Now;
                //_ = await client.SayHelloAsync(
                //                  new HelloRequest { Name = $"GreeterClient {startTime:yyyy-MM-dd HH:mm:ss.fffff}" });
                var reply = await client.NormalAsync(
                                  new StreamRequest { Name = $"GreeterClient {startTime:yyyy-MM-dd HH:mm:ss.fffff}" }, cancellationToken: cancellationTokenSource.Token);
                Console.WriteLine($"Normal: {reply.Message}; 耗时: {(DateTime.Now - startTime).TotalMilliseconds}ms");
                await Task.Delay(1000, cancellationTokenSource.Token);
            }
        }
    }
}
