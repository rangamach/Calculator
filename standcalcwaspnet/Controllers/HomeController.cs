using Microsoft.AspNetCore.Mvc;
using standcalcwaspnet.Models;
using System.Diagnostics;

namespace standcalcwaspnet.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public ActionResult Index(string Name)
        {
            CalcModel model = new CalcModel();
            HttpContext.Session.SetString("username", "Ranga");
            //HttpContext.Session.SetString("username", Name);
            HttpContext.Session.SetString("userReqCnt", "0");
            //model.user = HttpContext.Session.GetString("username") + "-->" + HttpContext.Session.GetString("userReqCnt");
            model.user = HttpContext.Session.GetString("username");
            model.userReqCount = HttpContext.Session.GetString("userReqCnt");
            return View(model);
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginModel input)
        {
            List<string> Users = new List<string>();
            Users.Add("Ranga");
            Users.Add("Hemanth");
            if(Users.Contains(input.user))
            {
                return RedirectToAction("Index", new { name = input.user });
                //return Redirect("/Home");
            }
            else
            {
                ViewBag.Sorry = "Invalid User Name";
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
        [HttpPost]
        public ActionResult Index(CalcModel input)
        {
            //split operators and operands string...
            string infixstr = addcommas(input.infstr);
            //create an array of operators and operands...
            string[] infixarr = removecommas(infixstr);
            //converts infix array to postfix array for easy evaluation...
            List<string> postfixlst = inftopof(infixarr);
            //postfix list evaluation to get the expression result...
            List<string> final_result = postfixeval(postfixlst);
            //get final model values...
            CalcModel final_model = output(final_result,input);
            //transfer model to view...
            return View(final_model);
        }
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
            var temp = 0;
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
        private CalcModel output(List<string> final_result,CalcModel model)
        {
            ModelState.Clear();
            model.result = final_result[0];
            string no_of_req = HttpContext.Session.GetString("userReqCnt");
            HttpContext.Session.SetString("userReqCnt", (Int32.Parse(no_of_req) + 1).ToString());
            //model.user = HttpContext.Session.GetString("username") + "-->" + HttpContext.Session.GetString("userReqCnt");
            model.user = HttpContext.Session.GetString("username");
            model.userReqCount = HttpContext.Session.GetString("userReqCnt");
            return model;
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