namespace BITCollege_MG.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class added_nextuniquenumber_and_studentcard_class : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StudentCards",
                c => new
                    {
                        StudentCardId = c.Int(nullable: false, identity: true),
                        StudentId = c.Int(nullable: false),
                        CardNumber = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.StudentCardId)
                .ForeignKey("dbo.Students", t => t.StudentId, cascadeDelete: true)
                .Index(t => t.StudentId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StudentCards", "StudentId", "dbo.Students");
            DropIndex("dbo.StudentCards", new[] { "StudentId" });
            DropTable("dbo.StudentCards");
        }
    }
}
