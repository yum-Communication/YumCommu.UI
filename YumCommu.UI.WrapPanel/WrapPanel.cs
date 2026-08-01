using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace YumCommu.UI;

/// <summary>アイテムを配置する主方向を表します。</summary>
public enum WrapPanelDirection {
    /// <summary>水平方向（左から右）に配置します。</summary>
    LeftToRight,
    /// <summary>水平方向（右から左）に配置します。</summary>
    RightToLeft,
    /// <summary>垂直方向（上から下）に配置します。</summary>
    TopToDown,
    /// <summary>垂直方向（下から上）に配置します。</summary>
    DownToTop
}

/// <summary>折り返し行の積み方向を表します。</summary>
public enum WrapPanelSubDirection {
    /// <summary>順方向（水平レイアウトでは上から下、垂直レイアウトでは左から右）に積み重ねます。</summary>
    Normal,
    /// <summary>逆方向（水平レイアウトでは下から上、垂直レイアウトでは右から左）に積み重ねます。</summary>
    Reverse
}

/// <summary>主軸方向のアイテムの揃え方を表します。</summary>
public enum WrapPanelJustifyContent {
    /// <summary>先頭側に寄せます。</summary>
    Start,
    /// <summary>末尾側に寄せます。</summary>
    End,
    /// <summary>中央に揃えます。</summary>
    Center,
    /// <summary>両端揃えにします（最初と最後のアイテムが両端に付き、残りのスペースを均等に分配します）。</summary>
    SpaceBetween,
    /// <summary>均等配置にします（各アイテムの両端に均等な余白が入ります）。</summary>
    SpaceAround,
    /// <summary>
    /// 隙間ではなく各アイテム自身を拡大して余白を埋めます。
    /// 各アイテムの主軸サイズの比率（子要素の最小サイズでの比率）を維持したまま拡大します。
    /// <see cref="WrapPanel.AlignContent"/> に指定した場合は <see cref="Start"/> と同様に扱われます。
    /// </summary>
    Stretch
}

/// <summary>交差軸方向の各アイテムの揃え方を表します。</summary>
public enum WrapPanelAlignItems {
    /// <summary>先頭側に寄せます。</summary>
    Start,
    /// <summary>末尾側に寄せます。</summary>
    End,
    /// <summary>中央に揃えます。</summary>
    Center
}

/// <summary>
/// CSS の Flexbox に近い動作をする折り返しパネル。
/// <see cref="Direction"/> で主軸方向を指定し、アイテムが収まらない場合は
/// 次の行または列に折り返します。
/// </summary>
public partial class WrapPanel : Panel {

