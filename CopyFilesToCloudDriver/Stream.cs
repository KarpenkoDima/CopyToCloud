using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyFilesToCloudDriver
{
    public class ProgressionStream : Stream
    {
        public delegate void ProgressionHandler(double progression, double size);

        private Stream _sourceStream;
        private ProgressionHandler _progressionHandler;

        public ProgressionStream(Stream sourceStream, ProgressionHandler progressionHandler)
        {
            this._sourceStream = sourceStream;
            this._progressionHandler = progressionHandler;
        }

        public override int Read(byte[] array, int offset, int count)
        {
            this._progressionHandler(this.Position / (double)this.Length * 100, (double)this.Length);

            return this._sourceStream.Read(array, offset, count);
        }

        public override bool CanRead
        {
            get
            {
                return this._sourceStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return this._sourceStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this._sourceStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return this._sourceStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return this._sourceStream.Position;
            }

            set
            {
                this._sourceStream.Position = value;
            }
        }

        public override void Flush()
        {
            this._sourceStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this._sourceStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this._sourceStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this._sourceStream.Write(buffer, offset, count);
        }
    }
}
