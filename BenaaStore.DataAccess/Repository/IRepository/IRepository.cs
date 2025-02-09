using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BenaaStore.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        T Get(Expression<Func<T, bool>> filter, string? includeProp = null, bool tracked = false);
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProp = null);
        void Add(T entity);
        void Remove (T entity);
        void RemoveRange(IEnumerable<T> entity);
        
    }
}