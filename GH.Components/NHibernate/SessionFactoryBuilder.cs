using System.Reflection;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;

namespace GH.Components
{
    public static class SessionFactoryBuilder
    {
        public static ISessionFactory BuildMySql(string connectionString, Assembly mappingAssembly)
        {
            return Build<MySQLDialect, MySqlDataDriver>(connectionString, mappingAssembly, 200);
        }

        public static ISessionFactory BuildFirebird(string connectionString, Assembly mappingAssembly)
        {
            return Build<FirebirdDialect, FirebirdClientDriver>(connectionString, mappingAssembly, 0);
        }

        private static ISessionFactory Build<TDialect, TDriver>(
            string connectionString,
            Assembly mappingAssembly,
            int commandTimeout)
            where TDialect : Dialect
            where TDriver : IDriver
        {
            var configuration = new Configuration();
            configuration.DataBaseIntegration(database =>
            {
                database.ConnectionString = connectionString;
                database.Dialect<TDialect>();
                database.Driver<TDriver>();
                if (commandTimeout > 0)
                    database.Timeout = commandTimeout;
            });

            var mapper = new ModelMapper();
            mapper.AddMappings(mappingAssembly.GetExportedTypes());
            configuration.AddMapping(mapper.CompileMappingForAllExplicitlyAddedEntities());
            return configuration.BuildSessionFactory();
        }
    }
}
