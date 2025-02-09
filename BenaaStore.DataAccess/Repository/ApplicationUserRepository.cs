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
    public class ApplicationUserRepository: Repository<ApplicationUser>,IApplicationUserRepository
    {
        private readonly ApplicationDbContext context;
        public ApplicationUserRepository(ApplicationDbContext _context):base(_context)
        {
            context = _context;
        }
    }
}
