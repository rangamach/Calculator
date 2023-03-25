using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using standcalcwaspnet.Data;
using standcalcwaspnet.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.Security.Cryptography.X509Certificates;

namespace standcalcwaspnet.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        CalcModel model = new CalcModel();
        [HttpGet]
        public ActionResult Index()
        {
            model = populatemodel();
            return View(model);
        }
        [HttpPost]
        public ActionResult Index(string unregister)
        {
            if(unregister == "Unregister")
            {
                ServerData delete = new ServerData();
                int uid = Convert.ToInt32(HttpContext.Session.GetInt32("userid"));
                bool result = delete.DeleteUser(uid);
                if (result)
                {
                    HttpContext.Session.SetString("unreg", "Succesively Unregistered.");
                    return RedirectToAction("Login");
                }
                else
                {
                    return View();
                }
            }
            else
            {
                model = populatemodel();
                return View(model);
            }
        }
        [HttpGet]
        public ActionResult Login()
        {
            if (HttpContext.Session.GetString("unreg") == "Succesively Unregistered.")
            {
                ViewBag.Msg = "Succesively Unregistered.";
            }
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginModel input,string submitBtn)
        {
            switch(submitBtn)
            {
                case "Login":
                    ServerData user_data = new ServerData();
                    List<LoginModel> User_List = user_data.GetAllUsers();
                    if (Validate_Login(input))
                    {
                        InitializeSession();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewBag.Msg = "Invalid User Name";
                        return View();
                    }
                    bool Validate_Login(LoginModel input_info)
                    {
                        foreach (var user_info in User_List)
                        {
                            if (input_info.UserName == user_info.UserName && input_info.Password == user_info.Password)
                            {
                                input.UserID = user_info.UserID;
                                return true;
                            }
                        }
                        return false;
                    }
                    void InitializeSession()
                    {
                        HttpContext.Session.SetString("username", input.UserName);
                        HttpContext.Session.SetInt32("userid", input.UserID);
                        HttpContext.Session.SetInt32("Usercount", -1);
                        HttpContext.Session.SetString("sid", HttpContext.Session.Id.ToString());
                        HttpContext.Session.SetString("infixexp", "");
                        HttpContext.Session.SetString("res", "");
                    }
                case "Register":
                    return RedirectToAction("Registration");
                default:
                    return View();
            }
        }
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Registration(LoginModel model,string submitbtn)
        {
            ServerData insert = new ServerData();
            if(submitbtn == "Register")
            {
                HttpContext.Session.SetString("unreg", "");
                model.UserCreationDate = DateTime.Now;
                model.AuthType = "Custom";
                bool result = insert.AddUser(model);
                if (result)
                {
                    ViewBag.Success = "Successively Registered.";
                }
                else
                {
                    ViewBag.Success = "Unsuccessively Registered.";
                }
                return View();
            }
            else if(submitbtn == "Go Back")
            {
                return RedirectToAction("Login");
            }
            else
            {
                return View();
            }
        }
        //constant string of operators...
        string operatorstr = "+-*/^()";
        //List to keep track of operators...
        List<string> stacklst = new List<string>();
        int top_op = -1;
        //fh --> first half and sh --> second half...
        string sh;
        string fh;
        private string addcommas(string infixstr)
        {
            if (infixstr is not null)
            {
                for (var i = 0; i < infixstr.Length; i++)
                {
                    var l = infixstr.Length;
                    if (operatorstr.Contains(infixstr[i]))
                    {
                        if (i == 0)
                        {
                            sh = infixstr.Substring(1);
                            fh = infixstr.Substring(0, 1);
                            fh += ",";
                            infixstr = fh + sh;
                            i += 1;
                        }
                        else if (i == l - 1)
                        {
                            sh = infixstr.Substring(l - 1);
                            fh = infixstr.Substring(0, l - 1);
                            fh += ",";
                            infixstr = fh + sh;
                            i += 1;
                        }
                        else
                        {
                            if (!(infixstr[i - 1].Equals(',')))
                            {
                                fh = infixstr.Substring(0, i);
                                sh = infixstr.Substring(i);
                                fh += ",";
                                infixstr = fh + sh;
                                i += 1;
                            }
                            fh = infixstr.Substring(0, i + 1);
                            sh = infixstr.Substring(i + 1);
                            fh += ",";
                            infixstr = fh + sh;
                            i += 1;
                        }
                    }
                }
            }
            return infixstr;
        }
        private string[] removecommas(string infixstr)
        {
            string[] infixarr = infixstr.Split(',');
            return infixarr;
        }
        private dynamic pop()
        {
            if (top_op == -1)
                return 0;
            else
            {
                var popped_ele = stacklst[top_op];
                stacklst.RemoveAt(top_op);
                top_op--;
                return popped_ele;
            }
        }
        private void push(string e)
        {
            top_op++;
            if(top_op == stacklst.Count)
            {
                stacklst.Add(e);
            }
            else
            {
                stacklst[top_op] = e;
            }
        }
        private int precedency(string pre)
        {
            if (pre == "(" || pre == ")")
            {
                return 1;
            }
            else if (pre == "+" || pre == "-")
            {
                return 2;
            }
            else if (pre == "/" || pre == "*")
            {
                return 3;
            }
            else if (pre == "^")
            {
                return 4;
            }
            else
            {
                return 0;
            }
        }
        private List<string> inftopof(string[] infixarr)
        {
            List<string> postfix = new List<string>();
            //var temp = 0;
            for (var i = 0; i < infixarr.Length; i++)
            {
                var el = infixarr[i];
                if (operatorstr.Contains(el))
                {
                    if (el.Contains(')'))
                    {
                        while (stacklst[top_op]!="(")
                        {
                            postfix.Add(pop());
                        }
                        pop();
                    }
                    else if (el.Contains('('))
                    {
                        push(el);
                    }
                    else if ( top_op > -1 && precedency(el) > precedency(stacklst[top_op]))
                    {
                        push(el);
                    }
                    else
                    {
                        while (top_op > -1 && precedency(el) <= precedency(stacklst[top_op]))
                        {
                            postfix.Add(pop());
                        }
                        push(el);
                    }
                }
                else
                {
                    postfix.Add(el);
                }
            }
            while (top_op != -1)
            {
                postfix.Add(pop());
            }
            return postfix;
        }
        private string power(string b, string p)
        {
            if (p == "0")
            {
                return "1";
            }
            else if(p == "1")
            {
                return b;
            }
            else
            {
                float x = float.Parse(b);
                float y = float.Parse(p);
                float exp = 1.0f;
                for (var i = 0; i < y; i++)
                {
                    exp *= x;    
                }
                return exp.ToString();
            }
        }
        private List<string> postfixeval(List<string> postfixlst)
        {
            while (postfixlst.Count > 1)
            {
                for (var i = 2; i < postfixlst.Count; i++)
                {
                    if (
                      operatorstr.Contains(postfixlst[i - 1]) ||
                      operatorstr.Contains(postfixlst[i - 2]))
                    {
                        i++;
                    }
                    else if (postfixlst.Count > 2)
                    {
                        if (postfixlst[i] == "+")
                        {
                            postfixlst[i] = (float.Parse((postfixlst[i - 2])) + float.Parse((postfixlst[i - 1]))).ToString();
                            postfixlst.RemoveRange(i - 2, 2);
                        }
                        else if (postfixlst[i] == "-")
                        {
                            postfixlst[i] = (float.Parse((postfixlst[i - 2])) - float.Parse((postfixlst[i - 1]))).ToString();
                            postfixlst.RemoveRange(i - 2, 2);
                        }
                        else if (postfixlst[i] == "*")
                        {
                            postfixlst[i] = (float.Parse((postfixlst[i - 2])) * float.Parse((postfixlst[i - 1]))).ToString();
                            postfixlst.RemoveRange(i - 2, 2);
                        }
                        else if (postfixlst[i] == "/")
                        {
                            postfixlst[i] = (float.Parse((postfixlst[i - 2])) / float.Parse((postfixlst[i - 1]))).ToString();
                            postfixlst.RemoveRange(i - 2, 2);
                        }
                        else if (postfixlst[i] == "^")
                        {
                            postfixlst[i] = power(postfixlst[i - 2], postfixlst[i - 1]);                  
                            postfixlst.RemoveRange(i - 2, 2);
                        }
                    }
                }
            }
            return postfixlst;
        }
        private CalcModel output()
        { 
            CalcModel model = populatemodel();
            return model;
        }
        private CalcModel populatemodel()
        {
            CalcModel model = new CalcModel();
            model.Username = HttpContext.Session.GetString("username");
            model.UserID = Convert.ToInt32(HttpContext.Session.GetInt32("userid"));
            model.SessionID = HttpContext.Session.GetString("sid");
            model.Expression = HttpContext.Session.GetString("infixexp");
            model.Date = DateTime.Now;
            model.Result = HttpContext.Session.GetString("res");
            int ucnt = Convert.ToInt32(HttpContext.Session.GetInt32("Usercount"));  
            HttpContext.Session.SetInt32("Usercount",ucnt + 1);
            model.UserCount = Convert.ToInt32(HttpContext.Session.GetInt32("Usercount"));
            HttpContext.Session.SetString("unreg", "");
            return model;
        }
        public JsonResult Evaluation(string infix)
        {
            HttpContext.Session.SetString("infixexp", infix);
            //split operators and operands string...
            string infixstr = addcommas(infix);
            //create an array of operators and operands...
            string[] infixarr = removecommas(infixstr);
            //converts infix array to postfix array for easy evaluation...
            List<string> postfixlst = inftopof(infixarr);
            //postfix list evaluation to get the expression result...
            List<string> final_result = postfixeval(postfixlst);
            HttpContext.Session.SetString("res", final_result[0]);
            //get final model values...
            model = populatemodel();
            //transfer model to view...
            return Json(model);

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}