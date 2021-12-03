using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Win32;
using WpfApp1.Annotations;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<Directory> treeItemConfigData;
        private Directory selectedItem;
        private const string RootPath = @"D:\Local\SWF\ServiceHub";
        private const string FactoryPath = @"C:\Program Files\Philips\FactoryConfigFiles";
        private const string backupLocaltion = @"D:\Local\SWF\ServiceHub\BackupLocation";

        public MainWindow()
        {
            InitializeComponent();
            TreeItemConfigData = new ObservableCollection<Directory>();
            XmlDocument xDoc;
            XmlNodeReader xNodeReader;
            XmlSerializer xmlSerializer;
            xDoc = new XmlDocument();
            xDoc.Load(@"FileInfo.xml");
            xNodeReader = new XmlNodeReader(xDoc.DocumentElement);
            xmlSerializer = new XmlSerializer(typeof(Directory));
            var fileInfo = xmlSerializer.Deserialize(xNodeReader);
            Directory actualfileInfo = (Directory)fileInfo;

            TreeItemConfigData.Add(actualfileInfo);
            this.DataContext = this;
        }

        public ObservableCollection<Directory> TreeItemConfigData
        {
            get { return treeItemConfigData; }
            set { treeItemConfigData = value; }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {

            //TODO: 
            var msgreslul = MessageBox.Show("Do you want to restore factory version of selected configuration file??", "Restore Options", MessageBoxButton.YesNo);
            string sourceFilePath = string.Empty;
            if (msgreslul == MessageBoxResult.Yes)
            {
                //factory
                sourceFilePath = GetFactoryFile(selectedItem.Path);
            }
            else
            {
                MessageBox.Show("Please select the file that you want to replace", "Browse Files", MessageBoxButton.OK);
                sourceFilePath = Browse();
            }


            if (!CopyFile(selectedItem.Path, sourceFilePath, selectedItem.IsEncryped))
            {
                //TODO:Message
                MessageBox.Show("Error while replacing configuration file. Kindly fallback to backed up version", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Successfully replaced the configuration file!!", "Copy Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private string GetFactoryFile(string selectedItem)
        {
            return System.IO.Path.Combine(FactoryPath, selectedItem);
        }

        private string Browse()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            //TODO:Add filter
            var dialogResult = dialog.ShowDialog();
            if ((bool)dialogResult)
            {
                return dialog.FileName;
            }
            return "";
        }

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            selectedItem = ((Directory)((TreeView)sender).SelectedItem);
            OnPropertyChanged("CanEnableRestore");
            OnPropertyChanged("CanEnableImport");
        }

        private bool CopyFile(string filename, string sourcePath, bool isEncrypted, bool isbackupNeeded = true)
        {
            try
            {
                StartStopService(false);

                string DestPath = System.IO.Path.Combine(RootPath, filename);
                string backupPath = System.IO.Path.Combine(backupLocaltion, filename);
                string finalsourcePath = System.IO.Path.Combine(sourcePath, filename);
                string backupdirlocation = System.IO.Path.GetDirectoryName(backupPath);
                if (isbackupNeeded)
                {
                    if (!System.IO.Directory.Exists(backupdirlocation))
                    {
                        System.IO.Directory.CreateDirectory(backupdirlocation);
                    }
                    File.Copy(DestPath, backupPath, true);
                }
                var tempPath = System.IO.Path.GetTempPath();
                //if (isEncrypted)
                //{

                //    File.Copy(sourcePath, System.IO.Path.Combine(tempPath, filename));
                //    System.Diagnostics.Process process = new Process();
                //    //process.Start("", System.IO.Path.Combine(tempPath, filename));
                //    //TODO:check what is the output of encrypter and update the source path for next step
                //    //sourcePath = 

                //}

                File.Copy(sourcePath, DestPath, true);

                return true; //use this true and false to display error msg in UI
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                StartStopService(true);
            }
        }

        private void StartStopService(bool isStart)
        {
            //System.ServiceProcess.ServiceController serviceController = new ServiceController("PSCServiceHost");
            //if (isStart)
            //     serviceController.Start();
            //else
            //    serviceController.Stop();
        }

        //TODO:
        // 1. ICommand implemenation for Restore button : Done
        // 2. If the file does not exist in back up location then disable the button
        // 3. Apply the Theme : done 
        // 4. Short cut for the exe 
        // 5. Mark the file for Encruption needed or not. If Yes then we have to encrypt and replace the file 
        // 6. Integrate the IST Login
        // 7. INotifyPropertyChanged for button enable disable : DOne
        //8 . Update the window name : Done

        private void RestoreBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (!CopyFile(selectedItem.Path, System.IO.Path.Combine(backupLocaltion, selectedItem.Path), selectedItem.IsEncryped, false))
            {
                //TODO:Message
                MessageBox.Show("Error while replacing configuration file. Kindly fallback to backed up version", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Successfully replaced the configuration file!!", "Copy Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }

        public bool CanEnableRestore
        {
            get
            {
                return selectedItem != null && !string.IsNullOrEmpty(selectedItem.Name) && System.IO.File.Exists(System.IO.Path.Combine(backupLocaltion, selectedItem.Path));
            }
        }

        public bool CanEnableImport
        {
            get
            {
                return selectedItem != null && !string.IsNullOrEmpty(selectedItem.Name);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
