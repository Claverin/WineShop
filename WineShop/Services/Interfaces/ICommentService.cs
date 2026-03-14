namespace WineShop.Services.Interfaces
{
    public interface ICommentService
    {
        Task<int> AddAsync(string userId, int productId, string content);
        Task<DeleteCommentResult> DeleteAsync(int commentId, string userId, bool isAdmin);
    }
}