﻿using System;
using System.IO;
using System.Threading;

namespace EdiabasLib
{
    public static class StreamExtension
    {
        public static bool HasData(this Stream inStream)
        {
            if (inStream == null)
            {
                return false;
            }

            if (inStream is MemoryQueueBufferStream memoryStream)
            {
                return memoryStream.IsDataAvailable();
            }
#if Android
            if (inStream is BtEscapeStreamReader escapeStream)
            {
                return escapeStream.IsDataAvailable();
            }

            return inStream.IsDataAvailable();
#else
            return inStream.Length > 0;
#endif
        }

        public static int ReadByteAsync(this Stream inStream, int timeout = 2000)
        {
            if (inStream == null)
            {
                return -1;
            }

            if (inStream is MemoryQueueBufferStream memoryStream)
            {
                return memoryStream.ReadByte();
            }

#if Android
            if (inStream is BtEscapeStreamReader escapeStream)
            {
                return escapeStream.ReadByte();
            }
#endif
            Semaphore waitSem = new Semaphore(0, 1);
            byte[] dataBuffer = new byte[1];
            int result = -1;
            IAsyncResult asyncResult = inStream.BeginRead(dataBuffer, 0, dataBuffer.Length, ar =>
            {
                try
                {
                    int bytes = inStream.EndRead(ar);
                    if (bytes > 0)
                    {
                        result = dataBuffer[0];
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                try
                {
                    // ReSharper disable once AccessToDisposedClosure
                    waitSem.Release();
                }
                catch (Exception)
                {
                    // ignored
                }
            }, null);

            if (!waitSem.WaitOne(timeout))
            {
                return -1;
            }

            if (!asyncResult.IsCompleted)
            {
                return -1;
            }

            waitSem.Dispose();

            return result;
        }
    }
}