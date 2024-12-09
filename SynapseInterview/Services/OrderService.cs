using Newtonsoft.Json.Linq;
using SynapseInterview.Code.Models;
using SynapseInterview.Code.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SynapseInterview.Code.Services
{
    public class OrderService
    {
        private readonly LoggingRepo _loggingRepo;
        private readonly HttpClient _httpClient;
        private readonly string _updateApiUrl;
        private readonly AlertService _alertService;

        public OrderService(LoggingRepo loggingRepo, HttpClient httpClient, string alertApiUrl, string updateApiUrl)
        {
            _loggingRepo = loggingRepo ?? throw new ArgumentNullException(nameof(loggingRepo));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _updateApiUrl = updateApiUrl;
            _alertService = new AlertService(alertApiUrl, httpClient, loggingRepo);
        }
        public async Task<Order[]?> FetchMedicalEquipmentOrders()
        {
            await _loggingRepo.InsertLog(new Log { IsErrorLog = false, Message = "Fetching medical equipment" });

            string ordersApiUrl = "https://orders-api.com/orders";
            var response = await _httpClient.GetAsync(ordersApiUrl);

            if (response.IsSuccessStatusCode)
            {
                var ordersData = await response.Content.ReadAsStringAsync();
                await _loggingRepo.InsertLog(new Log { IsErrorLog = false, Message = "Successfully fetched equipment" });
                return JArray.Parse(ordersData).ToObject<Order[]>();
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            await _loggingRepo.InsertLog(new Log { IsErrorLog = true, Message = errorMessage });
            throw new Exception(errorMessage);
        }

        internal bool IsItemDelivered(Item item) => item.Status == Status.Delivered;


        public async Task<Order> ProcessOrder(Order order)
        {
            await _loggingRepo.InsertLog(new Log { IsErrorLog = false, Message = $"Processing order: {order.OrderId}" });

            foreach (var item in order.Items)
            {
                if (IsItemDelivered(item))
                {
                    await _alertService.SendAlertMessage(item, order.OrderId.ToString());
                    await _alertService.IncrementDeliveryNotification(item);
                }
            }

            return order;
        }
        internal async Task SendAlertAndUpdateOrder(Order order)
        {
            await _loggingRepo.InsertLog(new Log { IsErrorLog = false, Message = $"Sending alert and updating order: {order.OrderId}" });

            var content = new StringContent(order.ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_updateApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                await _loggingRepo.InsertLog(new Log { IsErrorLog = false, Message = $"Successfully updated order: {order.OrderId}" });
            }
            else
            {
                await _loggingRepo.InsertLog(new Log { IsErrorLog = true, Message = $"Failed to update order: {order.OrderId}" });
            }
        }
    }

}
