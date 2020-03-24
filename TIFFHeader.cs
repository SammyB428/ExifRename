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

namespace ExifRename
{
    /// <summary>
    /// This class handles TIFF headers.
    /// </summary>
    public class TIFFHeader
    {
        /// <summary>
        /// When true, integers are little endian, when false they are big endian.
        /// </summary>
        public bool LittleEndian;

        /// <summary>
        /// The meaning of Life, The Universe and Everything.
        /// </summary>
        public int FortyTwo;

        /// <summary>
        /// The offset to the first IFD.
        /// </summary>
        public int OffsetToZerothIFD;

        /// <summary>
        /// The default constructor.
        /// </summary>
        public TIFFHeader()
        {
            Empty();
        }

        /// <summary>
        /// Returns the object to an initial state.
        /// </summary>
        public void Empty()
        {
            LittleEndian = false;
            FortyTwo = 0;
            OffsetToZerothIFD = 0;
        }

        /// <summary>
        /// Parses the given bytes.
        /// </summary>
        /// <param name="bytes">The bytes to parse.</param>
        /// <param name="offset">The offset into the byte array to begin parsing.</param>
        /// <returns>True on success, false on failure.</returns>
        public bool FromBytes(byte[] bytes, int offset)
        {
            Empty();

            if (bytes == null || offset < 0 || (offset + 8) >= bytes.Length)
            {
                return (false);
            }

            bool endianess_found = false;

            if (bytes[offset] == 'M' &&
                bytes[offset + 1] == 'M')
            {
                // Big Endian
                endianess_found = true;
                LittleEndian = false;
            }
            else if (bytes[offset] == 'I' &&
                      bytes[offset + 1] == 'I')
            {
                // Little Endian
                endianess_found = true;
                LittleEndian = true;
            }

            if (endianess_found == false)
            {
                return (false);
            }

            FortyTwo = ToUInt16(bytes, offset + 2, LittleEndian);

            if (FortyTwo != 42)
            {
                Empty();
                return (false);
            }

            OffsetToZerothIFD = ToInt32(bytes, offset + 4, LittleEndian);

            return (true);
        }

        /// <summary>
        /// Converts the bytes to a sixteen bit integer.
        /// </summary>
        /// <param name="bytes">The bytes to convert.</param>
        /// <param name="offset">The offset to begin converting.</param>
        /// <param name="is_little_endian">True when the bytes are to be interpreted as little endian or false for big endian interpretation.</param>
        /// <returns></returns>
        public static short ToInt16(byte[] bytes, int offset, bool is_little_endian)
        {
            return ((short)ToUInt16(bytes, offset, is_little_endian));
        }

        public static ushort ToUInt16(byte[] bytes, int offset, bool is_little_endian)
        {
            if (bytes == null || offset < 0 || (offset + 2) >= bytes.Length)
            {
                return (0);
            }

            if (is_little_endian)
            {
                return (ushort)(bytes[offset] + (bytes[offset + 1] << 8));
            }
            else
            {
                return (ushort)((bytes[offset] << 8) + bytes[offset + 1]);
            }
        }

        public static int ToInt32(byte[] bytes, int offset, bool is_little_endian)
        {
            if (bytes == null || offset < 0 || (offset + 4) >= bytes.Length)
            {
                return (0);
            }

            if (is_little_endian)
            {
                return (bytes[offset] + (bytes[offset + 1] << 8) + (bytes[offset + 2] << 16) + (bytes[offset + 3] << 24));
            }
            else
            {
                return ((bytes[offset] << 24) + (bytes[offset + 1] << 16) + (bytes[offset + 2] << 8) + bytes[offset + 3]);
            }
        }
    }
}
