using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Owin;
using Owin;
using System;
using System.IO;

[assembly: OwinStartup(typeof(ButikProje.Startup))]

namespace ButikProje
{
    public class Startup
    {
        public static FirebaseApp FirebaseApp { get; private set; }
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();

            FirebaseApp = FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pdbutik-d5391-firebase-adminsdk-t3in0-feb8380bc7.json"))
            });
        }
    }
}
