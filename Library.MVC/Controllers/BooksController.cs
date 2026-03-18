using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Library.Domain.Models;
using Library.MVC.Data;
using Library.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Library.MVC.Controllers;

[Authorize]
public class BooksController : Controller
{
    private readonly ApplicationDbContext _context;

    public BooksController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Books with search and filter
    public async Task<IActionResult> Index(string searchTerm, string selectedCategory, string availabilityFilter)
    {
        // Start with IQueryable for EF Core composition
        var booksQuery = _context.Books.AsQueryable();

        // Search by Title or Author
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            booksQuery = booksQuery.Where(b => 
                b.Title.Contains(searchTerm) || 
                b.Author.Contains(searchTerm));
        }

        // Filter by Category
        if (!string.IsNullOrWhiteSpace(selectedCategory) && selectedCategory != "All")
        {
            booksQuery = booksQuery.Where(b => b.Category == selectedCategory);
        }

        // Filter by Availability
        if (!string.IsNullOrWhiteSpace(availabilityFilter) && availabilityFilter != "All")
        {
            if (availabilityFilter == "Available")
            {
                booksQuery = booksQuery.Where(b => b.IsAvailable);
            }
            else if (availabilityFilter == "OnLoan")
            {
                booksQuery = booksQuery.Where(b => !b.IsAvailable);
            }
        }

        // Get distinct categories for filter dropdown
        var categories = await _context.Books
            .Select(b => b.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        var viewModel = new BookFilterViewModel
        {
            SearchTerm = searchTerm,
            SelectedCategory = selectedCategory,
            AvailabilityFilter = availabilityFilter,
            Categories = categories.Select(c => new SelectListItem 
            { 
                Value = c, 
                Text = c,
                Selected = c == selectedCategory 
            }).ToList()
        };

        ViewBag.FilterViewModel = viewModel;
        return View(await booksQuery.ToListAsync());
    }

    // GET: Books/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Books/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Author,Isbn,Category")] Book book)
    {
        if (ModelState.IsValid)
        {
            book.IsAvailable = true; // New books are available by default
            _context.Add(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(book);
    }

    // GET: Books/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var book = await _context.Books.FindAsync(id);
        if (book == null)
        {
            return NotFound();
        }
        return View(book);
    }

    // POST: Books/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,Isbn,Category,IsAvailable")] Book book)
    {
        if (id != book.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(book);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(book.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(book);
    }

    // GET: Books/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var book = await _context.Books
            .FirstOrDefaultAsync(m => m.Id == id);
        if (book == null)
        {
            return NotFound();
        }

        return View(book);
    }

    // POST: Books/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book != null)
        {
            _context.Books.Remove(book);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool BookExists(int id)
    {
        return _context.Books.Any(e => e.Id == id);
    }
}