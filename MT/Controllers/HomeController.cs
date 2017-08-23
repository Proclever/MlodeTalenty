using Microsoft.AspNet.Identity;
using MT.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MT.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        private MTModelsDBContext db = new MTModelsDBContext();
        private JToken GetLocation(string ip = null)
        {
            if(ip == null) ip = Request.UserHostAddress;
            string json = "";
            using (WebClient client = new WebClient())
            {
                json = client.DownloadString("http://freegeoip.net/json/" + ip);
            }
            return JObject.Parse(json);
        }

        private double GetDistance(float lat1, float lon1, float lat2, float lon2)
        {
            var R = 6371;
            var dLat = (Math.PI / 180) * (lat2 - lat1);
            var dLon = (Math.PI / 180) * (lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos((Math.PI / 180) * lat1) * Math.Cos((Math.PI / 180) * lat2) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
        public string GetSubcategories(int cid)
        {
            var subcategories = db.Subcategorys.Where(x => x.categoryid == cid).Select(x => new SelectListItem { Value = x.id.ToString(), Text = x.name }).ToList();
            string html = "";
            foreach(var s in subcategories)
            {
                html += "<option value=\"" + s.Value + "\">" + s.Text + "</option>" + Environment.NewLine;
            }
            return html;
        }
        public static string StripHTML(string input)
        {
            return input == null ? null : Regex.Replace(input, "<.*?>", String.Empty);
        }
        public static string StripScript(string input)
        {
            return input == null ? null : Regex.Replace(input, @"<script[^>]*>[\s\S]*?</script>", String.Empty);
        }
        
        public ActionResult Index()
        {
            if (User.Identity.GetUserId() != null)
            {
                var userid = User.Identity.GetUserId();
                if(db.Companys.Where(x => x.userid == userid).Count()>0) return RedirectToAction("Cdesktop", "Home");
                if (db.Users.Where(x => x.userid == userid).Count() > 0) return RedirectToAction("Udesktop", "Home");
            }
            ViewBag.index = true;
            return View();
        }
        public ActionResult Info(bool? regcompany = null, string userid = null)
        {
            if (User.Identity.GetUserId() != null) RedirectToAction("Index", "Home");
            if (userid==null) RedirectToAction("Index", "Home");
            if (regcompany == true && db.Companys.Where(x => x.userid == userid).Count() == 0)
            {
                var c = db.Companys.Add(new Company { userid = userid, created = DateTime.Now, logo = null, name = null, description = null, address = null, lat = null, lon = null, phone = null, email = User.Identity.GetUserName() });
                db.SaveChanges();
            }
            if (regcompany == false && db.Users.Where(x => x.userid == userid).Count() == 0)
            {
                var c = db.Users.Add(new User { userid = userid, created = DateTime.Now });
                db.SaveChanges();
            }  
            return View();
        }
        public ActionResult Udesktop()
        {
            var userid = User.Identity.GetUserId();
            if (db.Users.Where(x => x.userid == userid).Count() != 1) return RedirectToAction("Index", "Home");
            if (db.Users.Where(x => x.userid == userid).First().name == null || db.Users.Where(x => x.userid == userid).First().name == "")
            {
                return RedirectToAction("EditUser", "Home");
            }
            return View(db.Users.Where(x => x.userid == userid).First());
        }
        public ActionResult Cdesktop()
        {
            var userid = User.Identity.GetUserId();
            if (db.Companys.Where(x => x.userid == userid).Count() != 1) return RedirectToAction("Index", "Home");
            return View(db.Companys.Where(x => x.userid == userid).First());
        }

        public ActionResult Offers(
            string search = null, string location = null, string latitude = null, string longitude = null,
            int maxdistance = 10,
            string category1 = "", string subcategory11 = "", string subcategory12 = "", string subcategory13 = "",
            string category2 = "", string subcategory21 = "", string subcategory22 = "", string subcategory23 = "",
            string category3 = "", string subcategory31 = "", string subcategory32 = "", string subcategory33 = "",
            int? age = null,
            bool monday = true, string mondaytime = null, int mondayfromh = 0, int mondayfromm = 0, int mondaytoh = 23, int mondaytom = 45,
            bool tuesday = true, int tuesdayfromh = 0, int tuesdayfromm = 0, int tuesdaytoh = 23, int tuesdaytom = 45,
            bool wednesday = true, int wednesdayfromh = 0, int wednesdayfromm = 0, int wednesdaytoh = 23, int wednesdaytom = 45,
            bool thursday = true, int thursdayfromh = 0, int thursdayfromm = 0, int thursdaytoh = 23, int thursdaytom = 45,
            bool friday = true, int fridayfromh = 0, int fridayfromm = 0, int fridaytoh = 23, int fridaytom = 45,
            bool saturday = true, int saturdayfromh = 0, int saturdayfromm = 0, int saturdaytoh = 23, int saturdaytom = 45,
            bool sunday = true, int sundayfromh = 0, int sundayfromm = 0, int sundaytoh = 23, int sundaytom = 45,
            bool individual = false, bool firstfree = false, bool drive = false, bool online = false,
            int page = 1
            )
        {
            ViewBag.search = search; /*if (location == null) location = (string)GetLocation().SelectToken("ip");*/ ViewBag.location = location; if (latitude == null || latitude == "") latitude = (string)GetLocation().SelectToken("latitude"); ViewBag.latitude = latitude; if (longitude == null || longitude == "") longitude = (string)GetLocation().SelectToken("longitude"); ViewBag.longitude = longitude;
            ViewBag.maxdistance = maxdistance;
            ViewBag.category1 = category1; ViewBag.subcategory11 = subcategory11; ViewBag.subcategory12 = subcategory12; ViewBag.subcategory13 = subcategory13;
            ViewBag.category2 = category2; ViewBag.subcategory21 = subcategory21; ViewBag.subcategory22 = subcategory22; ViewBag.subcategory23 = subcategory23;
            ViewBag.category3 = category3; ViewBag.subcategory31 = subcategory31; ViewBag.subcategory32 = subcategory32; ViewBag.subcategory33 = subcategory33;
            ViewBag.age = age;
            if (new DateTime().AddHours(mondayfromh).AddMinutes(mondayfromm) > new DateTime().AddHours(mondaytoh).AddMinutes(mondaytom))
            { var temp = mondayfromh; mondayfromh = mondaytoh; mondaytoh = temp; temp = mondayfromm; mondayfromm = mondaytom; mondaytom = temp; }
            ViewBag.monday = monday; ViewBag.mondayfromh = mondayfromh; ViewBag.mondayfromm = mondayfromm; ViewBag.mondaytoh = mondaytoh; ViewBag.mondaytom = mondaytom;
            if (new DateTime().AddHours(tuesdayfromh).AddMinutes(tuesdayfromm) > new DateTime().AddHours(tuesdaytoh).AddMinutes(tuesdaytom))
            { var temp = tuesdayfromh; tuesdayfromh = tuesdaytoh; tuesdaytoh = temp; temp = tuesdayfromm; tuesdayfromm = tuesdaytom; tuesdaytom = temp; }
            ViewBag.tuesday = tuesday; ViewBag.tuesdayfromh = tuesdayfromh; ViewBag.tuesdayfromm = tuesdayfromm; ViewBag.tuesdaytoh = tuesdaytoh; ViewBag.tuesdaytom = tuesdaytom;
            if (new DateTime().AddHours(wednesdayfromh).AddMinutes(wednesdayfromm) > new DateTime().AddHours(wednesdaytoh).AddMinutes(wednesdaytom))
            { var temp = wednesdayfromh; wednesdayfromh = wednesdaytoh; wednesdaytoh = temp; temp = wednesdayfromm; wednesdayfromm = wednesdaytom; wednesdaytom = temp; }
            ViewBag.wednesday = wednesday; ViewBag.wednesdayfromh = wednesdayfromh; ViewBag.wednesdayfromm = wednesdayfromm; ViewBag.wednesdaytoh = wednesdaytoh; ViewBag.wednesdaytom = wednesdaytom;
            if (new DateTime().AddHours(thursdayfromh).AddMinutes(thursdayfromm) > new DateTime().AddHours(thursdaytoh).AddMinutes(thursdaytom))
            { var temp = thursdayfromh; thursdayfromh = thursdaytoh; thursdaytoh = temp; temp = thursdayfromm; thursdayfromm = thursdaytom; thursdaytom = temp; }
            ViewBag.thursday = thursday; ViewBag.thursdayfromh = thursdayfromh; ViewBag.thursdayfromm = thursdayfromm; ViewBag.thursdaytoh = thursdaytoh; ViewBag.thursdaytom = thursdaytom;
            if (new DateTime().AddHours(fridayfromh).AddMinutes(fridayfromm) > new DateTime().AddHours(fridaytoh).AddMinutes(fridaytom))
            { var temp = fridayfromh; fridayfromh = fridaytoh; fridaytoh = temp; temp = fridayfromm; fridayfromm = fridaytom; fridaytom = temp; }
            ViewBag.friday = friday; ViewBag.fridayfromh = fridayfromh; ViewBag.fridayfromm = fridayfromm; ViewBag.fridaytoh = fridaytoh; ViewBag.fridaytom = fridaytom;
            if (new DateTime().AddHours(saturdayfromh).AddMinutes(saturdayfromm) > new DateTime().AddHours(saturdaytoh).AddMinutes(saturdaytom))
            { var temp = saturdayfromh; saturdayfromh = saturdaytoh; saturdaytoh = temp; temp = saturdayfromm; saturdayfromm = saturdaytom; saturdaytom = temp; }
            ViewBag.saturday = saturday; ViewBag.saturdayfromh = saturdayfromh; ViewBag.saturdayfromm = saturdayfromm; ViewBag.saturdaytoh = saturdaytoh; ViewBag.saturdaytom = saturdaytom;
            if (new DateTime().AddHours(sundayfromh).AddMinutes(sundayfromm) > new DateTime().AddHours(sundaytoh).AddMinutes(sundaytom))
            { var temp = sundayfromh; sundayfromh = sundaytoh; sundaytoh = temp; temp = sundayfromm; sundayfromm = sundaytom; sundaytom = temp; }
            ViewBag.sunday = sunday; ViewBag.sundayfromh = sundayfromh; ViewBag.sundayfromm = sundayfromm; ViewBag.sundaytoh = sundaytoh; ViewBag.sundaytom = sundaytom;
            ViewBag.individual = individual; ViewBag.firstfree = firstfree; ViewBag.drive = drive; ViewBag.online = online;

            var offers = db.Offers.Where(x => x.publicated).ToList();
            
            foreach(var o in offers.ToList())
            {
                if (maxdistance < GetDistance(float.Parse(latitude, CultureInfo.InvariantCulture), float.Parse(longitude, CultureInfo.InvariantCulture), o.lat, o.lon))
                    offers.Remove(o);
            }

            if (individual) offers = offers.Where(x => x.individual).ToList();
            if (firstfree) offers = offers.Where(x => x.firstfree).ToList();
            if (drive) offers = offers.Where(x => x.drive).ToList();
            if (online) offers = offers.Where(x => x.online).ToList();

            var oloop = offers;
            foreach(var o in oloop.ToList())
            {
                bool match = false;
                if (monday)
                {
                    foreach(var d in o.EventDate.Where(x => x.day == 1))
                    {
                        if (new DateTime().AddYears(1753).AddHours(mondayfromh).AddMinutes(mondayfromm) <= d.from && new DateTime().AddYears(1753).AddHours(mondaytoh).AddMinutes(mondaytom) >= d.to) { match = true; goto End; }
                    }
                }
                if (tuesday)
                {
                    foreach (var d in o.EventDate.Where(x => x.day == 2))
                    {
                        if (new DateTime().AddYears(1753).AddHours(tuesdayfromh).AddMinutes(tuesdayfromm) <= d.from && new DateTime().AddYears(1753).AddHours(tuesdaytoh).AddMinutes(tuesdaytom) >= d.to) { match = true; goto End; }
                    }
                }
                if (wednesday)
                {
                    foreach (var d in o.EventDate.Where(x => x.day == 3))
                    {
                        if (new DateTime().AddYears(1753).AddHours(wednesdayfromh).AddMinutes(wednesdayfromm) <= d.from && new DateTime().AddYears(1753).AddHours(wednesdaytoh).AddMinutes(wednesdaytom) >= d.to) { match = true; goto End; }
                    }
                }
                if (thursday)
                {
                    foreach (var d in o.EventDate.Where(x => x.day == 4))
                    {
                        if (new DateTime().AddYears(1753).AddHours(thursdayfromh).AddMinutes(thursdayfromm) <= d.from && new DateTime().AddYears(1753).AddHours(thursdaytoh).AddMinutes(thursdaytom) >= d.to) { match = true; goto End; }
                    }
                }
                if (friday)
                {
                    foreach (var d in o.EventDate.Where(x => x.day == 5))
                    {
                        if (new DateTime().AddYears(1753).AddHours(fridayfromh).AddMinutes(fridayfromm) <= d.from && new DateTime().AddYears(1753).AddHours(fridaytoh).AddMinutes(fridaytom) >= d.to) { match = true; goto End; }
                    }
                }
                if (saturday)
                {
                    foreach (var d in o.EventDate.Where(x => x.day == 6))
                    {
                        if (new DateTime().AddYears(1753).AddHours(saturdayfromh).AddMinutes(saturdayfromm) <= d.from && new DateTime().AddYears(1753).AddHours(saturdaytoh).AddMinutes(saturdaytom) >= d.to) { match = true; goto End; }
                    }
                }
                if (tuesday)
                {
                    foreach (var d in o.EventDate.Where(x => x.day == 7))
                    {
                        if (new DateTime().AddYears(1753).AddHours(sundayfromh).AddMinutes(sundayfromm) <= d.from && new DateTime().AddYears(1753).AddHours(sundaytoh).AddMinutes(sundaytom) >= d.to) { match = true; goto End; }
                    }
                }
                End:
                if (!match) offers.Remove(o);
            }

            if(category1 != "" && category1 != null)
            {
                var cat1 = offers.Where(x => x.Subcategory.categoryid.ToString() == category1);
                if(subcategory11 != "" && subcategory11 != null)
                {
                    var sub1 = cat1.Where(x => x.subcategoryid.ToString() == subcategory11);
                    if (subcategory12 != "" && subcategory12 != null)
                    {
                        sub1 = sub1.Concat(cat1.Where(x => x.subcategoryid.ToString() == subcategory12));
                        if (subcategory13 != "" && subcategory13 != null)
                        {
                            sub1 = sub1.Concat(cat1.Where(x => x.subcategoryid.ToString() == subcategory13));
                        }
                    }
                }
                if (category2 != "" && category2 != null)
                {
                    var cat2 = offers.Where(x => x.Subcategory.categoryid.ToString() == category2);
                    if (subcategory21 != "" && subcategory21 != null)
                    {
                        var sub1 = cat2.Where(x => x.subcategoryid.ToString() == subcategory21);
                        if (subcategory22 != "" && subcategory22 != null)
                        {
                            sub1 = sub1.Concat(cat2.Where(x => x.subcategoryid.ToString() == subcategory22));
                            if (subcategory23 != "" && subcategory23 != null)
                            {
                                sub1 = sub1.Concat(cat2.Where(x => x.subcategoryid.ToString() == subcategory23));
                            }
                        }
                    }
                    cat1 = cat1.Concat(cat2);
                    if (category3 != "" && category3 != null)
                    {
                        var cat3 = offers.Where(x => x.Subcategory.categoryid.ToString() == category3);
                        if (subcategory31 != "" && subcategory31 != null)
                        {
                            var sub1 = cat3.Where(x => x.subcategoryid.ToString() == subcategory31);
                            if (subcategory32 != "" && subcategory32 != null)
                            {
                                sub1 = sub1.Concat(cat3.Where(x => x.subcategoryid.ToString() == subcategory32));
                                if (subcategory33 != "" && subcategory33 != null)
                                {
                                    sub1 = sub1.Concat(cat3.Where(x => x.subcategoryid.ToString() == subcategory33));
                                }
                            }
                        }
                        cat1 = cat1.Concat(cat3);
                    }
                }
                offers = cat1.ToList();
            }

            if(age != null) offers = offers.Where(x => x.agefrom <= age).Where(x => x.ageto >= age).ToList();

            if (search != "" && search != null && offers.Count() > 0)
            {
                string[] keywords = search.Split(' ');
                oloop = offers;
                foreach (var o in oloop.ToList())
                {
                    bool searched = false;
                    int match = 0;
                    string onestring = o.Subcategory.Category.name + "; " + o.Subcategory.name + "; " + o.title + "; " + o.description + "; " + o.address + "; " + (o.individual ? "indywidualnie; indywidualne; " : "") + (o.firstfree ? "darmowe; darmowa; próbne; próbna; " : "") + (o.drive ? "dojazd; dojazdem; " : "") + (o.online ? "online; zdalnie; " : "");
                    if (o.EventDate.Count() > 0) onestring += (o.EventDate.Where(x => x.day == 1).Count() > 0 ? "poniedziałek; " : "") + (o.EventDate.Where(x => x.day == 2).Count() > 0 ? "wtorek; " : "") + (o.EventDate.Where(x => x.day == 3).Count() > 0 ? "środa; " : "") + (o.EventDate.Where(x => x.day == 4).Count() > 0 ? "czwartek; " : "") + (o.EventDate.Where(x => x.day == 5).Count() > 0 ? "piątek; " : "") + (o.EventDate.Where(x => x.day == 6).Count() > 0 ? "sobota; " : "") + (o.EventDate.Where(x => x.day == 7).Count() > 0 ? "niedziela; " : "");
                    foreach (var k in keywords)
                    {
                        if (onestring.ToLower().Replace('ą', 'a').Replace('ć', 'c').Replace('ę', 'e').Replace('ł', 'l').Replace('ń', 'n').Replace('ó', 'o').Replace('ś', 's').Replace('ź', 'z').Replace('ż', 'z').Contains(k.ToLower().Replace('ą', 'a').Replace('ć', 'c').Replace('ę', 'e').Replace('ł', 'l').Replace('ń', 'n').Replace('ó', 'o').Replace('ś', 's').Replace('ź', 'z').Replace('ż', 'z'))) { searched = true; match++; }
                    }
                    if (!searched) offers.Remove(o);
                    else offers.Where(x => x.id == o.id).First().match = match;
                }
            }

            offers = offers.OrderByDescending(x => x.match).ToList();
            if (offers.Count > 0)
            {
                ViewBag.pagerange = 15;
                ViewBag.lastpage = (int)Math.Ceiling((decimal)offers.Count / (decimal)ViewBag.pagerange);
                if (page > ViewBag.lastpage) page = (int)ViewBag.lastpage;
                ViewBag.page = page;
                offers = offers.GetRange((page - 1) * ViewBag.pagerange, page == ViewBag.lastpage ? (offers.Count % ViewBag.pagerange == 0 ? ViewBag.pagerange : offers.Count % ViewBag.pagerange) : ViewBag.pagerange);
            }
            return View(offers);
        }
        public ActionResult EmailForm(int id, string kind)
        {
            var form = new EmailFormModel();
            var userid = User.Identity.GetUserId();
            if (userid != null && db.Users.Where(x => x.userid == userid).Count() == 1)
            {
                form.FromEmail = User.Identity.GetUserName();
                form.FromName = db.Users.Where(x => x.userid == userid).First().name;
            }
            if (kind == "Company")
            {
                ViewBag.Title = "Formularz kontaktowy";
                ViewBag.Subtitle = "Wiadomość do " + db.Companys.Find(id).name;
            }
            else
            {
                if (kind == "Offer")
                {
                    ViewBag.Title = "Formularz kontaktowy";
                    ViewBag.Subtitle = "Wiadomość do " + db.Offers.Find(id).Company.name + " w związku z ofertą \"" + db.Offers.Find(id).title + "\"";
                }
                else
                {
                    if (kind == "Contact")
                    {
                        ViewBag.Title = "Formularz kontaktowy";
                        ViewBag.Subtitle = "Wiadomość do zespołu mlodetalenty.net";
                    }
                }
            }
            form.StartTime = DateTime.Now;
            return View(form);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EmailForm(int id, string kind, EmailFormModel efm)
        {
            if (ModelState.IsValid && (DateTime.Now - efm.StartTime).TotalSeconds > 10)
            {
                var sysLogin = ConfigurationManager.AppSettings["mailAccount"];
                var sysPass = ConfigurationManager.AppSettings["mailPassword"];
                var sysAddress = new MailAddress(sysLogin, efm.FromName + " (mlodetalenty.net)");
                string mailto = null;
                string pre = "";
                if (kind == "Company")
                {
                    mailto = db.Companys.Find(id).email;
                    pre = "<p>Wiadomość od " + efm.FromName +" wysłana poprzez formularz kontaktowy mlodetalenty.net:</p>";
                }
                else
                {
                    if (kind == "Offer")
                    {
                        mailto = db.Offers.Find(id).email;
                        pre = "<p>Wiadomość od " + efm.FromName + " wysłana poprzez formularz kontaktowy mlodetalenty.net w zwiazku z ofertą \"" + db.Offers.Find(id).title + "\":</p>";
                    }
                    else
                    {
                        if (kind == "Contact")
                        {
                            mailto = "kontakt@mlodetalenty.net";
                            pre = "<p>Wiadomość od " + efm.FromName + " wysłana poprzez formularz kontaktowy mlodetalenty.net:</p>";
                        }
                    }
                }

                var receiverAddress = new MailAddress(mailto);

                var smtp = new SmtpClient
                {
                    Host = "smtp.webio.pl",
                    Port = 587,
                    EnableSsl = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(sysLogin, sysPass)
                };
                var body = pre + "<p>" + StripHTML(efm.Message) + "</p>" + "<p>Adres nadawcy: " + efm.FromEmail + " (aby odpowiedzieć, wyślij wiadomość na ten adres)</p>";
                using (var mess = new MailMessage(sysAddress, receiverAddress) { Subject = efm.Subject, Body = body, IsBodyHtml = true })
                {
                    smtp.Send(mess);
                }
                return RedirectToAction("EmailSent", "Home", new { action = kind, id = id });
            }
            else
            {
                ViewBag.addoffer = true;
                return View(efm);
            }
        }
        public ActionResult EmailSent(string action, int id)
        {
            if(id>0)
            {
                if(action == "Company")
                {
                    ViewBag.Backlink = "<a href=\"" + Url.Action(action, "Home", new { id = id }) + "\">Powrót do widoku firmy</a>";
                }
                if (action == "Offer")
                {
                    ViewBag.Backlink = "<a href=\"" + Url.Action(action, "Home", new { id = id }) + "\">Powrót do widoku oferty</a>";
                }
            }
            return View();
        }
        public ActionResult UserV()
        {
            var userid = User.Identity.GetUserId();
            ViewBag.userid = userid;
            if (db.Users.Where(x => x.userid == userid).Count() != 1) return RedirectToAction("Index", "Home");
            if (db.Users.Where(x => x.userid == userid).First().name == null || db.Users.Where(x => x.userid == userid).First().name == "")
            {
                return RedirectToAction("EditUser", "Home");
            }
            return View(db.Users.Where(x => x.userid == userid).First());
        }

        [Authorize]
        public ActionResult EditUser()
        {
            var userid = User.Identity.GetUserId();
            ViewBag.userid = userid;
            if (db.Users.Where(x => x.userid == userid).Count() != 1) return RedirectToAction("Index", "Home");
            ViewBag.addoffer = true;
            return View(db.Users.Where(x => x.userid == userid).First());
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(User user)
        {
            var userid = User.Identity.GetUserId();
            if (db.Users.Where(x => x.userid == userid).Count() != 1) return RedirectToAction("Index", "Home");
            if(user.name==null || user.name=="") ModelState.AddModelError("name", "Wpisz swoje imię.");
            if (ModelState.IsValid)
            {
                user.name = StripHTML(user.name);
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("UserV", "Home");
            }
            else
            {
                ViewBag.addoffer = true;
                return View(user);
            }
        }

        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost]
        public ActionResult AddUserPhoto(HttpPostedFileBase file, int id)
        {
            var userid = User.Identity.GetUserId();
            if (db.Users.Where(x => x.id == id).First().userid != userid) return RedirectToAction("Index", "Home");
            if (file != null && file.ContentLength / 1024 / 1024 < 10)
            {
                string oldlogo = db.Users.Where(x => x.id == id).First().photo;
                string pic = Guid.NewGuid() + Path.GetExtension(file.FileName);
                string dir = Server.MapPath("~/images/");
                string path = Path.Combine(dir, pic);
                //if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                file.SaveAs(path);
                using (MemoryStream ms = new MemoryStream())
                {
                    file.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();
                }
                db.Users.Where(x => x.id == id).First().photo = pic;
                db.SaveChanges();
                if (oldlogo != null && oldlogo != "")
                {
                    string oldpath = Path.Combine(dir, oldlogo);
                    System.IO.File.Delete(oldpath);
                }
            }
            return RedirectToAction("UserV", "Home");
        }
        public ActionResult Company(int id)
        {
            var company = db.Companys.Where(x => x.id == id).First();
            if (company.name == null || company.name == "")
            {
                return RedirectToAction("EditCompany", "Home");
            }
            var userid = User.Identity.GetUserId();
            ViewBag.lat = company.lat.ToString().Replace(',', '.');
            ViewBag.lon = company.lon.ToString().Replace(',', '.');
            ViewBag.objectname = company.name;
            ViewBag.address = company.address;
            ViewBag.isvuser = db.Users.Where(x => x.userid == userid).Count() == 1 ? true : false;
            return View(db.Companys.Where(x => x.id == id).First());
        }

        [Authorize]
        public ActionResult EditCompany()
        {
            var userid = User.Identity.GetUserId();
            ViewBag.userid = userid;
            if (db.Companys.Where(x => x.userid == userid).Count() != 1) return RedirectToAction("Index", "Home");
            ViewBag.addoffer = true;
            return View(db.Companys.Where(x => x.userid == userid).First());
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCompany(Company company, string lat, string lon)
        {
            var userid = User.Identity.GetUserId();
            if (db.Companys.Where(x => x.userid == userid).Count() != 1) return RedirectToAction("Index", "Home");
            ModelState.Remove("lat");
            ModelState.Remove("lon");
            if (company.name == null || company.name == "") ModelState.AddModelError("name", "Podaj nazwę firmy.");
            //if(lat==null || lat=="" || lon == null || lon == "") ModelState.AddModelError("address", "Podaj adres.");
            if (lat != "" && lat != null) company.lat = float.Parse(lat.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat);
            if (lon != "" && lon != null) company.lon = float.Parse(lon.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat);
            if (ModelState.IsValid)
            {
                company.name = StripHTML(company.name);
                company.description = StripScript(company.description);
                company.email = StripHTML(company.email);
                company.phone = StripHTML(company.phone);
                db.Entry(company).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Company", "Home", new { id = company.id });
            }
            else
            {
                if (lat != "" && lat != null) company.lat = float.Parse(lat.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat);
                if (lon != "" && lon != null) company.lon = float.Parse(lon.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat);
                ViewBag.addoffer = true;
                return View(company);
            }
        }

        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost]
        public ActionResult AddLogo(HttpPostedFileBase file, int id)
        {
            var userid = User.Identity.GetUserId();
            if (db.Companys.Where(x => x.id == id).First().userid != userid) return RedirectToAction("Index", "Home");
            if (file != null && file.ContentLength / 1024 / 1024 < 10)
            {
                string oldlogo = db.Companys.Where(x => x.id == id).First().logo;
                string pic = Guid.NewGuid() + Path.GetExtension(file.FileName);
                string dir = Server.MapPath("~/images/");
                string path = Path.Combine(dir, pic);
                //if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                file.SaveAs(path);
                using (MemoryStream ms = new MemoryStream())
                {
                    file.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();
                }
                db.Companys.Where(x => x.id == id).First().logo = pic;
                db.SaveChanges();
                if(oldlogo != null && oldlogo != "")
                {
                    string oldpath = Path.Combine(dir, oldlogo);
                    System.IO.File.Delete(oldpath);
                }
            }
            return RedirectToAction("Company", "Home", new { id = id });
        }

        public ActionResult Offer(int id)
        {
            var offer = db.Offers.Where(x => x.id == id).First();
            var userid = User.Identity.GetUserId();
            ViewBag.userid = userid;
            if (offer.Company.userid != userid && !offer.publicated) return RedirectToAction("Index", "Home");
            ViewBag.lat = offer.lat.ToString().Replace(',', '.');
            ViewBag.lon = offer.lon.ToString().Replace(',', '.');
            ViewBag.objectname = offer.objectname;
            ViewBag.address = offer.address;
            ViewBag.isvuser = db.Users.Where(x => x.userid == userid).Count() == 1 ? true : false;
            return View(offer);
        }

        [Authorize]
        public ActionResult AddOffer()
        {
            var userid = User.Identity.GetUserId();
            ViewBag.userid = userid;
            if (db.Companys.Where(x => x.userid == userid).Count() != 1) return RedirectToAction("Index", "Home");
            if(db.Companys.Where(x => x.userid == userid).First().name == null || db.Companys.Where(x => x.userid == userid).First().name == "") return RedirectToAction("EditCompany", "Home");
            ViewBag.addoffer = true;
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddOffer(Offer offer, string subcategory, string lat, string lon)
        {
            var userid = User.Identity.GetUserId();
            ViewBag.userid = userid;
            if (db.Companys.Where(x => x.userid == userid).Count() != 1) return RedirectToAction("Index", "Home");
            ModelState.Remove("lat");
            ModelState.Remove("lon");
            ModelState.Remove("Subcategory");
            if (lat == null || lat == "" || lon == null || lon == "") ModelState.AddModelError("address", "Podaj adres.");
            if (subcategory == "" || subcategory == null) ModelState.AddModelError("subcategoryid", "Wybierz kategorię i podkategorię.");
            if(offer.agefrom == 0 || offer.ageto == 0) ModelState.AddModelError("agefrom", "Podaj wiek większy od 0.");
            if (ModelState.IsValid && (subcategory != "" && subcategory != null) && (lat != "" && lat != null) && (lon != "" && lon != null) && (offer.agefrom!=0 && offer.ageto != 0))
            {
                offer.title = StripHTML(offer.title);
                offer.description = StripScript(offer.description);
                offer.period = StripHTML(offer.period);
                offer.objectname = StripHTML(offer.objectname);
                offer.address = StripHTML(offer.address);
                offer.phone = StripHTML(offer.phone);
                offer.email = StripHTML(offer.email);
                if (offer.agefrom > offer.ageto) { var temp = offer.agefrom; offer.agefrom = offer.ageto; offer.ageto = temp; }
                offer.subcategoryid = int.Parse(subcategory);
                offer.lat = float.Parse(lat.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat);
                offer.lon = float.Parse(lon.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat);
                var o = db.Offers.Add(new Offer { companyid = db.Companys.Where(x => x.userid == userid).First().id, subcategoryid = offer.subcategoryid, created = DateTime.Now, title = offer.title, description = offer.description, price = offer.price, period = offer.period, iscompanyaddress = offer.iscompanyaddress, objectname = offer.objectname, address = offer.address, lat = offer.lat, lon = offer.lon, phone = offer.phone, email = offer.email, agefrom = offer.agefrom, ageto = offer.ageto, individual = offer.individual, firstfree = offer.firstfree, drive = offer.drive, online = offer.online, sponsored = false, publicated = false, match = null });
                db.SaveChanges();
                return RedirectToAction("Offer", "Home", new { id = o.id });
            }
            else
            {
                ViewBag.addoffer = true;
                if (subcategory != "" && subcategory != null) offer.subcategoryid = int.Parse(subcategory);
                if (lat != "" && lat != null) offer.lat = float.Parse(lat.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat);
                if (lon != "" && lon != null) offer.lon = float.Parse(lon.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat);
                ViewBag.lat = lat;
                ViewBag.lon = lon;
                ViewBag.subcategory = subcategory;
                return View(offer);
            }
        }

        [Authorize]
        public ActionResult Publishing(int offerid, bool publicated)
        {
            var userid = User.Identity.GetUserId();
            var offer = db.Offers.Where(x => x.id == offerid).First();
            if (offer.Company.userid != userid) return RedirectToAction("Index", "Home");
            offer.publicated = publicated;
            db.Entry(offer).State = EntityState.Modified;
            db.SaveChanges();
            
            if(publicated)
            {
                foreach(var wat in offer.Watching.ToList())
                {
                    var un = db.UserNotifications.Add(new UserNotification { userid = wat.userid, created = DateTime.Now, title = "Oferta zajęć <strong>" + offer.title + "</strong>, którą obserwujesz, została zaktualizowana.", offer = offer.id, check = false });
                }
                foreach (var subc in offer.Company.SubscribeCompany.ToList())
                {
                    if (offer.Watching.Where(x => x.userid == subc.userid).Count() > 0) continue;
                    if (db.UserNotifications.Where(x => x.offer == offerid && x.userid == subc.userid).Count() == 0)
                    {
                        var un = db.UserNotifications.Add(new UserNotification { userid = subc.userid, created = DateTime.Now, title = "Firma <strong>" + subc.Company.name + "</strong>, którą obserwujesz, opublikowała ofertę zajęć <strong>" + offer.title + "</strong>.", offer = offer.id, check = false });
                    }
                }         
            }

            return RedirectToAction("Offer", "Home", new { id = offerid });
        }

        [Authorize]
        public ActionResult EditOffer(int id)
        {
            var offer = db.Offers.Where(x => x.id == id).First();
            var userid = User.Identity.GetUserId();
            ViewBag.userid = User.Identity.GetUserId();
            if (offer.Company.userid != userid) return RedirectToAction("Index", "Home");
            ViewBag.addoffer = true;
            return View(offer);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditOffer(Offer offer, string subcategory, string lat, string lon)
        {
            var userid = User.Identity.GetUserId();
            ViewBag.userid = userid;
            if (db.Companys.Where(x => x.userid == userid).Count() != 1) return RedirectToAction("Index", "Home");
            ModelState.Remove("lat");
            ModelState.Remove("lon");
            ModelState.Remove("Subcategory");
            if (lat == null || lat == "" || lon == null || lon == "") ModelState.AddModelError("address", "Podaj adres.");
            if (subcategory == "" || subcategory == null) ModelState.AddModelError("subcategoryid", "Wybierz kategorię i podkategorię.");
            if (offer.agefrom == 0 || offer.ageto == 0) ModelState.AddModelError("agefrom", "Podaj wiek większy od 0.");
            if (ModelState.IsValid && (subcategory != "" && subcategory != null) && (lat != "" && lat != null) && (lon != "" && lon != null) && (offer.agefrom != 0 && offer.ageto != 0))
            {
                offer.title = StripHTML(offer.title);
                offer.description = StripScript(offer.description);
                offer.period = StripHTML(offer.period);
                offer.objectname = StripHTML(offer.objectname);
                offer.address = StripHTML(offer.address);
                offer.phone = StripHTML(offer.phone);
                offer.email = StripHTML(offer.email);
                if (offer.agefrom > offer.ageto) { var temp = offer.agefrom; offer.agefrom = offer.ageto; offer.ageto = temp; }
                offer.subcategoryid = int.Parse(subcategory);
                offer.lat = float.Parse(lat.Replace(',','.'), CultureInfo.InvariantCulture.NumberFormat);
                offer.lon = float.Parse(lon.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat);
                db.Entry(offer).State = EntityState.Modified;
                db.SaveChanges();
                if (offer.publicated) return RedirectToAction("Publishing", "Home", new { offerid = offer.id, publicated = true });
                else return RedirectToAction("Offer", "Home", new { id = offer.id });
            }
            else
            {
                if (lat == null || lat == "" || lon == null || lon == "") ModelState.AddModelError("address", "Podaj adres.");
                ViewBag.addoffer = true;
                if (subcategory != "" && subcategory != null) offer.subcategoryid = int.Parse(subcategory);
                if (lat != "" && lat != null) offer.lat = float.Parse(lat.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat);
                if (lon != "" && lon != null) offer.lon = float.Parse(lon.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat);
                ViewBag.lat = lat;
                ViewBag.lon = lon;
                ViewBag.subcategory = subcategory;
                return View(offer);
            }
        }

        [Authorize]
        public ActionResult DeleteOffer(int id)
        {
            var offer = db.Offers.Where(x => x.id == id).First();
            var userid = User.Identity.GetUserId();
            if (offer.Company.userid != userid) return RedirectToAction("Index", "Home");

            foreach (var p in offer.Photo)
            {
                db.Photos.Remove(p);
                string dir = Server.MapPath("~/images/");
                string path = Path.Combine(dir, p.url);
                System.IO.File.Delete(path);
            }
            db.SaveChanges();
            foreach (var ed in offer.EventDate)
            {
                db.EventDates.Remove(ed);
            }
            db.SaveChanges();
            foreach (var w in offer.Watching)
            {
                db.Watchings.Remove(w);
            }
            db.SaveChanges();
            foreach (var c in offer.Comment)
            {
                db.Comments.Remove(c);
            }
            db.SaveChanges();
            db.Offers.Remove(offer);
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult AddEventDate(int offer_id)
        {
            var userid = User.Identity.GetUserId();
            if (db.Offers.Where(x => x.id == offer_id).First().Company.userid != userid) return RedirectToAction("Index", "Home");
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddEventDate(EventDate eventdate, int offer_id, int fromh, int fromm, int toh, int tom)
        {
            var userid = User.Identity.GetUserId();
            if (db.Offers.Where(x => x.id == offer_id).First().Company.userid != userid) return RedirectToAction("Index", "Home");
            if (eventdate.day > 0 && (fromh>=0 && fromh<=23) && (fromm >= 0 && fromm <= 59) && (toh >= 0 && toh <= 23) && (tom >= 0 && tom <= 59))
            {
                var from1 = new DateTime().AddYears(1753).AddHours(fromh).AddMinutes(fromm);
                var to1 = new DateTime().AddYears(1753).AddHours(toh).AddMinutes(tom);
                if (from1 > to1) { var temp = from1; from1 = to1; to1 = temp; }
                var e = db.EventDates.Add(new EventDate { offerid = offer_id, day = eventdate.day, from = from1, to = to1 });
                db.SaveChanges();
                return RedirectToAction("Offer", "Home", new { id = offer_id });
            }
            else
            {
                ViewBag.fromh = fromh;
                ViewBag.fromm = fromm;
                ViewBag.toh = toh;
                ViewBag.tom = tom;
                return View(eventdate);
            }
        }

        [Authorize]
        public ActionResult DeleteEventDate(int id)
        {
            var userid = User.Identity.GetUserId();
            if (db.EventDates.Where(x => x.id == id).First().Offer.Company.userid != userid) return RedirectToAction("Index", "Home");
            var offer_id = db.EventDates.Where(x => x.id == id).First().Offer.id;
            EventDate eventdate = db.EventDates.Find(id);
            db.EventDates.Remove(eventdate);
            db.SaveChanges();
            return RedirectToAction("Offer", "Home", new { id = offer_id });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPhoto(HttpPostedFileBase file, int offer_id)
        {
            var userid = User.Identity.GetUserId();
            if (db.Offers.Where(x => x.id == offer_id).First().Company.userid != userid) return RedirectToAction("Index", "Home");
            if(db.Offers.Where(x => x.id == offer_id).First().Photo.Count() == 3) return RedirectToAction("Offer", "Home", new { id = offer_id });
            bool main = false;
            if (db.Offers.Where(x => x.id == offer_id).First().Photo.Count() == 0) main = true;
            if(file != null && file.ContentLength/1024/1024<10)
            {
                string pic = Guid.NewGuid() + Path.GetExtension(file.FileName);
                string dir = Server.MapPath("~/images/");
                string path = Path.Combine(dir, pic);
                //if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                file.SaveAs(path);
                using (MemoryStream ms = new MemoryStream())
                {
                    file.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();
                }
                var e = db.Photos.Add(new Photo { offerid = offer_id, url = pic, ismain = main });
                db.SaveChanges();
            }
            return RedirectToAction("Offer", "Home", new { id = offer_id });
        }
        [Authorize]
        public ActionResult DeletePhoto(int id)
        {
            var userid = User.Identity.GetUserId();
            if (db.Photos.Where(x => x.id == id).First().Offer.Company.userid != userid) return RedirectToAction("Index", "Home");
            var offer_id = db.Photos.Where(x => x.id == id).First().Offer.id;
            Photo photo = db.Photos.Find(id);
            string pic = photo.url;
            db.Photos.Remove(photo);
            db.SaveChanges();
            string dir = Server.MapPath("~/images/");
            string path = Path.Combine(dir, pic);
            System.IO.File.Delete(path);
            return RedirectToAction("Offer", "Home", new { id = offer_id });
        }
        [Authorize]
        public ActionResult Subscriptions()
        {
            var userid = User.Identity.GetUserId();
            ViewBag.userid = userid;
            if (db.Users.Where(x => x.userid == userid).Count() != 1) return RedirectToAction("Index", "Home");
            return View();
        }
        [Authorize]
        public ActionResult AddSubscriptionCompany(int company_id)
        {
            var userid = User.Identity.GetUserId();
            if (db.Users.Where(x => x.userid == userid).Count() != 1) return RedirectToAction("Index", "Home");
            var e = db.SubscribeCompanys.Add(new SubscribeCompany { userid = db.Users.Where(x => x.userid == userid).First().id, companyid = company_id });
            db.SaveChanges();
            return RedirectToAction("Company", "Home", new { id = company_id });
        }
        [Authorize]
        public ActionResult DeleteSubscriptionCompany(int id)
        {
            var userid = User.Identity.GetUserId();
            var sub = db.SubscribeCompanys.Where(x => x.id == id).First();
            if (sub.User.userid != userid) return RedirectToAction("Index", "Home");
            var companyid = sub.companyid;
            db.SubscribeCompanys.Remove(sub);
            db.SaveChanges();
            return RedirectToAction("Company", "Home", new { id = companyid });
        }
        //[Authorize]
        //public ActionResult AddSubscriptionOffer()
        //{
        //    var userid = User.Identity.GetUserId();
        //    ViewBag.userid = userid;
        //    ViewBag.addoffer = true;
        //    if (db.Users.Where(x => x.userid == userid).Count() != 1) return RedirectToAction("Index", "Home");
        //    var uid = db.Users.Where(x => x.userid == userid).First().id;
        //    if (db.SubscribeOffers.Where(x => x.userid == uid).Count() >= 20) return RedirectToAction("Subscriptions", "Home");
        //    return View();
        //}
        //[Authorize]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult AddSubscriptionOffer(SubscribeOffer subscribeoffer, string category, string subcategory, string lat, string lon, int smondayfromh = 0, int smondayfromm = 0, int smondaytoh = 23, int smondaytom = 59, int stuesdayfromh = 0, int stuesdayfromm = 0, int stuesdaytoh = 23, int stuesdaytom = 59, int swednesdayfromh = 0, int swednesdayfromm = 0, int swednesdaytoh = 23, int swednesdaytom = 59, int sthursdayfromh = 0, int sthursdayfromm = 0, int sthursdaytoh = 23, int sthursdaytom = 59, int sfridayfromh = 0, int sfridayfromm = 0, int sfridaytoh = 23, int sfridaytom = 59, int ssaturdayfromh = 0, int ssaturdayfromm = 0, int ssaturdaytoh = 23, int ssaturdaytom = 59, int ssundayfromh = 0, int ssundayfromm = 0, int ssundaytoh = 23, int ssundaytom = 59)
        //{
        //    var userid = User.Identity.GetUserId();
        //    ViewBag.userid = userid;
        //    if (db.Users.Where(x => x.userid == userid).Count() != 1) return RedirectToAction("Index", "Home");
        //    var uid = db.Users.Where(x => x.userid == userid).First().id;
        //    if (db.SubscribeOffers.Where(x => x.userid == uid).Count() >= 20) return RedirectToAction("Subscriptions", "Home");
        //    bool validdates = true;
        //    if (subscribeoffer.smonday)
        //    {
        //        try
        //        {
        //            subscribeoffer.smondayfrom = new DateTime().AddYears(1753).AddHours(smondayfromh).AddMinutes(smondayfromm);
        //            subscribeoffer.smondayto = new DateTime().AddYears(1753).AddHours(smondaytoh).AddMinutes(smondaytom);
        //            if (subscribeoffer.smondayfrom > subscribeoffer.smondayto) { var temp = subscribeoffer.smondayfrom; subscribeoffer.smondayfrom = subscribeoffer.smondayto; subscribeoffer.smondayto = temp; }

        //        }
        //        catch { validdates = false; }
        //    }
        //    if (subscribeoffer.stuesday)
        //    {
        //        try
        //        {
        //            subscribeoffer.stuesdayfrom = new DateTime().AddYears(1753).AddHours(stuesdayfromh).AddMinutes(stuesdayfromm);
        //            subscribeoffer.stuesdayto = new DateTime().AddYears(1753).AddHours(stuesdaytoh).AddMinutes(stuesdaytom);
        //            if (subscribeoffer.stuesdayfrom > subscribeoffer.stuesdayto) { var temp = subscribeoffer.stuesdayfrom; subscribeoffer.stuesdayfrom = subscribeoffer.stuesdayto; subscribeoffer.stuesdayto = temp; }

        //        }
        //        catch { validdates = false; }
        //    }
        //    if (subscribeoffer.swednesday)
        //    {
        //        try
        //        {
        //            subscribeoffer.swednesdayfrom = new DateTime().AddYears(1753).AddHours(swednesdayfromh).AddMinutes(swednesdayfromm);
        //            subscribeoffer.swednesdayto = new DateTime().AddYears(1753).AddHours(swednesdaytoh).AddMinutes(swednesdaytom);
        //            if (subscribeoffer.swednesdayfrom > subscribeoffer.swednesdayto) { var temp = subscribeoffer.swednesdayfrom; subscribeoffer.swednesdayfrom = subscribeoffer.swednesdayto; subscribeoffer.swednesdayto = temp; }

        //        }
        //        catch { validdates = false; }
        //    }
        //    if (subscribeoffer.sthursday)
        //    {
        //        try
        //        {
        //            subscribeoffer.sthursdayfrom = new DateTime().AddYears(1753).AddHours(sthursdayfromh).AddMinutes(sthursdayfromm);
        //            subscribeoffer.sthursdayto = new DateTime().AddYears(1753).AddHours(sthursdaytoh).AddMinutes(sthursdaytom);
        //            if (subscribeoffer.sthursdayfrom > subscribeoffer.sthursdayto) { var temp = subscribeoffer.sthursdayfrom; subscribeoffer.sthursdayfrom = subscribeoffer.sthursdayto; subscribeoffer.sthursdayto = temp; }

        //        }
        //        catch { validdates = false; }
        //    }
        //    if (subscribeoffer.sfriday)
        //    {
        //        try
        //        {
        //            subscribeoffer.sfridayfrom = new DateTime().AddYears(1753).AddHours(sfridayfromh).AddMinutes(sfridayfromm);
        //            subscribeoffer.sfridayto = new DateTime().AddYears(1753).AddHours(sfridaytoh).AddMinutes(sfridaytom);
        //            if (subscribeoffer.sfridayfrom > subscribeoffer.sfridayto) { var temp = subscribeoffer.sfridayfrom; subscribeoffer.sfridayfrom = subscribeoffer.sfridayto; subscribeoffer.sfridayto = temp; }

        //        }
        //        catch { validdates = false; }
        //    }
        //    if (subscribeoffer.ssaturday)
        //    {
        //        try
        //        {
        //            subscribeoffer.ssaturdayfrom = new DateTime().AddYears(1753).AddHours(ssaturdayfromh).AddMinutes(ssaturdayfromm);
        //            subscribeoffer.ssaturdayto = new DateTime().AddYears(1753).AddHours(ssaturdaytoh).AddMinutes(ssaturdaytom);
        //            if (subscribeoffer.ssaturdayfrom > subscribeoffer.ssaturdayto) { var temp = subscribeoffer.ssaturdayfrom; subscribeoffer.ssaturdayfrom = subscribeoffer.ssaturdayto; subscribeoffer.ssaturdayto = temp; }

        //        }
        //        catch { validdates = false; }
        //    }
        //    if (subscribeoffer.ssunday)
        //    {
        //        try
        //        {
        //            subscribeoffer.ssundayfrom = new DateTime().AddYears(1753).AddHours(ssundayfromh).AddMinutes(ssundayfromm);
        //            subscribeoffer.ssundayto = new DateTime().AddYears(1753).AddHours(ssundaytoh).AddMinutes(ssundaytom);
        //            if (subscribeoffer.ssundayfrom > subscribeoffer.ssundayto) { var temp = subscribeoffer.ssundayfrom; subscribeoffer.ssundayfrom = subscribeoffer.ssundayto; subscribeoffer.ssundayto = temp; }

        //        }
        //        catch { validdates = false; }
        //    }
        //    if (validdates && ModelState.IsValid && !((category == "" || category == null) && (subcategory == "" || subcategory == null) && (lat == "" || lat == null) && (lon == "" || lon == null) && (subscribeoffer.age == null)) && !(subscribeoffer.smonday == false && subscribeoffer.stuesday == false && subscribeoffer.swednesday == false && subscribeoffer.sthursday == false && subscribeoffer.sfriday == false && subscribeoffer.ssaturday == false && subscribeoffer.ssunday == false))
        //    {
        //        subscribeoffer.address = StripHTML(subscribeoffer.address);
        //        if (lat != null && lat != null) subscribeoffer.lat = float.Parse(lat, CultureInfo.InvariantCulture.NumberFormat);
        //        if (lon != null && lon != null) subscribeoffer.lon = float.Parse(lon, CultureInfo.InvariantCulture.NumberFormat);
        //        var o = db.SubscribeOffers.Add(new SubscribeOffer { userid = uid, categoryid = category, subcategoryid = subcategory, address = subscribeoffer.address, range = subscribeoffer.range, lat = subscribeoffer.lat, lon = subscribeoffer.lon, age = subscribeoffer.age, individual = subscribeoffer.individual, firstfree = subscribeoffer.firstfree, drive = subscribeoffer.drive, online = subscribeoffer.online, smonday = subscribeoffer.smonday, smondayfrom = subscribeoffer.smondayfrom, smondayto = subscribeoffer.smondayto, stuesday = subscribeoffer.stuesday, stuesdayfrom = subscribeoffer.stuesdayfrom, stuesdayto = subscribeoffer.stuesdayto, swednesday = subscribeoffer.swednesday, swednesdayfrom = subscribeoffer.swednesdayfrom, swednesdayto = subscribeoffer.swednesdayto, sthursday = subscribeoffer.sthursday, sthursdayfrom = subscribeoffer.sthursdayfrom, sthursdayto = subscribeoffer.sthursdayto, sfriday = subscribeoffer.sfriday, sfridayfrom = subscribeoffer.sfridayfrom, sfridayto = subscribeoffer.sfridayto, ssaturday = subscribeoffer.ssaturday, ssaturdayfrom = subscribeoffer.ssaturdayfrom, ssaturdayto = subscribeoffer.ssaturdayto, ssunday = subscribeoffer.ssunday, ssundayfrom = subscribeoffer.ssundayfrom, ssundayto = subscribeoffer.ssundayto });
        //        db.SaveChanges();
        //        return RedirectToAction("Subscriptions", "Home");
        //    }
        //    else
        //    {
        //        ViewBag.addoffer = true;
        //        ViewBag.lat = lat;
        //        ViewBag.lat = lon;
        //        ViewBag.smondayfromh = smondayfromh; ViewBag.smondayfromm = smondayfromm; ViewBag.smondaytoh = smondaytoh; ViewBag.smondaytom = smondaytom;
        //        ViewBag.stuesdayfromh = stuesdayfromh; ViewBag.stuesdayfromm = stuesdayfromm; ViewBag.stuesdaytoh = stuesdaytoh; ViewBag.stuesdaytom = stuesdaytom;
        //        ViewBag.swednesdayfromh = swednesdayfromh; ViewBag.swednesdayfromm = swednesdayfromm; ViewBag.swednesdaytoh = swednesdaytoh; ViewBag.swednesdaytom = swednesdaytom;
        //        ViewBag.sthursdayfromh = sthursdayfromh; ViewBag.sthursdayfromm = sthursdayfromm; ViewBag.sthursdaytoh = sthursdaytoh; ViewBag.sthursdaytom = sthursdaytom;
        //        ViewBag.sfridayfromh = sfridayfromh; ViewBag.sfridayfromm = sfridayfromm; ViewBag.sfridaytoh = sfridaytoh; ViewBag.sfridaytom = sfridaytom;
        //        ViewBag.ssaturdayfromh = ssaturdayfromh; ViewBag.ssaturdayfromm = ssaturdayfromm; ViewBag.ssaturdaytoh = ssaturdaytoh; ViewBag.ssaturdaytom = ssaturdaytom;
        //        ViewBag.ssundayfromh = ssundayfromh; ViewBag.ssundayfromm = ssundayfromm; ViewBag.ssundaytoh = ssundaytoh; ViewBag.ssundaytom = ssundaytom;
        //        return View(subscribeoffer);
        //    }
        //}
        //[Authorize]
        //public ActionResult EditSubscriptionOffer(int id)
        //{
        //    var sub = db.SubscribeOffers.Where(x => x.id == id).First();
        //    var userid = User.Identity.GetUserId();
        //    //ViewBag.userid = userid;
        //    ViewBag.addoffer = true;
        //    if (sub.User.userid != userid) return RedirectToAction("Index", "Home");
        //    return View(sub);
        //}
        //[Authorize]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult EditSubscriptionOffer(int id, SubscribeOffer subscribeoffer, string category, string subcategory, string lat, string lon, int smondayfromh = 0, int smondayfromm = 0, int smondaytoh = 23, int smondaytom = 59, int stuesdayfromh = 0, int stuesdayfromm = 0, int stuesdaytoh = 23, int stuesdaytom = 59, int swednesdayfromh = 0, int swednesdayfromm = 0, int swednesdaytoh = 23, int swednesdaytom = 59, int sthursdayfromh = 0, int sthursdayfromm = 0, int sthursdaytoh = 23, int sthursdaytom = 59, int sfridayfromh = 0, int sfridayfromm = 0, int sfridaytoh = 23, int sfridaytom = 59, int ssaturdayfromh = 0, int ssaturdayfromm = 0, int ssaturdaytoh = 23, int ssaturdaytom = 59, int ssundayfromh = 0, int ssundayfromm = 0, int ssundaytoh = 23, int ssundaytom = 59)
        //{
        //    var sub = db.SubscribeOffers.Where(x => x.id == id).First();
        //    var userid = User.Identity.GetUserId();
        //    //ViewBag.userid = userid;
        //    ViewBag.addoffer = true;
        //    if (sub.User.userid != userid) return RedirectToAction("Index", "Home");
        //    var uid = db.Users.Where(x => x.userid == userid).First().id;
        //    if (db.SubscribeOffers.Where(x => x.userid == uid).Count() >= 20) return RedirectToAction("Subscriptions", "Home");
        //    bool validdates = true;
        //    if (subscribeoffer.smonday)
        //    {
        //        try
        //        {
        //            subscribeoffer.smondayfrom = new DateTime().AddYears(1753).AddHours(smondayfromh).AddMinutes(smondayfromm);
        //            subscribeoffer.smondayto = new DateTime().AddYears(1753).AddHours(smondaytoh).AddMinutes(smondaytom);
        //            if (subscribeoffer.smondayfrom > subscribeoffer.smondayto) { var temp = subscribeoffer.smondayfrom; subscribeoffer.smondayfrom = subscribeoffer.smondayto; subscribeoffer.smondayto = temp; }

        //        }
        //        catch { validdates = false; }
        //    }
        //    if (subscribeoffer.stuesday)
        //    {
        //        try
        //        {
        //            subscribeoffer.stuesdayfrom = new DateTime().AddYears(1753).AddHours(stuesdayfromh).AddMinutes(stuesdayfromm);
        //            subscribeoffer.stuesdayto = new DateTime().AddYears(1753).AddHours(stuesdaytoh).AddMinutes(stuesdaytom);
        //            if (subscribeoffer.stuesdayfrom > subscribeoffer.stuesdayto) { var temp = subscribeoffer.stuesdayfrom; subscribeoffer.stuesdayfrom = subscribeoffer.stuesdayto; subscribeoffer.stuesdayto = temp; }

        //        }
        //        catch { validdates = false; }
        //    }
        //    if (subscribeoffer.swednesday)
        //    {
        //        try
        //        {
        //            subscribeoffer.swednesdayfrom = new DateTime().AddYears(1753).AddHours(swednesdayfromh).AddMinutes(swednesdayfromm);
        //            subscribeoffer.swednesdayto = new DateTime().AddYears(1753).AddHours(swednesdaytoh).AddMinutes(swednesdaytom);
        //            if (subscribeoffer.swednesdayfrom > subscribeoffer.swednesdayto) { var temp = subscribeoffer.swednesdayfrom; subscribeoffer.swednesdayfrom = subscribeoffer.swednesdayto; subscribeoffer.swednesdayto = temp; }

        //        }
        //        catch { validdates = false; }
        //    }
        //    if (subscribeoffer.sthursday)
        //    {
        //        try
        //        {
        //            subscribeoffer.sthursdayfrom = new DateTime().AddYears(1753).AddHours(sthursdayfromh).AddMinutes(sthursdayfromm);
        //            subscribeoffer.sthursdayto = new DateTime().AddYears(1753).AddHours(sthursdaytoh).AddMinutes(sthursdaytom);
        //            if (subscribeoffer.sthursdayfrom > subscribeoffer.sthursdayto) { var temp = subscribeoffer.sthursdayfrom; subscribeoffer.sthursdayfrom = subscribeoffer.sthursdayto; subscribeoffer.sthursdayto = temp; }

        //        }
        //        catch { validdates = false; }
        //    }
        //    if (subscribeoffer.sfriday)
        //    {
        //        try
        //        {
        //            subscribeoffer.sfridayfrom = new DateTime().AddYears(1753).AddHours(sfridayfromh).AddMinutes(sfridayfromm);
        //            subscribeoffer.sfridayto = new DateTime().AddYears(1753).AddHours(sfridaytoh).AddMinutes(sfridaytom);
        //            if (subscribeoffer.sfridayfrom > subscribeoffer.sfridayto) { var temp = subscribeoffer.sfridayfrom; subscribeoffer.sfridayfrom = subscribeoffer.sfridayto; subscribeoffer.sfridayto = temp; }

        //        }
        //        catch { validdates = false; }
        //    }
        //    if (subscribeoffer.ssaturday)
        //    {
        //        try
        //        {
        //            subscribeoffer.ssaturdayfrom = new DateTime().AddYears(1753).AddHours(ssaturdayfromh).AddMinutes(ssaturdayfromm);
        //            subscribeoffer.ssaturdayto = new DateTime().AddYears(1753).AddHours(ssaturdaytoh).AddMinutes(ssaturdaytom);
        //            if (subscribeoffer.ssaturdayfrom > subscribeoffer.ssaturdayto) { var temp = subscribeoffer.ssaturdayfrom; subscribeoffer.ssaturdayfrom = subscribeoffer.ssaturdayto; subscribeoffer.ssaturdayto = temp; }

        //        }
        //        catch { validdates = false; }
        //    }
        //    if (subscribeoffer.ssunday)
        //    {
        //        try
        //        {
        //            subscribeoffer.ssundayfrom = new DateTime().AddYears(1753).AddHours(ssundayfromh).AddMinutes(ssundayfromm);
        //            subscribeoffer.ssundayto = new DateTime().AddYears(1753).AddHours(ssundaytoh).AddMinutes(ssundaytom);
        //            if (subscribeoffer.ssundayfrom > subscribeoffer.ssundayto) { var temp = subscribeoffer.ssundayfrom; subscribeoffer.ssundayfrom = subscribeoffer.ssundayto; subscribeoffer.ssundayto = temp; }

        //        }
        //        catch { validdates = false; }
        //    }
        //    if (validdates && ModelState.IsValid && !((category == "" || category == null) && (subcategory == "" || subcategory == null) && (lat == "" || lat == null) && (lon == "" || lon == null) && (subscribeoffer.age == null)) && !(subscribeoffer.smonday == false && subscribeoffer.stuesday == false && subscribeoffer.swednesday == false && subscribeoffer.sthursday == false && subscribeoffer.sfriday == false && subscribeoffer.ssaturday == false && subscribeoffer.ssunday == false))
        //    {
        //        subscribeoffer.address = StripHTML(subscribeoffer.address);
        //        if (lat != null && lat != null) subscribeoffer.lat = float.Parse(lat, CultureInfo.InvariantCulture.NumberFormat);
        //        if (lon != null && lon != null) subscribeoffer.lon = float.Parse(lon, CultureInfo.InvariantCulture.NumberFormat);
        //        db.Entry(subscribeoffer).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Subscriptions", "Home");
        //    }
        //    else
        //    {
        //        ViewBag.addoffer = true;
        //        ViewBag.lat = lat;
        //        ViewBag.lat = lon;
        //        ViewBag.smondayfromh = smondayfromh; ViewBag.smondayfromm = smondayfromm; ViewBag.smondaytoh = smondaytoh; ViewBag.smondaytom = smondaytom;
        //        ViewBag.stuesdayfromh = stuesdayfromh; ViewBag.stuesdayfromm = stuesdayfromm; ViewBag.stuesdaytoh = stuesdaytoh; ViewBag.stuesdaytom = stuesdaytom;
        //        ViewBag.swednesdayfromh = swednesdayfromh; ViewBag.swednesdayfromm = swednesdayfromm; ViewBag.swednesdaytoh = swednesdaytoh; ViewBag.swednesdaytom = swednesdaytom;
        //        ViewBag.sthursdayfromh = sthursdayfromh; ViewBag.sthursdayfromm = sthursdayfromm; ViewBag.sthursdaytoh = sthursdaytoh; ViewBag.sthursdaytom = sthursdaytom;
        //        ViewBag.sfridayfromh = sfridayfromh; ViewBag.sfridayfromm = sfridayfromm; ViewBag.sfridaytoh = sfridaytoh; ViewBag.sfridaytom = sfridaytom;
        //        ViewBag.ssaturdayfromh = ssaturdayfromh; ViewBag.ssaturdayfromm = ssaturdayfromm; ViewBag.ssaturdaytoh = ssaturdaytoh; ViewBag.ssaturdaytom = ssaturdaytom;
        //        ViewBag.ssundayfromh = ssundayfromh; ViewBag.ssundayfromm = ssundayfromm; ViewBag.ssundaytoh = ssundaytoh; ViewBag.ssundaytom = ssundaytom;
        //        return View(subscribeoffer);
        //    }
        //}
        //[Authorize]
        //public ActionResult DeleteSubscriptionOffer(int id)
        //{
        //    var sub = db.SubscribeOffers.Where(x => x.id == id).First();
        //    var userid = User.Identity.GetUserId();
        //    if (sub.User.userid != userid) return RedirectToAction("Index", "Home");
        //    db.SubscribeOffers.Remove(sub);
        //    db.SaveChanges();
        //    return RedirectToAction("Subscriptions", "Home");
        //}
        [Authorize]
        public ActionResult Comments()
        {
            var userid = User.Identity.GetUserId();
            ViewBag.userid = userid;
            if (db.Users.Where(x => x.userid == userid).Count() != 1) return RedirectToAction("Index", "Home");
            return View(db.Users.Where(x => x.userid == userid).First().Comment.ToList());
        }
        [Authorize]
        public ActionResult AddComment(int offerid)
        {
            var userid = User.Identity.GetUserId();
            ViewBag.userid = userid;
            if (db.Users.Where(x => x.userid == userid).Count() != 1) return RedirectToAction("Index", "Home");
            var uid = db.Users.Where(x => x.userid == userid).First().id;
            if (db.Comments.Where(x => x.offerid == offerid && x.userid == uid).Count() > 0) return RedirectToAction("Offer", "Home", new { id = offerid });
            if (db.Users.Where(x => x.userid == userid).First().name == null || db.Users.Where(x => x.userid == userid).First().name == "")
            {
                return RedirectToAction("EditUser", "Home");
            }
            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddComment(int offerid, Comment commodel)
        {
            var userid = User.Identity.GetUserId();
            ViewBag.userid = userid;
            if (db.Users.Where(x => x.userid == userid).Count() != 1) return RedirectToAction("Index", "Home");
            var uid = db.Users.Where(x => x.userid == userid).First().id;
            if (db.Comments.Where(x => x.offerid == offerid && x.userid == uid).Count() > 0) return RedirectToAction("Offer", "Home", new { id = offerid });
            if (ModelState.IsValid)
            {
                commodel.title = StripHTML(commodel.title);
                commodel.name = StripHTML(commodel.name);
                commodel.comment = StripHTML(commodel.comment);
                var o = db.Comments.Add(new Comment { userid = uid, offerid = offerid, created = DateTime.Now, title = commodel.title, comment = commodel.comment, name = commodel.name, rate = commodel.rate });
                db.SaveChanges();
                
                var cn = db.CompanyNotifications.Add(new CompanyNotification { companyid = db.Offers.Where(x => x.id == offerid).First().Company.id, created = DateTime.Now, title = "Użytkownik <strong>" + commodel.name + "</strong> dodał opinię na temat oferty <strong>" + db.Offers.Where(x => x.id == offerid).First().title + "</strong>.", offer = offerid, check = false });
                db.SaveChanges();

                return RedirectToAction("Offer", "Home", new { id = offerid });
            }
            else
            {
                return View(commodel);
            }
        }
        [Authorize]
        public ActionResult EditComment(int id)
        {
            var userid = User.Identity.GetUserId();
            //ViewBag.userid = userid;
            var com = db.Comments.Where(x => x.id == id).First();
            if (com.User.userid != userid) return RedirectToAction("Index", "Home");
            return View(com);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditComment(int id, Comment commodel)
        {
            var userid = User.Identity.GetUserId();
            //ViewBag.userid = userid;
            var com = db.Comments.Where(x => x.id == id).First();
            if (com.User.userid != userid) return RedirectToAction("Index", "Home");
            if (ModelState.IsValid)
            {
                commodel.title = StripHTML(commodel.title);
                commodel.name = StripHTML(commodel.name);
                commodel.comment = StripHTML(commodel.comment);
                //db.Entry(commodel).State = EntityState.Modified;
                db.Set<Comment>().AddOrUpdate(commodel);
                db.SaveChanges();
                return RedirectToAction("Comments", "Home");
            }
            else
            {
                return View(commodel);
            }
        }
        [Authorize]
        public ActionResult DeleteComment(int id)
        {
            var userid = User.Identity.GetUserId();
            //ViewBag.userid = userid;
            var com = db.Comments.Where(x => x.id == id).First();
            if (com.User.userid != userid) return RedirectToAction("Index", "Home");
            db.Comments.Remove(com);
            db.SaveChanges();
            return RedirectToAction("Comments", "Home");
        }
        [Authorize]
        public ActionResult AddWatching(int offer_id)
        {
            var userid = User.Identity.GetUserId();
            if (db.Users.Where(x => x.userid == userid).Count() != 1) return RedirectToAction("Index", "Home");
            var e = db.Watchings.Add(new Watching { userid = db.Users.Where(x => x.userid == userid).First().id, offerid = offer_id });
            db.SaveChanges();
            return RedirectToAction("Offer", "Home", new { id = offer_id });
        }
        [Authorize]
        public ActionResult DeleteWatching(int id)
        {
            var userid = User.Identity.GetUserId();
            var com = db.Watchings.Where(x => x.id == id).First();
            if (com.User.userid != userid) return RedirectToAction("Index", "Home");
            var offerid = com.offerid;
            db.Watchings.Remove(com);
            db.SaveChanges();
            return RedirectToAction("Offer", "Home", new { id = offerid });
        }
        public ActionResult Test()
        {
            ViewBag.categories = db.Categorys.Select(x => new SelectListItem { Value = x.id.ToString(), Text = x.name });
            return View();
        }
        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }
        public ActionResult TermsOfUse()
        {
            return View();
        }
        public ActionResult PrivacyPolicy()
        {
            return View();
        }
        public void Checked()
        {
            var userid = User.Identity.GetUserId();
            if (db.Companys.Where(x => x.userid == userid).Count()>0)
            {
                foreach(var c in db.Companys.Where(x => x.userid == userid).First().CompanyNotification.Where(x => !x.check).ToList())
                {
                    c.check = true;
                    db.Set<CompanyNotification>().AddOrUpdate(c);
                    db.SaveChanges();
                }
            }
            if (db.Users.Where(x => x.userid == userid).Count() > 0)
            {
                foreach (var c in db.Users.Where(x => x.userid == userid).First().UserNotification.Where(x => !x.check).ToList())
                {
                    c.check = true;
                    db.Set<UserNotification>().AddOrUpdate(c);
                    db.SaveChanges();
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public string Phone(bool isoffer, int id)
        {
            if (isoffer) return "Telefon: <a href=\"tel:" + db.Offers.Find(id).phone + "\">" + db.Offers.Find(id).phone + "</a>";
            else return "Telefon: <a href=\"tel:" + db.Companys.Find(id).phone + "\">" + db.Companys.Find(id).phone + "</a>";
        }
    }
}