//using Newtonsoft.Json;
using PusherClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using PiPusher = PusherServer.Pusher;
namespace ExampleApplication
{

    class Program
    {
        static Pusher _pusher = null;
        static Channel _chatChannel = null;
        static PresenceChannel _presenceChannel = null;
        static string _name;
            
        static void Main(string[] args)
        {
            InitPusher();
            Console.ReadLine();
        }

        private static void _myChannel_Subscribed(object sender)
        {
            Console.WriteLine("Subscribed");
        }

        #region Pusher Initiation / Connection

        private static void InitPusher()
        {
            //var piPusher = new PiPusher("","","");
            
            _pusher = new Pusher("4b6a8b2c758be4e58868", new PusherOptions() { Authorizer = new HttpAuthorizer("https://kuna.io/pusher/auth"), Encrypted = true, Endpoint = "pusher.kuna.io", ProtocolNumber = 7, Version = "3.0.0" });
            _pusher.ConnectionStateChanged += _pusher_ConnectionStateChanged;
            _pusher.Error += _pusher_Error;

            // Setup private channel
            _chatChannel = _pusher.Subscribe("private-7GBET5ZHZ");
            _chatChannel.Subscribed += _chatChannel_Subscribed;

            // Inline binding!
            _chatChannel.Bind("account", (dynamic data) =>
            {
                Console.WriteLine(data);
            });

            // Setup presence channel          

            _pusher.Connect();
        }

        static void _pusher_Error(object sender, PusherException error)
        {
            Console.WriteLine("Pusher Error: " + error.ToString());
        }

        static void _pusher_ConnectionStateChanged(object sender, ConnectionState state)
        {
            Console.WriteLine("Connection state: " + state.ToString());
        }

        #endregion

        #region Presence Channel Events

        static void _presenceChannel_Subscribed(object sender)
        {
            ListMembers();
        }

        static void _presenceChannel_MemberRemoved(object sender)
        {
            ListMembers();
        }

        static void _presenceChannel_MemberAdded(object sender, KeyValuePair<string, dynamic> member)
        {
            Console.WriteLine((string)member.Value.name.Value + " has joined");
            ListMembers();
        }

        #endregion

        #region Chat Channel Events

        static void _chatChannel_Subscribed(object sender)
        {
            Console.WriteLine("Hi " + _name + "! Type 'quit' to exit, otherwise type anything to chat!");
        }

        #endregion

        static void ListMembers()
        {
            List<string> names = new List<string>();

            foreach (var mem in _presenceChannel.Members)
            {
                names.Add((string)mem.Value.name.Value);
            }

            Console.WriteLine("[MEMBERS] " + names.Aggregate((i,j) => i + ", " + j ));
        }
        
    }

}
