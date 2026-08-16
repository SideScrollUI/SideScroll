namespace SideScroll.Extensions;

/// <summary>
/// Extension methods for reading streams with a bound on how much they produce
/// </summary>
public static class StreamExtensions
{
	/// <summary>
	/// The buffer size <see cref="Stream.CopyTo(Stream)"/> uses
	/// </summary>
	private const int DefaultCopyBufferSize = 81920;

	/// <summary>
	/// Copies up to <paramref name="maxBytes"/> from one stream to another, returning false without
	/// writing past it
	/// </summary>
	/// <remarks>
	/// <para>
	/// <see cref="Stream.CopyTo(Stream)"/> reads until the source runs out, which is decided by the
	/// source. That's fine for data this process wrote and not for a compressed payload it was
	/// handed, where the size of what arrives says nothing about how far it expands.
	/// </para>
	/// <para>
	/// The count is checked before each write, so nothing past the limit reaches the destination.
	/// Returns rather than throwing, since a caller reporting a payload it can't accept has more to
	/// say about it than this does
	/// </para>
	/// </remarks>
	/// <returns>true if the whole source was copied, false if it reached the limit first</returns>
	public static bool TryCopyUpTo(this Stream source, Stream destination, long maxBytes)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(destination);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxBytes);

		byte[] buffer = new byte[DefaultCopyBufferSize];
		long total = 0;

		int read;
		while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
		{
			total += read;
			if (total > maxBytes)
				return false;

			destination.Write(buffer, 0, read);
		}
		return true;
	}
}
