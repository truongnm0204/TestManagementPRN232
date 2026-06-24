using System.Linq;
using System.Threading.Tasks;
using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.ExamAssignments; // Bạn cần tạo các DTOs tương ứng ở đây nếu chưa có

namespace TestManagement.BAL.Services.Interfaces
{
    public interface IExamAssignmentService
    {
        IQueryable<ExamAssignmentODataResponse> GetODataQueryable();
        Task<ExamAssignmentResponse?> GetByIdAsync(int id);
        Task<ServiceResult<ExamAssignmentResponse>> CreateAsync(CreateAssignmentRequest request, int? currentUserId);
        Task<ServiceResult> DeleteAsync(int id);
    }
}