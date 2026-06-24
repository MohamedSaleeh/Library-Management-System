using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.ApiControllers
{
    /// <summary>
    /// RESTful API Controller for Member operations.
    /// Provides CRUD endpoints returning JSON responses.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class MembersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MembersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: api/MembersApi
        /// Retrieves all members.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetMembers()
        {
            var members = await _context.Members
                .Select(m => new MemberDto
                {
                    Id = m.Id,
                    FullName = m.FullName,
                    Email = m.Email,
                    PhoneNumber = m.PhoneNumber,
                    Address = m.Address,
                    RegistrationDate = m.RegistrationDate
                })
                .ToListAsync();

            return Ok(members);
        }

        /// <summary>
        /// GET: api/MembersApi/5
        /// Retrieves a specific member by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<MemberDto>> GetMember(int id)
        {
            var member = await _context.Members.FindAsync(id);

            if (member == null)
            {
                return NotFound(new { message = $"Member with ID {id} not found." });
            }

            var memberDto = new MemberDto
            {
                Id = member.Id,
                FullName = member.FullName,
                Email = member.Email,
                PhoneNumber = member.PhoneNumber,
                Address = member.Address,
                RegistrationDate = member.RegistrationDate
            };

            return Ok(memberDto);
        }

        /// <summary>
        /// POST: api/MembersApi
        /// Creates a new member.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<MemberDto>> PostMember(MemberDto memberDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var member = new Member
            {
                FullName = memberDto.FullName,
                Email = memberDto.Email,
                PhoneNumber = memberDto.PhoneNumber,
                Address = memberDto.Address,
                RegistrationDate = DateTime.Now
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            memberDto.Id = member.Id;
            memberDto.RegistrationDate = member.RegistrationDate;

            return CreatedAtAction(nameof(GetMember), new { id = member.Id }, memberDto);
        }

        /// <summary>
        /// PUT: api/MembersApi/5
        /// Updates an existing member.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMember(int id, MemberDto memberDto)
        {
            if (id != memberDto.Id)
            {
                return BadRequest(new { message = "ID mismatch." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound(new { message = $"Member with ID {id} not found." });
            }

            member.FullName = memberDto.FullName;
            member.Email = memberDto.Email;
            member.PhoneNumber = memberDto.PhoneNumber;
            member.Address = memberDto.Address;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// DELETE: api/MembersApi/5
        /// Deletes a member.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound(new { message = $"Member with ID {id} not found." });
            }

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.Id == id);
        }
    }
}
