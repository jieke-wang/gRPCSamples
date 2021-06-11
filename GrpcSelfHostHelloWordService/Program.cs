using System;
using System.Threading;
using System.Threading.Tasks;

using Grpc.Core;

using GrpcSelfHostHelloWordService.Impls;

namespace GrpcSelfHostHelloWordService
{
    class Program
    {
        const int Port = 5000;

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

            Server server = new Server(new[] 
            { 
                new ChannelOption("MaxReceiveMessageSize", int.MaxValue),
                new ChannelOption("MaxSendMessageSize", int.MaxValue),
            })
            {
                Services = { Greeter.BindService(new GreeterImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Greeter server listening on port " + Port);
            Console.WriteLine("Press Ctrl + c key to stop the server...");

            try
            {
                await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                await server.ShutdownAsync();
            }
        }
    }
}
