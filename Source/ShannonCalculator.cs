// -----------------------------------------------------------------------------
// PROJECT   : Shannon Calculator
// COPYRIGHT : Andy Thomas 2021
// LICENSE   : GPLv3
// HOMEPAGE  : https://kuiper.zone
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace KuiperZone
{
    /// <summary>
    /// Caclulates Shannon self-information in a message. A message comprises a sequence of "letters" which may be
    /// added using one of the Add() methods. The <see cref="Result"/> value then gives a messure of information
    /// in the message.
    /// </summary>
    public sealed class ShannonCalculator
    {
        private int _logBase = 2;
        private bool _isMetricEntropy;
        private bool _hasResult;
        private double _resultCache;
        private string? _stringCache;
        private readonly Dictionary<char, double> _alphabet = new();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ShannonCalculator()
        {
            Alphabet = _alphabet;

        }

        /// <summary>
        /// Gets or sets the logarithmic base. The default value is 2 (bits). If the value is 1 or less, the result
        /// is calculated using the natural logarithm (nats). The value cannot exceed 255.
        /// </summary>
        public int LogBase
        {
            get { return _logBase; }

            set
            {
                value = Math.Min(value, 255);

                if (_logBase != value)
                {
                    _logBase = value;
                    _hasResult = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the result is calculated as "metric entropy". This is simply Shannon measure
        /// divided by the message length.
        /// </summary>
        public bool IsMetricEntropy
        {
            get { return _isMetricEntropy; }

            set
            {
                if (_isMetricEntropy != value)
                {
                    _isMetricEntropy = value;
                    _hasResult = false;
                }
            }
        }

        /// <summary>
        /// Gets the total message length.
        /// </summary>
        public long Length { get; private set; }

        /// <summary>
        /// Gets the alphabet dictionary pertaining to the message. The key is the letter and the value
        /// is the count of the letter in the message. Counts can be divided by <see cref="Length"/> to
        /// determine probability.
        /// </summary>
        public IReadOnlyDictionary<char, double> Alphabet { get; }

        /// <summary>
        /// Gets the Shannon self-information measure calculated according to
        /// <see cref="LogBase"/>. The result is always NaN if <see cref="Length"/> is 0.
        /// </summary>
        public double Result
        {
            get
            {
                if (Length == 0)
                {
                    return double.NaN;
                }

                if (_hasResult)
                {
                    return _resultCache;
                }

                // Faster on the stack
                double sum = 0;
                double cnt = Length;

                if (_logBase < 2)
                {
                    foreach (var x in _alphabet.Values)
                    {
                        var p = x / cnt;
                        sum -= p * Math.Log(p);
                    }
                }
                else
                if (_logBase == 2)
                {
                    foreach (var x in _alphabet.Values)
                    {
                        var p = x / cnt;
                        sum -= p * Math.Log2(p);
                    }
                }
                else
                if (_logBase == 10)
                {
                    foreach (var x in _alphabet.Values)
                    {
                        var p = x / cnt;
                        sum -= p * Math.Log10(p);
                    }
                }
                else
                {
                    double b = _logBase;

                    foreach (var x in _alphabet.Values)
                    {
                        var p = x / cnt;
                        sum -= p * Math.Log(p) / Math.Log(b);
                    }
                }

                _resultCache = sum;

                if (_isMetricEntropy)
                {
                    _resultCache /= cnt;
                }

                _hasResult = true;
                return _resultCache;
            }
        }

        /// <summary>
        /// Adds a single char letter.
        /// </summary>
        public void Add(char x)
        {
            _alphabet.TryGetValue(x, out double p);
            _alphabet[x] = p + 1;

            Length += 1;
            _hasResult = false;
        }

        /// <summary>
        /// Adds a single byte letter.
        /// </summary>
        public void Add(byte x)
        {
            Add((char)x);
        }

        /// <summary>
        /// Adds a sequence of characters. This can be used with the string type.
        /// </summary>
        public void Add(IEnumerable<char> X)
        {
            foreach(var x in X)
            {
                Add(x);
            }
        }

        /// <summary>
        /// Adds a sequence of bytes.
        /// </summary>
        public void Add(IEnumerable<byte> X)
        {
            foreach (var x in X)
            {
                Add(x);
            }
        }

        /// <summary>
        /// Clears the data and sets <see cref="Length"/> to 0.
        /// </summary>
        public void Clear()
        {
            Length = 0;

            _alphabet.Clear();
            _hasResult = false;
        }

        /// <summary>
        /// Overrides to give result information.
        /// </summary>
        public override string? ToString()
        {
            if (_stringCache == null || !_hasResult)
            {
                var sb = new StringBuilder();

                sb.Append("Message Length: ");
                sb.AppendLine(Length.ToString());

                sb.Append("Letter Count: ");
                sb.AppendLine(_alphabet.Count.ToString());

                sb.Append("Log Base: ");

                if (_logBase < 2)
                {
                    sb.AppendLine("e (nats)");
                }
                else
                if (_logBase == 2)
                {
                    sb.AppendLine("2 (bits)");
                }
                else
                {
                    sb.AppendLine(_logBase.ToString());
                }

                sb.Append("Metric Entropy: ");
                sb.AppendLine(_isMetricEntropy.ToString());

                sb.Append("Result: ");
                sb.Append(Result.ToString());

                _stringCache = sb.ToString();
            }

            return _stringCache;
        }
    }
}