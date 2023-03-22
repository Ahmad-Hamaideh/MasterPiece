using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using test.Models;

namespace test.Controllers
{
    public class sellersController : Controller
    {
        private SilvanaEntities db = new SilvanaEntities();

        // GET: sellers
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var seller = db.sellers.Include(s => s.AspNetUser).FirstOrDefault(s => s.AspNetUser.Id == userId);

            return View(seller);
        }
        public ActionResult setting()
        {
            //var userId = User.Identity.GetUserId();
            //var seller = db.sellers.Include(s => s.AspNetUser).FirstOrDefault(s => s.AspNetUser.Id == userId);

            return View();
        }

        // GET: sellers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            seller seller = db.sellers.Find(id);
            if (seller == null)
            {
                return HttpNotFound();
            }
            return View(seller);
        }

        // GET: sellers/Create
        public ActionResult Create()
        {
            ViewBag.AspUser = new SelectList(db.AspNetUsers, "Id", "Email");
            ViewBag.city_id = new SelectList(db.cities, "city_id", "City_Name");
            return View();
        }

        // POST: sellers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "seller_id,seller_name,city_id,AspUser,address,email,phone_number,join_date")] seller seller)
        {
            if (ModelState.IsValid)
            {
                db.sellers.Add(seller);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AspUser = new SelectList(db.AspNetUsers, "Id", "Email", seller.AspUser);
            ViewBag.city_id = new SelectList(db.cities, "city_id", "City_Name", seller.city_id);
            return View(seller);
        }

        // GET: sellers/Edit/5

        public ActionResult sss()
        {
            string userId = User.Identity.GetUserId();

            // Retrieve the corresponding seller object from the database
            seller currentSeller = db.sellers.SingleOrDefault(s => s.AspUser == userId);

            if (currentSeller == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (currentSeller == null)
            {
                return HttpNotFound();
            }
           
            return View("setting");
        }
        // POST: sellers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.


        public ActionResult Edit([Bind(Include = "seller_id,seller_name,address")] seller seller)
        {
            string userId = User.Identity.GetUserId();

            // Retrieve the corresponding seller object from the database
            seller currentSeller = db.sellers.SingleOrDefault(s => s.AspUser == userId);

            if (currentSeller != null)
            {
                // Update the seller object with the new values
                currentSeller.seller_name = Request["seller_name"];
                currentSeller.address = Request["address"] ;

            
                db.SaveChanges();

                Session["Last_Updet"] = DateTime.Now;

                return RedirectToAction("Index");
            }
            ViewBag.AspUser = new SelectList(db.AspNetUsers, "Id", "Email", seller.AspUser);
            ViewBag.city_id = new SelectList(db.cities, "city_id", "City_Name", seller.city_id);
            return View("setting");
        }

        // GET: sellers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            seller seller = db.sellers.Find(id);
            if (seller == null)
            {
                return HttpNotFound();
            }
            return View(seller);
        }

        // POST: sellers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            seller seller = db.sellers.Find(id);
            db.sellers.Remove(seller);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
