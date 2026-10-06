namespace WineShop.Services.Interfaces
{
    public enum DeleteCommentStatus
    {
        Success,
        NotFound,
        Forbidden
    }

    public record DeleteCommentResult(DeleteCommentStatus Status, int? ProductId);
}