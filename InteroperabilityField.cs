/*
** Author: Samuel R. Blackburn
** Internet: wfc@pobox.com
**
** "You can get credit for something or get it done, but not both."
** Dr. Richard Garwin
**
** BSD License follows.
**
** Redistribution and use in source and binary forms, with or without
** modification, are permitted provided that the following conditions
** are met:
**
** Redistributions of source code must retain the above copyright notice,
** this list of conditions and the following disclaimer. Redistributions
** in binary form must reproduce the above copyright notice, this list
** of conditions and the following disclaimer in the documentation and/or
** other materials provided with the distribution. Neither the name of
** the WFC nor the names of its contributors may be used to endorse or
** promote products derived from this software without specific prior
** written permission.
**
** THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
** "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
** LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
** A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
** OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
** SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
** LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
** DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
** THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
** (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
** OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

/* SPDX-License-Identifier: BSD-2-Clause */

using System.Text;

namespace ExifRename
{
    /// <summary>
    /// This class handles IFDs
    /// </summary>
    public class InteroperabilityField
    {
        /// <summary>
        /// The human readable name of the tag.
        /// </summary>
        public string TagName;

        /// <summary>
        /// The numeric value of the tag
        /// </summary>
        public int Tag;

        /// <summary>
        /// The data type of the IFD.
        /// </summary>
        public int Type;

        /// <summary>
        /// The number of values.
        /// </summary>
        public int Count;

        /// <summary>
        /// The offset to the first value.
        /// </summary>
        public int OffsetToValue;

        /// <summary>
        /// True if integers are little endian when false, integers are big endian.
        /// </summary>
        public bool IsLittleEndian;

        /// <summary>
        /// The first byte of the value.
        /// </summary>
        public byte Byte0;

        /// <summary>
        /// The second byte of the value.
        /// </summary>
        public byte Byte1;

        /// <summary>
        /// The third byte of the value.
        /// </summary>
        public byte Byte2;

        /// <summary>
        /// The fourth byte of the value.
        /// </summary>
        public byte Byte3;

        /// <summary>
        /// The buffer containing the data
        /// </summary>
        public byte[] Buffer;

        /// <summary>
        /// The default constructor.
        /// </summary>
        public InteroperabilityField()
        {
            TagName = string.Empty;
            Tag = 0;
            Type = 0;
            Count = 0;
            OffsetToValue = 0;
            IsLittleEndian = false;
            Byte0 = 0;
            Byte1 = 0;
            Byte2 = 0;
            Byte3 = 0;
            Buffer = null;
        }

        /// <summary>
        /// Returns the value of this IFD as an 8-bit integer, affectionatly known as a byte.
        /// </summary>
        /// <returns>The byte value.</returns>
        public byte AsByte()
        {
            return (Byte0);
        }

        /// <summary>
        /// This returns the value of this IFD as a 16-bit integer.
        /// </summary>
        /// <returns>A 16-bit integer value.</returns>
        public short AsShort()
        {
            if (IsLittleEndian)
            {
                return ((short)(Byte0 + (short)(Byte1 << 8)));
            }
            else
            {
                return ((short)(Byte1 + (short)(Byte0 << 8)));
            }
        }

        /// <summary>
        /// Dumps the contents of this IFD to a string.
        /// </summary>
        /// <returns>The debugging contents of this IFD as a string.</returns>
        public override string ToString()
        {
            StringBuilder string_builder = new StringBuilder(256);

            string_builder.AppendFormat("Tag: {0:X04} ", Tag);

            if (string.IsNullOrEmpty(TagName) == false)
            {
                string_builder.AppendFormat("({0})", TagName);
            }

            string_builder.Append(", ");

            switch (Type)
            {
                case 1:

                    string_builder.Append("Byte (1)");
                    break;

                case 2:

                    string_builder.Append("ASCII (2)");
                    break;

                case 3:

                    string_builder.Append("Short (3)");
                    break;

                case 4:

                    string_builder.Append("Long (4)");
                    break;

                case 5:

                    string_builder.Append("Rational (5)");
                    break;

                case 7:

                    string_builder.Append("Undefined Byte (7)");
                    break;

                case 9:

                    string_builder.Append("Signed Long (9)");
                    break;

                case 10:

                    string_builder.Append("Signed Rational (10)");
                    break;

                default:

                    string_builder.AppendFormat("Unknown ({0})", Type);
                    break;
            }

            string_builder.AppendFormat(", Count: {0}, ", Count);

            string_builder.AppendFormat("Offset: {1}", Count, OffsetToValue);

            if (Count == 4)
            {
                string_builder.AppendFormat(", Data: {0:X02}{1:X02}{2:X02}{3:X02}", Byte0, Byte1, Byte2, Byte3);
            }
            else if (Count == 3)
            {
                string_builder.AppendFormat(", Data: {0:X02}{1:X02}{2:X02}", Byte0, Byte1, Byte2);
            }
            else if (Count == 2)
            {
                string_builder.AppendFormat(", Data: {0:X02}{1:X02}", Byte0, Byte1);
            }
            else if (Count == 1)
            {
                if (Type != 2 && Type != 3 && Type != 4 && Type != 5 && Type != 9 && Type != 10)
                {
                    string_builder.AppendFormat(", Data: {0:X02}", Byte0);
                }
            }
            else if (Count == 0)
            {
                string_builder.Append(", Data: none");
            }

            if (Buffer != null)
            {
                switch (Type)
                {
                    case 2:

                        string_builder.AppendFormat(", Value: \"{0}\"", AsString());
                        break;

                    case 3:

                        string_builder.AppendFormat(", Value: {0}", AsShort());
                        break;

                    case 9:
                    case 4:

                        string_builder.AppendFormat(", Value: {0}", OffsetToValue);
                        break;

                    case 5:
                    case 10:

                        string_builder.AppendFormat(", Value: {0}", AsDecimal());
                        break;
                }
            }

            return (string_builder.ToString());
        }

