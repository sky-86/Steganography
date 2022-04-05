using System.IO;
using System.Text;
using Xamarin.Forms;
using System.Collections.Generic;

// Creates a bitmap from scratch
public class BmpMaker
{
    // PROPERTIES
    const int headerSize = 54;
    readonly byte[] buffer;

    // CONSTRUCTOR
    public BmpMaker(int width, int height)
    {
        Width = width;
        Height = height;

        int numPixels = Width * Height;
        int numPixelBytes = 4 * numPixels;
        int fileSize = headerSize + numPixelBytes;
        buffer = new byte[fileSize];

        // Write headers in MemoryStream and hence the buffer.
        using (MemoryStream memoryStream = new MemoryStream(buffer))
        {
            using (BinaryWriter writer = new BinaryWriter(memoryStream, Encoding.UTF8))
            {
                // Construct BMP header (14 bytes).
                writer.Write(new char[] { 'B', 'M' });  // Signature
                writer.Write(fileSize);                 // File size
                writer.Write((short)0);                 // Reserved
                writer.Write((short)0);                 // Reserved
                writer.Write(headerSize);               // Offset to pixels

                // Construct BitmapInfoHeader (40 bytes).
                writer.Write(40);                       // Header size
                writer.Write(Width);                    // Pixel width
                writer.Write(Height);                   // Pixel height
                writer.Write((short)1);                 // Planes
                writer.Write((short)32);                // Bits per pixel
                writer.Write(0);                        // Compression
                writer.Write(numPixelBytes);            // Image size in bytes
                writer.Write(0);                        // X pixels per meter
                writer.Write(0);                        // Y pixels per meter
                writer.Write(0);                        // Number colors in color table
                writer.Write(0);                        // Important color count
            }
        }
    }

    public int Width
    {
        private set;
        get;
    }

    public int Height
    {
        private set;
        get;
    }

    public void SetPixel(int row, int col, Color color)
    {
        SetPixel(row, col, (int)(255 * color.R),
                            (int)(255 * color.G),
                            (int)(255 * color.B),
                            (int)(255 * color.A));
    }

    public void SetPixel(int row, int col, int r, int g, int b, int a = 255)
    {
        int index = (row * Width + col) * 4 + headerSize;
        buffer[index + 0] = (byte)b;
        buffer[index + 1] = (byte)g;
        buffer[index + 2] = (byte)r;
        buffer[index + 3] = (byte)a;
    }

    public ImageSource Generate()
    {
        // Create MemoryStream from buffer with bitmap.
        MemoryStream memoryStream = new MemoryStream(buffer);

        // Convert to StreamImageSource.
        ImageSource imageSource = ImageSource.FromStream(() =>
        {
            return memoryStream;
        });
        return imageSource;
    }

    public static ImageSource CreateBmpFromPixels(List<System.Drawing.Color> pixels, int width, int height)
    {// Creates a bitmap, then converts it to a ImageSource
        BmpMaker maker = new BmpMaker(height, width);

        int count = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                maker.SetPixel(x, y, pixels[count]);
                count++;
            }
        }

        return maker.Generate();
    }
}
