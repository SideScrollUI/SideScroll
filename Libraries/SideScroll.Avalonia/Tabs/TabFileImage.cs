using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SideScroll.Attributes;
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
		public ToolButton ButtonOpenFolder { get; set; } = new("Open Folder", Icons.Svg.OpenFolder);

		[Separator]
		public ToolButton ButtonDelete { get; set; } = new("Delete", Icons.Svg.Delete);
	}

	public class Instance(TabFileImage tab) : TabInstance
	{
		public static int MinDesiredWidth { get; set; } = 100;

		public string Path => tab.Path!;

		private Image? _image;

		public override void LoadUI(Call call, TabModel model)
		{
			model.CustomSettingsPath = Path;

			if (!File.Exists(Path))
			{
				model.AddObject("File doesn't exist");
				return;
			}

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
				model.AddObject(_image, true);
			}
			catch (Exception ex)
			{
				call.Log.Add(ex);
				model.AddObject(ex);
			}
		}
	}
}
