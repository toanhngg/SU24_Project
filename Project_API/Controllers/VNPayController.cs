
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project_API.DTO;
using Project_API.Lib;
using Project_API.Service;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Channels;

namespace Project_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VNPayController : Controller
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        private readonly IConfiguration _configuration;
        public VNPayController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        //[HttpGet]
        //public IActionResult Get()
        //{
        //    return Ok();
        //}
        private string ip()
        {
            string ipAddress;
            try
            {
                ipAddress = HttpContext.Request.HttpContext.Request.Headers["HTTP_X_FORWARDED_FOR"].ToString();

                if (string.IsNullOrEmpty(ipAddress) || ipAddress.ToLower() == "unknown")
                    ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();
            }
            catch (Exception ex)
            {
                ipAddress = "Invalid IP:" + ex.Message;
            }
            return ipAddress;
        }
        [HttpPost]
        public IActionResult Payment([FromBody] PaymentDTO paymentRequest)
        {
            var request = HttpContext.Request;
            var hostAddress = request.Host.Value;
            string url = _configuration["VNPAY:Url"];
            string returnUrl = request.Scheme + "://" + hostAddress + "/" + _configuration["VNPAY:ReturnUrl"];
            string tmnCode = _configuration["VNPAY:TmnCode"];
            string hashSecret = _configuration["VNPAY:HashSecret"];
            var IpAdress = ip();
            PayLib pay = new PayLib();


            pay.AddRequestData("vnp_Version", "2.1.0"); //Phiên bản api mà merchant kết nối. Phiên bản hiện tại là 2.0.0
            pay.AddRequestData("vnp_Command", "pay"); //Mã API sử dụng, mã cho giao dịch thanh toán là 'pay'
            pay.AddRequestData("vnp_TmnCode", tmnCode); //Mã website của merchant trên hệ thống của VNPAY (khi đăng ký tài khoản sẽ có trong mail VNPAY gửi về)
            pay.AddRequestData("vnp_Amount", (paymentRequest.Amount * 100).ToString()); //số tiền cần thanh toán, công thức: số tiền * 100 - ví dụ 10.000 (mười nghìn đồng) --> 1000000
            pay.AddRequestData("vnp_BankCode", "BIDV"); //Mã Ngân hàng thanh toán (tham khảo: https://sandbox.vnpayment.vn/apis/danh-sach-ngan-hang/), có thể để trống, người dùng có thể chọn trên cổng thanh toán VNPAY
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")); //ngày thanh toán theo định dạng yyyyMMddHHmmss
            pay.AddRequestData("vnp_CurrCode", "VND"); //Đơn vị tiền tệ sử dụng thanh toán. Hiện tại chỉ hỗ trợ VND
            pay.AddRequestData("vnp_IpAddr", IpAdress /*Util.GetIpAddress()*/); //Địa chỉ IP của khách hàng thực hiện giao dịch
            pay.AddRequestData("vnp_Locale", "vn"); //Ngôn ngữ giao diện hiển thị - Tiếng Việt (vn), Tiếng Anh (en)
            pay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang"); //Thông tin mô tả nội dung thanh toán
            pay.AddRequestData("vnp_OrderType", "other"); //topup: Nạp tiền điện thoại - billpayment: Thanh toán hóa đơn - fashion: Thời trang - other: Thanh toán trực tuyến
            pay.AddRequestData("vnp_ReturnUrl", returnUrl); //URL thông báo kết quả giao dịch khi Khách hàng kết thúc thanh toán
            pay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString()); //mã hóa đơn

            string paymentUrl = pay.CreateRequestUrlWithHmacSHA512(url, hashSecret);

            return Ok(paymentUrl);
        }
        //[HttpGet("PaymentConfirm")]
        //public IActionResult PaymentConfirm()
        //{
        //    if (Request.Query.Count > 0)
        //    {
        //        string hashSecret = _configuration["VNPAY:HashSecret"]; //Chuỗi bí mật
        //        var vnpayData = Request.Query.AsEnumerable();
        //        PayLib pay = new PayLib();

        //        //lấy toàn bộ dữ liệu được trả về
        //        foreach (var s in vnpayData)
        //        {
        //            pay.AddResponseData(s.Key, s.Value);
        //        }

        //        long orderId = Convert.ToInt64(pay.GetResponseData("vnp_TxnRef")); //mã hóa đơn
        //        long vnpayTranId = Convert.ToInt64(pay.GetResponseData("vnp_TransactionNo")); //mã giao dịch tại hệ thống VNPAY
        //        string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode"); //response code: 00 - thành công, khác 00 - xem thêm https://sandbox.vnpayment.vn/apis/docs/bang-ma-loi/
        //        string vnp_SecureHash = Request.Query["vnp_SecureHash"]; //hash của dữ liệu trả về

        //        bool checkSignature = pay.ValidateSignatureHmacSHA512(vnp_SecureHash, hashSecret); //check chữ ký đúng hay không?

        //        if (checkSignature)
        //        {
        //            if (vnp_ResponseCode == "00")
        //            {
        //                //Thanh toán thành công
        //                //ViewBag.Message = "Thanh toán thành công hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId;
        //            }
        //            else
        //            {
        //                //Thanh toán không thành công. Mã lỗi: vnp_ResponseCode
        //                //ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId + " | Mã lỗi: " + vnp_ResponseCode;
        //            }
        //        }
        //        else
        //        {
        //            //ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý";
        //        }
        //    }

        //    //return RedirectToAction("CheckoutSuccess", "Paypal");
        //    return Ok();
        //}
        [HttpGet("PaymentConfirm")]
        public IActionResult PaymentConfirm()
        {
            var rabbitMQService = new RabbitMQService();
            
                if (Request.Query.Count > 0)
                {
                    string hashSecret = _configuration["VNPAY:HashSecret"];
                    var vnpayData = Request.Query.AsEnumerable();
                    PayLib pay = new PayLib();

                    foreach (var s in vnpayData)
                    {
                        pay.AddResponseData(s.Key, s.Value);
                    }

                    long orderId = Convert.ToInt64(pay.GetResponseData("vnp_TxnRef"));
                    long vnpayTranId = Convert.ToInt64(pay.GetResponseData("vnp_TransactionNo"));
                    string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode");
                    string vnp_SecureHash = Request.Query["vnp_SecureHash"];

                    bool checkSignature = pay.ValidateSignatureHmacSHA512(vnp_SecureHash, hashSecret);

                    if (checkSignature)
                    {
                        if (vnp_ResponseCode == "00")
                        {
                            rabbitMQService.SendMessage($"Thanh toán thành công hóa đơn {orderId} | Mã giao dịch: {vnpayTranId}");
                            return Content("<h2>Thanh toán thành công</h2>", "text/html");
                        }
                        else
                        {
                            rabbitMQService.SendMessage($"Có lỗi xảy ra trong quá trình xử lý hóa đơn {orderId} | Mã giao dịch: {vnpayTranId} | Mã lỗi: {vnp_ResponseCode}");
                            return Content("<h2>Thanh toán không thành công</h2>", "text/html");
                        }
                    }
                    else
                    {
                        rabbitMQService.SendMessage("Có lỗi xảy ra trong quá trình xử lý: Chữ ký không hợp lệ");
                        return Content("<h2>Có lỗi xảy ra trong quá trình xử lý</h2>", "text/html");
                    }
                }

                rabbitMQService.SendMessage("Có lỗi xảy ra trong quá trình xử lý: Không có dữ liệu trả về");
                return Content("<h2>Có lỗi xảy ra trong quá trình xử lý: Không có dữ liệu trả về</h2>", "text/html");
            }

    }
}