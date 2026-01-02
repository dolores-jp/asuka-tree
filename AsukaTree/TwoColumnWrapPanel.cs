using System;
using System.Windows;
using System.Windows.Controls;

namespace AsukaTree
{
    /// <summary>
    /// 子要素を「上から下へ詰めて、溢れたら次の列へ」配置するパネル。
    /// Roots（最上段）用。列数は2想定だが可変にしてある。
    /// </summary>
    public sealed class TwoColumnWrapPanel : Panel
    {
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(
                nameof(Columns),
                typeof(int),
                typeof(TwoColumnWrapPanel),
                new FrameworkPropertyMetadata(2, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public int Columns
        {
            get => Math.Max(1, (int)GetValue(ColumnsProperty));
            set => SetValue(ColumnsProperty, value);
        }

        public static readonly DependencyProperty ColumnGapProperty =
            DependencyProperty.Register(
                nameof(ColumnGap),
                typeof(double),
                typeof(TwoColumnWrapPanel),
                new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public double ColumnGap
        {
            get => (double)GetValue(ColumnGapProperty);
            set => SetValue(ColumnGapProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // ScrollViewer配下でInfinityになると列分けできないので、ここでは幅だけ確保し、
            // 高さは「与えられた範囲」を前提に配置する（Window内なら有限になる）
            var cols = Columns;
            var gap = Math.Max(0, ColumnGap);

            double width = double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width;
            double height = double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height;

            double colWidth = width > 0
                ? (width - gap * (cols - 1)) / cols
                : double.PositiveInfinity;

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(new Size(colWidth, double.PositiveInfinity));
            }

            // 親が与えるサイズに合わせて描画する用途なので、ここは available を返す
            return new Size(width, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var cols = Columns;
            var gap = Math.Max(0, ColumnGap);

            double colWidth = (finalSize.Width - gap * (cols - 1)) / cols;
            if (colWidth < 0) colWidth = finalSize.Width;

            double colHeight = finalSize.Height;

            int col = 0;
            double x = 0;
            double y = 0;

            foreach (UIElement child in InternalChildren)
            {
                var h = child.DesiredSize.Height;

                // 収まりきらなければ次の列へ
                if (colHeight > 0 && y > 0 && (y + h) > colHeight && col < cols - 1)
                {
                    col++;
                    x = col * (colWidth + gap);
                    y = 0;
                }

                child.Arrange(new Rect(x, y, colWidth, h));
                y += h;
            }

            return finalSize;
        }
    }
}
