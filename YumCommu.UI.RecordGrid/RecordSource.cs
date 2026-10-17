using System;
using System.Collections;
using System.Collections.Generic;

namespace YumCommu.UI;

/// <summary>
/// インデックスで行オブジェクトにアクセスできるデータ元を表します。
/// </summary>
internal interface IRecordSource {

	/// <summary>
	/// 行数
	/// </summary>
	int Count { get; }

	/// <summary>
	/// 指定したインデックスの行オブジェクトを取得します。
	/// </summary>
	/// <param name="index">0 から始まる行インデックス</param>
	object? this[int index] { get; }
}

/// <summary>
/// <see cref="IRecordSource"/> を生成するファクトリです。
/// </summary>
internal static class RecordSource {

	/// <summary>
	/// 列挙可能なデータからデータ元を生成します。
	/// <see cref="IList"/> は複製せずにそのまま参照し、それ以外は生成時点のスナップショットを保持します。
	/// </summary>
	/// <param name="items">データ。null の場合は空のデータ元になります。</param>
	/// <returns>データ元</returns>
	public static IRecordSource Create(IEnumerable? items) {
		if (items is null) {
			return new SnapshotRecordSource(new List<object?>());
		}
		if (items is IList list) {
			return new ListRecordSource(list);
		}
		var snapshot = new List<object?>();
		foreach (object? item in items) {
			snapshot.Add(item);
		}
		return new SnapshotRecordSource(snapshot);
	}

	/// <summary>
	/// <see cref="IList"/> をそのまま参照するデータ元
	/// </summary>
	private sealed class ListRecordSource: IRecordSource {
		private readonly IList _list;

		/// <summary>
		/// リストを参照するデータ元を生成します。
		/// </summary>
		/// <param name="list">参照するリスト</param>
		public ListRecordSource(IList list) {
			_list = list;
		}

		/// <inheritdoc/>
		public int Count => _list.Count;

		/// <inheritdoc/>
		public object? this[int index] => _list[index];
	}

	/// <summary>
	/// 生成時点のスナップショットを保持するデータ元
	/// </summary>
	private sealed class SnapshotRecordSource: IRecordSource {
		private readonly List<object?> _items;

		/// <summary>
		/// スナップショットを保持するデータ元を生成します。
		/// </summary>
		/// <param name="items">保持する行</param>
		public SnapshotRecordSource(List<object?> items) {
			_items = items;
		}

		/// <inheritdoc/>
		public int Count => _items.Count;

		/// <inheritdoc/>
		public object? this[int index] => _items[index];
	}
}
