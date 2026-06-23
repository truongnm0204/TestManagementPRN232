using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.Exams;

namespace TestManagement.BAL.Services.Interfaces
{
    public interface IExamService
    {
        IQueryable<ExamODataResponse> GetOdataQueryable();
        Task<ExamResponse> GetByIdAsync(int id);
        Task<ServiceResult<ExamResponse>> CreateAsync(CreateExamRequest request, int? currentUserId);
        Task<ServiceResult> UpdateAsync(int id, UpdateExamRequest request);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