        /// <summary>
        /// Calculates the floating point value of this IFD.
        /// </summary>
        /// <returns>The value of this IFD as a float.</returns>
        public double AsDecimal()
        {
            int numerator = TIFFHeader.ToInt32(Buffer, OffsetToValue, IsLittleEndian);
            int denominator = TIFFHeader.ToInt32(Buffer, OffsetToValue + 4, IsLittleEndian);

            return (Exif.Rational(numerator, denominator));
        }

        /// <summary>
        /// The value of this IFD as a string.
        /// </summary>
        /// <returns>The string value in this IFD.</returns>
        public string AsString()
        {
            if (Count <= 4)
            {
                byte[] bytes = new byte[4];

                bytes[0] = (Count > 0) ? Byte0 : (byte)0;
                bytes[1] = (Count > 1) ? Byte1 : (byte)0;
                bytes[2] = (Count > 2) ? Byte2 : (byte)0;
                bytes[3] = (Count > 3) ? Byte3 : (byte)0;

                if (Count >= 1)
                {
                    return (System.Text.Encoding.ASCII.GetString(bytes, 0, Count).Trim("\0".ToCharArray()));
                }
                else
                {
                    return (string.Empty);
                }
            }

            if (OffsetToValue > 0 && Count > 1 && Buffer != null && Buffer.Length > (OffsetToValue + Count))
            {
                // last check...
                if (Type == 2)
                {
                    return (System.Text.Encoding.ASCII.GetString(Buffer, OffsetToValue, Count).Trim("\0".ToCharArray()));
                }
                else if (Type == 1)
                {
                    // Could be ASCII or Unicode
                    if (Buffer[OffsetToValue] != 0x00 && Buffer[OffsetToValue + 1] == 0x00)
                    {
                        // Little Endian Unicode
                        return (System.Text.Encoding.Unicode.GetString(Buffer, OffsetToValue, Count).Trim("\0".ToCharArray()));
                    }
                    else if (Buffer[OffsetToValue] == 0x00 && Buffer[OffsetToValue + 1] != 0x00)
                    {
                        // Big Endian Unicode
                        return (System.Text.Encoding.BigEndianUnicode.GetString(Buffer, OffsetToValue, Count).Trim("\0".ToCharArray()));
                    }
                    else if (Buffer[OffsetToValue] != 0x00 && Buffer[OffsetToValue + 1] != 0x00)
                    {
                        return (System.Text.Encoding.UTF8.GetString(Buffer, OffsetToValue, Count).Trim("\0".ToCharArray()));
                    }
                }
            }

            return (string.Empty);
        }

        /// <summary>
        /// Parses the bytes specified as an InteroperabilityField.
        /// </summary>
        /// <param name="buffer">The buffer containing the data.</param>
        /// <param name="offset">The offset into the buffer to begin parsing.</param>
        /// <param name="little_endian">When true, integers are in little endian format, when false integers are in big endian format.</param>
        /// <returns>An InteroperabilityField object.</returns>
        public static InteroperabilityField FromBytes(byte[] buffer, int offset, bool little_endian)
        {
            var return_value = new InteroperabilityField();

            return_value.IsLittleEndian = little_endian;
            return_value.Tag = TIFFHeader.ToUInt16(buffer, offset, little_endian);
            return_value.Type = TIFFHeader.ToUInt16(buffer, offset + 2, little_endian);
            return_value.Count = TIFFHeader.ToInt32(buffer, offset + 4, little_endian);
            return_value.OffsetToValue = TIFFHeader.ToInt32(buffer, offset + 8, little_endian);

            return_value.Byte0 = buffer[offset + 8];
            return_value.Byte1 = buffer[offset + 9];
            return_value.Byte2 = buffer[offset + 10];
            return_value.Byte3 = buffer[offset + 11];

            return_value.Buffer = buffer;

            return (return_value);
        }
    }
}
