using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SideScroll.Attributes;
using SideScroll.Avalonia.Controls.Viewer;
using SideScroll.Avalonia.Utilities;
using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.Tabs.Toolbar;
using SideScroll.Tabs.Tools.FileViewer;

namespace SideScroll.Avalonia.Tabs;

public class TabFileImage : ITab, IFileTypeView
{
	public static string[] DefaultExtensions { get; set; } =
		[".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp", ".svg", ".ico"];

	public static void Register()
	{
		TabFile.RegisterType<TabFileImage>(DefaultExtensions);
	}

	public string? Path { get; set; }

	public TabFileImage() { }

	public TabFileImage(string path)
	{
		Path = path;
	}

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonRotateLeft { get; set; } = new("Rotate Left", Icons.Svg.RotateLeft);
		public ToolButton ButtonRotateRight { get; set; } = new("Rotate Right", Icons.Svg.RotateRight);

		[Separator]
		public ToolButton ButtonCopy { get; set; } = new("Copy to Clipboard", Icons.Svg.Copy);
	}

	private class Instance(TabFileImage tab) : TabInstance
	{
		public static int MinDesiredWidth { get; set; } = 100;

		public string Path => tab.Path!;

		private Image? _image;
		private LayoutTransformControl? _imageContainer;
		private int _rotation;

		public override void LoadUI(Call call, TabModel model)
		{
			model.CustomSettingsPath = Path;

			if (!File.Exists(Path))
			{
				model.AddObject("File doesn't exist");
				return;
			}

			Toolbar toolbar = new();
			toolbar.ButtonRotateLeft.Action = RotateLeft;
			toolbar.ButtonRotateRight.Action = RotateRight;
			toolbar.ButtonCopy.ActionAsync = CopyToClipboardAsync;
			model.AddObject(toolbar);

			_image = new Image
			{
				VerticalAlignment = VerticalAlignment.Top,
			};

			try
			{
				if (Path.ToLower().EndsWith(".svg"))
				{
					if (SvgUtils.TryGetSvgImage(call, Path, out IImage? imageSource))
					{
						_image.Source = imageSource;
						model.MaxDesiredWidth = Math.Max(MinDesiredWidth, (int)imageSource.Size.Width);
					}
				}
				else
				{
					Bitmap bitmap = ImageUtils.LoadImage(_image, Path);
					model.MaxDesiredWidth = Math.Max(MinDesiredWidth, (int)bitmap.Size.Width);
				}

				// Wrap in a LayoutTransformControl so rotations also update the layout bounds
				// (e.g. a 90° rotation swaps the displayed width and height)
				_imageContainer = new LayoutTransformControl
				{
					Child = _image,
					VerticalAlignment = VerticalAlignment.Top,
				};
				UpdateRotation();

				model.AddObject(_imageContainer, true);
			}
			catch (Exception ex)
			{
				call.Log.Add(ex);
				model.AddObject(ex);
			}
		}

		private void RotateLeft(Call call)
		{
			_rotation = (_rotation + 270) % 360;
			UpdateRotation();
		}

		private void RotateRight(Call call)
		{
			_rotation = (_rotation + 90) % 360;
			UpdateRotation();
		}

		private void UpdateRotation()
		{
			if (_imageContainer == null)
				return;

			_imageContainer.LayoutTransform = new RotateTransform(_rotation);
		}

		private async Task CopyToClipboardAsync(Call call)
		{
			if (_image?.Source is Bitmap bitmap)
			{
				await ClipboardUtils.SetBitmapAsync(TabViewer.Instance, bitmap);
			}
		}
	}
}
