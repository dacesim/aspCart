﻿using aspCart.Core.Domain.Sale;
using aspCart.Core.Interface.Sale;
using aspCart.Infrastructure.EFRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aspCart.Infrastructure.Services.Sale
{
    public class OrderService : IOrderService
    {
        #region Fields

        private readonly ApplicationDbContext _context;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;

        #endregion

        #region Constrcutor

        public OrderService(
            ApplicationDbContext context,
            IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository)
        {
            _context = context;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all orders
        /// </summary>
        /// <returns>List of order entities</returns>
        public IList<Order> GetAllOrders()
        {
            // TODO: update when lazy loading is available
            var entities = _context.Orders
                .Include(x => x.Items)
                .AsNoTracking()
                .ToList();

            return entities;
        }

        /// <summary>
        /// Get order by id
        /// </summary>
        /// <param name="id">Order id</param>
        /// <returns>Order entity</returns>
        public Order GetOrderById(Guid id)
        {
            // TODO: update when lazy loading is available
            return _context.Orders
                .Include(x => x.Items)
                .AsNoTracking()
                .SingleOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Get order by order id
        /// </summary>
        /// <param name="id">Order number id</param>
        /// <returns>Order entity</returns>
        public Order GetOrderByOrderId(string id)
        {
            // TODO: update when lazy loading is available
            return _context.Orders
                .Include(x => x.Items)
                .AsNoTracking()
                .SingleOrDefault(x => x.OrderNumber == id);
        }

        /// <summary>
        /// Get all orders by user id
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>List of order entities</returns>
        public IList<Order> GetAllOrdersByUserId(Guid userId)
        {
            // TODO: update when lazy loading is available
            var entities = _context.Orders
                .Include(x => x.Items)
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .ToList();

            return entities;
        }

        /// <summary>
        /// Insert order
        /// </summary>
        /// <param name="order">Order entity</param>
        public void InsertOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            _orderRepository.Insert(order);
            _orderRepository.SaveChanges();
        }

        /// <summary>
        /// Update order
        /// </summary>
        /// <param name="order">Order entity</param>
        public void UpdateOrder(Order order)
        {
            if (order == null)
                throw new ArgumentException("order");

            _orderRepository.Update(order);
            _orderRepository.SaveChanges();
        }

        /// <summary>
        /// Delete all orders
        /// </summary>
        /// <param name="ids">List of order ids</param>
        public void DeleteOrders(IList<Guid> ids)
        {
            if (ids == null)
                throw new ArgumentNullException("ids");

            foreach (var id in ids)
                _orderRepository.Delete(GetOrderById(id));

            _orderRepository.SaveChanges();
        }

        #endregion
    }
}
