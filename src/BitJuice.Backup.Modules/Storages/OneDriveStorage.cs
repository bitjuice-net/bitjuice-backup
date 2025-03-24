using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using BitJuice.Backup.Infrastructure;
using BitJuice.Backup.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Drives.Item.Items.Item.CreateUploadSession;
using Microsoft.Graph.Models;

namespace BitJuice.Backup.Modules.Storages;

[ModuleName("onedrive-drive-storage")]
public class OneDriveStorage : ModuleBase<OneDriveConfig>, IStorage
{
    private static readonly string TokenPath = ".\\tokens\\msal.json";
    private static readonly string[] Scopes = ["Files.ReadWrite"];

    private readonly ILogger<OneDriveStorage> logger;

    public OneDriveStorage(ILogger<OneDriveStorage> logger)
    {
        this.logger = logger;
    }

    public async Task PushAsync(IAsyncEnumerable<IDataItem> items)
    {
        var graphClient = await AuthorizeAsync();
        var aaa = await graphClient.Me.Drive.GetAsync();
        await foreach (var item in items)
        {
            logger.LogInformation($"Starting upload of {item.Name} to OneDrive");
            await UploadAsync(graphClient, item);
            logger.LogInformation("Upload finished");
        }
    }

    private async Task<GraphServiceClient> AuthorizeAsync()
    {
        var options = new DeviceCodeCredentialOptions
        {
            ClientId = Config.ClientId,
            TenantId = Config.TenantId,
            TokenCachePersistenceOptions = new TokenCachePersistenceOptions
            {
                Name = "BitJuice.Backup",
                UnsafeAllowUnencryptedStorage = true
            }
        };

        if (File.Exists(TokenPath))
        {
            await using var stream = File.OpenRead(TokenPath);
            options.AuthenticationRecord = await AuthenticationRecord.DeserializeAsync(stream);
        }

        var tokenCredential = new DeviceCodeCredential(options);

        if (options.AuthenticationRecord is null)
        {
            var authRecord = await tokenCredential.AuthenticateAsync(new TokenRequestContext(Scopes));
            await using var stream = File.OpenWrite(TokenPath);
            await authRecord.SerializeAsync(stream);
        }

        return new GraphServiceClient(tokenCredential, Scopes);
    }

    private async Task UploadAsync(GraphServiceClient graphClient, IDataItem item)
    {
        var maxChunkSize = 327680; // 320 KB
        await using var stream = item.GetStream();
        var aaa = new BufferedStream(stream);
        var driveId = await graphClient.Me.Drive.GetAsync();
        var session = await graphClient.Drives[driveId.Id].Root.ItemWithPath("test\\" + item.Name).CreateUploadSession.PostAsync(new CreateUploadSessionPostRequestBody());
        var uploadTask = new LargeFileUploadTask<DriveItem>(session, aaa, maxChunkSize);
        var uploadResult = await uploadTask.UploadAsync();
        if (uploadResult.UploadSucceeded)
            Console.WriteLine($"Uploaded: {uploadResult.ItemResponse.Id}");
    }
}