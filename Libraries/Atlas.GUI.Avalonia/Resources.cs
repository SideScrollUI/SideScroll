using Atlas.Resources;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System.IO;
using System.Reflection;

namespace Atlas.GUI.Avalonia
{
	public class Resources
	{
		public static Bitmap GetBitmap(string name)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = name;

			Bitmap bitmap;
			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			{
				bitmap = new Bitmap(stream);
			}
			return bitmap;
		}

		public static Image GetImage(Bitmap bitmap)
		{
			Image imageBack = new Image()
			{
				Source = bitmap,
			};
			return imageBack;
		}

		public class Bitmaps
		{
			public static Bitmap Help { get { return new Bitmap(Assets.Streams.Forward); } }
			public static Bitmap Info { get { return new Bitmap(Assets.Streams.Info); } }
		}

		public class Images
		{
			public static Image Help { get { return GetImage(Bitmaps.Help); } }
			public static Image Info { get { return GetImage(Bitmaps.Info); } }
		}
	}
}
