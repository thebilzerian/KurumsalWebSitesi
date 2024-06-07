using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using KurumsalWeb.Models.DataContext;
using KurumsalWeb.Models.Model;

namespace KurumsalWeb.Controllers
{
    public class SliderController : Controller
    {
        private KurumsalDBContext db = new KurumsalDBContext();

        // GET: Sliders
        public ActionResult Index()
        {
            return View(db.Slider.ToList());
        }

        // GET: Sliders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Slider slider = db.Slider.Find(id);
            if (slider == null)
            {
                return HttpNotFound();
            }
            return View(slider);
        }

        // GET: Sliders/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Sliders/Create
        // Aşırı gönderim saldırılarından korunmak için bağlamak istediğiniz belirli özellikleri etkinleştirin. 
        // Daha fazla bilgi için bkz. https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SliderId,Baslik,Aciklama,ResimURL")] Slider slider,HttpPostedFileBase ResimURL)
        {
            if (ModelState.IsValid)
            {
                if (ResimURL != null)
                {
                    WebImage img = new WebImage(ResimURL.InputStream);
                    FileInfo imginfo = new FileInfo(ResimURL.FileName);
                    string sliderimgname = Guid.NewGuid().ToString() + imginfo.Extension;
                    img.Resize(600, 400);
                    img.Save("~/Uploads/Slider/" + sliderimgname);
                    slider.ResimURL = "/Uploads/Slider/" + sliderimgname;
                }
                db.Slider.Add(slider);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(slider);
        }

        // GET: Sliders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Slider slider = db.Slider.Find(id);
            if (slider == null)
            {
                return HttpNotFound();
            }
            return View(slider);
        }

        // POST: Sliders/Edit/5
        // Aşırı gönderim saldırılarından korunmak için bağlamak istediğiniz belirli özellikleri etkinleştirin. 
        // Daha fazla bilgi için bkz. https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SliderId,Baslik,Aciklama,ResimURL")] Slider slider,HttpPostedFileBase ResimURL,int id)
        {
            if (ModelState.IsValid)
            {
                var s = db.Slider.Where(x => x.SliderId == id).SingleOrDefault();
                if (ResimURL != null)
                {
                    
                    if (System.IO.File.Exists(Server.MapPath(s.ResimURL)))
                    {
                        System.IO.File.Delete(Server.MapPath(s.ResimURL));
                    }
                    WebImage img = new WebImage(ResimURL.InputStream);
                    FileInfo imginfo = new FileInfo(ResimURL.FileName);
                    string sliderimgname = Guid.NewGuid().ToString() + imginfo.Extension;
                    img.Resize(600, 400);
                    img.Save("~/Uploads/Slider/" + sliderimgname);
                    s.ResimURL = "/Uploads/Slider/" + sliderimgname;
                }
                s.Baslik =slider.Baslik;
                s.Aciklama =slider.Aciklama;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(slider);
        }

        // GET: Sliders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Slider slider = db.Slider.Find(id);
            if (slider == null)
            {
                return HttpNotFound();
            }
            return View(slider);
        }

        // POST: Sliders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var slider = db.Slider.Find(id);
            if (slider == null)
            {
                return HttpNotFound();
            }

            // Fiziksel yol olarak kullanılacak dosya adını alın
            string dosyaAdi = System.IO.Path.GetFileName(new Uri(slider.ResimURL).AbsolutePath);

            // Sanal yol oluşturun
            string sanalYol = "/Images/" + dosyaAdi;

            // Fiziksel yol olmadan, sanal yolu kullanarak dosya var mı kontrol edin
            string fizikselYol = Server.MapPath(sanalYol);
            if (System.IO.File.Exists(fizikselYol))
            {
                // Eğer dosya varsa, sil
                System.IO.File.Delete(fizikselYol);
            }

            // Slider öğesini veritabanından kaldır
            db.Slider.Remove(slider);
            db.SaveChanges();

            // Index sayfasına yönlendir
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
