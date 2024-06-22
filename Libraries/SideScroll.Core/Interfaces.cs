namespace SideScroll.Core;

public interface ILoadAsync
{
	Task<object?> LoadAsync(Call call);
}

// Called after object loaded when deserializing
public interface IReload
{
	void Reload();
}
