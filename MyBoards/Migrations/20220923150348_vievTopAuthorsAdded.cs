﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyBoards.Migrations
{
    public partial class vievTopAuthorsAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE VIEW View_TopAuthors AS
SELECT TOP 5 u.Id, u.FullName, COUNT(*) as [WorkItemsCreated]
FROM Users u
JOIN WorkItems wi on wi.AuthorId = u.Id
GROUP BY u.Id, u.FullName
ORDER BY [WorkItemsCreated] desc
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP VIEW View_TopAuthors
");
        }
    }
}