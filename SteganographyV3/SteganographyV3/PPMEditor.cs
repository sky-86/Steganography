using System.Drawing;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;


// Handles editing ppm files
public static class PPMEditor
{
    #region PUBLIC METHODS

    public static Queue<byte> GetImageBytes(string path)
    {// Just reads the raw bytes of the ppm
        return new Queue<byte>(File.ReadAllBytes(path));
    }

    public static byte[] GetHeader(ref Queue<byte> img)
    {// Pops the header from the given bytes
        int eolCount = 0;
        List<byte> header = new List<byte>();

        // Count End of line chars, the header should only have 4
        while (eolCount < 4)
        {
            byte current = img.Dequeue();

            header.Add(current);

            if (current == 10)
            {
                eolCount++;
            }
        }
        return header.ToArray();
    }

    public static List<Color> GetPixels(ref Queue<byte> img, string ppmType)
    {// Retrieves the pixel data from bytes; GetHeader() must be called first
        List<Color> pixels = new List<Color>();
        int count = img.Count();

        if (ppmType == "P3")
        {
            while (img.Count() != 0)
            {
                List<int> rgb = new List<int>();
                StringBuilder sb = new StringBuilder();

                // gets rgb values
                for (int j = 0; j < 3; j++)
                {
                    while (img.Peek() != 10)
                    {
                        sb.Append(Convert.ToChar(img.Dequeue()));
                    }
                    img.Dequeue();

                    rgb.Add(int.Parse(sb.ToString()));
                    sb.Clear();
                }
                Color c = Color.FromArgb(rgb[0], rgb[1], rgb[2]);

                pixels.Add(c);
            }
        }
        else if (ppmType == "P6")
        {
            for (int i = 0; i < count; i += 3)
            {
                int r = img.Dequeue();
                int g = img.Dequeue();
                int b = img.Dequeue();
                Color c = Color.FromArgb(r, g, b);

                pixels.Add(c);
            }
        }

        return pixels;
    }

    public static List<Color> EncodeMessage(string msg, int depth, List<Color> pixels)
    {// Encodes the msg into the pixel data
        // Convert message into binary
        string binMsg = StringToBinary(msg);

        // create a header and convert to binary
        string binLength = IntToBinary(binMsg.Length).PadLeft(16, '0');
        string binDepth = IntToBinary(depth).PadLeft(16, '0');
        string binHeader = binLength + binDepth;

        // Encode Header into pixel data
        int currPixel = 0;
        int currBit = 0;
        while (currBit != binHeader.Length)
        {
            // select current color
            Color currColor = pixels[currPixel];

            // create rgb array for ease of use
            int[] rgb = { currColor.R, currColor.G, currColor.B };

            // change rgb values based on the header
            for (int i = 0; i < 3; i++)
            {
                if (currBit < binHeader.Length)
                {
                    rgb[i] = IntToEvenOrOdd(binHeader[currBit], rgb[i]);
                    currBit++;
                }
            }

            // save changes to pixel
            pixels[currPixel] = Color.FromArgb(rgb[0], rgb[1], rgb[2]);
            currPixel++;
        }

        // Now Encode the message at specified depth
        currPixel = depth;
        currBit = 0;
        while (currBit != binMsg.Length)
        {
            Color currColor = pixels[currPixel];

            int[] rgb = { currColor.R, currColor.G, currColor.B };

            for (int i = 0; i < 3; i++)
            {
                if (currBit < binMsg.Length)
                {
                    rgb[i] = IntToEvenOrOdd(binMsg[currBit], rgb[i]);
                    currBit++;
                }
            }

            // save changes to pixel
            pixels[currPixel] = Color.FromArgb(rgb[0], rgb[1], rgb[2]);
            currPixel++;
        }
        return pixels;
    }

    public static void SaveImage(string path, string type, byte[] header, List<Color> pixels)
    {// Saves the ppm as the given ppm type
        byte[] newPixels;

        if (type == "P3")
        {// Save pixels in human readable format
            newPixels = ConvertPixelsToP3(pixels);
        }
        else if (type == "P6")
        {// Save pixels in the ascii format
            newPixels = ConvertPixelsToP6(pixels);
        }
        else
        {
            throw new Exception("Error with ppm type");
        }

        byte[] img = header.Concat(newPixels).ToArray();

        File.WriteAllBytes(path, img);
    }

    public static string DecodeMessage(List<Color> pixels)
    {// Decodes the hidden msg in the pixel data
        // First read header
        // Header contains the Length of the message and the starting pixel
        string[] header = ReadHiddenHeader(pixels);

        int msgLength = BinaryToInt(header[0]);
        int msgDepth = BinaryToInt(header[1]);

        // Now that we have the length and depth; find and read hidden msg
        string msg = ReadMessage(pixels, msgLength, msgDepth);

        return BinaryToString(msg);
    }
   
    #endregion

