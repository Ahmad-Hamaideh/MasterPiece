using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.EnterpriseServices.CompensatingResourceManager;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using test.Models;

namespace test.Controllers
{
    public class ProductsController : Controller
    {
        private SilvanaEntities db = new SilvanaEntities();

        // GET: Products
        public ActionResult Products()
        {
            {
                var categoryNames = db.categories
                  .Select(c => c.category_Name);
                return View();
            }
        }
        public ActionResult AllProducts()
        {
            var products = db.Products.ToList();
            return View("products", products);
        }
        public ActionResult NewArrivals()
        {
            var products = db.Products.OrderByDescending(p => p.Date).Take(50).ToList();

            return View("products", products);
        }
        public ActionResult Trinnding()
        {

            var Trinnding = db.Products.Where(p => p.count > 50).ToList();
            return View("products", Trinnding);
        }
        public ActionResult TopRated()
        {
            //var topRated = db.Products
            //.Where(p => p.count > 50 && p.reviews.Any(r => r.rating> 4.5 )).Take(4).ToList();

            var topRated = db.Products.Where(p => p.count > 50).ToList();
            return View("products", topRated);
        }

        public ActionResult Filtered(int id)
        {
            var Filtered = db.Products.Where(s => s.category_id == id).Include(p => p.category).ToList();
            return View("products", Filtered);
        }



        // فلترة المنتجات عن طريق السعر
        public ActionResult FilterProducts(float? minPrice, float? maxPrice)
        {
            var products = db.Products.ToList();
            var productsInRange = products
                  .Where(p => (!minPrice.HasValue || p.ProductID >= minPrice.Value) &&
                    (!maxPrice.HasValue || p.ProductID <= maxPrice.Value)).ToList();

            return View("products", productsInRange);
        }


        public ActionResult SingleProduct(int? id)
        {

            var product = db.Products.Where(x => x.ProductID == id).FirstOrDefault();

            return View( "Singleproduct" , product );

        }
        //public ActionResult Singleproperity(int? id)
        //{
        //    var userId = User.Identity.GetUserId();

        //    if (userId == null)
        //    {
        //        TempData["reg"] = "Sorry, you need to log in to add a product";
        //    }
        //    else
        //    {
        //        TempData["reg"] = null;
        //    }
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    Product product = db.Products.Find(id);
        //    if (product == null)
        //    {
        //        return HttpNotFound();
        //    }


        //    Session["IdProduct"] = Request.QueryString["id"];


        //    return View(product);
        //}

        public ActionResult AddtoCart([Bind(Include = "cart_id,user_id,quantity,created_at,updated_at,product_id")] Cart cart, int id)
        {
            var userId = User.Identity.GetUserId();

            if (userId == null)
            {
                TempData["reg"] = "Sorry, you need to log in to add a product";
            }
            else
            {
                TempData["reg"] = null;

                Product product = db.Products.Find(id);

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }


                if (product == null)
                {
                    return HttpNotFound();
                }


                Session["IdProduct"] = Request.QueryString["id"];


                if (ModelState.IsValid)
                {
                    int quantitys = Convert.ToInt32(Request["quantity"]);
                    cart.quantity = quantitys;
                    cart.product_id = id;
                    cart.updated_at = DateTime.UtcNow;
                    cart.user_id = User.Identity.GetUserId();
                    db.Carts.Add(cart);
                    db.SaveChanges();
                    return RedirectToAction("cart");
                }
            }
                return View("Singleproduct");
          

        }


