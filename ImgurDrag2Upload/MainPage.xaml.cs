using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ImgurDrag2Upload
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region DataModel
        [DataContract]
        public class Data
        {
            [DataMember]
            public string id { get; set; }
            [DataMember]
            public object title { get; set; }
            [DataMember]
            public object description { get; set; }
            [DataMember]
            public int datetime { get; set; }
            [DataMember]
            public string type { get; set; }
            [DataMember]
            public bool animated { get; set; }
            [DataMember]
            public int width { get; set; }
            [DataMember]
            public int height { get; set; }
            [DataMember]
            public int size { get; set; }
            [DataMember]
            public int views { get; set; }
            [DataMember]
            public int bandwidth { get; set; }
            [DataMember]
            public object vote { get; set; }
            [DataMember]
            public bool favorite { get; set; }
            [DataMember]
            public object nsfw { get; set; }
            [DataMember]
            public object section { get; set; }
            [DataMember]
            public object account_url { get; set; }
            [DataMember]
            public int account_id { get; set; }
            [DataMember]
            public bool is_ad { get; set; }
            [DataMember]
            public List<object> tags { get; set; }
            [DataMember]
            public bool in_gallery { get; set; }
            [DataMember]
            public string deletehash { get; set; }
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public string link { get; set; }
        }

        [DataContract]
        public class RootObject
        {
            [DataMember]
            public Data data { get; set; }
            [DataMember]
            public bool success { get; set; }
            [DataMember]
            public int status { get; set; }
        }
        #endregion

        public readonly string IMGUR_ID = "fa73e72ec7ee543";
        public readonly string IMGUR_TOKEN_KEY = "082abdcaa325adbf1129f73fbff0aff0891cdd03";

        public MainPage()
        {
            this.InitializeComponent();
        }

        string _imgPath = "";
        public string GetJsonResult(string jsonString)
        {
            var data = JsonConvert.DeserializeObject<RootObject>(jsonString); // xả từ json thành list object
            var imgLink = data.data.link;
            return imgLink;
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();

                if (items.Any())
                {
                    var storageFile = items[0] as StorageFile;

                    using (var inputStream = await storageFile.OpenSequentialReadAsync())
                    {
                        var readStream = inputStream.AsStreamForRead();
                        var byteArray = new byte[readStream.Length];
                        await readStream.ReadAsync(byteArray, 0, byteArray.Length);

                        var client = new HttpClient();
                        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.imgur.com/3/image");
                        requestMessage.Headers.Add("Authorization", "Client-ID " + IMGUR_ID);
                        requestMessage.Content = new StringContent(Convert.ToBase64String(byteArray));

                        tblStatus2.Text = "Uploading....";

                        var respone = await client.SendAsync(requestMessage);
                        var responeAsString = await respone.Content.ReadAsStringAsync();

                        var strResult = GetJsonResult(responeAsString);

                        tblStatus2.Text = "Upload Success";

                        GetAndCoppyLink(strResult);

                        var bitmapImage = new BitmapImage();
                        var imgData = await storageFile.OpenAsync(FileAccessMode.Read);
                        bitmapImage.SetSource(imgData);
                        DroppedImage.Source = bitmapImage;

                        tblStatus2.Text = "Link coppied";
                    }
                }
            }
        }

        void GetAndCoppyLink(string strResult)
        {
            tblLink.Text = strResult;
            tblShortLink.Text = strResult.Remove(0, 7).Replace('/', '-');

            var dataPackage = new DataPackage();

            if (rdbtnFullLink.IsChecked == true)
            {
                dataPackage.SetText(strResult);
                Clipboard.SetContent(dataPackage);
            }

            if(rdbtnShortLink.IsChecked == true)
            {
                dataPackage.SetText(tblShortLink.Text);
                Clipboard.SetContent(dataPackage);
            }


        }
        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
        }
    }
}
