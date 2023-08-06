﻿// <auto-generated />
using System;
using Disco.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Disco.Web.Migrations
{
    [DbContext(typeof(DiscoContext))]
    [Migration("20230806174436_AddTopTagsTable")]
    partial class AddTopTagsTable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.4");

            modelBuilder.Entity("Disco.Web.Data.Account", b =>
                {
                    b.Property<long>("accountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("age")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("createdAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("displayName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("gender")
                        .HasColumnType("TEXT");

                    b.Property<string>("pronouns")
                        .HasColumnType("TEXT");

                    b.Property<string>("username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("accountId");

                    b.ToTable("accounts");
                });

            modelBuilder.Entity("Disco.Web.Data.AccountAvatar", b =>
                {
                    b.Property<long>("accountAvatarId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("accountId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("createdAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("source")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("updatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("url")
                        .HasColumnType("TEXT");

                    b.Property<long?>("userUploadedImageId")
                        .HasColumnType("INTEGER");

                    b.HasKey("accountAvatarId");

                    b.HasIndex("accountId")
                        .IsUnique();

                    b.ToTable("accountAvatars");
                });

            modelBuilder.Entity("Disco.Web.Data.AccountBan", b =>
                {
                    b.Property<long>("accountBanId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("bannedAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("createdAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("reason")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("updatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("accountBanId");

                    b.HasIndex("bannedAccountId")
                        .IsUnique();

                    b.ToTable("accountBans");
                });

            modelBuilder.Entity("Disco.Web.Data.AccountDescription", b =>
                {
                    b.Property<long>("accountDescriptionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("accountId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("description")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("updatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("accountDescriptionId");

                    b.HasIndex("accountId")
                        .IsUnique();

                    b.ToTable("accountDescriptions");
                });

            modelBuilder.Entity("Disco.Web.Data.AccountDiscord", b =>
                {
                    b.Property<long>("accountDiscordId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("accountId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("avatarUrl")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("createdAt")
                        .HasColumnType("TEXT");

                    b.Property<long>("discordId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("tag")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("updatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("accountDiscordId");

                    b.ToTable("accountDiscords");
                });

            modelBuilder.Entity("Disco.Web.Data.AccountDiscordBan", b =>
                {
                    b.Property<long>("accountDiscordBanId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long?>("bannedAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("createdAt")
                        .HasColumnType("TEXT");

                    b.Property<long>("discordId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("updatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("accountDiscordBanId");

                    b.HasIndex("discordId")
                        .IsUnique();

                    b.ToTable("accountDiscordBans");
                });

            modelBuilder.Entity("Disco.Web.Data.AccountDiscordCode", b =>
                {
                    b.Property<long>("accountDiscordCodeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("accountId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("code")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("createdAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("redirectUrl")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("updatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("accountDiscordCodeId");

                    b.ToTable("accountDiscordCodes");
                });

            modelBuilder.Entity("Disco.Web.Data.AccountMatrix", b =>
                {
                    b.Property<long>("accountMatrixId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("accountId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("avatarUrl")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("createdAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("domain")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("updatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("accountMatrixId");

                    b.ToTable("accountMatrix");
                });

            modelBuilder.Entity("Disco.Web.Data.AccountPassword", b =>
                {
                    b.Property<long>("accountPasswordId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("accountId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("createdAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("hash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("updatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("accountPasswordId");

                    b.HasIndex("accountId")
                        .IsUnique();

                    b.ToTable("accountPasswords");
                });

            modelBuilder.Entity("Disco.Web.Data.AccountRelationship", b =>
                {
                    b.Property<long>("accountRelationshipId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("accountId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("otherAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("relationship")
                        .HasColumnType("INTEGER");

                    b.HasKey("accountRelationshipId");

                    b.HasIndex("accountId");

                    b.ToTable("accountRelationships");
                });

            modelBuilder.Entity("Disco.Web.Data.AccountReport", b =>
                {
                    b.Property<long>("accountReportId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("accountId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("createdAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("field")
                        .HasColumnType("INTEGER");

                    b.Property<int>("reason")
                        .HasColumnType("INTEGER");

                    b.Property<long>("reportedAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("state")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("updatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("accountReportId");

                    b.ToTable("accountReports");
                });

            modelBuilder.Entity("Disco.Web.Data.AccountSession", b =>
                {
                    b.Property<string>("accountSessionId")
                        .HasColumnType("TEXT");

                    b.Property<long>("accountId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("createdAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("updatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("accountSessionId");

                    b.ToTable("accountSessions");
                });

            modelBuilder.Entity("Disco.Web.Data.AccountTag", b =>
                {
                    b.Property<long>("accountTagId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("accountId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("createdAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("displayTag")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("tag")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("accountTagId");

                    b.HasIndex("accountId");

                    b.HasIndex("accountId", "tag")
                        .IsUnique();

                    b.ToTable("accountTags");
                });

            modelBuilder.Entity("Disco.Web.Data.TopTag", b =>
                {
                    b.Property<long>("topTagId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("displayTag")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("tag")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("topTagId");

                    b.HasIndex("tag")
                        .IsUnique();

                    b.ToTable("topTags");
                });

            modelBuilder.Entity("Disco.Web.Data.UserUploadedImage", b =>
                {
                    b.Property<long>("userUploadedImageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("accountId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("createdAt")
                        .HasColumnType("TEXT");

                    b.Property<long>("fileSize")
                        .HasColumnType("INTEGER");

                    b.Property<int>("format")
                        .HasColumnType("INTEGER");

                    b.Property<string>("originalSha256Hash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("sha256Hash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("sizeX")
                        .HasColumnType("INTEGER");

                    b.Property<int>("sizeY")
                        .HasColumnType("INTEGER");

                    b.Property<int>("status")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("updatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("userUploadedImageId");

                    b.HasIndex("sha256Hash")
                        .IsUnique();

                    b.ToTable("images");
                });
#pragma warning restore 612, 618
        }
    }
}
