using Microsoft.EntityFrameworkCore;
using WineShop.Data;
using WineShop.Models;
using WineShop.Services.Interfaces;

namespace WineShop.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _db;

        public CommentService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<int> AddAsync(string userId, int productId, string content)
        {
            var newComment = new Comment
            {
                Date = DateTime.UtcNow,
                CommentContent = content,
                IdCustomer = userId,
                IdProduct = productId
            };

            _db.Comment.Add(newComment);
            await _db.SaveChangesAsync();

            return productId;
        }

        public async Task<DeleteCommentResult> DeleteAsync(int commentId, string userId, bool isAdmin)
        {
            var comment = await _db.Comment.FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment is null)
            {
                return new DeleteCommentResult(DeleteCommentStatus.NotFound, null);
            }

            if (!isAdmin && comment.IdCustomer != userId)
            {
                return new DeleteCommentResult(DeleteCommentStatus.Forbidden, comment.IdProduct);
            }

            var productId = comment.IdProduct;

            _db.Comment.Remove(comment);
            await _db.SaveChangesAsync();

            return new DeleteCommentResult(DeleteCommentStatus.Success, productId);
        }
    }
}