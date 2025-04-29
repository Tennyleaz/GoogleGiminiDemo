using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace GoogleGiminiDemo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private string apiKey;
    private string model = "gemini-2.0-flash";
    private HttpClient httpClient;
    private const string STREAMING_CONTENT = "streamGenerateContent?alt=sse&";
    private const string GENERATE_CONTENT = "generateContent?"; 

    public MainWindow()
    {
        InitializeComponent();

        tbSystemPrompt.Text = "Read the input image. Output as provided JSON schema.\n" +
                              "Set `image_type` and `image_content` fields as what you recognize what the image contains.\n" +
                              "If the image contains text, fill `text` field, in markdown format.\n" +
                              "Properly name the image, in the `title` field.\n";
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            apiKey = File.ReadAllText("google gimini api key.txt");
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }

        httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(120);
    }

    private async Task<string> Generation(string userPrompt, string systemPrompt, string imageFileName)
    {
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:{GENERATE_CONTENT}key={apiKey}";
        GiminiPostData postData = new GiminiPostData
        {
            contents = new List<GiminiContent>
            {
                new GiminiContent
                {
                    parts = new List<GiminiPart>
                    {
                        new GiminiPart
                        {
                            text = userPrompt
                        }
                    }
                }
            },
            system_instruction = new GiminiContent
            {
                parts = new List<GiminiPart>
                {
                    new GiminiPart
                    {
                        text = systemPrompt
                    }
                }
            },
            generationConfig = new GiminiConfig
            {
                response_mime_type = "application/json",
                response_schema = GetResonpseSchemaStr()
            }
        };

        if (File.Exists(imageFileName))
        {
            FileInfo fileInfo = new FileInfo(imageFileName);
            string dataType;
            if (fileInfo.Extension == ".png")
            {
                dataType = "image/png";
            }
            else if (fileInfo.Extension == ".jpg" || fileInfo.Extension == ".jpeg")
            {
                dataType = "image/jpeg";
            }
            else if (fileInfo.Extension == ".bmp")
            {
                dataType = "image/bmp";
            }
            else
            {
                throw new ArgumentException("Unsupported image format");
            }

            GiminiInlineData imageData = new GiminiInlineData
            {
                data = Convert.ToBase64String(await File.ReadAllBytesAsync(fileInfo.FullName)),
                mime_type = dataType
            };
            postData.contents[0].parts.Add(new GiminiPart
            {
                inline_data = imageData
            });
        }
        else
        {
            MessageBox.Show("Image not exist!");
            return null;
        }

        string payload = JsonSerializer.Serialize(postData, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        StringContent stringContent = new StringContent(
            payload,
            Encoding.UTF8,
            "application/json");
        HttpResponseMessage responseMessage = await httpClient.PostAsync(url, stringContent);
        string responseString = await responseMessage.Content.ReadAsStringAsync();
        if (responseMessage.StatusCode == HttpStatusCode.OK)
        {
            GiminiResponse response = JsonSerializer.Deserialize<GiminiResponse>(responseString);
            string jsonString = response.candidates[0].content.parts[0].text;
            try
            {
                ParseResult pr = JsonSerializer.Deserialize<ParseResult>(jsonString);
                ResultBrowser resultBrowser = new ResultBrowser(pr);
                resultBrowser.Owner = this;
                resultBrowser.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return jsonString;
        }
        else
        {
            return responseString;
        }
    }

    private JsonDocument GetResonpseSchemaStr()
    {
        string json = @"{
            ""type"": ""object"",
            ""properties"": {
                ""image_type"": {
                    ""type"": ""string"",
                    ""enum"": [""photo"", ""graph"", ""text"", ""table"", ""unrecognized""]
                },
                ""text"": {
                    ""type"": ""string"",
                    ""description"": ""Recognized text in the image, in markdown format."",
                    ""nullable"": true
                },
                ""image_content"": {
                    ""type"": ""string"",
                    ""description"": ""Short summary of what the input image contains."",
                    ""nullable"": true
                },
                ""title"": {
                    ""type"": ""string"",
                    ""description"": ""Provide a proper name for this image.""
                }
            }
        }";
        return JsonDocument.Parse(json);
    }

    private async void BtnGenerate_OnClick(object sender, RoutedEventArgs e)
    {
        progress.Visibility = Visibility.Visible;
        btnGenerate.IsEnabled = false;
        try
        {
            string result = await Generation(tbUserPrompt.Text, tbSystemPrompt.Text, tbFileName.Text);
            tbResult.Text = result;
        }
        catch (Exception ex)
        {
            tbResult.Text = ex.ToString();
        }
        progress.Visibility = Visibility.Collapsed;
        btnGenerate.IsEnabled = true;
    }

    private void BtnPickImage_OnClick(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new OpenFileDialog
        {
            Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
            Title = "Select an image file",
            DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
        };
        if (dialog.ShowDialog() == true)
        {
            tbFileName.Text = dialog.FileName;
            btnGenerate.IsEnabled = true;
            using (FileStream fs = new FileStream(dialog.FileName, FileMode.Open))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = fs;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                img.Source = bitmapImage;
            }
        }
    }
}