    /// <summary>アイテムを並べる主方向。</summary>
    public WrapPanelDirection Direction {
        get => (WrapPanelDirection)GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    /// <summary><see cref="Direction"/> 依存関係プロパティを識別します。</summary>
    public static readonly DependencyProperty DirectionProperty;

    /// <summary>折り返し行の積み方向。</summary>
    public WrapPanelSubDirection SubDirection {
        get => (WrapPanelSubDirection)GetValue(SubDirectionProperty);
        set => SetValue(SubDirectionProperty, value);
    }

    /// <summary><see cref="SubDirection"/> 依存関係プロパティを識別します。</summary>
    public static readonly DependencyProperty SubDirectionProperty;

    /// <summary>主軸方向のアイテムの揃え方。</summary>
    public WrapPanelJustifyContent JustifyContent {
        get => (WrapPanelJustifyContent)GetValue(JustifyContentProperty);
        set => SetValue(JustifyContentProperty, value);
    }

    /// <summary><see cref="JustifyContent"/> 依存関係プロパティを識別します。</summary>
    public static readonly DependencyProperty JustifyContentProperty;

    /// <summary>各行内でのアイテムの交差軸方向の揃え方。</summary>
    public WrapPanelAlignItems AlignItems {
        get => (WrapPanelAlignItems)GetValue(AlignItemsProperty);
        set => SetValue(AlignItemsProperty, value);
    }

    /// <summary><see cref="AlignItems"/> 依存関係プロパティを識別します。</summary>
    public static readonly DependencyProperty AlignItemsProperty;

    /// <summary>複数行の交差軸方向の揃え方。</summary>
    public WrapPanelJustifyContent AlignContent {
        get => (WrapPanelJustifyContent)GetValue(AlignContentProperty);
        set => SetValue(AlignContentProperty, value);
    }

    /// <summary><see cref="AlignContent"/> 依存関係プロパティを識別します。</summary>
    public static readonly DependencyProperty AlignContentProperty;

    /// <summary>依存関係プロパティを静的に初期化します。</summary>
    static WrapPanel() {
        DirectionProperty = DependencyProperty.Register(
            nameof(Direction), typeof(WrapPanelDirection), typeof(WrapPanel),
            new PropertyMetadata(WrapPanelDirection.LeftToRight, OnLayoutPropertyChanged));
        SubDirectionProperty = DependencyProperty.Register(
            nameof(SubDirection), typeof(WrapPanelSubDirection), typeof(WrapPanel),
            new PropertyMetadata(WrapPanelSubDirection.Normal, OnLayoutPropertyChanged));
        JustifyContentProperty = DependencyProperty.Register(
            nameof(JustifyContent), typeof(WrapPanelJustifyContent), typeof(WrapPanel),
            new PropertyMetadata(WrapPanelJustifyContent.Start, OnLayoutPropertyChanged));
        AlignItemsProperty = DependencyProperty.Register(
            nameof(AlignItems), typeof(WrapPanelAlignItems), typeof(WrapPanel),
            new PropertyMetadata(WrapPanelAlignItems.Start, OnLayoutPropertyChanged));
        AlignContentProperty = DependencyProperty.Register(
            nameof(AlignContent), typeof(WrapPanelJustifyContent), typeof(WrapPanel),
            new PropertyMetadata(WrapPanelJustifyContent.Start, OnLayoutPropertyChanged));
    }

    /// <summary>
    /// レイアウトに影響する依存関係プロパティが変更されたときに呼び出されます。
    /// 再メジャーを要求します。
    /// </summary>
    /// <param name="d">プロパティが変更された <see cref="WrapPanel"/> インスタンス。</param>
    /// <param name="e">変更前後の値を含むイベントデータ。</param>
    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        ((WrapPanel)d).InvalidateMeasure();
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize) {
        bool isHorizontal = Direction is WrapPanelDirection.LeftToRight or WrapPanelDirection.RightToLeft;
        double availableMain = isHorizontal ? availableSize.Width : availableSize.Height;
        double availableCross = isHorizontal ? availableSize.Height : availableSize.Width;

        List<LineInfo> lines = BuildLines(availableMain, availableCross, isHorizontal);

        double desiredMain = 0;
        double desiredCross = 0;
        foreach (LineInfo line in lines) {
            if (line.MainSize > desiredMain) {
                desiredMain = line.MainSize;
            }
            desiredCross += line.CrossSize;
        }

        if (isHorizontal) {
            return new Size(
                double.IsInfinity(availableMain) ? desiredMain : availableMain,
                desiredCross);
        } else {
            return new Size(
                desiredCross,
                double.IsInfinity(availableMain) ? desiredMain : availableMain);
        }
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize) {
        bool isHorizontal = Direction is WrapPanelDirection.LeftToRight or WrapPanelDirection.RightToLeft;
        bool isReverseMain = Direction is WrapPanelDirection.RightToLeft or WrapPanelDirection.DownToTop;
        bool isReverseCross = SubDirection is WrapPanelSubDirection.Reverse;

        double finalMain = isHorizontal ? finalSize.Width : finalSize.Height;
        double finalCross = isHorizontal ? finalSize.Height : finalSize.Width;

        List<LineInfo> lines = BuildLines(finalMain, finalCross, isHorizontal);
        if (lines.Count == 0) {
            return finalSize;
        }

        // SubDirection.Reverse の場合、行の積み順を逆にする
        if (isReverseCross) {
            lines.Reverse();
        }

        // AlignContent に基づいて各行の交差軸開始位置を計算する
        double[] crossSizes = new double[lines.Count];
        for (int i = 0; i < lines.Count; i++) {
            crossSizes[i] = lines[i].CrossSize;
        }
        // SubDirection.Reverse の場合、AlignContent の Start/End の意味が反転する
        WrapPanelJustifyContent effectiveAlignContent = FlipStartEnd(AlignContent, isReverseCross);
        double[] lineCrossPositions = Distribute(finalCross, crossSizes, effectiveAlignContent);

        for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++) {
            LineInfo line = lines[lineIndex];
            double lineCrossStart = lineCrossPositions[lineIndex];

            // JustifyContent に基づいて各アイテムの主軸開始位置（と Stretch 時のサイズ）を計算する
            double[] mainSizes = new double[line.Items.Count];
            for (int i = 0; i < line.Items.Count; i++) {
                (UIElement _, Size desiredSize) = line.Items[i];
                mainSizes[i] = isHorizontal ? desiredSize.Width : desiredSize.Height;
            }
            (double[] itemMainPositions, double[] itemMainSizes) = DistributeMain(finalMain, mainSizes, JustifyContent);

            for (int itemIndex = 0; itemIndex < line.Items.Count; itemIndex++) {
                (UIElement child, Size desired) = line.Items[itemIndex];
                double childMain = itemMainSizes[itemIndex];
                double childCross = isHorizontal ? desired.Height : desired.Width;

                double mainPos = itemMainPositions[itemIndex];
                double crossPos = CalcAlignItemsPos(lineCrossStart, line.CrossSize, childCross, AlignItems);

                // Direction が末尾起点の場合、主軸位置を反転する
                if (isReverseMain) {
                    mainPos = finalMain - mainPos - childMain;
                }

                Rect rect;
                if (isHorizontal) {
                    rect = new Rect(mainPos, crossPos, childMain, childCross);
                } else {
                    rect = new Rect(crossPos, mainPos, childCross, childMain);
                }

                child.Arrange(rect);
            }
        }

