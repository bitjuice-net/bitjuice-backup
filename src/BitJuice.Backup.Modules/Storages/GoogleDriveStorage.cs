using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        private static readonly string[] Scopes = { DriveService.Scope.Drive };
        private static readonly string ApplicationName = "VPS Backup";

        public GoogleDriveStorage(ILogger<GoogleDriveStorage> logger)
        {
            this.logger = logger;
        }

        public void Push(IEnumerable<IDataItem> items)
        {
            var credential = Authorize();

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
                var files = request.Execute().Files;
                Upload(service, item, files.FirstOrDefault());

                logger.LogInformation("Upload finished");
            }
        }

        private void Upload(DriveService service, IDataItem item, File file)
        {
            var metadata = new File
            {
                Name = item.Name,
                OriginalFilename = item.Name,
                Parents = new List<string>()
            };

            if (file == null && !string.IsNullOrWhiteSpace(Config.FolderId))
                metadata.Parents.Add(Config.FolderId);

            using var inputStream = item.GetStream();
            var request = file != null
                ? (ResumableUpload<File, File>) service.Files.Update(metadata, file.Id, inputStream, ContentType)
                : (ResumableUpload<File, File>) service.Files.Create(metadata, inputStream, ContentType);

            var progress = request.Upload();
            while (progress.Status != UploadStatus.Completed)
            {
                if (progress.Exception != null)
                    throw progress.Exception;
                Thread.Sleep(100);
            }
        }

        private UserCredential Authorize()
        {
            var clientSecrets = GoogleClientSecrets.FromFile(Config.CredentialsFile);
            var fileDataStore = new FileDataStore(Config.TokensDir, true);
            return GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets.Secrets, Scopes, "user", CancellationToken.None, fileDataStore, new GoogleCodeReceiver()).Result;
        }
    }
}