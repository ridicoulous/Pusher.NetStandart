namespace PusherClient
{
    public class PusherOptions
    {
        public bool Encrypted = false;
        public IAuthorizer Authorizer = null;
        public string Cluster = null;
        public string Endpoint = null;
        public string Client= "js";
        public int PingTimeout = 120000;
        public int ProtocolNumber = 7;
        public string Version = "4.4.0";

        internal string Host
        {
            
            get 
            {
                if (string.IsNullOrEmpty(Endpoint))
                {
                    if(string.IsNullOrEmpty(Cluster))
                    {
                        return "ws.pusher.com";
                    }
                    else
                    return string.Format("ws-{0}.pusher.com", this.Cluster);

                }
                else
                    return Endpoint;
            }
        }
    }
}
