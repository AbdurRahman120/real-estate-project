using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using Assignment.Models;
using System.Web.Security;
using System.Data.Entity.Validation;
using System.Diagnostics;
using Assignment.Services;
using System.Security.Cryptography.X509Certificates;
using System.Data.Entity;
using System.Web.Helpers;
namespace Assignment.Controllers
{
    public class HomeController : Controller
    {
        realEstateEntities db = new realEstateEntities();
        public ActionResult Index()
        {
            List<property> properties = db.properties.ToList();
            return View(properties);
        }
        public ActionResult About()
        {
            return View();
        }
        public ActionResult ContactUs()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ContactUs(contact contact)
        {
            if (ModelState.IsValid)
            {
                db.contacts.Add(contact);
                db.SaveChanges();

                return RedirectToAction("contact");
            }
            return View(contact);
        }
        public ActionResult AgentGrid()
        {
            return View();
        }
        public ActionResult PropertyGrid(int? page)
        {
            if (Session["Useremail"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            List<property> properties = db.properties.ToList();
            return View(properties);
        }
        public ActionResult Admin(string username, string password)
        {
            return View();
        }
        public ActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignUp(user user)
        {
            if (ModelState.IsValid)
            {
                // Hash the password before saving it
                user.password = Crypto.HashPassword(user.password);

                db.users.Add(user);
                db.SaveChanges();

                return RedirectToAction("login");
            }
            return View(user);


        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            if (ModelState.IsValid)
            {
                var user = db.users.FirstOrDefault(x => x.email == email);

                if (user != null && Crypto.VerifyHashedPassword(user.password, password))
                {
                    Session["Useremail"] = email;
                    return RedirectToAction("PropertyGrid", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt. Please check your email and password.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Email and password are required.");
            }

            return View();
        }
        public ActionResult RegisteredCustomer()
        {
            if (Session["AdminUsername"] == null)
            {
                return RedirectToAction("AdminLogin", "Home");
            }
            var user = db.users.ToList();
            return View(user);
        }
        public ActionResult AdminLogin()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdminLogin(admin admins)
        {
            if (ModelState.IsValid)
            {
                if (IsValidAdmin(admins.admin_email, admins.admin_password))
                {
                    FormsAuthentication.SetAuthCookie(admins.admin_email, false);
                    return RedirectToAction("Dashboard", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt. Please check your username and password.");
                }
            }

            return View(admins);
        }
        private bool IsValidAdmin(string admin_email, string admin_password)
        {

            if (admin_email == "admin@gmail.com" && admin_password == "admin123@456")
            {
                Session["AdminUsername"] = admin_email;

                return true;
            }
            return false;
        }
        public ActionResult AdminSignUp()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdminSignUp(admin admin)
        {
            if (ModelState.IsValid)
            {
                db.admins.Add(admin);
                db.SaveChanges();

                return RedirectToAction("login");
            }
            return View(admin);
        }
        public ActionResult CreateProperty()
        {
            if (Session["AdminUsername"] == null)
            {
                return RedirectToAction("AdminLogin", "Home");
            }

            var propertyTypeOptions = new List<SelectListItem>
        {
            new SelectListItem { Value = "Rent", Text = "Rent" },
            new SelectListItem { Value = "Buy", Text = "Buy" }
        };

            ViewBag.PropertyTypeList = new SelectList(propertyTypeOptions, "Value", "Text");

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProperty(property property, HttpPostedFileBase file)
        {
            if (Session["AdminUsername"] == null)
            {
                return RedirectToAction("AdminLogin", "Home");
            }
            var propertyTypeOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Rent", Text = "Rent" },
                new SelectListItem { Value = "Buy", Text = "Buy" }
            };

            ViewBag.PropertyTypeList = new SelectList(propertyTypeOptions, "Value", "Text", property.property_type);

            if (ModelState.IsValid && file != null && file.ContentLength > 0)
            {
                string fileExtension = Path.GetExtension(file.FileName).ToLower();
                string fileName = Guid.NewGuid().ToString() + fileExtension;
                string filePath = Path.Combine(Server.MapPath("~/Uploads"), fileName);
                file.SaveAs(filePath);

                property.property_type = property.property_type;
                SaveProductToDatabase(property, fileName);

                return RedirectToAction("ActiveProperties", "Home");
            }
            else
            {
                ViewBag.Error = "Invalid file or model state!";
            }
            return View();
        }

        private void SaveProductToDatabase(property property, string fileName)
        {
            db.properties.Add(new Models.property
            {
                property_name = property.property_name,
                property_price = property.property_price,
                area = property.area,
                beds = property.beds,
                bath = property.bath,
                garages = property.garages,
                property_image = fileName,
                city = property.city,
                property_type = property.property_type,
                description = property.description,
                location = property.location,
            });
            db.SaveChanges();
        }
        public ActionResult PropertyDetail(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Index");
            }

            property property = db.properties.Find(id.Value);

            if (property == null)
            {
                return RedirectToAction("Index");
            }
            return View(property);
        }
        [HttpPost]
        public ActionResult AddToCart(int id)
        {
            var property = db.properties.FirstOrDefault(p => p.id == id);

            if (property != null)
            {
                CartService.AddToCart(property);
            }

            return RedirectToAction("Cart");
        }

        public ActionResult Cart()
        {
            var cartItems = CartService.GetCartItems();

            return View(cartItems);
        }
        public ActionResult ConfirmOrder()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ConfirmOrder(string customerName, string customerEmail)
        {
            var cartItems = CartService.GetCartItems();

            foreach (var cartItem in cartItems)
            {
                var order = new Order
                {
                    PropertyId = cartItem.PropertyId,
                    CustomerName = customerName,
                    CustomerEmail = customerEmail,
                    propertyName = cartItem.PropertyName
                };

                db.Orders.Add(order);
            }

            CartService.ClearCart();
            db.SaveChanges();
            return RedirectToAction("ThankYou");
        }

        public ActionResult ThankYou()
        {
            return View();
        }

        public ActionResult Dashboard()
        {
            if (Session["AdminUsername"] == null)
            {
                return RedirectToAction("AdminLogin", "Home");
            }
            int totalProperties = db.properties.Count();
            ViewBag.TotalProperties = totalProperties;


            int totalCustomer = db.users.Count();
            ViewBag.TotalCustomer = totalCustomer;

            int totalOrders = db.Orders.Count();
            ViewBag.TotalOrders = totalOrders;

            return View();
        }
        public ActionResult GoogleMap()
        {
            return View();
        }
        public ActionResult Data()
        {
            return View();
        }
        public new ActionResult Profile()
        {
            return View();
        }
        public ActionResult ActiveProperties()
        {
            if (Session["AdminUsername"] == null)
            {
                return RedirectToAction("AdminLogin", "Home");
            }

            var property = db.properties.ToList();
            return View(property);
        }
        public ActionResult Edit(int? id)
        {
            if (id == null || db.properties == null)
            {
                return HttpNotFound();
            }
            var em = db.properties.Find(id);
            if (em == null)
            {
                return HttpNotFound();
            }
            return View(em);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(property properties, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                if (file != null && file.ContentLength > 0)
                {
                    string fileExtension = Path.GetExtension(file.FileName).ToLower();
                    string fileName = Guid.NewGuid().ToString() + fileExtension;
                    string filePath = Path.Combine(Server.MapPath("~/Uploads"), fileName);
                    file.SaveAs(filePath);
                    properties.property_image = fileName;
                }

                db.Entry(properties).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("ActiveProperties", "Home");
            }

            var propertyTypeOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Rent", Text = "Rent" },
                new SelectListItem { Value = "Buy", Text = "Buy" }
            };
            ViewBag.property_type = new SelectList(propertyTypeOptions, "Value", "Text");
            return View(properties);
        }
        public ActionResult Delete(int id)
            {
                var propertyToDelete = db.properties.Find(id);

                if (propertyToDelete == null)
                {
                    return HttpNotFound();
                }

                var associatedOrders = db.Orders.Where(o => o.PropertyId == id).ToList();
                db.Orders.RemoveRange(associatedOrders);
                db.properties.Remove(propertyToDelete);
                db.SaveChanges();

                return RedirectToAction("ActiveProperties", "Home");
            }
        
        public ActionResult Logout()
        {
            Session.Clear();

            return RedirectToAction("AdminLogin", "Home");
        }

        public ActionResult ShowOrder()
        {
            if (Session["AdminUsername"] == null)
            {
                return RedirectToAction("AdminLogin", "Home");
            }
            var order = db.Orders.ToList();
            return View(order);

        }
        public ActionResult DeleteOrder(int id)
        {
            var itemToDelete = db.Orders.Find(id);

            if (itemToDelete == null)
            {
                return HttpNotFound();
            }

            db.Orders.Remove(itemToDelete);
            db.SaveChanges();

            return RedirectToAction("ShowOrder", "Home");
        }
        public ActionResult UserLogout()
        {
            Session.Clear();

            return RedirectToAction("Login", "Home");
        }
        public ActionResult DeleteUser(int id)
        {
            var user = db.users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            db.users.Remove(user);
            db.SaveChanges();

            return RedirectToAction("RegisteredCustomer","Home");
        }
        public ActionResult EditUser(int id)
        {
            var user = db.users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(user updatedUser)
        {
            if (ModelState.IsValid)
            {
                var existingUser = db.users.Find(updatedUser.id);
                if (existingUser == null)
                {
                    return HttpNotFound();
                }
                existingUser.name = updatedUser.name;
                existingUser.email = updatedUser.email;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(updatedUser);
        }

    }
    internal class CheckoutViewModel : IView
    {
        public List<CartItem> CartItems { get; set; }
        public decimal TotalAmount { get; set; }

        void IView.Render(ViewContext viewContext, TextWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}

