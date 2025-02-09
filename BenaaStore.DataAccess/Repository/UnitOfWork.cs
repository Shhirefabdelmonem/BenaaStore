using BenaaStore.DataAccess.Repository.IRepository;
using BenaaStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenaaStore.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        public ICategoryRepository Category { get; private set; }
        public IProductRepository Product { get; private set; }
        public ICompanyRepository Company { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IShopingCartRepository ShoppingCart { get; private set; }
        public IOrderDetailsRepository OrderDetails { get; private set; }
        public IOrderHeaderRepository OrderHeader { get; private set; }

        private readonly ApplicationDbContext context;
        public UnitOfWork(ApplicationDbContext _context)
        {
            context = _context;
            Category= new CategoryRepository(context);
            Product= new ProductRepository(context);
            Company= new CompanyRepository(context);
            ShoppingCart=new ShopingCartRepository(context);
            ApplicationUser= new ApplicationUserRepository(context);
            OrderHeader= new OrderHeaderRepository(context);
            OrderDetails= new OrderDetailsRepository(context);
            
        }
       

        public void Save()
        {
            context.SaveChanges();
        }
    }
}
