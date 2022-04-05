using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace SteganographyV3
{	
	public partial class SecondPage : ContentPage
	{	
		public SecondPage ()
		{
			InitializeComponent();
		}

		// PROPERTIES
		PPM Current { get; set; }

        #region FORM EVENTS

        private async void OnOpenClick(object sender, EventArgs e)
		{// User presses open button; does the same as the other one
			if (await PickAndShow(PickOptions.Default) != null)
			{
				imgMain.Source = BmpMaker.CreateBmpFromPixels(Current.Pixels, Current.Width, Current.Height);
			}
		}

		private void OnDecodeClick(object sender, EventArgs e)
		{// Decode button is pressed;
			// decode image for message
			string secretMsg = Current.DecodeMessage();

			// display message
			lblOutput.Text = secretMsg;
		}

		private async void OnNextClick(object sender, EventArgs e)
		{// Next button is clicked; switch back to mainpage
			await Navigation.PushAsync(new MainPage());
		}

        #endregion

        #region HELPER METHODS

        async Task<FileResult> PickAndShow(PickOptions options)
		{// same as mainpage filepicker
			try
			{
				var result = await FilePicker.PickAsync(options);
				if (result != null)
				{
					if (result.FileName.EndsWith("ppm", StringComparison.OrdinalIgnoreCase))
					{
						Current = new PPM(result.FullPath);
					}
					else
                    {
						result = null;
                    }
				}

				return result;
			}
			catch (Exception ex)
			{
				// The user canceled or something went wrong
			}

			return null;
		}

        #endregion
    }
}

