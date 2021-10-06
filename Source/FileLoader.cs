// -----------------------------------------------------------------------------
// PROJECT   : Shannon Calculator
// COPYRIGHT : Andy Thomas 2021
// LICENSE   : GPLv3
// HOMEPAGE  : https://kuiper.zone
// -----------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;

namespace KuiperZone
{
    /// <summary>
    /// Loads a file. It can interpret most text file encodings.
    /// </summary>
    public sealed class FileLoader : IDisposable
    {
        private string? _string;
        private readonly MemoryStream _stream = new();

        /// <summary>
        /// Constructor with filename.
        /// </summary>
        public FileLoader(string filename)
        {
            using var file = new FileStream(filename, FileMode.Open, FileAccess.Read);
            file.CopyTo(_stream);

            Filename = filename;
        }

        /// <summary>
        /// Gets the filename provided on construction.
        /// </summary>
        public string Filename { get; }

        /// <summary>
        /// Gets the number of bytes read.
        /// </summary>
        public long ByteCount
        {
            get { return _stream.Length; }
        }

        /// <summary>
        /// Gets whether the content was interpretted as text. It returns false
        /// where <see cref="ByteCount"/> is 0.
        /// </summary>
        public bool IsText
        {
            get { return !string.IsNullOrEmpty(ToString()); }
        }

        /// <summary>
        /// Returns a new array of byte content.
        /// </summary>
        public byte[] ToArray()
        {
            return _stream.ToArray();
        }

        /// <summary>
        /// Overrides to provide string content. The result is an empty string if the content
        /// cannot be interpreted as text.
        /// </summary>
        public override string? ToString()
        {
            if (_string == null)
            {
                _string = ToString(_stream);
            }

            return _string;
        }

        /// <summary>
        /// Implements IDisposable.
        /// </summary>
        public void Dispose()
        {
            _stream.Dispose();
        }

        private static string ToString(Stream stream)
        {
            if (stream.Length == 0)
            {
                return "";
            }

            var rslt = TryEncoding(stream, GetFileBom(stream));

            if (rslt != null)
            {
                return rslt;
            }

            // The most (86.3 %) used encoding.
            // This will include ASCII.
            rslt = TryEncoding(stream, Encoding.UTF8);

            if (rslt != null)
            {
                return rslt;
            }

            // Look for a zero which indicate binary file
            int rb = 0;
            int count = 0;
            bool hasNulls = false;
            stream.Seek(0, SeekOrigin.Begin);

            while (rb > -1 && count++ < 32 * 1024)
            {
                rb = stream.ReadByte();

                if (rb == 0)
                {
                    hasNulls = true;
                    break;
                }
            }

            if (!hasNulls)
            {
                try
                {
                    // Supported on .NET Core
                    // ISO-8859-1/1252 is 6.7% used encoding.
                    rslt = TryEncoding(stream, Encoding.GetEncoding("ISO-8859-1"));

                    if (rslt != null)
                    {
                        return rslt;
                    }
                }
                catch (ArgumentException)
                {
                    // Not available
                    // Strangely, for example, 1252 is not supported on .NET Core.
                }
            }

            // UTF-16 LE
            rslt = TryEncoding(stream, Encoding.Unicode);

            if (rslt != null)
            {
                return rslt;
            }

            // Unknown / binary
            return "";
        }

        private static Encoding? GetFileBom(Stream stream)
        {
            // Read the BOM
            // https://stackoverflow.com/questions/3825390/effective-way-to-find-any-files-encoding
            stream.Seek(0, SeekOrigin.Begin);

            var bom = new byte[4];
            stream.Read(bom, 0, 4);

            // Analyze the BOM
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; // UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; // UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;

            return null;
        }

        private static string? TryEncoding(Stream stream, Encoding? enc)
        {
            if (enc != null)
            {
                // https://stackoverflow.com/questions/90838/how-can-i-detect-the-encoding-codepage-of-a-text-file
                stream.Seek(0, SeekOrigin.Begin);
                var verifier = Encoding.GetEncoding(enc.CodePage, new EncoderExceptionFallback(), new DecoderExceptionFallback());

                try
                {
                    var sb = new StringBuilder();
                    var reader = new StreamReader(stream, verifier, false, 1024, true);

                    while (true)
                    {
                        var line = reader.ReadLine();

                        if (line == null)
                        {
                            return sb.ToString();
                        }

                        sb.AppendLine(line);
                    }
                }
                catch (DecoderFallbackException)
                {
                }
            }

            return null;
        }

    }
}