﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ApiVersion.Sample.Migrations;

namespace ApiVersion.Sample.MigrationModule
{
    public class MigrationLoader : IMigrationLoader
    {
        private readonly string _ns;

        public MigrationLoader(string @namespace)
        {
            _ns = @namespace;
        }

        public IEnumerable<MigrationWrapper> Load()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.Namespace == _ns && IsMigration(t))
                .Select(t => CreateMigrationWrapper(t));
        }

        private MigrationWrapper CreateMigrationWrapper(Type migrationType)
        {
            string version = Regex.Match(migrationType.Name, @"v(.*)_").Groups[1].Value;            
            return new MigrationWrapper()
            {
                Version = version,
                Migration = (Migration) Activator.CreateInstance(migrationType)
            };
        }

        private bool IsMigration(Type type)
        {
            if (type == typeof (Migration))
            {
                return true;
            }
            if (type.BaseType == null)
            {
                return false;
            }
            return IsMigration(type.BaseType);
        }
    }
}
