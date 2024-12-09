using Newtonsoft.Json.Linq;
using SynapseInterview.Code.Models;
using SynapseInterview.Code.Repos;
using SynapseInterview.Code.Services;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.OrdersExample
{
    public class Program
    {
        private readonly LoggingRepo _loggingRepo;
        private readonly OrderService _orderService;

        public Program(LoggingRepo loggingRepo, HttpClient httpClient, string alertApiUrl, string updateApiUrl)
        {
            _loggingRepo = loggingRepo ?? throw new ArgumentNullException(nameof(loggingRepo));
            _orderService = new OrderService(loggingRepo, httpClient, alertApiUrl, updateApiUrl);
        }

        public static async Task<int> Main(string[] args)
        {
            Console.WriteLine("Start of App");

            var connectionString = "somesupersafesecretpassword"; // use a secure configuration method
            string alertApiUrl = "https://alert-api.com/alerts"; //use a secure configuration method
            string updateApiUrl = "https://update-api.com/update"; //use a secure congfiguration method

            var loggingRepo = new LoggingRepo(connectionString);
            var httpClient = new HttpClient();

            var program = new Program(loggingRepo, httpClient, alertApiUrl, updateApiUrl);
            await program.Run();

            Console.WriteLine("Results sent to relevant APIs.");
            return 0;
        }

        public async Task Run()
        {
            await _loggingRepo.InsertLog(new Log { IsErrorLog = false, Message = "Started Program" });

            var medicalEquipmentOrders = await _orderService.FetchMedicalEquipmentOrders();
            if (medicalEquipmentOrders != null)
            {
                foreach (var order in medicalEquipmentOrders)
                {
                    var updatedOrder = await _orderService.ProcessOrder(order);
                    await _orderService.SendAlertAndUpdateOrder(updatedOrder);
                }
            }
            else
            {
                await _loggingRepo.InsertLog(new Log { IsErrorLog = false, Message = "No orders fetched" });
            }
        }


    }

}