    #region PRIVATE METHODS

    private static string StringToBinary(string data)
    {// converts a string to binary digits

        StringBuilder sb = new StringBuilder();

        // data.ToCharArray() ?
        foreach (char c in data)
        {
            sb.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
        }
        return sb.ToString();
    }

    public static string BinaryToString(string bin)
    {// Converts binary to a string of digits

        List<byte> msg = new List<byte>();

        for (int i = 0; i < bin.Length; i += 8)
        {
            String t = bin.Substring(i, 8);

            msg.Add(Convert.ToByte(t, 2));
        }

        return Encoding.ASCII.GetString(msg.ToArray());
    }

    private static int BinaryToInt(string bin)
    {// convert binary to an int
        return Convert.ToInt32(bin, 2);
    }

    private static string IntToBinary(int data)
    {// convert int to a binary number
        string binary = Convert.ToString(data, 2);

        int pad = 8 - (binary.Length % 8) + binary.Length;

        return binary.PadLeft(pad, '0');
    }

    private static int IntToEvenOrOdd(char binary, int data)
    {// Makes the given int even or odd based on the given binary digit
        // make even if zero
        if (binary == '0')
        {
            if (!IsEven(data))
            {
                if (data == 255)
                {
                    data--;
                }
                else
                {
                    data++;
                }
            }
        }
        // else make it odd
        else
        {
            if (IsEven(data))
            {
                data++;
            }
        }
        return data;
    }

    private static bool IsEven(int n)
    {// ...
        if (n % 2 == 0)
        {
            return true;
        }
        return false;
    }

    private static int[] GetIntArray(int data)
    {// Convert the given int into an array of digits
        List<int> digits = new List<int>();

        if (data == 0)
        {
            digits.Add(data);
            return digits.ToArray();
        }

        while (data > 0)
        {
            digits.Add(data % 10);
            data = data / 10;
        }
        digits.Reverse();
        return digits.ToArray();
    }

    private static byte[] ConvertPixelsToP3(List<Color> pixels)
    {// converts pixel data into P3 format
        List<byte> newBytes = new List<byte>();

        foreach (Color c in pixels)
        {
            int[] rgb = new int[] { c.R, c.G, c.B };

            for (int i = 0; i < 3; i++)
            {
                int[] digits = GetIntArray(rgb[i]);

                foreach (int digit in digits)
                {
                    string d = digit.ToString();
                    newBytes.Add(Encoding.ASCII.GetBytes(d)[0]);
                }
                newBytes.Add(10);
            }
        }
        return newBytes.ToArray();
    }

    private static byte[] ConvertPixelsToP6(List<Color> pixels)
    {//converts pixel data into the p6 format
        List<byte> newBytes = new List<byte>();
        foreach (Color c in pixels)
        {
            int[] rgb = new int[] { c.R, c.G, c.B };

            for (int i = 0; i < 3; i++)
            {
                newBytes.Add((byte)rgb[i]);
            }
        }
        return newBytes.ToArray();
    }

    private static char GetBinaryFromDigit(int digit)
    {// converts odd and even number into respective binary digit
        if (IsEven(digit))
        {
            return '0';
        }
        else
        {
            return '1';
        }
    }

    private static string[] ReadHiddenHeader(List<Color> pixels)
    {// Reads the hidden messages header
        List<string> header = new List<string>();
        StringBuilder msgLength = new StringBuilder();
        StringBuilder msgDepth = new StringBuilder();
        int bitCount = 0;

        for (int currPixel = 0; currPixel <= 11; currPixel++)
        {
            // gets rgb values from current pixel
            int[] rgb = { pixels[currPixel].R, pixels[currPixel].G, pixels[currPixel].B };

            for (int i = 0; i < 3; i++)
            {
                if (bitCount < 16)
                {
                    msgLength.Append(GetBinaryFromDigit(rgb[i]));
                }
                else if (bitCount < 32)
                {
                    msgDepth.Append(GetBinaryFromDigit(rgb[i]));
                }
                else
                {
                    header.Add(msgLength.ToString());
                    header.Add(msgDepth.ToString());
                    return header.ToArray();
                }
                bitCount++;
            }
        }
        return null;
    }

    private static string ReadMessage(List<Color> pixels, int msgLength, int msgDepth)
    {// Reads the hidden message from pixel data
        StringBuilder msg = new StringBuilder();
        int bitCount = 0;

        for (int currPixel = msgDepth; currPixel < pixels.Count(); currPixel++)
        {
            int[] rgb = { pixels[currPixel].R, pixels[currPixel].G, pixels[currPixel].B };

            for (int i = 0; i < 3; i++)
            {
                if (bitCount < msgLength)
                {
                    msg.Append(GetBinaryFromDigit(rgb[i]));
                }
                else
                {
                    return msg.ToString();
                }
                bitCount++;
            }
        }
        return null;
    }

    #endregion
}


