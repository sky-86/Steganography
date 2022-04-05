using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Text.RegularExpressions;
using System.IO;

namespace SteganographyV3
{
    public partial class MainPage : ContentPage
    {
        // CONSTRUCTOR
        public MainPage()
        {
            InitializeComponent();
        }

        // PROPERTIES
        PPM Current { get; set; }
        int MsgDepth { get; set; } = 11;
        string UserInput { get; set; } = "";
        string SavePath { get; set; } = "";
        string OpenPath { get; set; } = "";

        #region FORM EVENTS

        private async void OnOpenClick(object sender, EventArgs e)
        {// User presses open button; display file picker; opens file if .ppm; displays image
            if (await PickAndShow(PickOptions.Default) != null)
            {
                imgMain.Source = BmpMaker.CreateBmpFromPixels(Current.Pixels, Current.Width, Current.Height);
            }
        }

        private void OnMessageChange(object sender, EventArgs e)
        {// Updates UserInput every time the text in msgInput is changed
            UserInput = ((Entry)sender).Text;
        }

        private void OnDepthChange(object sender, EventArgs e)
        {// Same as messageChange; should only take int;
            // As of making this, the macOS xamarin form does not have
            // Entry Keyboard support; so I can not stop the user from inputing
            // characters. However I can stop the property from saving them
            
            if (int.TryParse(((Entry)sender).Text, out int result))
            {
                MsgDepth = result;
            }
        }

        private void OnSaveChange(object sender, EventArgs e)
        {// takes a save path;
            SavePath = ((Entry)sender).Text;
        }

        private async void OnEncodeClick(object sender, EventArgs e)
        {// User press Encode btn; Encode msg into image bytes; update second imgbox with new image
            // Checks if depth entry has letters in it
            if (Regex.IsMatch(entDepth.Text, @"[a-zA-Z]"))
            {
                await DisplayAlert("Depth Error", "Depth can only contain numbers", "OK");
                return;
            }
            // the first 11 pixels are used as a header
            if (MsgDepth < 11)
            {
                await DisplayAlert("Depth Error", "Depth must be greater than 10", "OK");
                return;
            }

            // Makes sure the user doesnt place the msg outside of the image
            double msgPixelLength = (int)Math.Ceiling((double)((UserInput.Length * 8) / 3));
            int imgPixelCount = Current.Pixels.Count;
            int range = (int)(imgPixelCount - msgPixelLength) - 1;

            if ( MsgDepth > range )
            {
                await DisplayAlert("Depth Error", "Depth is to large; Must be less than " + range, "OK");
                return;
            }

            Current.EncodeMessage(UserInput, MsgDepth);

            imgSecond.Source = BmpMaker.CreateBmpFromPixels(Current.Pixels, Current.Width, Current.Height);
        }

        private async void OnSaveClick(object sender, EventArgs e)
        {// User press save; if user encoded a msg; save it

            // if the user hasnt encoded a message yet
            if (Current.Modified == false)
            {
                await DisplayAlert("Save Error", "Nothing to save", "OK");
                return;
            }

            // save at the location the image was opened
            if (SavePath == "")
            {
                SavePath = Path.GetDirectoryName(OpenPath);
                SavePath += "/modified_ppm.ppm";
            }

            // Checks if the path ends with .ppm
            if (!SavePath.EndsWith("ppm"))
            {
                await DisplayAlert("Save Error", "Path does not end with .ppm", "OK");
                return;
            }

            // Checks if the directory exists
            if (!Directory.Exists(Path.GetDirectoryName(SavePath)))
            {
                await DisplayAlert("Save Error", "Path does not exist", "OK");
                return;
            }

            Current.Save(SavePath);
        }

        private async void OnNextClick(object sender, EventArgs e)
        {// Changes the view to the Decode page
            await Navigation.PushAsync(new SecondPage());
        }

        #endregion

        #region HELPER METHODS

        async Task<FileResult> PickAndShow(PickOptions options)
        {// File picker; used in OpenClick(); checks for .ppm; initializes a new PPM object
            try
            {
                var result = await FilePicker.PickAsync(options);
                if (result != null)
                {
                    if (result.FileName.EndsWith("ppm", StringComparison.OrdinalIgnoreCase))
                    {
                        OpenPath = result.FullPath;
                        Current = new PPM(OpenPath);
                    }
                    else
                    {
                        await DisplayAlert("Error", "Image must be a '.ppm'", "OK");
                        result = null;
                    }
                }

                return result;
            }
            catch
            {
                // The user canceled or something went wrong
            }

            return null;
        }

        #endregion

    }
}

