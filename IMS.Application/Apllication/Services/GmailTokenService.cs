//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Gmail.v1;
//using Google.Apis.Gmail.v1.Data;
//using Google.Apis.Services;
//using Google.Apis.Util.Store;
//using IMS.APPLICATION.Interface.Services;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace IMS.APPLICATION.Apllication.Services
//{
//    public class GmailTokenService
//    {
//        private static readonly string[] Scopes = { GmailService.Scope.GmailSend };
//        private const string ApplicationName = "IMS Gmail API Sender";

//        public async Task<GmailService> GetGmailServiceAsync()
//        {
//            using var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read);

//            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
//                GoogleClientSecrets.FromStream(stream).Secrets,
//                Scopes,
//                "user",
//                CancellationToken.None,
//                new FileDataStore("token.json", true)
//            );

//            return new GmailService(new BaseClientService.Initializer
//            {
//                HttpClientInitializer = credential,
//                ApplicationName = ApplicationName
//            });
//        }
//    }
//}
