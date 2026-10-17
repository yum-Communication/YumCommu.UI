using System.Collections.ObjectModel;
using System.ComponentModel;
using YumCommu.UI;

namespace TestRecordGrid;

/// <summary>
/// RecordView と RecordSource のフィルタ・並べ替えに関するテストです。
/// </summary>
public class RecordViewTests {

	private sealed record Person(string Name, int? Age);

	private static readonly RecordGridColumn NameColumn = RecordGridColumn.Create<Person>("Name", p => p.Name);
	private static readonly RecordGridColumn AgeColumn = RecordGridColumn.Create<Person>("Age", p => p.Age);

	private static RecordView CreateView(params Person[] people) {
		return new RecordView { Source = RecordSource.Create(people) };
	}

	private static string[] Names(RecordView view) {
		return Enumerable.Range(0, view.Count).Select(i => ((Person)view.GetRecord(i)!).Name).ToArray();
	}

	/// <summary>
	/// フィルタも並べ替えもない場合は元データの順序のまま表示されます。
	/// </summary>
	[Fact]
	public void Rebuild_WithoutFilterOrSort_KeepsSourceOrder() {
		var view = CreateView(new Person("B", 2), new Person("A", 1), new Person("C", 3));

		Assert.Equal(new[] { "B", "A", "C" }, Names(view));
		Assert.Equal(new[] { 0, 1, 2 }, Enumerable.Range(0, view.Count).Select(view.GetSourceIndex));
	}

	/// <summary>
	/// フィルタに一致する行だけが残り、元データのインデックスが対応付けられます。
	/// </summary>
	[Fact]
	public void Filter_KeepsMatchingRowsWithSourceIndex() {
		var view = CreateView(new Person("A", 10), new Person("B", 20), new Person("C", 30));

		view.Filter = r => ((Person)r).Age >= 20;

		Assert.Equal(new[] { "B", "C" }, Names(view));
		Assert.Equal(1, view.GetSourceIndex(0));
		Assert.Equal(2, view.GetSourceIndex(1));
	}

	/// <summary>
	/// フィルタを解除すると全行が復元されます。
	/// </summary>
	[Fact]
	public void Filter_Cleared_RestoresAllRows() {
		var view = CreateView(new Person("A", 1), new Person("B", 2));
		view.Filter = r => false;
		Assert.Equal(0, view.Count);

		view.Filter = null;

		Assert.Equal(2, view.Count);
	}

	/// <summary>
	/// 昇順・降順で並べ替えられ、元データの並びは変わりません。
	/// </summary>
	[Fact]
	public void SetSort_OrdersRowsWithoutChangingSource() {
		var people = new List<Person> { new("B", 2), new("A", 1), new("C", 3) };
		var view = new RecordView { Source = RecordSource.Create(people) };

		view.SetSort(new[] { new RecordGridSortDescription(NameColumn, ListSortDirection.Ascending) });
		Assert.Equal(new[] { "A", "B", "C" }, Names(view));

		view.SetSort(new[] { new RecordGridSortDescription(NameColumn, ListSortDirection.Descending) });
		Assert.Equal(new[] { "C", "B", "A" }, Names(view));

		Assert.Equal(new[] { "B", "A", "C" }, people.Select(p => p.Name));
	}

	/// <summary>
	/// 複数列の並べ替えでは先頭の条件が優先されます。
	/// </summary>
	[Fact]
	public void SetSort_MultipleColumns_UsesPriorityOrder() {
		var view = CreateView(new Person("B", 1), new Person("A", 2), new Person("A", 1), new Person("B", 2));

		view.SetSort(new[] {
			new RecordGridSortDescription(NameColumn, ListSortDirection.Ascending),
			new RecordGridSortDescription(AgeColumn, ListSortDirection.Descending),
		});

		Assert.Equal(new[] { 1, 2, 3, 0 }, Enumerable.Range(0, view.Count).Select(view.GetSourceIndex));
	}

	/// <summary>
	/// 同値の行は元データの順序が保たれます (安定ソート)。
	/// </summary>
	[Fact]
	public void SetSort_EqualKeys_IsStable() {
		var view = CreateView(new Person("X", 1), new Person("Y", 1), new Person("Z", 1));

		view.SetSort(new[] { new RecordGridSortDescription(AgeColumn, ListSortDirection.Descending) });

		Assert.Equal(new[] { "X", "Y", "Z" }, Names(view));
	}

