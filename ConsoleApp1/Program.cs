using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            const string Auth0Domain = "https://pse-addons.auth0.com/"; // Your Auth0 domain
            const string Auth0Audience = "urn:backend:api"; // Your API Identifier
            var token =
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IlEwVkdOelU0UXpFNE1EZzNRVU01TnpSRVFUVTVSRGRFUTBKRk5USXlOa0k1TkRZek5UWXlRUSJ9.eyJpc3MiOiJodHRwczovL3BzZS1hZGRvbnMuYXV0aDAuY29tLyIsInN1YiI6ImF1dGgwfDVhOTA0ZjA2ZGZhZGQ3MGYyOWEzYmNkZCIsImF1ZCI6WyJ1cm46YmFja2VuZDphcGkiLCJodHRwczovL3BzZS1hZGRvbnMuYXV0aDAuY29tL3VzZXJpbmZvIl0sImlhdCI6MTU0MjQwNDI4NiwiZXhwIjoxNTQyNDExNDg2LCJhenAiOiJuNW1UYThxMVpyS0l5Z094VzJ4aGxXQkNTaWZ1QXRyQSIsInNjb3BlIjoib3BlbmlkIHByb2ZpbGUgcmVhZDptZXNzYWdlcyJ9.LU-PCNMlBX7HWXRmfsHRvy93WK3MUGKkif1opd51zZd_UINCQZD8-IRq85ovAxFBa9E94KnqEaIaYxJR55FMxVavn8WPDhyWyedOXnJQIPDWUCUeId5le8hB8trq2ysHj1tlff1jO2naHM_aFAcPCobG2dzpcLMEWBVJKZASV1p3dVrn_UNhiPaohA3PYTE83mVYjeJ4gceJPbdoLntqA5xcmsXICXKiZN6P8d0RvhhlrKTkWxFguijCMQvmts6fhqB3osOEIaqTs94irvX-n_f5123xAgl4QbTOTqoxmQEko41nEQScDqMKLxtlypHo5ntyACcePoglm-DZ8FJCmw";
            IConfigurationManager<OpenIdConnectConfiguration> configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>($"{Auth0Domain}.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
            OpenIdConnectConfiguration openIdConfig = AsyncHelper.RunSync(async () => await configurationManager.GetConfigurationAsync(CancellationToken.None));



            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = Auth0Domain,
                ValidAudiences = new[] { Auth0Audience },
                IssuerSigningKeys = openIdConfig.SigningKeys


            };

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            if (!string.IsNullOrWhiteSpace(token))
            {
                try
                {
                    var user = handler.ValidateToken(token, tokenValidationParameters,
                        out _);
                   // var claim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                    // The code provided will print ‘Hello World’ to the console.
                    // Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.
                    foreach (var claim in user.Claims)
                    {
                        Console.WriteLine(claim.Type + ": " + claim.Value);
                    }
                    Console.ReadKey();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

                // Go to http://aka.ms/dotnet-get-started-console to continue learning how to build a console app! 
        }
    }

    internal static class AsyncHelper
    {
        private static readonly TaskFactory TaskFactory = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        public static void RunSync(Func<Task> func)
        {
            TaskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();
        }

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return TaskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();
        }
    }
}
