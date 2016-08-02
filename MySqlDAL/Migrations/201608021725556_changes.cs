namespace MySqlDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Posts",
                c => new
                    {
                        PostId = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 50),
                        PostContent = c.String(nullable: false),
                        PostDate = c.DateTime(nullable: false),
                        ThreadName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.PostId);
            
            CreateTable(
                "dbo.Threads",
                c => new
                    {
                        ThreadId = c.Int(nullable: false, identity: true),
                        ThreadName = c.String(nullable: false, maxLength: 400),
                        UserName = c.String(maxLength: 50),
                        IsThreadPoll = c.Boolean(),
                    })
                .PrimaryKey(t => t.ThreadId);
            
            CreateTable(
                "dbo.UserProfiles",
                c => new
                    {
                        UserName = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.UserName);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UserProfiles");
            DropTable("dbo.Threads");
            DropTable("dbo.Posts");
        }
    }
}
