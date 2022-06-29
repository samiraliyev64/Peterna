using backend.DAL;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Helpers;
using Microsoft.AspNetCore.Hosting;
using backend.Models;

namespace backend.Areas.AdminPanel.Controllers
{
    [Area("AdminPanel")]
    public class ServiceController : Controller
    {
        private AppDbContext _context { get; }
        private IWebHostEnvironment _env { get; }
        public ServiceController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        //Read
        public IActionResult Index()
        {
            return View(_context.Services.ToList());
        }
        //Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return BadRequest();
            }
            Service service = _context.Services.Find(id);
            if(service == null)
            {
                return NotFound();
            }
            var path = Helper.GetPath(_env.WebRootPath, "images", service.Url);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
        //Create (GET)
        public IActionResult Create()
        {
            return View();
        }

        //Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            if (!service.Photo.CheckFileSize(200))
            {
                ModelState.AddModelError("Photo", "file size must be less than 200 KB");
                return View();
            }
            if (!service.Photo.CheckFileType("image/"))
            {
                ModelState.AddModelError("Photo", "file type must be image");
                return View();
            }
            service.Url = await service.Photo.SaveFileAsync(_env.WebRootPath, "images");
            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        //Update (GET)
        public IActionResult Update(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Service service = _context.Services.Find(id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }
        //Update (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Service service)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Service serviceDb = _context.Services.Find(id);
            if (serviceDb == null)
            {
                return NotFound();
            }
            service.Url = await service.Photo.SaveFileAsync(_env.WebRootPath, "images");
            var pathDb = Helper.GetPath(_env.WebRootPath, "images", serviceDb.Url);
            if (System.IO.File.Exists(pathDb))
            {
                System.IO.File.Delete(pathDb);
            }
            serviceDb.Url = service.Url;
            serviceDb.Title = service.Title;
            serviceDb.Subtitle = service.Subtitle;
            serviceDb.Description = service.Description;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
    }
}
