﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartHomeWWW.Core.Infrastructure;

namespace SmartHomeWWW.Migrations
{
    [DbContext(typeof(SmartHomeDbContext))]
    [Migration("20211127132631_AddWeatherCache")]
    partial class AddWeatherCache
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.11");

            modelBuilder.Entity("SmartHomeCore.Domain.Sensor", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Alias")
                        .HasColumnType("TEXT");

                    b.Property<string>("ChipType")
                        .HasColumnType("TEXT");

                    b.Property<string>("FirmwareVersion")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastContact")
                        .HasColumnType("TEXT");

                    b.Property<string>("Mac")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Sensors");
                });

            modelBuilder.Entity("SmartHomeCore.Domain.WeatherCache", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Data")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("Expires")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("WeatherCaches");
                });
#pragma warning restore 612, 618
        }
    }
}
