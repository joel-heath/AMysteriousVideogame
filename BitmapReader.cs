/// COURTESY OF CHATGPT ALONE. MANY THANKS TO CHATGPT. ///
namespace AMysteriousVideogame;

public static class BitmapReader
{
    /// <summary>
    /// Reads a 1-bit BMP file from the specified file path and returns a 2D array [width, height] representing the bitmap, where each element indicates the pixel value.
    /// </summary>
    /// <param name="filePath">The path to the BMP file to be read.</param>
    /// <returns>A 2D array [x, y] where each element represents a pixel value of the bitmap.</returns>
    /// <exception cref="FormatException">Thrown when the bit depth of the BMP file is not 1-bit.</exception>
    public static bool[,] Read(string filePath)
    {
        bool[,] map;

        using (Stream stream = File.OpenRead(filePath))
        using (BinaryReader reader = new(stream))
        {
            // BMP Header (14 bytes)
            reader.BaseStream.Seek(18, SeekOrigin.Begin); // Go to byte 18 where the width is stored

            // Read width and height from the BMP file (int32 each)
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();

            map = new bool[width, height];

            // Read bits per pixel (2 bytes) at offset 28
            reader.BaseStream.Seek(28, SeekOrigin.Begin);
            short bitsPerPixel = reader.ReadInt16();
            if (bitsPerPixel != 1)
            {
                throw new FormatException($"Unexpected bit depth. Expected 1-bit but received {bitsPerPixel}-bit.");
            }

            // Skip the colour palette (since we don't care about it)
            reader.BaseStream.Seek(10, SeekOrigin.Begin);
            int pixelArrayOffset = reader.ReadInt32();

            // Seek to the start of the pixel array
            reader.BaseStream.Seek(pixelArrayOffset, SeekOrigin.Begin);

            // Calculate the number of bytes per row. Each byte stores 8 pixels.
            int rowSizeInBytes = (width + 7) / 8; // Divide by 8 and round up for partial bytes
            int padding = (4 - rowSizeInBytes % 4) % 4; // Row padding to align to a multiple of 4 bytes

            // BMP pixel data is stored bottom-up, so we need to read from the bottom row first
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < rowSizeInBytes; x++)
                {
                    byte pixelByte = reader.ReadByte(); // Read one byte (8 pixels)

                    // Process each bit in the byte (from left to right, most significant bit first)
                    for (int bit = 7; bit >= 0; bit--)
                    {
                        // Check if the current bit represents a pixel within the image width
                        if (x * 8 + (7 - bit) < width)
                        {
                            // Extract the bit (0 or 1) for the pixel
                            int pixelValue = (pixelByte >> bit) & 1;

                            map[x * 8 + (7 - bit), y] = Convert.ToBoolean(pixelValue);
                        }
                    }
                }
                // Skip padding bytes at the end of each row
                reader.BaseStream.Seek(padding, SeekOrigin.Current);
            }
        }

        return map;
    }
}
