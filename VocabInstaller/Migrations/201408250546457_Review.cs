namespace VocabInstaller.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Review : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserProfile",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        UserName = c.String(),
                        UserEmail = c.String(),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "dbo.Questions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Word = c.String(nullable: false, maxLength: 128),
                        Meaning = c.String(nullable: false, maxLength: 256),
                        Note = c.String(maxLength: 512),
                        CreatedAt = c.DateTime(nullable: false),
                        ReviewedAt = c.DateTime(nullable: false),
                        ReviewLevel = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Questions");
            DropTable("dbo.UserProfile");
        }
    }
}
