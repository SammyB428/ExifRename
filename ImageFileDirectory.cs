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

using System.Collections.Generic;

namespace ExifRename
{
    /// <summary>
    /// IFD by any other name...
    /// </summary>
    public class ImageFileDirectory
    {
        /// <summary>
        /// Holds the human-friendly names of the tag values.
        /// </summary>
        public Dictionary<int, string> TagNames;

        /// <summary>
        /// The fields found in this IFD.
        /// </summary>
        public InteroperabilityField[] Fields;

        /// <summary>
        /// The offset to the next IFD.
        /// </summary>
        public int NextOffset;

        /// <summary>
        /// The default constructor.
        /// </summary>
        public ImageFileDirectory()
        {
            TagNames = null;
            Empty();
        }

        /// <summary>
        /// Initializes the names of the tags for the EXIF region.
        /// </summary>
        public void InitializeExifTagNames()
        {
            TagNames = new Dictionary<int, string>();

            TagNames.Add(0x829A, "ExposureTime");
            TagNames.Add(0x829D, "FNumber");
            TagNames.Add(0x8822, "ExposureProgram");
            TagNames.Add(0x8827, "ISO");
            TagNames.Add(0x9000, "ExifVersion");
            TagNames.Add(0x9003, "DateTimeOriginal");
            TagNames.Add(0x9004, "DateTimeDigitized");
            TagNames.Add(0x9101, "ComponentsConfiguration");
            TagNames.Add(0x9102, "CompressedBitsPerPixel");
            TagNames.Add(0x9202, "ApertureValue");
            TagNames.Add(0x9204, "ExposureBiasValue");
            TagNames.Add(0x9205, "MaxApertureValue");
            TagNames.Add(0x9206, "SubjectDistance");
            TagNames.Add(0x9207, "MeteringMode");
            TagNames.Add(0x9208, "LightSource");
            TagNames.Add(0x9209, "Flash");
            TagNames.Add(0x920A, "FocalLength");
            TagNames.Add(0x927C, "MakerNote");
            TagNames.Add(0x9286, "UserComment");
            TagNames.Add(0x9290, "SubSecTime");
            TagNames.Add(0x9291, "SubSecTimeOriginal");
            TagNames.Add(0x9292, "SubSecTimeDigitized");
            TagNames.Add(0xA000, "FlashpixVersion");
            TagNames.Add(0xA001, "ColorSpace");
            TagNames.Add(0xA002, "PixelXDimension");
            TagNames.Add(0xA003, "PixelYDimension");
            TagNames.Add(0xA005, "InteropOffset");
            TagNames.Add(0xA217, "SensingMethod");
            TagNames.Add(0xA300, "FileSource");
            TagNames.Add(0xA301, "SceneType");
            TagNames.Add(0xA302, "CFAPattern");
            TagNames.Add(0xA401, "CustomRendered");
            TagNames.Add(0xA402, "ExposureMode");
            TagNames.Add(0xA403, "WhiteBalance");
            TagNames.Add(0xA404, "DigitalZoomRatio");
            TagNames.Add(0xA405, "FocalLengthIn35mmFilm");
            TagNames.Add(0xA406, "SceneCaptureType");
            TagNames.Add(0xA407, "GainControl");
            TagNames.Add(0xA408, "Contrast");
            TagNames.Add(0xA409, "Saturation");
            TagNames.Add(0xA40A, "Sharpness");
            TagNames.Add(0xA40B, "DeviceSettingDescription");
            TagNames.Add(0xA40C, "SubjectDistanceRange");
            TagNames.Add(0xA431, "BodySerialNumber");
            TagNames.Add(0xEA1C, "Padding");
            TagNames.Add(0xEA1D, "OffsetSchema");
        }

