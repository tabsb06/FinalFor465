
using System.Windows;
using NAudio.Wave;
using NAudio.Gui;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.IO;
using Microsoft.Win32; // For SaveFileDialog and OpenFileDialog
using System.Windows.Media.Imaging;
using ScottPlot.WPF;
using Manufaktura.Controls.WPF;
using System.Windows.Media;
using System.Drawing;
using System.Drawing.Imaging;
using Music_Transcriber;
using System.Printing;
using System.Windows.Controls;  // Required for PrintDialog
using QRCoder;

namespace TestingMasuka
{
    public partial class MainWindow : Window
    {
        private WaveViewer waveViewer;
        private string selectedMp3FilePath;
        public MainWindow()
        {
            InitializeComponent();

            var viewModel = new TestData();
            DataContext = viewModel;
            //viewModel.LoadTestData();
        }

        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Open clicked");
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Music Files (*.mp3;*.wav)|*.mp3;*.wav|All Files (*.*)|*.*",
                Title = "Save Music File"
            };

            if (saveFileDialog.ShowDialog() == true)
            {

                MessageBox.Show("File saved: " + saveFileDialog.FileName);
            }
        }

        private BitmapSource GetPlotBitmap(WpfPlot plot)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                (int)plot.ActualWidth,
                (int)plot.ActualHeight,
                96d,
                96d,
                PixelFormats.Pbgra32);

            renderTargetBitmap.Render(plot);
            return renderTargetBitmap;
        }
        private Bitmap RenderNoteViewerToImage(NoteViewer noteViewer)
        {

            noteViewer.Measure(new System.Windows.Size(Double.PositiveInfinity, Double.PositiveInfinity));
            //noteViewer.Arrange(new Rect(0, 0, noteViewer.DesiredSize.Width, noteViewer.DesiredSize.Height));
            //noteViewer.UpdateLayout();


            var renderBitmap = new RenderTargetBitmap(
                (int)noteViewer.ActualWidth,
                (int)noteViewer.ActualHeight,
                96d,
                96d,
                System.Windows.Media.PixelFormats.Pbgra32);


            renderBitmap.Render(noteViewer);


            var bitmap = new System.Drawing.Bitmap(renderBitmap.PixelWidth, renderBitmap.PixelHeight);
            var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            renderBitmap.CopyPixels(Int32Rect.Empty, bitmapData.Scan0,
                bitmapData.Stride * bitmapData.Height, bitmapData.Stride);
            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }


        private void btnExportFrequencyPDf_click(object sender, RoutedEventArgs e)
        {
            // Capture the NoteViewer content as a bitmap image
            Bitmap noteViewerImage = RenderNoteViewerToImage(NoteViewer1);

            // Create a new PDF document and add a page
            PdfDocument document = new PdfDocument();
            document.Info.Title = $"Frequency Notes for {Path.GetFileNameWithoutExtension(selectedMp3FilePath)}.pdf";
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Set the left alignment x position
            double leftXPosition = 40;

            // Draw the header text at the specified x position
            string headerText = $"PDF of {Path.GetFileNameWithoutExtension(selectedMp3FilePath)}";
            XFont headerFont = new XFont("Verdana", 20);
            double headerYPosition = 40; // Top margin for header
            gfx.DrawString(headerText, headerFont, XBrushes.Black, new XPoint(leftXPosition, headerYPosition));

            // Position the image directly below the header with a small margin
            double imageTopMargin = 20;
            double imageYPosition = headerYPosition + headerFont.Size + imageTopMargin; // Start image right below header

            if (noteViewerImage != null)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    noteViewerImage.Save(stream, ImageFormat.Png);
                    stream.Position = 0;
                    XImage xImage = XImage.FromStream(stream);

                    // Maintain aspect ratio while adjusting image to fit page width
                    double originalWidth = noteViewerImage.Width;
                    double originalHeight = noteViewerImage.Height;
                    double desiredWidth = page.Width - (2 * leftXPosition); // Adjusted for side margins
                    double aspectRatio = originalHeight / originalWidth;
                    double desiredHeight = desiredWidth * aspectRatio;

                    // Draw the image aligned on the left, directly below the header
                    gfx.DrawImage(xImage, leftXPosition, imageYPosition, desiredWidth, desiredHeight);
                }
            }

            // Save the PDF document using a SaveFileDialog
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = "Save PDF File",
                FileName = $"Frequency Notes for {Path.GetFileNameWithoutExtension(selectedMp3FilePath)}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                try
                {
                    document.Save(filePath);
                    MessageBox.Show("PDF saved successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save PDF file. Error: {ex.Message}");
                }
            }
        }



        private void btnExportLoudnessPDF_click(object sender, RoutedEventArgs e)
        {
            Bitmap noteViewerImage = RenderNoteViewerToImage(NoteViewer2);

            PdfDocument document = new PdfDocument();
            document.Info.Title = $"Loudness Notes for {Path.GetFileNameWithoutExtension(selectedMp3FilePath)}.pdf";
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            double leftXPosition = 40;

            string headerText = $"PDF of {Path.GetFileNameWithoutExtension(selectedMp3FilePath)}";
            XFont headerFont = new XFont("Verdana", 20);
            double headerYPosition = 40; // Top margin for header
            gfx.DrawString(headerText, headerFont, XBrushes.Black, new XPoint(leftXPosition, headerYPosition));

            double imageTopMargin = 20;
            double imageYPosition = headerYPosition + headerFont.Size + imageTopMargin; // Start image right below header

            if (noteViewerImage != null)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    noteViewerImage.Save(stream, ImageFormat.Png);
                    stream.Position = 0;
                    XImage xImage = XImage.FromStream(stream);

                    double originalWidth = noteViewerImage.Width;
                    double originalHeight = noteViewerImage.Height;
                    double desiredWidth = page.Width - (2 * leftXPosition); // Adjusted for side margins
                    double aspectRatio = originalHeight / originalWidth;
                    double desiredHeight = desiredWidth * aspectRatio;

                    gfx.DrawImage(xImage, leftXPosition, imageYPosition, desiredWidth, desiredHeight);
                }
            }

            // Save the PDF document using a SaveFileDialog
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = "Save PDF File",
                FileName = $"Frequency Notes for {Path.GetFileNameWithoutExtension(selectedMp3FilePath)}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                try
                {
                    document.Save(filePath);
                    MessageBox.Show("PDF saved successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save PDF file. Error: {ex.Message}");
                }
            }
        }


        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            txtFileSelected.Clear();
        }

        private void btnRedo_Click(object sender, RoutedEventArgs e)
        {
            txtFileSelected.Text = Path.GetFileName(selectedMp3FilePath);
        }

        private void btnLoadFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                selectedMp3FilePath = LoadFiles();

                if (!string.IsNullOrEmpty(selectedMp3FilePath))
                {
                    txtFileSelected.Text = Path.GetFileName(selectedMp3FilePath);
                    //MessageBox.Show($"MP3 file '{Path.GetFileName(selectedMp3FilePath)}' selected.");
                }
                else
                {
                    MessageBox.Show("No file selected.");
                }
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show("File Not Found, please select an MP3 file.");
            }
        }


        private void btnExit_click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnHelp_click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is an application that transcribes audio files into " +
                "sheet music.  This application has the ability to save transcribed files into PDF " +
                "forms, and allow for saving to personal devices.");
        }

        public async void btnRecord_Click(object sender, RoutedEventArgs e)
        {
            PlayAudio();

            // Await the asynchronous conversion process
            await ConvertMp3ToFrequencies(selectedMp3FilePath);
            //await ConvertMp3ToLoudness(selectedMp3FilePath);

            var viewModel = (TestData)DataContext; // For the first NoteViewer
            var viewModel2 = new TestData(); 

            viewModel.LoadTestData();
            viewModel2.LoadTestData2();

            NoteViewer1.DataContext = viewModel;
            NoteViewer2.DataContext = viewModel2;
        }



        public async Task ConvertMp3ToFrequencies(string mp3FilePath)
        {
            var progress = new Progress<int>(value =>
            {
                ProgressBarFrequency.Value = value; // Update progress bar value
            });

            string appBasePath = AppDomain.CurrentDomain.BaseDirectory;

            string logFilePath = Path.Combine(appBasePath, "frequencies.txt");

            AudioFilePeakLogger readFile = new AudioFilePeakLogger(mp3FilePath);

            await Task.Run(() => readFile.ProcessFile(logFilePath, progress));

            ProgressBarFrequency.Value = 0;
        }

        // public async Task ConvertMp3ToLoudness(string mp3FilePath)
        //{
        //  var progress = new Progress<int>(value =>
        //{
        //  ProgressBarLoudness.Value = value; // Update progress bar value
        // });

        //string appBasePath = AppDomain.CurrentDomain.BaseDirectory;

        //string logFilePath = Path.Combine(appBasePath, "path_to_loudness_output_file.txt");

        //AudioFileLoudness readFile = new AudioFileLoudness(mp3FilePath);

        //await Task.Run(() => readFile.ProcessFile(logFilePath, progress));

        //ProgressBarLoudness.Value = 0;
        //}


        private string LoadFiles()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {

                Filter = "MP3 files (*.mp3)|*.mp3",
                Multiselect = false
            };

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string filePath = openFileDialog.FileName;

                return filePath;
            }

            return null;


        }
        private void LoadAlbumCover(string filePath)
        {
            try
            {
                var file = TagLib.File.Create(filePath);
                var mStream = new MemoryStream();

                if (file.Tag.Pictures.Length >= 1)
                {
                    var bin = file.Tag.Pictures[0].Data.Data;
                    mStream.Write(bin, 0, bin.Length);
                    mStream.Position = 0;
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = mStream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    AlbumCover.Source = bitmap;  // Set album art
                }
                else
                {
                    // Load a default image if no album art is found
                    AlbumCover.Source = new BitmapImage(new Uri(@"C:\Users\awazn\Downloads\mp3 icon.jpg"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading album cover: {ex.Message}");
                // Load a default image if there's an error
                AlbumCover.Source = new BitmapImage(new Uri(@"C:\Users\awazn\Downloads\mp3 icon.jpg"));
            }
        }



        private IWavePlayer outputDevice; // Class-level variable to hold the output device

        public void PlayAudio()
        {
            string nameOfMP3File = selectedMp3FilePath;
            if (string.IsNullOrEmpty(nameOfMP3File))
            {
                MessageBox.Show("Please enter a valid mp3 file location");
                return;
            }
            try
            {
                using (var audioFile = new AudioFileReader(nameOfMP3File))
                {
                    outputDevice = new WaveOutEvent();
                    outputDevice.Init(audioFile);
                    outputDevice.Play();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred, please try a different file: " + ex.Message);
            }
        }
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            ShowView(HomeView);
        }

        private void FrequenciesButton_Click(object sender, RoutedEventArgs e)
        {
            ShowView(FrequenciesView);
        }

        private void LoudnessButton_Click(object sender, RoutedEventArgs e)
        {
            ShowView(LoudnessView);
        }



        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of the Start window
            Start startWindow = new Start(); // Make sure "Start" is the class name of your Start.xaml window

            // Show the Start window
            startWindow.Show();

            // Close the current MainWindow
            this.Close();
        }

        private void MergeNoteViewerData()
        {
            // Assuming both NoteViewers have ScoreSource properties that can be accessed
            var frequenciesData = NoteViewer1.ScoreSource; // Get data from NoteViewer1
            var loudnessData = NoteViewer2.ScoreSource; // Get data from NoteViewer2



        }
        private void ShowView(UIElement viewToShow)
        {
            HomeView.Visibility = Visibility.Collapsed;
            FrequenciesView.Visibility = Visibility.Collapsed;
            LoudnessView.Visibility = Visibility.Collapsed;
            ComparisonView.Visibility = Visibility.Collapsed;
            BackView.Visibility = Visibility.Collapsed;

            viewToShow.Visibility = Visibility.Visible;
        }
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            // Create a new instance of the application
            MainWindow newWindow = new MainWindow();
            newWindow.Show();

            // Close the current window
            this.Close();
        }
        private void btnPrintFrequency_Click(object sender, RoutedEventArgs e)
        {
            PrintNoteViewer(NoteViewer1);
        }

        private void btnPrintLoudness_Click(object sender, RoutedEventArgs e)
        {
            PrintNoteViewer(NoteViewer2);
        }

        private void PrintNoteViewer(NoteViewer noteViewer)
        {
            // Create a PrintDialog instance
            PrintDialog printDialog = new PrintDialog();

            // Show the print dialog to the user
            if (printDialog.ShowDialog() == true)
            {
                // Set the visual to print
                printDialog.PrintVisual(noteViewer, "Printing Note Viewer");
            }
        }
        private void btnStopPlayingAudio_Click(object sender, RoutedEventArgs e)
        {
            if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Playing)
            {
                outputDevice.Stop();
                MessageBox.Show("Playback stopped.");
            }
        }

        public Bitmap GetQRCodeImage(string text)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q))
                {
                    using (QRCode qrCode = new QRCode(qrCodeData))
                    {
                        return qrCode.GetGraphic(20);
                    }
                }
            }
        }
        private void btnExportAndGenerateQRCode_Click(object sender, RoutedEventArgs e)
        {
            string pdfFilePath = ExportNoteViewerToPDF(NoteViewer1);
            if (!string.IsNullOrEmpty(pdfFilePath))
            {
                GenerateQRCode(pdfFilePath);
                MessageBox.Show("PDF and QR Code generated successfully.");
            }
        }

        private string ExportNoteViewerToPDF(NoteViewer noteViewer)
        {
            string filePath = "output.pdf"; // Specify your desired file path here
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Note Viewer Output";
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Render NoteViewer content to image (this function needs to be implemented)
            Bitmap noteViewerImage = RenderNoteViewerToImage(noteViewer);

            using (MemoryStream stream = new MemoryStream())
            {
                noteViewerImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;
                XImage xImage = XImage.FromStream(stream);
                gfx.DrawImage(xImage, 0, 0); // Draw the image on the PDF page
            }

            document.Save(filePath);
            return filePath; // Return the path of the saved PDF
        }

        private void GenerateQRCode(string pdfFilePath)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(pdfFilePath, QRCodeGenerator.ECCLevel.Q);
                using (QRCode qrCode = new QRCode(qrCodeData))
                {
                    using (Bitmap qrBitmap = qrCode.GetGraphic(20))
                    {
                        qrBitmap.Save("qrcode.png"); // Save QR code as an image
                    }
                }
            }
        }

        private void btnStartScanning_Click(object sender, RoutedEventArgs e)
        {
            // Your code here
        }


        private void ComparisonButton_Click(object sender, RoutedEventArgs e)
        {
            // Your code here
        }
    }
}
