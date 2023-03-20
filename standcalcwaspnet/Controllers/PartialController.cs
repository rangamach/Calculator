using standcalcwaspnet.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace standcalcwaspnet.Controllers
{
    public class PartialController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public JsonResult GreetName(string Lname,string age)
        {
            PartialModel p_model = new PartialModel()
            {
                Name = "Ranga " + Lname +". Your Age is: " + age
            };
                return Json(p_model);
        }
    }
}
