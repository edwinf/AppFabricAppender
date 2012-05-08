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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.ApplicationServer.Caching;

namespace AppFabricLogReader
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			DataCacheFactoryConfiguration factoryConfig = new DataCacheFactoryConfiguration();
			List<DataCacheServerEndpoint> servers = new List<DataCacheServerEndpoint>();
			servers.Add(new DataCacheServerEndpoint("127.0.0.1",22233));
			factoryConfig.Servers = servers;
			DataCacheFactory factory = new DataCacheFactory(factoryConfig);
			DataCache cache = factory.GetCache("default");
			List<KeyValuePair<string,object>> objects = cache.GetObjectsInRegion("EDWINPC").ToList();
			Grid.ItemsSource = objects;
		}
	}
}
