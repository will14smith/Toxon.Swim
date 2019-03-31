using System.Net;

namespace Toxon.Swim.Models
{
    public class SwimHost
    {
        private readonly IPEndPoint _endpoint;

        public SwimHost(IPEndPoint endpoint)
        {
            _endpoint = endpoint;
        }

        public IPEndPoint AsIPEndPoint()
        {
            return _endpoint;
        }
    }
}