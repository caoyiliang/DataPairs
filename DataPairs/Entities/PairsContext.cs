﻿/********************************************************************
 * *
 * * 使本项目源码或本项目生成的DLL前请仔细阅读以下协议内容，如果你同意以下协议才能使用本项目所有的功能，
 * * 否则如果你违反了以下协议，有可能陷入法律纠纷和赔偿，作者保留追究法律责任的权利。
 * *
 * * 1、你可以在开发的软件产品中使用和修改本项目的源码和DLL，但是请保留所有相关的版权信息。
 * * 2、不能将本项目源码与作者的其他项目整合作为一个单独的软件售卖给他人使用。
 * * 3、不能传播本项目的源码和DLL，包括上传到网上、拷贝给他人等方式。
 * * 4、以上协议暂时定制，由于还不完善，作者保留以后修改协议的权利。
 * *
 * * Copyright ©2013-? yzlm Corporation All rights reserved.
 * * 作者： 曹一梁 QQ：347739303
 * * 请保留以上版权信息，否则作者将保留追究法律责任。
 * *
 * * 创建时间：2021-06-28
 * * 说明：PairsContext.cs
 * *
********************************************************************/

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataPairs.Entities
{
    internal class PairsContext : DbContext
    {
        private string _connectionString;
        public DbSet<PairsEntity> Pairs { get; set; }

        public PairsContext(string connectionString) : base()
        {
            _connectionString = connectionString;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(new SqliteConnectionStringBuilder(_connectionString)
            {
                Mode = SqliteOpenMode.ReadWriteCreate,
                Password = "cd+8KpaWULi/W/jJNT3flg=="
            }.ToString());
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateVersion();
            return base.SaveChangesAsync(cancellationToken);
        }
        private void UpdateVersion()
        {
            foreach (var entity in this.ChangeTracker.Entries())
            {
                if (entity.State == EntityState.Modified)
                {
                    if (entity.Entity is IVersion v)
                        v.VersionNum++;
                }
            }
        }
    }
}
