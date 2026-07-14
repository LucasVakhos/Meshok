using GH.Configs;
using NewsMaker.NHibernate;
using NHibernate;
namespace GH.NHibernate
{
    public class FactoryCriatorNM : FactoryCriator<FactoryCriatorNM, User, CfgBridgeNote>
    {
        protected override void SetServerType()
        {
            _dbServerType = DbServerType.MySql;
        }
        public override ISessionFactory GetSessionFactory()
        {
            return SessionFactoryBuilder.BuildMySql(GetConnectionString(), typeof(User).Assembly);
        }
    }
}
