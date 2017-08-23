namespace MT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MTModels : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Companies", "name", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Companies", "name", c => c.String(nullable: false));
        }
    }
}
