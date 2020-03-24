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

using System;

namespace ExifRename
{
    /// <summary>
    /// This class parses just enough EXIF to get dates.
    /// </summary>
    public class Exif
    {
        public const int GpsIFDPointer = 0x8825;
        public const int MakerNote = 0x927C;
        public const int ExifIFDPointer = 0x8769;
        public const int Exif_Photo_DateTimeOriginal = 0x9003;
        public const int Exif_Photo_DateTimeDigitized = 0x9004;
        public const int Exif_Photo_SubSecTimeOriginal = 0x9291;
        public const int Exif_Photo_SubSecTimeDigitized = 0x9292;
        public const int Exif_MakerNote_Nikon3_SerialNumber = 0x1D;
        public const int Exif_MakerNote_Nikon3_SerialNO = 0xA0;
        public const int Exif_MakerNote_Nikon3_ShutterCount = 0xA7;
        public const int Exif_GPS_Time = 7;
        public const int Exif_GPS_Date = 0x1D;
        public const int Exif_Image_Make = 0x010F;
        public const int Exif_Image_Model = 0x0110;
        public const int Exif_Photo_BodySerialNumber = 0xA431;
        public const int Exif_Photo_FocalLengthIn35mmFilm = 0xA405;
        public const int Exif_Image_FocalLength = 0x920A;
        public const int Exif_GPS_Image_Direction = 17;
        public const int XPKeywords = 0x9C9E;
        public const int PixelYDimension = 0xA003;
        public const int PixelXDimension = 0xA002;
        public const int SonySequenceNumber = 0xB04A;
        public const int Exif_MakerNote_Nikon_ImageCount = 0x00A5;

        public TIFFHeader Header;
        public ImageFileDirectory IFD0;
        public ImageFileDirectory ExifIFD;
        public ImageFileDirectory GPSIFD;
        public byte[] TIFFData;

        /// <summary>
        /// The default constructor.
        /// </summary>
        public Exif()
        {
            Header = new TIFFHeader();
            IFD0 = new ImageFileDirectory();
            IFD0.InitializeIFD0TagNames();
            ExifIFD = null;
            GPSIFD = null;
            TIFFData = null;
        }

        /// <summary>
        /// This parses the given bytes as EXIF information.
        /// </summary>
        /// <param name="bytes">The bytes to parse.</param>
        /// <returns>True on success, false otherwise.</returns>
        public bool FromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 16)
            {
                return (false);
            }

            // Find the Exif signature...

            int array_index = 0;

            int exif_signature_offset = -1;
            int tiff_offset = -1;

            while (array_index < (bytes.Length - 6))
            {
                if (bytes[array_index] == 'E' &&
                    bytes[array_index + 1] == 'x' &&
                    bytes[array_index + 2] == 'i' &&
                    bytes[array_index + 3] == 'f' &&
                    bytes[array_index + 4] == 0 &&
                    bytes[array_index + 5] == 0)
                {
                    exif_signature_offset = array_index;
                    array_index = bytes.Length; // Exit the loop
                }

                array_index++;
            }

            if (exif_signature_offset < 0)
            {
                // Let's try to find the TIFF Header

                array_index = 0;

                while (array_index < (bytes.Length - 4))
                {
                    if (((bytes[array_index] == 'M' && bytes[array_index + 1] == 'M') ||
                           (bytes[array_index] == 'I' && bytes[array_index + 1] == 'I')) &&
                        bytes[array_index + 2] == 0x00 &&
                        bytes[array_index + 3] == 0x2A)
                    {
                        exif_signature_offset = array_index;
                        array_index = bytes.Length; // Exit the loop
                    }

                    array_index++;
                }

                if (exif_signature_offset < 0)
                {
                    // Can't find the TIFF signature either
                    return (false);
                }

                tiff_offset = exif_signature_offset;
            }
            else
            {
                tiff_offset = exif_signature_offset + 6;
            }