	/// <summary>
	/// null の値は昇順で先頭、降順で末尾になります。
	/// </summary>
	[Fact]
	public void SetSort_NullValues_AreFirstWhenAscending() {
		var view = CreateView(new Person("A", 2), new Person("B", null), new Person("C", 1));

		view.SetSort(new[] { new RecordGridSortDescription(AgeColumn, ListSortDirection.Ascending) });
		Assert.Equal(new[] { "B", "C", "A" }, Names(view));

		view.SetSort(new[] { new RecordGridSortDescription(AgeColumn, ListSortDirection.Descending) });
		Assert.Equal(new[] { "A", "C", "B" }, Names(view));
	}

	/// <summary>
	/// 列の SortComparer が指定されていればそれで比較されます。
	/// </summary>
	[Fact]
	public void SetSort_UsesColumnSortComparer() {
		var column = RecordGridColumn.Create<Person>("Name", p => p.Name);
		column.SortComparer = Comparer<object?>.Create((a, b) => -string.CompareOrdinal((string?)a, (string?)b));
		var view = CreateView(new Person("A", 1), new Person("C", 1), new Person("B", 1));

		view.SetSort(new[] { new RecordGridSortDescription(column, ListSortDirection.Ascending) });

		Assert.Equal(new[] { "C", "B", "A" }, Names(view));
	}

	/// <summary>
	/// フィルタと並べ替えを組み合わせると、残った行だけが並べ替えられます。
	/// </summary>
	[Fact]
	public void FilterAndSort_AreCombined() {
		var view = CreateView(new Person("D", 4), new Person("A", 1), new Person("C", 3), new Person("B", 2));

		view.Filter = r => ((Person)r).Age >= 2;
		view.SetSort(new[] { new RecordGridSortDescription(NameColumn, ListSortDirection.Ascending) });

		Assert.Equal(new[] { "B", "C", "D" }, Names(view));
		Assert.Equal(new[] { 3, 2, 0 }, Enumerable.Range(0, view.Count).Select(view.GetSourceIndex));
	}

	/// <summary>
	/// 範囲外の表示行を指定すると例外になります。
	/// </summary>
	[Fact]
	public void GetSourceIndex_OutOfRange_Throws() {
		var view = CreateView(new Person("A", 1));

		Assert.Throws<ArgumentOutOfRangeException>(() => view.GetSourceIndex(1));
		Assert.Throws<ArgumentOutOfRangeException>(() => view.GetSourceIndex(-1));
	}

	/// <summary>
	/// 再構築のたびに Changed が発生します。
	/// </summary>
	[Fact]
	public void Rebuild_RaisesChanged() {
		var view = CreateView(new Person("A", 1));
		int count = 0;
		view.Changed += (_, _) => count++;

		view.Filter = r => true;
		view.SetSort(Array.Empty<RecordGridSortDescription>());

		Assert.Equal(2, count);
	}

	/// <summary>
	/// IList は複製せずに参照するため、Rebuild で追加行が反映されます。
	/// </summary>
	[Fact]
	public void RecordSource_List_IsReferencedNotCopied() {
		var people = new ObservableCollection<Person> { new("A", 1) };
		var view = new RecordView { Source = RecordSource.Create(people) };

		people.Add(new Person("B", 2));
		view.Rebuild();

		Assert.Equal(2, view.Count);
	}

	/// <summary>
	/// IList 以外はスナップショットとして保持されます。
	/// </summary>
	[Fact]
	public void RecordSource_Enumerable_IsSnapshot() {
		var people = new List<Person> { new("A", 1) };
		IEnumerable<Person> lazy = people.Where(_ => true);
		var source = RecordSource.Create(lazy);

		people.Add(new Person("B", 2));

		Assert.Equal(1, source.Count);
	}

	/// <summary>
	/// null のデータは空のデータ元になります。
	/// </summary>
	[Fact]
	public void RecordSource_Null_IsEmpty() {
		Assert.Equal(0, RecordSource.Create(null).Count);
	}
}
