using System;
using System.IO;

namespace BitJuice.Backup.Modules.Aggregators
{
    public class ArchiveStream : Stream
    {
        private readonly ArchiveBuilder builder;

        public override bool CanRead => true;
        public override bool CanWrite => false;
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
        
        public ArchiveStream(ArchiveBuilder builder)
        {
            this.builder = builder;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return builder.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
    }
}