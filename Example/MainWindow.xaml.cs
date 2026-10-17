using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Example {

	internal sealed record Person(int Id, string Name, int? Age, string Email);


	/// <summary>
	/// An empty window that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainWindow: Window {
		public MainWindow() {
			InitializeComponent();

			RecordGrid.ItemsSource = new List<Person> {
				new Person(1, "Alice", 30, "alice@example.com"),
				new Person(2, "Bob", 25, "bob@example.com"),
				new Person(3, "Charlie", 35, "charlie@example.com")
			};
			RecordGrid.Columns[0].ValueGetter = r => ((Person)r).Id;
			RecordGrid.Columns[1].ValueGetter = r => ((Person)r).Name;
			RecordGrid.Columns[2].ValueGetter = r => ((Person)r).Age;
			RecordGrid.Columns[3].ValueGetter = r => ((Person)r).Email;
		}

		/// <summary>
		/// プロパティボタンがクリックされたときに呼び出されます。
		/// <see cref="YumCommu.UI.SideBar.PropertyVisibility"/> の表示・非表示を切り替えます。
		/// </summary>
		/// <param name="sender">イベントの送信元。</param>
		/// <param name="e">イベントデータ。</param>
		private void BtnToggleProperties_Click(object sender, RoutedEventArgs e) {
			MainSideBar.PropertyVisibility = MainSideBar.PropertyVisibility == Visibility.Visible
				? Visibility.Collapsed
				: Visibility.Visible;
		}
	}
}