            if (Header.FromBytes(bytes, tiff_offset) == false)
            {
                return (false);
            }

            int tiff_data_length = bytes.Length - tiff_offset;

            if (tiff_data_length > Program.OneMegabyte)
            {
                tiff_data_length = Program.OneMegabyte;
            }

            TIFFData = new byte[tiff_data_length];

            Array.Copy(bytes, tiff_offset, TIFFData, 0, tiff_data_length);

            IFD0.FromBytes(TIFFData, Header.OffsetToZerothIFD, Header.LittleEndian);

            var exif_field = IFD0.GetTag(ExifIFDPointer);

            if (exif_field != null)
            {
                ExifIFD = new ImageFileDirectory();
                ExifIFD.InitializeExifTagNames();

                ExifIFD.FromBytes(TIFFData, exif_field.OffsetToValue, Header.LittleEndian);
            }

            var gps_field = IFD0.GetTag(GpsIFDPointer);

            if (gps_field != null)
            {
                GPSIFD = new ImageFileDirectory();
                GPSIFD.InitializeGPSTagNames();

                GPSIFD.FromBytes(TIFFData, gps_field.OffsetToValue, Header.LittleEndian);
            }

            return (true);
        }

        public static double Rational(int numerator, int denominator)
        {
            if (numerator == 0 || denominator == 0)
            {
                return (0.0D);
            }

            return ((double)((double)numerator / (double)denominator));
        }

        public static double ConvertDegreesMinutesSecondsCoordinateToDecimalDegrees(double degrees, double minutes, double seconds)
        {
            double decimal_degrees = degrees;

            if (decimal_degrees < 0.0D)
            {
                decimal_degrees *= (-1.0D);
            }

            decimal_degrees += (double)(minutes / 60.0D);
            decimal_degrees += (double)(seconds / 3600.0D);

            if (degrees < 0.0D)
            {
                // Maintain the sign of the original...
                decimal_degrees *= (-1.0D);
            }

            return (decimal_degrees);
        }

        public double DegreesValue(ImageFileDirectory ifd, int tag)
        {
            if (TIFFData == null || ifd == null)
            {
                return (0.0D);
            }

            InteroperabilityField field = ifd.GetTag(tag);

            if (field == null || (field.Type != 5 && field.Type != 10))
            {
                return (0.0D);
            }

            if (field.Count < 3)
            {
                return (0.0D);
            }

            int[] integer_array = new int[field.Count * 2]; // Rationals are two integers each

            int array_index = 0;

            while (array_index < integer_array.Length)
            {
                integer_array[array_index] = TIFFHeader.ToInt32(TIFFData, field.OffsetToValue + (array_index * 4), Header.LittleEndian);
                array_index++;
            }

            double degrees = Rational(integer_array[0], integer_array[1]);
            double minutes = Rational(integer_array[2], integer_array[3]);
            double seconds = Rational(integer_array[4], integer_array[5]);

            double return_value = ConvertDegreesMinutesSecondsCoordinateToDecimalDegrees(degrees, minutes, seconds);

            return (return_value);
        }

        public bool GPSTime(ImageFileDirectory ifd, int tag, out double hours, out double minutes, out double seconds)
        {
            hours = 0.0D;
            minutes = 0.0D;
            seconds = 0.0D;

            if (TIFFData == null || ifd == null)
            {
                return (false);
            }

            var field = ifd.GetTag(tag);

            if (field == null || field.Type != 5)
            {
                return (false);
            }

            if (field.Count < 3)
            {
                return (false);
            }

            int[] integer_array = new int[field.Count * 2]; // Rationals are two integers each

            int array_index = 0;

            while (array_index < integer_array.Length)
            {
                integer_array[array_index] = TIFFHeader.ToInt32(TIFFData, field.OffsetToValue + (array_index * 4), Header.LittleEndian);
                array_index++;
            }

            hours = Rational(integer_array[0], integer_array[1]);
            minutes = Rational(integer_array[2], integer_array[3]);
            seconds = Rational(integer_array[4], integer_array[5]);

            return (true);
        }

        public double DecimalValue(int tag)
        {
            double return_value = DecimalValue(IFD0, tag);

            if (return_value == 0.0D)
            {
                return_value = DecimalValue(ExifIFD, tag);
            }

            if (return_value == 0.0D)
            {
                return_value = DecimalValue(GPSIFD, tag);
            }

            return (return_value);
        }

        public double DecimalValue(ImageFileDirectory ifd, int tag, byte[] buffer)
        {
            if (buffer == null || ifd == null)
            {
                return (0.0D);
            }

            var field = ifd.GetTag(tag);

            if (field == null || field.Type != 5)
            {
                return (0.0D);
            }

            int numerator = TIFFHeader.ToInt32(buffer, field.OffsetToValue, Header.LittleEndian);
            int denominator = TIFFHeader.ToInt32(buffer, field.OffsetToValue + 4, Header.LittleEndian);

            double return_value = Rational(numerator, denominator);

            return (return_value);
        }

        public double DecimalValue(ImageFileDirectory ifd, int tag)
        {
            return (DecimalValue(ifd, tag, TIFFData));
        }

        public string StringValue(ImageFileDirectory ifd, int tag, byte[] buffer)
        {
            if (buffer == null || ifd == null)
            {
                return (string.Empty);
            }

            var field = ifd.GetTag(tag);

            if (field == null || (field.Type != 2 && field.Type != 1) || field.Count < 2)
            {
                return (string.Empty);
            }

            // Lookout for values less than 4...

            if (field.Count <= 4)
            {
                byte[] bytes = new byte[4];

                bytes[0] = (field.Count > 0) ? field.Byte0 : (byte)0;
                bytes[1] = (field.Count > 1) ? field.Byte1 : (byte)0;
                bytes[2] = (field.Count > 2) ? field.Byte2 : (byte)0;
                bytes[3] = (field.Count > 3) ? field.Byte3 : (byte)0;

                if (field.Count >= 1)
                {
                    return (System.Text.Encoding.ASCII.GetString(bytes, 0, field.Count).Trim("\0".ToCharArray()));
                }
                else
                {
                    return (string.Empty);
                }
            }

            if (field.OffsetToValue > 0 && field.Count > 1 && buffer.Length > (field.OffsetToValue + field.Count))
            {
                // last check...
                if (field.Type == 2)
                {
                    return (System.Text.Encoding.ASCII.GetString(buffer, field.OffsetToValue, field.Count).Trim("\0".ToCharArray()));
                }
                else if (field.Type == 1)
                {
                    // Could be ASCII or Unicode
                    if (buffer[field.OffsetToValue] != 0x00 && buffer[field.OffsetToValue + 1] == 0x00)
                    {
                        // Little Endian Unicode
                        return (System.Text.Encoding.Unicode.GetString(buffer, field.OffsetToValue, field.Count).Trim("\0".ToCharArray()));
                    }
                    else if (buffer[field.OffsetToValue] == 0x00 && buffer[field.OffsetToValue + 1] != 0x00)
                    {
                        // Big Endian Unicode
                        return (System.Text.Encoding.BigEndianUnicode.GetString(buffer, field.OffsetToValue, field.Count).Trim("\0".ToCharArray()));
                    }
                    else if (buffer[field.OffsetToValue] != 0x00 && buffer[field.OffsetToValue + 1] != 0x00)
                    {
                        return (System.Text.Encoding.UTF8.GetString(buffer, field.OffsetToValue, field.Count).Trim("\0".ToCharArray()));
                    }
                }
            }

            return (string.Empty);
        }

        public string StringValue(ImageFileDirectory ifd, int tag)
        {
            if (TIFFData == null || ifd == null)
            {
                return (string.Empty);
            }

            return (StringValue(ifd, tag, TIFFData));
        }

        public string StringValue(int tag)
        {
            string return_value = StringValue(IFD0, tag);

            if (string.IsNullOrEmpty(return_value) == true)
            {
                return_value = StringValue(ExifIFD, tag);
            }

            if (string.IsNullOrEmpty(return_value) == true)
            {
                return_value = StringValue(GPSIFD, tag);
            }

            return (return_value);
        }

        public int ByteValue(ImageFileDirectory ifd, int tag)
        {
            if (TIFFData == null || ifd == null)
            {
                return (0);
            }

            var field = ifd.GetTag(tag);

            if (field == null || field.Type != 1 || field.Count < 1)
            {
                return (0);
            }

            // Lookout for values less than 4...

            if (field.Count <= 4)
            {
                return (field.Byte0);
            }

            return (0);
        }

        public int IntegerValue(ImageFileDirectory ifd, int tag)
        {
            if (TIFFData == null || ifd == null)
            {
                return (0);
            }

            var field = ifd.GetTag(tag);

            if (field == null || field.Count < 1)
            {
                return (0);
            }

            if (field.Type == 3)
            {
                return (field.AsShort());
            }

            if (field.Type == 4)
            {
                return (LongValue(ifd, tag));
            }

            return (0);
        }

        public int ShortValue(ImageFileDirectory ifd, int tag)
        {
            if (TIFFData == null || ifd == null)
            {
                return (0);
            }

            var field = ifd.GetTag(tag);

            if (field == null || field.Type != 3 || field.Count < 1)
            {
                return (0);
            }

            // Lookout for values less than 4...

            if (field.Count <= 1)
            {
                return (field.AsShort());
            }

            return (0);
        }

        public int LongValue(ImageFileDirectory ifd, int tag)
        {
            if (TIFFData == null || ifd == null)
            {
                return (0);
            }

            var field = ifd.GetTag(tag);

            if (field == null || field.Type != 4 || field.Count < 1)
            {
                return (0);
            }

            // Lookout for values less than 4...

            if (field.Count <= 1)
            {
                return (field.OffsetToValue);
            }

            return (0);
        }

        public int ByteValue(int tag)
        {
            int return_value = ByteValue(IFD0, tag);

            if (return_value == 0)
            {
                return_value = ByteValue(ExifIFD, tag);
            }

            if (return_value == 0)
            {
                return_value = ByteValue(GPSIFD, tag);
            }

            return (return_value);
        }

        public string Date()
        {
            return (StringValue(Exif_Photo_DateTimeOriginal));
        }

        public static DateTime ParseDateTime(string date_string)
        {
            if (string.IsNullOrEmpty(date_string) == true)
            {
                return (DateTime.MinValue);
            }

            //           11111111112
            // 012345678901234567890
            // 2007:12:06 17:32:03
            // 2010-07-01T07:09:13.5-04:00

            if (date_string.Length < 18)
            {
                return (DateTime.MinValue);
            }

            if ((date_string[4] != ':' && date_string[4] != '-') ||
                (date_string[7] != ':' && date_string[7] != '-') ||
                date_string[13] != ':' ||
                date_string[16] != ':')
            {
                return (DateTime.MinValue);
            }

            int year = 0;
            int month = 0;
            int day = 0;
            int hour = 0;
            int minute = 0;
            int second = 0;

            string string_to_parse = date_string.Substring(0, 4);

            if (Int32.TryParse(string_to_parse, out year) == false)
            {
                return (DateTime.MinValue);
            }

            string_to_parse = date_string.Substring(5, 2);

            if (Int32.TryParse(string_to_parse, out month) == false)
            {
                return (DateTime.MinValue);
            }

            string_to_parse = date_string.Substring(8, 2);

            if (Int32.TryParse(string_to_parse, out day) == false)
            {
                return (DateTime.MinValue);
            }

            string_to_parse = date_string.Substring(11, 2);

            if (Int32.TryParse(string_to_parse, out hour) == false)
            {
                return (DateTime.MinValue);
            }

            string_to_parse = date_string.Substring(14, 2);

            if (Int32.TryParse(string_to_parse, out minute) == false)
            {
                return (DateTime.MinValue);
            }

            string_to_parse = date_string.Substring(17, 2);

            if (Int32.TryParse(string_to_parse, out second) == false)
            {
                return (DateTime.MinValue);
            }

            return (new DateTime(year, month, day, hour, minute, second));
        }

        public DateTime Date(ImageFileDirectory ifd, int tag)
        {
            string date_string = StringValue(ifd, tag);

            if (string.IsNullOrEmpty(date_string) == true)
            {
                return (DateTime.MinValue);
            }

            if (date_string.Length != 10)
            {
                return (DateTime.MinValue);
            }

            if (date_string[4] != ':' || date_string[7] != ':')
            {
                return (DateTime.MinValue);
            }

            int year = 0;
            int month = 0;
            int day = 0;

            string string_to_parse = date_string.Substring(0, 4);

            if (Int32.TryParse(string_to_parse, out year) == false)
            {
                return (DateTime.MinValue);
            }

            string_to_parse = date_string.Substring(5, 2);

            if (Int32.TryParse(string_to_parse, out month) == false)
            {
                return (DateTime.MinValue);
            }

            string_to_parse = date_string.Substring(8, 2);

            if (Int32.TryParse(string_to_parse, out day) == false)
            {
                return (DateTime.MinValue);
            }

            return (new DateTime(year, month, day));
        }

        public DateTime GPSDate()
        {
            var gps_date = Date(GPSIFD, Exif_GPS_Date);

            if (gps_date.Year == 1)
            {
                // Try to fixup the year, Apple really sucks at this, they like to set only the GPS Time not GPS Date

                DateTime taken = Taken();

                if (taken.Year > 1601)
                {
                    gps_date = new DateTime(taken.Year, taken.Month, taken.Day);
                }
            }

            return (gps_date);
        }

        public TimeSpan GPSTime()
        {
            var return_value = new TimeSpan();

            if (GPSIFD == null)
            {
                return (return_value);
            }

            double hours = 0.0D;
            double minutes = 0.0D;
            double seconds = 0.0D;

            if (GPSTime(GPSIFD, Exif_GPS_Time, out hours, out minutes, out seconds) == true)
            {
                double partial_seconds = seconds - System.Math.Floor(seconds);

                return_value = new TimeSpan(0, (int)hours, (int)minutes, (int)seconds, (int)(partial_seconds * 1000.0D));
            }

            return (return_value);
        }

        #region IPhotoInformation implementation
        // begin IPhotoInformation methods

        public string Make()
        {
            return (StringValue(Exif_Image_Make));
        }

        public string Model()
        {
            return (StringValue(Exif_Image_Model));
        }

        public string SerialNumber()
        {
            string return_value = StringValue(Exif_Photo_BodySerialNumber);

            if (string.IsNullOrEmpty(return_value) == false)
            {
                return (return_value);
            }

            // See if we can pull the data from a makernote.

            string make = Make().ToLowerInvariant();

            if (make.Contains("nikon") == true && ExifIFD != null)
            {
                // Find the maker note...

                InteroperabilityField maker_note_field = ExifIFD.GetTag(MakerNote);

                if (maker_note_field != null)
                {
                    byte[] maker_note_buffer = new byte[maker_note_field.Count - 10];

                    Array.Copy(TIFFData, maker_note_field.OffsetToValue + 10, maker_note_buffer, 0, maker_note_field.Count - 10);

                    TIFFHeader th = new TIFFHeader();

                    if (th.FromBytes(maker_note_buffer, 0) == true)
                    {
                        ImageFileDirectory maker_note = new ImageFileDirectory();

                        if (maker_note.FromBytes(maker_note_buffer, th.OffsetToZerothIFD, th.LittleEndian) == true)
                        {
                            // Now read from the Maker Note Buffer!

                            return_value = StringValue(maker_note, Exif_MakerNote_Nikon3_SerialNumber, maker_note_buffer);

                            if (string.IsNullOrEmpty(return_value) == true)
                            {
                                return_value = StringValue(maker_note, Exif_MakerNote_Nikon3_SerialNO, maker_note_buffer);

                                if (return_value.StartsWith("NO=") == true)
                                {
                                    return (return_value.Substring(3, return_value.Length - 3).Trim());
                                }
                            }
                        }
                    }

                    return (return_value);
                }
            }

            return (string.Empty);
        }

        public DateTime GPSDateTime()
        {
            DateTime date_part = GPSDate();
            TimeSpan time_part = GPSTime();

            return (new DateTime(date_part.Year, date_part.Month, date_part.Day, time_part.Hours, time_part.Minutes, time_part.Seconds, time_part.Milliseconds, DateTimeKind.Utc));
        }

        public double FocalLength()
        {
            // Try to get the focal length normalized to 35mm first
            double return_value = (double)IntegerValue(ExifIFD, Exif_Photo_FocalLengthIn35mmFilm);

            if (return_value != 0.0D)
            {
                return (return_value);
            }

            return (DecimalValue(Exif_Image_FocalLength));
        }

        public double CompassDirection()
        {
            if (GPSIFD == null)
            {
                return (0.0D);
            }

            InteroperabilityField field = GPSIFD.GetTag(Exif_GPS_Image_Direction);

            if (field == null || field.Type != 5)
            {
                return (0.0D);
            }

            if (field.Count == 3)
            {
                double degrees = 0.0D;
                double minutes = 0.0D;
                double seconds = 0.0D;

                GPSTime(GPSIFD, Exif_GPS_Image_Direction, out degrees, out minutes, out seconds);

                double return_value = ConvertDegreesMinutesSecondsCoordinateToDecimalDegrees(degrees, minutes, seconds);

                if (return_value >= 360.0D)
                {
                    return_value = 0.0D;
                }

                return (return_value);
            }

            return (DecimalValue(GPSIFD, Exif_GPS_Image_Direction));
        }

        public string[] PeopleNames()
        {
            return (new string[0]);
        }

        public string[] Tags()
        {
            string return_value = StringValue(XPKeywords);
            return (return_value.Split(','));
        }

        /// <summary>
        /// This method finds the date the photograph was taken.
        /// It first attempts to get EXIF tag 0x9003 (DateTimeOriginal). If that is not
        /// present, it will then attempt to use the date time in tag 0x9004 (DateTimeDigitized).
        /// If neither tag is present, it will return DateTime.MinValue. If a sub-second value
        /// is present in EXIF, the appropriate interval will added to the return value. Those
        /// tags are 0x9291 (SubSecTimeOriginal) and 0x9292 (SubSecTimeDigitized).
        /// </summary>
        /// <returns>The date the photograph was taken.</returns>
        public DateTime Taken()
        {
            // DateTimeOriginal
            int tag_id = Exif_Photo_DateTimeOriginal;

            string value = StringValue(ExifIFD, tag_id);

            if (string.IsNullOrEmpty(value) == true)
            {
                tag_id = Exif_Photo_DateTimeDigitized;
                value = StringValue(ExifIFD, tag_id);
            }

            if (string.IsNullOrEmpty(value) == true)
            {
                return (DateTime.MinValue);
            }

            // Now parse the string...

            var return_value = ParseDateTime(value);

            string sub_seconds = string.Empty;

            if (tag_id == Exif_Photo_DateTimeOriginal)
            {
                sub_seconds = StringValue(ExifIFD, Exif_Photo_SubSecTimeOriginal);
            }
            else if (tag_id == Exif_Photo_DateTimeDigitized)
            {
                sub_seconds = StringValue(ExifIFD, Exif_Photo_SubSecTimeDigitized);
            }

            if (string.IsNullOrEmpty(sub_seconds) == false)
            {
                string double_number_string = "0." + sub_seconds;

                double partial_second = 0.0D;

                if (double.TryParse(double_number_string, out partial_second) == true)
                {
                    if (partial_second > 0.0D)
                    {
                        return_value = return_value.AddSeconds(partial_second);
                    }
                }
            }

            return (return_value);
        }

        /// <summary>
        /// Retrieves the height of the image.
        /// </summary>
        /// <returns>The height of the image.</returns>
        public int Height()
        {
            return (IntegerValue(ExifIFD, PixelYDimension));
        }

        /// <summary>
        /// Retrieves the width of the image.
        /// </summary>
        /// <returns>The width of the image.</returns>
        public int Width()
        {
            return (IntegerValue(ExifIFD, PixelXDimension));
        }

        public int SequenceNumber()
        {
            // See if we can pull the data from a makernote.

            string make = Make().ToLowerInvariant();

            if (make.Contains("sony") == true && ExifIFD != null)
            {
                // Find the maker note...

                var maker_note_field = ExifIFD.GetTag(MakerNote);

                if (maker_note_field != null)
                {
                    byte[] maker_note_buffer = new byte[maker_note_field.Count - 12];

                    Array.Copy(TIFFData, maker_note_field.OffsetToValue + 12, maker_note_buffer, 0, maker_note_field.Count - 12);

                    var tiff_header = new TIFFHeader();

                    if (tiff_header.FromBytes(maker_note_buffer, 0) == true)
                    {
                        var maker_note = new ImageFileDirectory();

                        if (maker_note.FromBytes(maker_note_buffer, tiff_header.OffsetToZerothIFD, tiff_header.LittleEndian) == true)
                        {
                            return (IntegerValue(maker_note, SonySequenceNumber));
                        }
                    }
                }
            }
            else if (make.Contains("nikon") == true && ExifIFD != null)
            {
                // Find the maker note...

                var maker_note_field = ExifIFD.GetTag(MakerNote);

                if (maker_note_field != null)
                {
                    byte[] maker_note_buffer = new byte[maker_note_field.Count - 10];

                    Array.Copy(TIFFData, maker_note_field.OffsetToValue + 10, maker_note_buffer, 0, maker_note_field.Count - 10);

                    var tiff_header = new TIFFHeader();

                    if (tiff_header.FromBytes(maker_note_buffer, 0) == true)
                    {
                        var maker_note = new ImageFileDirectory();

                        if (maker_note.FromBytes(maker_note_buffer, tiff_header.OffsetToZerothIFD, tiff_header.LittleEndian) == true)
                        {
                            return (IntegerValue(maker_note, Exif_MakerNote_Nikon_ImageCount));
                        }
                    }
                }
            }

            return (0);
        }

        /// <summary>
        /// Returns the number of times the shutter has opened on the camera.
        /// </summary>
        /// <returns>The number of times the camera's shutter has opened.</returns>
        public int ShutterCount()
        {
            // See if we can pull the data from a makernote.

            string make = Make().ToLowerInvariant();

            if (make.Contains("nikon") == true && ExifIFD != null)
            {
                // Find the maker note...

                var maker_note_field = ExifIFD.GetTag(MakerNote);

                if (maker_note_field != null)
                {
                    byte[] maker_note_buffer = new byte[maker_note_field.Count - 10];

                    Array.Copy(TIFFData, maker_note_field.OffsetToValue + 10, maker_note_buffer, 0, maker_note_field.Count - 10);

                    var tiff_header = new TIFFHeader();

                    if (tiff_header.FromBytes(maker_note_buffer, 0) == true)
                    {
                        var maker_note = new ImageFileDirectory();

                        if (maker_note.FromBytes(maker_note_buffer, tiff_header.OffsetToZerothIFD, tiff_header.LittleEndian) == true)
                        {
                            return (IntegerValue(maker_note, Exif_MakerNote_Nikon3_ShutterCount));
                        }
                    }
                }
            }

            return (0);
        }

        // end IPhotoInformation methods
        #endregion
    }
}
