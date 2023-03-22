using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using test.Models;

namespace test.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }




        public ActionResult Registration()
        {
            Session.Remove("join");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Include = "seller_id,seller_name,city_id,AspUser,address,email,phone_number,join_date")] seller seller , HttpPostedFileBase pic)
        {
            SilvanaEntities db = new SilvanaEntities();
            string userId = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                if (pic != null)
                {
                    var pathpic = Path.GetFileName(pic.FileName);
                    pic.SaveAs(Path.Combine(Server.MapPath("~/assets/images/ID_Photo/"), pic.FileName));
                    seller.ID_photo = pathpic;
                    seller.AspUser = userId;
                    seller.join_date = DateTime.UtcNow;
                    seller.city_id = seller.city_id;
                    seller.phone_number = Request["phone_number"];
                    seller.seller_name = Request["phone_number"];
                    seller.address = Request["address"];
                    seller.ID_photo = pathpic;
                    seller.Gender = true;
                    seller.city_id = int.Parse(Request["city_id"]);
                    seller.Accept = false;
                    var userRole = db.AspNetUserRoles.SingleOrDefault(x => x.UserId == userId);
                    if (userRole != null)
                    {
                        db.AspNetUserRoles.Remove(userRole);
                        db.SaveChanges();
                        db.AspNetUserRoles.Add(new AspNetUserRole { UserId = userId, RoleId = "2" });
                        db.SaveChanges();
                    }
                } 
              
                db.sellers.Add(seller);
                db.SaveChanges();
             
                return RedirectToAction( "Index" , "Home" );
            }
          

            ViewBag.AspUser = new SelectList(db.AspNetUsers, "Id", "Email", seller.AspUser);
            ViewBag.city_id = new SelectList(db.cities, "city_id", "City_Name", seller.city_id);
            return View();
        }


        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View(model);
            //}

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    if (Session["join"]!= null)
                    {
                        return RedirectToAction("Registration", "Account");

                    }
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl,RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            SilvanaEntities db = new SilvanaEntities(); 
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    string email = model.Email;
                    var userId = db.AspNetUsers.Where(m => m.Email == email).Select(m => m.Id).SingleOrDefault();
                    UserManager.AddToRole(userId, "User");
                    
                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    
                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link

                     string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                     var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id , code=code}, protocol: Request.Url.Scheme);
                     await UserManager.SendEmailAsync(user.Id, "Confirm your account", " < !DOCTYPE html >\r\n < html lang =\"en\" >\r\n<head>\r\n  <meta charset=\"UTF-8\">\r\n  <title>Silvana</title>\r\n  \r\n\r\n</head>\r\n<body>\r\n<!-- partial:index.partial.html -->\r\n<head>\r\n  <meta http-equiv=\"Content-Type\" content=\"text/html; charset=US-ASCII\">\r\n  <meta name=\"viewport\" content=\"width=device-width\">\r\n\r\n</head>\r\n\r\n<body style=\"-moz-box-sizing: border-box; -ms-text-size-adjust: 100%; -webkit-box-sizing: border-box; -webkit-text-size-adjust: 100%; box-sizing: border-box; color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 22px; margin: 0; min-width: 100%; padding: 0; text-align: left; width: 100% !important\">\r\n  <style type=\"text/css\">\r\n    body {\r\n      width: 100% !important;\r\n      min-width: 100%;\r\n      -webkit-text-size-adjust: 100%;\r\n      -ms-text-size-adjust: 100%;\r\n      margin: 0;\r\n      padding: 0;\r\n      -moz-box-sizing: border-box;\r\n      -webkit-box-sizing: border-box;\r\n      box-sizing: border-box;\r\n    }\r\n\r\n    .ExternalClass {\r\n      width: 100%;\r\n    }\r\n\r\n    .ExternalClass {\r\n      line-height: 100%;\r\n    }\r\n\r\n    #backgroundTable {\r\n      margin: 0;\r\n      padding: 0;\r\n      width: 100% !important;\r\n      line-height: 100% !important;\r\n    }\r\n\r\n    img {\r\n      outline: none;\r\n      text-decoration: none;\r\n      -ms-interpolation-mode: bicubic;\r\n      width: auto;\r\n      max-width: 100%;\r\n      clear: both;\r\n      display: block;\r\n    }\r\n\r\n    body {\r\n      color: #1C232B;\r\n      font-family: Helvetica, Arial, sans-serif;\r\n      font-weight: normal;\r\n      padding: 0;\r\n      margin: 0;\r\n      text-align: left;\r\n      line-height: 1.3;\r\n    }\r\n\r\n    body {\r\n      font-size: 16px;\r\n      line-height: 1.3;\r\n    }\r\n\r\n    a:hover {\r\n      color: #1f54ed;\r\n    }\r\n\r\n    a:active {\r\n      color: #1f54ed;\r\n    }\r\n\r\n    a:visited {\r\n      color: #4E78F1;\r\n    }\r\n\r\n    h1 a:visited {\r\n      color: #4E78F1;\r\n    }\r\n\r\n    h2 a:visited {\r\n      color: #4E78F1;\r\n    }\r\n\r\n    h3 a:visited {\r\n      color: #4E78F1;\r\n    }\r\n\r\n    h4 a:visited {\r\n      color: #4E78F1;\r\n    }\r\n\r\n    h5 a:visited {\r\n      color: #4E78F1;\r\n    }\r\n\r\n    h6 a:visited {\r\n      color: #4E78F1;\r\n    }\r\n\r\n    table.button:hover table tr td a {\r\n      color: #FFFFFF;\r\n    }\r\n\r\n    table.button:active table tr td a {\r\n      color: #FFFFFF;\r\n    }\r\n\r\n    table.button table tr td a:visited {\r\n      color: #FFFFFF;\r\n    }\r\n\r\n    table.button.tiny:hover table tr td a {\r\n      color: #FFFFFF;\r\n    }\r\n\r\n    table.button.tiny:active table tr td a {\r\n      color: #FFFFFF;\r\n    }\r\n\r\n    table.button.tiny table tr td a:visited {\r\n      color: #FFFFFF;\r\n    }\r\n\r\n    table.button.small:hover table tr td a {\r\n      color: #FFFFFF;\r\n    }\r\n\r\n    table.button.small:active table tr td a {\r\n      color: #FFFFFF;\r\n    }\r\n\r\n    table.button.small table tr td a:visited {\r\n      color: #FFFFFF;\r\n    }\r\n\r\n    table.button.large:hover table tr td a {\r\n      color: #FFFFFF;\r\n    }\r\n\r\n    table.button.large:active table tr td a {\r\n      color: #FFFFFF;\r\n    }\r\n\r\n    table.button.large table tr td a:visited {\r\n      color: #FFFFFF;\r\n    }\r\n\r\n    table.button:hover table td {\r\n      background: #1f54ed;\r\n      color: #FFFFFF;\r\n    }\r\n\r\n    table.button:visited table td {\r\n      background: #1f54ed;\r\n      color: #FFFFFF;\r\n    }\r\n\r\n    table.button:active table td {\r\n      background: #1f54ed;\r\n      color: #FFFFFF;\r\n    }\r\n\r\n    table.button:hover table a {\r\n      border: 0 solid #1f54ed;\r\n    }\r\n\r\n    table.button:visited table a {\r\n      border: 0 solid #1f54ed;\r\n    }\r\n\r\n    table.button:active table a {\r\n      border: 0 solid #1f54ed;\r\n    }\r\n\r\n    table.button.secondary:hover table td {\r\n      background: #fefefe;\r\n      color: #FFFFFF;\r\n    }\r\n\r\n    table.button.secondary:hover table a {\r\n      border: 0 solid #fefefe;\r\n    }\r\n\r\n    table.button.secondary:hover table td a {\r\n      color: #FFFFFF;\r\n    }\r\n\r\n    table.button.secondary:active table td a {\r\n      color: #FFFFFF;\r\n    }\r\n\r\n    table.button.secondary table td a:visited {\r\n      color: #FFFFFF;\r\n    }\r\n\r\n    table.button.success:hover table td {\r\n      background: #009482;\r\n    }\r\n\r\n    table.button.success:hover table a {\r\n      border: 0 solid #009482;\r\n    }\r\n\r\n    table.button.alert:hover table td {\r\n      background: #ff6131;\r\n    }\r\n\r\n    table.button.alert:hover table a {\r\n      border: 0 solid #ff6131;\r\n    }\r\n\r\n    table.button.warning:hover table td {\r\n      background: #fcae1a;\r\n    }\r\n\r\n    table.button.warning:hover table a {\r\n      border: 0px solid #fcae1a;\r\n    }\r\n\r\n    .thumbnail:hover {\r\n      box-shadow: 0 0 6px 1px rgba(78, 120, 241, 0.5);\r\n    }\r\n\r\n    .thumbnail:focus {\r\n      box-shadow: 0 0 6px 1px rgba(78, 120, 241, 0.5);\r\n    }\r\n\r\n    body.outlook p {\r\n      display: inline !important;\r\n    }\r\n\r\n    body {\r\n      font-weight: normal;\r\n      font-size: 16px;\r\n      line-height: 22px;\r\n    }\r\n\r\n    @media only screen and (max-width: 596px) {\r\n      .small-float-center {\r\n        margin: 0 auto !important;\r\n        float: none !important;\r\n        text-align: center !important;\r\n      }\r\n      .small-text-center {\r\n        text-align: center !important;\r\n      }\r\n      .small-text-left {\r\n        text-align: left !important;\r\n      }\r\n      .small-text-right {\r\n        text-align: right !important;\r\n      }\r\n      .hide-for-large {\r\n        display: block !important;\r\n        width: auto !important;\r\n        overflow: visible !important;\r\n        max-height: none !important;\r\n        font-size: inherit !important;\r\n        line-height: inherit !important;\r\n      }\r\n      table.body table.container .hide-for-large {\r\n        display: table !important;\r\n        width: 100% !important;\r\n      }\r\n      table.body table.container .row.hide-for-large {\r\n        display: table !important;\r\n        width: 100% !important;\r\n      }\r\n      table.body table.container .callout-inner.hide-for-large {\r\n        display: table-cell !important;\r\n        width: 100% !important;\r\n      }\r\n      table.body table.container .show-for-large {\r\n        display: none !important;\r\n        width: 0;\r\n        mso-hide: all;\r\n        overflow: hidden;\r\n      }\r\n      table.body img {\r\n        width: auto;\r\n        height: auto;\r\n      }\r\n      table.body center {\r\n        min-width: 0 !important;\r\n      }\r\n      table.body .container {\r\n        width: 95% !important;\r\n      }\r\n      table.body .columns {\r\n        height: auto !important;\r\n        -moz-box-sizing: border-box;\r\n        -webkit-box-sizing: border-box;\r\n        box-sizing: border-box;\r\n        padding-left: 16px !important;\r\n        padding-right: 16px !important;\r\n      }\r\n      table.body .column {\r\n        height: auto !important;\r\n        -moz-box-sizing: border-box;\r\n        -webkit-box-sizing: border-box;\r\n        box-sizing: border-box;\r\n        padding-left: 16px !important;\r\n        padding-right: 16px !important;\r\n      }\r\n      table.body .columns .column {\r\n        padding-left: 0 !important;\r\n        padding-right: 0 !important;\r\n      }\r\n      table.body .columns .columns {\r\n        padding-left: 0 !important;\r\n        padding-right: 0 !important;\r\n      }\r\n      table.body .column .column {\r\n        padding-left: 0 !important;\r\n        padding-right: 0 !important;\r\n      }\r\n      table.body .column .columns {\r\n        padding-left: 0 !important;\r\n        padding-right: 0 !important;\r\n      }\r\n      table.body .collapse .columns {\r\n        padding-left: 0 !important;\r\n        padding-right: 0 !important;\r\n      }\r\n      table.body .collapse .column {\r\n        padding-left: 0 !important;\r\n        padding-right: 0 !important;\r\n      }\r\n      td.small-1 {\r\n        display: inline-block !important;\r\n        width: 8.333333% !important;\r\n      }\r\n      th.small-1 {\r\n        display: inline-block !important;\r\n        width: 8.333333% !important;\r\n      }\r\n      td.small-2 {\r\n        display: inline-block !important;\r\n        width: 16.666666% !important;\r\n      }\r\n      th.small-2 {\r\n        display: inline-block !important;\r\n        width: 16.666666% !important;\r\n      }\r\n      td.small-3 {\r\n        display: inline-block !important;\r\n        width: 25% !important;\r\n      }\r\n      th.small-3 {\r\n        display: inline-block !important;\r\n        width: 25% !important;\r\n      }\r\n      td.small-4 {\r\n        display: inline-block !important;\r\n        width: 33.333333% !important;\r\n      }\r\n      th.small-4 {\r\n        display: inline-block !important;\r\n        width: 33.333333% !important;\r\n      }\r\n      td.small-5 {\r\n        display: inline-block !important;\r\n        width: 41.666666% !important;\r\n      }\r\n      th.small-5 {\r\n        display: inline-block !important;\r\n        width: 41.666666% !important;\r\n      }\r\n      td.small-6 {\r\n        display: inline-block !important;\r\n        width: 50% !important;\r\n      }\r\n      th.small-6 {\r\n        display: inline-block !important;\r\n        width: 50% !important;\r\n      }\r\n      td.small-7 {\r\n        display: inline-block !important;\r\n        width: 58.333333% !important;\r\n      }\r\n      th.small-7 {\r\n        display: inline-block !important;\r\n        width: 58.333333% !important;\r\n      }\r\n      td.small-8 {\r\n        display: inline-block !important;\r\n        width: 66.666666% !important;\r\n      }\r\n      th.small-8 {\r\n        display: inline-block !important;\r\n        width: 66.666666% !important;\r\n      }\r\n      td.small-9 {\r\n        display: inline-block !important;\r\n        width: 75% !important;\r\n      }\r\n      th.small-9 {\r\n        display: inline-block !important;\r\n        width: 75% !important;\r\n      }\r\n      td.small-10 {\r\n        display: inline-block !important;\r\n        width: 83.333333% !important;\r\n      }\r\n      th.small-10 {\r\n        display: inline-block !important;\r\n        width: 83.333333% !important;\r\n      }\r\n      td.small-11 {\r\n        display: inline-block !important;\r\n        width: 91.666666% !important;\r\n      }\r\n      th.small-11 {\r\n        display: inline-block !important;\r\n        width: 91.666666% !important;\r\n      }\r\n      td.small-12 {\r\n        display: inline-block !important;\r\n        width: 100% !important;\r\n      }\r\n      th.small-12 {\r\n        display: inline-block !important;\r\n        width: 100% !important;\r\n      }\r\n      .columns td.small-12 {\r\n        display: block !important;\r\n        width: 100% !important;\r\n      }\r\n      .column td.small-12 {\r\n        display: block !important;\r\n        width: 100% !important;\r\n      }\r\n      .columns th.small-12 {\r\n        display: block !important;\r\n        width: 100% !important;\r\n      }\r\n      .column th.small-12 {\r\n        display: block !important;\r\n        width: 100% !important;\r\n      }\r\n      table.body td.small-offset-1 {\r\n        margin-left: 8.333333% !important;\r\n      }\r\n      table.body th.small-offset-1 {\r\n        margin-left: 8.333333% !important;\r\n      }\r\n      table.body td.small-offset-2 {\r\n        margin-left: 16.666666% !important;\r\n      }\r\n      table.body th.small-offset-2 {\r\n        margin-left: 16.666666% !important;\r\n      }\r\n      table.body td.small-offset-3 {\r\n        margin-left: 25% !important;\r\n      }\r\n      table.body th.small-offset-3 {\r\n        margin-left: 25% !important;\r\n      }\r\n      table.body td.small-offset-4 {\r\n        margin-left: 33.333333% !important;\r\n      }\r\n      table.body th.small-offset-4 {\r\n        margin-left: 33.333333% !important;\r\n      }\r\n      table.body td.small-offset-5 {\r\n        margin-left: 41.666666% !important;\r\n      }\r\n      table.body th.small-offset-5 {\r\n        margin-left: 41.666666% !important;\r\n      }\r\n      table.body td.small-offset-6 {\r\n        margin-left: 50% !important;\r\n      }\r\n      table.body th.small-offset-6 {\r\n        margin-left: 50% !important;\r\n      }\r\n      table.body td.small-offset-7 {\r\n        margin-left: 58.333333% !important;\r\n      }\r\n      table.body th.small-offset-7 {\r\n        margin-left: 58.333333% !important;\r\n      }\r\n      table.body td.small-offset-8 {\r\n        margin-left: 66.666666% !important;\r\n      }\r\n      table.body th.small-offset-8 {\r\n        margin-left: 66.666666% !important;\r\n      }\r\n      table.body td.small-offset-9 {\r\n        margin-left: 75% !important;\r\n      }\r\n      table.body th.small-offset-9 {\r\n        margin-left: 75% !important;\r\n      }\r\n      table.body td.small-offset-10 {\r\n        margin-left: 83.333333% !important;\r\n      }\r\n      table.body th.small-offset-10 {\r\n        margin-left: 83.333333% !important;\r\n      }\r\n      table.body td.small-offset-11 {\r\n        margin-left: 91.666666% !important;\r\n      }\r\n      table.body th.small-offset-11 {\r\n        margin-left: 91.666666% !important;\r\n      }\r\n      table.body table.columns td.expander {\r\n        display: none !important;\r\n      }\r\n      table.body table.columns th.expander {\r\n        display: none !important;\r\n      }\r\n      table.body .right-text-pad {\r\n        padding-left: 10px !important;\r\n      }\r\n      table.body .text-pad-right {\r\n        padding-left: 10px !important;\r\n      }\r\n      table.body .left-text-pad {\r\n        padding-right: 10px !important;\r\n      }\r\n      table.body .text-pad-left {\r\n        padding-right: 10px !important;\r\n      }\r\n      table.menu {\r\n        width: 100% !important;\r\n      }\r\n      table.menu td {\r\n        width: auto !important;\r\n        display: inline-block !important;\r\n      }\r\n      table.menu th {\r\n        width: auto !important;\r\n        display: inline-block !important;\r\n      }\r\n      table.menu.vertical td {\r\n        display: block !important;\r\n      }\r\n      table.menu.vertical th {\r\n        display: block !important;\r\n      }\r\n      table.menu.small-vertical td {\r\n        display: block !important;\r\n      }\r\n      table.menu.small-vertical th {\r\n        display: block !important;\r\n      }\r\n      table.menu[align=\"center\"] {\r\n        width: auto !important;\r\n      }\r\n      table.button.small-expand {\r\n        width: 100% !important;\r\n      }\r\n      table.button.small-expanded {\r\n        width: 100% !important;\r\n      }\r\n      table.button.small-expand table {\r\n        width: 100%;\r\n      }\r\n      table.button.small-expanded table {\r\n        width: 100%;\r\n      }\r\n      table.button.small-expand table a {\r\n        text-align: center !important;\r\n        width: 100% !important;\r\n        padding-left: 0 !important;\r\n        padding-right: 0 !important;\r\n      }\r\n      table.button.small-expanded table a {\r\n        text-align: center !important;\r\n        width: 100% !important;\r\n        padding-left: 0 !important;\r\n        padding-right: 0 !important;\r\n      }\r\n      table.button.small-expand center {\r\n        min-width: 0;\r\n      }\r\n      table.button.small-expanded center {\r\n        min-width: 0;\r\n      }\r\n      table.body .container {\r\n        width: 100% !important;\r\n      }\r\n    }\r\n\r\n    @media only screen and (min-width: 732px) {\r\n      table.body table.milkyway-email-card {\r\n        width: 525px !important;\r\n      }\r\n      table.body table.emailer-footer {\r\n        width: 525px !important;\r\n      }\r\n    }\r\n\r\n    @media only screen and (max-width: 731px) {\r\n      table.body table.milkyway-email-card {\r\n        width: 320px !important;\r\n      }\r\n      table.body table.emailer-footer {\r\n        width: 320px !important;\r\n      }\r\n    }\r\n\r\n    @media only screen and (max-width: 320px) {\r\n      table.body table.milkyway-email-card {\r\n        width: 100% !important;\r\n        border-radius: 0;\r\n        box-sizing: none;\r\n      }\r\n      table.body table.emailer-footer {\r\n        width: 100% !important;\r\n        border-radius: 0;\r\n        box-sizing: none;\r\n      }\r\n    }\r\n\r\n    @media only screen and (max-width: 280px) {\r\n      table.body table.milkyway-email-card .milkyway-content {\r\n        width: 100% !important;\r\n      }\r\n    }\r\n\r\n    @media (min-width: 596px) {\r\n      .milkyway-header {\r\n        width: 11%;\r\n      }\r\n    }\r\n\r\n    @media (max-width: 596px) {\r\n      .milkyway-header {\r\n        width: 50%;\r\n      }\r\n      .emailer-footer .emailer-border-bottom {\r\n        border-bottom: 0.5px solid #E2E5E7;\r\n      }\r\n      .emailer-footer .make-you-smile {\r\n        margin-top: 24px;\r\n      }\r\n      .emailer-footer .make-you-smile .email-tag-line {\r\n        width: 80%;\r\n        position: relative;\r\n        left: 10%;\r\n      }\r\n      .emailer-footer .make-you-smile .universe-address {\r\n        margin-bottom: 10px !important;\r\n      }\r\n      .emailer-footer .make-you-smile .email-tag-line {\r\n        margin-bottom: 10px !important;\r\n      }\r\n      .have-questions-text {\r\n        width: 70%;\r\n      }\r\n      .hide-on-small {\r\n        display: none;\r\n      }\r\n      .product-card-stacked-row .thumbnail-image {\r\n        max-width: 32% !important;\r\n      }\r\n      .product-card-stacked-row .thumbnail-content p {\r\n        width: 64%;\r\n      }\r\n      .welcome-subcontent {\r\n        text-align: left;\r\n        margin: 20px 0 10px;\r\n      }\r\n      .milkyway-title {\r\n        padding: 16px;\r\n      }\r\n      .meta-data {\r\n        text-align: center;\r\n      }\r\n      .label {\r\n        text-align: center;\r\n      }\r\n      .welcome-email .wavey-background-subcontent {\r\n        width: calc(100% - 32px);\r\n      }\r\n    }\r\n\r\n    @media (min-width: 597px) {\r\n      .emailer-footer .show-on-mobile {\r\n        display: none;\r\n      }\r\n      .emailer-footer .emailer-border-bottom {\r\n        border-bottom: none;\r\n      }\r\n      .have-questions-text {\r\n        border-bottom: none;\r\n      }\r\n      .hide-on-large {\r\n        display: none;\r\n      }\r\n      .milkyway-title {\r\n        padding: 55px 55px 16px;\r\n      }\r\n    }\r\n\r\n    @media only screen and (max-width: 290px) {\r\n      table.container.your-tickets .tickets-container {\r\n        width: 100%;\r\n      }\r\n    }\r\n  </style>\r\n  <table class=\"body\" data-made-with-foundation=\"\" style=\"background: #FAFAFA; border-collapse: collapse; border-spacing: 0; color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; height: 100%; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; width: 100%\"\r\n    bgcolor=\"#FAFAFA\">\r\n    <tbody>\r\n      <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n        <td class=\"center\" align=\"center\" valign=\"top\" style=\"-moz-hyphens: auto; -webkit-hyphens: auto; border-collapse: collapse !important; color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word\">\r\n          <center style=\"min-width: 580px; width: 100%\">\r\n            <table class=\" spacer  float-center\" align=\"center\" style=\"border-collapse: collapse; border-spacing: 0; float: none; margin: 0 auto; padding: 0; text-align: center; vertical-align: top; width: 100%\">\r\n              <tbody>\r\n                <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                  <td height=\"20px\" style=\"-moz-hyphens: auto; -webkit-hyphens: auto; border-collapse: collapse !important; color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 20px; font-weight: normal; hyphens: auto; line-height: 20px; margin: 0; mso-line-height-rule: exactly; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word\"\r\n                    align=\"left\" valign=\"top\">&nbsp;</td>\r\n                </tr>\r\n              </tbody>\r\n            </table>\r\n            <table class=\"header-spacer spacer  float-center\" align=\"center\" style=\"border-collapse: collapse; border-spacing: 0; float: none; line-height: 60px; margin: 0 auto; padding: 0; text-align: center; vertical-align: top; width: 100%\">\r\n              <tbody>\r\n                <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                  <td height=\"16px\" style=\"-moz-hyphens: auto; -webkit-hyphens: auto; border-collapse: collapse !important; color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 16px; margin: 0; mso-line-height-rule: exactly; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word\"\r\n                    align=\"left\" valign=\"top\">&nbsp;</td>\r\n                </tr>\r\n              </tbody>\r\n            </table>\r\n            <BR><BR><BR>\r\n            <div class=\"milkyway-header float-center\" align=\"center\">\r\n              \r\n              \r\n            </div>\r\n            <table class=\"header-spacer-bottom spacer  float-center\" align=\"center\" style=\"border-collapse: collapse; border-spacing: 0; float: none; line-height: 30px; margin: 0 auto; padding: 0; text-align: center; vertical-align: top; width: 100%\">\r\n              <tbody>\r\n                <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                  <td height=\"16px\" style=\"-moz-hyphens: auto; -webkit-hyphens: auto; border-collapse: collapse !important; color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 16px; margin: 0; mso-line-height-rule: exactly; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word\"\r\n                    align=\"left\" valign=\"top\">&nbsp;</td>\r\n                </tr>\r\n              </tbody>\r\n            </table>\r\n\r\n            <table class=\"milkyway-email-card container float-center\" align=\"center\" style=\"background: #FFFFFF; border-collapse: collapse; border-radius: 6px; border-spacing: 0; box-shadow: 0 1px 8px 0 rgba(28,35,43,0.15); float: none; margin: 0 auto; overflow: hidden; padding: 0; text-align: center; vertical-align: top; width: 580px\"\r\n              bgcolor=\"#FFFFFF\">\r\n              <tbody>\r\n                <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                  <td style=\"-moz-hyphens: auto; -webkit-hyphens: auto; border-collapse: collapse !important; color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word\"\r\n                    align=\"left\" valign=\"top\">\r\n\r\n                    <table class=\"milkyway-content confirmation-instructions container\" align=\"center\" style=\"background: #FFFFFF; border-collapse: collapse; border-spacing: 0; hyphens: none; margin: auto; max-width: 100%; padding: 0; text-align: inherit; vertical-align: top; width: 280px !important\"\r\n                      bgcolor=\"#FFFFFF\">\r\n                      <tbody>\r\n                        <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                          <td style=\"-moz-hyphens: auto; -webkit-hyphens: auto; border-collapse: collapse !important; color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word\"\r\n                            align=\"left\" valign=\"top\">\r\n                            <table class=\" spacer \" style=\"border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%\">\r\n                              <tbody>\r\n                                <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                                  <td height=\"30px\" style=\"-moz-hyphens: auto; -webkit-hyphens: auto; border-collapse: collapse !important; color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 30px; font-weight: normal; hyphens: auto; line-height: 30px; margin: 0; mso-line-height-rule: exactly; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word\"\r\n                                    align=\"left\" valign=\"top\">&nbsp;</td>\r\n                                </tr>\r\n                              </tbody>\r\n                            </table>\r\n                            <table class=\" row\" style=\"border-collapse: collapse; border-spacing: 0; display: table; padding: 0; position: relative; text-align: left; vertical-align: top; width: 100%\">\r\n                              <tbody>\r\n                                <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                                  <th class=\" small-12 large-12 columns first last\" style=\"color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0 auto; padding: 0; text-align: left; width: 564px\" align=\"left\">\r\n                                    <table style=\"border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%\">\r\n                                      <tbody>\r\n                                        <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                                          <th style=\"color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left\" align=\"left\">\r\n                                            <center style=\"min-width: 0; width: 100%\">\r\n                                              <img width=\"250\" src=\"https://img.freepik.com/free-vector/confirmed-concept-illustration_114360-5400.jpg?w=740&t=st=1678474685~exp=1678475285~hmac=baddaa07f0f060bb5517aa0741f48ed71fd32bb02704bfcc408d00a6a0f783be\" \r\n                                              align=\"center\" class=\" float-center float-center\" style=\"-ms-interpolation-mode: bicubic; clear: both; display: block; float: none; margin: 0 auto; max-width: 100%; outline: none; text-align: center; text-decoration: none; width: auto \" />\r\n                                            </center>\r\n                                          </th>\r\n                                          <th class=\"expander\" style=\"color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left; visibility: hidden; width: 0\" align=\"left\"></th>\r\n                                        </tr>\r\n                                      </tbody>\r\n                                    </table>\r\n                                  </th>\r\n                                </tr>\r\n                              </tbody>\r\n                            </table>\r\n                            <table class=\" spacer \" style=\"border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%\">\r\n                              <tbody>\r\n                                <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                                  <td height=\"30px\" style=\"-moz-hyphens: auto; -webkit-hyphens: auto; border-collapse: collapse !important; color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 30px; font-weight: normal; hyphens: auto; line-height: 30px; margin: 0; mso-line-height-rule: exactly; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word\"\r\n                                    align=\"left\" valign=\"top\">&nbsp;</td>\r\n                                </tr>\r\n                              </tbody>\r\n                            </table>\r\n                            <table class=\" row\" style=\"border-collapse: collapse; border-spacing: 0; display: table; padding: 0; position: relative; text-align: left; vertical-align: top; width: 100%\">\r\n                              <tbody>\r\n                                <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                                  <th class=\"header-padding small-12 large-12 columns first last\" style=\"color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0 auto; padding: 0; text-align: left; width: 564px\" align=\"left\">\r\n                                    <table style=\"border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%\">\r\n                                      <tbody>\r\n                                        <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                                          <th style=\"color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left\" align=\"left\">\r\n                                            <h1 class=\"welcome-header\" style=\"color: inherit; font-family: Helvetica, Arial, sans-serif; font-size: 24px; font-weight: 600; hyphens: none; line-height: 30px; margin: 0 0 24px; padding: 0; text-align: left; width: 100%; word-wrap: normal\" align=\"left\">\r\n                                               Welcome to the Silvana family ❤️\r\n                                            </h1>\r\n                                          </th>\r\n                                          <th class=\"expander\" style=\"color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left; visibility: hidden; width: 0\" align=\"left\"></th>\r\n                                        </tr>\r\n                                      </tbody>\r\n                                    </table>\r\n                                  </th>\r\n                                </tr>\r\n                              </tbody>\r\n                            </table>\r\n                            <table class=\" row\" style=\"border-collapse: collapse; border-spacing: 0; display: table; padding: 0; position: relative; text-align: left; vertical-align: top; width: 100%\">\r\n                              <tbody>\r\n                                <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                                  <th class=\"body-content small-12 large-12 columns first last\" style=\"color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0 auto; padding: 0; text-align: left; width: 564px\" align=\"left\">\r\n                                    <table style=\"border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%\">\r\n                                      <tbody>\r\n                                        <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                                          <th style=\"color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left\" align=\"left\">\r\n                                            <h2 class=\"welcome-subcontent\" style=\"color: #6F7881; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: 300; line-height: 22px; margin: 0; padding: 0; text-align: left; width: 100%; word-wrap: normal\" align=\"left\">\r\n                                              Congratulations 👏\r\n,\r\n                                            </h2>\r\n                                          </th>\r\n                                          <th class=\"expander\" style=\"color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left; visibility: hidden; width: 0\" align=\"left\"></th>\r\n                                        </tr>\r\n                                      </tbody>\r\n                                    </table>\r\n                                  </th>\r\n                                </tr>\r\n                              </tbody>\r\n                            </table>\r\n                            <table class=\" row\" style=\"border-collapse: collapse; border-spacing: 0; display: table; padding: 0; position: relative; text-align: left; vertical-align: top; width: 100%\">\r\n                              <tbody>\r\n                                <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                                  <th class=\"body-content-end small-12 large-12 columns first last\" style=\"color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0 auto; padding: 0; text-align: left; width: 564px\" align=\"left\">\r\n                                    <table style=\"border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%\">\r\n                                      <tbody>\r\n                                        <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                                          <th style=\"color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left\" align=\"left\">\r\n                                            <h2 class=\"welcome-subcontent\" style=\"color: #6F7881; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: 300; line-height: 22px; margin: 0; padding: 0; text-align: left; width: 100%; word-wrap: normal\" align=\"left\">\r\n                          We've finished setting up your Silvana account. Just confirm your email to get started!  \r\n                                            </h2>\r\n                                          </th>\r\n                                          <th class=\"expander\" style=\"color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left; visibility: hidden; width: 0\" align=\"left\"></th>\r\n                                        </tr>\r\n                                      </tbody>\r\n                                    </table>\r\n                                  </th>\r\n                                </tr>\r\n                              </tbody>\r\n                            </table>\r\n                            <table class=\" spacer \" style=\"border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%\">\r\n                              <tbody>\r\n                                <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                                  <td height=\"30px\" style=\"-moz-hyphens: auto; -webkit-hyphens: auto; border-collapse: collapse !important; color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 30px; font-weight: normal; hyphens: auto; line-height: 30px; margin: 0; mso-line-height-rule: exactly; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word\"\r\n                                    align=\"left\" valign=\"top\">&nbsp;</td>\r\n                                </tr>\r\n                              </tbody>\r\n                            </table>\r\n                            <table class=\"milkyway-content row\" style=\"border-collapse: collapse; border-spacing: 0; display: table; hyphens: none; margin: auto; max-width: 100%; padding: 0; position: relative; text-align: left; vertical-align: top; width: 280px !important\">\r\n                              <tbody>\r\n                                <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                                  <th class=\"milkyway-padding small-12 large-12 columns first last\" valign=\"middle\" style=\"color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0 auto; padding: 0; text-align: left; width: 564px\"\r\n                                    align=\"left\">\r\n                                    <table style=\"border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%\">\r\n                                      <tbody>\r\n                                        <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                                          <th style=\"color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left\" align=\"left\">\r\n                                            <table class=\"cta-text primary radius expanded button\" style=\"border-collapse: collapse; border-spacing: 0; font-size: 14px; font-weight: 400; line-height: 0; margin: 0 0 16px; padding: 0; text-align: left; vertical-align: top; width: 100% !important\">\r\n                                              <tbody>\r\n                                                <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                                                  <td style=\"-moz-hyphens: auto; -webkit-hyphens: auto; border-collapse: collapse !important; color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word\"\r\n                                                    align=\"left\" valign=\"top\">\r\n                                                    <table style=\"border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%\">\r\n                                                      <tbody>\r\n                                                        <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                                                          <td style=\"-moz-hyphens: auto; -webkit-hyphens: auto; background: #4E78F1; border: 2px none #4e78f1; border-collapse: collapse !important; border-radius: 6px; color: #FFFFFF; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word\"\r\n                                                            align=\"left\" bgcolor=\"#4E78F1\" valign=\"top\">\r\n                                                            <a href=" + callbackUrl + " style=\"border: 0 solid #4e78f1; border-radius: 6px; color: #FFFFFF; display: inline-block; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: bold; line-height: 1.3; margin: 0; padding: 13px 0; text-align: center; text-decoration: none; width: 100%\"\r\n target=\"_blank\">\r\n                                                              <p class=\"text-center\" style=\"color: white; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: 300; letter-spacing: 1px; line-height: 1.3; margin: 0; padding: 0; text-align: center\" align=\"center\">\r\n                                                                Confirm email                    </p> </a>\r\n                                                          </td>\r\n                                                        </tr>\r\n                                                      </tbody>\r\n                                                    </table>\r\n                                                  </td>\r\n                                                </tr>\r\n                                              </tbody>\r\n                                            </table>\r\n                                          </th>\r\n                                          <th class=\"expander\" style=\"color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left; visibility: hidden; width: 0\" align=\"left\"></th>\r\n                                        </tr>\r\n                                      </tbody>\r\n                                    </table>\r\n                                  </th>\r\n                                </tr>\r\n                              </tbody>\r\n                            </table>\r\n\r\n                            <table class=\" spacer \" style=\"border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%\">\r\n                              <tbody>\r\n                                <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                                  <td height=\"10px\" style=\"-moz-hyphens: auto; -webkit-hyphens: auto; border-collapse: collapse !important; color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 10px; font-weight: normal; hyphens: auto; line-height: 10px; margin: 0; mso-line-height-rule: exactly; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word\"\r\n                                    align=\"left\" valign=\"top\">&nbsp;</td>\r\n                                </tr>\r\n                              </tbody>\r\n                            </table>\r\n                          </td>\r\n                        </tr>\r\n                      </tbody>\r\n                    </table>\r\n\r\n                  </td>\r\n                </tr>\r\n              </tbody>\r\n            </table>\r\n            <table class=\" spacer  float-center\" align=\"center\" style=\"border-collapse: collapse; border-spacing: 0; float: none; margin: 0 auto; padding: 0; text-align: center; vertical-align: top; width: 100%\">\r\n              <tbody>\r\n                <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                  <td height=\"20px\" style=\"-moz-hyphens: auto; -webkit-hyphens: auto; border-collapse: collapse !important; color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 20px; font-weight: normal; hyphens: auto; line-height: 20px; margin: 0; mso-line-height-rule: exactly; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word\"\r\n                    align=\"left\" valign=\"top\">&nbsp;</td>\r\n                </tr>\r\n              </tbody>\r\n            </table>\r\n            <table class=\"emailer-footer container float-center\" align=\"center\" style=\"background-color: transparent !important; border-collapse: collapse; border-spacing: 0; float: none; margin: 0 auto; padding: 0; text-align: center; vertical-align: top; width: 580px\"\r\n              bgcolor=\"transparent\">\r\n              <tbody>\r\n                <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                  <td style=\"-moz-hyphens: auto; -webkit-hyphens: auto; border-collapse: collapse !important; color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word\"\r\n                    align=\"left\" valign=\"top\">\r\n                    <table class=\" row\" style=\"border-collapse: collapse; border-spacing: 0; display: table; padding: 0; position: relative; text-align: left; vertical-align: top; width: 100%\">\r\n                      <tbody>\r\n                        <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                          <th class=\" small-12 large-4 columns first\" style=\"color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0 auto; padding: 0 8px 16px 16px; text-align: left; width: 177.3333333333px\"\r\n                            align=\"left\">\r\n                            <table style=\"border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%\">\r\n                              <tbody>\r\n                                <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                                  <th style=\"color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left\" align=\"left\">\r\n                                  </th>\r\n                                  <th class=\"emailer-border-bottom small-12 large-11 columns first\" style=\"color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0 auto; padding: 0 0 16px; text-align: left; width: 91.666666%\"\r\n                                    align=\"left\">\r\n                                    <table style=\"border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%\">\r\n                               \r\n                                    </table>\r\n                            \r\n                                </tr>\r\n                              </tbody>\r\n                            </table>\r\n                          </th>\r\n                     \r\n                        </tr>\r\n                      </tbody>\r\n                    </table>\r\n                    <table class=\"make-you-smile row\" style=\"border-collapse: collapse; border-spacing: 0; display: table; padding: 0; position: relative; text-align: left; vertical-align: top; width: 100%\">\r\n                      <tbody>\r\n                        <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                          <th class=\" small-12 large-12 columns first last\" style=\"color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0 auto; padding: 0 16px 16px; text-align: left; width: 564px\" align=\"left\">\r\n                            <table style=\"border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%\">\r\n                              <tbody>\r\n                                <tr style=\"padding: 0; text-align: left; vertical-align: top\" align=\"left\">\r\n                                  <th style=\"color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left\" align=\"left\">\r\n                                    <p class=\"text-center email-tag-line\" style=\"color: #6F7881; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.5; margin: 0; padding: 0; text-align: center\" align=\"center\">\r\n                                     Silvana ❤️ \r\n                                    </p>\r\n                                  \r\n\r\n\r\n                                    </p>\r\n                                  </th>\r\n                                  <th class=\"expander\" style=\"color: #1C232B; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left; visibility: hidden; width: 0\" align=\"left\"></th>\r\n                                </tr>\r\n                              </tbody>\r\n                            </table>\r\n                          </th>\r\n                        </tr>\r\n                      </tbody>\r\n                    </table>\r\n                  </td>\r\n                </tr>\r\n              </tbody>\r\n            </table>\r\n\r\n          </center>\r\n        </td>\r\n      </tr>\r\n    </tbody>\r\n  </table>\r\n\r\n</body>\r\n<!-- partial -->\r\n  \r\n</body>\r\n</html>\r\n");
                    return RedirectToAction("index" , "Home");
                }

                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return RedirectToAction("login");
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }
       
        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }


        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}