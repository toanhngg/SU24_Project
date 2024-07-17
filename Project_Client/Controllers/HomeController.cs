using BusinessObject.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project_API.DTO;
using Project_Client.Models;
using Project_Client.Util;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace Project_Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient client = null;
        private string ProductApiUrl = "";
        private string CategoryApiUrl = "";

        public IActionResult Cart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            return View(cart);
        }
        public HomeController()
        {
            client = new HttpClient();
            var content = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(content);
            ProductApiUrl = "https://localhost:7135/api/Products";
            CategoryApiUrl = "https://localhost:7135/api/Category";


        }

        public async Task<IActionResult> Index()
        {
            using (HttpResponseMessage res = await client.GetAsync(ProductApiUrl))
            {
                using (HttpContent content = res.Content)
                {
                    string data = await content.ReadAsStringAsync();
                    List<Product> product = JsonConvert.DeserializeObject<List<Product>>(data);
                    ViewBag.Product = product;

                }

            }
            using (HttpResponseMessage res = await client.GetAsync(CategoryApiUrl))
            {
                using (HttpContent content = res.Content)
                {
                    string data = await content.ReadAsStringAsync();
                    List<Category> category = JsonConvert.DeserializeObject<List<Category>>(data);
                    ViewBag.Category = category;

                }

            }
            return View();
        }
        public async Task<IActionResult> ProductDetail(int id)
        {
			//https://localhost:7135/api/Products/getProduct?id=3
			using (HttpResponseMessage res = await client.GetAsync(ProductApiUrl + "/getProduct?id=" + id))
            {
                using (HttpContent content = res.Content)
                {
                    string data = await content.ReadAsStringAsync();
                    ProductDTO productdetail = JsonConvert.DeserializeObject<ProductDTO>(data);
                    ViewBag.Productdetail = productdetail;

                }

			}
			return View();

		}
	}
}