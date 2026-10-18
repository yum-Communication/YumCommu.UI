using Microsoft.UI.Xaml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace YumCommu.UI;

public sealed partial class RecordGrid {

	private readonly RecordView _view = new RecordView();
	private INotifyCollectionChanged? _observedItemsSource;

	/// <summary>
	/// フィルタと並べ替えを適用した表示行の対応
	/// </summary>
	internal RecordView View => _view;

	/// <summary>
	/// 行を残す条件。null の場合はすべての行を表示する。
	/// </summary>
	public Predicate<object>? Filter {
		get => _view.Filter;
		set => _view.Filter = value;
	}

	/// <summary>
	/// 現在の並べ替え条件。先頭ほど優先度が高い。
	/// </summary>
	public IReadOnlyList<RecordGridSortDescription> SortDescriptions => _view.SortDescriptions;

	/// <summary>
	/// 表示行数 (フィルタ後の行数)
	/// </summary>
	public int VisibleRecordCount => _view.Count;

	/// <summary>
	/// データの変更通知とビューの変更通知を購読する。
	/// </summary>
	private void InitializeData() {
		_view.Changed += (_, _) => _canvas?.Invalidate();
		Loaded += (_, _) => ObserveItemsSource(ItemsSource);
		Unloaded += (_, _) => ObserveItemsSource(null);
	}

	/// <summary>
	/// 指定した列で昇順または降順に並べ替える。既存の並べ替え条件は置き換えられる。
	/// </summary>
	/// <param name="column">並べ替えの対象列</param>
	/// <param name="direction">並べ替えの方向</param>
	public void SortBy(RecordGridColumn column, ListSortDirection direction = ListSortDirection.Ascending) {
		ArgumentNullException.ThrowIfNull(column);
		_view.SetSort(new[] { new RecordGridSortDescription(column, direction) });
	}

	/// <summary>
	/// 複数列の並べ替え条件を設定する。先頭ほど優先度が高い。
	/// </summary>
	/// <param name="sortDescriptions">並べ替え条件</param>
	public void SortBy(IEnumerable<RecordGridSortDescription> sortDescriptions) {
		_view.SetSort(sortDescriptions);
	}

	/// <summary>
	/// 並べ替えを解除し、元データの順序に戻す。
	/// </summary>
	public void ClearSort() {
		_view.SetSort(Array.Empty<RecordGridSortDescription>());
	}

	/// <summary>
	/// 表示行に対応する行オブジェクトを取得する。
	/// </summary>
	/// <param name="viewIndex">表示行のインデックス</param>
	/// <returns>行オブジェクト</returns>
	public object? GetRecord(int viewIndex) {
		return _view.GetRecord(viewIndex);
	}

	/// <summary>
	/// 表示行に対応する元データのインデックスを取得する。
	/// </summary>
	/// <param name="viewIndex">表示行のインデックス</param>
	/// <returns>元データのインデックス</returns>
	public int GetSourceIndex(int viewIndex) {
		return _view.GetSourceIndex(viewIndex);
	}

	/// <summary>
	/// ItemsSource が変更されたときにデータ元を作り直す。
	/// </summary>
	/// <param name="d">変更されたグリッド</param>
	/// <param name="e">変更の内容</param>
	private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		var grid = (RecordGrid)d;
		grid.ObserveItemsSource(e.NewValue as IEnumerable);
		grid._view.Source = RecordSource.Create(e.NewValue as IEnumerable);
	}

	/// <summary>
	/// データ元の変更通知の購読先を切り替える。
	/// </summary>
	/// <param name="items">新しいデータ。null の場合は購読を解除するだけ。</param>
	private void ObserveItemsSource(IEnumerable? items) {
		if (_observedItemsSource is not null) {
			_observedItemsSource.CollectionChanged -= OnItemsSourceCollectionChanged;
			_observedItemsSource = null;
		}
		if (items is INotifyCollectionChanged observable) {
			observable.CollectionChanged += OnItemsSourceCollectionChanged;
			_observedItemsSource = observable;
		}
	}

	/// <summary>
	/// データ元のコレクションが変更されたときに表示行を作り直す。
	/// </summary>
	/// <param name="sender">イベント発行元</param>
	/// <param name="e">コレクション変更の内容</param>
	private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		// IList 以外はスナップショットのため、データ元ごと作り直す。
		_view.Source = RecordSource.Create(ItemsSource);
	}
}
