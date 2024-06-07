﻿using KurumsalWeb.Models.DataContext;
using KurumsalWeb.Models.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace KurumsalWeb.Controllers
{
    public class BlogController : Controller
    {
        private KurumsalDBContext db = new KurumsalDBContext();
        // GET: Blog

        public ActionResult Index()
        {
            db.Configuration.LazyLoadingEnabled = false;
            return View(db.Blog.Include("Kategori").ToList().OrderByDescending(x => x.BlogId));
        }
        public ActionResult Create()
        {
            ViewBag.KategoriId = new SelectList(db.Kategori, "KategoriId", "KategoriAd");
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Blog blog, HttpPostedFileBase ResimURL)
        {
            if (ResimURL != null)
            {
                WebImage img = new WebImage(ResimURL.InputStream);
                FileInfo imginfo = new FileInfo(ResimURL.FileName);
                string blogimgname = Guid.NewGuid().ToString() + imginfo.Extension;
                img.Resize(600, 400);
                img.Save("~/Uploads/Blog/" + blogimgname);
                blog.ResimURL = "/Uploads/Blog/" + blogimgname;
            }
            db.Blog.Add(blog);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Edit(int id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            var blog = db.Blog.SingleOrDefault(x => x.BlogId == id);
            if (blog == null)
            {
                return HttpNotFound();
            }

            var kategoriListesi = db.Kategori.Select(k => new SelectListItem
            {
                Value = k.KategoriId.ToString(),
                Text = k.KategoriAd
            });

            ViewBag.KategoriId = kategoriListesi;
            return View(blog);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(int id, Blog blog, HttpPostedFileBase ResimURL)
        {
            if (ModelState.IsValid)
            {
                var b = db.Blog.Where(x => x.BlogId == id).SingleOrDefault();
                if (ResimURL != null)
                {
                    if (System.IO.File.Exists(Server.MapPath(b.ResimURL)))
                    {
                        System.IO.File.Delete(Server.MapPath(b.ResimURL));
                    }
                    WebImage img = new WebImage(ResimURL.InputStream);
                    FileInfo imginfo = new FileInfo(ResimURL.FileName);
                    string blogeimgname = Guid.NewGuid().ToString() + imginfo.Extension;
                    img.Resize(600, 400);
                    img.Save("~/Uploads/Blog/" + blogeimgname);
                    b.ResimURL = "/Uploads/Blog/" + blogeimgname;
                }
                b.Baslik = blog.Baslik;
                b.Icerik = blog.Icerik;
                b.KategoriId = blog.KategoriId;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(blog);
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var b = db.Blog.Find(id);
            if (b == null)
            {
                return HttpNotFound();
            }

            // Fiziksel yol olarak kullanılacak dosya adını alın
            string dosyaAdi = System.IO.Path.GetFileName(new Uri(b.ResimURL).AbsolutePath);

            // Sanal yol oluşturun
            string sanalYol = "/Images/" + dosyaAdi;

            // Fiziksel yol olmadan, sanal yolu kullanarak dosya var mı kontrol edin
            string fizikselYol = Server.MapPath(sanalYol);
            if (System.IO.File.Exists(fizikselYol))
            {
                // Eğer dosya varsa, sil
                System.IO.File.Delete(fizikselYol);
            }

            // Blog öğesini veritabanından kaldır
            db.Blog.Remove(b);
            db.SaveChanges();

            // Index sayfasına yönlendir
            return RedirectToAction("Index");
        }

    }
}