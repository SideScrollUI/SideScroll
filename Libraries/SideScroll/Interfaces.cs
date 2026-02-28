namespace SideScroll;

// TabViewer will show loaded object instead
public interface ILoadAsync
{
	Task<object?> LoadAsync(Call call);
}
