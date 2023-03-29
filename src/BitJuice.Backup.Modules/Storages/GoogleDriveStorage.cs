using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BitJuice.Backup.Infrastructure;
using BitJuice.Backup.Model;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Logging;
using File = Google.Apis.Drive.v3.Data.File;

namespace BitJuice.Backup.Modules.Storages
{
    [ModuleName("google-drive-storage")]
    public class GoogleDriveStorage : ModuleBase<GoogleDriveConfig>, IStorage
    {
        private const string ContentType = "application/octet-stream";
        private readonly ILogger<GoogleDriveStorage> logger;

        private static readonly string ApplicationName = "VPS Backup";
        private static readonly string[] Scopes = { DriveService.Scope.Drive };

        public GoogleDriveStorage(ILogger<GoogleDriveStorage> logger)
        {
            this.logger = logger;
        }

        public async Task PushAsync(IEnumerable<IDataItem> items)
        {
            var credential = await AuthorizeAsync();

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            foreach (var item in items)
            {
                logger.LogInformation($"Starting upload of {item.Name} to Google Drive");

                var request = service.Files.List();
                request.Q = string.IsNullOrWhiteSpace(Config.FolderId)
                    ? $"name = '{item.Name}' and 'root' in parents and trashed = false"
                    : $"name = '{item.Name}' and '{Config.FolderId}' in parents and trashed = false";
                request.PageSize = 10;
                var response = await request.ExecuteAsync();
                await UploadAsync(service, item, response.Files.FirstOrDefault());

                logger.LogInformation("Upload finished");
            }
        }

        private async Task UploadAsync(DriveService service, IDataItem item, File file)
        {
            var metadata = new File
            {
                Name = item.Name,
                OriginalFilename = item.Name,
                Parents = new List<string>()
            };

            if (file == null && !string.IsNullOrWhiteSpace(Config.FolderId))
                metadata.Parents.Add(Config.FolderId);

            await using var inputStream = item.GetStream();
            ResumableUpload<File, File> request = file != null
                ? service.Files.Update(metadata, file.Id, inputStream, ContentType)
                : service.Files.Create(metadata, inputStream, ContentType);

            var progress = await request.UploadAsync();
            while (progress.Status != UploadStatus.Completed)
            {
                if (progress.Exception != null)
                    throw new Exception("GoogleDrive upload error.", progress.Exception);
                await Task.Delay(100);
            }
        }

        private async Task<UserCredential> AuthorizeAsync()
        {
            var clientSecrets = await GoogleClientSecrets.FromFileAsync(Config.CredentialsFile);
            var fileDataStore = new FileDataStore(Config.TokensDir, true);
            var timeout = Task.Delay(TimeSpan.FromMinutes(3));
            var task = GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets.Secrets, Scopes, "user", CancellationToken.None, fileDataStore, new GoogleCodeReceiver());
            if (await Task.WhenAny(task, timeout) == timeout)
                throw new TimeoutException("GoogleDrive authentication failed.");
            return task.Result;
        }
    }
}