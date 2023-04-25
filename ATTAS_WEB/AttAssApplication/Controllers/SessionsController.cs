using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AttAssApplication.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace AttAssApplication.Controllers
{
    public class SessionsController : Controller
    {
        private readonly AttAssAppDbContext _context;

        public SessionsController(AttAssAppDbContext context)
        {
            _context = context;
        }

        // GET: Sessions
        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("user");
            if (username.IsNullOrEmpty())
            {
                return RedirectToAction("Create", "Login");
            }
            var attAssAppDbContext = _context.Sessions.Include(s => s.Status);
            return View(await attAssAppDbContext.ToListAsync());
        }

        // GET: Sessions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var username = HttpContext.Session.GetString("user");
            if (username.IsNullOrEmpty())
            {
                return RedirectToAction("Create", "Login");
            }
            if (id == null || _context.Sessions == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions
                .Include(s => s.Status)
                .FirstOrDefaultAsync(m => m.SessionId == id);
            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }
    }
}