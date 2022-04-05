using System;
using System.Drawing;
using System.Text;
using System.Collections.Generic;
using System.IO;

// stores info about the given ppm image
public class PPM
{
    // FIELDS
    private string _ppmType = "";
    private byte[] _ppmHeader;
    private int _width = 0;
    private int _height = 0;
    private List<Color> _pixels;
    private bool _modified = false;

    #region PROPERTIES

    public string Type { get { return _ppmType; } set { _ppmType = value; } }
    public byte[] Header { get { return _ppmHeader; } set { _ppmHeader = value; } }
    public int Width { get { return _width; } set { _width = value; } }
    public int Height { get { return _height; } set { _height = value; } }
    public List<Color> Pixels { get { return _pixels; } set { _pixels = value; } }
    public bool Modified { get { return _modified; } set { _modified = value; } }

    #endregion

    // CONSTRUCTOR
    public PPM(string path)
	{
        // Check if file is a ppm
        if (Path.GetExtension(path) != ".ppm")
        {
            throw new Exception("File is not a ppm");
        }

        // First get the bytes from the image
        Queue<byte> bytes = PPMEditor.GetImageBytes(path);

        // Next remove header from bytes and return the rest
        Header = PPMEditor.GetHeader(ref bytes);

        // Set the PPM type, p6 or p3
        SetType();

        // Set height and width
        SetDimensions();

        // Now get the pixel data
        Pixels = PPMEditor.GetPixels(ref bytes, Type);
	}

    #region PUBLIC METHODS

    public void EncodeMessage(string msg, int depth)
    {// Wrapper for PPMEditor encode
        if (Pixels == null)
        {
            throw new Exception("No pixel data");
        }

        Pixels = PPMEditor.EncodeMessage(msg, depth, Pixels);
        Modified = true;
    }

    public void Save(string path)
    {// Wrapper for PPMEditor save
        if (Pixels == null)
        {
            throw new Exception("No pixel data");
        }

        PPMEditor.SaveImage(path, Type, Header, Pixels);
    }

    public string DecodeMessage()
    {// wrapper for PPMEditor decode
        if (Pixels == null)
        {
            throw new Exception("No pixel data");
        }

        return PPMEditor.DecodeMessage(Pixels);
    }

    #endregion

    #region PRIVATE METHODS

    private void SetType()
    {// The first two bytes of the header will always
        // be the ppm magic number.  P3 or P6
        if (Header == null)
        {
            throw new Exception("Header is null");
        }

        char[] l = { (char)Header[0], (char)Header[1] };
        Type = new string(l);
    }

    private void SetDimensions()
    {// Gets the height and width of the image from the header
        if (Header == null)
        {
            throw new Exception("Header is null");
        }

        int eolCount = 0;
        int count = 0;
        StringBuilder sb = new StringBuilder();

        while (eolCount < 3)
        {
            // counted 2 end of lines, so we are on the dimensions line
            if (eolCount == 2 && Header[count] != 10)
            {
                // if current count is a space
                // set width, bc always comes first
                if (Header[count] == 32)
                {
                    Width = int.Parse(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(Convert.ToChar(Header[count]));
                }
            }

            if (Header[count] == 10)
            {
                eolCount++;
            }
            count++;
        }
        Height = int.Parse(sb.ToString());
    }

    #endregion
}


