using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace foodiya.app.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FenceOutboxLeaseOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Lease ownership is an EF concurrency mapping; existing columns are unchanged.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
