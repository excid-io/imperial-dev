﻿using iam.Data;
using iam.Models.RelBAC;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json;

namespace iam.Controllers
{
    public class RelBACController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IamDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public RelBACController(IConfiguration configuration, ILogger<HomeController> logger, IamDbContext context)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var authorizations = _context.Authorizations.ToList();
            return View(authorizations);
        }

        public IActionResult Create()
        {
           var resources = _configuration.GetSection("Objects").Get<List<Resource>>();
            ViewData["relObjects"] = resources;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name", "AuthType", "Relations")] Authorization authorization)
        {
            List<Dictionary<string, string>> relations = new List <Dictionary<string, string>>();
            foreach (var relation in authorization.Relations["Read"])
            {
                if (relation != "false")
                    relations.Add(new Dictionary<string, string>(){ { relation, "reader" } });
            }  
            foreach (var relation in authorization.Relations["Write"])
            {
                if (relation != "false")
                    relations.Add(new Dictionary<string, string>() { { relation, "writer" } });
            }
            authorization.Relationships = JsonSerializer.Serialize(relations);
            authorization.Code = RandomString();
            _context.Add(authorization);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var authorization = _context.Authorizations.Where(m => m.Id == id).FirstOrDefault();
            return View(authorization);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var resource = _context.Authorizations.Where(m => m.Id == id).FirstOrDefault();

            if (resource == null)
            {
                return NotFound();
            }
            _context.Authorizations.Remove(resource);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int? id)
        {
            string response = string.Empty;
            //TODO add security check
            var authorization = _context.Authorizations.FirstOrDefault(q => q.Id == id);
            if (authorization != null)
            {
                response = authorization.Relationships;
            }
            return Ok(response);
        }

        private string RandomString()
        {
            // Create a new instance of the RNGCryptoServiceProvider class
            var rng = RandomNumberGenerator.Create();

            // Create a byte array to store the random bytes
            byte[] bytes = new byte[16];

            // Fill the array with random bytes
            rng.GetBytes(bytes);

            // Convert the byte array to a hexadecimal string
            string hex = Convert.ToHexString(bytes);
            return hex;
        }
    }
}
