using BenaaStore.DataAccess.Repository.IRepository;
using BenaaStore.Models;
using BenaaStore.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BenaaStore.DataAccess.Repository
{
    public class ShopingCartRepository : Repository<ShoppingCart>,IShopingCartRepository
    {
        private readonly ApplicationDbContext context;
        public ShopingCartRepository(ApplicationDbContext _context):base(_context)
        {
            context = _context;
        }

        public void update(ShoppingCart obj)
        {
            context.Update(obj);
        }
    }
}
