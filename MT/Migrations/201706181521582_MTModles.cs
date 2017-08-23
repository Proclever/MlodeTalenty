namespace MT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MTModles : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Subcategories",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        categoryid = c.Int(nullable: false),
                        name = c.String(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Categories", t => t.categoryid, cascadeDelete: true)
                .Index(t => t.categoryid);
            
            CreateTable(
                "dbo.Offers",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        companyid = c.Int(nullable: false),
                        subcategoryid = c.Int(nullable: false),
                        created = c.DateTime(nullable: false),
                        title = c.String(nullable: false),
                        description = c.String(nullable: false),
                        price = c.Single(),
                        period = c.String(),
                        iscompanyaddress = c.Boolean(nullable: false),
                        objectname = c.String(),
                        address = c.String(nullable: false),
                        lat = c.Single(nullable: false),
                        lon = c.Single(nullable: false),
                        phone = c.String(),
                        email = c.String(),
                        agefrom = c.Int(nullable: false),
                        ageto = c.Int(nullable: false),
                        individual = c.Boolean(nullable: false),
                        firstfree = c.Boolean(nullable: false),
                        drive = c.Boolean(nullable: false),
                        online = c.Boolean(nullable: false),
                        sponsored = c.Boolean(nullable: false),
                        publicated = c.Boolean(nullable: false),
                        match = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Companies", t => t.companyid, cascadeDelete: true)
                .ForeignKey("dbo.Subcategories", t => t.subcategoryid, cascadeDelete: true)
                .Index(t => t.companyid)
                .Index(t => t.subcategoryid);
            
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        userid = c.Int(nullable: false),
                        offerid = c.Int(nullable: false),
                        created = c.DateTime(nullable: false),
                        title = c.String(),
                        comment = c.String(nullable: false),
                        name = c.String(nullable: false),
                        rate = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Offers", t => t.offerid, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.userid, cascadeDelete: true)
                .Index(t => t.userid)
                .Index(t => t.offerid);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        userid = c.String(),
                        created = c.DateTime(nullable: false),
                        photo = c.String(),
                        name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.SubscribeCompanies",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        userid = c.Int(nullable: false),
                        companyid = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Companies", t => t.companyid, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.userid, cascadeDelete: true)
                .Index(t => t.userid)
                .Index(t => t.companyid);
            
            CreateTable(
                "dbo.Companies",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        userid = c.String(),
                        created = c.DateTime(nullable: false),
                        logo = c.String(),
                        name = c.String(nullable: false),
                        description = c.String(),
                        address = c.String(),
                        lat = c.Single(),
                        lon = c.Single(),
                        phone = c.String(),
                        email = c.String(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.CompanyNotifications",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        companyid = c.Int(nullable: false),
                        created = c.DateTime(nullable: false),
                        title = c.String(),
                        offer = c.Int(),
                        check = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Companies", t => t.companyid, cascadeDelete: true)
                .Index(t => t.companyid);
            
            CreateTable(
                "dbo.SubscribeOffers",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        userid = c.Int(nullable: false),
                        categoryid = c.String(),
                        subcategoryid = c.String(),
                        address = c.String(),
                        range = c.Int(),
                        lat = c.Single(),
                        lon = c.Single(),
                        age = c.Int(),
                        individual = c.Boolean(nullable: false),
                        firstfree = c.Boolean(nullable: false),
                        drive = c.Boolean(nullable: false),
                        online = c.Boolean(nullable: false),
                        smonday = c.Boolean(nullable: false),
                        smondayfrom = c.DateTime(),
                        smondayto = c.DateTime(),
                        stuesday = c.Boolean(nullable: false),
                        stuesdayfrom = c.DateTime(),
                        stuesdayto = c.DateTime(),
                        swednesday = c.Boolean(nullable: false),
                        swednesdayfrom = c.DateTime(),
                        swednesdayto = c.DateTime(),
                        sthursday = c.Boolean(nullable: false),
                        sthursdayfrom = c.DateTime(),
                        sthursdayto = c.DateTime(),
                        sfriday = c.Boolean(nullable: false),
                        sfridayfrom = c.DateTime(),
                        sfridayto = c.DateTime(),
                        ssaturday = c.Boolean(nullable: false),
                        ssaturdayfrom = c.DateTime(),
                        ssaturdayto = c.DateTime(),
                        ssunday = c.Boolean(nullable: false),
                        ssundayfrom = c.DateTime(),
                        ssundayto = c.DateTime(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Users", t => t.userid, cascadeDelete: true)
                .Index(t => t.userid);
            
            CreateTable(
                "dbo.UserNotifications",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        userid = c.Int(nullable: false),
                        created = c.DateTime(nullable: false),
                        title = c.String(),
                        offer = c.Int(),
                        check = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Users", t => t.userid, cascadeDelete: true)
                .Index(t => t.userid);
            
            CreateTable(
                "dbo.Watchings",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        userid = c.Int(nullable: false),
                        offerid = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Offers", t => t.offerid, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.userid, cascadeDelete: true)
                .Index(t => t.userid)
                .Index(t => t.offerid);
            
            CreateTable(
                "dbo.EventDates",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        offerid = c.Int(nullable: false),
                        day = c.Int(nullable: false),
                        from = c.DateTime(nullable: false),
                        to = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Offers", t => t.offerid, cascadeDelete: true)
                .Index(t => t.offerid);
            
            CreateTable(
                "dbo.Photos",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        offerid = c.Int(nullable: false),
                        url = c.String(),
                        ismain = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Offers", t => t.offerid, cascadeDelete: true)
                .Index(t => t.offerid);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Offers", "subcategoryid", "dbo.Subcategories");
            DropForeignKey("dbo.Photos", "offerid", "dbo.Offers");
            DropForeignKey("dbo.EventDates", "offerid", "dbo.Offers");
            DropForeignKey("dbo.Offers", "companyid", "dbo.Companies");
            DropForeignKey("dbo.Comments", "userid", "dbo.Users");
            DropForeignKey("dbo.Watchings", "userid", "dbo.Users");
            DropForeignKey("dbo.Watchings", "offerid", "dbo.Offers");
            DropForeignKey("dbo.UserNotifications", "userid", "dbo.Users");
            DropForeignKey("dbo.SubscribeOffers", "userid", "dbo.Users");
            DropForeignKey("dbo.SubscribeCompanies", "userid", "dbo.Users");
            DropForeignKey("dbo.SubscribeCompanies", "companyid", "dbo.Companies");
            DropForeignKey("dbo.CompanyNotifications", "companyid", "dbo.Companies");
            DropForeignKey("dbo.Comments", "offerid", "dbo.Offers");
            DropForeignKey("dbo.Subcategories", "categoryid", "dbo.Categories");
            DropIndex("dbo.Photos", new[] { "offerid" });
            DropIndex("dbo.EventDates", new[] { "offerid" });
            DropIndex("dbo.Watchings", new[] { "offerid" });
            DropIndex("dbo.Watchings", new[] { "userid" });
            DropIndex("dbo.UserNotifications", new[] { "userid" });
            DropIndex("dbo.SubscribeOffers", new[] { "userid" });
            DropIndex("dbo.CompanyNotifications", new[] { "companyid" });
            DropIndex("dbo.SubscribeCompanies", new[] { "companyid" });
            DropIndex("dbo.SubscribeCompanies", new[] { "userid" });
            DropIndex("dbo.Comments", new[] { "offerid" });
            DropIndex("dbo.Comments", new[] { "userid" });
            DropIndex("dbo.Offers", new[] { "subcategoryid" });
            DropIndex("dbo.Offers", new[] { "companyid" });
            DropIndex("dbo.Subcategories", new[] { "categoryid" });
            DropTable("dbo.Photos");
            DropTable("dbo.EventDates");
            DropTable("dbo.Watchings");
            DropTable("dbo.UserNotifications");
            DropTable("dbo.SubscribeOffers");
            DropTable("dbo.CompanyNotifications");
            DropTable("dbo.Companies");
            DropTable("dbo.SubscribeCompanies");
            DropTable("dbo.Users");
            DropTable("dbo.Comments");
            DropTable("dbo.Offers");
            DropTable("dbo.Subcategories");
            DropTable("dbo.Categories");
        }
    }
}