        public ActionResult Cart()
        {
            //var user = User.Identity.GetUserId();   
            var user = User.Identity.GetUserId();
            int count = db.Carts.Where(a => a.user_id == user).Count();
            ViewBag.CartCount = count;
            Session["CartCount"] = count;
            if (!db.Carts.Where(a => a.user_id == user).Any())
            {

            }
            else
            {
                double totalPrice = db.Carts.Where(a => a.user_id == user).Sum(a => a.Product.Price * 1);
             ViewBag.TotalPrice = totalPrice;
            Session["TotalPrice"] = totalPrice;
            }
       







            //}
            //if (count > 0)
            //{
            //decimal totalPrice = db.Carts.Where(a => a.user_id == user).Sum(a => a.Product.Price * a.quantity);
            //ViewBag.TotalPrice = totalPrice;
            //    ViewBag.Discount = "0";




            //    if (totalPrice >= 455 && totalPrice <= 689)
            //    {
            //        ViewBag.Discount = "5";
            //        //priceafter = totalPrice - (totalPrice * (10 / 100));
            //        float sub = totalPrice * ((float)5.0 / (float)100.0);
            //        priceafter = totalPrice - sub;


            //    }
            //    else if (totalPrice >= 690 && totalPrice <= 849)
            //    {
            //        ViewBag.Discount = "10";
            //        //priceafter = totalPrice - (totalPrice * (15 / 100));
            //        float sub = totalPrice * ((float)10.0 / (float)100.0);
            //        priceafter = totalPrice - sub;

            //    }
            //    else if (totalPrice >= 850 && totalPrice <= 949)
            //    {
            //        ViewBag.Discount = "15";
            //        float sub = totalPrice * ((float)15.0 / (float)100.0);
            //        priceafter = totalPrice - sub;

            //    }
            //    else if (totalPrice >= 950)
            //    {
            //        ViewBag.Discount = "20";
            //        //priceafter = totalPrice - (totalPrice * (25 / 100));
            //        float sub = totalPrice * ((float)20.0 / (float)100.0);
            //        priceafter = totalPrice - sub;

            //    }

            //    if (totalPrice > 1500)
            //    {

            //        ViewBag.shipping = 0;
            //    }
            //    else
            //    {
            //        priceafter += 10;
            //        ViewBag.shipping = 10;
            //    }
            //    ViewBag.AfterPrice = priceafter;
            //    Session["FinalTotal"] = priceafter;
            //}
            return View(db.Carts.Where(a => a.user_id == user).ToList());
        }


     
        public ActionResult DeleteConfirmed(int id)
        {
            Cart cart = db.Carts.Find(id);
            db.Carts.Remove(cart);
            db.SaveChanges();
            return RedirectToAction("cart");

        }


        public ActionResult checkout()
        {
            var user = User.Identity.GetUserId(); 
            return View(db.Carts.Where(a => a.user_id == user).ToList());

        }

        public ActionResult CreateOrder([Bind(Include = "OrderID,UserID,OrderDate,OrderStatus,Payment_id,ShippingAddress,recipientName,Phone,Email,OrderNotes,City")] Order order)
        {

            var user = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
       

                order.ShippingAddress = Request["ShippingAddress"];
                order.Email = Request["email"];
                order.Phone= Request["phone"];
                order.recipientName= Request["recipientName"];
                order.OrderNotes = Request["OrderNotes"];
                order.City = Request["city"];
                order.UserID = user;
                order.Payment_id = 1;
                order.OrderStatus = "1" ;
                order.OrderDate = DateTime.Now;
                db.Orders.Add(order);
                db.SaveChanges();     
                Cart cart = db.Carts.Find(user);
                if (cart != null)
                {
                    db.Carts.Remove(cart);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }

          
            return View(order);
        }





        //       public List<Product> SingleProduct(int id)
        //       {
        //           // code to retrieve products from database and map to List<Product>
        //           List<Product> products = new List<Product>();
        //           // add products to list
        //           var product = db.Products.Find(id);

        //           var productData = (
        //    from p in db.Products
        //    where p.ProductID == id
        //    select new
        //    {
        //        Id = p.ProductID,
        //        Name = p.Name,
        //        Description = p.Description,
        //        Price = p.Price,
        //        ImageUrl = p.Image,
        //    }
        //).FirstOrDefault();

        //           return products;
        //       }


        //public PartialViewResult lol()
        //{
        //    var products = db.Products.Include(p => p.category).Take(3).ToList();
        //    return PartialView("_lol", products);
        //}


        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.category_id = new SelectList(db.categories, "category_id", "category_Name");
            ViewBag.sellerID = new SelectList(db.sellers, "seller_id", "seller_name");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,Name,Description,Price,Quantity,Image,Status,sellerID,category_id,Date")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.category_id = new SelectList(db.categories, "category_id", "category_Name", product.category_id);
            ViewBag.sellerID = new SelectList(db.sellers, "seller_id", "seller_name", product.sellerID);
            return View(product);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.category_id = new SelectList(db.categories, "category_id", "category_Name", product.category_id);
            ViewBag.sellerID = new SelectList(db.sellers, "seller_id", "seller_name", product.sellerID);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,Name,Description,Price,Quantity,Image,Status,sellerID,category_id,Date")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.category_id = new SelectList(db.categories, "category_id", "category_Name", product.category_id);
            ViewBag.sellerID = new SelectList(db.sellers, "seller_id", "seller_name", product.sellerID);
            return View(product);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Product product = db.Products.Find(id);
        //    db.Products.Remove(product);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

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
