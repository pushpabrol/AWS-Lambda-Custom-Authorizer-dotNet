using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSLambda1
{
    public class Function
    {
        
        public APIGatewayCustomAuthorizerResponse FunctionHandler(APIGatewayCustomAuthorizerRequest apigAuthRequest, ILambdaContext context)
        {
        string auth0Domain = context.ClientContext.Environment["Auth0Domain"];
        string auth0Audience = context.ClientContext.Environment["Auth0Audience"];
        IConfigurationManager<OpenIdConnectConfiguration> configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>($"{auth0Domain}.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
            OpenIdConnectConfiguration openIdConfig = AsyncHelper.RunSync(async () => await configurationManager.GetConfigurationAsync(CancellationToken.None));


            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = auth0Domain,
                ValidAudiences = new[] { auth0Audience },
                IssuerSigningKeys = openIdConfig.SigningKeys
             

            };

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            bool authorized = false;
            Claim subject = null;
            string[] scopes = null;
            if (!string.IsNullOrWhiteSpace(apigAuthRequest.AuthorizationToken))
            {
                try
                {
                    var user = handler.ValidateToken(apigAuthRequest.AuthorizationToken, tokenValidationParameters, out _);
                    var audience = user.Claims.FirstOrDefault(c => c.Type == "aud");
                    var issuer = user.Claims.FirstOrDefault(c => c.Type == "iss");
                    subject = user.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                    if (user.Claims.Any(c => c.Type == "scope"))
                    {
                        scopes = user.Claims.First(c => c.Type == "scope").Value.Split(" ").Where(x => x.Contains(":")).ToArray();
                    }

                    if (audience != null && issuer != null)
                        authorized = issuer.Value == auth0Domain && audience.Value == auth0Audience; // Ensure that the claim value matches the assertion
                }
                catch (Exception ex)
                {
                    LambdaLogger.Log($"Error occurred validating token: {ex.Message}");
                }
            }
            APIGatewayCustomAuthorizerPolicy policy = new APIGatewayCustomAuthorizerPolicy
            {
                Version = "2012-10-17",
                Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>()
            };

            policy.Statement.Add(new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
            {
                Action = new HashSet<string>(new string[] { "execute-api:Invoke" }),
                Effect = authorized ? "Allow" : "Deny",
                Resource = new HashSet<string>(new string[] { apigAuthRequest.MethodArn })

            });


            APIGatewayCustomAuthorizerContextOutput contextOutput = new APIGatewayCustomAuthorizerContextOutput();
            contextOutput["User"] = authorized ? subject.Value : null;
            contextOutput["Path"] = apigAuthRequest.MethodArn;
            //adding a permissions array - ideally this would be in a claim or the scope from the token
            var permissions = scopes ?? new string[]{};

            contextOutput["Permissions"] = string.Join(",", permissions);

            return new APIGatewayCustomAuthorizerResponse
            {
                PrincipalID = authorized ? subject.Value : null,
                Context = contextOutput,
                PolicyDocument = policy
            };

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