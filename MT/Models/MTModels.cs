using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Web.Mvc;

namespace MT.Models
{
    public class EmailFormModel
    {
        [Required, Display(Name = "Imię")]
        public string FromName { get; set; }
        [Required, Display(Name = "Twój email"), EmailAddress]
        public string FromEmail { get; set; }
        [Required, Display(Name = "Temat")]
        public string Subject { get; set; }
        [Required, Display(Name = "Wiadomość")]
        public string Message { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
    }
    public class Company
    {
        [Key]
        public int id { get; set; }
        public string userid { get; set; }
        public DateTime created { get; set; }
        public string logo { get; set; }
        [Display(Name = "Nazwa")]
        public string name { get; set; }
        [AllowHtml]
        [Display(Name = "Opis")]
        public string description { get; set; }
        [Display(Name = "Adres")]
        public string address { get; set; }
        //public string city { get; set; }
        public float? lat { get; set; }
        public float? lon { get; set; }
        [Display(Name = "Telefon")]
        public string phone { get; set; }
        [Display(Name = "Email")]
        public string email { get; set; }
        public virtual ICollection<Offer> Offer { get; set; }
        public virtual ICollection<CompanyNotification> CompanyNotification { get; set; }
        public virtual ICollection<SubscribeCompany> SubscribeCompany { get; set; }
    }

    public class User
    {
        [Key]
        public int id { get; set; }
        public string userid { get; set; }
        public DateTime created { get; set; }
        public string photo { get; set; }
        [Display(Name = "Imię")]
        public string name { get; set; }
        public virtual ICollection<UserNotification> UserNotification { get; set; }
        public virtual ICollection<Watching> Watching { get; set; }
        public virtual ICollection<SubscribeCompany> SubscribeCompany { get; set; }
        public virtual ICollection<SubscribeOffer> SubscribeOffer { get; set; }
        public virtual ICollection<Comment> Comment { get; set; }
    }

    public class Offer
    {
        [Key]
        public int id { get; set; }
        [ForeignKey("Company")]
        public int companyid { get; set; }
        [Required]
        [Display(Name = "Podkategoria")]
        [ForeignKey("Subcategory")]
        public int subcategoryid { get; set; }
        public DateTime created { get; set; }
        [Required]
        [Display(Name = "Tytuł")]
        public string title { get; set; }
        [Required]
        [AllowHtml]
        [Display(Name = "Opis")]
        public string description { get; set; }
        [DataType(DataType.Currency)]
        [Display(Name = "Cena")]
        public float? price { get; set; }
        [Display(Name = "Okres")]
        public string period { get; set; }
        [Display(Name = "Zajęcia odbywają się w siedzibie firmy")]
        public bool iscompanyaddress { get; set; }
        [Display(Name = "Nazwa obiektu")]
        public string objectname { get; set; }
        [Required]
        [Display(Name = "Adres")]
        public string address { get; set; }
        //public string city { get; set; }
        [Required]
        public float lat { get; set; }
        [Required]
        public float lon { get; set; }
        [Display(Name = "Telefon")]
        public string phone { get; set; }
        [Display(Name = "Email")]
        public string email { get; set; }
        [Required]
        [Display(Name = "Wiek od")]
        public int agefrom { get; set; }
        [Required]
        [Display(Name = "Wiek do")]
        public int ageto { get; set; }
        [Display(Name = "Tylko zajęcia indywidualne")]
        public bool individual { get; set; }
        [Display(Name = "Pierwsze zajęcia darmowe")]
        public bool firstfree { get; set; }
        [Display(Name = "Tylko z dojazdem na zajęcia do klienta")]
        public bool drive { get; set; }
        [Display(Name = "Tylko zajęcia prowadzone online")]
        public bool online { get; set; }
        public bool sponsored { get; set; }
        public bool publicated { get; set; }
        public int? match { get; set; }
        public virtual ICollection<EventDate> EventDate { get; set; }
        public virtual ICollection<Photo> Photo { get; set; }
        public virtual ICollection<Watching> Watching { get; set; }
        public virtual ICollection<Comment> Comment { get; set; }
        public virtual Company Company { get; set; }
        public virtual Subcategory Subcategory { get; set; }

    }

    public class EventDate
    {
        [Key]
        public int id { get; set; }
        [ForeignKey("Offer")]
        public int offerid { get; set; }
        [Required]
        [Display(Name = "Dzień")]
        public int day { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:hh:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Od")]
        public DateTime from { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:hh:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Do")]
        public DateTime to { get; set; }
        public virtual Offer Offer { get; set; }
    }
    public class Photo
    {
        [Key]
        public int id { get; set; }
        [ForeignKey("Offer")]
        public int offerid { get; set; }
        public string url { get; set; }
        public bool ismain { get; set; }
        public virtual Offer Offer { get; set; }
    }

    public class Category
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        //public virtual ICollection<Offer> Offer { get; set; }
        public virtual ICollection<Subcategory> Subcategory { get; set; }
    }

