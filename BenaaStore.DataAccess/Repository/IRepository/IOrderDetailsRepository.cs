﻿using BenaaStore.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenaaStore.DataAccess.Repository.IRepository
{
    public interface IOrderDetailsRepository:IRepository<OrderDetails>
    {
        public void Update(OrderDetails obj);
    }
}
