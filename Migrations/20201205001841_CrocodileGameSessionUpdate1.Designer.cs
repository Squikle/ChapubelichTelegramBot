﻿// <auto-generated />
using System;
using System.Collections.Generic;
using ChapubelichBot.Main.Chapubelich;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ChapubelichBot.Migrations
{
    [DbContext(typeof(ChapubelichdbContext))]
    [Migration("20201205001841_CrocodileGameSessionUpdate1")]
    partial class CrocodileGameSessionUpdate1
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("Botdb")
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("ChapubelichBot.Types.Entities.BoyCompliment", b =>
                {
                    b.Property<int>("ComplimentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("ComplimentText")
                        .IsRequired()
                        .HasColumnType("VARCHAR");

                    b.HasKey("ComplimentId");

                    b.HasIndex("ComplimentText")
                        .IsUnique();

                    b.ToTable("BoyCompliments");
                });

            modelBuilder.Entity("ChapubelichBot.Types.Entities.Configuration", b =>
                {
                    b.Property<bool>("Id")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("LastResetTime")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.ToTable("Configurations");
                });

            modelBuilder.Entity("ChapubelichBot.Types.Entities.CrocodileGameSession", b =>
                {
                    b.Property<long>("GroupId")
                        .HasColumnType("bigint");

                    b.Property<int>("Attempts")
                        .HasColumnType("integer");

                    b.Property<int>("GameMessageId")
                        .HasColumnType("integer");

                    b.Property<string>("GameMessageText")
                        .HasColumnType("text");

                    b.Property<string>("GameWord")
                        .HasColumnType("text");

                    b.Property<int?>("HostUserId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("LastActivity")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime?>("StartTime")
                        .IsConcurrencyToken()
                        .HasColumnType("timestamp without time zone");

                    b.Property<string[]>("WordVariants")
                        .HasMaxLength(50)
                        .HasColumnType("text[]");

                    b.HasKey("GroupId");

                    b.HasIndex("HostUserId");

                    b.ToTable("CrocodileGameSessions");
                });

            modelBuilder.Entity("ChapubelichBot.Types.Entities.CrocodileHostCandidate", b =>
                {
                    b.Property<int>("CandidateId")
                        .HasColumnType("integer");

                    b.Property<long>("CrocodileGameSessionId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("RegistrationTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("timezone('utc', now())");

                    b.HasKey("CandidateId");

                    b.HasIndex("CrocodileGameSessionId");

                    b.ToTable("CrocodileHostCandidate");
                });

            modelBuilder.Entity("ChapubelichBot.Types.Entities.DailyReward", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.Property<bool>("Rewarded")
                        .IsConcurrencyToken()
                        .HasColumnType("boolean");

                    b.Property<int>("Stage")
                        .HasColumnType("integer");

                    b.HasKey("UserId");

                    b.ToTable("DailyReward");
                });

            modelBuilder.Entity("ChapubelichBot.Types.Entities.GirlCompliment", b =>
                {
                    b.Property<int>("ComplimentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("ComplimentText")
                        .IsRequired()
                        .HasColumnType("VARCHAR");

                    b.HasKey("ComplimentId");

                    b.HasIndex("ComplimentText")
                        .IsUnique();

                    b.ToTable("GirlCompliments");
                });

            modelBuilder.Entity("ChapubelichBot.Types.Entities.Group", b =>
                {
                    b.Property<long>("GroupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityByDefaultColumn();

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("boolean");

                    b.Property<List<int>>("LastGameSessions")
                        .HasColumnType("integer[]");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("GroupId");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("ChapubelichBot.Types.Entities.GroupDailyPerson", b =>
                {
                    b.Property<long>("GroupId")
                        .HasColumnType("bigint");

                    b.Property<int?>("RollMessageId")
                        .HasColumnType("integer");

                    b.Property<string>("RolledName")
                        .HasColumnType("text");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("GroupId");

                    b.HasIndex("UserId");

                    b.ToTable("GroupDailyPerson");
                });

            modelBuilder.Entity("ChapubelichBot.Types.Entities.RouletteGameSession", b =>
                {
                    b.Property<long>("ChatId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityByDefaultColumn();

                    b.Property<int>("AnimationMessageId")
                        .HasColumnType("integer");

                    b.Property<int>("GameMessageId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("LastActivity")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("ResultNumber")
                        .HasColumnType("integer");

                    b.Property<bool>("Resulting")
                        .HasColumnType("boolean");

                    b.HasKey("ChatId");

                    b.ToTable("RouletteGameSessions");
                });

            modelBuilder.Entity("ChapubelichBot.Types.Entities.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<long>("Balance")
                        .HasColumnType("bigint");

                    b.Property<short>("DefaultBet")
                        .HasColumnType("smallint");

                    b.Property<bool>("Gender")
                        .HasColumnType("boolean");

                    b.Property<List<int>>("LastGameSessions")
                        .HasColumnType("integer[]");

                    b.Property<string>("Username")
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ChapubelichBot.Types.Entities.UserCompliment", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.Property<bool>("Praised")
                        .HasColumnType("boolean");

                    b.HasKey("UserId");

                    b.ToTable("UserCompliment");
                });

            modelBuilder.Entity("ChapubelichBot.Types.Entities.UserTheft", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("LastMoneyTheft")
                        .IsConcurrencyToken()
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("UserId");

                    b.ToTable("UserTheft");
                });

            modelBuilder.Entity("GroupUser", b =>
                {
                    b.Property<long>("GroupsGroupId")
                        .HasColumnType("bigint");

                    b.Property<int>("UsersUserId")
                        .HasColumnType("integer");

                    b.HasKey("GroupsGroupId", "UsersUserId");

                    b.HasIndex("UsersUserId");

                    b.ToTable("GroupUser");
                });

            modelBuilder.Entity("ChapubelichBot.Types.Entities.CrocodileGameSession", b =>
                {
                    b.HasOne("ChapubelichBot.Types.Entities.Group", "Group")
                        .WithOne("CrocodileGameSession")
                        .HasForeignKey("ChapubelichBot.Types.Entities.CrocodileGameSession", "GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ChapubelichBot.Types.Entities.User", "Host")
                        .WithMany()
                        .HasForeignKey("HostUserId");

                    b.Navigation("Group");

                    b.Navigation("Host");
                });

            modelBuilder.Entity("ChapubelichBot.Types.Entities.CrocodileHostCandidate", b =>
                {
                    b.HasOne("ChapubelichBot.Types.Entities.User", "Candidate")
                        .WithOne("CrocodileHostingRegistration")
                        .HasForeignKey("ChapubelichBot.Types.Entities.CrocodileHostCandidate", "CandidateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ChapubelichBot.Types.Entities.CrocodileGameSession", "CrocodileGameSession")
                        .WithMany("HostCandidates")
                        .HasForeignKey("CrocodileGameSessionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Candidate");

                    b.Navigation("CrocodileGameSession");
                });

            modelBuilder.Entity("ChapubelichBot.Types.Entities.DailyReward", b =>
                {
                    b.HasOne("ChapubelichBot.Types.Entities.User", "User")
                        .WithOne("DailyReward")
                        .HasForeignKey("ChapubelichBot.Types.Entities.DailyReward", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ChapubelichBot.Types.Entities.GroupDailyPerson", b =>
                {
                    b.HasOne("ChapubelichBot.Types.Entities.Group", "Group")
                        .WithOne("GroupDailyPerson")
                        .HasForeignKey("ChapubelichBot.Types.Entities.GroupDailyPerson", "GroupId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired();

                    b.HasOne("ChapubelichBot.Types.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("User");
                });

            modelBuilder.Entity("ChapubelichBot.Types.Entities.RouletteGameSession", b =>
                {
                    b.OwnsMany("ChapubelichBot.Types.Entities.RouletteColorBetToken", "ColorBetTokens", b1 =>
                        {
                            b1.Property<long>("RouletteGameSessionChatId")
                                .HasColumnType("bigint");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer")
                                .UseIdentityByDefaultColumn();

                            b1.Property<long>("BetSum")
                                .HasColumnType("bigint");

                            b1.Property<int>("ChoosenColor")
                                .HasColumnType("integer");

                            b1.Property<int>("UserId")
                                .HasColumnType("integer");

                            b1.HasKey("RouletteGameSessionChatId", "Id");

                            b1.HasIndex("UserId");

                            b1.ToTable("RouletteColorBetToken");

                            b1.WithOwner()
                                .HasForeignKey("RouletteGameSessionChatId");

                            b1.HasOne("ChapubelichBot.Types.Entities.User", "User")
                                .WithMany()
                                .HasForeignKey("UserId")
                                .OnDelete(DeleteBehavior.Cascade)
                                .IsRequired();

                            b1.Navigation("User");
                        });

                    b.OwnsMany("ChapubelichBot.Types.Entities.RouletteNumbersBetToken", "NumberBetTokens", b1 =>
                        {
                            b1.Property<long>("RouletteGameSessionChatId")
                                .HasColumnType("bigint");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer")
                                .UseIdentityByDefaultColumn();

                            b1.Property<long>("BetSum")
                                .HasColumnType("bigint");

                            b1.Property<int[]>("ChoosenNumbers")
                                .HasColumnType("integer[]");

                            b1.Property<int>("UserId")
                                .HasColumnType("integer");

                            b1.HasKey("RouletteGameSessionChatId", "Id");

                            b1.HasIndex("UserId");

                            b1.ToTable("RouletteNumbersBetToken");

                            b1.WithOwner()
                                .HasForeignKey("RouletteGameSessionChatId");

                            b1.HasOne("ChapubelichBot.Types.Entities.User", "User")
                                .WithMany()
                                .HasForeignKey("UserId")
                                .OnDelete(DeleteBehavior.Cascade)
                                .IsRequired();

                            b1.Navigation("User");
                        });

                    b.Navigation("ColorBetTokens");

                    b.Navigation("NumberBetTokens");
                });

            modelBuilder.Entity("ChapubelichBot.Types.Entities.UserCompliment", b =>
                {
                    b.HasOne("ChapubelichBot.Types.Entities.User", "User")
                        .WithOne("UserCompliment")
                        .HasForeignKey("ChapubelichBot.Types.Entities.UserCompliment", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ChapubelichBot.Types.Entities.UserTheft", b =>
                {
                    b.HasOne("ChapubelichBot.Types.Entities.User", "User")
                        .WithOne("UserTheft")
                        .HasForeignKey("ChapubelichBot.Types.Entities.UserTheft", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("GroupUser", b =>
                {
                    b.HasOne("ChapubelichBot.Types.Entities.Group", null)
                        .WithMany()
                        .HasForeignKey("GroupsGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ChapubelichBot.Types.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UsersUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ChapubelichBot.Types.Entities.CrocodileGameSession", b =>
                {
                    b.Navigation("HostCandidates");
                });

            modelBuilder.Entity("ChapubelichBot.Types.Entities.Group", b =>
                {
                    b.Navigation("CrocodileGameSession");

                    b.Navigation("GroupDailyPerson");
                });

            modelBuilder.Entity("ChapubelichBot.Types.Entities.User", b =>
                {
                    b.Navigation("CrocodileHostingRegistration");

                    b.Navigation("DailyReward");

                    b.Navigation("UserCompliment");

                    b.Navigation("UserTheft");
                });
#pragma warning restore 612, 618
        }
    }
}