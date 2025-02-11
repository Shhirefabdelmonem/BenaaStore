using BenaaStore.DataAccess.Repository.IRepository;
using BenaaStore.Models;
using BenaaStore.Models.Models;
using Microsoft.AspNetCore.Mvc.Diagnostics;
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

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderHeaderDB = context.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if (orderHeaderDB != null)
            {
                orderHeaderDB.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderHeaderDB.PaymentStatus = paymentStatus;
                }
            }
        }
        
        // store session id on database orderHeaderTable
        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            var orderHeaderDB= context.OrderHeaders.FirstOrDefault(u=>u.Id==id);
            if (!string.IsNullOrEmpty(sessionId))
            {
                // before orderConfirmation 
                orderHeaderDB.SessionId=sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                // orderConfirmation call this after payment complete successfully
                // so that SessionId will be already populated 
                orderHeaderDB.PaymentIntentId=paymentIntentId;
                orderHeaderDB.PaymentDate = DateTime.Now;
            }
        }
    }
}
