using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BitJuice.Backup.Model;
using Microsoft.Extensions.Logging;

namespace BitJuice.Backup.Modules.Aggregators
{
    public abstract class ArchiveBuilder
    {
        private const int DefaultBufferSize = 10 * 1024 * 1024;
        
        private readonly byte[] readBuffer;
        private readonly IAsyncEnumerator<IDataItem> enumerator;
        private ArchiveBuilderState currentState = ArchiveBuilderState.OpenArchive;

        private ILogger<ArchiveBuilder> Logger { get; }
        protected ProxyStream OutputStream { get; }
        protected IDataItem CurrentItem => enumerator.Current;
        protected Stream CurrentItemStream { get; private set; }
        protected long ContentSize { get; private set; }
        protected long ContentRead { get; private set; }

        protected ArchiveBuilder(ILogger<ArchiveBuilder> logger, IAsyncEnumerable<IDataItem> items, int fileBufferSize = DefaultBufferSize)
        {
            Logger = logger;
            enumerator = items.GetAsyncEnumerator();
            readBuffer = new byte[fileBufferSize];
            OutputStream = new ProxyStream(fileBufferSize);
        }

        public async Task<int> Read(byte[] buffer, int offset, int count)
        {
            await FetchBufferAsync();
            return OutputStream.Read(buffer, offset, count);
        }

        private async Task FetchBufferAsync()
        {
            if (OutputStream.Available > 0)
                return;

            OutputStream.Clear();

            while (OutputStream.Length <= 0 && currentState != ArchiveBuilderState.Done)
                currentState = await HandleStatusAsync(currentState);

            OutputStream.Rewind();
        }

        private async Task<ArchiveBuilderState> HandleStatusAsync(ArchiveBuilderState state)
        {
            switch (state)
            {
                case ArchiveBuilderState.OpenArchive:
                    OpenArchive();
                    return ArchiveBuilderState.OpenFile;

                case ArchiveBuilderState.OpenFile:
                    if (!await enumerator.MoveNextAsync())
                        return ArchiveBuilderState.CloseArchive;

                    try
                    {
                        CurrentItemStream = CurrentItem.GetStream();
                        ContentSize = CurrentItemStream.Length;
                        ContentRead = 0;
                    }
                    catch (Exception ex)
                    {
                        var name = string.IsNullOrWhiteSpace(CurrentItem.VirtualPath) ? CurrentItem.Name : CurrentItem.VirtualPath;
                        Logger.LogWarning(ex, $"Cannot open data item: {name}. Skipping.");
                        return ArchiveBuilderState.OpenFile;
                    }

                    OpenFile();
                    return ArchiveBuilderState.ReadFile;

                case ArchiveBuilderState.ReadFile:
                    var count = CurrentItemStream.Read(readBuffer, 0, readBuffer.Length);
                    if (count == 0)
                        return ArchiveBuilderState.CloseFile;

                    if (ContentRead + count > ContentSize)
                    {
                        Logger.LogWarning("File " + CurrentItem.VirtualPath + " has been altered (+) during read. File content may be corrupted.");
                        count = (int)(ContentSize - ContentRead);
                    }
                    
                    ContentRead += count;
                    WriteFile(readBuffer, count);
                    return ArchiveBuilderState.ReadFile;

                case ArchiveBuilderState.CloseFile:
                    if(ContentRead < ContentSize)
                        Logger.LogWarning("File " + CurrentItem.VirtualPath + " has been altered (-) during read. File content may be corrupted.");

                    CloseFile();
                    CurrentItemStream.Dispose();
                    CurrentItemStream = null;
                    ContentSize = 0;
                    ContentRead = 0;
                    return ArchiveBuilderState.OpenFile;

                case ArchiveBuilderState.CloseArchive:
                    CloseArchive();
                    return ArchiveBuilderState.Done;

                case ArchiveBuilderState.Done:
                    return ArchiveBuilderState.Done;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected abstract void OpenArchive();
        protected abstract void OpenFile();
        protected abstract void WriteFile(byte[] buffer, int count);
        protected abstract void CloseFile();
        protected abstract void CloseArchive();
    }
}