    public class Subcategory
    {
        [Key]
        public int id { get; set; }
        [ForeignKey("Category")]
        public int categoryid { get; set; }
        public string name { get; set; }
        public virtual ICollection<Offer> Offer { get; set; }
        public virtual Category Category { get; set; }
    }

    public class UserNotification
    {
        [Key]
        public int id { get; set; }
        [ForeignKey("User")]
        public int userid { get; set; }
        public DateTime created { get; set; }
        [AllowHtml]
        public string title { get; set; }
        public int? offer { get; set; }
        public bool check { get; set; }
        public virtual User User { get; set; }
    }

    public class CompanyNotification
    {
        [Key]
        public int id { get; set; }
        [ForeignKey("Company")]
        public int companyid { get; set; }
        public DateTime created { get; set; }
        [AllowHtml]
        public string title { get; set; }
        public int? offer { get; set; }
        public bool check { get; set; }
        public virtual Company Company { get; set; }
    }

    public class Watching
    {
        [Key]
        public int id { get; set; }
        [ForeignKey("User")]
        public int userid { get; set; }
        [ForeignKey("Offer")]
        public int offerid { get; set; }
        public virtual User User { get; set; }
        public virtual Offer Offer { get; set; }
    }

    public class SubscribeCompany
    {
        [Key]
        public int id { get; set; }
        [ForeignKey("User")]
        public int userid { get; set; }
        [ForeignKey("Company")]
        public int companyid { get; set; }
        public virtual User User { get; set; }
        public virtual Company Company { get; set; }
    }

    public class Comment
    {
        [Key]
        public int id { get; set; }
        [ForeignKey("User")]
        public int userid { get; set; }
        [ForeignKey("Offer")]
        public int offerid { get; set; }
        public DateTime created { get; set; }
        [Display(Name = "Tytuł")]
        [AllowHtml]
        public string title { get; set; }
        [Required]
        [Display(Name = "Komentarz")]
        public string comment { get; set; }
        [Required]
        [Display(Name = "Podpis")]
        public string name { get; set; }
        [Required]
        [Display(Name = "Ocena")]
        public int rate { get; set; }
        public virtual User User { get; set; }
        public virtual Offer Offer { get; set; }
    }

    public class SubscribeOffer
    {
        [Key]
        public int id { get; set; }
        [ForeignKey("User")]
        public int userid { get; set; }
        [Display(Name = "Kategoria")]
        public string categoryid { get; set; }
        [Display(Name = "Podkategoria")]
        public string subcategoryid { get; set; }
        [Display(Name = "Adres")]
        public string address { get; set; }
        [Display(Name = "Odległość")]
        public int? range { get; set; }
        public float? lat { get; set; }
        public float? lon { get; set; }
        [Display(Name = "Wiek")]
        public int? age { get; set; }
        [Display(Name = "Tylko zajęcia indywidualne")]
        public bool individual { get; set; }
        [Display(Name = "Pierwsze zajęcia darmowe")]
        public bool firstfree { get; set; }
        [Display(Name = "Tylko z dojazdem na zajęcia do klienta")]
        public bool drive { get; set; }
        [Display(Name = "Tylko zajęcia prowadzone online")]
        public bool online { get; set; }
        public bool smonday { get; set; }
        public DateTime? smondayfrom { get; set; }
        public DateTime? smondayto { get; set; }
        public bool stuesday { get; set; }
        public DateTime? stuesdayfrom { get; set; }
        public DateTime? stuesdayto { get; set; }
        public bool swednesday { get; set; }
        public DateTime? swednesdayfrom { get; set; }
        public DateTime? swednesdayto { get; set; }
        public bool sthursday { get; set; }
        public DateTime? sthursdayfrom { get; set; }
        public DateTime? sthursdayto { get; set; }
        public bool sfriday { get; set; }
        public DateTime? sfridayfrom { get; set; }
        public DateTime? sfridayto { get; set; }
        public bool ssaturday { get; set; }
        public DateTime? ssaturdayfrom { get; set; }
        public DateTime? ssaturdayto { get; set; }
        public bool ssunday { get; set; }
        public DateTime? ssundayfrom { get; set; }
        public DateTime? ssundayto { get; set; }
        public virtual User User { get; set; }
    }

    public class MTModelsDBContext : DbContext
    {
        public DbSet<Company> Companys { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<EventDate> EventDates { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Category> Categorys { get; set; }
        public DbSet<Subcategory> Subcategorys { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<CompanyNotification> CompanyNotifications { get; set; }
        public DbSet<Watching> Watchings { get; set; }
        public DbSet<SubscribeCompany> SubscribeCompanys { get; set; }
        public DbSet<SubscribeOffer> SubscribeOffers { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }
}