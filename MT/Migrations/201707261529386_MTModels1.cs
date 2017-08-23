namespace MT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MTModels1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Users", "name", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Users", "name", c => c.String(nullable: false));
        }
    }
}
