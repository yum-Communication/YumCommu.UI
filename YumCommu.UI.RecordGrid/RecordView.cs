using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace YumCommu.UI;

/// <summary>
/// データ元に対するフィルタと並べ替えの結果を、表示行から元データのインデックスへの対応として保持します。
/// 元データは変更・複製しません。
/// </summary>
internal sealed class RecordView {
	private IRecordSource _source = RecordSource.Create(null);
	private Predicate<object>? _filter;
	private List<RecordGridSortDescription> _sortDescriptions = new();
	private int[] _viewToSource = Array.Empty<int>();

	/// <summary>
	/// 表示内容が再構築されたときに発生します。
	/// </summary>
	public event EventHandler? Changed;

	/// <summary>
	/// データ元。設定すると表示内容を再構築します。
	/// </summary>
	public IRecordSource Source {
		get => _source;
		set {
			ArgumentNullException.ThrowIfNull(value);
			_source = value;
			Rebuild();
		}
	}

	/// <summary>
	/// 行を残す条件。null の場合はすべての行を残します。
	/// フィルタが設定されている場合、null の行は除外されます。設定すると表示内容を再構築します。
	/// </summary>
	public Predicate<object>? Filter {
		get => _filter;
		set {
			_filter = value;
			Rebuild();
		}
	}

	/// <summary>
	/// 現在の並べ替え条件。先頭ほど優先度が高くなります。
	/// </summary>
	public IReadOnlyList<RecordGridSortDescription> SortDescriptions => _sortDescriptions;

	/// <summary>
	/// 表示行数 (フィルタ後の行数)
	/// </summary>
	public int Count => _viewToSource.Length;

	/// <summary>
	/// 並べ替え条件を置き換え、表示内容を再構築します。
	/// </summary>
	/// <param name="sortDescriptions">新しい並べ替え条件。空の場合は元データの順序になります。</param>
	public void SetSort(IEnumerable<RecordGridSortDescription> sortDescriptions) {
		ArgumentNullException.ThrowIfNull(sortDescriptions);
		_sortDescriptions = new List<RecordGridSortDescription>(sortDescriptions);
		Rebuild();
	}

	/// <summary>
	/// 表示行に対応する元データのインデックスを取得します。
	/// </summary>
	/// <param name="viewIndex">表示行のインデックス</param>
	/// <returns>元データのインデックス</returns>
	public int GetSourceIndex(int viewIndex) {
		if ((uint)viewIndex >= (uint)_viewToSource.Length) {
			throw new ArgumentOutOfRangeException(nameof(viewIndex));
		}
		return _viewToSource[viewIndex];
	}

	/// <summary>
	/// 表示行に対応する行オブジェクトを取得します。
	/// </summary>
	/// <param name="viewIndex">表示行のインデックス</param>
	/// <returns>行オブジェクト</returns>
	public object? GetRecord(int viewIndex) {
		return _source[GetSourceIndex(viewIndex)];
	}

	/// <summary>
	/// フィルタと並べ替えを元データに適用して、表示行の対応を作り直します。
	/// データ元の内容が変わったときにも呼び出します。
	/// </summary>
	public void Rebuild() {
		int sourceCount = _source.Count;
		var indices = new List<int>(sourceCount);
		for (int i = 0; i < sourceCount; i++) {
			if (_filter is null) {
				indices.Add(i);
				continue;
			}
			object? record = _source[i];
			if (record is not null && _filter(record)) {
				indices.Add(i);
			}
		}

		if (_sortDescriptions.Count > 0 && indices.Count > 1) {
			SortIndices(indices);
		}

		_viewToSource = indices.ToArray();
		Changed?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// 並べ替え条件に従って、元データのインデックスを安定ソートします。
	/// 各行のキーは並べ替えの前に 1 回だけ取得します。
	/// </summary>
	/// <param name="indices">並べ替える元データのインデックス</param>
	private void SortIndices(List<int> indices) {
		int sortCount = _sortDescriptions.Count;
		int sourceCount = _source.Count;
		var keys = new object?[sortCount][];
		var comparers = new IComparer<object?>[sortCount];
		for (int s = 0; s < sortCount; s++) {
			RecordGridColumn column = _sortDescriptions[s].Column;
			comparers[s] = column.SortComparer ?? Comparer<object?>.Default;
			var columnKeys = new object?[sourceCount];
			foreach (int index in indices) {
				object? record = _source[index];
				columnKeys[index] = record is null ? null : column.GetValue(record);
			}
			keys[s] = columnKeys;
		}

		indices.Sort((a, b) => {
			for (int s = 0; s < sortCount; s++) {
				int result = comparers[s].Compare(keys[s][a], keys[s][b]);
				if (result != 0) {
					return _sortDescriptions[s].Direction == ListSortDirection.Ascending ? result : -result;
				}
			}
			// List.Sort は不安定なため、同値の場合は元の順序で決める。
			return a.CompareTo(b);
		});
	}
}