        return finalSize;
    }

    /// <summary>
    /// 利用可能サイズに基づいてアイテムを行（または列）に分割します。
    /// </summary>
    /// <param name="availableMain">主軸の利用可能サイズ。無限大の場合は折り返しなしで全アイテムを 1 行に収めます。</param>
    /// <param name="availableCross">交差軸の利用可能サイズ。子のメジャーパスに渡します。</param>
    /// <param name="isHorizontal">主軸が水平方向かどうか。</param>
    /// <returns>構築されたラインのリスト。</returns>
    private List<LineInfo> BuildLines(double availableMain, double availableCross, bool isHorizontal) {
        List<LineInfo> lines = [];
        List<(UIElement Child, Size DesiredSize)> currentItems = [];
        double currentMainSize = 0.0;
        double currentCrossSize = 0.0;

        Size measureSize = isHorizontal
            ? new Size(double.PositiveInfinity, double.IsInfinity(availableCross) ? double.PositiveInfinity : availableCross)
            : new Size(double.IsInfinity(availableCross) ? double.PositiveInfinity : availableCross, double.PositiveInfinity);

        foreach (UIElement child in Children) {
            child.Measure(measureSize);
            Size desired = child.DesiredSize;
            double childMain = isHorizontal ? desired.Width : desired.Height;
            double childCross = isHorizontal ? desired.Height : desired.Width;

            // 現在の行にアイテムがあり、追加すると主軸サイズを超える場合は新しい行を開始する
            bool exceedsMain = !double.IsInfinity(availableMain) && currentMainSize + childMain > availableMain;
            if (currentItems.Count > 0 && exceedsMain) {
                lines.Add(new LineInfo(currentItems, currentMainSize, currentCrossSize));
                currentItems = [];
                currentMainSize = 0.0;
                currentCrossSize = 0.0;
            }

            currentItems.Add((child, desired));
            currentMainSize += childMain;
            if (childCross > currentCrossSize) {
                currentCrossSize = childCross;
            }
        }

        if (currentItems.Count > 0) {
            lines.Add(new LineInfo(currentItems, currentMainSize, currentCrossSize));
        }

        return lines;
    }

    /// <summary>
    /// 指定されたサイズ内にアイテムを分布させ、各アイテムの開始位置の配列を返します。
    /// </summary>
    /// <param name="totalSize">利用可能な合計サイズ。</param>
    /// <param name="sizes">各アイテムのサイズ配列。</param>
    /// <param name="mode">分布モード。</param>
    /// <returns>各アイテムの開始位置の配列。</returns>
    private static double[] Distribute(double totalSize, double[] sizes, WrapPanelJustifyContent mode) {
        int count = sizes.Length;
        if (count == 0) {
            return [];
        }

        double totalItemSize = 0;
        foreach (double s in sizes) {
            totalItemSize += s;
        }

        double[] positions = new double[count];
        double remaining = Math.Max(0, totalSize - totalItemSize);

        switch (mode) {
            case WrapPanelJustifyContent.Start: {
                double pos = 0;
                for (int i = 0; i < count; i++) {
                    positions[i] = pos;
                    pos += sizes[i];
                }
                break;
            }
            case WrapPanelJustifyContent.End: {
                double pos = remaining;
                for (int i = 0; i < count; i++) {
                    positions[i] = pos;
                    pos += sizes[i];
                }
                break;
            }
            case WrapPanelJustifyContent.Center: {
                double pos = remaining / 2;
                for (int i = 0; i < count; i++) {
                    positions[i] = pos;
                    pos += sizes[i];
                }
                break;
            }
            case WrapPanelJustifyContent.SpaceBetween: {
                if (count == 1) {
                    positions[0] = 0;
                } else {
                    double gap = remaining / (count - 1);
                    double pos = 0;
                    for (int i = 0; i < count; i++) {
                        positions[i] = pos;
                        pos += sizes[i] + gap;
                    }
                }
                break;
            }
            case WrapPanelJustifyContent.SpaceAround: {
                double gap = remaining / count;
                double pos = gap / 2;
                for (int i = 0; i < count; i++) {
                    positions[i] = pos;
                    pos += sizes[i] + gap;
                }
                break;
            }
            case WrapPanelJustifyContent.Stretch: {
                // このメソッドはサイズを変更できないため、Start と同様に扱う（実際の拡大処理は DistributeMain が担う）
                double pos = 0;
                for (int i = 0; i < count; i++) {
                    positions[i] = pos;
                    pos += sizes[i];
                }
                break;
            }
        }

        return positions;
    }

    /// <summary>
    /// 主軸方向にアイテムを分布させ、各アイテムの開始位置を計算します。
    /// <paramref name="mode"/> が <see cref="WrapPanelJustifyContent.Stretch"/> の場合は、
    /// <see cref="Distribute"/> による位置計算に加えて、各アイテムのサイズ比（子要素の最小サイズでの比率）を
    /// 維持したまま拡大し、隙間なく主軸を埋めます。
    /// </summary>
    /// <param name="totalSize">利用可能な主軸の合計サイズ。</param>
    /// <param name="sizes">各アイテムの主軸サイズ（子要素の最小サイズ）の配列。</param>
    /// <param name="mode">分布モード。</param>
    /// <returns>各アイテムの開始位置と、実際に配置する主軸サイズの組。</returns>
    private static (double[] Positions, double[] Sizes) DistributeMain(double totalSize, double[] sizes, WrapPanelJustifyContent mode) {
        if (mode != WrapPanelJustifyContent.Stretch) {
            return (Distribute(totalSize, sizes, mode), sizes);
        }

        int count = sizes.Length;
        if (count == 0) {
            return ([], []);
        }

        double totalItemSize = 0;
        foreach (double s in sizes) {
            totalItemSize += s;
        }

        double[] stretchedSizes = new double[count];
        if (totalItemSize <= 0 || totalSize <= totalItemSize) {
            // 拡大の余地がない場合は最小サイズをそのまま使用する
            Array.Copy(sizes, stretchedSizes, count);
        } else {
            // 全アイテムを同じ倍率で拡大することで、サイズ比を維持したまま隙間を埋める
            double scale = totalSize / totalItemSize;
            for (int i = 0; i < count; i++) {
                stretchedSizes[i] = sizes[i] * scale;
            }
        }

        double[] positions = new double[count];
        double position = 0;
        for (int i = 0; i < count; i++) {
            positions[i] = position;
            position += stretchedSizes[i];
        }

        return (positions, stretchedSizes);
    }

    /// <summary>
    /// 行の交差軸範囲内でのアイテムの開始位置を計算します。
    /// </summary>
    /// <param name="lineStart">行の交差軸開始位置。</param>
    /// <param name="lineSize">行の交差軸サイズ。</param>
    /// <param name="itemSize">アイテムの交差軸サイズ。</param>
    /// <param name="mode">揃え方。</param>
    /// <returns>アイテムの交差軸開始位置。</returns>
    private static double CalcAlignItemsPos(double lineStart, double lineSize, double itemSize, WrapPanelAlignItems mode) {
        return mode switch {
            WrapPanelAlignItems.Start => lineStart,
            WrapPanelAlignItems.End => lineStart + lineSize - itemSize,
            WrapPanelAlignItems.Center => lineStart + (lineSize - itemSize) / 2,
            _ => lineStart
        };
    }

    /// <summary>
    /// <paramref name="flip"/> が <see langword="true"/> のとき、
    /// <see cref="WrapPanelJustifyContent.Start"/> と <see cref="WrapPanelJustifyContent.End"/> を入れ替えます。
    /// </summary>
    /// <param name="mode">元の揃えモード。</param>
    /// <param name="flip">反転するかどうか。</param>
    /// <returns>変換後の揃えモード。</returns>
    private static WrapPanelJustifyContent FlipStartEnd(WrapPanelJustifyContent mode, bool flip) {
        if (!flip) {
            return mode;
        }
        return mode switch {
            WrapPanelJustifyContent.Start => WrapPanelJustifyContent.End,
            WrapPanelJustifyContent.End => WrapPanelJustifyContent.Start,
            _ => mode
        };
    }

    /// <summary>1 行（または 1 列）の情報を保持する内部クラス。</summary>
    private sealed class LineInfo {
        /// <summary>行内のアイテムとそのメジャー済みサイズ。</summary>
        public List<(UIElement Child, Size DesiredSize)> Items { get; }

        /// <summary>行内のアイテムの主軸サイズの合計。</summary>
        public double MainSize { get; }

        /// <summary>行内で最も大きい交差軸サイズ。</summary>
        public double CrossSize { get; }

        /// <summary><see cref="LineInfo"/> の新しいインスタンスを初期化します。</summary>
        /// <param name="items">行内のアイテムリスト。</param>
        /// <param name="mainSize">主軸サイズの合計。</param>
        /// <param name="crossSize">交差軸の最大サイズ。</param>
        public LineInfo(List<(UIElement Child, Size DesiredSize)> items, double mainSize, double crossSize) {
            Items = items;
            MainSize = mainSize;
            CrossSize = crossSize;
        }
    }
}
