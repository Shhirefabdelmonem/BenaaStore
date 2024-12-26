using BenaaStore.DataAccess.Repository.IRepository;
using BenaaStore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BenaaStore.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext context;
        public Repository(ApplicationDbContext _context)
        {
            context = _context;
        }
        public void Add(T entity)
        {
            context.Set<T>().Add(entity);
        }

        public T Get(Expression<Func<T, bool>> filter)
        {
            IQueryable<T> query = context.Set<T>();
            query=query.Where(filter);
           return  query.FirstOrDefault();

        }

        public IEnumerable<T> GetAll()
        {
            return context.Set<T>().ToList();
        }

        public void Remove(T entity)
        {
            context.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
             context.RemoveRange(entity);
        }
       
        
    }
}
