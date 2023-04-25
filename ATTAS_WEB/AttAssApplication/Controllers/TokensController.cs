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
    public class TokensController : Controller
    {
        private readonly AttAssAppDbContext _context;

        public TokensController(AttAssAppDbContext context)
        {
            _context = context;
        }
        // GET: Tokens
        [HttpGet]

        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("user");
            if (username.IsNullOrEmpty())
            {
                return RedirectToAction("Create", "Login");
            }
            return _context.Tokens != null ?
                        View(await _context.Tokens.ToListAsync()) :
                        Problem("Entity set 'AttAssAppDbContext.Tokens'  is null.");
        }

        //// GET: Tokens/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    var username = HttpContext.Session.GetString("user");
        //    if (username.IsNullOrEmpty())
        //    {
        //        return RedirectToAction("Create", "Login");
        //    }

        //    if (id == null || _context.Tokens == null)
        //    {
        //        return NotFound();
        //    }

        //    var token = await _context.Tokens
        //        .FirstOrDefaultAsync(m => m.TokenId == id);
        //    if (token == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(token);
        //}

        // GET: Tokens/Create

        [HttpGet]
        public IActionResult Create()
        {
            var username = HttpContext.Session.GetString("user");
            if (username.IsNullOrEmpty())
            {
                return RedirectToAction("Create", "Login");
            }
            return View();
        }

        // POST: Tokens/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TokenId,TokenHash,User")] Token token)
        {
            var username = HttpContext.Session.GetString("user");
            if (username.IsNullOrEmpty())
            {
                return RedirectToAction("Create", "Login");
            }
            if (ModelState.IsValid)
            {
                var tokens = new Token()
                {
                    TokenHash = token.TokenHash,
                    User = token.User
                };
                await _context.Tokens.AddAsync(tokens);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(token);
        }

        //// GET: Tokens/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    var username = HttpContext.Session.GetString("user");
        //    if (username.IsNullOrEmpty())
        //    {
        //        return RedirectToAction("Create", "Login");
        //    }
        //    if (id == null || _context.Tokens == null)
        //    {
        //        return NotFound();
        //    }

        //    var token = await _context.Tokens.FindAsync(id);
        //    if (token == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(token);
        //}

        // POST: Tokens/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("TokenId,TokenHash,User")] Token token)
        //{
        //    if (id != token.TokenId)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(token);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!TokenExists(token.TokenId))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(token);
        //}

        // GET: Tokens/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Tokens == null)
            {
                return NotFound();
            }

            var token = await _context.Tokens
                .FirstOrDefaultAsync(m => m.TokenId == id);
            
            if (token != null)
            {
                
                _context.Tokens.Remove(token);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            
            return RedirectToAction("Index");
        }

        // POST: Tokens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Tokens == null)
            {
                return Problem("Entity set 'AttAssAppDbContext.Tokens'  is null.");
            }
            var token = await _context.Tokens.FindAsync(id);
            System.Diagnostics.Debug.WriteLine(token);
            if (token != null)
            {
                System.Diagnostics.Debug.WriteLine(token);
                _context.Tokens.Remove(token);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TokenExists(int id)
        {
          return (_context.Tokens?.Any(e => e.TokenId == id)).GetValueOrDefault();
        }
    }
}
