using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AttAssApplication.Model;
using NuGet.Protocol.Plugins;
using Microsoft.AspNetCore.Authorization;
using System.Xml;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.AspNetCore.Identity;

namespace AttAssApplication.Controllers
{
    public class LoginController : Controller
    {
        private readonly AttAssAppDbContext _context;


        public LoginController(AttAssAppDbContext context)
        {
            _context = context;
        }
        // GET: Login
        public async Task<IActionResult> Index()
        {
            return  _context.User != null ?
                        View() :
                        Problem("Entity set 'AttAssAppDbContext.User'  is null.");
        }

        // GET: Login/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.User == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.UserName == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        

        // GET: Login/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("user") == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Tokens");
            }
        }

        public IActionResult Logout()
        {
            if (HttpContext.Session != null)
            {
                HttpContext.Session.Remove("user");
            }
            return RedirectToAction("Create", "Login");
        }

        // POST: Login/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserName,Password")] User user)
        {
            System.Diagnostics.Debug.WriteLine(user.UserName);
            System.Diagnostics.Debug.WriteLine(user.Password);

            if (ModelState.IsValid)
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load("Data.xml");

                XmlNodeList users = xmlDoc.SelectNodes("//user");
                foreach (XmlNode user1 in users)
                {
                    XmlNode usernameNode = user1.SelectSingleNode("username");
                    XmlNode passwordNode = user1.SelectSingleNode("password");
                    XmlNode roleNode = user1.SelectSingleNode("role");
                    if (usernameNode.InnerText == user.UserName && passwordNode.InnerText == user.Password)
                    {
                        if(roleNode.InnerText == "admin")
                        {
                            System.Diagnostics.Debug.WriteLine("ACCESS GRANTED!");
                            HttpContext.Session.SetString("user", user.UserName);
                            return RedirectToAction("Index", "Tokens");
                        }
                        else
                        {
                            ModelState.AddModelError("", "You don't have permission!");
                            return View(user);
                        }
                    }
                    else
                    {
                        ModelState.Clear();
                        ModelState.AddModelError("", "Incorrect username or password!");
                    }
                }
                return View(user);
            }
            else
            {

                return View(user);
            }

        }
    }
}
