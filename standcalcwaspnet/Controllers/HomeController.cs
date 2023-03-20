using Microsoft.AspNetCore.Mvc;
using standcalcwaspnet.Models;
using System;
using System.Diagnostics;
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
        [HttpGet]
        public ActionResult Index(string Name)
        {
            CalcModel model = new CalcModel();
            HttpContext.Session.SetString("username", Name);
            HttpContext.Session.SetString("userReqCnt", "0");
            model.user = HttpContext.Session.GetString("username");
            model.userReqCount = HttpContext.Session.GetString("userReqCnt");
            return View(model);
        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginModel input)
        {
            List<LoginModel> Login_Info = new List<LoginModel>();
            Login_Info.Add(new LoginModel
            {
                user = "Ranga",
                pass = "Mach"
            });
            Login_Info.Add(new LoginModel
            {
                user = "Hemanth",
                pass = "Venk"
            });
            if (Validate_Login(input))
            {
                return RedirectToAction("Index", new { name = input.user });
            }
            else
            {
                ViewBag.Sorry = "Invalid User Name";
                return View();
            }
            bool Validate_Login(LoginModel input)
            {
                foreach (var input_info in Login_Info)
                {
                    if (input.user == input_info.user && input.pass == input_info.pass)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        //constant string of operators...
        string operatorstr = " -*/^()";
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
            else if (pre == " " || pre == "-")
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
                        if (postfixlst[i] == " ")
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
        private CalcModel output(string infix,List<string> final_result)
        {
            string no_of_req = HttpContext.Session.GetString("userReqCnt");
            HttpContext.Session.SetString("userReqCnt", (Int32.Parse(no_of_req) + 1).ToString());
            CalcModel final_model = new CalcModel()
            {
                result = final_result[0],
                user = HttpContext.Session.GetString("username"),
                userReqCount = HttpContext.Session.GetString("userReqCnt")
            };
            return final_model;
        }
        public JsonResult Evaluation(string infix)
        {
            //split operators and operands string...
            string infixstr = addcommas(infix);
            //create an array of operators and operands...
            string[] infixarr = removecommas(infixstr);
            //converts infix array to postfix array for easy evaluation...
            List<string> postfixlst = inftopof(infixarr);
            //postfix list evaluation to get the expression result...
            List<string> final_result = postfixeval(postfixlst);
            //get final model values...
            CalcModel final_model = output(infix,final_result);
            //transfer model to view...
            return Json(final_model);

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