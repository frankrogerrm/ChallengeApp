using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChallengeApp.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ChallengeApp.API.DTOs;

namespace ChallengeApp.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/documents")]
public class DocumentsController : BaseController
{
    private readonly ChallengeAppDbContext _context;

    public DocumentsController(ChallengeAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Contributor,Viewer,Manager")]
    public IActionResult GetAccessibleDocuments([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var role = GetUserRole();
        var email = GetUserEmail();

        var sharedWithMe = _context.DocumentShares
                            .Where(s => s.SharedWithUserEmail == email)
                            .Select(s => s.DocumentId)
                            .ToList();

        IQueryable<Document> query = role switch
        {
            "Admin" => _context.Documents,
            "Contributor" => _context.Documents.Where(d => d.UploadedBy == email || d.AccessType == "Public" || sharedWithMe.Contains(d.Id)),
            "Manager" => _context.Documents.Where(d => d.AccessType == "Public" || sharedWithMe.Contains(d.Id)),
            "Viewer" => _context.Documents.Where(d => d.AccessType == "Public" || sharedWithMe.Contains(d.Id)),
            _ => Enumerable.Empty<Document>().AsQueryable()
        };

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var documents = query
            .Include(d => d.Tags) 
            .OrderByDescending(d => d.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new
        {
            currentPage = page,
            pageSize,
            totalPages,
            totalCount,
            documents
        });
    }


    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Contributor")]
    public IActionResult UpdateDocument(int id, [FromBody] DocumentUpdateDto dto)
    {
        var email = GetUserEmail();
        var role = GetUserRole();

        var doc = _context.Documents
            .Include(d => d.Tags)
            .FirstOrDefault(d => d.Id == id);
        if (doc == null) return NotFound();

        var isOwner = doc.UploadedBy == email;
        var hasWriteAccess = _context.DocumentShares
            .Any(s => s.DocumentId == id && s.SharedWithUserEmail == email && s.Permission == "Write");

        if (!isOwner && role != "Admin" && !hasWriteAccess)
            return Forbid();

        doc.Title = dto.Title;
        doc.AccessType = dto.AccessType;
        doc.ModifiedDate = DateTime.UtcNow;
        doc.Tags = dto.Tags.Select(tag => new DocumentTag { Name = tag.Name }).ToList();

        _context.AuditLogs.Add(new AuditLog
        {
            UserEmail = email,
            Action = "Updated Document",
            Entity = "Document",
            Timestamp = DateTime.UtcNow,
            IpAddress = GetClientIp()
        });

        _context.SaveChanges();
        return Ok(doc);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Contributor")]
    public IActionResult DeleteDocument(int id)
    {
        var email = GetUserEmail();
        var role = GetUserRole();

        var doc = _context.Documents.Find(id);
        if (doc == null) return NotFound();

        var isOwner = doc.UploadedBy == email;
        var hasWriteAccess = _context.DocumentShares
            .Any(s => s.DocumentId == id && s.SharedWithUserEmail == email && s.Permission == "Write");

        if (!isOwner && role != "Admin" && !hasWriteAccess)
            return Forbid();

        var filePath = Path.Combine("Uploads", doc.FileName);
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        _context.Documents.Remove(doc);
        _context.AuditLogs.Add(new AuditLog
        {
            UserEmail = email,
            Action = "Deleted Document",
            Entity = "Document",
            Timestamp = DateTime.UtcNow,
            IpAddress = GetClientIp()
        });

        _context.SaveChanges();
        return NoContent();
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Admin,Contributor")]
    public async Task<IActionResult> UploadDocument([FromForm] DocumentUploadDto dto)
    {
        var email = GetUserEmail();
        if (string.IsNullOrWhiteSpace(email)){
            return Unauthorized("Missing email claim.");
        }           

        if (dto.File == null || dto.File.Length == 0)
            return BadRequest("File is required.");

        if (dto.File.Length > 10 * 1024 * 1024)
            return BadRequest("File size exceeds 10MB limit.");

        var allowedExtensions = new[] { ".pdf", ".docx", ".txt" };
        var extension = Path.GetExtension(dto.File.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return BadRequest("Unsupported file type.");

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var uploadPath = Path.Combine("Uploads", uniqueFileName);
        Directory.CreateDirectory("Uploads");

        using (var stream = new FileStream(uploadPath, FileMode.Create))
        {
            await dto.File.CopyToAsync(stream);
        }

        var tagList = JsonSerializer.Deserialize<List<TagDto>>(dto.Tags);
        var document = new Document
        {
            Title = dto.Title,
            AccessType = dto.AccessType,
            FileName = uniqueFileName,
            FileSize = dto.File.Length,
            UploadedBy = email,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow,
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        if (tagList!.Count > 0)
        {
            var tagEntities = tagList.DistinctBy(x => x.Name).Select(tag => new DocumentTag
            {
                Name = tag.Name,
                DocumentId = document.Id
            }).ToList();
            _context.DocumentTags.AddRange(tagEntities);
        }    

        _context.AuditLogs.Add(new AuditLog
        {
            UserEmail = email,
            Action = "Uploaded Document",
            Entity = "Document",
            Timestamp = DateTime.UtcNow,
            IpAddress = GetClientIp()
        });

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAccessibleDocuments), new { id = document.Id }, document);
    }

    [HttpGet("download/{id}")]
    [Authorize(Roles = "Admin,Contributor,Viewer,Manager")]
    public IActionResult DownloadDocument(int id)
    {
        var email = GetUserEmail();
        var role = GetUserRole();

        var doc = _context.Documents
            .Include(d => d.Tags)
            .FirstOrDefault(d => d.Id == id);

        if (doc == null)
            return NotFound();

        var isOwner = doc.UploadedBy == email;
        var isPublic = doc.AccessType == "Public";
        var isAdmin = role == "Admin";

        var hasShare = _context.DocumentShares
             .Any(s => s.DocumentId == id && s.SharedWithUserEmail == email &&
              (s.Permission == "Read" || s.Permission == "Write"));
        if (!isOwner && !isPublic && !isAdmin && !hasShare)
        {
            return Forbid();
        }

        var filePath = Path.Combine("Uploads", doc.FileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound("File not found on server.");

        var mimeType = GetMimeType(doc.FileName);
        var fileBytes = System.IO.File.ReadAllBytes(filePath);

        _context.AuditLogs.Add(new AuditLog
        {
            UserEmail = email,
            Action = "Downloaded Document",
            Entity = "Document",
            Timestamp = DateTime.UtcNow,
            IpAddress = GetClientIp()
        });

        _context.SaveChanges();

        return File(fileBytes, mimeType, doc.FileName);
    }

    private string GetMimeType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }

    [HttpGet("search")]
    [Authorize(Roles = "Admin,Contributor,Viewer,Manager")]
    public IActionResult SearchDocuments([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var role = GetUserRole();
        var email = GetUserEmail();

        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Search query is required.");

        var normalized = query.Trim().ToLower();

        var sharedWithMe = _context.DocumentShares
            .Where(s => s.SharedWithUserEmail == email)
            .Select(s => s.DocumentId)
            .ToList();

        IQueryable<Document> baseQuery = role switch
        {
            "Admin" => _context.Documents,
            "Contributor" => _context.Documents.Where(d =>
                d.UploadedBy == email || d.AccessType == "Public" || sharedWithMe.Contains(d.Id)),
            "Manager" => _context.Documents.Where(d =>
                d.AccessType == "Public" || sharedWithMe.Contains(d.Id)),
            "Viewer" => _context.Documents.Where(d =>
                d.AccessType == "Public" || sharedWithMe.Contains(d.Id)),
            _ => Enumerable.Empty<Document>().AsQueryable()
        };

        baseQuery = baseQuery
            .Include(d => d.Tags)
            .Where(d =>
                d.Title.ToLower().Contains(normalized) ||
                d.Tags.Any(t => t.Name.ToLower().Contains(normalized)) ||
                d.FileName.ToLower().EndsWith(normalized)
            );

        var totalCount = baseQuery.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var documents = baseQuery
            .OrderByDescending(d => d.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new
        {
            query,
            currentPage = page,
            pageSize,
            totalPages,
            totalCount,
            documents
        });
    }

    [HttpPost("{id}/share")]
    [Authorize(Roles = "Admin,Contributor")]
    public IActionResult ShareDocument(int id, [FromBody] DocumentShareDto dto)
    {
        var email = GetUserEmail();
        var role = GetUserRole();

        var doc = _context.Documents.Find(id);
        if (doc == null) return NotFound();

        if (doc.UploadedBy != email && role != "Admin")
            return Forbid();

        foreach (var recipient in dto.Emails.Distinct())
        {
            var existing = _context.DocumentShares
                .FirstOrDefault(s => s.DocumentId == id && s.SharedWithUserEmail == recipient);

            if (existing != null)
            {
                existing.Permission = dto.Permission;
            }
            else
            {
                _context.DocumentShares.Add(new DocumentShare
                {
                    DocumentId = id,
                    SharedWithUserEmail = recipient,
                    Permission = dto.Permission,
                    Document = doc
                });
            }
        }

        _context.AuditLogs.Add(new AuditLog
        {
            UserEmail = email,
            Action = "Shared Document",
            Entity = "Document",
            Timestamp = DateTime.UtcNow,
            IpAddress = GetClientIp()
        });

        _context.SaveChanges();
        return Ok(new { message = "Document shared successfully." });
    }
    [HttpGet("{id}/shares")]
    [Authorize(Roles = "Admin,Contributor")]
    public IActionResult GetDocumentShares(int id)
    {
        var email = GetUserEmail();
        var role = GetUserRole();

        var doc = _context.Documents.Find(id);
        if (doc == null) return NotFound();

        var isOwner = doc.UploadedBy == email;
        if (!isOwner && role != "Admin")
            return Forbid();

        var shares = _context.DocumentShares
            .Where(s => s.DocumentId == id)
            .Select(s => new
            {
                email = s.SharedWithUserEmail,
                permission = s.Permission
            })
            .ToList();

        return Ok(shares);
    }

    [HttpDelete("{id}/share")]
    [Authorize(Roles = "Admin,Contributor")]
    public IActionResult RevokeShare(int id, [FromQuery] string email)
    {
        var currentUser = GetUserEmail();
        var role = GetUserRole();

        var doc = _context.Documents.Find(id);
        if (doc == null) return NotFound();

        if (doc.UploadedBy != currentUser && role != "Admin")
            return Forbid();

        var share = _context.DocumentShares
            .FirstOrDefault(s => s.DocumentId == id && s.SharedWithUserEmail == email);

        if (share == null) return NotFound("Share entry not found.");

        _context.DocumentShares.Remove(share);

        _context.AuditLogs.Add(new AuditLog
        {
            UserEmail = currentUser,
            Action = "Revoked Share",
            Entity = "Document",
            Timestamp = DateTime.UtcNow,
            IpAddress = GetClientIp()
        });

        _context.SaveChanges();
        return Ok(new { message = "Access revoked." });
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Contributor,Viewer,Manager")]
    public IActionResult GetDocumentById(int id)
    {
        var email = GetUserEmail();
        var role = GetUserRole();

        var doc = _context.Documents
            .Include(d => d.Tags)
            .FirstOrDefault(d => d.Id == id);

        if (doc == null)
            return NotFound();

        var isOwner = doc.UploadedBy == email;
        var isPublic = doc.AccessType == "Public";
        var isAdmin = role == "Admin";

        var hasShare = _context.DocumentShares
            .Any(s => s.DocumentId == id && s.SharedWithUserEmail == email);

        if (!isOwner && !isPublic && !isAdmin && !hasShare)
            return Forbid();

        var metadata = new
        {
            doc.Id,
            doc.Title,
            doc.AccessType,
            doc.FileName,
            doc.FileSize,
            doc.UploadedBy,
            doc.CreatedDate,
            doc.ModifiedDate,
            Tags = doc.Tags.Select(t => t.Name)
        };

        return Ok(metadata);
    }
}
