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
    public class ProductRepository:Repository<Product>,IProductRepository
    {
        private readonly ApplicationDbContext context;
        public ProductRepository(ApplicationDbContext _context):base(_context)
        {
            context= _context;
        }

        public void Update(Product product)
        {
            //context.Update(product);
            var dbProduct=context.Products.FirstOrDefault(p=>p.Id==product.Id);
            if (dbProduct!=null)
            {
                dbProduct.Title = product.Title;
                dbProduct.ISBN = product.ISBN;
                dbProduct.Price = product.Price;
                dbProduct.Price50 = product.Price50;
                dbProduct.ListPrice = product.ListPrice;
                dbProduct.Price100 = product.Price100;
                dbProduct.Description = product.Description;
                dbProduct.CategoryId = product.CategoryId;
                dbProduct.Author = product.Author;
                if (product.ImageUrl != null)
                {
                    dbProduct.ImageUrl = product.ImageUrl;
                }

            }
        }
    }
}
