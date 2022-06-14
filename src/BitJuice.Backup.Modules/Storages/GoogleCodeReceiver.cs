using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;

namespace BitJuice.Backup.Modules.Storages;

public class GoogleCodeReceiver : ICodeReceiver
{
    public string RedirectUri => "https://google.com";

    public Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url, CancellationToken taskCancellationToken)
    {
        var authorizationUrl = url.Build().AbsoluteUri;

        Console.WriteLine("Please visit the following URL in a web browser, then enter the code shown after authorization:");
        Console.WriteLine(authorizationUrl);
        Console.WriteLine();

        var code = string.Empty;
        while (string.IsNullOrEmpty(code))
        {
            Console.WriteLine("Please enter code: ");
            code = Console.ReadLine();
        }

        return Task.FromResult(new AuthorizationCodeResponseUrl { Code = code });
    }
}