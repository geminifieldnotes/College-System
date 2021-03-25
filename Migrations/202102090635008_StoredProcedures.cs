namespace BITCollege_MG.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StoredProcedures : DbMigration
    {
        public override void Up()
        {
            //call script to create the stored procedure
            this.Sql(Properties.Resource.create_next_number);
        }
        
        public override void Down()
        {
            //call script to drop the stored procedure
            this.Sql(Properties.Resource.drop_next_number);
        }
    }
}
