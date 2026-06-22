using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestManagement.DAL.Models;

namespace TestManagement.DAL.Repositories.Interfaces
{
    public interface IExamRepository : IRepository<Exam>
    {
        Task<Exam?> GetDetailAsync(int id);
        IQueryable<Exam> GetQueryable();
    }
}
