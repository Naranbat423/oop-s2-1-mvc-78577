using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Library.Domain.Models;
using Library.MVC.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace Library.MVC.Controllers;

[Authorize]
public class LoansController : Controller
{
    private readonly ApplicationDbContext _context;

    public LoansController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Loans
    public async Task<IActionResult> Index()
    {
        var loans = await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .OrderByDescending(l => l.LoanDate)
            .ToListAsync();
        
        return View(loans);
    }

    // GET: Loans/Create
    public async Task<IActionResult> Create()
    {
        // Only show available books
        ViewBag.BookId = new SelectList(await _context.Books
            .Where(b => b.IsAvailable)
            .ToListAsync(), "Id", "Title");
        
        ViewBag.MemberId = new SelectList(await _context.Members
            .ToListAsync(), "Id", "FullName");
        
        return View();
    }

    // POST: Loans/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("BookId,MemberId")] Loan loan)
    {
        // Check if book exists and is available
        var book = await _context.Books.FindAsync(loan.BookId);
        if (book == null || !book.IsAvailable)
        {
            ModelState.AddModelError("BookId", "This book is not available for loan.");
            await PopulateDropdowns(loan.BookId, loan.MemberId);
            return View(loan);
        }

        // Check if there's already an active loan for this book (business rule)
        var activeLoan = await _context.Loans
            .AnyAsync(l => l.BookId == loan.BookId && l.ReturnedDate == null);
        
        if (activeLoan)
        {
            ModelState.AddModelError("BookId", "This book is already on loan.");
            await PopulateDropdowns(loan.BookId, loan.MemberId);
            return View(loan);
        }

        if (ModelState.IsValid)
        {
            loan.LoanDate = DateTime.Today;
            loan.DueDate = DateTime.Today.AddDays(14); // 2-week loan period
            
            _context.Add(loan);
            
            // Update book availability
            book.IsAvailable = false;
            _context.Update(book);
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        await PopulateDropdowns(loan.BookId, loan.MemberId);
        return View(loan);
    }

    // POST: Loans/MarkReturned/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkReturned(int id)
    {
        var loan = await _context.Loans
            .Include(l => l.Book)
            .FirstOrDefaultAsync(l => l.Id == id);
        
        if (loan == null)
        {
            return NotFound();
        }

        if (loan.ReturnedDate == null)
        {
            loan.ReturnedDate = DateTime.Today;
            
            // Make book available again
            if (loan.Book != null)
            {
                loan.Book.IsAvailable = true;
                _context.Update(loan.Book);
            }
            
            _context.Update(loan);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns(int? selectedBookId = null, int? selectedMemberId = null)
    {
        ViewBag.BookId = new SelectList(await _context.Books
            .Where(b => b.IsAvailable)
            .ToListAsync(), "Id", "Title", selectedBookId);
        
        ViewBag.MemberId = new SelectList(await _context.Members
            .ToListAsync(), "Id", "FullName", selectedMemberId);
    }

    private bool LoanExists(int id)
    {
        return _context.Loans.Any(e => e.Id == id);
    }
}