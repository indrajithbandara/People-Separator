﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PipSep.Models;

namespace PipSep.Controllers
{
	[Authorize]
	public class CVController : Controller
	{
		private PipSepDb db = new PipSepDb();

		//
		// GET: /CV/
		//[Authorize(Roles = "Administrator, Editor")]
		public ViewResult Index()
        {
            ViewBag.CVsSent = db.ApplicationCVs.Count();
            ViewBag.CVsApproved = db.ApplicationCVs.Count(r => r.IsAccepted == true);
            if (db.ApplicationCVs.Count() == db.ApplicationCVs.Count(r => r.HasBeenVerified == true)) 
            {
                ViewBag.CanFinish = true;
            }
            else
            { 
                ViewBag.CanFinish = false;
            }

            return View(db.ApplicationCVs.ToList());

        }

		//
		// GET: /CV/Details/5
		//[Authorize(Roles = "Administrator, Editor")]
		public ViewResult Details(int id)
		{
			ApplicationCV applicationcv = db.ApplicationCVs.Find(id);
			return View(applicationcv);
		}

		//
		// GET: /CV/Create

		public ActionResult Create()
		{
			return View();
		}

		//
		// POST: /CV/Create

		[HttpPost]
		public ActionResult Create(ApplicationCV applicationcv)
		{
			applicationcv.UserName = HttpContext.User.Identity.Name;
			if (ModelState.IsValid)
			{
				//Stupid ASP
				if (HttpContext.Request.Form.Get("gender") == "Male")
					applicationcv.Gender = true;
				else
					applicationcv.Gender = false;

				db.ApplicationCVs.Add(applicationcv);
				db.SaveChanges();
				
				return RedirectToAction("EditCreate", new {hasSent=true});
			}

			return View(applicationcv);
		}

		//
		// GET: /CV/Edit/5

		public ActionResult Edit(int id, bool message)
		{
			ApplicationCV applicationcv = db.ApplicationCVs.Find(id);
			ViewBag.Gender = applicationcv.Gender;
            if (message)
                ViewBag.Message = "You have succesfully sent your CV.";
               


			if (this.User.Identity.Name == applicationcv.UserName)
			{
				return View(applicationcv);
			}
			else
			{
				return RedirectToAction("Index", "Home");
			}
		}

		//
		// POST: /CV/Edit/5

		[HttpPost]
		public ActionResult Edit(ApplicationCV applicationcv)
		{
			if (1 == 1) //TO DO: Add validation
			{
				applicationcv.UserName = HttpContext.User.Identity.Name;

				

				if (ModelState.IsValid)
				{
					//Stupid ASP
					if (HttpContext.Request.Form.Get("gender") == "Male")
						applicationcv.Gender = true;
					else
						applicationcv.Gender = false;

					db.Entry(applicationcv).State = EntityState.Modified;
					db.SaveChanges();
					ViewBag.Message = "You have succesfully sent your CV.";
					return View(applicationcv);
				}
				ViewBag.Message = "You have an error.";
				return View(applicationcv);
			}
		}

		//
		// GET: /CV/Delete/5
		[Authorize(Roles = "Administrator, Editor")]
		public ActionResult Delete(int id)
		{
			ApplicationCV applicationcv = db.ApplicationCVs.Find(id);
			return View(applicationcv);
		}

		//
		// POST: /CV/Delete/5
		[Authorize(Roles = "Administrator, Editor")]
		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(int id)
		{
			ApplicationCV applicationcv = db.ApplicationCVs.Find(id);
			db.ApplicationCVs.Remove(applicationcv);
			db.SaveChanges();
			return RedirectToAction("Index");
		}

		protected override void Dispose(bool disposing)
		{
			db.Dispose();
			base.Dispose(disposing);
		}

		//
		// GET: /CV/EditCreate
		[Authorize]
		public ActionResult EditCreate(bool hasSent = false)
		{
			string UserName = HttpContext.User.Identity.Name;
			ApplicationCV applicationcv = db.ApplicationCVs.SingleOrDefault(r => r.UserName == UserName);
			if (applicationcv == null)
				return RedirectToAction("Create", "CV");
			else
				return RedirectToAction("Edit", new { id = applicationcv.Id, message=hasSent });
		}

		//
		// GET: /CV/Editor/5
		//[Authorize(Roles = "Administrator, Editor")]
		public ActionResult Editor(int id)
		{
			ApplicationCV applicationcv = db.ApplicationCVs.Find(id);
			return View(applicationcv);
		}

		//
		// POST: /CV/Editor/5
		//[Authorize(Roles = "Administrator, Editor")]
		[HttpPost]
		public ActionResult Editor(string UserName, string Id, ApplicationCV applicationcv)
		{
			//applicationcv.UserName = HttpContext.User.Identity.Name;
			applicationcv.WhoConfirmedIt = HttpContext.User.Identity.Name;
			applicationcv.HasBeenVerified = true;
			applicationcv.UserName = UserName;
			applicationcv.Id = int.Parse(Id);

			if (ModelState.IsValid)
			{
				db.Entry(applicationcv).State = EntityState.Modified;
				db.SaveChanges();

				ApplicationCV next = db.ApplicationCVs.FirstOrDefault(r => r.Id > applicationcv.Id && r.HasBeenVerified == false);
				if (next != null)
					ViewBag.Next = next.Id;

				ViewBag.Message = "The operation has suceeded.";
				return View(applicationcv);
			}

			ViewBag.Message = "There has been an error.";
			return View(applicationcv);
		}

        //
        // POST: /CV/Finish

        public ActionResult Finish()
        {
            System.Web.Configuration.WebConfigurationManager.AppSettings.Set("AreRegistrationsAllowed","false");
            List<string> users = db.ApplicationCVs.Where(r => r.IsAccepted == true).Select(r => r.UserName).ToList();
            Page page = new Page();
            page = Page.AddDateCreatedAndAuthor(page);
            page.Content = "<p>And the participants are:</p><ul>";
            foreach(var user in users){
                page.Content += "<li>" + user + "</li>";
            }
            page.Content += "</ul>";
            page.IsMenu = true;
            page.IsBlog = false;
            page.IsComment = false;
            page.IsSticky = false;
            
            page.Title = "Participants";
            db.Pages.Add(page);
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
	}


}