        /// <summary>
        /// Initializes the names of the tags in the GPS region
        /// </summary>
        public void InitializeGPSTagNames()
        {
            TagNames = new Dictionary<int, string>();

            TagNames.Add(0, "GPSVersionID");
            TagNames.Add(1, "GPSLatitudeRef");
            TagNames.Add(2, "GPSLatitude");
            TagNames.Add(3, "GPSLongitudeRef");
            TagNames.Add(4, "GPSLongitude");
            TagNames.Add(5, "GPSAltitudeRef");
            TagNames.Add(6, "GPSAltitude");
            TagNames.Add(7, "GPSTimeStamp");
            TagNames.Add(8, "GPSSatellites");
            TagNames.Add(9, "GPSStatus");
            TagNames.Add(10, "GPSMeasureMode");
            TagNames.Add(0x10, "GPSImgDirectionRef");
            TagNames.Add(0x11, "GPSImgDirection");
            TagNames.Add(0x12, "GPSMapDatum");
            TagNames.Add(0x1D, "GPSDateStamp");
        }

        /// <summary>
        /// Initializes the names of the tags in the IFD0 region.
        /// </summary>
        public void InitializeIFD0TagNames()
        {
            TagNames = new Dictionary<int, string>();

            TagNames.Add(0x0100, "ImageWidth");
            TagNames.Add(0x0101, "ImageLength");
            TagNames.Add(0x0102, "BitsPerSample");
            TagNames.Add(0x010E, "ImageDescription");
            TagNames.Add(0x010F, "Make");
            TagNames.Add(0x0110, "Model");
            TagNames.Add(0x0112, "Orientation");
            TagNames.Add(0x011A, "XResolution");
            TagNames.Add(0x011B, "YResolution");
            TagNames.Add(0x0128, "ResolutionUnit");
            TagNames.Add(0x0131, "Software");
            TagNames.Add(0x0132, "DateTime");
            TagNames.Add(0x013B, "Artist");
            TagNames.Add(0x0213, "YCbCrPositioning");
            TagNames.Add(0x4746, "Rating");
            TagNames.Add(0x4747, "XP_DIP_XML");
            TagNames.Add(0x4748, "HDViewInfo");
            TagNames.Add(0x4749, "RatingPercent");
            TagNames.Add(0x8769, "ExifIFDPointer");
            TagNames.Add(0x8298, "Copyright");
            TagNames.Add(0x8825, "GpsIFDPointer");
            TagNames.Add(0x9216, "TIFF/EPStandardID");
            TagNames.Add(0x9C9B, "XPTitle");
            TagNames.Add(0x9C9C, "XPComment");
            TagNames.Add(0x9C9D, "XPAuthor");
            TagNames.Add(0x9C9E, "XPKeywords");
            TagNames.Add(0x9C9F, "XPSubject");
            TagNames.Add(0xEA1C, "Padding");
        }

        /// <summary>
        /// Returns the object to an initial state. Doesn't touch the tag name maps.
        /// </summary>
        public void Empty()
        {
            Fields = null;
            NextOffset = 0;
        }

        /// <summary>
        /// Gets the IFD of the desired tag/
        /// </summary>
        /// <param name="desired_tag">The identifier of the desired tag.</param>
        /// <returns>An InteroperabilityField if found, null if the desired tag was not found.</returns>
        public InteroperabilityField GetTag(int desired_tag)
        {
            if (Fields == null)
            {
                return (null);
            }

            int array_index = 0;

            while (array_index < Fields.Length)
            {
                if (Fields[array_index].Tag == desired_tag)
                {
                    return (Fields[array_index]);
                }

                array_index++;
            }

            return (null);
        }

        public bool FromBytes(byte[] buffer, int offset, bool little_endian)
        {
            Empty();

            if (buffer == null || offset < 0 || buffer.Length < 6)
            {
                return (false);
            }

            int number_of_entries = TIFFHeader.ToUInt16(buffer, offset, little_endian);

            if (number_of_entries > 5460) // 5461 entries in 64KB buffer
            {
                return (false);
            }

            Fields = new InteroperabilityField[number_of_entries];

            string tag_name = string.Empty;

            int entry_index = 0;

            while (entry_index < number_of_entries)
            {
                Fields[entry_index] = InteroperabilityField.FromBytes(buffer, offset + 2 + (entry_index * 12), little_endian);

                if (TagNames != null && TagNames.TryGetValue(Fields[entry_index].Tag, out tag_name) == true)
                {
                    Fields[entry_index].TagName = tag_name;
                }

                entry_index++;
            }

            NextOffset = TIFFHeader.ToInt32(buffer, offset + 2 + (number_of_entries * 12), little_endian);

            return (true);
        }
    }
}
