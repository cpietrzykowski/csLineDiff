using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace csLineDiff
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string _appPath;
        ObservableCollection<string> _files;

        public MainWindow()
        {
            InitializeComponent();

            this._appPath = AppDomain.CurrentDomain.BaseDirectory;
            this._files = new ObservableCollection<string>();
            this._files.CollectionChanged += this.collectionChanged;
            this.filesList.ItemsSource = this._files;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                DefaultExt = "*.*",
                Filter = "All Files (*.*)|*.*",
                InitialDirectory = this._appPath,
                Multiselect = true
            };

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string[] filenames = dlg.FileNames;
                foreach (string f in filenames)
                {
                    if (!this._files.Contains(f))
                    {
                        this._files.Add(f);
                    }
                }
            }
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> selected = this.filesList.SelectedItems.Cast<string>().ToList();

            foreach (string s in selected)
            {
                this._files.Remove(s);
            }
        }

        private void collectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (sender is ObservableCollection<string>)
            {
                ObservableCollection<string> files = (ObservableCollection<string>)sender;
                this.removeButton.IsEnabled = (files.Count > 0);
                this.diffButton.IsEnabled = (files.Count > 1);
            }
        }

        private void diffButton_Click(object sender, RoutedEventArgs e)
        {
            // dictionary of lines (key) and occurrences (value)
            Dictionary<string, int> differences = new Dictionary<string, int>();

            foreach (string file in this._files)
            {
                // load the file into a hashset
                if (File.Exists(file))
                {
                    string[] contents = File.ReadAllLines(file); // pull text lines into array
                    IEnumerator<string> i = contents.AsEnumerable<string>().GetEnumerator();
                    while (i.MoveNext())
                    {
                        string current = i.Current;
                        if (differences.ContainsKey(current))
                        {
                            int occurences = differences[current];
                            differences[current] += 1;
                        }
                        else
                        {
                            differences.Add(current, 1);
                        }
                    }
                }
            }

            // sort the differences
            string[] linesOut = differences.Where(pair => pair.Value == 1).Select(pair => pair.Key).ToArray();
            Array.Sort<string>(linesOut, (a, b) => String.Compare(a, b));
            this.saveSetAsTextFile(linesOut);
        }

        private void saveSetAsTextFile(string[] lines)
        {
            // get output file from user
            SaveFileDialog dlg = new SaveFileDialog
            {
                DefaultExt = "*.*",
                Filter = "All Files (*.*)|*.*",
                InitialDirectory = this._appPath,
                FileName = "diffs.txt"
            };

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string saveFile = dlg.FileName;

                // write to file
                StreamWriter writer = new StreamWriter(saveFile);

                foreach (string line in lines)
                {
                    writer.WriteLine(line);
                }

                writer.Flush();
                writer.Close();
            }
        }
    }
}
