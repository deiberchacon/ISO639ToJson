using System;
using System.IO;
using System.Net;
using System.Text;

namespace ISO639ToJson
{
	internal class HttpPageGetter
	{
		internal string Get(string urlAddress)
		{
			HttpWebResponse response = null;
			StreamReader readStream = null;

			try
			{
				var request = (HttpWebRequest)WebRequest.Create(urlAddress);
				response = (HttpWebResponse)request.GetResponse();

				if (response.StatusCode == HttpStatusCode.OK)
				{
					var receiveStream = response.GetResponseStream();
					if (receiveStream == null)
					{
						throw new NotSupportedException($"Failed to retrieve the stream for URL {urlAddress}");
					}

					readStream = response.CharacterSet == null
						? new StreamReader(receiveStream)
						: new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

					return readStream.ReadToEnd();
				}
			}
			finally
			{
				response?.Close();
				readStream?.Close();
			}

			return null;
		}
	}
}
