using System.IO;

namespace Ionic.Zlib
{
	internal class ZlibBaseStream : Stream
	{
		internal enum StreamMode
		{
			Writer = 0,
			Reader = 1,
			Undefined = 2
		}

		protected internal ZlibCodec _z;

		protected internal StreamMode _streamMode;

		protected internal FlushType _flushMode;

		protected internal ZlibStreamFlavor _flavor;

		protected internal CompressionMode _compressionMode;

		protected internal CompressionLevel _level;

		protected internal bool _leaveOpen;

		protected internal byte[] _workingBuffer;

		protected internal int _bufferSize;

		protected internal byte[] _buf1;

		protected internal Stream _stream;

		protected internal CompressionStrategy Strategy;

		private bool nomoreinput;

		protected internal bool _wantCompress => false;

		private ZlibCodec z => null;

		private byte[] workingBuffer => null;

		public override bool CanRead => false;

		public override bool CanSeek => false;

		public override bool CanWrite => false;

		public override long Length => 0L;

		public override long Position
		{
			get
			{
				return 0L;
			}
			set
			{
			}
		}

		public ZlibBaseStream(Stream stream, CompressionMode compressionMode, CompressionLevel level, ZlibStreamFlavor flavor, bool leaveOpen)
		{
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
		}

		private void finish()
		{
		}

		private void end()
		{
		}

		protected override void Dispose(bool disposing)
		{
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return 0L;
		}

		public override void SetLength(long value)
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return 0;
		}

		public static void CompressBuffer(byte[] b, Stream compressor)
		{
		}

		public static byte[] UncompressBuffer(byte[] compressed, Stream decompressor)
		{
			return null;
		}
	}
}
