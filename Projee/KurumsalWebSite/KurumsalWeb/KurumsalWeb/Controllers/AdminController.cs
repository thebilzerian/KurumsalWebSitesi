using KurumsalWeb.Models;
using KurumsalWeb.Models.DataContext;
using KurumsalWeb.Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace KurumsalWeb.Controllers
{
    public class AdminController : Controller
    {
        KurumsalDBContext db = new KurumsalDBContext();
        // GET: Admin
        [Route("yonetimpaneli")]
        public ActionResult Index()
        {
            ViewBag.BlogSay = db.Blog.Count();
            ViewBag.KategoriSay = db.Kategori.Count();
            ViewBag.HizmetSay = db.Hizmet.Count();
            ViewBag.YorumSay = db.Yorum.Count();
            ViewBag.YorumOnay = db.Yorum.Where(x => x.Onay == false).Count();
            var sorgu = db.Kategori.ToList();
            return View(sorgu);
        }
        [Route("yonetimpaneli/giris/")]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(Admin admin)
        {
            var login = db.Admin.Where(x => x.Eposta == admin.Eposta).SingleOrDefault();
            if (login.Eposta == admin.Eposta && login.Sifre == Crypto.Hash(admin.Sifre, "MD5"))
            {
                Session["adminid"] = login.AdminId;
                Session["eposta"] = login.Eposta;
                Session["yetki"] = login.Yetki;
                return RedirectToAction("Index", "Admin");
            }
            ViewBag.Uyari = "Böyle bir hesap bulunamadı lütfen e-mail adresinizi veya şifrenizi kontrol ediniz.";
            return View(admin);
        }
        public ActionResult Logout()
        {
            Session["adminid"] = null;
            Session["eposta"] = null;
            Session.Abandon();
            return RedirectToAction("Login", "Admin");
        }
        public ActionResult SifremiUnuttum()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SifremiUnuttum(string eposta)
        {
            var mail = db.Admin.Where(x => x.Eposta == eposta).SingleOrDefault();
            if (mail != null)
            {
                // Yeni rastgele şifre oluşturma
                Random rnd = new Random();
                int yenisifre = rnd.Next(100000, 999999); // Rastgele 6 haneli şifre

                // Yeni şifreyi MD5 ile şifreleme
                mail.Sifre = Crypto.Hash(Convert.ToString(yenisifre), "MD5");
                db.SaveChanges();

                // E-posta parametrelerini ayarlama
                var fromAddress = new MailAddress("kurumsalweb01@gmail.com", "Admin Panel");
                var toAddress = new MailAddress(eposta);
                const string subject = "Admin Panel Giriş Şifreniz";
                string body = "Şifreniz: " + yenisifre;

                try
                {
                    // SMTP istemcisini yapılandırma
                    var smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587, // Alternatif olarak 465 kullanabilirsiniz.
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(fromAddress.Address, "UygulamaOzelSifreniz") // Buraya uygulama şifrenizi girin
                    };

                    // E-postayı gönderme
                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body
                    })
                    {
                        smtp.Send(message);
                    }

                    // Kullanıcıya e-postanın başarıyla gönderildiğini bildir
                    ViewBag.Uyari = "Şifreniz başarıyla gönderilmiştir.";
                }
                catch (SmtpException smtpEx)
                {
                    // SMTP spesifik hataları yakala
                    ViewBag.Uyari = "SMTP hatası oluştu: " + smtpEx.Message;
                }
                catch (Exception ex)
                {
                    // Diğer tüm hataları yakala
                    ViewBag.Uyari = "E-posta gönderilirken hata oluştu: " + ex.Message;
                }
            }
            else
            {
                // Kullanıcıya hata oluştuğunu bildir
                ViewBag.Uyari = "Hata oluştu. Tekrar deneyiniz.";
            }
            return View();
        }
        public ActionResult Adminler()
        {
            return View(db.Admin.ToList());
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(Admin admin, string sifre, string eposta)
        {
            if (ModelState.IsValid)
            {
                admin.Sifre = Crypto.Hash(sifre, "MD5");
                db.Admin.Add(admin);
                db.SaveChanges();
                return RedirectToAction("Adminler");
            }
            return View();
        }
        public ActionResult Edit(int id)
        {
            var a = db.Admin.Where(x => x.AdminId == id).SingleOrDefault();
            return View(a);
        }
        [HttpPost]
        public ActionResult Edit(int id, Admin admin, string sifre, string eposta)
        {
            if (ModelState.IsValid)
            {
                var a = db.Admin.Where(x => x.AdminId == id).SingleOrDefault();
                a.Sifre = Crypto.Hash(sifre, "MD5");
                a.Eposta = admin.Eposta;
                a.Yetki = admin.Yetki;
                db.SaveChanges();
                return RedirectToAction("Adminler");
            }
            return View(admin);
        }
        public ActionResult Delete(int id)
        {
            var a = db.Admin.Where(x => x.AdminId == id).SingleOrDefault();
            if (a != null)
            {
                db.Admin.Remove(a);
                db.SaveChanges();
                return RedirectToAction("Adminler");
            }
            return View();
        }
    }
}