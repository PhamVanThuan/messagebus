using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace YmatouMQNet4.Extensions._Task
{
    /// <summary>Provides asynchronous counterparts to members of the File class.</summary>
    public static class FileAsync
    {
        private const int BUFFER_SIZE = 0x2000;

        /// <summary>Opens an existing file for asynchronous reading.</summary>
        /// <param name="path">The path to the file to be opened for reading.</param>
        /// <returns>A read-only FileStream on the specified path.</returns>
        public static FileStream OpenRead(string path)
        {
            // Open a file stream for reading and that supports asynchronous I/O
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, true);
        }

        /// <summary>Opens an existing file for asynchronous writing.</summary>
        /// <param name="path">The path to the file to be opened for writing.</param>
        /// <returns>An unshared FileStream on the specified path with access for writing.</returns>
        public static FileStream OpenWrite(string path)
        {
            // Open a file stream for writing and that supports asynchronous I/O
            return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, BUFFER_SIZE, true);
        }      
        /// <summary>
        /// Opens a binary file for asynchronous operation, writes the contents of the byte array into the file, and then closes the file.
        /// </summary>
        /// <param name="path">The path to the file to be written.</param>
        /// <returns>A task that will signal the completion of the operation.</returns>
        public static Task WriteAllBytes(string path, byte[] bytes)
        {
            // Open the file for writing
            var fs = OpenWrite(path);

            // Write the contents to the file
            var asyncWrite = fs.WriteAsync(bytes, 0, bytes.Length);

            // When complete, close the file and propagate any exceptions
            var closedFile = asyncWrite.ContinueWith(t =>
            {
                var e = t.Exception;
                fs.Close();
                if (e != null) throw e;
            }, TaskContinuationOptions.ExecuteSynchronously);

            // Return a task that represents the operation having completed
            return closedFile;
        }      

        /// <summary>
        /// Opens a text file for asynchronosu operation, writes a string into the file, and then closes the file.
        /// </summary>
        /// <param name="path">The path to the file to be written.</param>
        /// <returns>A task that will signal the completion of the operation.</returns>
        public static Task WriteAllText(string path, string contents)
        {
            // First encode the string contents into a byte array
            var encoded = Task.Factory.StartNew(
                state => Encoding.UTF8.GetBytes((string)state),
                contents);

            // When encoding is done, write all of the contents to the file.  Return
            // a task that represents the completion of that write.
            return encoded.ContinueWith(t => WriteAllBytes(path, t.Result)).Unwrap();
        }        
    }
}