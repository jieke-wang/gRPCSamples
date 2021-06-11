using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grpc.Core;

namespace SelfHostStreamService.Impls
{
    public class StreamSampleImpl : StreamSample.StreamSampleBase
    {
        public override Task<StreamReply> Normal(StreamRequest request, ServerCallContext context)
        {
            return Task.FromResult(new StreamReply { Message = $"Normal {request.Name}, {DateTime.Now}" });
        }

        public override async Task ServerSide(StreamRequest request, IServerStreamWriter<StreamReply> responseStream, ServerCallContext context)
        {
            //while (context.CancellationToken.IsCancellationRequested == false)
            {
                long messageCounter = 0;
                Console.WriteLine(request.Name);
                for (int i = 0; i < 100; i++)
                {
                    await responseStream.WriteAsync(new StreamReply { Message = $"ServerSide[{messageCounter++}] {request.Name}, {DateTime.Now}" });
                }
                //await Task.Delay(1000, context.CancellationToken);
            }
        }

        public override async Task<StreamReply> ClientSide(IAsyncStreamReader<StreamRequest> requestStream, ServerCallContext context)
        {
            long messageCounter = 0;
            while (await requestStream.MoveNext(context.CancellationToken))
            {
                StreamRequest request = requestStream.Current;
                Console.WriteLine($"ClientSide[{messageCounter++}], {request.Name}, {DateTime.Now}");
            }

            StreamReply streamReply = new StreamReply 
            {
                Message = "ClientSide Complete"
            };
            return streamReply;
        }

        public override async Task Bidirectional(IAsyncStreamReader<StreamRequest> requestStream, IServerStreamWriter<StreamReply> responseStream, ServerCallContext context)
        {
            long messageCounter = 0;
            while (await requestStream.MoveNext(context.CancellationToken))
            {
                StreamRequest request = requestStream.Current;
                Console.WriteLine(request.Name);
                await responseStream.WriteAsync(new StreamReply { Message = $"ClientSide[{messageCounter++}], {request.Name}, {DateTime.Now}" });
            }
        }
    }
}
