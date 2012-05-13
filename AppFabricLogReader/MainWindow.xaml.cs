using System;
using System.Collections.Generic;
using System.Configuration;
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
using Microsoft.ApplicationServer.Caching;
using Microsoft.Win32;

namespace AppFabricLogReader
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private List<KeyValuePair<long, string>> logEntries;
		public MainWindow()
		{
			InitializeComponent();
			Loaded += MainWindow_Loaded;
		}

		void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				string cacheName = ConfigurationManager.AppSettings["CacheName"] ?? "default";
				string region = ConfigurationManager.AppSettings["LogRegion"];
				DataCacheFactoryConfiguration config = new DataCacheFactoryConfiguration();
				DataCacheFactory factory = new DataCacheFactory();
				DataCache cache = factory.GetCache(cacheName);
				List<KeyValuePair<string, object>> objects = cache.GetObjectsInRegion(region).ToList();

				logEntries = new List<KeyValuePair<long, string>>();
				for (int i = 0; i < objects.Count; i++)
				{
					long key;
					if (long.TryParse(objects[i].Key, out key))
					{
						logEntries.Add(new KeyValuePair<long, string>(key, objects[i].Value.ToString()));
					}
				}
				logEntries.Sort((a, b) => a.Key.CompareTo(b.Key));
				Grid.ItemsSource = logEntries;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error getting objects from cache: " + ex.Message);
			}
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.FileName = "Logs"; // Default file name
			sfd.DefaultExt = ".txt"; // Default file extension
			sfd.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

			if (sfd.ShowDialog().GetValueOrDefault(false))
			{
				string fileName = sfd.FileName;
				StreamWriter sw = new StreamWriter(fileName);
				for (int i = 0; i < logEntries.Count; i++)
				{
					sw.Write(logEntries[i].Value);
				}
				sw.Flush();
				sw.Close();
			}
		}
	}
}
