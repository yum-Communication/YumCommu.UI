using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace YumCommu.UI; 

public partial class RecordGrid {

	/// <summary>
	///  列定義
	/// </summary>
	public List<RecordGridColumn> Columns { get; } = new List<RecordGridColumn>();

	public static readonly DependencyProperty ColumnsProperty;


	static RecordGrid() {
		ColumnsProperty = DependencyProperty.Register(
			nameof(Columns),
			typeof(List<RecordGridColumn>),
			typeof(RecordGrid),
			new PropertyMetadata(new List<RecordGridColumn>(), OnColumnsChanged));
	}

	/// <summary>
	/// 列定義が変更されたときの処理
	/// </summary>
	/// <param name="d"></param>
	/// <param name="e"></param>
	/// <exception cref="NotImplementedException"></exception>
	private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		throw new NotImplementedException();
	}
}
