using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Reactive;
using Avalonia.VisualTree;

namespace SideScroll.Avalonia.Controls.DataGrids;

public class DataGridButton : Button
{
	private DataGridRow? _row;
	private IDisposable? _isSelectedSub;
	private IDisposable? _isPointerOverSub;
	private IDisposable? _parentSub;

	public DataGridButton(string? label = null)
	{
		Content = label;
	}

	public void BindVisible(string propertyName)
	{
		var binding = new Binding(propertyName)
		{
			Path = propertyName,
			Mode = BindingMode.OneWay,
		};
		Bind(IsVisibleProperty, binding);
	}

	protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnAttachedToVisualTree(e);

		// Track parent chain changes so we can re-find the row if needed
		_parentSub = this.GetObservable(VisualParentProperty)
						 .Subscribe(new AnonymousObserver<Visual?>(RehookRow));

		RehookRow(this);
	}

	protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnDetachedFromVisualTree(e);
		_parentSub?.Dispose();
		_parentSub = null;

		UnhookRow();
	}

	private void RehookRow(Visual? visual)
	{
		var newRow = this.FindAncestorOfType<DataGridRow>();
		if (ReferenceEquals(newRow, _row))
			return;

		UnhookRow();
		_row = newRow;

		if (_row is not null)
		{
			var isSelectedObservable = _row.GetObservable(DataGridRow.IsSelectedProperty);
			_isSelectedSub = isSelectedObservable.Subscribe(new AnonymousObserver<bool>(UpdateVisibility));

			var isPointerOverObservable = _row.GetObservable(IsPointerOverProperty);
			_isPointerOverSub = isPointerOverObservable.Subscribe(new AnonymousObserver<bool>(UpdateVisibility));
		}

		UpdateVisibility(false);
	}

	private void UnhookRow()
	{
		_isSelectedSub?.Dispose();
		_isSelectedSub = null;

		_isPointerOverSub?.Dispose();
		_isPointerOverSub = null;

		_row = null;
	}

	private void UpdateVisibility(bool visible)
	{
		IsVisible = _row is not null && (_row.IsSelected || _row.IsPointerOver);
		// If you want to avoid column width changes, do this instead:
		// Opacity = _row is not null && (_row.IsSelected || _row.IsPointerOver) ? 1 : 0;
		// IsHitTestVisible = Opacity == 1;
	}
}
