using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace YumCommu.UI; 

public partial class RecordGrid {

	/// <summary>
	/// 縦スクロールバーの幅
	/// </summary>
	public double VerticalScrollBarWidth {
		get => (double)GetValue(VerticalScrollBarWidthProperty);
		set => SetValue(VerticalScrollBarWidthProperty, value);
	}

	/// <summary>
	/// 縦スクロールバーの幅（依存関係プロパティ）
	/// </summary>
	public static readonly DependencyProperty VerticalScrollBarWidthProperty =
		DependencyProperty.Register(
			nameof(VerticalScrollBarWidth),
			typeof(double),
			typeof(RecordGrid),
			new PropertyMetadata(20.0));

	/// <summary>
	/// 横スクロールバーの高さ
	/// </summary>
	public double HorizontalScrollBarHeight {
		get => (double)GetValue(HorizontalScrollBarHeightProperty);
		set => SetValue(HorizontalScrollBarHeightProperty, value);
	}


	/// <summary>
	/// 横スクロールバーの高さ（依存関係プロパティ）
	/// </summary>
	public static readonly DependencyProperty HorizontalScrollBarHeightProperty =
		DependencyProperty.Register(
			nameof(HorizontalScrollBarHeight),
			typeof(double),
			typeof(RecordGrid),
			new PropertyMetadata(20.0));


	/// <summary>
	/// 列定義のコレクション。追加・削除・列の変更はグリッドの再レイアウト要求として通知される。
	/// </summary>
	public ObservableCollection<RecordGridColumn> Columns { get; } = new ObservableCollection<RecordGridColumn>();

	/// <summary>
	/// 列定義、またはいずれかの列のプロパティが変更されたときに発生する。
	/// </summary>
	public event EventHandler? ColumnsChanged;

	/// <summary>
	/// 列コレクションの変更を購読する。
	/// </summary>
	private void InitializeColumns() {
		Columns.CollectionChanged += OnColumnCollectionChanged;
	}

	/// <summary>
	/// 列の追加・削除に応じて、列ごとの変更通知の購読を更新する。
	/// </summary>
	/// <param name="sender">イベント発行元</param>
	/// <param name="e">コレクション変更の内容</param>
	private void OnColumnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		// Clear などの Reset では旧要素が通知されないため、購読済みの列を保持して全件付け替える。
		foreach (RecordGridColumn column in _subscribedColumns) {
			column.PropertyChanged -= OnColumnPropertyChanged;
		}
		_subscribedColumns.Clear();
		foreach (RecordGridColumn column in Columns) {
			column.PropertyChanged += OnColumnPropertyChanged;
			_subscribedColumns.Add(column);
		}
		ColumnsChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// 変更通知を購読中の列
	/// </summary>
	private readonly List<RecordGridColumn> _subscribedColumns = new List<RecordGridColumn>();

	/// <summary>
	/// 列のプロパティ変更を再レイアウト要求として通知する。
	/// </summary>
	/// <param name="sender">変更された列</param>
	/// <param name="e">変更されたプロパティの情報</param>
	private void OnColumnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
		ColumnsChanged?.Invoke(this, EventArgs.Empty);
	}
}
