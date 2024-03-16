using System.Text;
using UnityEngine;
using WebSocketSharp.Net;

namespace Barebones.MasterServer
{
    public class HomeHttpController : HttpController
    {
        public override void Initialize(HttpServerModule server)
        {
            base.Initialize(server);

            server.RegisterHttpRequestHandler("home", OnHomeHttpRequestHandler);
        }

        private void OnHomeHttpRequestHandler(HttpListenerRequest request, HttpListenerResponse response)
        {
            Msf.Concurrency.RunInMainThread(() => {
                // Here you can call unity API
                
            });

            byte[] contents = GetHtmlBytes();

            response.ContentType = "text/html";
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = contents.LongLength;
            response.Close(contents, true);
        }
    }
}