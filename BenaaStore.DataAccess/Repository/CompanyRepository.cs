using BenaaStore.DataAccess.Repository.IRepository;
using BenaaStore.Models;
using BenaaStore.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenaaStore.DataAccess.Repository
{
    public class CompanyRepository:Repository<Company>,ICompanyRepository
    {
       private readonly ApplicationDbContext context;
        public CompanyRepository(ApplicationDbContext _context):base(_context)
        {
            context = _context;
        }

        public void update(Company company)
        {
            context.Update(company);
        }
    }
}
