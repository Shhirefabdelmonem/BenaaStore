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
    public class OrderHeaderRepository:Repository<OrderHeader>,IOrderHeaderRepository
    {
        ApplicationDbContext context;
        public OrderHeaderRepository(ApplicationDbContext _context):base(_context)
        {
            context = _context;
        }

        public void Update(OrderHeader obj)
        {
            context.Update(obj);
        }
    }
}
