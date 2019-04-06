using System;
using System.Net;

namespace Toxon.Swim.Models
{
    public class SwimHost : IEquatable<SwimHost>
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

        public override string ToString()
        {
            return $"{_endpoint}";
        }

        public bool Equals(SwimHost other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return Equals(_endpoint, other._endpoint);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is SwimHost other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _endpoint?.GetHashCode() ?? 0;
        }

        public static bool operator ==(SwimHost left, SwimHost right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SwimHost left, SwimHost right)
        {
            return !Equals(left, right);
        }
    }
}