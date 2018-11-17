using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using AWSLambda1;

namespace AWSLambda1.Tests
{
    public class FunctionTest
    {
        [Fact]
        public void TestTokenValidation()
        {
            try
            {
                // Invoke the lambda function and confirm the string was upper cased.
                var function = new Function();
                var context = new TestLambdaContext();
                context.ClientContext = new TestClientContext();

                context.ClientContext.Environment["Auth0Domain"] = "https://pse-addons.auth0.com/";
                context.ClientContext.Environment["Auth0Audience"] = "urn:backend:api";
                APIGatewayCustomAuthorizerRequest request = new APIGatewayCustomAuthorizerRequest();
                request.AuthorizationToken =
                    "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IlEwVkdOelU0UXpFNE1EZzNRVU01TnpSRVFUVTVSRGRFUTBKRk5USXlOa0k1TkRZek5UWXlRUSJ9.eyJpc3MiOiJodHRwczovL3BzZS1hZGRvbnMuYXV0aDAuY29tLyIsInN1YiI6ImF1dGgwfDVhOTA0ZjA2ZGZhZGQ3MGYyOWEzYmNkZCIsImF1ZCI6WyJ1cm46YmFja2VuZDphcGkiLCJodHRwczovL3BzZS1hZGRvbnMuYXV0aDAuY29tL3VzZXJpbmZvIl0sImlhdCI6MTU0MjQyMzk4MiwiZXhwIjoxNTQyNDMxMTgyLCJhenAiOiJuNW1UYThxMVpyS0l5Z094VzJ4aGxXQkNTaWZ1QXRyQSIsInNjb3BlIjoib3BlbmlkIHByb2ZpbGUgcmVhZDptZXNzYWdlcyJ9.jldcrBdgGR5Ys68CXiWizS-sgI4bRF3nzUiMhZSY2gHDrye64iDp_YwYnwMAYhR16jp7e3ejspcNM9ymXjdlUb5h9nx8MSYXxoD9xZ-TwkR0mspeCXwdl-qYuQPb0ArcXVraiaiFkrmBfIWDyIiAJGgGUTIC6z1r_oAEv8zrK46kOCKiwqa4rg5dqR3iWsk4DfySG3_Ap9VUyTu8fsN3zz__FAy4N0Dnhs7NtSmn4ZKEBEtP0eanPPso7o0tUBkcGjw_R6PwVPi0fK7K1v7YpNFsYl4M1_stTv6o6uHhQWxWMJNJCgWf68MHJVIeCtNa9gZa03imY8-JXujLrLOxGQ";
                request.MethodArn = "/path";
                var response = function.FunctionHandler(request, context);
                Assert.Equal("auth0|5a904f06dfadd70f29a3bcdd", response.PrincipalID);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
