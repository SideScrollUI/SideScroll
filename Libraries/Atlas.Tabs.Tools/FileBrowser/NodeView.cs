using Atlas.Core;
using Atlas.Core.Utilities;
using Atlas.Extensions;
using Atlas.Serialize;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Atlas.Tabs.Tools;

// Shows if files present
public abstract class NodeView : IHasLinks, INotifyPropertyChanged
{
	[Unserialized]
	public DataRepoView<NodeView>? DataRepoFavorites;

	[Name("  ★"), Editing]
	public bool Favorite
	{
		get => _favorite;
		set
		{
			_favorite = value;
			UpdateDataRepo();
			NotifyPropertyChanged();
		}
	}
	private bool _favorite;

	public abstract string Name { get; }

	[StyleValue, Formatter(typeof(ByteFormatter))]
	public abstract long? Size { get; set; }

	[StyleValue, Formatted]
	public abstract TimeSpan Modified { get; }

	[Hidden]
	public abstract bool HasLinks { get; }

	[DataKey]
	public string Path;

	[InnerValue, Unserialized]
	public ITab? Tab;

	public event PropertyChangedEventHandler? PropertyChanged;

	public override string ToString() => Name;

	protected NodeView(string path)
		: this(path, null)
	{ }

	protected NodeView(string path, DataRepoView<NodeView>? dataRepoFavorites = null)
	{
		Path = path;
		DataRepoFavorites = dataRepoFavorites;
		_favorite = dataRepoFavorites?.Items.TryGetValue(Path, out _) == true;
	}

	private void UpdateDataRepo()
	{
		if (DataRepoFavorites == null) return;

		if (_favorite)
			DataRepoFavorites.Save(null, Path, this);
		else
			DataRepoFavorites.Delete(null, Path);
	}

	protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

// Shows if only directories present
public interface IDirectoryView : IHasLinks
{
	[Name("  ★"), Editing]
	public bool Favorite { get; set; }

	public string Name { get; }
}

public class DirectoryView : NodeView, IDirectoryView
{
	public string Directory { get; set; }

	public override string Name => Directory;
	public override long? Size { get; set; } = null;
	public DateTime LastWriteTime { get; set; }
	public override TimeSpan Modified => LastWriteTime.Age();
	public override bool HasLinks => true;

	public DirectoryView(string path)
		: this(path, null)
	{ }

	public DirectoryView(string path, DataRepoView<NodeView>? dataRepoFavorites = null) :
		base(path, dataRepoFavorites)
	{
		Directory = System.IO.Path.GetFileName(path);
		var info = new DirectoryInfo(path);
		LastWriteTime = info.LastWriteTime.Trim();
		Tab = new TabDirectory(this);
	}
}

public class FileView : NodeView
{
	public string Filename { get; set; }
	public override long? Size { get; set; }
	public DateTime LastWriteTime { get; set; }
	public override TimeSpan Modified => LastWriteTime.Age();
	public override bool HasLinks => false;

	public override string Name => Filename;

	public FileInfo FileInfo;

	public FileView(string path)
		: this(path, null)
	{ }

	public FileView(string path, DataRepoView<NodeView>? dataRepoFavorites = null)
		: base(path, dataRepoFavorites)
	{
		FileInfo = new FileInfo(path);
		Filename = System.IO.Path.GetFileName(path);
		Size = FileInfo.Length;
		LastWriteTime = FileInfo.LastWriteTime.Trim();

		if (Filename.EndsWith(".atlas"))
			Tab = new TabFileSerialized(path);
		else
			Tab = new TabFile(this);
	}
}
