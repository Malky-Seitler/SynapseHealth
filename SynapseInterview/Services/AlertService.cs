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
    public class AlertService
    {
        private readonly String _alertApiUrl;
        private readonly HttpClient _httpClient;
        private readonly LoggingRepo _loggingRepo;


        public AlertService(string alertApiUrl, HttpClient httpClient, LoggingRepo loggingRepo)
        {
            _alertApiUrl = alertApiUrl;
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _loggingRepo = loggingRepo;

        }
        public async Task SendAlertMessage(Item item, string orderId)
        {
            var alertData = new
            {
                Message = $"Alert for delivered item: Order {orderId}, Item: {item.Description}, Delivery Notifications: {item.DeliveryNotification}"
            };

            var content = new StringContent(JObject.FromObject(alertData).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_alertApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                await _loggingRepo.InsertLog(new Log { IsErrorLog = false, Message = $"Alert sent for delivered item: {item.Description}" });
            }
            else
            {
                await _loggingRepo.InsertLog(new Log { IsErrorLog = true, Message = $"Failed to send alert for delivered item: {item.Description}" });
            }
        }

        internal async Task IncrementDeliveryNotification(Item item)
        {
            await _loggingRepo.InsertLog(new Log { IsErrorLog = false, Message = $"Incrementing delivery notification for item: {item.Description}" });
            item.DeliveryNotification++;
        }

    }
}
