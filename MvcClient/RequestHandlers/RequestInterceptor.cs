﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MvcClient.RequestHandlers
{
    public class RequestInterceptor: DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            

            //request.Headers.Add("Authorization", "xdsadad");

            return base.SendAsync(request, cancellationToken);
        }
    }
}
