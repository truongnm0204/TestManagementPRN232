using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestManagement.DAL.Data;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces;

namespace TestManagement.DAL.Repositories
{
    public class ExamRepository : Repository<Exam>, IExamRepository
    {
        public ExamRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<Exam?> GetDetailAsync(int id)
        {
            return await Context.Exams
                .Include(x=> x.Subject)
                .Include(x=> x.Creator)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public IQueryable<Exam> GetQueryable()
        {
            return Context.Exams.AsNoTracking()
                .Include(x => x.Subject)
                .Include(x => x.Creator)
                .Where(x=> !x.IsDeleted);
        }   
    }
}
