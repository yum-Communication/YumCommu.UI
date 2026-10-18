using System.ComponentModel;
using Microsoft.UI.Xaml;
using YumCommu.UI;

namespace TestRecordGrid;

/// <summary>
/// RecordGridColumn のテスト
/// </summary>
public class RecordGridColumnTests {

	/// <summary>
	/// テスト用の行オブジェクト
	/// </summary>
	private sealed record Person(string Name, int Age);

	/// <summary>
	/// Create で指定した関数により行オブジェクトから値を取得できる。
	/// </summary>
	[Fact]
	public void Create_ValueGetterReturnsValue() {
		var column = RecordGridColumn.Create<Person>("名前", p => p.Name, 80, TextAlignment.Right);

		Assert.Equal("名前", column.Caption);
		Assert.Equal(80, column.Width);
		Assert.Equal(TextAlignment.Right, column.Alignment);
		Assert.Equal("山田", column.GetValue(new Person("山田", 30)));
	}

	/// <summary>
	/// ValueGetter 未設定の場合は null を返す。
	/// </summary>
	[Fact]
	public void GetValue_WithoutGetter_ReturnsNull() {
		var column = new RecordGridColumn();

		Assert.Null(column.GetValue(new Person("山田", 30)));
	}

	/// <summary>
	/// Width を変更すると PropertyChanged が発生し、同じ値では発生しない。
	/// </summary>
	[Fact]
	public void Width_RaisesPropertyChangedOnlyWhenChanged() {
		var column = new RecordGridColumn { Width = 100 };
		var names = new List<string?>();
		column.PropertyChanged += (_, e) => names.Add(e.PropertyName);

		column.Width = 100;
		column.Width = 150;

		Assert.Equal(new[] { nameof(RecordGridColumn.Width) }, names);
	}

	/// <summary>
	/// 書式化文字列が指定されていれば書式化され、null は空文字列になる。
	/// </summary>
	[Fact]
	public void FormatValue_AppliesFormatString() {
		var column = new RecordGridColumn { FormatString = "N2" };

		Assert.Equal("1,234.50", column.FormatValue(1234.5).Replace("，", ","));
		Assert.Equal(string.Empty, column.FormatValue(null));
	}
}
