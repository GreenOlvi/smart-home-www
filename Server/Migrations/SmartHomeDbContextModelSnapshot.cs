﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartHomeWWW.Core.Infrastructure;

#nullable disable

namespace SmartHomeWWW.Migrations
{
    [DbContext(typeof(SmartHomeDbContext))]
    partial class SmartHomeDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.11");

            modelBuilder.Entity("SmartHomeWWW.Core.Domain.Entities.CacheStoreEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ExpireAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ExpireAt");

                    b.HasIndex("Key")
                        .IsUnique();

                    b.ToTable("CacheEntries");
                });

            modelBuilder.Entity("SmartHomeWWW.Core.Domain.Entities.RelayEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("ConfigSerialized")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("Config");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Relays");
                });

            modelBuilder.Entity("SmartHomeWWW.Core.Domain.Entities.Sensor", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Alias")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ChipType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FirmwareVersion")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastContact")
                        .HasColumnType("TEXT");

                    b.Property<string>("Mac")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("UpdateChannel")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Sensors");
                });

            modelBuilder.Entity("SmartHomeWWW.Core.Domain.Entities.SettingEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Key")
                        .IsUnique();

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("SmartHomeWWW.Core.Domain.Entities.TelegramUser", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<long>("TelegramId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("TelegramId")
                        .IsUnique();

                    b.ToTable("TelegramUsers");
                });

            modelBuilder.Entity("SmartHomeWWW.Core.Domain.Entities.WeatherCache", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("Expires")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
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
