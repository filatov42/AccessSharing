using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AccessSharingClient
{
    /// <summary>
    /// Логика взаимодействия для FileUploadWindow.xaml
    /// </summary>
    public partial class FileUploadWindow : Window
    {
        public FileUploadWindow()
        {
            InitializeComponent();
        }

        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            bool falg = true;
            if(!File.Exists(pathTextBox.Text)) { falg = false; }
            try
            {
                int i = Convert.ToInt32(accesLevelTextBox.Text);
            }
            catch (Exception)
            {
                falg = false;
            }
            if(tagTextBox.Text.Length == 0) { falg = false; }
            if (!falg)
            {
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    string caption = "Ошибка";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage image = MessageBoxImage.Error;
                    MessageBoxResult result;
                    result = MessageBox.Show("Параметры указаны неверно!", caption, button, image);
                }
            ));
                thread.Start();
                return;
            }
            DialogResult = true;
            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                pathTextBox.Text = openFileDialog.FileName;
        }

        public string Path()
        {
            return pathTextBox.Text;
        }
        public int SecurityLevel()
        {
            return Convert.ToInt32(accesLevelTextBox.Text);
        }
        public string DataTag()
        {
            return tagTextBox.Text;
        }
    }
}
