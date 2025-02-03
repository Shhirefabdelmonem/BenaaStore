using BenaaStore.DataAccess.Repository.IRepository;
using BenaaStore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

        public T Get(Expression<Func<T, bool>> filter, string? includeProp = null)
        {
            IQueryable<T> query = context.Set<T>();
            query=query.Where(filter);
            if (!string.IsNullOrEmpty(includeProp))
            {
                foreach (var prop in includeProp.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(prop);
                }
            }
           return  query.FirstOrDefault();

        }

        public IEnumerable<T> GetAll(string? includeProp = null)
        {
            IQueryable<T> query = context.Set<T>();
            if (!string.IsNullOrEmpty(includeProp))
            {
                foreach (var prop in includeProp.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(prop);
                }
            }
            return query.ToList();
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
