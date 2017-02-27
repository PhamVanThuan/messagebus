using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace YmatouMQNet4.Extensions._Task
{
    /// <summary>Extension methods for asynchronously working with streams.</summary>
    public static class StreamExtensions
    {
        private const int BUFFER_SIZE = 0x2000;

        /// <summary>Read from a stream asynchronously.</summary>
        /// <param name="stream">The stream.</param>
        /// <param name="buffer">An array of bytes to be filled by the read operation.</param>
        /// <param name="offset">The offset at which data should be stored.</param>
        /// <param name="count">The number of bytes to be read.</param>
        /// <returns>A Task containing the number of bytes read.</returns>
        public static Task<int> ReadAsync(
            this Stream stream, byte[] buffer, int offset, int count)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            return Task<int>.Factory.FromAsync(
                stream.BeginRead, stream.EndRead,
                buffer, offset, count, stream /* object state */);
        }

        /// <summary>Write to a stream asynchronously.</summary>
        /// <param name="stream">The stream.</param>
        /// <param name="buffer">An array of bytes to be written.</param>
        /// <param name="offset">The offset from which data should be read to be written.</param>
        /// <param name="count">The number of bytes to be written.</param>
        /// <returns>A Task representing the completion of the asynchronous operation.</returns>
        public static Task WriteAsync(
            this Stream stream, byte[] buffer, int offset, int count)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            return Task.Factory.FromAsync(
                stream.BeginWrite, stream.EndWrite,
                buffer, offset, count, stream);
        }

        /// <summary>
        /// Creates an enumerable to be used with TaskFactoryExtensions.Iterate that reads data
        /// from an input stream and passes it to a user-provided delegate.
        /// </summary>
        /// <param name="input">The source stream.</param>
        /// <param name="bufferSize">The size of the buffers to be used.</param>
        /// <param name="bufferAvailable">
        /// A delegate to be invoked when a buffer is available (provided the
        /// buffer and the number of bytes in the buffer starting at offset 0.
        /// </param>
        /// <returns>An enumerable containing yielded tasks from the operation.</returns>
        private static IEnumerable<Task> ReadIterator(Stream input, int bufferSize, Action<byte[], int> bufferAvailable)
        {
            // Create a buffer that will be used over and over
            var buffer = new byte[bufferSize];

            // Until there's no more data
            while (true)
            {
                // Asynchronously read a buffer and yield until the operation completes
                var readTask = input.ReadAsync(buffer, 0, buffer.Length);
                yield return readTask;

                // If there's no more data in the stream, we're done.
                if (readTask.Result <= 0) break;

                // Otherwise, hand the data off to the delegate
                bufferAvailable(buffer, readTask.Result);
            }
        }      

        /// <summary>
        /// Creates an enumerable to be used with TaskFactoryExtensions.Iterate that copies data from one
        /// stream to another.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="output">The output stream.</param>
        /// <returns>An enumerable containing yielded tasks from the copy operation.</returns>
        public static IEnumerable<Task> CopyStreamIterator(this Stream input, Stream output)
        {
            // Create two buffers.  One will be used for the current read operation and one for the current
            // write operation.  We'll continually swap back and forth between them.
            byte[][] buffers = new byte[2][] { new byte[BUFFER_SIZE], new byte[BUFFER_SIZE] };
            int filledBufferNum = 0;
            Task writeTask = null;
            
            // Until there's no more data to be read
            while (true)
            {
                // Read from the input asynchronously
                var readTask = input.ReadAsync(buffers[filledBufferNum], 0, buffers[filledBufferNum].Length);

                // If we have no pending write operations, just yield until the read operation has
                // completed.  If we have both a pending read and a pending write, yield until both the read
                // and the write have completed.
                if (writeTask == null)
                {
                    yield return readTask;
                    readTask.Wait(); // propagate any exception that may have occurred
                }
                else
                {
                    var tasks = new[] { readTask, writeTask };
                    yield return Task.Factory.ContinueWhenAll(tasks, completedTasks => completedTasks);
                    Task.WaitAll(tasks); // propagate any exceptions that may have occurred
                }
                
                // If no data was read, nothing more to do.
                if (readTask.Result <= 0) break;

                // Otherwise, write the written data out to the file
                writeTask = output.WriteAsync(buffers[filledBufferNum], 0, readTask.Result);

                // Swap buffers
                filledBufferNum ^= 1;
            }
        }
    }
}