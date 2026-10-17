using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections;
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
	/// 列ヘッダの高さ
	/// </summary>
	public double HeaderHeight {
		get => (double)GetValue(HeaderHeightProperty);
		set => SetValue(HeaderHeightProperty, value);
	}

	/// <summary>
	/// 列ヘッダの高さ（依存関係プロパティ）
	/// </summary>
	public static readonly DependencyProperty HeaderHeightProperty =
		DependencyProperty.Register(
			nameof(HeaderHeight),
			typeof(double),
			typeof(RecordGrid),
			new PropertyMetadata(30.0, OnHeaderLayoutChanged));

	/// <summary>
	/// 水平スクロールオフセット (ピクセル)
	/// </summary>
	public double HorizontalOffset {
		get => (double)GetValue(HorizontalOffsetProperty);
		set => SetValue(HorizontalOffsetProperty, value);
	}

	/// <summary>
	/// 水平スクロールオフセット（依存関係プロパティ）
	/// </summary>
	public static readonly DependencyProperty HorizontalOffsetProperty =
		DependencyProperty.Register(
			nameof(HorizontalOffset),
			typeof(double),
			typeof(RecordGrid),
			new PropertyMetadata(0.0, OnHeaderLayoutChanged));

	/// <summary>
	/// 行の高さ (ピクセル)
	/// </summary>
	public double RowHeight {
		get => (double)GetValue(RowHeightProperty);
		set => SetValue(RowHeightProperty, value);
	}

	/// <summary>
	/// 行の高さ（依存関係プロパティ）
	/// </summary>
	public static readonly DependencyProperty RowHeightProperty =
		DependencyProperty.Register(
			nameof(RowHeight),
			typeof(double),
			typeof(RecordGrid),
			new PropertyMetadata(28.0, OnHeaderLayoutChanged));

	/// <summary>
	/// 垂直スクロールオフセット (ピクセル)
	/// </summary>
	public double VerticalOffset {
		get => (double)GetValue(VerticalOffsetProperty);
		set => SetValue(VerticalOffsetProperty, value);
	}

	/// <summary>
	/// 垂直スクロールオフセット（依存関係プロパティ）
	/// </summary>
	public static readonly DependencyProperty VerticalOffsetProperty =
		DependencyProperty.Register(
			nameof(VerticalOffset),
			typeof(double),
			typeof(RecordGrid),
			new PropertyMetadata(0.0, OnHeaderLayoutChanged));

	/// <summary>
	/// 表示する行オブジェクトの集合。IList の場合は複製せず参照し、それ以外は設定時のスナップショットを保持する。
	/// INotifyCollectionChanged を実装していれば変更に追従する。
	/// </summary>
	public IEnumerable? ItemsSource {
		get => (IEnumerable?)GetValue(ItemsSourceProperty);
		set => SetValue(ItemsSourceProperty, value);
	}

	/// <summary>
	/// 表示する行オブジェクトの集合（依存関係プロパティ）
	/// </summary>
	public static readonly DependencyProperty ItemsSourceProperty =
		DependencyProperty.Register(
			nameof(ItemsSource),
			typeof(IEnumerable),
			typeof(RecordGrid),
			new PropertyMetadata(null, OnItemsSourceChanged));

	/// <summary>
	/// ヘッダの配置に関わるプロパティが変更されたときに再描画を要求する。
	/// </summary>
	/// <param name="d">変更されたグリッド</param>
	/// <param name="e">変更の内容</param>
	private static void OnHeaderLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		((RecordGrid)d)._canvas?.Invalidate();
	}

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
