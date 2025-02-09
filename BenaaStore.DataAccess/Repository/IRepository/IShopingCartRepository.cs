using BenaaStore.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenaaStore.DataAccess.Repository.IRepository
{
    public interface IShopingCartRepository:IRepository<ShoppingCart>
    {
        void update(ShoppingCart obj);
    }
}
