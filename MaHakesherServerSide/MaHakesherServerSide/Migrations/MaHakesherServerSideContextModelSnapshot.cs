﻿// <auto-generated />
using MaHakesherServerSide.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MaHakesherServerSide.Migrations
{
    [DbContext(typeof(MaHakesherServerSideContext))]
    partial class MaHakesherServerSideContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.26")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("MaHakesherServerSide.Models.Relations", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Destination")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserName");

                    b.ToTable("Relations");
                });

            modelBuilder.Entity("MaHakesherServerSide.Models.User", b =>
                {
                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserName");

                    b.ToTable("User");
                });

            modelBuilder.Entity("MaHakesherServerSide.Models.Relations", b =>
                {
                    b.HasOne("MaHakesherServerSide.Models.User", null)
                        .WithMany("SourceAndDest")
                        .HasForeignKey("UserName");
                });

            modelBuilder.Entity("MaHakesherServerSide.Models.User", b =>
                {
                    b.Navigation("SourceAndDest");
                });
#pragma warning restore 612, 618
        }
    }
}
