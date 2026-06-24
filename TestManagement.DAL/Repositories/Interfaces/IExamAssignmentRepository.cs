using System.Linq;
using System.Threading.Tasks;
using TestManagement.DAL.Models;

namespace TestManagement.DAL.Repositories.Interfaces
{
    public interface IExamAssignmentRepository : IRepository<ExamAssignment>
    {
        Task<ExamAssignment?> GetDetailAsync(int id);
        IQueryable<ExamAssignment> GetQueryable();
    }
}