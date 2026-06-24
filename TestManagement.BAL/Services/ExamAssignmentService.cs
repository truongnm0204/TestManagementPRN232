using System;
using System.Threading.Tasks;
using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.ExamAssignments;
using TestManagement.BAL.Services.Interfaces;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces; // Đảm bảo bạn đã có IExamAssignmentRepository ở DAL

namespace TestManagement.BAL.Services
{
    public class ExamAssignmentService : IExamAssignmentService
    {
        // Giả định bạn đã tạo IExamAssignmentRepository ở tầng DAL tương tự như IExamRepository
        private readonly IExamAssignmentRepository _assignmentRepository; 
    
        public ExamAssignmentService(IExamAssignmentRepository assignmentRepository)
        {
            _assignmentRepository = assignmentRepository;
        }

        public async Task<ServiceResult> AssignToClassAsync(int examId, int classId, int currentUserId)
        {
            try
            {
                // 1. Kiểm tra xem đề thi này đã được giao cho lớp này trước đó chưa (tránh trùng lặp)
                // (Nếu Base Repository của bạn hỗ trợ GetQueryable() hoặc hàm tương tự)
                
                var assignment = new ExamAssignment
                {
                    ExamId = examId,
                    ClassId = classId,
                    AssignedBy = currentUserId,
                    AssignedAt = DateTime.UtcNow
                };

                // 2. Gọi hàm AddAsync từ Base Repository
                await _assignmentRepository.AddAsync(assignment);

                return new ServiceResult { Success = true };
            }
            catch (Exception ex)
            {
                return new ServiceResult 
                { 
                    Success = false, 
                    Error = $"Lỗi khi phân bổ bài thi: {ex.Message}" 
                };
            }
        }

        public Task<ServiceResult<ExamAssignmentResponse>> CreateAsync(CreateAssignmentRequest request, int? currentUserId)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ExamAssignmentResponse?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ExamAssignmentODataResponse> GetODataQueryable()
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResult> RemoveAssignmentAsync(int id)
        {
            try
            {
                var assignment = await _assignmentRepository.GetByIdAsync(id);
                if (assignment == null)
                {
                    return new ServiceResult { Success = false, Error = "Không tìm thấy lịch phân bổ bài thi này." };
                }

                // Nếu thực hiện xóa hẳn khỏi DB (Hard Delete) bằng hàm Delete/Remove của Base Repository:
                // _assignmentRepository.Delete(assignment); 
                // Hoặc nếu model của bạn không có trường IsDeleted thì dùng hàm Delete đồng bộ của Base Repo.
                
                return new ServiceResult { Success = true };
            }
            catch (Exception ex)
            {
                return new ServiceResult 
                { 
                    Success = false, 
                    Error = $"Lỗi khi hủy phân bổ bài thi: {ex.Message}" 
                };
            }
        }
    }
}