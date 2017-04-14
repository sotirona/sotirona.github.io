﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using MyBlog.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MyBlog.Controllers.Admin
{
    public class UserController : Controller
    {
        // GET: User
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            using(var database = new BlogDbContext())
            {
                var users = database.Users.ToList();

                var admins = GetAdminUserNames(users, database);
                ViewBag.Admins = admins;

                return View(users);
            }
        }

        private HashSet<string> GetAdminUserNames(List<ApplicationUser> users, BlogDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            var admins = new HashSet<string>();

            foreach (var user in users)
            {
                if (userManager.IsInRole(user.Id, "Admin"))
                {
                    admins.Add(user.UserName);
                }
            }

            return admins;
        }

        public ActionResult Edit (string id)
        {
            if (id==null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                var user = database.Users.Where(u => u.Id == id).First();

                if (user==null)
                {
                    return HttpNotFound();
                }

                var viewModel = new EditUserViewModel();
                viewModel.User = user;
                viewModel.Roles = GetUserRoles(user, database);

                return View(viewModel);
            }
        }

        private List<Role> GetUserRoles(ApplicationUser user, BlogDbContext db)
        {
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var roles = db.Roles.Select(r => r.Name).OrderBy(r => r).ToList();

            var userRoles = new List<Role>();

            foreach (var roleName in roles)
            {
                var role = new Role { Name = roleName };

                if (userManager.IsInRole(user.Id,roleName))
                {
                    role.isSelected = true;
                }
                userRoles.Add(role);
            }
            return userRoles;

        }

        [HttpPost]
        public ActionResult Edit (string id, EditUserViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using(var database = new BlogDbContext())
                {
                    var user = database.Users.FirstOrDefault(u => u.Id == id);

                    if (user==null)
                    {
                        return HttpNotFound();
                    }

                    if (!string.IsNullOrEmpty(viewModel.Password))
                    {
                        var hasher = new PasswordHasher();
                        var passwordHash = hasher.HashPassword(viewModel.Password);
                        user.PasswordHash = passwordHash;
                    }

                    user.Email = viewModel.User.Email;
                    user.FullName = viewModel.User.FullName;
                    user.UserName = viewModel.User.UserName;
                    this.SetUserRoles(user, database, viewModel);

                    database.Entry(user).State = EntityState.Modified;
                    database.SaveChanges();                    

                    return RedirectToAction("List");
                }
            }

            return View(viewModel);
        }

        private void SetUserRoles(ApplicationUser user,BlogDbContext db, EditUserViewModel model)
        {
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            foreach (var role in model.Roles)
            {
                if (role.isSelected)
                {
                    userManager.AddToRole(user.Id, role.Name);
                }

                else if (!role.isSelected)
                {
                    userManager.RemoveFromRole(user.Id, role.Name);
                }
            }
        }

        public ActionResult Delete (string id)
        {
            if (id==null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                
            }

            using (var database = new BlogDbContext())
            {
                var user = database.Users.Where(u => u.Id.Equals(id)).First();
                if (user == null)
                {
                    return HttpNotFound();
                }
                return View(user);
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            }
            using (var database = new BlogDbContext())
            {
                var user = database.Users.Where(u => u.Id.Equals(id)).First();
                var userArticles = database.Articles.Where(a => a.Author.Id == user.Id);

                foreach (var article in userArticles)
                {
                    database.Articles.Remove(article);
                }
                database.Users.Remove(user);
                database.SaveChanges();

                return RedirectToAction("List");
            }
        }
    }
}