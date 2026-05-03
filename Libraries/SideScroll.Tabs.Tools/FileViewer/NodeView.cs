using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Serialize.DataRepos;
using SideScroll.Utilities;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SideScroll.Tabs.Tools.FileViewer;

/// <summary>Delegate invoked when a file path is selected in the file browser.</summary>
public delegate void SelectFileDelegate(Call call, string path);

/// <summary>
/// Options passed to a node view for integrating with the favorites repository and file selection callbacks.
/// </summary>
[Unserialized]
public class FileSelectorOptions
{
	/// <summary>Gets or sets the data repository view used to load and save favorite nodes.</summary>
	public DataRepoView<NodeView>? DataRepoFavorites { get; set; }

	/// <summary>Gets or sets the callback invoked when a file path is selected.</summary>
	public SelectFileDelegate? SelectFileDelegate { get; set; }

	/// <summary>Gets or sets the list of allowed file extensions, or <c>null</c> to allow all.</summary>
	public List<string>? FileExtensions { get; set; }
}

/// <summary>
/// Abstract base class for file and directory entries displayed in the file viewer.
/// Shown when files are present.
/// </summary>
public abstract class NodeView : IHasLinks, INotifyPropertyChanged
{
	/// <summary>Gets or sets the file selector options applied to this node.</summary>
	[Unserialized, HiddenColumn]
	public FileSelectorOptions? FileSelectorOptions { get; set; }

	/// <summary>Gets or sets whether this node is marked as a favorite.</summary>
	[Name("  ★"), EditColumn]
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

	/// <summary>Gets the display name of the node.</summary>
	public abstract string Name { get; }

	/// <summary>Gets or sets the size in bytes, or <c>null</c> if not applicable.</summary>
	[StyleValue, Formatter(typeof(ByteFormatter))]
	public abstract long? Size { get; set; }

	/// <summary>Gets the time elapsed since the node was last modified, or <c>null</c> if unavailable.</summary>
	[StyleValue, Formatted]
	public abstract TimeSpan? Modified { get; }

	/// <summary>Gets whether this node can be navigated into.</summary>
	[Hidden]
	public abstract bool HasLinks { get; }

	/// <summary>Gets the full file system path of this node, used as the data key.</summary>
	[DataKey, HiddenColumn]
	public string Path { get; }

	/// <summary>Gets or sets the inner tab used to display this node's content.</summary>
	[InnerValue, Unserialized, HiddenColumn]
	public ITab? Tab { get; set; }

	/// <summary>Raised when a property value changes.</summary>
	public event PropertyChangedEventHandler? PropertyChanged;

	public override string ToString() => Name;

	/// <summary>Initializes the node with its file system path and optional file selector options.</summary>
	protected NodeView(string path, FileSelectorOptions? fileSelectorOptions = null)
	{
		Path = path;
		FileSelectorOptions = fileSelectorOptions;
		_favorite = fileSelectorOptions?.DataRepoFavorites?.Items.TryGetValue(Path, out _) == true;
	}

	private void UpdateDataRepo()
	{
		if (FileSelectorOptions?.DataRepoFavorites is not { } dataRepoFavorites) return;

		if (_favorite)
		{
			dataRepoFavorites.Save(null, Path, this);
		}
		else
		{
			dataRepoFavorites.Delete(null, Path);
		}
	}

	/// <summary>Raises <see cref="PropertyChanged"/> for the specified property name.</summary>
	protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

/// <summary>
/// Interface for directory nodes shown when only directories are present.
/// </summary>
public interface IDirectoryView : IHasLinks
{
	/// <summary>Gets or sets whether this directory node is marked as a favorite.</summary>
	[Name("  ★"), EditColumn]
	public bool Favorite { get; set; }

	/// <summary>Gets the display name of the directory.</summary>
	public string Name { get; }
}

/// <summary>
/// Represents a file system directory entry in the file viewer.
/// </summary>
public class DirectoryView : NodeView, IDirectoryView
{
	/// <summary>Gets the directory name (last segment of the path).</summary>
	public string Directory { get; }

	public override string Name => Directory;
	public override long? Size { get; set; } = null;

	/// <summary>Gets the last write time of the directory.</summary>
	public DateTime LastWriteTime { get; }

	public override TimeSpan? Modified => LastWriteTime.Age();
	public override bool HasLinks => true;

	/// <summary>Initializes a directory view for the given path.</summary>
	public DirectoryView(string path, FileSelectorOptions? fileSelectorOptions = null) :
		base(path, fileSelectorOptions)
	{
		Directory = System.IO.Path.GetFileName(path);
		var info = new DirectoryInfo(path);
		LastWriteTime = info.LastWriteTime.Trim();
		Tab = new TabDirectory(this);
	}
}

/// <summary>
/// Represents a file entry in the file viewer.
/// </summary>
public class FileView : NodeView
{
	/// <summary>Gets the file name (last segment of the path).</summary>
	public string Filename { get; }

	public override long? Size { get; set; }

	/// <summary>Gets the last write time of the file.</summary>
	public DateTime? LastWriteTime { get; }

	public override TimeSpan? Modified => LastWriteTime?.Age();
	public override bool HasLinks => false;

	public override string Name => Filename;

	/// <summary>Gets the file system info for this file, or <c>null</c> if unavailable.</summary>
	[HiddenColumn]
	public FileInfo? FileInfo { get; }

	/// <summary>Initializes a file view for the given path, reading file info and selecting the appropriate tab.</summary>
	public FileView(string path, FileSelectorOptions? fileSelectorOptions = null)
		: base(path, fileSelectorOptions)
	{
		Filename = System.IO.Path.GetFileName(path);
		try
		{
			FileInfo = new FileInfo(path);
			Size = FileInfo.Length;
			LastWriteTime = FileInfo.LastWriteTime.Trim();
		}
		catch (Exception e)
		{
			Debug.WriteLine(e);
		}

		if (Filename.EndsWith(".atlas"))
		{
			Tab = new TabFileSerialized(path);
		}
		else
		{
			Tab = new TabFile(this);
		}
	}
}
