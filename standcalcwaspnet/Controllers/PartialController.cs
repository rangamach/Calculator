using Microsoft.AspNetCore.Mvc;

namespace standcalcwaspnet.Controllers
{
    public class PartialController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
