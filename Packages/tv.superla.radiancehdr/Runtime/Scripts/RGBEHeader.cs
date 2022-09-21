// Copyright 2022 Superla.tv
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Superla.RadianceHDR
{
    [System.Serializable]
    public class RGBEHeader
    {
        public const int RGBE_VALID_PROGRAMTYPE = 0x01;
        public const int RGBE_VALID_FORMAT = 0x02;
        public const int RGBE_VALID_DIMENSIONS = 0x04;

        const char NEWLINE = '\n';

        public int valid;
        public string rawHeader;
        public string comments;
        public string programType;
        public string format;
        public float gamma;
        public float exposure;
        public int width;
        public int height;

        public RGBEHeader()
        {
            valid = 0;
            rawHeader = string.Empty;
            comments = string.Empty;
            programType = "RGBE";
            format = string.Empty;
            gamma = 1.0f;
            exposure = 1.0f;
            width = 0;
            height = 0;
        }

        public RGBEReturnCode ReadHeader(Stream stream)
        {
            const string MAGIC_TOKEN_RE = @"^#\?(\S+)";
            const string GAMMA_RE = @"^\s*GAMMA\s*=\s*(\d+(\.\d+)?)\s*$";
            const string EXPOSURE_RE = @"^\s*EXPOSURE\s*=\s*(\d+(\.\d+)?)\s*$";
            const string FORMAT_RE = @"^\s*FORMAT=(\S+)\s*$";
            const string DIMENSIONS_RE = @"^\s*\-Y\s+(\d+)\s+\+X\s+(\d+)\s*$";

            Match match;
            string line;

            using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8, true, 1024, true))
            {
                line = reader.ReadLine();
                if (stream.Position >= stream.Length && line == null)
                {
                    return RGBEError.LogError(RGBEErrorCode.READ_ERROR, "no header found!");
                }

                match = Regex.Match(line, MAGIC_TOKEN_RE);
                if (!match.Success)
                {
                    return RGBEError.LogError(RGBEErrorCode.FORMAT_ERROR, "bad initial token!");
                }

                valid |= RGBE_VALID_PROGRAMTYPE;
                programType = match.Groups[1].Value;
                rawHeader += line + '\n';

                while (true)
                {
                    line = reader.ReadLine();
                    if (line == null) break;
                    rawHeader += line + '\n';

                    if (line.Length > 0 && '#' == line[0])
                    {
                        comments += line + '\n';
                        continue; // comment line
                    }

                    match = Regex.Match(line, GAMMA_RE);
                    if (match.Success)
                    {
                        gamma = float.Parse(match.Groups[1].Value);
                    }

                    match = Regex.Match(line, EXPOSURE_RE);
                    if (match.Success)
                    {
                        exposure = float.Parse(match.Groups[1].Value);
                    }

                    match = Regex.Match(line, FORMAT_RE);
                    if (match.Success)
                    {
                        valid |= RGBE_VALID_FORMAT;
                        format = match.Groups[1].Value;
                    }

                    match = Regex.Match(line, DIMENSIONS_RE);
                    if (match.Success)
                    {
                        valid |= RGBE_VALID_DIMENSIONS;
                        height = int.Parse(match.Groups[1].Value);
                        width = int.Parse(match.Groups[2].Value);
                    }

                    if ((valid & RGBE_VALID_FORMAT) > 0 && (valid & RGBE_VALID_DIMENSIONS) > 0) break;
                }

                if (!((valid & RGBE_VALID_FORMAT) > 0))
                {
                    return RGBEError.LogError(RGBEErrorCode.FORMAT_ERROR, "missing format specifier");
                }

                if (!((valid & RGBE_VALID_DIMENSIONS) > 0))
                {
                    return RGBEError.LogError(RGBEErrorCode.FORMAT_ERROR, "missing image size specifier");
                }

                // Set the stream position to the end of the header, as 
                stream.Position = rawHeader.Length;
            }
            return RGBEReturnCode.RGBE_RETURN_SUCCESS;
        }
    }

}
