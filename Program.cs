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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ExifRename
{
    class Program
    {
        public const int OneMegabyte = 1048576;
        public const int BufferSize = 20 * OneMegabyte;

        static void ProcessFile( string this_file_to_process, int file_number )
        {
            int buffer_size = BufferSize;

            var file_information = new FileInfo(this_file_to_process);

            if ( file_information.Length < buffer_size )
            {
                buffer_size = (int) file_information.Length;
            }

            byte[] buffer = new byte[buffer_size];

            int number_of_bytes_read = 0;

            try
            {
                using (var input_stream = File.OpenRead(this_file_to_process))
                {
                    number_of_bytes_read = input_stream.Read(buffer, 0, buffer.Length);
                }
            }
            catch (Exception)
            {
                number_of_bytes_read = 0;
            }

            if (number_of_bytes_read > 0)
            {
                var exif_data = new Exif();

                string filename_extension = System.IO.Path.GetExtension(this_file_to_process);

                if (exif_data.FromBytes(buffer) == true)
                {
                    int final_field = exif_data.ShutterCount();

                    if (final_field == 0)
                    {
                        final_field = exif_data.SequenceNumber();
                    }

                    if (final_field == 0)
                    {
                        final_field = file_number;
                    }

                    var taken = exif_data.Taken();

                    string new_filename = string.Format("{0}_{1}{2}", taken.ToString("yyyyMMdd_HHmmss", System.Globalization.CultureInfo.InvariantCulture), final_field, filename_extension);
                    string just_filename = System.IO.Path.GetFileName(this_file_to_process);

                    if (string.Compare(new_filename, just_filename) != 0)
                    {
                        string rename_to = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this_file_to_process), new_filename);
                        System.IO.File.Move(this_file_to_process, rename_to);
                    }
                }
            }
        }

        static void ProcessFiles(string[] filenames)
        {
#if false
            // Single threaded mode
            int loop_index = 0;

            while( loop_index < filenames.Length )
            {
                ProcessFile(filenames[loop_index], loop_index);
                loop_index++;
            }
#else
            // Multithreaded mode

            Parallel.For(0, filenames.Length,
                loop_index => { ProcessFile(filenames[loop_index], loop_index); });

#endif
        }

        static void Main(string[] args)
        {
            var files_to_process = new List<string>();

            if (args.Length > 0)
            {
                foreach (string argument in args)
                {
                    if (Directory.Exists(argument) == true)
                    {
                        string directory_full_path = Path.GetFullPath(argument);

                        files_to_process.AddRange(Directory.EnumerateFiles(directory_full_path, "*", SearchOption.AllDirectories));
                    }
                    else if (File.Exists(argument) == true)
                    {
                        files_to_process.Add(Path.GetFullPath(argument));
                    }
                }

                ProcessFiles(files_to_process.ToArray());
            }
        }
    }
}
