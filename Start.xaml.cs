using System.Windows;
using TestingMasuka;

namespace Music_Transcriber
{
    public partial class Start : Window
    {
        public Start()
        {
            InitializeComponent();
        }

        // Event handler for Piano button click
        private void PianoButton_Click(object sender, RoutedEventArgs e)
        {
            // Logic to open MainWindow.xaml when Piano button is clicked
            MainWindow pianoWindow = new MainWindow();  // Create a new instance of MainWindow
            pianoWindow.Show();  // Show the MainWindow

            this.Close();  // Optionally, close the current window (Start window)
        }


        // Event handler for Guitar button click
        private void GuitarButton_Click(object sender, RoutedEventArgs e)
        {
            // Logic for Guitar button
            MessageBox.Show("Guitar Coming Soon!");
        }

        // Event handler for Violin button click
        private void ViolinButton_Click(object sender, RoutedEventArgs e)
        {
            // Logic for Violin button
            MessageBox.Show("Violin Coming Soon!");
        }
    }
}
