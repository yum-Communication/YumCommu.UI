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
		Assert.Equal(24, range.EndIndex);
		Assert.Equal(0.0, range.OffsetWithinFirstRow);
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
		Assert.Equal(22, range.EndIndex);

		range = engine.GetVisibleRange(100, 600);
		Assert.Equal(3, range.StartIndex);
		Assert.Equal(26, range.EndIndex);
		Assert.Equal(25.0, range.OffsetWithinFirstRow);
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
		Assert.Equal(24, range.EndIndex);
		Assert.Equal(0.0, range.OffsetWithinFirstRow);
	}

	/// <summary>
	/// オフセットがリスト総高を超える場合の表示範囲計算テスト（最終行のみが表示対象となることを確認する）。
	/// </summary>
	[Fact]
	public void GetVisibleRange_WithOffsetExceedingTotalHeight_ReturnsEndOfList() {
		// Arrange
		var engine = new VirtualizationEngine(25.0, 200);
		
		// Act
		var range = engine.GetVisibleRange(engine.TotalHeight + 100, 600);
		
		// Assert
		Assert.Equal(176, range.StartIndex);
		Assert.Equal(200, range.EndIndex);
	}

	/// <summary>
	/// ビューポートの下端が行境界に一致する場合、境界の次の行を含めないことを確認するテスト
	/// </summary>
	[Fact]
	public void GetVisibleRange_WhenViewportEndsAtRowBoundary_ExcludesNextRow() {
		// Arrange
		var engine = new VirtualizationEngine(25.0, 10);

		// Act
		var range = engine.GetVisibleRange(0, 50);

		// Assert
		Assert.Equal(0, range.StartIndex);
		Assert.Equal(2, range.EndIndex);
	}

	/// <summary>
	/// オフセットが行境界に一致する場合、その行から表示範囲を開始するテスト
	/// </summary>
	[Fact]
	public void GetVisibleRange_WhenScrollOffsetIsRowBoundary_StartsAtNextRow() {
		// Arrange
		var engine = new VirtualizationEngine(25.0, 10);

		// Act
		var range = engine.GetVisibleRange(50, 25);

		// Assert
		Assert.Equal(2, range.StartIndex);
		Assert.Equal(3, range.EndIndex);
		Assert.Equal(0.0, range.OffsetWithinFirstRow);
	}

	/// <summary>
	/// 総行数が0の場合に空範囲を返すテスト
	/// </summary>
	[Fact]
	public void GetVisibleRange_WithNoRows_ReturnsEmptyRange() {
		// Arrange
		var engine = new VirtualizationEngine(25.0, 0);

		// Act
		var range = engine.GetVisibleRange(0, 100);

		// Assert
		Assert.Equal(0, range.StartIndex);
		Assert.Equal(0, range.EndIndex);
		Assert.Equal(0.0, range.OffsetWithinFirstRow);
	}

	/// <summary>
	/// ビューポート高が0の場合に空範囲を返すテスト
	/// </summary>
	[Fact]
	public void GetVisibleRange_WithZeroViewportHeight_ReturnsEmptyRange() {
		// Arrange
		var engine = new VirtualizationEngine(25.0, 10);

		// Act
		var range = engine.GetVisibleRange(0, 0);

		// Assert
		Assert.Equal(0, range.StartIndex);
		Assert.Equal(0, range.EndIndex);
		Assert.Equal(0.0, range.OffsetWithinFirstRow);
	}

	/// <summary>
	/// ビューポート高が総高以上の場合に全行を返すテスト
	/// </summary>
	[Fact]
	public void GetVisibleRange_WhenViewportCoversAllRows_ReturnsAllRows() {
		// Arrange
		var engine = new VirtualizationEngine(25.0, 10);

		// Act
		var range = engine.GetVisibleRange(0, engine.TotalHeight + 100);

		// Assert
		Assert.Equal(0, range.StartIndex);
		Assert.Equal(10, range.EndIndex);
	}

	/// <summary>
	/// 個別行高を設定した行を除き、既定行高の変更が総高に反映されるテスト
	/// </summary>
	[Fact]
	public void TotalHeight_WhenDefaultRowHeightChanges_PreservesCustomRowHeight() {
		// Arrange
		var engine = new VirtualizationEngine(25.0, 4);
		engine.SetRowHeight(1, 40.0);

		// Act
		engine.DefaultRowHeight = 30.0;

		// Assert
		Assert.Equal(130.0, engine.TotalHeight);
	}

	/// <summary>
	/// 行数を減らした場合に範囲外の個別行高を破棄し、再増加時は既定行高を使うテスト
	/// </summary>
	[Fact]
	public void TotalHeight_WhenRowCountShrinksAndGrows_DiscardsRemovedRowHeights() {
		// Arrange
		var engine = new VirtualizationEngine(25.0, 4);
		engine.SetRowHeight(3, 50.0);

		// Act
		engine.TotalRowCount = 3;
		var heightAfterShrink = engine.TotalHeight;
		engine.TotalRowCount = 4;

		// Assert
		Assert.Equal(75.0, heightAfterShrink);
		Assert.Equal(100.0, engine.TotalHeight);
	}

	/// <summary>
	/// 負の総行数を設定すると例外になるテスト
	/// </summary>
	[Fact]
	public void TotalRowCount_WhenNegative_ThrowsArgumentOutOfRangeException() {
		// Arrange
		var engine = new VirtualizationEngine(25.0, 1);

		// Act & Assert
		Assert.Throws<ArgumentOutOfRangeException>(() => engine.TotalRowCount = -1);
	}

	/// <summary>
	/// 負の総行数で初期化すると例外になるテスト
	/// </summary>
	[Fact]
	public void Constructor_WhenRowCountIsNegative_ThrowsArgumentOutOfRangeException() {
		// Act & Assert
		Assert.Throws<ArgumentOutOfRangeException>(() => new VirtualizationEngine(25.0, -1));
	}

	/// <summary>
	/// 0以下または非有限の既定行高を設定すると例外になるテスト
	/// </summary>
	[Theory]
	[InlineData(0.0)]
	[InlineData(-1.0)]
	[InlineData(double.NaN)]
	[InlineData(double.PositiveInfinity)]
	[InlineData(double.NegativeInfinity)]
	public void DefaultRowHeight_WhenHeightIsInvalid_ThrowsArgumentOutOfRangeException(double height) {
		// Arrange
		var engine = new VirtualizationEngine(25.0, 1);

		// Act & Assert
		Assert.Throws<ArgumentOutOfRangeException>(() => engine.DefaultRowHeight = height);
	}

	/// <summary>
	/// 0以下または非有限の既定行高で初期化すると例外になるテスト
	/// </summary>
	[Theory]
	[InlineData(0.0)]
	[InlineData(-1.0)]
	[InlineData(double.NaN)]
	[InlineData(double.PositiveInfinity)]
	[InlineData(double.NegativeInfinity)]
	public void Constructor_WhenDefaultRowHeightIsInvalid_ThrowsArgumentOutOfRangeException(double height) {
		// Act & Assert
		Assert.Throws<ArgumentOutOfRangeException>(() => new VirtualizationEngine(height, 1));
	}

	/// <summary>
	/// 範囲外の行インデックスに個別行高を設定すると例外になるテスト
	/// </summary>
	[Theory]
	[InlineData(-1)]
	[InlineData(2)]
	public void SetRowHeight_WhenRowIndexIsOutOfRange_ThrowsArgumentOutOfRangeException(int rowIndex) {
		// Arrange
		var engine = new VirtualizationEngine(25.0, 2);

		// Act & Assert
		Assert.Throws<ArgumentOutOfRangeException>(() => engine.SetRowHeight(rowIndex, 30.0));
	}

	/// <summary>
	/// 0以下または非有限の個別行高を設定すると例外になるテスト
	/// </summary>
	[Theory]
	[InlineData(0.0)]
	[InlineData(-1.0)]
	[InlineData(double.NaN)]
	[InlineData(double.PositiveInfinity)]
	[InlineData(double.NegativeInfinity)]
	public void SetRowHeight_WhenHeightIsInvalid_ThrowsArgumentOutOfRangeException(double height) {
		// Arrange
		var engine = new VirtualizationEngine(25.0, 1);

		// Act & Assert
		Assert.Throws<ArgumentOutOfRangeException>(() => engine.SetRowHeight(0, height));
	}

	/// <summary>
	/// 非有限のスクロールオフセットを指定すると例外になるテスト
	/// </summary>
	[Theory]
	[InlineData(double.NaN)]
	[InlineData(double.PositiveInfinity)]
	[InlineData(double.NegativeInfinity)]
	public void GetVisibleRange_WhenScrollOffsetIsNotFinite_ThrowsArgumentOutOfRangeException(double scrollOffset) {
		// Arrange
		var engine = new VirtualizationEngine(25.0, 1);

		// Act & Assert
		Assert.Throws<ArgumentOutOfRangeException>(() => engine.GetVisibleRange(scrollOffset, 25.0));
	}

	/// <summary>
	/// 負または非有限のビューポート高を指定すると例外になるテスト
	/// </summary>
	[Theory]
	[InlineData(-1.0)]
	[InlineData(double.NaN)]
	[InlineData(double.PositiveInfinity)]
	[InlineData(double.NegativeInfinity)]
	public void GetVisibleRange_WhenViewportHeightIsInvalid_ThrowsArgumentOutOfRangeException(double viewportHeight) {
		// Arrange
		var engine = new VirtualizationEngine(25.0, 1);

		// Act & Assert
		Assert.Throws<ArgumentOutOfRangeException>(() => engine.GetVisibleRange(0, viewportHeight));
	}
}
