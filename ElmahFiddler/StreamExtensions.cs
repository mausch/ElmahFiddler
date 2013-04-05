using System;
using System.IO;

namespace ElmahFiddler {
    // Backported from .NET 4.0
    public static class StreamExtensions {
        public static void CopyTo(this Stream source, Stream destination) {
            if (destination == null) {
                throw new ArgumentNullException("destination");
            }
            if (!source.CanRead && !source.CanWrite) {
                throw new ObjectDisposedException("source", "ObjectDisposed_StreamClosed");
            }
            if (!destination.CanRead && !destination.CanWrite) {
                throw new ObjectDisposedException("destination", "ObjectDisposed_StreamClosed");
            }
            if (!source.CanRead) {
                throw new NotSupportedException("NotSupported_UnreadableStream");
            }
            if (!destination.CanWrite) {
                throw new NotSupportedException("NotSupported_UnwritableStream");
            }
            source.InternalCopyTo(destination, 0x1000);
        }

        public static void CopyTo(this Stream source, Stream destination, int bufferSize) {
            if (destination == null) {
                throw new ArgumentNullException("destination");
            }
            if (bufferSize <= 0) {
                throw new ArgumentOutOfRangeException("bufferSize", "ArgumentOutOfRange_NeedPosNum");
            }
            if (!source.CanRead && !source.CanWrite) {
                throw new ObjectDisposedException("source", "ObjectDisposed_StreamClosed");
            }
            if (!destination.CanRead && !destination.CanWrite) {
                throw new ObjectDisposedException("destination", "ObjectDisposed_StreamClosed");
            }
            if (!source.CanRead) {
                throw new NotSupportedException("NotSupported_UnreadableStream");
            }
            if (!destination.CanWrite) {
                throw new NotSupportedException("NotSupported_UnwritableStream");
            }
            source.InternalCopyTo(destination, bufferSize);
        }

        private static void InternalCopyTo(this Stream source, Stream destination, int bufferSize) {
            int num;
            var buffer = new byte[bufferSize];
            while ((num = source.Read(buffer, 0, buffer.Length)) != 0) {
                destination.Write(buffer, 0, num);
            }
        }

        /// <summary>
        /// Fully reads a Stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] ReadAll(this Stream stream) {
            var buffer = new byte[32768];
            using (var ms = new MemoryStream()) {
                while (true) {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }
    }
}