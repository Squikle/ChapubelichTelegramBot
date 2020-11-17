﻿// <auto-generated />
using System;
using ChapubelichBot.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ChapubelichBot.Migrations
{
    [DbContext(typeof(ChapubelichdbContext))]
    [Migration("20201116234826_LastMoneyTheft added to User")]
    partial class LastMoneyTheftaddedtoUser
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("Botdb")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("ChapubelichBot.Database.Models.BoyCompliment", b =>
                {
                    b.Property<int>("ComplimentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("ComplimentText")
                        .IsRequired()
                        .HasColumnType("VARCHAR");

                    b.HasKey("ComplimentId");

                    b.HasIndex("ComplimentText")
                        .IsUnique();

                    b.ToTable("BoyCompliments");
                });

            modelBuilder.Entity("ChapubelichBot.Database.Models.Configuration", b =>
                {
                    b.Property<bool>("Id")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("LastResetTime")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.ToTable("Configurations");
                });

            modelBuilder.Entity("ChapubelichBot.Database.Models.GirlCompliment", b =>
                {
                    b.Property<int>("ComplimentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("ComplimentText")
                        .IsRequired()
                        .HasColumnType("VARCHAR");

                    b.HasKey("ComplimentId");

                    b.HasIndex("ComplimentText")
                        .IsUnique();

                    b.ToTable("GirlCompliments");
                });

            modelBuilder.Entity("ChapubelichBot.Database.Models.Group", b =>
                {
                    b.Property<long>("GroupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("GroupId");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("ChapubelichBot.Database.Models.RouletteGameSessionData", b =>
                {
                    b.Property<long>("ChatId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("AnimationMessageId")
                        .HasColumnType("integer");

                    b.Property<int>("GameMessageId")
                        .HasColumnType("integer");

                    b.Property<int>("ResultNumber")
                        .HasColumnType("integer");

                    b.Property<bool>("Resulting")
                        .HasColumnType("boolean");

                    b.HasKey("ChatId");

                    b.ToTable("RouletteGameSessions");
                });

            modelBuilder.Entity("ChapubelichBot.Database.Models.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long>("Balance")
                        .HasColumnType("bigint");

                    b.Property<bool>("ComplimentSubscription")
                        .HasColumnType("boolean");

                    b.Property<bool>("Complimented")
                        .HasColumnType("boolean");

                    b.Property<bool>("DailyRewarded")
                        .HasColumnType("boolean");

                    b.Property<short>("DefaultBet")
                        .HasColumnType("smallint");

                    b.Property<string>("FirstName")
                        .HasColumnType("character varying(64)")
                        .HasMaxLength(64);

                    b.Property<bool>("Gender")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("LastMoneyTheft")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Username")
                        .HasColumnType("character varying(32)")
                        .HasMaxLength(32);

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ChapubelichBot.Database.Models.UserGroup", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.Property<long>("GroupId")
                        .HasColumnType("bigint");

                    b.HasKey("UserId", "GroupId");

                    b.HasIndex("GroupId");

                    b.ToTable("UserGroup");
                });

            modelBuilder.Entity("ChapubelichBot.Database.Models.RouletteGameSessionData", b =>
                {
                    b.OwnsMany("ChapubelichBot.Types.Games.RouletteGame.RouletteColorBetToken", "ColorBetTokens", b1 =>
                        {
                            b1.Property<long>("RouletteGameSessionDataChatId")
                                .HasColumnType("bigint");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer")
                                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                            b1.Property<long>("BetSum")
                                .HasColumnType("bigint");

                            b1.Property<int>("ChoosenColor")
                                .HasColumnType("integer");

                            b1.Property<int>("UserId")
                                .HasColumnType("integer");

                            b1.HasKey("RouletteGameSessionDataChatId", "Id");

                            b1.ToTable("RouletteColorBetToken");

                            b1.WithOwner()
                                .HasForeignKey("RouletteGameSessionDataChatId");
                        });

                    b.OwnsMany("ChapubelichBot.Types.Games.RouletteGame.RouletteNumbersBetToken", "NumberBetTokens", b1 =>
                        {
                            b1.Property<long>("RouletteGameSessionDataChatId")
                                .HasColumnType("bigint");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer")
                                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                            b1.Property<long>("BetSum")
                                .HasColumnType("bigint");

                            b1.Property<int[]>("ChoosenNumbers")
                                .HasColumnType("integer[]");

                            b1.Property<int>("UserId")
                                .HasColumnType("integer");

                            b1.HasKey("RouletteGameSessionDataChatId", "Id");

                            b1.ToTable("RouletteNumbersBetToken");

                            b1.WithOwner()
                                .HasForeignKey("RouletteGameSessionDataChatId");
                        });
                });

            modelBuilder.Entity("ChapubelichBot.Database.Models.UserGroup", b =>
                {
                    b.HasOne("ChapubelichBot.Database.Models.Group", "Group")
                        .WithMany("UserGroups")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ChapubelichBot.Database.Models.User", "User")
                        .WithMany("UserGroups")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
