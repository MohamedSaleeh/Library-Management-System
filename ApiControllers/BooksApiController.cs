using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.ApiControllers
{
    /// <summary>
    /// RESTful API Controller for Book operations.
    /// Provides CRUD endpoints returning JSON responses.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BooksApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BooksApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: api/BooksApi
        /// Retrieves all books.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks()
        {
            var books = await _context.Books
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    ISBN = b.ISBN,
                    Category = b.Category,
                    PublishedYear = b.PublishedYear,
                    QuantityAvailable = b.QuantityAvailable
                })
                .ToListAsync();

            return Ok(books);
        }

        /// <summary>
        /// GET: api/BooksApi/5
        /// Retrieves a specific book by ID.
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<BookDto>> GetBook(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound(new { message = $"Book with ID {id} not found." });
            }

            var bookDto = new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                Category = book.Category,
                PublishedYear = book.PublishedYear,
                QuantityAvailable = book.QuantityAvailable
            };

            return Ok(bookDto);
        }

        /// <summary>
        /// POST: api/BooksApi
        /// Creates a new book.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BookDto>> PostBook(BookDto bookDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var book = new Book
            {
                Title = bookDto.Title,
                Author = bookDto.Author,
                ISBN = bookDto.ISBN,
                Category = bookDto.Category,
                PublishedYear = bookDto.PublishedYear,
                QuantityAvailable = bookDto.QuantityAvailable
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            bookDto.Id = book.Id;

            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, bookDto);
        }

        /// <summary>
        /// PUT: api/BooksApi/5
        /// Updates an existing book.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutBook(int id, BookDto bookDto)
        {
            if (id != bookDto.Id)
            {
                return BadRequest(new { message = "ID mismatch." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound(new { message = $"Book with ID {id} not found." });
            }

            book.Title = bookDto.Title;
            book.Author = bookDto.Author;
            book.ISBN = bookDto.ISBN;
            book.Category = bookDto.Category;
            book.PublishedYear = bookDto.PublishedYear;
            book.QuantityAvailable = bookDto.QuantityAvailable;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// DELETE: api/BooksApi/5
        /// Deletes a book.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound(new { message = $"Book with ID {id} not found." });
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
