// Adapted from https://github.com/mrdoob/three.js/blob/dev/examples/jsm/loaders/RGBELoader.js
// & http://www.graphics.cornell.edu/~bjw/rgbe.html, which it is based on.

using UnityEngine;
using System;
using System.IO;

namespace Superla.RadianceHDR
{
    public class RadianceHDRTexture
    {
        public RGBEHeader header;
        public Color[] colorData;

        public Texture2D texture;
        private string errorCode = string.Empty;

        public RadianceHDRTexture(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                header = new RGBEHeader();
                RGBEReturnCode result = header.ReadHeader(stream);
                if (result != RGBEReturnCode.RGBE_RETURN_SUCCESS)
                {
                    Debug.LogError(errorCode);
                    return;
                }

                result = ReadPixelsRLE(stream);
                if (result != RGBEReturnCode.RGBE_RETURN_SUCCESS)
                {
                    Debug.LogError(errorCode);
                    return;
                }
            }

            texture = new Texture2D(header.width, header.height,
                UnityEngine.Experimental.Rendering.DefaultFormat.HDR,
                UnityEngine.Experimental.Rendering.TextureCreationFlags.None
            );

            texture.SetPixels(colorData);
            texture.Apply(false);
        }

        private static Color RGBEBytetoColor(byte[] rgbeByte)
        {
            Color pixel = Color.black;
            float e = rgbeByte[3];
            if (e > 0)
            {
                double f = Math.Pow(2.0, e - 128.0f) / 255.0f;
                pixel.r = (float)(rgbeByte[0] * f);
                pixel.g = (float)(rgbeByte[1] * f);
                pixel.b = (float)(rgbeByte[2] * f);
            }

            return pixel;
        }

        // Adapted from http://answers.unity.com/answers/1761722/view.html
        private static void FlipTexture(Color[] colorData, int width, int height)
        {
            for (int j = 0; j < height; j++)
            {
                int rowStart = 0;
                int rowEnd = width - 1;

                while (rowStart < rowEnd)
                {
                    Color hold = colorData[(j * width) + (rowStart)];
                    colorData[(j * width) + (rowStart)] = colorData[(j * width) + (rowEnd)];
                    colorData[(j * width) + (rowEnd)] = hold;
                    rowStart++;
                    rowEnd--;
                }
            }
        }

        private RGBEReturnCode ReadPixels(Stream stream, int numPixels)
        {
            using (var reader = new BinaryReader(stream, System.Text.Encoding.Default, true))
            {
                colorData = new Color[numPixels];

                while (numPixels-- > 0)
                {
                    byte[] rgbe = reader.ReadBytes(4);
                    colorData[numPixels] = RGBEBytetoColor(rgbe);
                }

                FlipTexture(colorData, header.width, header.height);
            }

            return RGBEReturnCode.RGBE_RETURN_SUCCESS;
        }


        private RGBEReturnCode ReadPixelsRLE(Stream stream)
        {
            int scanlineWidth = header.width;
            int numScanlines = header.height;

            Color[] data = new Color[scanlineWidth * numScanlines];

            if ((scanlineWidth < 8) || (scanlineWidth > 0x7fff))
            {
                // Run length encoding is not allowed, so read flat
                return ReadPixels(stream, scanlineWidth * numScanlines);
            }

            using (var reader = new BinaryReader(stream, System.Text.Encoding.Default, true))
            {
                // RLE test, peek first bytes
                var rgbe = reader.ReadBytes(4);
                reader.BaseStream.Position -= 4;

                if (rgbe == null)
                {
                    return RGBEError.LogError(RGBEErrorCode.READ_ERROR);
                }

                if ((rgbe[0] != 2) || (rgbe[1] != 2) || ((rgbe[2] & 0x80) != 0))
                {
                    // file is not run length encoded
                    return ReadPixels(stream, scanlineWidth * numScanlines);
                }

                int ptr_end = 4 * scanlineWidth;
                byte[] scanlineBuffer = new byte[ptr_end];

                long datalength = stream.Length - stream.Position;

                colorData = new Color[scanlineWidth * numScanlines];

                while (numScanlines > 0 && stream.Position < stream.Length)
                {
                    if (stream.Position + 4 > datalength)
                    {
                        return RGBEError.LogError(RGBEErrorCode.READ_ERROR);
                    }

                    rgbe = reader.ReadBytes(4);

                    if ((((int)rgbe[2]) << 8 | rgbe[3]) != scanlineWidth)
                    {
                        return RGBEError.LogError(RGBEErrorCode.FORMAT_ERROR, "wrong scanline width!");
                    }

                    int ptr = 0;

                    // Read each of the four channels for the scanline into the buffer
                    // First red, then green, then blue, then exponent
                    while (ptr < ptr_end && stream.Position < stream.Length)
                    {

                        byte count = reader.ReadByte();
                        bool isEncodedRun = count > 128;
                        if (isEncodedRun)
                        {
                            count -= 128;
                        }

                        if ((count == 0) || (ptr + count > ptr_end))
                        {
                            return RGBEError.LogError(RGBEErrorCode.FORMAT_ERROR, "Bad scanline data");
                        }

                        if (isEncodedRun)
                        {
                            // A run of the same value
                            byte value = reader.ReadByte();

                            for (int i = 0; i < count; i++)
                            {
                                scanlineBuffer[ptr++] = value;
                            }
                        }
                        else
                        {
                            // a literal-run
                            byte[] run = reader.ReadBytes(count);
                            //ptr += count;
                            for (int i = 0; i < count; i++)
                            {
                                scanlineBuffer[ptr++] = run[i];
                            }
                        }
                    }

                    // now convert data from buffer into rgba
                    for (int i = 0; i < scanlineWidth; i++)
                    {
                        rgbe[0] = scanlineBuffer[i];
                        rgbe[1] = scanlineBuffer[i + scanlineWidth];
                        rgbe[2] = scanlineBuffer[i + 2 * scanlineWidth];
                        rgbe[3] = scanlineBuffer[i + 3 * scanlineWidth];
                        Color pixel = RGBEBytetoColor(rgbe);

                        int row = (header.height - numScanlines);
                        colorData[(scanlineWidth * (numScanlines - 1)) + i] = pixel;
                    }

                    numScanlines--;
                }
            }

            return RGBEReturnCode.RGBE_RETURN_SUCCESS;
        }
    }
}
