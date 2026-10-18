using System;
using System.ComponentModel;

namespace YumCommu.UI;

/// <summary>
/// 並べ替えの条件 (対象の列と方向) を表します。
/// </summary>
/// <param name="Column">並べ替えの対象列</param>
/// <param name="Direction">並べ替えの方向</param>
public readonly record struct RecordGridSortDescription(RecordGridColumn Column, ListSortDirection Direction);
