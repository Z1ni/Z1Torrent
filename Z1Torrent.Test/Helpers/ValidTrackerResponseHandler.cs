using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Z1Torrent.Test.Helpers {

    public class ValidTrackerResponseHandler : HttpMessageHandler {

        private readonly byte[] _response;

        public ValidTrackerResponseHandler(byte[] response) {
            _response = response;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            var resp = new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new ByteArrayContent(_response)
            };
            return await Task.FromResult(resp); // await Task.FromResult is here for removing the async warning (unnecessary async keyword)
        }

    }

}
