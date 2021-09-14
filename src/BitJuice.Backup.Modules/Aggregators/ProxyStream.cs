using System.IO;

namespace BitJuice.Backup.Modules.Aggregators
{
    public class ProxyStream : MemoryStream
    {
        public override bool CanSeek => false;
        public long Available => Length - Position;

        public ProxyStream(int size) : base(size)
        {
        }

        public void Clear()
        {
            SetLength(0);
        }

        public void Rewind()
        {
            Seek(0, SeekOrigin.Begin);
        }
    }
}
