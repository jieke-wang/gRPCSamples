using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grpc.Core;

namespace GrpcSelfHostHelloWordService.Impls
{
    public class GreeterImpl : Greeter.GreeterBase
    {
        // 服务端RPC的SayHello操作方法
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply { Message = "你好 " + request.Name });
        }
    }
}
