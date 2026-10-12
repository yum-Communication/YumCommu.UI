using YumCommu.UI;

namespace TestRecordGrid;

/// <summary>
/// VirtualizationEngine のユニットテストクラス
/// </summary>
public class VirtualizationEngineTests {
	/// <summary>
	/// デフォルトの行高で表示範囲を計算するテスト
	/// </summary>
	[Fact]
	public void GetVisibleRange_WithDefaultRowHeight_ReturnsCorrectRange() {
		// Arrange
		var engine = new VirtualizationEngine(25.0, 200);
		
		// Act
		var range = engine.GetVisibleRange(0, 600);
		
		// Assert
		Assert.Equal(0, range.StartIndex);
		Assert.Equal(23, range.EndIndex);
		Assert.Equal(0.0, range.SubHeight);
	}
	
	/// <summary>
	/// カスタム行高設定時の総高計算テスト
	/// </summary>
	[Fact]
	public void TotalHeight_WithCustomRowHeight_CalculatesCorrectly() {
		// Arrange
		var engine = new VirtualizationEngine(25.0, 200);
		var expectedHeight = 25.0 * 199 + 55.0;
		
		// Act
		engine.SetRowHeight(3, 55.0);
		
		// Assert
		Assert.Equal(expectedHeight, engine.TotalHeight);
	}

	/// <summary>
	/// カスタム行高設定時の表示範囲計算テスト
	/// </summary>
	[Fact]
	public void GetVisibleRange_WithCustomRowHeight_ReturnsCorrectRange() {
		// Arrange
		var engine = new VirtualizationEngine(25, 200);
		engine.SetRowHeight(3, 90);
		
		// Act & Assert
		var range = engine.GetVisibleRange(0, 600);
		Assert.Equal(0, range.StartIndex);
		Assert.Equal(21, range.EndIndex);

		range = engine.GetVisibleRange(100, 600);
		Assert.Equal(3, range.StartIndex);
		Assert.Equal(25, range.EndIndex);
		Assert.Equal(25.0, range.SubHeight);
	}

	/// <summary>
	/// 負のオフセットでの表示範囲計算テスト。ゼロの場合と同じ結果が返ることを確認する。
	/// </summary>
	[Fact]
	public void GetVisibleRange_WithNegativeOffset_HandlesCorrectly() {
		// Arrange
		var engine = new VirtualizationEngine(25.0, 200);
		
		// Act
		var range = engine.GetVisibleRange(-50, 600);
		
		// Assert
		Assert.Equal(0, range.StartIndex);
		Assert.Equal(23, range.EndIndex);
		Assert.Equal(0.0, range.SubHeight);
	}

	/// <summary>
	/// オフセットがリスト総高を超える場合の表示範囲計算テスト
	/// </summary>
	[Fact]
	public void GetVisibleRange_WithOffsetExceedingTotalHeight_ReturnsEndOfList() {
		// Arrange
		var engine = new VirtualizationEngine(25.0, 200);
		double totalHeight = engine.TotalHeight;
		
		// Act
		var range = engine.GetVisibleRange(totalHeight + 100, 600);
		
		// Assert
		Assert.Equal(199, range.StartIndex);
		Assert.Equal(199, range.EndIndex);
	}
}
