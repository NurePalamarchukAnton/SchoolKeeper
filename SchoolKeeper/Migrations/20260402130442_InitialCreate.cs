using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolKeeper.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "School",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    address = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    region = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    contact_number = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_School", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Device",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    device_name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    device_type = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    location = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    device_guid = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true),
                    school_id = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Device", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Device_School_school_id",
                        column: x => x.school_id,
                        principalTable: "School",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    full_name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    role = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    phone_number = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    school_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_School_school_id",
                        column: x => x.school_id,
                        principalTable: "School",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Incident",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    device_id = table.Column<int>(type: "INTEGER", nullable: true),
                    reported_by = table.Column<int>(type: "INTEGER", nullable: false),
                    incident_type = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    severity = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    school_id = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incident", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incident_Device_device_id",
                        column: x => x.device_id,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Incident_School_school_id",
                        column: x => x.school_id,
                        principalTable: "School",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incident_User_reported_by",
                        column: x => x.reported_by,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParentStudent",
                columns: table => new
                {
                    parent_id = table.Column<int>(type: "INTEGER", nullable: false),
                    student_id = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentStudent", x => new { x.parent_id, x.student_id });
                    table.ForeignKey(
                        name: "FK_ParentStudent_User_parent_id",
                        column: x => x.parent_id,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParentStudent_User_student_id",
                        column: x => x.student_id,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Rept",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    school_id = table.Column<int>(type: "INTEGER", nullable: false),
                    generated_by = table.Column<int>(type: "INTEGER", nullable: false),
                    period_start = table.Column<DateOnly>(type: "date", nullable: false),
                    period_end = table.Column<DateOnly>(type: "date", nullable: false),
                    summary = table.Column<string>(type: "text", nullable: true),
                    generated_on = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rept", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rept_School_school_id",
                        column: x => x.school_id,
                        principalTable: "School",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Rept_User_generated_by",
                        column: x => x.generated_by,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentTeacher",
                columns: table => new
                {
                    student_id = table.Column<int>(type: "INTEGER", nullable: false),
                    teacher_id = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentTeacher", x => new { x.student_id, x.teacher_id });
                    table.ForeignKey(
                        name: "FK_StudentTeacher_User_student_id",
                        column: x => x.student_id,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentTeacher_User_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserIncident",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    incident_id = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserIncident", x => new { x.user_id, x.incident_id });
                    table.ForeignKey(
                        name: "FK_UserIncident_Incident_incident_id",
                        column: x => x.incident_id,
                        principalTable: "Incident",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserIncident_User_user_id",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReptIncident",
                columns: table => new
                {
                    rept_id = table.Column<int>(type: "INTEGER", nullable: false),
                    incident_id = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReptIncident", x => new { x.rept_id, x.incident_id });
                    table.ForeignKey(
                        name: "FK_ReptIncident_Incident_incident_id",
                        column: x => x.incident_id,
                        principalTable: "Incident",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReptIncident_Rept_rept_id",
                        column: x => x.rept_id,
                        principalTable: "Rept",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Device_DeviceGuid",
                table: "Device",
                column: "device_guid");

            migrationBuilder.CreateIndex(
                name: "IX_Device_school_id",
                table: "Device",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_Incident_device_id",
                table: "Incident",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_Incident_reported_by",
                table: "Incident",
                column: "reported_by");

            migrationBuilder.CreateIndex(
                name: "IX_Incident_school_id",
                table: "Incident",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_Incident_timestamp",
                table: "Incident",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ParentStudent_student_id",
                table: "ParentStudent",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_Rept_generated_by",
                table: "Rept",
                column: "generated_by");

            migrationBuilder.CreateIndex(
                name: "IX_Rept_school_id",
                table: "Rept",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_ReptIncident_incident_id",
                table: "ReptIncident",
                column: "incident_id");

            migrationBuilder.CreateIndex(
                name: "IX_School_name",
                table: "School",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTeacher_teacher_id",
                table: "StudentTeacher",
                column: "teacher_id");

            migrationBuilder.CreateIndex(
                name: "IX_User_email",
                table: "User",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_school_id",
                table: "User",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserIncident_incident_id",
                table: "UserIncident",
                column: "incident_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParentStudent");

            migrationBuilder.DropTable(
                name: "ReptIncident");

            migrationBuilder.DropTable(
                name: "StudentTeacher");

            migrationBuilder.DropTable(
                name: "UserIncident");

            migrationBuilder.DropTable(
                name: "Rept");

            migrationBuilder.DropTable(
                name: "Incident");

            migrationBuilder.DropTable(
                name: "Device");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "School");
        }
    }
}
