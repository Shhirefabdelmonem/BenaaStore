﻿using BenaaStore.DataAccess.Repository.IRepository;
using BenaaStore.Models;
using BenaaStore.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenaaStore.DataAccess.Repository
{
    public class OrderDetailsRepository:Repository<OrderDetails>,IOrderDetailsRepository
    {
        ApplicationDbContext context;
        public OrderDetailsRepository(ApplicationDbContext _context):base(_context)
        {
            context = _context;
        }

        public void Update(OrderDetails obj)
        {
            context.Update(obj);
        }
    }
}
