using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace XamarinOffline
{
	class LoggingHandler : DelegatingHandler
	{
		protected override async Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
		{
			var response = await base.SendAsync (request, cancellationToken);
			var content = await response.Content.ReadAsStringAsync ();
			System.Diagnostics.Debug.WriteLine ("Response:   " + content);
			return response;
		}
	}
}

