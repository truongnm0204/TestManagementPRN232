using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TestManagement.DAL.Data;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces;

namespace TestManagement.DAL.Repositories
{
    public class ExamAssignmentRepository : Repository<ExamAssignment>, IExamAssignmentRepository
    {
        public ExamAssignmentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ExamAssignment?> GetDetailAsync(int id)
        {
            return await Context.ExamAssignments
                .Include(x => x.Exam)
                .Include(x => x.Class)
                .Include(x => x.Assigner)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public IQueryable<ExamAssignment> GetQueryable()
        {
            return Context.ExamAssignments.AsNoTracking()
                .Include(x => x.Exam)
                .Include(x => x.Class)
                .Include(x => x.Assigner);
        }
    }
}